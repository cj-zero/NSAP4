using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// certoperationhistory操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CertOperationHistoryController : ControllerBase
    {
        private readonly CertOperationHistoryApp _app;

        //获取详情
        //[HttpGet]
        //public Response<CertOperationHistory> Get(string id)
        //{
        //    var result = new Response<CertOperationHistory>();
        //    try
        //    {
        //        result.Result = _app.Get(id);
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //    }

        //    return result;
        //}

        //添加
        //[HttpPost]
        // public Response Add(AddOrUpdateCertOperationHistoryReq obj)
        // {
        //     var result = new Response();
        //     try
        //     {
        //         _app.Add(obj);

        //     }
        //     catch (Exception ex)
        //     {
        //         result.Code = 500;
        //         result.Message = ex.InnerException?.Message ?? ex.Message;
        //     }

        //     return result;
        // }

        //修改
        //[HttpPost]
        // public Response Update(AddOrUpdateCertOperationHistoryReq obj)
        // {
        //     var result = new Response();
        //     try
        //     {
        //         _app.Update(obj);

        //     }
        //     catch (Exception ex)
        //     {
        //         result.Code = 500;
        //         result.Message = ex.InnerException?.Message ?? ex.Message;
        //     }

        //     return result;
        // }

        /// <summary>
        /// 加载列表
        /// </summary>
        //[HttpGet]
        //public TableData Load([FromQuery]QueryCertOperationHistoryListReq request)
        //{
        //    return _app.Load(request);
        //}

        /// <summary>
        /// 获取证书操作记录列表
        /// </summary>
        [HttpGet]
        public async Task<Response<List<CertOperationHistory>>> GetCertOperationHistory(string id)
        {
            var result = new Response<List<CertOperationHistory>>();
            try
            {
                result.Result = await _app.GetCertOperationHistory(id);
            }catch(Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id}， 错误：{ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// 批量删除
        /// </summary>
       //[HttpPost]
       // public Response Delete([FromBody]string[] ids)
       // {
       //     var result = new Response();
       //     try
       //     {
       //         _app.Delete(ids);

       //     }
       //     catch (Exception ex)
       //     {
       //         result.Code = 500;
       //         result.Message = ex.InnerException?.Message ?? ex.Message;
       //     }

       //     return result;
       // }

        public CertOperationHistoryController(CertOperationHistoryApp app) 
        {
            _app = app;
        }
    }
}
