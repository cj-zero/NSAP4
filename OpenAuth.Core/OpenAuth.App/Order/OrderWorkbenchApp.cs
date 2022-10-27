extern alias MySqlConnectorAlias;
using MySql.Data.MySqlClient;
using Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using NSAP.Entity.BillFlow;
using OpenAuth.App.Interface;
using OpenAuth.App.Order.ModelDto;
using OpenAuth.Repository;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using OpenAuth.App.Response;
using NSAP.Entity.Sales;
using OpenAuth.App.Order.Request;

namespace OpenAuth.App.Order
{
    /// <summary>
    /// 订单工作台
    /// </summary>
    public class OrderWorkbenchApp : OnlyUnitWorkBaeApp
    {
        ServiceSaleOrderApp _serviceSaleOrderApp;
        ServiceBaseApp _serviceBaseApp;
        ILogger<OrderWorkbenchApp> _logger;
        public OrderWorkbenchApp(IUnitWork unitWork, ILogger<OrderWorkbenchApp> logger, ServiceBaseApp serviceBaseApp, IAuth auth, ServiceSaleOrderApp serviceSaleOrderApp) : base(unitWork, auth)
        {
            _logger = logger;
            _serviceSaleOrderApp = serviceSaleOrderApp;
            _serviceBaseApp = serviceBaseApp;
        }
        #region 提交给我的
        /// <summary>
        /// 提交给我的
        /// </summary>
        public DataTable GetSubmtToMe(int pageSize, int pageIndex, string filterQuery, string sortname, string sortorder, int user_id, string types, string Applicator, string Customer, string Status, string BeginDate, string EndDate, bool ViewCustom, bool ViewSales, out int rowCount)
        {
            string sortString = string.Empty;
            string filterString = string.Empty;
            string line = string.Empty;
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());

            #region 搜索条件
            if (types.Replace(" ", "") != "")
            {
                string[] typeArr = types.Split('☉');
                if (typeArr.Length > 0)
                {
                    filterString += string.Format(" ( ");
                    for (int i = 0; i < typeArr.Length; i++)
                    {
                        if (i == 0)
                        {
                            filterString += string.Format(" a.job_type_id={0} ", typeArr[i].FilterSQL().Trim());
                        }
                        else
                        {
                            filterString += string.Format(" OR a.job_type_id={0} ", typeArr[i].FilterSQL().Trim());
                        }
                    }
                    filterString += string.Format(" ) AND ");
                }
            }

            if (!string.IsNullOrWhiteSpace(Applicator))
            {
                string[] num;
                num = Applicator.Split(',');
                string para = "";
                foreach (string c in num)
                {
                    para += "'" + c + "'" + ",";
                }
                para = "(" + para.TrimEnd(',') + ")";
                filterString += string.Format(" b.user_nm IN {0} AND ", para);
            }
            if (!string.IsNullOrWhiteSpace(Customer))
            {
                filterString += string.Format(" (a.card_code LIKE '%{0}%' OR a.card_name LIKE '%{0}%') AND ", Customer);
            }
            if (!string.IsNullOrWhiteSpace(Status))
            {
                filterString += string.Format(" a.job_state = {0} AND ", int.Parse(Status));
            }
            //filterString += " c.job_type_nm='销售报价单' AND  ";
            if (!string.IsNullOrWhiteSpace(BeginDate))
            {
                filterString += string.Format(" DATE_FORMAT(a.upd_dt,'%Y/%m/%d') BETWEEN '{0}' AND '{1}' AND ", BeginDate, EndDate);
            }
            if (!string.IsNullOrEmpty(filterQuery))
            {
                string[] fields = filterQuery.Split('`');
                string[] p = fields[0].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format(" a.job_id LIKE '%{0}%' AND ", p[1].FilterSQL().Trim());
                }
                p = fields[1].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("c.job_type_nm LIKE '%{0}%' AND ", p[1].FilterSQL().Trim());
                }
                p = fields[2].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("a.job_state = '{0}' AND ", p[1].FilterSQL().Trim());
                }
                p = fields[3].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("a.job_nm LIKE '%{0}%' AND ", p[1].FilterWildCard().FilterSQL().Trim());
                }
                p = fields[4].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("(a.card_code LIKE '%{0}%' OR a.card_name LIKE '%{0}%') AND ", p[1].FilterWildCard().FilterSQL().Trim());
                }
                p = fields[5].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("b.user_nm LIKE '%{0}%' AND ", p[1].FilterWildCard().FilterSQL().Trim());
                }
                p = fields[6].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("a.remarks LIKE '%{0}%' AND ", p[1].FilterWildCard().FilterSQL().Trim());
                }
                p = fields[7].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("a.base_entry LIKE '%{0}%' AND ", p[1].FilterWildCard().FilterSQL().Trim());
                }
            }
            #endregion
            if (user_id > 0)
            {
                filterString += string.Format("(a.job_state=1 or a.job_state= 4) AND a.job_id IN(SELECT a.job_id FROM {0}.wfa_jump a INNER JOIN {0}.wfa_job b ON a.job_id=b.job_id INNER JOIN {0}.wfa_step c ON b.step_id=c.step_id WHERE a.user_id={1} AND a.state=0 AND a.audit_level=c.audit_level) AND ", "nsap_base", user_id);
            }
            if (!string.IsNullOrEmpty(filterString))
                filterString = filterString.Substring(0, filterString.Length - 5);
            return GetSubmtToMe(out rowCount, pageSize, pageIndex, filterString, sortString, ViewCustom, ViewSales);
        }
        /// <summary>
        /// 提交给我的
        /// </summary>
        public DataTable GetSubmtToMe(out int rowCounts, int pageSize, int pageIndex, string filterQuery, string orderName, bool ViewCustom, bool ViewSales)
        {
            StringBuilder tableName = new StringBuilder();
            StringBuilder filedName = new StringBuilder();
            filedName.Append("ROW_NUMBER() OVER (ORDER BY a.job_id) RowNum,'', a.job_id,c.job_type_nm,a.job_nm,b.user_nm,a.user_id,a.job_state,a.upd_dt,a.remarks,c.job_type_id,a.step_id,a.card_code,");
            filedName.Append("CASE WHEN a.card_name IS NULL THEN NULL WHEN a.card_name = '' THEN '' WHEN a.card_name IS NOT NULL AND a.card_name <> '' AND " + ViewCustom + " THEN a.card_name ELSE '******' END AS CardName,");
            filedName.Append("CASE WHEN a.doc_total IS NULL THEN NULL WHEN a.doc_total = '' THEN '' WHEN a.doc_total IS NOT NULL AND a.doc_total <> '' AND " + ViewSales + " THEN a.doc_total ELSE '******' END AS DocTotal,");
            filedName.Append("a.base_type,a.base_entry,d.step_nm,a.sbo_id,f.page_url,d.audit_level,g.sbo_nm");
            tableName.AppendFormat("{0}.wfa_job a", "nsap_base");
            tableName.AppendFormat(" LEFT JOIN {0}.base_user b ON a.user_id=b.user_id ", "nsap_base");
            tableName.AppendFormat(" LEFT JOIN {0}.wfa_type c ON a.job_type_id=c.job_type_id ", "nsap_base");
            tableName.AppendFormat(" LEFT JOIN {0}.wfa_step d ON a.step_id=d.step_id ", "nsap_base");
            tableName.AppendFormat(" LEFT JOIN {0}.base_func e ON c.job_type_id=e.job_type_id ", "nsap_base");
            tableName.AppendFormat(" LEFT JOIN {0}.base_page f ON e.page_id =f.page_id ", "nsap_base");
            tableName.AppendFormat(" LEFT JOIN {0}.sbo_info g ON a.sbo_id =g.sbo_id ", "nsap_base");
            filterQuery += " AND c.has_flow = 1 AND a.job_state != -1 GROUP BY a.job_id";
            DataTable tempTab = _serviceSaleOrderApp.SelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), pageSize, pageIndex, orderName, filterQuery, out rowCounts);
            //tempTab.Columns.Add(new DataColumn("rowIdx"));
            //int startidx = (pageIndex - 1) * pageSize;
            //foreach (DataRow thisrow in tempTab.Rows)
            //{
            //    startidx++;
            //    thisrow["rowIdx"] = startidx;
            //}
            return tempTab;
        }

        #endregion

        #region 我处理过的
        /// <summary>
        /// 我处理过的
        /// </summary>
        public DataTable GetIDeal(int pageSize, int pageIndex, string filterQuery, string sortname, string sortorder, int user_id, string Applicator, string Customer, string Status, string BeginDate, string EndDate, bool ViewCustom, bool ViewSales)
        {
            int rowCount = 0;
            string sortString = string.Empty;
            string filterString = string.Empty;
            string line = string.Empty;
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());

            #region 搜索条件
            //if (types.Replace(" ", "") != "")
            //{
            //    string[] typeArr = types.Split('☉');
            //    if (typeArr.Length > 0)
            //    {
            //        filterString += string.Format(" ( ");
            //        for (int i = 0; i < typeArr.Length; i++)
            //        {
            //            if (i == 0)
            //            {
            //                filterString += string.Format(" a.job_type_id={0} ", typeArr[i].FilterSQL().Trim());
            //            }
            //            else
            //            {
            //                filterString += string.Format(" OR a.job_type_id={0} ", typeArr[i].FilterSQL().Trim());
            //            }
            //        }
            //        filterString += string.Format(" ) AND ");
            //    }
            //}
            filterString += " c.job_type_nm='销售报价单' AND";
            if (!string.IsNullOrWhiteSpace(Applicator))
            {
                string[] num;
                num = Applicator.Split(',');
                string para = "";
                foreach (string c in num)
                {
                    para += "'" + c + "'" + ",";
                }
                para = "(" + para.TrimEnd(',') + ")";
                filterString += string.Format(" b.user_nm IN {0} AND ", para);
            }
            if (!string.IsNullOrWhiteSpace(Customer))
            {
                filterString += string.Format(" (a.card_code LIKE '%{0}%' OR a.card_name LIKE '%{0}%') AND ", Customer);
            }
            if (!string.IsNullOrWhiteSpace(Status))
            {
                filterString += string.Format(" a.job_state = {0} AND ", int.Parse(Status));
            }
            if (!string.IsNullOrWhiteSpace(BeginDate))
            {
                filterString += string.Format(" DATE_FORMAT(a.upd_dt,'%Y/%m/%d') BETWEEN '{0}' AND '{1}' AND ", BeginDate, EndDate);
            }
            if (!string.IsNullOrEmpty(filterQuery))
            {
                string[] fields = filterQuery.Split('`');
                string[] p = fields[0].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format(" a.job_id LIKE '%{0}%' AND ", p[1].FilterSQL().Trim());
                }
                p = fields[1].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("c.job_type_nm LIKE '%{0}%' AND ", p[1].FilterSQL().Trim());
                }
                p = fields[2].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("a.job_state = {0} AND ", p[1].FilterSQL().Trim());
                }
                p = fields[3].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("a.job_nm LIKE '%{0}%' AND ", p[1].FilterWildCard().FilterSQL().Trim());
                }
                p = fields[4].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("(a.card_code LIKE '%{0}%' OR a.card_name LIKE '%{0}%') AND ", p[1].FilterWildCard().FilterSQL().Trim());
                }
                p = fields[5].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("b.user_nm LIKE '%{0}%' AND ", p[1].FilterWildCard().FilterSQL().Trim());
                }
                p = fields[6].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("a.remarks LIKE '%{0}%' AND ", p[1].FilterWildCard().FilterSQL().Trim());
                }
                p = fields[7].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("a.base_entry LIKE '%{0}%' AND ", p[1].FilterWildCard().FilterSQL().Trim());
                }
            }
            #endregion
            if (user_id > 0)
            {
                filterString += string.Format("a.job_state!=-1 AND c.has_flow=1 AND a.job_id IN(SELECT job_id FROM {0}.wfa_log WHERE user_id={1}) AND ", "nsap_base", user_id);
            }
            if (!string.IsNullOrEmpty(filterString))
                filterString = filterString.Substring(0, filterString.Length - 5);
            return GetIDeal(out rowCount, pageSize, pageIndex, filterString, sortString, ViewCustom, ViewSales);
        }
        /// <summary>
        /// 我处理过的
        /// </summary>
        public DataTable GetIDeal(out int rowCounts, int pageSize, int pageIndex, string filterQuery, string orderName, bool ViewCustom, bool ViewSales)
        {
            StringBuilder tableName = new StringBuilder();
            StringBuilder filedName = new StringBuilder();
            filedName.Append("a.job_id,c.job_type_nm,a.job_nm,b.user_nm,a.job_state,a.upd_dt,a.remarks,c.job_type_id,a.card_code,");
            filedName.Append("CASE WHEN a.card_name IS NULL THEN NULL WHEN a.card_name = '' THEN '' WHEN a.card_name IS NOT NULL AND a.card_name <> '' AND " + ViewCustom + " THEN a.card_name ELSE '******' END AS CardName,");
            filedName.Append("CASE WHEN a.doc_total IS NULL THEN NULL WHEN a.doc_total = '' THEN '' WHEN a.doc_total IS NOT NULL AND a.doc_total <> '' AND " + ViewSales + " THEN a.doc_total ELSE '******' END AS DocTotal,");
            filedName.Append("a.base_type,a.base_entry,d.step_nm,a.sbo_id,'' as page_url,a.sbo_itf_return,g.sbo_nm");
            tableName.AppendFormat("{0}.wfa_job a", "nsap_base");
            tableName.AppendFormat(" LEFT JOIN {0}.base_user b ON a.user_id=b.user_id ", "nsap_base");
            tableName.AppendFormat(" LEFT JOIN {0}.wfa_type c ON a.job_type_id=c.job_type_id ", "nsap_base");
            tableName.AppendFormat(" LEFT JOIN {0}.wfa_step d ON a.step_id=d.step_id ", "nsap_base");
            tableName.AppendFormat(" LEFT JOIN {0}.sbo_info g ON a.sbo_id =g.sbo_id ", "nsap_base");
            filterQuery += " AND c.has_flow = 1 ";
            return _serviceSaleOrderApp.SelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), pageSize, pageIndex, orderName, filterQuery, out rowCounts);
        }
        #endregion
        /// <summary>
        /// 审核操作记录
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public List<FlowChartDto> GetApprovalRecord(string jobID, string type)
        {
            var result = new List<FlowChartDto>();
            FlowChart flowChart = _serviceSaleOrderApp.GetFlowChartByJobID(jobID);
            var dt = new DateTime();
            int i = 0;
            foreach (var caps in flowChart.Steps)
            {
                var scon = new FlowChartDto();
                scon.stepName = caps.StepName;
                if (i == 0)
                {
                    if (type == "sale_oqut")
                    {
                        scon.stepName = "销售报价单创建";

                    }
                    if (type == "sale_ordr")
                    {
                        scon.stepName = "销售订单单创建";

                    }
                }
                if (caps.RealAuditors != null && caps.RealAuditors.Count > 0)
                {
                    foreach (var doin in caps.RealAuditors)
                    {
                        scon.realAuditorsName = doin.Name;
                        scon.realAuditorsComment = doin.Comment;
                        DateTime checkTime;
                        DateTime.TryParse(doin.CheckTime, out checkTime);
                        scon.realAuditorsCheckTime = checkTime.ToString("yyyy.MM.dd hh:mm:ss");
                        scon.realAuditorsResult = doin.Result;
                        var dts = new DateTime();
                        dts = (DateTime)doin.CheckTime.ToDateTime();
                        if (i != 0)
                        {
                            var subTime = dts.Subtract(dt.ToDateTime());
                            //scon.Audittime = $"{subTime.Days}天{subTime.Hours}小时{subTime.Minutes}分钟{subTime.Seconds}秒{subTime.Milliseconds}毫秒";
                            scon.Audittime = $"{subTime.Days}天{subTime.Hours}小时{subTime.Minutes}分钟";
                        }
                        dt = (DateTime)doin.CheckTime.ToDateTime();
                        i++;
                    }
                }
                result.Add(scon);
            }
            return result;
        }


        #region 判断是否是最后一步
        public async Task<string> IsLastStep(string jobID)
        {
            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> sqlParameters = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pJobID",  jobID)
            };
            return UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, string.Format("{0}.sp_process_laststep", "nsap_base"), CommandType.StoredProcedure, sqlParameters).ToString();
        }
        #endregion
        #region 判断物料是否是活跃的
        /// <summary>
        /// 判断物料是否是活跃的
        /// </summary>
        /// <returns></returns>
        public async Task<DataTable> IsActive(IsActiveNewReq isActiveNewReq)
        {
            DataTable dt = _serviceSaleOrderApp.GetSboNamePwd(isActiveNewReq.SboId);
            string dRowData = string.Empty; string isOpen = "0";
            string sqlconn = ""; string sboname = "";
            if (dt.Rows.Count > 0)
            {
                sqlconn = dt.Rows[0][5].ToString();
                isOpen = dt.Rows[0][6].ToString();
                sboname = dt.Rows[0][0].ToString();
            }
            DataTable itemCodeList = new DataTable();
            itemCodeList.Columns.Add("itemCode", typeof(string));
            itemCodeList.Columns.Add("Advanced", typeof(int));
            itemCodeList.Columns.Add("Activity", typeof(int));
            itemCodeList.Columns.Add("Inactivity", typeof(int));
            itemCodeList.Columns.Add("Advanced2", typeof(int));
            foreach (SrialNumbers i in isActiveNewReq.srialNumbers)
            {
                if (i.ItemCode.Contains("'")) { i.ItemCode = i.ItemCode.Replace("'", "''"); }
                IsActive(i.ItemCode, isActiveNewReq.DocDate, isOpen, isActiveNewReq.SboId, sqlconn, sboname, itemCodeList);
            }
            return itemCodeList;
        }
        #endregion
        #region 判断物料是否活跃
        /// <summary>
        /// 判断物料是否是活跃的
        /// </summary>
        public void IsActive(string ItemCode, string DocDate, string IsOpen, int SboId, string sqlconn, string sboname, DataTable itemCodeList)
        {
            DataTable dTable = new DataTable();
            StringBuilder strSql = new StringBuilder();
            if (IsOpen == "1")
            {
                if (string.IsNullOrEmpty(sboname)) { sboname = ""; } else { sboname = sboname + ".dbo."; }
                strSql.Append("SELECT a.ItemCode,a.Advanced,b.Activity,c.Inactivity,d.Advanced2 FROM");
                strSql.AppendFormat("(SELECT '{0}' AS ItemCode,ISNULL(COUNT(*),0) AS Advanced FROM " + sboname + "OITM WHERE ItemCode='{0}' AND validFor ='Y' AND frozenFor ='Y' AND ISNULL(validFrom,'{1}') <= '{1}' AND '{1}' <= ISNULL(validTo,'{1}')) AS a", ItemCode, DocDate);
                strSql.Append(" INNER JOIN");
                strSql.AppendFormat("(SELECT '{0}' AS ItemCode, ISNULL(COUNT(*),0) AS Activity FROM " + sboname + "OITM WHERE ItemCode='{0}' AND validFor ='Y' AND frozenFor ='N' AND (ISNULL(validFrom,'{1}') <= '{1}' AND '{1}'<= ISNULL(validTo,'{1}'))) AS b", ItemCode, DocDate);
                strSql.Append(" ON a.ItemCode=b.ItemCode");
                strSql.Append(" INNER JOIN");
                strSql.AppendFormat("(SELECT '{0}' AS ItemCode, ISNULL(COUNT(*),0) AS Inactivity FROM " + sboname + "OITM WHERE ItemCode='{0}' AND validFor ='N' AND frozenFor ='Y' AND (ISNULL(frozenFrom,'{1}') > '{1}' OR '{1}' > ISNULL(frozenTo,'{1}'))) AS c", ItemCode, DocDate);
                strSql.Append(" ON b.ItemCode=c.ItemCode");
                strSql.Append(" INNER JOIN");
                strSql.AppendFormat("(SELECT '{0}' AS ItemCode,ISNULL(COUNT(*),0) AS Advanced2 FROM " + sboname + "OITM WHERE ItemCode='{0}' AND validFor ='Y' AND frozenFor ='Y' AND ISNULL(frozenFrom,'{1}') <= '{1}' AND '{1}' <= ISNULL(frozenTo,'{1}')) AS d", ItemCode, DocDate);
                strSql.Append(" ON b.ItemCode=c.ItemCode");
                dTable = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
            }
            else
            {
                strSql.Append("SELECT a.ItemCode,a.Advanced,b.Activity,c.Inactivity,d.Advanced2 FROM");
                strSql.AppendFormat("(SELECT '{0}' AS ItemCode,IFNULL(COUNT(*),0) AS Advanced FROM {2}.store_OITM WHERE sbo_id={3} AND ItemCode='{0}' AND validFor ='Y' AND frozenFor ='Y' AND IFNULL(IF(validFrom='0000-00-00 00:00:00','{1}',validFrom),'{1}') <= '{1}' AND '{1}' <= IFNULL(IF(validTo='0000-00-00 00:00:00','{1}',validTo),'{1}')) AS a", ItemCode, DocDate, "nsap_bone", SboId);
                strSql.Append(" INNER JOIN");
                strSql.AppendFormat("(SELECT '{0}' AS ItemCode, IFNULL(COUNT(*),0) AS Activity FROM {2}.store_OITM WHERE sbo_id={3} AND ItemCode='{0}' AND validFor ='Y' AND frozenFor ='N' AND (IFNULL(IF(validFrom='0000-00-00 00:00:00','{1}',validFrom),'{1}') <= '{1}' AND '{1}'<= IFNULL(IF(validTo='0000-00-00 00:00:00','{1}',validTo),'{1}'))) AS b", ItemCode, DocDate, "nsap_bone", SboId);
                strSql.Append(" ON a.ItemCode=b.ItemCode");
                strSql.Append(" INNER JOIN");
                strSql.AppendFormat("(SELECT '{0}' AS ItemCode, IFNULL(COUNT(*),0) AS Inactivity FROM {2}.store_OITM WHERE sbo_id={3} AND ItemCode='{0}' AND validFor ='N' AND frozenFor ='Y' AND (IFNULL(IF(frozenFrom='0000-00-00 00:00:00','{1}',frozenFrom),'{1}') > '{1}' OR '{1}' > IFNULL(IF(frozenTo='0000-00-00 00:00:00','{1}',frozenTo),'{1}'))) AS c", ItemCode, DocDate, "nsap_bone", SboId);
                strSql.Append(" ON b.ItemCode=c.ItemCode");
                strSql.Append(" INNER JOIN");
                strSql.AppendFormat("(SELECT '{0}' AS ItemCode,IFNULL(COUNT(*),0) AS Advanced2 FROM {2}.store_OITM WHERE sbo_id={3} AND ItemCode='{0}' AND validFor ='Y' AND frozenFor ='Y' AND IFNULL(IF(frozenFrom='0000-00-00 00:00:00','{1}',frozenFrom),'{1}') <= '{1}' AND '{1}' <= IFNULL(IF(frozenTo='0000-00-00 00:00:00','{1}',frozenTo),'{1}')) AS d", ItemCode, DocDate, "nsap_bone", SboId);
                strSql.Append(" ON b.ItemCode=c.ItemCode");
                dTable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
            }

            if (dTable.Rows.Count > 0)
            {
                DataRow dRow = itemCodeList.NewRow();
                dRow[0] = dTable.Rows[0][0].ToString();
                dRow[1] = int.Parse(dTable.Rows[0][1].ToString());
                dRow[2] = int.Parse(dTable.Rows[0][2].ToString());
                dRow[3] = int.Parse(dTable.Rows[0][3].ToString());
                dRow[4] = int.Parse(dTable.Rows[0][4].ToString());
                itemCodeList.Rows.Add(dRow);
            }
        }
        #endregion
        #region 交货序列号
        /// <summary>
        /// 交货序列号
        /// </summary>
        public async Task<DataTable> SerialDelivery(SerialDeliveryNewReq serialDeliveryNewReq)
        {
            if (serialDeliveryNewReq.srialNumbers.Count > 0)
            {
                DataTable dt = _serviceSaleOrderApp.GetSboNamePwd(serialDeliveryNewReq.SboId);
                string dRowData = string.Empty; string isOpen = "0";
                string sqlconn = ""; string sboname = "";
                if (dt.Rows.Count > 0)
                {
                    sqlconn = dt.Rows[0][5].ToString();
                    isOpen = dt.Rows[0][6].ToString();
                    sboname = dt.Rows[0][0].ToString();
                }


                DataTable itemCodeList = new DataTable();
                itemCodeList.Columns.Add("itemCode", typeof(string));
                itemCodeList.Columns.Add("isCountHas", typeof(int));
                itemCodeList.Columns.Add("isSerialHas", typeof(int));
                itemCodeList.Columns.Add("isInvntItem", typeof(string));
                itemCodeList.Columns.Add("itemIndex", typeof(String));

                foreach (SrialNumbers i in serialDeliveryNewReq.srialNumbers)
                {
                    string index = string.IsNullOrEmpty(i.ItemIndex.ToString()) ? "0" : i.ItemIndex.ToString(); ;
                    double icount = string.IsNullOrEmpty(i.Count.ToString()) ? 1 : double.Parse(i.Count.ToString());
                    if (isOpen == "0")
                    {
                        //   NSAP.Data.Sales.BillDelivery.GetOnhand(i.ItemCode, int.Parse(icount.ToString()), i.WhsCod, itemCodeList, serialDeliveryNewReq.SboId, int.Parse(index));
                    }
                    else
                    {
                        if (i.ItemCode.Contains("'")) { i.ItemCode = i.ItemCode.Replace("'", "''"); }
                        GetOnhandSql(i.ItemCode, icount, i.WhsCod, itemCodeList, sqlconn, sboname, int.Parse(index));
                    }
                }
                return itemCodeList;
            }
            return null;
        }
        #endregion
        #region 判断物料是否为序列号管理和物料库存量

        /// <summary>
        /// 判断物料是否为序列号管理和物料库存量(SqlServer)
        /// </summary>
        public void GetOnhandSql(string ItemCode, double Count, string WhsCod, DataTable countList, string sqlconn, string sboname, int itemindex)
        {
            if (string.IsNullOrEmpty(sboname)) { sboname = ""; } else { sboname = sboname + ".dbo."; }
            string sql = string.Format("SELECT W.ItemCode,W.onhand,M.srial,M.InvntItem FROM");
            sql += string.Format(" (SELECT '{0}' AS ItemCode,ISNULL(COUNT(*),0) AS onhand FROM " + sboname + "OITW WHERE ItemCode='{0}' AND WhsCode='{1}' AND OnHand>={2}) AS W", ItemCode, WhsCod, Count);
            sql += string.Format(" INNER JOIN");
            sql += string.Format(" (SELECT '{0}' AS ItemCode,case when ManSerNum='Y' AND ManBtchNum='N' then 1 else 0 end AS srial,case when InvntItem='Y' then 1 else 0 end as InvntItem FROM " + sboname + "OITM WHERE ItemCode='{0}') AS M", ItemCode);
            sql += string.Format(" ON W.ItemCode=M.ItemCode");
            DataTable dTable = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, sql, CommandType.Text, null);
            if (dTable.Rows.Count > 0)
            {
                DataRow dRow = countList.NewRow();
                dRow[0] = dTable.Rows[0][0].ToString();
                dRow[1] = int.Parse(dTable.Rows[0][1].ToString());
                dRow[2] = int.Parse(dTable.Rows[0][2].ToString());
                dRow[3] = dTable.Rows[0][3].ToString();
                dRow[4] = itemindex;
                countList.Rows.Add(dRow);
            }
        }
        #endregion
        #region 根据物料获取序列号
        /// <summary>
        /// 根据物料获取序列号
        /// </summary>
        public async Task<TableData> GetDisrNumber(GetDisrNumberReq getDisrNumberReq)
        {
            string sortString = string.Empty; string filterString = string.Empty;
            if (!string.IsNullOrEmpty(getDisrNumberReq.sortname) && !string.IsNullOrEmpty(getDisrNumberReq.sortorder))
                sortString = string.Format("{0} {1}", getDisrNumberReq.sortname, getDisrNumberReq.sortorder.ToUpper());
            if (!string.IsNullOrEmpty(getDisrNumberReq.serial))
            {
                filterString += string.Format("(o.SysSerial LIKE '%{0}%' OR o.SuppSerial LIKE '%{0}%' OR o.IntrSerial LIKE '%{0}%' OR o.BaseType like '%{0}%' OR o.BaseEntry like '%{0}%') AND ", getDisrNumberReq.serial);

            }


            filterString += string.Format("(o.ItemCode = '{0}' AND Status='{1}' AND WhsCode='{2}') AND ", getDisrNumberReq.ItemCode, getDisrNumberReq.Status, getDisrNumberReq.WhsCode);

            if (!string.IsNullOrEmpty(filterString))
                filterString = filterString.Substring(0, filterString.Length - 5);
            DataTable dt = _serviceSaleOrderApp.GetSboNamePwd(int.Parse(getDisrNumberReq.SboId));
            string dRowData = string.Empty; string isOpen = "0"; string sqlconn = "0";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); sqlconn = dt.Rows[0][5].ToString(); }

            return GetDisrNumberSql(getDisrNumberReq.limit, getDisrNumberReq.page, filterString, sortString, getDisrNumberReq.ItemCode, getDisrNumberReq.Status, getDisrNumberReq.WhsCode, getDisrNumberReq.ExistsSerialStr, sqlconn);


        }
        #endregion
        #region 获取物料的可用序列号
        /// <summary>
        /// 获取物料的可用序列号
        /// </summary>
        /// <param name="rowCounts"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="filterQuery"></param>
        /// <param name="orderName"></param>
        /// <param name="ItemCode"></param>
        /// <param name="Status"></param>
        /// <param name="WhsCode"></param>
        /// <param name="existsList"></param>
        /// <param name="sqlconn"></param>
        /// <returns></returns>
        public TableData GetDisrNumberSql(int pageSize, int pageIndex, string filterQuery, string orderName, string ItemCode, string Status, string WhsCode, string existsList, string sqlconn)
        {
            StringBuilder tableName = new StringBuilder();
            StringBuilder filedName = new StringBuilder();
            filedName.Append("o.SysSerial,o.SuppSerial,o.IntrSerial,o.CreateDate,o.BaseType,o.BaseEntry ");
            tableName.Append(" OSRI o");
            DataTable rDataRow = GetChooseDisrNumber(ItemCode, "0");
            if (rDataRow.Rows.Count > 0)
            {
                for (int i = 0; i < rDataRow.Rows.Count; i++)
                {
                    existsList += (string.IsNullOrEmpty(existsList) ? "" : ",") + string.Format("'{0}'", rDataRow.Rows[i][0]);
                }
            }
            if (!string.IsNullOrEmpty(existsList))
            {
                if (!string.IsNullOrEmpty(filterQuery)) { filterQuery += string.Format(" AND SysSerial NOT IN (" + existsList + ")"); }
                else { filterQuery += string.Format(" SysSerial NOT IN(" + existsList + ")"); }
            }
            string strSql = string.Format("SELECT {0} FROM {1} WHERE {2} ORDER BY {3}", filedName.ToString(), tableName.ToString(), filterQuery, orderName);
            return _serviceSaleOrderApp.SAPSelectPagingNoneRowsCount(tableName.ToString(), filedName.ToString(), pageSize, pageIndex, orderName, filterQuery);
        }
        //获取已进入审核流程的序列号
        public DataTable GetChooseDisrNumber(string ItemCode, string IsChange)
        {
            string strSql = string.Format("Select SysNumber FROM {0}.store_osrn_alreadyexists WHERE ItemCode='{1}'", "nsap_bone", ItemCode);
            if (IsChange == "0") { strSql += string.Format(" AND IsChange ={0}", 1); }
            DataTable dowTable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            return dowTable;

            #endregion
        }
    }
}
