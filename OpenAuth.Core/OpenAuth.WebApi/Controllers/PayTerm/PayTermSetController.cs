using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using OpenAuth.App.Response;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using OpenAuth.Repository.Domain;
using NSAP.Entity.Sales;
using OpenAuth.App.PayTerm;
using OpenAuth.App.PayTerm.PayTermSetHelp;
using OpenAuth.App.Client;
using OpenAuth.App.Client.Request;
using OpenAuth.App.Client.Response;
using OpenAuth.App;
using OpenAuth.App.Order;

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
        private ServiceBaseApp _serviceBaseApp;
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="UnitWork"></param>
        /// <param name="auth"></param>
        /// <param name="payTermApp"></param>
        public PayTermSetController(ServiceSaleOrderApp serviceSaleOrderApp, ServiceBaseApp serviceBaseApp, PayTermApp payTermApp, IUnitWork UnitWork, IAuth auth)
        {
            this.UnitWork = UnitWork;
            this._auth = auth;
            this._payTermApp = payTermApp;
            this._serviceBaseApp = serviceBaseApp;
            this._serviceSaleOrderApp = serviceSaleOrderApp;
        }

        #region 付款条件设置
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
        /// 付款条件详细信息
        /// </summary>
        /// <param name="GroupNum">付款条件名称</param>
        /// <returns>返回付款条件详细信息</returns>
        [HttpGet]
        public async Task<TableData> GetPayTermSaveDetail(string GroupNum)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.GetPayTermSaveDetail(GroupNum);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{GroupNum.ToJson()}，错误：{result.Message}");
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

        /// <summary>
        /// 客户付款条件历史记录
        /// </summary>
        /// <param name="cardCode">客户编码</param>
        /// <returns>返回客户付款条件历史记录信息</returns>
        [HttpGet]
        public async Task<TableData> GetCardCodePayTermHistory(string cardCode)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.GetCardCodePayTermHistory(cardCode);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{cardCode}，错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// f付款条件Id
        /// </summary>
        /// <param name="groupNum">付款条件</param>
        /// <returns>返回付款条件信息</returns>
        [HttpGet]
        public async Task<TableData> GetGroupNumId(string groupNum)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.GetGroupNumId(groupNum);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{groupNum}，错误：{result.Message}");
            }

            return result;
        }
        #endregion

        #region 常用比例
        /// <summary>
        /// 添加常用比例
        /// </summary>
        /// <param name="obj">添加常用比例实体</param>
        /// <returns>返回添加结果</returns>
        [HttpPost]
        public async Task<TableData> AddUsedRate(AddPayUsedRate obj)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.AddUsedRate(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj}，错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 修改常用比例
        /// </summary>
        /// <param name="obj">修改常用比例实体</param>
        /// <returns>返回修改结果</returns>
        [HttpPost]
        public async Task<TableData> UpdateUsedRate(AddPayUsedRate obj)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.UpdateUsedRate(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj}，错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 删除常用比例
        /// </summary>
        /// <param name="payUsedRateId">常用比例Id</param>
        /// <returns>返回删除结果</returns>
        [HttpGet]
        public async Task<TableData> DeleteUsedRate(string payUsedRateId)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.DeleteUsedRate(payUsedRateId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{payUsedRateId}，错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 常用比例列表
        /// </summary>
        /// <returns>返回常用比例列表信息</returns>
        [HttpGet]
        public async Task<TableData> GetUsedRate()
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.GetUsedRate();
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
        /// 常用比例详情
        /// </summary>
        /// <param name="payUsedRateId">常用比例Id</param>
        /// <returns>返回常用比例详情信息</returns>
        [HttpGet]
        public async Task<TableData> GetUsedRateDetail(string payUsedRateId)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.GetUsedRateDetail(payUsedRateId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数{payUsedRateId.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        #endregion

        #region 限制规则
        /// <summary>
        /// 限制规则列表
        /// </summary>
        /// <returns>返回限制规则列表信息</returns>
        [HttpGet]
        public async Task<TableData> GetPayLimitRule()
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.GetPayLimitRule();
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
        /// 限制规则详情
        /// </summary>
        /// <param name="payLimitRuleId">限制规则Id</param>
        /// <returns>返回限制规则详情信息</returns>
        [HttpGet]
        public async Task<TableData> GetPayLimitRuleDetail(string payLimitRuleId)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.GetPayLimitRuleDetail(payLimitRuleId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数{payLimitRuleId.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 添加限制规则
        /// </summary>
        /// <param name="obj">限制规则实体</param>
        /// <returns>返回添加结果</returns>
        [HttpPost]
        public async Task<TableData> AddPayLimitRule(PayLimitRule obj)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.AddPayLimitRule(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 修改限制规则
        /// </summary>
        /// <param name="obj">限制规则实体</param>
        /// <returns>返回修改结果</returns>
        [HttpPost]
        public async Task<TableData> UpdatePayLimitRule(PayLimitRule obj)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.UpdatePayLimitRule(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 删除限制规则
        /// </summary>
        /// <param name="payLimitRuleId">限制规则Id</param>
        /// <returns>返回删除结果</returns>
        [HttpGet]
        public async Task<TableData> DeletePayLimitRule(string payLimitRuleId)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.DeletePayLimitRule(payLimitRuleId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数{payLimitRuleId.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        #endregion

        #region 自动冻结
        /// <summary>
        /// 获取自动冻结
        /// </summary>
        /// <returns>返回自动冻结</returns>
        [HttpGet]
        public async Task<TableData> GetAutoFreeze()
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.GetAutoFreeze();
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
        /// 添加自动规则
        /// </summary>
        /// <param name="objs">自动规则实体集合</param>
        /// <returns>返回添加结果</returns>
        [HttpPost]
        public async Task<TableData> AddAutoFreeze(List<PayAutoFreeze> objs)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.AddAutoFreeze(objs);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数{objs.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 修改自动冻结规则
        /// </summary>
        /// <param name="objs">自动冻结实体集合</param>
        /// <returns>返回修改结果</returns>
        [HttpPost]
        public async Task<TableData> UpdateAutoFreeze(List<PayAutoFreeze> objs)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.UpdateAutoFreeze(objs);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数{objs.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 删除自动冻结
        /// </summary>
        /// <param name="payAutoFreezeIds">自动冻结Id集合</param>
        /// <returns>返回删除结果</returns>
        [HttpPost]
        public async Task<TableData> DeleteAutoFreeze(List<string> payAutoFreezeIds)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.DeleteAutoFreeze(payAutoFreezeIds);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数{payAutoFreezeIds.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 设置自动冻结任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task SetAutoFreezeJob()
        {
            try
            {
                await _payTermApp.SetAutoFreezeJob();
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"地址：{Request.Path}， 错误：{ex.InnerException?.Message ?? ex.Message}");
            }
        }

        /// <summary>
        /// 钉钉推送即将冻结客户和已经冻结客户给业务员任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task SetDDSendMsgJob()
        {
            try
            {
                await _payTermApp.SetDDSendMsgJob();
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"地址：{Request.Path}， 错误：{ex.InnerException?.Message ?? ex.Message}");
            }
        }
        #endregion

        #region 3.0获取客户列表
        /// <summary>
        /// 客户列表
        /// </summary>
        /// <param name="clientListReq"></param>
        /// <returns></returns>
        [HttpPost]
        public TableData GetClientList(ClientListReq clientListReq)
        {
            int rowCount = 0;
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            var depID = _serviceBaseApp.GetSalesDepID(userId);
            bool rIsViewSelf = true;
            bool rIsViewSelfDepartment = _serviceSaleOrderApp.GetPagePowersByUrl("client/ClientInfo.aspx", userId)
                .ViewSelfDepartment;
            bool rIsViewFull = false;
            if (loginUser.Name == "韦京生" || loginUser.Name == "郭睿心" || loginUser.Name == "骆灵芝")
            {
                rIsViewFull = true;
            }

            bool rIsViewSales = true;
            try
            {
                result.Data = _payTermApp.SelectClientList(clientListReq.limit, clientListReq.page,
                    clientListReq.query, clientListReq.sortname, clientListReq.sortorder, sboid, userId, rIsViewSales,
                    rIsViewSelf, rIsViewSelfDepartment, rIsViewFull, depID, clientListReq.Label, clientListReq.ContectTel, clientListReq.SlpName, clientListReq.isReseller, clientListReq.Day, clientListReq.CntctPrsn, clientListReq.address
                    , clientListReq.U_CardTypeStr, clientListReq.U_ClientSource, clientListReq.U_CompSector, clientListReq.U_TradeType, clientListReq.U_StaffScale,
                    clientListReq.CreateStartTime, clientListReq.CreateEndTime, clientListReq.DistributionStartTime, clientListReq.DistributionEndTime,
            clientListReq.dNotesBalStart, clientListReq.dNotesBalEnd, clientListReq.ordersBalStart, clientListReq.ordersBalEnd,
            clientListReq.balanceStart, clientListReq.balanceEnd, clientListReq.balanceTotalStart, clientListReq.balanceTotalEnd, clientListReq.CardName, out rowCount);
                result.Count = rowCount;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{clientListReq.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        #endregion

        #region VIP客户
        /// <summary>
        /// VIP客户列表
        /// </summary>
        /// <param name="obj">查询VIP客户实体</param>
        /// <returns>返回VIP客户列表信息</returns>
        [HttpPost]
        public async Task<TableData> GetVIPCustomer(CustomerReq obj)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.GetVIPCustomer(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()} 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// VIP客户详情
        /// </summary>
        /// <param name="Id">VIP客户Id</param>
        /// <returns>返回客户详情信息</returns>
        [HttpGet]
        public async Task<TableData> GetVIPCustomerDetail(string Id)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.GetVIPCustomerDetail(Id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{Id.ToJson()} 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 添加VIP客户
        /// </summary>
        /// <param name="obj">VIP客户实体</param>
        /// <returns>返回添加结果</returns>
        [HttpPost]
        public async Task<TableData> AddVIPCustomer(PayVIPCustomer obj)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.AddVIPCustomer(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()} 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 修改VIP客户
        /// </summary>
        /// <param name="obj">VIP客户实体</param>
        /// <returns>返回修改结果</returns>
        [HttpPost]
        public async Task<TableData> UpdateVIPCustomer(PayVIPCustomer obj)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.UpdateVIPCustomer(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()} 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 删除VIP客户
        /// </summary>
        /// <param name="Id">VIP客户Id</param>
        /// <returns>返回删除结果</returns>
        [HttpGet]
        public async Task<TableData> DeleteVIPCustomer(string Id)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.DeleteVIPCustomer(Id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{Id.ToJson()} 错误：{result.Message}");
            }

            return result;
        }
        #endregion

        #region 冻结客户
        /// <summary>
        /// 冻结客户列表
        /// </summary>
        /// <param name="obj">客户信息实体</param>
        /// <returns>返回冻结客户列表信息</returns>
        [HttpPost]
        public async Task<TableData> GetFreezeCustomer(CustomerReq obj)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.GetFreezeCustomer(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()} 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 冻结客户详细信息
        /// </summary>
        /// <param name="Id">冻结客户Id</param>
        /// <returns>返回冻结客户详细信息</returns>
        [HttpGet]
        public async Task<TableData> GetFreezeCustomerDetail(string Id)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.GetFreezeCustomerDetail(Id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{Id.ToJson()} 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 添加冻结客户
        /// </summary>
        /// <param name="obj">冻结客户实体</param>
        /// <returns>返回添加结果</returns>
        [HttpPost]
        public async Task<TableData> AddFreezeCustomer(PayFreezeCustomer obj)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.AddFreezeCustomer(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()} 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 修改冻结客户
        /// </summary>
        /// <param name="obj">冻结客户实体</param>
        /// <returns>返回修改结果</returns>
        [HttpPost]
        public async Task<TableData> UpdateFreezeCustomer(PayFreezeCustomer obj)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.UpdateFreezeCustomer(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()} 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 删除冻结客户
        /// </summary>
        /// <param name="Id">冻结客户实体</param>
        /// <returns>返回删除结果</returns>
        [HttpGet]
        public async Task<TableData> DeleteFreezeCustomer(string Id)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.DeleteFreezeCustomer(Id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{Id.ToJson()} 错误：{result.Message}");
            }

            return result;
        }
        #endregion

        #region 即将冻结客户
        /// <summary>
        /// 即将冻结客户列表
        /// </summary>
        /// <param name="obj">客户信息实体</param>
        /// <returns>返回即将冻结客户列表信息</returns>
        [HttpPost]
        public async Task<TableData> GetWillFreezeCustomer(CustomerReq obj)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.GetWillFreezeCustomer(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()} 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 删除即将冻结客户
        /// </summary>
        /// <param name="Id">即将冻结客户Id</param>
        /// <returns>返回删除结果</returns>
        [HttpGet]
        public async Task<TableData> DeleteWillFreezeCustomer(string Id)
        {
            var result = new TableData();
            try
            {
                return await _payTermApp.DeleteWillFreezeCustomer(Id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{Id.ToJson()} 错误：{result.Message}");
            }

            return result;
        }
        #endregion
    }
}