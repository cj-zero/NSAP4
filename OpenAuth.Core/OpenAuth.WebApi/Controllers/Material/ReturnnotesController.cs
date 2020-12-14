using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 退料表接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ReturnNotesController : ControllerBase
    {
        private readonly ReturnNoteApp _returnnoteApp;

        /// <summary>
        /// 退料
        /// </summary>
        /// <param name="returnMaterialReq"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> ReturnMaterials(ReturnMaterialReq returnMaterialReq)
        {
            var result = new Response();
            try
            {
                await _returnnoteApp.ReturnMaterials(returnMaterialReq);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 获取退料详情
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <param name="appUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetReturnNoteInfo(int serviceOrderId, int appUserId)
        {
            var result = new TableData();
            try
            {
                result = await _returnnoteApp.GetReturnNoteInfo(appUserId, serviceOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 获取物流详情
        /// </summary>
        /// <param name="expressageId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetExpressageInfo(string expressageId)
        {
            var result = new TableData();
            try
            {
                result = await _returnnoteApp.GetExpressageInfo(expressageId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 保存仓库验收记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> SaveReceiveInfo(ReturnNoteAuditReq req)
        {
            var result = new Response();
            try
            {
                await _returnnoteApp.SaveReceiveInfo(req);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 获取退料列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetReturnNoteList([FromQuery] GetReturnNoteListReq req)
        {
            var result = new TableData();
            try
            {
                result = await _returnnoteApp.GetReturnNoteList(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 获取退料详情（nsap）
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetReturnNoteDetail(int Id)
        {
            var result = new TableData();
            try
            {
                result = await _returnnoteApp.GetReturnNoteDetail(Id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 验收
        /// </summary>
        /// <param name="returnMaterialReq"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Accraditation(ReturnNoteAuditReq req)
        {
            var result = new Response();
            try
            {
                await _returnnoteApp.Accraditation(req);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 获取退料结算列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetClearReturnNoteList([FromQuery] GetClearReturnNoteListReq req)
        {
            var result = new TableData();
            try
            {
                result = await _returnnoteApp.GetClearReturnNoteList(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 获取退料结算详情
        /// </summary>
        /// <param name="serviceSapId"></param>
        /// <param name="createrId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetClearReturnNoteDetail(int serviceSapId, string createrId)
        {
            var result = new TableData();
            try
            {
                result = await _returnnoteApp.GetClearReturnNoteDetail(serviceSapId, createrId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        public ReturnNotesController(ReturnNoteApp app)
        {
            _returnnoteApp = app;
        }
    }
}
