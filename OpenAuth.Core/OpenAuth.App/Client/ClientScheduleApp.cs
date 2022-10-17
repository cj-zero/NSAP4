extern alias MySqlConnectorAlias;
using OpenAuth.App.Clue.Request;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using NSAP.Entity.Client;
using NSAP.Entity.Sales;
using OpenAuth.App.Client.Request;
using OpenAuth.App.Order;
using OpenAuth.Repository;
using billAttchment = NSAP.Entity.Sales.billAttchment;
using clientCRD1 = NSAP.Entity.Client.clientCRD1;
using clientOCPR = NSAP.Entity.Client.clientOCPR;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Serve;
using clientAcct1 = NSAP.Entity.Client.clientAcct1;
using OpenAuth.Repository.Domain.Customer;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.App.Client.Response;
using Microsoft.AspNetCore.SignalR;
using OpenAuth.App.SignalR;

namespace OpenAuth.App.Client
{
    public class ClientScheduleApp : OnlyUnitWorkBaeApp
    {
        ServiceBaseApp _serviceBaseApp;
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly ClientInfoApp _clientInfoapp;

        public ClientScheduleApp(ServiceSaleOrderApp serviceSaleOrderApp, ServiceBaseApp serviceBaseApp, IUnitWork unitWork, IAuth auth, IHubContext<MessageHub> hubContext, ClientInfoApp clientInfoapp) : base(unitWork, auth)
        {
            _serviceBaseApp = serviceBaseApp;
            _serviceSaleOrderApp = serviceSaleOrderApp;
            _hubContext = hubContext;
            _clientInfoapp = clientInfoapp;
        }

        /// <summary>
        /// 新增日程
        /// </summary>
        /// <param name="addClueFollowUpReq"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> AddClientScheduleAsync(ClientSchedule clientSchedule, bool isAdd)
        {
            var response = new Infrastructure.Response();
            response.Message = "";
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var userId = loginUser.User_Id.Value;
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            int SlpCode = Convert.ToInt16(GetUserInfoById(sboid.ToString(), userId.ToString(), "1"));
            var slpInfo = UnitWork.Find<OSLP>(q => q.SlpCode == SlpCode).FirstOrDefault();
            var SlpName = slpInfo == null ? "" : slpInfo.SlpName;
            if (isAdd)
            {
                clientSchedule.SlpCode = SlpCode;
                clientSchedule.SlpName = SlpName;
                clientSchedule.CreateUser = loginUser.Name;
                clientSchedule.CreateDate = DateTime.Now;
                clientSchedule.IsDelete = false;
                clientSchedule.IsRemind = false;
                await UnitWork.AddAsync<ClientSchedule, int>(clientSchedule);
            }
            else
            {
                ClientSchedule info = UnitWork.Find<ClientSchedule>(q => q.Id == clientSchedule.Id && !q.IsDelete).FirstOrDefault();
                info.CardCode = clientSchedule.CardCode;
                info.CardName = clientSchedule.CardName;
                info.SlpCode = SlpCode;
                info.SlpName = SlpName;
                info.Title = clientSchedule.Title;
                info.StartDate = clientSchedule.StartDate;
                info.EndDate = clientSchedule.EndDate;// NextFollowTime;
                info.Participants = clientSchedule.Participants;
                info.RemindType = clientSchedule.RemindType;
                info.RemindTime = clientSchedule.RemindTime;
                info.ScheduleType = clientSchedule.ScheduleType;
                info.ScheduleRemark = clientSchedule.ScheduleRemark;
                info.Remark = clientSchedule.Remark;
                info.UpdateUser = loginUser.Name;
                info.UpdateDate = DateTime.Now;
                await UnitWork.UpdateAsync<ClientSchedule>(info);
            }
            await UnitWork.SaveAsync();
            response.Message = "操作成功";
            return response;
        }


        /// <summary>
        /// 日程列表
        /// </summary>
        /// <param name="CardCode"></param>
        /// <returns></returns>
        public async Task<List<ClientSchedule>> ClientScheduleByIdAsync(string CardCode)
        {
            var result = new List<ClientSchedule>();

            var loginUser = _auth.GetCurrentUser().User;
            if (loginUser.Name == "韦京生" || loginUser.Name == "郭睿心" || loginUser.Name == "骆灵芝")
            {
                result = UnitWork.Find<ClientSchedule>(q => q.CardCode == CardCode && !q.IsDelete).OrderByDescending(t => t.CreateDate).MapToList<ClientSchedule>();
            }
            else
            {
                var userId = _serviceBaseApp.GetUserNaspId();
                var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
                int SlpCode = Convert.ToInt16(GetUserInfoById(sboid.ToString(), userId.ToString(), "1"));
                result = UnitWork.Find<ClientSchedule>(q => q.CardCode == CardCode && q.SlpCode == SlpCode && !q.IsDelete).OrderByDescending(t => t.CreateDate).MapToList<ClientSchedule>();
            }
            return result;
        }

        /// <summary>
        /// 删除日程
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> DeleteScheduleByCodeAsync(int Id)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var clientFollowUp = await UnitWork.FindSingleAsync<ClientSchedule>(q => q.Id == Id);

            clientFollowUp.IsDelete = true;
            await UnitWork.UpdateAsync(clientFollowUp);
            await UnitWork.SaveAsync();

            return true;
        }

        private string GetUserInfoById(string SboId, string UserId, string SeeType)
        {
            string rRoleNm = "", rFiledName = "";
            if (SeeType == "1")
            {
                rFiledName = "A.sale_id";
                rRoleNm = "销售";
            }
            else if (SeeType == "2")
            {
                rFiledName = "A.sale_id";
                rRoleNm = "采购";
            }
            else if (SeeType == "3")
            {
                rFiledName = "A.tech_id";
                rRoleNm = "技术";
            }
            else
            {
                return "0";
            }
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT {0} FROM {1}.sbo_user A ", rFiledName, "nsap_base");
            strSql.AppendFormat("LEFT JOIN {0}.base_user_role B ON A.user_id=B.user_id  ", "nsap_base");
            strSql.AppendFormat("LEFT JOIN {0}.base_role C ON B.role_id=C.role_id ", "nsap_base");
            strSql.AppendFormat("WHERE A.sbo_id={0} AND A.user_id={1} AND C.role_nm LIKE '%{2}%' AND {3}>0 GROUP BY A.user_id ", SboId, UserId, rRoleNm, rFiledName);
            object strObj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
            return strObj == null ? "0" : strObj.ToString();
        }

    }
}

