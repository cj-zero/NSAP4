using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Response;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using Serilog;
using OpenAuth.App.DDVoice;
using OpenAuth.App.DDVoice.EntityHelp;

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
        /// 获取钉钉所有部门id
        /// </summary>
        /// <param name="departIds"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> GetPayTermSetList(List<DDDepartMsg> departIds)
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
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 更新部门用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetLasterDepartUserMsg()
        {
            var result = new TableData();
            try
            {
                await _ddVoiceApp.GetDDLasterDepartUserMsg();
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
                result.Message = await _ddVoiceApp.GetAutoDDBindUser();
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
    }
}
