using System;
using System.Collections.Generic;
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
    public class RolesController : ControllerBase
    {
        private readonly RoleApp _app;
        private readonly RevelanceManagerApp _revelanceManagerApp;
        public RolesController(RevelanceManagerApp revelanceManagerApp, RoleApp app) 
        {
            _revelanceManagerApp = revelanceManagerApp;
            _app = app;
        }

        [HttpGet]
        public Response<Role> Get(string id)
        {
            var result = new Response<Role>();
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
        public Response<RoleView> Add(RoleView obj)
        {
            var result = new Response<RoleView>();
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
        public Response Update(RoleView obj)
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
        public Response<List<Role>> Load([FromQuery]QueryRoleListReq request)
        {
            var result = new Response<List<Role>>();
            try
            {
                result.Result = _app.Load(request);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 加载用户的角色
        /// </summary>
        [HttpGet]
        public Response<List<string>> LoadForUser(string userId)
        {
            var result = new Response<List<string>>();
            try
            {
                result.Result = _revelanceManagerApp.Get(Define.USERROLE, true, userId);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.InnerException?.Message ?? e.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{userId}， 错误：{result.Message}");
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
                foreach (var id in ids)
                {
                    _revelanceManagerApp.UnAssign(new AssignReq()
                    {
                        type = Define.ROLEDATAPROPERTY,
                        firstId = id
                    });
                    _revelanceManagerApp.UnAssign(new AssignReq()
                    {
                        type = Define.ROLEELEMENT,
                        firstId = id
                    });
                    _revelanceManagerApp.UnAssign(new AssignReq()
                    {
                        type = Define.ROLEMODULE,
                        firstId = id
                    });
                    _revelanceManagerApp.UnAssign(new AssignReq()
                    {
                        type = Define.ROLERESOURCE,
                        firstId = id
                    });
                }
                _revelanceManagerApp.UnAssign(new AssignReq()
                {
                    type = Define.USERROLE,
                    secIds = ids
                });


            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ids.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
    }
}