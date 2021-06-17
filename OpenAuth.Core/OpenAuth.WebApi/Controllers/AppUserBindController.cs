using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// App绑定用户
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "General")]
    public class AppUserBindController : Controller
    {
        private readonly AppUserBindApp _app;
        private readonly HttpClienService _httpClienService;

        public AppUserBindController(AppUserBindApp app, HttpClienService httpClienService)
        {
            _app = app;
            _httpClienService = httpClienService;
        }


        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public TableData Load([FromQuery] QueryAppUserBindListReq request)
        {
            return _app.Load(request);
        }

        #region  #region App售后接口 如无特殊情况勿动，修改请告知！！！
        /// <summary>
        /// 添加绑定记录
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AddOrUpdateAppUserBind(AddOrUpdateAppUserBindReq obj)
        {
            var result = new Response();
            try
            {
                var r = await _httpClienService.Post(obj, "api/user/UserManage/AddOrUpdateAppUserBind");
                result = JsonConvert.DeserializeObject<Response>(r);
                //await _app.Add(obj);

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
        /// 获取绑定结果
        /// </summary>
        [HttpGet]
        public async Task<TableData> GetBindInfo(int AppUserId)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("AppUserId", AppUserId);
                var r = await _httpClienService.Get(parameters, "api/user/UserManage/GetBindInfo");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _app.GetBindInfo(AppUserId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{AppUserId}， 错误：{result.Message}");
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 审核客户绑定申请
        /// </summary>
        /// <param name="Id">申请Id</param>
        /// <param name="Type">审核操作 1审核成功 2审核失败</param>
        /// <param name="Reason">审核失败原因</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AuditApply(string Id, int Type, string Reason)
        {
            var result = new Response();
            try
            {
                await _app.AuditApply(Id, Type, Reason);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{Id},{Type},{Reason}， 错误：{result.Message}");
            }

            return result;
        }
    }
}
