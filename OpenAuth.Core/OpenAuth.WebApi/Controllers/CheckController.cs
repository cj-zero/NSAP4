// ***********************************************************************
// Assembly         : OpenAuth.WebApi
// Author           : yubaolee
// Created          : 07-11-2016
//
// Last Modified By : yubaolee
// Last Modified On : 07-11-2016
// Contact :
// File: CheckController.cs
// 登录相关的操作
// ***********************************************************************

using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.App.SSO;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using OpenAuth.App.Request;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// 登录及与登录信息获取相关的接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "General")]
    public class CheckController : ControllerBase
    {
        private readonly IAuth _authUtil;
        //private ILogger _logger;
        private AuthStrategyContext _authStrategyContext;
        private CorpApp _corpApp;
        private RevelanceManagerApp _revelanceManagerApp;
        public CheckController(IAuth authUtil, ILogger<CheckController> logger, CorpApp corpApp, RevelanceManagerApp revelanceManagerApp)
        {
            _authUtil = authUtil;
           // _logger = logger;
            _authStrategyContext = _authUtil.GetCurrentUser();
            _corpApp = corpApp;
            _revelanceManagerApp = revelanceManagerApp;
        }
        
        /// <summary>
        /// 获取登录用户资料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Response<UserView> GetUserProfile()
        {
               var resp = new Response<UserView>();
            try
            {
                resp.Result = _authStrategyContext.User.MapTo<UserView>();
            }
            catch (Exception e)
            {
                resp.Code = 500;
                resp.Message = e.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{resp.Message}");
            }

            return resp;
        }

        /// <summary>
        /// 检验token是否有效
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="requestid">备用参数.</param>
        [HttpGet]
       
        public Response<bool> GetStatus()
        {
            var result = new Response<bool>();
            try
            {
                result.Result = _authUtil.CheckLogin();
            }
            catch (Exception ex)
            {
                result.Code = Define.INVALID_TOKEN;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 获取登录用户的所有可访问的角色
        /// </summary>
        [HttpGet]
        public Response<List<Role>> GetRoles()
        {
            var result = new Response<List<Role>>();
            try
            {
                result.Result = _authStrategyContext.Roles;
            }
            catch (CommonException ex)
            {
                if (ex.Code == Define.INVALID_TOKEN)
                {
                    result.Code = ex.Code;
                    result.Message = ex.Message;
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

        /// <summary>
        /// 获取当前登录用户可访问的字段
        /// </summary>
        /// <param name="moduleCode">模块的Code，如Category</param>
        /// <returns></returns>
        [HttpGet]
        public Response<List<KeyDescription>> GetProperties(string moduleCode)
        {
            var result = new Response<List<KeyDescription>>();
            try
            {
                result.Result = _authStrategyContext.GetProperties(moduleCode);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{moduleCode}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 获取公司列表
        /// </summary>
        [HttpGet]
        public Response<List<Corp>> GetCorp()
        {
            var result = new Response<List<Corp>>();
            try
            {
                result.Result = _corpApp.GetAll(null);
            }
            catch (CommonException ex)
            {
                if (ex.Code == Define.INVALID_TOKEN)
                {
                    result.Code = ex.Code;
                    result.Message = ex.Message;
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
        /// <summary>
        /// 获取登录用户的所有可访问的组织信息
        /// </summary>
        [HttpGet]
        public Response<List<OpenAuth.Repository.Domain.Org>> GetOrgs(string corpId)
        {
            var result = new Response<List<OpenAuth.Repository.Domain.Org>>();
            try
            {
                if (!string.IsNullOrWhiteSpace(corpId))
                    result.Result = _authStrategyContext.Orgs.Where(org => org.CorpId.Equals(corpId)).ToList();
                else
                    result.Result = _authStrategyContext.Orgs;
            }
            catch (CommonException ex)
            {
                if (ex.Code == Define.INVALID_TOKEN)
                {
                    result.Code = ex.Code;
                    result.Message = ex.Message;
                    Log.Logger.Error($"地址：{Request.Path}，参数：{corpId}， 错误：{result.Message}");
                }
                else
                {
                    result.Code = 500;
                    result.Message = ex.InnerException != null
                        ? "OpenAuth.WebAPI数据库访问失败:" + ex.InnerException.Message
                        : "OpenAuth.WebAPI数据库访问失败:" + ex.Message;
                    Log.Logger.Error($"地址：{Request.Path}，参数：{corpId}， 错误：{result.Message}");
                }

            }

            return result;
        }



        /// <summary>
        /// 加载机构的全部下级机构
        /// </summary>
        /// <param name="orgId">机构ID</param>
        /// <returns></returns>
        [HttpGet]
        public TableData GetSubOrgs(string orgId)
        {
            string cascadeId = ".0.";
            if (!string.IsNullOrEmpty(orgId))
            {
                var org = _authStrategyContext.Orgs.SingleOrDefault(u => u.Id == orgId);
                if (org == null)
                {
                    return new TableData
                    {
                        Message = "未找到指定的节点",
                        Code = 500,
                    };
                }
                cascadeId = org.CascadeId;
            }

            var deptManage = _revelanceManagerApp.GetDeptManager();

            var query = from a in _authStrategyContext.Orgs
                        join b in deptManage on a.Id equals b.OrgId into ab
                        from b in ab.DefaultIfEmpty()
                        where a.CascadeId.Contains(cascadeId)
                        orderby a.CascadeId
                        select new
                        {
                            a.BizCode,
                            a.CascadeId,
                            a.CorpId,
                            a.CreateId,
                            a.CreateTime,
                            a.CustomCode,
                            a.HotKey,
                            a.IconName,
                            a.Id,
                            a.IsAutoExpand,
                            a.IsLeaf,
                            a.Name,
                            a.ParentId,
                            a.ParentName,
                            a.SortNo,
                            a.Status,
                            a.TypeId,
                            a.TypeName,
                            UserId = b == null ? new string[] { } : b.UserId,
                            UserName = b == null ? new string[] { } : b.UserName
                        };
            //var query = _authStrategyContext.Orgs
            //    .Where(u => u.CascadeId.Contains(cascadeId))
            //    .OrderBy(u => u.CascadeId);

            return new TableData
            {
                Data = query.ToList(),
                Count = query.Count(),
            };
        }

        /// <summary>
        /// 获取登录用户的所有可访问的模块及菜单，以列表形式返回结果
        /// </summary>
        [HttpGet]
        public Response<List<ModuleView>> GetModules()
        {
            var result = new Response<List<ModuleView>>();
            try
            {
                result.Result = _authStrategyContext.Modules;
            }
            catch (CommonException ex)
            {
                if (ex.Code == Define.INVALID_TOKEN)
                {
                    result.Code = ex.Code;
                    result.Message = ex.Message;
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

        /// <summary>
        /// 获取模块下拉框信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Response<List<ModuleDropdownView>> GetDropdownModules()
        {
            var result = new Response<List<ModuleDropdownView>>();
            try
            {
                result.Result = _authStrategyContext.Modules.Select(m => new ModuleDropdownView { Id = m.Id, Name = m.Name }).ToList();
            }
            catch (CommonException ex)
            {
                if (ex.Code == Define.INVALID_TOKEN)
                {
                    result.Code = ex.Code;
                    result.Message = ex.Message;
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

        /// <summary>
        /// 获取登录用户的所有可访问的模块及菜单，以树状结构返回
        /// </summary>
        [HttpGet]
        public Response<IEnumerable<TreeItem<ModuleView>>> GetModulesTree()
        {
            var result = new Response<IEnumerable<TreeItem<ModuleView>>>();
            try
            {
                result.Result = _authStrategyContext.Modules.GenerateTree(u => u.Id, u => u.ParentId);
            }
            catch (CommonException ex)
            {
                if (ex.Code == Define.INVALID_TOKEN)
                {
                    result.Code = ex.Code;
                    result.Message = ex.Message;
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

        /// <summary>
        /// 获取登录用户的所有可访问的资源
        /// </summary>
        [HttpGet]
        public Response<List<Resource>> GetResources()
        {
            var result = new Response<List<Resource>>();
            try
            {
                result.Result = _authStrategyContext.Resources;
            }
            catch (CommonException ex)
            {
                if (ex.Code == Define.INVALID_TOKEN)
                {
                    result.Code = ex.Code;
                    result.Message = ex.Message;
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

        /// <summary>
        /// 根据token获取用户名称
        /// </summary>
        [HttpGet]
        public Response<string> GetUserName()
        {
            var result = new Response<string>();
            try
            {
                result.Result = _authStrategyContext.User.Name;
            }
            catch (CommonException ex)
            {
                if (ex.Code == Define.INVALID_TOKEN)
                {
                    result.Code = ex.Code;
                    result.Message = ex.Message;
                }
                else
                {
                    result.Code = 500;
                    result.Message = ex.InnerException != null
                        ?  ex.InnerException.Message :  ex.Message;
                    Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
                }

            }

            return result;
        }
        
        /// <summary>
        /// 登录接口
        /// </summary>
        /// <param name="request">登录参数</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public LoginResult Login([FromBody]PassportLoginRequest request)
        {
            var result = new LoginResult();
            try
            {
                result = _authUtil.Login(request.AppKey, request.Account, request.Password);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 注销登录
        /// </summary>
        /// <param name="token"></param>
        /// <param name="requestid">备用参数.</param>
        [HttpPost]
        public Response<bool> Logout()
        {
            var resp = new Response<bool>();
            try
            {
                resp.Result = _authUtil.Logout();
            }
            catch (Exception e)
            {
                resp.Result = false;
                resp.Message = e.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{resp.Message}");
            }

            return resp;
        }
    }
}