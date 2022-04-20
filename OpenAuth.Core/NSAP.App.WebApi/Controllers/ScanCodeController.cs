using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Response;
using System;
using System.Threading.Tasks;

namespace NSAP.App.WebApi.Controllers
{
    /// <summary>
    /// App扫码接口
    /// </summary>
    [Route("ErpAppApi/scanCode/[controller]/[action]")]
    [ApiController]
    public class ScanCodeController : Controller
    {
        private readonly AppScanCodeApp _appScanCodeApp;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appScanCodeApp"></param>
        public ScanCodeController(AppScanCodeApp appScanCodeApp)
        {
            _appScanCodeApp = appScanCodeApp;
        }
        /// <summary>
        /// 根据序列号获取guid集合
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetCertInfoByGuidSn(string serialNumber)
        {
            var result = new TableData();
            try
            {
                result = await _appScanCodeApp.GetGuidBySn(serialNumber); 
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 根据guid集合获取序列号集合
        /// </summary>
        /// <param name="guids"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetSnListByGuidList(string guids)
        {
            var result = new TableData();
            try
            {
                result = await _appScanCodeApp.GetSnListByGuidList(guids); 
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 根据序列号获取订单信息
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetOrderInfoBySn(string sn)
        {
            var result = new TableData();
            try
            {
                result = await _appScanCodeApp.GetOrderInfoBySn(sn);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 通过序列号查询销售交货明细
        /// </summary>
        /// <param name="manufSN">序列号</param>
        /// <param name="customer_code">客户代码</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetSalesDeliveryDetail(string manufSN, string customer_code)
        {
            var result = new TableData();
            try
            {
                result = await _appScanCodeApp.GetSalesDeliveryDetail(manufSN, customer_code);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 获取交货单对应物料的序列号列表
        /// </summary>
        /// <param name="deliveryNo">销售交货单号</param>
        /// <param name="itemCode">物料编码</param>
        /// <param name="customer_code">客户代码</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetManufSNList(int deliveryNo, string itemCode, string customer_code)
        {
            var result = new TableData();
            try
            {
                result = await _appScanCodeApp.GetManufSNList(deliveryNo, itemCode, customer_code);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 出厂序列号获取客户信息
        /// </summary>
        /// <param name="manufSN"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetCustomerInfo(string manufSN)
        {
            var result = new TableData();
            try
            {
                result = await _appScanCodeApp.GetCustomerInfo(manufSN);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 判断用户是否有服务单
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> IsExistServerOrder(int userId)
        {
            var result = new TableData();
            try
            {
                result = await _appScanCodeApp.IsExistServerOrder(userId);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
    }
}
