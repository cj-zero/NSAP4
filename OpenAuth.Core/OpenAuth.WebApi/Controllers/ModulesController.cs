using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 模块及菜单管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "General")]
    public class ModulesController : ControllerBase
    {
        private ModuleManagerApp _app;
        private IAuth _authUtil;
        public ModulesController(IAuth authUtil, ModuleManagerApp app)
        {
            _app = app;
            _authUtil = authUtil;
        }

        /// <summary>
        /// 获取所有菜单
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Response<List<ModuleView>> LoadModuleAll()
        {
            var result = new Response<List<ModuleView>>();
            try
            {
                result.Result = _app.LoadModuleAll();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 获取菜单树
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public TableData LoadModuleForTree()
        {
            var result = new TableData();
            try
            {
                result = _app.LoadModuleForTree();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 加载角色模块
        /// </summary>
        /// <param name="firstId">The role identifier.</param>
        /// <returns>System.String.</returns>
        [HttpGet]
        public Response<IEnumerable<Module>> LoadForRole(string firstId)
        {
            var result = new Response<IEnumerable<Module>>();
            try
            {
                result.Result = _app.LoadForRole(firstId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{firstId}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 加载有菜单权限的角色
        /// </summary>
        /// <param name="moduleId">The role identifier.</param>
        /// <returns>System.String.</returns>
        [HttpGet]
        public Response<IEnumerable<Role>> LoadForRoleModule(string moduleId)
        {
            var result = new Response<IEnumerable<Role>>();
            result.Result = _app.LoadForRoleModule(moduleId);
            return result;
        }

        /// <summary>
        /// 根据某角色ID获取可访问某模块的菜单项
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Response<IEnumerable<ModuleElement>> LoadMenusForRole(string moduleId, string firstId)
        {
            var result = new Response<IEnumerable<ModuleElement>>();
            try
            {
                result.Result = _app.LoadMenusForRole(moduleId, firstId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{moduleId},{firstId}， 错误：{result.Message}");
            }

            return result;
           
        }

        /// <summary>
        /// 获取角色已经分配的字段
        /// </summary>
        /// <param name="roleId">角色id</param>
        /// <param name="moduleCode">模块代码，如Category</param>
        /// <returns></returns>
        [HttpGet]
        public Response<IEnumerable<string>> LoadPropertiesForRole(string roleId, string moduleCode)
        {
            var result = new Response<IEnumerable<string>>();
            try
            {
                result.Result = _app.LoadPropertiesForRole(roleId, moduleCode);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{roleId},{moduleCode}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取发起页面的菜单权限
        /// </summary>
        /// <returns>System.String.</returns>
        [HttpGet]
        public Response<List<ModuleElement>> LoadMenus(string moduleId)
        {
            var result = new Response<List<ModuleElement>>();
            try
            {
                var user = _authUtil.GetCurrentUser();
                if (string.IsNullOrEmpty(moduleId))
                {
                    result.Result = user.ModuleElements;
                }
                else
                {
                    var module = user.Modules.First(u => u.Id == moduleId);
                    if (module == null)
                    {
                        throw new Exception("模块不存在");
                    }
                    result.Result = module.Elements;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{moduleId}， 错误：{result.Message}");
            }

            return result;
        }


        #region 添加编辑模块

        //添加或修改
        [HttpPost]
        public Response<Module> Add(Module obj)
        {
            var result = new Response<Module>();
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
        public Response Update(Module obj)
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

        #endregion 添加编辑模块

        //添加或修改
        [HttpPost]
        public Response<ModuleElement> AddMenu(ModuleElement obj)
        {
            var result = new Response<ModuleElement>();
            try
            {
                _app.AddMenu(obj);
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
        public Response UpdateMenu(ModuleElement obj)
        {
            var result = new Response();
            try
            {
                _app.UpdateMenu(obj);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }


        [HttpPost]
        public Response DeleteMenu([FromBody]string[] ids)
        {
            var result = new Response();
            try
            {
                _app.DelMenu(ids);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ids.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        #region 菜单字段
        /// <summary>
        /// 添加菜单字段
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public Response<ModuleField> AddMenuField(ModuleField obj)
        {
            var result = new Response<ModuleField>();
            try
            {
                _app.AddMenuField(obj);
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
        /// <summary>
        /// 修改菜单字段
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public Response UpdateMenuField(ModuleField obj)
        {
            var result = new Response();
            try
            {
                _app.UpdateMenuField(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        [HttpGet]
        public Response DeleteMenuField(string id)
        {
            var result = new Response();
            try
            {
                _app.DelMenuField(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id}， 错误：{result.Message}");
            }

            return result;
        }


        /// <summary>
        /// 获取模块下字段（列表）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Response<List<ModuleField>> LoadModuleField(string moduleId, string key, string description)
        {
            var result = new Response<List<ModuleField>>();
            try
            {
                result.Result = _app.LoadModuleField(moduleId, key, description);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{moduleId},{key},{description}， 错误：{result.Message}");
            }

            return result;

        }

        /// <summary>
        /// 获取模块下字段（授权）
        /// </summary>
        /// <param name="moduleCode">模块code</param>
        /// <param name="roleId">角色名</param>
        /// <returns></returns>
        [HttpGet]
        public Response<List<KeyDescription>> GetProperties(string moduleCode, string roleId)
        {
            var result = new Response<List<KeyDescription>>();
            try
            {
                result.Result = _app.GetProperties(moduleCode, roleId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{moduleCode}， 错误：{result.Message}");
            }

            return result;

        }
        #endregion

    }
}