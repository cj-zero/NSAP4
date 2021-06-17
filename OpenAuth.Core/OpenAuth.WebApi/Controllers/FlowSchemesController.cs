using System;
using System.Collections.Generic;
using System.Linq;
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
    /// 表单操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "General")]
    public class FlowSchemesController : ControllerBase
    {
        private readonly FlowSchemeApp _app;

        [HttpGet]
        public Response<FlowScheme> Get(string id)
        {
            var result = new Response<FlowScheme>();
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

        //添加或修改
       [HttpPost]
        public Response Add(FlowScheme obj)
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

        //添加或修改
       [HttpPost]
        public Response Update(FlowScheme obj)
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
        public TableData Load([FromQuery]QueryFlowSchemeListReq request)
        {
            return _app.Load(request);
        }
        /// <summary>
        /// 获取流程设计下拉列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<List<FlowSchemeDropdownView>>> GetDropdownFlowSchemes()
        {
            var result = new Response<List<FlowSchemeDropdownView>>();
            try
            {
                var list = await _app.GetAllAsync(null);
                result.Result = list.Select(m => new FlowSchemeDropdownView { Id = m.Id, Name = m.SchemeName }).ToList();
            }
            catch (CommonException ex)
            {
                if (ex.Code == Define.INVALID_TOKEN)
                {
                    result.Code = ex.Code;
                    result.Message = ex.Message;
                    Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
                }
                else
                {
                    result.Code = 500;
                    result.Message = ex.InnerException != null
                        ? "OpenAuth.WebAPI数据库访问失败:" + ex.InnerException.Message
                        : "OpenAuth.WebAPI数据库访问失败:" + ex.Message;
                    Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
                }

            }

            return result;
        }

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

        public FlowSchemesController(FlowSchemeApp app) 
        {
            _app = app;
        }
    }
}