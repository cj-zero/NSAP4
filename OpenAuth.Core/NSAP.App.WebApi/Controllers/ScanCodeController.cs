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
    [Route("api/scanCode/[controller]/[action]")]
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
        /// 扫描出厂码获取guid集合
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetCertInfoByGuidSn(string serialNumber)
        {
            var result = new TableData();
            try
            {
                if (serialNumber.ToLower().Contains("zwj"))
                {
                  result = await _appScanCodeApp.GetGuidBySn(serialNumber);
                }
                else
                {
                    result = await _appScanCodeApp.GetCertInfoBySn(serialNumber);
                }
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
