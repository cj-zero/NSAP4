using DotNetCore.CAP;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using OpenAuth.App.Interface;
using OpenAuth.App.Order.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.NsapBase;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Order
{
    /// <summary>
    /// 销售订单业务
    /// </summary>
    public class ServiceSaleOrderApp : OnlyUnitWorkBaeApp
    {
        private readonly RevelanceManagerApp _revelanceApp;
        private readonly AppServiceOrderLogApp _appServiceOrderLogApp;
        private readonly ServiceOrderLogApp _ServiceOrderLogApp;
        private IOptions<AppSetting> _appConfiguration;
        private ICapPublisher _capBus;
        private readonly ServiceFlowApp _serviceFlowApp;
        public ServiceSaleOrderApp(IUnitWork unitWork, RevelanceManagerApp app, ServiceOrderLogApp serviceOrderLogApp, IAuth auth, AppServiceOrderLogApp appServiceOrderLogApp, IOptions<AppSetting> appConfiguration, ICapPublisher capBus, ServiceOrderLogApp ServiceOrderLogApp, ServiceFlowApp serviceFlowApp) : base(unitWork, auth)
        {
            _appConfiguration = appConfiguration;
            _revelanceApp = app;
            _appServiceOrderLogApp = appServiceOrderLogApp;
            _capBus = capBus;
            _ServiceOrderLogApp = ServiceOrderLogApp;
            _serviceFlowApp = serviceFlowApp;
        }
        /// <summary>
        /// 获取业务员信息
        /// </summary>
        /// <param name="sboId"></param>
        /// <returns></returns>
        public List<SelectOption> GetSalesSelect(int sboId)
        {
            var loginContext = _auth.GetCurrentUser();
            //if (loginContext == null)
            //{
            //    throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            //}
            //业务员Id
            var selectOption = UnitWork.Find<crm_oslp>(null).Select(zw => new SelectOption { Key = zw.SlpCode.ToString(), Option = zw.SlpName }).ToList();
            return selectOption;
        }

        public string Save(AddOrUpdateOrderReq orderReq)
        {
            var userInfo = _auth.GetCurrentUser();
            string funcId = "0";
            string logstring = "";
            string jobname = "";
            string result = "";
            int sboID = GetUserNaspSboID();
            byte[] job_data = ByteExtension.ToSerialize(orderReq.Order);
            if (orderReq.Copy == "1")
            {
                funcId = GetFuncsByUserID("sales/SalesOrder.aspx").ToString();
                logstring = "根据销售报价单下销售订单";
                jobname = "销售订单";
                //  billNo = NSAP.Biz.Sales.BillDelivery.SalesDeliverySave_ORDR(rData, ations, JobId, UserID, int.Parse(funcId), "0", jobname, SboID, IsTemplate);
            }
            else
            {
                string className = "NSAP.B1Api.BOneOQUT";
                funcId = GetFuncsByUserID("sales/SalesQuotation.aspx").ToString();
                logstring = "新建销售报价单";
                jobname = "销售报价单";
                //  billNo = NSAP.Biz.Sales.BillDelivery.SalesDeliverySave_OQUT(rData, ations, JobId, UserID, int.Parse(funcId), "0", jobname, SboID, IsTemplate);
                int FuncID = int.Parse(funcId);
                int UserID = 0;
                if (orderReq.Ations == OrderAtion.Draft)
                {
                    result = OrderWorkflowBuild(jobname, FuncID, UserID, job_data, orderReq.Order.Remark, int.Parse(orderReq.Order.SboId), orderReq.Order.CardCode, orderReq.Order.CardName, (double.Parse(orderReq.Order.DocTotal) > 0 ? double.Parse(orderReq.Order.DocTotal) : 0), int.Parse(orderReq.Order.billBaseType), int.Parse(orderReq.Order.billBaseEntry), "BOneAPI", className);
                }
                else if (orderReq.Ations == OrderAtion.Submit)
                {
                    result = OrderWorkflowBuild(jobname, FuncID, UserID, job_data, orderReq.Order.Remark, int.Parse(orderReq.Order.SboId), orderReq.Order.CardCode, orderReq.Order.CardName, (double.Parse(orderReq.Order.DocTotal) > 0 ? double.Parse(orderReq.Order.DocTotal) : 0), int.Parse(orderReq.Order.billBaseType), int.Parse(orderReq.Order.billBaseEntry), "BOneAPI", className);
                    if (int.Parse(result) > 0)
                    {
                        var par = SaveJobPara(result, orderReq.IsTemplate);
                        if (par)
                        {
                            //string _jobID = result;
                            //if ("0" != NSAP.Data.MyWork.MyWork.WorkflowSubmit(int.Parse(result), UserID, orderReq.Order.Remark, "", 0))
                            //{
                            //    #region 更新商城订单状态
                            //    Entity.Mywork.WfaEshopStatus thisinfo = new Entity.Mywork.WfaEshopStatus();
                            //    thisinfo.job_id = _jobID;
                            //    thisinfo.user_id = UserID.ToString();
                            //    thisinfo.slp_code = Model.SlpCode;
                            //    thisinfo.card_code = Model.CardCode;
                            //    thisinfo.card_name = Model.CardName;
                            //    thisinfo.cur_status = "0";
                            //    thisinfo.order_phase = "0000";
                            //    thisinfo.shipping_phase = "0000";
                            //    thisinfo.complete_phase = "0";
                            //    thisinfo.order_lastdate = DateTime.Now.ToString();
                            //    thisinfo.first_createdate = DateTime.Now.ToString();
                            //    //设置报价单提交
                            //    result = NSAP.Data.Sales.BillDelivery.Eshop_SetOrderStatusFlow(thisinfo, Model.billSalesDetails, Model.U_New_ORDRID);
                            //    #endregion
                            //}
                            //else { result = "0"; }
                        }
                        else { result = "0"; }
                    }
                }
                else if (orderReq.Ations == OrderAtion.Resubmit)
                {
                    //if (jobdata == "1")
                    //{
                    //    if (NSAP.Data.Sales.BillDelivery.UpdateAudit(JobId, job_data, Model.Remark, Model.DocTotal, Model.CardCode, Model.CardName))
                    //    {
                    //        result = NSAP.Data.MyWork.MyWork.WorkflowSubmit(JobId, UserID, Model.Remark, "", 0);
                    //    }
                    //}
                    //else
                    //{
                    //    result = NSAP.Data.MyWork.MyWork.WorkflowSubmit(JobId, UserID, Model.Remark, "", 0);
                    //}
                }
            }
            string log = string.Format("{1}：{0}", result, logstring);
            AddUserOperateLog(log);
            return result;
        }
        /// <summary>
        /// 草稿
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="funcID"></param>
        /// <param name="userID"></param>
        /// <param name="jobdata"></param>
        /// <param name="remarks"></param>
        /// <param name="sboID"></param>
        /// <param name="carCode"></param>
        /// <param name="carName"></param>
        /// <param name="docTotal"></param>
        /// <param name="baseType"></param>
        /// <param name="baseEntry"></param>
        /// <param name="assemblyName"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        private string OrderWorkflowBuild(string jobName, int funcID, int userID, byte[] jobdata, string remarks, int sboID, string carCode, string carName, double docTotal, int baseType, int baseEntry, string assemblyName, string className)
        {
            string code = "";
            if (carCode != "")
            {
                var crmOcrd = UnitWork.FindSingle<crm_ocrd>(zw => zw.sbo_id == sboID && zw.CardCode == carCode);
                if (crmOcrd != null)
                {
                    carName = crmOcrd.CardName;
                }
            }
            //IDataParameter[] parameters =
            //{
            //    Sql.Action.GetParameter("?pJobName",    jobName),
            //    Sql.Action.GetParameter("?pFuncID",     funcID),
            //    Sql.Action.GetParameter("?pUserID",     userID),
            //    Sql.Action.GetParameter("?pJobData",    jobdata),
            //    Sql.Action.GetParameter("?pRemarks",    remarks),
            //    Sql.Action.GetParameter("?pSboID",      sboID),
            //    Sql.Action.GetParameter("?pCarCode",    carCode),
            //    Sql.Action.GetParameter("?pCarName",    carName),
            //    Sql.Action.GetParameter("?pDocTotal",   docTotal),
            //    Sql.Action.GetParameter("?pBaseType",   baseType),
            //    Sql.Action.GetParameter("?pBaseEntry",  baseEntry),
            //    Sql.Action.GetParameter("?pAssemblyName",  assemblyName),
            //    Sql.Action.GetParameter("?pClassName",  className)
            //};
            //string code = Sql.Action.ExecuteScalar(Sql.UTF8ConnectionString, CommandType.StoredProcedure, string.Format("{0}.sp_process_build", Sql.BaseDatabaseName), parameters).ToString();
            return code;
        }
        /// <summary>
        /// 审核（提交）
        /// </summary>
        /// <returns>返回  提交失败 0   提交成功 1   流程完成 2</returns>
        private string OrderWorkflowSubmit(int jobID, int userID, string remarks, string cont, int auditor)
        {
            string code = "";
            //IDataParameter[] parameters =
            //{
            //    Sql.Action.GetParameter("?pJobID",      jobID),
            //    Sql.Action.GetParameter("?pUserID",     userID),
            //    Sql.Action.GetParameter("?pRemarks",    remarks),
            //    Sql.Action.GetParameter("?pCont",       cont),
            //    Sql.Action.GetParameter("?pAuditor",    auditor)
            //};
            //return Sql.Action.ExecuteScalar(Sql.UTF8ConnectionString, CommandType.StoredProcedure, string.Format("{0}.sp_process_submit", Sql.BaseDatabaseName), parameters).ToString();
            return code;
        }
        #region 保存审核参数
        /// <summary>
        /// 保存审核参数
        /// </summary>
        /// <returns></returns>
        public bool SaveJobPara(string jobID, string setNumber)
        {
            int executeRow = 0;
            //string strSql = string.Format("INSERT INTO {0}.wfa_job_para (job_id,para_idx,para_val) VALUES(?job_id,?para_idx,?para_val)", Sql.BaseDatabaseName);
            //IDataParameter[] parameters =
            //{
            //    Sql.Action.GetParameter("?job_id",  jobID),
            //    Sql.Action.GetParameter("?para_idx",  "1"),
            //    Sql.Action.GetParameter("?para_val",  setNumber==""?"1":setNumber)
            //};
            //strSql += string.Format(" ON Duplicate KEY UPDATE ");
            //strSql += string.Format("para_val=VALUES(para_val)");
            //executeRow = Sql.Action.ExecuteNonQuery(Sql.GB2312ConnectionString, CommandType.Text, strSql, parameters) > 0 ? "1" : "0";
            return executeRow > 0;

        }
        #endregion
        /// <summary>
        /// 获取权限Id 
        /// </summary>
        /// <param name="functonUrl"></param>
        /// <returns></returns>
        private int GetFuncsByUserID(string functonUrl)
        {
            int functionId = 0;
            string sql = string.Format("SELECT a.func_id funcID,b.page_url pageUrl,a.auth_map authMap FROM (SELECT a.func_id,a.page_id,b.auth_map FROM {0}.base_func a INNER JOIN (SELECT t.func_id,BIT_OR(t.auth_map) auth_map FROM (SELECT func_id,BIT_OR(auth_map) auth_map FROM {0}.base_role_func WHERE role_id IN (SELECT role_id FROM {0}.base_user_role WHERE user_id=?userID) GROUP BY func_id UNION ALL SELECT func_id,auth_map FROM {0}.base_user_func WHERE user_id=?userID) t GROUP BY t.func_id) b ON a.func_id=b.func_id) AS a INNER JOIN {0}.base_page AS b ON a.page_id=b.page_id", "nsap_base");
            return functionId;
        }
        /// <summary>
        /// 获取NsapId
        /// </summary>
        /// <returns></returns>
        private int GetUserNaspId()
        {
            var loginContext = _auth.GetCurrentUser();
            return loginContext.User.User_Id;
        }
        /// <summary>
        /// 获取NsapId
        /// </summary>
        /// <returns></returns>
        private int GetUserNaspSboID()
        {
            int sboID = 0;
            string sql = "SELECT sbo_id FROM nsap_base.sbo_info WHERE is_curr = 1 AND valid = 1 LIMIT 1";
            return sboID;
        }
        /// <summary>
        /// 操作日志
        /// </summary>
        /// <param name="msg"></param>
        private void AddUserOperateLog(string msg)
        {
            try
            {
                base_user_log log = new base_user_log();
                log.opt_cont = msg;
                log.rec_dt = DateTime.Now;
                log.func_id = 0;
                log.user_id = 1;
                UnitWork.Add<base_user_log>(log);
            }
            catch (Exception ex)
            {
                string errormsg = ex.Message;
            }
        }
    }
}
