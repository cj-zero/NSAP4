using System;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// completionreport操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CompletionReportsController : ControllerBase
    {
        private readonly CompletionReportApp _app;
        
        //获取详情
        [HttpGet]
        public Response<CompletionReport> Get(string id)
        {
            var result = new Response<CompletionReport>();
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

        //添加
       [HttpPost]
        public async Task<Response> Add(AddOrUpdateCompletionReportReq obj)
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
        public Response Update(AddOrUpdateCompletionReportReq obj)
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
        public TableData Load([FromQuery]QueryCompletionReportListReq request)
        {
            return _app.Load(request);
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

        [HttpGet]
        public async Task<Response<CompletionReportDetailsResp>> GetOrderWorkInfoForAdd(int ServiceWorkOrderId)
        {
            var result = new Response<CompletionReportDetailsResp>();
            try
            {
                result.Result = await _app.GetOrderWorkInfoForAdd(ServiceWorkOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        public CompletionReportsController(CompletionReportApp app) 
        {
            _app = app;
        }
    }
}
