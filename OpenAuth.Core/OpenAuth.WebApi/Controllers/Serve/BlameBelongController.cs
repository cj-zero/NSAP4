using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Nwcali.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve;
using OpenAuth.App.Serve.Request;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Serve
{
    /// <summary>
    /// 责任归属
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Serve")]
    public class BlameBelongController :  ControllerBase
    {

        private readonly BlameBelongApp _app;
        public BlameBelongController(BlameBelongApp app)
        {
            _app = app;
        }


        /// <summary>
        /// 获取责任归属列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Load([FromQuery] QueryBlameBelongReq req)
        {
            return await _app.Load(req);
        }

        /// <summary>
        /// 发起申请
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Add(AddOrUpdateBlameBelongReq obj)
        {
            var result = new Response();
            try
            {
                return await _app.Add(obj);
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
        /// 获取采购单下销售单或服务单下序列号
        /// </summary>
        /// <param name="id">单号</param>
        /// <param name="type">1-采购单 2-服务单</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Infrastructure.Response<dynamic>> GetSaleOrderOrSN(int id, int type)
        {
            return await _app.GetSaleOrderOrSN(id, type);
        }

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="docentry"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetDetails(int id)
        {
            return await _app.GetDetails(id);
        }

        /// <summary>
        /// 移交
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> TransferPerson(AccraditationBlameBelongReq req)
        {
            Response result = new Response();
            try
            {
                await _app.TransferPerson(req);
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
        /// 撤回
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Revocation(AccraditationBlameBelongReq req)
        {
            Response result = new Response();
            try
            {
                return await _app.Revocation(req.Id);
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
        /// 审核
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Accraditation(AccraditationBlameBelongReq req)
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
        /// 获取责任金额
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetLiabilityAmount(int blameBelongId)
        {
            return await _app.GetLiabilityAmount(blameBelongId);
        }

        /// <summary>
        /// 单据是否已存在
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Infrastructure.Response<bool>> CheckAndeng(int id, int type)
        {
            return await _app.CheckAndeng(id, type);
        }

        /// <summary>
        /// 出纳收款
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> MakeCollection(List<AccraditationBlameBelongReq> req)
        {
            Response result = new Response();
            try
            {
                await _app.MakeCollection(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }
    }
}
