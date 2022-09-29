using System;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using OpenAuth.App.Response;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using OpenAuth.Repository.Domain;
using NSAP.Entity.Sales;
using OpenAuth.App.PayTerm;

namespace OpenAuth.WebApi.Controllers.PayTerm
{
    /// <summary>
    /// 付款条件设置
    /// </summary>
    [Route("api/PayTerm/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "PayTerm")]
    public class PayTermSetController : ControllerBase
    {
        IAuth _auth;
        IUnitWork UnitWork;
        private PayTermApp _payTermApp;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="UnitWork"></param>
        /// <param name="auth"></param>
        /// <param name="payTermApp"></param>
        public PayTermSetController(PayTermApp payTermApp, IUnitWork UnitWork, IAuth auth)
        {
            this.UnitWork = UnitWork;
            this._auth = auth;
            this._payTermApp = payTermApp;
        }

        /// <summary>
        /// 付款条件设置信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetPayTermSetList()
        {
            var result = new TableData();
            try
            {
                 return await _payTermApp.GetPayTermSetList();
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
        /// 付款条件设置详情
        /// </summary>
        /// <param name="payTermSetId">付款条件Id</param>
        /// <returns>返回付款条件设置详情信息</returns>
        [HttpGet]
        public async Task<TableData> GetPayTermSetDetail(string payTermSetId)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.GetPayTermSetDetail(payTermSetId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{payTermSetId.ToJson()} 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="obj">付款条件实体参数</param>
        /// <returns>返回操作成功</returns>
        [HttpPost]
        public async Task<Response> Add(PayTermSet obj)
        {
            var result = new Response();
            try
            {
                var Message = await _payTermApp.Add(obj);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
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
        /// <param name="obj">付款条件实体参数</param>
        /// <returns>返回操作成功</returns>
        [HttpPost]
        public async Task<Response> UpDate(PayTermSet obj)
        {
            var result = new Response();
            try
            {
                var Message = await _payTermApp.UpDate(obj);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
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
        /// 删除
        /// </summary>
        /// <param name="payTermSetId">付款条件Id</param>
        /// <returns>返回操作成功</returns>
        [HttpGet]
        public async Task<Response> Delete(string payTermSetId)
        {
            var result = new Response();
            try
            {
                var Message = await _payTermApp.Delete(payTermSetId);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{payTermSetId.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 4.0付款条件保存
        /// </summary>
        /// <param name="obj">付款条件保存实体</param>
        /// <returns>返回操作成功</returns>
        [HttpPost]
        public async Task<TableData> PayTermSave(PayTermSave obj)
        {
            var result = new TableData();
            try
            {
                decimal totalPro = Convert.ToDecimal(obj.PrepaPro + obj.BefShipPro + obj.GoodsToPro + obj.AcceptancePayPro + obj.QualityAssurancePro);
                if (totalPro == 100m)
                {
                    result = await _payTermApp.PayTermSave(obj);
                   
                }
                else
                {
                    result.Code = 500;
                    result.Message = "总比例不等于100%";
                }
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
        /// 付款条件可选时间信息
        /// </summary>
        /// <returns>返回设置付款条件各阶段可选时间集合信息</returns>
        [HttpGet]
        public async Task<TableData> GetPayTermSetMsg()
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.GetPayTermSetMsg();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 付款条件详细信息
        /// </summary>
        /// <param name="groupNum">付款条件分组名称</param>
        /// <returns>返回付款条件详细信息</returns>
        [HttpGet]
        public async Task<TableData> GetPayTermSetDetailMsg(string groupNum)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.GetPayTermSetDetailMsg(groupNum);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{groupNum},错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 应收详情
        /// </summary>
        /// <param name="docEntry">销售订单单据编号</param>
        /// <param name="groupNum">付款条件</param>
        /// <returns>返回销售订单总体情况和应收明细信息</returns>
        [HttpGet]
        public async Task<TableData> GetReceivableDetail(int docEntry, string groupNum)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.GetReceivableDetail(docEntry, groupNum);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{groupNum}，{docEntry}，错误：{result.Message}");
            }

            return result;
        }
    }
}