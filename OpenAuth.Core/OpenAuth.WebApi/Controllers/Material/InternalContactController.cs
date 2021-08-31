using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Material;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Response;
using OpenAuth.WebApi.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Material
{
    /// <summary>
    /// 内部联络单
    /// </summary>
    [Route("api/Material/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName ="Material")]
    public class InternalContactController : ControllerBase
    {
        private InternalContactApp _app;
        public InternalContactController(InternalContactApp internalContactApp)
        {
            _app = internalContactApp;
        }

        /// <summary>
        /// 获取联络单列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Load([FromQuery] QueryInternalContactReq req)
        {
            return await _app.Load(req);
        }

        /// <summary>
        /// 添加联络单
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Add(AddOrUpdateInternalContactReq obj)
        {
            var result = new Response();
            try
            {
                await _app.Add(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetDetails(int id)
        {
            return await _app.GetDetails(id);
        }

        /// <summary>
        /// 审批、执行
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Accraditation(AccraditationInternalContactReq req)
        {
            Response result = new Response();
            try
            {
                await _app.Accraditation(req);
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
        /// 撤销\过期\停用\启用
        /// </summary>
        /// <param name="internalContactId"></param>
        /// <param name="isRevoke"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response> Revocation(int internalContactId, int handleType)
        {
            Response result = new Response();
            try
            {
                await _app.Revocation(internalContactId, handleType);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{internalContactId},{handleType}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="sign"></param>
        /// <param name="timespan"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(CertAuthFilter))]
        [HttpGet]
        public async Task<IActionResult> PrintInternalContact(string serialNumber, string sign, string timespan)
        {
            try
            {
                return File(await _app.PrintInternalContact(serialNumber), "application/pdf");
            }
            catch (Exception e)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{serialNumber},{sign},{timespan}, 错误：{e.Message}");
                throw new Exception(e.Message);
            }
        }
    }
}
