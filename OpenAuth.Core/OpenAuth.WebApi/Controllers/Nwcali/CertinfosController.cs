using System;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 校准证书信息操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CertinfosController : ControllerBase
    {
        private readonly CertinfoApp _app;

        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public async Task<TableData> Load([FromQuery]QueryCertinfoListReq request)
        {
            return await _app.LoadAsync(request);
        }

        /// <summary>
        /// 证书待审批流程查询列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> LoadApprover(QueryCertApproverListReq req)
        {
            return await _app.LoadApprover(req);
        }

        /// <summary>
        /// 证书审批操作
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> CertVerification(CertVerificationReq req)
        {
            var result = new Response();
            try
            {
                await _app.CertVerification(req);
            }
            catch(Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        public CertinfosController(CertinfoApp app) 
        {
            _app = app;
        }
    }
}
