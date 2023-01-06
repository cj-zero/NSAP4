using System;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using OpenAuth.App.Response;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.App.Problem;
using OpenAuth.App.Problem.Request;

namespace OpenAuth.WebApi.Controllers.Problem
{
    /// <summary>
    /// 问题反馈
    /// </summary>
    [Route("api/Problem/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Problem")]
    public class ProblemFeedController : ControllerBase
    {
        IAuth _auth;
        IUnitWork UnitWork;
        private ProblemFeedbackApp _problemFeedbackApp;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="UnitWork"></param>
        /// <param name="auth"></param>
        /// <param name="problemFeedbackApp"></param>
        public ProblemFeedController(ProblemFeedbackApp problemFeedbackApp, IUnitWork UnitWork, IAuth auth)
        {
            this.UnitWork = UnitWork;
            this._auth = auth;
            this._problemFeedbackApp = problemFeedbackApp;
        }

        /// <summary>
        /// 获取问题反馈列表
        /// </summary>
        /// <param name="req">问题反馈查询实体</param>
        /// <returns>返回问题反馈列表信息</returns>
        [HttpPost]
        public async Task<TableData> GetProblemFeedbacks(QueryProblemReq req)
        {
            var result = new TableData();
            try
            {
                return await _problemFeedbackApp.GetProblemFeedbacks(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="obj">问题反馈表实体</param>
        /// <returns>返回添加结果</returns>
        [HttpPost]
        public async Task<TableData> Add(ProblemFeedback obj)
        {
            var result = new TableData();
            try
            {
                result = await _problemFeedbackApp.Add(obj);
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
        /// 修改
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> Update(ProblemFeedback obj)
        {
            var result = new TableData();
            try
            {
                result = await _problemFeedbackApp.Update(obj);
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
        /// 问题反馈状态
        /// </summary>
        /// <returns>返回全部状态信息</returns>
        [HttpGet]
        public async Task<TableData> GetProblemStatusList()
        {
            var result = new TableData();
            try
            {
                return await _problemFeedbackApp.GetProblemStatusList();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 问题反馈类型
        /// </summary>
        /// <returns>返回全部类型</returns>
        [HttpGet]
        public async Task<TableData> GetProblemTypeList()
        {
            var result = new TableData();
            try
            {
                return await _problemFeedbackApp.GetProblemTypeList();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}, 错误：{result.Message}");
            }

            return result;
        }
    }
}