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
    /// 退料表接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ReturnnotesController : ControllerBase
    {
        private readonly Returnnote _app;
        
        //获取详情
        [HttpGet]
        public Response<Returnnote> Get(string id)
        {
            var result = new Response<Returnnote>();
            try
            {
                
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
        public Response Add(AddOrUpdateReturnnoteReq obj)
        {
            var result = new Response();
            try
            {
                

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
        public Response Update(AddOrUpdateReturnnoteReq obj)
        {
            var result = new Response();
            try
            {
               

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
        public TableData Load([FromQuery]QueryReturnnoteListReq request)
        {
            return null;
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
                

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        public ReturnnotesController(Returnnote app) 
        {
            _app = app;
        }
    }
}
