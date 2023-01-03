using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Response;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using Serilog;
using Infrastructure;
using OpenAuth.App.DDVoice;
using OpenAuth.App.DDVoice.EntityHelp;
using Microsoft.AspNetCore.Authorization;

namespace OpenAuth.WebApi.Controllers.DingTalk
{
    /// <summary>
    /// 钉钉设置
    /// </summary>
    [Route("api/DingTalk/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "DingTalk")]
    public class DingTalkSetController : ControllerBase
    {
        IAuth _auth;
        IUnitWork UnitWork;
        private DDVoiceApp _ddVoiceApp;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ddVoiceApp"></param>
        /// <param name="UnitWork"></param>
        /// <param name="auth"></param>
        public DingTalkSetController(DDVoiceApp ddVoiceApp, IUnitWork UnitWork, IAuth auth)
        {
            this._auth = auth;
            this.UnitWork = UnitWork;
            this._ddVoiceApp = ddVoiceApp;
        }

        /// <summary>
        /// 钉钉登录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task DDLogin()
        {
            _ddVoiceApp.DDLogin();
        }

        /// <summary>
        /// 获取钉钉所有部门id
        /// </summary>
        /// <param name="departIds"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> GetPayTermSetList(List<DDDepartMsgs> departIds)
        {
            var result = new TableData();
            try
            {
                result.Data = await _ddVoiceApp.DDDepartMsg(departIds);
                return result;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{departIds.ToJson()} 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 更新部门用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetLasterDepartUserMsg(string departId)
        {
            var result = new TableData();
            try
            {
                await _ddVoiceApp.GetDDLasterDepartUserMsg(departId);
                result.Message = "操作成功";
                return result;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 自动绑定
        /// </summary>
        /// <returns>返回绑定结果</returns>
        [HttpGet]
        public async Task<TableData> GetAutoBindUser()
        {
            var result = new TableData();
            try
            {
                return await _ddVoiceApp.GetAutoDDBindUser();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 手动绑定
        /// </summary>
        /// <param name="request">手动绑定用户实体</param>
        /// <returns>返回手动绑定结果</returns>
        [HttpPost]
        public async Task<TableData> UpdateBindUser(DDUpdateBindUserParam request)
        {
            var result = new TableData();
            try
            {
                return await _ddVoiceApp.UpdateBindUser(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()} 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 解除绑定
        /// </summary>
        /// <param name="ddUserId">钉钉用户Id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetDelBindUser(string ddUserId)
        {
            var result = new TableData();
            try
            {
                return await _ddVoiceApp.DeleteBindUser(ddUserId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ddUserId.ToJson()} 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取未绑定用户
        /// </summary>
        /// <param name="req">查询未绑定用户实体</param>
        /// <returns>返回未绑定用户信息</returns>
        [HttpPost]
        public async Task<TableData> GetNotBindUser(QueryDDUserMsg req)
        {
            var result = new TableData();
            try
            {
                return await _ddVoiceApp.GetNotBindUser(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()} 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取一级部门信息
        /// </summary>
        /// <returns>返回一级部门信息</returns>
        [HttpGet]
        public async Task<TableData> GetOneLevelDeparts()
        {
            var result = new TableData();
            try
            {
                return await _ddVoiceApp.GetOneLevelDeparts();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 钉钉推送消息
        /// <param name="msgType">消息类型</param>
        /// <param name="remarks">文本消息</param>
        /// <param name="userIds">需要发送的用户Id</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task DDSendMsg(string msgType, string remarks, string userIds)
        {
            try
            {
                 await _ddVoiceApp.DDSendMsg(msgType, remarks, userIds);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"地址：{Request.Path}， 错误：{ex.Message.ToString()}");
            }
        }
    }
}
