using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Order;
using OpenAuth.App.Order.Request;
using OpenAuth.App.Response;
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
    /// 销售交货
    /// </summary>
    [Route("api/Order/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Order")]
    public class SalesDeliveryController : Controller
    {
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
        private readonly SalesDeliveryApp _salesDeliveryApp;
        ServiceBaseApp _serviceBaseApp;

        IAuth _auth;
        IUnitWork _UnitWork;

        //ServiceBaseApp _serviceBaseApp;
        public SalesDeliveryController(ServiceBaseApp serviceBaseApp, IUnitWork UnitWork, SalesDeliveryApp salesDeliveryApp, /*ServiceBaseApp _serviceBaseApp,*/ IAuth _auth, ServiceSaleOrderApp serviceSaleOrderApp)
        {
            this._UnitWork = UnitWork;
            //this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            this._serviceSaleOrderApp = serviceSaleOrderApp;
            this._salesDeliveryApp = salesDeliveryApp;
            this._serviceBaseApp = serviceBaseApp;

        }
        #region 添加销售交货
        /// <summary>
        /// 添加销售交货
        /// </summary>
        [HttpPost]
        public async Task<Response<string>> SalesDeliverySave(SalesDeliverySaveReq salesDeliverySaveReq)
        {
            var result = new Response<string>();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            try
            {
                string funcId = "0"; string logstring = ""; string jobname = "";
                funcId = _serviceSaleOrderApp.GetJobTypeByAddress("sales/SalesDelivery.aspx");
                logstring = "根据销售订单下销售交货单"; jobname = "销售交货";
                result.Result = await _salesDeliveryApp.SalesDeliverySave(salesDeliverySaveReq, UserID, int.Parse(funcId), "0", jobname, SboID, "0");
                return result;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{salesDeliverySaveReq}， 错误：{ex.Message}");
                throw new Exception("导出失败！" + ex.ToString());
            }
        }
        #endregion
        #region 查看视图
        /// <summary>
        /// 查看视图
        /// </summary>
        [HttpPost]
        [Route("GridDataBind")]
        public TableData GridDataBind(SalesDeliveryListReq salesDeliveryListReq)
        {
            var TableData = new TableData();
            int rowCount = 0;
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            var DepID = _serviceBaseApp.GetSalesDepID(UserID);
            string type = "ODLN";
            DataTable dt = _serviceSaleOrderApp.GetSboNamePwd(SboID);
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
                    TableData.Data = _serviceSaleOrderApp.SelectBillViewInfo(out rowCount, salesDeliveryListReq.limit, salesDeliveryListReq.page, salesDeliveryListReq.qtype, salesDeliveryListReq.sortname, salesDeliveryListReq.sortorder, type, true, true, UserID, SboID, true, DepID, true, true);
                    TableData.Count = rowCount;
                }
                else
                {
                    //默认接口 
                    TableData.Data = _serviceSaleOrderApp.SelectBillListInfo(out rowCount, salesDeliveryListReq.limit, salesDeliveryListReq.page, salesDeliveryListReq.query, salesDeliveryListReq.sortname, salesDeliveryListReq.sortorder, type, true, true, UserID, SboID, true, DepID, true, true, sqlcont, sboname);
                    TableData.Count = rowCount;
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{salesDeliveryListReq}， 错误：{ex.Message}");
                throw new Exception("查询列表失败！" + ex.ToString());
            }
            return TableData;
        }
        #endregion
        #region 销售交货详情
        /// <summary>
        /// 销售交货详情
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        //[Route("QuerySaleDetailsNew")]
        //public async Task<TableData> QuerySaleDetailsNew(QuerySaleDetailsNewReq querySaleDetailsNewReq)
        //{
        //    var result = new TableData();
        //    DataTable dtTable = await _salesDeliveryApp.QuerySaleDetailsNew(querySaleDetailsNewReq.DocNum, int.Parse(querySaleDetailsNewReq.SboId), querySaleDetailsNewReq.tablename, querySaleDetailsNewReq.ViewCustom, querySaleDetailsNewReq.ViewSales);
        //    if (dtTable.Rows.Count == 0) return null;
        //    string _manager = NSAP.Data.Sales.BillDelivery.DropPopupOwnerCodeNew(SboId, dtTable.Rows[0][31].ToString()).DataTableToJSON();
        //    string _sales = NSAP.Data.Sales.BillDelivery.DropPopupSlpCodeNew(SboId, dtTable.Rows[0][18].ToString()).DataTableToJSON();
        //    string _mark = NSAP.Data.Sales.BillDelivery.DropPopupIndicatorNew(SboId, dtTable.Rows[0][24].ToString()).DataTableToJSON();
        //    string _shipType = NSAP.Data.Sales.BillDelivery.DropPopupTrnspCodeNew(SboId, dtTable.Rows[0][19].ToString()).DataTableToJSON();
        //    string _paymentCond = NSAP.Data.Sales.BillDelivery.GetGroupNumNew(SboId, dtTable.Rows[0][20].ToString()).DataTableToJSON();
        //    string _andbuy = NSAP.Data.Sales.BillDelivery.DropPopupSlpCodeNew(SboId, dtTable.Rows[0][35].ToString()).DataTableToJSON();
        //    string _docCur = NSAP.Data.Sales.BillDelivery.DropPopupDocCurNew(dtTable.Rows[0][4].ToString(), SboId).DataTableToJSON();
        //    string _cntctCode = NSAP.Data.Sales.BillDelivery.DropPopupCntctPrsnNew(dtTable.Rows[0][0].ToString(), SboId, isCopy == "1" ? "0" : dtTable.Rows[0][2].ToString()).DataTableToJSON();
        //    string _shipToCode = NSAP.Data.Sales.BillDelivery.GetAddressNew("S", dtTable.Rows[0][0].ToString(), SboId).DataTableToJSON();
        //    string _payToCode = NSAP.Data.Sales.BillDelivery.GetAddressNew("B", dtTable.Rows[0][0].ToString(), SboId).DataTableToJSON();
        //    string _balance = NSAP.Data.Sales.BillDelivery.SelectBalanceNew(dtTable.Rows[0][0].ToString(), SboId.ToString());
        //    string _balanceS = NSAP.Data.Sales.BillDelivery.GetSumBalDueNew(dtTable.Rows[0][18].ToString(), "C", SboId.ToString());
        //    DataTable _lineTable = new DataTable(); //NSAP.Data.Sales.BillDelivery.GetItemCodeListNew(DocNum, linename, ViewSales, SboId);
        //    if (tablename.ToString().ToLower() == "buy_opor" || tablename.ToString().ToLower() == "opor")
        //    {
        //        _lineTable = NSAP.Data.Sales.BillDelivery.GetItemCodeListPur(DocNum, linename, ViewSales, SboId);
        //    }
        //    else
        //    {
        //        _lineTable = NSAP.Data.Sales.BillDelivery.GetItemCodeListNew(DocNum, linename, ViewSales, SboId);
        //    }
        //    //获得关联报价单的物料成本
        //    DataTable _stockInfo = NSAP.Data.Sales.BillDelivery.GetCallInfoById(DocNum, SboId.ToString(), tablename, "0", "stock");
        //    if (_stockInfo.Rows.Count > 0 && _stockInfo.Rows[0][0].ToString() != "")
        //    {
        //        for (int i = 0; i < _lineTable.Rows.Count; i++)
        //        {
        //            for (int j = 0; j < _stockInfo.Rows.Count; j++)
        //            {
        //                if (_lineTable.Rows[i][1].ToString() == _stockInfo.Rows[j][1].ToString())
        //                {
        //                    _lineTable.Rows[i][9] = _stockInfo.Rows[j][2];
        //                }
        //            }
        //        }
        //    }

        //    string _line = _lineTable.DataTableToJSON();
        //    string _customNew = NSAP.Data.Sales.BillDelivery.QuerySalesCustomNew(DocNum, SboId, tablename).DataRowByIndexToJSON(0);
        //    string _storehouse = NSAP.Data.Sales.BillDelivery.DropPopupWhsCodeNew(SboId, "0").DataTableToJSON();
        //    string _attachmentData = "";
        //    if (!string.IsNullOrEmpty(funcID))
        //    {
        //        int fileType = int.Parse(NSAP.Data.Sales.BillDelivery.GetattchtypeByfuncid(int.Parse(funcID)).Rows[0]["type_id"].ToString());
        //        _attachmentData = NSAP.Data.Sales.BillDelivery.GetFilesList(DocNum, fileType.ToString(), SboId).DataTableToJSON();
        //    }

        //    //服务呼叫
        //    DataTable _callInfo = NSAP.Data.Sales.BillDelivery.GetCallInfoById(DocNum, SboId.ToString(), tablename, "0", "call");
        //    string U_CallID = "", U_CallName = "", U_SerialNumber = "";
        //    if (_callInfo.Rows.Count > 0 && _callInfo.Rows[0][0].ToString() != "")
        //    {
        //        U_CallID = _callInfo.Rows[0][0].ToString();
        //        U_CallName = _callInfo.Rows[0][1].ToString();
        //        U_SerialNumber = _callInfo.Rows[0][2].ToString();
        //    }
        //    if ((tablename == "sale_ordn" || tablename == "sale_orin") && !string.IsNullOrEmpty(dtTable.Rows[0][37].ToString()))
        //    {
        //        U_CallID = dtTable.Rows[0][37].ToString();
        //        U_CallName = dtTable.Rows[0][38].ToString();
        //        U_SerialNumber = dtTable.Rows[0][39].ToString();
        //    }

        //    string _main = dtTable.DataRowByIndexToJSON(0);
        //    StringBuilder sMain = new StringBuilder();
        //    var billSalesName = string.Empty;
        //    if (dtTable.Rows[0][7].ToString() == "I")
        //    {
        //        billSalesName = "billSalesDetails";
        //    }
        //    else
        //    {
        //        billSalesName = "billSalesAcctCode";
        //    }
        //    sMain.AppendFormat("{0},\"WhsCode\":\"{3}\",\"" + billSalesName + "\":{1},\"CustomFields\":{2},\"attachmentData\":{4},\"U_CallID\":\"{5}\",\"U_CallName\":\"{6}\",\"U_SerialNumber\":\"{7}\"", _main.TrimEnd('}'), _line, _customNew, _lineTable.Rows[0]["WhsCode"].ToString(), _attachmentData, U_CallID, U_CallName, U_SerialNumber);
        //    sMain.Append("}");
        //    StringBuilder sBuilder = new StringBuilder("{");
        //    sBuilder.AppendFormat("\"manager\":{0},\"sales\":{1},\"mark\":{2},\"shipType\":{3},\"storehouse\":{14},\"paymentCond\":{4},\"paymentMode\":{5},\"andbuy\":{6},\"docCur\":{7},\"cntctCode\":{8},\"balance\":{9},\"balanceS\":{10},\"main\":{11},\"shipToCode\":{12},\"payToCode\":{13}",
        //        _manager, _sales, _mark, _shipType, _paymentCond, "[]", _andbuy, _docCur, _cntctCode, _balance == "" ? "0" : _balance, _balanceS == "" ? "0" : _balanceS, sMain, _shipToCode, _payToCode, _storehouse
        //    );
        //    sBuilder.Append("}");
        //    return sBuilder.ToString();
        //}
        #endregion


    }
}
