using System;
using System.Data;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSAP.Entity.Client;
using OpenAuth.App;
using OpenAuth.App.Client;
using OpenAuth.App.Client.Request;
using OpenAuth.App.Interface;
using OpenAuth.App.Order;
using OpenAuth.App.Response;
using OpenAuth.Repository;
using OpenAuth.Repository.Interface;

namespace OpenAuth.WebApi.Controllers.Client
{
    /// <summary>
    /// 客户管理
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Client")]

    public class ClientController : Controller
    {
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        private readonly ClientInfoApp _clientInfoApp;
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
        public ClientController(ServiceSaleOrderApp serviceSaleOrderApp, ClientInfoApp clientInfoApp, IUnitWork UnitWork, ServiceBaseApp _serviceBaseApp, IAuth _auth)
        {
            this._clientInfoApp = clientInfoApp;
            this.UnitWork = UnitWork;
            this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            _serviceSaleOrderApp = serviceSaleOrderApp;

        }
        /// <summary>
        /// 新增/修改客户
        /// </summary>
        /// <param name="addClientInfoReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddClientAsync")]

        public async Task<Response<string>> SaveClient(AddClientInfoReq addClientInfoReq)
        {
            var result = new Response<string>();
            try
            {
                switch (addClientInfoReq.type)
                {
                    case "Add":
                        result.Result = await _clientInfoApp.AddClientAsync(addClientInfoReq, false);
                        break;
                    case "Edit":
                        result.Result = await _clientInfoApp.AddClientAsync(addClientInfoReq, true);
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 查询属性对应的名称
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPropertyName")]
        [AllowAnonymous]
        public TableData GetPropertyName()
        {
            var result = new TableData();
            string strSql = string.Format("SELECT GroupCode AS PropertyCode,GroupName AS PropertyName FROM {0}.crm_OCQG WHERE sbo_id={1}", "nsap_bone", "1");
            result.Data = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            return result;
        }
        /// <summary>
        /// 获取销售员信息
        /// </summary>
        [HttpGet]
        [Route("GetSellerInfo")]
        public TableData GetSellerInfo()
        {
            var result = new TableData();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);

            var dt = _clientInfoApp.GetSellerInfo(sboid.ToString(), userId.ToString());
            result.Data = dt;
            result.Count = dt.Rows.Count;
            return result;
        }
        #region 查询视图集合
        /// <summary>
        /// 查询视图集合
        /// </summary>
        [HttpPost]
        [Route("View")]
        public TableData GetClientList(ClientListReq clientListReq)
        {
            int rowCount = 0;
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            var depID = _serviceBaseApp.GetSalesDepID(userId);
            //bool rIsViewSelf = _serviceSaleOrderApp.GetPagePowersByUrl("client/ClientInfo.aspx", userId).ViewSelf;
            bool rIsViewSelf = true;
            bool rIsViewSelfDepartment = _serviceSaleOrderApp.GetPagePowersByUrl("client/ClientInfo.aspx", userId).ViewSelfDepartment;
            //bool rIsViewFull = _serviceSaleOrderApp.GetPagePowersByUrl("client/ClientInfo.aspx", userId).ViewFull;
            bool rIsViewFull = false;
            if (loginUser.Name == "韦京生" || loginUser.Name == "郭睿心")
            {
                rIsViewFull = true;
            }
            bool rIsViewSales = _serviceSaleOrderApp.GetPagePowersByUrl("client/ClientInfo.aspx", userId).ViewSales;
            result.Data = _clientInfoApp.SelectClientList(clientListReq.limit, clientListReq.page, clientListReq.query, clientListReq.sortname, clientListReq.sortorder, sboid, userId, rIsViewSales, rIsViewSelf, rIsViewSelfDepartment, rIsViewFull, depID, out rowCount);
            result.Count = rowCount;
            return result;

        }
        #endregion
        /// <summary>
        /// 根据jobId获取审核任务信息(我的创建/审批)
        /// </summary>
        [HttpGet]
        [Route("GetAuditInfo")]
        public Response<NSAP.Entity.Client.clientOCRD> GetAuditInfo(string jobId)
        {

            var result = new Response<clientOCRD>
            {
                Result = _clientInfoApp.GetAuditInfoNew(jobId)
            };
            return result;

        }
        /// <summary>
        /// 上级客户下拉
        /// </summary>
        [HttpGet]
        [Route("GetSuperClient")]
        public TableData GetSuperClient()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var sql = string.Format("SELECT A.CardCode,A. CardName FROM OCRD A LEFT JOIN OSLP B ON B.SlpCode=A.SlpCode WHERE b.SlpName='{0}' ORDER BY  A.CardCode", loginUser.Name);
            var result = new TableData
            {
                Data = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, sql, CommandType.Text, null)
            };
            return result;

        }
        /// <summary>
        /// 查询单个业务伙伴信息
        /// </summary>
        [HttpGet]
        [Route("SelectCrmClientInfo")]

        public TableData SelectCrmClientInfo(string CardCode, string IsOpenSap)
        {
            var result = new TableData();
            var UserId = _serviceBaseApp.GetUserNaspId();
            var SboId = _serviceBaseApp.GetUserNaspSboID(UserId);
            bool rIsViewSales = _serviceSaleOrderApp.GetPagePowersByUrl("client/ClientInfo.aspx", UserId).ViewSales;
            bool rIsOpenSap = IsOpenSap == "1" ? true : false;
            if (!string.IsNullOrEmpty(CardCode) && !string.IsNullOrEmpty(SboId.ToString()))
                result.Data = _clientInfoApp.SelectCrmClientInfo(CardCode, SboId.ToString(), rIsOpenSap, rIsViewSales);
            return result;
        }

        /// <summary>
        /// 查询业务伙伴的联系人
        /// </summary>
        [HttpGet]
        [Route("SelectClientContactData")]

        public TableData SelectClientContactData(string CardCode, string IsOpenSap)
        {

            var result = new TableData();
            var UserId = _serviceBaseApp.GetUserNaspId();
            var SboId = _serviceBaseApp.GetUserNaspSboID(UserId);
            bool rIsViewFull = _serviceSaleOrderApp.GetPagePowersByUrl("client/ClientInfo.aspx", UserId).ViewFull;
            bool rIsViewSelf = _serviceSaleOrderApp.GetPagePowersByUrl("client/ClientInfo.aspx", UserId).ViewSelf;
            bool rIsOpenSap = IsOpenSap == "1" ? true : false;
            if (!string.IsNullOrEmpty(CardCode) && !string.IsNullOrEmpty(SboId.ToString()))
                result.Data = _clientInfoApp.SelectClientContactData(CardCode, SboId.ToString(), rIsOpenSap, rIsViewFull);
            return result;
        }
        /// <summary>
        /// 查询业务伙伴的地址
        /// </summary>
        [HttpGet]
        [Route("SelectClientAddrData")]
        public TableData SelectClientAddrData(string CardCode, string SboId, string IsOpenSap)
        {
            var result = new TableData();
            bool rIsOpenSap = IsOpenSap == "1" ? true : false;
            if (!string.IsNullOrEmpty(CardCode) && !string.IsNullOrEmpty(SboId))
                result.Data = _clientInfoApp.SelectClientAddrData(CardCode, SboId, rIsOpenSap);
            return result;
        }

        /// <summary>
        /// 查询所有技术员
        /// </summary>
        [HttpPost]
        [Route("GetTcnicianInfo")]
        public TableData GetTcnicianInfo(GetTcnicianInfoReq getTcnicianInfoReq)
        {
            var result = new TableData();
            int rowsCount = 0;
            result.Data = _clientInfoApp.GetTcnicianInfo(getTcnicianInfoReq.limit, getTcnicianInfoReq.page, getTcnicianInfoReq.query, getTcnicianInfoReq.sortname, getTcnicianInfoReq.sortorder, getTcnicianInfoReq.SboId, "1", out rowsCount);
            result.Count = rowsCount;
            return result;

        }


    }

}

