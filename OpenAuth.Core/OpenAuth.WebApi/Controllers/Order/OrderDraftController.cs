
using Infrastructure;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Order;
using OpenAuth.App.Order.ModelDto;
using OpenAuth.App.Order.Request;
using OpenAuth.App.ProductModel;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository;
using OpenAuth.Repository.Extensions;
using OpenAuth.Repository.Interface;
using OpenAuth.WebApi.Comm;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Order
{
    /// <summary>
    /// 销售报价单
    /// </summary>
    [Route("api/Order/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Order")]
    public class OrderDraftController : Controller
    {
        private readonly FileApp _app;
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
        private IHttpClientFactory _httpClient;
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        public OrderDraftController(IHttpClientFactory _httpClient, FileApp app, IUnitWork UnitWork, ServiceBaseApp _serviceBaseApp, IAuth _auth, ServiceSaleOrderApp serviceSaleOrderApp)
        {
            this._httpClient = _httpClient;
            this._app = app;
            this.UnitWork = UnitWork;
            this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            _serviceSaleOrderApp = serviceSaleOrderApp;
        }
        /// <summary>
        /// 获取业务伙伴\客户 应收账款
        /// </summary>
        /// <param name="cardCode">客户代码</param>
        /// <param name="slpCode">业务员代码</param>
        /// <returns></returns>
        [HttpGet]
        [Route("sumbaldue")]
        public Response<BalDueDto> SumBalDue(string cardCode, string slpCode)
        {
            var result = new Response<BalDueDto>();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            // var depId = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, $"SELECT dep_id value FROM base_user_detail WHERE user_id = {userId}", CommandType.Text, null).FirstOrDefault();
            BalDueDto balDueDto = new BalDueDto();
            decimal BalDue = 0;
            decimal Total = 0;
            List<Balsbodetail> balSboDetails = new List<Balsbodetail>();
            DataTable sbotable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, "SELECT sbo_id AS id,sbo_nm AS name FROM nsap_base.sbo_info;", CommandType.Text, null);
            // sbotable.Columns.Add("BalSboAmount", typeof(decimal));
            foreach (DataRow sborow in sbotable.Rows)
            {
                balSboDetails.Add(new Balsbodetail()
                {
                    id = sborow["id"].ToString(),
                    name = sborow["name"].ToString(),
                    BalSboAmount = "0.00"
                });
                DataTable baltable = new DataTable();
                if (!string.IsNullOrEmpty(slpCode))
                {
                    baltable = _serviceSaleOrderApp.GetSalesSboBalPercent(slpCode, int.Parse(sborow["id"].ToString()));
                    if (baltable.Rows.Count > 0)
                    {
                        decimal tempDue = 0, tempTotal = 0;
                        if (!string.IsNullOrEmpty(baltable.Rows[0]["BalDue"].ToString()) && decimal.TryParse(baltable.Rows[0]["BalDue"].ToString(), out tempDue))
                        {
                            BalDue += tempDue;
                        }
                        if (!string.IsNullOrEmpty(baltable.Rows[0]["Total"].ToString()) && decimal.TryParse(baltable.Rows[0]["Total"].ToString(), out tempTotal))
                        {
                            Total += tempTotal;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(cardCode))
                {
                    baltable = _serviceSaleOrderApp.GetClientSboBalPercent(cardCode, int.Parse(sborow["id"].ToString()));
                    if (baltable.Rows.Count > 0)
                    {
                        decimal tempBalance = 0, tempINVtotal = 0, tempRINtotal = 0;
                        if (!string.IsNullOrEmpty(baltable.Rows[0]["Balance"].ToString()) && decimal.TryParse(baltable.Rows[0]["Balance"].ToString(), out tempBalance))
                        {
                            BalDue += tempBalance;
                        }
                        if (!string.IsNullOrEmpty(baltable.Rows[0]["INVtotal"].ToString()) && decimal.TryParse(baltable.Rows[0]["INVtotal"].ToString(), out tempINVtotal))
                        {
                            Total += tempINVtotal;
                        }
                        if (!string.IsNullOrEmpty(baltable.Rows[0]["RINtotal"].ToString()) && decimal.TryParse(baltable.Rows[0]["RINtotal"].ToString(), out tempRINtotal))
                        {
                            Total -= tempRINtotal;
                        }
                    }
                }
            }
            //当前账套金额
            decimal due90 = 0;
            decimal total90 = 0;
            DataTable curbaltab = _serviceSaleOrderApp.GetClientSboBalPercent(cardCode, sboid);
            decimal sboBalance = 0, rctBalance90 = 0, invBalance90 = 0, rinBalance90 = 0, invTotal90P = 0, invBalance90P = 0, rctBalance90P = 0, rinBalance90P = 0;
            if (!string.IsNullOrEmpty(curbaltab.Rows[0]["Balance"].ToString()) && decimal.TryParse(curbaltab.Rows[0]["Balance"].ToString(), out sboBalance))
            {
                due90 += sboBalance;
            }
            if (!string.IsNullOrEmpty(curbaltab.Rows[0]["RCTBal90"].ToString()) && decimal.TryParse(curbaltab.Rows[0]["RCTBal90"].ToString(), out rctBalance90))
            {
                due90 += rctBalance90;
            }
            if (!string.IsNullOrEmpty(curbaltab.Rows[0]["INVBal90"].ToString()) && decimal.TryParse(curbaltab.Rows[0]["INVBal90"].ToString(), out invBalance90))
            {
                due90 -= invBalance90;
            }
            if (!string.IsNullOrEmpty(curbaltab.Rows[0]["RINBal90"].ToString()) && decimal.TryParse(curbaltab.Rows[0]["RINBal90"].ToString(), out rinBalance90))
            {
                due90 += rinBalance90;
            }
            if (!string.IsNullOrEmpty(curbaltab.Rows[0]["INVTotal90P"].ToString()) && decimal.TryParse(curbaltab.Rows[0]["INVTotal90P"].ToString(), out invTotal90P))
            {
                total90 += invTotal90P;
            }
            balDueDto.BalDue = BalDue.ToString("#0.00").FilterString();
            balDueDto.Total = Total.ToString("#0.00").FilterString();
            balDueDto.Due90 = due90.ToString("#0.00").FilterString();
            balDueDto.Total90 = total90.ToString("#0.00").FilterString();
            balDueDto.BalSboDetails = balSboDetails;
            result.Result = balDueDto;
            return result;
        }
        /// <summary>
        /// 业务伙伴联系人
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("cardcontacts")]
        public async Task<Response<List<DropDownOption>>> CardContacts(string code, int sboId)
        {
            var result = new Response<List<DropDownOption>>();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            int billSboId = 0;
            if (sboId == sboid || sboId == 0)
            {
                billSboId = sboid;
            }
            else
            {
                billSboId = sboId;
            }
            List<DropDownOption> dropDownOptions = UnitWork.ExcuteSql<DropDownOption>(ContextType.NsapBaseDbContext, $@"SELECT b.CntctCode AS id,b.Name AS name FROM  nsap_bone.crm_ocrd a LEFT JOIN  nsap_bone.crm_ocpr b ON a.CardCode=b.CardCode and a.sbo_id=b.sbo_id WHERE a.CardCode='{code}' and a.sbo_id={billSboId} and b.Active='Y' ", CommandType.Text, null);
            if (dropDownOptions.Count > 0)
            {
                result.Result = dropDownOptions;
            }
            return result;
        }
        /// <summary>
        /// 获取账套数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("dboinfo")]
        public async Task<Response<List<SboInfoDto>>> DboInfo()
        {
            var result = new Response<List<SboInfoDto>>();
            result.Result = UnitWork.ExcuteSql<SboInfoDto>(ContextType.NsapBaseDbContext, "SELECT sbo_id AS id,sbo_nm AS name FROM nsap_base.sbo_info;", CommandType.Text, null).OrderBy(s => s.Id).ToList();
            return result;
        }
        /// <summary>
        /// 获取客户代码详细数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("cardinfo")]
        public async Task<Response<CardInfoDto>> CardInfo(string cardCode, int sboId)
        {
            var result = new Response<CardInfoDto>();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            var depId = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, $"SELECT dep_id value FROM base_user_detail WHERE user_id = {userId}", CommandType.Text, null).FirstOrDefault();
            bool isSql = true;
            if (sboId == sboid || sboId == 0)
            {
                sboId = sboid;
            }
            else
            {
                isSql = false;
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
            result.Result = _serviceSaleOrderApp.CardInfo(cardCode, sboId, isSql, viewSelf, viewSelfDepartment, viewFull, userId, Convert.ToInt32(depId.Value));
            return result;
        }
        /// <summary>
        /// 销售报价单生产部门列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("productiondepartment")]
        public async Task<Response<List<DropDownOption>>> ProductionDepartment()
        {
            var result = new Response<List<DropDownOption>>();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            result.Result = UnitWork.ExcuteSql<DropDownOption>(ContextType.NsapBaseDbContext, $@"	SELECT FldValue AS id,Descr AS name FROM nsap_bone.base_ufd1 WHERE TableID='sale_oqut' AND FieldID='36' order by  FldDate ", CommandType.Text, null);
            return result;
        }
        /// <summary>
        /// 销售报价单经理列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("managerinfo")]
        public async Task<Response<List<ManagerDto>>> ManagerInfo()
        {
            var result = new Response<List<ManagerDto>>();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            result.Result = UnitWork.ExcuteSql<ManagerDto>(ContextType.NsapBaseDbContext, $@"SELECT empID,CONCAT(lastName,+firstName) AS name FROM nsap_bone.crm_ohem WHERE sbo_id={sboid}", CommandType.Text, null).OrderBy(s => s.EmpId).ToList();
            return result;
        }
        /// <summary>
        /// 发票类别查询列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("invoicetypeinfo")]
        public async Task<Response<List<DropDownOption>>> InvoiceTypeInfo()
        {
            var result = new Response<List<DropDownOption>>();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            result.Result = new List<DropDownOption>() {
                new DropDownOption(){ Id=0,Name="增值税普通发票"},
                new DropDownOption(){Id=1,Name="增值税专用发票" }
            };
            return result;
        }
        /// <summary>
        /// 付款条件查询列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("paycondinfo")]
        public async Task<Response<List<DropDownOption>>> PayCondcInfo()
        {
            var result = new Response<List<DropDownOption>>();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            result.Result = UnitWork.ExcuteSql<DropDownOption>(ContextType.NsapBaseDbContext, $@"SELECT GroupNum AS id,PymntGroup AS name FROM nsap_bone.crm_octg WHERE sbo_id={sboid}", CommandType.Text, null).ToList();
            return result;
        }
        /// <summary>
        /// 标识查询列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("oidcinfo")]
        public async Task<Response<List<DropDownOption>>> OidcInfo()
        {
            var result = new Response<List<DropDownOption>>();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            result.Result = UnitWork.ExcuteSql<DropDownOption>(ContextType.NsapBaseDbContext, $@" SELECT Code as id,Name AS name FROM nsap_bone.crm_oidc WHERE sbo_id={sboid}", CommandType.Text, null).ToList();
            return result;
        }
        /// <summary>
        /// 货币查询列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("crminfo")]
        public async Task<Response<List<DropDownOption>>> CrmInfo()
        {
            var result = new Response<List<DropDownOption>>();
            result.Result = UnitWork.ExcuteSql<DropDownOption>(ContextType.NsapBaseDbContext, $@" SELECT CurrCode AS Id,CurrName AS Name FROM nsap_bone.crm_ocrn", CommandType.Text, null).ToList();
            return result;
        }

        /// <summary>
        /// 业务伙伴列表
        /// </summary>
        [HttpPost]
        [Route("cardcodeview")]
        public async Task<TableData> LoadCardCodeViewAsync(CardCodeRequest request)
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
            DataTable rDataRowsSlp = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $@"SELECT sale_id,tech_id FROM nsap_base.sbo_user WHERE user_id={userId} AND sbo_id={sboid}", CommandType.Text, null);
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
        /// 加载销售报价单列表
        /// </summary>
        [HttpPost]
        [Route("sales")]
        public async Task<TableData> LoadOrderGridAsync(QuerySalesQuotationReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var userId = _serviceBaseApp.GetUserNaspId();//(await UnitWork.Find<NsapUserMap>(n => n.UserID.Equals(loginContext.User.Id)).FirstOrDefaultAsync())?.NsapUserId;
            var depId = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, $"SELECT dep_id value FROM base_user_detail WHERE user_id = {userId}", CommandType.Text, null).FirstOrDefault();
            string type = "OQUT";
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);//UnitWork.ExcuteSql<sbo_info>(ContextType.Nsap4ServeDbContextType, "SELECT sbo_id FROM nsap_base.sbo_info WHERE is_curr = 1 AND valid = 1 LIMIT 1;", CommandType.Text, null).FirstOrDefault()?.sbo_id;
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
            if (isOpen == "0")
            {

                //return NSAP.Biz.Sales.BillDelivery.SelectBillViewInfo(int.Parse(rp), int.Parse(page), query, sortname, sortorder, type, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewFull, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewSelf, UserID, SboID, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewSelfDepartment, DepID, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewCustom, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewSales);
            }
            else
            {
                result = _serviceSaleOrderApp.SelectOrderDraftInfo(request.limit, request.page, request, type, viewFull, viewSelf, userId, sboid, viewSelfDepartment, Convert.ToInt32(depId.Value), viewCustom, viewSales, sqlcont, sboname);
                // return NSAP.Biz.Sales.BillDelivery.SelectBillListInfo(int.Parse(rp), int.Parse(page), query, sortname, sortorder, type, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewFull, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewSelf, UserID, SboID, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewSelfDepartment, DepID, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewCustom, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewSales, sqlcont, sboname);
            }
            return result;
        }
        /// <summary>
        /// 获取业务员
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("salesmaninfo")]
        public async Task<Response<List<SelectOption>>> GetSalesManInfo()
        {
            var result = new Response<List<SelectOption>>();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            result.Result = _serviceSaleOrderApp.GetSalesSelect(sboid);
            return result;
        }
        /// <summary>
        /// 销售报价单保存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("Save")]
        public async Task<Response<string>> Save(AddOrderReq orderReq)
        {
            var result = new Response<string>();
            try
            {
                result.Result = _serviceSaleOrderApp.Save(orderReq);
            }
            catch (Exception ex)
            {
                result.Result = "";
                result.Code = 500;
                result.Message = "服务器内部错误：" + ex.Message;
                Log.Logger.Error($"报错地址:{Request.Path},请求方式：{"Post"},参数:{JsonConvert.SerializeObject(orderReq)},异常描述：{ex.Message},堆栈信息：{ex.StackTrace}");
            }
            return result;
        }
        /// <summary>
        /// 销售报价单转换销售订单
        /// </summary>
        /// <param name="salesOrderSaveReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("OrderDraftToSales")]
        public Response<string> OrderDraftToSales(SalesOrderSaveReq salesOrderSaveReq)
        {
            var result = new Response<string>();
            try
            {
                result.Result = _serviceSaleOrderApp.SalesOrderSave_ORDR(salesOrderSaveReq);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}, 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 仓库下拉列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("warehouseinfo")]
        public async Task<Response<List<DropDownOption>>> WarehouseInfo(int sboid = 1)
        {
            var result = new Response<List<DropDownOption>>();
            result.Result = UnitWork.ExcuteSql<DropDownOption>(ContextType.NsapBaseDbContext, $@" SELECT WhsCode AS id,WhsName AS name FROM nsap_bone.store_owhs WHERE sbo_id={sboid}", CommandType.Text, null).ToList();
            return result;
        }
        /// <summary>
        /// 加载物料数据
        /// </summary>pp
        [HttpPost]
        [Route("saleitem")]
        public async Task<TableData> SaleItem(ItemRequest request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            request.TypeId = "0";
            request.IsbillCfg = "1";
            var result = new TableData();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            result = _serviceSaleOrderApp.SalesItems(request, sboid.ToString());
            return result;
        }
        /// <summary>
        /// 获取物料编码配件信息
        /// </summary>
        /// <param name="ItemCode">item_cfg_id</param>
        /// <param name="WhsCode">仓库code</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetItemConfigList")]
        public async Task<TableData> GetItemConfigList(string ItemCode, string WhsCode)
        {
            var result = new TableData();
            result = _serviceSaleOrderApp.GetItemConfigList(ItemCode, WhsCode);
            return result;
        }
        /// <summary>
        /// 客户未清销售订单
        /// GetOpenORDRsByCustomer
        /// </summary>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("untreatedorder")]
        public async Task<Response<int>> UntreatedOrder(string cardCode)
        {
            var result = new Response<int>();
            result.Result = UnitWork.ExcuteSql<CountDto>(ContextType.SapDbContextType, $@"select count(1) count from ordr where CANCELED='N' AND DocStatus='O' and CardCode='{cardCode}'", CommandType.Text, null).FirstOrDefault().Count;
            return result;
        }
        /// <summary>
        /// 客户类型（国内外客户）
        /// GetClientEconomicNature
        /// </summary>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("cardtype")]
        public async Task<Response<object>> CardType(string cardCode)
        {
            var result = new Response<object>();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            result.Result = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, $@"select b.GroupName value from nsap_bone.crm_ocrd a left join nsap_bone.crm_OCRG b on a.GroupCode = b.GroupCode and a.sbo_id = b.sbo_id where a.sbo_id ='{sboid}' and a.CardCode = '{cardCode}'", CommandType.Text, null).FirstOrDefault()?.Value;
            return result;
        }
        /// <summary>
        /// 获取当前登录业务员
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("saleuser")]
        public async Task<Response<DropDownOption>> GetSalesUser()
        {
            var result = new Response<DropDownOption>();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            result.Result = UnitWork.ExcuteSql<DropDownOption>(ContextType.NsapBaseDbContext, $@"SELECT b.SlpCode  id,b.SlpName name FROM nsap_base.sbo_user a LEFT JOIN nsap_bone.crm_oslp b ON a.sale_id=b.SlpCode AND a.sbo_id=b.sbo_id WHERE a.sbo_id={sboid} AND a.user_id={userId}", CommandType.Text, null).FirstOrDefault();
            return result;
        }
        /// <summary>
        /// 获取拟取消订单
        /// </summary>pp
        [HttpPost]
        [Route("relordr")]
        public async Task<TableData> RelORDR(RelORDRRequest request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            result = _serviceSaleOrderApp.RelORDR(request);
            return result;
        }
        /// <summary>
        /// 客户代码模糊查询
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("cardcodecheck")]
        public async Task<Response<List<CardCodeCheckDto>>> GetCardCodeCheeck(string cardCode)
        {
            var result = new Response<List<CardCodeCheckDto>>();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            var depId = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, $"SELECT dep_id value FROM base_user_detail WHERE user_id = {userId}", CommandType.Text, null).FirstOrDefault();
            string type = "SDR";
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
            result.Result = _serviceSaleOrderApp.GetCardCodeCheck(type, cardCode, viewFull, viewSelf, userId, sboid, viewSelfDepartment, Convert.ToInt32(depId.Value));
            return result;
        }
        /// <summary>
        /// 获取汇率数据
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("currency")]
        public async Task<Response<object>> Currency(string currency)
        {
            var result = new Response<object>();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            result.Result = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.SapDbContextType, $@"SELECT Rate value From ORTT WHERE Currency='{currency}' AND RateDate='{DateTime.Now.ToString("yyyy-MM-dd")}'", CommandType.Text, null).FirstOrDefault()?.Value;
            return result;
        }
        /// <summary>
        /// 查询指定业务伙伴的科目余额与百分比数据
        /// </summary>
        /// <param name="cardCode"></param>
        /// <param name="sboId"></param>
        /// <returns></returns>
        private DataTable GetClientSboBalPercent(string cardCode, string sboId)
        {
            ResultOrderDto resultDto = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, $@"SELECT is_open value FROM nsap_base.sbo_info WHERE sbo_id={sboId}", CommandType.Text, null).FirstOrDefault();
            if (resultDto != null && resultDto.Value.ToString() == "1")
            {
                string sapConn = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, $@"SELECT sql_conn value FROM nsap_base.sbo_info WHERE  sbo_id={sboId}", CommandType.Text, null).FirstOrDefault()?.Value.ToString();
                string strSql = $@"SELECT (Select sum(Balance) from OCRD where CardCode='{cardCode}') as Balance
                                  ,(select sum(DocTotal) from OINV WHERE CANCELED ='N' and CardCode='{cardCode}') as INVtotal
                                  ,(select SUM(DocTOTal) from ORIN where CANCELED<>'Y' and CardCode='{cardCode}') as RINtotal
                                --90天内未清收款
                                ,(select SUM(openBal) from ORCT WHERE CANCELED='N' AND openBal<>0 AND CardCode='{cardCode}' and datediff(DAY,docdate,getdate())<=90) as RCTBal90
                                --90天内未清发票金额
                                ,(select SUM(DocTotal-PaidToDate) from OINV WHERE CANCELED ='N' and CardCode='{cardCode}' and DocTotal-PaidToDate>0 and datediff(DAY,docdate,getdate())<=90) as INVBal90
                                --90天内未清贷项金额
                                ,(select SUM(DocTotal-PaidToDate) from ORIN where CANCELED ='N' and CardCode='{cardCode}' and DocTotal-PaidToDate>0 and datediff(DAY,docdate,getdate())<=90) as RINBal90
                                --90天前未清发票的发票总额
                                ,(select SUM(DocTotal) from OINV WHERE CANCELED ='N' and CardCode = '{cardCode}' and DocTotal-PaidToDate > 0 and datediff(DAY, docdate, getdate())> 90) as INVTotal90P
                ";
                return null;//UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql);
            }
            else
            {
                string strSql = string.Format($@"SELECT(Select sum(Balance) from nsap_bone.crm_ocrd_oldsbo_balance where sbo_id=?sbo_id and CardCode='{cardCode}') as Balance
                                               , (select sum(DocTotal) from nsap_bone.sale_oinv WHERE CANCELED ='N' and sbo_id=?sbo_id and CardCode ='{cardCode}') as INVtotal
                                               ,(select SUM(DocTOTal) from nsap_bone.sale_orin where CANCELED ='N' and sbo_id=?sbo_id and CardCode ='{cardCode}') as RINtotal
                                            ,'' as RCTBal90
                                            ,'' as INVBal90
                                            ,'' as RINBal90
                                            ,'' as INVTotal90P
                                            ");
                return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            }
        }
        /// <summary>
        /// 配置类型
        /// </summary>
        /// <param name="TableID"></param>
        /// <param name="AliasID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQUTCustomeValueByFN")]
        public Response<List<DropDownOption>> GetQUTCustomeValueByFN(string TableID, string AliasID = "ZS")
        {

            //客户端设置的自定义字段U_ZS
            //string TableID = "QUT1";string AliasID = "ZS";
            string saptabname = "";
            switch (TableID)
            {
                case "sale_qut1":
                    saptabname = "QUT1";
                    break;
                case "sale_rdr1":
                    saptabname = "RDR1";
                    break;
                default:
                    saptabname = TableID.Split('_')[1];
                    break;
            }
            string strSql = string.Format(@"select t1.FldValue as id,t1.Descr as name from ufd1 t1 LEFT JOIN cufd t0 on t0.TableID=t1.TableID and t0.FieldID=t1.FieldID
                                            where t0.TableID='{0}' AND t0.AliasID='{1}' order by t1.IndexID asc ", saptabname, AliasID);
            var result = new Response<List<DropDownOption>>();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            result.Result = UnitWork.ExcuteSql<DropDownOption>(ContextType.SapDbContextType, strSql, CommandType.Text, null);
            return result;

        }
        /// <summary>
        /// 获取后勤信息接口
        /// </summary>
        /// <param name="CardCode">客户编码</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAddress")]
        public Response<List<AddresTypeDto>> GetAddress(string CardCode)
        {
            var result = new Response<List<AddresTypeDto>>();
            try
            {
                if (!string.IsNullOrEmpty(CardCode))
                {
                    List<AddresTypeDto> addresTypeList = new List<AddresTypeDto>();
                    List<string> addresTypes = new List<string>() { "B", "S" };
                    var userId = _serviceBaseApp.GetUserNaspId();
                    var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
                    string strSql = string.Format("SELECT sql_db,sql_name,sql_pswd,sap_name,sap_pswd,sql_conn,is_open FROM {0}.sbo_info WHERE sbo_id={1}", "nsap_base", sboid);
                    DataTable dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
                    string dRowData = string.Empty;
                    string isOpen = "0";
                    if (dt.Rows.Count > 0)
                    {
                        isOpen = dt.Rows[0][6].ToString();
                    }
                    foreach (var item in addresTypes)
                    {
                        if (isOpen == "0")
                        {
                            StringBuilder sql = new StringBuilder();
                            sql.Append(" SELECT Address AS name,CONCAT(IFNULL(ZipCode,''),IFNULL(b.Name,''),IFNULL(c.Name,''),IFNULL(City,''),IFNULL(Building,'')) AS id,a.ZipCode,a.State ");
                            sql.AppendFormat(" FROM {0}.crm_crd1 a", "nsap_bone");
                            sql.AppendFormat(" LEFT JOIN {0}.store_ocry b ON a.Country=b.Code", "nsap_bone");
                            sql.AppendFormat(" LEFT JOIN {0}.store_ocst c ON a.State=c.Code", "nsap_bone");
                            sql.AppendFormat(" WHERE AdresType='{0}' AND CardCode='{1}'", item, CardCode);
                            //  UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql.ToString(), CommandType.Text, null);
                            //item.Address = UnitWork.ExcuteSql<AddressDto>(ContextType.NsapBaseDbContext, sql.ToString(), CommandType.Text, null).FirstOrDefault();
                            addresTypeList.Add(new AddresTypeDto()
                            {
                                AddresType = item,
                                Address = UnitWork.ExcuteSql<AddressDto>(ContextType.NsapBaseDbContext, sql.ToString(), CommandType.Text, null)
                            });
                        }
                        else
                        {
                            //string strsql = string.Format("SELECT sql_db,sql_name,sql_pswd,sap_name,sap_pswd,sql_conn,is_open FROM {0}.sbo_info WHERE sbo_id={1}", "nsap_base", sboid);
                            //DataTable dtsql = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strsql, CommandType.Text, null);
                            //string sqlconn = "";
                            //if (dt.Rows.Count > 0)
                            //{
                            //    sqlconn = dt.Rows[0][5].ToString();
                            //}
                            StringBuilder sql = new StringBuilder();
                            sql.Append(" SELECT Address AS name,(ISNULL(ZipCode,'') + ISNULL(b.Name,'')+ISNULL(c.Name,'')+ISNULL(City,'')+ISNULL(CONVERT(VARCHAR(1000),Building),'')) AS id,a.ZipCode,a.State ");
                            sql.Append(" FROM CRD1 a ");
                            sql.Append(" LEFT JOIN OCRY b ON a.Country=b.Code");
                            sql.Append(" LEFT JOIN OCST c ON a.State=c.Code");
                            sql.AppendFormat(" WHERE AdresType='{0}' AND CardCode='{1}'", item, CardCode);
                            addresTypeList.Add(new AddresTypeDto()
                            {
                                AddresType = item,
                                Address = UnitWork.ExcuteSql<AddressDto>(ContextType.SapDbContextType, sql.ToString(), CommandType.Text, null)
                            });
                        }
                    }
                    result.Result = addresTypeList;
                }
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// 付款条件
        /// </summary>
        /// <param name="GroupNum"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPayMentInfo")]
        public Response<PayMentInfoDto> GetPayMentInfo(string GroupNum)
        {
            try
            {
                var result = new Response<PayMentInfoDto>();
                string strSql = string.Format("SELECT PrepaDay,PrepaPro,PayBefShip,GoodsToPro,GoodsToDay");
                strSql += string.Format(" FROM {0}.crm_octg_cfg", "nsap_bone");
                strSql += string.Format(" WHERE GroupNum={0}", GroupNum);
                result.Result = UnitWork.ExcuteSql<PayMentInfoDto>(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).FirstOrDefault();
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// 装运类型
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("DropPopupTrnspCode")]
        public Response<List<PopupTrnspDto>> DropPopupTrnspCode(int SboId)
        {
            var result = new Response<List<PopupTrnspDto>>();
            string strSql = " SELECT TrnspCode,TrnspName FROM nsap_bone.crm_oshp WHERE sbo_id=" + SboId + "";
            result.Result = UnitWork.ExcuteSql<PopupTrnspDto>(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            return result;
        }
        /// <summary>
        /// 税率是否启用
        /// IsSwitching
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("EnableTaxRate")]
        public Response<bool> EnableTaxRate()
        {
            var result = new Response<bool>() { Result = false };
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT is_valid FROM nsap_bone.base_sys_switch WHERE table_nm='sale_oqut' AND fld_nm='U_FPLB'");
            DataTable dataTable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                if (dataTable.Rows[0][0].ToString() == "1")
                {
                    result.Result = true;
                }
            }
            return result;
        }
        /// <summary>
        /// 科目代码(服务)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SelectAcctCodeView")]
        public TableData GridAcctCodeList(OactRequest query)
        {
            return _serviceSaleOrderApp.SelectAcctCodeView(query);
        }

        #region 查看销售报价单
        /// <summary>
        /// 销售报价单详情
        /// </summary>
        /// <param name="DocNum">订单号</param>
        /// <param name="tablename">sale_oqut </param>
        /// <param name="ations">edit</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSaleDeliveryinfo")]
        public TableData QuerySaleDeliveryDetails(string DocNum, string tablename, string ations, int? sboId)
        {
            TableData tableData = new TableData();
            try
            {
                string billPageurl = "sales/SalesQuotation.aspx";
                var userId = _serviceBaseApp.GetUserNaspId();
                int SboId = _serviceBaseApp.GetUserNaspSboID(userId);
                bool ViewCustom = false;
                bool ViewSales = false;
                bool isSql = true;
                if (sboId.HasValue && sboId.Value != SboId)
                {
                    SboId = sboId.Value;
                    isSql = false;
                }
                if (!string.IsNullOrEmpty(billPageurl))
                {
                    var powerList = UnitWork.ExcuteSql<PowerDto>(ContextType.NsapBaseDbContext, $@"SELECT a.func_id funcID,b.page_url pageUrl,a.auth_map authMap FROM (SELECT a.func_id,a.page_id,b.auth_map FROM nsap_base.base_func a INNER JOIN (SELECT t.func_id,BIT_OR(t.auth_map) auth_map FROM (SELECT func_id,BIT_OR(auth_map) auth_map FROM nsap_base.base_role_func WHERE role_id IN (SELECT role_id FROM nsap_base.base_user_role WHERE user_id={userId}) GROUP BY func_id UNION ALL SELECT func_id,auth_map FROM nsap_base.base_user_func WHERE user_id={userId}) t GROUP BY t.func_id) b ON a.func_id=b.func_id) AS a INNER JOIN nsap_base.base_page AS b ON a.page_id=b.page_id", CommandType.Text, null);
                    var power = powerList.FirstOrDefault(s => s.PageUrl == "sales/SalesQuotation.aspx");
                    if (power != null)
                    {
                        Powers powers = new Powers(power.AuthMap);
                        ViewCustom = powers.ViewCustom;//查看客户资料
                        ViewSales = powers.ViewSales;//查看销售价格
                    }
                }
                if (ations == "copy")
                {
                    ViewCustom = true;
                    ViewSales = true;
                }
                tableData.Data = _serviceSaleOrderApp.QuerySaleDeliveryDetails(DocNum, ViewCustom, tablename, ViewSales, SboId, isSql);
            }
            catch (Exception e)
            {
                string msg = e.Message;
            }
            return tableData;
        }
        /// <summary>
        /// 查看附件
        /// <summary>
        /// <param name="OrderId">订单Id</param>
        /// <param name="TypeId">默认6</param>
        /// <param name="sboId">选择账套Id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetFilesList")]
        public Response<List<OrderFile>> GetFilesList(string OrderId, string TypeId, string sboId)
        {
            TableData tableData = new TableData();
            var result = new Response<List<OrderFile>>();
            if (!string.IsNullOrEmpty(TypeId))
            {
                var userId = _serviceBaseApp.GetUserNaspId();
                var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
                int billSboId = 0;
                if (!string.IsNullOrEmpty(sboId))
                {
                    billSboId = int.Parse(sboId);
                }
                else { billSboId = sboid; }
                string strSql = string.Format("SELECT a.file_id as Id ,b.type_nm FileType ,a.file_nm as FileName ,a.remarks Remark,a.file_path as FilePath ,a.upd_dt CreateTime,c.user_nm CreateUserName ,a.view_file_path ViewFilePath ");//,a.file_type_id,a.acct_id
                strSql += string.Format(" FROM {0}.file_main a", "nsap_oa");
                strSql += string.Format(" LEFT JOIN {0}.file_type b ON a.file_type_id=b.type_id", "nsap_oa");
                strSql += string.Format(" LEFT JOIN {0}.base_user c ON a.acct_id=c.user_id", "nsap_base");
                if (TypeId == "5")//销售订单附件附带销售提成附件
                {
                    strSql += string.Format(" WHERE a.docEntry='{0}' AND a.file_type_id in ({1},37) AND sbo_id={2}", OrderId, TypeId, billSboId);
                }
                else
                {
                    //非销售订单附件
                    strSql += string.Format(" WHERE a.docEntry='{0}' AND a.file_type_id='{1}' AND sbo_id={2}", OrderId, TypeId, sboid);
                }
                result.Result = UnitWork.ExcuteSql<OrderFile>(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            }
            return result;
        }
        /// <summary>
        /// 查看物料明显
        /// </summary>
        [HttpGet]
        [Route("GetItemCodeList")]
        public TableData GetItemCodeList(string DocNum, string tablename = "sale_qut1", string ations = "", string billPageurl = "")
        {
            TableData tableData = new TableData();
            var userId = _serviceBaseApp.GetUserNaspId();
            bool ViewSales = true;
            bool ViewCustom = true;
            if (!string.IsNullOrEmpty(billPageurl))
            {
                var powerList = UnitWork.ExcuteSql<PowerDto>(ContextType.NsapBaseDbContext, $@"SELECT a.func_id funcID,b.page_url pageUrl,a.auth_map authMap FROM (SELECT a.func_id,a.page_id,b.auth_map FROM nsap_base.base_func a INNER JOIN (SELECT t.func_id,BIT_OR(t.auth_map) auth_map FROM (SELECT func_id,BIT_OR(auth_map) auth_map FROM nsap_base.base_role_func WHERE role_id IN (SELECT role_id FROM nsap_base.base_user_role WHERE user_id={userId}) GROUP BY func_id UNION ALL SELECT func_id,auth_map FROM nsap_base.base_user_func WHERE user_id={userId}) t GROUP BY t.func_id) b ON a.func_id=b.func_id) AS a INNER JOIN nsap_base.base_page AS b ON a.page_id=b.page_id", CommandType.Text, null);
                var power = powerList.FirstOrDefault(s => s.PageUrl == "sales/SalesQuotation.aspx");
                if (power != null)
                {
                    Powers powers = new Powers(power.AuthMap);
                    ViewCustom = powers.ViewCustom;//查看客户资料
                    ViewSales = powers.ViewSales;//查看销售价格
                }
            }
            if (ations == "copy")
            {
                ViewCustom = true;
                ViewSales = true;
            }
            int SboId = _serviceBaseApp.GetUserNaspSboID(userId);
            string U_YFTCBL = "";
            string strSqlViewSales = string.Format("SELECT COUNT(*) FROM information_schema.columns WHERE table_schema='nsap_bone' AND table_name ='{0}' AND column_name='{1}'", tablename, "U_YFTCBL");
            string IsU_YFTCBL = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSqlViewSales, CommandType.Text, null).ToString();
            if (!string.IsNullOrEmpty(IsU_YFTCBL) && IsU_YFTCBL != "0")
            {
                U_YFTCBL = ",IF(" + ViewSales + ",d.U_YFTCBL,0)";
            }
            StringBuilder stringBuilder = new StringBuilder();
            string strSql = string.Format(" SELECT  d.ItemCode,Dscription,Quantity ," +
                "IF(" + ViewSales + ",d.PriceBefDi,0)PriceBefDi," +

                "IF(" + ViewSales + ",DiscPrcnt,0)DiscPrcnt,d.U_PDXX," +
                "IF(" + ViewSales + ",d.U_XSTCBL,0)U_XSTCBL," +
                "IF(" + ViewSales + ",d.U_YWF,0)U_YWF," +
                "IF(" + ViewSales + ",d.U_FWF,0)U_FWF," +
                "IF(" + ViewSales + ",Price,0)Price,VatGroup," +
                "IF(" + ViewSales + ",PriceAfVAT,0)PriceAfVAT,");
            strSql += string.Format("IF(" + ViewSales + ",LineTotal,0) LineTotal," +
                "IF(" + ViewSales + ",TotalFrgn,0) TotalFrgn," +
                "IF(" + ViewSales + ",d.U_SCTCBL,0) U_SCTCBL," +
                "IF(" + ViewSales + ",StockPrice,0) StockPrice,d.U_YF,d.WhsCode,w.OnHand,d.VatPrcnt,d.LineNum,d.U_YFTCBL,");
            strSql += string.Format("m.IsCommited,m.OnOrder,m.U_TDS,m.U_DL,m.U_DY,d.DocEntry,d.OpenQty,m.U_JGF,");
            strSql += string.Format("(CASE m.QryGroup1 WHEN 'N' THEN 0 ELSE '0.5' END) AS QryGroup1,");
            strSql += string.Format("(CASE m.QryGroup2 WHEN 'N' THEN 0 ELSE '3' END) AS QryGroup2,");
            strSql += string.Format("(CASE m.QryGroup3 WHEN 'N' THEN 0 ELSE '2' END) AS QryGroup3");
            strSql += string.Format(",m.U_JGF1,IFNULL(m.U_YFCB,0)U_YFCB," +
                "IFNULL(d.U_SHJSDJ,0)U_SHJSDJ," +
                "IFNULL(d.U_SHJSJ,0) U_SHJSJ ," +
                "IFNULL(d.U_SHTC,0) U_SHTC," +
                "IFNULL(SumQuantity,0) as SumQuantity");
            strSql += string.Format(",(CASE m.QryGroup8 WHEN 'N' THEN 0 ELSE '1' END) AS QryGroup8");//3008n
            strSql += string.Format(",(CASE m.QryGroup9 WHEN 'N' THEN 0 ELSE '2' END) AS QryGroup9");//9系列
            strSql += string.Format(",(CASE m.QryGroup10 WHEN 'N' THEN 0 ELSE '1.5' END) AS QryGroup10");//ES系列 
            strSql += string.Format(",m.buyunitmsr,d.U_ZS");
            if (tablename.ToLower() == "sale_qut1" || tablename.ToLower() == "sale_rdr1")
            {
                strSql += ",d.U_RelDoc";
            }
            strSql += ",d.LineStatus,d.BaseEntry,d.BaseLine,d.BaseType";
            strSql += string.Format(" FROM {0}." + tablename + " d", "nsap_bone");
            strSql += string.Format(" LEFT JOIN {0}.store_oitw w ON d.ItemCode=w.ItemCode AND d.WhsCode=w.WhsCode AND d.sbo_id=w.sbo_id", "nsap_bone");
            strSql += string.Format(" LEFT JOIN {0}.store_oitm m ON d.ItemCode=m.ItemCode AND d.sbo_id=m.sbo_id", "nsap_bone");
            strSql += string.Format(" LEFT JOIN (select d1.sbo_id,d1.BaseEntry ,d1.BaseLine,SUM(d1.Quantity) as SumQuantity from {0}.sale_DLN1 d1 inner join {0}.sale_odln d0 on d0.docentry=d1.docentry and d0.sbo_id=d1.sbo_id where d0.Canceled='N' AND d1.BaseType=17 and d1.BaseEntry=" + DocNum + " GROUP BY d1.sbo_id,d1.BaseEntry,d1.BaseLine) as T on d.sbo_id=T.sbo_id and d.DocEntry=T.BaseEntry and  d.LineNum=T.BaseLine  ", "nsap_bone");
            strSql += string.Format(" WHERE d.DocEntry=" + DocNum + " AND d.sbo_id={0}", SboId);
            DataTable dts = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
            //int itemindex = 0;
            if (tablename.ToLower() == "sale_rdr1")
            {
                foreach (DataRow tempr in dts.Rows)
                {
                    //itemindex++;
                    //tempr[0] = itemindex.ToString();
                    string statusSql = string.Format("select top 1 LineStatus from RDR1 where DocEntry={0} and LineNum={1}", DocNum, tempr["LineNum"].ToString());
                    object statusobj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, statusSql.ToString(), CommandType.Text, null);
                    tempr["LineStatus"] = statusobj == null ? "" : statusobj.ToString();
                }
            }
            tableData.Data = dts.Tolist<OrderItemInfo>();
            return tableData;
        }

        /// <summary>
        /// 历史单据
        /// </summary>
        [HttpGet]
        [Route("GetHistoricalDoc")]
        public TableData GetHistoricalDoc(string TableName, int SboId, string CardCode)
        {
            TableData tableData = new TableData();
            tableData.Data = _serviceSaleOrderApp.GetHistoricalDoc(TableName, SboId, CardCode);
            return tableData;
        }
        #endregion
        #region 查看单据详细信息
        /// <summary>
        ///  查看单据详细信息
        /// </summary>
        /// <param name="DocNum"></param>
        /// <param name="tablename"></param>
        /// <param name="ations"></param>
        /// <param name="SboId"></param>
        /// <param name="billPageurl"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("QuerySaleDeliveryDetails")]
        public TableData QuerySaleDeliveryDetails(string DocNum, string tablename, string ations, string SboId, string billPageurl)
        {
            var result = new TableData();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            try
            {
                bool ViewCustom = false;
                bool ViewSales = false;
                int billSboId = 0; bool isSql = true;
                if (int.Parse(SboId) == SboID || SboId == "") { billSboId = SboID; } else { billSboId = int.Parse(SboId); isSql = false; }
                if (!string.IsNullOrEmpty(billPageurl))
                {
                    long AuthMap = _serviceSaleOrderApp.GetCurrentPage(UserID, billPageurl).AuthMap;
                    Powers Powers = new Powers(AuthMap);
                    ViewCustom = Powers.ViewCustom;
                    ViewSales = Powers.ViewSales;
                }
                if (ations == "copy") { ViewCustom = true; ViewSales = true; }
                result.Data = _serviceSaleOrderApp.QuerySaleDeliveryDetailsV1(DocNum, true, tablename, true, billSboId, isSql);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }

        #endregion
        /// <summary>
        ///  批量上传文件接口
        /// </summary>
        /// <param name="files"></param>
        /// <returns>服务器存储的文件信息</returns>
        [HttpPost]
        [Route("billAttachUpload")]
        public async Task<Response<IList<UploadFileResp>>> billAttachUpload([FromForm] IFormFileCollection files)
        {
            var result = new Response<IList<UploadFileResp>>();
            try
            {
                result.Result = await _app.Add(files);
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
        ///  批量上传文件接口(新)
        /// </summary>
        /// <param name="files"></param>
        /// <returns>服务器存储的文件信息</returns>
        [HttpPost]
        [Route("BillAttachUploadNew")]
        public async Task<Response<List<UploadFileResp>>> BillAttachUploadNew([FromForm] IFormFileCollection files)
        {
            string host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host;

            var result = new Response<List<UploadFileResp>>();
            try
            {
                result.Result = _serviceSaleOrderApp.BillAttachUploadNew(files, host);
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
        ///  批量上传保存接口
        /// </summary>
        /// <param name="model"></param>
        /// <returns>服务器存储的文件信息</returns>Fsave
        [HttpPost]
        [Route("UpdateSalesDocAttachment")]
        public Response UpdateSalesDocAttachment(BillDeliveryReq model)
        {
            var result = new Response();
            try
            {
                _serviceSaleOrderApp.UpdateSalesDocAttachment(model);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：'', 错误：{result.Message}");
            }
            return result;
        }


        /// <summary>
        ///  复制生产订单
        /// </summary>
        /// <param name="page"></param>
        /// <param name="rp"></param>
        /// <param name="qtype"></param>
        /// <param name="query"></param>
        /// <param name="sortname"></param>
        /// <param name="sortorder"></param>
        /// <param name="sboID"></param>
        /// <returns>复制生产订单</returns>
        [HttpGet]
        [Route("CopyProductToSaleSelect")]
        public TableData CopyProductToSaleSelect(int page, int rp, string qtype, string query, string sortname, string sortorder, int sboID)
        {
            var result = new TableData();

            result.Data = _serviceSaleOrderApp.CopyProductToSaleSelect(pageSize: rp, pageIndex: page, filterQuery: query, sortname: sortname, sortorder: sortorder, sboID: sboID);
            return result;


        }
        /// <summary>
        /// 获取联系信息
        /// </summary>
        /// <param name="page"></param>
        /// <param name="rp"></param>
        /// <param name="qtype"></param>
        /// <param name="query"></param>
        /// <param name="sortname"></param>
        /// <param name="sortorder"></param>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCustomerCntctPrsnInfo")]
        public TableData GetCustomerCntctPrsnInfo(string page, string rp, string qtype, string query, string sortname, string sortorder, string cardCode)
        {
            var result = new TableData();
            try
            {
                result = _serviceSaleOrderApp.GetCustomerCntctPrsnInfo(int.Parse(rp), int.Parse(page), query, sortname, sortorder, cardCode);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 根据客户代码查询客户地址
        /// </summary>
        /// <param name="page"></param>
        /// <param name="rp"></param>
        /// <param name="qtype"></param>
        /// <param name="query"></param>
        /// <param name="sortname"></param>
        /// <param name="sortorder"></param>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCustomerAddressInfo")]
        public TableData GetCustomerAddressInfo(string page, string rp, string qtype, string query, string sortname, string sortorder, string cardCode)
        {
            var result = new TableData();
            try
            {
                result = _serviceSaleOrderApp.GetCustomerAddressInfo(int.Parse(rp), int.Parse(page), query, sortname, sortorder, cardCode);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 查看呼叫服务信息
        /// </summary>
        [HttpGet]
        [Route("GetCustomerInfo")]
        public Response<GetCustomerInfoDto> GetCustomerInfo(string cardCode, string sboId)
        {
            var result = new Response<GetCustomerInfoDto>();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);

            result.Result = _serviceSaleOrderApp.GetCustomerDetailsInfo(cardCode, sboId, SboID.ToString());

            return result;
        }
        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="DocNum"></param>
        /// <param name="SboId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("UpdataSalesDoc")]
        public Response<bool> UpdataSalesDoc(string DocNum, string SboId, string type)
        {
            var result = new Response<bool>();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            result.Result = _serviceSaleOrderApp.UpdataSalesDoc(DocNum, SboID, type, UserID);
            return result;
        }
        #region 查看视图
        /// <summary>
        /// 查看视图
        /// </summary>
        [HttpGet]
        [Route("GridDataBind")]
        public TableData GridDataBind(string page, string rp, string qtype, string query, string sortname, string sortorder)
        {
            int rowCount = 0;
            var tabledata = new TableData();
            string type = "OQUT";
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            var DepID = _serviceBaseApp.GetSalesDepID(UserID);
            DataTable dt = _serviceBaseApp.GetSboNamePwd(SboID);
            string dRowData = string.Empty; string isOpen = "0"; string sqlcont = string.Empty; string sboname = string.Empty;
            if (dt.Rows.Count > 0)
            {
                isOpen = dt.Rows[0][6].ToString();
                sqlcont = dt.Rows[0][5].ToString(); sboname = dt.Rows[0][0].ToString();
            }
            try
            {
                if (isOpen == "0")
                {
                    DataTable datatable = _serviceSaleOrderApp.SelectBillViewInfo(out rowCount, int.Parse(rp), int.Parse(page), query, sortname, sortorder, type, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesQuotation.aspx", UserID).ViewFull, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesQuotation.aspx", UserID).ViewSelf, UserID, SboID, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesQuotation.aspx", UserID).ViewSelfDepartment, DepID, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesQuotation.aspx", UserID).ViewCustom, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesQuotation.aspx", UserID).ViewSales);
                    tabledata.Data = datatable;
                    tabledata.Count = rowCount;

                }
                else
                {
                    DataTable datatable = _serviceSaleOrderApp.SelectBillListInfo(out rowCount, int.Parse(rp), int.Parse(page), query, sortname, sortorder, type, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesQuotation.aspx", UserID).ViewFull, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesQuotation.aspx", UserID).ViewSelf, UserID, SboID, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesQuotation.aspx", UserID).ViewSelfDepartment, DepID, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesQuotation.aspx", UserID).ViewCustom, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesQuotation.aspx", UserID).ViewSales, sqlcont, sboname);
                    tabledata.Data = datatable;
                    tabledata.Count = rowCount;

                }
            }
            catch (Exception e)
            {
                tabledata.Message = e.Message;
            }

            return tabledata;
        }
        /// <summary>
        /// PDF打印（老）
        /// </summary>
        /// <param name="val"></param>
        /// <param name="Indicator"></param>
        /// <param name="sboid"></param>
        /// <param name="DocEntry"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ExportShow")]
        public Response<string> ExportShow(string val, string Indicator, string sboid, string DocEntry)
        {
            var result = new Response<string>();
            string host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host;
            try
            {
                result.Result = _serviceSaleOrderApp.ExportShow(val, Indicator, sboid, DocEntry, host);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }
        #endregion
        #region
        /// <summary>
        /// PDF打印（新）
        /// </summary>
        /// <param name="val"></param>
        /// <param name="Indicator"></param>
        /// <param name="sboid"></param>
        /// <param name="DocEntry"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ExportShowNew")]
        public async Task<FileResult> ExportShow(string sboid, string DocEntry)
        {
            try
            {
                return File(await _serviceSaleOrderApp.ExportShow(sboid, DocEntry), "application/pdf");
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{DocEntry}， 错误：{ex.Message}");
                throw new Exception("导出失败！" + ex.ToString());
            }
        }
        #endregion
        #region 根据页面地址获取FunId.
        /// <summary>
        /// 根据页面地址获取FunId
        /// </summary>
        /// <param name="Address"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetJobTypeByAddress")]
        public Response<string> GetJobTypeByAddress(string Address)
        {
            var result = new Response<string>();
            try
            {
                result.Result = _serviceSaleOrderApp.GetJobTypeByAddress(Address);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }
        #endregion
        #region 判断审核里是否已经提交该单据
        /// <summary>
        /// 判断审核里是否已经提交该单据
        /// </summary>
        /// <param name="base_entry"></param>
        /// <param name="base_type"></param>
        /// <param name="funId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("IsExistDoc")]
        public Response<bool> IsExistDoc(string base_entry, string base_type)
        {
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            var result = new Response<bool>();
            try
            {
                result.Result = _serviceSaleOrderApp.IsExistDoc(base_entry, base_type, SboID.ToString());
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }
        #endregion
        #region 货币类型
        /// <summary>
        /// 货币类型
        /// </summary>
        /// <returns></returns>
        // 

        [HttpGet]
        [Route("DropPopupDocCur")]
        public Response<List<DropPopupDocCurDto>> DropPopupDocCur()
        {
            var result = new Response<List<DropPopupDocCurDto>>();
            try
            {
                result.Result = _serviceSaleOrderApp.DropPopupDocCur();

            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }

        #endregion
        #region 仓库
        /// <summary>
        /// 仓库
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("DropPopupWhsCode")]
        public Response<List<DropPopupDocCurDto>> DropPopupWhsCode()
        {
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            var result = new Response<List<DropPopupDocCurDto>>();
            try
            {
                result.Result = _serviceSaleOrderApp.DropPopupWhsCode(SboID);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;

        }

        #endregion

        #region 开关项
        /// <summary>
        ///开关项
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="filevalue"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("IsSwitching")]
        public Response<bool> IsSwitching(string tablename, string filevalue)
        {
            var result = new Response<bool>();
            result.Result = _serviceSaleOrderApp.IsSwitching(tablename, filevalue);
            return result;
        }
        #endregion
        /// <summary>
        /// 销售员
        /// </summary>
        /// <param name="SboId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DropPopupSlpCode")]
        public Response<List<GetItemTypeCustomValueDto>> DropPopupSlpCode(string SboId)
        {
            var result = new Response<List<GetItemTypeCustomValueDto>>();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            try
            {
                if (string.IsNullOrEmpty(SboId)) { SboId = SboID.ToString(); }
                result.Result = _serviceSaleOrderApp.DropPopupSlpCode(int.Parse(SboId));
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        ///联系人名字
        /// </summary>
        /// <param name="CntctCode"></param>
        /// <param name="SboId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetConfiguresCntctPrsn")]
        public Response<string> GetConfiguresCntctPrsn(string CntctCode, string SboId)
        {
            var result = new Response<string>();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            try
            {
                int billSboId = 0; bool isSql = true;
                if (int.Parse(SboId) == SboID || SboId == "") { billSboId = SboID; } else { billSboId = int.Parse(SboId); isSql = false; }
                result.Result = _serviceSaleOrderApp.GetConfiguresCntctPrsn(CntctCode, billSboId, isSql);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 应收
        /// </summary>
        /// <param name="SlpCode"></param>
        /// <param name="CardCode"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSumBalDue")]
        public string GetSumBalDue(string SlpCode, string CardCode, string type)
        {
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            try
            {
                return _serviceSaleOrderApp.GetSumBalDue(SlpCode, CardCode, type, SboID);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        /// <summary>
        /// 获取地址标识(开票到、运达到)
        /// </summary>
        /// <param name="AdresType"></param>
        /// <param name="CardCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAddressNos")]
        public Response<string> GetAddressNos(string AdresType, string CardCode)
        {
            var result = new Response<string>();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            try
            {
                if (!string.IsNullOrEmpty(AdresType) && !string.IsNullOrEmpty(CardCode))
                {
                    result.Result = _serviceSaleOrderApp.GetAddress(AdresType, CardCode, SboID);
                }
                else { return null; }
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 拟取消订单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GridRelORDRList")]
        public TableData GridRelORDRList(GridRelORDRReq model)
        {
            int rowCount = 0;
            var result = new TableData();

            result.Data = _serviceSaleOrderApp.GridRelORDRList(out rowCount, model.limit, model.page, model.DocEntry, model.cardcode, model.sortname, model.sortorder, model.SlpCode);
            result.Count = rowCount;
            return result;

        }
        [HttpGet]
        [Route("GridRelORDR")]
        public TableData GridRelORDR(string SlpCode, string DocEntry, string cardcode)
        {
            int rowCount = 0;
            var result = new TableData();

            result.Data = _serviceSaleOrderApp.GridRelORDR(out rowCount, 20, 1, DocEntry, cardcode, "docentry", "desc", SlpCode);
            result.Count = rowCount;
            return result;

        }
        /// <summary>
        /// 销售员详情
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SelectAccountsView")]
        public TableData SelectAccountsView(SelectAccountsReq model)
        {
            var result = new TableData();
            int rowCount = 0;
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            result.Data = _serviceSaleOrderApp.SelectAccountsView(out rowCount, model, SboID.ToString());
            result.Count = rowCount;
            return result;

        }
        /// <summary>
        ///根据单据获取物料（复制）
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GridCopyItemList")]
        public TableData GridCopyItemList(GridCopyItemListReq model)
        {
            int rowCount = 0;
            var result = new TableData();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            result.Data = _serviceSaleOrderApp.SelectCopyItemAllView(out rowCount, model, SboID, UserID);
            result.Count = rowCount;
            return result;


        }

        /// <summary>
        ///合约评审
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GridRelationContractList")]
        public TableData GridRelationContractList(GridRelationContractListReq model)
        {
            int rowCount = 0;
            var result = new TableData();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            result.Data = _serviceSaleOrderApp.GridRelationContractList(out rowCount, model, SboID, UserID);
            result.Count = rowCount;
            return result;
        }
        /// <summary>
        /// 合约评审PDF
        /// </summary>
        /// <param name="contractId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ContractExportShow_ForSale")]
        public Response<string> ContractExportShow_ForSale(string contractId)
        {
            var result = new Response<string>();
            string host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host;
            try
            {
                result.Result = _serviceSaleOrderApp.ContractExportShow_ForSale(contractId, host);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 销售报价单审核
        /// </summary>
        /// <param name="resubmitReq">请求参数</param>
        /// <returns></returns>
        [HttpPost]
        [Route("AuditResubmitNextNew")]
        public Response<string> AuditResubmitNextNew(AuditResubmitReq resubmitReq)
        {
            var result = new Response<string>();
            try
            {
                var userId = _serviceBaseApp.GetUserNaspId();
                result.Result = _serviceSaleOrderApp.AuditResubmitNextNew(resubmitReq.jobId, userId, resubmitReq.recommend, resubmitReq.auditOpinionid, resubmitReq.IsUpdate, resubmitReq.vStock, resubmitReq.Comments, resubmitReq.Remark);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }


        /// <summary>
        /// 获取业务伙伴的默认联系人
        /// </summary>
        [HttpGet]
        [Route("GetCntctCode")]
        public Response<string> GetCntctCode(string Code, string Name, int SboId, bool isSql)
        {
            var result = new Response<string>();
            DataTable dt = _serviceSaleOrderApp.GetSboNamePwd(SboId);
            string dRowData = string.Empty; string isOpen = "0";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); }
            if (isSql && isOpen == "1")
            {
                result.Result = _serviceSaleOrderApp.GetCntctCodeSql(Code, Name, SboId);
            }
            else
            {
                result.Result = _serviceSaleOrderApp.GetCntctCode(Code, Name, SboId);
            }
            return result;
        }


        [HttpGet]
        [Route("GetPagePowersByUrl")]
        public Response<string> GetPagePowersByUrl(string url)
        {
            var result = new Response<string>();
            try
            {
                result.Result = _serviceSaleOrderApp.GetPagePowersByUrlWithClient(url);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 根据业务伙伴代码获取相关数据
        /// </summary>
        /// <param name="CardCode"></param>
        /// <param name="SboId"></param>
        /// <param name="billViewFull"></param>
        /// <param name="billViewSelfDepartment"></param>
        /// <param name="billViewSelf"></param>
        /// <returns></returns>
        // 
        [HttpGet]
        [Route("GetCardInfo")]
        public TableData GetCardInfo(string CardCode, string SboId = "1", bool billViewFull = true, bool billViewSelfDepartment = true, bool billViewSelf = true)
        {
            var result = new TableData();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            var DepID = _serviceBaseApp.GetSalesDepID(UserID);
            try
            {
                int billSboId = 0; bool isSql = true;
                if (int.Parse(SboId) == SboID || SboId == "") { billSboId = SboID; } else { billSboId = int.Parse(SboId); isSql = false; }
                result.Data = _serviceSaleOrderApp.GetCardInfo(CardCode, billSboId, isSql, billViewSelf, billViewSelfDepartment, billViewFull, UserID, DepID);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 审核备注
        /// </summary>
        /// <param name="DocEntry"></param>
        /// <param name="SboId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSaleQuotationRemarkById")]
        public Response<string> GetSaleQuotationRemarkById(string DocEntry, string SboId)
        {
            var result = new Response<string>();
            try
            {
                result.Result = _serviceSaleOrderApp.GetSaleQuotationRemarkById(DocEntry, SboId);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;

        }
        /// <summary>
        /// 调用商城接口
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetShopProduct")]
        public async Task<Response<ShopProductDto>> GetShopProduct(string ErpCode)
        {
            var result = new Response<ShopProductDto>();
            try
            {
                //设置请求的路径
                var url = $"http://shopapi.neware.work:8081/api/v1/product/shopproduct?erpCode={ErpCode}";
                ////使用注入的httpclientfactory获取client
                var client = _httpClient.CreateClient();
                client.BaseAddress = new Uri(url);
                //设置请求体中的内容，并以post的方式请求
                var response = await client.GetAsync(url);

                //var request = new HttpRequestMessage(HttpMethod.Get, "http://shopapi.neware.work:8081/?ErpCode={ErpCode}");
                //HttpResponseMessage response = await client.SendAsync(request);
                //获取请求到数据，并转化为字符串
                //result.Result = response.Content.ReadAsStringAsync().Result;
                result.Result = JsonConvert.DeserializeObject<ShopProductDto>(response.Content.ReadAsStringAsync().Result);

            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;

        }
    }
}
