using Infrastructure;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Order;
using OpenAuth.App.Order.ModelDto;
using OpenAuth.App.Order.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Interface;
using Serilog;
using System;
using System.Data;
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
        public SalesDeliveryController(ServiceBaseApp serviceBaseApp, IUnitWork UnitWork, SalesDeliveryApp salesDeliveryApp, IAuth _auth, ServiceSaleOrderApp serviceSaleOrderApp)
        {
            this._UnitWork = UnitWork;
            this._auth = _auth;
            this._serviceSaleOrderApp = serviceSaleOrderApp;
            this._salesDeliveryApp = salesDeliveryApp;
            this._serviceBaseApp = serviceBaseApp;

        }
        #region 销售订单转交货
        /// <summary>
        /// 销售订单转交货
        /// </summary>
        [HttpPost]
        [Route("SalesDeliverySave")]
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{salesDeliverySaveReq}， 错误：{ex.Message},堆栈信息：{ex.StackTrace}");
                throw new Exception("添加失败！" + ex.ToString());
            }
        }
        #endregion

        #region 查看视图/列表
        /// <summary>
        /// 查看视图/列表
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
        [HttpPost]
        [Route("QuerySaleDetailsNew")]
        public async Task<Response<QuerySaleDetailsNewDto>> QuerySaleDetailsNew(QuerySaleDetailsNewReq querySaleDetailsNewReq)
        {
            var result = new Response<QuerySaleDetailsNewDto>();
            var data = new QuerySaleDetailsNewDto();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            data.main = await _salesDeliveryApp.QuerySaleDetailsNew(querySaleDetailsNewReq.DocNum, int.Parse(querySaleDetailsNewReq.SboId), querySaleDetailsNewReq.tablename, querySaleDetailsNewReq.ViewCustom, querySaleDetailsNewReq.ViewSales);
            if (data.main == null) return result;
            data.manager = await _salesDeliveryApp.DropPopupOwnerCodeNew(SboID, data.main.OwnerCode.ToString());
            data.sales = await _salesDeliveryApp.DropPopupSlpCodeNew(SboID, data.main.SlpCode.ToString());
            data.mark = await _salesDeliveryApp.DropPopupIndicatorNew(SboID, data.main.Indicator.ToString());
            data.shipType = await _salesDeliveryApp.DropPopupTrnspCodeNew(SboID, data.main.TrnspCode.ToString());
            data.paymentCond = await _salesDeliveryApp.GetGroupNumNew(SboID, data.main.GroupNum.ToString());
            data.andbuy = await _salesDeliveryApp.DropPopupSlpCodeNew(SboID, data.main.SlpCode.ToString());
            data.docCur = await _salesDeliveryApp.DropPopupDocCurNew(data.main.DocCur.ToString(), SboID);
            data.cntctCode = await _salesDeliveryApp.DropPopupCntctPrsnNew(data.main.CardCode.ToString(), SboID, querySaleDetailsNewReq.isCopy == "1" ? "0" : data.main.CntctCode.ToString());
            data.shipToCode = await _salesDeliveryApp.GetAddressNew("S", data.main.CardCode.ToString(), SboID);
            data.payToCode = await _salesDeliveryApp.GetAddressNew("B", data.main.CardCode.ToString(), SboID);
            data.balance = await _salesDeliveryApp.SelectBalanceNew(data.main.CardCode.ToString(), SboID.ToString());
            data.balanceS = await _salesDeliveryApp.GetSumBalDueNew(data.main.SlpCode.ToString(), "C", SboID.ToString());
            DataTable _lineTable = new DataTable(); //NSAP.Data.Sales.BillDelivery.GetItemCodeListNew(DocNum, linename, ViewSales, SboId);

            _lineTable = await _salesDeliveryApp.GetItemCodeListNew(querySaleDetailsNewReq.DocNum, querySaleDetailsNewReq.linename, querySaleDetailsNewReq.ViewSales, int.Parse(querySaleDetailsNewReq.SboId));

            //获得关联报价单的物料成本
            DataTable _stockInfo = _serviceSaleOrderApp.GetCallInfoById(querySaleDetailsNewReq.DocNum, querySaleDetailsNewReq.SboId.ToString(), querySaleDetailsNewReq.tablename, "0", "stock");
            if (_stockInfo.Rows.Count > 0 && _stockInfo.Rows[0][0].ToString() != "")
            {
                for (int i = 0; i < _lineTable.Rows.Count; i++)
                {
                    for (int j = 0; j < _stockInfo.Rows.Count; j++)
                    {
                        if (_lineTable.Rows[i][1].ToString() == _stockInfo.Rows[j][1].ToString())
                        {
                            _lineTable.Rows[i][9] = _stockInfo.Rows[j][2];
                        }
                    }
                }
            }
            data.lineTable = _lineTable;
            data.CustomNew = await _salesDeliveryApp.QuerySalesCustomNew(querySaleDetailsNewReq.DocNum, int.Parse(querySaleDetailsNewReq.SboId), querySaleDetailsNewReq.tablename);
            data.Storehouse = await _salesDeliveryApp.DropPopupWhsCodeNew(int.Parse(querySaleDetailsNewReq.SboId), "0");
            if (!string.IsNullOrEmpty(querySaleDetailsNewReq.funcID))
            {
                int fileType = int.Parse(_serviceSaleOrderApp.GetattchtypeByfuncid(int.Parse(querySaleDetailsNewReq.funcID)).Rows[0]["type_id"].ToString());
                data.AttachmentData = await _salesDeliveryApp.GetFilesList(querySaleDetailsNewReq.DocNum, fileType.ToString(), int.Parse(querySaleDetailsNewReq.SboId));
            }

            //服务呼叫
            DataTable _callInfo = _serviceSaleOrderApp.GetCallInfoById(querySaleDetailsNewReq.DocNum, querySaleDetailsNewReq.SboId.ToString(), querySaleDetailsNewReq.tablename, "0", "call");
            string U_CallID = "", U_CallName = "", U_SerialNumber = "";
            if (_callInfo.Rows.Count > 0 && _callInfo.Rows[0][0].ToString() != "")
            {
                U_CallID = _callInfo.Rows[0][0].ToString();
                U_CallName = _callInfo.Rows[0][1].ToString();
                U_SerialNumber = _callInfo.Rows[0][2].ToString();
            }
            data.WhsCode = _lineTable.Rows[0]["WhsCode"].ToString();

            var billSalesName = string.Empty;
            if (data.main.DocType.ToString() == "I")
            {
                billSalesName = "billSalesDetails";
            }
            else
            {
                billSalesName = "billSalesAcctCode";
            }
            data.billSalesName = billSalesName;
            data.main.U_CallID = U_CallID;
            data.main.U_CallName = U_CallName;
            data.main.U_SerialNumber = U_SerialNumber;
            data.CustomFields = data.CustomNew.DataRowByIndexToJSON(0);
            result.Result = data;
            return result;
        }
        #endregion

        #region 销售交货草稿保存和提交
        /// <summary>
        /// 销售交货草稿保存和提交
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        [Route("SalesSaveDraft")]
        public async Task<Response<string>> SalesSaveDraft(SalesSaveDraftReq salesSaveDraftReq)
        {
            var result = new Response<string>();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            try
            {
                result.Result = await _salesDeliveryApp.SalesSaveDraft(salesSaveDraftReq, UserID, SboID);
            }
            catch (Exception ex)
            {
                result.Result = "";
                result.Code = 500;
                result.Message = "服务器内部错误：" + ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{salesSaveDraftReq}， 错误：{ex.Message},堆栈信息：{ex.StackTrace}");
            }
            return result;
        }
        #endregion

    }
}
