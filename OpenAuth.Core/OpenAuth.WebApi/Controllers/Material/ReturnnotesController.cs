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
        public async Task<TableData> GetExpressInfo(string expressageId)
        {
            var result = new TableData();
            try
            {
                result = await _returnnoteApp.GetExpressInfo(expressageId);
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
        /// <param name="ReturnMaterials"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> SaveReceiveInfo(List<ReturnMaterial> ReturnMaterials)
        {
            var result = new Response();
            try
            {
                await _returnnoteApp.SaveReceiveInfo(ReturnMaterials);

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
