
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.OpenXml4Net.OPC.Internal;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Order;
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
        public OrderDraftController(IUnitWork UnitWork, IAuth _auth, ServiceSaleOrderApp serviceSaleOrderApp)
        {
            this.UnitWork = UnitWork;
            this._auth = _auth;
            _serviceSaleOrderApp = serviceSaleOrderApp;
        }
        /// <summary>
        /// 获取账套数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("dboinfo")]
        public async Task<List<SboInfoDto>> DboInfo()
        {
            List<SboInfoDto> sboList = UnitWork.ExcuteSql<SboInfoDto>(ContextType.NsapBaseDbContext, "SELECT sbo_id AS id,sbo_nm AS name FROM nsap_base.sbo_info;", CommandType.Text, null).OrderBy(s => s.Id).ToList();
            return sboList;
        }
        /// <summary>
        /// 业务伙伴列表
        /// </summary>
        [HttpGet]
        [Route("cardcodeview")]
        public async Task<TableData> LoadCardCodeViewAsync([FromQuery]CardCodeRequest request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var userId = (await UnitWork.Find<NsapUserMap>(n => n.UserID.Equals(loginContext.User.Id)).FirstOrDefaultAsync())?.NsapUserId;
            var depId = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, $"SELECT dep_id value FROM base_user_detail WHERE user_id = {userId}", CommandType.Text, null).FirstOrDefault();
            string type = "SQO";
            var sboid = UnitWork.ExcuteSql<sbo_info>(ContextType.Nsap4ServeDbContextType, "SELECT sbo_id FROM nsap_base.sbo_info WHERE is_curr = 1 AND valid = 1 LIMIT 1;", CommandType.Text, null).FirstOrDefault()?.sbo_id;
            var dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT sql_db,sql_name,sql_pswd,sap_name,sap_pswd,sql_conn,is_open FROM nsap_base.sbo_info WHERE sbo_id={sboid}", CommandType.Text, null);
            string dRowData = string.Empty;
            string isOpen = "0";
            string sqlcont = string.Empty;
            string sboname = string.Empty;
            string sortString = string.Empty;
            string filterString = string.Empty;
            if (dt.Rows.Count > 0)
            {
                isOpen = dt.Rows[0][6].ToString();
                sqlcont = dt.Rows[0][5].ToString();
                sboname = dt.Rows[0][0].ToString();
            }
            if (!string.IsNullOrEmpty(request.SortName) && !string.IsNullOrEmpty(request.SortName))
            {
                sortString = string.Format("{0} {1}", request.SortName, request.SortOrder.ToUpper());
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
              //  return NSAP.Data.Sales.BillDelivery.SelectCardCodeList(out rowCount, pageSize, pageIndex, filterString, sortString, sboname, sqlconn).FelxgridDataToJSON(pageIndex.ToString(), rowCount.ToString());
            }
            return result;
        }
        /// <summary>
        /// 加载销售报价单列表
        /// </summary>
        [HttpGet]
        [Route("sales")]
        public async Task<TableData> LoadAsync([FromQuery]QuerySalesQuotationReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var userId = (await UnitWork.Find<NsapUserMap>(n => n.UserID.Equals(loginContext.User.Id)).FirstOrDefaultAsync())?.NsapUserId;
            var depId = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, $"SELECT dep_id value FROM base_user_detail WHERE user_id = {userId}", CommandType.Text, null).FirstOrDefault();
            string type = "OQUT";
            var sboid = UnitWork.ExcuteSql<sbo_info>(ContextType.Nsap4ServeDbContextType, "SELECT sbo_id FROM nsap_base.sbo_info WHERE is_curr = 1 AND valid = 1 LIMIT 1;", CommandType.Text, null).FirstOrDefault()?.sbo_id;
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
                result = _serviceSaleOrderApp.SelectOrderDraftInfo(request.limit, request.page, request, type, viewFull, viewSelf, userId.Value, sboid.Value, viewSelfDepartment, Convert.ToInt32(depId.Value), viewCustom, viewSales, sqlcont, sboname);
                // return NSAP.Biz.Sales.BillDelivery.SelectBillListInfo(int.Parse(rp), int.Parse(page), query, sortname, sortorder, type, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewFull, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewSelf, UserID, SboID, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewSelfDepartment, DepID, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewCustom, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesQuotation.aspx").ViewSales, sqlcont, sboname);
            }
            return result;
        }
        /// <summary>
        /// 获取业务员
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("SalesManInfo")]
      //  [AllowAnonymous]
        public async Task<Response<List<SelectOption>>> GetSalesManInfo()
        {
            var result = new Response<List<SelectOption>>();
            try
            {
                result.Result = _serviceSaleOrderApp.GetSalesSelect(0);
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
        /// 销售报价单保存
        /// </summary>
        /// <returns></returns>
        [HttpGet]
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
