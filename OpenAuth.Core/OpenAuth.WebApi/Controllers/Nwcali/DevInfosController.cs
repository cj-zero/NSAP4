using System;
using System.Collections.Generic;
using System.Data;
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
    /// devinfo操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DevInfosController : ControllerBase
    {
        private readonly DevInfoApp _app;
        
        //获取详情
        [HttpGet]
        public Response<TableData> Get(long id)
        {
            var result = new Response<TableData>();
            try
            {
                result.Result = _app.GetDetails(id).Result;
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
        public Response Add(AddOrUpdateDevInfoReq obj)
        {
            var result = new Response();
            try
            {
                _app.Add(obj);

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
        public Response Update(AddOrUpdateDevInfoReq obj)
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
        public TableData Load([FromQuery] QueryDevInfoListReq request)
        {
            return _app.Load(request);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
       [HttpPost]
        public async Task<Infrastructure.Response> Delete([FromBody] List<long>ids)
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

        public DevInfosController(DevInfoApp app) 
        {
            _app = app;
        }
    }
}
