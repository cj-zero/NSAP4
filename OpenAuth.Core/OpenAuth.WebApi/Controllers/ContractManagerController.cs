using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.ContractManager;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.App.Sap.BusinessPartner;
using OpenAuth.App.ContractManager.Request;
using OpenAuth.App.Order;
using OpenAuth.App.Order.Request;
using OpenAuth.Repository;
using OpenAuth.Repository.Interface;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 合同管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "General")]
    public class ContractManagerController : ControllerBase
    {
        private readonly ContractApplyApp _contractapplyapp;
        private readonly ContractSealApp _contractSealApp;
        private readonly ContractTemplateApp _contractTemplateApp;
        private readonly BusinessPartnerApp _businessPartnerApp;
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
        private readonly FileApp _fileApp;
        private readonly ServiceBaseApp _serviceBaseApp;
        private readonly ServiceOrderApp _serviceOrderApp;
        IAuth _auth;
        IUnitWork UnitWork;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="contractapplyapp"></param>
        /// <param name="businessPartnerApp"></param>
        /// <param name="UnitWork"></param>
        /// <param name="auth"></param>
        /// <param name="serviceSaleOrderApp"></param>
        /// <param name="serviceBaseApp"></param>
        /// <param name="contractSealApp"></param>
        /// <param name="contractTemplateApp"></param>
        public ContractManagerController(ServiceOrderApp serviceOrderApp, ContractApplyApp contractapplyapp, IUnitWork UnitWork, IAuth auth, ServiceSaleOrderApp serviceSaleOrderApp,
            ServiceBaseApp serviceBaseApp, ContractSealApp contractSealApp, FileApp fileApp, ContractTemplateApp contractTemplateApp, BusinessPartnerApp businessPartnerApp)
        {
            this._serviceOrderApp = serviceOrderApp;
            this._contractapplyapp = contractapplyapp;
            this._businessPartnerApp = businessPartnerApp;
            this._serviceBaseApp = serviceBaseApp;
            this._serviceSaleOrderApp = serviceSaleOrderApp;
            this._contractSealApp = contractSealApp;
            this._contractTemplateApp = contractTemplateApp;
            this.UnitWork = UnitWork;
            this._auth = auth;
            this._fileApp = fileApp;
        }

        #region 合同申请
        /// <summary>
        /// 获取合同申请单列表
        /// </summary>
        /// <param name="request">合同申请单查询实体数据</param>
        /// <returns>成功返回合同申请单列表信息，失败返回异常信息</returns>
        [HttpPost]
        public async Task<TableData> GetContractApplyList(QueryContractApplyListReq request)
        {
            var result = new TableData();
            try
            {
                return await _contractapplyapp.GetContractApplyList(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取合同申请单详情信息
        /// </summary>
        /// <param name="contractId">合同申请单Id</param>
        /// <returns>成功返回合同申请单详情信息，失败返回异常信息</returns>
        [HttpGet]
        public async Task<TableData> GetDetails(string contractId)
        {
            var result = new TableData();
            try
            {
                return await _contractapplyapp.GetDetails(contractId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{contractId.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 客户详情/销售报价单/销售订单的合同申请
        /// </summary>
        /// <param name="ModuleType">模块类型(客户-KHXQ,销售报价单-XSBJD,销售订单-XSDD)</param>
        /// <param name="ModuleCode">模块编码</param>
        /// <returns>返回客户详情/销售报价单/销售订单对应的合同申请单</returns>
        [HttpGet]
        public async Task<TableData> GetCardOrOrderDraftOrSaleOrderContract(string ModuleType, string ModuleCode)
        {
            var result = new TableData();
            try
            {
                return await _contractapplyapp.GetCardOrOrderDraftOrSaleOrderContract(ModuleType, ModuleCode);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ModuleType.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 添加合同申请单
        /// </summary>
        /// <param name="obj">合同申请单实体数据</param>
        /// <returns>成功返回操作成功，失败返回异常信息</returns>
        [HttpPost]
        public async Task<Response> Add(ContractApply obj)
        {
            var result = new Response();
            try
            {
                var Message = await _contractapplyapp.Add(obj);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 修改合同申请单
        /// </summary>
        /// <param name="obj">合同申请单实体数据</param>
        /// <returns>成功返回操作成功，失败返回异常信息</returns>
        [HttpPost]
        public async Task<Response> Update(ContractApply obj)
        {
            var result = new Response();
            try
            {
                var Message = await _contractapplyapp.UpDate(obj);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 删除合同申请单
        /// </summary>
        /// <param name="contractId">合同申请单Id</param>
        /// <returns>成功返回操作成功，失败返回异常信息</returns>
        [HttpPost]
        public async Task<Response> Delete(string contractId)
        {
            var result = new Response();
            try
            {
                var Message = await _contractapplyapp.Delete(contractId);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{contractId.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 批量删除合同申请单
        /// </summary>
        /// <param name="contractIdList">合同申请单Id集合</param>
        /// <returns>成功返回操作成功，失败返回异常信息</returns>
        [HttpPost]
        public async Task<Response> Deletes(List<string> contractIdList)
        {
            var result = new Response();
            try
            {
                var Message = await _contractapplyapp.Deletes(contractIdList);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{contractIdList.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 审批
        /// </summary>
        /// <param name="req">合同申请审批实体数据</param>
        /// <returns>成功返回，失败抛出异常</returns>
        [HttpPost]
        public async Task<TableData> Accraditation(AccraditationContractApplyReq req)
        {
            var result = new TableData();
            try
            {
                result = await _contractapplyapp.Accraditation(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 提交给我的
        /// </summary>
        /// <param name="request">查询合同申请单集合实体数据</param>
        /// <returns>返回TableData信息</returns>
        [HttpPost]
        public async Task<TableData> GetMyAccraditation(QueryContractApplyListReq request)
        {
            var result = new TableData();
            try
            {
                result = await _contractapplyapp.GetMyAccraditation(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取客户信息
        /// </summary>
        /// <returns>返回TableData信息</returns>
        [HttpPost]
        [Route("GetCustomerMsg")]
        public async Task<TableData> GetCustomerMsg(CardCodeRequest request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            var userId = _serviceBaseApp.GetUserNaspId();
            var depId = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, $"SELECT dep_id value FROM base_user_detail WHERE user_id = {userId}", CommandType.Text, null).FirstOrDefault();
            string type = "SQO";
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);//UnitWork.ExcuteSql<sbo_info>(ContextType.Nsap4ServeDbContextType, "SELECT sbo_id FROM nsap_base.sbo_info WHERE is_curr = 1 AND valid = 1 LIMIT 1;", CommandType.Text, null).FirstOrDefault()?.sbo_id;
            var dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT sql_db,sql_name,sql_pswd,sap_name,sap_pswd,sql_conn,is_open FROM nsap_base.sbo_info WHERE sbo_id={sboid}", CommandType.Text, null);
            DataTable rDataRowsSlp = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $@"SELECT sale_id,tech_id FROM nsap_base.sbo_user WHERE user_id={userId} AND sbo_id={sboid}", CommandType.Text, null);
            string dRowData = string.Empty;
            string isOpen = "0";
            string sqlcont = string.Empty;
            string sboname = string.Empty;
            string sortString = string.Empty;
            string filterString = string.Empty;
            string sortName = string.Empty;
            if (dt.Rows.Count > 0)
            {
                isOpen = dt.Rows[0][6].ToString();
                sqlcont = dt.Rows[0][5].ToString();
                sboname = dt.Rows[0][0].ToString();
            }

            if (!string.IsNullOrEmpty(request.SortOrder) && !string.IsNullOrEmpty(request.SortName))
            {
                request.SortName = request.SortName.Replace("cardcode", "a.cardcode").Replace("cardname", "a.cardname").Replace("slpname", "b.slpname").Replace("currency", "a.currency").Replace("balance", "a.balance");
                sortString = string.Format("{0} {1}", request.SortName, request.SortOrder.ToUpper());
            }
            else
            {
                sortString = " a.cardcode asc ";
            }

            if (!string.IsNullOrWhiteSpace(request.CardCode))
            {
                filterString += string.Format("(a.CardCode  LIKE '%{0}%' OR a.CardName LIKE '%{0}%') AND ", request.CardCode);
            }

            if (!loginContext.Roles.Any(r => r.Name.Equals("管理员")))
            {
                filterString += string.Format(" a.SlpCode = '{0}' AND ", rDataRowsSlp.Rows[0][0].ToString());
            }

            var powerList = UnitWork.ExcuteSql<PowerDto>(ContextType.NsapBaseDbContext, $@"SELECT a.func_id funcID,b.page_url pageUrl,a.auth_map authMap FROM (SELECT a.func_id,a.page_id,b.auth_map FROM nsap_base.base_func a INNER JOIN (SELECT t.func_id,BIT_OR(t.auth_map) auth_map FROM (SELECT func_id,BIT_OR(auth_map) auth_map FROM nsap_base.base_role_func WHERE role_id IN (SELECT role_id FROM nsap_base.base_user_role WHERE user_id={userId}) GROUP BY func_id UNION ALL SELECT func_id,auth_map FROM nsap_base.base_user_func WHERE user_id={userId}) t GROUP BY t.func_id) b ON a.func_id=b.func_id) AS a INNER JOIN nsap_base.base_page AS b ON a.page_id=b.page_id", CommandType.Text, null);
            bool viewFull = false;
            bool viewSelf = false;
            bool viewSelfDepartment = false;
            bool viewSales = false;
            bool viewCustom = false;
            var power = powerList.FirstOrDefault(s => s.PageUrl == "sales/SalesQuotation.aspx");
            if (power != null)
            {
                Powers powers = new Powers(power.AuthMap);
                viewFull = powers.ViewFull;
                viewSelf = powers.ViewSelf;
                viewSelfDepartment = powers.ViewSelfDepartment;
                viewSales = powers.ViewSales;
                viewCustom = powers.ViewCustom;
            }

            #region 根据不同的单据类型获取不同的业务伙伴
            if (!string.IsNullOrEmpty(type))
            {
                if (type == "SQO")//销售报价单\订单
                {
                    filterString += string.Format("(a.CardType='C' OR a.CardType='L') AND ");
                }
                else if (type == "SDR")//销售交货\退货,应收发票\贷项凭证
                {
                    filterString += string.Format("a.CardType='C' AND ");
                }
                else if (type == "P")//采购
                {
                    filterString += string.Format("a.CardType='S' AND ");
                }
                else if (type == "ST")//库存转储
                {
                    filterString += string.Format("a.SlpCode IN (SELECT sale_id FROM nsap_base.sbo_user WHERE user_id={0}) AND ", userId);
                }
                else if (type == "TR")//运输页面只能选择运输供应商
                {
                    filterString += string.Format("a.CardType='S' AND a.QryGroup1='Y' AND ");
                }
            }

            //判断是否机械类
            string CardTypeFilter = string.Empty;
            string DfTcnician = "";

            if (rDataRowsSlp.Rows.Count > 0)
            {
                string slpCode = rDataRowsSlp.Rows[0][0].ToString();
                DfTcnician = rDataRowsSlp.Rows[0][1].ToString();
                if (type == "P")
                {
                    string filter_str = string.Empty;
                    var isMechanical = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, $@" SELECT count(*) value FROM nsap_bone.crm_OCQG_assign WHERE sbo_id={sboid} AND SlpCode='{slpCode}' AND GroupCode='2' ", CommandType.Text, null).FirstOrDefault();
                    if (isMechanical != null && Convert.ToBoolean(isMechanical.Value))
                    {
                        filter_str = string.Format(" OR ( a.SlpCode='-1' and (a.QryGroup2='Y' OR a.QryGroup3='Y')) OR a.CardCode IN ('V00733','V00735','V00836') ");
                    }
                    else
                    {
                        filter_str = string.Format(" OR ( a.SlpCode='-1' and a.QryGroup2='N') OR a.CardCode IN ('V00733','V00735','V00836') ");
                    }

                    CardTypeFilter = filter_str;
                }
            }
            #endregion

            if (viewSelfDepartment && !viewFull)
            {
                var rDataRows = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT a.sale_id,a.tech_id FROM nsap_base.sbo_user a LEFT JOIN nsap_base.base_user_detail b ON a.user_id=b.user_id WHERE b.dep_id={depId} AND a.sbo_id={userId}", CommandType.Text, null);
                if (rDataRows.Rows.Count > 0)
                {
                    filterString += string.Format(" (a.SlpCode IN(");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][0]);
                    }

                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(") OR a.DfTcnician IN (");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][1]);
                    }

                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    }

                    filterString += string.Format(") {0} ) AND ", CardTypeFilter);
                }
            }

            if (viewSelf && !viewFull && !viewSelfDepartment)
            {
                var rDataRowsSlp1 = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT sale_id,tech_id FROM nsap_base.sbo_user WHERE user_id={userId} AND sbo_id={sboid}", CommandType.Text, null);
                if (rDataRowsSlp1.Rows.Count > 0)
                {
                    string slpCode = rDataRowsSlp1.Rows[0][0].ToString();
                    filterString += string.Format(" (a.SlpCode = {0} OR a.DfTcnician={1} {2} ) AND ", slpCode, DfTcnician, CardTypeFilter);
                }
                else
                {
                    filterString += string.Format(" a.SlpCode =0  AND ");
                }
            }

            if (!string.IsNullOrEmpty(filterString))
            {
                filterString = filterString.Substring(0, filterString.Length - 5);
            }

            if (isOpen == "0")
            {
                filterString += string.Format(" AND a.sbo_id = {0}", sboid);
                // return NSAP.Data.Sales.BillDelivery.SelectCardCodeView(out rowCount, pageSize, pageIndex, filterString, sortString).FelxgridDataToJSON(pageIndex.ToString(), rowCount.ToString());
            }
            else
            {
                result = _serviceSaleOrderApp.SelectCardCodeInfo(request, sortString, filterString, sboname);
            }

            return result;
        }

        /// <summary>
        /// 获取销售单信息
        /// </summary>
        /// <returns>返回TableData信息</returns>
        [HttpPost]
        [Route("GetSaleMsg")]
        public TableData GetSaleMsg(SalesOrderListReq model)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            int rowCount = 0;
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            var DepID = _serviceBaseApp.GetSalesDepID(UserID);
            var result = new TableData();
            bool rata = false;
            if (loginUser.Name == "韦京生" || loginUser.Name == "郭睿心" || loginUser.Name == "唐琴" || loginUser.Name == "ErpAdmin")
            {
                rata = true;
            }

            string type = "ORDR";
            DataTable dt = _serviceSaleOrderApp.GetSboNamePwd(SboID);
            string dRowData = string.Empty; string isOpen = "0"; string sqlcont = string.Empty; string sboname = string.Empty;
            if (dt.Rows.Count > 0)
            {
                isOpen = dt.Rows[0][6].ToString();
                sqlcont = dt.Rows[0][5].ToString(); sboname = dt.Rows[0][0].ToString();
            }

            try
            {
                if (isOpen == "1")
                {
                    DataTable dts = _contractapplyapp.SelectBillListInfo_ORDR(loginContext.Roles, out rowCount, model, type, rata, true, UserID, SboID, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesOrder.aspx", UserID).ViewSelfDepartment, DepID, true, true, sqlcont, sboname);
                    result.Data = dts;
                    result.Count = rowCount;
                }
                else
                {
                    DataTable dts = _contractapplyapp.SelectBillViewInfo(out rowCount, model.limit, model.page, "", model.sortname, model.sortorder, type, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesOrder.aspx", UserID).ViewFull, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesOrder.aspx", UserID).ViewSelf, UserID, SboID, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesOrder.aspx", UserID).ViewSelfDepartment, DepID, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesOrder.aspx", UserID).ViewCustom, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesOrder.aspx", UserID).ViewSales);
                    result.Data = dts;
                    result.Count = rowCount;
                }
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }

            return result;
        }

        /// <summary>
        /// 加载销售报价单列表
        /// </summary>
        /// <param name="request">查询销售报价单实体数据</param>
        /// <returns>返回TableData信息</returns>
        [HttpPost]
        [Route("GetSaleQuotation")]
        public async Task<TableData> GetSaleQuotation(QuerySalesQuotationReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            var userId = _serviceBaseApp.GetUserNaspId();
            var depId = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, $"SELECT dep_id value FROM base_user_detail WHERE user_id = {userId}", CommandType.Text, null).FirstOrDefault();
            string type = "OQUT";
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            var dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT sql_db,sql_name,sql_pswd,sap_name,sap_pswd,sql_conn,is_open FROM nsap_base.sbo_info WHERE sbo_id={sboid}", CommandType.Text, null);
            string dRowData = string.Empty;
            string isOpen = "0";
            string sqlcont = string.Empty;
            string sboname = string.Empty;
            if (dt.Rows.Count > 0)
            {
                isOpen = dt.Rows[0][6].ToString();
                sqlcont = dt.Rows[0][5].ToString();
                sboname = dt.Rows[0][0].ToString();
            }

            var powerList = UnitWork.ExcuteSql<PowerDto>(ContextType.NsapBaseDbContext, $@"SELECT a.func_id funcID,b.page_url pageUrl,a.auth_map authMap FROM (SELECT a.func_id,a.page_id,b.auth_map FROM nsap_base.base_func a INNER JOIN (SELECT t.func_id,BIT_OR(t.auth_map) auth_map FROM (SELECT func_id,BIT_OR(auth_map) auth_map FROM nsap_base.base_role_func WHERE role_id IN (SELECT role_id FROM nsap_base.base_user_role WHERE user_id={userId}) GROUP BY func_id UNION ALL SELECT func_id,auth_map FROM nsap_base.base_user_func WHERE user_id={userId}) t GROUP BY t.func_id) b ON a.func_id=b.func_id) AS a INNER JOIN nsap_base.base_page AS b ON a.page_id=b.page_id", CommandType.Text, null);
            bool viewFull = false;
            bool viewSelf = false;
            bool viewSelfDepartment = false;
            bool viewSales = false;
            bool viewCustom = false;
            var power = powerList.FirstOrDefault(s => s.PageUrl == "sales/SalesQuotation.aspx");
            if (power != null)
            {
                Powers powers = new Powers(power.AuthMap);
                if (loginContext.User.Name == "韦京生" || loginContext.User.Name == "郭睿心")
                {
                    viewFull = powers.ViewFull;
                }

                viewSelf = powers.ViewSelf;
                viewSelfDepartment = powers.ViewSelfDepartment;
                viewSales = powers.ViewSales;
                viewCustom = powers.ViewCustom;
            }

            result = _contractapplyapp.SelectOrderDraftInfo(loginContext.Roles, request.limit, request.page, request, type, viewFull, viewSelf, userId, sboid, viewSelfDepartment, Convert.ToInt32(depId.Value), viewCustom, viewSales, sqlcont, sboname);
            return result;
        }

        /// <summary>
        /// 呼叫服务(客服)右侧查询列表
        /// </summary>
        /// <param name="req">查询服务单实体</param>
        /// <returns>返回呼叫服务查询实体</returns>
        [HttpGet]
        public async Task<TableData> ServiceWorkOrderList([FromQuery] QueryServiceOrderListReq req)
        {
            var result = new TableData();
            try
            {
                result.Data = await _serviceOrderApp.ServiceWorkOrderList(req);
                result.Count = result.Data.Count;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        ///  上传合同文件
        /// </summary>
        /// <param name="files">表单文件集合</param>
        /// <returns>服务器存储的文件信息</returns>
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<Response<IList<UploadFileResp>>> UploadContractFiles([FromForm] IFormFileCollection files)
        {
            var result = new Response<IList<UploadFileResp>>();
            try
            {
                result.Result = await _fileApp.Add(files);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        ///  上传合同原件
        /// </summary>
        /// <param name="files">表单文件集合</param>
        /// <param name="obj">合同申请单实体数据</param>
        /// <returns>服务器存储的文件信息</returns>
        [HttpPost]
        public async Task<TableData> UploadContractOriginalFiles(ContractApply obj)
        {
            var result = new TableData();
            try
            {
                if (obj.ContractStatus == "7")
                {
                    result = await _contractapplyapp.AddOriginal(obj);
                }
                else
                {
                    result.Code = 500;
                    result.Message = "当前状态不是待上传原件，请盖章之后上传合同原件";
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取印章列表
        /// </summary>
        /// <returns>成功返回印章列表信息，失败返回异常信息</returns>
        [HttpGet]
        public async Task<TableData> GetContractSealList(string companyType)
        {
            var result = new TableData();
            try
            {
                return await _contractapplyapp.GetContractSealList(companyType);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取所有所属公司
        /// </summary>
        /// <returns>成功返回所属公司信息，失败返回异常信息</returns>
        [HttpGet]
        public async Task<TableData> GetContractCompanyList()
        {
            var result = new TableData();
            try
            {
                return await _contractapplyapp.GetContractCompanyList();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取所有合同类型
        /// </summary>
        /// <returns>成功返回合同类型，失败返回异常信息</returns>
        [HttpGet]
        public async Task<TableData> GetContractTypeList()
        {
            var result = new TableData();
            try
            {
                return await _contractapplyapp.GetContractTypeList();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取所有节点状态
        /// </summary>
        /// <returns>成功返回节点状态，失败返回异常信息</returns>
        [HttpGet]
        public async Task<TableData> GetContractStatusList()
        {
            var result = new TableData();
            try
            {
                return await _contractapplyapp.GetContractStatusList();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取所有文件类型
        /// </summary>
        /// <param name="contractType">合同类型</param>
        /// <returns>成功返回文件类型，失败返回异常信息</returns>
        [HttpGet]
        public async Task<TableData> GetContractFileTypeList(string contractType)
        {
            var result = new TableData();
            try
            {
                return await _contractapplyapp.GetContractFileTypeList(contractType);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 线下盖章
        /// </summary>
        /// <param name="obj">合同申请单实体数据</param>
        /// <returns>成功返回操作成功，失败返回异常信息</returns>
        [HttpPost]
        public async Task<TableData> SetContractSeal(ContractApply obj)
        {
            var result = new TableData();
            try
            {
                if (obj.ContractStatus == "6")
                {
                    result = await _contractapplyapp.SetContractSeal(obj);
                }
                else
                {
                    result.Code = 500;
                    result.Message = "当前节点不允许盖章，请在总助审批完成后盖章";
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 合同文件更新下载次数
        /// </summary>
        /// <param name="contractId">合同申请单Id</param>
        /// <param name="fileId">文件Id</param>
        /// <returns>返回TableData信息</returns>
        [HttpGet]
        public async Task<TableData> GetDownloadFile(string contractId, string fileId)
        {
            var result = new TableData();
            try
            {
                bool isSuccess = await _contractapplyapp.GetFileDownloadNum(contractId, fileId);
                if (isSuccess)
                {
                    result.Message = "更新成功";
                    result.Code = 200;
                }
                else
                {
                    result.Message = "更新失败";
                    result.Code = 500;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{contractId.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 多文件下载zip压缩包
        /// </summary>
        /// <param name="request">查询合同压缩包实体数据</param>
        /// <returns>返回TableData信息</returns>
        [HttpPost]
        public async Task<TableData> DownloadZIP(QueryContractZipReq request)
        {
            List<string> upList = new List<string>();
            Dictionary<string, string> dict = new Dictionary<string, string>();
            var result = new TableData();
            try
            {
                foreach (string fileId in request.FileIdList)
                {
                    var file = await _fileApp.GetFileAsync(fileId);
                    if (file is null)
                    {
                        result.Code = 500;
                        result.Message = "文件不存在";
                        return result;
                    }
                    else
                    {
                        dict.Add(fileId, file.FilePath);
                    }
                }

                result = await _contractapplyapp.ZipFilesAsync(request.ContractId, dict);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 上传电子盖章后合同
        /// </summary>
        /// <param name="files">表单文件</param>
        /// <param name="obj">查询电子盖章实体</param>
        /// <returns>返回上传文件信息</returns>
        [HttpPost]
        public async Task<Response<UploadFileResp>> UploadSealFile([FromForm] IFormFile files, QueryEleSealReq obj)
        {
            var result = new Response<UploadFileResp>();
            try
            {
                //获取合同申请单
                var objs = UnitWork.Find<ContractApply>(r => r.Id == obj.ContractApplyId).FirstOrDefault();
                if (objs.ContractStatus != "6")
                {
                    result.Code = 500;
                    result.Message = "当前状态不允许电子盖章，请待总助审批完成";
                }
                else
                {
                    result.Result = await _contractapplyapp.UploadSealFile(files, obj);
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 文件类型实体数据集返回前端
        /// </summary>
        /// <param name="contractFileTypeList">合同文件类型实体数据集合</param>
        /// <returns>返回TableData信息</returns>
        [HttpPost]
        public async Task<TableData> OriginFileType(List<QueryContractFileTypeRequest> contractFileTypeList)
        {
            var result = new TableData();
            try
            {
                result.Data = new
                {
                    ContractFileTypeReq = contractFileTypeList
                };

                result.Code = 200;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{contractFileTypeList.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 撤回合同申请单
        /// </summary>
        /// <param name="req">撤回合同申请单实体数据</param>
        /// <returns>返回撤回结果</returns>
        [HttpPost]
        public async Task<Response> ReCallContract(QueryReCallContractReq req)
        {
            var result = new Response();
            try
            {
                await _contractapplyapp.ReCallContract(req);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 合同文件下载历史记录
        /// </summary>
        /// <param name="req">合同历史记录查询</param>
        /// <returns>返回合同文件下载历史记录信息</returns>
        [HttpPost]
        public async Task<TableData> GetDownLoadFileHis(QueryContractDownLoadFilesReq req)
        {
            var result = new TableData();
            try
            {
                return await _contractapplyapp.GetDownLoadFileHis(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }
        #endregion

        #region 合同印章管理
        /// <summary>
        /// 获取印章信息列表
        /// </summary>
        /// <param name="request">印章查询实体数据</param>
        /// <returns>成功返回印章列表信息，失败返回异常信息</returns>
        [HttpPost]
        public async Task<TableData> GetSealList(QueryContractSealListReq request)
        {
            var result = new TableData();
            try
            {
                return await _contractSealApp.GetContractSealList(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 印章使用历史记录
        /// </summary>
        /// <param name="request">印章历史记录查询实体</param>
        /// <returns>返回印章使用历史记录列表信息</returns>
        [HttpPost]
        public async Task<TableData> GetSealOperationHistory(QuerySealHistoryReq request)
        {
            var result = new TableData();
            try
            {
                return await _contractSealApp.GetSealOperationHistory(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取印章详情信息
        /// </summary>
        /// <param name="sealId">印章Id</param>
        /// <returns>成功返回合同申请单详情信息，失败返回异常信息</returns>
        [HttpGet]
        public async Task<TableData> GetContractSealDetail(string sealId)
        {
            var result = new TableData();
            try
            {
                return await _contractSealApp.GetContractSealDetail(sealId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{sealId.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 添加印章信息
        /// </summary>
        /// <param name="obj">印章实体</param>
        /// <returns>成功返回操作成功，失败返回异常信息</returns>
        [HttpPost]
        public async Task<Response> AddSeal(ContractSeal obj)
        {
            var result = new Response();
            try
            {
                var Message = await _contractSealApp.AddSeal(obj);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 修改印章信息
        /// </summary>
        /// <param name="obj">印章实体</param>
        /// <returns>成功返回操作成功，失败返回异常信息</returns>
        [HttpPost]
        public async Task<Response> UpdateSeal(ContractSeal obj)
        {
            var result = new Response();
            try
            {
                var Message = await _contractSealApp.UpDateSeal(obj);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 删除印章
        /// </summary>
        /// <param name="sealId">印章Id</param>
        /// <returns>成功返回操作成功，失败返回异常信息</returns>
        [HttpPost]
        public async Task<Response> DeleteSeal(string sealId)
        {
            var result = new Response();
            try
            {
                var Message = await _contractSealApp.DeleteSeal(sealId);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{sealId.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 批量删除印章
        /// </summary>
        /// <param name="sealIdList">印章Id集合</param>
        /// <returns>成功返回操作成功，失败返回异常信息</returns>
        [HttpPost]
        public async Task<Response> DeleteSeals(List<string> sealIdList)
        {
            var result = new Response();
            try
            {
                var Message = await _contractSealApp.DeleteSeals(sealIdList);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{sealIdList.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取所有印章类型
        /// </summary>
        /// <returns>成功返回印章类型，失败返回异常信息</returns>
        [HttpGet]
        public async Task<TableData> GetContractSealTypeList()
        {
            var result = new TableData();
            try
            {
                return await _contractSealApp.GetContractSealTypeList();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}, 错误：{result.Message}");
            }

            return result;
        }
        #endregion

        #region 合同模板管理
        /// <summary>
        /// 获取模板信息列表
        /// </summary>
        /// <param name="request">合同模板查询实体数据</param>
        /// <returns>成功返回模板列表信息，失败返回异常信息</returns>
        [HttpPost]
        public async Task<TableData> GetContractTemplateList(QueryContractTemplateListReq request)
        {
            var result = new TableData();
            try
            {
                return await _contractTemplateApp.GetContractTemplateList(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取模板详情信息
        /// </summary>
        /// <param name="sealId">合同模板Id</param>
        /// <returns>成功返回合同模板详情信息，失败返回异常信息</returns>
        [HttpGet]
        public async Task<TableData> GetContractTemplateDetail(string sealId)
        {
            var result = new TableData();
            try
            {
                return await _contractTemplateApp.GetContractTemplateDetail(sealId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{sealId.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 添加模板信息
        /// </summary>
        /// <param name="obj">合同模板实体数据</param>
        /// <returns>成功返回操作成功，失败返回异常信息</returns>
        [HttpPost]
        public async Task<Response> AddTemplate(ContractTemplate obj)
        {
            var result = new Response();
            try
            {
                var Message = await _contractTemplateApp.AddTemplate(obj);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 修改模板信息
        /// </summary>
        /// <param name="obj">合同模板实体数据</param>
        /// <returns>成功返回操作成功，失败返回异常信息</returns>
        [HttpPost]
        public async Task<Response> UpdateTemplate(ContractTemplate obj)
        {
            var result = new Response();
            try
            {
                var Message = await _contractTemplateApp.UpDateTemplate(obj);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="templateId">合同模板Id</param>
        /// <returns>成功返回操作成功，失败返回异常信息</returns>
        [HttpPost]
        public async Task<Response> DeleteTemplate(string templateId)
        {
            var result = new Response();
            try
            {
                var Message = await _contractTemplateApp.DeleteTemplate(templateId);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{templateId.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 批量删除合同模板
        /// </summary>
        /// <param name="templateIdList">合同模板Id集合</param>
        /// <returns>成功返回操作成功，失败返回异常信息</returns>
        [HttpPost]
        public async Task<Response> DeleteTemplates(List<string> templateIdList)
        {
            var result = new Response();
            try
            {
                var Message = await _contractTemplateApp.DeleteTemplates(templateIdList);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{templateIdList.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 更新模板下载次数
        /// </summary>
        /// <param name="contractId">合同模板Id</param>
        /// <returns>返回TableData信息</returns>
        [HttpGet]
        public async Task<TableData> DownloadTemplate(string contractId)
        {
            var result = new TableData();
            try
            {
                bool isSuccess = await _contractTemplateApp.GetDownloadNum(contractId);
                if (isSuccess)
                {
                    result.Message = "更新成功";
                    result.Code = 200;
                }
                else
                {
                    result.Message = "更新失败";
                    result.Code = 500;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{contractId.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }
        #endregion
    }
}