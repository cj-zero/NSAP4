using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NetOffice.Extensions.Invoker;
using Newtonsoft.Json;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;
using OpenAuth.Repository.Domain;
using Serilog;
using static Infrastructure.HttpHelper;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 服务单
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    public class ServiceOrderController : Controller
    {
        private readonly ServiceOrderApp _serviceOrderApp;
        private AppServiceOrderLogApp _appServiceOrderLogApp;
        public readonly OrgManagerApp _orgmanagerapp;
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁
        private readonly HttpClienService _httpClienService;

        public ServiceOrderController(ServiceOrderApp serviceOrderApp, AppServiceOrderLogApp appServiceOrderLogApp, HttpClienService httpClienService, OrgManagerApp orgmanagerapp)
        {
            _serviceOrderApp = serviceOrderApp;
            _appServiceOrderLogApp = appServiceOrderLogApp;
            _orgmanagerapp = orgmanagerapp;
            _httpClienService = httpClienService;
        }


        #region<<nSAP System>>
        /// <summary>
        /// 服务单查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public Task<TableData> Load([FromQuery] QueryServiceOrderListReq query)
        {
            var result = new TableData();
            try
            {
                result.Data = _serviceOrderApp.Load(query);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{query.ToJson()}， 错误：{result.Message}");
            }
            return Task.FromResult(result);
        }

        /// <summary>
        /// 待确认服务呼叫列表
        /// </summary>
        /// <param name="req">查询条件对象</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> UnConfirmedServiceOrderList([FromQuery] QueryServiceOrderListReq req)
        {
            var result = new TableData();
            try
            {
                result.Data = await _serviceOrderApp.UnConfirmedServiceOrderList(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 待确认服务申请信息
        /// </summary>
        /// <param name="serviceOrderId">服务单ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<ServiceOrderDetailsResp>> GetUnConfirmedServiceOrderDetails(int serviceOrderId)
        {
            var result = new Response<ServiceOrderDetailsResp>();
            try
            {
                result.Result = await _serviceOrderApp.GetUnConfirmedServiceOrderDetails(serviceOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{serviceOrderId}， 错误：{result.Message}");
            }

            return result;
        }


        /// <summary>
        /// 创建工单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> CreateWorkOrder(UpdateServiceOrderReq request)
        {
            var result = new Response();
            try
            {
                await _serviceOrderApp.CreateWorkOrder(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }


        /// <summary>
        /// 删除一个工单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> DeleteWorkOrder(int id)
        {
            var result = new Response();
            try
            {
                await _serviceOrderApp.DeleteWorkOrder(id);
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
        /// 新增一个工单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AddWorkOrder(AddServiceWorkOrderReq request)
        {
            var result = new Response();
            try
            {
                await _serviceOrderApp.AddWorkOrder(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }


        /// <summary>
        /// 更新修改工单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> UpdateWorkOrder(UpdateWorkOrderReq request)
        {
            var result = new Response();
            try
            {
                await _serviceOrderApp.UpdateWorkOrder(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 修改服务单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> ModifyServiceOrder(ModifyServiceOrderReq request)
        {
            var result = new Response();
            try
            {
                await _serviceOrderApp.ModifyServiceOrder(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 呼叫服务未派单页面左侧树数据源
        /// </summary>
        /// <param name="req">页面查询条件</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> UnsignedWorkOrderTree([FromQuery] QueryServiceOrderListReq req)
        {
            var result = new TableData();
            try
            {
                result.Data = await _serviceOrderApp.UnsignedWorkOrderTree(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 呼叫服务未派单右侧查询列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> UnsignedWorkOrderList([FromQuery] QueryServiceOrderListReq req)
        {
            var result = new TableData();
            try
            {
                result.Data = await _serviceOrderApp.UnsignedWorkOrderList(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 客服新建服务单
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> CustomerServiceAgentCreateOrder(CustomerServiceAgentCreateOrderReq req)
        {
            var result = new Response();
            try
            {
                await _serviceOrderApp.CustomerServiceAgentCreateOrder(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 工程部新建服务单
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> CISECreateServiceOrder(CustomerServiceAgentCreateOrderReq req)
        {
            var result = new Response();
            try
            {
                await _serviceOrderApp.CISECreateServiceOrder(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }
        
        /// <summary>
        /// 获取服务单详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<ServiceOrderDetailsResp>> GetDetails(int id)
        {

            var result = new Response<ServiceOrderDetailsResp>();

            try
            {
                result.Result = await _serviceOrderApp.GetDetails(id);
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
        /// 技术员工单池列表（暂未使用）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetTechnicianServiceWorkOrderPool([FromQuery] TechnicianServiceWorkOrderPoolReq req)
        {
            var result = new TableData();

            try
            {
                result = await _serviceOrderApp.GetTechnicianServiceWorkOrderPool(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 呼叫服务(客服)右侧查询列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ServiceWorkOrderList([FromQuery] QueryServiceOrderListReq req)
        {
            var result = new TableData();
            try
            {
                result.Data = await _serviceOrderApp.ServiceWorkOrderList(req);
                result.Count = result.Data.Count;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }


        /// <summary>
        /// 查询可以被派单的技术员列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetAllowSendOrderUser([FromQuery] GetAllowSendOrderUserReq req)
        {
            var result = new TableData();
            try
            {
                result = await _serviceOrderApp.GetAllowSendOrderUser(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 调出该客户代码近10个呼叫ID,及未关闭的近10个呼叫ID
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<dynamic>> GetCustomerNewestOrders(string code)
        {
            var result = new Response<dynamic>();
            try
            {
                result.Result = await _serviceOrderApp.GetCustomerNewestOrders(code);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{code}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取服务工单详情（废弃）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetAppServiceOrderDetail([FromQuery] QueryServiceOrderDetailReq req)
        {
            var result = new TableData();

            try
            {
                result = await _serviceOrderApp.GetAppServiceOrderDetail(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 回访服务单
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> ServiceOrderCallback(int serviceOrderId)
        {
            var result = new Response();
            try
            {
                await _serviceOrderApp.ServiceOrderCallback(serviceOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{serviceOrderId}， 错误：{result.Message}");
            }
            return result;
        }


        /// <summary>
        /// 根据服务单id获取行为报告单数据
        /// </summary>
        /// <param name="ServiceOrderId"></param>
        /// <returns></returns>
        [HttpGet]
        public TableData GetServiceOrder(string ServiceOrderId)
        {
            var result = new TableData();
            try
            {
                result = _serviceOrderApp.GetServiceOrder(ServiceOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ServiceOrderId}， 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 根据服务单id判断是否撤销服务单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> UpDateServiceOrderStatus([FromQuery]SendServiceOrderMessageReq req)
        {
            var result = new TableData();
            try
            {
                result = await _serviceOrderApp.UpDateServiceOrderStatus(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req}， 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ExportExcel([FromQuery] QueryServiceOrderListReq req)
        {
            var data = await _serviceOrderApp.ExportExcel(req);


            return File(data, "application/vnd.ms-excel");
        }


        ///// <summary>
        ///// 获取隐私号码
        ///// </summary>
        ///// <param name="ServiceOrderId"></param>
        ///// <param name="MaterialType"></param>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<TableData> GetProtectPhone(int ServiceOrderId, string MaterialType, int type)
        //{
        //    var result = new TableData();
        //    try
        //    {
        //        result = await _serviceOrderApp.GetProtectPhone(ServiceOrderId, MaterialType, type);
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.Message;
        //    }
        //    return result;
        //}

        /// <summary>
        /// 服务呼叫按售后部门、销售员、问题类型、接单员统计处理数量并排行
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ServiceWorkOrderReport([FromQuery] QueryServiceOrderListReq req)
        {
            var result = new TableData();
            try
            {
                result = await _serviceOrderApp.ServiceWorkOrderReport(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取工单详情根据工单Id
        /// </summary>
        /// <param name="workOrderId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetWorkOrderDetailById(int workOrderId)
        {
            var result = new TableData();
            try
            {
                result = await _serviceOrderApp.GetWorkOrderDetailById(workOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{workOrderId}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// nSAP主管给技术员派单
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response<bool>> nSAPSendOrders(SendOrdersReq req)
        {

            var result = new Response<bool>();
            try
            {
                //var parameters = new { req.ServiceOrderId, req.QryMaterialTypes, req.CurrentUserId };
                //var r = _helper.Post(parameters, "api/serve/ServiceOrder/SendOrders", Request.Headers["X-Token"].ToString());
                //result = JsonConvert.DeserializeObject<Response<bool>>(r);
                await _serviceOrderApp.nSAPSendOrders(req);
                result.Result = true;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                result.Result = false;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 呼叫服务(销售员)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> SalesManServiceWorkOrderList([FromQuery] QueryServiceOrderListReq req)
        {
            var result = new TableData();
            try
            {
                return await _serviceOrderApp.SalesManServiceWorkOrderList(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 消息已读
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> ReadMsg(ReadMsgReq req)
        {
            var result = new Response();
            try
            {
                await _serviceOrderApp.ReadMsg(req.currentUserId, req.serviceOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 一键重派
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> OneKeyResetServiceOrder(OneKeyResetServiceOrderReq req)
        {

            var result = new Response();
            try
            {
                await _serviceOrderApp.OneKeyResetServiceOrder(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 撤回聊天室消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> RevocationMessage(SendServiceOrderMessageReq request)
        {
            var result = new TableData();
            try
            {
                return await _serviceOrderApp.RevocationMessage(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }


        /// <summary>
        /// 获取服务单日报信息
        /// </summary>
        /// <param name="ServiceOrderId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="reimburseId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetErpTechnicianDailyReport(int ServiceOrderId, string startDate, string endDate, string reimburseId)
        {
            var result = new TableData();
            try
            {
                return await _serviceOrderApp.GetErpTechnicianDailyReport(ServiceOrderId, startDate, endDate, reimburseId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ServiceOrderId},{startDate},{endDate},{reimburseId}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 查询费用归属数据
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetReimburseOrgs()
        {
            return await _orgmanagerapp.GetReimburseOrgs();
        }
        #endregion

        #region App售后接口 如无特殊情况勿动，修改请告知！！！
        #region<<Common Methods>>
        /// <summary>
        /// 获取服务单图片Id列表
        /// </summary>
        /// <param name="id">报价单Id</param>
        /// <param name="type">1-客户上传 2-客服上传</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<List<UploadFileResp>>> GetServiceOrderPictures(int id, int type)
        {
            var result = new Response<List<UploadFileResp>>();

            try
            {
                result.Result = await _serviceOrderApp.GetServiceOrderPictures(id, type);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id},{type}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取技术员位置信息(废弃)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetTechnicianLocation([FromQuery] GetTechnicianLocationReq req)
        {
            var result = new TableData();
            try
            {
                result = await _serviceOrderApp.GetTechnicianLocation(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }
        #endregion

        #region<<Message>>
        /// <summary>
        /// 发送聊天室消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> SendServiceOrderMessage(SendServiceOrderMessageReq request)
        {
            var result = new Response();
            try
            {
                await _serviceOrderApp.SendServiceOrderMessage(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        ///获取服务单聊天记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetServiceOrderMessage([FromQuery] GetServiceOrderMessageReq request)
        {
            var result = new TableData();
            try
            {
                result = await _serviceOrderApp.GetServiceOrderMessage(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        ///获取服务单聊天列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetServiceOrderMessageList([FromQuery] GetServiceOrderMessageListReq request)
        {
            var result = new TableData();
            try
            {
                result = await _serviceOrderApp.GetServiceOrderMessageList(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        ///获取未读消息个数
        /// </summary>
        /// <param name="CurrentUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetMessageCount(int CurrentUserId)
        {
            var result = new TableData();
            try
            {
                result = await _serviceOrderApp.GetMessageCount(CurrentUserId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{CurrentUserId}， 错误：{result.Message}");
            }
            return result;
        }


        #endregion

        #region<<Customer>>
        /// <summary>
        /// App提交服务单
        /// </summary>
        /// <param name="addServiceOrderReq"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Add(AddServiceOrderReq addServiceOrderReq)
        {
            var result = new Response();
            try
            {
                var r = await _httpClienService.Post(addServiceOrderReq, "api/serve/ServiceOrder/Add");
                result = JsonConvert.DeserializeObject<Response>(r);
                //await _serviceOrderApp.Add(addServiceOrderReq);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{addServiceOrderReq.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// app查询服务单详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<dynamic>> AppLoadServiceOrderDetails([FromQuery] AppQueryServiceOrderReq request)
        {
            var result = new Response<dynamic>();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("ServiceOrderId", request.ServiceOrderId);
                parameters.Add("MaterialType", request.MaterialType);

                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/AppLoadServiceOrderDetails");
                result = JsonConvert.DeserializeObject<Response<dynamic>>(r);
                //result = await _serviceOrderApp.AppLoadServiceOrderDetails(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取设备列表中间页/售后详情页（客户）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetAppCustServiceOrderDetails(int ServiceOrderId)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("ServiceOrderId", ServiceOrderId);
                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetAppCustServiceOrderDetails");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _serviceOrderApp.GetAppCustServiceOrderDetails(ServiceOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ServiceOrderId}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取客户快报
        /// </summary>
        /// <param name="appUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetCustServiceNews(int appUserId)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("appUserId", appUserId);
                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetCustServiceNews");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _serviceOrderApp.GetAppCustServiceOrderDetails(ServiceOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{appUserId}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// app查询服务单列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AppLoad([FromQuery] AppQueryServiceOrderListReq request)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("AppUserId", request.AppUserId);
                parameters.Add("Type", request.Type);
                parameters.Add("limit", request.limit);
                parameters.Add("page", request.page);
                parameters.Add("key", request.key);

                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/AppLoad");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _serviceOrderApp.AppLoad(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }
        #endregion

        #region<<Technician>>
        /// <summary>
        /// 技术员查看服务单单列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetTechnicianServiceOrder([FromQuery] TechnicianServiceWorkOrderReq req)
        {

            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("TechnicianId", req.TechnicianId);
                parameters.Add("Type", req.Type);
                //parameters.Add("Longitude", req.Longitude);
                //parameters.Add("Latitude", req.Latitude);
                parameters.Add("limit", req.limit);
                parameters.Add("page", req.page);
                parameters.Add("key", req.key);
                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetTechnicianServiceOrder");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _serviceOrderApp.GetTechnicianServiceOrder(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 技术员查看服务单单列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetTechnicianServiceOrderNew([FromQuery] TechnicianServiceWorkOrderReq req)
        {

            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("TechnicianId", req.TechnicianId);
                parameters.Add("Type", req.Type);
                parameters.Add("TechOrg", req.TechOrg);
                parameters.Add("limit", req.limit);
                parameters.Add("page", req.page);
                parameters.Add("key", req.key);
                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetTechnicianServiceOrderNew");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _serviceOrderApp.GetTechnicianServiceOrder(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取技术员设备类型列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AppTechnicianLoad(int SapOrderId, int CurrentUserId, string MaterialType)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("SapOrderId", SapOrderId);
                parameters.Add("CurrentUserId", CurrentUserId);
                parameters.Add("MaterialType", MaterialType);

                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/AppTechnicianLoad");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _serviceOrderApp.AppTechnicianLoad(SapOrderId, CurrentUserId, MaterialType);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{SapOrderId},{CurrentUserId},{MaterialType}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 保存接单类型
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> SaveOrderTakeType(SaveWorkOrderTakeTypeReq request)
        {
            var result = new TableData();
            try
            {
                var parameters = new { request.ServiceOrderId, request.Type, request.CurrentUserId, request.MaterialType };
                var r = await _httpClienService.Post(parameters, "api/serve/ServiceOrder/SaveOrderTakeType");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //await _serviceOrderApp.SaveOrderTakeType(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 技术员预约工单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> BookingWorkOrder(BookingWorkOrderReq req)
        {
            var result = new Response();
            try
            {
                var parameters = new { req.BookingDate, req.ServiceOrderId, req.CurrentUserId, req.MaterialType };
                var r = await _httpClienService.Post(parameters, "api/serve/ServiceOrder/BookingWorkOrder");
                result = JsonConvert.DeserializeObject<Response>(r);
                //await _serviceOrderApp.BookingWorkOrder(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 技术员核对设备
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> CheckTheEquipment(CheckTheEquipmentReq req)
        {
            var result = new Response();
            try
            {
                var parameters = new { req.MaterialType, req.ServiceOrderId, req.CurrentUserId, req.ErrorWorkOrderIds, req.CheckWorkOrderIds };
                var r = await _httpClienService.Post(parameters, "api/serve/ServiceOrder/CheckTheEquipment");
                result = JsonConvert.DeserializeObject<Response>(r);
                //await _serviceOrderApp.CheckTheEquipment(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 修改描述（故障/过程）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> UpdateWorkOrderDescription(UpdateWorkOrderDescriptionReq request)
        {
            var result = new Response();
            try
            {
                var parameters = new { request.MaterialType, request.ServiceOrderId, request.CurrentUserId, request.Description };
                var r = await _httpClienService.Post(parameters, "api/serve/ServiceOrder/UpdateWorkOrderDescription");
                result = JsonConvert.DeserializeObject<Response>(r);
                //await _serviceOrderApp.UpdateWorkOrderDescription(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 技术员填写解决方案
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> SaveWorkOrderSolution(SaveWorkOrderSolutionReq req)
        {
            var result = new Response();
            try
            {
                var parameters = new { req.MaterialType, req.ServiceOrderId, req.CurrentUserId, req.SolutionId };
                var r = await _httpClienService.Post(parameters, "api/serve/ServiceOrder/SaveWorkOrderSolution");
                result = JsonConvert.DeserializeObject<Response>(r);
                //await _serviceOrderApp.SaveWorkOrderSolution(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }


        /// <summary>
        /// 技术员接单(暂未使用/使用时再修改)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> TechnicianTakeOrder(TechnicianTakeOrderReq req)
        {
            var result = new TableData();

            try
            {
                //用信号量代替锁
                await semaphoreSlim.WaitAsync();
                try
                {
                    await _serviceOrderApp.TechnicianTakeOrder(req);
                    result.Data = await _serviceOrderApp.GetUserCanOrderCount(req.TechnicianId);
                }
                finally
                {
                    semaphoreSlim.Release();
                }

            }
            catch (CommonException ex)
            {
                result.Code = ex.Code;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取技术员服务单工单列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetAppTechnicianServiceWorkOrder([FromQuery] GetAppTechnicianServiceWorkOrderReq req)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("ServiceOrderId", req.ServiceOrderId);
                parameters.Add("TechnicianId", req.TechnicianId);
                parameters.Add("Type", req.Type);
                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetAppTechnicianServiceWorkOrder");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _serviceOrderApp.GetAppTechnicianServiceWorkOrder(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }


        /// <summary>
        /// 获取技术员服务单详情
        /// </summary>
        /// <param name="SapOrderId">SapId</param>
        /// <param name="CurrentUserId">当前技术员App用户Id</param>
        /// <param name="MaterialType">设备类型</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetAppTechnicianServiceOrderDetails(int SapOrderId, int CurrentUserId, string MaterialType)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("SapOrderId", SapOrderId);
                parameters.Add("CurrentUserId", CurrentUserId);
                parameters.Add("MaterialType", MaterialType);

                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetAppTechnicianServiceOrderDetails");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _serviceOrderApp.GetAppTechnicianServiceOrderDetails(SapOrderId, CurrentUserId, MaterialType);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{SapOrderId},{CurrentUserId},{MaterialType}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取售后详情（技术员）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetAppTechServiceOrderDetails(int ServiceOrderId)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("ServiceOrderId", ServiceOrderId);

                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetAppTechServiceOrderDetails");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _serviceOrderApp.GetAppTechServiceOrderDetails(ServiceOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ServiceOrderId}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取技术员单据数量列表
        /// </summary>
        /// <param name="CurrentUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetTechnicianServiceOrderCount(int CurrentUserId, int TechOrg)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("CurrentUserId", CurrentUserId);
                parameters.Add("TechOrg", TechOrg);

                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetTechnicianServiceOrderCount");
                result = JsonConvert.DeserializeObject<TableData>(r);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{CurrentUserId}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 技术员结束维修
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> TechicianEndRepair(TechicianEndRepairReq request)
        {
            var result = new Response();
            try
            {
                var parameters = new { request.MaterialType, request.ServiceOrderId, request.CurrentUserId };
                var r = await _httpClienService.Post(parameters, "api/serve/ServiceOrder/TechicianEndRepair");
                result = JsonConvert.DeserializeObject<Response>(r);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取该技术员下的当前服务单所有设备
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetTechnicianServiceMaterials([FromQuery] GetTechnicianServiceMaterialsReq req)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("ServiceOrderId", req.ServiceOrderId);
                parameters.Add("TechnicianId", req.TechnicianId);

                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetTechnicianServiceMaterials");
                result = JsonConvert.DeserializeObject<TableData>(r);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 技术员填写日报
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AddDailyReport(AddDailyReportReq req)
        {
            var result = new Response();
            try
            {
                var r = await _httpClienService.Post(req, "api/serve/ServiceOrder/AddDailyReport");
                result = JsonConvert.DeserializeObject<Response>(r);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取日报详情（根据日期过滤）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetTechnicianDailyReport([FromQuery] GetTechnicianDailyReportReq req)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("ServiceOrderId", req.ServiceOrderId);
                parameters.Add("TechnicianId", req.TechnicianId);
                parameters.Add("Date", req.Date);

                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetTechnicianDailyReport");
                result = JsonConvert.DeserializeObject<TableData>(r);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 自定义问题描述/解决方案
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AddProblemOrSolution(AddProblemOrSolutionReq req)
        {
            var result = new Response();
            try
            {
                var r = await _httpClienService.Post(req, "api/serve/ServiceOrder/AddProblemOrSolution");
                result = JsonConvert.DeserializeObject<Response>(r);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error(result.Message);
            }

            return result;
        }

        /// <summary>
        /// 获取我自定义的问题描述和解决方案
        /// </summary>
        /// <param name="AppUserId"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetMyProblemOrSolution(int AppUserId, int Type)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("AppUserId", AppUserId);
                parameters.Add("Type", Type);

                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetMyProblemOrSolution");
                result = JsonConvert.DeserializeObject<TableData>(r);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{AppUserId},{Type}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 添加日费
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AddDailyExpends(AddDailyExpendsReq req)
        {
            var result = new Response();
            try
            {
                var r = await _httpClienService.Post(req, "api/serve/ServiceOrder/AddDailyExpends");
                result = JsonConvert.DeserializeObject<Response>(r);
                //await _serviceOrderApp.AddDailyExpands(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取日费详情（根据日期过滤）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetTechnicianDailyExpend([FromQuery] GetTechnicianDailyReportReq req)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("ServiceOrderId", req.ServiceOrderId);
                parameters.Add("TechnicianId", req.TechnicianId);
                parameters.Add("Date", req.Date);

                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetTechnicianDailyExpend");
                result = JsonConvert.DeserializeObject<TableData>(r);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取日费汇总
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetServiceDailyExpendSum([FromQuery] GetTechnicianDailyReportReq req)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("ServiceOrderId", req.ServiceOrderId);
                parameters.Add("TechnicianId", req.TechnicianId);

                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetServiceDailyExpendSum");
                result = JsonConvert.DeserializeObject<TableData>(r);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }
        #endregion

        #region<<Admin/Supervisor>>
        /// <summary>
        ///  获取管理员服务单列表（App）
        /// </summary>
        /// <param name="req">查询条件对象</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AppUnConfirmedServiceOrderList([FromQuery] QueryAppServiceOrderListReq req)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("QryState", req.QryState);
                parameters.Add("AppUserId", req.AppUserId);
                parameters.Add("limit", req.limit);
                parameters.Add("page", req.page);
                parameters.Add("key", req.key);
                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/AppUnConfirmedServiceOrderList");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _serviceOrderApp.AppUnConfirmedServiceOrderList(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取待派单的服务单详情/获取设备类型列表（管理员）
        /// </summary>
        /// <param name="SapOrderId">SapId</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetAppAdminServiceOrderDetails(int SapOrderId)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("SapOrderId", SapOrderId);

                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetAppAdminServiceOrderDetails");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _serviceOrderApp.GetAppAdminServiceOrderDetails(SapOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{SapOrderId}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 查询可以被派单的技术员列表(App)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetAppAllowSendOrderUser([FromQuery] GetAllowSendOrderUserReq req)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("Longitude", req.Longitude);
                parameters.Add("Latitude", req.Latitude);
                parameters.Add("CurrentUserId", req.CurrentUserId);
                parameters.Add("CurrentUser", req.CurrentUser);
                parameters.Add("TechnicianId", req.TechnicianId);
                parameters.Add("limit", req.limit);
                parameters.Add("page", req.page);
                parameters.Add("key", req.key);

                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetAppAllowSendOrderUser");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _serviceOrderApp.GetAppAllowSendOrderUser(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 主管给技术员派单
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response<bool>> SendOrders(SendOrdersReq req)
        {

            var result = new Response<bool>();
            try
            {
                var r = await _httpClienService.Post(req, "api/serve/ServiceOrder/SendOrders");
                result = JsonConvert.DeserializeObject<Response<bool>>(r);
                //await _serviceOrderApp.SendOrders(req);
                //result.Result = true;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                result.Result = false;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 主管给技术员派单(转派)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response<bool>> TransferOrders(TransferOrdersReq req)
        {

            var result = new Response<bool>();
            try
            {
                var parameters = new { req.ServiceOrderId, req.MaterialType, req.TechnicianId };
                var r = await _httpClienService.Post(parameters, "api/serve/ServiceOrder/TransferOrders");
                result = JsonConvert.DeserializeObject<Response<bool>>(r);
                //await _serviceOrderApp.SendOrders(req);
                //result.Result = true;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                result.Result = false;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 主管关单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> CloseWorkOrder(CloseWorkOrderReq request)
        {
            var result = new Response();
            try
            {
                var parameters = new { request.ServiceOrderId, request.Reason, request.CurrentUserId };
                var r = await _httpClienService.Post(parameters, "api/serve/ServiceOrder/CloseWorkOrder");
                result = JsonConvert.DeserializeObject<Response>(r);
                //await _serviceOrderApp.CloseWorkOrder(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        ///获取当前技术员剩余可接单数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetUserCanOrderCount(int id)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("id", id);

                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetUserCanOrderCount");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result.Data = await _serviceOrderApp.GetUserCanOrderCount(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id}， 错误：{result.Message}");
            }
            return result;
        }

        #endregion

        #region<<SalesMan>>
        /// <summary>
        /// 获取业务员工单列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetSalesManServiceOrder([FromQuery] GetSalesManServiceOrderReq req)
        {

            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("AppUserId", req.AppUserId);
                parameters.Add("Type", req.Type);
                parameters.Add("limit", req.limit);
                parameters.Add("page", req.page);
                parameters.Add("key", req.key);

                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetSalesManServiceOrder");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _serviceOrderApp.GetSalesManServiceOrder(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取服务单设备类型列表（业务员查看）
        /// </summary>
        /// <param name="ServiceorderId"></param>
        /// <param name="AppUserId"></param>
        /// <param name="MaterialType"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AppSalesManLoad(int ServiceorderId, int AppUserId, string MaterialType)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("ServiceorderId", ServiceorderId);
                parameters.Add("AppUserId", AppUserId);
                parameters.Add("MaterialType", MaterialType);

                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/AppSalesManLoad");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _serviceOrderApp.AppSalesManLoad(ServiceorderId, AppUserId, MaterialType);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ServiceorderId},{AppUserId},{MaterialType}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取业务员关联服务单据数量
        /// </summary>
        /// <param name="AppUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetSalesManServiceOrderCount(int AppUserId)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("AppUserId", AppUserId);

                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetSalesManServiceOrderCount");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _serviceOrderApp.GetSalesManServiceOrderCount(AppUserId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{AppUserId}， 错误：{result.Message}");
            }

            return result;
        }
        #endregion

        #endregion
    }
}
