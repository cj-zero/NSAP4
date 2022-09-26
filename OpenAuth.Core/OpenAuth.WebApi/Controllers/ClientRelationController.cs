using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Response;
using OpenAuth.App;
using System.Threading.Tasks;
using System;
using OpenAuth.App.ClientRelation;
using OpenAuth.App.ClientRelation.Response;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 客户（中间商与终端）关系操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "General")]
    public class ClientRelationController : ControllerBase
    {

        private readonly ClientRelationApp _app;
        /// <summary>
        /// construct func
        /// </summary>
        /// <param name="app"></param>
        public ClientRelationController(ClientRelationApp app)
        {
            _app = app;
        }

        /// <summary>
        /// 获取客户关系
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<RelationGraphRsp>> GetRelations(string clientId)
        {
            var result = new Response<RelationGraphRsp>();
            try
            {
                result.Result = await _app.GetClientRelationList(clientId);
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
