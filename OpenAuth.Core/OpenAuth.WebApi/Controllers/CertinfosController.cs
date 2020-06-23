using System;
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
        public TableData Load([FromQuery]QueryCertinfoListReq request)
        {
            return _app.Load(request);
        }


        public CertinfosController(CertinfoApp app) 
        {
            _app = app;
        }
    }
}
