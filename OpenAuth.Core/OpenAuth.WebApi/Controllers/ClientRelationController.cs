using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Response;
using OpenAuth.App;
using System.Threading.Tasks;
using System;
using OpenAuth.App.ClientRelation;
using OpenAuth.App.ClientRelation.Response;
using OpenAuth.App.Request;
using OpenAuth.App.ClientRelation.Request;
using OpenAuth.Repository.Domain;
using System.Collections.Generic;

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

        /// <summary>
        /// 新增客户获取数据源
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<ClientPoolRsp>> GetRelatedClients()
        {
            var result = new Response<ClientPoolRsp>();
            try
            {
                result.Result = await _app.GetRelatedClients();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 新增客户保存草稿，提交 保存关系
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response<bool>> SaveScriptRelations([FromBody] JobScriptReq jobScript)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _app.SaveScriptRelations(jobScript);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 审核通过，同步成功后更新关系
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response<bool>> UpdateRelations([FromBody] JobReq job)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _app.UpdateRelationsAfterSync(job);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 关联变更
        /// </summary>
        /// <param name="resignReq"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response<bool>> ResignRelations([FromBody] ResignRelReq resignReq)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _app.ResignRelations(resignReq);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 获取历史记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<List<ClientRelHistory>>> GetHistory()
        {
            var result = new Response<List<ClientRelHistory>>();
            try
            {
                result.Result = await _app.GetHistory();
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
