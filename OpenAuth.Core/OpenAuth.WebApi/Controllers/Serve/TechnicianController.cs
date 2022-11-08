using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Response;
using OpenAuth.App.Serve;
using OpenAuth.App.Serve.Request;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Serve
{
    /// <summary>
    /// 技术员
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Serve")]
    public class TechnicianController : ControllerBase
    {
        private readonly TechnicianApp _technicianApp;
        public TechnicianController(TechnicianApp technicianApp)
        {
            _technicianApp = technicianApp;

        }

        /// <summary>
        /// 获取技术员列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetTechnicianList([FromQuery]QueryTechnicianListReq req)
        {
            return await _technicianApp.GetTechnicianList(req);
        }
        /// <summary>
        /// 获取技术员服务单信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetTechnicianOrder(string Id)
        {
            return await _technicianApp.GetTechnicianOrder(Id);
        }





        
    }
}
