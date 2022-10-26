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
using Microsoft.AspNetCore.Authorization;

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
        /// 业务员修改客户
        /// </summary>
        /// <param name="resignReq"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response<bool>> ResignTerminals([FromBody] ResignOper resignReq)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _app.ResignTerminals(resignReq);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 新增销售报价单是否中间商和终端关系
        /// </summary>
        /// <param name="quoteReq"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response<bool>> AddSaleQuoteRelations([FromBody] SalesQuoteReq quoteReq)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _app.AddSaleQuoteRelations(quoteReq);
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

        /// <summary>
        /// 获取终端关系
        /// </summary>
        /// <param name="clientNo"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<JobClientRelation>> GetTerminals(string clientNo, int flag)
        {
            var result = new Response<JobClientRelation>();
            try
            {
                result.Result = await _app.GetTerminals(clientNo,flag);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 手动同步关系
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<bool> GetSyncRelations()
        {
            var result = true;
            try
            {
                result = await _app.SyncRelations();
            }
            catch (Exception ex)
            {

            }

            return result;
        }


    }
}
