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
    /// 服务解决方案管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Serve")]
    public class SolutionsController : ControllerBase
    {
        private readonly SolutionApp _app;
        
        //获取详情
        [HttpGet]
        public Response<Solution> Get(string id)
        {
            var result = new Response<Solution>();
            try
            {
                result.Result = _app.Get(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }
        /// <summary> 
        /// 添加客服解决方案
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Add(AddOrUpdateSolutionReq obj)
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
        /// <summary>
        /// 添加技术员解决方案
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> TechnicianAdd(AddOrUpdateSolutionReq obj)
        {
            var result = new Response();
            try
            {
                await _app.TechnicianAdd(obj);

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
        public Response Update(AddOrUpdateSolutionReq obj)
        {
            var result = new Response();
            try
            {
                _app.Update(obj);

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
        public async Task<TableData> Load([FromQuery]QuerySolutionListReq request)
        {
            return await _app.Load(request);
        }
        /// <summary>
        /// 加载技术员解决方案列表
        /// </summary>
        [HttpGet]
        public async Task<TableData> TechnicianLoad([FromQuery]QuerySolutionListReq request)
        {
            return await _app.TechnicianLoad(request);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
       [HttpPost]
        public Response Delete([FromBody]string[] ids)
        {
            var result = new Response();
            try
            {
                _app.Delete(ids);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        public SolutionsController(SolutionApp app) 
        {
            _app = app;
        }
    }
}
