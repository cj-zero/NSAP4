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
    /// 服务评价操作
    /// </summary>
    [Route("api/Serve/[controller]/[action]")]
    [ApiController]
    public class ServiceEvaluatesController : ControllerBase
    {
        private readonly ServiceEvaluateApp _app;
        
        //获取详情
        [HttpGet]
        public async Task<Response<ServiceEvaluate>> Get(long id)
        {
            var result = new Response<ServiceEvaluate>();
            try
            {
                result.Result = await _app.Get(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        //添加
       [HttpPost]
        public async Task<Response> Add(AddOrUpdateServiceEvaluateReq obj)
        {
            var result = new Response();
            try
            {
                await _app.Add(obj);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        //修改
       [HttpPost]
        public async Task<Response> Update(AddOrUpdateServiceEvaluateReq obj)
        {
            var result = new Response();
            try
            {
                await _app.Update(obj);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public TableData Load([FromQuery]QueryServiceEvaluateListReq request)
        {
            return _app.Load(request);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
       [HttpPost]
        public async Task<Response> Delete([FromBody]long[] ids)
        {
            var result = new Response();
            try
            {
                await _app.Delete(ids);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        public ServiceEvaluatesController(ServiceEvaluateApp app) 
        {
            _app = app;
        }
    }
}
