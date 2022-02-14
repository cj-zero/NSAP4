using System;
using System.Data;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSAP.Entity.Client;
using NSAP.Entity.Sales;
using OpenAuth.App;
using OpenAuth.App.Client;
using OpenAuth.App.Client.Request;
using OpenAuth.App.Clue.Request;
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
        [HttpPost]
        [Route("GetPropertyName")]
        [AllowAnonymous]
        public DataTable GetPropertyName()
        {
            string strSql = string.Format("SELECT GroupCode AS PropertyCode,GroupName AS PropertyName FROM {0}.crm_OCQG WHERE sbo_id={1}", "nsap_base", "1");
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
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
        /// <summary>
        /// 根据jobId获取审核任务信息
        /// </summary>
        //[HttpGet]
        //[Route("GetAuditInfo")]
        //public Response<clientOCRD> GetAuditInfo(string jobId)
        //{

        //    var result = new Response<clientOCRD>();
        //    result.Result = _clientInfoApp.GetAuditInfoNew(jobId);
        //    return result;

        //}

        #endregion
    }
}
