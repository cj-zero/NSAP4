
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Order;
using OpenAuth.App.Order.Request;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository;
using OpenAuth.Repository.Interface;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        public OrderDraftController(IUnitWork UnitWork, ServiceBaseApp _serviceBaseApp, IAuth _auth, ServiceSaleOrderApp serviceSaleOrderApp)
        {
            this.UnitWork = UnitWork;
            this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            _serviceSaleOrderApp = serviceSaleOrderApp;
        }
        /// <summary>
        /// 获取业务伙伴账款
        /// </summary>
        /// <param name="cardCode">客户代码</param>
        /// <param name="slpCode"></param>
        /// <param name="type">C</param>
        /// <returns></returns>
        [HttpGet]
        [Route("sumbaldue")]
        public async Task<Response<CardInfoDto>> SumBalDue(string cardCode, string slpCode, string type = "C")
        {
            var result = new Response<CardInfoDto>();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            var depId = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, $"SELECT dep_id value FROM base_user_detail WHERE user_id = {userId}", CommandType.Text, null).FirstOrDefault();
            string balstr = "";
            decimal BalDue = 0;
            decimal Total = 0;
            DataTable sbotable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, "SELECT sbo_id AS id,sbo_nm AS name FROM nsap_base.sbo_info;", CommandType.Text, null);
            sbotable.Columns.Add("BalSboAmount", typeof(decimal));
            foreach (DataRow sborow in sbotable.Rows)
            {
                DataTable baltable = new DataTable();
                if (!string.IsNullOrEmpty(slpCode))
                {
                    //baltable = NSAP.Data.Sales.BillDelivery.GetSalesSboBalPercent(slpCode, type, sborow["id"].ToString());
                    //if (baltable.Rows.Count > 0)
                    //{
                    //    decimal tempDue = 0, tempTotal = 0;
                    //    if (!string.IsNullOrEmpty(baltable.Rows[0]["BalDue"].ToString()) && decimal.TryParse(baltable.Rows[0]["BalDue"].ToString(), out tempDue))
                    //    {
                    //        BalDue += tempDue;
                    //    }
                    //    if (!string.IsNullOrEmpty(baltable.Rows[0]["Total"].ToString()) && decimal.TryParse(baltable.Rows[0]["Total"].ToString(), out tempTotal))
                    //    {
                    //        Total += tempTotal;
                    //    }
                    //    sborow["BalSboAmount"] = tempDue.ToString("#0.00");
                    //}
                }
                else if (!string.IsNullOrEmpty(cardCode))
                {
                    baltable = null;//NSAP.Data.Client.ClientInfo.GetClientSboBalPercent(cardCode, sborow["id"].ToString());
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

                        sborow["BalSboAmount"] = tempBalance.ToString("#0.00");
                    }
                }
            }
            //当前账套金额
            decimal due90 = 0; decimal total90 = 0;
            DataTable curbaltab = null;//NSAP.Data.Client.ClientInfo.GetClientSboBalPercent(cardCode, SboId.ToString());
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
           // balstr = "[{\"BalDue\":\"" + BalDue.ToString("#0.00").FilterString() + "\",\"Total\":\"" + Total.ToString("#0.00").FilterString() + "\",\"BalSboDetails\":" + sbotable.DataTableToJSON() + ",\"Due90\":\"" + due90.ToString("#0.00").FilterString() + "\",\"Total90\":\"" + total90.ToString("#0.00").FilterString() + "\"}]";
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
            result.Result = UnitWork.ExcuteSql<DropDownOption>(ContextType.NsapBaseDbContext, $@"	SELECT b.CntctCode AS id,b.Name AS name FROM  nsap_bone.crm_ocrd a LEFT JOIN  nsap_bone.crm_ocpr b ON a.CardCode=b.CardCode and a.sbo_id=b.sbo_id WHERE a.CardCode='{code}' and a.sbo_id={billSboId} ", CommandType.Text, null);
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
            if (!string.IsNullOrEmpty(request.SortName) && !string.IsNullOrEmpty(request.SortName))
            {
                sortString = string.Format("{0} {1}", request.SortName, request.SortOrder.ToUpper());
                //sortName = " " + request.SortName + " " + request.SortOrder;
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
        /// </summary>pp
        [HttpPost]
        [Route("sales")]
        public async Task<TableData> LoadAsync(QuerySalesQuotationReq request)
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
                viewFull = powers.ViewFull;
                viewSelf = powers.ViewSelf;
                viewSelfDepartment = powers.ViewSelfDepartment;
                viewSales = powers.ViewSales;
                viewCustom = powers.ViewCustom;
            }
            if (isOpen == "0")
            {

                // return NSAP.Biz.Sales.BillDelivery.SelectBillViewInfo(int.Parse(rp), int.Parse(page), query, sortname, sortorder, type, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewFull, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewSelf, UserID, SboID, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewSelfDepartment, DepID, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewCustom, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewSales);
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
        [AllowAnonymous]
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
        public async Task<Response<string>> Save(AddOrUpdateOrderReq orderReq)
        {
            var result = new Response<string>();
            try
            {
                result.Result = _serviceSaleOrderApp.Save(orderReq);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}, 错误：{result.Message}");
            }
            return result;
        }
    }
}
