using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenAuth.App;
using OpenAuth.App.Order;
using OpenAuth.App.Order.ModelDto;
using OpenAuth.App.Order.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository;
using OpenAuth.Repository.Extensions;
using OpenAuth.Repository.Interface;
using OpenAuth.App.CommonHelp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Order
{
    /// <summary>
    /// 订单我创建审批工作台
    /// </summary>
    [Route("api/OrderWorkbench/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Order")]
    public class OrderWorkbenchController : Controller
    {
        private UserDepartMsgHelp _userDepartMsgHelp;
        OrderWorkbenchApp _orderWorkbenchApp;
        ServiceBaseApp _serviceBaseApp;
        IUnitWork _unitWork;
        public OrderWorkbenchController(UserDepartMsgHelp userDepartMsgHelp,OrderWorkbenchApp orderWorkbenchApp, ServiceBaseApp serviceBaseApp, IUnitWork unitWork)
        {
            _orderWorkbenchApp = orderWorkbenchApp;
            _serviceBaseApp = serviceBaseApp;
            this._unitWork = unitWork;
            _userDepartMsgHelp = userDepartMsgHelp;
        }
        /// <summary>
        /// 提交给我的
        /// </summary>
        /// <param name="orderSubmtToMeReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetSubmtToMe")]
        public TableData GetSubmtToMe(OrderSubmtToMeReq orderSubmtToMeReq)
        {
            var UserID = _serviceBaseApp.GetUserNaspId();
            var result = new TableData();
            int rowCount = 0;
            var powerList = _unitWork.ExcuteSql<PowerDto>(ContextType.NsapBaseDbContext, $@"SELECT a.func_id funcID,b.page_url pageUrl,a.auth_map authMap FROM (SELECT a.func_id,a.page_id,b.auth_map FROM nsap_base.base_func a INNER JOIN (SELECT t.func_id,BIT_OR(t.auth_map) auth_map FROM (SELECT func_id,BIT_OR(auth_map) auth_map FROM nsap_base.base_role_func WHERE role_id IN (SELECT role_id FROM nsap_base.base_user_role WHERE user_id={UserID}) GROUP BY func_id UNION ALL SELECT func_id,auth_map FROM nsap_base.base_user_func WHERE user_id={UserID}) t GROUP BY t.func_id) b ON a.func_id=b.func_id) AS a INNER JOIN nsap_base.base_page AS b ON a.page_id=b.page_id", CommandType.Text, null);
            bool viewSales = false;
            bool viewCustom = false;
            var power = powerList.FirstOrDefault(s => s.PageUrl == "mywork/AuditToMeNew.aspx");
            if (power != null)
            {
                Powers powers = new Powers(power.AuthMap);
                viewSales = powers.ViewSales;
                viewCustom = true;
            }
            DataTable dt = _orderWorkbenchApp.GetSubmtToMe(orderSubmtToMeReq.limit, orderSubmtToMeReq.page, orderSubmtToMeReq.query, orderSubmtToMeReq.sortname, orderSubmtToMeReq.sortorder, UserID, orderSubmtToMeReq.types, orderSubmtToMeReq.Applicator, orderSubmtToMeReq.Customer, orderSubmtToMeReq.Status, orderSubmtToMeReq.BeginDate, orderSubmtToMeReq.EndDate, ViewCustom: viewCustom, ViewSales: viewSales, out rowCount);

            //List<SaleDeptSubToMe> saleDeptSubToMes = dt.Tolist<SaleDeptSubToMe>();
            //foreach (SaleDeptSubToMe item in saleDeptSubToMes)
            //{
            //    item.DeptName = _userDepartMsgHelp.GetUserIdDepart(item.user_id);
            //}
            result.Data = dt;
            //result.Data = saleDeptSubToMes;
            result.Count = rowCount;
            return result;
        }
        /// <summary>
        /// 我处理的
        /// </summary>
        /// <param name="orderSubmtToMeReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetMyHandle")]
        public TableData GetMyHandle(OrderSubmtToMeReq orderSubmtToMeReq)
        {
            var UserID = _serviceBaseApp.GetUserNaspId();
            var result = new TableData();
            int rowCount = 0;
            var powerList = _unitWork.ExcuteSql<PowerDto>(ContextType.NsapBaseDbContext, $@"SELECT a.func_id funcID,b.page_url pageUrl,a.auth_map authMap FROM (SELECT a.func_id,a.page_id,b.auth_map FROM nsap_base.base_func a INNER JOIN (SELECT t.func_id,BIT_OR(t.auth_map) auth_map FROM (SELECT func_id,BIT_OR(auth_map) auth_map FROM nsap_base.base_role_func WHERE role_id IN (SELECT role_id FROM nsap_base.base_user_role WHERE user_id={UserID}) GROUP BY func_id UNION ALL SELECT func_id,auth_map FROM nsap_base.base_user_func WHERE user_id={UserID}) t GROUP BY t.func_id) b ON a.func_id=b.func_id) AS a INNER JOIN nsap_base.base_page AS b ON a.page_id=b.page_id", CommandType.Text, null);
            bool viewSales = false;
            bool viewCustom = false;
            var power = powerList.FirstOrDefault(s => s.PageUrl == "mywork/AuditIDealNew.aspx");
            if (power != null)
            {
                Powers powers = new Powers(power.AuthMap);
                viewSales = powers.ViewSales;
                viewCustom = powers.ViewCustom;
            }
            DataTable dt = _orderWorkbenchApp.GetIDeal(orderSubmtToMeReq.limit, orderSubmtToMeReq.page, orderSubmtToMeReq.query, orderSubmtToMeReq.sortname, orderSubmtToMeReq.sortorder, UserID, orderSubmtToMeReq.Applicator, orderSubmtToMeReq.Customer, orderSubmtToMeReq.Status, orderSubmtToMeReq.BeginDate, orderSubmtToMeReq.EndDate, ViewCustom: viewCustom, ViewSales: viewSales);
            result.Data = dt;
            result.Count = rowCount;
            return result;
        }
        /// <summary>
        /// 审批记录
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetApprovalRecord")]
        public Response<List<FlowChartDto>> GetApprovalRecord(string jobID, string type)
        {
            var result = new Response<List<FlowChartDto>>();
            result.Result = _orderWorkbenchApp.GetApprovalRecord(jobID, type);
            return result;
        }
        #region 判断是否是最后一步
        /// <summary>
        /// 判断是否是最后一步
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("IsLastStep")]
        public async Task<Response<string>> IsLastStep(string jobId)
        {
            var result = new Response<string>();
            try
            {
                result.Result = await _orderWorkbenchApp.IsLastStep(jobId);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
                result.Code = 500;
                Log.Logger.Error($"报错地址:{Request.Path},请求方式：{"Post"},参数:{JsonConvert.SerializeObject(jobId)},异常描述：{e.Message},堆栈信息：{e.StackTrace}");

            }
            return result;
        }

        #endregion
        #region 判断物料是否活跃
        /// <summary>
        /// 判断物料是否活跃
        /// </summary>
        /// <param name="isActiveNewReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("IsActiveNew")]
        public async Task<TableData> IsActiveNew(IsActiveNewReq isActiveNewReq)
        {
            var result = new TableData();
            try
            {
                result.Data = await _orderWorkbenchApp.IsActive(isActiveNewReq);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
                result.Code = 500;
                Log.Logger.Error($"报错地址:{Request.Path},请求方式：{"Post"},参数:{JsonConvert.SerializeObject(isActiveNewReq)},异常描述：{e.Message},堆栈信息：{e.StackTrace}");

            }
            return result;
        }
        #endregion
        #region 查询交货序列号
        /// <summary>
        /// 查询交货序列号
        /// </summary>
        [HttpPost]
        [Route("SerialDeliveryNew")]
        public async Task<TableData> SerialDeliveryNew(SerialDeliveryNewReq serialDeliveryNewReq)
        {
            var result = new TableData();
            try
            {
                result.Data = await _orderWorkbenchApp.SerialDelivery(serialDeliveryNewReq);

            }
            catch (Exception e)
            {
                result.Message = e.Message;
                result.Code = 500;
                Log.Logger.Error($"报错地址:{Request.Path},请求方式：{"Post"},参数:{JsonConvert.SerializeObject(serialDeliveryNewReq)},异常描述：{e.Message},堆栈信息：{e.StackTrace}");

            }
            return result;
        }

        #endregion
        #region 根据物料获取序列号

        /// <summary>
        /// 根据物料获取序列号
        /// </summary>
        /// <param name="getDisrNumberReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetDisrNumber")]
        public async Task<TableData> GetDisrNumber(GetDisrNumberReq getDisrNumberReq)
        {
            var result = new TableData();
            try
            {
                result = await _orderWorkbenchApp.GetDisrNumber(getDisrNumberReq);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
                result.Code = 500;
                Log.Logger.Error($"报错地址:{Request.Path},请求方式：{"Post"},参数:{JsonConvert.SerializeObject(getDisrNumberReq)},异常描述：{e.Message},堆栈信息：{e.StackTrace}");

            }
            return result;
        }
        #endregion

    }
}
