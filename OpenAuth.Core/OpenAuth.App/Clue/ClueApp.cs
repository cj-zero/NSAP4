using Infrastructure;
using Newtonsoft.Json;
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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static OpenAuth.App.Clue.ModelDto.KuaiBaosHelper;

namespace OpenAuth.App
{
    /// <summary>
    /// 线索服务
    /// </summary>
    public class ClueApp : OnlyUnitWorkBaeApp
    {
        private const String host = "https://kop.kuaidihelp.com";
        private const String path = "/api";
        private const String requestMethod = "POST";
        //public static Express express = new Express();
        private RevelanceManagerApp _revelanceApp;
        ServiceBaseApp _serviceBaseApp;
        public ClueApp(ServiceBaseApp serviceBaseApp, IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
            _serviceBaseApp = serviceBaseApp;

        }



        #region 线索
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
            exp = exp.And(t => t.CreateUser == loginUser.Name);
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
                else
                {
                    result.DaysNotFollowedUp = "0天";
                    result.FollowUpTime = "暂无跟进时间";
                }
                datascoure.Add(result);
            }
            rowcount = list.Count;
            return datascoure;
        }





        /// <summary>
        /// 新增线索
        /// </summary>
        /// <param name="addClueReq"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
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
                SerialNumber = !string.IsNullOrEmpty(await GetCardCode()) ? await GetCardCode() : "X00001",
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

            var log = new AddClueLogReq();
            log.ClueId = data.Id;
            log.LogType = 0;
            log.CreateTime = DateTime.Now;
            log.CreateUser = loginUser.Name;
            log.Details = "客户" + "'" + addClueReq.CardName + "'";
            await AddClueLogAsync(log);
            return data.Id.ToString();
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
            if (clue != null)
            {

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
                var cluecontacts = UnitWork.FindSingle<Repository.Domain.Serve.ClueContacts>(q => q.ClueId == clueId && q.IsDefault);
                if (cluecontacts != null)
                {
                    result.Essential.Name = cluecontacts.Name;
                    result.Essential.Tel1 = cluecontacts.Tel1;
                    result.Essential.Address1 = cluecontacts.Address1;
                    result.Essential.Address2 = cluecontacts.Address2;
                    result.Essential.Role = cluecontacts.Role;
                    result.Essential.Position = cluecontacts.Position;
                }
                var clueLogList = UnitWork.Find<Repository.Domain.Serve.ClueLog>(q => q.ClueId == clueId).OrderByDescending(q => q.CreateTime).MapToList<ClueLog>();
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
            }

            return result;
        }



        /// <summary>
        /// 生成线索编号
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetCardCode()
        {
            string sql = "SELECT SerialNumber FROM Clue ORDER BY SUBSTR(SerialNumber FROM 1 FOR 1),CAST(SUBSTR(SerialNumber FROM 2) AS UNSIGNED) DESC LIMIT 1";
            var obj = UnitWork.ExecuteScalar(ContextType.Nsap4ServeDbContextType, sql, CommandType.Text, null);
            if (obj != null)
            {
                var max = obj.ToString();
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
            else
            {
                return "";
            }

        }
        /// <summary>
        /// 编辑更新
        /// </summary>
        /// <param name="updateClueReq"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> UpdateClueAsync(UpdateClueReq updateClueReq)
        {
            bool result = false;
            var mes = "";
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var entity = UnitWork.FindSingle<Repository.Domain.Serve.Clue>(q => q.Id == updateClueReq.Id);
            if (entity != null)
            {
                if (updateClueReq.CardName != null && entity.CardName != updateClueReq.CardName) { mes = updateClueReq.CardName + ":原客户名称'" + entity.CardName + "';"; }
                entity.CardName = updateClueReq.CardName;
                if (entity.CustomerSource != updateClueReq.CustomerSource) { mes += updateClueReq.CustomerSource + ":原客户来源'" + entity.CustomerSource + "'，（0:领英、1:国内展会、2:国外展会、3:客户介绍、4:新威官网、5:其他）;"; }
                entity.CustomerSource = updateClueReq.CustomerSource;
                if (entity.IndustryInvolved != updateClueReq.IndustryInvolved)
                {
                    mes += updateClueReq.IndustryInvolved + ":原所属行业'" + entity.IndustryInvolved + "'，（0:农、林、牧、渔业,1:制造业,2:电力、热力、燃气及水生产和供应业," +
"3:建筑业,4:批发和零售业,5:交通运输、仓储和邮政业,6:住宿和餐饮业,7:信息传输、软件和信息技术服务业,8:金融业,9:房地产业,10:租赁和商务服务业,11:科学研究和技术服务业,12:水利、环境和公共设施管理业," +
"13:居民服务、修理和其他服务业,14:文化、体育和娱乐业,15:其他行业）;";
                }
                entity.IndustryInvolved = updateClueReq.IndustryInvolved;
                if (entity.StaffSize != updateClueReq.StaffSize) { mes += updateClueReq.StaffSize + ":原人员规模'" + entity.StaffSize + "'，（0:1-20、1:20-100、2:100-500、3:500-1000、4:1000-10000、5:10000以上); "; }
                entity.StaffSize = updateClueReq.StaffSize;
                if (updateClueReq.WebSite != null && entity.WebSite != updateClueReq.WebSite) { mes += updateClueReq.WebSite + ":原网址'" + entity.WebSite + "';"; }
                entity.WebSite = updateClueReq.WebSite;
                if (updateClueReq.Remark != null && entity.Remark != updateClueReq.Remark) { mes += updateClueReq.Remark + ":原备注'" + entity.Remark + "';"; }
                entity.Remark = updateClueReq.Remark;
                entity.IsCertification = updateClueReq.IsCertification;
                entity.UpdateTime = DateTime.Now;
                entity.UpdateUser = loginUser.Name;
                var emodel = UnitWork.FindSingle<Repository.Domain.Serve.ClueContacts>(q => q.ClueId == updateClueReq.Id && q.IsDefault);
                if (emodel != null)
                {
                    emodel.ClueId = updateClueReq.Id;
                    if (emodel.Name != updateClueReq.Name) { mes += updateClueReq.Name + ":原客户名称'" + emodel.Name + "';"; }
                    emodel.Name = updateClueReq.Name;
                    if (emodel.Tel1 != updateClueReq.Tel1) { mes += updateClueReq.Tel1 + ":原联系电话一'" + emodel.Tel1 + "';"; }
                    emodel.Tel1 = updateClueReq.Tel1;
                    if (emodel.Role != updateClueReq.Role) { mes += updateClueReq.Role + ":原角色'" + emodel.Role + "'，（0：决策者、1：普通人）;"; }
                    emodel.Role = updateClueReq.Role;
                    if (emodel.Position != updateClueReq.Position) { mes += updateClueReq.Position + ":原职位'" + emodel.Position + "';"; }
                    emodel.Position = updateClueReq.Position;
                    if (emodel.Address1 != updateClueReq.Address1) { mes += updateClueReq.Address1 + ":原省市'" + emodel.Address1 + "';"; }
                    emodel.Address1 = updateClueReq.Address1;
                    if (emodel.Address2 != updateClueReq.Address2) { mes += updateClueReq.Address2 + ":原详细地址'" + emodel.Address2 + "';"; }
                    emodel.Address2 = updateClueReq.Address2;
                    emodel.UpdateTime = DateTime.Now;
                    emodel.UpdateUser = loginUser.Name;
                    emodel.IsDefault = true;
                    result = true;
                    if (result)
                    {

                        var log = new AddClueLogReq();
                        log.ClueId = updateClueReq.Id;
                        log.LogType = 1;
                        log.CreateTime = DateTime.Now;
                        log.CreateUser = loginUser.Name;
                        log.Details = mes;
                        await AddClueLogAsync(log);
                    }
                    UnitWork.Update(emodel);

                }
                UnitWork.Update(entity);
                UnitWork.Save();

            }

            return result;
        }
        public async Task<bool> DeleteClueByIdAsync(List<int> clueId)
        {
            bool result = false;
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            foreach (var item in clueId)
            {
                var clue = await UnitWork.FindSingleAsync<Repository.Domain.Serve.Clue>(q => q.Id == item);
                if (clue != null)
                {
                    clue.IsDelete = true;
                    await UnitWork.UpdateAsync(clue);
                    //联系人
                    var clueContacts = UnitWork.Find<ClueContacts>(q => q.ClueId == item).MapToList<ClueContacts>();
                    if (clueContacts.Count > 0)
                    {
                        foreach (var scon in clueContacts)
                        {
                            scon.IsDelete = true;
                        }
                        await UnitWork.BatchUpdateAsync(clueContacts.ToArray());
                    }

                    //日程
                    var clueSchedule = UnitWork.Find<ClueSchedule>(q => q.ClueId == item).MapToList<ClueSchedule>();
                    if (clueSchedule.Count > 0)
                    {
                        foreach (var scon in clueSchedule)
                        {
                            scon.IsDelete = true;
                        }
                        await UnitWork.BatchUpdateAsync(clueSchedule.ToArray());
                    }

                    //跟进
                    var clueFollowUp = UnitWork.Find<ClueFollowUp>(q => q.ClueId == item).MapToList<ClueFollowUp>();
                    if (clueFollowUp.Count > 0)
                    {
                        foreach (var scon in clueFollowUp)
                        {
                            scon.IsDelete = true;
                        }
                        await UnitWork.BatchUpdateAsync(clueFollowUp.ToArray());
                    }

                    //附件
                    var clueFile = UnitWork.Find<ClueFile>(q => q.ClueId == item).MapToList<ClueFile>();
                    if (clueFile.Count > 0)
                    {
                        foreach (var scon in clueFile)
                        {
                            scon.IsDelete = true;
                        }
                        await UnitWork.BatchUpdateAsync(clueFile.ToArray());
                    }

                    //意向商品
                    var clueIntentionProduct = UnitWork.Find<ClueIntentionProduct>(q => q.ClueId == item).MapToList<ClueIntentionProduct>();
                    if (clueIntentionProduct.Count > 0)
                    {
                        foreach (var scon in clueIntentionProduct)
                        {
                            scon.IsDelete = true;
                        }
                        await UnitWork.BatchUpdateAsync(clueIntentionProduct.ToArray());
                    }

                    //日志
                    var log = new AddClueLogReq();
                    log.ClueId = item;
                    log.LogType = 2;
                    log.CreateTime = DateTime.Now;
                    log.CreateUser = loginUser.Name;
                    var mes = "删除线索以及其余附属信息：线索Id为_" + item;
                    log.Details = JsonHelper.Instance.Serialize(mes);
                    await AddClueLogAsync(log);
                    UnitWork.Save();
                    result = true;
                }

            }
            return result;
        }
        #endregion

        #region 日程
        /// <summary>
        /// 新增日程
        /// </summary>
        /// <param name="addClueScheduleReq"></param>
        /// <returns></returns>
        public async Task<string> AddClueScheduleAsync(AddClueScheduleReq addClueScheduleReq)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            DateTime startTime;
            DateTime.TryParse(addClueScheduleReq.StartTime, out startTime);
            DateTime endTime;
            DateTime.TryParse(addClueScheduleReq.EndTime, out endTime);
            DateTime remindTime;
            DateTime.TryParse(addClueScheduleReq.RemindTime, out remindTime);
            ClueSchedule clueSchedule = new ClueSchedule
            {
                ClueId = addClueScheduleReq.ClueId,
                Details = addClueScheduleReq.Details,
                StartTime = startTime,
                EndTime = endTime,
                Participant = JsonHelper.Instance.Serialize(addClueScheduleReq.Participant),
                RemindTime = remindTime,
                RelatedObjects = addClueScheduleReq.RelatedObjects,
                Remark = addClueScheduleReq.Remark,
                CreateUser = loginUser.Name,
                CreateTime = DateTime.Now
            };
            var data = UnitWork.Add<OpenAuth.Repository.Domain.Serve.ClueSchedule, int>(clueSchedule);
            UnitWork.Save();
            return data.Id.ToString();
        }

        /// <summary>
        /// 日程详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ClueSchedule> GetScheduleByIdAsync(int id)
        {
            return await UnitWork.FindSingleAsync<ClueSchedule>(q => q.Id == id);

        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="updateClueScheduleReq"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> UpdateScheduleByIdAsync(UpdateClueScheduleReq updateClueScheduleReq)
        {
            bool result = false;
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var clueSchedule = await UnitWork.FindSingleAsync<ClueSchedule>(q => q.Id == updateClueScheduleReq.Id);
            if (clueSchedule != null)
            {


                DateTime startTime;
                DateTime.TryParse(updateClueScheduleReq.StartTime, out startTime);
                DateTime endTime;
                DateTime.TryParse(updateClueScheduleReq.EndTime, out endTime);
                DateTime remindTime;
                DateTime.TryParse(updateClueScheduleReq.RemindTime, out remindTime);

                clueSchedule.ClueId = updateClueScheduleReq.ClueId;
                clueSchedule.Details = updateClueScheduleReq.Details;
                clueSchedule.StartTime = startTime;
                clueSchedule.EndTime = endTime;
                clueSchedule.Participant = JsonHelper.Instance.Serialize(updateClueScheduleReq.Participant);
                clueSchedule.RemindTime = remindTime;
                clueSchedule.RelatedObjects = updateClueScheduleReq.RelatedObjects;
                clueSchedule.Remark = updateClueScheduleReq.Remark;
                clueSchedule.UpdateUser = loginUser.Name;
                clueSchedule.UpdateTime = DateTime.Now;

                await UnitWork.UpdateAsync(clueSchedule);
                UnitWork.Save();
            }
            return result;
        }

        /// <summary>
        /// 删除日程
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<bool> DeleteScheduleByIdAsync(List<int> ids)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            bool result = false;
            foreach (var item in ids)
            {
                var clueSchedule = UnitWork.FindSingle<ClueSchedule>(q => q.Id == item);
                clueSchedule.IsDelete = true;
                await UnitWork.UpdateAsync(clueSchedule);
                //日志
                var log = new AddClueLogReq();
                log.ClueId = item;
                log.LogType = 2;
                log.CreateTime = DateTime.Now;
                log.CreateUser = loginUser.Name;
                var mes = "删除线索Id为_" + clueSchedule.ClueId + "下，日程Id为_" + clueSchedule.Id + "的记录";
                log.Details = JsonHelper.Instance.Serialize(mes);
                await AddClueLogAsync(log);
                UnitWork.Save();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 根据部门id查用户列表
        /// </summary>
        /// <returns></returns>
        public List<TextVaule> UserListAsync()
        {
            var userId = _serviceBaseApp.GetUserNaspId();
            var depId = _serviceBaseApp.GetSalesDepID(userId);
            var list = new List<TextVaule>();
            var test = UnitWork.ExcuteSql<Repository.Domain.base_user_detail>(ContextType.NsapBaseDbContext, $"SELECT user_id FROM base_user_detail WHERE dep_id={depId}", CommandType.Text, null);
            foreach (var item in test)
            {
                var scon = UnitWork.FindSingle<Repository.Domain.base_user>(q => q.user_id == item.user_id);
                if (scon != null)
                {
                    var nes = new TextVaule();
                    nes.Text = scon.user_nm;
                    nes.Value = (int)scon.user_id;
                    list.Add(nes);
                }
            }
            return list;
        }
        /// <summary>
        /// 日程详情
        /// </summary>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        public async Task<List<ClueScheduleListDto>> ClueScheduleByIdAsync(int clueId)
        {
            var result = new List<ClueScheduleListDto>();
            var clue = UnitWork.FindSingle<Repository.Domain.Serve.Clue>(q => q.Id == clueId);
            if (clue != null)
            {
                var clueSchedule = UnitWork.Find<Repository.Domain.Serve.ClueSchedule>(q => q.ClueId == clueId).MapToList<ClueSchedule>();
                foreach (var item in clueSchedule)
                {
                    var scon = new ClueScheduleListDto();
                    scon.Id = item.Id;
                    scon.Details = item.Details;
                    scon.EndTime = item.EndTime;
                    scon.CardName = clue.CardName;
                    scon.RelatedObjects = item.RelatedObjects;
                    scon.RemindTime = item.RemindTime;
                    scon.Status = item.Status;
                    scon.Remark = item.Remark;
                    scon.Participant = JsonHelper.Instance.Deserialize<List<TextVaule>>(item.Participant);
                    result.Add(scon);

                }
            }
            return result;
        }
        /// <summary>
        /// 日程完成
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> SuccessByIdAsync(int id)
        {
            bool result = false;
            var clueSchedule = UnitWork.FindSingle<ClueSchedule>(q => q.Id == id);
            if (clueSchedule != null)
            {
                clueSchedule.Status = 1;
                result = true;
                await UnitWork.UpdateAsync(clueSchedule);
                await UnitWork.SaveAsync();
            }
            return result;
        }
        #endregion

        #region 跟进
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
        /// 跟进列表
        /// </summary>
        /// <param name="clueId"></param>
        /// <returns></returns>
        public async Task<List<ClueFollowUpListDto>> ClueFollowUpByIdAsync(int clueId)
        {
            var result = new List<ClueFollowUpListDto>();
            var clue = UnitWork.FindSingle<Repository.Domain.Serve.Clue>(q => q.Id == clueId);
            if (clue == null)
            {
                var clueSchedule = UnitWork.Find<Repository.Domain.Serve.ClueFollowUp>(q => q.ClueId == clueId).MapToList<ClueFollowUp>();
                foreach (var item in clueSchedule)
                {
                    var scon = new ClueFollowUpListDto();
                    scon.Id = item.Id;
                    scon.ClueId = item.ClueId;
                    var clueContacts = UnitWork.FindSingle<ClueContacts>(q => q.ClueId == clueId && q.IsDefault);
                    scon.Name = clueContacts.Name;
                    scon.FollowUpWay = item.FollowUpWay;
                    scon.Details = item.Details;
                    scon.FollowUpTime = item.FollowUpTime;
                    scon.NextFollowTime = item.NextFollowTime;
                    result.Add(scon);
                }
            }
            return result;
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

        #endregion

        #region 附件
        /// <summary>
        /// 附件列表
        /// </summary>
        /// <param name="clueId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<ClueFileListDto>> ClueFileByIdAsync(int clueId)
        {
            var result = new List<ClueFileListDto>();
            var clue = UnitWork.FindSingle<Repository.Domain.Serve.Clue>(q => q.Id == clueId);
            if (clue != null)
            {
                var clueFile = UnitWork.Find<Repository.Domain.Serve.ClueFile>(q => q.Id == clueId).MapToList<ClueFile>();
                foreach (var item in clueFile)
                {
                    var scon = new ClueFileListDto();
                    scon.Id = item.Id;
                    scon.ClueId = item.ClueId;
                    scon.FileName = item.FileName;
                    scon.FileUrl = item.FileUrl;
                    scon.CreateUser = item.CreateUser;
                    scon.UpdateTime = item.UpdateTime;
                    result.Add(scon);

                }
            }
            return result;
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="addClueFileUploadReq"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<bool> AddClueFileUploadAsync(List<AddClueFileUploadReq> addClueFileUploadReq)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var clueFilelist = new List<ClueFile>();
            foreach (var item in addClueFileUploadReq)
            {
                ClueFile clueFile = new ClueFile
                {
                    ClueId = item.ClueId,
                    FileName = item.FileName,
                    FileUrl = item.FileUrl,
                    CreateUser = loginUser.Name,
                    CreateTime = DateTime.Now
                };
                clueFilelist.Add(clueFile);
            }

            await UnitWork.BatchAddAsync<ClueFile, int>(clueFilelist.ToArray());
            UnitWork.Save();
            return true;
        }


        #endregion

        #region 联系人
        /// <summary>
        /// 联系人列表
        /// </summary>
        /// <param name="clueId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<ClueContactsListDto>> ClueContactsByIdAsync(int clueId)
        {
            var result = new List<ClueContactsListDto>();
            var clue = UnitWork.FindSingle<Repository.Domain.Serve.Clue>(q => q.Id == clueId);
            if (clue != null)
            {
                var clueContacts = UnitWork.Find<Repository.Domain.Serve.ClueContacts>(q => q.Id == clueId).MapToList<ClueContacts>();
                foreach (var item in clueContacts)
                {
                    var scon = new ClueContactsListDto();
                    scon.Id = item.Id;
                    scon.ClueId = item.ClueId;
                    scon.Name = item.Name;
                    scon.Tel1 = item.Tel1;
                    scon.Tel2 = item.Tel2;
                    scon.Address1 = item.Address1;
                    scon.Address2 = item.Address2;
                    scon.IsDefault = item.IsDefault;
                    scon.Role = item.Role;
                    scon.Position = item.Position;
                    result.Add(scon);
                }
            }
            return result;
        }

        /// <summary>
        /// 新增联系人
        /// </summary>
        /// <param name="addClueContactsReq"></param>
        /// <returns></returns>
        public async Task<string> AddClueContactsAsync(AddClueContactsReq addClueContactsReq)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            ClueContacts clueContacts = new ClueContacts
            {
                ClueId = addClueContactsReq.ClueId,
                Name = addClueContactsReq.Name,
                Tel1 = addClueContactsReq.Tel1,
                Tel2 = addClueContactsReq.Tel2,
                Role = addClueContactsReq.Role,
                Position = addClueContactsReq.Position,
                Address1 = addClueContactsReq.Address1,
                Address2 = addClueContactsReq.Address2,
                CreateUser = loginUser.Name,
                CreateTime = DateTime.Now

            };
            var data = UnitWork.Add<ClueContacts, int>(clueContacts);
            UnitWork.Save();
            return data.Id.ToString();
        }

        /// <summary>
        /// 设置默认联系人
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        public async Task<bool> IsDefaultClueContactsAsync(int id, int clueId)
        {
            UnitWork.Find<ClueContacts>(q => q.ClueId == clueId).MapToList<ClueContacts>()
                  .ForEach(zw =>
                  {
                      zw.IsDefault = false;
                  });
            UnitWork.Save();
            var entity = UnitWork.FindSingle<ClueContacts>(q => q.Id == id && q.ClueId == clueId);
            entity.IsDefault = true;
            UnitWork.Update(entity);
            UnitWork.Save();
            return true;
        }
        #endregion
        #region 日志

        /// <summary>
        /// 操作记录列表
        /// </summary>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        public async Task<List<ClueLogListDto>> ClueLogByIdAsync(int clueId)
        {
            var result = UnitWork.Find<ClueLog>(q => q.ClueId == clueId).MapToList<ClueLogListDto>();
            return result;
        }

        /// <summary>
        /// 新增操作记录
        /// </summary>
        /// <param name="addClueLogReq"></param>
        /// <returns></returns>
        public async Task<bool> AddClueLogAsync(AddClueLogReq addClueLogReq)
        {
            ClueLog clueLog = new ClueLog();
            addClueLogReq.CopyTo(clueLog);
            var result = UnitWork.Add<ClueLog, int>(clueLog);
            UnitWork.Save();
            return true;
        }
        #endregion




        #region 工具方法
        /// <summary>
        /// 地址智能解析
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public KuaiBaoResponse GetAddres(string str)
        {
            KuaiBaoResponse kuaiBaoResponse = new KuaiBaoResponse();
            String querys = "";
            String appId = "107493";
            String method = "cloud.address.resolve";
            String ts = GetTimeStamp() + "";
            String appKey = "9461b36037d07276f85c492b063231847f242afb";

            // 计算签名
            String signStr = appId + method + ts + appKey;
            String sign = GetMd5(signStr, 32);

            String bodys = "app_id=" + appId + "&method=" + method + "&ts=" + ts + "&sign=" + sign;

            // data参数是个json格式字符串 建议使用函数或方法生成json字符串
            KuaiRequest kuaiRequest = new KuaiRequest
            {
                text = str
            };
            bodys = bodys + "&data=" + JsonConvert.SerializeObject(kuaiRequest);
            String url = host + path;
            HttpWebRequest httpRequest = null;
            HttpWebResponse httpResponse = null;
            if (0 < querys.Length)
            {
                url = url + "?" + querys;
            }

            if (host.Contains("https://"))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                httpRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
            }
            else
            {
                httpRequest = (HttpWebRequest)WebRequest.Create(url);
            }
            httpRequest.Method = requestMethod;

            //根据API的要求，定义相对应的Content-Type
            httpRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            if (0 < bodys.Length)
            {
                byte[] data = Encoding.UTF8.GetBytes(bodys);
                using (Stream stream = httpRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            try
            {
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            }
            catch (WebException ex)
            {
                httpResponse = (HttpWebResponse)ex.Response;
            }
            Stream st = httpResponse.GetResponseStream();
            StreamReader reader = new StreamReader(st, Encoding.GetEncoding("utf-8"));
            var result = JsonConvert.DeserializeObject<KuaiBao>(reader.ReadToEnd());
            if (result != null && result.code == 0 && result.data != null)
            {
                kuaiBaoResponse.Phone = !string.IsNullOrWhiteSpace(result.data.FirstOrDefault().mobile) ? result.data.FirstOrDefault().mobile : result.data.FirstOrDefault().phone;
                kuaiBaoResponse.Name = result.data.FirstOrDefault().name;
                kuaiBaoResponse.ProvinceName = result.data.FirstOrDefault().province_name;
                kuaiBaoResponse.CityName = result.data.FirstOrDefault().city_name;
                kuaiBaoResponse.CountyName = result.data.FirstOrDefault().county_name;
                kuaiBaoResponse.Detail = result.data.FirstOrDefault().detail;

            }
            return kuaiBaoResponse;
        }
        // 计算md5值
        public static string GetMd5(string md5str, int type)
        {
            if (type == 16)
            {
                MD5 algorithm = MD5.Create();
                byte[] data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(md5str));
                string sh1 = "";
                for (int i = 0; i < data.Length; i++)
                {
                    sh1 += data[i].ToString("x2").ToUpperInvariant();
                }
                return sh1.Substring(8, 16).ToLower();
            }
            else if (type == 32)
            {
                MD5 algorithm = MD5.Create();
                byte[] data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(md5str));
                string sh1 = "";
                for (int i = 0; i < data.Length; i++)
                {
                    sh1 += data[i].ToString("x2").ToUpperInvariant();
                }
                return sh1.ToLower();
            }
            return "";
        }
        // 获取当前时间戳
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
        #endregion
    }
}
