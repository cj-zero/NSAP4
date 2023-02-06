using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Settlement;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Serve
{
    public class ServiceOrderDetailsApp : OnlyUnitWorkBaeApp
    {
        private IOptions<AppSetting> _appConfiguration;
        private HttpHelper _helper;

        private ReimburseInfoApp _reimburseInfoApp;
        private OutsourcApp _outsourcApp;

        
        public ServiceOrderDetailsApp(IUnitWork unitWork, ModuleFlowSchemeApp moduleFlowSchemeApp, IAuth auth, IOptions<AppSetting> appConfiguration, ReimburseInfoApp reimburseInfoApp, OutsourcApp outsourcApp) : base(unitWork, auth)
        {
            _appConfiguration = appConfiguration;
            _helper = new HttpHelper(_appConfiguration.Value.AppPushMsgUrl);
            _reimburseInfoApp = reimburseInfoApp;
            _outsourcApp = outsourcApp;
        }
        /// <summary>
        /// 获取按灯记录
        /// </summary>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        public async Task<TableData> GetBlameBelongInfo(int serviceOrderId)
        {
            var result = new TableData();
            var query = UnitWork.Find<BlameBelong>(c => c.OrderNo == serviceOrderId).Include(a=>a.BlameBelongUser.Where(a=> a.IsRead == false)).ToList();
            result.Data = query;
            return result;
        }
        /// <summary>
        /// 工单数据
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <returns></returns>
        public async Task<TableData> ServiceWorkOrderListDetail(int serviceOrderId)
        {
            TableData result = new TableData();
            var data = UnitWork.Find<ServiceWorkOrder>(a => a.ServiceOrderId == serviceOrderId).ToList();
            var sss = data.Select(a => a.ManufacturerSerialNumber).ToList();


            result.Data = data;
            return result;
        }
     
        /// <summary>
        /// 获取技术员数据
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <returns></returns>
        public async Task<TableData> GetTechnician(int serviceOrderId)
        {
            TableData result = new TableData();

            var serviceOrder = UnitWork.Find<ServiceOrder>(a => a.Id == serviceOrderId).FirstOrDefault();
            var srviceWorkOrder = UnitWork.Find<ServiceWorkOrder>(a => a.ServiceOrderId == serviceOrderId).Select(a=> a.FromTheme).ToList();

            List<string> listFromTheme = new List<string>();
            foreach (var item in srviceWorkOrder)
            {
                listFromTheme.AddRange(item.Split(";"));
            }
            var userList = GetSendOrderTechnicianList(listFromTheme);

            var userListId = userList?.Select(a => (int?)a.appUserId).ToList();
            var currentDate = await UnitWork.Find<RealTimeLocation>(c => userListId.Contains(c.AppUserId) && c.CreateTime >= DateTime.Now.AddDays(-1) && c.CreateTime < DateTime.Now).ToListAsync();

            var userCount = await UnitWork.Find<ServiceWorkOrder>(s => userListId.Contains(s.CurrentUserId) && s.Status < 6 && s.CompleteDate == null)
    .Select(s => new { s.CurrentUserId, s.ServiceOrderId }).Distinct()
    .GroupBy(s => s.CurrentUserId)
    .Select(g => new { g.Key, Count = g.Count() }).ToListAsync();


            var userFromTheme = UnitWork.Find<ServiceWorkOrder>(s => userListId.Contains(s.CurrentUserId) && s.Status < 6 && s.CompleteDate == null && listFromTheme.Contains(s.FromTheme))
                .Select(s => new { s.CurrentUserId, s.ServiceOrderId }).ToList();


            var erpUser = from a in UnitWork.Find<AppUserMap>(c => userListId.Contains(c.AppUserId))
                          join b in UnitWork.Find<User>(null) on a.UserID equals b.Id
                          select new { a.AppUserId,b.Id, b.Name };
            var isServiceList = (from a in UnitWork.Find<ServiceOrder>(a => a.CustomerId == serviceOrder.CustomerId && a.Id != serviceOrderId)
                         join b in UnitWork.Find<ServiceWorkOrder>(null) on a.Id equals b.ServiceOrderId
                         select b.CurrentUserId).ToList();
            var data = from a in userList
                       join b in erpUser on a.appUserId equals b.AppUserId
                       select new
                       {
                           b.Id,
                           b.Name,
                           a.skillName,
                           a.gradeName,
                           distance = GetDistance(currentDate, b.AppUserId, (decimal)serviceOrder.Latitude, (decimal)serviceOrder.Longitude),//距离
                           count = userCount.FirstOrDefault(a => a.Key == b.AppUserId)?.Count,//待处理单数
                           isService = isServiceList.Contains(b.AppUserId) ? true : false,//服务
                           isQuestion = userFromTheme.Count(a => a.CurrentUserId == b.AppUserId) > 0 ? true : false,//问题
                       };
            result.Data = data.ToList();
            return result;
        }
        public List<SendOrderTechnicianResp> GetSendOrderTechnicianList(List<string> info)
        {
            var technicianLevelList = new List<SendOrderTechnicianResp>();
            try
            {
                var timespan = DatetimeUtil.ToUnixTimestampBySeconds(DateTime.Now.AddMinutes(5));
                var text = $"NewareApiTokenDeadline:{timespan}";
                var aes = Encryption.AESEncrypt(text);
                var grade = _helper.Post(info,
                    (string.IsNullOrEmpty(_appConfiguration.Value.AppVersion) ? string.Empty : _appConfiguration.Value.AppVersion + "/") + "Exam/TechnicianByThemeCode", "EncryToken", aes);

                JObject resObj = JObject.Parse(grade);
                if (resObj["Data"] != null)
                {
                    technicianLevelList = JsonHelper.Instance.Deserialize<List<SendOrderTechnicianResp>>(resObj["Data"].ToString());
                    if (technicianLevelList == null)
                    {
                        technicianLevelList = new List<SendOrderTechnicianResp>();
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return technicianLevelList;
        }
        public double GetDistance(List<RealTimeLocation> currentDate, int? AppUserId, decimal Latitude, decimal Longitude)
        {
            var info = currentDate.Where(a => a.AppUserId == AppUserId).OrderByDescending(a => a.CreateTime).FirstOrDefault();
            if (info == null)
            {
                return 0;
            }
            var Distance = (info.Latitude == 0 || Latitude == 0) ? 0 :
                Math.Round(NauticaUtil.GetDistance(Convert.ToDouble(info.Latitude), Convert.ToDouble(info.Longitude ?? 0), Convert.ToDouble(Latitude), Convert.ToDouble(Longitude)) / 1000);

            return Distance;
        }
        
        /// <summary>
        /// 获取报销记录
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <returns></returns>
        public async Task<TableData> GetReimbursement(int serviceOrderId)
        {
            var id = UnitWork.Find<ReimburseInfo>(a => a.ServiceOrderId == serviceOrderId).FirstOrDefault().Id;
            return  await _reimburseInfoApp.GetDetails(id);

        }
        /// <summary>
        /// 获取提成记录
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <returns></returns>
        public async Task<TableData> GetCommission(int serviceOrderId)
        {
            var info = (from a in UnitWork.Find<OutsourcExpenses>(a => a.ServiceOrderId == serviceOrderId)
                     join b in UnitWork.Find<Outsourc>(null) on a.OutsourcId equals b.Id
                     select b.Id).FirstOrDefault();
            QueryoutsourcListReq req = new QueryoutsourcListReq() { OutsourcId = info.ToString()};
            return await _outsourcApp.GetDetails(req);
        }
        /// <summary>
        /// 获取历史记录
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <returns></returns>
        public async Task<TableData> GetHistory(int serviceOrderId)
        {
            TableData result = new TableData();
            //result.Data = data;
            return result;
        }

    }
}
