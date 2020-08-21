using System;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;
using OpenAuth.Repository.Domain;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 技术员提交设备操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SeviceTechnicianApplyOrdersController : ControllerBase
    {
        private readonly SeviceTechnicianApplyOrdersApp _app;

        /// <summary>
        /// 提交核对错误(新)设备信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> ApplyNewOrErrorDevices(ApplyNewOrErrorDevicesReq request)
        {
            var result = new Response();
            try
            {
                await _app.ApplyNewOrErrorDevices(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        ///获取技术员提交/修改的设备信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetApplyDevices([FromQuery] GetApplyDevicesReq request)
        {
            var result = new TableData();
            try
            {
                result = await _app.GetApplyDevices(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        public SeviceTechnicianApplyOrdersController(SeviceTechnicianApplyOrdersApp app)
        {
            _app = app;
        }
    }
}
