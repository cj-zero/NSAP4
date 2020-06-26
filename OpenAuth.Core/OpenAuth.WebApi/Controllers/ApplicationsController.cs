using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using Org.BouncyCastle.Ocsp;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 应用管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly AppManager _app;

        public ApplicationsController(AppManager app) 
        {
            _app = app;
        }
        /// <summary>
        /// 加载应用列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public TableData Load([FromQuery]QueryAppListReq request)
        {
            var applications = _app.GetList(request);
            return new TableData
            {
                data = applications,
                count = applications.Count
            };
        }
        /// <summary>
        /// 新增或更新应用信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task AddOrUpdate([FromBody]AddOrUpdateAppReq request)
        {
            if(string.IsNullOrWhiteSpace(request.Id))
            {
                await _app.AddAsync(request);
            }
            else
            {
                await _app.UpdateAsync(request);
            }
        }

    }
}