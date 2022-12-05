using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Material;
using OpenAuth.App.Order;
using OpenAuth.App.Order.ModelDto;
using OpenAuth.App.Order.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using OpenAuth.App.PayTerm;

namespace OpenAuth.WebApi.Controllers.Order
{
    /// <summary>
    /// 销售订单
    /// </summary>
    [Route("api/Order/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Order")]
    public class SalesOrderController : Controller
    {
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
        private readonly PayTermApp _payTermApp;
        private readonly IOptions<AppSetting> _appConfiguration;
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        MaterialDesignApp _materialdesignapp;
        public SalesOrderController(IUnitWork UnitWork,PayTermApp payTermApp,  ServiceBaseApp _serviceBaseApp, IOptions<AppSetting> appConfiguration, IAuth _auth, ServiceSaleOrderApp serviceSaleOrderApp, MaterialDesignApp materialdesignapp)
        {
            this.UnitWork = UnitWork;
            this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            _serviceSaleOrderApp = serviceSaleOrderApp;
            _materialdesignapp = materialdesignapp;
            _payTermApp = payTermApp;
            _appConfiguration = appConfiguration;
        }
        #region 销售订单列表视图
        /// <summary>
        /// 查看视图
        /// </summary>
        [HttpPost]
        [Route("GridDataBind")]
        public TableData GridDataBind(SalesOrderListReq model)
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
            if (loginContext.Roles.Any(r => r.Name.Equals("销售订单管理员")))
            {
                rata = true;
            }

            string docEntrys = "";
            if (model.ReceiptStatus == "K")
            {
               docEntrys = _payTermApp.GetDocEntrys();
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
                    DataTable dts = _serviceSaleOrderApp.SelectBillListInfo_ORDR(out rowCount, docEntrys, model, type, rata, true, UserID, SboID, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesOrder.aspx", UserID).ViewSelfDepartment, DepID, true, true, sqlcont, sboname);
                    result.Data = dts;
                    result.Count = rowCount;
                }
                else
                {
                    DataTable dts = _serviceSaleOrderApp.SelectBillViewInfo(out rowCount, model.limit, model.page, model.query, model.sortname, model.sortorder, type, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesOrder.aspx", UserID).ViewFull, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesOrder.aspx", UserID).ViewSelf, UserID, SboID, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesOrder.aspx", UserID).ViewSelfDepartment, DepID, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesOrder.aspx", UserID).ViewCustom, _serviceSaleOrderApp.GetPagePowersByUrl("sales/SalesOrder.aspx", UserID).ViewSales);
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
        #endregion

        #region GetBaseEntrybyDocId
        /// <summary>
        /// GetBaseEntrybyDocId
        /// </summary>
        /// <param name="DocEntry"></param>
        /// <param name="SboId"></param>
        /// <param name="LineTable"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetBaseEntrybyDocId")]
        public Response<string> GetBaseEntrybyDocId(string DocEntry, string SboId, string LineTable)
        {
            var result = new Response<string>();
            try
            {
                result.Result = _serviceSaleOrderApp.GetBaseEntrybyDocId(DocEntry, SboId, LineTable);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }
        #endregion

        #region  联系人(业务伙伴所有联系人)
        /// <summary>
        ///  联系人(业务伙伴所有联系人)
        /// </summary>
        /// <param name="Code"></param>
        /// <param name="SboId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DropPopupCntctPrsn")]
        public TableData DropPopupCntctPrsn(string Code, string SboId)
        {
            var result = new TableData();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            try
            {
                int billSboId = 0; bool isSql = true;
                if (int.Parse(SboId) == SboID || SboId == "") { billSboId = SboID; } else { billSboId = int.Parse(SboId); isSql = false; }
                result.Data = _serviceSaleOrderApp.DropPopupCntctPrsn(Code, billSboId);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }
        #endregion

        #region 销售订单所属公司下拉
        /// <summary>
        /// 销售订单所属公司下拉
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("DropPopupIndicator")]

        public TableData DropPopupIndicator()
        {
            var result = new TableData();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            try
            {
                result.Data = _serviceSaleOrderApp.DropPopupIndicator(SboID);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }

        #endregion

        #region 账套下拉数据
        /// <summary>
        /// 账套下拉数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("DropPopupSboInfo")]
        public TableData DropPopupSboInfo()
        {
            var result = new TableData();
            try
            {
                result.Data = _serviceSaleOrderApp.DropPopupSboInfo();
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }
        #endregion

        #region 付款条件
        /// <summary>
        /// 付款条件
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("DropPopupGroupNum")]
        public TableData DropPopupGroupNum()
        {
            var result = new TableData();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            try
            {
                result.Data = _serviceSaleOrderApp.DropPopupGroupNum(SboID);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }
        #endregion

        #region  发票类型
        /// <summary>
        /// 发票类型
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("DropPopupInvoiceType")]
        public Response<string> DropPopupInvoiceType()
        {
            var result = new Response<string>();
            result.Result = "[{id:'0',name:'增值税普通发票'},{id:'1',name:'增值税专用发票'}]";
            return result;
        }
        /// <summary>
        /// 销售订单PDF打印
        /// </summary>
        /// <param name="sboid"></param>
        /// <param name="DocEntry"></param>
        /// <param name="Indicator"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("OrderExportShow")]
        public Response<string> OrderExportShow(string sboid, string DocEntry, string Indicator)
        {
            var result = new Response<string>();
            string host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host;
            try
            {
                result.Result = _serviceSaleOrderApp.OrderExportShow(sboid, DocEntry, Indicator, host);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 销售订单打印
        /// </summary>
        /// <param name="DocEntry">凭证编码</param>
        /// <param name="Indicator">公司标识</param>
        /// <returns></returns>
        [HttpGet]
        [Route("PrintSalesOrder")]
        public async Task<TableData> PrintSalesOrder(string DocEntry, string Indicator)
        {
            var result = new TableData();
            try
            {
                HttpHelper httpHelper = new HttpHelper(_appConfiguration.Value.ERP3Url);
                var resultApi = httpHelper.Get<Dictionary<string, string>>(new Dictionary<string, string> { { "DocEntry", DocEntry }, { "Indicator", Indicator }, { "DataType", "SaleOrder" } }, "/spv/exportsaleorder.ashx");
                if (resultApi["msg"] == "success")
                {
                    var url = resultApi["url"].Replace("192.168.0.208", "218.17.149.195");
                    result.Data = url;
                }
                else
                {
                    result.Code = 500;
                    result.Message = resultApi["msg"];
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ex.Message.ToString().ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// PDF打印（新）
        /// </summary>
        /// <param name="sboid">账套Id</param>
        /// <param name="DocEntry">单据编号</param>
        /// <returns>成功返回文件，失败抛出异常</returns>
        [HttpGet]
        [Route("OrderExportShowNew")]
        public async Task<FileResult> OrderExportShowNew(string sboid, string DocEntry)
        {
            try
            {
                return File(await _serviceSaleOrderApp.OrderExportShowNew(sboid, DocEntry), "application/pdf");
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{DocEntry}， 错误：{ex.Message}");
                throw new Exception("导出失败！" + ex.ToString());
            }
        }
        #endregion

        #region 确认是否取消
        /// <summary>
        /// 确认是否取消
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <param name="SboId"></param>
        /// <param name="OrderTitle"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet]
        [Route("GetPurchaseItemByOrderNo")]
        public async Task<Response<string>> GetPurchaseItemByOrderNo(string OrderNo, string SboId, string OrderTitle)
        {
            var result = new Response<string>();
            try
            {
                result.Result = await _serviceSaleOrderApp.GetPurchaseItemByOrderNo(OrderNo, SboId, OrderTitle);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{OrderNo}， 错误：{ex.Message}");
                throw new Exception("确认失败！" + ex.ToString());
            }
            return result;
        }
        #endregion

        #region 取消销售订单
        /// <summary>
        /// 取消销售订单
        /// </summary>
        /// <param name="DocNum"></param>
        /// <param name="SboId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet]
        [Route("UpdataSalesDoc")]
        public async Task<Response<bool>> UpdataSalesDoc(string DocNum, string SboId, string type)
        {
            var result = new Response<bool>();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            try
            {
                result.Result = _serviceSaleOrderApp.UpdataSalesDoc(DocNum, SboID, type, UserID);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{DocNum}， 错误：{ex.Message}");
                throw new Exception("取消失败！" + ex.ToString());
            }
            return result;
        }
        #endregion
        #region 关闭销售订单
        /// <summary>
        /// 关闭销售订单
        /// </summary>
        /// <param name="docNum"></param>
        /// <param name="sboId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SalesCloseFlow")]
        public async Task<Response<string>> SalesCloseFlow(string docNum, string sboId)
        {
            var result = new Response<string>();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            try
            {
                string funcId = _serviceSaleOrderApp.GetJobTypeByAddress("sales/SalesOrdrFunId.aspx");
                string jobname = "关闭销售订单";
                result.Result = _serviceSaleOrderApp.CloseDocFlow(17, int.Parse(docNum), int.Parse(funcId), jobname, UserID, "NSAP.B1Api.BOneORDRClose", sboId);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{docNum}， 错误：{ex.Message}");
                throw new Exception("关闭失败！" + ex.ToString());
            }
            return result;
        }
        #endregion

        #region 销售订单明细列表

        /// <summary>
        /// 销售订单明细列表
        /// </summary>
        [HttpPost]
        [Route("GridDataBindDetails")]
        public TableData GridDataBindDetails(SalesOrderListReq model)
        {
            int rowCount = 0;
            var result = new TableData();
            string type = "ORDR";
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            var DepID = _serviceBaseApp.GetSalesDepID(UserID);
            DataTable dt = _serviceSaleOrderApp.GetSboNamePwd(SboID);
            bool ViewFull = false;
            bool ViewSelf = true;
            bool ViewSelfDepartment = false;
            bool ViewSales = true;
            bool ViewCustom = true;
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            if (loginContext.Roles.Any(r => r.Name.Equals("销售订单管理员")))
            {
                ViewFull = true;
            }

            string dRowData = string.Empty; string isOpen = "0"; string sqlcont = string.Empty; string sboname = string.Empty;
            if (dt.Rows.Count > 0)
            {
                isOpen = dt.Rows[0][6].ToString();
                sqlcont = dt.Rows[0][5].ToString(); sboname = dt.Rows[0][0].ToString();
            }
            try
            {
                //if (isOpen == "0")
                //{

                var data = _serviceSaleOrderApp.SelectBillView(model.limit, model.page, model.query, model.sortname, model.sortorder, type, ViewFull, ViewSelf, UserID, SboID, ViewSelfDepartment, DepID, ViewCustom, ViewSales, out rowCount);
                //var progressAll = new DataTable().AsEnumerable();
                //try
                //{
                //    progressAll = _materialdesignapp.GetProgressAll().AsEnumerable();
                //}
                //catch (Exception)
                //{
                //    progressAll = new DataTable().AsEnumerable();
                //}

                //var obj = from n in data
                //          join p in progressAll
                //          on new { DocEntry = n.Field<string>("docEntry1"), itemCode = n.Field<string>("itemCode") } equals new { DocEntry = p.Field<string>("DocEntry"), itemCode = p.Field<string>("itemCode") } into temp
                //          from t in temp.DefaultIfEmpty()
                //          select new
                //          {

                //              updateDate = n.Field<DateTime?>("updateDate"),
                //              docEntry = n.Field<uint>("docEntry"),
                //              cardCode = n.Field<string>("cardCode"),
                //              cardName = n.Field<string>("CardName"),
                //              itemCode = n.Field<string>("itemCode"),
                //              dscription = n.Field<string>("dscription"),
                //              quantity = n.Field<decimal>("quantity"),
                //              price = n.Field<string>("price"),
                //              lineTotal = n.Field<string>("lineTotal"),
                //              docTotal = n.Field<string>("docTotal"),
                //              openDocTotal = n.Field<string>("openDocTotal"),
                //              createDate = n.Field<DateTime?>("createDate"),
                //              slpCode = n.Field<Int16>("slpCode"),
                //              comments = n.Field<string>("comments"),
                //              docStatus = n.Field<string>("docStatus"),
                //              printed = n.Field<string>("printed"),
                //              slpName = n.Field<string>("slpName"),
                //              docDueDate = n.Field<DateTime?>("docDueDate"),
                //              lineNum = n.Field<uint>("lineNum"),
                //              u_RelDoc = n.Field<string>("u_RelDoc"),
                //              RecordGuid = t == null ? null : t.Field<string>("RecordGuid"),
                //              progress = t == null ? 0 : t.Field<double>("progress"),
                //              DocEntry1 = n.Field<string>("docEntry1"),
                //          };


                List<SaleOrderDetailT> saleOrderDetails = data.Tolist<SaleOrderDetailT>();
                foreach (SaleOrderDetailT dataRow in saleOrderDetails)
                {
                    dataRow.DeptName = _userDepartMsgHelp.GetUserDepart(dataRow.SlpCode);
                }

                result.Count = rowCount;
                //}
                //else
                //{
                //    return NSAP.Biz.Sales.BillDelivery.SelectBillList(int.Parse(rp), int.Parse(page), query, sortname, sortorder, type, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesOrderLine.aspx").ViewFull, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesOrderLine.aspx").ViewSelf, UserID, SboID, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesOrderLine.aspx").ViewSelfDepartment, DepID, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesOrderLine.aspx").ViewCustom, NSAP.Biz.Account.Global.GetPagePowersByUrl("sales/SalesOrderLine.aspx").ViewSales, sqlcont, sboname);
                //}
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{model}， 错误：{ex.Message}");
                throw new Exception("关闭失败！" + ex.ToString());
            }

            return result;
        }

        #endregion

        #region 单据对比
        /// <summary>
        /// 单据对比
        /// </summary>
        /// <param name="orderDataDocEntryReq">单据比较实体</param>
        /// <returns>返回单据基础信息对比、单据物料明细对比信息、单据物料明细金额百分比信息</returns>
        //[HttpPost]
        //[Route("GetSaleOrderDataContent")]
        //public async Task<TableData> GetSaleOrderDataContent(OrderDataDocEntryReq orderDataDocEntryReq)
        //{
        //    var result = new TableData();
        //    try
        //    {
        //        return await _orderDraftServiceApp.GetOrderDataContent(orderDataDocEntryReq.DocEntry, orderDataDocEntryReq.DocEntrys, "SaleOrder");
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.Message;
        //        Log.Logger.Error($"地址：{Request.Path}，参数：{ex.Message.ToString().ToJson()}, 错误：{result.Message}");
        //    }

        //    return result;
        //}
        #endregion
    }
}
