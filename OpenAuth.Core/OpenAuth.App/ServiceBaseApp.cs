using DotNetCore.CAP;
using Microsoft.Extensions.Options;
using OpenAuth.App.Interface;
using OpenAuth.App.Order.ModelDto;
using OpenAuth.Repository;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace OpenAuth.App
{
    /// <summary>
    /// 公共业务（基础）
    /// </summary>
    public class ServiceBaseApp : OnlyUnitWorkBaeApp
    {
        private readonly RevelanceManagerApp _revelanceApp;
        private readonly AppServiceOrderLogApp _appServiceOrderLogApp;
        private readonly ServiceOrderLogApp _ServiceOrderLogApp;
        private IOptions<AppSetting> _appConfiguration;
        private ICapPublisher _capBus;
        private readonly ServiceFlowApp _serviceFlowApp;
        public ServiceBaseApp(IUnitWork unitWork, RevelanceManagerApp app, IAuth auth, IOptions<AppSetting> appConfiguration, ICapPublisher capBus, ServiceFlowApp serviceFlowApp) : base(unitWork, auth)
        {
            _appConfiguration = appConfiguration;
            _revelanceApp = app;
            _capBus = capBus;
            _serviceFlowApp = serviceFlowApp;
        }
        /// <summary>
        /// 获取权限Id 
        /// </summary>
        /// <param name="functonUrl"></param>
        /// <returns></returns>
        public int GetFuncsByUserID(string functonUrl, int userId)
        {
            int functionId = 0;
            string sql = string.Format("SELECT a.func_id funcID,b.page_url pageUrl,a.auth_map authMap FROM (SELECT a.func_id,a.page_id,b.auth_map FROM {0}.base_func a INNER JOIN (SELECT t.func_id,BIT_OR(t.auth_map) auth_map FROM (SELECT func_id,BIT_OR(auth_map) auth_map FROM {0}.base_role_func WHERE role_id IN (SELECT role_id FROM {0}.base_user_role WHERE user_id={1}) GROUP BY func_id UNION ALL SELECT func_id,auth_map FROM {0}.base_user_func WHERE user_id={1}) t GROUP BY t.func_id) b ON a.func_id=b.func_id) AS a INNER JOIN {0}.base_page AS b ON a.page_id=b.page_id  WHERE page_url='" + functonUrl + "' ", "nsap_base", userId);
            DataTable dataTable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
            if (dataTable != null)
            {
                functionId = int.Parse(dataTable.Rows[0][0].ToString());
            }
            return functionId;
        }
        /// <summary>
        /// 获取NsapId
        /// </summary>
        /// <returns></returns>
        public int GetUserNaspId()
        {
            var loginContext = _auth.GetCurrentUser();
            return Convert.ToInt32(loginContext.User.User_Id);
        }
        /// <summary>
        /// 验证当前审批人的提交操作是否可以（避免一个工作流多个人同时操作引起的跳过步骤问题）
        /// </summary>
        /// <param name="job_id">工作流ID</param>
        /// <param name="auditid">审批人ID</param>
        /// <returns></returns>
        public bool IsValidSubmit(int job_id, int auditid)
        {
            string auSQL = string.Format(@"select 1 from {0}.wfa_job t0 left outer join {0}.wfa_step t1 on t0.step_id=t1.step_id"
                                       + " LEFT OUTER JOIN {0}.wfa_jump t2 on t2.job_id=t0.job_id and t2.audit_level=t1.audit_level"
                                       + " where t2.state=0 and t0.job_id={1} and t2.user_id={2} ", "nsap_base", job_id, auditid);
            object existau = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, auSQL, CommandType.Text);
            return existau == null ? false : true;
        }
        #region 获取流程任务的流程类型
        /// <summary>
        /// 获取流程任务的流程类型
        /// </summary>
        public string GetJobTypeNm(int JobId)
        {
            string strSql = $"SELECT B.job_type_nm FROM nsap_base.wfa_job A LEFT JOIN nsap_base.wfa_type B ON A.job_type_id=B.job_type_id WHERE a.job_id={JobId}";
            return UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text).ToString();
        }
        #endregion
        /// <summary>
        /// 获取sboId
        /// </summary>
        /// <returns></returns>
        public int GetUserNaspSboID(int UserID)
        {
            int sboID = 0;
            string sql = $@"SELECT (
                    SELECT a.user_id FROM nsap_base.base_user a
                    INNER JOIN nsap_base.base_user_detail b ON a.user_id = b.user_id
                    INNER JOIN nsap_base.base_dep c ON b.dep_id = c.dep_id
                    WHERE a.user_id = {UserID} AND a.valid = 1 AND b.status < 2 AND c.valid = 1
                    ) UserId,
                    (SELECT sbo_id FROM nsap_base.sbo_info WHERE is_curr = 1 AND valid = 1 LIMIT 1
                    )SboID";
            SboModelDto sboModel = UnitWork.ExcuteSql<SboModelDto>(ContextType.NsapBaseDbContext, sql, CommandType.Text, null).FirstOrDefault();
            if (sboModel != null)
            {
                if (sboModel.UserId > 0)
                {
                    sboID = sboModel.SboID;
                }
            }
            return sboID;
        }
        #region 获取销售员所属部门
        /// <summary>
        /// 
        /// </summary>
        /// <param name="slpCode"></param>
        /// <param name="sboId"></param>
        /// <returns></returns>
        public string GetSalesDepname(string slpCode, string sboId)
        {
            string depName = "";
            string sql = string.Format(@"SELECT c.dep_nm FROM
                                            nsap_base.sbo_user a 
                                            LEFT JOIN nsap_base.base_user_detail b ON a.user_id=b.user_id 
                                            LEFT JOIN nsap_base.base_dep c ON c.dep_id=b.dep_id
                                            WHERE a.sale_id={0} AND a.sbo_id ={1} limit 1 ;", slpCode, sboId);
            DataTable dataTable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                depName = dataTable.Rows[0][0].ToString();
            }
            return depName;
        }

        public DataTable GetSboNamePwd(int SboId)
        {
            string strSql = string.Format("SELECT sql_db,sql_name,sql_pswd,sap_name,sap_pswd,sql_conn,is_open FROM {0}.sbo_info WHERE sbo_id={1}", "nsap_base", SboId);
            DataTable dataTable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            return dataTable;
        }
        /// <summary>
        /// 获取自定义字段名
        /// </summary>
        public DataTable GetCustomFields(string TableName)
        {
            string strSql = string.Format("SELECT AliasID,Descr,FieldID,TableID,FieldID,EditSize,EditType FROM {0}.base_cufd", "nsap_bone");
            if (!string.IsNullOrEmpty(TableName))
            {
                strSql += string.Format(" WHERE TableID='{0}'", TableName);
            }
            DataTable dataTable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            return dataTable;
        }
        /// <summary>
        /// 获取部门ID
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public int GetSalesDepID(int userID)
        {
            int DepID = 0;
            string sql = string.Format(@"SELECT c.dep_id FROM nsap_base.base_dep a INNER JOIN nsap_base.base_user_detail b ON a.dep_id=b.dep_id INNER JOIN nsap_base.base_dep c ON c.dep_id=b.dep_id WHERE b.user_id={0}", userID);
            DataTable dataTable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                DepID = int.Parse(dataTable.Rows[0][0].ToString());
            }
            return DepID;
        }
        #endregion
    }
}
