using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Sap.Request;
using OpenAuth.App.Sap.Service;
using OpenAuth.App.Serve.Request;
using OpenAuth.Repository.Domain;

namespace NSAP.App.WebApi.Controllers
{
    /// <summary>
    /// 售后接口服务
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    public class ServiceOrderController : Controller
    {
        private readonly ServiceOrderApp _serviceOrderApp;
        private AppServiceOrderLogApp _appServiceOrderLogApp;
        private readonly ServiceEvaluateApp _serviceEvaluateApp;
        private readonly SeviceTechnicianApplyOrdersApp _seviceTechnicianApplyOrdersApp;
        private readonly CompletionReportApp _completionReportApp;
        private readonly SerialNumberApp _serialNumberApp;
        private readonly ProblemTypeApp _problemTypeApp;
        private readonly AttendanceClockApp _attendanceClockApp;
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁

        public ServiceOrderController(ServiceOrderApp serviceOrderApp, AppServiceOrderLogApp appServiceOrderLogApp, ServiceEvaluateApp serviceEvaluateApp, SeviceTechnicianApplyOrdersApp seviceTechnicianApplyOrdersApp, CompletionReportApp completionReportApp, SerialNumberApp serialNumberApp, ProblemTypeApp problemTypeApp, AttendanceClockApp attendanceClockApp)
        {
            _serviceOrderApp = serviceOrderApp;
            _appServiceOrderLogApp = appServiceOrderLogApp;
            _serviceEvaluateApp = serviceEvaluateApp;
            _seviceTechnicianApplyOrdersApp = seviceTechnicianApplyOrdersApp;
            _completionReportApp = completionReportApp;
            _serialNumberApp = serialNumberApp;
            _problemTypeApp = problemTypeApp;
            _attendanceClockApp = attendanceClockApp;
        }


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
                var order = await _serviceOrderApp.Add(addServiceOrderReq);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 获取服务单（设备类型）详情(客户/管理员)
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
        /// 获取设备列表中间页/售后详情页（客户）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetAppCustServiceOrderDetails(int ServiceOrderId)
        {
            var result = new TableData();
            try
            {
                result = await _serviceOrderApp.GetAppCustServiceOrderDetails(ServiceOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// 评价
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AppEvaluateAdd(APPAddServiceEvaluateReq req)
        {

            var result = new Response();
            try
            {
                await _serviceEvaluateApp.AppAdd(req);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 获取评价
        /// </summary>
        [HttpGet]
        public async Task<TableData> AppEvaluateLoad([FromQuery] QueryServiceEvaluateListReq request)
        {
            return await _serviceEvaluateApp.AppLoad(request);
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
        /// 加载客户问题类型列表(APP只显示一级)
        /// </summary>
        [HttpGet]
        public TableData AppProblemTypesLoad()
        {
            return _problemTypeApp.AppLoad();
        }
        #endregion

        #region<<Technician>>
        /// <summary>
        /// 技术员查看服务单单列表（工单池）
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
        /// 获取技术员设备类型列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AppTechnicianLoad(int SapOrderId, int CurrentUserId, string MaterialType)
        {
            var result = new TableData();
            try
            {
                result = await _serviceOrderApp.AppTechnicianLoad(SapOrderId, CurrentUserId, MaterialType);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
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
                result = await _serviceOrderApp.SaveOrderTakeType(request);
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
        ///获取技术员提交/修改的设备信息(APP)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetApplyDevices([FromQuery] GetApplyDevicesReq request)
        {
            var result = new TableData();
            try
            {
                result = await _seviceTechnicianApplyOrdersApp.GetApplyDevices(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
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
        /// 提交核对错误(新)设备信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> ApplyNewOrErrorDevices(ApplyNewOrErrorDevicesReq request)
        {
            var result = new Response();
            try
            {
                await _seviceTechnicianApplyOrdersApp.ApplyNewOrErrorDevices(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
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
                await _serviceOrderApp.UpdateWorkOrderDescription(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
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
                await _serviceOrderApp.SaveWorkOrderSolution(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 填写完工报告单页面需要取到的服务工单信息。
        /// </summary>
        /// <param name="ServiceOrderId">服务单ID</param>
        /// <param name="currentUserId">当前技术员Id</param>
        /// <param name="MaterialType">设备类型</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<CompletionReportDetailsResp>> GetOrderWorkInfoForAdd(int ServiceOrderId, int currentUserId, string MaterialType)
        {
            var result = new Response<CompletionReportDetailsResp>();
            try
            {
                result.Result = await _completionReportApp.GetOrderWorkInfoForAdd(ServiceOrderId, currentUserId, MaterialType);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 提交完工报告
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AddCompletionReport(AddOrUpdateCompletionReportReq obj)
        {
            var result = new Response();
            try
            {
                await _completionReportApp.Add(obj);
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
                result = await _serviceOrderApp.GetAppTechnicianServiceOrderDetails(SapOrderId, CurrentUserId, MaterialType);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
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
                result = await _serviceOrderApp.GetAppTechServiceOrderDetails(ServiceOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 打卡
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Clock(AddOrUpdateAttendanceClockReq req)
        {
            var result = new Response();
            try
            {
                await _attendanceClockApp.Add(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// App技术员查询打卡记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AppGetClockHistory([FromQuery] AppGetClockHistoryReq req)
        {
            var result = new TableData();
            try
            {
                result = await _attendanceClockApp.AppGetClockHistory(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<AttendanceClockDetailsResp>> GetAttendanceClockDetail(string id)
        {
            var result = new Response<AttendanceClockDetailsResp>();
            try
            {
                result.Result = await _attendanceClockApp.GetDetails(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 获取技术员单据数量列表
        /// </summary>
        /// <param name="CurrentUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetTechnicianServiceOrderCount(int CurrentUserId)
        {
            var result = new TableData();
            try
            {
                result = await _serviceOrderApp.GetTechnicianServiceOrderCount(CurrentUserId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
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
                result = await _serviceOrderApp.AppUnConfirmedServiceOrderList(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
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
                result = await _serviceOrderApp.GetAppAdminServiceOrderDetails(SapOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
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
                result = await _serviceOrderApp.GetAppAllowSendOrderUser(req);
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
        /// 主管给技术员派单(转派)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response<bool>> TransferOrders(TransferOrdersReq req)
        {

            var result = new Response<bool>();
            try
            {
                await _serviceOrderApp.TransferOrders(req);

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
                await _serviceOrderApp.CloseWorkOrder(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
        #endregion

        #region<<Common Methods>>
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
                result.Data = await _serviceOrderApp.GetUserCanOrderCount(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

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
            }
            return result;
        }


        /// <summary>
        /// 获取技术员位置信息
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
            }
            return result;
        }

        /// 序列号查询（App 已生成服务单）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> AppSerialNumberGet(QueryAppSerialNumberListReq req)
        {
            var result = new TableData();
            try
            {
                result = await _serialNumberApp.AppGet(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 序列号查询
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> AppSerialNumberFind(QueryAppSerialNumberListReq req)
        {
            var result = new TableData();
            try
            {
                result = await _serialNumberApp.AppFind(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }
        #endregion

    }
}
