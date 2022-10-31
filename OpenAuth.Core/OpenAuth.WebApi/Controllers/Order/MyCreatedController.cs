using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using NSAP.Entity.BillFlow;
using NSAP.Entity.Sales;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Order;
using OpenAuth.App.Order.ModelDto;
using OpenAuth.App.Order.Request;
using OpenAuth.App.Response;
using OpenAuth.App.CommonHelp;
using OpenAuth.Repository.Interface;
using OpenAuth.Repository.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Order
{
    /// <summary>
    /// 我创建的
    /// </summary>
    [Route("api/Order/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Order")]
    public class MyCreatedController : Controller
    {
        private UserDepartMsgHelp _userDepartMsgHelp;
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        public MyCreatedController(IUnitWork UnitWork, UserDepartMsgHelp userDepartMsgHelp, ServiceBaseApp _serviceBaseApp, IAuth _auth, ServiceSaleOrderApp serviceSaleOrderApp)
        {
            this.UnitWork = UnitWork;
            this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            _serviceSaleOrderApp = serviceSaleOrderApp;
            _userDepartMsgHelp = userDepartMsgHelp;
        }
        #region 我创建的
        /// <summary>
        /// 我创建的
        /// </summary>
        [HttpPost]
        [Route("SelectMaterialsInventoryData")]
        public TableData GetICreated(GetICreatedReq model)
        {
            var UserID = _serviceBaseApp.GetUserNaspId();
            var result = new TableData();
            int rowCount = 0;
            //DataTable dt = _serviceSaleOrderApp.GetICreated(model.limit, model.page, model.query, model.sortname, model.sortorder, UserID, model.types, model.Applicator, model.Customer, model.Status, model.BeginDate, model.EndDate, _serviceSaleOrderApp.GetPagePowersByUrl("mywork/AuditAllNew.aspx",UserID).ViewCustom, _serviceSaleOrderApp.GetPagePowersByUrl("mywork/AuditAllNew.aspx",UserID).ViewSales);
            DataTable dt = _serviceSaleOrderApp.GetICreated(out rowCount, model, UserID, true, true);
            List<SaleMyCreates> saleMyCreates = dt.Tolist<SaleMyCreates>();
            foreach (SaleMyCreates item in saleMyCreates)
            {
                item.DeptName = _userDepartMsgHelp.GetUserIdDepart(item.user_id);
            }
            result.Data = dt;
            result.Count = rowCount;
            return result;

        }
        #endregion
        /// <summary>
        /// 销售信息
        /// </summary>
        /// <param name="job_id"></param>
        /// <param name="isAudit"></param>
        /// <param name="docType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDeliverySalesInfoNew")]
        public Response<billDelivery> GetDeliverySalesInfoNew(string job_id, string isAudit, string docType)
        {
            var result = new Response<billDelivery>();

            result.Result = _serviceSaleOrderApp.GetDeliverySalesInfoNew(job_id, isAudit, docType);

            return result;
        }
        /// <summary>
        /// 经理下拉
        /// </summary>
        /// <param name="job_id"></param>
        /// <param name="isAudit"></param>
        /// <param name="docType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DropPopupOwnerCodeNew")]
        public Response<List<CurrencyList>> DropPopupOwnerCodeNew(string job_id, string isAudit, string docType)
        {
            var result = new Response<List<CurrencyList>>();

            result.Result = _serviceSaleOrderApp.DropPopupOwnerCodeNew(job_id, isAudit, docType);

            return result;
        }
        /// <summary>
        /// 销售下拉
        /// </summary>
        /// <param name="job_id"></param>
        /// <param name="isAudit"></param>
        /// <param name="docType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DropPopupSlpCodeNew")]
        public Response<List<CurrencyList>> DropPopupSlpCodeNew(string job_id, string isAudit, string docType)
        {
            var result = new Response<List<CurrencyList>>();

            result.Result = _serviceSaleOrderApp.DropPopupSlpCodeNew(job_id, isAudit, docType);

            return result;
        }
        /// <summary>
        /// 标识下拉
        /// </summary>
        /// <param name="job_id"></param>
        /// <param name="isAudit"></param>
        /// <param name="docType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DropPopupIndicatorNew")]
        public Response<List<CurrencyList>> DropPopupIndicatorNew(string job_id, string isAudit, string docType)
        {
            var result = new Response<List<CurrencyList>>();

            result.Result = _serviceSaleOrderApp.DropPopupIndicatorNew(job_id, isAudit, docType);

            return result;
        }
        /// <summary>
        /// 装运类型
        /// </summary>
        /// <param name="job_id"></param>
        /// <param name="isAudit"></param>
        /// <param name="docType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DropPopupTrnspCodeNew")]
        public Response<List<CurrencyList>> DropPopupTrnspCodeNew(string job_id, string isAudit, string docType)
        {
            var result = new Response<List<CurrencyList>>();

            result.Result = _serviceSaleOrderApp.DropPopupTrnspCodeNew(job_id, isAudit, docType);

            return result;
        }
        /// <summary>
        /// 仓库
        /// </summary>
        /// <param name="job_id"></param>
        /// <param name="isAudit"></param>
        /// <param name="docType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DropPopupWhsCodeNew")]
        public Response<List<CurrencyList>> DropPopupWhsCodeNew(string job_id, string isAudit, string docType)
        {
            var result = new Response<List<CurrencyList>>();

            result.Result = _serviceSaleOrderApp.DropPopupWhsCodeNew(job_id, isAudit, docType);

            return result;
        }
        /// <summary>
        /// 付款条款（新）
        /// </summary>
        /// <param name="job_id"></param>
        /// <param name="isAudit"></param>
        /// <param name="docType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetGroupNumNew")]
        public Response<List<CurrencyList>> GetGroupNumNew(string job_id, string isAudit, string docType)
        {
            var result = new Response<List<CurrencyList>>();

            result.Result = _serviceSaleOrderApp.GetGroupNumNew(job_id, isAudit, docType);

            return result;
        }
        /// <summary>
        /// 货币
        /// </summary>
        /// <param name="job_id"></param>
        /// <param name="isAudit"></param>
        /// <param name="docType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DropPopupDocCurNew")]
        public Response<List<DropPopupDocCurDto>> DropPopupDocCurNew(string job_id, string isAudit, string docType)
        {
            var result = new Response<List<DropPopupDocCurDto>>();

            result.Result = _serviceSaleOrderApp.DropPopupDocCurNew(job_id, isAudit, docType);

            return result;
        }
        /// <summary>
        /// 联系人
        /// </summary>
        /// <param name="job_id"></param>
        /// <param name="isAudit"></param>
        /// <param name="docType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DropPopupCntctPrsnSqlNew")]
        public Response<List<CurrencyList>> DropPopupCntctPrsnSqlNew(string job_id, string isAudit, string docType)
        {
            var result = new Response<List<CurrencyList>>();

            result.Result = _serviceSaleOrderApp.DropPopupCntctPrsnSqlNew(job_id, isAudit, docType);

            return result;
        }
        /// <summary>
        /// 查询指定业务伙伴的科目余额
        /// </summary>
        /// <param name="job_id"></param>
        /// <param name="isAudit"></param>
        /// <param name="docType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SelectBalanceNew")]
        public Response<string> SelectBalanceNew(string job_id, string isAudit, string docType)
        {
            var result = new Response<string>();

            result.Result = _serviceSaleOrderApp.SelectBalanceNew(job_id, isAudit, docType);

            return result;
        }
        /// <summary>
        ///  查询销售员所有客户总的科目余额
        /// </summary>
        /// <param name="job_id"></param>
        /// <param name="isAudit"></param>
        /// <param name="docType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSumBalDueNew")]
        public Response<string> GetSumBalDueNew(string job_id, string isAudit, string docType)
        {
            var result = new Response<string>();

            result.Result = _serviceSaleOrderApp.GetSumBalDueNew(job_id);

            return result;
        }
        /// <summary>
        /// 船舶代码
        /// </summary>
        /// <param name="job_id"></param>
        /// <param name="isAudit"></param>
        /// <param name="docType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAddressNew")]
        public Response<string> GetAddressNew(string job_id, string isAudit, string docType)
        {
            var result = new Response<string>();

            result.Result = _serviceSaleOrderApp.GetAddressNew(job_id, isAudit, docType);

            return result;
        }
        /// <summary>
        /// 自定义字段
        /// </summary>
        /// <param name="SlpCode"></param>
        /// <param name="CardCode"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSumBalDue")]
        public Response<string> GetSumBalDue(string SlpCode, string CardCode, string type)
        {
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);

            var result = new Response<string>();

            result.Result = _serviceSaleOrderApp.GetSumBalDue(SlpCode, CardCode, type, SboID);

            return result;

        }
        /// <summary>
        /// 查询自定义下拉列表字段
        /// </summary>
        /// <param name="TableID"></param>
        /// <param name="AliasID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetQUTCustomeValueByFN")]
        public TableData GetQUTCustomeValueByFN(string TableID, string AliasID)
        {

            var result = new TableData();
            //客户端设置的自定义字段U_ZS
            //string TableID = "QUT1";string AliasID = "ZS";
            string saptabname = "";
            switch (TableID)
            {
                case "sale_qut1":
                    saptabname = "QUT1";
                    break;
                case "sale_rdr1":
                    saptabname = "RDR1";
                    break;
                default:
                    saptabname = TableID.Split('_')[1];
                    break;
            }
            result.Data = _serviceSaleOrderApp.SQLGetCustomeValueByFN(saptabname, AliasID);
            return result;

        }
        #region 仓库
        /// <summary>
        /// 仓库
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("DropPopupWhsCode")]
        public Response<List<DropPopupDocCurDto>> DropPopupWhsCode()
        {
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            var result = new Response<List<DropPopupDocCurDto>>();
            try
            {
                result.Result = _serviceSaleOrderApp.DropPopupWhsCode(SboID);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;

        }
        /// <summary>
        /// 根据付款条件获取相关信息
        /// </summary>
        [HttpGet]
        [Route("GetPayMentInfo")]
        public TableData GetPayMentInfo(string GroupNum)
        {
            var result = new TableData();

            result.Data = _serviceSaleOrderApp.GetPayMentInfo(GroupNum);

            return result;
        }
        /// <summary>
        /// 获取新的自定义字段
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCustomFieldsNew")]
        public Response<List<CustomFieldsNewDto>> GetCustomFieldsNew(string TableName)
        {
            var result = new Response<List<CustomFieldsNewDto>>();

            result.Result = _serviceSaleOrderApp.GetCustomFieldsNew(TableName);
            return result;


        }
        #endregion
        /// <summary>
        /// 审核中
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetFlowChartByJobID")]
        public Response<FlowChart> GetFlowChartByJobID(string jobID)
        {
            var result = new Response<FlowChart>();

            result.Result = _serviceSaleOrderApp.GetFlowChartByJobID(jobID);


            return result;


        }
        #region 流程任务 - 撤回(审核中的)
        /// <summary>
        /// 流程任务 - 撤回(审核中的)
        /// </summary>
        [HttpGet]
        [Route("BackReceipt")]
        public Response<bool> BackReceipt(string jobId, string urlType = "sales/SalesQuotation.aspx")
        {
            var result = new Response<bool>();
            var UserID = _serviceBaseApp.GetUserNaspId();

            result.Result = _serviceSaleOrderApp.ICreatedBack(jobId, UserID.ToString(), urlType);


            return result;
        }
        #region 获取流程任务当前状态
        /// <summary>
        /// 获取流程任务当前状态 1才可请求撤回
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetJobStateById")]
        public Response<string> GetJobStateById(string JobId)
        {
            var result = new Response<string>();
            result.Result = _serviceSaleOrderApp.GetJobStateById(JobId);
            return result;
        }
        #endregion
        #endregion
        #region 删除(草稿&驳回)
        /// <summary>
        /// 删除(草稿/驳回)
        /// </summary>
        [HttpGet]
        [Route("Delete")]
        public Response<bool> Delete(string keyIds)
        {
            var result = new Response<bool>();



            result.Result = _serviceSaleOrderApp.ICreatedDelete(keyIds);


            return result;
        }

        #endregion

        #region 获取当前用户角色姓名
        /// <summary>
        /// 获取当前用户角色姓名
        /// </summary>
        [HttpGet]
        [Route("GetuserInfo")]
        public Response<GetUserInfoDto> GetuserInfo()
        {
            var result = new Response<GetUserInfoDto>();

            var loginContext = _auth.GetCurrentUser();
            var data = new GetUserInfoDto();
            data.userid = (int)loginContext.User.User_Id;
            data.username = loginContext.User.Name;
            data.rolename = new List<string>();
            foreach (var item in loginContext.Roles)
            {
                data.rolename.Add(item.Name);
            }
            result.Result = data;
            return result;
        }

        #endregion

    }
}
