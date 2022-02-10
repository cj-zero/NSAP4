using System;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Reponse;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 售前需求对接申请流程
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Serve")]
    public class BeforeSaleDemandsController : ControllerBase
    {
        private readonly BeforeSaleDemandApp _beforeSaleDemandApp;
        
        /// <summary>
        /// 加载列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Load([FromQuery] QueryBeforeSaleDemandListReq req)
        {
            return await _beforeSaleDemandApp.Load(req);
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<BeforeSaleDemandResp>> GetDetails(int id)
        {
            var result = new Response<BeforeSaleDemandResp>();
            try
            {
                result.Result = await _beforeSaleDemandApp.GetDetails(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Add(AddOrUpdateBeforeSaleDemandReq obj)
        {
            var result = new Response();
            try
            {
                await _beforeSaleDemandApp.Add(obj);
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
        /// 修改
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public Response Update(AddOrUpdateBeforeSaleDemandReq obj)
        {
            var result = new Response();
            try
            {
                _beforeSaleDemandApp.Update(obj);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }


        /// <summary>
        /// 审批售前需求申请流程
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Accraditation(AccraditationBeforeSaleDemandReq req)
        {
            var result = new Response();
            try
            {
                await _beforeSaleDemandApp.Accraditation(req);
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
        /// 获取售前需求申请流程操作记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetBeforeSaleDemandOperationHistory([FromQuery] QueryBeforeSaleDemandOperationHistoryListReq req)
        {
            return await _beforeSaleDemandApp.GetBeforeSaleDemandOperationHistory(req);
        }


        /// <summary>
        /// 批量删除
        /// </summary>
        [HttpPost]
        public Response Delete([FromBody]string[] ids)
        {
            var result = new Response();
            try
            {
                //_app.Delete(ids);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        public BeforeSaleDemandsController(BeforeSaleDemandApp app) 
        {
            _beforeSaleDemandApp = app;
        }
    }
}
