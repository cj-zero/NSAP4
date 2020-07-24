using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using NetOffice.Extensions.Invoker;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;


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

        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁
        public ServiceOrderController(ServiceOrderApp serviceOrderApp, AppServiceOrderLogApp appServiceOrderLogApp)
        {
            _serviceOrderApp = serviceOrderApp;
            _appServiceOrderLogApp = appServiceOrderLogApp;
        }
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
                var order = await _serviceOrderApp.Add(addServiceOrderReq);
                await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                {
                    Title = "提交成功",
                    Details = "已收到您的反馈，正在为您分配客服中。",
                    ServiceOrderId = order.Id
                });
                await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                {
                    Title = "已分配专属客服",
                    Details = "已为您分配专属客服进行处理，如有消息将第一时间通知您，请耐心等候。",
                    ServiceOrderId = order.Id
                });
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

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
            }
            return Task.FromResult(result);
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
                result = await _serviceOrderApp.AppLoad(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
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
                result = await _serviceOrderApp.AppLoadServiceOrderDetails(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
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
                await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                {
                    Title = "客服确认售后信息",
                    Details = "客服确认售后信息，将交至技术员。",
                    ServiceOrderId = request.Id
                });
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// 删除一个工单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<Response> DeleteWorkOrder([FromQuery] int id)
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
            }
            return result;
        }
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
                result = await _serviceOrderApp.GetTechnicianServiceOrder(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
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
                result = await _serviceOrderApp.GetAppTechnicianServiceWorkOrder(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 技术员接单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> TechnicianTakeOrder(TechnicianTakeOrderReq req)
        {
            var result = new Response();

            try
            {
                //用信号量代替锁
                await semaphoreSlim.WaitAsync();
                try
                {
                    await _serviceOrderApp.TechnicianTakeOrder(req);
                }
                finally
                {
                    semaphoreSlim.Release();
                }
                await _appServiceOrderLogApp.BatchAddAsync(new AddOrUpdateAppServiceOrderLogReq
                {
                    Title = "移转至技术员",
                    Details = "以为您分配技术员进行处理，如有消息将第一时间通知您，请耐心等候",
                }, req.ServiceWorkOrderIds);
                
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }



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
            }
            return result;
        }


        /// <summary>
        /// 获取服务单详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<ServiceOrder>> GetDetails(int id)
        {

            var result = new Response<ServiceOrder>();

            try
            {
                result.Result = await _serviceOrderApp.GetDetails(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// 技术员工单池列表
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
                await _serviceOrderApp.BookingWorkOrder(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
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
                await _serviceOrderApp.CheckTheEquipment(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
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
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }



        /// <summary>
        /// 查询可以被派单的技术员列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<List<AllowSendOrderUserResp>>> GetAllowSendOrderUser()
        {
            var result = new Response<List<AllowSendOrderUserResp>>();
            try
            {
                result.Result = await _serviceOrderApp.GetAllowSendOrderUser();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
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
                await _serviceOrderApp.SendOrders(req);
                result.Result = true;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                result.Result = false;
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
            }
            return result;
        }

        /// <summary>
        /// 获取待分配工单服务详情
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
            }
            return result;
        }
    }
}
