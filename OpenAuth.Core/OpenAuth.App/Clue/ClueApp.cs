using Infrastructure;
using OpenAuth.App.Clue.ModelDto;
using OpenAuth.App.Clue.Request;
using OpenAuth.App.Interface;
using OpenAuth.App.Meeting.ModelDto;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain.ProductModel;
using OpenAuth.Repository.Domain.Serve;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OpenAuth.App
{
    /// <summary>
    /// 线索服务
    /// </summary>
    public class ClueApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;
        ServiceBaseApp _serviceBaseApp;
        public ClueApp(ServiceBaseApp serviceBaseApp, IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
            _serviceBaseApp = serviceBaseApp;

        }
        /// <summary>
        /// 线索列表
        /// </summary>
        /// <param name="clueListReq"></param>
        /// <param name="rowcount"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<ClueListDto> GetClueListAsync(ClueListReq clueListReq, out int rowcount)
        {
            var datascoure = new List<ClueListDto>();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            Expression<Func<OpenAuth.Repository.Domain.Serve.Clue, bool>> exp = t => true;
            exp = exp.And(t => !t.IsDelete);
            //exp = exp.And(t => t.CreateUser == loginUser.Name);
            if (clueListReq.SboId != -1)
            {

            }
            if (!string.IsNullOrWhiteSpace(clueListReq.Key))
            {
                exp = exp.And(t => t.CardName.Contains(clueListReq.Key) || t.SerialNumber.Contains(clueListReq.Key));

            }
            if (clueListReq.Status != -1)
            {
                exp = exp.And(t => t.Status == clueListReq.Status);

            }
            if (!string.IsNullOrEmpty(clueListReq.StartTime) && !string.IsNullOrEmpty(clueListReq.EndTime))
            {
                DateTime startTime;
                DateTime.TryParse(clueListReq.StartTime, out startTime);
                DateTime endTime;
                DateTime.TryParse(clueListReq.EndTime, out endTime);
                exp = exp.And(t => t.CreateTime >= startTime && t.CreateTime <= endTime);
            }
            if (!string.IsNullOrEmpty(clueListReq.Remark))
            {
                exp = exp.And(t => t.Remark.Contains(clueListReq.Remark));
            }
            if (!string.IsNullOrEmpty(clueListReq.Tag))
            {
                exp = exp.And(t => t.Tags.Contains(clueListReq.Tag));
            }
            var objs = UnitWork.Find(clueListReq.page, clueListReq.limit, "", exp);
            var list = objs.MapToList<Repository.Domain.Serve.Clue>();
            foreach (var item in list)
            {
                var result = new ClueListDto();
                result.Id = item.Id;
                result.Remark = item.Remark;
                result.SerialNumber = item.SerialNumber;
                result.CardName = item.CardName;
                result.CustomerSource = item.CustomerSource;
                result.CreateTime = item.CreateTime;
                result.UpdateTime = item.UpdateTime;
                result.Status = item.Status;
                Expression<Func<OpenAuth.Repository.Domain.Serve.ClueContacts, bool>> exps = t => true;
                exps = exps.And(t => !t.IsDelete);
                exps = exps.And(t => t.ClueId == item.Id);
                exps = exps.And(t => t.IsDefault == true);
                if (!string.IsNullOrEmpty(clueListReq.Contacts))
                {
                    exps = exps.And(t => t.Name.Contains(clueListReq.Remark));
                }
                if (!string.IsNullOrEmpty(clueListReq.Address))
                {
                    exps = exps.And(t => t.Address2.Contains(clueListReq.Tag));
                }
                var data = UnitWork.FindSingle(exps);
                if (data != null)
                {
                    result.Name = data.Name;
                    result.Tel1 = data.Tel1;
                    result.Address1 = data.Address1;
                    result.Address2 = data.Address2;
                    var cluefollowup = UnitWork.FindSingle<ClueFollowUp>(q => q.ClueId == item.Id);
                    if (cluefollowup != null)
                    {
                        result.FollowUpTime = cluefollowup.FollowUpTime.ToString();
                        var subTime = (DateTime.Now.Subtract(cluefollowup.FollowUpTime));
                        result.DaysNotFollowedUp = $"{subTime.Days}天";
                    }
                    else
                    {
                        result.DaysNotFollowedUp = "0天";
                        result.FollowUpTime = "暂无跟进时间";
                    }
                }
              
                datascoure.Add(result);
            }
            rowcount = list.Count;
            return datascoure;
        }


        public async Task<string> AddClueAsync(AddClueReq addClueReq)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            OpenAuth.Repository.Domain.Serve.Clue clue = new Repository.Domain.Serve.Clue
            {
                CardName = addClueReq.CardName,
                CustomerSource = addClueReq.CustomerSource,
                IndustryInvolved = addClueReq.IndustryInvolved,
                StaffSize = addClueReq.StaffSize,
                WebSite = addClueReq.WebSite,
                Remark = addClueReq.Remark,
                IsCertification = addClueReq.IsCertification,
                Status = 0,
                CreateTime = DateTime.Now,
                CreateUser = loginUser.Name,
                SerialNumber = await GetCardCode()
            };
            var data = UnitWork.Add<OpenAuth.Repository.Domain.Serve.Clue, int>(clue);
            UnitWork.Save();
            OpenAuth.Repository.Domain.Serve.ClueContacts cluecontacts = new Repository.Domain.Serve.ClueContacts
            {
                ClueId = data.Id,
                Name = addClueReq.Name,
                Tel1 = addClueReq.Tel1,
                Role = addClueReq.Role,
                Position = addClueReq.Position,
                Address1 = addClueReq.Address1,
                Address2 = addClueReq.Address2,
                CreateTime = DateTime.Now,
                CreateUser = loginUser.Name,
                IsDefault = true
            };
            UnitWork.Add<OpenAuth.Repository.Domain.Serve.ClueContacts, int>(cluecontacts);
            UnitWork.Save();
            return data.Id.ToString();
        }

        /// <summary>
        /// 生成线索编号
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetCardCode()
        {
            string sql = "SELECT SerialNumber FROM Clue ORDER BY SUBSTR(SerialNumber FROM 1 FOR 1),CAST(SUBSTR(SerialNumber FROM 2) AS UNSIGNED) DESC LIMIT 1";
            var max = UnitWork.ExecuteScalar(ContextType.Nsap4ServeDbContextType, sql, CommandType.Text, null).ToString();
            string zyt = max.Substring(0, 1).ToUpper();
            string tmpNum = max.Substring(1, max.Length - 1);
            string Num = (Convert.ToInt32(tmpNum) + 1).ToString();
            string mid = string.Empty;
            for (int i = 0; i < tmpNum.Length - Num.Length; i++)
            {
                mid += "0";
            }
            return zyt + mid + Num;
        }
        /// <summary>
        /// 新增跟进
        /// </summary>
        /// <param name="addClueFollowUpReq"></param>
        /// <returns></returns>
        public async Task<string> AddClueFollowAsync(AddClueFollowUpReq addClueFollowUpReq)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            ClueFollowUp clueFollowUp = new ClueFollowUp
            {
                ClueId = addClueFollowUpReq.ClueId,
                ContactsId = addClueFollowUpReq.ContactsId,
                FollowUpWay = addClueFollowUpReq.FollowUpWay,
                FollowUpTime = addClueFollowUpReq.FollowUpTime,
                NextFollowTime = addClueFollowUpReq.NextFollowTime,
                CreateUser = loginUser.Name,
                CreateTime = DateTime.Now
            };
            var data = UnitWork.Add<OpenAuth.Repository.Domain.Serve.ClueFollowUp, int>(clueFollowUp);
            UnitWork.Save();
            return data.Id.ToString();
        }
        /// <summary>
        /// 新建客户跟联系人下拉框
        /// </summary>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        public async Task<List<TextVaule>> ContactsListAsync(int ClueId)
        {
            var list = new List<TextVaule>();
            var data = UnitWork.Find<ClueContacts>(q => q.ClueId == ClueId);
            var asc = data.MapToList<ClueContacts>();
            list = asc.Select(m => new TextVaule
            {
                Text = m.Name,
                Value = m.Id
            }).ToList();
            return list;
        }
        /// <summary>
        /// 线索详情
        /// </summary>
        /// <param name="clueId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ClueInfoDto> ClueByIdAsync(int clueId)
        {
            var result = new ClueInfoDto();
            var clue = UnitWork.FindSingle<Repository.Domain.Serve.Clue>(q => q.Id == clueId);
            result.SerialNumber = clue.SerialNumber;
            result.CardName = clue.CardName;
            result.CreateUser = clue.CreateUser;
            result.CreateTime = clue.CreateTime.ToString();
            result.Essential.CustomerSource = clue.CustomerSource;
            result.Essential.IndustryInvolved = clue.IndustryInvolved;
            result.Essential.Tags = clue.Tags;
            result.Essential.StaffSize = clue.StaffSize;
            result.Essential.WebSite = clue.WebSite;
            result.Essential.Remark = clue.Remark;
            var cluecontacts = UnitWork.FindSingle<Repository.Domain.Serve.ClueContacts>(q => q.ClueId == clueId && !q.IsDefault);
            if (cluecontacts != null)
            {
                result.Essential.Name = cluecontacts.Name;
                result.Essential.Tel1 = cluecontacts.Tel1;
                result.Essential.Address1 = cluecontacts.Address1;
                result.Essential.Address2 = cluecontacts.Address2;
            }
            var clueLogList = UnitWork.Find<Repository.Domain.Serve.ClueLog>(q => q.ClueId == clueId).MapToList<ClueLog>();
            //var clueLog = clueLogList.MapToList<ClueLog>();
            if (clueLogList.Count > 0 && clueLogList != null)
            {
                foreach (var item in clueLogList)
                {
                    var scon = new Log();
                    scon.LogType = item.LogType;
                    scon.Details = item.Details;
                    scon.CreateUser = item.CreateUser;
                    scon.CreateTime = item.CreateTime;
                    result.Log.Add(scon);
                }
            }
            var clueSchedule = UnitWork.Find<Repository.Domain.Serve.ClueSchedule>(q => q.ClueId == clueId).OrderByDescending(q => q.StartTime).MapToList<ClueSchedule>();
            if (clueSchedule.Count > 0)
            {
                foreach (var item in clueSchedule)
                {
                    var scon = new Schedule();
                    scon.EndTime = item.EndTime.ToString();
                    scon.Details = item.Details;
                    scon.CreateUser = item.CreateUser;
                    result.Schedule.Add(scon);
                }
            }
            var cluefollowup = UnitWork.Find<Repository.Domain.Serve.ClueFollowUp>(q => q.ClueId == clueId).OrderByDescending(q => q.FollowUpTime).MapToList<ClueFollowUp>();
            if (cluefollowup.Count > 0 && cluefollowup != null)
            {
                result.FollowUpTime = cluefollowup[0].FollowUpTime.ToString();
                foreach (var item in cluefollowup)
                {
                    var scon = new FollowUp();
                    scon.CardName = clue.CardName;
                    scon.FollowUpWay = item.FollowUpWay;
                    scon.CreateUser = item.CreateUser;
                    scon.FollowUpTime = item.FollowUpTime.ToString();
                    result.FollowUp.Add(scon);
                }
            }
            var clueIntentionProduct = UnitWork.Find<Repository.Domain.Serve.ClueIntentionProduct>(q => q.ClueId == clueId).OrderByDescending(q => q.CreateTime).MapToList<ClueIntentionProduct>();
            if (clueIntentionProduct.Count > 0)
            {
                foreach (var item in clueIntentionProduct)
                {
                    var scon = new IntentionProduct();
                    scon.ItemCode = item.ItemCode;
                    scon.ItemName = item.ItemName;
                    scon.ItemDescription = item.ItemDescription;
                    scon.Pic = item.Pic;
                    result.IntentionProduct.Add(scon);
                }
            }
            return result;
        }
    }
}
