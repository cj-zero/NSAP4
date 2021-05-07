using System;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using System.Threading.Tasks;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// appusermap操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AppUserMapController : ControllerBase
    {
        private readonly AppUserMapApp _app;
        
        //获取详情
        [HttpGet]
        public Response<AppUserMap> Get(string id)
        {
            var result = new Response<AppUserMap>();
            try
            {
                result.Result = _app.Get(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id}， 错误：{result.Message}");
            }

            return result;
        }

        //添加
       [HttpPost]
        public Response Add(AddOrUpdateAppUserMapReq obj)
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        //修改
       [HttpPost]
        public Response Update(AddOrUpdateAppUserMapReq obj)
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public TableData Load([FromQuery]QueryAppUserMapListReq request)
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{ids.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        public AppUserMapController(AppUserMapApp app) 
        {
            _app = app;
        }

        [HttpGet] 
        public async Task<Response<UserView>> GetFirstNsapUser(int appUserId)
        {
            var result = new Response<UserView>();
            try
            {
                result.Result = await _app.GetFirstNsapUser(appUserId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{appUserId}， 错误：{result.Message}");
            }
            return result;
        }
    }
}
