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
    [Route("api/order/[controller]")]
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
        public async Task<Response<string>> SalesDeliverySave(SalesOrderSaveReq salesOrderSaveReq)
        {
            var result = new Response<string>();
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            try
            {
                string funcId = "0"; string logstring = ""; string jobname = "";

                funcId = _serviceSaleOrderApp.GetJobTypeByAddress("sales/SalesDelivery.aspx");
                logstring = "根据销售订单下销售交货单"; jobname = "销售交货";

                //break;
                result.Result = await _salesDeliveryApp.SalesDeliverySave(salesOrderSaveReq, UserID, int.Parse(funcId), "0", jobname, SboID, "0");

                return result;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{salesOrderSaveReq}， 错误：{ex.Message}");
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
            var TableData=new TableData();
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
                    TableData.Data= _serviceSaleOrderApp.SelectBillViewInfo(out  rowCount,salesDeliveryListReq.limit, salesDeliveryListReq.page, salesDeliveryListReq.qtype, salesDeliveryListReq.sortname, salesDeliveryListReq.sortorder, type, true, true, UserID, SboID, true, DepID, true, true);
                    TableData.Count = rowCount;
                }
                else
                {
                    //默认接口
                    TableData.Data= _serviceSaleOrderApp.SelectBillListInfo(out rowCount, salesDeliveryListReq.limit, salesDeliveryListReq.page, salesDeliveryListReq.query, salesDeliveryListReq.sortname, salesDeliveryListReq.sortorder, type, true, true, UserID, SboID, true, DepID, true, true, sqlcont, sboname);
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
    }
}
