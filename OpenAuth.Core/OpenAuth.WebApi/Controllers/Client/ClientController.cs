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
using OpenAuth.App.Client.Response;
using OpenAuth.App.Interface;
using OpenAuth.App.Order;
using OpenAuth.App.Response;
using OpenAuth.Repository;
using OpenAuth.Repository.Interface;
using Serilog;

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

        public ClientController(ServiceSaleOrderApp serviceSaleOrderApp, ClientInfoApp clientInfoApp,
            IUnitWork UnitWork, ServiceBaseApp _serviceBaseApp, IAuth _auth)
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
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{addClientInfoReq.ToJson()}， 错误：{result.Message}");
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
            try
            {
                string strSql =
                    string.Format(
                        "SELECT GroupCode AS PropertyCode,GroupName AS PropertyName FROM {0}.crm_OCQG WHERE sbo_id={1}",
                        "nsap_bone", "1");
                result.Data = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{null}， 错误：{result.Message}");
            }

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
            try
            {
                var userId = _serviceBaseApp.GetUserNaspId();
                var sboid = _serviceBaseApp.GetUserNaspSboID(userId);

                var dt = _clientInfoApp.GetSellerInfo(sboid.ToString(), userId.ToString());
                result.Data = dt;
                result.Count = dt.Rows.Count;
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{null}， 错误：{result.Message}");
            }

            return result;
        }



        /// <summary>
        /// 查询客户列表
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
            bool rIsViewSelfDepartment = _serviceSaleOrderApp.GetPagePowersByUrl("client/ClientInfo.aspx", userId)
                .ViewSelfDepartment;
            //bool rIsViewFull = _serviceSaleOrderApp.GetPagePowersByUrl("client/ClientInfo.aspx", userId).ViewFull;
            bool rIsViewFull = false;
            if (loginUser.Name == "韦京生" || loginUser.Name == "郭睿心"|| loginUser.Name == "骆灵芝")
            {
                rIsViewFull = true;
            }

            //bool rIsViewSales = _serviceSaleOrderApp.GetPagePowersByUrl("client/ClientInfo.aspx", userId).ViewSales;
            bool rIsViewSales = true;
            try
            {
                result.Data = _clientInfoApp.SelectClientList(clientListReq.limit, clientListReq.page,
                    clientListReq.query, clientListReq.sortname, clientListReq.sortorder, sboid, userId, rIsViewSales,
                    rIsViewSelf, rIsViewSelfDepartment, rIsViewFull, depID, clientListReq.Label, out rowCount);
                result.Count = rowCount;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{clientListReq.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取各个状态的客户数量
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCustomerCount")]
        public async Task<TableData> GetCustomerCount()
        {
            var result = new TableData();
            try
            {
                result.Data = await _clientInfoApp.GetCustomerCount();
            }
            catch(Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex?.Message ?? "";
            }

            return result;
        }

        /// <summary>
        /// 根据jobId获取审核任务信息(我的创建/审批)
        /// </summary>
        [HttpGet]
        [Route("GetAuditInfo")]
        public Response<NSAP.Entity.Client.clientOCRD> GetAuditInfo(string jobId)
        {
            var result = new Response<clientOCRD>();
            try
            {
                result.Result = _clientInfoApp.GetAuditInfoNew(jobId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{jobId.ToJson()}， 错误：{result.Message}");
            }

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

            var result = new TableData();

            try
            {
                var loginUser = loginContext.User;
                var sql = string.Format(
                    "SELECT A.CardCode,A. CardName FROM OCRD A LEFT JOIN OSLP B ON B.SlpCode=A.SlpCode WHERE b.SlpName='{0}' ORDER BY  A.CardCode",
                    loginUser.Name);
                result.Data = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, sql, CommandType.Text, null);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{null}， 错误：{result.Message}");
            }

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

            try
            {
                var UserId = _serviceBaseApp.GetUserNaspId();
                var SboId = _serviceBaseApp.GetUserNaspSboID(UserId);
                //bool rIsViewSales = _serviceSaleOrderApp.GetPagePowersByUrl("client/ClientInfo.aspx", UserId).ViewSales;
                bool rIsViewSales = true;
                bool rIsOpenSap = IsOpenSap == "1" ? true : false;
                if (!string.IsNullOrEmpty(CardCode) && !string.IsNullOrEmpty(SboId.ToString()))
                    result.Data =
                        _clientInfoApp.SelectCrmClientInfo(CardCode, SboId.ToString(), rIsOpenSap, rIsViewSales);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{CardCode}， 错误：{result.Message}");
            }

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
            try
            {
                var UserId = _serviceBaseApp.GetUserNaspId();
                var SboId = _serviceBaseApp.GetUserNaspSboID(UserId);
                bool rIsViewFull = _serviceSaleOrderApp.GetPagePowersByUrl("client/ClientInfo.aspx", UserId).ViewFull;
                bool rIsViewSelf = _serviceSaleOrderApp.GetPagePowersByUrl("client/ClientInfo.aspx", UserId).ViewSelf;
                bool rIsOpenSap = IsOpenSap == "1" ? true : false;
                if (!string.IsNullOrEmpty(CardCode) && !string.IsNullOrEmpty(SboId.ToString()))
                    result.Data =
                        _clientInfoApp.SelectClientContactData(CardCode, SboId.ToString(), rIsOpenSap, rIsViewFull);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{CardCode}， 错误：{result.Message}");
            }

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
            try
            {
                bool rIsOpenSap = IsOpenSap == "1" ? true : false;
                if (!string.IsNullOrEmpty(CardCode) && !string.IsNullOrEmpty(SboId))
                    result.Data = _clientInfoApp.SelectClientAddrData(CardCode, SboId, rIsOpenSap);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{CardCode}， 错误：{result.Message}");
            }

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
            try
            {
                result.Data = _clientInfoApp.GetTcnicianInfo(getTcnicianInfoReq.limit, getTcnicianInfoReq.page,
                    getTcnicianInfoReq.query, getTcnicianInfoReq.sortname, getTcnicianInfoReq.sortorder,
                    getTcnicianInfoReq.SboId, "1", out rowsCount);
                result.Count = rowsCount;
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{getTcnicianInfoReq.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 修改方法与新增方法
        /// </summary>
        [HttpPost]
        [Route("UpdateClientJob")]
        public Response<string> UpdateClientJob(UpdateClientJobReq updateClientJobReq)
        {
            var result = new Response<string>();
            try
            {
                result.Result = _clientInfoApp.UpdateClientJob(updateClientJobReq);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{updateClientJobReq.ToJson()}， 错误：{result.Message}");
            }

            return result;

        }



        /// <summary>
        /// 保存业务伙伴审核的录入方案
        /// </summary>
        [HttpPost]
        [Route("SaveCrmAuditInfo")]

        public Response<string> SaveCrmAuditInfo(SaveCrmAuditInfoReq saveCrmAuditInfoReq)
        {
            var result = new Response<string>();
            try
            {
                if (!string.IsNullOrEmpty(saveCrmAuditInfoReq.AuditType) &&
                    !string.IsNullOrEmpty(saveCrmAuditInfoReq.JobId) &&
                    ((saveCrmAuditInfoReq.AuditType == "Edit" && !string.IsNullOrEmpty(saveCrmAuditInfoReq.CardCode)) ||
                     saveCrmAuditInfoReq.AuditType == "Add"))
                    result.Result = _clientInfoApp.SaveCrmAuditInfo(saveCrmAuditInfoReq.AuditType,
                        saveCrmAuditInfoReq.CardCode, saveCrmAuditInfoReq.DfTcnician, saveCrmAuditInfoReq.JobId);
                else
                    result.Result = "0";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{saveCrmAuditInfoReq.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 审核
        /// </summary>
        [HttpPost]
        [Route("AuditResubmitNext")]
        public Response<string> AuditResubmitNext(AuditResubmitNextReq auditResubmitNextReq)
        {
            var result = new Response<string>();
            var UserId = _serviceBaseApp.GetUserNaspId();
            try
            {
                result.Result = _clientInfoApp.AuditResubmitNext(auditResubmitNextReq.jobId, UserId,
                    auditResubmitNextReq.recommend, auditResubmitNextReq.auditOpinionid);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{auditResubmitNextReq.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }


        /// <summary>
        /// 查询 国家·省·市
        /// </summary>
        [HttpPost]
        [Route("GetStateProvincesInfo")]
        public TableData GetStateProvincesInfo(GetStateProvincesInfoReq getStateProvincesInfoReq)
        {
            var result = new TableData();
            try
            {

                int rowCounts = 0;
                if (!string.IsNullOrEmpty(getStateProvincesInfoReq.AddrType) &&
                    (getStateProvincesInfoReq.AddrType == "1" ||
                     (getStateProvincesInfoReq.AddrType == "2" &&
                      !string.IsNullOrEmpty(getStateProvincesInfoReq.CountryId)) ||
                     (getStateProvincesInfoReq.AddrType == "3" &&
                      !string.IsNullOrEmpty(getStateProvincesInfoReq.StateId))))
                    result.Data = _clientInfoApp.GetStateProvincesInfo(getStateProvincesInfoReq.limit,
                        getStateProvincesInfoReq.page, getStateProvincesInfoReq.query,
                        getStateProvincesInfoReq.sortname, getStateProvincesInfoReq.sortorder,
                        getStateProvincesInfoReq.AddrType, getStateProvincesInfoReq.CountryId,
                        getStateProvincesInfoReq.StateId, out rowCounts);
                result.Count = rowCounts;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{getStateProvincesInfoReq.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 查询指定城市的邮编
        /// </summary>
        [HttpGet]
        [Route("GetClientCityZipCode")]
        public Response<string> GetClientCityZipCode(string CityId)
        {
            var result = new Response<string>();
            try
            {
                if (!string.IsNullOrEmpty(CityId))
                    result.Result = _clientInfoApp.GetClientCityZipCode(CityId);
                else
                    result.Result = "";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{CityId.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 查询是否提交过指定客户的变更信息
        /// </summary>
        [HttpGet]
        [Route("GetAuditCardNameExsit")]
        public Response<string> GetAuditCardNameExsit(string OperaType, string SboId, string CardCode, string CardName)
        {
            var UserId = _serviceBaseApp.GetUserNaspId();
            var result = new Response<string>();
            try
            {
                if (!string.IsNullOrEmpty(OperaType) && !string.IsNullOrEmpty(SboId) && !string.IsNullOrEmpty(CardName))
                {
                    result.Result = _clientInfoApp.GetAuditCardNameExsit(OperaType, SboId, UserId.ToString(), CardCode, CardName);
                }
                else { result.Result = "2"; }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{CardName.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 查询审核中的客户名是否存在相同
        /// </summary>
        [HttpGet]
        [Route("GetAuditCardNameSimilar")]
        public Response<string> GetAuditCardNameSimilar(string SboId, string CardName)
        {
            var result = new Response<string>();
            try
            {
                if (!string.IsNullOrEmpty(SboId) && !string.IsNullOrEmpty(CardName))
                {
                    result.Result = _clientInfoApp.GetAuditCardNameSimilar(SboId, CardName);
                }
                else { result.Result = "2"; }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{CardName.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 检验公司名称简称是否存在相似
        /// </summary>
        [HttpGet]
        [Route("CheckCardNameSimilar")]
        public Response<string> CheckCardNameSimilar(string SboId, string CardName, string CardCode, string CheckType)
        {
            var result = new Response<string>();
            try
            {
                if (!string.IsNullOrEmpty(SboId) && !string.IsNullOrEmpty(CardName) && !string.IsNullOrEmpty(CheckType))
                {
                    if (CheckType == "1")
                    {
                        result.Result = _clientInfoApp.CheckCardNameSimilar(SboId, "CardName", CardName, false, "");
                    }
                    else if (CheckType == "2")
                    {
                        result.Result = _clientInfoApp.CheckCardNameSimilar(SboId, "CardName", CardName, true, CardCode);
                    }
                    else if (CheckType == "3")
                    {
                        result.Result = _clientInfoApp.CheckCardNameSimilar(SboId, "U_Name", CardName, false, "");
                    }
                    else if (CheckType == "4")
                    {
                        result.Result = _clientInfoApp.CheckCardNameSimilar(SboId, "U_Name", CardName, true, CardCode);
                    }
                    else { result.Result = "2"; }
                }
                else { result.Result = "2"; }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{CardName.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 相似客户
        /// </summary>
        [HttpPost]
        [Route("CheckCardSimilar")]
        public TableData CheckCardSimilar(CheckCardSimilarReq checkCardSimilarReq)
        {
            var result = new TableData();
            try
            {
                bool isSearchAll = checkCardSimilarReq.SearchAll == "1" ? true : false;
                result.Data = _clientInfoApp.CheckCardSimilar(checkCardSimilarReq.query, checkCardSimilarReq.JobId, isSearchAll);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{checkCardSimilarReq.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 查询业务伙伴附件
        /// </summary>
        [HttpGet]
        [Route("SelectClientFiles")]
        public TableData SelectClientFiles(string FileType, string SboId, string IssueReason)
        {
            var result = new TableData();
            try
            {
                if (!string.IsNullOrEmpty(FileType) && !string.IsNullOrEmpty(SboId) && !string.IsNullOrEmpty(IssueReason))
                    result.Data = _clientInfoApp.SelectClientFiles(FileType, SboId, IssueReason);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{FileType.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 查询业务伙伴报价单
        /// </summary>
        [HttpPost]
        [Route("SelectOqut")]
        public TableData SelectOqut(SelectOqutReq selectOqutReq)
        {
            var result = new TableData();
            try
            {
                if (!string.IsNullOrEmpty(selectOqutReq.CardCode))
                    result.Data = _clientInfoApp.SelectOqut(selectOqutReq);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{selectOqutReq.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 查询业务伙伴订单
        /// </summary>
        [HttpPost]
        [Route("SelectOrdr")]
        public TableData SelectOrdr(SelectOrdrReq selectOrdrReq)
        {
            var result = new TableData();
            try
            {
                if (!string.IsNullOrEmpty(selectOrdrReq.CardCode))
                    result.Data = _clientInfoApp.SelectOrdr(selectOrdrReq);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{selectOrdrReq.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 线索同步接口
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [Route("Synchronous")]
        public async Task<Response<bool>> Synchronous(int ClueId)
        {
            var result = new Response<bool>();
            try
            {
                    result.Result =await _clientInfoApp.Synchronous(ClueId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ClueId.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

    }
}

