using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using NSAP.Entity.Admin;
using NSAP.Entity.Hr;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Serve;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Serve
{
    public class TechnicianApp : OnlyUnitWorkBaeApp
    {
        private HttpHelper _helper;
        private IOptions<AppSetting> _appConfiguration;
        public TechnicianApp(IUnitWork unitWork, IAuth auth,
           IOptions<AppSetting> appConfiguration) : base(unitWork, auth)
        {
            _appConfiguration = appConfiguration;
            _helper = new HttpHelper(_appConfiguration.Value.AppPushMsgUrl);
        }
        public async Task<TableData> GetTechnicianList(QueryTechnicianListReq req)
        {
            TableData result = new TableData();
            List<string> listStr = new List<string> { "R8", "R9", "T1", "P5", "P6", "P8", "P26", "M6" };
            var orgList = UnitWork.Find<Repository.Domain.Org>(a => a.ParentName == "售后部" || listStr.Contains(a.Name))
                .WhereIf(!string.IsNullOrWhiteSpace(req.Org), a => a.Name == req.Org)
                .Select(a => a.Id).ToList();
                
            var userList = (from a in UnitWork.Find<User>(a => a.Status == 0).WhereIf(!string.IsNullOrWhiteSpace(req.Name), a => a.Name == req.Name)
                            join b in UnitWork.Find<Relevance>(a => orgList.Contains(a.SecondId) && a.Key == Define.USERORG) on a.Id equals b.FirstId
                            select a).ToList();

            var order = (UnitWork.Find<ServiceWorkOrder>(a => a.CurrentUserNsapId != null)
                .GroupBy(a => new { a.CurrentUserNsapId, a.ServiceOrderId })
                .Select(a => new { a.Key.CurrentUserNsapId, a.Key.ServiceOrderId }))
                .GroupBy(a => a.CurrentUserNsapId)
                .Select(a => new { CurrentUserNsapId = a.Key, count = a.Count() });

            var date = DateTime.Now;
            var userId = userList.Select(a => a.Id).ToList();

            //获取工资
            var nsapUserList = UnitWork.Find<NsapUserMap>(a => userId.Contains(a.UserID)).ToList();
            var nsapUserId = nsapUserList.Select(a => (uint)a.NsapUserId);
            var detail = UnitWork.Find<base_user_detail>(a => nsapUserId.Contains(a.user_id)).ToList();


            //获取等级
            var AppUserId = UnitWork.Find<AppUserMap>(r => userId.Contains(r.UserID)).ToList();
            var technicianLevelList = GetTechnicianGrade(AppUserId.Select(a => a.AppUserId).ToArray());
            var depts = new string[] { "CS1", "CS7", "CS12", "CS14", "CS15", "CS29", "CS36", "CS37", "CS32", "CS20" }; //规定要查看的部门


            var userOrgList = (from a in UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && userId.Contains(r.FirstId))
                            join c in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on a.SecondId equals c.Id
                            select new { a.FirstId, c.Name }).ToList();

            var data = from a in userList
                       join u in AppUserId on a.Id equals u.UserID into u2
                       from us  in u2.DefaultIfEmpty()

                       join n in nsapUserList on a.Id     equals n.UserID into  n2
                       from n3 in n2.DefaultIfEmpty()

                       join b in order on a.Id equals b.CurrentUserNsapId into c
                       from temp in c.DefaultIfEmpty()
                       select new
                       {
                           a.Id,
                           us?.AppUserId,
                           Name = userOrgList.FirstOrDefault(f => f.FirstId == a.Id)?.Name +"-"+  a.Name,
                           Sex = a.Sex == 0 ? "男" : "女",
                           a.EntryTime,
                           Year = GetMonth(date, a.EntryTime),
                           count = temp?.count,
                           Grade = technicianLevelList.FirstOrDefault(a => a.AppUserId == us?.AppUserId)?.GradeName,//等级
                           GradeList =  new { },
                           Wages = DeSerialize(detail.FirstOrDefault(a => a.user_id == n3?.NsapUserId)?.full_salary)?.conversion_wages,//工资
                           AgeWages = ""//月均收入
                       };


            result.Count = data.Count();
            result.Data = data.OrderBy(a => a.EntryTime)
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToList();
            return result;
        }
        public static NSAP.Entity.Admin.conversionWages DeSerialize(byte[] bytes)
        {
            try
            {
                if (bytes == null || bytes.Length == 0) return null;
                using (MemoryStream stream = new MemoryStream())
                {
                    IFormatter bs = new BinaryFormatter();
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Seek(0, SeekOrigin.Begin);
                    return (NSAP.Entity.Admin.conversionWages)bs.Deserialize(stream);
                }
            }
            catch (Exception ex )
            {

                return null;
            }
       
        }
        public List<TechnicianGrades> GetTechnicianGrade(int?[] userArr)
        {
            var technicianLevelList = new List<TechnicianGrades>();
            try
            {
                var timespan = DatetimeUtil.ToUnixTimestampBySeconds(DateTime.Now.AddMinutes(5));
                var text = $"NewareApiTokenDeadline:{timespan}";
                var aes = Encryption.AESEncrypt(text);
                var grade = _helper.Post(new
                {
                    UserIds = userArr,

                }, (string.IsNullOrEmpty(_appConfiguration.Value.AppVersion) ? string.Empty : _appConfiguration.Value.AppVersion + "/") + "Exam/GetTechnicianGrades", "EncryToken", aes);

                JObject resObj = JObject.Parse(grade);
                if (resObj["Data"] != null)
                {
                    technicianLevelList = JsonHelper.Instance.Deserialize<List<TechnicianGrades>>(resObj["Data"].ToString());
                }
            }
            catch (Exception ex)
            {

            }
            return technicianLevelList;
        }

        public string GetMonth(DateTime now, DateTime? EntryTime)
        {
            if (EntryTime == null)
            {
                return "";
            }
            var oldDate = Convert.ToDateTime(EntryTime);
            var month = (now.Year - oldDate.Year) * 12 + now.Month - oldDate.Month;
            if (month <= 0)
            {
                return "";
            }
            return month / 12 + "年" + month % 12 + "月";
        }

        public async Task<TableData> GetTechnicianOrder(string Id )
        {
            TableData result = new TableData();
            var data = UnitWork.Find<TechnicianCompletedQuantity>(a => a.UserId == Id)
                .OrderByDescending(a => a.Year)
                .ThenByDescending(a => a.Month)
                .ToList();
            result.Data = data;
            return result;
        }
    

    }
}
