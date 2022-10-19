using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenAuth.App.Clue.ModelDto;
using OpenAuth.App.Clue.Request;
using OpenAuth.App.Interface;
using OpenAuth.App.Meeting.ModelDto;
using OpenAuth.App.Order.ModelDto;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain.ProductModel;
using OpenAuth.Repository.Domain.Serve;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using MailKit.Search;
using Microsoft.EntityFrameworkCore.Internal;
using static OpenAuth.App.Clue.ModelDto.KuaiBaosHelper;
using OpenAuth.Repository.Domain;

namespace OpenAuth.App
{
    /// <summary>
    /// 线索服务
    /// </summary>
    public class ClueApp : OnlyUnitWorkBaeApp
    {
        private IHttpClientFactory _httpClient;
        private const String host = "https://kop.kuaidihelp.com";
        private const String path = "/api";
        private const String requestMethod = "POST";
        //public static Express express = new Express();
        private RevelanceManagerApp _revelanceApp;
        ServiceBaseApp _serviceBaseApp;
        public ClueApp(IHttpClientFactory _httpClient, ServiceBaseApp serviceBaseApp, IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
            _serviceBaseApp = serviceBaseApp;
            this._httpClient = _httpClient;

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
            if (!loginContext.Roles.Exists(a => a.Name == "公海管理员"))
            {
                exp = exp.And(t => t.CreateUser == loginUser.Name);
            }
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
                exp = exp.And(t => t.CreateTime >= startTime && t.CreateTime <= endTime.AddDays(1));
            }
            if (!string.IsNullOrEmpty(clueListReq.Remark))
            {
                exp = exp.And(t => t.Remark.Contains(clueListReq.Remark));
            }
            if (!string.IsNullOrEmpty(clueListReq.Tag))
            {
                exp = exp.And(t => t.Tags.Contains(clueListReq.Tag));
            }
            var clue = UnitWork.Find(exp).MapToList<Repository.Domain.Serve.Clue>();
            var clueFollowUp = new List<ClueFollowUp>();
            foreach (var item in clue)
            {
                var scon = UnitWork.Find<ClueFollowUp>(q => !q.IsDelete && q.ClueId == item.Id).MapToList<ClueFollowUp>().OrderByDescending(q => q.FollowUpTime).Take(1);
                clueFollowUp.AddRange(scon);
            }
            var clueContacts = UnitWork.Find<ClueContacts>(q => !q.IsDelete && q.IsDefault).MapToList<ClueContacts>();
            var queryAllCustomers = from a in clue
                                    join b in clueContacts on a.Id equals b.ClueId

                                    where !string.IsNullOrEmpty(clueListReq.Contacts) ? b.Name.Contains(clueListReq.Contacts) : true
                                    where !string.IsNullOrEmpty(clueListReq.Address) ? b.Address2.Contains(clueListReq.Address) : true
                                    orderby a.Id descending
                                    select new ClueListDto
                                    {
                                        Id = a.Id,
                                        Remark = a.Remark,
                                        SerialNumber = a.SerialNumber,
                                        CardName = a.CardName,
                                        CustomerSource = a.CustomerSource,
                                        CreateTime = a.CreateTime,
                                        UpdateTime = a.UpdateTime,
                                        Status = a.Status,
                                        Tags = !string.IsNullOrWhiteSpace(a.Tags)
                                            ? JsonConvert.DeserializeObject<List<string>>(a.Tags)
                                            : null,
                                        Name = b.Name,
                                        Tel1 = b.Tel1,
                                        Address1 = b.Address1,
                                        Address2 = b.Address2,
                                        Email = b.Email
                                    };
            rowcount = queryAllCustomers.Count();
            var datas = queryAllCustomers.Skip((clueListReq.page - 1) * clueListReq.limit).Take(clueListReq.limit);
            foreach (var item in datas)
            {
                var cluefollowups = UnitWork.FindSingle<ClueFollowUp>(q => q.ClueId == item.Id);
                if (cluefollowups != null)
                {
                    item.FollowUpTime = cluefollowups.FollowUpTime.ToString();
                    var subTime = (DateTime.Now.Subtract(cluefollowups.FollowUpTime));
                    item.DaysNotFollowedUp = $"{subTime.Days}天";
                }
            }
            var list = datas.MapToList<ClueListDto>();
            return list;
        }

        /// <summary>
        /// 修改线索的状态
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> ChangeClueStatus(string serialNumber)
        {
            var result = new Infrastructure.Response();

            await UnitWork.UpdateAsync<Repository.Domain.Serve.Clue>(c => c.SerialNumber == serialNumber, x => new Repository.Domain.Serve.Clue
            {
                Status = 1
            });
            await UnitWork.SaveAsync();

            return result;
        }


        /// <summary>
        /// 修改线索的状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> ChangeClueStatusById(int id,int jobid)
        {
            var result = new Infrastructure.Response();
            //2022.10.18 check if wfa.job is legit ,if not ,pass ，wait for a minute for the sync process
            System.Threading.Thread.Sleep(50000);
            var legitJob = UnitWork.FindSingle<wfa_job>(a => a.base_entry == id && a.job_id ==jobid && a.sync_stat == 4 );
            if (legitJob== null)
            {
                return result;
            }
            await UnitWork.UpdateAsync<Repository.Domain.Serve.Clue>(c => c.Id == id, x => new Repository.Domain.Serve.Clue
            {
                Status = 1
            });
            await UnitWork.SaveAsync();

            return result;
        }

        /// <summary>
        /// 回写客户编码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> GetCardcodeById(int id,string cardcode)
        {
            var result = new Infrastructure.Response();

            await UnitWork.UpdateAsync<Repository.Domain.Serve.Clue>(c => c.Id == id, x => new Repository.Domain.Serve.Clue
            {
                CardCode = cardcode
            });
            await UnitWork.SaveAsync();

            return result;
        }


        /// <summary>
        /// 获取标签
        /// </summary>
        /// <param name="clueId"></param>
        /// <returns></returns>
        public async Task<List<string>> GetClueTagById(int clueId)
        {
            var clue = await UnitWork.FindSingleAsync<Repository.Domain.Serve.Clue>(q => q.Id == clueId);
            if (clue == null)
            {
                return null;
            }
            return JsonHelper.Instance.Deserialize<List<string>>(clue.Tags);
        }



        /// <summary>
        /// 添加标签
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> AddTag(AddTag tags)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var clue = await UnitWork.FindSingleAsync<Repository.Domain.Serve.Clue>(q => q.Id == tags.ClueId);
            if (tags.Tag.Count > 6)
            {

                return false;
            }
            var log = new AddClueLogReq();
            log.ClueId = clue.Id;
            log.LogType = 1;
            if (clue.Tags == null)
            {
                log.LogType = 0;
            }
            log.CreateTime = DateTime.Now;
            log.CreateUser = loginUser.Name;
            clue.Tags = JsonHelper.Instance.Serialize(tags.Tag);
            log.Details = "'" + clue.Tags + "'标签";

            await UnitWork.UpdateAsync(clue);
            await UnitWork.SaveAsync();
            await AddClueLogAsync(log);

            return true;
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
                Email = addClueReq.Email,
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
                result.Essential.Tags = !string.IsNullOrWhiteSpace(clue.Tags) ? JsonConvert.DeserializeObject<List<string>>(clue.Tags) : null;
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
                    result.Essential.Email = cluecontacts.Email;
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
                var clueSchedule = UnitWork.Find<Repository.Domain.Serve.ClueSchedule>(q => q.ClueId == clueId && !q.IsDelete).OrderByDescending(q => q.StartTime).MapToList<ClueSchedule>();
                if (clueSchedule.Count > 0)
                {
                    foreach (var item in clueSchedule)
                    {
                        var scon = new Schedule();
                        scon.EndTime = item.EndTime.ToString();
                        scon.Details = item.Details;
                        scon.CreateUser = item.CreateUser;
                        scon.Status = item.Status;
                        result.Schedule.Add(scon);
                    }
                }
                var cluefollowup = UnitWork.Find<Repository.Domain.Serve.ClueFollowUp>(q => q.ClueId == clueId && !q.IsDelete).OrderByDescending(q => q.FollowUpTime).MapToList<ClueFollowUp>();
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
                var clueIntentionProduct = UnitWork.Find<Repository.Domain.Serve.ClueIntentionProduct>(q => q.ClueId == clueId && !q.IsDelete).OrderByDescending(q => q.CreateTime).MapToList<ClueIntentionProduct>();
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
                    if (emodel.Email != updateClueReq.Email) { mes += updateClueReq.Email + ":原邮箱'" + emodel.Email + "';"; }
                    emodel.Email = updateClueReq.Email;
                    emodel.UpdateTime = DateTime.Now;
                    emodel.UpdateUser = loginUser.Name;
                    emodel.IsDefault = true;
                    result = true;
                    if (result)
                    {
                        //日志
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
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="clueId"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
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
                    if (clue.IsDelete)
                    {
                        return false;
                    }
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
                    log.Details = "删除线索以及其余附属信息：线索Id为_" + item; ;
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
            var log = new AddClueLogReq();
            log.ClueId = addClueScheduleReq.ClueId;
            log.LogType = 0;
            log.CreateTime = DateTime.Now;
            log.CreateUser = loginUser.Name;
            log.Details = "'" + addClueScheduleReq.Details + "'日程";
            await AddClueLogAsync(log);
            return data.Id.ToString();
        }

        /// <summary>
        /// 日程详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ClueScheduleInfoDto> GetScheduleByIdAsync(int id)
        {
            var clueSchedule = await UnitWork.FindSingleAsync<Repository.Domain.Serve.ClueSchedule>(q => q.Id == id && !q.IsDelete);
            var clue = await UnitWork.FindSingleAsync<Repository.Domain.Serve.Clue>(q => q.Id == clueSchedule.ClueId);
            var scon = new ClueScheduleInfoDto();
            scon.Id = clueSchedule.Id;
            scon.Details = clueSchedule.Details;
            scon.EndTime = clueSchedule.EndTime;
            scon.StartTime = clueSchedule.StartTime;
            scon.CardName = clue.CardName;
            scon.RelatedObjects = clueSchedule.RelatedObjects;
            scon.RemindTime = clueSchedule.RemindTime;
            scon.Status = clueSchedule.Status;
            scon.Remark = clueSchedule.Remark;
            scon.Participant = JsonHelper.Instance.Deserialize<List<TextVaule>>(clueSchedule.Participant);
            return scon;

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

                //日志
                var log = new AddClueLogReq();
                log.ClueId = clueSchedule.ClueId;
                log.LogType = 1;
                log.CreateTime = DateTime.Now;
                log.CreateUser = loginUser.Name;
                log.Details = "日程'" + updateClueScheduleReq.Details + "':原'" + clueSchedule.Details + "',修改为:'" + updateClueScheduleReq.Details + "'";
                await AddClueLogAsync(log);
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
                log.Details = "'" + clueSchedule.Details + "'日程";
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
        public List<TextVaule> UserListAsync(string Name)
        {
            var userId = _serviceBaseApp.GetUserNaspId();
            var depId = _serviceBaseApp.GetSalesDepID(userId);
            var list = new List<TextVaule>();
            var test = UnitWork.ExcuteSql<Repository.Domain.base_user_detail>(ContextType.NsapBaseDbContext, $"SELECT user_id FROM base_user_detail WHERE dep_id={depId}", CommandType.Text, null);
            foreach (var item in test)
            {
                var scon = UnitWork.FindSingle<Repository.Domain.base_user>(q => q.user_id == item.user_id);
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    scon = UnitWork.FindSingle<Repository.Domain.base_user>(q => q.user_id == item.user_id && q.user_nm.Contains(Name));
                }
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
        /// 日程列表
        /// </summary>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        public async Task<List<ClueScheduleListDto>> ClueScheduleByIdAsync(int clueId)
        {
            var result = new List<ClueScheduleListDto>();
            var clue = UnitWork.FindSingle<Repository.Domain.Serve.Clue>(q => q.Id == clueId);
            if (clue != null)
            {
                var clueSchedule = UnitWork.Find<Repository.Domain.Serve.ClueSchedule>(q => q.ClueId == clueId && !q.IsDelete).MapToList<ClueSchedule>();
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
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            bool result = false;
            var clueSchedule = UnitWork.FindSingle<ClueSchedule>(q => q.Id == id);
            if (clueSchedule != null)
            {
                clueSchedule.Status = 1;
                result = true;
                await UnitWork.UpdateAsync(clueSchedule);
                //日志
                var log = new AddClueLogReq();
                log.ClueId = clueSchedule.ClueId;
                log.LogType = 1;
                log.CreateTime = DateTime.Now;
                log.CreateUser = loginUser.Name;
                log.Details = "'" + clueSchedule.Details + "'日程已完成";
                await AddClueLogAsync(log);
                await UnitWork.SaveAsync();
            }
            return result;
        }
        #endregion

        #region 跟进
        /// <summary>
        /// 跟进附件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<ClueFileUploadDto>> GetClueFileByIdAsync(int id)
        {
            var data = new List<ClueFileUploadDto>();
            var clueFile = UnitWork.Find<ClueFile>(q => q.ClueFollowUpId == id && !q.IsDelete).MapToList<ClueFile>();
            foreach (var item in clueFile)
            {
                var scon = new ClueFileUploadDto();
                scon.ClueId = item.ClueId;
                scon.ClueFollowUpId = item.Id;
                scon.FileType = item.FileType;
                scon.FileName = item.FileName;
                scon.FileUrl = item.FileUrl;
                data.Add(scon);
            }
            return data;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> DeleteFollowByIdAsync(List<int> Ids)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            foreach (var item in Ids)
            {
                var clueFollowUp = await UnitWork.FindSingleAsync<ClueFollowUp>(q => q.Id == item);
                clueFollowUp.IsDelete = true;
                await UnitWork.UpdateAsync(clueFollowUp);
                await UnitWork.SaveAsync();
                //日志
                var log = new AddClueLogReq();
                log.ClueId = clueFollowUp.Id;
                log.LogType = 2;
                log.CreateTime = DateTime.Now;
                log.CreateUser = loginUser.Name;
                log.Details = "'" + clueFollowUp.FollowUpWay + "'跟进记录,（0：电话营销，1：邮件跟进，2：微信跟进，3：拜访客户，4，客户来访，5：其他）";
                await AddClueLogAsync(log);
            }

            return true;
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
            DateTime followUpTime;
            DateTime.TryParse(addClueFollowUpReq.FollowUpTime, out followUpTime);
            DateTime nextFollowTime;
            if (!string.IsNullOrWhiteSpace(addClueFollowUpReq.NextFollowTime))
            {

                DateTime.TryParse(addClueFollowUpReq.NextFollowTime, out nextFollowTime);
            }
            else
            {
                nextFollowTime = followUpTime.AddMonths(1);


            }
            ClueFollowUp clueFollowUp = new ClueFollowUp
            {
                ClueId = addClueFollowUpReq.ClueId,
                ContactsId = addClueFollowUpReq.ContactsId,
                FollowUpWay = addClueFollowUpReq.FollowUpWay,
                FollowUpTime = followUpTime,
                NextFollowTime = nextFollowTime,
                Details = addClueFollowUpReq.Details,
                CreateUser = loginUser.Name,
                CreateTime = DateTime.Now
            };
            var data = UnitWork.Add<ClueFollowUp, int>(clueFollowUp);
            UnitWork.Save();
            if (addClueFollowUpReq.AddClueFileUploadReq.Count > 0)
            {
                addClueFollowUpReq.AddClueFileUploadReq.ForEach(f => { f.ClueFollowUpId = data.Id; });
                await AddClueFileUploadAsync(addClueFollowUpReq.AddClueFileUploadReq);
            }
            //日志
            var log = new AddClueLogReq();
            log.ClueId = addClueFollowUpReq.ClueId;
            log.LogType = 0;
            log.CreateTime = DateTime.Now;
            log.CreateUser = loginUser.Name;
            log.Details = "'" + addClueFollowUpReq.FollowUpWay + "'跟进记录,（0：电话营销，1：邮件跟进，2：微信跟进，3：拜访客户，4，客户来访，5：其他）";
            await AddClueLogAsync(log);
            return data.Id.ToString();
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="updateClueFollowUpReq"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> UpdateClueFollowAsync(UpdateClueFollowUpReq updateClueFollowUpReq)
        {
            var mes = "";
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var clueFollowUp = await UnitWork.FindSingleAsync<ClueFollowUp>(q => q.Id == updateClueFollowUpReq.Id);
            clueFollowUp.ContactsId = updateClueFollowUpReq.ContactsId;
            if (updateClueFollowUpReq.FollowUpWay != clueFollowUp.FollowUpWay)
            {
                mes = "'" + updateClueFollowUpReq.FollowUpWay + "'跟进记录,原跟进方式:'" + clueFollowUp.FollowUpWay + "',修改为'" + updateClueFollowUpReq.Details + "（0：电话营销，1：邮件跟进，2：微信跟进，3：拜访客户，4，客户来访，5：其他）";
            }
            clueFollowUp.FollowUpWay = updateClueFollowUpReq.FollowUpWay;
            if (updateClueFollowUpReq.Details != null && updateClueFollowUpReq.Details != clueFollowUp.Details)
            {
                mes = "'" + updateClueFollowUpReq.Details + "'跟进记录,原跟进内容:'" + clueFollowUp.Details + "',修改为'" + updateClueFollowUpReq.Details + "'";
            }
            clueFollowUp.Details = updateClueFollowUpReq.Details;

            clueFollowUp.FollowUpTime = updateClueFollowUpReq.FollowUpTime;
            clueFollowUp.NextFollowTime = updateClueFollowUpReq.NextFollowTime;
            clueFollowUp.UpdateUser = loginUser.Name;
            clueFollowUp.UpdateTime = DateTime.Now;
            UnitWork.Update(clueFollowUp);

            if (updateClueFollowUpReq.AddClueFileUploadReq.Count > 0)
            {
                var clueFile = UnitWork.Find<ClueFile>(q => q.ClueId == clueFollowUp.ClueId).MapToList<ClueFile>();
                clueFile.ForEach(clueFile => { clueFile.IsDelete = true; });
                await UnitWork.BatchUpdateAsync(clueFile.ToArray());
                UnitWork.Save();
                updateClueFollowUpReq.AddClueFileUploadReq.ForEach(f => { f.ClueFollowUpId = updateClueFollowUpReq.Id; });
                await AddClueFileUploadAsync(updateClueFollowUpReq.AddClueFileUploadReq);
            }
            //日志
            var log = new AddClueLogReq();
            log.ClueId = clueFollowUp.ClueId;
            log.LogType = 1;
            log.CreateTime = DateTime.Now;
            log.CreateUser = loginUser.Name;
            log.Details = mes;
            await AddClueLogAsync(log);
            return true;
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
            if (clue != null)
            {
                var clueSchedule = UnitWork.Find<Repository.Domain.Serve.ClueFollowUp>(q => q.ClueId == clueId && !q.IsDelete).MapToList<ClueFollowUp>();
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
            var data = UnitWork.Find<ClueContacts>(q => q.ClueId == ClueId && !q.IsDelete);
            var asc = data.MapToList<ClueContacts>();
            list = asc.Select(m => new TextVaule
            {
                Text = m.Name,
                Value = m.Id
            }).ToList();
            return list;
        }
        /// <summary>
        /// 跟进详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ClueFollowUpInfoDto> FollowByIdAsync(int id)
        {
            var result = new ClueFollowUpInfoDto();
            var clueFollowUp = await UnitWork.FindSingleAsync<ClueFollowUp>(q => q.Id == id && !q.IsDelete);
            result.ClueId = clueFollowUp.ClueId;
            result.ContactsId = clueFollowUp.ContactsId;
            result.UpdateTime = clueFollowUp.UpdateTime;
            result.NextFollowTime = clueFollowUp.NextFollowTime;
            result.FollowUpTime = clueFollowUp.FollowUpTime;
            result.FollowUpWay = clueFollowUp.FollowUpWay;
            result.Details = clueFollowUp.Details;
            result.CreateUser = clueFollowUp.CreateUser;
            result.CreateTime = clueFollowUp.CreateTime;
            result.UpdateUser = clueFollowUp.UpdateUser;
            var clueFile = UnitWork.Find<ClueFile>(q => q.ClueFollowUpId == id && !q.IsDelete).MapToList<ClueFile>();
            foreach (var item in clueFile)
            {
                result.ClueFileList.Add(item);
            }

            return result;
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
                var clueFile = UnitWork.Find<Repository.Domain.Serve.ClueFile>(q => q.ClueId == clueId && !q.IsDelete).MapToList<ClueFile>();
                foreach (var item in clueFile)
                {
                    var scon = new ClueFileListDto();
                    scon.Id = item.Id;
                    scon.ClueId = item.ClueId;
                    scon.FileName = item.FileName;
                    scon.FileUrl = item.FileUrl;
                    scon.CreateUser = item.CreateUser;
                    scon.CreateTime = item.CreateTime;
                    scon.FileSize = item.FileSize;
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
                    FileType = item.FileType,
                    FileName = item.FileName,
                    FileUrl = item.FileUrl,
                    FileSize = item.FileSize,
                    CreateUser = loginUser.Name,
                    CreateTime = DateTime.Now,
                    ClueFollowUpId = item.ClueFollowUpId.Value
                };
                clueFilelist.Add(clueFile);
                //日志
                var log = new AddClueLogReq();
                log.ClueId = item.ClueId;
                log.LogType = 0;
                log.CreateTime = DateTime.Now;
                log.CreateUser = loginUser.Name;
                log.Details = "'" + item.FileName + "'附件";
                await AddClueLogAsync(log);
            }

            await UnitWork.BatchAddAsync<ClueFile, int>(clueFilelist.ToArray());
            UnitWork.Save();
            return true;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> DeleteFileByIdAsync(List<int> Ids)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            foreach (var item in Ids)
            {
                var clueFile = await UnitWork.FindSingleAsync<ClueFile>(q => q.Id == item);
                clueFile.IsDelete = true;
                await UnitWork.UpdateAsync(clueFile);
                await UnitWork.SaveAsync();
                //日志
                var log = new AddClueLogReq();
                log.ClueId = clueFile.ClueId;
                log.LogType = 2;
                log.CreateTime = DateTime.Now;
                log.CreateUser = loginUser.Name;
                log.Details = "'" + clueFile.FileName + "'附件";
                await AddClueLogAsync(log);
            }

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
        public List<ClueContactsListDto> ClueContactsByIdAsync(int clueId, out int rowcount)
        {
            var result = new List<ClueContactsListDto>();
            var clue = UnitWork.FindSingle<Repository.Domain.Serve.Clue>(q => q.Id == clueId);
            if (clue != null)
            {
                var clueContacts = UnitWork.Find<Repository.Domain.Serve.ClueContacts>(q => q.ClueId == clueId && !q.IsDelete).MapToList<ClueContacts>();
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
                    scon.Email = item.Email;
                    result.Add(scon);
                }
            }
            rowcount = result.Count;
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
                Email = addClueContactsReq.Email,
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
            //日志
            var log = new AddClueLogReq();
            log.ClueId = addClueContactsReq.ClueId;
            log.LogType = 0;
            log.CreateTime = DateTime.Now;
            log.CreateUser = loginUser.Name;
            log.Details = "'" + addClueContactsReq.Name + "'";
            await AddClueLogAsync(log);
            return data.Id.ToString();
        }
        /// <summary>
        /// 删除联系人
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<string> DeleteContactsByIdAsync(List<int> ids)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            foreach (var item in ids)
            {
                var clueContacts = UnitWork.FindSingle<ClueContacts>(q => q.Id == item);
                if (clueContacts.IsDelete == true)
                {
                    return "不可删除默认联系人";
                }
                clueContacts.IsDelete = true;
                UnitWork.Update(clueContacts);
                UnitWork.Save();
                //日志
                var log = new AddClueLogReq();
                log.ClueId = clueContacts.ClueId;
                log.LogType = 2;
                log.CreateTime = DateTime.Now;
                log.CreateUser = loginUser.Name;
                log.Details = "'" + clueContacts.Name + "'联系人";
                await AddClueLogAsync(log);
            }

            return "true";
        }
        /// <summary>
        /// 获取当前联系人
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ClueContacts> ContactsByIdInfoAsync(int id)
        {
            return await UnitWork.FindSingleAsync<ClueContacts>(q => q.Id == id);
        }

        /// <summary>
        /// 设置默认联系人
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        public async Task<bool> IsDefaultClueContactsAsync(int id, int clueId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;

            var clueContacts = UnitWork.Find<ClueContacts>(q => q.ClueId == clueId).MapToList<ClueContacts>();
            clueContacts.ForEach(zw => { zw.IsDefault = false; });
            foreach (var item in clueContacts)
            {
                if (item.Id == id)
                {
                    item.IsDefault = true;
                    var log = new AddClueLogReq();
                    log.ClueId = item.Id;
                    log.LogType = 1;
                    log.CreateTime = DateTime.Now;
                    log.CreateUser = loginUser.Name;
                    log.Details = "将'" + item.Name + "'设置为默认联系人";
                    await AddClueLogAsync(log);
                }
            }

            await UnitWork.BatchUpdateAsync(clueContacts.ToArray());
            //var entity = UnitWork.FindSingle<ClueContacts>(q => q.Id == id && q.ClueId == clueId);
            //entity.IsDefault = true;
            //await UnitWork.UpdateAsync(entity);
            await UnitWork.SaveAsync();
            //日志

            return true;
        }
        /// <summary>
        /// 编辑更新
        /// </summary>
        /// <param name="updateClueContactsReq"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> UpdateClueContactsAsync(UpdateClueContactsReq updateClueContactsReq)
        {
            var mes = "";
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var clueContacts = await UnitWork.FindSingleAsync<ClueContacts>(q => q.Id == updateClueContactsReq.Id);
            {
                if (updateClueContactsReq.Name != null && updateClueContactsReq.Name != clueContacts.Name)
                {
                    mes = "'" + updateClueContactsReq.Name + "'联系人，原：联系人名称'" + clueContacts.Name + "'，修改为'" + updateClueContactsReq.Name + "'";
                }
                clueContacts.Name = updateClueContactsReq.Name;
                if (updateClueContactsReq.Tel1 != null && updateClueContactsReq.Tel1 != clueContacts.Tel1)
                {
                    mes += "'" + updateClueContactsReq.Name + "'联系方式一，原：联系方式一'" + clueContacts.Tel1 + "'，修改为'" + updateClueContactsReq.Tel1 + "'";
                }
                clueContacts.Tel1 = updateClueContactsReq.Tel1;
                if (updateClueContactsReq.Tel2 != null && updateClueContactsReq.Tel2 != clueContacts.Tel2)
                {
                    mes += "'" + updateClueContactsReq.Name + "'联系方式二，原：联系方式二'" + clueContacts.Tel2 + "'，修改为'" + updateClueContactsReq.Tel2 + "'";
                }
                clueContacts.Tel2 = updateClueContactsReq.Tel2;
                if (updateClueContactsReq.Role != "" && updateClueContactsReq.Role != clueContacts.Role)
                {
                    mes += "'" + updateClueContactsReq.Role + "'角色，原：角色'" + clueContacts.Role + "'，修改为'" + updateClueContactsReq.Role + "'，（0：决策者、1：普通人）";
                }
                clueContacts.Role = updateClueContactsReq.Role;
                if (updateClueContactsReq.Position != null && updateClueContactsReq.Position != clueContacts.Position)
                {
                    mes += "'" + updateClueContactsReq.Position + "'职位，原：职位'" + clueContacts.Position + "'，修改为'" + updateClueContactsReq.Position + "'";
                }
                if (updateClueContactsReq.Email != null && updateClueContactsReq.Email != clueContacts.Email)
                {
                    mes += "'" + updateClueContactsReq.Email + "'邮箱，原：邮箱'" + clueContacts.Email + "'，修改为'" + updateClueContactsReq.Email + "'";
                }
                clueContacts.Email = updateClueContactsReq.Email;
                clueContacts.Position = updateClueContactsReq.Position;
                clueContacts.Address1 = updateClueContactsReq.Address1;
                clueContacts.Address2 = updateClueContactsReq.Address2;
                clueContacts.UpdateUser = loginUser.Name;
                clueContacts.UpdateTime = DateTime.Now;

            };
            UnitWork.Update(clueContacts);
            UnitWork.Save();
            //日志
            var log = new AddClueLogReq();
            log.ClueId = clueContacts.Id;
            log.LogType = 1;
            log.CreateTime = DateTime.Now;
            log.CreateUser = loginUser.Name;
            log.Details = mes;
            await AddClueLogAsync(log);
            return true;
        }

        #endregion
        #region 日志

        /// <summary>
        /// 操作记录列表
        /// </summary>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        public async Task<List<ClueLogListDto>> ClueLogByIdAsync(int clueId, string StartTime, string EndTime)
        {
            Expression<Func<OpenAuth.Repository.Domain.Serve.ClueLog, bool>> exp = t => true;
            exp = exp.And(t => t.ClueId == clueId);
            if (!string.IsNullOrWhiteSpace(StartTime) && !string.IsNullOrWhiteSpace(EndTime))
            {
                DateTime startTime;
                DateTime.TryParse(StartTime, out startTime);
                DateTime endTime;
                DateTime.TryParse(EndTime, out endTime);
                exp = exp.And(t => t.CreateTime >= startTime && t.CreateTime < endTime.AddDays(1));

            }
            var result = UnitWork.Find<ClueLog>(exp).OrderByDescending(q => q.CreateTime).MapToList<ClueLogListDto>();
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


        public async Task<ShopProductDto> GetShopProduct(string ErpCode)
        {
            var url = $"http://shopapi.neware.work:8081/api/v1/product/shopproduct?erpCode={ErpCode}";
            ////使用注入的httpclientfactory获取client
            var client = _httpClient.CreateClient();
            client.BaseAddress = new Uri(url);
            //设置请求体中的内容，并以post的方式请求
            var response = await client.GetAsync(url);
            //获取请求到数据，并转化为字符串
            //result.Result = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<ShopProductDto>(response.Content.ReadAsStringAsync().Result);
        }


        // 通用文字识别（高精度版）
        public string accurateBasic(AccurateBasicReq accurateBasicReq)
        {

            var token = getAccessToken();
            string host = "https://aip.baidubce.com/rest/2.0/ocr/v1/accurate_basic?access_token=" + token.access_token;
            Encoding encoding = Encoding.Default;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(host);
            request.Method = "post";
            request.KeepAlive = true;
            // 图片的base64编码

            string str = "image=" + HttpUtility.UrlEncode(accurateBasicReq.files);
            byte[] buffer = encoding.GetBytes(str);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);
            string result = reader.ReadToEnd();
            Console.WriteLine("通用文字识别（高精度版）:");
            Console.WriteLine(result);
            return result;
        }

        public String getFileBase64(String fileName)
        {
            FileStream filestream = new FileStream(fileName, FileMode.Open);
            byte[] arr = new byte[filestream.Length];
            filestream.Read(arr, 0, (int)filestream.Length);
            string baser64 = Convert.ToBase64String(arr);
            filestream.Close();
            return baser64;
        }
        public static BaiduAccessToken getAccessToken()
        {
            String authHost = "https://aip.baidubce.com/oauth/2.0/token";
            HttpClient client = new HttpClient();
            List<KeyValuePair<String, String>> paraList = new List<KeyValuePair<string, string>>();
            paraList.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            paraList.Add(new KeyValuePair<string, string>("client_id", "zG4MnrnTv2gG8jo0yGgEu9Yw"));
            paraList.Add(new KeyValuePair<string, string>("client_secret", "9Tfd9AC2nmc0TM2NQmx1Ip7P0Xqk5OKH"));

            HttpResponseMessage response = client.PostAsync(authHost, new FormUrlEncodedContent(paraList)).Result;
            BaiduAccessToken result = JsonConvert.DeserializeObject<BaiduAccessToken>(response.Content.ReadAsStringAsync().Result);
            Console.WriteLine(result);
            return result;
        }

        /// <summary>
        /// 解析地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetAddressBasic(string address)
        {

            var token = getAccessTokenAddress();

            var url = $"https://aip.baidubce.com/rpc/2.0/nlp/v1/address?access_token=" + token.access_token;
            byte[] encodeBytes = Encoding.UTF8.GetBytes(address);
            string inputString = Encoding.UTF8.GetString(encodeBytes);
            //string urlcode = WebUtility.UrlEncode(address);
            var client = _httpClient.CreateClient();
            client.BaseAddress = new Uri(url);
            var content = new
            {
                text = inputString
            };
            HttpHelper httpHelper = new HttpHelper(url);
            return httpHelper.Post(content, url, "");
        }
        public static BaiduAccessToken getAccessTokenAddress()
        {
            String authHost = "https://aip.baidubce.com/oauth/2.0/token";
            HttpClient client = new HttpClient();
            List<KeyValuePair<String, String>> paraList = new List<KeyValuePair<string, string>>();
            paraList.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            paraList.Add(new KeyValuePair<string, string>("client_id", "YpGG9H3jIqhmg8bXmzFMvGx7"));
            paraList.Add(new KeyValuePair<string, string>("client_secret", "Zqi6u7NUbdiWAzEGXnBGe3nEkR2qsZH1"));

            HttpResponseMessage response = client.PostAsync(authHost, new FormUrlEncodedContent(paraList)).Result;
            BaiduAccessToken result = JsonConvert.DeserializeObject<BaiduAccessToken>(response.Content.ReadAsStringAsync().Result);
            Console.WriteLine(result);
            return result;
        }
        /// <summary>
        /// 递归
        /// </summary>
        /// <param name="pid">父级Id</param>
        /// <param name="demos">数据源</param>
        /// <returns></returns>
        private List<ClassificationDto> GetTypes(int pid, List<ClassificationDto> demos = null)
        {
            /*
             思路：1.从数据源中找到父级
                   2.循环父级并赋值，再循环父级时查找子集
                   3.如果有子集调用用GetMenu（父级Id,数据源）方法一层一层向下找
                   4.注意：（，是套娃模式。也就是循环第二层第三层的时候还是在一个父
                   级下面）
             */
            var parent = demos.Where(P => P.ParentId == pid);
            List<ClassificationDto> lists = new List<ClassificationDto>();
            foreach (var item in parent)
            {

                ClassificationDto DemosChilder = new ClassificationDto();
                DemosChilder.Id = item.Id;
                DemosChilder.Name = item.Name;
                DemosChilder.Level = item.Level;
                DemosChilder.ParentId = item.ParentId;
                DemosChilder.CreateTime = item.CreateTime;
                DemosChilder.CreateUser = item.CreateUser;
                DemosChilder.UpdateTime = item.UpdateTime;
                DemosChilder.UpdateUser = item.UpdateUser;
                DemosChilder.Children = GetSon(DemosChilder, demos);
                lists.Add(DemosChilder);
            }


            //找子集有就返回NUll，并执行Add
            List<ClassificationDto> GetSon(ClassificationDto demos, List<ClassificationDto> demosd = null)
            {
                if (!demosd.Exists(x => x.ParentId == demos.Id))
                {
                    return null;
                }
                else
                {
                    return GetTypes(demos.Id, demosd);
                }
            }
            return lists;
        }

        #endregion


        #region 意向商品
        /// <summary>
        /// 新增意向商品
        /// </summary>
        /// <param name="addClueIntentionProductReq"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> AddClueIntentionProductAsync(List<AddClueIntentionProductReq> addClueIntentionProductReq)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var clueIntentionProductList = new List<ClueIntentionProduct>();
            foreach (var item in addClueIntentionProductReq)
            {
                var codeinfo = await GetShopProduct(item.ItemCode);
                var clueIntentionProduct = new ClueIntentionProduct();

                clueIntentionProduct.ClueId = item.ClueId;
                clueIntentionProduct.ItemCode = item.ItemCode;
                clueIntentionProduct.ItemDescription = item.ItemDescription;
                clueIntentionProduct.AvailableStock = item.AvailableStock;
                clueIntentionProduct.UnitCost = item.UnitCost;
                if (codeinfo != null)
                {
                    clueIntentionProduct.ItemName = !string.IsNullOrWhiteSpace(codeinfo.ProductName) ? codeinfo.ProductName : "";
                    clueIntentionProduct.Pic = !string.IsNullOrWhiteSpace(codeinfo.MainImgUrl) ? codeinfo.MainImgUrl : "";
                    clueIntentionProduct.CommoditySellingPoint = !string.IsNullOrWhiteSpace(codeinfo.SellingPoints) ? codeinfo.SellingPoints : "";
                }
                clueIntentionProductList.Add(clueIntentionProduct);
                //日志
                var log = new AddClueLogReq();
                log.ClueId = item.ClueId;
                log.LogType = 0;
                log.CreateTime = DateTime.Now;
                log.CreateUser = loginUser.Name;
                log.Details = "'" + item.ItemCode + "',意向产品";
                await AddClueLogAsync(log);
            }

            await UnitWork.BatchAddAsync<ClueIntentionProduct, int>(clueIntentionProductList.ToArray());
            await UnitWork.SaveAsync();
            return true;
        }
        /// <summary>
        /// 意向商品列表
        /// </summary>
        /// <param name="clueId"></param>
        /// <returns></returns>
        public async Task<List<ClueIntentionProduct>> ClueIntentionProductByIdAsync(int clueId)
        {
            return UnitWork.Find<ClueIntentionProduct>(q => q.ClueId == clueId && !q.IsDelete).OrderByDescending(q => q.CreateTime).MapToList<ClueIntentionProduct>();
        }
        /// <summary>
        /// 删除意向商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> DeleteClueIntentionProductByIdAsync(List<int> ids)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            foreach (var item in ids)
            {
                var clueIntentionProduct = await UnitWork.FindSingleAsync<ClueIntentionProduct>(q => q.Id == item);
                if (clueIntentionProduct != null)
                {
                    clueIntentionProduct.IsDelete = true;
                    await UnitWork.UpdateAsync(clueIntentionProduct);
                    await UnitWork.SaveAsync();
                    //日志
                    var log = new AddClueLogReq();
                    log.ClueId = clueIntentionProduct.ClueId;
                    log.LogType = 2;
                    log.CreateTime = DateTime.Now;
                    log.CreateUser = loginUser.Name;
                    log.Details = "'" + clueIntentionProduct.ItemCode + "',意向产品";
                    await AddClueLogAsync(log);
                }
            }

            return true;
        }


        #endregion

        #region 分类字典
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="addClassificationReq"></param>
        /// <returns></returns>
        public async Task<string> AddClassificationAsync(AddClassificationReq addClassificationReq)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            //if (await UnitWork.IsExistAsync<ClueClassification>(u => u.Name == addClassificationReq.Name))
            //{
            //    return "名称已存在";
            //}
            var entity = await UnitWork.FindSingleAsync<ClueClassification>(q => q.Id == addClassificationReq.ParentId);
            var clueClassification = new ClueClassification
            {
                Name = addClassificationReq.Name,
            };
            if (entity != null)
            {
                clueClassification.Level = ++entity.Level;
                clueClassification.ParentId = addClassificationReq.ParentId;
            }
            else
            {
                clueClassification.Level = 0;
                clueClassification.ParentId = 0;
            }
            clueClassification.CreateTime = DateTime.Now;
            clueClassification.CreateUser = loginUser.Name;
            var data = await UnitWork.AddAsync<ClueClassification, int>(clueClassification);
            await UnitWork.SaveAsync();
            return data.Id.ToString();
        }

        //列表
        public async Task<List<ClassificationDto>> ClassificationAsync()
        {
            var entity = UnitWork.Find<ClueClassification>(q => !q.IsDelete).MapToList<ClassificationDto>();
            var res = GetTypes(0, entity);
            return res;
        }
        //详情
        public async Task<ClueClassification> GetClassificationByIdAsync(int id)
        {
            return await UnitWork.FindSingleAsync<ClueClassification>(q => q.Id == id);
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="updateClassificationReq"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<string> UpdateClassificationAsync(UpdateClassificationReq updateClassificationReq)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var clueClassification = await UnitWork.FindSingleAsync<ClueClassification>(q => q.Id == updateClassificationReq.Id);
            clueClassification.Name = updateClassificationReq.Name;
            clueClassification.UpdateTime = DateTime.Now;
            clueClassification.UpdateUser = loginUser.Name;
            await UnitWork.UpdateAsync(clueClassification);
            await UnitWork.SaveAsync();
            return "true";
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<string> DeleteClassificationByIdAsync(int id)
        {
            var clueClassification = await UnitWork.FindSingleAsync<ClueClassification>(q => q.Id == id);
            if (clueClassification != null)
            {
                clueClassification.IsDelete = true;
                await UnitWork.UpdateAsync(clueClassification);
                var clueClassificationchaid = UnitWork.Find<ClueClassification>(q => q.ParentId == id).MapToList<ClueClassification>();
                if (clueClassificationchaid.Count > 0)
                {
                    foreach (var item in clueClassificationchaid)
                    {
                        item.IsDelete = true;
                        await UnitWork.UpdateAsync(item);
                    }
                }
                await UnitWork.SaveAsync();
                return "true";

            }

            return "false";
        }
        public async Task<List<ClassificationDto>> IndustryDropDownAsync(string Name)
        {
            var model = await UnitWork.FindSingleAsync<ClueClassification>(q => q.Name == Name);
            var entity = UnitWork.Find<ClueClassification>(q => !q.IsDelete).MapToList<ClassificationDto>();
            var res = GetTypes(model.Id, entity);
            return res;
        }
        #endregion


        public string GetCompanyNameBasic(string name)
        {
            // token可以从 数据中心 -> 我的接口 中获取
            var token = "57fe81b8-35db-4363-b097-59dc1db82334";
            var url = $"http://open.api.tianyancha.com/services/open/search/2.0?word={name}&pageSize=20&pageNum=1";

            // 请求处理
            var responseStr = httpGet(url, token);
            return responseStr;
        }
        static String httpGet(String url, String token)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            // set header
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("Authorization", token);
            request.UserAgent = null;
            request.Headers = headers;
            request.Method = "GET";

            // response deal
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var httpStatusCode = (int)response.StatusCode;
            Console.WriteLine("返回码为 {0}", httpStatusCode);
            if (httpStatusCode == 200)
            {
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                return retString;
            }
            else
            {   // todo 可以通过返回码判断处理
                Console.WriteLine("未返回数据 {0}", httpStatusCode);
                throw new Exception("no data response");
            }

        }

    }
}
