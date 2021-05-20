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
    /// usersign操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserSignsController : ControllerBase
    {
        private readonly UserSignApp _app;
        
        //添加
       [HttpPost]
        public Response Add(AddOrUpdateUserSignReq obj)
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        //修改
       [HttpPost]
        public async Task<Response> Update(AddOrUpdateUserSignReq obj)
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public async Task<TableData<List<UserSign>>> Load([FromQuery]QueryUserSignListReq request)
        {
            return await _app.Load(request);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
       [HttpPost]
        public async Task<Response> Delete([FromBody]List<int> ids)
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{ids.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        public UserSignsController(UserSignApp app) 
        {
            _app = app;
        }
    }
}
