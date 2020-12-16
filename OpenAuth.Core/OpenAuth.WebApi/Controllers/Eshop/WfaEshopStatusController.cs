using System;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// wfa_eshop_status操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WfaEshopStatusController : Controller
    {
        private readonly WfaEshopStatusApp _wfastatusapp;

        public WfaEshopStatusController(WfaEshopStatusApp app)
        {
            _wfastatusapp = app;
        }

        /// <summary>
        /// 根据商城注册电话查对应订单状态列表
        /// </summary>
        /// <param name="MobileNo"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<TableData> GetOrderStatusByRegMobile([FromQuery] QryWfaEshopStatusListReq req)
        {
            var result = new TableData();
            try
            {
                result.Data =await  _wfastatusapp.GetOrderStatusByRegMobile(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 根据主键获取进度明细信息
        /// </summary>
        /// <param name="DocumentId"></param>
        /// <returns></returns>
        [HttpGet]
         public async Task<Response<WfaEshopStatusResp>> GetStatusInfoById(int DocumentId)
        {
            var result = new Response<WfaEshopStatusResp>();
            try
            {
                result.Result = await _wfastatusapp.GetStatusInfoById(DocumentId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

    }
}
