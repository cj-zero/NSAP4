using System;
using System.Collections.Generic;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.Repository.Domain;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 机构操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "General")]
    public class OrgsController : ControllerBase
    {
        private readonly OrgManagerApp _app;

        [HttpGet]
        public Response<OpenAuth.Repository.Domain.Org> Get(string id)
        {
            var result = new Response<OpenAuth.Repository.Domain.Org>();
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

        [HttpGet]
        public Response<List<OpenAuth.Repository.Domain.Org>> GetAllOrg()
        {
            var result = new Response<List<OpenAuth.Repository.Domain.Org>>();
            try
            {
                result.Result = _app.GetAll(null);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }

            return result;
        }
        //添加或修改
        [HttpPost]
        public Response<OpenAuth.App.Request.AddOrUpdateOrgReq> Add(OpenAuth.App.Request.AddOrUpdateOrgReq obj)
        {
            var result = new Response<OpenAuth.App.Request.AddOrUpdateOrgReq>();
            try
            {
                _app.Add(obj);
                result.Result = obj;
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
        public Response Update(OpenAuth.App.Request.AddOrUpdateOrgReq obj)
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
        /// 获取部门树和用户
        /// </summary>
        /// <param name="corpId"></param>
        /// <returns></returns>
        [HttpGet]
        public async System.Threading.Tasks.Task<App.Response.TableData> GetOrgTreeAndUser()
        {
            App.Response.TableData result = new App.Response.TableData();
            result = await _app.GetOrgTreeAndUser();
            return result;
        }


        /// <summary>
        /// 删除选中的部门及所有的子部门
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        public Response Delete([FromBody]string[] ids)
        {
            var result = new Response();
            try
            {
                _app.DelOrgCascade(ids);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ids.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        public OrgsController(OrgManagerApp app) 
        {
            _app = app;
        }
    }
}