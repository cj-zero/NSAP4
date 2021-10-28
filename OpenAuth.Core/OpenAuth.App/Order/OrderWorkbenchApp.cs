using Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

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
        public DataTable GetSubmtToMe(int pageSize, int pageIndex, string filterQuery, string sortname, string sortorder, int user_id, string Applicator, string Customer, string Status, string BeginDate, string EndDate, bool ViewCustom, bool ViewSales)
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
            filedName.Append(" a.job_id,c.job_type_nm,a.job_nm,b.user_nm,a.job_state,a.upd_dt,a.remarks,c.job_type_id,a.step_id,a.card_code,");
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

    }
}
