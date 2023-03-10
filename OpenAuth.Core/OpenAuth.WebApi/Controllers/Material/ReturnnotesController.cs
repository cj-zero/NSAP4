using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Material.Response;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.WebApi.Model;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 退料表接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Material")]
    public class ReturnNotesController : ControllerBase
    {
        private readonly ReturnNoteApp _returnnoteApp;
        #region 暂时废弃
        ///// <summary>
        ///// 退料
        ///// </summary>
        ///// <param name="returnMaterialReq"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<TableData> ReturnMaterials(ReturnMaterialReq returnMaterialReq)
        //{
        //    var result = new TableData();
        //    try
        //    {
        //        await _returnnoteApp.ReturnMaterials(returnMaterialReq);
        //        List<ReturnMaterialDetail> data = new List<ReturnMaterialDetail>();
        //        foreach (var item in returnMaterialReq.ReturnMaterialDetail)
        //        {
        //            if (item.ReturnQty > 0)
        //            {
        //                data.Add(item);
        //            }
        //        }
        //        returnMaterialReq.ReturnMaterialDetail = data;
        //        result.Data = returnMaterialReq;
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //        Log.Logger.Error($"地址：{Request.Path}，参数：{returnMaterialReq.ToJson()}, 错误：{result.Message}");
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// 获取退料详情
        ///// </summary>
        ///// <param name="serviceOrderId"></param>
        ///// <param name="appUserId"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<TableData> GetReturnNoteInfo(int serviceOrderId, int appUserId)
        //{
        //    var result = new TableData();
        //    try
        //    {
        //        result = await _returnnoteApp.GetReturnNoteInfo(appUserId, serviceOrderId);
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //        Log.Logger.Error($"地址：{Request.Path}，参数：{serviceOrderId},{appUserId}, 错误：{result.Message}");
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// 获取物流详情
        ///// </summary>
        ///// <param name="expressageId"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<TableData> GetExpressageInfo(string expressageId)
        //{
        //    var result = new TableData();
        //    try
        //    {
        //        result = await _returnnoteApp.GetExpressageInfo(expressageId);
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //        Log.Logger.Error($"地址：{Request.Path}，参数：{expressageId}, 错误：{result.Message}");
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// 保存仓库验收记录（ERP）
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<Response> SaveReceiveInfo(ReturnNoteAuditReq req)
        //{
        //    var result = new Response();
        //    try
        //    {
        //        await _returnnoteApp.SaveReceiveInfo(req);

        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //        Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// 获取退料列表（ERP）
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<TableData> GetReturnNoteList([FromQuery] GetReturnNoteListReq req)
        //{
        //    var result = new TableData();
        //    try
        //    {
        //        result = await _returnnoteApp.GetReturnNoteList(req);
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //        Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// 获取退料详情（ERP）
        ///// </summary>
        ///// <param name="Id"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<TableData> GetReturnNoteDetail(int Id)
        //{
        //    var result = new TableData();
        //    try
        //    {
        //        result = await _returnnoteApp.GetReturnNoteDetail(Id);
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //        Log.Logger.Error($"地址：{Request.Path}，参数：{Id}, 错误：{result.Message}");
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// 验收（ERP）
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<Response> Accraditation(ReturnNoteAuditReq req)
        //{
        //    var result = new Response();
        //    try
        //    {
        //        await _returnnoteApp.Accraditation(req);

        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //        Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// 获取退料结算列表（ERP）
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<TableData> GetClearReturnNoteList([FromQuery] GetClearReturnNoteListReq req)
        //{
        //    var result = new TableData();
        //    try
        //    {
        //        result = await _returnnoteApp.GetClearReturnNoteList(req);
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //        Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// 获取退料结算详情（ERP）
        ///// </summary>
        ///// <param name="Id">退料单Id</param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<TableData> GetClearReturnNoteDetail(int Id)
        //{
        //    var result = new TableData();
        //    try
        //    {
        //        result = await _returnnoteApp.GetClearReturnNoteDetail(Id);
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //        Log.Logger.Error($"地址：{Request.Path}，参数：{Id}, 错误：{result.Message}");
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// 获取服务单详情(ERP)
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<TableData> GetServiceOrderInfo([FromQuery] PageReq req)
        //{
        //    var result = new TableData();
        //    try
        //    {
        //        result = await _returnnoteApp.GetServiceOrderInfo(req);
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //        Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// 获取退料单列表(ERP 仓库收货/品质入库/仓库入库)
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<TableData> GetReturnNoteListByExpress([FromQuery] GetReturnNoteListByExpressReq req)
        //{
        //    var result = new TableData();
        //    try
        //    {
        //        result = await _returnnoteApp.GetReturnNoteListByExpress(req);
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //        Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// 获取退料单详情（根据物流单号）
        ///// </summary>
        ///// <param name="ExpressageId">物流单Id</param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<TableData> GetReturnNoteDetailByExpress(string ExpressageId)
        //{
        //    var result = new TableData();
        //    try
        //    {
        //        result = await _returnnoteApp.GetReturnNoteDetailByExpress(ExpressageId);
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //        Log.Logger.Error($"地址：{Request.Path}，参数：{ExpressageId}, 错误：{result.Message}");
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// 品质检验
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<TableData> CheckOutMaterials(CheckOutMaterialsReq req)
        //{
        //    var result = new TableData();
        //    try
        //    {
        //        await _returnnoteApp.CheckOutMaterials(req);
        //        result.Data = req;
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //        Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// 仓库入库
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<TableData> WarehousePutMaterialsIn(WarehousePutMaterialsInReq req)
        //{
        //    var result = new TableData();
        //    try
        //    {
        //        await _returnnoteApp.WarehousePutMaterialsIn(req);
        //        result.Data = req;
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //        Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
        //    }

        //    return result;
        //}

        #endregion
        
        /// <summary>
        /// 获取带退料物料明细
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetMaterialList([FromQuery] ReturnMaterialReq req)
        {
            return await _returnnoteApp.GetMaterialList(req);
        }
        /// <summary>
        /// 查询退料信息列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Load([FromQuery] ReturnMaterialReq req)
        {
            return await _returnnoteApp.Load(req);
        }
        /// <summary>
        /// 获取带退料物料明细
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetOinvList([FromQuery] ReturnMaterialReq req)
        {
            return await _returnnoteApp.GetOinvList(req);

        }
        /// <summary>
        /// 获取序列号信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetSerialNumberList([FromQuery] ReturnMaterialReq req)
        {
            return await _returnnoteApp.GetSerialNumberList(req);
        }

        /// <summary>
        /// 通过序列号和领料单号获取物料
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetMaterialListBySerialNumber([FromQuery] ReturnMaterialReq req)
        {
            return await _returnnoteApp.GetMaterialListBySerialNumber(req);
        }
        /// <summary>
        /// 获取退料单详情
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetDetails([FromQuery] ReturnMaterialReq req) 
        {
            return await _returnnoteApp.GetDetails(req);
        }

        /// <summary>
        /// 更换列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetQuotationList([FromQuery] ReturnMaterialReq req)
        {
            TableData result = new TableData();
            try
            {
                result= await _returnnoteApp.GetQuotationList(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 添加退料单
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Add(AddOrUpdateReturnnoteReq obj) 
        {
            await _returnnoteApp.Add(obj);
            return new Response();
        }
        /// <summary>
        /// 修改退料单
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Update(AddOrUpdateReturnnoteReq obj)
        {
            await _returnnoteApp.Update(obj);
            return new Response();
        }
        /// <summary>
        /// 删除退料单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Delete(ReturnMaterialReq req)
        {
            await _returnnoteApp.Delete((int)req.returnNoteId);
            return new Response();
        }
        /// <summary>
        /// 审批退料单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Accraditation(AccraditationReturnNoteReq req)
        {
            await _returnnoteApp.Accraditation(req);
            return new Response();
        }

        /// <summary>
        /// 添加物料更换记录
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> CreateMaterialReplaceRecord(AddOrUpdateMaterialReplaceRecordReq obj)
        {
            await _returnnoteApp.CreateMaterialReplaceRecord(obj);
            return new Response();
        }

        /// <summary>
        /// 同步应收贷项凭证
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task WarehousePutMaterialsIn(int? id)
        {
            await _returnnoteApp.WarehousePutMaterialsIn(new AccraditationReturnNoteReq { Id = id });
        }

        /// <summary>
        /// 打印退料单
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="IsTrue"></param>
        /// <param name="sign"></param>
        /// <param name="timespan"></param>
        /// <returns></returns>
        [HttpGet]
        //[ServiceFilter(typeof(CertAuthFilter))]
        public async Task<IActionResult> PrintReturnnote(string serialNumber, string sign, string timespan)
        {
            try
            {
                return File(await _returnnoteApp.PrintReturnnote(serialNumber), "application/pdf");
            }
            catch (Exception e)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{serialNumber},{sign},{timespan}, 错误：{e.Message}");
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 退料物料汇总
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetReturnNoteList(int ReturnnoteId)
        {
            try
            {
                return await _returnnoteApp.GetReturnNoteList(ReturnnoteId);
            }
            catch (Exception e)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{ReturnnoteId}, 错误：{e.Message}");
                throw new Exception(e.Message);
            }
        }

        public ReturnNotesController(ReturnNoteApp app)
        {
            _returnnoteApp = app;
        }
    }
}
