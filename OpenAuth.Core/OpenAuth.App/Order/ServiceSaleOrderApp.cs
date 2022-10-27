extern alias MySqlConnectorAlias;
using DotNetCore.CAP;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using OpenAuth.App.Interface;
using OpenAuth.App.Order.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.NsapBase;
using Microsoft.Data.SqlClient;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository;
using OpenAuth.Repository.Extensions;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using OpenAuth.App.Order.ModelDto;
using OpenAuth.Repository.Domain.Sap;
using NSAP.Entity.Sales;
using Infrastructure.Helpers;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using static Infrastructure.Helpers.CommonHelper;
using NSAP.Entity.BillFlow;
using NSAP.Entity.Store;
using NSAP.Entity.Product;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Infrastructure.Export;
using DinkToPdf;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenAuth.App.Client.Request;
using SAPbobsCOM;
using DocumentFormat.OpenXml.Math;

namespace OpenAuth.App.Order
{
    /// <summary>
    /// 销售订单业务
    /// </summary>
    public partial class ServiceSaleOrderApp : OnlyUnitWorkBaeApp
    {
        static Dictionary<int, DataTable> gAmbit = new Dictionary<int, DataTable>();
        static Dictionary<int, DataTable> gRoles = new Dictionary<int, DataTable>();
        private readonly RevelanceManagerApp _revelanceApp;
        private readonly AppServiceOrderLogApp _appServiceOrderLogApp;
        private readonly ServiceOrderLogApp _ServiceOrderLogApp;
        //private OrderDraftServiceApp _orderDraftServiceApp;
        private IOptions<AppSetting> _appConfiguration;
        private ICapPublisher _capBus;
        private readonly ServiceFlowApp _serviceFlowApp;
        ServiceBaseApp _serviceBaseApp;
        private ILogger<ServiceSaleOrderApp> _logger;
        private const string NewareName = "新威尔";
        private const string NewllName = "新能源";
        private const string DGNewareName = "东莞新威";
        private const string WBName = "外币";

        public ServiceSaleOrderApp(IUnitWork unitWork, ILogger<ServiceSaleOrderApp> logger, RevelanceManagerApp app, ServiceBaseApp serviceBaseApp, ServiceOrderLogApp serviceOrderLogApp, IAuth auth, AppServiceOrderLogApp appServiceOrderLogApp, IOptions<AppSetting> appConfiguration, ICapPublisher capBus, ServiceOrderLogApp ServiceOrderLogApp, ServiceFlowApp serviceFlowApp) : base(unitWork, auth)
        {
            _logger = logger;
            //_orderDraftServiceApp = orderDraftServiceApp;
            _appConfiguration = appConfiguration;
            _revelanceApp = app;
            _appServiceOrderLogApp = appServiceOrderLogApp;
            _capBus = capBus;
            _ServiceOrderLogApp = ServiceOrderLogApp;
            _serviceFlowApp = serviceFlowApp;
            _serviceBaseApp = serviceBaseApp;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="filterQuery"></param>
        /// <param name="sortname"></param>
        /// <param name="sortorder"></param>
        /// <param name="type"></param>
        /// <param name="ViewFull"></param>
        /// <param name="ViewSelf"></param>
        /// <param name="UserID"></param>
        /// <param name="SboID"></param>
        /// <param name="ViewSelfDepartment"></param>
        /// <param name="DepID"></param>
        /// <param name="ViewCustom"></param>
        /// <param name="ViewSales"></param>
        /// <param name="sqlcont"></param>
        /// <param name="sboname"></param>
        /// <returns></returns>
        public TableData SelectOrderDraftInfo(int pageSize, int pageIndex, QuerySalesQuotationReq query, string type, bool ViewFull, bool ViewSelf, int UserID, int SboID, bool ViewSelfDepartment, int DepID, bool ViewCustom, bool ViewSales, string sqlcont, string sboname)
        {
            TableData tableData = null;
            int rowCount = 0;
            bool IsSql = true;
            string sortString = string.Empty;
            string filterString = string.Empty;
            string line = string.Empty;
            int uSboId = SboID;
            //排序
            if (string.IsNullOrWhiteSpace(query.SortName))
            {
                sortString = string.Format("{0} {1}", "a.docentry", "desc".ToUpper());
            }
            else
            {
                sortString = string.Format("{0} {1}", query.SortName, query.SortOrder);
            }
            string dRowData = string.Empty;
            #region 搜索条件
            //账
            if (query.Sboid > 0)
            {
                if (query.Sboid == SboID)
                {
                    IsSql = true;
                }
                else
                {
                    IsSql = false;
                }
            }

            //单号条件
            if (!string.IsNullOrWhiteSpace(query.DocEntry))
            {
                filterString += string.Format("a.DocEntry LIKE '{0}' AND ", query.DocEntry.Trim());
            }

            if (!string.IsNullOrWhiteSpace(query.CardCode))
            {
                filterString += string.Format("(a.CardCode LIKE '%{0}%' OR a.CardName LIKE '%{0}%') AND ", query.CardCode.Trim());
            }
            if (!string.IsNullOrWhiteSpace(query.DocStatus))
            {
                if (query.DocStatus == "ON")
                {
                    filterString += string.Format(" (a.DocStatus = 'O' AND a.Printed = 'N' AND a.CANCELED = 'N') AND ");
                }
                if (query.DocStatus == "OY")
                {
                    filterString += string.Format(" (a.DocStatus = 'O' AND a.Printed = 'Y' AND a.CANCELED = 'N') AND ");
                }
                if (query.DocStatus == "CY")
                {
                    filterString += string.Format(" a.CANCELED = 'Y' AND ");
                }
                if (query.DocStatus == "CN")
                {
                    filterString += string.Format(" (a.DocStatus = 'C' AND a.CANCELED = 'N') AND ");
                }
            }
            if (!string.IsNullOrWhiteSpace(query.Comments))
            {
                filterString += string.Format("a.Comments LIKE '%{0}%' AND ", query.Comments);
            }
            if (!string.IsNullOrWhiteSpace(query.SlpName))
            {
                filterString += string.Format("c.SlpName LIKE '%{0}%' AND ", query.SlpName.Trim());
            }
            //时间区间
            if (!string.IsNullOrWhiteSpace(query.FirstTime) && !string.IsNullOrWhiteSpace(query.LastTime))
            {

                //filterString += string.Format("DATE_FORMAT(a.UpdateDate,'%Y-%m-%d')  BETWEEN '{0}' AND '{1}' AND ", query.FirstTime, query.LastTime);
                filterString += string.Format("a.UpdateDate BETWEEN '{0}' AND '{1}' AND ", query.FirstTime, query.LastTime);
            }

            if (type == "ORDR")
            {
                if (!string.IsNullOrWhiteSpace(query.Indicator))
                {
                    filterString += string.Format("a.Indicator = '{0}' AND ", query.Indicator);
                }
            }


            #endregion

            #region 根据不同的单据显示不同的内容
            if (!string.IsNullOrEmpty(type))
            {
                if (type == "OQUT") { type = "OQUT"; line = "QUT1"; }//销售报价单
                else if (type == "ORDR") { type = "ORDR"; line = "RDR1"; }//销售订单
                else if (type == "ODLN") { type = "ODLN"; line = "DLN1"; }//销售交货单
                else if (type == "OINV") { type = "OINV"; line = "INV1"; }//应收发票
                else if (type == "ORDN") { type = "ORDN"; line = "RDN1"; }//销售退货单
                else if (type == "ORIN") { type = "ORIN"; line = "RIN1"; }//应收贷项凭证
                else if (type == "OPQT") { type = "OPQT"; line = "PQT1"; }//采购报价单
                else if (type == "OPOR") { type = "OPOR"; line = "POR1"; }//采购订单
                else if (type == "OPDN") { type = "OPDN"; line = "PDN1"; }//采购收货单
                else if (type == "OPCH") { type = "OPCH"; line = "PCH1"; }//应付发票
                else if (type == "ORPD") { type = "ORPD"; line = "RPD1"; }//采购退货单
                else if (type == "ORPC") { type = "ORPC"; line = "RPC1"; }//应付贷项凭证
                else { type = "OQUT"; line = "QUT1"; }
            }
            #endregion

            #region 判断权限
            string arr_roles = UnitWork.ExcuteSql<RolesDto>(ContextType.NsapBaseDbContext, $"SELECT a.role_id Id,b.role_nm Name FROM nsap_base.base_user_role AS a INNER JOIN nsap_base.base_role AS b ON a.role_id=b.role_id WHERE a.user_id={UserID}", CommandType.Text, null).FirstOrDefault()?.Name;
            if ((line == "PQT1" || line == "POR1" || line == "PDN1" || line == "PCH1" || line == "RPD1" || line == "RPC1") && ((!arr_roles.Contains("物流文员")) && (!arr_roles.Contains("系统管理员"))))//若不含有物流文员角色，则则屏蔽运输采购单
            {
                filterString += string.Format(" d.QryGroup1='N' AND ");
            }
            if (ViewSelfDepartment && !ViewFull)
            {
                System.Data.DataTable rDataRows;
                DataTable rdepRows = new DataTable();
                if (line == "POR1" || line == "PDN1" || line == "PQT1" || line == "PCH1" || line == "RPD1" || line == "RPC1")
                {
                    //查询部门映射关系表
                    rdepRows = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT dep_id2 FROM nsap_bone.base_dep_map  WHERE dep_id1={DepID} and map_type_id=1", CommandType.Text, null);
                }
                if (rdepRows.Rows.Count > 0)
                {
                    string dep_ids = string.Empty;
                    foreach (DataRow item in rdepRows.Rows)
                    {
                        dep_ids += item[0].ToString() + ",";
                    }
                    //部门查询
                    rDataRows = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT a.sale_id,a.tech_id FROM nsap_base.sbo_user a LEFT JOIN nsap_base.base_user_detail b ON a.user_id = b.user_id WHERE b.dep_id in ({dep_ids.TrimEnd(',')}) AND a.sbo_id = {SboID}", CommandType.Text, null);
                }
                else
                {
                    //根据部门ID获取销售员ID
                    rDataRows = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT a.sale_id,a.tech_id FROM nsap_base.sbo_user a LEFT JOIN nsap_base.base_user_detail b ON a.user_id = b.user_id WHERE b.dep_id = {DepID} AND a.sbo_id = {SboID}", CommandType.Text, null);
                }
                if (rDataRows.Rows.Count > 0)
                {
                    filterString += string.Format(" (a.SlpCode IN(");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][0]);
                    }
                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(") OR d.DfTcnician IN (");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][1]);
                    }
                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(")) AND ");
                }
            }
            else if (ViewSelf && !ViewFull && !ViewSelfDepartment)
            {
                System.Data.DataTable rDataRowsSlp = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT sale_id,tech_id FROM nsap_base.sbo_user WHERE user_id={UserID} AND sbo_id={SboID}", CommandType.Text, null); ;
                if (rDataRowsSlp.Rows.Count > 0)
                {
                    string slpCode = rDataRowsSlp.Rows[0][0].ToString();
                    string slpTcnician = rDataRowsSlp.Rows[0][1].ToString();
                    filterString += string.Format(" (a.SlpCode = {0}) AND ", slpCode);// OR d.DfTcnician={1}   , slpTcnician  不允许售后查看业务员的单
                }
                else
                {
                    filterString = string.Format(" (a.SlpCode = {0}) AND ", 0);
                }
            }
            #endregion
            if (!string.IsNullOrEmpty(filterString))
            {
                filterString = filterString.Substring(0, filterString.Length - 5);
            }
            if (IsSql)
            {
                if (!ViewSelf)
                {
                    filterString += "1 = 2";
                }
                //视图查询数据
                tableData = SelectOrdersInfo(out rowCount, pageSize, pageIndex, filterString, sortString, type, line, ViewCustom, ViewSales, sqlcont, sboname);

            }

            return tableData;
        }
        /// <summary>
        /// 查看视图【主页面 - 帐套开启】
        /// </summary>
        /// <returns></returns>
        public TableData SelectOrdersInfo(out int rowCounts, int pageSize, int pageIndex, string filterQuery, string orderName, string type, string line, bool ViewCustom, bool ViewSales, string sqlcont, string sboname)
        {
            TableData tableData = new TableData();
            StringBuilder tableName = new StringBuilder();
            StringBuilder filedName = new StringBuilder(); int Custom = 0; int Sales = 0;
            if (ViewCustom) { Custom = 1; }
            if (ViewSales) { Sales = 1; }
            if (string.IsNullOrEmpty(sboname)) { sboname = ""; } else { sboname = sboname + ".dbo."; }
            filedName.Append(" a.UpdateDate,a.DocEntry,a.CardCode,CASE WHEN 1 = " + Custom + " THEN a.CardName ELSE '******' END AS CardName,CASE WHEN 1 = " + Sales + " THEN a.DocTotal ELSE 0 END AS DocTotal,CASE WHEN 1 = " + Sales + " THEN (a.DocTotal-a.PaidToDate) ELSE 0 END AS OpenDocTotal,a.CreateDate,a.SlpCode,a.Comments,a.DocStatus,a.Printed,c.SlpName,a.CANCELED,a.Indicator,a.DocDueDate,e.PymntGroup,'' as billID,'' AS ActualDocDueDate ");
            if (type == "OPDN")
            {
                filedName.Append(" ,a.U_YGMD ");
                filedName.Append(@",(
                                    select ',' + CONVERT(VARCHAR(20), docentry) from (
                                    SELECT DISTINCT PCH1.DocEntry from " + sboname + "PCH1 LEFT JOIN " + sboname + "PDN1 on PCH1.BaseType = 20 and PCH1.BaseEntry = PDN1.DocEntry and PCH1.BaseLine = PDN1.LineNum"
                                    + " where PDN1.DocEntry = a.DocEntry) v1 for xml path('') ) as pchdocstr");
            }
            if (type.ToLower() == "ordr" || type.ToLower() == "opor")
            {
                filedName.Append(",'' as PrintNo,'' as PrintNumIndex");
            }
            if (type == "OPOR")
            {
                filedName.Append(",'' as PurchaseBillNo,'' as IndicatorName ,a.U_THYY ");
                //应付发票号
                filedName.Append(@",(
                                        select ',' + CONVERT(VARCHAR(20), docentry) from (
                                        SELECT DISTINCT PCH1.DocEntry from " + sboname + "PCH1 LEFT JOIN " + sboname + "PDN1 on PCH1.BaseType = 20 and PCH1.BaseEntry = PDN1.DocEntry and PCH1.BaseLine = PDN1.LineNum"
                                        + " LEFT JOIN " + sboname + "POR1 on PDN1.BaseType = 22 and PDN1.BaseEntry = POR1.DocEntry and PDN1.BaseLine = POR1.LineNum"
                                        + " LEFT JOIN " + sboname + "POR1 p1 on PCH1.BaseType=22 and PCH1.BaseEntry =p1.Docentry and PCH1.BaseLine=p1.LineNum"
                                        + " where POR1.DocEntry = a.DocEntry or p1.DocEntry=a.DocEntry) v1 for xml path('') ) as pchdocstr");
            }
            if (type.ToLower() == "oqut")
            {
                filedName.Append(",'0'  as AttachFlag ");
            }
            if (type.ToLower() == "odln")
            {
                filedName.Append(",'' as BuyDocEntry,'' as TransportName,'' as TransportID,'' as TransportSum,a.DocDate");
            }
            tableName.AppendFormat("" + sboname + type + " a ");
            tableName.AppendFormat(" LEFT JOIN " + sboname + "OSLP c ON a.SlpCode = c.SlpCode");
            tableName.AppendFormat(" LEFT JOIN " + sboname + "OCRD d ON a.CardCode = d.CardCode");
            tableName.AppendFormat(" LEFT JOIN " + sboname + "OCTG e ON a.GroupNum = e.GroupNum");
            //增加打印编号一列
            List<SqlParameter> sqlParameters = new List<SqlParameter>()
            {
                new SqlParameter("@strFrom",tableName.ToString()),
                new SqlParameter("@strSelect",filedName.ToString()),
                new SqlParameter("@pageSize",pageSize),
                new SqlParameter("@pageIndex",pageIndex),
                new SqlParameter("@strOrder",orderName),
                new SqlParameter("@strWhere",filterQuery),

            };
            SqlParameter isStats = new SqlParameter("@isStats", SqlDbType.Int);
            isStats.Value = 1;
            sqlParameters.Add(isStats);
            SqlParameter paramOut = new SqlParameter("@rowCount", SqlDbType.Int);
            paramOut.Value = 0;
            paramOut.Direction = ParameterDirection.Output;
            sqlParameters.Add(paramOut);
            DataTable dt = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, $"sp_common_pager", CommandType.StoredProcedure, sqlParameters);

            if (dt.Rows.Count > 0)
            {
                tableData.Count = Convert.ToInt32(paramOut.Value);
                rowCounts = Convert.ToInt32(sqlParameters[7].Value);
            }
            else
            {
                tableData.Count = 0;
                rowCounts = 0;
            }
            #region comment
            // dt = Sql.SAPSelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), pageSize, pageIndex, orderName, filterQuery, out rowCounts);
            //if (type.ToLower() == "ordr" || type.ToLower() == "opor")
            //{
            //    string bonetype = type.ToLower();
            //    string sql = "SELECT  a.PrintNo,a.PrintNumIndex  FROM nsap_bone.sale_ordr a  where a.DocEntry=1941 and a.sbo_id=1";
            //    if ("opor" == type.ToLower())
            //    {
            //        sql = "SELECT  a.PrintNo,a.PrintNumIndex  FROM nsap_bone.buy_opor a  where a.DocEntry=1941 and a.sbo_id=1";
            //    }
            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        DataTable dtPrintnum = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);//取编号
            //        if (dtPrintnum.Rows.Count > 0)
            //        {
            //            dt.Rows[i]["PrintNo"] = dtPrintnum.Rows[0][0].ToString();
            //            dt.Rows[i]["PrintNumIndex"] = dtPrintnum.Rows[0][1].ToString();
            //        }

            //    }
            //}
            #endregion
            if (type.ToLower() == "oqut")
            {
                if (dt.Rows.Count > 0)
                {
                    //添加4.0 中间商 终端
                    #region
                    dt.Columns.Add("Flag", typeof(int));
                    dt.Columns.Add("Terminals", typeof(String));
                    var clientNoList = dt.AsEnumerable().Select(row => row.Field<string>("CardCode")).ToList();  //CardCode
                    var reimbursementList = dt.AsEnumerable().Select(row => row.Field<int>("DocEntry").ToString()).ToList();
                    var legitJobList = UnitWork.Find<wfa_job>(a => reimbursementList.Contains(a.sbo_itf_return) && a.sync_stat == 4 && a.job_type_id == 13).ToList();
                    var legitJobIdList = legitJobList.Select(a => a.job_id).ToList();
                    var jobrelations = UnitWork.Find<OpenAuth.Repository.Domain.JobClientRelation>(a => clientNoList.Contains(a.AffiliateData) && legitJobIdList.Contains(a.Jobid) && a.IsDelete == 0 && a.Origin ==2).ToList();
                    foreach (var datarow in dt.AsEnumerable())
                    {
                        var specJob = legitJobList.Where(a => a.sbo_itf_return == datarow["DocEntry"].ToString()).FirstOrDefault();
                        if (specJob != null)
                        {
                            var relation = jobrelations.Where(a => a.AffiliateData == datarow["CardCode"].ToString() && a.Jobid == specJob.job_id).FirstOrDefault();
                            if (relation != null)
                            {
                                datarow["Flag"] = 1;
                                datarow["Terminals"] = relation.Terminals;

                            }
                        }
                        else
                        {
                            datarow["Flag"] = 0;
                            datarow["Terminals"] = "";
                       
                        }
                    }
                    #endregion

                    //获取订单号
                    List<int> orderNos = (from d in dt.AsEnumerable() select d.Field<int>("DocEntry")).ToList();
                    string orderNo = string.Join(",", orderNos);
                    if (!string.IsNullOrWhiteSpace(orderNo))
                    {
                        //取销售合同类型附件
                        //var typeObj = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, $"SELECT a.type_id value FROM nsap_oa.file_type a LEFT JOIN nsap_base.base_func b ON a.func_id=b.func_id LEFT JOIN nsap_base.base_page c ON c.page_id=b.page_id WHERE c.page_url='{"sales/SalesQuotation.aspx"}'", CommandType.Text, null).FirstOrDefault();
                        //string fileType = typeObj == null ? "-1" : typeObj.Value.ToString();
                        string strSql2 = string.Format("SELECT  DISTINCT 1 value,T0.docEntry docEntry  FROM nsap_oa.file_main AS T0 ");
                        strSql2 += string.Format("LEFT JOIN nsap_oa.file_type AS T1 ON T0.file_type_id = T1.type_id ");
                        strSql2 += string.Format(@"INNER JOIN (
SELECT a.type_id FROM nsap_oa.file_type a LEFT JOIN nsap_base.base_func b ON a.func_id = b.func_id LEFT JOIN nsap_base.base_page c ON c.page_id = b.page_id WHERE c.page_url = 'sales/SalesQuotation.aspx'
) as t ON t.type_id = T0.file_type_id ");
                        strSql2 += string.Format("WHERE T0.docEntry  in( {0} ) ", orderNo);
                        List<ResultOrderDto> fileflags = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, strSql2, CommandType.Text, null);
                        if (fileflags != null && fileflags.Count > 0)
                        {
                            DataRow[] dataRowS = dt.Select("DocEntry in (" + orderNo + ")");
                            foreach (DataRow temprow in dataRowS)
                            {
                                var fileflag = fileflags.FirstOrDefault(zw => zw.docEntry.ToString() == temprow["DocEntry"].ToString());
                                if (fileflag != null)
                                {
                                    temprow["AttachFlag"] = fileflag == null ? "0" : fileflag.Value.ToString();
                                }
                            }
                        }
                    }
                }
            }
            tableData.Data = dt.Tolist<SalesDraftDto>();
            return tableData;
        }
        /// <summary>
        /// 业务伙伴获取
        /// </summary>
        /// <param name="query"></param>
        /// <param name="filterQuery"></param>
        /// <param name="sqlcont"></param>
        /// <param name="sboname"></param>
        /// <returns></returns>
        public TableData SelectCardCodeInfo(CardCodeRequest query, string sortSt, string filterQuery, string sboname)
        {
            TableData tableData = new TableData();
            StringBuilder filefName = new StringBuilder();
            StringBuilder tableName = new StringBuilder();
            if (string.IsNullOrEmpty(sboname))
            {
                sboname = "";
            }
            else
            {
                sboname = sboname + ".dbo.";
            }
            string U_FPLB = string.Empty;
            var syscolumn = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.SapDbContextType, $@"SELECT COUNT(*) value FROM syscolumns WHERE id=object_id('OCRD') AND name='U_FPLB'", CommandType.Text, null).FirstOrDefault();
            if (syscolumn != null && syscolumn.Value.ToString() != "0")
            {
                U_FPLB = ",a.U_FPLB";
            }
            filefName.AppendFormat(" a.CardCode,a.CardName,a.CntctPrsn,b.SlpName,a.Currency,a.Balance,(ISNULL(ZipCode,'')+ISNULL(c.Name,'')+ISNULL(d.Name,'')+ISNULL(City,'')+ISNULL(CONVERT(VARCHAR(100),Building),'''')) AS Address,(ISNULL(MailZipCod,'')+ISNULL(e.Name,'')+ISNULL(f.Name,'')+ISNULL(MailCity,'')+ISNULL(CONVERT(VARCHAR(100),MailBuildi),'''')) AS Address2{0},a.SlpCode", U_FPLB);
            tableName.AppendFormat(" " + sboname + "OCRD a");
            tableName.AppendFormat(" LEFT JOIN " + sboname + "OSLP b ON a.SlpCode=b.SlpCode");
            tableName.AppendFormat(" LEFT JOIN " + sboname + "OCRY c ON a.Country=c.Code");
            tableName.AppendFormat(" LEFT JOIN " + sboname + "OCST d ON a.State1=c.Code");
            tableName.AppendFormat(" LEFT JOIN " + sboname + "OCRY e ON a.MailCountr=e.Code");
            tableName.AppendFormat(" LEFT JOIN " + sboname + "OCST f ON a.State1=f.Code");
            List<SqlParameter> sqlParameters = new List<SqlParameter>()
            {
                new SqlParameter("@strFrom",tableName.ToString()),
                new SqlParameter("@strSelect",filefName.ToString()),
                new SqlParameter("@pageSize",query.limit),
                new SqlParameter("@pageIndex",query.page),
                new SqlParameter("@strOrder",sortSt),
                new SqlParameter("@strWhere",filterQuery),
            };
            SqlParameter isStats = new SqlParameter("@isStats", SqlDbType.Int);
            isStats.Value = 1;
            sqlParameters.Add(isStats);
            SqlParameter paramOut = new SqlParameter("@rowCount", SqlDbType.Int);
            paramOut.Value = 0;
            paramOut.Direction = ParameterDirection.Output;
            sqlParameters.Add(paramOut);
            DataTable dt = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, $"sp_common_pager", CommandType.StoredProcedure, sqlParameters);
            tableData.Data = dt.Tolist<CardCodeDto>();
            tableData.Count = Convert.ToInt32(paramOut.Value);
            return tableData;
        }
        /// <summary>
        /// 获取业务员信息
        /// </summary>
        /// <param name="sboId"></param>
        /// <returns></returns>
        public List<SelectOption> GetSalesSelect(int sboId)
        {
            var loginContext = _auth.GetCurrentUser();
            //业务员Id
            var selectOption = UnitWork.Find<crm_oslp>(s => s.sbo_id == sboId).Select(zw => new SelectOption { Key = zw.SlpCode.ToString(), Option = zw.SlpName }).ToList();
            return selectOption;
        }
        /// <summary>
        /// 销售报价单保存
        /// </summary>
        /// <param name="orderReq"></param>
        /// <returns></returns>
        public string Save(AddOrderReq orderReq)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            string result = "";
            int userID = _serviceBaseApp.GetUserNaspId();
            int sboID = _serviceBaseApp.GetUserNaspSboID(userID);
            int billSboId = sboID;
            List<DropDownOption> dropDownOptions = UnitWork.ExcuteSql<DropDownOption>(ContextType.NsapBaseDbContext, $@"SELECT b.CntctCode AS id,b.Name AS name FROM  nsap_bone.crm_ocrd a LEFT JOIN  nsap_bone.crm_ocpr b ON a.CardCode=b.CardCode and a.sbo_id=b.sbo_id WHERE a.CardCode='{orderReq.Order.CardCode}' and a.sbo_id={billSboId} and b.Active='Y' ", CommandType.Text, null);
            var customer = dropDownOptions.Where(r => r.Id.ToString() == orderReq.Order.CntctCode).ToList();
            if (customer.Count() <= 0 || customer == null)
            {
                result = "联系人无效-201";
                return result;
            }

            if (orderReq.JobId != 0)
            {
                if (orderReq.IsCopy == true)
                {
                    if (IsExistDocOqut(orderReq.JobId.ToString(), "23", "7"))
                    {
                        result = "该销售订单已提交-201";
                        return result;
                    }
                }
                if (IsExistDocOqut(orderReq.JobId.ToString(), "-5", "13"))
                {
                    result = "该销售报价单已提交-201";
                    return result;
                }
                //DataTable objTable = GetAuditObjWithFlowChart(orderReq.JobId.ToString());
                //if (objTable.Rows.Count > 0)
                //{
                //    foreach (DataRow objRow in objTable.Rows)
                //    {
                //        if (!objRow[0].ToString().Contains(loginContext.User.Name))
                //        {
                //            return "单据已提交，请勿重复提交";
                //        }
                //    }
                //}
            }
     
            int funcId = 50;
            string logstring = "";
            string jobname = "";
            try
            {
                if (orderReq.Order.FileList != null && orderReq.Order.FileList.Count > 0)
                {
                    orderReq.Order.FileList.ForEach(zw =>
                    {
                        zw.fileUserId = userID.ToString();
                    });
                }

                billDelivery billDelivery = BulidBillDelivery(orderReq.Order);
                //if (orderReq.IsCopy)
                //{
                //    funcId = _serviceBaseApp.GetFuncsByUserID("sales/SalesOrder.aspx", userID);
                //    logstring = "根据销售报价单下销售订单";
                //    jobname = "销售订单";
                //    SalesOrderSave_ORDR(orderReq);
                //}
                //else
                //{
                funcId = _serviceBaseApp.GetFuncsByUserID("sales/SalesQuotation.aspx", userID);
                byte[] job_data = ByteExtension.ToSerialize(billDelivery);
                string className = "NSAP.B1Api.BOneOQUT";
                logstring = "新建销售报价单";
                jobname = "销售报价单";

                if (orderReq.Ations == OrderAtion.Draft)
                {
                    result = OrderWorkflowBuild(jobname, funcId, userID, job_data, orderReq.Order.Remark, sboID, orderReq.Order.CardCode, orderReq.Order.CardName, (double.Parse(orderReq.Order.DocTotal.ToString()) > 0 ? double.Parse(orderReq.Order.DocTotal.ToString()) : 0), -5, int.Parse(orderReq.Order.BillBaseEntry), "BOneAPI", className);
                }
                else if (orderReq.Ations == OrderAtion.Submit)
                {
                    result = OrderWorkflowBuild(jobname, funcId, userID, job_data, orderReq.Order.Remark, sboID, orderReq.Order.CardCode, orderReq.Order.CardName, (double.Parse(orderReq.Order.DocTotal.ToString()) > 0 ? double.Parse(orderReq.Order.DocTotal.ToString()) : 0), -5, int.Parse(orderReq.Order.BillBaseEntry), "BOneAPI", className);
                    if (int.Parse(result) > 0)
                    {
                        var par = SaveJobPara(result, orderReq.IsTemplate);
                        if (par == "1")
                        {
                            string _jobID = result;
                            if ("0" != WorkflowSubmit(int.Parse(result), userID, orderReq.Order.Remark, "", 0))
                            {
                                #region 更新商城订单状态
                                WfaEshopStatus thisinfo = new WfaEshopStatus();
                                thisinfo.JobId = int.Parse(result);
                                thisinfo.UserId = userID;
                                thisinfo.SlpCode = sboID;
                                thisinfo.CardCode = orderReq.Order.CardCode;
                                thisinfo.CardName = orderReq.Order.CardName;
                                thisinfo.CurStatus = 0;
                                thisinfo.OrderPhase = "0000";
                                thisinfo.ShippingPhase = "0000";
                                thisinfo.CompletePhase = "0";
                                thisinfo.OrderLastDate = DateTime.Now;
                                thisinfo.FirstCreateDate = DateTime.Now;
                                //设置报价单提交
                                if (string.IsNullOrEmpty(orderReq.Order.U_New_ORDRID))
                                {
                                    result = Eshop_OrderStatusFlow(thisinfo, billDelivery.billSalesDetails, 0);
                                }
                                else
                                {
                                    if (orderReq.Order.U_New_ORDRID.Contains(","))
                                    {
                                        string[] orderids = orderReq.Order.U_New_ORDRID.Split(',');
                                        result = Eshop_OrderStatusFlow(thisinfo, billDelivery.billSalesDetails, Convert.ToInt32(orderids[0]));
                                    }
                                    else
                                    {
                                        result = Eshop_OrderStatusFlow(thisinfo, billDelivery.billSalesDetails, Convert.ToInt32(orderReq.Order.U_New_ORDRID));
                                    }
                                }
                               
                                #endregion
                            }
                            else { result = "0"; }
                        }
                        else { result = "0"; }
                    }
                }
                else if (orderReq.Ations == OrderAtion.Resubmit)
                {

                    result = WorkflowSubmit(orderReq.JobId, userID, orderReq.Order.Remark, "", 0);
                }
                else if (orderReq.Ations == OrderAtion.DraftUpdate)
                {
                    result = UpdateAudit(orderReq.JobId, job_data, orderReq.Order.Remark, orderReq.Order.DocTotal.ToString(), orderReq.Order.CardCode, orderReq.Order.CardName);
                }
                else if (orderReq.Ations == OrderAtion.DrafSubmit)
                {
                    result = UpdateAudit(orderReq.JobId, job_data, orderReq.Order.Remark, orderReq.Order.DocTotal.ToString(), orderReq.Order.CardCode, orderReq.Order.CardName);
                    if (result != null)
                    {
                        //var par = SaveJobPara(orderReq.JobId.ToString(), orderReq.IsTemplate);
                        //if (par == "1")
                        //{
                        string _jobID = orderReq.JobId.ToString();
                        if ("0" != WorkflowSubmit(orderReq.JobId, userID, orderReq.Order.Remark, "", 0))
                        {
                            #region 更新商城订单状态
                            WfaEshopStatus thisinfo = new WfaEshopStatus();
                            thisinfo.JobId = orderReq.JobId;
                            thisinfo.UserId = userID;
                            thisinfo.SlpCode = sboID;
                            thisinfo.CardCode = orderReq.Order.CardCode;
                            thisinfo.CardName = orderReq.Order.CardName;
                            thisinfo.CurStatus = 0;
                            thisinfo.OrderPhase = "0000";
                            thisinfo.ShippingPhase = "0000";
                            thisinfo.CompletePhase = "0";
                            thisinfo.OrderLastDate = DateTime.Now;
                            thisinfo.FirstCreateDate = DateTime.Now;
                            //设置报价单提交
                            if (string.IsNullOrEmpty(orderReq.Order.U_New_ORDRID))
                            {
                                result = Eshop_OrderStatusFlow(thisinfo, billDelivery.billSalesDetails, 0);
                            }
                            else
                            {
                                if (orderReq.Order.U_New_ORDRID.Contains(","))
                                {
                                    string[] orderids = orderReq.Order.U_New_ORDRID.Split(',');
                                    result = Eshop_OrderStatusFlow(thisinfo, billDelivery.billSalesDetails, Convert.ToInt32(orderids[0]));
                                }
                                else
                                {
                                    result = Eshop_OrderStatusFlow(thisinfo, billDelivery.billSalesDetails, Convert.ToInt32(orderReq.Order.U_New_ORDRID));
                                }
                            }
                            #endregion
                        }
                        else { result = "0"; }
                        //}
                        //else { result = "0"; }
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            //  string log = string.Format("{1}：{0}", result, logstring);
            // AddUserOperateLog(log);
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
            //if (carCode != "")
            //{
            //    var crmOcrd = UnitWork.FindSingle<crm_ocrd>(zw => zw.sbo_id == sboID && zw.CardCode == carCode);
            //    if (crmOcrd != null)
            //    {
            //        carName = crmOcrd.CardName;
            //    }
            //}
            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> sqlParameters = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pJobName",    jobName),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pFuncID",     funcID),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pUserID",     userID),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pJobData",    jobdata),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pRemarks",    remarks),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pSboID",      sboID),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pCarCode",    carCode),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pCarName",    carName),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pDocTotal",   docTotal),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pBaseType",   baseType),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pBaseEntry",  baseEntry),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pAssemblyName",  assemblyName),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pClassName",  className)
            };
            code = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, "nsap_base.sp_process_build", CommandType.StoredProcedure, sqlParameters).ToString();
            return code;
        }
        /// <summary>
        /// 审核（提交）
        /// </summary>
        /// <returns>返回  提交失败 0   提交成功 1   流程完成 2</returns>
        private string OrderWorkflowSubmit(int jobID, int userID, string remarks, string cont, int auditor)
        {
            string code = "";
            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> sqlParameters = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pJobID",      jobID),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pUserID",     userID),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pRemarks",    remarks),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pCont",       cont),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pAuditor",    auditor)
            };
            code = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, "nsap_base.sp_process_submit", CommandType.StoredProcedure, sqlParameters).ToString();
            return code;
        }
        #region 保存审核参数
        /// <summary>
        /// 保存审核参数
        /// </summary>
        /// <returns></returns>
        public string SaveJobPara(string jobID, string setNumber)
        {
            string returns = "1";
            string para_val = setNumber == "" ? "1" : setNumber;
            string strSql = $@"INSERT INTO nsap_base.wfa_job_para (job_id,para_idx,para_val) VALUES({jobID},'1','{para_val}')";

            object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text);
            if (obj != null)
            {
                returns = obj.ToString();
            }
            return returns;
        }
        #endregion
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
                UnitWork.Save();
            }
            catch (Exception ex)
            {
                string errormsg = ex.Message;
            }
        }
        /// <summary>
        /// 修改审核数据
        /// </summary>
        public string UpdateAudit(int jobId, byte[] jobData, string remarks, string doc_total, string card_code, string card_name)
        {
            string isSave = "";
            string strSql = string.Format("UPDATE {0}.wfa_job SET job_data=?job_data,remarks='{1}',job_state={2},doc_total={3},", "nsap_base", remarks, "0", doc_total == "" ? "0" : doc_total);
            strSql += string.Format("card_code='{0}',card_name='{1}' WHERE job_id ={2}", card_code, card_name, jobId);
            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> sqlParameters = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?job_data",  jobData),

            };
            isSave = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, sqlParameters).ToString();
            return isSave == "" ? "true" : "false";
        }

        /// <summary>
        /// 审核（提交）
        /// </summary>
        /// <returns>返回  提交失败 0   提交成功 1   流程完成 2</returns>
        public string WorkflowSubmit(int jobID, int userID, string remarks, string cont, int auditor)
        {
            string code = "";
            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> sqlParameters = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pJobID",      jobID),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pUserID",     userID),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pRemarks",    remarks),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pCont",       cont),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pAuditor",    auditor)
            };
            code = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, "nsap_base.sp_process_submit", CommandType.StoredProcedure, sqlParameters).ToString();
            return code;
        }
        #region 驳回
        /// <summary>
        /// 审核（驳回）
        /// </summary>
        /// <returns>返回  驳回失败 0   驳回成功 1</returns>
        public string WorkflowReject(int jobID, int userID, string remarks, string cont, int goStepID)
        {
            string code = "";
            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> sqlParameters = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pJobID",      jobID),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pUserID",     userID),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pRemarks",    remarks),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pCont",       cont),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pGoStepID",    goStepID)
            };
            code = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, "nsap_base.sp_process_goback", CommandType.StoredProcedure, sqlParameters).ToString();
            return code;
        }
        #endregion
        #region 更新状态为未决
        /// <summary>
        /// 审核（未决）
        /// </summary>
        /// <returns>返回  失败 0   成功 1</returns>
        public string SavePanding(int jobID, int userID, string remarks)
        {
            string code = "";
            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> sqlParameters = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pJobID",      jobID),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pUserID",     userID),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pRemarks",    remarks)
            };
            code = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, "nsap_base.sp_process_pending", CommandType.StoredProcedure, sqlParameters).ToString();
            return code;
        }
        //删除已选择序列号
        public bool DeleteSerialNumber(string ItemCode, string SysNumber)
        {
            string strSql = string.Format(" DELETE FROM {0}.store_osrn_alreadyexists WHERE ItemCode = '{1}' AND SysNumber ='{2}'", "nsap_base", ItemCode, SysNumber);
            object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text);
            if (obj != null)
            {
                return int.Parse(obj.ToString()) > 0;
            }
            return false;
        }
        public DataTable GetItemOnhand(DataTable itemtab)
        {
            string strSql = string.Format("SELECT m.ItemCode,w.WhsCode,ISNULL(w.Onhand,'0') AS ItemOnhand,case when m.InvntItem='Y' then 1 else 0 end as InvntItem FROM OITW w inner join OITM m on w.ItemCode=m.ItemCode");
            if (itemtab != null && itemtab.Rows.Count > 0)
            {
                strSql += string.Format(" WHERE "); int i = 1;
                foreach (DataRow thisrow in itemtab.Rows)
                {
                    strSql += (i == 1 ? "" : " OR ") + string.Format(" (w.WhsCode='{0}' AND w.ItemCode='{1}')", thisrow["WhsCode"].ToString(), thisrow["ItemCode"].ToString().FilterSQL());
                    i++;
                }
            }
            return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql, CommandType.Text);
        }
        #endregion
        /// <summary>
        /// 客户代码数据
        /// </summary>
        /// <param name="CardCode"></param>
        /// <param name="SboID"></param>
        /// <param name="isSql"></param>
        /// <param name="ViewSelf"></param>
        /// <param name="ViewSelfDepartment"></param>
        /// <param name="ViewFull"></param>
        /// <param name="UserId"></param>
        /// <param name="DepId"></param>
        /// <returns></returns>
        public CardInfoDto CardInfo(string CardCode, int SboID, bool isSql, bool ViewSelf, bool ViewSelfDepartment, bool ViewFull, int UserId, int DepId)
        {
            var dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT sql_db,sql_name,sql_pswd,sap_name,sap_pswd,sql_conn,is_open FROM nsap_base.sbo_info WHERE sbo_id={SboID}", CommandType.Text, null);
            string dRowData = string.Empty;
            string isOpen = "0";
            string sboname = "0";
            string sqlconn = "0";
            if (dt.Rows.Count > 0)
            {
                isOpen = dt.Rows[0][6].ToString();
                sboname = dt.Rows[0][0].ToString();
                sqlconn = dt.Rows[0][5].ToString();
            }
            string filterString = string.Empty;
            if (ViewSelfDepartment && !ViewFull)
            {
                DataTable rDataRows = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT a.sale_id,a.tech_id FROM nsap_base.sbo_user a LEFT JOIN nsap_base.base_user_detail b ON a.user_id=b.user_id WHERE b.dep_id={DepId} AND a.sbo_id={SboID}", CommandType.Text, null); ;
                if (rDataRows.Rows.Count > 0)
                {
                    filterString += string.Format(" AND (a.SlpCode IN(");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][0]);
                    }
                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(") OR a.DfTcnician IN (");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][1]);
                    }
                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(") {0})", " OR a.SlpCode = -1 ");
                }

            }
            if (ViewSelf && !ViewFull && !ViewSelfDepartment)
            {
                DataTable rDataRowsSlp = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT sale_id,tech_id FROM nsap_base.sbo_user WHERE user_id={UserId} AND sbo_id={SboID}", CommandType.Text, null);
                if (rDataRowsSlp.Rows.Count > 0)
                {
                    string slpCode = rDataRowsSlp.Rows[0][0].ToString();
                    string slpTcnician = rDataRowsSlp.Rows[0][1].ToString();
                    string SlpCodeViewSelf = "";
                    if (CardCode.Substring(0, 1) == "V") { SlpCodeViewSelf = " OR a.SlpCode = -1"; } else { SlpCodeViewSelf = ""; }
                    filterString += string.Format(" AND (a.SlpCode = {0} OR a.DfTcnician={1} {2}) ", slpCode, slpTcnician, SlpCodeViewSelf);
                }
                else
                {
                    filterString += string.Format(" a.SlpCode =0  AND ");
                }
            }
            if (isSql && isOpen == "1")
            {
                if (string.IsNullOrEmpty(sboname))
                {
                    sboname = "";
                }
                else
                {
                    sboname = sboname + ".dbo.";
                }
                string U_FPLB = string.Empty;
                var syscolumn = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.SapDbContextType, $@"SELECT COUNT(*) value FROM syscolumns WHERE id=object_id('OCRD') AND name='U_FPLB'", CommandType.Text, null).FirstOrDefault();
                if (syscolumn != null && syscolumn.Value.ToString() != "0")
                {
                    U_FPLB = ",a.U_FPLB";
                }
                string strSql = string.Format("SELECT CardName,Currency,Building,MailBuildi{0},CntctPrsn,BillToDef AS PayToCode,ShipToDef AS ShipToCode,", U_FPLB);
                strSql += string.Format("(ISNULL(ZipCode,'')+ISNULL(b.Name,'')+ISNULL(c.Name,'')+ISNULL(City,'')+ISNULL(CONVERT(VARCHAR(1000),Building),'''')) AS Address,");
                strSql += string.Format("(ISNULL(MailZipCod,'')+ISNULL(d.Name,'')+ISNULL(e.Name,'')+ISNULL(MailCity,'')+ISNULL(CONVERT(VARCHAR(8000),MailBuildi),'''')) AS Address2,");
                strSql += string.Format("a.MailZipCod,a.State2,a.HsBnkIBAN,a.SlpCode AS U_YWY,a.QryGroup1 as IsTransport,U_is_reseller,U_EndCustomerName,U_EndCustomerContact FROM " + sboname + "OCRD a ");
                strSql += string.Format(" LEFT JOIN " + sboname + "OCRY b ON a.Country=b.Code");
                strSql += string.Format(" LEFT JOIN " + sboname + "OCST c ON a.State1=c.Code");
                strSql += string.Format(" LEFT JOIN " + sboname + "OCRY d ON a.MailCountr=d.Code");
                strSql += string.Format(" LEFT JOIN " + sboname + "OCST e ON a.State2=e.Code");
                strSql += string.Format(" WHERE CardCode='{0}'", CardCode);
                if (!string.IsNullOrEmpty(filterString))
                {
                    strSql += string.Format("{0}", filterString);
                }
                return UnitWork.ExcuteSql<CardInfoDto>(ContextType.SapDbContextType, strSql, CommandType.Text, null).FirstOrDefault();
            }
            else
            {
                filterString += string.Format(" AND a.sbo_id={0}", SboID);
                string strSql = string.Format("SELECT CardName,Currency,Building,MailBuildi,U_FPLB,CntctPrsn,CONCAT(IFNULL(a.ZipCode,''),IFNULL(b.Name,''),");
                strSql += string.Format("IFNULL(c.Name,''),IFNULL(a.City,''),IFNULL(a.Building,'')) AS Address,CONCAT(IFNULL(a.MailZipCod,''),IFNULL(d.Name,''),");
                strSql += string.Format("IFNULL(e.Name,''),IFNULL(a.MailCity,''),IFNULL(a.MailBuildi,'')) AS Address2,a.MailZipCod,a.State2,a.HsBnkIBAN,a.QryGroup1 as IsTransport,U_is_reseller,U_EndCustomerName,U_EndCustomerContact");
                strSql += string.Format(",a.SlpCode");
                strSql += string.Format(" FROM {0}.crm_ocrd a", "nsap_bone");
                strSql += string.Format(" LEFT JOIN {0}.store_ocry b ON a.Country=b.Code", "nsap_bone");
                strSql += string.Format(" LEFT JOIN {0}.store_ocst c ON a.State1=c.Code", "nsap_bone");
                strSql += string.Format(" LEFT JOIN {0}.store_ocry d ON a.MailCountr=d.Code", "nsap_bone");
                strSql += string.Format(" LEFT JOIN {0}.store_ocst e ON a.State2=e.Code", "nsap_bone");

                strSql += string.Format(" WHERE CardCode='{0}'", CardCode);
                if (!string.IsNullOrEmpty(filterString))
                {
                    strSql += string.Format("{0}", filterString);
                }
                return UnitWork.ExcuteSql<CardInfoDto>(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).FirstOrDefault();
            }
        }
        /// <summary>
        /// 物料数据获取
        /// </summary>
        /// <param name="query"></param>
        /// <param name="sboid"></param>
        /// <returns></returns>
        public TableData SalesItems(ItemRequest query, string sboid)
        {
            TableData tableData = new TableData();
            string sortString = string.Empty;
            string filterString = string.Empty;
            //lims推广员），只能选对应产品的对应物料编码。lims软件的物料编码： S111-SERVICE-LIMS  && u.Type == "LIMS"
            var currentUser = _auth.GetCurrentUser().User;
            var erpLims = UnitWork.Find<LimsInfo>(u => u.UserId == currentUser.Id && u.Type == "LIMS").ToList();
            //var lims1 = UnitWork.FromSql<LimsInfo>(" SELECT * from client_limsinfo where Type =\"LIMS\" AND UserId =\"" + currentUser.Id + "\" ").ToList();


            if (!string.IsNullOrEmpty(query.SortName) && !string.IsNullOrEmpty(query.SortOrder))
            {
                sortString = string.Format("{0} {1}", query.SortName.Replace("itemcode", "m.itemcode"), query.SortOrder.ToUpper());
            }
            if (!string.IsNullOrEmpty(query.ItemCode))
            {
                filterString += string.Format("(m.ItemCode LIKE '%{0}%' OR m.ItemName LIKE '%{0}%') AND ", query.ItemCode.FilterWildCard());
            }
            if (erpLims!=null)
            {
                filterString += string.Format(" (m.ItemCode = \"{0}\" ) AND  ", "S111-SERVICE-LIMS");
            }

            if (query.TypeId == "1")
            {
                filterString += string.Format("(m.ItemCode NOT LIKE 'CT%') AND ");
            }
            if (query.TypeId == "2")
            {
                filterString += string.Format("(m.ItemCode NOT LIKE 'CT%' AND m.ItemCode NOT LIKE 'CE%' AND m.ItemCode NOT LIKE 'CG%') AND ");
            }
            filterString += string.Format("w.WhsCode='{0}' AND m.sbo_id={1} AND ", query.WhsCode == "" ? "01" : query.WhsCode, sboid);
            if (!string.IsNullOrEmpty(filterString))
            {
                filterString = filterString.Substring(0, filterString.Length - 5);
            }
            StringBuilder tableName = new StringBuilder();
            StringBuilder filedName = new StringBuilder();
            filedName.Append("m.ItemCode,m.ItemName,IFNULL(c.high_price,0) AS high_price,IFNULL(c.low_price,0) AS low_price,w.OnHand,m.OnHand AS SumOnHand,m.IsCommited,m.OnOrder,(w.OnHand-w.IsCommited+w.OnOrder) AS OnAvailable,");
            filedName.Append("(m.OnHand-m.IsCommited+m.OnOrder) AS Available,w.WhsCode,IFNULL(U_TDS,'0') AS U_TDS,IFNULL(U_DL,0) AS U_DL,");
            filedName.Append("IFNULL(U_DY,0) AS U_DY,m.U_JGF,m.LastPurPrc,IFNULL(c.item_cfg_id,0) item_cfg_id,IFNULL(c.pic_path,m.PicturName) pic_path,");
            filedName.Append("((CASE m.QryGroup1 WHEN 'N' then 0 else 0.5 END)");
            filedName.Append("+(CASE m.QryGroup2 WHEN 'N' then 0 else 3 END)");
            filedName.Append("+(CASE m.QryGroup3 WHEN 'N' then 0 else 2 END)) AS QryGroup,c.item_desp,IFNULL(m.U_US,0) U_US,IFNULL(m.U_FS,0) U_FS,m.QryGroup3,m.SVolume,m.SWeight1,");
            filedName.Append("(CASE m.QryGroup1 WHEN 'N' THEN 0 ELSE '0.5' END) AS QryGroup1,");
            filedName.Append("(CASE m.QryGroup2 WHEN 'N' THEN 0 ELSE '3' END) AS QryGroup2,");
            filedName.Append("(CASE m.QryGroup3 WHEN 'N' THEN 0 ELSE '2' END) AS _QryGroup3,m.U_JGF1,IFNULL(m.U_YFCB,'0') U_YFCB,m.MinLevel,m.PurPackUn,c.item_counts,m.buyunitmsr");
            tableName.AppendFormat(" {0}.store_oitm m", "nsap_bone");
            tableName.AppendFormat(" LEFT JOIN {0}.store_oitw w ON m.ItemCode = w.ItemCode AND m.sbo_id=w.sbo_id ", "nsap_bone");
            tableName.AppendFormat(" LEFT JOIN {0}.base_item_cfg c ON m.ItemCode = c.ItemCode AND type_id={1} ", "nsap_bone", query.TypeId);
            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> sqlParameters = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("pTableName",tableName.ToString()),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("pFieldName",filedName.ToString()),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("pPageSize",query.limit),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("pPageIndex",query.page),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("pStrOrder",sortString),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("pStrWhere",filterString)
            };
            MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter isStats = new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("@pIsTotal", SqlDbType.Int);
            isStats.Value = 1;
            sqlParameters.Add(isStats);
            MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter paramOut = new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("@rowsCount", SqlDbType.Int);
            paramOut.Value = 0;
            paramOut.Direction = ParameterDirection.Output;
            sqlParameters.Add(paramOut);
            DataTable dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"nsap_base.sp_common_pager", CommandType.StoredProcedure, sqlParameters);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow tempr in dt.Rows)
                {
                    tempr["OnHand"] = 0;
                    tempr["SumOnHand"] = 0;
                    tempr["IsCommited"] = 0;
                    tempr["OnOrder"] = 0;
                    tempr["OnAvailable"] = 0;
                    tempr["Available"] = 0;
                }
                DataTable dtsbo = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT sql_db,sql_name,sql_pswd,sap_name,sap_pswd,sql_conn,is_open FROM nsap_base.sbo_info WHERE sbo_id={sboid}", CommandType.Text, null);
                string IsOpen = "0";
                if (dtsbo.Rows.Count > 0)
                {
                    IsOpen = dtsbo.Rows[0]["is_open"].ToString();
                }
                if (IsOpen == "1")
                {
                    List<string> itemCodeList = (from d in dt.AsEnumerable() select '\'' + d.Field<string>("ItemCode").FilterWildCard() + '\'').ToList();
                    string itemCodes = string.Join(",", itemCodeList);
                    string tempsql = string.Format(@"select m.ItemCode,w.OnHand,m.OnHand AS SumOnHand,m.IsCommited,m.OnOrder,(w.OnHand-w.IsCommited+w.OnOrder) AS OnAvailable,(m.OnHand-m.IsCommited+m.OnOrder) AS Available 
                                              from OITM M LEFT OUTER JOIN OITW W ON m.ItemCode = w.ItemCode where m.ItemCode in({0}) and w.WhsCode={1}", itemCodes, query.WhsCode);
                    DataTable tempt = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, tempsql, CommandType.Text, null);
                    if (tempt != null && tempt.Rows.Count > 0)
                    {
                        foreach (DataRow tempr in dt.Rows)
                        {
                            for (int i = 0; i < tempt.Rows.Count; i++)
                            {
                                if (tempt.Rows[i]["ItemCode"].ToString().Equals(tempr["ItemCode"].ToString()))
                                {
                                    tempr["OnHand"] = tempt.Rows[i]["OnHand"] == null ? 0 : tempt.Rows[i]["OnHand"];
                                    tempr["SumOnHand"] = tempt.Rows[i]["SumOnHand"] == null ? 0 : tempt.Rows[i]["SumOnHand"];
                                    tempr["IsCommited"] = tempt.Rows[i]["IsCommited"] == null ? 0 : tempt.Rows[i]["IsCommited"];
                                    tempr["OnOrder"] = tempt.Rows[i]["OnOrder"] == null ? 0 : tempt.Rows[i]["OnOrder"];
                                    tempr["OnAvailable"] = tempt.Rows[i]["OnAvailable"] == null ? 0 : tempt.Rows[i]["OnAvailable"];
                                    tempr["Available"] = tempt.Rows[i]["Available"] == null ? 0 : tempt.Rows[i]["Available"];
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            tableData.Data = dt.Tolist<SaleItemDto>();
            tableData.Count = Convert.ToInt32(paramOut.Value);
            return tableData;
        }
        /// <summary>
        /// 包材物料数据获取
        /// </summary>
        /// <param name="query"></param>
        /// <param name="sboid"></param>
        /// <returns></returns>
        public TableData PackingMaterialSaleItem(ItemRequest query, string sboid)
        {
            TableData tableData = new TableData();
            string sortString = string.Empty;
            string filterString = string.Empty;
            if (!string.IsNullOrEmpty(query.SortName) && !string.IsNullOrEmpty(query.SortOrder))
            {
                sortString = string.Format("{0} {1}", query.SortName.Replace("itemcode", "m.itemcode"), query.SortOrder.ToUpper());
            }
            if (!string.IsNullOrEmpty(query.ItemCode))
            {
                filterString += string.Format("m.ItemCode LIKE '%{0}%' AND ", query.ItemCode.FilterWildCard());
            }
            filterString += string.Format("(m.ItemCode ='{0}' OR ", "F02-003-BTS-1U");
            filterString += string.Format("m.ItemCode = '{0}'  OR ", "F02-003-BTS-3U");
            filterString += string.Format("m.ItemCode = '{0}' OR ", "F02-003-BTS-3U3F-MX");
            filterString += string.Format("m.ItemCode = '{0}' OR ", "F02-003-BTS-6U");
            filterString += string.Format("m.ItemCode = '{0}' OR ", "F02-003-BVIR");
            filterString += string.Format("m.ItemCode = '{0}' OR ", "F02-003-QPD-280-780");
            filterString += string.Format("m.ItemCode = '{0}' OR ", "F02-003-QZD-1U");
            filterString += string.Format("m.ItemCode = '{0}' OR ", "F02-003-QZD-3U");
            filterString += string.Format("m.ItemCode = '{0}' OR ", "F02-003-ZZM");
            filterString += string.Format("m.ItemCode = '{0}' OR ", "F02-003-ZZM-1U");
            filterString += string.Format("m.ItemCode = '{0}' OR ", "E03-LBD");
            filterString += string.Format("m.ItemCode = '{0}' ) AND ", "F02-003-ZZM-BVIR");
            if (query.TypeId == "1")
            {
                filterString += string.Format("(m.ItemCode NOT LIKE 'CT%') AND ");
            }
            if (query.TypeId == "2")
            {
                filterString += string.Format("(m.ItemCode NOT LIKE 'CT%' AND m.ItemCode NOT LIKE 'CE%' AND m.ItemCode NOT LIKE 'CG%') AND ");
            }
            filterString += string.Format(" w.WhsCode = '37' AND m.sbo_id={0} AND ", sboid);
            if (!string.IsNullOrEmpty(filterString))
            {
                filterString = filterString.Substring(0, filterString.Length - 5);
            }
            StringBuilder tableName = new StringBuilder();
            StringBuilder filedName = new StringBuilder();
            filedName.Append("m.ItemCode,m.ItemName,IFNULL(c.high_price,0) AS high_price,IFNULL(c.low_price,0) AS low_price,w.OnHand,m.OnHand AS SumOnHand,m.IsCommited,m.OnOrder,(w.OnHand-w.IsCommited+w.OnOrder) AS OnAvailable,");
            filedName.Append("(m.OnHand-m.IsCommited+m.OnOrder) AS Available,w.WhsCode,IFNULL(U_TDS,'0') AS U_TDS,IFNULL(U_DL,0) AS U_DL,");
            filedName.Append("IFNULL(U_DY,0) AS U_DY,m.U_JGF,m.LastPurPrc,IFNULL(c.item_cfg_id,0) item_cfg_id,IFNULL(c.pic_path,m.PicturName) pic_path,");
            filedName.Append("((CASE m.QryGroup1 WHEN 'N' then 0 else 0.5 END)");
            filedName.Append("+(CASE m.QryGroup2 WHEN 'N' then 0 else 3 END)");
            filedName.Append("+(CASE m.QryGroup3 WHEN 'N' then 0 else 2 END)) AS QryGroup,c.item_desp,IFNULL(m.U_US,0) U_US,IFNULL(m.U_FS,0) U_FS,m.QryGroup3,m.SVolume,m.SWeight1,");
            filedName.Append("(CASE m.QryGroup1 WHEN 'N' THEN 0 ELSE '0.5' END) AS QryGroup1,");
            filedName.Append("(CASE m.QryGroup2 WHEN 'N' THEN 0 ELSE '3' END) AS QryGroup2,");
            filedName.Append("(CASE m.QryGroup3 WHEN 'N' THEN 0 ELSE '2' END) AS _QryGroup3,m.U_JGF1,IFNULL(m.U_YFCB,'0') U_YFCB,m.MinLevel,m.PurPackUn,c.item_counts,m.buyunitmsr");
            tableName.AppendFormat(" {0}.store_oitm m", "nsap_bone");
            tableName.AppendFormat(" LEFT JOIN {0}.store_oitw w ON m.ItemCode = w.ItemCode AND m.sbo_id=w.sbo_id ", "nsap_bone");
            tableName.AppendFormat(" LEFT JOIN {0}.base_item_cfg c ON m.ItemCode = c.ItemCode AND type_id={1} ", "nsap_bone", query.TypeId);
            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> sqlParameters = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("pTableName",tableName.ToString()),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("pFieldName",filedName.ToString()),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("pPageSize",query.limit),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("pPageIndex",query.page),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("pStrOrder",sortString),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("pStrWhere",filterString)
            };
            MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter isStats = new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("@pIsTotal", SqlDbType.Int);
            isStats.Value = 1;
            sqlParameters.Add(isStats);
            MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter paramOut = new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("@rowsCount", SqlDbType.Int);
            paramOut.Value = 0;
            paramOut.Direction = ParameterDirection.Output;
            sqlParameters.Add(paramOut);
            DataTable dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"nsap_base.sp_common_pager", CommandType.StoredProcedure, sqlParameters);
            DataTable dtsbo = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT sql_db,sql_name,sql_pswd,sap_name,sap_pswd,sql_conn,is_open FROM nsap_base.sbo_info WHERE sbo_id={sboid}", CommandType.Text, null); ;
            string IsOpen = "0";
            if (dtsbo.Rows.Count > 0)
            {
                IsOpen = dtsbo.Rows[0]["is_open"].ToString();
            }
            if (IsOpen == "1")
            {
                foreach (DataRow tempr in dt.Rows)
                {
                    string tempsql = string.Format(@"select w.OnHand,m.OnHand AS SumOnHand,m.IsCommited,m.OnOrder,(w.OnHand-w.IsCommited+w.OnOrder) AS OnAvailable,(m.OnHand-m.IsCommited+m.OnOrder) AS Available 
                                              from OITM M LEFT OUTER JOIN OITW W ON m.ItemCode = w.ItemCode where m.ItemCode='{0}' and w.WhsCode={1}", tempr["ItemCode"].ToString().FilterWildCard(), tempr["WhsCode"].ToString());
                    DataTable tempt = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, tempsql, CommandType.Text, null);
                    if (tempt.Rows.Count > 0)
                    {
                        tempr["OnHand"] = tempt.Rows[0]["OnHand"] == null ? 0 : tempt.Rows[0]["OnHand"];
                        tempr["SumOnHand"] = tempt.Rows[0]["SumOnHand"] == null ? 0 : tempt.Rows[0]["SumOnHand"];
                        tempr["IsCommited"] = tempt.Rows[0]["IsCommited"] == null ? 0 : tempt.Rows[0]["IsCommited"];
                        tempr["OnOrder"] = tempt.Rows[0]["OnOrder"] == null ? 0 : tempt.Rows[0]["OnOrder"];
                        tempr["OnAvailable"] = tempt.Rows[0]["OnAvailable"] == null ? 0 : tempt.Rows[0]["OnAvailable"];
                        tempr["Available"] = tempt.Rows[0]["Available"] == null ? 0 : tempt.Rows[0]["Available"];
                    }
                }
            }
            tableData.Data = dt.Tolist<SaleItemDto>();
            tableData.Count = Convert.ToInt32(paramOut.Value);
            return tableData;
        }
        /// <summary>
        /// 获取物料配置清单
        /// </summary>
        /// <param name="ItemCode"></param>
        /// <param name="WhsCode"></param>
        public TableData GetItemConfigList(string ItemCode, string WhsCode)
        {
            TableData tableData = new TableData();
            if (!string.IsNullOrEmpty(ItemCode))
            {
                string sql = $@"SELECT ROW_NUMBER() OVER (ORDER BY a.ItemCode) RowNum, a.ItemCode,a.item_name as ItemName,a.high_price,a.low_price,w.OnHand,m.OnHand AS SumOnHand,m.IsCommited,m.OnOrder,
                        (w.OnHand-w.IsCommited+w.OnOrder) AS OnAvailable,
                        (m.OnHand-m.IsCommited+m.OnOrder) AS Available,
                                                w.WhsCode,a.Factor_1,a.Factor_2,a.Factor_3,
                        IFNULL(U_TDS,0) AS U_TDS,
                        IFNULL(U_DL,0) AS U_DL,
                        IFNULL(U_DY,0) AS U_DY,m.U_JGF,
                        IFNULL(m.LastPurPrc,0) LastPurPrc,
                                                a.item_cfg_id,'' AS PicturName,b.type_id,
                        ((IFNULL((CASE m.QryGroup1 WHEN 'N' THEN 0 ELSE '0.5' END),0))+(IFNULL((CASE m.QryGroup2 WHEN 'N' THEN 0 ELSE '3' END),0))+(IFNULL((CASE m.QryGroup3 WHEN 'N' THEN 0 ELSE '2' END),0))) AS QryGroup,
                                                b.item_counts,0 AS IsTimes,
                        IFNULL(m.U_US,0) U_US,
                        IFNULL(m.U_FS,0) U_FS,m.QryGroup3,m.SVolume,m.SWeight1,
                        (IFNULL((CASE m.QryGroup1 WHEN 'N' THEN 0 ELSE '0.5' END),0)) AS QryGroup1,
                        (IFNULL((CASE m.QryGroup2 WHEN 'N' THEN 0 ELSE '3' END),0)) AS QryGroup2,
                                                (IFNULL((CASE m.QryGroup3 WHEN 'N' THEN 0 ELSE '2' END),0)) AS _QryGroup3,
                        IFNULL(m.U_JGF1,'0') U_JGF1,
                        IFNULL(m.U_YFCB,'0') U_YFCB,
                        m.MinLevel,m.PurPackUn,m.buyunitmsr 
                        FROM nsap_bone.base_item_cfg_detail a  
                        LEFT JOIN nsap_bone.base_item_cfg b ON a.item_cfg_id=b.item_cfg_id 
                        LEFT JOIN nsap_bone.store_oitm m ON m.ItemCode=a.ItemCode AND m.sbo_id=1 
                        LEFT JOIN nsap_bone.store_oitw w ON w.ItemCode=a.ItemCode AND w.WhsCode='{WhsCode}' AND m.sbo_id=w.sbo_id 
                        WHERE a.item_cfg_id={ItemCode}";
                DataTable dataTable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
                tableData.Data = dataTable.Tolist<SaleItemDto>();
            }
            return tableData;
        }
        /// <summary>
        /// 物料数据获取
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public TableData RelORDR(RelORDRRequest query)
        {
            TableData tableData = new TableData();
            string sortString = string.Empty;
            string filterString = "(Canceled = 'Y' or DocStatus = 'O') and SlpCode =" + query.SlpCode;
            if (!string.IsNullOrEmpty(query.SortName) && !string.IsNullOrEmpty(query.SortOrder))
            {
                sortString = string.Format("{0} {1}", query.SortName.Replace("cardcode", "a.cardcode"), query.SortOrder.ToUpper());
            }
            else
            {
                sortString = " docentry desc";
            }
            string dRowData = string.Empty;

            if (!string.IsNullOrEmpty(query.DocEntry))
            {
                filterString += "and docentry=" + query.DocEntry;
            }
            if (!string.IsNullOrEmpty(query.CardCode))
            {
                filterString += string.Format("and cardcode LIKE '%{0}%'", query.CardCode.FilterSQL().Trim());
            }
            SqlParameter paramOut;
            DataTable dt;
            SqlStore("ORDR", "docentry,cardcode,doctotal,CreateDate,docstatus,Printed,CANCELED,Comments", query.limit, query.page, sortString, filterString, out paramOut, out dt);
            tableData.Data = dt.Tolist<RelORDRRDto>();
            tableData.Count = Convert.ToInt32(paramOut.Value);
            return tableData;
        }
        public List<CardCodeCheckDto> GetCardCodeCheck(string type, string q, bool ViewFull, bool ViewSelf, int UserId, int SboId, bool ViewSelfDepartment, int DepId)
        {
            DataTable dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT sql_db,sql_name,sql_pswd,sap_name,sap_pswd,sql_conn,is_open FROM nsap_base.sbo_info WHERE sbo_id={SboId}", CommandType.Text, null);
            string dRowData = string.Empty; string isOpen = "0";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); }
            string strWhere = string.Empty;
            if (ViewSelfDepartment && !ViewFull)
            {
                DataTable rDataRows = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT a.sale_id,a.tech_id FROM nsap_base.sbo_user a LEFT JOIN nsap_base.base_user_detail b ON a.user_id = b.user_id WHERE b.dep_id in ({DepId}) AND a.sbo_id = {SboId}", CommandType.Text, null);
                if (rDataRows.Rows.Count > 0)
                {
                    strWhere += string.Format(" AND (SlpCode IN(");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        strWhere += string.Format("{0},", rDataRows.Rows[i][0]);
                    }
                    if (!string.IsNullOrEmpty(strWhere))
                        strWhere = strWhere.Substring(0, strWhere.Length - 1);
                    strWhere += string.Format(") OR DfTcnician IN (");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        strWhere += string.Format("{0},", rDataRows.Rows[i][1]);
                    }
                    if (!string.IsNullOrEmpty(strWhere))
                        strWhere = strWhere.Substring(0, strWhere.Length - 1);
                    strWhere += string.Format(")) ");
                }

            }
            if (ViewSelf && !ViewFull && !ViewSelfDepartment)
            {
                DataTable rDataRowsSlp = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT sale_id,tech_id FROM nsap_base.sbo_user WHERE user_id={UserId} AND sbo_id={SboId}", CommandType.Text, null);
                if (rDataRowsSlp.Rows.Count > 0)
                {
                    string slpTcnician = rDataRowsSlp.Rows[0][1].ToString();
                    string slpCode = rDataRowsSlp.Rows[0][0].ToString();
                    string SlpCodeViewSelf = "";
                    if (type == "P") { SlpCodeViewSelf = " OR a.SlpCode = -1"; } else { SlpCodeViewSelf = ""; }
                    strWhere += string.Format(" AND (SlpCode = {0} OR DfTcnician={1} {2}) ", slpCode, slpTcnician, SlpCodeViewSelf);
                }
                else
                {
                    strWhere += string.Format(" AND SlpCode =0 ");
                }
            }
            string strSql = string.Format("SELECT TOP 10 CardCode,CardName,(DATALENGTH(CardCode)-DATALENGTH('{0}')) as CardCodelike FROM OCRD ", q);

            #region 根据不同的单据类型获取不同的业务伙伴
            if (!string.IsNullOrEmpty(type))
            {
                if (type == "SQO")//销售报价单\订单
                {
                    strSql += string.Format(" WHERE (CardType='C' OR CardType='L') AND (CardCode LIKE '%{0}%' OR CardName LIKE '%{0}%')", q.Replace('*', '%'));
                }
                else if (type == "SDR")//销售交货\退货,应收发票\贷项凭证
                {
                    strSql += string.Format(" WHERE CardType='C' AND (CardCode LIKE '%{0}%' OR CardName LIKE '%{0}%')", q.Replace('*', '%'));
                }
                else if (type == "P")//采购
                {
                    strSql += string.Format(" WHERE CardType='S' AND (CardCode LIKE '%{0}%' OR CardName LIKE '%{0}%')", q.Replace('*', '%'));
                }
                else if (type == "ST")//库存转储
                {
                    strSql += string.Format(" WHERE (CardType='S' OR CardType='C') AND frozenFor='N' AND (CardCode LIKE '%{0}%' OR CardName LIKE '%{0}%')", q);
                }
            }
            #endregion
            if (!string.IsNullOrEmpty(strWhere)) { strSql += string.Format("{0}", strWhere); }
            strSql += string.Format("GROUP BY CardCode,CardName ORDER BY CardCodelike");
            return UnitWork.ExcuteSql<CardCodeCheckDto>(ContextType.SapDbContextType, strSql, CommandType.Text, null);
        }
        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="filefName"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sort"></param>
        /// <param name="where"></param>
        /// <param name="paramOut"></param>
        /// <param name="dt"></param>
        private void SqlStore(string tableName, string filefName, int pageSize, int pageIndex, string sort, string where, out SqlParameter paramOut, out DataTable dt)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>()
            {
                new SqlParameter("@strFrom",tableName),
                new SqlParameter("@strSelect",filefName),
                new SqlParameter("@pageSize",pageSize),
                new SqlParameter("@pageIndex",pageIndex),
                new SqlParameter("@strOrder",sort),
                new SqlParameter("@strWhere",where),
            };
            SqlParameter isStats = new SqlParameter("@isStats", SqlDbType.Int);
            isStats.Value = 1;
            sqlParameters.Add(isStats);
            paramOut = new SqlParameter("@rowCount", SqlDbType.Int);
            paramOut.Value = 0;
            paramOut.Direction = ParameterDirection.Output;
            sqlParameters.Add(paramOut);
            dt = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, $"sp_common_pager", CommandType.StoredProcedure, sqlParameters);
        }

        /// <summary>
        /// 科目代码查询（服务）
        /// </summary>
        /// <returns></returns>
        public TableData SelectAcctCodeView(OactRequest query)
        {
            TableData tableData = new TableData();
            int UserID = _serviceBaseApp.GetUserNaspId();
            int SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            DataTable dt = _serviceBaseApp.GetSboNamePwd(SboID);
            string isOpen = "0";
            if (dt.Rows.Count > 0)
            {
                isOpen = dt.Rows[0][6].ToString();
            }
            string sortString = string.Empty;
            string filterString = string.Empty;
            string line = string.Empty;
            if (!string.IsNullOrEmpty(query.SortName) && !string.IsNullOrEmpty(query.SortOrder))
            {
                sortString = string.Format("{0} {1}", query.SortName, query.SortOrder.ToUpper());
            }
            #region 搜索条件
            if (!string.IsNullOrEmpty(query.key))
            {
                string[] fields = query.key.Split('`');
                string[] p = fields[0].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("AcctCode LIKE '%{0}%' AND ", p[1].FilterWildCard());
                }
                p = fields[1].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("AcctName LIKE '%{0}%' AND ", p[1].FilterWildCard());
                }
            }
            #endregion
            if (!string.IsNullOrEmpty(filterString))
            {
                filterString = filterString.Substring(0, filterString.Length - 5);
            }
            if (isOpen == "1")
            {
                StringBuilder filedName = new StringBuilder();
                StringBuilder tableName = new StringBuilder();
                filedName.Append(" AcctCode,AcctName,CurrTotal,Details");
                tableName.AppendFormat(" OACT ");
                StringBuilder strWhere = new StringBuilder();
                strWhere.Append(" (AcctCode NOT IN (SELECT AcctCode FROM OACT WHERE Postable='N' OR frozenFor='Y' OR validTo<GETDATE() OR frozenFrom>GETDATE()))");
                if (!string.IsNullOrEmpty(filterString))
                {
                    strWhere.AppendFormat(" AND {0}", filterString);
                }
                SqlParameter paramOut;
                DataTable dts;
                SqlStore(tableName.ToString(), filedName.ToString(), query.limit, query.page, sortString, strWhere.ToString(), out paramOut, out dts);
                tableData.Data = dts.Tolist<OactDto>();
                tableData.Count = Convert.ToInt32(paramOut.Value);
                return tableData;
            }
            else
            {
                MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter paramOut;
                StringBuilder tableName = new StringBuilder();
                StringBuilder filedName = new StringBuilder();
                filedName.Append(" '',AcctCode,AcctName,CurrTotal,Details");
                tableName.AppendFormat(" {0}.finance_oact ", "nsap_bone");
                StringBuilder strWhere = new StringBuilder();
                strWhere.Append(" (AcctCode NOT IN (SELECT AcctCode FROM nsap_bone.finance_oact WHERE Postable='N' OR frozenFor='Y' OR validTo<NOW() OR frozenFrom>NOW()))");
                if (!string.IsNullOrEmpty(filterString))
                {
                    strWhere.AppendFormat(" AND {0}", filterString);
                }
                List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> sqlParameters = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
                    {
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("@pTableName",tableName.ToString()),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("@pFieldName",filedName.ToString()),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("@pPageSize",query.limit),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("@pPageIndex",query.page),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("@pStrOrder",sortString),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("@pIsTotal",1),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("@pStrWhere",strWhere.ToString()),
                    };
                paramOut = new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("@rowsCount", SqlDbType.Int);
                paramOut.Value = 0;
                paramOut.Direction = ParameterDirection.Output;
                sqlParameters.Add(paramOut);
                dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, string.Format("{0}.sp_common_pager", "nsap_base"), CommandType.StoredProcedure, sqlParameters);
                tableData.Data = dt.Tolist<OactDto>();
                tableData.Count = Convert.ToInt32(paramOut.Value);
            }
            return tableData;
        }
        /// <summary>
        /// 查询销售员所选账套所有客户科目余额与百分比数据
        /// </summary>
        /// <returns></returns>
        public DataTable GetSalesSboBalPercent(string slpCode, int SboId)
        {
            DataTable dataTable = new DataTable();
            string sql = $"SELECT * FROM sbo_info WHERE sbo_id= {SboId}";
            sbo_info sboInfo = UnitWork.ExcuteSql<sbo_info>(ContextType.NsapBaseDbContext, sql, CommandType.Text, null)?.FirstOrDefault();
            if (sboInfo != null && sboInfo.is_open)
            {
                //账套欠款（SAP）
                string strSql = $@"select ttotal.Total,isnull((select sum(balance) from ocrd where SlpCode={slpCode}),0) as BalDue from (
                                   select sum(isnull(ocrdbal.INVtotal,0)-isnull(ocrdbal.RINtotal,0)) as Total from ( select
                                  (select sum(DocTotal) from OINV WHERE CANCELED = 'N' and CardCode=C.CardCode) as INVtotal
                                  ,(select SUM(DocTOTal) from ORIN where CANCELED='N' and CardCode=c.CardCode) as RINtotal
                                  FROM OCRD C WHERE C.SlpCode={slpCode} ) as ocrdbal) as ttotal ";
                dataTable = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql, CommandType.Text, null);
            }
            else
            {
                //账套欠款
                string strSql = $@"select ocrdtotal.Total,IFNULL((select sum(balance) from nsap_bone.crm_ocrd_oldsbo_balance where sbo_id={SboId} and slpname=(select SlpName from nsap_bone.crm_oslp where SlpCode={slpCode} LIMIT 1)),0) as BalDue
                                                from (select sum(ifnull(ocrdbal.INVtotal,0)-ifnull(ocrdbal.RINtotal,0)) as Total from (
                                                SELECT (select sum(DocTotal) from nsap_bone.sale_oinv WHERE CANCELED = 'N' AND sbo_id=C.sbo_id and CardCode=C.CardCode) as INVtotal
                                                ,(select SUM(DocTOTal) from nsap_bone.sale_orin where CANCELED = 'N' and sbo_id=C.sbo_id and CardCode=C.CardCode) AS RINtotal 
                                                FROM nsap_bone.crm_ocrd C
                                                WHERE C.sbo_id={SboId} and C.SlpCode ={slpCode} ) as ocrdbal) as ocrdtotal";
                dataTable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            }
            return dataTable;
        }


        /// <summary>
        /// 查询指定业务伙伴的科目余额与百分比数据
        /// </summary>
        /// <param name="CardCode"></param>
        /// <param name="SboId"></param>
        /// <returns></returns>
        public DataTable GetClientSboBalPercent(string CardCode, int SboId)
        {
            DataTable dataTable = new DataTable();
            string sql = $"SELECT * FROM sbo_info WHERE sbo_id= {SboId}";
            sbo_info sboInfo = UnitWork.ExcuteSql<sbo_info>(ContextType.NsapBaseDbContext, sql, CommandType.Text, null)?.FirstOrDefault();
            if (sboInfo != null && sboInfo.is_open)
            {
                string strSql = $@"SELECT (Select sum(Balance) from OCRD where CardCode='{CardCode}') as Balance
                                  ,(select sum(DocTotal) from OINV WHERE CANCELED ='N' and CardCode='{CardCode}') as INVtotal
                                  ,(select SUM(DocTOTal) from ORIN where CANCELED<>'Y' and CardCode='{CardCode}') as RINtotal
                                --90天内未清收款
                                ,(select SUM(openBal) from ORCT WHERE CANCELED='N' AND openBal<>0 AND CardCode='{CardCode}' and datediff(DAY,docdate,getdate())<=90) as RCTBal90
                                --90天内未清发票金额
                                ,(select SUM(DocTotal-PaidToDate) from OINV WHERE CANCELED ='N' and CardCode='{CardCode}' and DocTotal-PaidToDate>0 and datediff(DAY,docdate,getdate())<=90) as INVBal90
                                --90天内未清贷项金额
                                ,(select SUM(DocTotal-PaidToDate) from ORIN where CANCELED ='N' and CardCode='{CardCode}' and DocTotal-PaidToDate>0 and datediff(DAY,docdate,getdate())<=90) as RINBal90
                                --90天前未清发票的发票总额
                                ,(select SUM(DocTotal) from OINV WHERE CANCELED ='N' and CardCode = '{CardCode}' and DocTotal-PaidToDate > 0 and datediff(DAY, docdate, getdate())> 90) as INVTotal90P
                ";
                //--90天前未清收款
                //,(select SUM(openBal) from ORCT WHERE CANCELED = 'N' AND openBal<>0 AND CardCode = @cardcode and datediff(DAY, docdate, getdate())> 90) as RCTBal90P
                //--90天前未清发票金额
                //,(select SUM(DocTotal - PaidToDate) from OINV WHERE CANCELED ='N' and CardCode = @cardcode and DocTotal-PaidToDate > 0 and datediff(DAY, docdate, getdate())> 90) as INVBal90P
                //--90天前未清贷项金额
                //,(select SUM(DocTotal - PaidToDate) from ORIN where CANCELED ='N' and CardCode = @cardcode and DocTotal-PaidToDate > 0 and datediff(DAY, docdate, getdate())> 90) as RINBal90P
                dataTable = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql, CommandType.Text, null);
            }
            else
            {
                string strSql = $@"SELECT(Select sum(Balance) from nsap_bone.crm_ocrd_oldsbo_balance where sbo_id={SboId}  and CardCode = '{CardCode}') as Balance
                                               , (select sum(DocTotal) from nsap_bone.sale_oinv WHERE CANCELED ='N' and sbo_id={SboId} and CardCode = '{CardCode}') as INVtotal
                                               ,(select SUM(DocTOTal) from nsap_bone.sale_orin where CANCELED ='N' and sbo_id={SboId} and CardCode = '{CardCode}') as RINtotal
                                            ,'' as RCTBal90
                                            ,'' as INVBal90
                                            ,'' as RINBal90
                                            ,'' as INVTotal90P
                                            ";
                //,'' as RCTBal90P
                //,'' as INVBal90P
                //,'' as RINBal90P

                dataTable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            }
            return dataTable;
        }

        /// <summary>
        /// 构建销售报价单草稿
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public billDelivery BulidBillDelivery(OrderDraft order)
        {
            int userID = _serviceBaseApp.GetUserNaspId();
            int sboID = _serviceBaseApp.GetUserNaspSboID(userID);
            int funcId = 50;
            billDelivery billDelivery = new billDelivery()
            {
                Address = order.Address,
                Address2 = order.Address2,
                billBaseType = "-5",
                billBaseEntry = "-1",
                CardName = order.CardName,//供应商名称

                CardCode = !string.IsNullOrEmpty(order.CardCode) ? order.CardCode : "",
                Comments = order.Comments.Replace("'", "\'"),//备注
                CurSource = order.CurSource,//货币类型
                CustomFields = !string.IsNullOrEmpty(order.CustomFields) ? order.CustomFields.Replace(" ", "").Replace("　", "") : "",//  $"U_ShipName≮1≯≮0≯U_SCBM≮1≯P3-陈友祥",
                BeforeDiscSum = !string.IsNullOrEmpty(order.BeforeDiscSum) ? order.BeforeDiscSum : "0.0",// 折扣前总计
                DiscSum = !string.IsNullOrEmpty(order.DiscSum.ToString()) ? order.DiscSum.ToString() : "0",//折扣金额
                DiscPrcnt = !string.IsNullOrEmpty(order.DiscPrcnt.ToString()) ? order.DiscPrcnt.ToString() : "0.0",//折扣（折扣率）
                DocTotal = order.DocTotal.ToString(),//折扣后总价
                                                     //付款条件------------------------
                GoodsToDay = !string.IsNullOrEmpty(order.GoodsToDay) ? order.GoodsToDay : "0",//货到付百分比
                PrepaPro = !string.IsNullOrEmpty(order.PrepaPro) ? order.PrepaPro : "0.0",//预付百分比
                PayBefShip = !string.IsNullOrEmpty(order.PayBefShip) ? order.PayBefShip : "0.0",//发货前付
                GoodsToPro = !string.IsNullOrEmpty(order.GoodsToPro) ? order.GoodsToPro : "0.0",//货到付百分比
                DocCur = order.DocCur,
                U_ERPFrom = "5",//来源4.0系统
                DocDate = order.DocDate.ToString(),
                DocDueDate = order.DocDueDate.ToString(),
                DocRate = order.DocRate.ToString(),
                DocStatus = "O",
                DocType = order.DocType,
                GoodsToDate = order.GoodsToDate,
                FuncId = funcId.ToString(),
                GroupNum = !string.IsNullOrEmpty(order.GroupNum.ToString()) ? order.GroupNum.ToString() : "0",
                Indicator = order.Indicator,
                LicTradNum = "0",//国税编号 许可的经销商号
                NumAtCard = order.NumAtCard,
                OwnerCode = order.OwnerCode != 0 ? order.OwnerCode.ToString() : "",//
                PartSupply = order.PartSupply,
                PayToCode = order.PayToCode,
                PeyMethod = order.PeyMethod,
                PrepaData = order.PrepaData,
                Printed = "N",//未清
                Remark = order.Remark.Replace("'", "\'"),
                SboId = sboID.ToString(),
                ShipToCode = order.ShipToCode,
                SlpCode = order.SlpCode.ToString(),
                U_YWY = order.U_YWY,
                TaxDate = order.TaxDate.ToString(),
                TotalExpns = !string.IsNullOrEmpty(order.TotalExpns.ToString()) ? order.TotalExpns.ToString() : "0",
                TrnspCode = !string.IsNullOrEmpty(order.TrnspCode.ToString()) ? order.TrnspCode.ToString() : "0",
                U_FPLB = order.U_FPLB,
                U_SL = order.U_SL,
                UserId = userID.ToString(),
                VatGroup = order.VatGroup,
                VatSum = !string.IsNullOrEmpty(order.VatSum.ToString()) ? order.VatSum.ToString() : "0",
                WhsCode = order.WhsCode,
                CntctCode = order.CntctCode.ToString(),
                U_New_ORDRID = string.IsNullOrEmpty(order.U_New_ORDRID) ? "" : order.U_New_ORDRID,
                attachmentData = order.FileList,//new List<billAttchment>(),
                billSalesAcctCode = new List<billSalesAcctCode>(),
                billSalesDetails = new List<billSalesDetails>(),
                serialNumber = new List<billSerialNumber>(),
                billDeliveryItemAid = new List<billDeliveryItemAid>(),
                IQCDetails = new List<NSAP.Entity.Quality.IQCDetail>(),
            };
            foreach (var item in order.OrderItems)
            {
                billSalesDetails billSalesDetail = new billSalesDetails()
                {
                    IsCfgMainCode = item.IsCfgMainCode,
                    BaseEntry = item.BaseEntry,//基本凭证代码
                    BaseLine = !string.IsNullOrEmpty(item.BaseLine) ? item.BaseLine : "0",//基础行
                    BaseRef = item.BaseRef,//基本凭证参考
                    BaseType = item.BaseType,//基本凭证类型(-1,0,23，17，16，13，165,默认值为-1)
                    DiscPrcnt = !string.IsNullOrEmpty(item.DiscPrcnt) ? item.DiscPrcnt : "0",// 每行折扣 %
                    Dscription = item.Dscription,// 物料/服务描述
                    ItemCfgId = !string.IsNullOrEmpty(item.ItemCfgId) ? item.ItemCfgId : "0",//物料配置Id
                    ItemCode = item.ItemCode,//物料号
                    LineTotal = !string.IsNullOrEmpty(item.LineTotal) ? item.LineTotal : "0",//行总计
                    OnHand = !string.IsNullOrEmpty(item.OnHand) ? item.OnHand : "0",//库存量
                    Price = !string.IsNullOrEmpty(item.Price) ? item.Price : "0",//价格
                    PriceAfVAT = !string.IsNullOrEmpty(item.PriceAfVAT) ? item.PriceAfVAT : "0",//毛价
                    PriceBefDi = !string.IsNullOrEmpty(item.PriceBefDi) ? item.PriceBefDi : "0",//折扣后价格
                    Quantity = item.Quantity,//数量
                    StockPrice = !string.IsNullOrEmpty(item.StockPrice) ? item.StockPrice : "0",//物料成本
                    TargetType = item.TargetType,//目标凭证类型(-1,0,13,16,203,默认值为-1)
                    TotalFrgn = !string.IsNullOrEmpty(item.TotalFrgn) ? item.TotalFrgn : "0",//以外币计的行总计
                    TrgetEntry = item.TrgetEntry,// 目标凭证代码
                    U_DL = !string.IsNullOrEmpty(item.U_DL) ? item.U_DL : "0",
                    U_DY = !string.IsNullOrEmpty(item.U_DY) ? item.U_DY : "0",
                    U_PDXX = item.U_PDXX,//配电选项
                    U_SCTCJE = !string.IsNullOrEmpty(item.U_SCTCJE) ? item.U_SCTCJE : "0",//生产提成金额
                    U_TDS = !string.IsNullOrEmpty(item.U_TDS) ? item.U_TDS : "0",
                    U_XSTCBL = !string.IsNullOrEmpty(item.U_XSTCBL) ? item.U_XSTCBL : "0",//销售提成比例
                    U_YF = !string.IsNullOrEmpty(item.U_YF) ? item.U_YF : "0",//运费
                    U_YWF = "0",//业务费
                    U_FWF = "0",//服务费
                    VatGroup = "",//税定义
                    WhsCode = item.WhsCode,
                    Lowest = "0",//每行税收百分比
                    VatPrcnt = "",//配电选项
                    ConfigLowest = "0",//配电选项
                    IsExistMo = item.IsExistMo,
                    QryGroup1 = item.QryGroup1,
                    QryGroup2 = item.QryGroup2,
                    _QryGroup3 = item._QryGroup3,
                    Weight = item.Weight,
                    Volume = item.Volume,
                    U_JGF = item.U_JGF,
                    U_JGF1 = item.U_JGF1,
                    U_YFCB = item.U_YFCB,
                    QryGroup8 = item.QryGroup8,////3008n
                    QryGroup9 = item.QryGroup9,//9系列
                    QryGroup10 = item.QryGroup10,// ES系列
                    U_YFTCJE = item.U_YFTCJE,//研发提成金额
                    U_SHJSDJ = item.U_SHJSDJ,
                    U_SHJSJ = item.U_SHJSJ,
                    U_SHTC = item.U_SHTC,
                    U_ZS = !string.IsNullOrEmpty(item.U_ZS) ? item.U_ZS : "",//配置类型，
                    U_RelDoc = !string.IsNullOrEmpty(item.U_RelDoc) ? item.U_RelDoc : "",//关联订单号
                    ChildBillSalesDetails = item.ChildBillSalesDetails

                };
                billDelivery.billSalesDetails.Add(billSalesDetail);
            }
            return billDelivery;
        }

        #region 订单详情
        /// <summary>
        /// 订单详情
        /// </summary>
        /// <param name="DocNum"></param>
        /// <param name="ViewCustom"></param>
        /// <param name="tablename"></param>
        /// <param name="ViewSales"></param>
        /// <param name="SboId"></param>
        /// <param name="isSql"></param>
        /// <returns></returns>
        public OrderDraftInfo QuerySaleDeliveryDetails(string DocNum, bool ViewCustom, string tablename, bool ViewSales, int SboId, bool isSql)
        {
            DataTable orderDraftInfo = new DataTable();
            DataTable dtConfig = _serviceBaseApp.GetSboNamePwd(SboId);
            string dRowData = string.Empty;
            string sqlconn = "";
            string sboname = "";
            string isOpen = "1";
            if (dtConfig.Rows.Count > 0)
            {
                isOpen = dtConfig.Rows[0][6].ToString();
                sqlconn = dtConfig.Rows[0][5].ToString();
                sboname = dtConfig.Rows[0][0].ToString();
            }
            DataTable dt = _serviceBaseApp.GetCustomFields(tablename);
            if (isSql && isOpen == "1")
            {
                if (tablename == "sale_ordr") { tablename = "ORDR"; }
                if (tablename == "sale_odln") { tablename = "ODLN"; }
                if (tablename == "sale_oqut") { tablename = "OQUT"; }
                if (tablename == "sale_oinv") { tablename = "OINV"; }
                if (tablename == "sale_orin") { tablename = "ORIN"; }
                if (tablename == "sale_ordn") { tablename = "ORDN"; }
                if (tablename == "buy_opqt") { tablename = "OPQT"; }
                if (tablename == "buy_opor") { tablename = "OPOR"; }
                if (tablename == "buy_opdn") { tablename = "OPDN"; }
                if (tablename == "buy_opch") { tablename = "OPCH"; }
                if (tablename == "buy_orpc") { tablename = "ORPC"; }
                if (tablename == "buy_orpd") { tablename = "ORPD"; }
                string CustomFields = "";
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (IsExist(tablename, dt.Rows[i][0].ToString()))
                        {
                            CustomFields += "," + dt.Rows[i][0].ToString();
                        }
                    }
                }
                if (string.IsNullOrEmpty(sboname)) { sboname = ""; } else { sboname = sboname + ".dbo."; }
                int Custom = 0; if (ViewCustom) { Custom = 1; }
                int Sales = 0; if (ViewSales) { Sales = 1; }
                string U_FPLB = string.Empty;
                if (IsExist(sboname + tablename, "U_FPLB"))
                {
                    U_FPLB = ",U_FPLB";
                }
                string U_YWY = string.Empty;
                if (IsExist(sboname + tablename, "U_YWY"))
                {
                    U_YWY = ",U_YWY";
                }
                string U_New_ORDRID = string.Empty;
                if (IsExist(sboname + tablename, "U_New_ORDRID"))
                {
                    U_New_ORDRID = ",U_New_ORDRID";
                }
                string U_EshopNo = string.Empty;
                if (tablename.ToUpper() == "OQUT" || tablename.ToUpper() == "ORDR")
                {
                    U_EshopNo = ",U_EshopNo";
                }
                string strSql = string.Format("SELECT U_YGMD,CardCode,CASE WHEN 1 = " + Custom + " THEN CardName ELSE '******' END AS CardName,CASE WHEN 1 = " + Custom + " THEN CntctCode ELSE 0 END AS CntctCode,CASE WHEN 1 = " + Custom + " THEN NumAtCard ELSE '******' END AS NumAtCard,CASE WHEN 1 = " + Custom + " THEN DocCur ELSE '' END AS DocCur,CASE WHEN 1 = " + Custom + " THEN DocRate ELSE 0 END AS DocRate");
                strSql += string.Format(",DocNum,DocType,CASE WHEN 1 = " + Sales + " THEN DiscSum ELSE 0 END AS DiscSum,CASE WHEN 1 = " + Sales + " THEN DiscPrcnt ELSE 0 END AS DiscPrcnt,CASE WHEN 1 = " + Sales + " THEN TotalExpns ELSE 0 END AS TotalExpns,CASE WHEN 1 = " + Sales + " THEN VatSum ELSE 0 END AS VatSum,CASE WHEN 1 = " + Sales + " THEN DocTotal ELSE 0 END AS DocTotal,DocDate,DocDueDate,TaxDate,SupplCode,ShipToCode,PayToCode,Address,Address2,Comments,SlpCode,TrnspCode,GroupNum,PeyMethod,VatPercent,LicTradNum,Indicator,PartSupply,ReqDate,CANCELED");
                strSql += string.Format("" + CustomFields + "");
                strSql += string.Format(",DpmPrcnt,Printed,DocStatus,OwnerCode{0},U_SL{1}{2}{3}", U_FPLB, U_YWY, U_New_ORDRID, U_EshopNo);
                strSql += string.Format(",CASE WHEN 1 = " + Sales + " THEN DocTotalFC ELSE 0 END AS DocTotalFC,CASE WHEN 1 = " + Sales + " THEN DiscSumFC ELSE 0 END AS DiscSumFC");
                strSql += string.Format(" FROM " + sboname + tablename + "");
                strSql += string.Format(" WHERE DocEntry={0}", DocNum);
                orderDraftInfo = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql, CommandType.Text, null);
            }
            else
            {
                string CustomFields = "";
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (IsExistMySql(tablename, dt.Rows[i][0].ToString()))
                        {
                            CustomFields += "," + dt.Rows[i][0].ToString();
                        }
                    }
                }
                string U_YWY = string.Empty;
                if (IsExistMySql(tablename, "U_YWY"))
                {
                    U_YWY = ",U_YWY";
                }
                string U_New_ORDRID = string.Empty;
                if (IsExistMySql(tablename, "U_New_ORDRID"))
                {
                    U_New_ORDRID = ",U_New_ORDRID";
                }
                string U_EshopNo = string.Empty;
                if (tablename.ToLower() == "sale_oqut")
                {
                    U_EshopNo = ",U_EshopNo";
                }
                string strSql = string.Format("SELECT U_YGMD,CardCode,IF(" + ViewCustom + ",CardName,'******' ) AS CardName,IF(" + ViewCustom + ",CntctCode,0) AS CntctCode,IF(" + ViewCustom + ",NumAtCard,'******' ) AS NumAtCard,IF(" + ViewCustom + ",DocCur,'') AS DocCur,IF(" + ViewCustom + ",DocRate,0) AS DocRate");
                strSql += string.Format(",DocNum,DocType,IF(" + ViewSales + ",DiscSum,0) AS DiscSum,IF(" + ViewSales + ",DiscPrcnt,0) AS DiscPrcnt,IF(" + ViewSales + ",TotalExpns,0) AS TotalExpns,IF(" + ViewSales + ",VatSum,0) AS VatSum,IF(" + ViewSales + ",DocTotal,0) AS DocTotal,DocDate,DocDueDate,TaxDate,SupplCode,ShipToCode,PayToCode,Address,Address2,Comments,BillDocType,SlpCode,TrnspCode,GroupNum,PeyMethod,VatPercent,LicTradNum,Indicator,PartSupply,ReqDate,CANCELED");
                strSql += string.Format("" + CustomFields + "");
                strSql += string.Format(",DpmPrcnt,Printed,DocStatus,OwnerCode,U_FPLB,U_SL{0}{1}{2}", U_YWY, U_New_ORDRID, U_EshopNo);
                strSql += string.Format(" FROM {0}." + tablename + "", "nsap_bone");
                strSql += string.Format($" WHERE DocNum={DocNum} AND sbo_id={0}", SboId);
                orderDraftInfo = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            }
            return orderDraftInfo.Tolist<OrderDraftInfo>().FirstOrDefault();
        }
        /// <summary>
        /// SAP
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool IsExist(string tablename, string filename)
        {
            bool result = false;
            string strSql = string.Format("SELECT COUNT(*) FROM syscolumns WHERE id=object_id('{0}') AND name='{1}'", tablename, filename);
            object obj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, strSql, CommandType.Text, null);
            if (obj.ToString() == "0" || obj.ToString() == null)
            { result = false; }
            else { result = true; }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool IsExistMySql(string tablename, string filename)
        {
            bool result = false;
            string strSql = string.Format("SELECT COUNT(*) FROM information_schema.columns WHERE table_schema='nsap_bone' AND table_name ='{0}' AND column_name='{1}'", tablename, filename);
            object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            if (obj.ToString() == "0" || obj.ToString() == null)
            { result = false; }
            else { result = true; }
            return result;
        }
        #endregion

        #region 历史单据
        /// <summary>
        /// 历史单据
        /// </summary>
        public List<HistoricalOrder> GetHistoricalDoc(string TableName, int SboId, string CardCode)
        {
            string strSql = string.Format("SELECT DocEntry,DocTotal,DocDate FROM {0}." + TableName + " WHERE CardCode='{1}' AND sbo_id={2} ORDER BY DocEntry DESC LIMIT 5", "nsap_bone", CardCode, SboId);
            return UnitWork.ExcuteSql<HistoricalOrder>(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>

        /// <returns></returns>
        public bool UpdateSalesDocAttachment(BillDeliveryReq model)
        {
            var UserID = _serviceBaseApp.GetUserNaspId();
            #region 删除原附件数据
            string sql = string.Format("DELETE FROM nsap_oa.file_main WHERE docEntry={0} AND file_type_id = {1}", model.docEntry, model.filetypeId);
            UnitWork.ExcuteSqlTable(ContextType.NsapOaDbContextType, sql, CommandType.Text, null);
            #endregion
            #region 新增附件数据
            foreach (var item in model.deatil)
            {

                StringBuilder nSql = new StringBuilder();
                nSql.AppendFormat("INSERT INTO nsap_oa.file_main (file_sn,file_nm,file_type_id,docEntry,job_id,file_ver,acct_id,issue_reason,file_path,content,remarks,file_status,upd_dt,view_file_path,sbo_id) VALUES");
                nSql.AppendFormat("(1,'{0}',{1},{2},0,'A0','{3}','','{4}','','',0,'{5}','{6}',{7});", item.filename, model.filetypeId, model.docEntry, UserID, item.filepath, DateTime.Now, item.filepath, item.filesboid);
                UnitWork.ExcuteSqlTable(ContextType.NsapOaDbContextType, nSql.ToString(), CommandType.Text, null);
            }
            #endregion
            return true;
        }
        public DataTable SelectPagingHaveRowsCount(string tableName, string fieldName, int pageSize, int pageIndex, string strOrder, string strWhere, out int rowsCount)
        {
            return SelectPaging(tableName, fieldName, pageSize, pageIndex, strOrder, strWhere, 1, out rowsCount);
        }
        private DataTable SelectPaging(string tableName, string fieldName, int pageSize, int pageIndex, string strOrder, string strWhere, int isTotal, out int rowsCount)
        {
            DataTable dataTable = new DataTable();
            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> sqlParameters = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pTableName",    tableName),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pFieldName",     fieldName),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pPageSize",     pageSize),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pPageIndex",    pageIndex),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pStrOrder",    strOrder),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pStrWhere",      strWhere),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pIsTotal",    isTotal),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?rowsCount",  0)
            };
            sqlParameters[7].Direction = ParameterDirection.Output;
            dataTable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, "nsap_base.sp_common_pager", CommandType.StoredProcedure, sqlParameters);
            rowsCount = isTotal == 1 ? Convert.ToInt32(sqlParameters[7].Value) : 0;
            return dataTable;

        }
        #region 复制生产订单
        public DataTable CopyProductToSaleSelect(int pageSize, int pageIndex, string filterQuery, string sortname, string sortorder, int sboID)
        {
            int rowCount = 0;
            string sortString = string.Empty;
            string filterString = string.Empty;
            string line = string.Empty;
            filterString = " a.sbo_id = " + sboID;
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
            {
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());
            }
            #region 搜索条件
            if (!string.IsNullOrEmpty(filterQuery))
            {
                string[] fields = filterQuery.Split('`');
                string[] p = fields[0].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += " AND a.DocEntry like'%" + p[1].FilterWildCard() + "%'";
                };
                p = fields[1].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += " AND a.ItemCode like'%" + p[1].FilterWildCard() + "%'";
                };
                p = fields[2].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += " AND a.U_WO_LTDW like'%" + p[1].FilterWildCard() + "%'";
                };
            }
            filterString += " AND c.dep_id =58 ";
            #endregion
            return CopyProductToSaleSelectNos(out rowCount, pageSize, pageIndex, filterString, sortString, sboID);
        }

        public DataTable CopyProductToSaleSelectNos(out int rowCounts, int pageSize, int pageIndex, string filterQuery, string orderName, int sboID)
        {
            StringBuilder tableName = new StringBuilder();
            StringBuilder filedName = new StringBuilder();
            filedName.AppendFormat("a.DocEntry,a.ItemCode,b.ItemName,a.PlannedQty,a.U_WO_LTDW");
            tableName.AppendFormat("{0}.product_owor a LEFT JOIN {0}.store_oitm b ON a.sbo_id=b.sbo_id AND a.ItemCode = b.ItemCode left join {0}.store_owhs c on c.whscode=a.Warehouse ", "nsap_bone", sboID);
            return SelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), pageSize, pageIndex, orderName, filterQuery, out rowCounts);
        }

        #endregion


        #region 查询物料的库存数据
        /// <summary>
        /// 查询物料的库存数据
        /// </summary>
        /// <returns></returns>
        public DataTable SelectMaterialsInventoryData(string ItemCode, string SboId, bool IsOpenSap, string Operating)
        {
            DataTable dt = SelectMaterialsInventoryDataNos(ItemCode.FilterESC(), SboId, IsOpenSap, Operating);
            if (Operating == "search" || Operating == "edit")
            {
                dt.Rows.Add(ItemCode, "", "", "S", dt.Compute("SUM(OnHand)", "true"), dt.Compute("SUM(IsCommited)", "true"), dt.Compute("SUM(OnOrder)", "true"), dt.Compute("SUM(Available)", "true"), "0", "0", "0", "");
            }
            return dt;
        }

        /// <summary>
        /// 查询物料的库存数据
        /// </summary>
        /// <returns></returns>
        public DataTable SelectMaterialsInventoryDataNos(string ItemCode, string SboId, bool IsOpenSap, string Operating)
        {
            StringBuilder strSql = new StringBuilder();
            if (IsOpenSap)
            {
                if (Operating == "search" || Operating == "edit")
                {
                    strSql.Append("SELECT b.ItemCode,b.WhsCode,a.WhsName,b.Locked,b.OnHand,b.IsCommited,b.OnOrder,");
                    strSql.Append("(b.OnHand+b.OnOrder-b.IsCommited) as Available,b.MinStock,b.MaxStock,b.AvgPrice,b.U_KW ");
                    strSql.Append("FROM OWHS a LEFT JOIN OITW b ON a.WhsCode=b.WhsCode ");
                    strSql.AppendFormat("WHERE b.ItemCode='{0}' ORDER BY b.WhsCode ASC", ItemCode);
                    return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
                }
                else
                {
                    strSql.Append("SELECT '' AS ItemCode,WhsCode,WhsName,'N' AS Locked,'0' AS OnHand,'0' AS IsCommited,");
                    strSql.Append("'0' AS OnOrder,'0' AS Available,'' AS MinStock,'' AS MaxStock,'0' AS AvgPrice,");
                    strSql.Append("'' AS U_KW FROM OWHS ORDER BY WhsCode ASC");
                    return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text);
                }
            }
            else
            {
                if (Operating == "search" || Operating == "edit")
                {
                    strSql.Append("SELECT b.ItemCode,b.WhsCode,a.WhsName,b.Locked,b.OnHand,b.IsCommited,b.OnOrder,");
                    strSql.Append("(b.OnHand+b.OnOrder-b.IsCommited) as Available,b.MinStock,b.MaxStock,b.AvgPrice,b.U_KW ");
                    strSql.Append("FROM nsap_bone.store_OWHS a LEFT JOIN {0}.store_OITW b ON a.sbo_id=b.sbo_id AND a.WhsCode=b.WhsCode ");
                    strSql.AppendFormat("WHERE b.sbo_id=?{0} b.ItemCode=?'{1}' ORDER BY b.WhsCode ASC", SboId, ItemCode);
                    return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);

                }
                else
                {
                    strSql.Append("SELECT '' AS ItemCode,WhsCode,WhsName,'N' AS Locked,'0' AS OnHand,'0' AS IsCommited,");
                    strSql.Append("'0' AS OnOrder,'0' AS Available,'0' AS MinStock,'0' AS MaxStock,'0' AS AvgPrice,'' AS U_KW ");
                    strSql.AppendFormat("FROM {0}.store_OWHS WHERE sbo_id=?{1} ORDER BY WhsCode ASC", "nsap_bone", SboId);
                    return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);

                }
            }
        }

        #endregion


        #region 查询单个物料信息
        /// <summary>
        /// 查询单个物料信息
        /// </summary>
        public SelectSingleStoreOitmInfoDto SelectSingleStoreOitmInfo(string ItemCode, string SboId, bool IsOpenSap)
        {
            return SelectSingleStoreOitmInfonos(ItemCode.FilterESC(), SboId, IsOpenSap);
        }



        /// <summary>
        /// 查询单个物料信息
        /// </summary>
        public SelectSingleStoreOitmInfoDto SelectSingleStoreOitmInfonos(string ItemCode, string SboId, bool IsOpenSap)
        {
            string CustomFields = "";
            DataTable dt = GetCustomFields("store_OITM");
            if (IsOpenSap)
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (QueryExistsStoreItemTypeField("OITM", dt.Rows[i][0].ToString(), true))
                        {
                            CustomFields += ",a." + dt.Rows[i][0].ToString();
                        }
                    }
                }
            }
            else
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (QueryExistsStoreItemTypeField("store_OITM", dt.Rows[i][0].ToString(), false))
                        {
                            CustomFields += ",a." + dt.Rows[i][0].ToString();
                        }
                    }
                }
            }
            StringBuilder strSql = new StringBuilder("SELECT ");
            if (!IsOpenSap) { strSql.Append("a.sbo_id,"); }
            strSql.Append("a.ItemCode,a.ItemName,a.FrgnName,a.ItmsGrpCod,a.CstGrpCode,a.VatGourpSa,a.CodeBars AS BarCode,");
            strSql.Append("a.VATLiable,a.PrchseItem,a.SellItem,a.InvntItem,a.OnHand,a.IsCommited,a.OnOrder,");
            strSql.Append("a.IncomeAcct,a.ExmptIncom,a.MaxLevel,a.DfltWH,a.CardCode,a.SuppCatNum,a.BuyUnitMsr,");
            strSql.Append("a.NumInBuy,a.ReorderQty,a.MinLevel,a.LstEvlPric,a.LstEvlDate,a.CustomPer,a.Canceled,");
            strSql.Append("a.MnufctTime,a.WholSlsTax,a.RetilrTax,a.SpcialDisc,a.DscountCod,a.TrackSales,a.SalUnitMsr,");
            strSql.Append("a.NumInSale,a.Consig,a.QueryGroup,a.Counted,a.OpenBlnc,a.EvalSystem,a.UserSign,a.FREE,");
            strSql.Append("a.PicturName,a.Transfered,a.BlncTrnsfr,a.UserText,a.SerialNum,a.CommisPcnt,a.CommisSum,");
            strSql.Append("a.CommisGrp,a.TreeType,a.TreeQty,a.LastPurPrc,a.LastPurCur,a.LastPurDat,a.ExitCur,a.ExitPrice,");
            strSql.Append("a.ExitWH,a.AssetItem,a.WasCounted,a.ManSerNum,a.SHeight1,a.SHght1Unit,a.SHeight2,a.SHght2Unit,");
            strSql.Append("a.SWidth1,a.SWdth1Unit,a.SWidth2,a.SWdth2Unit,a.SLength1,a.SLen1Unit,a.Slength2,a.SLen2Unit,");
            strSql.Append("a.SVolume,a.SVolUnit,a.SWeight1,a.SWght1Unit,a.SWeight2,a.SWght2Unit,a.BHeight1,a.BHght1Unit,");
            strSql.Append("a.BHeight2,a.BHght2Unit,a.BWidth1,a.BWdth1Unit,a.BWidth2,a.BWdth2Unit,a.BLength1,a.BLen1Unit,");
            strSql.Append("a.Blength2,a.BLen2Unit,a.BVolume,a.BVolUnit,a.BWeight1,a.BWght1Unit,a.BWeight2,a.BWght2Unit,");
            strSql.Append("a.FixCurrCms,a.FirmCode,a.LstSalDate,a.QryGroup1,a.QryGroup2,a.QryGroup3,a.QryGroup4,");
            strSql.Append("a.QryGroup5,a.QryGroup6,a.QryGroup7,a.QryGroup8,a.QryGroup9,a.QryGroup10,a.QryGroup11,");
            strSql.Append("a.QryGroup12,a.QryGroup13,a.QryGroup14,a.QryGroup15,a.QryGroup16,a.QryGroup17,a.QryGroup18,");
            strSql.Append("a.QryGroup19,a.QryGroup20,a.QryGroup21,a.QryGroup22,a.QryGroup23,a.QryGroup24,a.QryGroup25,");
            strSql.Append("a.QryGroup26,a.QryGroup27,a.QryGroup28,a.QryGroup29,a.QryGroup30,a.QryGroup31,a.QryGroup32,");
            strSql.Append("a.QryGroup33,a.QryGroup34,a.QryGroup35,a.QryGroup36,a.QryGroup37,a.QryGroup38,a.QryGroup39,");
            strSql.Append("a.QryGroup40,a.QryGroup41,a.QryGroup42,a.QryGroup43,a.QryGroup44,a.QryGroup45,a.QryGroup46,");
            strSql.Append("a.QryGroup47,a.QryGroup48,a.QryGroup49,a.QryGroup50,a.QryGroup51,a.QryGroup52,a.QryGroup53,");
            strSql.Append("a.QryGroup54,a.QryGroup55,a.QryGroup56,a.QryGroup57,a.QryGroup58,a.QryGroup59,a.QryGroup60,");
            strSql.Append("a.QryGroup61,a.QryGroup62,a.QryGroup63,a.QryGroup64,a.CreateDate,a.UpdateDate,a.ExportCode,");
            strSql.Append("a.SalFactor1,a.SalFactor2,a.SalFactor3,a.SalFactor4,a.PurFactor1,a.PurFactor2,a.PurFactor3,");
            strSql.Append("a.PurFactor4,a.SalFormula,a.PurFormula,a.VatGroupPu,a.AvgPrice,a.PurPackMsr,a.PurPackUn,");
            strSql.Append("a.SalPackMsr,a.SalPackUn,a.ManBtchNum,a.ManOutOnly,");
            if (IsOpenSap)
            {
                strSql.Append("a.validFor AS ValidFor,a.validFrom AS ValidFrom,a.validTo AS ValidTo,");
                strSql.Append("a.frozenFor AS FrozenFor,a.frozenFrom AS FrozenFrom,a.frozenTo AS FrozenTo,");
            }
            else
            {
                strSql.Append("a.validFor AS ValidFor,IF(a.validFrom='0000/0/0 00:00:00','',a.validFrom) AS ValidFrom,IF(a.validTo='0000/0/0 00:00:00','',a.validTo) AS ValidTo,");
                strSql.Append("a.frozenFor AS FrozenFor,IF(a.frozenFrom='0000/0/0 00:00:00','',a.frozenFrom) AS FrozenFrom,IF(a.frozenTo='0000/0/0 00:00:00','',a.frozenTo) AS FrozenTo,");
            }

            strSql.Append("a.BlockOut,a.ValidComm,a.FrozenComm,a.ObjType,");
            strSql.Append("a.SWW,a.Deleted,a.DocEntry,a.ExpensAcct,a.FrgnInAcct,a.ShipType,a.GLMethod,a.ECInAcct,");
            strSql.Append("a.FrgnExpAcc,a.ECExpAcc,a.TaxType,a.ByWh,a.WTLiable,a.ItemType,a.WarrntTmpl,a.BaseUnit,");
            strSql.Append("a.StockValue,a.Phantom,a.IssueMthd,a.FREE1,a.PricingPrc,a.MngMethod,a.ReorderPnt,a.InvntryUom,");
            strSql.Append("a.PlaningSys,a.PrcrmntMtd,a.OrdrIntrvl,a.OrdrMulti,a.MinOrdrQty,a.LeadTime,a.IndirctTax,");
            strSql.Append("a.TaxCodeAR,a.TaxCodeAP,a.ServiceGrp,a.MatType,a.MatGrp,a.ProductSrc,a.ServiceCtg,a.ItemClass,");
            strSql.Append("a.Excisable,a.ChapterID,a.NotifyASN,a.ProAssNum,a.AssblValue,a.Spec,a.TaxCtg,a.Series,a.Number,");
            strSql.AppendFormat("a.ToleranDay,a.U_WLBM AS ItemCodeType,b.SlpName {0} ", CustomFields);
            if (IsOpenSap)
            {
                strSql.AppendFormat("FROM OITM a LEFT JOIN OSLP b ON a.U_USER_ID=b.SlpCode WHERE a.ItemCode='{0}'", ItemCode);
                return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null).Tolist<SelectSingleStoreOitmInfoDto>()[0];
            }
            else
            {
                strSql.AppendFormat("FROM {0}.store_OITM a LEFT JOIN {0}.crm_OSLP b ON a.U_USER_ID=b.SlpCode AND a.sbo_id=b.sbo_id ", "nsap_bone");
                strSql.AppendFormat("WHERE a.sbo_id={0} AND a.ItemCode='{1}'", SboId, ItemCode);

                return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null).Tolist<SelectSingleStoreOitmInfoDto>()[0];
            }
        }
        /// <summary>
        /// 获取表的自定义字段名
        /// </summary>
        public DataTable GetCustomFields(string TableName)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT AliasID,Descr,FieldID,TableID,FieldID,EditSize,EditType ");
            strSql.AppendFormat("FROM {0}.base_cufd WHERE TableID='{1}'", "nsap_bone", TableName);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null);
        }

        /// <summary>
        /// 查询字段是否存在
        /// </summary>
        public bool QueryExistsStoreItemTypeField(string TableName, string CustomField, bool IsOpenSap)
        {
            bool result = false;
            StringBuilder strSql = new StringBuilder();
            if (IsOpenSap)
            {
                strSql.AppendFormat("SELECT COUNT(*) FROM syscolumns WHERE id=object_id('{0}') AND name='{1}' ", TableName, CustomField);
                DataTable fieldRes = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
                if (fieldRes != null && fieldRes.Rows.Count > 0)
                {
                    if (fieldRes.Rows[0][0].ToString() == "1")
                    {
                        result = true;
                    }
                }
            }
            else
            {
                strSql.AppendFormat("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = '{0}' ", "nsap_bone");
                strSql.AppendFormat("AND TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'", TableName, CustomField);
                DataTable fieldRes = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
                if (fieldRes != null && fieldRes.Rows.Count > 0)
                {
                    if (fieldRes.Rows[0][0].ToString() == "1")
                    {
                        result = true;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 获取物料类型的示例
        /// </summary>
        /// <returns></returns>
        public GetItemTypeExpInfoDto GetItemTypeExpInfo(string TypeId)
        {
            return GetItemTypeExpInfoNos(TypeId);
        }
        /// <summary>
        /// 获取物料类型的示例
        /// </summary>
        public GetItemTypeExpInfoDto GetItemTypeExpInfoNos(string TypeId)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT type_id, code_rule, type_coding_exp,type_desc_exp FROM {0}.store_item_type where type_id={1}", "nsap_bone", TypeId);

            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null).Tolist<GetItemTypeExpInfoDto>()[0];

        }
        /// <summary>
        /// 获取物料类型的自定义字段
        /// </summary>
        public List<GetItemTypeCustomFieldsDto> GetItemTypeCustomFields(string TypeId)
        {
            return GetItemTypeCustomFieldsNos(TypeId);
        }
        /// <summary>
        /// 获取物料类型的自定义字段
        /// </summary>
        public List<GetItemTypeCustomFieldsDto> GetItemTypeCustomFieldsNos(string TypeId)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT a.TypeID,a.Fld_nm,a.Fld_Alias,a.Fld_Desc,a.EditType,a.EditSizeMin,a.EditSizeMax,a.Fld_dflt,a.NotNull,IFNULL(b.valRows,0) AS valRows ");
            strSql.AppendFormat("FROM {0}.store_itemtype_cufd a LEFT JOIN (SELECT TypeID,Fld_nm,COUNT(*) AS valRows ", "nsap_bone");
            strSql.AppendFormat("FROM {0}.store_itemtype_ufd1 GROUP BY TypeID,Fld_nm) b ON a.TypeID=b.TypeID AND a.Fld_nm=b.Fld_nm ", "nsap_bone");
            strSql.AppendFormat("WHERE a.TypeID={0} AND a.valid=1 ORDER BY a.Fld_Alias+0 ASC", TypeId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null).Tolist<GetItemTypeCustomFieldsDto>();
        }
        /// <summary>
        /// 获取物料类型的自定义字段 — 有效值
        /// </summary>
        public List<GetItemTypeCustomValueDto> GetItemTypeCustomValue(string TypeId, string FieldNm)
        {
            return GetItemTypeCustomValueNos(TypeId, FieldNm);
        }
        /// <summary>
        /// 获取物料类型的自定义字段 — 有效值
        /// </summary>
        public List<GetItemTypeCustomValueDto> GetItemTypeCustomValueNos(string TypeId, string FieldNm)
        {
            string strSql = string.Format("SELECT FldValue AS id,Descr AS name FROM {0}.store_itemtype_ufd1 WHERE TypeID={1} AND Fld_nm='{2}' ORDER BY IndexID ASC", "nsap_bone", TypeId, FieldNm);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null).Tolist<GetItemTypeCustomValueDto>();
        }

        /// <summary>
        /// 关税组 - 值
        /// </summary>
        /// <returns></returns>
        public string DropListCstGrpCodeValue(string SboId, string KeyId)
        {
            string strSql = string.Format("SELECT TotalTax FROM {0}.store_OARG WHERE sbo_id={1} AND CstGrpCode={2}", "nsap_bone", SboId, KeyId);
            object strObj = UnitWork.ExecuteScalar(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null);
            return strObj == null ? "0" : strObj.ToString();

        }

        /// <summary>
        /// 税收组（采购/销售） - 值
        /// </summary>
        /// <returns></returns>
        public string DropListVatGroupValue(string SboId, string type, string KeyId)
        {
            string strSql = string.Format("SELECT Rate FROM {0}.store_OVTG WHERE sbo_id={1} AND Code='{2}' ", "nsap_bone", SboId, KeyId);
            if (!string.IsNullOrEmpty(type))
            {
                if (type == "sale") { strSql += string.Format(" AND Category='O'"); } else if (type == "buy") { strSql += string.Format(" AND Category='I'"); }
            }

            object strObj = UnitWork.ExecuteScalar(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null);
            return strObj == null ? "0" : strObj.ToString();

        }
        /// <summary>
        /// 获取物料的类型自定义字段 — 值
        /// </summary>
        public string GetMaterialTypeCustomValue(string ItemCode, string SboId)
        {
            return GetMaterialTypeCustomValueNos(ItemCode.FilterESC(), SboId).FirstRowToJSON();
        }
        /// <summary>
        /// 获取物料的类型自定义字段 — 值
        /// </summary>
        public DataTable GetMaterialTypeCustomValueNos(string ItemCode, string SboId)
        {
            string CustomFields = "";
            DataTable typeDt = GetItemTypeCustomFields();
            if (typeDt.Rows.Count > 0)
            {
                for (int j = 0; j < typeDt.Rows.Count; j++)
                {
                    if (QueryExistsStoreItemTypeField("store_OITM", typeDt.Rows[j][0].ToString(), "", false))
                    {
                        CustomFields += "," + typeDt.Rows[j][0].ToString();
                    }
                }
            }
            string strSql = string.Format("SELECT ItemCode{0} FROM {1}.store_OITM WHERE sbo_id={2} AND ItemCode='{3}'", CustomFields, "nsap_bone", SboId, ItemCode);

            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null);
        }
        /// <summary>
        /// 获取物料类型的自定义字段(用于查询)
        /// </summary>
        public DataTable GetItemTypeCustomFields()
        {
            string strSql = string.Format("SELECT Fld_nm FROM {0}.store_itemtype_cufd WHERE valid=1 GROUP BY Fld_nm", "nsap_bone");
            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null);
        }
        /// <summary>
        /// 查询字段是否存在
        /// </summary>
        public bool QueryExistsStoreItemTypeField(string TableName, string CustomField, string SapConn, bool IsOpenSap)
        {
            bool result = false;
            StringBuilder strSql = new StringBuilder();
            if (IsOpenSap)
            {
                strSql.AppendFormat("SELECT COUNT(*) FROM syscolumns WHERE id=object_id('{0}') AND name='{1}' ", TableName, CustomField);
                string fieldRes = UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null).Rows[0][0].ToString();

                result = fieldRes == "1" ? true : false;
            }
            else
            {
                strSql.AppendFormat("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = '{0}' ", "nsap_bone");
                strSql.AppendFormat("AND TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'", TableName, CustomField);
                string fieldRes = UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null).Rows[0][0].ToString();
                result = fieldRes == "1" ? true : false;
            }
            return result;
        }
        /// <summary>
        /// 获取页面地址对应的附件类型
        /// </summary>
        /// <returns></returns>
        public string GetFileTypeByUrl(string PageUrl)
        {
            StringBuilder typeSql = new StringBuilder();
            typeSql.AppendFormat("SELECT a.type_id FROM {0}.file_type a LEFT JOIN {1}.base_func b ON a.func_id=b.func_id ", "nsap_oa", "nsap_base");
            typeSql.AppendFormat("LEFT JOIN {0}.base_page c ON c.page_id=b.page_id WHERE c.page_url='{1}' ", "nsap_base", PageUrl);
            string strObj = UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, typeSql.ToString(), CommandType.Text, null).Rows[0][0].ToString();

            return strObj == null ? "-1" : strObj.ToString();
        }
        /// <summary>
        /// 自定义字段
        /// </summary>
        public string GetCustomFieldsNos(string TableName)
        {
            return GetCustomFieldsRs(TableName).DataTableToJSON();
        }
        /// <summary>
        /// 获取自定义字段名
        /// </summary>
        public DataTable GetCustomFieldsRs(string TableName)
        {
            string strSql = string.Format("SELECT AliasID,Descr,FieldID,TableID,FieldID,EditSize,EditType FROM {0}.base_cufd", "nsap_bone");
            if (!string.IsNullOrEmpty(TableName))
            {
                strSql += string.Format(" WHERE TableID='{0}'", TableName);
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null);
        }
        /// <summary>
        /// 自定义字段(下拉列表)
        /// </summary>
        public string GetCustomValue(string TableID, string FieldID)
        {
            return GetCustomValueNos(TableID, FieldID).DataTableToJSON();
        }
        /// <summary>
        /// 查询自定义下拉列表字段
        /// </summary>
        public DataTable GetCustomValueNos(string TableID, string FieldID)
        {
            string strSql = string.Format("SELECT FldValue AS id,Descr AS name FROM {0}.base_ufd1 WHERE TableID='{1}' AND FieldID='{2}' order by  FldDate ", "nsap_bone", TableID, FieldID);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null);
        }
        /// <summary>
        /// 查询物料属性对应的名称
        /// </summary>
        /// <returns></returns>
        public string GetMaterialPropertyName(string SboId)
        {
            return GetMaterialPropertyNameNos(SboId).DataTableToJSON();
        }
        /// <summary>
        /// 查询物料属性对应的名称
        /// </summary>
        /// <returns></returns>
        public DataTable GetMaterialPropertyNameNos(string SboId)
        {
            string strSql = string.Format("SELECT ItmsTypCod,ItmsGrpNam FROM {0}.store_OITG WHERE sbo_id={1}", "nsap_bone", SboId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null);
        }

        /// <summary>
        /// 物料组
        /// </summary>
        /// <returns></returns>
        public string DropListItmsGrpCod(string SboId)
        {
            return DropListItmsGrpCodNos(SboId).DataTableToJSON();
        }
        /// <summary>
        /// 物料组
        /// </summary>
        /// <returns></returns>
        public DataTable DropListItmsGrpCodNos(string SboId)
        {
            string strSql = string.Format("SELECT ItmsGrpCod AS id,ItmsGrpNam AS name FROM {0}.store_OITB WHERE sbo_id={1}", "nsap_bone", SboId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null);
        }
        /// <summary>
        /// 装运类型
        /// </summary>
        /// <returns></returns>
        public string DropListShipType(string SboId)
        {
            return DropListShipTypeNos(SboId).DataTableToJSON();
        }
        /// <summary>
        /// 装运类型
        /// </summary>
        /// <returns></returns>
        public DataTable DropListShipTypeNos(string SboId)
        {
            string strSql = string.Format("SELECT TrnspCode AS id,TrnspName AS name FROM {0}.crm_oshp WHERE sbo_id={1}", "nsap_bone", SboId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null);
        }
        /// <summary>
        /// 关税组
        /// </summary>
        /// <returns></returns>
        public string DropListCstGrpCode(string SboId)
        {
            return DropListCstGrpCodeNos(SboId).DataTableToJSON();
        }
        /// <summary>
        /// 关税组
        /// </summary>
        /// <returns></returns>
        public DataTable DropListCstGrpCodeNos(string SboId)
        {
            string strSql = string.Format("SELECT CstGrpCode AS id,CstGrpName AS name,TotalTax AS rate FROM {0}.store_OARG WHERE sbo_id={1}", "nsap_bone", SboId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null);
        }
        /// <summary>
        /// 税收组（采购/销售）
        /// </summary>
        /// <returns></returns>
        public string DropListVatGroup(string SboId, string type)
        {
            return DropListVatGroupNos(SboId, type).DataTableToJSON();
        }
        public DataTable DropListVatGroupNos(string SboId, string type)
        {
            string strSql = string.Format("SELECT Code AS id, CONCAT(Code,'  -  ',Name) name, Rate AS rate FROM {0}.store_OVTG WHERE sbo_id={1}", "nsap_bone", SboId);
            if (!string.IsNullOrEmpty(type))
            {
                if (type == "sale") { strSql += string.Format(" AND Category='O'"); } else if (type == "buy") { strSql += string.Format(" AND Category='I'"); }
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null);
        }
        /// <summary>
        /// 价格清单
        /// </summary>
        /// <returns></returns>
        public string DropListPriceList(string SboId)
        {
            return DropListPriceListNos(SboId).DataTableToJSON();
        }
        /// <summary>
        /// 价格清单
        /// </summary>
        /// <returns></returns>
        public DataTable DropListPriceListNos(string SboId)
        {
            string strSql = string.Format("SELECT ListNum AS id,ListName AS name FROM {0}.store_opln WHERE sbo_id={1}", "nsap_bone", SboId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null);
        }
        /// <summary>
        /// 制造商
        /// </summary>
        /// <returns></returns>
        public string DropListFirmCode(string SboId)
        {
            return DropListFirmCodeNos(SboId).DataTableToJSON();
        }
        /// <summary>
        /// 制造商
        /// </summary>
        /// <returns></returns>
        public DataTable DropListFirmCodeNos(string SboId)
        {
            string strSql = string.Format("SELECT FirmCode AS id,FirmName AS name FROM {0}.store_omrc WHERE sbo_id={1}", "nsap_bone", SboId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null);
        }
        /// <summary>
        /// 长度单位
        /// </summary>
        /// <returns></returns>
        public List<DropListUnit> DropListLengthUnit(string SboId)
        {
            return DropListLengthUnitNos(SboId);
        }
        public List<DropListUnit> DropListLengthUnitNos(string SboId)
        {
            bool IsOpenSap = GetSapSboIsOpen(SboId);
            if (IsOpenSap)
            {
                string strSql = "SELECT UnitCode AS id,UnitDisply AS name FROM OLGT ORDER BY UnitCode ASC";
                return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null).Tolist<DropListUnit>();
            }
            else
            {
                string strSql = string.Format("SELECT UnitCode AS id,UnitDisply AS name FROM {0}.store_OLGT ORDER BY UnitCode ASC", "nsap_bone");
                return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null).Tolist<DropListUnit>();
            }
        }
        /// <summary>
        /// 体积单位
        /// </summary>
        /// <returns></returns>
        public List<DropListUnit> DropListVolumeUnit(string SboId)
        {
            return DropListVolumeUnitNos(SboId);
        }
        public List<DropListUnit> DropListVolumeUnitNos(string SboId)
        {
            bool IsOpenSap = GetSapSboIsOpen(SboId);
            if (IsOpenSap)
            {

                string strSql = "SELECT UnitCode AS id,VolDisply AS name FROM OLGT ORDER BY UnitCode ASC";
                return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null).Tolist<DropListUnit>();

            }
            else
            {
                string strSql = string.Format("SELECT UnitCode AS id,VolDisply AS name FROM {0}.store_OLGT ORDER BY UnitCode ASC", "nsap_bone");
                return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null).Tolist<DropListUnit>();
            }
        }
        /// <summary>
        /// 重量单位
        /// </summary>
        /// <returns></returns>
        public List<DropListUnit> DropListWeightUnitNos(string SboId)
        {
            bool IsOpenSap = GetSapSboIsOpen(SboId);
            if (IsOpenSap)
            {

                string strSql = "SELECT UnitCode AS id,UnitDisply AS name FROM OWGT ORDER BY UnitCode ASC";
                return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null).Tolist<DropListUnit>();
            }
            else
            {
                string strSql = string.Format("SELECT UnitCode AS id,UnitDisply AS name FROM {0}.store_OWGT ORDER BY UnitCode ASC", "nsap_bone");
                return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null).Tolist<DropListUnit>();
            }
        }
        /// <summary>
        /// 查询指定帐套是否开启
        /// </summary>
        /// <returns></returns>
        public bool GetSapSboIsOpen(string sbo_id)
        {
            string strSql = string.Format("SELECT is_open FROM {0}.sbo_info WHERE sbo_id={1}", "nsap_base", sbo_id);
            object strObj = UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null).Rows[0][0];
            string isopen = strObj == null ? "0" : strObj.ToString();
            return isopen == "1" ? true : false;
        }
        /// <summary>
        /// 重量单位
        /// </summary>
        /// <returns></returns>
        public List<DropListUnit> DropListWeightUnit(string SboId)
        {
            return DropListWeightUnitNos(SboId);
        }

        #endregion


        #region 根据客户代码查询客户联系人
        /// <summary>
        /// 根据客户代码查询客户联系人
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="filterQuery"></param>
        /// <param name="sortname"></param>
        /// <param name="sortorder"></param>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        public TableData GetCustomerCntctPrsnInfo(int pageSize, int pageIndex, string filterQuery, string sortname, string sortorder, string cardCode)
        {
            string sortString = string.Empty;
            string filterString = string.Empty;

            string line = string.Empty;
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
            {
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());
            }
            return GetCustomerCntctPrsnInfo(pageSize, pageIndex, filterString, sortString, cardCode);

        }
        /// <summary>
        /// 根据客户代码查询客户联系人
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="filterString"></param>
        /// <param name="sortString"></param>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        public TableData GetCustomerCntctPrsnInfo(int pageSize, int pageIndex, string filterString, string sortString, string cardCode)
        {
            StringBuilder tableName = new StringBuilder();
            StringBuilder filedName = new StringBuilder();
            filterString = "CardCode='" + cardCode + "'";
            filedName.Append("Active,[Name],Position,[Address],Tel1,Tel2,Fax,E_MailL,'','' ");
            tableName.Append("OCPR");
            return SAPSelectPagingNoneRowsCount(tableName.ToString(), filedName.ToString(), pageSize, pageIndex, sortString, filterString);
        }
        /// <summary>
        /// 根据客户代码查询客户地址
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="filterQuery"></param>
        /// <param name="sortname"></param>
        /// <param name="sortorder"></param>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        public TableData GetCustomerAddressInfo(int pageSize, int pageIndex, string filterQuery, string sortname, string sortorder, string cardCode)
        {
            string sortString = string.Empty;
            string filterString = string.Empty;

            string line = string.Empty;
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
            {
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());
            }
            return GetCustomerAddressInfo(pageSize, pageIndex, filterString, sortString, cardCode);

        }
        /// <summary>
        /// 根据客户代码查询客户地址
        /// </summary>
        /// <param name="callID"></param>
        /// <returns></returns>
        public TableData GetCustomerAddressInfo(int pageSize, int pageIndex, string filterString, string sortString, string cardCode)
        {
            StringBuilder tableName = new StringBuilder();
            StringBuilder filedName = new StringBuilder();
            filterString = "  CardCode='" + cardCode + "' ";
            filedName.Append(" '是',AdresType,'',Country,[State],City,County,Building");
            tableName.Append(" CRD1 ");
            return SAPSelectPagingNoneRowsCount(tableName.ToString(), filedName.ToString(), pageSize, pageIndex, sortString, filterString);
        }
        public TableData SAPSelectPagingNoneRowsCount(string tableName, string fieldName, int pageSize, int pageIndex, string strOrder, string strWhere)
        {
            int rowsCount = 0;
            return SAPSelectPaging(tableName, fieldName, pageSize, pageIndex, strOrder, strWhere, 0, out rowsCount);
        }
        public TableData SAPSelectPaging(string tableName, string fieldName, int pageSize, int pageIndex, string strOrder, string strWhere, int isTotal, out int rowsCount)
        {
            TableData tableData = new TableData();
            List<SqlParameter> sqlParameters = new List<SqlParameter>()
            {
                new SqlParameter("@strFrom",tableName),
                new SqlParameter("@strSelect",fieldName),
                new SqlParameter("@pageSize", pageSize),
                new SqlParameter("@pageIndex", pageIndex),
                new SqlParameter("@strOrder",  strOrder),
                new SqlParameter("@strWhere", strWhere),
                new SqlParameter("@isStats", isTotal)
             };

            //sqlParameters[7].Direction = ParameterDirection.Output;
            SqlParameter paramOut = new SqlParameter("@rowCount", SqlDbType.Int);
            paramOut.Value = 0;
            paramOut.Direction = ParameterDirection.Output;
            sqlParameters.Add(paramOut);
            DataTable dataTable = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, $"sp_common_pager", CommandType.StoredProcedure, sqlParameters);
            rowsCount = isTotal == 1 ? Convert.ToInt32(paramOut.Value) : 0;
            //tableData.Count = Convert.ToInt32(paramOut.Value);
            tableData.Count = dataTable.Rows.Count;
            tableData.Data = dataTable;
            return tableData;
        }

        /// <summary>
        /// 根据调用ID查询呼叫服务详细信息
        /// </summary>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        public GetCustomerInfoDto GetCustomerDetailsInfo(string cardCode, string sboId, string oldSboId)
        {

            string isOpen = "0";
            string sqlconstr = string.Empty;
            string tbname = string.Empty;
            DataTable dt = GetSboNamePwd(Convert.ToInt32(sboId));
            if (dt.Rows.Count > 0)
            {
                isOpen = dt.Rows[0][6].ToString();
                sqlconstr = dt.Rows[0][5].ToString();
                tbname = dt.Rows[0][0].ToString();
            }
            if (sboId == oldSboId)
            {
                tbname = "";
            }
            else
            {
                tbname += ".dbo.";
            }
            return GetCustomerDetailsInfo(cardCode, sboId, isOpen, sqlconstr, tbname);
        }
        /// <summary>
        /// 根据客户代码查询客户详细信息
        /// </summary>
        /// <param name="cardCode"></param>
        /// <param name="sboId"></param>
        /// <param name="isOpen"></param>
        /// <param name="sqlcont"></param>
        /// <param name="tbname"></param>
        /// <returns></returns>
        public GetCustomerInfoDto GetCustomerDetailsInfo(string cardCode, string sboId, string isOpen, string sqlcont, string tbname)
        {

            string strSql = string.Empty;
            DataTable dtRet;
            if (isOpen == "0")
            {
                strSql = string.Format("select a.CardCode,a.CardName,a.CardFName,a.CmpPrivate,a.Phone1,a.Phone2,a.Fax,a.Cellular,");
                strSql += string.Format(" b.SlpName,a.CntctPrsn,a.Notes,a.Balance,a.Industry,a.Business,a.ShipType,a.Address,a.Building,");
                strSql += string.Format(" a.validFrom,a.validTo,CONCAT(d.lastName,d.firstName) as tcnician,a.MailCounty,a.VatIdUnCmp,a.E_Mail,");
                strSql += string.Format(" a.GroupNum,a.Currency,a.GTSRegNum,a.GTSBankAct,a.GTSBilAddr,a.U_PYSX,a.U_Name,a.U_FName,a.U_FPLB,a.U_job_id");
                strSql += string.Format(" from {0}.crm_ocrd  a left join {0}.crm_oslp b on (a.SlpCode=b.SlpCode  and b.sbo_id={1})", "nsap_bone", sboId);
                strSql += string.Format(" left join {0}.crm_ohem d on (a.DfTcnician=d.empID and d.sbo_id={1})", "nsap_bone", sboId);
                strSql += string.Format(" WHERE a.CardCode={0} and a.sbo_id={1}", "nsap_bone", cardCode, sboId);

                dtRet = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            }
            else
            {
                strSql = string.Format("select a.CardCode,a.CardName,a.CardFName,a.CmpPrivate,a.Phone1,a.Phone2,a.Fax,a.Cellular,");
                strSql += string.Format(" b.SlpName,a.CntctPrsn,a.Notes,a.Balance,a.Industry,a.Business,a.ShipType,a.Address,a.Building,");
                strSql += string.Format(" a.validFrom,a.validTo,(d.lastName+d.firstName) as tcnician,a.MailCounty,a.VatIdUnCmp,a.E_Mail,");
                strSql += string.Format(" a.GroupNum,a.Currency,a.GTSRegNum,a.GTSBankAct,a.GTSBilAddr,a.U_PYSX,a.U_Name,a.U_FName,a.U_FPLB,a.U_job_id");
                strSql += string.Format(" from {0}OCRD  a left join {0}OSLP b on (a.SlpCode=b.SlpCode)", tbname);
                strSql += string.Format(" left join {0}OHEM d on (a.DfTcnician=d.empID)", tbname);
                strSql += string.Format(" WHERE a.CardCode='{0}'", cardCode);
                dtRet = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql, CommandType.Text, null);
            }
            //客戶科目餘額為所有賬套總科目餘額
            if (dtRet != null && dtRet.Rows.Count > 0)
            {
                DataTable sbotable = new DataTable();
                sbotable = DropListSboId();
                decimal totalbalance = 0;
                foreach (DataRow sborow in sbotable.Rows)
                {
                    string sbobalancestr = GetClientSboBalance(dtRet.Rows[0]["CardCode"].ToString(), sborow["id"].ToString());
                    decimal sbobalance;
                    if (!string.IsNullOrEmpty(sbobalancestr) && Decimal.TryParse(sbobalancestr, out sbobalance))
                        totalbalance += sbobalance;
                }
                dtRet.Rows[0]["Balance"] = totalbalance;
            }

            return dtRet.Tolist<GetCustomerInfoDto>()[0];
        }
        public DataTable GetSboNamePwd(int SboId)
        {
            string strSql = string.Format("SELECT sql_db,sql_name,sql_pswd,sap_name,sap_pswd,sql_conn,is_open FROM {0}.sbo_info WHERE sbo_id={1}", "nsap_base", SboId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }
        /// <summary>
        /// 帐套
        /// </summary>
        /// <returns></returns>
        public DataTable DropListSboId()
        {
            string strSql = string.Format("SELECT sbo_id AS id,sbo_nm AS name FROM {0}.sbo_info", "nsap_base");
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }
        /// <summary>
        /// 查詢指定業務夥伴的科目余额（老系统用excel导入的crm_ocrd_oldsbo_balance)
        /// </summary>
        /// <param name="CardCode">客戶代碼</param>
        /// <param name="SboId">賬套</param>
        /// <returns></returns>
        public string GetClientSboBalance(string CardCode, string SboId)
        {
            bool sapflag = GetSapSboIsOpen(SboId);
            if (sapflag)
            {
                string strSql = string.Format("SELECT Balance FROM OCRD WHERE CardCode='{0}'", CardCode);

                object sapbobj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, strSql, CommandType.Text, null);
                return sapbobj == null ? "" : sapbobj.ToString();
            }
            else
            {
                string returnstr = "";
                string strSql = string.Format("SELECT Balance FROM {0}.crm_ocrd_oldsbo_balance WHERE sbo_id={1} and CardCode='{2}'", "nsap_bone", SboId, CardCode);

                object balobj = UnitWork.ExecuteScalar(ContextType.NsapBoneDbContextType, strSql, CommandType.Text, null);
                if (balobj != null) { returnstr = balobj.ToString(); }
                return returnstr;
            }
        }
        #endregion
        #region 修改单据状态
        /// <summary>
        /// 修改单据状态
        /// </summary>
        public bool UpdataSalesDoc(string DocNum, int SboId, string type, int UserID)
        {
            string BOneName = ""; string FuncId = string.Empty; string jobname = string.Empty;
            switch (type.ToLower())
            {
                case "ordr":
                    BOneName = "NSAP.B1Api.BOneORDRUpdate";
                    FuncId = GetJobTypeByAddress("sales/SalesOrdrFunId.aspx");
                    jobname = "取消销售订单";
                    break;

                case "oqut":
                    BOneName = "NSAP.B1Api.BOneOQUTUpdate";
                    FuncId = GetJobTypeByAddress("sales/SalesOqutFunId.aspx");
                    jobname = "取消销售报价单";
                    break;
                case "opqt":
                    BOneName = "NSAP.B1Api.BOneOPQTUpdate";
                    FuncId = GetJobTypeByAddress("purchase/PurchaseQuotationFunId.aspx");
                    jobname = "取消采购报价单";
                    break;
                case "opor":
                    BOneName = "NSAP.B1Api.BOneOPORpdate";
                    FuncId = GetJobTypeByAddress("purchase/PurchaseOrderFunId.aspx");
                    jobname = "取消采购订单";
                    break;
                case "odln":
                    BOneName = "NSAP.B1Api.BOneODLNCancel";
                    FuncId = GetJobTypeByAddress("sales/SalesOrdrFunId.aspx");
                    jobname = "取消销售交货单";
                    break;
            }
            billDelivery Model = new billDelivery();
            Model.DocNum = DocNum;
            Model.SboId = SboId.ToString();
            byte[] job_data = Serialize(Model);
            string job_id = WorkflowBuild(jobname, Convert.ToInt32(FuncId), UserID, job_data, jobname, SboId, "", "", 0, 0, 0, "BOneAPI", BOneName);

            if (int.Parse(job_id) > 0)
            {
                string result = WorkflowSubmit(int.Parse(job_id), UserID, jobname, "", 0);
                return result == "2" ? true : false;
            }
            return false;

        }
        #endregion
        #region 根据页面地址获取FunId
        /// <summary>
        /// 根据页面地址获取FunId
        /// </summary>
        public string GetJobTypeByAddress(string Address)
        {
            string strSql = string.Format("SELECT a.func_id FROM {0}.base_func a LEFT JOIN {0}.base_page b ON a.page_id=b.page_id where b.page_url='{1}'", "nsap_base", Address.Trim());
            object strObj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            return strObj == null ? "-1" : strObj.ToString();
        }
        #endregion
        public static byte[] Serialize(dynamic oClass)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, oClass);
                return stream.ToArray();
            }
        }

        #region 存草稿
        /// <summary>
        /// 审核单(草稿)
        /// </summary>
        /// <returns></returns>
        public string WorkflowBuild(string jobName, int funcID, int userID, byte[] jobdata, string remarks, int sboID, string carCode,
            string carName, double docTotal, int baseType, int baseEntry, string assemblyName, string className)
        {
            if (carCode != "")
            {
                string carNameis = "";
                int rowCounts;
                StringBuilder tableName = new StringBuilder();
                StringBuilder filedName = new StringBuilder();
                filedName.Append(" A.CardName ");
                tableName.Append("  nsap_bone.crm_ocrd A ");
                DataTable dt = SelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), 1, 1, "", " A.sbo_id=" + sboID + " and A.CardCode='" + carCode + "'", out rowCounts);
                if (dt.Rows.Count > 0)
                {
                    carName = dt.Rows[0][0].ToString();
                }
            }
            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> sqlParameters = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pJobName",jobName),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pFuncID",funcID),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pUserID",userID),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pJobData",jobdata),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pRemarks",remarks),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pSboID",sboID),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pCarCode",carCode),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pCarName",carName),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pDocTotal",docTotal),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pBaseType",baseType),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pBaseEntry",baseEntry),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pAssemblyName",assemblyName),
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pClassName",className),
                    };

            return UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, string.Format("{0}.sp_process_build", "nsap_base"), CommandType.StoredProcedure, sqlParameters).ToString();
        }
        #endregion
        /// <summary>
        /// 查看视图【主页面 - 帐套关闭】
        /// </summary>
        /// <returns></returns>
        public DataTable SelectBillViewInfo(out int rowCount, int pageSize, int pageIndex, string filterQuery, string sortname, string sortorder, string type, bool ViewFull, bool ViewSelf, int UserID, int SboID, bool ViewSelfDepartment, int DepID, bool ViewCustom, bool ViewSales)
        {
            string sortString = string.Empty;
            string filterString = string.Empty;
            string line = string.Empty;
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());
            #region 搜索条件
            if (!string.IsNullOrEmpty(filterQuery))
            {
                string[] fields = filterQuery.Split('`');
                string[] p = fields[0].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("a.sbo_id = {0} AND ", p[1]);
                }
                p = fields[1].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("{0} LIKE '{1}' AND ", p[0], p[1].FilterSQL().Trim());
                }
                p = fields[2].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("({0} LIKE '%{1}%' OR a.CardName LIKE '%{1}%') AND ", p[0], p[1].FilterWildCard());
                }
                p = fields[3].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    if (p[1] == "ON") { filterString += string.Format(" (a.DocStatus = 'O' AND a.Printed = 'N' AND a.CANCELED = 'N') AND "); }
                    if (p[1] == "OY") { filterString += string.Format(" (a.DocStatus = 'O' AND a.Printed = 'Y' AND a.CANCELED = 'N') AND "); }
                    if (p[1] == "CY") { filterString += string.Format(" a.CANCELED = 'Y' AND "); }
                    if (p[1] == "CN") { filterString += string.Format(" (a.DocStatus = 'C' AND a.CANCELED = 'N') AND "); }
                    if (p[1] == "NC") { filterString += string.Format(" a.CANCELED = 'N' AND "); }
                }
                p = fields[4].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("a.Comments LIKE '%{0}%' AND ", p[1].FilterWildCard());
                }
                p = fields[5].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("{0} LIKE '%{1}%' AND ", p[0], p[1].FilterSQL().Trim());
                }
                if (type == "ODLN")
                {
                    p = fields[6].Split(':');
                    if (!string.IsNullOrEmpty(p[1]))
                    {
                        filterString += string.Format("a.GroupNum = {1} AND ", p[0], p[1]);
                    }
                    string[] p7 = fields[7].Split(':');
                    string[] p8 = fields[8].Split(':');
                    if (!string.IsNullOrEmpty(p8[1]))
                    {
                        filterString += " (a.CreateDate BETWEEN '" + p7[1].FilterSQL().Trim() + "' AND '" + p8[1].FilterSQL().Trim() + "') AND ";
                    }
                }
            }
            else
            {
                filterString += string.Format("a.sbo_id={0} AND ", SboID);
            }
            #endregion

            #region 根据不同的单据显示不同的内容
            if (!string.IsNullOrEmpty(type))
            {
                if (type == "OQUT") { type = "sale_oqut"; line = "sale_qut1"; }//销售报价单
                else if (type == "ORDR") { type = "sale_ordr"; line = "sale_rdr1"; }//销售订单
                else if (type == "ODLN") { type = "sale_odln"; line = "sale_dln1"; }//销售交货单
                else if (type == "OINV") { type = "sale_oinv"; line = "sale_inv1"; }//应收发票
                else if (type == "ORDN") { type = "sale_ordn"; line = "sale_rdn1"; }//销售退货单
                else if (type == "ORIN") { type = "sale_orin"; line = "sale_rin1"; }//应收贷项凭证
                else if (type == "OPQT") { type = "buy_opqt"; line = "buy_pqt1"; }//采购报价单
                else if (type == "OPOR") { type = "buy_opor"; line = "buy_por1"; }//采购订单
                else if (type == "OPDN") { type = "buy_opdn"; line = "buy_pdn1"; }//采购收货单
                else if (type == "OPCH") { type = "buy_opch"; line = "buy_pch1"; }//应付发票
                else if (type == "ORPD") { type = "buy_orpd"; line = "buy_rpd1"; }//采购退货单
                else if (type == "ORPC") { type = "buy_orpc"; line = "buy_rpc1"; }//应付贷项凭证
                else { type = "sale_oqut"; line = "sale_qut1"; }
            }
            #endregion

            #region 判断权限
            // if (!ViewFull)
            //{
            string arr_roles = GetRolesName(UserID);
            if ((line.Contains("buy")) && ((!arr_roles.Contains("物流文员")) && (!arr_roles.Contains("系统管理员"))))//若不含有物流文员角色，则则屏蔽运输采购单
            {
                filterString += string.Format(" d.QryGroup1='N' AND ");
            }
            //}
            if (ViewSelfDepartment && !ViewFull)
            {
                DataTable rDataRows;
                DataTable rdepRows = new DataTable();
                if (line == "buy_por1" || line == "buy_pdn1" || line == "buy_pqt1" || line == "buy_pch1" || line == "buy_rpd1" || line == "buy_rpc1")
                {
                    rdepRows = GetDep_map(DepID);
                }
                if (rdepRows.Rows.Count > 0)
                {
                    string dep_ids = string.Empty;
                    foreach (DataRow item in rdepRows.Rows)
                    {
                        dep_ids += item[0].ToString() + ",";
                    }
                    rDataRows = GetSboSlpCodeIds(dep_ids.TrimEnd(','), SboID);
                }
                else
                {
                    rDataRows = GetSboSlpCodeIds(DepID, SboID);
                }
                //DataTable rDataRows = NSAP.Data.Sales.BillDelivery.GetSboSlpCodeIds(DepID, SboID);
                if (rDataRows.Rows.Count > 0)
                {
                    filterString += string.Format(" (a.SlpCode IN(");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][0]);
                    }
                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(") OR d.DfTcnician IN (");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][1]);
                    }
                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(")) AND ");
                }

            }
            if (ViewSelf && !ViewFull && !ViewSelfDepartment)
            {
                DataTable rDataRowsSlp = GetSboSlpCodeId(UserID, SboID);
                if (rDataRowsSlp.Rows.Count > 0)
                {
                    string slpCode = rDataRowsSlp.Rows[0][0].ToString();
                    string DfTcnician = rDataRowsSlp.Rows[0][1].ToString();
                    filterString += string.Format(" (a.SlpCode = {0}) AND ", slpCode);// OR d.DfTcnician={1}  , DfTcnician  不允许售后查看业务员的单
                }
                else
                {
                    filterString = string.Format(" (a.SlpCode = {0}) AND ", 0);
                }
            }
            #endregion
            if (!string.IsNullOrEmpty(filterString))
                filterString = filterString.Substring(0, filterString.Length - 5);
            DataTable dt = SelectBillViewInfoNos(out rowCount, pageSize, pageIndex, filterString, sortString, type, line, ViewCustom, ViewSales);
            if (type.ToLower() == "sale_odln")
            {
                //dt.Columns.Add("BuyDocEntry", typeof(string));
                //dt.Columns.Add("TransportName", typeof(string));
                //dt.Columns.Add("TransportID", typeof(string));
                //dt.Columns.Add("TransportSum", typeof(string));
                foreach (DataRow odlnrow in dt.Rows)
                {
                    string docnum = odlnrow["DocEntry"].ToString();
                    DataTable thist = GetSalesDelivery_PurchaseOrderList(docnum, SboID.ToString());
                    string buyentry = "";
                    string transname = "";
                    string transid = "";
                    double transsum = 0.00;
                    string tempname = "";
                    string transDocTotal = "";
                    for (int i = 0; i < thist.Rows.Count; i++)
                    {
                        transsum += double.Parse(thist.Rows[i]["DocTotal"].ToString());// 交货对应采购单总金额
                                                                                       //快递单号，对应采购单编号
                        if (string.IsNullOrEmpty(buyentry))
                        {
                            buyentry = thist.Rows[i]["Buy_DocEntry"].ToString();
                            transid = string.IsNullOrEmpty(thist.Rows[i]["LicTradNum"].ToString()) ? "000000000" : thist.Rows[i]["LicTradNum"].ToString();
                            tempname = thist.Rows[i]["CardName"].ToString();
                            transname = tempname;
                            transDocTotal = thist.Rows[i]["DocTotal"].ToString();
                        }
                        else
                        {
                            buyentry += ";" + thist.Rows[i]["Buy_DocEntry"].ToString();
                            transid += ";" + (string.IsNullOrEmpty(thist.Rows[i]["LicTradNum"].ToString()) ? "000000000" : thist.Rows[i]["LicTradNum"].ToString());
                            //物流公司名称如果连续重复，则只显示第一个
                            if (tempname != thist.Rows[i]["CardName"].ToString())
                                tempname = thist.Rows[i]["CardName"].ToString();
                            else
                                tempname = "";
                            transname += ";;" + tempname;
                            transDocTotal += ";" + thist.Rows[i]["DocTotal"].ToString();
                        }
                    }
                    odlnrow["BuyDocEntry"] = buyentry;
                    odlnrow["TransportName"] = transname;
                    odlnrow["TransportID"] = transid;
                    odlnrow["TransportSum"] = transsum.ToString() + ";" + transDocTotal;
                }
            }
            if (type.ToLower() == "sale_ordr")
            {
                dt.Columns.Add("billStatus", typeof(string));
                dt.Columns.Add("bonusStatus", typeof(string));
                dt.Columns.Add("proStatus", typeof(string));
                dt.Columns.Add("IndicatorName", typeof(string));
                dt.Columns.Add("EmpAcctWarn", typeof(string));
                string bonustypeid = GetJobTypeByUrl("sales/SalesBonus.aspx");
                string bonusatypeid = GetJobTypeByUrl("sales/BonusAfterSales.aspx");
                string protypeid = GetJobTypeByUrl("product/ProductionOrder.aspx");
                string protypeid_cp = GetJobTypeByUrl("product/ProductionOrder_CP.aspx");
                string typeidstr = bonustypeid + "," + bonusatypeid + "," + protypeid + "," + protypeid_cp;

                foreach (DataRow ordrrow in dt.Rows)
                {
                    string orderid = ordrrow["DocEntry"].ToString();
                    ordrrow["billStatus"] = GetBillStatusByOrderId(orderid, SboID.ToString());
                    DataTable jobtab = GetJobStateForDoc(orderid, typeidstr, SboID.ToString());
                    DataRow[] bonusrows = jobtab.Select("job_type_id=" + bonustypeid + " or job_type_id=" + bonusatypeid);
                    DataRow[] prorows = jobtab.Select("job_type_id=" + protypeid + " or job_type_id=" + protypeid_cp, "upd_dt desc");
                    ordrrow["bonusStatus"] = "";
                    ordrrow["proStatus"] = "";
                    if (bonusrows.Length > 0)
                    {
                        ordrrow["bonusStatus"] = bonusrows[0]["job_state"].ToString();
                    }
                    if (prorows.Length > 0)
                    {
                        ordrrow["proStatus"] = prorows[0]["job_state"].ToString();
                    }
                    ordrrow["IndicatorName"] = GetBillIndicatorByOrderId(orderid, SboID.ToString());
                    ordrrow["EmpAcctWarn"] = GetEmptyAcctByOrderId(orderid, SboID.ToString());
                }
            }
            if (type.ToLower() == "buy_opor")
            {
                dt.Columns.Add("PurchaseBillNo", typeof(string));
                dt.Columns.Add("IndicatorName", typeof(string));
                foreach (DataRow temprow in dt.Rows)
                {
                    string indicator = temprow["Indicator"].ToString();
                    string taxstr = GetTaxNoByPO(temprow["DocEntry"].ToString(), SboID.ToString());
                    temprow["PurchaseBillNo"] = taxstr;
                    temprow["IndicatorName"] = GetToCompanyName(SboID.ToString(), indicator);
                }
            }
            return dt;
        }
        /// <summary>
        /// 查看视图【主页面 - 帐套开启】
        /// </summary>
        /// <returns></returns>
        public DataTable SelectBillListInfo(out int rowCount, int pageSize, int pageIndex, string filterQuery, string sortname, string sortorder, string type, bool ViewFull, bool ViewSelf, int UserID, int SboID, bool ViewSelfDepartment, int DepID, bool ViewCustom, bool ViewSales, string sqlcont, string sboname)
        {
            bool IsSql = true;
            string sortString = string.Empty;
            string filterString = string.Empty;
            string line = string.Empty; int uSboId = SboID;
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());
            string dRowData = string.Empty;
            #region 搜索条件
            if (!string.IsNullOrEmpty(filterQuery))
            {
                string[] fields = filterQuery.Split('`');
                string[] p = fields[0].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    uSboId = int.Parse(p[1].ToString());
                    if (uSboId == SboID) { IsSql = true; } else { IsSql = false; }
                }
                p = fields[1].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("{0} LIKE '{1}' AND ", p[0], p[1].FilterSQL().Trim());
                }
                p = fields[2].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("({0} LIKE '%{1}%' OR a.CardName LIKE '%{1}%') AND ", p[0], p[1].FilterWildCard());
                }
                p = fields[3].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    if (p[1] == "ON") { filterString += string.Format(" (a.DocStatus = 'O' AND a.Printed = 'N' AND a.CANCELED = 'N') AND "); }
                    if (p[1] == "OY") { filterString += string.Format(" (a.DocStatus = 'O' AND a.Printed = 'Y' AND a.CANCELED = 'N') AND "); }
                    if (p[1] == "CY") { filterString += string.Format(" a.CANCELED = 'Y' AND "); }
                    if (p[1] == "CN") { filterString += string.Format(" (a.DocStatus = 'C' AND a.CANCELED = 'N') AND "); }
                    if (p[1] == "CPN") { filterString += string.Format(" (a.DocStatus = 'C' AND a.CANCELED = 'N' and a.Printed = 'N') AND "); }
                    if (p[1] == "CPY") { filterString += string.Format(" (a.DocStatus = 'C' AND a.CANCELED = 'N' and a.Printed = 'Y') AND "); }
                }
                p = fields[4].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("a.Comments LIKE '%{0}%' AND ", p[1].FilterWildCard());
                }
                p = fields[5].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("{0} LIKE '%{1}%' AND ", p[0], p[1].FilterSQL().Trim());
                }
                if (type == "ODLN")
                {
                    p = fields[6].Split(':');
                    if (!string.IsNullOrEmpty(p[1]))
                    {
                        filterString += string.Format("a.GroupNum = {1} AND ", p[0], p[1]);
                    }
                    string[] p7 = fields[7].Split(':');
                    string[] p8 = fields[8].Split(':');
                    if (!string.IsNullOrEmpty(p8[1]))
                    {
                        filterString += " (a.CreateDate BETWEEN '" + p7[1].FilterSQL().Trim() + "' AND '" + p8[1].FilterSQL().Trim() + "') AND ";
                    }
                }
                if (type == "ORDR")
                {
                    p = fields[6].Split(':');
                    if (!string.IsNullOrEmpty(p[1]))
                    {
                        filterString += string.Format("a.Indicator = '{0}' AND ", p[1]);
                    }
                }
                //查询关联订单
                if (fields.Length > 6)
                {
                    p = fields[6].Split(':');
                    if (!string.IsNullOrEmpty(p[1]))
                    {
                        if (type == "OPDN")
                        {
                            filterString += string.Format("EXISTS (select 1 from {0}.dbo.PDN1 p1 LEFT JOIN {0}.dbo.POR1 p2 on p1.BaseEntry=p2.DocEntry and p1.BaseLine=p2.LineNum where p1.docentry=a.docentry and p2.U_RelDoc like '%{1}%') AND ", sboname, p[1].FilterSQL().Trim());
                        }
                        if (type == "OPOR")
                        {
                            filterString += string.Format("EXISTS (select 1 from {0}.dbo.POR1 p2  where p2.docentry=a.docentry and p2.U_RelDoc like '%{1}%') AND ", sboname, p[1].FilterSQL().Trim());
                        }
                    }
                }
                if (type == "OPDN" && fields.Length > 7)
                {
                    p = fields[7].Split(':');
                    if (!string.IsNullOrEmpty(p[1]))
                    {
                        filterString += string.Format("EXISTS (select 1 from {0}.dbo.PDN1 p2  where p2.docentry=a.docentry and p2.BaseEntry={1}) AND ", sboname, p[1].FilterSQL().Trim());
                    }
                }
            }
            #endregion

            #region 根据不同的单据显示不同的内容
            if (!string.IsNullOrEmpty(type))
            {
                if (type == "OQUT") { type = "OQUT"; line = "QUT1"; }//销售报价单
                else if (type == "ORDR") { type = "ORDR"; line = "RDR1"; }//销售订单
                else if (type == "ODLN") { type = "ODLN"; line = "DLN1"; }//销售交货单
                else if (type == "OINV") { type = "OINV"; line = "INV1"; }//应收发票
                else if (type == "ORDN") { type = "ORDN"; line = "RDN1"; }//销售退货单
                else if (type == "ORIN") { type = "ORIN"; line = "RIN1"; }//应收贷项凭证
                else if (type == "OPQT") { type = "OPQT"; line = "PQT1"; }//采购报价单
                else if (type == "OPOR") { type = "OPOR"; line = "POR1"; }//采购订单
                else if (type == "OPDN") { type = "OPDN"; line = "PDN1"; }//采购收货单
                else if (type == "OPCH") { type = "OPCH"; line = "PCH1"; }//应付发票
                else if (type == "ORPD") { type = "ORPD"; line = "RPD1"; }//采购退货单
                else if (type == "ORPC") { type = "ORPC"; line = "RPC1"; }//应付贷项凭证
                else { type = "OQUT"; line = "QUT1"; }
            }
            #endregion

            #region 判断权限
            //if (!ViewFull)
            // {
            string arr_roles = GetRolesName(UserID);
            if ((line == "PQT1" || line == "POR1" || line == "PDN1" || line == "PCH1" || line == "RPD1" || line == "RPC1") && ((!arr_roles.Contains("物流文员")) && (!arr_roles.Contains("系统管理员"))))//若不含有物流文员角色，则则屏蔽运输采购单
            {
                filterString += string.Format(" d.QryGroup1='N' AND ");
            }
            //}
            if (ViewSelfDepartment && !ViewFull)
            {
                DataTable rDataRows;
                DataTable rdepRows = new DataTable();
                if (line == "POR1" || line == "PDN1" || line == "PQT1" || line == "PCH1" || line == "RPD1" || line == "RPC1")
                {
                    rdepRows = GetDep_map(DepID);
                }
                if (rdepRows.Rows.Count > 0)
                {
                    string dep_ids = string.Empty;
                    foreach (DataRow item in rdepRows.Rows)
                    {
                        dep_ids += item[0].ToString() + ",";
                    }
                    rDataRows = GetSboSlpCodeIds(dep_ids.TrimEnd(','), SboID);
                }
                else
                {
                    rDataRows = GetSboSlpCodeIds(DepID, SboID);
                }
                //DataTable rDataRows = NSAP.Data.Sales.BillDelivery.GetSboSlpCodeIds(DepID, SboID);
                if (rDataRows.Rows.Count > 0)
                {
                    filterString += string.Format(" (a.SlpCode IN(");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][0]);
                    }
                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(") OR d.DfTcnician IN (");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][1]);
                    }
                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(")) AND ");
                }
            }
            else if (ViewSelf && !ViewFull && !ViewSelfDepartment)
            {
                DataTable rDataRowsSlp = GetSboSlpCodeId(UserID, SboID);
                if (rDataRowsSlp.Rows.Count > 0)
                {
                    string slpCode = rDataRowsSlp.Rows[0][0].ToString();
                    string slpTcnician = rDataRowsSlp.Rows[0][1].ToString();
                    filterString += string.Format(" (a.SlpCode = {0}) AND ", slpCode);// OR d.DfTcnician={1}   , slpTcnician  不允许售后查看业务员的单
                }
                else
                {
                    filterString = string.Format(" (a.SlpCode = {0}) AND ", 0);
                }
            }
            #endregion
            if (!string.IsNullOrEmpty(filterString))
                filterString = filterString.Substring(0, filterString.Length - 5);
            if (IsSql)
            {
                DataTable thistab = SelectBillListInfoNos(out rowCount, pageSize, pageIndex, filterString, sortString, type, line, ViewCustom, ViewSales, sqlcont, sboname);
                if (type.ToLower() == "odln")
                {
                    //thistab.Columns.Add("BuyDocEntry", typeof(string));
                    //thistab.Columns.Add("TransportName", typeof(string));
                    //thistab.Columns.Add("TransportID", typeof(string));
                    //thistab.Columns.Add("TransportSum", typeof(string));
                    foreach (DataRow odlnrow in thistab.Rows)
                    {
                        string docnum = odlnrow["DocEntry"].ToString();
                        DataTable thist = GetSalesDelivery_PurchaseOrderList(docnum, SboID.ToString());
                        string buyentry = "";
                        string transname = "";
                        string transid = "";
                        double transsum = 0.00;
                        string tempname = "";
                        string transDocTotal = "";
                        for (int i = 0; i < thist.Rows.Count; i++)
                        {
                            transsum += double.Parse(thist.Rows[i]["DocTotal"].ToString());// 交货对应采购单总金额
                                                                                           //快递单号，对应采购单编号
                            if (string.IsNullOrEmpty(buyentry))
                            {
                                buyentry = thist.Rows[i]["Buy_DocEntry"].ToString();
                                transid = string.IsNullOrEmpty(thist.Rows[i]["LicTradNum"].ToString()) ? "000000000" : thist.Rows[i]["LicTradNum"].ToString();
                                tempname = thist.Rows[i]["CardName"].ToString();
                                transname = tempname;
                                transDocTotal = thist.Rows[i]["DocTotal"].ToString();
                            }
                            else
                            {
                                buyentry += ";" + thist.Rows[i]["Buy_DocEntry"].ToString();
                                transid += ";" + (string.IsNullOrEmpty(thist.Rows[i]["LicTradNum"].ToString()) ? "000000000" : thist.Rows[i]["LicTradNum"].ToString());
                                //物流公司名称如果连续重复，则只显示第一个
                                if (tempname != thist.Rows[i]["CardName"].ToString())
                                    tempname = thist.Rows[i]["CardName"].ToString();
                                else
                                    tempname = "";
                                transname += ";;" + tempname;
                                transDocTotal += ";" + thist.Rows[i]["DocTotal"].ToString();
                            }

                        }
                        odlnrow["BuyDocEntry"] = buyentry;
                        odlnrow["TransportName"] = transname;
                        odlnrow["TransportID"] = transid;
                        odlnrow["TransportSum"] = transsum.ToString() + ";" + transDocTotal;
                    }
                }
                if (type.ToLower() == "ordr")//对应发票状态
                {
                    thistab.Columns.Add("billStatus", typeof(string));
                    thistab.Columns.Add("bonusStatus", typeof(string));
                    thistab.Columns.Add("proStatus", typeof(string));
                    thistab.Columns.Add("IndicatorName", typeof(string));
                    thistab.Columns.Add("EmpAcctWarn", typeof(string));
                    string bonustypeid = GetJobTypeByUrl("sales/SalesBonus.aspx");
                    string bonusatypeid = GetJobTypeByUrl("sales/BonusAfterSales.aspx");
                    string protypeid = GetJobTypeByUrl("product/ProductionOrder.aspx");
                    string protypeid_cp = GetJobTypeByUrl("product/ProductionOrder_CP.aspx");
                    string typeidstr = bonustypeid + "," + bonusatypeid + "," + protypeid + "," + protypeid_cp;
                    foreach (DataRow ordrrow in thistab.Rows)
                    {
                        string orderid = ordrrow["DocEntry"].ToString();
                        ordrrow["billStatus"] = GetBillStatusByOrderId(orderid, SboID.ToString());
                        DataTable jobtab = GetJobStateForDoc(orderid, typeidstr, SboID.ToString());
                        DataRow[] bonusrows = jobtab.Select("job_type_id=" + bonustypeid + " or job_type_id=" + bonusatypeid);
                        DataRow[] prorows = jobtab.Select("job_type_id=" + protypeid + " or job_type_id=" + protypeid_cp, "upd_dt desc");
                        ordrrow["bonusStatus"] = "";
                        ordrrow["proStatus"] = "";
                        if (bonusrows.Length > 0)
                        {
                            ordrrow["bonusStatus"] = bonusrows[0]["job_state"].ToString();
                        }
                        if (prorows.Length > 0)
                        {
                            ordrrow["proStatus"] = prorows[0]["job_state"].ToString();
                        }
                        ordrrow["IndicatorName"] = GetBillIndicatorByOrderId(orderid, SboID.ToString());
                        ordrrow["EmpAcctWarn"] = GetEmptyAcctByOrderId(orderid, SboID.ToString());
                    }
                }
                if (type.ToLower() == "opor")
                {
                    //thistab.Columns.Add("PurchaseBillNo", typeof(string));
                    //thistab.Columns.Add("IndicatorName", typeof(string));
                    foreach (DataRow temprow in thistab.Rows)
                    {
                        string indicator = temprow["Indicator"].ToString();
                        string taxstr = GetTaxNoByPO(temprow["DocEntry"].ToString(), SboID.ToString());
                        temprow["PurchaseBillNo"] = taxstr;
                        temprow["IndicatorName"] = GetToCompanyName(SboID.ToString(), indicator);
                    }
                }
                if (type.ToLower() == "opdn")
                {
                    double totalamount = 0.00;
                    foreach (DataRow temprow in thistab.Rows)
                    {
                        totalamount += double.Parse(temprow["Doctotal"].ToString());
                    }
                    string pageExtend = string.Format("总金额：{0}", totalamount.ToString("0.00"));
                    return thistab;
                }
                else
                {
                    return thistab;
                }
            }
            else
            {
                return SelectBillViewInfo(out rowCount, pageSize, pageIndex, filterQuery, sortname, sortorder, type, ViewFull, ViewSelf, UserID, uSboId, ViewSelfDepartment, DepID, ViewCustom, ViewSales);
            }
        }

        #region 第二层
        /// <summary>
        /// 查看视图【主页面 - 帐套关闭】
        /// </summary>
        /// <returns></returns>
        public DataTable SelectBillViewInfoNos(out int rowCounts, int pageSize, int pageIndex, string filterQuery, string orderName, string type, string line, bool ViewCustom, bool ViewSales)
        {
            StringBuilder tableName = new StringBuilder();
            StringBuilder filedName = new StringBuilder();
            filedName.Append(" '',a.UpdateDate,a.DocEntry,a.CardCode,IF(" + ViewCustom + ",a.CardName,'******'),IF(" + ViewSales + ",a.DocTotal,'******') DocTotal,IF(" + ViewSales + ",(a.DocTotal-a.PaidToDate),'******') OpenDocTotal,a.CreateDate,a.SlpCode,a.Comments,a.DocStatus,a.Printed,c.SlpName,a.CANCELED,a.Indicator,a.DocDueDate,e.PymntGroup,f.billID");

            if (type == "buy_opor")
            {
                filedName.Append(",a.ActualDocDueDate");
            }
            else
            {
                filedName.Append(",'' as ActualDocDueDate");
            }
            if (type == "buy_opdn")
            {
                filedName.Append(",a.U_YGMD");
            }
            if (type.ToLower() == "sale_oqut")
            {
                filedName.Append(",''  as AttachFlag ");
            }
            if (type.ToLower() == "sale_odln")
            {
                filedName.Append(",'' as BuyDocEntry,'' as TransportName,'' as TransportID,'' as TransportSum,a.DocDate");
            }
            tableName.AppendFormat("{0}." + type + " a ", "nsap_bone");
            tableName.AppendFormat(" LEFT JOIN {0}.crm_oslp c ON a.SlpCode = c.SlpCode AND a.sbo_id=c.sbo_id", "nsap_bone");
            tableName.AppendFormat(" LEFT JOIN {0}.crm_ocrd d ON a.CardCode = d.CardCode AND a.sbo_id=d.sbo_id", "nsap_bone");
            tableName.AppendFormat(" LEFT JOIN {0}.crm_octg e ON a.GroupNum = e.GroupNum AND a.sbo_id=e.sbo_id", "nsap_bone");
            tableName.AppendFormat(" LEFT JOIN {0}.finance_billapplication_master f ON f.DocEntry = a.DocEntry AND f.sbo_id=a.sbo_id", "nsap_bone");
            // tableName.AppendFormat(" LEFT JOIN {0}.sale_trans_apply   n ON n.cardcode= a.CardCode AND a.sbo_id=n.sbo_id", Sql.BOneDatabaseName);

            DataTable dt = SelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), pageSize, pageIndex, orderName, filterQuery, out rowCounts);
            if (type.ToLower() == "sale_ordr" || type.ToLower() == "buy_opor")
            {
                string bonetype = type.ToLower();
                dt.Columns.Add("PrintNo");//从1算第17
                dt.Columns.Add("PrintNumIndex");//从1算第18
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataTable dtPrintnum = PurgetPrintNum("1", dt.Rows[i]["DocEntry"].ToString().Trim(), bonetype);//取编号
                    if (dtPrintnum.Rows.Count > 0)
                    {
                        dt.Rows[i]["PrintNo"] = dtPrintnum.Rows[0][0].ToString();
                        dt.Rows[i]["PrintNumIndex"] = dtPrintnum.Rows[0][1].ToString();
                    }

                }
            }
            //查询是否有附件
            if (type.ToLower() == "oqut")
            {
                foreach (DataRow temprow in dt.Rows)
                {
                    //取该类型附件
                    string attTypeSql = string.Format("SELECT a.type_id FROM {0}.file_type a LEFT JOIN {1}.base_func b ON a.func_id=b.func_id LEFT JOIN {1}.base_page c ON c.page_id=b.page_id WHERE c.page_url='{2}'", "nsap_oa", "nsap_base", "sales/SalesQuotation.aspx");
                    object typeObj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, attTypeSql, CommandType.Text, null);
                    string fileType = typeObj == null ? "-1" : typeObj.ToString();

                    string strSql2 = string.Format("SELECT 1 FROM {0}.file_main AS T0 ", "nsap_oa");
                    strSql2 += string.Format("LEFT JOIN {0}.file_type AS T1 ON T0.file_type_id = T1.type_id ", "nsap_oa");
                    strSql2 += string.Format("WHERE T0.file_type_id = {0} AND T0.docEntry = {1} limit 1", int.Parse(fileType), int.Parse(temprow["DocEntry"].ToString()));
                    object fileflag = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql2, CommandType.Text, null);
                    temprow["AttachFlag"] = fileflag == null ? "0" : fileflag.ToString();
                }
            }

            return dt;
        }

        /// <summary>
        /// 查看视图【主页面 - 帐套开启】
        /// </summary>
        /// <returns></returns>
        public DataTable SelectBillListInfoNos(out int rowCounts, int pageSize, int pageIndex, string filterQuery, string orderName, string type, string line, bool ViewCustom, bool ViewSales, string sqlcont, string sboname)
        {
            StringBuilder tableName = new StringBuilder();
            StringBuilder filedName = new StringBuilder(); int Custom = 0; int Sales = 0;
            if (ViewCustom) { Custom = 1; }
            if (ViewSales) { Sales = 1; }
            if (string.IsNullOrEmpty(sboname)) { sboname = ""; } else { sboname = sboname + ".dbo."; }
            filedName.Append(" a.UpdateDate,a.DocEntry,a.CardCode,CASE WHEN 1 = " + Custom + " THEN a.CardName ELSE '******' END AS CardName,CASE WHEN 1 = " + Sales + " THEN a.DocTotal ELSE 0 END AS DocTotal,CASE WHEN 1 = " + Sales + " THEN (a.DocTotal-a.PaidToDate) ELSE 0 END AS OpenDocTotal,a.CreateDate,a.SlpCode,a.Comments,a.DocStatus,a.Printed,c.SlpName,a.CANCELED,a.Indicator,a.DocDueDate,e.PymntGroup,'' as billID,'' AS ActualDocDueDate ");
            if (type == "OPDN")
            {
                filedName.Append(" ,a.U_YGMD ");
                filedName.Append(@",(
                                    select ',' + CONVERT(VARCHAR(20), docentry) from (
                                    SELECT DISTINCT PCH1.DocEntry from " + sboname + "PCH1 LEFT JOIN " + sboname + "PDN1 on PCH1.BaseType = 20 and PCH1.BaseEntry = PDN1.DocEntry and PCH1.BaseLine = PDN1.LineNum"
                                    + " where PDN1.DocEntry = a.DocEntry) v1 for xml path('') ) as pchdocstr");
            }
            if (type.ToLower() == "ordr" || type.ToLower() == "opor")
            {
                filedName.Append(",'' as PrintNo,'' as PrintNumIndex");
            }
            if (type == "OPOR")
            {
                filedName.Append(",'' as PurchaseBillNo,'' as IndicatorName ,a.U_THYY ");
                //应付发票号
                filedName.Append(@",(
                                        select ',' + CONVERT(VARCHAR(20), docentry) from (
                                        SELECT DISTINCT PCH1.DocEntry from " + sboname + "PCH1 LEFT JOIN " + sboname + "PDN1 on PCH1.BaseType = 20 and PCH1.BaseEntry = PDN1.DocEntry and PCH1.BaseLine = PDN1.LineNum"
                                        + " LEFT JOIN " + sboname + "POR1 on PDN1.BaseType = 22 and PDN1.BaseEntry = POR1.DocEntry and PDN1.BaseLine = POR1.LineNum"
                                        + " LEFT JOIN " + sboname + "POR1 p1 on PCH1.BaseType=22 and PCH1.BaseEntry =p1.Docentry and PCH1.BaseLine=p1.LineNum"
                                        + " where POR1.DocEntry = a.DocEntry or p1.DocEntry=a.DocEntry) v1 for xml path('') ) as pchdocstr");
            }
            if (type.ToLower() == "oqut")
            {
                filedName.Append(",''  as AttachFlag ");
            }
            if (type.ToLower() == "odln")
            {
                filedName.Append(",'' as BuyDocEntry,'' as TransportName,'' as TransportID,'00' as TransportSum,a.DocDate");
            }
            tableName.AppendFormat("" + sboname + type + " a ");
            tableName.AppendFormat(" LEFT JOIN " + sboname + "OSLP c ON a.SlpCode = c.SlpCode");
            tableName.AppendFormat(" LEFT JOIN " + sboname + "OCRD d ON a.CardCode = d.CardCode");
            tableName.AppendFormat(" LEFT JOIN " + sboname + "OCTG e ON a.GroupNum = e.GroupNum");
            //tableName.AppendFormat(" LEFT JOIN " + sboname + "APPLY  n ON n.cardcode= a.CardCode AND a.sbo_id=n.sbo_id");
            //增加打印编号一列
            DataTable dt = SAPSelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), pageSize, pageIndex, orderName, filterQuery, out rowCounts);
            if (type.ToLower() == "ordr" || type.ToLower() == "opor")
            {
                string bonetype = type.ToLower();
                //dt.Columns.Add("PrintNo");//从1算第17
                //dt.Columns.Add("PrintNumIndex");//从1算第18
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataTable dtPrintnum = PurgetPrintNum("1", dt.Rows[i]["DocEntry"].ToString().Trim(), bonetype);//取编号
                    if (dtPrintnum.Rows.Count > 0)
                    {
                        dt.Rows[i]["PrintNo"] = dtPrintnum.Rows[0][0].ToString();
                        dt.Rows[i]["PrintNumIndex"] = dtPrintnum.Rows[0][1].ToString();
                    }

                }
            }
            if (type.ToLower() == "oqut")
            {
                foreach (DataRow temprow in dt.Rows)
                {
                    //取销售合同类型附件
                    string attTypeSql = string.Format("SELECT a.type_id FROM {0}.file_type a LEFT JOIN {1}.base_func b ON a.func_id=b.func_id LEFT JOIN {1}.base_page c ON c.page_id=b.page_id WHERE c.page_url='{2}'", "nsap_oa", "nsap_base", "sales/SalesQuotation.aspx");
                    object typeObj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, attTypeSql, CommandType.Text, null);
                    string fileType = typeObj == null ? "-1" : typeObj.ToString();

                    string strSql2 = string.Format("SELECT 1 FROM {0}.file_main AS T0 ", "nsap_oa");
                    strSql2 += string.Format("LEFT JOIN {0}.file_type AS T1 ON T0.file_type_id = T1.type_id ", "nsap_oa");
                    strSql2 += string.Format("WHERE T0.file_type_id = {0} AND T0.docEntry = {1} limit 1", int.Parse(fileType), int.Parse(temprow["DocEntry"].ToString()));
                    object fileflag = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql2, CommandType.Text, null);
                    temprow["AttachFlag"] = fileflag == null ? "0" : fileflag.ToString();
                }
            }
            return dt;
        }

        #endregion
        public Powers GetPagePowersByUrl(string url, int UserID)
        {
            CurrentPage page = GetCurrentPage(UserID, url);
            return new Powers(page.AuthMap);
        }
        public CurrentPage GetCurrentPage(int userID, string pageUrl)
        {
            CurrentPage pageInfo = new CurrentPage();
            try
            {
                DataTable dTable = null;
                lock (gAmbit)
                {
                    gAmbit.TryGetValue(userID, out dTable);
                }
                if (dTable != null && dTable.Rows.Count > 0)
                {
                    DataRow dRow = dTable.AsEnumerable().Single(r =>
                    {
                        return (pageUrl.ToLower() == r[1].ToString().Trim().ToLower().TrimStart('/'));
                    });
                    if (dRow != null && !dRow.IsNull(0))
                    {
                        pageInfo.FuncID = int.Parse(dRow[0].ToString());
                        pageInfo.AuthMap = long.Parse(dRow[2].ToString());
                    }
                }
            }
            catch { }

            return pageInfo;
        }
        public static string GetRolesName(int userID)
        {
            try
            {
                DataTable dTable = null;
                lock (gRoles)
                {
                    gRoles.TryGetValue(userID, out dTable);
                }
                if (dTable != null && dTable.Rows.Count > 0)
                {
                    return string.Join(",", dTable.AsEnumerable().Select(r => r[1].ToString().Trim()).ToArray());
                }
                return string.Empty;
            }
            catch { return string.Empty; }
        }
        /// <summary>
        /// 根据部门ID获取映射的部门ID
        /// </summary>
        public DataTable GetDep_map(int depId)
        {
            string strSql = string.Format(" SELECT dep_id2 FROM {0}.base_dep_map ", "nsap_bone");
            strSql += string.Format(" WHERE dep_id1={0} and map_type_id=1", depId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }
        /// <summary>
        /// 根据部门ID获取销售员ID
        /// </summary>
        public DataTable GetSboSlpCodeIds(int depId, int SboId)
        {
            string strSql = string.Format("SELECT a.sale_id,a.tech_id FROM {0}.sbo_user a", "nsap_base");
            strSql += string.Format(" LEFT JOIN {0}.base_user_detail b ON a.user_id=b.user_id", "nsap_base");
            strSql += string.Format(" WHERE b.dep_id={0} AND a.sbo_id={1}", depId, SboId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);

        }
        public DataTable GetSboSlpCodeIds(string depIds, int SboId)
        {
            string strSql = string.Format("SELECT a.sale_id,a.tech_id FROM {0}.sbo_user a", "nsap_base");
            strSql += string.Format(" LEFT JOIN {0}.base_user_detail b ON a.user_id=b.user_id", "nsap_base");
            if (string.IsNullOrEmpty(depIds)) { strSql += string.Format(" WHERE b.dep_id=-1 AND a.sbo_id={0}", SboId); } else { strSql += string.Format(" WHERE b.dep_id in ({0}) AND a.sbo_id={1}", depIds, SboId); }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }
        /// <summary>
        /// 根据用户ID获取销售员ID
        /// </summary>
        public DataTable GetSboSlpCodeId(int UserId, int SboId)
        {
            string sql = string.Format("SELECT sale_id,tech_id FROM {0}.sbo_user WHERE user_id={1} AND sbo_id={2}", "nsap_base", UserId, SboId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);

            //object obj = Sql.Action.ExecuteScalar(Sql.GBKConnectionString, CommandType.Text, sql, para);
            //return obj == null ? "0" : obj.ToString();
        }
        public DataTable GetSalesDelivery_PurchaseOrderList(string DeliveryId, string SboId)
        {
            string lstr = string.Format(@"select t0.Buy_DocEntry,t1.CardCode,t1.CardName,t1.DocTotal,t1.LicTradNum from {0}.sale_transport t0
                                        INNER JOIN {0}.buy_opor t1 on t1.DocEntry=t0.Buy_DocEntry and t1.sbo_id=t0.SboId and t1.CANCELED='N'
                                        WHERE t0.Base_DocType=24 and t0.Base_DocEntry={1} and t0.SboId={2} ", "nsap_bone", DeliveryId, SboId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, lstr, CommandType.Text, null);


        }
        /// <summary>
        /// 获取页面对应的流程类型
        /// </summary>
        /// <returns></returns>
        public string GetJobTypeByUrl(string PageUrl)
        {
            string strSql = string.Format("SELECT a.job_type_id FROM {0}.base_func a LEFT JOIN {0}.base_page b ON a.page_id=b.page_id WHERE b.page_url='{1}'", "nsap_base", PageUrl);
            object strObj = UnitWork.ExecuteScalar(ContextType.NsapBoneDbContextType, strSql, CommandType.Text, null);
            return strObj == null ? "-1" : strObj.ToString();
        }
        /// <summary>
        /// 根据销售订单号获取对应增值税发票状态
        /// </summary>
        /// <param name="orderid">销售订单号</param>
        /// <param name="sbo_id">账套ID</param>
        /// <returns></returns>
        public string GetBillStatusByOrderId(string orderid, string sbo_id)
        {
            string strsql = string.Format("select a.billstatus from {0}.finance_billapplication_master a where a.DocEntry = {1} AND a.sbo_id={2} ORDER BY updatetime desc limit 1", "nsap_bone", orderid, sbo_id);
            object res = UnitWork.ExecuteScalar(ContextType.NsapBoneDbContextType, strsql, CommandType.Text, null);
            return res == null ? "" : res.ToString();
        }
        public DataTable GetJobStateForDoc(string base_entry, string jobtypestr, string sboId)
        {
            string[] typearr = jobtypestr.Split(',');
            string temptypestr = "";
            foreach (string thisjobtype in typearr)
            {
                temptypestr += (string.IsNullOrEmpty(temptypestr) ? "" : " or ") + "job_type_id=" + thisjobtype;
            }
            string strSql = string.Format("SELECT job_id,job_type_id,job_state,upd_dt FROM {0}.wfa_job", "nsap_base");
            strSql += string.Format(" WHERE job_state<>-1 and sbo_id={0} AND base_entry={1} ", base_entry, sboId);
            if (!string.IsNullOrEmpty(temptypestr))
            {
                strSql += string.Format(" AND ({0})", temptypestr);
            }
            strSql += " order by upd_dt desc";

            DataTable dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            return dt;
        }
        public string GetBillIndicatorByOrderId(string orderid, string sbo_id)
        {
            string strsql = string.Format("select b.Name from {0}.sale_ordr a left join {0}.crm_oidc b on a.Indicator=b.Code  where a.DocEntry = {1} AND a.sbo_id={2} limit 1", "nsap_bone", orderid, sbo_id);
            object res = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strsql, CommandType.Text, null);
            return res == null ? "" : res.ToString();
        }
        public string GetEmptyAcctByOrderId(string orderid, string sbo_id)
        {
            string strsql = string.Format("select group_concat(docentry) as orinentry from {0}.sale_orin  where U_New_ORDRID = {1} AND sbo_id={2} ", "nsap_bone", orderid, sbo_id);
            object res = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strsql, CommandType.Text, null);
            return res == null ? "" : res.ToString();
        }
        public string GetTaxNoByPO(string thispo, string sboid)
        {
            string sqlstr = string.Format(@"select group_concat(d.taxno) from {0}.finance_purchasebill_detail d 
                    inner join {0}.finance_purchasebill_master m on d.sboid=m.sboid and d.billId=m.billId
                    where m.BillStatus<>-2 and d.sboid={1} and FIND_IN_SET('{2}',REPLACE(d.purchaseno,'，',','))", "nsap_bone", sboid, thispo);
            object thisstr = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, sqlstr, CommandType.Text, null);
            return thisstr == null ? "" : thisstr.ToString();
        }
        /// <summary>
        /// 获取所属公司名称
        /// </summary>
        /// <returns></returns>
        public string GetToCompanyName(string SboId, string ToCompany)
        {
            string sql = string.Format("SELECT Name FROM {0}.crm_oidc WHERE sbo_id = {1} AND Code = '{2}'", "nsap_bone", SboId, ToCompany);
            object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);

            return obj == null ? "" : obj.ToString();
        }
        public DataTable PurgetPrintNum(string sboid, string DocEntry, string bonetype)
        {
            if (bonetype.ToLower() == "ordr")
            {
                bonetype = "sale_ordr";
            }
            if (bonetype.ToLower() == "opor")
            {
                bonetype = "buy_opor";
            }

            StringBuilder str = new StringBuilder();
            str.Append("SELECT  a.PrintNo,a.PrintNumIndex ");
            str.AppendFormat(" FROM {0}." + bonetype + " a ", "nsap_bone");
            str.AppendFormat(" where a.DocEntry={0} and a.sbo_id={1}", DocEntry, sboid);

            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, str.ToString(), CommandType.Text, null);
        }
        public DataTable SAPSelectPagingHaveRowsCount(string tableName, string fieldName, int pageSize, int pageIndex, string strOrder, string strWhere, out int rowsCount)
        {
            return SAPSelectPagingNos(tableName, fieldName, pageSize, pageIndex, strOrder, strWhere, 1, out rowsCount);
        }
        public DataTable SAPSelectPagingNos(string tableName, string fieldName, int pageSize, int pageIndex, string strOrder, string strWhere, int isTotal, out int rowsCount)
        {

            List<SqlParameter> sqlParameters = new List<SqlParameter>()
        {
                new SqlParameter("@strFrom",tableName),
                new SqlParameter("@strSelect",fieldName),
                new SqlParameter("@pageSize",pageSize),
                new SqlParameter("@pageIndex",pageIndex),
                new SqlParameter("@strOrder",strOrder),
                new SqlParameter("@strWhere",strWhere),
                new SqlParameter("@isStats",isTotal),
                new SqlParameter("@rowCount",0),
            };
            sqlParameters[7].Direction = ParameterDirection.Output;
            DataTable dataTable = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, "sp_common_pager", CommandType.StoredProcedure, sqlParameters);
            if (dataTable.Rows.Count > 0)
            {
                rowsCount = isTotal == 1 ? Convert.ToInt32(sqlParameters[7].Value) : 0;

            }
            else
            {
                rowsCount = 0;

            }
            return dataTable;
        }


        #region 销售报价单导出
        /// <summary>
        ///销售报价单导出
        /// </summary>

        public string ExportShow(string val, string Indicator, string sboid, string DocEntry, string host)
        {
            DataTable dtb = ExportViewNos(sboid, DocEntry);
            if (dtb.Rows.Count > 0)
            {
                string mbval = "";
                DataTable dtNm = GetSalaIndicators(string.IsNullOrEmpty(Indicator) ? " " : Indicator, sboid);
                switch (val)
                {
                    case "0":
                        if (dtNm.Rows.Count > 0)
                        {
                            if (dtNm.Rows[0][1].ToString() == "01")
                            {
                                mbval = "销售报价单-新威尔.doc";
                            }
                            else if (dtNm.Rows[0][1].ToString() == "02")
                            {
                                mbval = "销售报价单-新能源.doc";
                            }
                            else if (dtNm.Rows[0][1].ToString() == "05")
                            {
                                mbval = "销售报价单-东莞新威.doc";
                            }
                            else if (dtNm.Rows[0][1].ToString() == "07")
                            {
                                mbval = "销售报价单-钮威.doc";
                            }
                            else
                            {
                                return "1";
                            }
                        }
                        else
                        {
                            mbval = "销售报价单.doc";
                        }
                        break;
                    case "1":
                        if (dtNm.Rows.Count > 0)
                        {
                            if (dtNm.Rows[0][1].ToString() == "01")
                            {
                                mbval = "新威尔维修报价单.doc";
                            }
                            else if (dtNm.Rows[0][1].ToString() == "02")
                            {
                                mbval = "新能源维修报价单.doc";
                            }
                            else if (dtNm.Rows[0][1].ToString() == "05")
                            {
                                mbval = "东莞新威维修报价单.doc";
                            }
                            else if (dtNm.Rows[0][1].ToString() == "07")
                            {
                                mbval = "钮威维修报价单.doc";
                            }
                            else
                            {
                                return "1";
                            }
                        }
                        else
                        {
                            mbval = "维修报价单.doc";
                        }
                        break;
                }
                string jpgName = string.Format("{0}.jpg", Guid.NewGuid().ToString());
                string path = FileHelper.OrdersFilePath.PhysicalPath;
                QRCoderHelper.BuildBarcode(int.Parse(dtb.Rows[0][0].ToString()).ToString("d4"), FileHelper.OrdersFilePath.PhysicalPath + jpgName, 3);

                List<FileHelper.WordTemplate> workMarks = new List<FileHelper.WordTemplate>();
                if (mbval == "销售报价单-新能源.doc" || mbval == "销售报价单-新威尔.doc" || mbval == "销售报价单-东莞新威.doc" || mbval == "销售报价单-钮威.doc")
                {
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 1, YCellMark = 5, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][0].ToString()) ? " " : int.Parse(dtb.Rows[0][0].ToString()).ToString("d4") });//11150
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 1, YCellMark = 6, ValueType = 1, ValueData = FileHelper.OrdersFilePath.PhysicalPath + jpgName });//D:\\barCode.jpg
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 2, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][15].ToString()) ? " " : dtb.Rows[0][15].ToString() });//2013.03.04 11:55:20
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 3, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][8].ToString()) ? " " : dtb.Rows[0][8].ToString() });//欧阳永志

                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][1].ToString()) ? " " : dtb.Rows[0][1].ToString() });//c00102
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][3].ToString()) ? " " : dtb.Rows[0][3].ToString() });//晓蓉
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 6, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][6].ToString()) ? " " : dtb.Rows[0][6].ToString() });//移动电话
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 8, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][17].ToString()) ? " " : dtb.Rows[0][17].ToString() });//0755-83108866
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 2, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][2].ToString()) ? " " : dtb.Rows[0][2].ToString() });//深圳市新威新能源电子有限公司

                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 2, YCellMark = 1, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][2].ToString()) ? " " : dtb.Rows[0][2].ToString() });//518049
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 3, YCellMark = 1, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][7].ToString()) ? " " : dtb.Rows[0][7].ToString() });//深圳市福田区下梅林梅华路207号安通大厦4楼北
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 4, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][3].ToString()) ? " " : dtb.Rows[0][3].ToString() });//晓蓉
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 4, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][4].ToString()) ? " " : dtb.Rows[0][4].ToString() });//0755-83108866
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 4, YCellMark = 6, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][8].ToString()) ? " " : dtb.Rows[0][8].ToString() });//欧阳永志
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 4, YCellMark = 8, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][9].ToString()) ? " " : dtb.Rows[0][9].ToString() });//0755-83108866

                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 4, XCellMark = 1, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][14].ToString()) ? " " : dtb.Rows[0][14].ToString() });//2013.03.04
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 4, XCellMark = 1, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][0].ToString()) ? " " : dtb.Rows[0][16].ToString() });//货运
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 4, XCellMark = 1, YCellMark = 6, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][11].ToString()) ? " " : dtb.Rows[0][11].ToString() });//货到付款
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 4, XCellMark = 2, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][10].ToString()) ? " " : dtb.Rows[0][10].ToString().Replace("<br>", " ") });//深圳市福田区下梅林梅华路207号安通大厦4楼北
                }
                else if (mbval == "新威尔维修报价单.doc" || mbval == "新能源维修报价单.doc" || mbval == "维修报价单.doc" || mbval == "东莞新威维修报价单.doc" || mbval == "钮威维修报价单.doc")
                {
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 1, YCellMark = 5, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][0].ToString()) ? " " : dtb.Rows[0][0].ToString() });//11150
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 1, YCellMark = 6, ValueType = 1, ValueData = FileHelper.OrdersFilePath.PhysicalPath + jpgName });//D:\\barCode.jpg
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 2, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][15].ToString()) ? " " : dtb.Rows[0][15].ToString() });//2013.03.04 11:55:20
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 3, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][8].ToString()) ? " " : dtb.Rows[0][8].ToString() });//欧阳永志
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 3, YCellMark = 5, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][6].ToString()) ? " " : dtb.Rows[0][6].ToString() });//欧阳永志

                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][1].ToString()) ? " " : dtb.Rows[0][1].ToString() });//c00102
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][3].ToString()) ? " " : dtb.Rows[0][3].ToString() });//晓蓉
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 6, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][4].ToString()) ? " " : dtb.Rows[0][4].ToString() });//0755-83108866
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 2, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][2].ToString()) ? " " : dtb.Rows[0][2].ToString() });//深圳市新威新能源电子有限公司
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 2, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][17].ToString()) ? " " : dtb.Rows[0][17].ToString() });//传真
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 3, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][7].ToString()) ? " " : dtb.Rows[0][7].ToString() });//手机
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 3, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][6].ToString()) ? " " : dtb.Rows[0][6].ToString() });//深圳市新威新能源电子有限公司

                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 1, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][10].ToString()) ? " " : dtb.Rows[0][10].ToString() });//518049
                }
                else if (mbval == "销售报价单.doc")
                {
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 1, YCellMark = 5, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][0].ToString()) ? " " : dtb.Rows[0][0].ToString() });//11150
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 1, YCellMark = 6, ValueType = 1, ValueData = FileHelper.OrdersFilePath.PhysicalPath + jpgName });//D:\\barCode.jpg
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 2, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][15].ToString()) ? " " : dtb.Rows[0][15].ToString() });//2013.03.04 11:55:20
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 3, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][8].ToString()) ? " " : dtb.Rows[0][8].ToString() });//欧阳永志
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 3, YCellMark = 5, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][6].ToString()) ? " " : dtb.Rows[0][6].ToString() });//欧阳永志

                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][1].ToString()) ? " " : dtb.Rows[0][1].ToString() });//客户编号
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][3].ToString()) ? " " : dtb.Rows[0][3].ToString() });//联系人
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 6, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][4].ToString()) ? " " : dtb.Rows[0][4].ToString() });//移动电话
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 8, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][17].ToString()) ? " " : dtb.Rows[0][17].ToString() });//传真
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 10, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][6].ToString()) ? " " : dtb.Rows[0][6].ToString() }); //手机
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 2, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][2].ToString()) ? " " : dtb.Rows[0][2].ToString() });//客户名称
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 3, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][7].ToString()) ? " " : dtb.Rows[0][7].ToString() });//客户地址
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 3, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][19].ToString()) ? " " : dtb.Rows[0][19].ToString() });//交货地址

                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 1, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][11].ToString()) ? " " : dtb.Rows[0][11].ToString() });//付款条款
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 1, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][14].ToString()) ? " " : dtb.Rows[0][14].ToString() });//交货日期
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 2, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][16].ToString()) ? " " : dtb.Rows[0][16].ToString() });//交货方式
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 2, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][20].ToString()) ? " " : dtb.Rows[0][20].ToString() });//验收期限
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 3, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][10].ToString()) ? " " : dtb.Rows[0][10].ToString().Replace("<br>", " ") });//备注
                }
                DataTable dTable = new DataTable();
                dTable.Columns.Add("C1", typeof(string));
                dTable.Columns.Add("C2", typeof(string));
                dTable.Columns.Add("C3", typeof(string));
                dTable.Columns.Add("C4", typeof(string));
                dTable.Columns.Add("C5", typeof(string));
                dTable.Columns.Add("C6", typeof(string));
                dTable.Columns.Add("C7", typeof(string));

                DataTable dtbs = ExportViews(sboid, DocEntry);
                for (int i = 0; i < dtbs.Rows.Count; i++)
                {
                    DataRow dRow = dTable.NewRow();
                    dRow[0] = i + 1;//"1";
                    dRow[1] = string.IsNullOrEmpty(dtbs.Rows[i][1].ToString()) ? " " : dtbs.Rows[i][1].ToString(); //"CT-3008-5V5mA";
                    dRow[2] = string.IsNullOrEmpty(dtbs.Rows[i][2].ToString()) ? " " : dtbs.Rows[i][2].ToString();//"BTS-5V5mA-8通道-钢壳-四线扣式圆头夹具-3U19\"白色机箱";
                    dRow[3] = string.IsNullOrEmpty(dtbs.Rows[i][3].ToString()) ? " " : dtbs.Rows[i][3].ToString(); //"1";
                    dRow[4] = string.IsNullOrEmpty(dtbs.Rows[i][4].ToString()) ? " " : dtbs.Rows[i][4].ToString(); //"Pcs";
                    dRow[5] = string.IsNullOrEmpty(dtbs.Rows[i][5].ToString()) ? " " : dtbs.Rows[i][5].ToString(); //"3200.000000";
                    dRow[6] = string.IsNullOrEmpty(dtbs.Rows[i][6].ToString()) ? " " : dtbs.Rows[i][6].ToString(); //"3200.00";
                    dTable.Rows.Add(dRow);
                }
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 0, TableMark = 1, ValueType = 2, ValueData = dTable });
                if (mbval == "销售报价单-新能源.doc" || mbval == "销售报价单-新威尔.doc" || mbval == "销售报价单-东莞新威.doc" || mbval == "销售报价单-钮威.doc")
                {
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 2, TableMark = 1, XCellMark = 1, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][13].ToString()) ? " " : dtb.Rows[0][13].ToString() });
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 2, TableMark = 1, XCellMark = 6, YCellMark = 2, ValueType = 0, ValueData = "" });
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 2, TableMark = 1, XCellMark = 6, YCellMark = 3, ValueType = 0, ValueData = DateTime.Now.ToString() });
                }
                else if (mbval == "新威尔维修报价单.doc" || mbval == "新能源维修报价单.doc" || mbval == "维修报价单.doc" || mbval == "东莞新威维修报价单.doc" || mbval == "钮威维修报价单.doc")
                {
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 2, TableMark = 1, XCellMark = 2, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][13].ToString()) ? " " : dtb.Rows[0][13].ToString() });
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 2, TableMark = 1, XCellMark = 6, YCellMark = 2, ValueType = 0, ValueData = _serviceBaseApp.ToString() });
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 2, TableMark = 1, XCellMark = 6, YCellMark = 3, ValueType = 0, ValueData = DateTime.Now.ToString() });
                }
                else if (mbval == "销售报价单.doc")
                {
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 2, TableMark = 1, XCellMark = 1, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][13].ToString()) ? " " : dtb.Rows[0][13].ToString() });//合计金额
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 2, TableMark = 1, XCellMark = 4, YCellMark = 3, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][18].ToString()) ? " " : dtb.Rows[0][18].ToString() });//操作人
                }
                string pdfName = string.Format("{0}.pdf", Guid.NewGuid().ToString());
                var s = FileHelper.TempletFilePath.PhysicalPath + mbval;
                var ss = FileHelper.OrdersFilePath.PhysicalPath + jpgName + pdfName;
                var sss = FileHelper.OrdersFilePath.PhysicalPath + pdfName;
                _logger.LogInformation(FileHelper.TempletFilePath.PhysicalPath);
                _logger.LogInformation(host + FileHelper.OrdersFilePath.VirtualPath);
                _logger.LogInformation(FileHelper.OrdersFilePath.PhysicalPath);
                if (FileHelper.DOCTemplateToPDF(FileHelper.TempletFilePath.PhysicalPath + mbval, FileHelper.OrdersFilePath.PhysicalPath + pdfName, workMarks))
                {
                    return host + FileHelper.OrdersFilePath.VirtualPath + pdfName;
                }
                else
                {
                    return "false";
                }
            }
            else
            {
                return "0";
            }
        }

        /// <summary>
        /// 导出报价单（新）
        /// </summary>
        /// <param name="sboid">帐套Id</param>
        /// <param name="DocEntry">单据编号</param>
        /// <returns>成功返回字节流，失败抛出异常</returns>
        public async Task<byte[]> ExportShowNew(string sboid, string DocEntry)
        {
            DataTable dtb = ExportViewNos(sboid, DocEntry);
            DataTable dtbs = ExportViews(sboid, DocEntry);
            DataTable dtbTotal = ExportTotalAmount(sboid, DocEntry);
            var logopath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "logo.png");
            var logostr = "";
            using (var fs = new FileStream(logopath, FileMode.Open))
            {
                var photo = new byte[fs.Length];
                fs.Position = 0;
                await fs.ReadAsync(photo, 0, photo.Length);
                logostr = Convert.ToBase64String(photo);
                Console.WriteLine(logostr);
            }

            //公司标识
            var indicator = dtb.Rows[0][24].ToString();
            var companyAddressData = new Category();
            string companyName = "";
            string Chapter = "";
            string Chapterpath = "";
            string cssWeight = "";
            string cssSize = "";
            string DiscTotal = "";
            string DiscSum = "";
            string DiscPrcnt = "";
            if (!string.IsNullOrEmpty(indicator))
            {
                //公司地址信息,在字典中维护
                companyAddressData = await UnitWork.Find<Category>(c => c.TypeId == "SYS_CompanyAddress" && c.DtValue == indicator).FirstOrDefaultAsync();
                companyName = companyAddressData == null ? "" : companyAddressData.Name;
                cssWeight = "normal";
                cssSize = "13px";
                DiscPrcnt = "含税： " + (string.IsNullOrEmpty(dtb.Rows[0][22].ToString()) || (dtb.Rows[0][22].ToString() == "0") ? " " : dtb.Rows[0][22].ToString()) + "%";
                DiscSum = dtb.Rows[0][28].ToString() == "0.00" ? " " : "折扣金额：  " + _serviceBaseApp.MoneyToCoin(dtb.Rows[0][28].ToDecimal(), 2);
                DiscTotal = dtb.Rows[0][28].ToString() == "0.00" ? " " : "总计金额：   " + _serviceBaseApp.MoneyToCoin(dtbTotal.Rows[0][0].ToDecimal(), 2);
            }

            if (indicator == "01")
            {
                Chapterpath = Path.Combine(Directory.GetCurrentDirectory(), "Templates\\seal", NewareName + ".png");
            }
            else if (indicator == "02")
            {
                Chapterpath = Path.Combine(Directory.GetCurrentDirectory(), "Templates\\seal", NewllName + ".png");
            }
            else if (indicator == "05")
            {
                Chapterpath = Path.Combine(Directory.GetCurrentDirectory(), "Templates\\seal", DGNewareName + ".png");
            }
            else if (string.IsNullOrEmpty(indicator) && dtb.Rows[0][26].ToString() != "RMB")
            {
                Chapterpath = Path.Combine(Directory.GetCurrentDirectory(), "Templates\\seal", WBName + ".png");
                companyName = "NEWARE";
                cssWeight = "bold";
                cssSize = "39px";
            }
            else
            {
                throw new Exception("报价单/订单所选择标识无打印模板，无法进行打印操作");
            }

            using (var fs = new FileStream(Chapterpath, FileMode.Open))
            {
                var photo = new byte[fs.Length];
                fs.Position = 0;
                await fs.ReadAsync(photo, 0, photo.Length);
                Chapter = Convert.ToBase64String(photo);
                Console.WriteLine(Chapter);
            }

            var PrintSalesQuotation = new PrintSalesQuotation
            {
                DocEntry = string.IsNullOrEmpty(dtb.Rows[0][0].ToString()) ? " " : dtb.Rows[0][0].ToString(),
                DateTime = string.IsNullOrEmpty(dtb.Rows[0][25].ToString()) ? " " : dtb.Rows[0][25].ToString(),
                SalseName = string.IsNullOrEmpty(dtb.Rows[0][8].ToString()) ? " " : dtb.Rows[0][8].ToString(),
                CardCode = string.IsNullOrEmpty(dtb.Rows[0][1].ToString()) ? " " : dtb.Rows[0][1].ToString(),
                Name = string.IsNullOrEmpty(dtb.Rows[0][3].ToString()) ? " " : dtb.Rows[0][3].ToString(),
                Tel = string.IsNullOrEmpty(dtb.Rows[0][4].ToString()) ? " " : dtb.Rows[0][4].ToString(),
                Fax = string.IsNullOrEmpty(dtb.Rows[0][17].ToString()) ? " " : dtb.Rows[0][17].ToString(),
                Memo = string.IsNullOrEmpty(dtb.Rows[0][9].ToString()) ? " " : dtb.Rows[0][9].ToString(),
                CardName = string.IsNullOrEmpty(dtb.Rows[0][2].ToString()) ? " " : dtb.Rows[0][2].ToString(),
                Address = string.IsNullOrEmpty(dtb.Rows[0][7].ToString()) ? " " : dtb.Rows[0][7].ToString(),
                Address2 = companyAddressData == null ? "" : companyAddressData.Description,
                PymntGroup = string.IsNullOrEmpty(dtb.Rows[0][11].ToString()) ? " " : dtb.Rows[0][11].ToString(),
                Comments = string.IsNullOrEmpty(dtb.Rows[0][10].ToString()) ? " " : dtb.Rows[0][10].ToString().Replace("<br>", " "),
                DocTotal = string.IsNullOrEmpty(dtb.Rows[0][13].ToString()) ? " " : dtb.Rows[0][13].ToString().Split(' ')[0] + " " + _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(dtb.Rows[0][13].ToString().Split(' ')[1]), 2),
                DATEFORMAT = string.IsNullOrEmpty(dtb.Rows[0][14].ToString()) ? " " : dtb.Rows[0][14].ToString(),
                logo = logostr,
                QRcode = QRCoderHelper.CreateQRCodeToBase64(DocEntry),
                ReimburseCosts = new List<ReimburseCost>()
            };

            for (int i = 0; i < dtbs.Rows.Count; i++)
            {
                ReimburseCost scon = new ReimburseCost
                {
                    ItemCode = string.IsNullOrEmpty(dtbs.Rows[i][2].ToString()) ? " " : dtbs.Rows[i][2].ToString(),
                    Dscription = string.IsNullOrEmpty(dtbs.Rows[i][3].ToString()) ? " " : dtbs.Rows[i][3].ToString(),
                    Quantity = string.IsNullOrEmpty(dtbs.Rows[i][4].ToString()) ? " " : dtbs.Rows[i][4].ToString(),
                    unitMsr = string.IsNullOrEmpty(dtbs.Rows[i][5].ToString()) ? " " : dtbs.Rows[i][5].ToString(),
                    Price = string.IsNullOrEmpty(dtbs.Rows[i][6].ToString()) ? " " : _serviceBaseApp.MoneyToCoin(dtbs.Rows[i][6].ToDecimal() , 4),
                    Money = string.IsNullOrEmpty(dtbs.Rows[i][7].ToString()) ? " " : _serviceBaseApp.MoneyToCoin(dtbs.Rows[i][7].ToDecimal(), 2)
                };

                PrintSalesQuotation.ReimburseCosts.Add(scon);
            }

            var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "PrintSalesQuotationheader.html");
            var text = System.IO.File.ReadAllText(url);
            text = text.Replace("@Model.Data.logo", PrintSalesQuotation.logo);
            text = text.Replace("@Model.Data.DocEntry", PrintSalesQuotation.DocEntry);
            text = text.Replace("@Model.Data.DateTime", PrintSalesQuotation.DateTime);
            text = text.Replace("@Model.Data.QRcode", PrintSalesQuotation.QRcode);
            text = text.Replace("@Model.Data.SalseName", PrintSalesQuotation.SalseName);
            text = text.Replace("@Model.Data.CardCode", PrintSalesQuotation.CardCode);
            text = text.Replace("@Model.Data.Name", PrintSalesQuotation.Name);
            text = text.Replace("@Model.Data.Tel", PrintSalesQuotation.Tel);
            text = text.Replace("@Model.Data.Fax", PrintSalesQuotation.Fax);
            text = text.Replace("@Model.Data.CardName", PrintSalesQuotation.CardName);
            text = text.Replace("@Model.Data.Address", PrintSalesQuotation.Address);
            text = text.Replace("@Model.Data.Addrestwo", PrintSalesQuotation.Address2);
            text = text.Replace("@Model.Data.SalseName", PrintSalesQuotation.SalseName);
            text = text.Replace("@Model.Data.Cellolar", PrintSalesQuotation.Memo);
            text = text.Replace("@Model.Data.DATEFORMAT", PrintSalesQuotation.DATEFORMAT);
            text = text.Replace("@Model.Data.PymntGroup", PrintSalesQuotation.PymntGroup);
            text = text.Replace("@Model.Data.Comments", PrintSalesQuotation.Comments);
            var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"PrintSalesQuotationheader{PrintSalesQuotation.DocEntry}.html");
            System.IO.File.WriteAllText(tempUrl, text, Encoding.Unicode);
            var footUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "PrintSalesQuotationfooter.html");
            var foottext = System.IO.File.ReadAllText(footUrl);
            foottext = foottext.Replace("@Model.Data.Chapter", Chapter);
            foottext = foottext.Replace("@Model.Data.DocTotal", dtb.Rows[0][28].ToString() == "0.00" ? "总计金额：     " +  PrintSalesQuotation.DocTotal : "折扣后金额：      " +  PrintSalesQuotation.DocTotal);
            foottext = foottext.Replace("@Model.Data.Company", companyName);
            foottext = foottext.Replace("@Model.Data.Address", companyAddressData == null ? "" : companyAddressData.Description);
            foottext = foottext.Replace("@Model.Data.Weight", cssWeight);
            foottext = foottext.Replace("@Model.Data.Size", cssSize);
            foottext = foottext.Replace("@Model.Data.DiscTotal", DiscTotal);
            foottext = foottext.Replace("@Model.Data.DiscSum", DiscSum);
            foottext = foottext.Replace("@Model.Data.DiscPrcnt", DiscPrcnt);
            var foottempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"PrintSalesQuotationfooter{PrintSalesQuotation.DocEntry}.html");
            System.IO.File.WriteAllText(foottempUrl, foottext, Encoding.Unicode);
            byte[] basecode = await ExportAllHandler.Exporterpdf(PrintSalesQuotation, "PrintSalesQuotation.cshtml", pdf =>
             {
                 pdf.Orientation = Orientation.Portrait;
                 pdf.IsWriteHtml = true;
                 pdf.PaperKind = PaperKind.A4;
                 pdf.IsEnablePagesCount = true;
                 pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                 pdf.FooterSettings = new FooterSettings() { HtmUrl = foottempUrl };
             });

            System.IO.File.Delete(tempUrl);
            System.IO.File.Delete(foottempUrl);
            return basecode;
        }

        /// <summary>
        /// 销售报价单主数据导出
        /// </summary>
        public DataTable ExportViewNos(string sboid, string DocEntry)
        {
            StringBuilder str = new StringBuilder();
            str.Append("SELECT distinct a.DocEntry,a.CardCode,a.CardName,b.Name,b.Tel1,b.Tel2,b.Cellolar,b.Address,c.SlpName,c.Memo,a.Comments,d.PymntGroup,");
            str.Append(" a.DocTotal,CONCAT(a.DocCur,' ',ROUND(IF(a.DocCur = 'RMB',a.DocTotal,IFNUll(a.DocTotalFC,0.000000)),2))  ,DATE_FORMAT(a.DocDueDate,'%Y.%m.%d'),DATE_FORMAT(a.DocDate,'%Y.%m.%d'),a.U_ShipName,b.Fax,a.U_YGMD,a.Address2");
            str.Append(" ,a.U_YSQX,a.BnkAccount,a.U_SL,a.NumAtCard,a.indicator, DATE_FORMAT(h.log_dt,'%Y.%m.%d'), a.DocCur,ROUND(a.DiscPrcnt,2) as DiscPrcnt,ROUND(IF(a.DocCur = 'RMB',a.DiscSum,(a.DiscSum/a.DocRate)),2) as DiscSum,a.DocRate ");
            str.AppendFormat(" FROM {0}.sale_oqut a ", "nsap_bone");
            str.AppendFormat(" left join {0}.crm_ocpr b on a.CntctCode=b.CntctCode and a.sbo_id=b.sbo_id and a.CardCode=b.CardCode ", "nsap_bone");
            str.AppendFormat(" left join {0}.crm_oslp c on a.SlpCode=c.SlpCode and a.sbo_id=c.sbo_id ", "nsap_bone");
            str.AppendFormat(" left join {0}.crm_octg d on a.GroupNum=d.GroupNum AND a.sbo_id=d.sbo_id ", "nsap_bone");
            str.AppendFormat(" left join (select f.sbo_id,f.sbo_itf_return,DATE_FORMAT(g.log_dt,'%Y.%m.%d') as log_dt ");
            str.AppendFormat(" from {0}.wfa_job f ", "nsap_base");
            str.AppendFormat(" left join {0}.wfa_log g on f.job_id= g.job_id ", "nsap_base");
            str.AppendFormat(" where sbo_itf_return = {0} and job_nm = '销售报价单' order by g.log_dt desc limit 1) h on a.sbo_id = h.sbo_id and a.DocEntry = h.sbo_itf_return", DocEntry);
            str.AppendFormat(" where a.DocEntry={0} and a.sbo_id={1} ", DocEntry, sboid);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, str.ToString(), CommandType.Text, null);
        }
        /// <summary>
        /// 获取销售报价单内部标识
        /// </summary>
        public DataTable GetSalaIndicators(string Indicator, string sbo_id)
        {
            string strSql = string.Format(@"SELECT a.Name,a.Code FROM {0}.crm_oidc a
		                                    LEFT JOIN {0}.sale_oqut b ON a.Code=b.Indicator AND a.sbo_id=b.sbo_id
		                                    WHERE b.Indicator='{1}' AND b.sbo_id={2} ", "nsap_bone", Indicator, sbo_id);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }
        /// <summary>
        /// 销售报价单行明细导出
        /// </summary>
        public DataTable ExportViews(string sboid, string DocEntry)
        {
            StringBuilder str = new StringBuilder();
            str.Append(" SELECT ROW_NUMBER() OVER (ORDER BY b.sbo_id) RowNum, b.sbo_id,b.ItemCode,b.Dscription,ROUND(b.Quantity,2),b.unitMsr,ROUND(b.Price,6),ROUND(b.Quantity*b.Price,2)");
            str.AppendFormat(" from {0}.sale_qut1  b ", "nsap_bone");
            str.AppendFormat(" LEFT JOIN {0}.sale_oqut a on b.DocEntry=a.DocEntry and b.sbo_id=a.sbo_id ", "nsap_bone");
            str.AppendFormat(" where a.DocEntry={0} and a.sbo_id={1} ", DocEntry, sboid);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, str.ToString(), CommandType.Text, null);

        }

        /// <summary>
        /// 销售报价单行折扣前总金额计算
        /// </summary>
        public DataTable ExportTotalAmount(string sboid, string DocEntry)
        {
            StringBuilder str = new StringBuilder();
            str.Append(" SELECT CONCAT(c.Currency,' ', ROUND( SUM( c.Quantity * c.Price ), 2 )) AS TotalAmount  FROM (");
            str.Append(" SELECT ROW_NUMBER() OVER (ORDER BY b.sbo_id) RowNum, b.sbo_id,b.ItemCode,b.Currency,b.Dscription,ROUND(b.Quantity,2) AS Quantity,b.unitMsr,ROUND(b.Price,6) AS Price,CONCAT(b.Currency,' ',ROUND(b.Quantity*b.Price,2))");
            str.AppendFormat(" from {0}.sale_qut1  b ", "nsap_bone");
            str.AppendFormat(" LEFT JOIN {0}.sale_oqut a on b.DocEntry=a.DocEntry and b.sbo_id=a.sbo_id ", "nsap_bone");
            str.AppendFormat(" where a.DocEntry={0} and a.sbo_id={1} ) c", DocEntry, sboid);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, str.ToString(), CommandType.Text, null);
        }
        #endregion
        #region 判断审核里是否已经提交该单据
        /// <summary>
        /// 判断审核里是否已经提交该单据（销售订单）
        /// </summary>
        public bool IsExistDoc(string base_entry, string base_type, string sboId, string func_id)
        {
            bool result = false;
            string strSql = string.Format("SELECT COUNT(*) FROM {0}.wfa_job", "nsap_base");
            strSql += string.Format(" WHERE (base_type={0} OR base_type=-5 )AND sbo_id={1} AND base_entry={2}  AND (job_state=1 OR job_state=0  OR job_state=2  OR job_state=4)AND job_type_id=(SELECT job_type_id FROM nsap_base.base_func WHERE func_id={3} LIMIT 1)", base_type, sboId, base_entry, func_id);

            object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            if (obj.ToString() == "0" || obj == null)
            {
                result = false;
            }
            else { result = true; }

            return result;
        }
        #endregion
        #region 判断审核里是否已经提交该销售报价单
        /// <summary>
        /// 判断审核里是否已经提交该单据（销售报价单）
        /// </summary>
        public bool IsExistDocOqut(string job_id, string base_type, string job_type_id)
        {
            bool result = false;
            string strSql = string.Format("SELECT COUNT(*) FROM {0}.wfa_job", "nsap_base");
            strSql += string.Format(" WHERE (base_type={0}) AND job_id={1}  AND (job_state=1 )AND job_type_id={2}", base_type, job_id, job_type_id);

            object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            if (obj.ToString() == "0" || obj == null)
            {
                result = false;
            }
            else { result = true; }

            return result;
        }
        #endregion
        #region 货币
        public List<DropPopupDocCurDto> DropPopupDocCur()
        {
            string strSql = " SELECT CurrCode AS id,CurrName AS name FROM " + "nsap_bone" + ".crm_ocrn";
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).Tolist<DropPopupDocCurDto>();
        }
        #endregion
        #region 仓库
        /// <summary>
        /// 仓库
        /// </summary>
        public List<DropPopupDocCurDto> DropPopupWhsCode(int sbo_id)
        {
            string strSql = string.Format(" SELECT WhsCode AS id,WhsName AS name FROM {0}.store_owhs WHERE sbo_id={1}", "nsap_bone", sbo_id);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).Tolist<DropPopupDocCurDto>();
        }
        #endregion
        #region 查看销售交货详细主数据
        ///<summary>
        ///查看销售交货详细主数据
        ///</summary>
        public DataTable QuerySaleDeliveryDetailsV1(string DocNum, bool ViewCustom, string type, bool ViewSales, int SboId, bool isSql)
        {
            DataTable dt = GetSboNamePwd(SboId);
            string dRowData = string.Empty; string sqlconn = ""; string sboname = ""; string isOpen = "1";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); sqlconn = dt.Rows[0][5].ToString(); sboname = dt.Rows[0][0].ToString(); } //isOpen = "1"改为isOpen = "0"
            if (isSql && isOpen == "1")
            {
                return QuerySaleDeliveryDetailsSql(DocNum, ViewCustom, type, ViewSales, sboname);
            }
            else
            {
                return QuerySaleDeliveryDetails(DocNum, ViewCustom, type, ViewSales, SboId);
            }
        }
        ///<summary>
        ///查看销售交货详细主数据
        ///</summary>
        public DataTable QuerySaleDeliveryDetailsSql(string DocNum, bool ViewCustom, string tablename, bool ViewSales, string sboname)
        {
            DataTable dt = GetCustomFields(tablename);
            if (tablename == "sale_ordr") { tablename = "ORDR"; }
            if (tablename == "sale_odln") { tablename = "ODLN"; }
            if (tablename == "sale_oqut") { tablename = "OQUT"; }
            if (tablename == "sale_oinv") { tablename = "OINV"; }
            if (tablename == "sale_orin") { tablename = "ORIN"; }
            if (tablename == "sale_ordn") { tablename = "ORDN"; }
            if (tablename == "buy_opqt") { tablename = "OPQT"; }
            if (tablename == "buy_opor") { tablename = "OPOR"; }
            if (tablename == "buy_opdn") { tablename = "OPDN"; }
            if (tablename == "buy_opch") { tablename = "OPCH"; }
            if (tablename == "buy_orpc") { tablename = "ORPC"; }
            if (tablename == "buy_orpd") { tablename = "ORPD"; }
            string CustomFields = "";
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (IsExist(tablename, dt.Rows[i][0].ToString()))
                    {
                        CustomFields += "," + dt.Rows[i][0].ToString();
                    }
                }
            }
            if (string.IsNullOrEmpty(sboname)) { sboname = ""; } else { sboname = sboname + ".dbo."; }
            int Custom = 0; if (ViewCustom) { Custom = 1; }
            int Sales = 0; if (ViewSales) { Sales = 1; }
            string U_FPLB = string.Empty;
            if (IsExist(sboname + tablename, "U_FPLB"))
            {
                U_FPLB = ",U_FPLB";
            }
            string U_YWY = string.Empty;
            if (IsExist(sboname + tablename, "U_YWY"))
            {
                U_YWY = ",U_YWY";
            }
            string U_New_ORDRID = string.Empty;
            if (IsExist(sboname + tablename, "U_New_ORDRID"))

            {
                U_New_ORDRID = ",U_New_ORDRID";
            }
            string U_EshopNo = string.Empty;
            if (tablename.ToUpper() == "OQUT" || tablename.ToUpper() == "ORDR")
            {
                U_EshopNo = ",U_EshopNo";
            }
            string strSql = string.Format("SELECT U_YGMD,CardCode,CASE WHEN 1 = " + Custom + " THEN CardName ELSE '******' END AS CardName,CASE WHEN 1 = " + Custom + " THEN CntctCode ELSE 0 END AS CntctCode,CASE WHEN 1 = " + Custom + " THEN NumAtCard ELSE '******' END AS NumAtCard,CASE WHEN 1 = " + Custom + " THEN DocCur ELSE '' END AS DocCur,CASE WHEN 1 = " + Custom + " THEN DocRate ELSE 0 END AS DocRate");
            strSql += string.Format(",DocNum,DocType,CASE WHEN 1 = " + Sales + " THEN DiscSum ELSE 0 END AS DiscSum,CASE WHEN 1 = " + Sales + " THEN DiscPrcnt ELSE 0 END AS DiscPrcnt,CASE WHEN 1 = " + Sales + " THEN TotalExpns ELSE 0 END AS TotalExpns,CASE WHEN 1 = " + Sales + " THEN VatSum ELSE 0 END AS VatSum,CASE WHEN 1 = " + Sales + " THEN DocTotal ELSE 0 END AS DocTotal,DocDate,DocDueDate,TaxDate,SupplCode,ShipToCode,PayToCode,Address,Address2,Comments,SlpCode,TrnspCode,GroupNum,PeyMethod,VatPercent,LicTradNum,Indicator,PartSupply,ReqDate,CANCELED");
            strSql += string.Format("" + CustomFields + "");
            strSql += string.Format(",DpmPrcnt,Printed,DocStatus,OwnerCode{0},U_SL{1}{2}{3}", U_FPLB, U_YWY, U_New_ORDRID, U_EshopNo);
            strSql += string.Format(",CASE WHEN 1 = " + Sales + " THEN DocTotalFC ELSE 0 END AS DocTotalFC,CASE WHEN 1 = " + Sales + " THEN DiscSumFC ELSE 0 END AS DiscSumFC");
            strSql += string.Format(" FROM " + sboname + tablename + "");
            strSql += string.Format(" WHERE DocEntry={0}", DocNum);
            return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql, CommandType.Text, null);
        }

        ///<summary>
        ///查看销售交货详细主数据
        ///</summary>
        public DataTable QuerySaleDeliveryDetails(string DocNum, bool ViewCustom, string tablename, bool ViewSales, int SboId)
        {
            DataTable dt = GetCustomFields(tablename);
            string CustomFields = "";
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (IsExistMySql(tablename, dt.Rows[i][0].ToString()))
                    {
                        CustomFields += "," + dt.Rows[i][0].ToString();
                    }
                }
            }
            string U_YWY = string.Empty;
            if (IsExistMySql(tablename, "U_YWY"))
            {
                U_YWY = ",U_YWY";
            }
            string U_New_ORDRID = string.Empty;
            if (IsExistMySql(tablename, "U_New_ORDRID"))
            {
                U_New_ORDRID = ",U_New_ORDRID";
            }
            string U_EshopNo = string.Empty;
            if (tablename.ToLower() == "sale_oqut")
            {
                U_EshopNo = ",U_EshopNo";
            }
            string strSql = string.Format("SELECT U_YGMD,CardCode,IF(" + ViewCustom + ",CardName,'******' ) AS CardName,IF(" + ViewCustom + ",CntctCode,0) AS CntctCode,IF(" + ViewCustom + ",NumAtCard,'******' ) AS NumAtCard,IF(" + ViewCustom + ",DocCur,'') AS DocCur,IF(" + ViewCustom + ",DocRate,0) AS DocRate");
            strSql += string.Format(",DocNum,DocType,IF(" + ViewSales + ",DiscSum,0) AS DiscSum,IF(" + ViewSales + ",DiscPrcnt,0) AS DiscPrcnt,IF(" + ViewSales + ",TotalExpns,0) AS TotalExpns,IF(" + ViewSales + ",VatSum,0) AS VatSum,IF(" + ViewSales + ",DocTotal,0) AS DocTotal,DocDate,DocDueDate,TaxDate,SupplCode,ShipToCode,PayToCode,Address,Address2,Comments,BillDocType,SlpCode,TrnspCode,GroupNum,PeyMethod,VatPercent,LicTradNum,Indicator,PartSupply,ReqDate,CANCELED");
            strSql += string.Format("" + CustomFields + "");
            strSql += string.Format(",DpmPrcnt,Printed,DocStatus,OwnerCode,U_FPLB,U_SL{0}{1}{2}", U_YWY, U_New_ORDRID, U_EshopNo);
            strSql += string.Format(" FROM {0}." + tablename + "", "nsap_bone");
            strSql += string.Format(" WHERE DocNum={0} AND sbo_id={1}", DocNum, SboId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }
        #endregion
        #region 开关项
        //<summary>
        //判断开关项是否开启
        //</summary>
        //<param name="tablename"></param>
        //<param name="filevalue"></param>
        //<returns></returns>
        public bool IsSwitching(string tablename, string filevalue)
        {
            bool isClose = true;
            string strSql = string.Format("SELECT is_valid FROM {0}.base_sys_switch WHERE table_nm='{1}' AND fld_nm='{2}'", "nsap_bone", tablename, filevalue);
            object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            if (obj == null || obj.ToString() == "0") { isClose = false; }
            return isClose;
        }
        #endregion
        #region 销售员
        /// <summary>
        /// 销售员
        /// </summary>
        /// <returns></returns>
        public List<GetItemTypeCustomValueDto> DropPopupSlpCode(int SboID)
        {
            string strSql = " SELECT SlpCode,SlpName FROM " + "nsap_bone" + ".crm_oslp WHERE sbo_id=" + SboID + "";
            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql, CommandType.Text, null).Tolist<GetItemTypeCustomValueDto>();
        }
        #endregion
        /// <summary>
        /// 获取指定联系人（名称）
        /// </summary>
        public string GetConfiguresCntctPrsn(string CntctCode, int SboId, bool isSql)
        {
            DataTable dt = GetSboNamePwd(SboId);
            string dRowData = string.Empty; string isOpen = "0";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); }
            if (isSql && isOpen == "1")
            {
                return GetConfiguresCntctPrsnSql(CntctCode, SboId);
            }
            else
            {
                return GetConfiguresCntctPrsn(CntctCode, SboId);
            }
        }
        /// <summary>
        /// 获取指定联系人（名称）
        /// </summary>
        public string GetConfiguresCntctPrsn(string CntctCode, int SboId)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT `Name` FROM {0}.crm_ocpr WHERE CntctCode=" + CntctCode + " and sbo_id=" + SboId + " limit 1", "nsap_bone");
            object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
            return obj == null ? "" : obj.ToString();
        }
        public string GetConfiguresCntctPrsnSql(string CntctCode, int SboId)
        {
            string sql = "SELECT [Name] From OCPR WHERE CntctCode=" + CntctCode;
            object obj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, sql, CommandType.Text, null);
            return obj == null ? "" : obj.ToString();
        }
        public string GetSumBalDue(string slpCode, string CardCode, string type, int SboId)
        {
            //return NSAP.Data.Sales.BillDelivery.GetSumBalDue(slpCode, CardCode, type, SboId.ToString()).DataTableToJSON();
            string balstr = ""; decimal BalDue = 0; decimal Total = 0;
            DataTable sbotable = DropListSboId();
            sbotable.Columns.Add("BalSboAmount", typeof(decimal));
            foreach (DataRow sborow in sbotable.Rows)
            {
                DataTable baltable = new DataTable();
                if (!string.IsNullOrEmpty(slpCode))
                {
                    baltable = GetSalesSboBalPercent(slpCode, type, sborow["id"].ToString());
                    if (baltable.Rows.Count > 0)
                    {
                        decimal tempDue = 0, tempTotal = 0;
                        if (!string.IsNullOrEmpty(baltable.Rows[0]["BalDue"].ToString()) && decimal.TryParse(baltable.Rows[0]["BalDue"].ToString(), out tempDue))
                        {
                            BalDue += tempDue;
                        }
                        if (!string.IsNullOrEmpty(baltable.Rows[0]["Total"].ToString()) && decimal.TryParse(baltable.Rows[0]["Total"].ToString(), out tempTotal))
                        {
                            Total += tempTotal;
                        }
                        sborow["BalSboAmount"] = tempDue.ToString("#0.00");
                    }
                }
                else if (!string.IsNullOrEmpty(CardCode))
                {
                    baltable = GetClientSboBalPercent(CardCode, sborow["id"].ToString());
                    if (baltable.Rows.Count > 0)
                    {
                        decimal tempBalance = 0, tempINVtotal = 0, tempRINtotal = 0;
                        if (!string.IsNullOrEmpty(baltable.Rows[0]["Balance"].ToString()) && decimal.TryParse(baltable.Rows[0]["Balance"].ToString(), out tempBalance))
                        {
                            BalDue += tempBalance;
                        }
                        if (!string.IsNullOrEmpty(baltable.Rows[0]["INVtotal"].ToString()) && decimal.TryParse(baltable.Rows[0]["INVtotal"].ToString(), out tempINVtotal))
                        {
                            Total += tempINVtotal;
                        }
                        if (!string.IsNullOrEmpty(baltable.Rows[0]["RINtotal"].ToString()) && decimal.TryParse(baltable.Rows[0]["RINtotal"].ToString(), out tempRINtotal))
                        {
                            Total -= tempRINtotal;
                        }

                        sborow["BalSboAmount"] = tempBalance.ToString("#0.00");
                    }
                }
            }
            //当前账套金额
            decimal due90 = 0; decimal total90 = 0;
            DataTable curbaltab = GetClientSboBalPercent(CardCode, SboId.ToString());
            decimal sboBalance = 0, rctBalance90 = 0, invBalance90 = 0, rinBalance90 = 0, invTotal90P = 0, invBalance90P = 0, rctBalance90P = 0, rinBalance90P = 0;
            if (!string.IsNullOrEmpty(curbaltab.Rows[0]["Balance"].ToString()) && decimal.TryParse(curbaltab.Rows[0]["Balance"].ToString(), out sboBalance))
            {
                due90 += sboBalance;
            }
            if (!string.IsNullOrEmpty(curbaltab.Rows[0]["RCTBal90"].ToString()) && decimal.TryParse(curbaltab.Rows[0]["RCTBal90"].ToString(), out rctBalance90))
            {
                due90 += rctBalance90;
            }
            if (!string.IsNullOrEmpty(curbaltab.Rows[0]["INVBal90"].ToString()) && decimal.TryParse(curbaltab.Rows[0]["INVBal90"].ToString(), out invBalance90))
            {
                due90 -= invBalance90;
            }
            if (!string.IsNullOrEmpty(curbaltab.Rows[0]["RINBal90"].ToString()) && decimal.TryParse(curbaltab.Rows[0]["RINBal90"].ToString(), out rinBalance90))
            {
                due90 += rinBalance90;
            }
            if (!string.IsNullOrEmpty(curbaltab.Rows[0]["INVTotal90P"].ToString()) && decimal.TryParse(curbaltab.Rows[0]["INVTotal90P"].ToString(), out invTotal90P))
            {
                total90 += invTotal90P;
            }
            //if (!string.IsNullOrEmpty(curbaltab.Rows[0]["INVBal90P"].ToString()) && decimal.TryParse(curbaltab.Rows[0]["INVBal90P"].ToString(), out invBalance90P))
            //{
            //    total90 += invBalance90P;
            //}
            //if (!string.IsNullOrEmpty(curbaltab.Rows[0]["RINBal90P"].ToString()) && decimal.TryParse(curbaltab.Rows[0]["RINBal90P"].ToString(), out rinBalance90P))
            //{
            //    total90 -= rinBalance90P;
            //}
            //if (!string.IsNullOrEmpty(curbaltab.Rows[0]["RCTBal90P"].ToString()) && decimal.TryParse(curbaltab.Rows[0]["RCTBal90P"].ToString(), out rctBalance90P))
            //{
            //    total90 -= rctBalance90P;
            //}
            balstr = "[{\"BalDue\":\"" + BalDue.ToString("#0.00").FilterString() + "\",\"Total\":\"" + Total.ToString("#0.00").FilterString() + "\",\"BalSboDetails\":" + sbotable.DataTableToJSON() + ",\"Due90\":\"" + due90.ToString("#0.00").FilterString() + "\",\"Total90\":\"" + total90.ToString("#0.00").FilterString() + "\"}]";
            return balstr;
        }
        /// <summary>
        /// 查询销售员所选账套所有客户科目余额与百分比数据
        /// </summary>
        /// <param name="slpCode"></param>
        /// <param name="type"></param>
        /// <param name="SboId"></param>
        /// <returns></returns>
        public DataTable GetSalesSboBalPercent(string slpCode, string type, string SboId)
        {
            bool sapflag = GetSapSboIsOpen(SboId);
            if (sapflag)
            {
                string strSql = string.Format(@"select ttotal.Total,isnull((select sum(balance) from ocrd where SlpCode={0}),0) as BalDue from (
                                   select sum(isnull(ocrdbal.INVtotal,0)-isnull(ocrdbal.RINtotal,0)) as Total from ( select
                                  (select sum(DocTotal) from OINV WHERE CANCELED = 'N' and CardCode=C.CardCode) as INVtotal
                                  ,(select SUM(DocTOTal) from ORIN where CANCELED='N' and CardCode=c.CardCode) as RINtotal
                                  FROM OCRD C WHERE C.SlpCode={0} ) as ocrdbal) as ttotal ", slpCode, type);
                return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql, CommandType.Text, null);
            }
            else
            {
                string strSql = string.Format(@"select ocrdtotal.Total,IFNULL((select sum(balance) from {0}.crm_ocrd_oldsbo_balance where sbo_id={2} and slpname=(select SlpName from {0}.crm_oslp where SlpCode='{3}' LIMIT 1)),0) as BalDue
                                                from (select sum(ifnull(ocrdbal.INVtotal,0)-ifnull(ocrdbal.RINtotal,0)) as Total from (
                                                SELECT (select sum(DocTotal) from {0}.sale_oinv WHERE CANCELED = 'N' AND sbo_id=C.sbo_id and CardCode=C.CardCode) as INVtotal
                                                ,(select SUM(DocTOTal) from {0}.sale_orin where CANCELED = 'N' and sbo_id=C.sbo_id and CardCode=C.CardCode) AS RINtotal 
                                                FROM {0}.crm_ocrd C
                                                WHERE C.sbo_id={2} and C.SlpCode ='{3}') as ocrdbal) as ocrdtotal", "nsap_bone", type, SboId, slpCode);

                return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            }
        }
        /// <summary>
        /// 查询指定业务伙伴的科目余额与百分比数据
        /// </summary>
        /// <param name="CardCode"></param>
        /// <param name="SboId"></param>
        /// <returns></returns>
        public DataTable GetClientSboBalPercent(string CardCode, string SboId)
        {
            bool sapflag = GetSapSboIsOpen(SboId);
            if (sapflag)
            {
                string strSql = string.Format(@"SELECT (Select sum(Balance) from OCRD where CardCode='{0}') as Balance
                                  ,(select sum(DocTotal) from OINV WHERE CANCELED ='N' and CardCode='{0}') as INVtotal
                                  ,(select SUM(DocTOTal) from ORIN where CANCELED<>'Y' and CardCode='{0}') as RINtotal
                                --90天内未清收款
                                ,(select SUM(openBal) from ORCT WHERE CANCELED='N' AND openBal<>0 AND CardCode='{0}' and datediff(DAY,docdate,getdate())<=90) as RCTBal90
                                --90天内未清发票金额
                                ,(select SUM(DocTotal-PaidToDate) from OINV WHERE CANCELED ='N' and CardCode='{0}' and DocTotal-PaidToDate>0 and datediff(DAY,docdate,getdate())<=90) as INVBal90
                                --90天内未清贷项金额
                                ,(select SUM(DocTotal-PaidToDate) from ORIN where CANCELED ='N' and CardCode='{0}' and DocTotal-PaidToDate>0 and datediff(DAY,docdate,getdate())<=90) as RINBal90
                                --90天前未清发票的发票总额
                                ,(select SUM(DocTotal) from OINV WHERE CANCELED ='N' and CardCode = '{0}' and DocTotal-PaidToDate > 0 and datediff(DAY, docdate, getdate())> 90) as INVTotal90P
                ", CardCode);
                //--90天前未清收款
                //,(select SUM(openBal) from ORCT WHERE CANCELED = 'N' AND openBal<>0 AND CardCode = @cardcode and datediff(DAY, docdate, getdate())> 90) as RCTBal90P
                //--90天前未清发票金额
                //,(select SUM(DocTotal - PaidToDate) from OINV WHERE CANCELED ='N' and CardCode = @cardcode and DocTotal-PaidToDate > 0 and datediff(DAY, docdate, getdate())> 90) as INVBal90P
                //--90天前未清贷项金额
                //,(select SUM(DocTotal - PaidToDate) from ORIN where CANCELED ='N' and CardCode = @cardcode and DocTotal-PaidToDate > 0 and datediff(DAY, docdate, getdate())> 90) as RINBal90P


                return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql, CommandType.Text, null);
            }
            else
            {
                string strSql = string.Format(@"SELECT(Select sum(Balance) from {0}.crm_ocrd_oldsbo_balance where sbo_id={1} and CardCode = '{2}') as Balance
                                               , (select sum(DocTotal) from {0}.sale_oinv WHERE CANCELED ='N' and sbo_id={1} and CardCode = '{2}') as INVtotal
                                               ,(select SUM(DocTOTal) from {0}.sale_orin where CANCELED ='N' and sbo_id={1} and CardCode = '{2}') as RINtotal
                                            ,'' as RCTBal90
                                            ,'' as INVBal90
                                            ,'' as RINBal90
                                            ,'' as INVTotal90P
                                            ", "nsap_bone", SboId, CardCode);



                return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            }
        }
        /// <summary>
        /// 获取地址标识(开票到、运达到)
        /// </summary>
        /// <returns></returns>
        public string GetAddress(string AdresType, string CardCode, int SboID)
        {
            DataTable dt = GetSboNamePwd(SboID);
            string dRowData = string.Empty; string isOpen = "0";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); }
            if (isOpen == "0")
            {
                return GetAddressNos(AdresType, CardCode, SboID).DataTableToJSON();
            }
            else
            {
                return GetAddressSql(AdresType, CardCode, SboID).DataTableToJSON();
            }

        }
        public DataTable GetAddressNos(string AdresType, string CardCode, int SboId)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT Address AS name,CONCAT(IFNULL(ZipCode,''),IFNULL(b.Name,''),IFNULL(c.Name,''),IFNULL(City,''),IFNULL(Building,'')) AS id,a.ZipCode,a.State ");
            sql.AppendFormat(" FROM {0}.crm_crd1 a", "nsap_bone");
            sql.AppendFormat(" LEFT JOIN {0}.store_ocry b ON a.Country=b.Code", "nsap_bone");
            sql.AppendFormat(" LEFT JOIN {0}.store_ocst c ON a.State=c.Code", "nsap_bone");
            sql.AppendFormat(" WHERE AdresType='{0}' AND CardCode='{1}'", AdresType, CardCode);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql.ToString(), CommandType.Text, null);
        }
        public DataTable GetAddressSql(string AdresType, string CardCode, int SboId)
        {
            DataTable dt = GetSboNamePwd(SboId);
            string sqlconn = "";
            if (dt.Rows.Count > 0)
            {
                sqlconn = dt.Rows[0][5].ToString();
            }
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT Address AS name,(ISNULL(ZipCode,'') + ISNULL(b.Name,'')+ISNULL(c.Name,'')+ISNULL(City,'')+ISNULL(CONVERT(VARCHAR(1000),Building),'')) AS id,a.ZipCode,a.State ");
            sql.Append(" FROM CRD1 a ");
            sql.Append(" LEFT JOIN OCRY b ON a.Country=b.Code");
            sql.Append(" LEFT JOIN OCST c ON a.State=c.Code");
            sql.AppendFormat(" WHERE AdresType='{0}' AND CardCode='{1}'", AdresType, CardCode);

            return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, sql.ToString(), CommandType.Text, null);
        }
        #region 查询物料的过往采购记录
        /// <summary>
        /// 查询物料的过往采购记录
        /// </summary>
        public DataTable GetMaterialsPurHistory(int pageSize, int pageIndex, string filterQuery, string sortname, string sortorder, string ItemCode)
        {
            int rowCount = 0;
            string sortString = string.Empty;
            string filterString = string.Format("t1.canceled='N' AND t2.ItemCode='{0}'", ItemCode.FilterSQL().Trim());
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());

            string tbName2 = "buy_POR1";
            string tbName1 = "buy_OPOR";
            #region 搜索条件
            if (!string.IsNullOrEmpty(filterQuery))
            {
                string[] fields = filterQuery.Split('`');
                string[] p = fields[0].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    switch (p[1].ToString())
                    {
                        case "1":
                            tbName2 = "buy_POR1";
                            tbName1 = "buy_OPOR";
                            break;
                        case "2":
                            tbName2 = "buy_PCH1";
                            tbName1 = "buy_OPCH";
                            break;
                        case "3":
                            tbName2 = "buy_RPC1";
                            tbName1 = "buy_ORPC";
                            break;
                    }
                }
            }

            #endregion
            DataTable dt = GetPurTradingHistory(out rowCount, pageSize, pageIndex, filterString, sortString, tbName1, tbName2);
            dt.Columns.Add("pdn_no", typeof(string));
            dt.Columns.Add("pdn_quantity", typeof(string));
            foreach (DataRow dr in dt.Rows)
            {
                if (tbName2 == "buy_POR1")
                {
                    string pdn_data = CheckPDNData(dr[1].ToString(), ItemCode.Replace("&#39;", "'").Replace("&#34;", "\""), dr[12].ToString());
                    dr["pdn_no"] = pdn_data.Split(';')[0].Split(':')[1];
                    dr["pdn_quantity"] = pdn_data.Split(';')[1].Split(':')[1];
                    DataTable dt_state = SelectOPORState(dr[1].ToString());
                    if (dt_state.Rows.Count > 0)
                    {
                        dr["DocStatus"] = dt_state.Rows[0][0];
                        dr["Printed"] = dt_state.Rows[0][1];
                        dr["CANCELED"] = dt_state.Rows[0][2];
                    }
                }
                else
                {
                    dr["pdn_no"] = "";
                    dr["pdn_quantity"] = "";
                }
            }
            return dt;
        }
        public DataTable GetPurTradingHistory(out int rowCounts, int pageSize, int pageIndex, string filterQuery, string orderName, string tbName1, string tbName2)
        {
            string fieldname = @"t4.sbo_nm as sbo_nm,t1.DocEntry as DocEntry,t1.ObjType as ObjType,t3.SlpName as SlpName,t1.DocDate as DocDate,t1.DocDueDate as DocDueDate
                                 ,t2.Dscription as Dscription,t2.Quantity as Quantity,t2.Price as Price,t2.LineTotal as LineTotal,t1.CardName as CardName,t1.Comments as Comments,t1.sbo_id,t1.DocStatus,t1.Printed,t1.CANCELED";
            string tableName = string.Format(@"{0}.{1} t1 LEFT JOIN {0}.{2} t2 ON t1.DocEntry=t2.DocEntry AND t1.sbo_id=t2.sbo_id 
                                            LEFT JOIN {0}.crm_OSLP t3 on t1.SlpCode = t3.SlpCode AND t1.sbo_id = t3.sbo_id LEFT JOIN {3}.sbo_info t4 ON t1.sbo_id = t4.sbo_id", "nsap_bone", tbName1, tbName2, "nsap_base");
            return SelectPagingHaveRowsCount(tableName.ToString(), fieldname.ToString(), pageSize, pageIndex, orderName, filterQuery, out rowCounts);
        }
        public string CheckPDNData(string pur_no, string itemCode, string sbo_id)
        {
            string sql = string.Format("select DocEntry,Quantity from {0}.buy_pdn1 where BaseEntry={1} and ItemCode={2} and sbo_id={3} ", "nsap_bone", pur_no, itemCode, sbo_id);

            DataTable dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
            string pdn_no = string.Empty;
            float pdn_quantity = 0f;
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    pdn_no += dr[0].ToString() + ",";
                    pdn_quantity += float.Parse(dr[1].ToString());
                }
            }

            return string.Format("pdn_no:{0};pdn_quantity:{1}", pdn_no.TrimEnd(','), pdn_quantity);
        }
        public DataTable SelectOPORState(string docentry)
        {
            string sql = string.Format("select DocStatus,Printed,CANCELED from OPOR where DocEntry='{0}'", docentry);
            return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, sql, CommandType.Text, null);
        }
        #endregion
        #region 查询物料库存数据来源
        /// <summary>
        /// 查询物料库存数据来源
        /// </summary>
        /// <returns></returns>
        public DataTable SelectMaterialStockDataSource(int pageSize, int pageIndex, string filterQuery, string sortname, string sortorder, string SboId, string WhsCode, string ItemCode, string ItemOperaType, bool IsOpenSap)
        {
            string sortString = string.Empty;
            string filterString = "";
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());
            return SelectMaterialStockDataSource(pageSize, pageIndex, filterString.ToString(), sortString, SboId, WhsCode, ItemCode.FilterESC().Replace("'", "''"), ItemOperaType, IsOpenSap);
        }
        #endregion
        #region 查询物料库存数据来源
        /// <summary>
        /// 查询物料库存数据来源
        /// </summary>
        /// <returns></returns>
        public DataTable SelectMaterialStockDataSource(int pageSize, int pageIndex, string filterQuery, string orderName, string SboId, string WhsCode, string ItemCode, string ItemOperaType, bool IsOpenSap)
        {
            StringBuilder filedName = new StringBuilder();
            StringBuilder tableName = new StringBuilder();
            bool isQueryWhsCode = string.IsNullOrEmpty(WhsCode) ? false : true;
            if (IsOpenSap)
            {
                if (ItemOperaType == "IsCommited")
                {
                    #region SQLSERVER 承诺来源
                    tableName.Append("((SELECT o.DocEntry,'销售订单' as DocType,s.SlpName as Department,'' as ItemCode,r.Quantity,r.DocDate,");
                    tableName.Append("o.DocDueDate as DueDate,r.Quantity as PlannedQty,(r.Quantity-r.OpenQty) as IssuedQty, r.OpenQty,o.Comments,g.PymntGroup as PayCon,'' as ORDRFlag1,o.CreateDate  ");
                    tableName.Append("FROM RDR1 r INNER JOIN ORDR o ON r.DocEntry=o.DocEntry AND r.LineStatus='O' LEFT JOIN OSLP s ON o.SlpCode=s.SlpCode ");
                    tableName.Append(" LEFT JOIN OCTG g ON g.GroupNum=o.GroupNum ");
                    tableName.AppendFormat("WHERE r.ItemCode='{0}' ", ItemCode);
                    if (isQueryWhsCode)
                    {
                        tableName.AppendFormat(" AND r.WhsCode = '{0}'", WhsCode);
                    }

                    tableName.Append(") UNION ALL (SELECT o.DocEntry,'生产订单' as DocType,o.U_WO_LTDW as Department,o.ItemCode, o.PlannedQty as Quantity,o.PostDate as DocDate,");
                    tableName.Append("o.DueDate, w.PlannedQty,w.IssuedQty,(w.PlannedQty-w.IssuedQty) as OpenQty,o.Comments,'' as PayCon,'' as ORDRFlag1,o.CreateDate  ");
                    tableName.AppendFormat("FROM WOR1 w INNER JOIN OWOR o ON w.DocEntry=o.DocEntry WHERE w.ItemCode='{0}' ", ItemCode);
                    tableName.Append("AND o.Type='S' AND (o.Status='R' OR o.Status='P') AND (w.PlannedQty-w.IssuedQty)>0 ");
                    if (isQueryWhsCode)
                    {
                        tableName.AppendFormat(" AND w.Warehouse = '{0}'", WhsCode);
                    }

                    tableName.Append(") UNION ALL(SELECT DocEntry,'拆机单' as DocType,'' as Department,ItemCode,PlannedQty as Quantity,PostDate as DocDate,");
                    tableName.Append("DueDate, PlannedQty,CmpltQty as IssuedQty,(PlannedQty-CmpltQty) as OpenQty,Comments,'' as PayCon,'' as ORDRFlag1,CreateDate  FROM OWOR ");
                    tableName.AppendFormat("WHERE ItemCode='{0}' AND Type='D' AND (Status='R' or Status='P') AND (PlannedQty-CmpltQty)>0 ", ItemCode);
                    if (isQueryWhsCode)
                    {
                        tableName.AppendFormat(" AND Warehouse = '{0}'", WhsCode);
                    }
                    tableName.Append(")) T");
                    #endregion
                }
                else
                {
                    #region SQLSERVER 订购来源
                    tableName.Append("((SELECT o.DocEntry,'采购订单' as DocType,s.SlpName as Department,'' as ItemCode,r.Quantity,r.DocDate,");
                    tableName.Append("o.DocDueDate as DueDate,r.Quantity as PlannedQty,(r.Quantity-r.OpenQty) as IssuedQty, r.OpenQty,o.Comments,'' as PayCon,'' as ORDRFlag1,o.CreateDate  ");
                    tableName.Append("FROM POR1 r INNER JOIN OPOR o ON r.DocEntry=o.DocEntry AND r.LineStatus='O' LEFT JOIN OSLP s ON o.SlpCode=s.SlpCode ");
                    tableName.AppendFormat("WHERE r.ItemCode='{0}' ", ItemCode);
                    if (isQueryWhsCode)
                    {
                        tableName.AppendFormat(" AND r.WhsCode = '{0}'", WhsCode);
                    }

                    tableName.Append(") UNION ALL (SELECT DocEntry,'生产订单' as DocType,U_WO_LTDW as Department,ItemCode, PlannedQty as Quantity,PostDate as DocDate,");
                    tableName.Append("DueDate,PlannedQty,CmpltQty as IssuedQty,(PlannedQty-CmpltQty) as OpenQty,Comments,'' as PayCon,'' as ORDRFlag1,CreateDate  ");
                    tableName.AppendFormat("FROM OWOR WHERE ItemCode='{0}' AND Type='S' AND (Status='R' OR Status='P') AND (PlannedQty-CmpltQty)>0 ", ItemCode);
                    if (isQueryWhsCode)
                    {
                        tableName.AppendFormat(" AND Warehouse = '{0}'", WhsCode);
                    }

                    tableName.Append(") UNION ALL(SELECT o.DocEntry,'拆机单' as DocType,'' as Department,o.ItemCode,o.PlannedQty as Quantity,o.PostDate as DocDate,");
                    tableName.Append("o.DueDate, w.PlannedQty,w.IssuedQty, (w.PlannedQty-w.IssuedQty) as OpenQty, o.Comments,'' as PayCon,'' as ORDRFlag1,o.CreateDate  ");
                    tableName.AppendFormat("FROM WOR1 w INNER JOIN OWOR o ON w.DocEntry=o.DocEntry WHERE o.ItemCode='{0}' ", ItemCode);
                    tableName.Append("AND o.Type='D' and (o.Status='R' OR o.Status='P') AND w.IssuedQty>0 ");
                    if (isQueryWhsCode)
                    {
                        tableName.AppendFormat(" AND w.Warehouse = '{0}'", WhsCode);
                    }
                    tableName.Append(")) T");
                    #endregion
                }

                filedName.Append("DocEntry,DocType,Department,ItemCode,Quantity,DocDate,DueDate,PlannedQty,IssuedQty,OpenQty,Comments,PayCon,'' as ORDRFlag1,CreateDate");
                //return Sql.SAPSelectPagingNoneRowsCount(sapConn, tableName.ToString(), filedName.ToString(), pageSize, pageIndex, orderName, filterQuery);
                string strSql = string.Format("SELECT {0} FROM {1} ", filedName, tableName);
                if (!string.IsNullOrEmpty(filterQuery))
                {
                    strSql += string.Format(" WHERE {0} ", filterQuery);
                }
                if (!string.IsNullOrEmpty(orderName))
                {
                    strSql += string.Format(" ORDER BY {0} ", orderName);
                }
                var tb = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
                //tb.Columns.Add("ORDRFlag1", typeof(string));
                foreach (DataRow therow in tb.Rows)
                {
                    string thedoctype = therow["DocType"].ToString();

                    #region 加上销售订单的付款生产单情况
                    if (thedoctype == "销售订单" && (therow["PayCon"].ToString().IndexOf("预付") > -1 || therow["PayCon"].ToString().IndexOf("款到发货") > -1))
                    {
                        bool flagPro = ExistsProductOrderByordr(therow["DocEntry"].ToString());
                        if (flagPro)
                        {
                            therow["ORDRFlag1"] = "1";
                        }
                        else
                        {
                            bool flagRCT = ExistsPaymentByordr(therow["DocEntry"].ToString(), SboId);
                            therow["ORDRFlag1"] = flagRCT ? "1" : "0";
                        }
                    }
                    #endregion
                }
                return tb;
            }
            else
            {
                if (ItemOperaType == "IsCommited")
                {
                    #region MYSQL 承诺来源
                    tableName.Append("((SELECT o.sbo_id,o.DocEntry,'销售订单' as DocType,s.SlpName as Department,'' as ItemCode,r.Quantity,r.DocDate,");
                    tableName.Append("o.DocDueDate as DueDate,r.Quantity as PlannedQty,(r.Quantity-r.OpenQty) as IssuedQty, r.OpenQty,o.Comments,g.PymntGroup as PayCon,'' as ORDRFlag1,o.CreateDate ");
                    tableName.AppendFormat("FROM {0}.sale_RDR1 r INNER JOIN {0}.sale_ORDR o ON r.DocEntry=o.DocEntry AND r.LineStatus='O' ", "nsap_bone");
                    tableName.AppendFormat("LEFT JOIN {0}.crm_OSLP s ON o.SlpCode=s.SlpCode AND o.sbo_id=s.sbo_id ", "nsap_bone");
                    tableName.AppendFormat("WHERE o.sbo_id={0} AND r.ItemCode='{1}' ", SboId, ItemCode);
                    if (isQueryWhsCode)
                    {
                        tableName.AppendFormat(" AND r.WhsCode = '{0}'", WhsCode);
                    }

                    tableName.Append(") UNION ALL (SELECT o.sbo_id,o.DocEntry,'生产订单' as DocType,o.U_WO_LTDW as Department,o.ItemCode, o.PlannedQty as Quantity,o.PostDate as DocDate,");
                    tableName.Append("o.DueDate, w.PlannedQty,w.IssuedQty,(w.PlannedQty-w.IssuedQty) as OpenQty,o.Comments ");
                    tableName.AppendFormat("FROM {0}.product_WOR1 w INNER JOIN {0}.product_OWOR o ON w.DocEntry=o.DocEntry WHERE o.sbo_id={1} ", "nsap_bone", SboId);
                    tableName.AppendFormat("AND w.ItemCode='{0}' AND o.Type='S' AND (o.Status='R' OR o.Status='P')  AND (w.PlannedQty-w.IssuedQty)>0 ", ItemCode);
                    if (isQueryWhsCode)
                    {
                        tableName.AppendFormat(" AND w.Warehouse = '{0}'", WhsCode);
                    }

                    tableName.Append(") UNION ALL(SELECT sbo_id,DocEntry,'拆机单' as DocType,'' as Department,ItemCode,PlannedQty as Quantity,PostDate as DocDate,");
                    tableName.AppendFormat("DueDate, PlannedQty,CmpltQty as IssuedQty,(PlannedQty-CmpltQty) as OpenQty,Comments FROM {0}.product_OWOR ", "nsap_bone");
                    tableName.AppendFormat("WHERE sbo_id={0} AND ItemCode='{1}' AND Type='D' AND (Status='R' or Status='P') AND (PlannedQty-CmpltQty)>0 ", SboId, ItemCode);
                    if (isQueryWhsCode)
                    {
                        tableName.AppendFormat(" AND Warehouse = '{0}'", WhsCode);
                    }
                    tableName.Append(")) T");
                    #endregion
                }
                else
                {
                    #region MYSQL 订购来源
                    tableName.Append("((SELECT o.sbo_id,o.DocEntry,'采购订单' as DocType,s.SlpName as Department,'' as ItemCode,r.Quantity,r.DocDate,");
                    tableName.Append("o.DocDueDate as DueDate,r.Quantity as PlannedQty,(r.Quantity-r.OpenQty) as IssuedQty, r.OpenQty,o.Comments ");
                    tableName.AppendFormat("FROM {0}.buy_POR1 r INNER JOIN {0}.buy_OPOR o ON r.DocEntry=o.DocEntry AND r.LineStatus='O' ", "nsap_bone");
                    tableName.AppendFormat("LEFT JOIN {0}.crm_OSLP s ON o.SlpCode=s.SlpCode AND o.sbo_id=s.sbo_id ", "nsap_bone");
                    tableName.AppendFormat("WHERE o.sbo_id={0} AND r.ItemCode='{0}' ", SboId, ItemCode);
                    if (isQueryWhsCode)
                    {
                        tableName.AppendFormat(" AND r.WhsCode = '{0}'", WhsCode);
                    }

                    tableName.Append(") UNION ALL (SELECT sbo_id,DocEntry,'生产订单' as DocType,U_WO_LTDW as Department,ItemCode, PlannedQty as Quantity,PostDate as DocDate,");
                    tableName.Append("DueDate,PlannedQty,CmpltQty as IssuedQty,(PlannedQty-CmpltQty) as OpenQty,Comments ");
                    tableName.AppendFormat("FROM {0}.product_OWOR WHERE sbo_id={1} AND ItemCode='{2}' AND Type='S' ", "nsap_bone", SboId, ItemCode);
                    tableName.Append("AND (Status='R' OR Status='P')  AND (PlannedQty-CmpltQty)>0 ");
                    if (isQueryWhsCode)
                    {
                        tableName.AppendFormat(" AND Warehouse = '{0}'", WhsCode);
                    }

                    tableName.Append(") UNION ALL(SELECT o.sbo_id,o.DocEntry,'拆机单' as DocType,'' as Department,o.ItemCode,o.PlannedQty as Quantity,o.PostDate as DocDate,");
                    tableName.Append("o.DueDate, w.PlannedQty,w.IssuedQty, (w.PlannedQty-w.IssuedQty) as OpenQty, o.Comments ");
                    tableName.AppendFormat("FROM {0}.product_WOR1 w INNER JOIN {0}.product_OWOR o ON w.DocEntry=o.DocEntry WHERE o.sbo_id={1} ", "nsap_bone", SboId);
                    tableName.AppendFormat("AND o.ItemCode='{0}' AND Type='D' and (o.Status='R' OR o.Status='P') AND w.IssuedQty>0 ", ItemCode);
                    if (isQueryWhsCode)
                    {
                        tableName.AppendFormat(" AND w.Warehouse = '{0}'", WhsCode);
                    }
                    tableName.Append(")) T");
                    #endregion
                }

                filedName.Append("DocEntry,DocType,Department,ItemCode,Quantity,DocDate,DueDate,PlannedQty,IssuedQty,OpenQty,Comments");
                //return Sql.SelectPagingNoneRowsCount(tableName.ToString(), filedName.ToString(), pageSize, pageIndex, orderName, filterQuery);
                string strSql = string.Format("SELECT {0} FROM {1} ", filedName, tableName);
                if (!string.IsNullOrEmpty(filterQuery))
                {
                    strSql += string.Format(" WHERE {0} ", filterQuery);
                }
                if (!string.IsNullOrEmpty(orderName))
                {
                    strSql += string.Format(" ORDER BY {0} ", orderName);
                }
                return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
            }
        }
        /// <summary>
        /// 判断销售订单是否已下生产订单
        /// </summary>
        /// <param name="ordrEntry">销售订单号</param>
        /// <returns></returns>
        public bool ExistsProductOrderByordr(string ordrEntry)
        {
            bool resultf = false;
            //string sqlstr = "select 1 from owor where status!='C'AND OriginAbs=" + ordrEntry;
            //IDataReader tempr = UnitWork.ExecuteReader(ContextType.SapDbContextType, sqlstr,CommandType.Text,null );
            //if (tempr.Read()) {
            //	resultf = true;
            //}
            //tempr.Close();
            //tempr.Dispose();
            return resultf;
        }
        /// <summary>
        /// 判断销售订单是否已收款
        /// </summary>
        /// <param name="ordrEntry">销售订单号</param>
        /// <param name="sboId">账套</param>
        /// <returns></returns>
        public bool ExistsPaymentByordr(string ordrEntry, string sboId)
        {
            string sqlstr = string.Format(@"select U_DocRCTAmount FROM {0}.sale_ordr where docentry={1} and sbo_id={2}", "nsap_bone", ordrEntry, sboId);
            object theamount = UnitWork.ExecuteScalar(ContextType.SapDbContextType, sqlstr, CommandType.Text, null);
            return theamount == null ? false : (((decimal)theamount) > 0 ? true : false);
        }
        #endregion
        #region 我创建的
        /// <summary>
        /// 我创建的
        /// </summary>
        public DataTable GetICreated(out int rowCount, GetICreatedReq model, int user_id, bool ViewCustom = true, bool ViewSales = true)
        {

            string sortString = string.Empty;
            string filterString = string.Empty;
            string line = string.Empty;
            if (!string.IsNullOrEmpty(model.sortname) && !string.IsNullOrEmpty(model.sortorder))
                sortString = string.Format("{0} {1}", model.sortname, model.sortorder.ToUpper());
            if (user_id > 0)
            {
                filterString += string.Format("a.user_id = {0} AND a.job_state >-1 AND ", user_id);
            }
            #region 搜索条件
            if (model.types.Replace(" ", "") != "")
            {
                string[] typeArr = model.types.Split('☉');
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

            if (model.Applicator != "")
            {
                string[] num;
                num = model.Applicator.Split(',');
                string para = "";
                foreach (string c in num)
                {
                    para += "'" + c + "'" + ",";
                }
                para = "(" + para.TrimEnd(',') + ")";
                filterString += string.Format(" c.user_nm IN {0} AND ", para);
            }
            if (model.Customer != "")
            {
                filterString += string.Format(" (a.card_code LIKE '%{0}%' OR a.card_name LIKE '%{0}%') AND ", model.Customer);
            }
            if (model.Job_state != "")
            {
                filterString += string.Format(" a.job_state = {0} AND ", int.Parse(model.Job_state));
            }
            if (model.BeginDate != "")
            {
                filterString += string.Format(" DATE_FORMAT(a.upd_dt,'%Y/%m/%d') BETWEEN '{0}' AND '{1}' AND ", model.BeginDate, model.EndDate);
            }
            if (!string.IsNullOrEmpty(model.Job_Id))
            {
                filterString += string.Format(" a.job_id LIKE '%{0}%' AND ", model.Job_Id);
            }

            if (!string.IsNullOrEmpty(model.Job_Type_nm))
            {
                filterString += string.Format("b.job_type_nm LIKE '%{0}%' AND ", model.Job_Type_nm);
            }


            if (!string.IsNullOrEmpty(model.Job_state))
            {
                filterString += string.Format("a.job_state = {0} AND ", model.Job_state);
            }

            if (!string.IsNullOrEmpty(model.Job_nm))
            {
                filterString += string.Format("a.job_nm LIKE '%{0}%' AND ", model.Job_nm);
            }

            if (!string.IsNullOrEmpty(model.Remarks))
            {
                filterString += string.Format("a.remarks LIKE '%{0}%' AND ", model.Remarks);
            }

            if (!string.IsNullOrEmpty(model.Sbo_itf_return))
            {
                filterString += string.Format("a.sbo_itf_return LIKE '%{0}%' AND ", model.Sbo_itf_return);
            }
            if (!string.IsNullOrEmpty(model.Base_entry))
            {
                filterString += string.Format("a.base_entry LIKE '%{0}%' AND ", model.Base_entry);
            }

            //filterString += string.Format("(b.job_type_nm LIKE '%{0}%' OR b.job_type_nm LIKE '%{1}%') AND ", "销售报价单","销售订单");
            filterString += string.Format("(b.job_type_nm = '{0}' OR b.job_type_nm = '{1}'  OR b.job_type_nm = '{2}' OR b.job_type_nm = '{3}'OR b.job_type_nm = '{4}' OR b.job_type_nm = '{5}'OR b.job_type_nm = '{6}') AND ", "销售报价单", "销售订单", "销售交货", "业务伙伴审核", "应收发票", "销售交货修改工作流", "取消销售订单");
            #endregion
            #region
            if (!string.IsNullOrEmpty(filterString))
                filterString = filterString.Substring(0, filterString.Length - 5);
            #endregion
            return GetICreated(out rowCount, model.limit, model.page, filterString, sortString, ViewCustom, ViewSales);
        }
        #endregion
        #region 我创建的
        /// <summary>
        /// 我创建的
        /// </summary>
        public DataTable GetICreated(out int rowCounts, int pageSize, int pageIndex, string filterQuery, string orderName, bool ViewCustom, bool ViewSales)
        {
            StringBuilder tableName = new StringBuilder();
            StringBuilder filedName = new StringBuilder();
            filedName.Append(" '',a.job_id,b.job_type_nm,a.job_nm,c.user_nm,a.job_state,a.upd_dt,a.remarks,b.job_type_id,a.card_code,");
            filedName.Append("CASE WHEN a.card_name IS NULL THEN NULL WHEN a.card_name = '' THEN '' WHEN a.card_name IS NOT NULL AND a.card_name <> '' AND " + ViewCustom + " THEN a.card_name ELSE '******' END AS CardName,");
            filedName.Append("CASE WHEN a.doc_total IS NULL THEN NULL WHEN a.doc_total = '' THEN '' WHEN a.doc_total IS NOT NULL AND a.doc_total <> '' AND " + ViewSales + " THEN a.doc_total ELSE '******' END AS DocTotal,");
            filedName.Append("a.base_type,a.base_entry,d.step_nm,a.sbo_id,f.page_url,a.sbo_itf_return,g.sbo_nm,a.sync_start,a.sync_stat,b.sync_sap ");
            tableName.AppendFormat("{0}.wfa_job a", "nsap_base");
            tableName.AppendFormat(" LEFT JOIN {0}.wfa_type b ON a.job_type_id=b.job_type_id ", "nsap_base");
            tableName.AppendFormat(" LEFT JOIN {0}.base_user c ON a.user_id=c.user_id ", "nsap_base");
            tableName.AppendFormat(" LEFT JOIN {0}.wfa_step d ON a.step_id=d.step_id ", "nsap_base");
            tableName.AppendFormat(" LEFT JOIN {0}.base_func e ON b.job_type_id=e.job_type_id ", "nsap_base");
            tableName.AppendFormat(" LEFT JOIN {0}.base_page f ON e.page_id =f.page_id ", "nsap_base");
            tableName.AppendFormat(" LEFT JOIN {0}.sbo_info g ON a.sbo_id =g.sbo_id ", "nsap_base");
            filterQuery += " AND a.job_state != -1 GROUP BY a.job_id";
            return SelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), pageSize, pageIndex, orderName, filterQuery, out rowCounts);
        }
        #endregion
        #region 销售信息
        public billDelivery GetDeliverySalesInfoNew(string jobId, string isAudit, string docType)
        {
            billDelivery bill = DeSerialize<billDelivery>((byte[])(GetSalesInfo(jobId)));
            DataTable dt = GetSboNamePwd(int.Parse(bill.SboId));
            string dRowData = string.Empty; string isOpen = "0"; string sboname = "0"; string sqlconn = "0";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); sboname = dt.Rows[0][0].ToString(); sqlconn = dt.Rows[0][5].ToString(); }
            if (docType != "oqut" && !string.IsNullOrEmpty(bill.billBaseEntry))
            {
                DataTable _callInfo = GetCallInfoById(bill.billBaseEntry, bill.SboId, docType, "1", "call");
                if (_callInfo.Rows.Count > 0)
                {
                    bill.U_CallID = _callInfo.Rows[0][0].ToString();
                    bill.U_CallName = _callInfo.Rows[0][1].ToString();
                    bill.U_SerialNumber = _callInfo.Rows[0][2].ToString();
                }
            }
            string type = bill.DocType;
            string _main = JsonHelper.ParseModel(bill);
            DateTime docDate;
            DateTime.TryParse(bill.DocDate, out docDate);
            bill.DocDate = docDate.ToString("yyyy/MM/dd");
            DateTime docDueDate;
            DateTime.TryParse(bill.DocDueDate, out docDueDate);
            bill.DocDueDate = docDueDate.ToString("yyyy/MM/dd");
            DateTime prepaData;
            DateTime.TryParse(bill.PrepaData, out prepaData);
            bill.PrepaData = prepaData.ToString("yyyy/MM/dd");
            DateTime goodsToDate;
            DateTime.TryParse(bill.GoodsToDate, out goodsToDate);
            bill.GoodsToDate = goodsToDate.ToString("yyyy/MM/dd");
            if (bill.attachmentData != null)
            {
                foreach (var files in bill.attachmentData)
                {
                    DateTime filetime;
                    DateTime.TryParse(files.filetime, out filetime);
                    files.filetime = filetime.ToString("yyyy/MM/dd hh:mm:ss");
                }
            }
            //if (bill.CustomFields.ToString().Contains("≯"))
            //{
            //	var ba = bill.CustomFields.Split("≯")[3];
            //	bill.CustomFields = ba;

            //}


            return bill;
        }
        public billDelivery GetDeliverySalesInfoNewNos(string jobId, int typeId)
        {
            billDelivery bill = null;
            object obj = GetSalesInfoNos(jobId, typeId);
            if (obj != null)
            {
                bill = DeSerialize<billDelivery>((byte[])(obj));
            }
            if (bill != null)
            {
                DataTable dt = GetSboNamePwd(int.Parse(bill.SboId));
                string dRowData = string.Empty; string isOpen = "0"; string sboname = "0"; string sqlconn = "0";
                if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); sboname = dt.Rows[0][0].ToString(); sqlconn = dt.Rows[0][5].ToString(); }

                string type = bill.DocType;
                string _main = JsonHelper.ParseModel(bill);
                //if (bill.CustomFields.ToString().Contains("≮1≯"))
                //{
                //	var ba = bill.CustomFields.Split("≮1≯")[4];
                //	bill.CustomFields = ba;
                //}
            }
            return bill;
        }
        /// <summary>
        /// 经理
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="isAudit"></param>
        /// <param name="docType"></param>
        /// <returns></returns>
        public List<CurrencyList> DropPopupOwnerCodeNew(string jobId, string isAudit, string docType)
        {
            billDelivery bill = DeSerialize<billDelivery>((byte[])(GetSalesInfo(jobId)));
            DataTable dt = GetSboNamePwd(int.Parse(bill.SboId));
            string dRowData = string.Empty; string isOpen = "0"; string sboname = "0"; string sqlconn = "0";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); sboname = dt.Rows[0][0].ToString(); sqlconn = dt.Rows[0][5].ToString(); }

            string isEdit = "0";
            if (isAudit == "1")
            {
                isEdit = GetIsEdit(jobId).ToString();
            }
            string storehouse = "0", mark = "0", manager = "0", sales = "0", shipType = "0", paymentCond = "0", paymentMode = "0", andbuy = "0"
                , docCur = "0", cntctCode = "0";
            if (isAudit == "1" && isEdit == "0")
            {
                storehouse = bill.WhsCode;
                mark = bill.Indicator;
                manager = bill.OwnerCode;
                sales = bill.SlpCode;
                shipType = bill.TrnspCode;
                paymentCond = bill.GroupNum;
                paymentMode = bill.PeyMethod;
                andbuy = bill.U_YWY;
                docCur = bill.DocCur;
                cntctCode = bill.CntctCode;
            }
            string strSql = " SELECT empID AS id,CONCAT(lastName,+firstName) AS name FROM " + "nsap_bone" + ".crm_ohem WHERE sbo_id=" + int.Parse(bill.SboId) + "";
            if (manager != "0")
            {
                strSql += "  AND empID='" + manager + "'";
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).Tolist<CurrencyList>();
        }
        /// <summary>
        /// 销售
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="isAudit"></param>
        /// <param name="docType"></param>
        /// <returns></returns>
        public List<CurrencyList> DropPopupSlpCodeNew(string jobId, string isAudit, string docType)
        {
            billDelivery bill = DeSerialize<billDelivery>((byte[])(GetSalesInfo(jobId)));
            DataTable dt = GetSboNamePwd(int.Parse(bill.SboId));
            string dRowData = string.Empty; string isOpen = "0"; string sboname = "0"; string sqlconn = "0";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); sboname = dt.Rows[0][0].ToString(); sqlconn = dt.Rows[0][5].ToString(); }

            string isEdit = "0";
            if (isAudit == "1")
            {
                isEdit = GetIsEdit(jobId).ToString();
            }
            string storehouse = "0", mark = "0", manager = "0", sales = "0", shipType = "0", paymentCond = "0", paymentMode = "0", andbuy = "0"
                , docCur = "0", cntctCode = "0";
            if (isAudit == "1" && isEdit == "0")
            {
                storehouse = bill.WhsCode;
                mark = bill.Indicator;
                manager = bill.OwnerCode;
                sales = bill.SlpCode;
                shipType = bill.TrnspCode;
                paymentCond = bill.GroupNum;
                paymentMode = bill.PeyMethod;
                andbuy = bill.U_YWY;
                docCur = bill.DocCur;
                cntctCode = bill.CntctCode;
            }
            string strSql = " SELECT SlpCode AS id,SlpName AS name FROM " + "nsap_bone" + ".crm_oslp WHERE sbo_id=" + int.Parse(bill.SboId) + "";
            if (sales != "0")
            {
                strSql += " AND SlpCode='" + sales + "'";
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).Tolist<CurrencyList>();

        }
        /// <summary>
        /// 标识
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="isAudit"></param>
        /// <param name="docType"></param>
        /// <returns></returns>
        public List<CurrencyList> DropPopupIndicatorNew(string jobId, string isAudit, string docType)
        {
            billDelivery bill = DeSerialize<billDelivery>((byte[])(GetSalesInfo(jobId)));
            DataTable dt = GetSboNamePwd(int.Parse(bill.SboId));
            string dRowData = string.Empty; string isOpen = "0"; string sboname = "0"; string sqlconn = "0";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); sboname = dt.Rows[0][0].ToString(); sqlconn = dt.Rows[0][5].ToString(); }

            string isEdit = "0";
            if (isAudit == "1")
            {
                isEdit = GetIsEdit(jobId).ToString();
            }
            string storehouse = "0", mark = "0", manager = "0", sales = "0", shipType = "0", paymentCond = "0", paymentMode = "0", andbuy = "0"
                , docCur = "0", cntctCode = "0";
            if (isAudit == "1" && isEdit == "0")
            {
                storehouse = bill.WhsCode;
                mark = bill.Indicator;
                manager = bill.OwnerCode;
                sales = bill.SlpCode;
                shipType = bill.TrnspCode;
                paymentCond = bill.GroupNum;
                paymentMode = bill.PeyMethod;
                andbuy = bill.U_YWY;
                docCur = bill.DocCur;
                cntctCode = bill.CntctCode;
            }
            string strSql = string.Format(" SELECT Code as id,Name AS name FROM {0}.crm_oidc WHERE sbo_id={1}", "nsap_bone", int.Parse(bill.SboId));
            if (mark != "" && mark != "0")
            {
                strSql += string.Format(" AND Code='{0}'", mark);
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).Tolist<CurrencyList>();


        }
        public List<CurrencyList> DropPopupTrnspCodeNew(string jobId, string isAudit, string docType)
        {
            billDelivery bill = DeSerialize<billDelivery>((byte[])(GetSalesInfo(jobId)));
            DataTable dt = GetSboNamePwd(int.Parse(bill.SboId));
            string dRowData = string.Empty; string isOpen = "0"; string sboname = "0"; string sqlconn = "0";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); sboname = dt.Rows[0][0].ToString(); sqlconn = dt.Rows[0][5].ToString(); }

            string isEdit = "0";
            if (isAudit == "1")
            {
                isEdit = GetIsEdit(jobId).ToString();
            }
            string storehouse = "0", mark = "0", manager = "0", sales = "0", shipType = "0", paymentCond = "0", paymentMode = "0", andbuy = "0"
                , docCur = "0", cntctCode = "0";
            if (isAudit == "1" && isEdit == "0")
            {
                storehouse = bill.WhsCode;
                mark = bill.Indicator;
                manager = bill.OwnerCode;
                sales = bill.SlpCode;
                shipType = bill.TrnspCode;
                paymentCond = bill.GroupNum;
                paymentMode = bill.PeyMethod;
                andbuy = bill.U_YWY;
                docCur = bill.DocCur;
                cntctCode = bill.CntctCode;
            }
            string strSql = " SELECT TrnspCode AS id,TrnspName AS name FROM " + "nsap_bone" + ".crm_oshp WHERE sbo_id=" + int.Parse(bill.SboId) + "";
            if (shipType != "0")
            {
                strSql += " AND TrnspCode='" + shipType + "'";
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).Tolist<CurrencyList>();


        }
        public List<CurrencyList> DropPopupWhsCodeNew(string jobId, string isAudit, string docType)
        {
            billDelivery bill = DeSerialize<billDelivery>((byte[])(GetSalesInfo(jobId)));
            DataTable dt = GetSboNamePwd(int.Parse(bill.SboId));
            string dRowData = string.Empty; string isOpen = "0"; string sboname = "0"; string sqlconn = "0";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); sboname = dt.Rows[0][0].ToString(); sqlconn = dt.Rows[0][5].ToString(); }

            string isEdit = "0";
            if (isAudit == "1")
            {
                isEdit = GetIsEdit(jobId).ToString();
            }
            string storehouse = "0", mark = "0", manager = "0", sales = "0", shipType = "0", paymentCond = "0", paymentMode = "0", andbuy = "0"
                , docCur = "0", cntctCode = "0";
            if (isAudit == "1" && isEdit == "0")
            {
                storehouse = bill.WhsCode;
                mark = bill.Indicator;
                manager = bill.OwnerCode;
                sales = bill.SlpCode;
                shipType = bill.TrnspCode;
                paymentCond = bill.GroupNum;
                paymentMode = bill.PeyMethod;
                andbuy = bill.U_YWY;
                docCur = bill.DocCur;
                cntctCode = bill.CntctCode;
            }
            string strSql = string.Format(" SELECT WhsCode AS id,WhsName AS name FROM {0}.store_owhs WHERE sbo_id={1}", "nsap_bone", int.Parse(bill.SboId));
            if (storehouse != "0")
            {
                strSql += string.Format(" AND WhsCode='{0}'", storehouse);
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).Tolist<CurrencyList>();


        }
        public List<CurrencyList> GetGroupNumNew(string jobId, string isAudit, string docType)
        {
            billDelivery bill = DeSerialize<billDelivery>((byte[])(GetSalesInfo(jobId)));
            DataTable dt = GetSboNamePwd(int.Parse(bill.SboId));
            string dRowData = string.Empty; string isOpen = "0"; string sboname = "0"; string sqlconn = "0";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); sboname = dt.Rows[0][0].ToString(); sqlconn = dt.Rows[0][5].ToString(); }

            string isEdit = "0";
            if (isAudit == "1")
            {
                isEdit = GetIsEdit(jobId).ToString();
            }
            string storehouse = "0", mark = "0", manager = "0", sales = "0", shipType = "0", paymentCond = "0", paymentMode = "0", andbuy = "0"
                , docCur = "0", cntctCode = "0";
            if (isAudit == "1" && isEdit == "0")
            {
                storehouse = bill.WhsCode;
                mark = bill.Indicator;
                manager = bill.OwnerCode;
                sales = bill.SlpCode;
                shipType = bill.TrnspCode;
                paymentCond = bill.GroupNum;
                paymentMode = bill.PeyMethod;
                andbuy = bill.U_YWY;
                docCur = bill.DocCur;
                cntctCode = bill.CntctCode;
            }
            string strSql = string.Format(" SELECT GroupNum AS id,PymntGroup AS name FROM {0}.crm_octg WHERE sbo_id={1}", "nasp_bone", int.Parse(bill.SboId));
            if (paymentCond != "0")
            {
                strSql += string.Format(" AND GroupNum = '{0}'", paymentCond);
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).Tolist<CurrencyList>(); ;

        }
        public List<DropPopupDocCurDto> DropPopupDocCurNew(string jobId, string isAudit, string docType)
        {
            billDelivery bill = DeSerialize<billDelivery>((byte[])(GetSalesInfo(jobId)));
            DataTable dt = GetSboNamePwd(int.Parse(bill.SboId));
            string dRowData = string.Empty; string isOpen = "0"; string sboname = "0"; string sqlconn = "0";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); sboname = dt.Rows[0][0].ToString(); sqlconn = dt.Rows[0][5].ToString(); }

            string isEdit = "0";
            if (isAudit == "1")
            {
                isEdit = GetIsEdit(jobId).ToString();
            }
            string storehouse = "0", mark = "0", manager = "0", sales = "0", shipType = "0", paymentCond = "0", paymentMode = "0", andbuy = "0"
                , docCur = "0", cntctCode = "0";
            if (isAudit == "1" && isEdit == "0")
            {
                storehouse = bill.WhsCode;
                mark = bill.Indicator;
                manager = bill.OwnerCode;
                sales = bill.SlpCode;
                shipType = bill.TrnspCode;
                paymentCond = bill.GroupNum;
                paymentMode = bill.PeyMethod;
                andbuy = bill.U_YWY;
                docCur = bill.DocCur;
                cntctCode = bill.CntctCode;
            }
            string strSql = " SELECT CurrCode AS id,CurrName AS name FROM " + "nsap_bone" + ".crm_ocrn WHERE sbo_id = " + bill.SboId + "";
            if (docCur != "0")
            {
                strSql += " AND CurrCode='" + docCur + "'";
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).Tolist<DropPopupDocCurDto>(); ;

        }
        public List<CurrencyList> DropPopupCntctPrsnSqlNew(string jobId, string isAudit, string docType)
        {
            billDelivery bill = DeSerialize<billDelivery>((byte[])(GetSalesInfo(jobId)));
            DataTable dt = GetSboNamePwd(int.Parse(bill.SboId));
            string dRowData = string.Empty; string isOpen = "0"; string sboname = "0"; string sqlconn = "0";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); sboname = dt.Rows[0][0].ToString(); sqlconn = dt.Rows[0][5].ToString(); }

            string isEdit = "0";
            if (isAudit == "1")
            {
                isEdit = GetIsEdit(jobId).ToString();
            }
            string storehouse = "0", mark = "0", manager = "0", sales = "0", shipType = "0", paymentCond = "0", paymentMode = "0", andbuy = "0"
                , docCur = "0", cntctCode = "0";
            if (isAudit == "1" && isEdit == "0")
            {
                storehouse = bill.WhsCode;
                mark = bill.Indicator;
                manager = bill.OwnerCode;
                sales = bill.SlpCode;
                shipType = bill.TrnspCode;
                paymentCond = bill.GroupNum;
                paymentMode = bill.PeyMethod;
                andbuy = bill.U_YWY;
                docCur = bill.DocCur;
                cntctCode = bill.CntctCode;
            }
            if (isOpen == "1")
            {
                if (!string.IsNullOrEmpty(sboname)) { sboname = sboname + ".dbo."; } else { sboname = ""; }
                string strSql = string.Format("SELECT b.CntctCode AS id,b.Name AS name FROM " + sboname + "OCRD a LEFT JOIN " + sboname + "OCPR b ON a.CardCode=b.CardCode WHERE a.CardCode='{0}' AND b.Active <> 'N' ", bill.CardCode);
                return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql, CommandType.Text, null).Tolist<CurrencyList>();
            }
            else
            {
                string strSql = string.Format("SELECT b.CntctCode AS id,b.Name AS `name` FROM {0}.crm_OCRD a LEFT JOIN {0}.crm_OCPR b ON a.sbo_id=b.sbo_id AND a.CardCode=b.CardCode WHERE a.sbo_id={1} AND a.CardCode='{2}' AND b.Active <> 'N' ", "nasp_bone", int.Parse(bill.SboId), bill.CardCode);

                return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).Tolist<CurrencyList>();
            }

        }
        public string SelectBalanceNew(string jobId, string isAudit, string docType)
        {
            billDelivery bill = DeSerialize<billDelivery>((byte[])(GetSalesInfo(jobId)));
            bool IsOpenSap = GetSapSboIsOpen(bill.SboId);
            if (IsOpenSap)
            {
                string strSql = string.Format(@"SELECT ISNULL(Balance,0) AS Balance FROM OCRD WHERE CardCode='{0}'", bill.CardCode);
                object obj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, strSql, CommandType.Text, null);
                return obj == null ? "0" : obj.ToString();
            }
            else
            {
                string strSql = string.Format("SELECT IFNULL(Balance,0) AS Balance FROM {0}.crm_OCRD WHERE sbo_id={1} AND CardCode='{2}'", "nsap_bone", int.Parse(bill.SboId), bill.CardCode);

                object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
                return obj == null ? "0" : obj.ToString();
            }

        }
        public string GetSumBalDueNew(string jobId)
        {
            billDelivery bill = DeSerialize<billDelivery>((byte[])(GetSalesInfo(jobId)));

            bool IsOpenSap = GetSapSboIsOpen(bill.SboId);
            if (IsOpenSap)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("SELECT sum(ISNULL(b.Balance,0)) AS Balance FROM OCRD b");
                strSql.AppendFormat(" WHERE b.SlpCode='{0}' and (b.cardtype='C' or b.cardtype='L')", bill.SlpCode);
                object obj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
                return obj == null ? "0" : obj.ToString();
            }
            else
            {
                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat("SELECT SUM(a.Debit-a.Credit) AS Total FROM {0}.finance_jdt1 a ", "nsap_bone");
                strSql.AppendFormat("LEFT JOIN {0}.crm_ocrd b ON a.sbo_id=b.sbo_id AND a.ShortName=b.CardCode AND a.ShortName LIKE '{1}%' ", "nsap_bone", "C");
                strSql.AppendFormat(" WHERE b.sbo_id={0} AND b.SlpCode={1}", bill.SboId, bill.SlpCode);

                object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
                return obj == null ? "0" : obj.ToString();
            }
        }
        public string GetAddressNew(string jobId, string isAudit, string docType)
        {
            billDelivery bill = DeSerialize<billDelivery>((byte[])(GetSalesInfo(jobId)));
            DataTable dt = GetSboNamePwd(int.Parse(bill.SboId));
            string dRowData = string.Empty; string isOpen = "0"; string sboname = "0"; string sqlconn = "0";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); sboname = dt.Rows[0][0].ToString(); sqlconn = dt.Rows[0][5].ToString(); }
            if (isOpen == "1")
            {
                StringBuilder sql = new StringBuilder();
                sql.Append(" SELECT Address AS name,(ISNULL(ZipCode,'') + ISNULL(b.Name,'')+ISNULL(c.Name,'')+ISNULL(City,'')+ISNULL(CONVERT(VARCHAR(1000),Building),'')) AS id,a.ZipCode,a.State ");
                sql.Append(" FROM CRD1 a ");
                sql.Append(" LEFT JOIN OCRY b ON a.Country=b.Code");
                sql.Append(" LEFT JOIN OCST c ON a.State=c.Code");
                sql.AppendFormat(" WHERE AdresType='{0}' AND CardCode='{1}' ", "B", bill.CardCode);

                return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, sql.ToString(), CommandType.Text, null).DataTableToJSON();
            }
            else
            {
                StringBuilder sql = new StringBuilder();
                sql.Append(" SELECT Address AS name,CONCAT(IFNULL(ZipCode,''),IFNULL(b.Name,''),IFNULL(c.Name,''),IFNULL(City,''),IFNULL(Building,'')) AS id,a.ZipCode,a.State ");
                sql.AppendFormat(" FROM {0}.crm_crd1 a", "nsap_bone");
                sql.AppendFormat(" LEFT JOIN {0}.store_ocry b ON a.Country=b.Code", "nsap_bone");
                sql.AppendFormat(" LEFT JOIN {0}.store_ocst c ON a.State=c.Code", "nsap_bone");
                sql.AppendFormat(" WHERE AdresType='{0}' AND CardCode='{1}' ", "B", bill.CardCode);
                return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql.ToString(), CommandType.Text, null).DataTableToJSON();
            }
        }
        public T DeSerialize<T>(byte[] bytes)
        {
            T oClass = default(T);
            if (bytes.Length == 0 || bytes == null) return oClass;
            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter bs = new BinaryFormatter();
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)bs.Deserialize(stream);
            }
        }
        #endregion
        #region 根据jobid获取信息
        /// <summary>
        /// 根据jobid获取信息
        /// </summary>
        public object GetSalesInfo(string jobId)
        {
            string sql = string.Format("SELECT job_data FROM {0}.wfa_job WHERE job_id = {1}", "nsap_base", jobId);
            return UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
        }
        /// <summary>
        /// 根据原单号查询
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public object GetSalesInfoNos(string jobId, int typeId)
        {
            string sql = string.Format("SELECT job_data FROM {0}.wfa_job where job_id = (SELECT job_id FROM {0}.wfa_job WHERE job_type_id={1} AND job_state=3 AND sync_stat=4 AND  sbo_itf_return={2} limit 1)", "nsap_base", typeId, jobId);
            return UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
        }
        public int GetIsEdit(string jobId)
        {
            int isEdit = 0;
            string sql = string.Format("SELECT is_edit FROM {0}.wfa_step WHERE step_id = (SELECT step_id FROM {0}.wfa_job WHERE job_id = {1} LIMIT 1)", "nsap_base", jobId);
            object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
            if (obj != null)
            {
                isEdit = int.Parse(UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, sql, CommandType.Text, null).ToString());
            }
            return isEdit;
            //return int.Parse(Sql.Action.ExecuteScalar(Sql.UTF8ConnectionString, CommandType.Text, sql).ToString());
        }

        public billDelivery GetSalesDealDetail(int jobid)
        {
            billDelivery Model = DeSerialize<billDelivery>((byte[])(GetSalesInfo(jobid.ToString())));
            return Model;
        }












        /// <summary>
        /// 查询单据关联的服务呼叫或物料成本
        /// </summary>
        /// <param name="docEntry">单据编号</param>
        /// <param name="sboId">帐套ID</param>
        /// <param name="docType">当前表</param>
        /// <param name="isAudit">查看或是审核</param>
        /// <param name="openDoc">call 关联报价单服务呼叫信息   stock关联报价单的物料成本</param>
        /// <returns></returns>
        public DataTable GetCallInfoById(string docEntry, string sboId, string docType, string isAudit, string openDoc)
        {
            if (isAudit == "0")
            {
                switch (docType)
                {
                    case "sale_oqut": docType = "a"; break;
                    case "sale_ordr": docType = "b"; break;
                    case "sale_odln": docType = "c"; break;
                    case "sale_oinv": docType = "d"; break;
                    case "sale_ordn": docType = "e"; break;
                    case "sale_orin": docType = "f"; break;
                    default: docType = "a"; break;
                }
            }
            else
            {
                switch (docType)
                {
                    case "ordr": docType = "a"; break;
                    case "odln": docType = "b"; break;
                    case "oinv": docType = "c"; break;
                    case "ordn": docType = "d"; break;
                    case "orin": docType = "e"; break;
                    default: docType = "a"; break;
                }
            }
            StringBuilder strSql = new StringBuilder();
            if (openDoc == "call")
            {
                strSql.AppendFormat("SELECT a.U_CallID,a.U_CallName,a.U_SerialNumber FROM {0}.sale_oqut a ", "nsap_bone");
            }
            else
            {
                strSql.AppendFormat("SELECT a.DocEntry,a.ItemCode ,a.StockPrice FROM {0}.sale_qut1 a  ", "nsap_bone");
            }
            strSql.AppendFormat("LEFT JOIN {0}.sale_rdr1 b ON b.BaseEntry = a.DocEntry AND b.sbo_id = a.sbo_id ", "nsap_bone");
            strSql.AppendFormat("LEFT JOIN {0}.sale_dln1 c ON c.BaseEntry = b.DocEntry AND c.sbo_id = b.sbo_id ", "nsap_bone");
            strSql.AppendFormat("LEFT JOIN {0}.sale_inv1 d ON d.BaseEntry = c.DocEntry AND d.sbo_id = c.sbo_id ", "nsap_bone");
            strSql.AppendFormat("LEFT JOIN {0}.sale_rdn1 e ON e.BaseEntry = c.DocEntry AND e.sbo_id = c.sbo_id ", "nsap_bone");
            strSql.AppendFormat("LEFT JOIN {0}.sale_rin1 f ON f.BaseEntry = d.DocEntry AND f.sbo_id = d.sbo_id ", "nsap_bone");
            strSql.AppendFormat("WHERE {2}.DocEntry = {0} AND a.sbo_id = '{1}' GROUP BY a.DocEntry", docEntry, sboId, docType);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
        }
        #endregion
        public DataTable SQLGetCustomeValueByFN(string TableID, string AliasID)
        {
            string strSql = string.Format(@"select t1.FldValue as id,t1.Descr as name from ufd1 t1 LEFT JOIN cufd t0 on t0.TableID=t1.TableID and t0.FieldID=t1.FieldID
                                            where t0.TableID='{0}' AND t0.AliasID='{1}' order by t1.IndexID asc ", TableID, AliasID);
            return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql, CommandType.Text, null);
        }

        #region 根据付款条件获取相关信息
        /// <summary>
        /// 根据付款条件获取相关信息
        /// </summary>
        public DataTable GetPayMentInfo(string GroupNum)
        {
            string strSql = string.Format("SELECT PrepaDay,PrepaPro,PayBefShip,GoodsToPro,GoodsToDay");
            strSql += string.Format(" FROM {0}.crm_octg_cfg", "nsap_bone");
            strSql += string.Format(" WHERE GroupNum={0}", GroupNum);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }
        #endregion
        public List<CustomFieldsNewDto> GetCustomFieldsNew(string TableName)
        {
            StringBuilder sBuilder = new StringBuilder();
            DataTable dt = GetCustomFieldsNewNos(TableName);

            var NewDtoList = new List<CustomFieldsNewDto>();


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var NewDto = new CustomFieldsNewDto();
                NewDto.AliasID = dt.Rows[i]["AliasID"].ToString().FilterString();
                NewDto.Descr = dt.Rows[i]["Descr"].ToString();
                NewDto.FieldID = dt.Rows[i]["FieldID"].ToString();
                NewDto.TableID = dt.Rows[i]["TableID"].ToString();
                NewDto.NewEditType = dt.Rows[i]["NewEditType"].ToString();
                NewDto.EditSize = dt.Rows[i]["EditSize"].ToString();
                NewDto.Line = new List<LineDto>();


                DataTable dtR = GetCustomValueNos(dt.Rows[i]["TableID"].ToString(), dt.Rows[i]["FieldID"].ToString());
                if (dtR.Rows.Count > 0)
                {
                    for (int k = 0; k < dtR.Rows.Count; k++)
                    {
                        var Line = new LineDto();
                        Line.id = dtR.Rows[k]["id"].ToString();
                        Line.name = dtR.Rows[k]["name"].ToString();
                        NewDto.Line.Add(Line);
                    }


                }
                NewDtoList.Add(NewDto);
            }


            return NewDtoList;
        }
        public DataTable GetCustomFieldsNewNos(string TableName)
        {
            string strSql = string.Format("SELECT AliasID,Descr,FieldID,TableID,EditSize,CASE LENGTH(EditType) WHEN 0 then 'A' ELSE EditType END NewEditType FROM {0}.base_cufd", "nsap_bone");
            if (!string.IsNullOrEmpty(TableName))
            {
                strSql += string.Format(" WHERE TableID='{0}'", TableName);
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }

        #region 流程图
        /// <summary>
        /// 流程图
        /// </summary>
        public FlowChart GetFlowChartByJobID(string jobID)
        {
            DataTable logTable = GetAuditLogWithFlowChart(jobID);
            if (logTable.Rows.Count > 0)
            {
                FlowChart flowChart = new FlowChart();
                List<FlowStep> flowSteps = new List<FlowStep>();
                for (int i = 0; i < logTable.Rows.Count; i++)
                {
                    DataRow logRow = logTable.Rows[i];
                    FlowStep flowStep = new FlowStep();

                    if (logRow[1].ToString() == "0" && logRow[2].ToString() == "4")
                    {
                        flowStep.StepName = "完 成";
                    }
                    else
                    {
                        DataTable stepTable = GetAuditStepWithFlowChart(logRow[4].ToString(), logRow[1].ToString());
                        if (stepTable.Rows.Count > 0)
                        {
                            DataRow stepRow = stepTable.Rows[0];

                            flowStep.StepName = stepRow[0].ToString();
                            flowStep.Relation = stepRow[1].ToString();
                            flowStep.StepGoto = stepRow[2].ToString();
                        }
                        List<string> planAuditor = new List<string>();
                        if (i == 0)
                        {
                            DataTable objTable = GetAuditObjWithFlowChart(jobID);
                            if (objTable.Rows.Count > 0)
                            {
                                foreach (DataRow objRow in objTable.Rows)
                                {
                                    planAuditor.Add(objRow[0].ToString());
                                }
                            }
                        }
                        flowStep.PlanAuditors = planAuditor;

                        List<Auditor> realAuditor = new List<Auditor>();
                        realAuditor.Add(new Auditor()
                        {
                            Name = logRow[5].ToString(),
                            Result = logRow[2].ToString(),
                            CheckTime = logRow[0].ToString(),
                            Comment = logRow[3].ToString(),
                            depAlias = logRow[6].ToString()
                        });
                        if (i < logTable.Rows.Count - 1)
                        {
                            DataRow nextRow = logTable.Rows[++i]; bool isEqual = false;
                            while (nextRow[1].ToString() == logRow[1].ToString())
                            {
                                realAuditor.Add(new Auditor()
                                {
                                    Name = nextRow[5].ToString(),
                                    Result = nextRow[2].ToString(),
                                    CheckTime = nextRow[0].ToString(),
                                    Comment = nextRow[3].ToString(),
                                    depAlias = logRow[6].ToString()
                                });
                                if (i < logTable.Rows.Count - 1)
                                {
                                    nextRow = logTable.Rows[++i];
                                }
                                else { isEqual = true; break; }
                            }
                            if (!isEqual) --i;
                        }
                        flowStep.RealAuditors = realAuditor;
                    }
                    flowSteps.Add(flowStep);
                }
                flowChart.Steps = flowSteps;
                //return JsonHelper.ParseModel(flowChart);
                return flowChart;
            }
            else return null;
        }
        /// <summary>
        /// 获取指定流程任务的审核记录
        /// </summary>
        public DataTable GetAuditLogWithFlowChart(string jobID)
        {
            string sql = string.Format("SELECT a.log_dt,a.audit_level,a.state,a.remarks,b.job_type_id,IFNULL(c.user_nm,'') user_nm,IFNULL(e.dep_alias,'') dep_alias FROM {0}.wfa_log a INNER JOIN {0}.wfa_job b ON a.job_id=b.job_id LEFT JOIN {0}.base_user c ON a.user_id=c.user_id LEFT JOIN {0}.base_user_detail d ON c.user_id=d.user_id LEFT JOIN {0}.base_dep e ON d.dep_id=e.dep_id WHERE a.job_id={1} ORDER BY a.job_id ASC", "nsap_base", jobID);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
        }
        /// <summary>
        /// 获取指定审核流程的步骤
        /// </summary>
        public DataTable GetAuditStepWithFlowChart(string jobType, string auditLevel)
        {
            string sql = string.Format("SELECT step_nm,audit_obj_rela,sql_explain FROM {0}.wfa_step WHERE job_type_id={1} AND audit_level={2} LIMIT 1", "nsap_base", jobType, auditLevel);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
        }
        /// <summary>
        /// 获取指定流程任务的流程跳转数据
        /// </summary>
        public DataTable GetAuditObjWithFlowChart(string jobID)
        {
            string sql = string.Format("SELECT CONCAT(IF(d.dep_alias IS NULL,'',CONCAT(d.dep_alias,'-')),b.user_nm) Name FROM {0}.wfa_jump a INNER JOIN {0}.base_user b ON a.user_id=b.user_id LEFT JOIN {0}.base_user_detail c ON b.user_id=c.user_id INNER JOIN {0}.base_dep d ON c.dep_id=d.dep_id WHERE a.job_id={1} AND a.audit_level=(SELECT b.audit_level FROM {0}.wfa_job a INNER JOIN {0}.wfa_step b ON a.step_id=b.step_id WHERE a.job_id={1} LIMIT 1)", "nsap_base", jobID);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
        }
        #endregion
        public DataTable GridRelORDRList(out int rowCount, int pageSize, int pageIndex, string DocEntry, string cardcode, string sortname, string sortorder, string SlpCode)
        {
            string sortString = string.Empty;
            string filterString = "(Canceled = 'Y' or DocStatus = 'O') and SlpCode =" + SlpCode;
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());
            string dRowData = string.Empty;
            #region 搜索条件
            if (!string.IsNullOrEmpty(DocEntry))
            {
                filterString += "and docentry=" + DocEntry;
            }
            if (!string.IsNullOrEmpty(cardcode))
            {
                filterString += string.Format("and cardcode LIKE '%{0}%'", cardcode);
            }
            #endregion

            return GridRelORDRList(out rowCount, pageSize, pageIndex, filterString, sortString);
        }
        public DataTable GridRelORDR(out int rowCount, int pageSize, int pageIndex, string DocEntry, string cardcode, string sortname, string sortorder, string SlpCode)
        {
            string sortString = string.Empty;
            string filterString = "(Canceled = 'Y' or DocStatus = 'O') and SlpCode =" + SlpCode;
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());
            string dRowData = string.Empty;
            #region 搜索条件
            if (!string.IsNullOrEmpty(DocEntry))
            {
                filterString += string.Format("and docentry LIKE '%{0}%'", DocEntry);
            }
            if (!string.IsNullOrEmpty(cardcode))
            {
                filterString += string.Format("and cardcode LIKE '%{0}%'", cardcode);
            }
            #endregion

            return GridRelORDRList(out rowCount, pageSize, pageIndex, filterString, sortString);
        }
        /// <summary>
        /// 取到销售员所有未清销售合同与已取消的
        /// </summary>
        /// <param name="rowCounts"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="filterQuery"></param>
        /// <param name="sortname"></param>
        /// <returns></returns>
        public DataTable GridRelORDRList(out int rowCounts, int pageSize, int pageIndex, string filterQuery, string sortname)
        {
            string fieldstr = "docentry,cardcode,doctotal,CreateDate,docstatus,Printed,CANCELED,Comments";
            return SAPSelectPagingHaveRowsCount("ORDR", fieldstr, pageSize, pageIndex, sortname, filterQuery, out rowCounts);
        }
        #region 根据func_id获取附件类型
        /// <summary>
        /// 根据func_id获取附件类型
        /// </summary>
        public DataTable GetattchtypeByfuncid(int func_id)
        {
            string strSql = string.Format("SELECT type_id,type_nm from {0}.file_type", "nsap_oa");
            strSql += string.Format(" WHERE func_id={0}", func_id);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }
        #endregion
        #region  流程任务 - 撤回(审核中的)
        /// <summary>
        ///流程任务 - 撤回(审核中的)
        /// </summary>
        public bool ICreatedBack(string keyIds, string userId, string urlType)
        {

            if (GetJobStateById(keyIds) == "1")
            {

                string errorMsg = string.Empty;

                List<CmdParameter> cmdParameters = new List<CmdParameter>();
                IDataParameter[] parameter = new IDataParameter[] { new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?job_id", keyIds) };
                CmdParameter qcmdParameter = new CmdParameter();
                qcmdParameter.Sql = string.Format("DELETE FROM {0}.wfa_jump WHERE job_id ={1};", "nsap_base", keyIds);
                cmdParameters.Add(qcmdParameter);

                CmdParameter ecmdParameter = new CmdParameter();
                ecmdParameter.Sql = string.Format("UPDATE {0}.wfa_job SET job_state=0,step_id=0 WHERE job_id ={1};", "nsap_base", keyIds);
                cmdParameters.Add(ecmdParameter);

                CmdParameter tcmdParameter = new CmdParameter();
                tcmdParameter.Sql = string.Format("DELETE FROM {0}.wfa_job_para WHERE job_id ={1};", "nsap_base", keyIds);
                cmdParameters.Add(tcmdParameter);

                CmdParameter rcmdParameter = new CmdParameter();
                rcmdParameter.Sql = string.Format("INSERT INTO {0}.wfa_log(job_id,user_id,audit_level,state) VALUES({1},{2},{3},{4});", "nsap_base", keyIds, userId, "0", "6");
                cmdParameters.Add(rcmdParameter);

                //销售序列号撤销
                if (urlType.ToUpper() == "sales/SalesDelivery.aspx".ToUpper() && urlType.ToUpper() == "sales/salesreturnofgoods.aspx".ToUpper() || urlType.ToUpper() == "sales/salesreturnofgoodsline.aspx".ToUpper() || urlType.ToUpper() == "sales/salescreditmemo.aspx".ToUpper())
                {
                    billDelivery bill = DeSerialize<billDelivery>((byte[])(GetSalesInfo(keyIds)));
                    foreach (billSerialNumber osrn in bill.serialNumber)
                    {
                        foreach (billSerialNumberChooseItem serial in osrn.Details)
                        {
                            CmdParameter snbParameter = new CmdParameter();
                            snbParameter.Sql = string.Format("DELETE FROM {0}.store_osrn_alreadyexists WHERE ItemCode ={1} AND SysNumber ={2};", "nsap_bone", osrn.ItemCode, serial.SysSerial);
                            cmdParameters.Add(snbParameter);
                        }
                    }
                }
                //库存转储序列号撤销
                if (urlType.ToUpper() == "store/stocktransfer.aspx".ToUpper())
                {
                    storeOWTR bill = DeSerialize<storeOWTR>((byte[])(GetSalesInfo(keyIds)));
                    foreach (billSerialNumber osrn in bill.serialNumber)
                    {
                        foreach (billSerialNumberChooseItem serial in osrn.Details)
                        {
                            CmdParameter snbParameter = new CmdParameter();
                            snbParameter.Sql = string.Format("DELETE FROM {0}.store_osrn_alreadyexists WHERE ItemCode = {1} AND SysNumber = {2};", "nsap_bone", osrn.ItemCode, serial.SysSerial);
                            cmdParameters.Add(snbParameter);
                        }
                    }
                }
                //采购序列号撤销 
                if (urlType.ToUpper() == "purchase/purchasereturn.aspx".ToUpper() || urlType.ToUpper() == "purchase/purchasecreditmemo.aspx".ToUpper())
                {
                    billDelivery bill = DeSerialize<billDelivery>((byte[])(GetSalesInfo(keyIds)));
                    foreach (billSerialNumber osrn in bill.serialNumber)
                    {
                        foreach (billSerialNumberChooseItem serial in osrn.Details)
                        {
                            CmdParameter snbParameter = new CmdParameter();
                            snbParameter.Sql = string.Format("DELETE FROM {0}.store_osrn_alreadyexists WHERE ItemCode = {1} AND SysNumber = {2};", "nasp_bone", osrn.ItemCode, serial.SysSerial);
                            cmdParameters.Add(snbParameter);
                        }
                    }
                }
                // 生产发料 
                if (urlType.ToUpper() == "product/producematerial.aspx".ToUpper())
                {
                    proReceipt bill = DeSerialize<proReceipt>((byte[])(GetSalesInfo(keyIds)));
                    foreach (billSerialNumber osrn in bill.serialNumber)
                    {
                        foreach (billSerialNumberChooseItem serial in osrn.Details)
                        {
                            CmdParameter snbParameter = new CmdParameter();
                            snbParameter.Sql = string.Format("DELETE FROM {0}.store_osrn_alreadyexists WHERE ItemCode = {1} AND SysNumber = {2};", "nsap_bone", osrn.ItemCode, serial.SysSerial);
                            cmdParameters.Add(snbParameter);
                        }
                    }
                }
                int resultCount = ExecuteTransaction(cmdParameters, out errorMsg);

                //解除服务呼叫绑定售后报价单
                if (urlType.ToUpper() == "sales/SalesQuotation.aspx".ToUpper() && resultCount > 0)
                {
                    billDelivery bill = DeSerialize<billDelivery>((byte[])(GetSalesInfo(keyIds)));
                    if (!string.IsNullOrEmpty(bill.U_CallID))
                    {
                        UpdateUsftjbjFromOscl(bill.U_CallID, bill.SboId, "0");
                    }
                }

                return resultCount > 0 ? true : false;
            }
            else
            {
                return false;
            }
        }

        #region 获取流程任务当前状态
        /// <summary>
        /// 获取流程任务当前状态
        /// </summary>
        /// <returns></returns>
        public string GetJobStateById(string JobId)
        {
            string strSql = string.Format("SELECT job_state FROM {0}.wfa_job WHERE job_id={1}", "nsap_base", JobId);
            return UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).ToString();
        }
        #endregion
        public int ExecuteTransaction(List<CmdParameter> array, out string errorMsg)
        {
            if (array.Count == 0) { errorMsg = string.Empty; return 0; }
            int count = 0; errorMsg = string.Empty;
            try
            {
                string rValue = string.Empty;
                foreach (CmdParameter cmd in array)
                {
                    if (!string.IsNullOrEmpty(cmd.Sql))
                    {
                        try
                        {
                            if (cmd.IsReturn)
                            {
                                object o = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, cmd.Sql.Replace(CmdParameter.ReturnMark, rValue), CommandType.Text, null);
                                if (o != null) rValue = o.ToString();
                            }
                            else UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, cmd.Sql.Replace(CmdParameter.ReturnMark, rValue), CommandType.Text, null);
                            count++;
                        }
                        catch (Exception e) { throw new Exception(string.Format("An error occurs when transactions are executed: \r\nReason[{0}]\r\nStatement[{1}]", e.Message, cmd.Sql)); }
                    }
                    else throw new Exception("When transactions are executed, Sql statement can not be empty");
                }

            }
            catch (Exception e)
            {

                count = 0; errorMsg = e.Message;
            }


            return count;
        }
        #endregion
        #region 修改服务呼叫状态
        /// <summary>
        /// 修改服务呼叫状态（U_SFTJBJ 0未关联报价单  1已关联报价单）
        /// </summary>
        /// <param name="callID">服务呼叫ID</param>
        /// <param name="sbo_id">帐套ID</param>
        /// <returns></returns>
        public int UpdateUsftjbjFromOscl(string callID, string sbo_id, string state)
        {
            string sqlStr = string.Format("UPDATE {0}.service_oscl SET U_SFTJBJ={1} WHERE callID = {2} AND sbo_id = {3};", "nsap_bone", state, callID, sbo_id);
            return UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, sqlStr, CommandType.Text, null) != null ? 1 : 0;
        }
        #endregion
        #region 删除我创建的(草稿&驳回)
        /// <summary>
        ///删除我创建的(草稿&驳回)
        /// </summary>
        public bool ICreatedDelete(string keyIds)
        {
            string sql = string.Format("UPDATE {0}.wfa_job SET job_state=-1 WHERE (job_state=0 OR job_state=2) AND  job_id IN ({1})", "nsap_base", keyIds);
            return UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, sql, CommandType.Text, null) == null ? true : false;
        }
        #endregion
        public DataTable SelectAccountsView(out int rowCount, SelectAccountsReq model, string SboId)
        {

            string sortString = string.Empty; string filterString = string.Empty;
            if (!string.IsNullOrEmpty(model.sortname) && !string.IsNullOrEmpty(model.sortorder))
                sortString = string.Format("{0} {1}", model.sortname, model.sortorder.ToUpper());
            if (!string.IsNullOrEmpty(model.CardCode))
            {
                filterString += string.Format("T0.ShortName = '{0}' AND ", model.CardCode);
            }
            if (!string.IsNullOrEmpty(model.SlpCode))
            {
                filterString += string.Format("T1.SlpCode = '{0}'  AND (CASE T0.TransType WHEN T3.ObjType THEN T3.ItemCode ELSE T4.ItemCode END) IS NOT NULL AND ", model.SlpCode);
            }
            filterString += string.Format(" T0.sbo_id={0} AND ", SboId);
            if (!string.IsNullOrEmpty(filterString))
                filterString = filterString.Substring(0, filterString.Length - 5);
            return SelectAccountsView(out rowCount, model.limit, model.page, filterString, sortString, model.type);
        }
        public DataTable SelectAccountsView(out int rowCount, int pageSize, int pageIndex, string filterQuery, string orderName, string type)
        {
            StringBuilder tableName = new StringBuilder();
            StringBuilder filedName = new StringBuilder();
            //改动后正确版本 filedName.Append(" (CASE T0.TransType WHEN T3.ObjType THEN T3.DocDate ELSE T4.DocDate END) AS DocDate,T0.LineMemo,T0.BaseRef");

            filedName.Append(" distinct (CASE T0.TransType WHEN T3.ObjType THEN T3.DocDate ELSE T4.DocDate END) AS DocDate, T0.LineMemo,T0.BaseRef,'',T0.BalDueDeb");
            tableName.AppendFormat(" {0}.finance_jdt1 T0", "nsap_bone");
            tableName.AppendFormat(" LEFT JOIN {0}.crm_ocrd T1 ON  T0.ShortName=T1.CardCode AND T0.sbo_id=T1.sbo_id", "nsap_bone");
            tableName.AppendFormat(" LEFT JOIN {0}.base_sap_doctype T2 ON T0.TransType=T2.doc_type ", "nsap_bone");
            if (type == "C")
            {
                tableName.AppendFormat(" LEFT JOIN {0}.sale_inv1 T3 ON T0.BaseRef=T3.DocEntry AND T0.TransType=T3.ObjType AND T0.sbo_id=T3.sbo_id", "nsap_bone");
                tableName.AppendFormat(" LEFT JOIN {0}.sale_rin1 T4 ON T0.BaseRef=T4.DocEntry AND T0.TransType=T4.ObjType AND T0.sbo_id=T4.sbo_id", "nsap_bone");
                tableName.AppendFormat(" LEFT JOIN {0}.finance_orct T6 ON T0.BaseRef=T6.DocEntry AND T0.TransType=T6.ObjType AND T0.sbo_id=T6.sbo_id", "nsap_bone");
                tableName.AppendFormat(" LEFT JOIN {0}.sbo_info T5 ON T0.sbo_id=T5.sbo_id", "nsap_bone");
            }
            else
            {
                tableName.AppendFormat(" LEFT JOIN {0}.buy_pch1 T3 ON T0.BaseRef=T3.DocEntry AND T0.TransType=T3.ObjType AND T0.sbo_id=T3.sbo_id", "nsap_bone");
                tableName.AppendFormat(" LEFT JOIN {0}.buy_rpc1 T4 ON T0.BaseRef=T4.DocEntry AND T0.TransType=T4.ObjType AND T0.sbo_id=T4.sbo_id", "nsap_bone");
                tableName.AppendFormat(" LEFT JOIN {0}.sbo_info T5 ON T0.sbo_id=T5.sbo_id", "nsap_bone");
            }
            //filterQuery += string.Format(" GROUP BY T2.Name,T0.BaseRef,(CASE T0.TransType WHEN T3.ObjType THEN T3.ItemCode ELSE T4.ItemCode END)");
            return SelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), pageSize, pageIndex, orderName, filterQuery, out rowCount);
        }
        public DataTable SelectCopyItemAllView(out int rowCount, GridCopyItemListReq model, int SboID, int UserID)
        {
            string sortString = string.Empty; string filterString = string.Empty;
            string type = string.Empty; string line = string.Empty;
            if (!string.IsNullOrEmpty(model.sortname) && !string.IsNullOrEmpty(model.sortorder))
                sortString = string.Format("{0} {1}", model.sortname, model.sortorder.ToUpper());
            DataTable dt = GetSboNamePwd(SboID);
            string dRowData = string.Empty; string isOpen = "0"; string sboname = ""; string sqlconn = "";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); sboname = dt.Rows[0][0].ToString(); sqlconn = dt.Rows[0][5].ToString(); }
            #region 搜索条件
            #region 根据不同的单据类型获取不同的物料
            if (!string.IsNullOrEmpty(model.txtCopyDocType))
            {
                if (model.txtCopyDocType == "23") { type = "sale_oqut"; line = "sale_qut1"; }//销售报价单
                else if (model.txtCopyDocType == "17") { type = "sale_ordr"; line = "sale_rdr1"; }//销售订单
                else if (model.txtCopyDocType == "15") { type = "sale_odln"; line = "sale_dln1"; }//销售交货单
                else if (model.txtCopyDocType == "13") { type = "sale_oinv"; line = "sale_inv1"; }//应收发票
                else if (model.txtCopyDocType == "16") { type = "sale_ordn"; line = "sale_rdn1"; }//销售退货单
                else if (model.txtCopyDocType == "14") { type = "sale_orin"; line = "sale_rin1"; }//应收贷项凭证
                else if (model.txtCopyDocType == "54") { type = "buy_opqt"; line = "buy_pqt1"; }//采购报价单
                else if (model.txtCopyDocType == "22") { type = "buy_opor"; line = "buy_por1"; }//采购订单
                else if (model.txtCopyDocType == "20") { type = "buy_opdn"; line = "buy_pdn1"; }//采购收货单
                else if (model.txtCopyDocType == "18") { type = "buy_opch"; line = "buy_pch1"; }//应付发票
                else if (model.txtCopyDocType == "21") { type = "buy_orpd"; line = "buy_rpd1"; }//采购退货单
                else if (model.txtCopyDocType == "19") { type = "buy_orpc"; line = "buy_rpc1"; }//应付贷项凭证
                else if (model.txtCopyDocType == "wtr1") { type = "store_owtr"; line = "store_wtr1"; }//库存转储
                else
                {
                    if (model.doctype == "buy") { type = "buy_opqt"; line = "buy_pqt1"; }
                    else { type = "sale_oqut"; line = "sale_qut1"; }
                }
            }
            else
            {
                if (model.doctype == "buy") { type = "buy_opqt"; line = "buy_pqt1"; }
                else { type = "sale_oqut"; line = "sale_qut1"; }
            }
            #endregion
            if (!string.IsNullOrEmpty(model.txtCardCode))
            {
                filterString += string.Format("(a.CardCode LIKE '%{0}%' OR a.CardName LIKE '%{0}%') AND ", model.txtCardCode.FilterWildCard());
            }

            if (!string.IsNullOrEmpty(model.txtItemCode))
            {
                filterString += string.Format("(b.ItemCode LIKE '%{0}%' OR b.Dscription LIKE '%{0}%') AND ", model.txtItemCode.FilterWildCard());
            }
            if (!string.IsNullOrEmpty(model.txtDocEntry))
            {
                filterString += string.Format("(a.DocEntry = '{0}') AND ", model.txtDocEntry.FilterWildCard());
            }

            #endregion
            DataTable rDataRowsSlp = GetSboSlpCodeId(UserID, SboID);
            if (type != "store_owtr")
            {
                if (rDataRowsSlp.Rows.Count > 0)
                {
                    string slpCode = rDataRowsSlp.Rows[0][0].ToString();
                    filterString += string.Format(" a.SlpCode = '{0}' AND a.sbo_id={1} AND m.ItemCode IS NOT NULL AND ", slpCode, SboID);
                }
            }
            if (!string.IsNullOrEmpty(filterString))
                filterString = filterString.Substring(0, filterString.Length - 5);
            //filterQuery += string.Format(" GROUP BY T2.Name,T0.BaseRef,(CASE T0.TransType WHEN T3.ObjType THEN T3.ItemCode ELSE T4.ItemCode END)");
            StringBuilder filedName = new StringBuilder();
            StringBuilder tableName = new StringBuilder();
            string U_SHJSDJ = "", U_SHJSJ = "", U_SHTC = "";
            if (IsExistMySql(line, "U_SHJSDJ"))
            {
                U_SHJSDJ = ",IFNULL(b.U_SHJSDJ,0)";
            }
            if (IsExistMySql(line, "U_SHJSJ"))
            {
                U_SHJSJ = ",IFNULL(b.U_SHJSJ,0)";
            }
            if (IsExistMySql(line, "U_SHTC"))
            {
                U_SHTC = ",IFNULL(b.U_SHTC,0)";
            }
            filedName.Append("ROW_NUMBER() OVER (ORDER BY a.DocEntry) RowNum,a.DocEntry,a.CardCode,a.CardName,b.ItemCode,b.Dscription,b.Quantity,b.Price,b.LineTotal,b.WhsCode,w.OnHand,b.BaseLine,m.LastPurPrc,");
            filedName.Append("IFNULL(m.U_TDS,'0') AS U_TDS,IFNULL(m.U_DL,'0') AS U_DL,IFNULL(m.U_DY,'0') AS U_DY,m.U_JGF,");
            filedName.Append("((IFNULL((CASE m.QryGroup1 WHEN 'N' THEN 0 ELSE '0.5' END),0))");
            filedName.Append("+(IFNULL((CASE m.QryGroup2 WHEN 'N' THEN 0 ELSE '3' END),0))");
            filedName.Append("+(IFNULL((CASE m.QryGroup3 WHEN 'N' THEN 0 ELSE '2' END),0))) AS QryGroup,");
            filedName.Append("IFNULL(m.U_US,0) AS U_US,IFNULL(m.U_FS,0) AS U_FS,m.QryGroup3,m.SVolume,m.SWeight1,");
            filedName.Append("b.U_PDXX,m.IsCommited,m.OnOrder,(m.OnHand-m.IsCommited+m.OnOrder) AS OnAvailable,m.U_JGF1,IFNULL(m.U_YFCB,'0'),m.OnHand AS OnHandS,m.MinLevel,m.PurPackUn,m.buyunitmsr");
            filedName.AppendFormat("{0}{1}{2}", U_SHJSDJ, U_SHJSJ, U_SHTC);
            tableName.AppendFormat(" {0}." + type + " a LEFT JOIN {0}." + line + " b ON a.DocEntry=b.DocEntry AND a.sbo_id=b.sbo_id", "nsap_bone");
            tableName.AppendFormat(" LEFT JOIN {0}.store_oitw w ON b.ItemCode=w.ItemCode AND b.WhsCode=w.WhsCode AND b.sbo_id=w.sbo_id", "nsap_bone");
            tableName.AppendFormat(" LEFT JOIN {0}.store_oitm m ON b.ItemCode=m.ItemCode AND m.sbo_id=b.sbo_id", "nsap_bone");
            return SelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), model.limit, model.page, sortString, filterString, out rowCount);
        }
        public DataTable GridRelationContractList(out int rowCount, GridRelationContractListReq model, int SboID, int UserID)
        {
            string sortString = string.Empty;
            string filterString = string.Empty;
            if (!string.IsNullOrEmpty(model.sortname) && !string.IsNullOrEmpty(model.sortorder))
                sortString = string.Format(" {0} {1}", model.sortname, model.sortorder.ToUpper());
            filterString = string.Format(" sbo_id={0} and itemcode='{1}' and CardCode='{2}'", SboID, model.ItemCode.FilterSQL(), model.CardCode);
            #region 搜索条件  
            if (!string.IsNullOrEmpty(model.DocEntry))
            {
                filterString += string.Format(" and contract_id={0} ", model.DocEntry.FilterSQL().Trim());

            }

            #endregion
            return GridRelationContractList(out rowCount, model.limit, model.page, filterString, sortString);

        }
        /// <summary>
        /// 获取销售报价单/订单 同客户/物料的合约评审
        /// </summary>
        /// <param name="itemcode"></param>
        /// <param name="cardcode"></param>
        /// <param name="sboid"></param>
        /// <returns></returns>
        public DataTable GridRelationContractList(out int rowCount, int pageSize, int pageIndex, string filterQuery, string orderName)
        {
            string tablename = string.Format(" {0}.sale_contract_review", "nsap_bone");
            string fieldname = " sbo_id,contract_id,price,qty,sum_total,deliver_dt,walts,comm_rate,custom_req";
            return SelectPagingHaveRowsCount(tablename, fieldname, pageSize, pageIndex, orderName, filterQuery, out rowCount);
        }
        #region 10.9销售订单接口
        public DataTable SelectBillListInfo_ORDR(out int rowCount, string docEntrys, SalesOrderListReq model, string type, bool ViewFull, bool ViewSelf, int UserID, int SboID, bool ViewSelfDepartment, int DepID, bool ViewCustom, bool ViewSales, string sqlcont, string sboname)
        {
            bool IsSql = true;
            string sortString = string.Empty;
            string filterString = string.Empty;
            string line = string.Empty; int uSboId = SboID;
            if (!string.IsNullOrEmpty(model.sortname) && !string.IsNullOrEmpty(model.sortorder))
                sortString = string.Format("{0} {1}", model.sortname, model.sortorder.ToUpper());
            string dRowData = string.Empty;
            #region 搜索条件
            //if (!string.IsNullOrEmpty(model.NewDocEntry))
            //{
            //    List<int> docEntrys = _orderDraftServiceApp.GetDocEntrys(model.NewDocEntry);
            //    filterString += string.Format("a.DocEntry in ({0}) AND ", string.Join(",", docEntrys, 0, docEntrys.Count()));
            //}
            if (model.ReceiptStatus == "K")
            {
                
                filterString += string.Format("a.DocEntry in ('{0}') AND ", docEntrys);
            }


            if (!string.IsNullOrEmpty(model.DocEntry))
            {
                filterString += string.Format("a.DocEntry LIKE '{0}' AND ", model.DocEntry.FilterSQL().Trim());

            }
            if (!string.IsNullOrEmpty(model.CardCode))
            {
                filterString += string.Format("(a.CardCode LIKE '%{0}%' OR a.CardName LIKE '%{0}%') AND ", model.CardCode.FilterWildCard());

            }
            if (!string.IsNullOrEmpty(model.DocStatus))
            {
                if (model.DocStatus == "ON") { filterString += string.Format(" (a.DocStatus = 'O' AND a.Printed = 'N' AND a.CANCELED = 'N') AND "); }
                if (model.DocStatus == "OY") { filterString += string.Format(" (a.DocStatus = 'O' AND a.Printed = 'Y' AND a.CANCELED = 'N') AND "); }
                if (model.DocStatus == "CY") { filterString += string.Format(" a.CANCELED = 'Y' AND "); }
                if (model.DocStatus == "CN") { filterString += string.Format(" (a.DocStatus = 'C' AND a.CANCELED = 'N') AND "); }
                if (model.DocStatus == "NC") { filterString += string.Format(" a.CANCELED = 'N' AND "); }
            }
            if (!string.IsNullOrEmpty(model.Comments))
            {
                filterString += string.Format("a.Comments LIKE '%{0}%' AND ", model.Comments.FilterWildCard());
            }
            if (!string.IsNullOrEmpty(model.SlpName))
            {
                filterString += string.Format("c.SlpName LIKE '%{0}%' AND ", model.SlpName.FilterSQL().Trim());
            }
            if (!string.IsNullOrEmpty(model.ToCompany))
            {
                filterString += string.Format("a.Indicator = '{0}' AND ", model.ToCompany);
            }
            if (!string.IsNullOrEmpty(model.ReceiptStatus))
            {
                if (model.ReceiptStatus == "Y")
                {
                    filterString += string.Format("a.U_DocRCTAmount>0.00 AND ");
                }
                else if (model.ReceiptStatus == "W")
                {
                    filterString += string.Format("a.U_DocRCTAmount =0.00 AND ");
                }
            }
            #endregion

            #region 判断权限

            if (ViewSelfDepartment && !ViewFull)
            {
                DataTable rDataRows = GetSboSlpCodeIds(DepID, SboID);
                if (rDataRows.Rows.Count > 0)
                {
                    filterString += string.Format(" (a.SlpCode IN(");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][0]);
                    }
                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(") OR d.DfTcnician IN (");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][1]);
                    }
                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(")) AND ");
                }
            }
            else if (ViewSelf && !ViewFull && !ViewSelfDepartment)
            {
                DataTable rDataRowsSlp = GetSboSlpCodeId(UserID, SboID);
                if (rDataRowsSlp.Rows.Count > 0)
                {
                    string slpCode = rDataRowsSlp.Rows[0][0].ToString();
                    string slpTcnician = rDataRowsSlp.Rows[0][1].ToString();
                    filterString += string.Format(" (a.SlpCode = {0}) AND ", slpCode);// OR d.DfTcnician={1}   , slpTcnician  不允许售后查看业务员的单
                }
                else
                {
                    filterString = string.Format(" (a.SlpCode = {0}) AND ", 0);
                }
            }
            #endregion
            if (!string.IsNullOrEmpty(filterString))
                filterString = filterString.Substring(0, filterString.Length - 5);
            if (IsSql)
            {
                DataTable thistab = SelectBillListInfo_ORDRNew(out rowCount, model.limit, model.page, filterString, sortString, line, ViewCustom, ViewSales, sqlcont, sboname, SboID);
                return thistab;
            }
            else
            {
                return SelectBillViewInfo(out rowCount, model.limit, model.page, model.query, model.sortname, model.sortorder, type, ViewFull, ViewSelf, UserID, uSboId, ViewSelfDepartment, DepID, ViewCustom, ViewSales);
            }
        }
        /// <summary>
        /// 查看销售订单视图
        /// </summary>
        /// <returns></returns>
        public DataTable SelectBillListInfo_ORDRNew(out int rowCounts, int pageSize, int pageIndex, string filterQuery, string orderName, string line, bool ViewCustom, bool ViewSales, string sqlcont, string sboname, int nsapsboId = 1)
        {
            StringBuilder tableName = new StringBuilder();
            StringBuilder filedName = new StringBuilder(); int Custom = 0; int Sales = 0;
            if (ViewCustom) { Custom = 1; }
            if (ViewSales) { Sales = 1; }
            if (string.IsNullOrEmpty(sboname)) { sboname = ""; } else { sboname = sboname + ".dbo."; }
            filedName.Append(" a.UpdateDate,a.DocEntry,a.CardCode,CASE WHEN 1 = " + Custom + " THEN a.CardName ELSE '******' END AS CardName,CASE WHEN 1 = " + Sales + " THEN a.DocTotal ELSE 0 END AS DocTotal,CASE WHEN 1 = " + Sales + " THEN (a.DocTotal-a.PaidToDate) ELSE 0 END AS OpenDocTotal,a.CreateDate,a.SlpCode,a.Comments,a.DocStatus,a.Printed,c.SlpName,a.CANCELED,a.Indicator,a.DocDueDate,e.PymntGroup,'' as billID,'' AS ActualDocDueDate ");
            filedName.Append(",'10011111-28a9-4767-854f-77246e36d24d1111111111111111111' as PrintNo,'00' as PrintNumIndex,'' as billStatus,'' as bonusStatus,'' as proStatus,n.Name as IndicatorName,'*********************************' as EmpAcctWarn,'' as AttachFlag,a.U_DocRCTAmount");
            filedName.Append(", '0000000000000' as TransFee,a.DocCur");
            tableName.AppendFormat("" + sboname + "ORDR a ");
            tableName.AppendFormat(" LEFT JOIN " + sboname + "OSLP c ON a.SlpCode = c.SlpCode");
            tableName.AppendFormat(" LEFT JOIN " + sboname + "OCRD d ON a.CardCode = d.CardCode");
            tableName.AppendFormat(" LEFT JOIN " + sboname + "OCTG e ON a.GroupNum = e.GroupNum");
            tableName.AppendFormat(" LEFT JOIN " + sboname + "OIDC  n ON a.Indicator=n.Code");
            //tableName.AppendFormat(" LEFT JOIN " + sboname + "OWOR w on w.Status!='C' AND a.docentry=w.originAbs");
            DataTable dt = SAPSelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), pageSize, pageIndex, orderName, filterQuery, out rowCounts);

            #region 给特定字段赋值（只能取自外挂）
            string bonustypeid = GetJobTypeByUrl("sales/SalesBonus.aspx");
            string bonusatypeid = GetJobTypeByUrl("sales/BonusAfterSales.aspx");
            string protypeid = GetJobTypeByUrl("product/ProductionOrder.aspx");
            string protypeid_cp = GetJobTypeByUrl("product/ProductionOrder_CP.aspx");
            string typeidstr = bonustypeid + "," + bonusatypeid + "," + protypeid + "," + protypeid_cp;
            foreach (DataRow ordrrow in dt.Rows)
            {
                ordrrow["bonusStatus"] = "";
                ordrrow["PrintNo"] = "";
                ordrrow["PrintNumIndex"] = 0;
                ordrrow["billStatus"] = "";
                ordrrow["proStatus"] = "";
                ordrrow["EmpAcctWarn"] = "";
                ordrrow["AttachFlag"] = "0";
                ordrrow["TransFee"] = "0.00";
                //string orderid = ordrrow["DocEntry"].ToString();
                //var dataTable = GetDataTableV2(orderid, nsapsboId.ToString(), typeidstr);
                //if (dataTable.Rows.Count > 0)
                //{

                //    if (!string.IsNullOrEmpty(dataTable.Rows[0].Field<string>("PrintNo")))
                //    {
                //        ordrrow["PrintNo"] = dataTable.Rows[0].Field<string>("PrintNo");
                //    }
                //    if (!string.IsNullOrEmpty(dataTable.Rows[0]["PrintNumIndex"].ToString()))
                //    {
                //        ordrrow["PrintNumIndex"] = dataTable.Rows[0]["PrintNumIndex"].ToString();
                //    }
                //    if (!string.IsNullOrEmpty(dataTable.Rows[0]["job_state"].ToString()))
                //    {
                //        ordrrow["bonusStatus"] = dataTable.Rows[0]["job_state"].ToString();
                //    }//发票、提成、生产状态
                //    if (!string.IsNullOrEmpty(dataTable.Rows[0]["billStatus"].ToString()))
                //    {
                //        ordrrow["billStatus"] = dataTable.Rows[0]["billStatus"].ToString();
                //    }
                //    else
                //    {
                //        ordrrow["billStatus"] = "";
                //    }
                //    if (!string.IsNullOrEmpty(dataTable.Rows[0]["Status"].ToString()))
                //    {
                //        ordrrow["proStatus"] = dataTable.Rows[0]["Status"].ToString();
                //    }
                //    else
                //    {
                //        ordrrow["proStatus"] = "";
                //    }
                //    if (string.IsNullOrEmpty(ordrrow["proStatus"].ToString()))
                //    {
                //        DataRow[] prorows = dataTable.Select("job_type_id=" + protypeid + " or job_type_id=" + protypeid_cp, "upd_dt desc");
                //        if (prorows.Length > 0)
                //        {
                //            ordrrow["proStatus"] = prorows[0]["job_state"].ToString();
                //        }
                //    }
                //    if (!string.IsNullOrEmpty(dataTable.Rows[0]["orinentry"].ToString()))
                //    {
                //        ordrrow["EmpAcctWarn"] = dataTable.Rows[0]["orinentry"].ToString();
                //    }
                //    else
                //    {
                //        ordrrow["EmpAcctWarn"] = "";
                //    }
                //    if (!string.IsNullOrEmpty(dataTable.Rows[0]["AttachFlag"].ToString()))
                //    {
                //        ordrrow["AttachFlag"] = dataTable.Rows[0]["AttachFlag"].ToString();
                //    }
                //    else
                //    {
                //        ordrrow["AttachFlag"] = "0";
                //    }
                //    //取运费
                //    if (!string.IsNullOrEmpty(dataTable.Rows[0]["TransFee"].ToString()))
                //    {
                //        ordrrow["TransFee"] = dataTable.Rows[0]["TransFee"].ToString();
                //    }
                //    else
                //    {
                //        ordrrow["TransFee"] = "0.00";
                //    }
                //}
                /*************************************************************************************/
                //var dataTable = GetDataTable(ordrrow, nsapsboId.ToString(), typeidstr);
                ////打印信息赋值
                //if (dataTable.Item1.Rows.Count > 0)
                //{
                //    if (!string.IsNullOrEmpty(dataTable.Item1.Rows[0].Field<string>("PrintNo")))
                //    {
                //        ordrrow["PrintNo"] = dataTable.Item1.Rows[0].Field<string>("PrintNo");
                //    }
                //    if (!string.IsNullOrEmpty(dataTable.Item1.Rows[0]["PrintNumIndex"].ToString()))
                //    {
                //        ordrrow["PrintNumIndex"] = dataTable.Item1.Rows[0]["PrintNumIndex"].ToString();
                //    }
                //    if (!string.IsNullOrEmpty(dataTable.Item1.Rows[0]["job_state"].ToString()))
                //    {
                //        ordrrow["bonusStatus"] = dataTable.Item1.Rows[0]["job_state"].ToString();
                //    }
                //}
                //else
                //{
                //    ordrrow["bonusStatus"] = "";
                //    ordrrow["PrintNo"] = "";
                //    ordrrow["PrintNumIndex"] = 0;
                //}
                ////发票、提成、生产状态
                //if (!string.IsNullOrEmpty(dataTable.Item2.Rows[0]["billStatus"].ToString()))
                //{
                //    ordrrow["billStatus"] = dataTable.Item2.Rows[0]["billStatus"].ToString();
                //}
                //else
                //{
                //    ordrrow["billStatus"] = "";
                //}
                //if (!string.IsNullOrEmpty(dataTable.Item2.Rows[0]["Status"].ToString()))
                //{
                //    ordrrow["proStatus"] = dataTable.Item2.Rows[0]["Status"].ToString();
                //}
                //else
                //{
                //    ordrrow["proStatus"] = "";
                //}
                //if (string.IsNullOrEmpty(ordrrow["proStatus"].ToString()))
                //{
                //    DataRow[] prorows = dataTable.Item1.Select("job_type_id=" + protypeid + " or job_type_id=" + protypeid_cp, "upd_dt desc");
                //    if (prorows.Length > 0)
                //    {
                //        ordrrow["proStatus"] = prorows[0]["job_state"].ToString();
                //    }
                //}
                //if (!string.IsNullOrEmpty(dataTable.Item2.Rows[0]["orinentry"].ToString()))
                //{
                //    ordrrow["EmpAcctWarn"] = dataTable.Item2.Rows[0]["orinentry"].ToString();
                //}
                //else
                //{
                //    ordrrow["EmpAcctWarn"] = "";
                //}
                //if (!string.IsNullOrEmpty(dataTable.Item2.Rows[0]["AttachFlag"].ToString()))
                //{
                //    ordrrow["AttachFlag"] = dataTable.Item2.Rows[0]["AttachFlag"].ToString();
                //}
                //else
                //{
                //    ordrrow["AttachFlag"] = "0";
                //}
                ////取运费
                //if (!string.IsNullOrEmpty(dataTable.Item2.Rows[0]["TransFee"].ToString()))
                //{
                //    ordrrow["TransFee"] = dataTable.Item2.Rows[0]["TransFee"].ToString();
                //}
                //else
                //{
                //    ordrrow["TransFee"] = "0.00";
                //}
                /*********************************************************************************/
                //    //打印信息赋值
                //    DataTable dtPrintnum = PurgetPrintNum("1", ordrrow["DocEntry"].ToString().Trim(), "ordr");//取编号
                //    if (dtPrintnum.Rows.Count > 0)
                //    {
                //        ordrrow["PrintNo"] = dtPrintnum.Rows[0][0].ToString();
                //        var aa = dtPrintnum.Rows[0][1].ToString();
                //        ordrrow["PrintNumIndex"] = dtPrintnum.Rows[0][1].ToString();
                //    }
                //    else
                //    {
                //        ordrrow["PrintNo"] = "";
                //        ordrrow["PrintNumIndex"] = 0;
                //    }
                //    //发票、提成、生产状态
                //    string orderid = ordrrow["DocEntry"].ToString();
                //    ordrrow["billStatus"] = GetBillStatusByOrderId(orderid, nsapsboId.ToString());
                //    DataTable jobtab = GetJobStateForDoc(orderid, typeidstr, nsapsboId.ToString());
                //    DataRow[] bonusrows = jobtab.Select("job_type_id=" + bonustypeid + " or job_type_id=" + bonusatypeid);
                //    ordrrow["bonusStatus"] = "";
                //    ordrrow["proStatus"] = GetProdStatusByOrderId(orderid, nsapsboId.ToString());
                //    if (bonusrows.Length > 0)
                //    {
                //        ordrrow["bonusStatus"] = bonusrows[0]["job_state"].ToString();
                //    }
                //    if (string.IsNullOrEmpty(ordrrow["proStatus"].ToString()))
                //    {
                //        DataRow[] prorows = jobtab.Select("job_type_id=" + protypeid + " or job_type_id=" + protypeid_cp, "upd_dt desc");
                //        if (prorows.Length > 0)
                //        {
                //            ordrrow["proStatus"] = prorows[0]["job_state"].ToString();
                //        }
                //    }
                //    ordrrow["EmpAcctWarn"] = GetEmptyAcctByOrderId(orderid, nsapsboId.ToString());
                //    //取销售合同类型附件
                //    string attTypeSql = string.Format("SELECT a.type_id FROM {0}.file_type a LEFT JOIN {1}.base_func b ON a.func_id=b.func_id LEFT JOIN {1}.base_page c ON c.page_id=b.page_id WHERE c.page_url=?page_url", "nsap_oa", "nsap_base");
                //    List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> sqlParameters = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
                //{
                //    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?page_url","sales/SalesOrder.aspx")
                //};
                //    object typeObj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, attTypeSql, CommandType.Text, sqlParameters);
                //    string fileType = typeObj == null ? "-1" : typeObj.ToString();

                //    string strSql2 = string.Format("SELECT 1 FROM {0}.file_main AS T0 ", "nsap_oa");
                //    strSql2 += string.Format("LEFT JOIN {0}.file_type AS T1 ON T0.file_type_id = T1.type_id ", "nsap_oa");
                //    strSql2 += string.Format("WHERE T0.file_type_id = {0} AND T0.docEntry = {1} limit 1", int.Parse(fileType), int.Parse(orderid));
                //    object fileflag = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql2, CommandType.Text, null);
                //    ordrrow["AttachFlag"] = fileflag == null ? "0" : fileflag.ToString();
                //    //取运费
                //    ordrrow["TransFee"] = GetSaleDeliveryYFByORDRID(orderid, nsapsboId.ToString());
            }

            #endregion
            return dt;
        }

        public (DataTable, DataTable) GetDataTable(DataRow dataRow, string sboid, string typeidstr)
        {
            string orderNo = dataRow["DocEntry"].ToString();
            //  sql1
            string sql1 = $@"
                SELECT  a.PrintNo,a.PrintNumIndex  ,b.job_id,b.job_type_id,b.job_state,upd_dt
                FROM nsap_bone.sale_ordr a,
                nsap_base.wfa_job b
                where a.DocEntry in( {orderNo}) and a.sbo_id ={sboid} AND
                b.job_state <> -1 and b.sbo_id  in( {orderNo})
                AND b.base_entry = {sboid}
                AND b.job_type_id IN({typeidstr}) order by b.upd_dt desc";
            DataTable dt1 = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql1, CommandType.Text, null);
            // sql2
            string sql2 = $@"SELECT 
                (select a.billstatus from nsap_bone.finance_billapplication_master a where a.DocEntry = {orderNo}  AND a.sbo_id={sboid} ORDER BY updatetime desc limit 1) billstatus,
                (select a.Status from nsap_bone.product_owor a where a.Status!='C' AND a.originAbs={orderNo}  AND a.sbo_id={sboid} limit 1) Status,
                (select group_concat(docentry) as orinentry from nsap_bone.sale_orin  where U_New_ORDRID = {orderNo}  AND sbo_id={sboid}) orinentry,
                (SELECT a.type_id FROM nsap_oa.file_type a LEFT JOIN nsap_base.base_func b ON a.func_id=b.func_id LEFT JOIN nsap_base.base_page c ON c.page_id=b.page_id WHERE c.page_url='sales/SalesOrder.aspx') type_id,
                (SELECT 1 as AttachFlag FROM nsap_oa.file_main AS T0 LEFT JOIN nsap_oa.file_type AS T1 ON T0.file_type_id = T1.type_id WHERE T0.file_type_id = 5 AND T0.docEntry = {orderNo}  limit 1) AttachFlag,
                (select sum(v1.doctotal) as TransFee from (
                                                                select DISTINCT t0.Buy_DocEntry,t1.DocTotal from nsap_bone.sale_transport t0
                                                                INNER JOIN nsap_bone.buy_opor t1 on t1.DocEntry = t0.Buy_DocEntry and t1.sbo_id = t0.SboId and t1.CANCELED = 'N'
                                                                INNER JOIN nsap_bone.sale_dln1 dl on dl.sbo_id=t0.sboid and dl.docentry=t0.base_docentry
                                                                WHERE t0.Base_DocType = 24 and  dl.basetype=17 and t0.SboId ={sboid} and dl.baseentry={orderNo} 
                                                                union all
												                select DISTINCT t0.Buy_DocEntry,t1.DocTotal from nsap_bone.sale_transport t0
                                                                INNER JOIN nsap_bone.buy_opor t1 on t1.DocEntry = t0.Buy_DocEntry and t1.sbo_id = t0.SboId and t1.CANCELED = 'N'
                                                                WHERE t0.Base_DocType = 17 and t0.SboId ={sboid} and t0.Base_DocEntry={orderNo} 
                                                            ) v1) TransFee
                ";
            DataTable dt2 = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql2, CommandType.Text, null);

            return (dt1, dt2);
        }

        public DataTable GetDataTableV2(string orderNo, string sboid, string typeidstr)
        {
            string sql = @$"SELECT * FROM (
                    SELECT
                    (select a.billstatus from nsap_bone.finance_billapplication_master a where a.DocEntry IN(1) AND a.sbo_id = {sboid} ORDER BY updatetime desc limit 1) billstatus,
                    (select a.Status from nsap_bone.product_owor a where a.Status != 'C' AND a.originAbs IN(1) AND a.sbo_id = {sboid} limit 1) Status,
                    (select group_concat(docentry) as orinentry from nsap_bone.sale_orin  where U_New_ORDRID= {orderNo} AND sbo_id = 1) orinentry,
                    (SELECT a.type_id FROM nsap_oa.file_type a LEFT JOIN nsap_base.base_func b ON a.func_id = b.func_id LEFT JOIN nsap_base.base_page c ON c.page_id = b.page_id WHERE c.page_url = 'sales/SalesOrder.aspx') type_id,
                    (SELECT 1 as AttachFlag FROM nsap_oa.file_main AS T0 LEFT JOIN nsap_oa.file_type AS T1 ON T0.file_type_id = T1.type_id WHERE T0.file_type_id = 5 AND T0.docEntry= {orderNo} limit 1) AttachFlag,
                    (select sum(v1.doctotal) as TransFee from(
                                                                    select DISTINCT t0.Buy_DocEntry, t1.DocTotal from nsap_bone.sale_transport t0
                                                                     INNER JOIN nsap_bone.buy_opor t1 on t1.DocEntry = t0.Buy_DocEntry and t1.sbo_id = t0.SboId and t1.CANCELED = 'N'
                                                                    INNER JOIN nsap_bone.sale_dln1 dl on dl.sbo_id = t0.sboid and dl.docentry = t0.base_docentry
                                                                    WHERE t0.Base_DocType = 24 and  dl.basetype = 17 and t0.SboId =  {sboid} and dl.baseentry= {orderNo}
                                                                    union all
                                                                    select DISTINCT t0.Buy_DocEntry, t1.DocTotal from nsap_bone.sale_transport t0
                                                                        INNER JOIN nsap_bone.buy_opor t1 on t1.DocEntry = t0.Buy_DocEntry and t1.sbo_id = t0.SboId and t1.CANCELED = 'N'
                                                                    WHERE t0.Base_DocType = 17 and t0.SboId = {sboid} and t0.Base_DocEntry= {orderNo}
                                                                ) v1) TransFee
                    ) a
                    LEFT JOIN(
                    SELECT  a.PrintNo, a.PrintNumIndex, b.job_id, b.job_type_id, b.job_state, upd_dt
                    FROM nsap_bone.sale_ordr a,
                    nsap_base.wfa_job b
                        where a.DocEntry = {orderNo} and a.sbo_id = 1 AND
                    b.job_state <> -1 and b.sbo_id = 1 AND b.base_entry = 1  AND(b.job_type_id = 95 or b.job_type_id = 96 or b.job_type_id = 45 or b.job_type_id = 112) order by b.upd_dt desc
                    ) b ON 1 = 1";
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
        }
        public string GetProdStatusByOrderId(string orderid, string sbo_id)
        {
            string strsql = string.Format("select a.Status from {0}.product_owor a where a.Status!='C' AND a.originAbs={1} AND a.sbo_id={2} limit 1", "nsap_bone", orderid, sbo_id);
            object res = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strsql, CommandType.Text, null);
            return res == null ? "" : res.ToString();
        }
        /// <summary>
        /// 得到销售订单对应交货运费
        /// </summary>
        /// <param name="ordrid">销售订单号</param>
        /// <param name="sboid"></param>
        /// <returns></returns>
        public string GetSaleDeliveryYFByORDRID(string ordrid, string sboid)
        {
            string strsql = string.Format(@"select sum(v1.doctotal) from (
                                                select DISTINCT t0.Buy_DocEntry,t1.DocTotal from {0}.sale_transport t0
                                                INNER JOIN {0}.buy_opor t1 on t1.DocEntry = t0.Buy_DocEntry and t1.sbo_id = t0.SboId and t1.CANCELED = 'N'
                                                INNER JOIN {0}.sale_dln1 dl on dl.sbo_id=t0.sboid and dl.docentry=t0.base_docentry
                                                WHERE t0.Base_DocType = 24 and  dl.basetype=17 and t0.SboId ={1} and dl.baseentry={2}
                                                union all
												select DISTINCT t0.Buy_DocEntry,t1.DocTotal from {0}.sale_transport t0
                                                INNER JOIN {0}.buy_opor t1 on t1.DocEntry = t0.Buy_DocEntry and t1.sbo_id = t0.SboId and t1.CANCELED = 'N'
                                                WHERE t0.Base_DocType = 17 and t0.SboId ={1} and t0.Base_DocEntry={2}
                                            ) v1", "nsap_bone", sboid, ordrid);
            object yfobj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strsql, CommandType.Text, null);
            return yfobj == null ? "0.00" : yfobj.ToString();
        }
        public string GetBaseEntrybyDocId(string docentry, string sboid, string linetable)
        {
            string sqlstr = string.Format("select baseentry from {0}.{1} where basetype>0 and sbo_id={2} and docentry={3} limit 1", "nsap_bone", linetable, sboid, docentry);
            object resultentry = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, sqlstr, CommandType.Text, null);
            return resultentry == null ? "0" : resultentry.ToString();
        }

        /// <summary>
        /// 获取业务伙伴的所有联系人
        /// </summary>
        public DataTable DropPopupCntctPrsn(string Code, int SboId)
        {
            string sql = string.Format("SELECT b.CntctCode AS id,b.Name AS name FROM  " + "nsap_bone" + ".crm_ocrd a LEFT JOIN  " + "nsap_bone" + ".crm_ocpr b ON a.CardCode=b.CardCode and a.sbo_id=b.sbo_id WHERE a.CardCode='{0}' and a.sbo_id={1}", Code, SboId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
        }
        /// <summary>
        /// 所属公司下拉列表
        /// </summary>
        /// <param name="sbo_id"></param>
        /// <returns></returns>
        public DataTable DropPopupIndicator(int sbo_id)
        {
            string strSql = string.Format(" SELECT Code as id,Name AS name FROM {0}.crm_oidc WHERE sbo_id={1}", "nsap_bone", sbo_id);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }
        /// <summary>
        /// 历史帐套
        /// </summary>
        /// <returns></returns>
        public DataTable DropPopupSboInfo()
        {
            string strSql = string.Format("SELECT sbo_id AS id,sbo_nm AS name FROM {0}.sbo_info", "nsap_base");
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }
        /// <summary>
        /// 付款条款
        /// </summary>
        /// <returns></returns>
        public DataTable DropPopupGroupNum(int sbo_id)
        {
            string strSql = string.Format(" SELECT GroupNum AS id,PymntGroup AS name FROM {0}.crm_octg WHERE sbo_id={1}", "nsap_bone", sbo_id);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }
        #endregion
        #region 合约评审生成PDF
        public string ContractExportShow_ForSale(string contractId, string host)
        {
            DataTable maintab = GetContractReviewInfo(contractId, "1", "1", true);
            if (maintab.Rows.Count > 0)
            {
                if (!string.IsNullOrEmpty(maintab.Rows[0]["PDF_FilePath_S"].ToString()))
                {
                    return host + maintab.Rows[0]["PDF_FilePath_S"].ToString();
                }
                string mbval = ""; mbval = "合约评审-CT.doc";
                List<FileHelper.WordTemplate> workMarks = new List<FileHelper.WordTemplate>();
                //页眉部分
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 1, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["contract_id"].ToString()) ? " " : maintab.Rows[0]["contract_id"].ToString() });//评审单号
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 1, YCellMark = 6, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["SlpName"].ToString()) ? " " : maintab.Rows[0]["SlpName"].ToString() }); //销售员
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 2, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["ApplyDate"].ToString()) ? " " : DateTime.Parse(maintab.Rows[0]["ApplyDate"].ToString()).ToShortDateString() }); //申请日期
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 3, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["DeliverDate"].ToString()) ? " " : DateTime.Parse(maintab.Rows[0]["DeliverDate"].ToString()).ToShortDateString() });//交货日期
                #region 基本信设置，属性
                //常规
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 2, ValueType = 0, ValueData = (string.IsNullOrEmpty(maintab.Rows[0]["ItemCode"].ToString()) ? " " : maintab.Rows[0]["ItemCode"].ToString()) + (maintab.Rows[0]["IsNew"].ToString() == "1" ? "(新)" : "") });//产品型号
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["ItemDesc"].ToString()) ? " " : maintab.Rows[0]["ItemDesc"].ToString() });//物料描述
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 2, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["CardCode"].ToString()) ? " " : maintab.Rows[0]["CardCode"].ToString() });//客户编号
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 2, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["CardName"].ToString()) ? " " : maintab.Rows[0]["CardName"].ToString() }); //客户名称 
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 2, YCellMark = 6, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["CustomType"].ToString()) ? " " : GetCustomFldDescrByValue("saleContractReview", "1", maintab.Rows[0]["CustomType"].ToString()) });//客户类型
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 3, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["CommRate"].ToString()) ? " " : double.Parse(maintab.Rows[0]["CommRate"].ToString()).ToString("0.00") });//提成比例
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 3, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["Wattage"].ToString()) ? " " : maintab.Rows[0]["Wattage"].ToString() }); //每瓦单价 
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 3, YCellMark = 6, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["UnitMsr"].ToString()) ? " " : maintab.Rows[0]["UnitMsr"].ToString() }); //计量单位
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 3, YCellMark = 8, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["U_JGF"].ToString()) ? " " : maintab.Rows[0]["U_JGF"].ToString() });//加工费 
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 3, YCellMark = 10, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["U_FDY"].ToString()) ? " " : maintab.Rows[0]["U_FDY"].ToString() });//负电源个数
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 4, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["Price"].ToString()) ? " " : double.Parse(maintab.Rows[0]["Price"].ToString()).ToString("0.00") });//单价 
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 4, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["Quantity"].ToString()) ? " " : maintab.Rows[0]["Quantity"].ToString() });//数量
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 4, YCellMark = 6, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["SumTotal"].ToString()) ? " " : double.Parse(maintab.Rows[0]["SumTotal"].ToString()).ToString("0.00") });//总计 
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 4, YCellMark = 8, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["CostTotal"].ToString()) ? " " : double.Parse(maintab.Rows[0]["CostTotal"].ToString()).ToString("0.00") });//总成本 
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 4, YCellMark = 10, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["Maori"].ToString()) ? " " : double.Parse(maintab.Rows[0]["Maori"].ToString()).ToString("0.00") });//毛利 
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 5, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["DetectionCapability"].ToString()) ? " " : maintab.Rows[0]["DetectionCapability"].ToString() });//检测能力
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 5, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["CustomReq"].ToString()) ? " " : maintab.Rows[0]["CustomReq"].ToString() });//客户特殊要求
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 6, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["Remarks"].ToString()) ? " " : maintab.Rows[0]["Remarks"].ToString() });//备注
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 7, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["ProjectDesc"].ToString()) ? " " : maintab.Rows[0]["ProjectDesc"].ToString() });//项目情况说明

                //属性-基本要求
                string CaseColor = "", CaseSilkPRT = "", MinDischargeVoltage = "", AgeingTime = "", MainCaseNo = "", RequiredPrecision = "", MiddleMachineType = "", AdditionalCaseNo = "";
                string WiringMethod = "", LanguageVer = "", DisOptions = "", MainSpecialRequire = "", MainOtherRequire = "", AirExhaustingMethod = "";
                //属性-配件要求
                string EthernetCable = "", PowerLine = "", PlugAdaptor = "", SwitchBoard = "", ChannelElectricCurrentLine = "", ChannelVoltageLine = "", ChannelThermalCouple = "", Pallet = "", FixturePanel = "", Gantry = "", CustomBatteryElevator = "", CustomAntiExplosionType = "";
                //属性-电池信息
                string CylindricalBatteryInfo_Size = "", CylindricalBatteryInfo_Struct = "", SoftPackingBatteryInfo_Size = "", SoftPackingBatteryInfo_Jtype = "", LiionBatteryInfo_Size = "", LiionBatteryInfo_Jtype = "", LiionBatteryInfo_struct = "";
                //配电选项，最低放电电压默认取主表字段
                switch (maintab.Rows[0]["DisOptions"].ToString())
                {
                    case "AC110":
                        DisOptions = "单相110V";
                        break;
                    case "AC220":
                        DisOptions = "单相220V";
                        break;
                    case "TC220":
                        DisOptions = "三相220V";
                        break;
                    case "AC380":
                        DisOptions = "三相380V";
                        break;
                    case "AC420":
                        DisOptions = "三相420V";
                        break;
                    default:
                        DisOptions = maintab.Rows[0]["DisOptions"].ToString();
                        break;
                }
                MinDischargeVoltage = maintab.Rows[0]["MinDischargeVoltage"].ToString() + "V";

                DataTable tempt = GetContractReviewProperty(contractId);
                if (tempt.Rows.Count > 0)
                {
                    string[] seprates = { "||" };
                    if (!object.Equals(tempt.Rows[0]["ProductProperty"], DBNull.Value))
                    {
                        byte[] thisPropbyte = (byte[])tempt.Rows[0]["ProductProperty"]; string[] valuearr;
                        if (tempt.Rows[0]["ProductType"].ToString() == "CT")
                        {
                            saleContractReview_CTPPE ctpro = DeSerialize<saleContractReview_CTPPE>(thisPropbyte);
                            #region 设备基本要求
                            if (!string.IsNullOrEmpty(ctpro.CaseColor))//机箱颜色
                            {
                                valuearr = ctpro.CaseColor.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    CaseColor = valuearr[1] + "色";
                                }
                                else
                                {
                                    CaseColor = "常规（暖灰）";
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.CaseSilkPrint))//机箱丝印
                            {
                                valuearr = ctpro.CaseSilkPrint.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    CaseSilkPRT = valuearr[1];
                                }
                                else
                                {
                                    CaseSilkPRT = "常规";
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.MinDischargeVoltage))//最低放电电压
                            {
                                valuearr = ctpro.MinDischargeVoltage.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    MinDischargeVoltage = valuearr[1] + "V";
                                }
                                else
                                {
                                    MinDischargeVoltage = valuearr[0] + "V";
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.AgeingTime))//设备老化时间
                            {
                                valuearr = ctpro.AgeingTime.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    AgeingTime = valuearr[1];
                                }
                                else
                                {
                                    switch (valuearr[0])
                                    {
                                        case "1":
                                            AgeingTime = "公司默认标准";
                                            break;
                                        case "2":
                                            AgeingTime = "1个星期";
                                            break;
                                        case "3":
                                            AgeingTime = "半个月";
                                            break;
                                        default:
                                            AgeingTime = valuearr[0];
                                            break;
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.MainCaseNo))
                            {
                                valuearr = ctpro.MainCaseNo.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    MainCaseNo = "要求从" + valuearr[1] + "箱号开始";
                                }
                                else
                                {
                                    MainCaseNo = "无要求";
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.RequiredPrecision))
                            {
                                switch (ctpro.RequiredPrecision)
                                {
                                    case "1":
                                        RequiredPrecision = "0.1%FS";
                                        break;
                                    case "2":
                                        RequiredPrecision = "0.05%FS";
                                        break;
                                    case "3":
                                        RequiredPrecision = "0.02%FS";
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.MiddleMachineType))
                            {
                                switch (ctpro.MiddleMachineType)
                                {
                                    case "1":
                                        MiddleMachineType = "CT-ZWJ-3'S";
                                        break;
                                    case "2":
                                        MiddleMachineType = "CT-ZWJ-4'S";
                                        break;
                                    case "3":
                                        MiddleMachineType = "内置";
                                        break;
                                    case "4":
                                        MiddleMachineType = "无（原先已配）";
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.AdditionalCaseNo))
                            {
                                valuearr = ctpro.AdditionalCaseNo.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    AdditionalCaseNo = "要求从" + valuearr[1] + "箱号开始";
                                }
                                else
                                {
                                    AdditionalCaseNo = "无要求";
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.WiringMethod))
                            {
                                valuearr = ctpro.WiringMethod.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    WiringMethod = valuearr[1];
                                }
                                else
                                {
                                    switch (valuearr[0])
                                    {
                                        case "1":
                                            WiringMethod = "设备前面出线（默认）";
                                            break;
                                        case "2":
                                            WiringMethod = "设备背部出线";
                                            break;
                                        case "3":
                                            WiringMethod = "设备底部出线";
                                            break;
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.LanguageVer))
                            {
                                switch (ctpro.LanguageVer)
                                {
                                    case "0":
                                        LanguageVer = "中文版";
                                        break;
                                    case "1":
                                        LanguageVer = "中英文版";
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.DisOptions))
                            {
                                valuearr = ctpro.DisOptions.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    DisOptions = valuearr[1] + "V";
                                    if (valuearr.Length > 2)
                                    {
                                        DisOptions += " " + valuearr[2];
                                    }
                                }
                                else
                                {
                                    switch (valuearr[0])
                                    {
                                        case "AC110":
                                            DisOptions = "单相110V";
                                            break;
                                        case "AC220":
                                            DisOptions = "单相220V";
                                            break;
                                        case "TC220":
                                            DisOptions = "三相220V";
                                            break;
                                        case "AC380":
                                            DisOptions = "三相380V";
                                            break;
                                        case "AC420":
                                            DisOptions = "三相420V";
                                            break;
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.MainSpecialRequire))
                            {
                                string[] specReqarr = ctpro.MainSpecialRequire.Split(',');
                                for (int i = 0; i < specReqarr.Length; i++)
                                {
                                    switch (specReqarr[i])
                                    {
                                        case "0":
                                            MainSpecialRequire += " 无";
                                            break;
                                        case "1":
                                            MainSpecialRequire += " 极性切换功能";
                                            break;
                                        case "2":
                                            MainSpecialRequire += " 恒压放电功能";
                                            break;
                                        case "3":
                                            MainSpecialRequire += " 带报警灯";
                                            break;
                                        case "4":
                                            MainSpecialRequire += " 无防反接功能";
                                            break;
                                        case "5":
                                            MainSpecialRequire += " CAN通讯";
                                            break;
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.MainOtherRequire))
                            {
                                MainOtherRequire = ctpro.MainOtherRequire;
                            }
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 8, YCellMark = 2, ValueType = 0, ValueData = CaseColor });//机箱颜色
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 8, YCellMark = 4, ValueType = 0, ValueData = CaseSilkPRT }); //机箱丝印
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 8, YCellMark = 6, ValueType = 0, ValueData = MinDischargeVoltage });//最低放电电压
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 8, YCellMark = 8, ValueType = 0, ValueData = AgeingTime }); //设备老化时间
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 9, YCellMark = 2, ValueType = 0, ValueData = MainCaseNo }); //主设备箱号
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 9, YCellMark = 4, ValueType = 0, ValueData = RequiredPrecision });//设备精度要求
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 9, YCellMark = 6, ValueType = 0, ValueData = MiddleMachineType }); //中位机
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 10, YCellMark = 2, ValueType = 0, ValueData = AdditionalCaseNo });//辅助设备箱号
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 10, YCellMark = 4, ValueType = 0, ValueData = WiringMethod }); //出线方式
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 10, YCellMark = 6, ValueType = 0, ValueData = LanguageVer });//中英文版本
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 11, YCellMark = 2, ValueType = 0, ValueData = DisOptions });//配电选项
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 11, YCellMark = 4, ValueType = 0, ValueData = MainSpecialRequire });//其它特殊要求
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 12, YCellMark = 2, ValueType = 0, ValueData = MainOtherRequire });//其它要求
                            #endregion
                            #region 辅助通道
                            if (ctpro.AddChannelFlag != null && ctpro.AddChannelFlag == "1")
                            {
                                StringBuilder addchlinfo = new StringBuilder();
                                addchlinfo.Append("类型:");
                                string dRow1value = " ";
                                if (!string.IsNullOrEmpty(ctpro.AdditionalChannelType))
                                {
                                    switch (ctpro.AdditionalChannelType)
                                    {
                                        case "0":
                                            dRow1value = "无";
                                            break;
                                        case "1":
                                            dRow1value = "CA-4008-1U-VT";
                                            break;
                                        case "2":
                                            dRow1value = "CA-4008-1U-VT-KX";
                                            break;
                                        case "3":
                                            dRow1value = "CA-4008-1U-VT-TX";
                                            break;
                                        case "4":
                                            dRow1value = "CA-5008-1U-VT";
                                            break;
                                    }
                                }
                                addchlinfo.Append(dRow1value);
                                addchlinfo.Append(" 电压:");
                                string dRow2value = " ";
                                if (!string.IsNullOrEmpty(ctpro.AdditionalChannelVoltage))
                                {
                                    switch (ctpro.AdditionalChannelVoltage)
                                    {
                                        case "0":
                                            dRow2value = "无";
                                            break;
                                        default:
                                            dRow2value = ctpro.AdditionalChannelVoltage + "V";
                                            break;
                                    }
                                }
                                addchlinfo.Append(dRow2value);
                                addchlinfo.Append(" 温度:");
                                string dRow3value = " ";
                                if (!string.IsNullOrEmpty(ctpro.AdditionalChannelTemperature))
                                {
                                    switch (ctpro.AdditionalChannelTemperature)
                                    {
                                        case "4S":
                                            dRow3value = "2K热敏电阻(4S)";
                                            break;
                                        case "3S":
                                            dRow3value = "10K热敏电阻(3S)";
                                            break;
                                        case "T":
                                            dRow3value = "T型热电偶";
                                            break;
                                        case "K":
                                            dRow3value = "K型热电偶";
                                            break;
                                        case "0":
                                            dRow3value = "无";
                                            break;
                                    }
                                }
                                addchlinfo.Append(dRow3value);
                                addchlinfo.Append(" 通道线长度:");
                                string dRow4value = " ";
                                if (!string.IsNullOrEmpty(ctpro.AdditionalChannelLinelength))
                                {
                                    valuearr = ctpro.AdditionalChannelLinelength.Split(seprates, StringSplitOptions.None);
                                    if (valuearr.Length > 1)
                                    {
                                        dRow4value = valuearr[1];
                                    }
                                    else
                                    {
                                        switch (valuearr[0])
                                        {
                                            case "1":
                                                dRow4value = "0.5米（常规3U）";
                                                break;
                                            case "2":
                                                dRow4value = "2米";
                                                break;
                                            case "3":
                                                dRow4value = "3米";
                                                break;
                                        }
                                    }
                                }
                                addchlinfo.Append(dRow4value);
                                addchlinfo.Append(" 电压夹具:");
                                string dRow5value = " ";
                                if (!string.IsNullOrEmpty(ctpro.AdditionalChannelFixture))
                                {
                                    valuearr = ctpro.AdditionalChannelFixture.Split(seprates, StringSplitOptions.None);
                                    switch (valuearr[0])
                                    {
                                        case "1":
                                            dRow5value = "鳄鱼夹";
                                            break;
                                        case "2":
                                            dRow5value = "无";
                                            break;
                                        case "0":
                                            dRow5value = "线鼻子";
                                            break;
                                    }
                                    if (valuearr.Length > 1)
                                    {
                                        dRow5value += valuearr[1] + "mm";
                                    }
                                }
                                addchlinfo.Append(dRow5value);
                                //添加到单元格中
                                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 13, YCellMark = 2, ValueType = 0, ValueData = addchlinfo.ToString() });//辅助通道信息
                            }
                            else
                            {
                                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 13, YCellMark = 2, ValueType = 0, ValueData = "无" });//无辅助通道
                            }
                            #endregion
                            #region 压床
                            if (ctpro.PressMacFlag != null && ctpro.PressMacFlag == "1")
                            {
                                StringBuilder pressmacinfo = new StringBuilder();
                                pressmacinfo.Append("压床类型:");
                                string dRow1value = " ";
                                if (!string.IsNullOrEmpty(ctpro.PressType))
                                {
                                    valuearr = ctpro.PressType.Split(seprates, StringSplitOptions.None);
                                    if (valuearr.Length > 1)
                                    {
                                        switch (valuearr[1])
                                        {
                                            case "3":
                                                dRow1value = "无";
                                                break;
                                            case "2":
                                                dRow1value = "全自动";
                                                break;
                                            case "1":
                                                dRow1value = "半自动";
                                                break;
                                        }
                                        pressmacinfo.Append(dRow1value);
                                    }
                                    else
                                    {
                                        pressmacinfo.Append("(" + valuearr[0] + ")");
                                    }
                                }
                                pressmacinfo.Append("   压床通讯方式:");
                                if (!string.IsNullOrEmpty(ctpro.PLCLine))
                                {
                                    switch (ctpro.PLCLine)
                                    {
                                        case "NO":
                                            pressmacinfo.Append("无");
                                            break;
                                        default:
                                            pressmacinfo.Append(ctpro.PLCLine);
                                            break;
                                    }
                                }
                                pressmacinfo.Append("   每层通道数:");
                                if (!string.IsNullOrEmpty(ctpro.ChannelsPerLayer))
                                {
                                    pressmacinfo.Append(ctpro.ChannelsPerLayer);
                                }
                                pressmacinfo.Append("   压床层数:");
                                if (!string.IsNullOrEmpty(ctpro.PressLayers))
                                {
                                    pressmacinfo.Append(ctpro.PressLayers);
                                }
                                pressmacinfo.Append("   单托盘通道数:");
                                if (!string.IsNullOrEmpty(ctpro.ChannelsPerPallet))
                                {
                                    pressmacinfo.Append(ctpro.ChannelsPerPallet);
                                }
                                pressmacinfo.Append("   单列通道数:");
                                if (!string.IsNullOrEmpty(ctpro.ChannelsPerList))
                                {
                                    pressmacinfo.Append(ctpro.ChannelsPerList);
                                }
                                if (!string.IsNullOrEmpty(ctpro.MESBISFlag))
                                {
                                    pressmacinfo.Append("   ");
                                    valuearr = ctpro.MESBISFlag.Split(seprates, StringSplitOptions.None);
                                    switch (valuearr[0])
                                    {
                                        case "0":
                                            break;
                                        case "NO":
                                            pressmacinfo.Append("无");
                                            break;
                                        default:
                                            pressmacinfo.Append(valuearr[0]);
                                            break;
                                    }
                                    if (valuearr.Length > 1)
                                    {
                                        pressmacinfo.Append(valuearr[1]);
                                    }
                                }
                                //添加到单元格中
                                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 14, YCellMark = 2, ValueType = 0, ValueData = pressmacinfo.ToString() });//压床信息
                            }
                            else
                            {
                                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 14, YCellMark = 2, ValueType = 0, ValueData = "无" });//无压床
                            }
                            #endregion
                            #region 高低温箱
                            if (ctpro.HLTempFlag != null && ctpro.HLTempFlag == "1")
                            {
                                StringBuilder hltempinfo = new StringBuilder();
                                hltempinfo.Append("通讯方式:");
                                if (!string.IsNullOrEmpty(ctpro.HTTemp_PLCLine))
                                {
                                    switch (ctpro.HTTemp_PLCLine)
                                    {
                                        case "NO":
                                            hltempinfo.Append("无");
                                            break;
                                        default:
                                            hltempinfo.Append(ctpro.PLCLine);
                                            break;
                                    }
                                }
                                hltempinfo.Append(" 体积:");
                                if (!string.IsNullOrEmpty(ctpro.HTTemp_Volume))
                                {
                                    hltempinfo.Append(ctpro.HTTemp_Volume);
                                }
                                hltempinfo.Append(" 供应商");
                                if (!string.IsNullOrEmpty(ctpro.HTTemp_Vendor))
                                {
                                    hltempinfo.Append(ctpro.HTTemp_Vendor);
                                }
                                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 15, YCellMark = 2, ValueType = 0, ValueData = hltempinfo.ToString() });//无高低温箱
                            }
                            else
                            {
                                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 15, YCellMark = 2, ValueType = 0, ValueData = "无" });//无高低温箱
                            }
                            #endregion
                            #region 配件要求
                            if (!string.IsNullOrEmpty(ctpro.EthernetCable))
                            {
                                valuearr = ctpro.EthernetCable.Split(seprates, StringSplitOptions.None);
                                switch (valuearr[0])
                                {
                                    case "1":
                                        EthernetCable = "0.25米";
                                        if (valuearr.Length > 1)
                                        {
                                            EthernetCable += valuearr[1] + "条";
                                        }
                                        break;
                                    case "2":
                                        EthernetCable = "0.5米";
                                        if (valuearr.Length > 1)
                                        {
                                            EthernetCable += valuearr[1] + "条";
                                        }
                                        break;
                                    case "3":
                                        EthernetCable = "3米";
                                        if (valuearr.Length > 1)
                                        {
                                            EthernetCable += valuearr[1] + "条";
                                        }
                                        break;
                                    case "4":
                                        EthernetCable = "5米";
                                        if (valuearr.Length > 1)
                                        {
                                            EthernetCable += valuearr[1] + "条";
                                        }
                                        break;
                                    case "0":
                                        if (valuearr.Length > 1)
                                        {
                                            EthernetCable += valuearr[1] + "米";
                                        }
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.PowerLine))
                            {
                                valuearr = ctpro.PowerLine.Split(seprates, StringSplitOptions.None);
                                switch (valuearr[0])
                                {
                                    case "1":
                                        PowerLine = "1*1";
                                        if (valuearr.Length > 1)
                                        {
                                            PowerLine += " " + valuearr[1] + "条";
                                        }
                                        if (valuearr.Length > 2)
                                        {
                                            string cver = "";
                                            switch (valuearr[2])
                                            {
                                                case "U":
                                                    cver = "欧式";
                                                    break;
                                                case "A":
                                                    cver = "美式";
                                                    break;
                                                case "E":
                                                    cver = "英式";
                                                    break;
                                            }
                                            PowerLine += "(" + cver + ")";
                                        }
                                        break;
                                    case "2":
                                        PowerLine = "1*5";
                                        if (valuearr.Length > 1)
                                        {
                                            PowerLine += " " + valuearr[1] + "条";
                                        }
                                        break;
                                    case "3":
                                        PowerLine = "1*6";
                                        if (valuearr.Length > 1)
                                        {
                                            PowerLine += " " + valuearr[1] + "条";
                                        }
                                        break;
                                    case "0":
                                        PowerLine = "设备自带";
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.PlugAdaptor))
                            {
                                valuearr = ctpro.PlugAdaptor.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    switch (valuearr[1])
                                    {
                                        case "U":
                                            PlugAdaptor = "欧式";
                                            break;
                                        case "A":
                                            PlugAdaptor = "美式";
                                            break;
                                        case "E":
                                            PlugAdaptor = "英式";
                                            break;
                                    }
                                }
                                if (!string.IsNullOrEmpty(valuearr[0]))
                                {
                                    PlugAdaptor += " " + valuearr[0] + "个";
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.SwitchBoard))
                            {
                                switch (ctpro.SwitchBoard)
                                {
                                    case "CP":
                                        SwitchBoard = "电脑";
                                        break;
                                    default:
                                        SwitchBoard = ctpro.SwitchBoard;
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.ChannelElectricCurrentLine))
                            {
                                valuearr = ctpro.ChannelElectricCurrentLine.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 2)
                                {
                                    switch (valuearr[1])
                                    {
                                        case "1":
                                            ChannelElectricCurrentLine = "线鼻子";
                                            if (!string.IsNullOrEmpty(valuearr[2]))
                                            {
                                                ChannelElectricCurrentLine += valuearr[2] + "mm";
                                            }
                                            break;
                                        case "2":
                                            ChannelElectricCurrentLine = "鳄鱼夹";
                                            if (!string.IsNullOrEmpty(valuearr[2]))
                                            {
                                                ChannelElectricCurrentLine += valuearr[2] + "A";
                                            }
                                            break;
                                        case "3":
                                            ChannelElectricCurrentLine = "聚合物夹具";
                                            if (!string.IsNullOrEmpty(valuearr[2]))
                                            {
                                                ChannelElectricCurrentLine += valuearr[2] + "A";
                                            }
                                            break;
                                        case "4":
                                            ChannelElectricCurrentLine = "顶针夹具";
                                            if (!string.IsNullOrEmpty(valuearr[2]))
                                            {
                                                ChannelElectricCurrentLine += valuearr[2] + "A";
                                            }
                                            break;
                                    }
                                }
                                if (!string.IsNullOrEmpty(valuearr[0]))
                                {
                                    ChannelElectricCurrentLine += " 长度" + valuearr[0] + "米";
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.ChannelVoltageLine))
                            {
                                valuearr = ctpro.ChannelVoltageLine.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 2)
                                {
                                    switch (valuearr[1])
                                    {
                                        case "1":
                                            ChannelVoltageLine = "线鼻子";
                                            if (!string.IsNullOrEmpty(valuearr[2]))
                                            {
                                                ChannelVoltageLine += valuearr[2] + "mm";
                                            }
                                            break;
                                        case "2":
                                            ChannelVoltageLine = "鳄鱼夹";
                                            if (!string.IsNullOrEmpty(valuearr[2]))
                                            {
                                                ChannelVoltageLine += valuearr[2] + "A";
                                            }
                                            break;
                                        case "3":
                                            ChannelVoltageLine = "聚合物夹具";
                                            if (!string.IsNullOrEmpty(valuearr[2]))
                                            {
                                                ChannelVoltageLine += valuearr[2] + "A";
                                            }
                                            break;
                                        case "4":
                                            ChannelVoltageLine = "顶针夹具";
                                            if (!string.IsNullOrEmpty(valuearr[2]))
                                            {
                                                ChannelVoltageLine += valuearr[2] + "A";
                                            }
                                            break;
                                    }
                                }
                                if (!string.IsNullOrEmpty(valuearr[0]))
                                {
                                    ChannelVoltageLine += " 长度" + valuearr[0] + "米";
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.Pallet))
                            {
                                valuearr = ctpro.Pallet.Split(seprates, StringSplitOptions.None);
                                switch (valuearr[0])
                                {
                                    case "1":
                                        Pallet = "19寸托盘";
                                        break;
                                    case "2":
                                        Pallet = "24寸托盘";
                                        break;
                                    case "0":
                                        if (valuearr.Length > 1)
                                        {
                                            Pallet = valuearr[1];
                                        }
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.FixturePanel))
                            {
                                switch (ctpro.FixturePanel)
                                {
                                    case "3":
                                        FixturePanel = "3U夹具面板";
                                        break;
                                    case "4":
                                        FixturePanel = "5U夹具面板";
                                        break;
                                    case "5":
                                        FixturePanel = "纽扣夹具";
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.Gantry))
                            {
                                valuearr = ctpro.Gantry.Split(seprates, StringSplitOptions.None);
                                switch (valuearr[0])
                                {
                                    case "1":
                                        Gantry = "A604-19\"-LMJ-n";
                                        break;
                                    case "2":
                                        Gantry = "A604-19\"-LMJ-m";
                                        break;
                                    case "0":
                                        if (valuearr.Length > 1)
                                        {
                                            Gantry = valuearr[1];
                                        }
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.CustomBatteryElevator))
                            {
                                CustomBatteryElevator = ctpro.CustomBatteryElevator;
                            }
                            if (!string.IsNullOrEmpty(ctpro.CustomAntiExplosionType))
                            {
                                CustomAntiExplosionType = ctpro.CustomAntiExplosionType;
                            }
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 16, YCellMark = 2, ValueType = 0, ValueData = EthernetCable });//网线
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 16, YCellMark = 4, ValueType = 0, ValueData = PowerLine });//电源线
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 16, YCellMark = 6, ValueType = 0, ValueData = PlugAdaptor });//转换插头
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 16, YCellMark = 8, ValueType = 0, ValueData = SwitchBoard });//交换机
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 17, YCellMark = 2, ValueType = 0, ValueData = Pallet });//托盘
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 17, YCellMark = 4, ValueType = 0, ValueData = ChannelElectricCurrentLine });//通道电流线
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 17, YCellMark = 6, ValueType = 0, ValueData = FixturePanel });//夹具面板
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 18, YCellMark = 2, ValueType = 0, ValueData = Gantry });//龙门架
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 18, YCellMark = 4, ValueType = 0, ValueData = ChannelVoltageLine });//通道电压线
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 19, YCellMark = 2, ValueType = 0, ValueData = CustomBatteryElevator });//定制电池架
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 19, YCellMark = 4, ValueType = 0, ValueData = CustomAntiExplosionType });//定制防爆箱
                            #endregion
                            #region 电池信息
                            if (!string.IsNullOrEmpty(ctpro.CylindricalBatteryInfo))
                            {
                                valuearr = ctpro.CylindricalBatteryInfo.Split(seprates, StringSplitOptions.None);
                                CylindricalBatteryInfo_Size = valuearr[0] + " " + valuearr[1];
                                CylindricalBatteryInfo_Struct = valuearr[2];
                            }
                            if (!string.IsNullOrEmpty(ctpro.SoftPackingBatteryInfo))
                            {
                                valuearr = ctpro.SoftPackingBatteryInfo.Split(seprates, StringSplitOptions.None);
                                SoftPackingBatteryInfo_Size = valuearr[0] + " " + valuearr[1];
                                if (valuearr.Length > 2)
                                {
                                    switch (valuearr[2])
                                    {
                                        case "1":
                                            SoftPackingBatteryInfo_Jtype = "两端出极耳";
                                            break;
                                        case "2":
                                            SoftPackingBatteryInfo_Jtype = "一端出极耳";
                                            break;
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(ctpro.LiionBatteryInfo))
                            {
                                valuearr = ctpro.LiionBatteryInfo.Split(seprates, StringSplitOptions.None);
                                LiionBatteryInfo_Size = valuearr[0] + " " + valuearr[1];
                                LiionBatteryInfo_struct = valuearr[2];
                                if (valuearr.Length > 2)
                                {
                                    switch (valuearr[2])
                                    {
                                        case "1":
                                            LiionBatteryInfo_Jtype = "两端出极耳";
                                            break;
                                        case "2":
                                            LiionBatteryInfo_Jtype = "一端出极耳";
                                            break;
                                    }
                                }
                            }
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 20, YCellMark = 3, ValueType = 0, ValueData = CylindricalBatteryInfo_Size });//圆柱电池-尺寸
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 20, YCellMark = 5, ValueType = 0, ValueData = CylindricalBatteryInfo_Struct });//圆柱电池-极柱结构
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 21, YCellMark = 3, ValueType = 0, ValueData = SoftPackingBatteryInfo_Size });//软包电池-尺寸
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 21, YCellMark = 4, ValueType = 0, ValueData = SoftPackingBatteryInfo_Jtype });//软包电池-极耳类型
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 22, YCellMark = 3, ValueType = 0, ValueData = LiionBatteryInfo_Size });//方形电池-尺寸
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 22, YCellMark = 4, ValueType = 0, ValueData = LiionBatteryInfo_Jtype });//方形电池-极耳类型
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 22, YCellMark = 6, ValueType = 0, ValueData = LiionBatteryInfo_struct });//方形电池-极柱结构
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 23, YCellMark = 2, ValueType = 0, ValueData = ctpro.ProductBatteryInfo });//方形电池-极柱结构
                            #endregion
                            #region 其它特殊要求
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 24, YCellMark = 2, ValueType = 0, ValueData = ctpro.CTSepecialRequire });//其它特殊要求
                            #endregion
                        }
                        else if (tempt.Rows[0]["ProductType"].ToString() == "CE")
                        {
                            saleContractReview_CEPPE cepro = DeSerialize<saleContractReview_CEPPE>(thisPropbyte);
                            mbval = "合约评审-CE.doc";
                            #region 设备基本信息
                            if (!string.IsNullOrEmpty(cepro.CaseColor))//机箱颜色
                            {
                                valuearr = cepro.CaseColor.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    CaseColor = valuearr[1] + "色";
                                }
                                else
                                {
                                    CaseColor = "常规（暖灰）";
                                }
                            }
                            if (!string.IsNullOrEmpty(cepro.CaseSilkPrint))//机箱丝印
                            {
                                valuearr = cepro.CaseSilkPrint.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    CaseSilkPRT = valuearr[1];
                                }
                                else
                                {
                                    CaseSilkPRT = "常规";
                                }
                            }
                            if (!string.IsNullOrEmpty(cepro.MinDischargeVoltage))//最低放电电压
                            {
                                valuearr = cepro.MinDischargeVoltage.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    MinDischargeVoltage = valuearr[1] + "V";
                                }
                                else
                                {
                                    MinDischargeVoltage = valuearr[0] + "V";
                                }
                            }
                            if (!string.IsNullOrEmpty(cepro.AgeingTime))//设备老化时间
                            {
                                AgeingTime = cepro.AgeingTime + "小时";

                            }
                            if (!string.IsNullOrEmpty(cepro.MainCaseNo))
                            {
                                valuearr = cepro.MainCaseNo.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    MainCaseNo = "要求从" + valuearr[1] + "箱号开始";
                                }
                                else
                                {
                                    MainCaseNo = "无要求";
                                }
                            }
                            if (!string.IsNullOrEmpty(cepro.RequiredPrecision))
                            {
                                switch (cepro.RequiredPrecision)
                                {
                                    case "1":
                                        RequiredPrecision = "0.1%FS";
                                        break;
                                    case "2":
                                        RequiredPrecision = "0.05%FS(温度25℃±5℃) ";
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(cepro.MiddleMachineType))
                            {
                                switch (cepro.MiddleMachineType)
                                {
                                    case "1":
                                        MiddleMachineType = "ZWJ-4S";
                                        break;
                                    case "2":
                                        MiddleMachineType = "ZWJ-3S";
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(cepro.AdditionalCaseNo))
                            {
                                valuearr = cepro.AdditionalCaseNo.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    AdditionalCaseNo = "要求从" + valuearr[1] + "箱号开始";
                                }
                                else
                                {
                                    AdditionalCaseNo = "无要求";
                                }
                            }
                            if (!string.IsNullOrEmpty(cepro.WiringMethod))
                            {
                                valuearr = cepro.WiringMethod.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    WiringMethod = valuearr[1];
                                }
                                else
                                {
                                    switch (valuearr[0])
                                    {
                                        case "1":
                                            WiringMethod = "设备前面出线";
                                            break;
                                        case "2":
                                            WiringMethod = "设备背部出线";
                                            break;
                                        case "3":
                                            WiringMethod = "设备底部出线";
                                            break;
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(cepro.LanguageVer))
                            {
                                switch (cepro.LanguageVer)
                                {
                                    case "0":
                                        LanguageVer = "中文版";
                                        break;
                                    case "1":
                                        LanguageVer = "英文版";
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(cepro.DisOptions))
                            {
                                valuearr = cepro.DisOptions.Split(seprates, StringSplitOptions.None);
                                switch (valuearr[0])
                                {
                                    case "AC110":
                                        DisOptions = "单相110V";
                                        break;
                                    case "AC220":
                                        DisOptions = "单相220V";
                                        break;
                                    case "TC220":
                                        DisOptions = "三相220V";
                                        break;
                                    case "AC380":
                                        DisOptions = "三相380V";
                                        break;
                                    case "AC420":
                                        DisOptions = "三相420V";
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(cepro.MainSpecialRequire))
                            {
                                string[] specReqarr = cepro.MainSpecialRequire.Split(',');
                                for (int i = 0; i < specReqarr.Length; i++)
                                {
                                    switch (specReqarr[i])
                                    {
                                        case "0":
                                            MainSpecialRequire += "无";
                                            break;
                                        case "1":
                                            MainSpecialRequire += "极性切换功能";
                                            break;
                                        case "2":
                                            MainSpecialRequire += "恒压放电功能";
                                            break;
                                        case "3":
                                            MainSpecialRequire += "带报警灯";
                                            break;
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(cepro.AirExhaustingMethod))
                            {
                                switch (cepro.AirExhaustingMethod)
                                {
                                    case "1":
                                        AirExhaustingMethod = "前后";
                                        break;
                                    case "2":
                                        AirExhaustingMethod = "向上";
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(cepro.MainOtherRequire))
                            {
                                MainOtherRequire = cepro.MainOtherRequire;
                            }
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 8, YCellMark = 2, ValueType = 0, ValueData = CaseColor });//机箱颜色
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 8, YCellMark = 4, ValueType = 0, ValueData = CaseSilkPRT }); //机箱丝印
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 8, YCellMark = 6, ValueType = 0, ValueData = MinDischargeVoltage });//最低放电电压
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 8, YCellMark = 8, ValueType = 0, ValueData = AgeingTime }); //设备老化时间
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 9, YCellMark = 2, ValueType = 0, ValueData = MainCaseNo }); //主设备箱号
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 9, YCellMark = 4, ValueType = 0, ValueData = RequiredPrecision });//设备精度要求
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 9, YCellMark = 6, ValueType = 0, ValueData = MiddleMachineType }); //中位机
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 10, YCellMark = 2, ValueType = 0, ValueData = AdditionalCaseNo });//辅助设备箱号
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 10, YCellMark = 4, ValueType = 0, ValueData = WiringMethod }); //出线方式
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 10, YCellMark = 6, ValueType = 0, ValueData = LanguageVer });//中英文版本
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 11, YCellMark = 2, ValueType = 0, ValueData = DisOptions });//配电选项
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 11, YCellMark = 4, ValueType = 0, ValueData = MainSpecialRequire });//其它特殊要求
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 11, YCellMark = 6, ValueType = 0, ValueData = AirExhaustingMethod });//排风方式
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 12, YCellMark = 2, ValueType = 0, ValueData = MainOtherRequire });//其它要求
                            #endregion
                            #region 辅助通道
                            if (cepro.AddChannelFlag != null && cepro.AddChannelFlag == "1")
                            {
                                StringBuilder addchlinfo = new StringBuilder();
                                addchlinfo.Append("类型:");
                                string dRow1value = " ";
                                if (!string.IsNullOrEmpty(cepro.AdditionalChannelType))
                                {
                                    switch (cepro.AdditionalChannelType)
                                    {
                                        case "0":
                                            dRow1value = "无";
                                            break;
                                        case "1":
                                            dRow1value = "CA-4008-1U-VT";
                                            break;
                                        case "2":
                                            dRow1value = "CA-4008-1U-VT-KX";
                                            break;
                                        case "3":
                                            dRow1value = "CA-4008-1U-VT-TX";
                                            break;
                                        case "4":
                                            dRow1value = "CA-5008-1U-VT";
                                            break;
                                    }
                                }
                                addchlinfo.Append(dRow1value);
                                addchlinfo.Append(" 电压:");
                                string dRow2value = " ";
                                if (!string.IsNullOrEmpty(cepro.AdditionalChannelVoltage))
                                {
                                    switch (cepro.AdditionalChannelVoltage)
                                    {
                                        case "0":
                                            dRow2value = "无";
                                            break;
                                        default:
                                            dRow2value = cepro.AdditionalChannelVoltage + "V";
                                            break;
                                    }
                                }
                                addchlinfo.Append(dRow2value);
                                addchlinfo.Append(" 温度:");
                                string dRow3value = " ";
                                if (!string.IsNullOrEmpty(cepro.AdditionalChannelTemperature))
                                {
                                    switch (cepro.AdditionalChannelTemperature)
                                    {
                                        case "4S":
                                            dRow3value = "2K热敏电阻(4S)";
                                            break;
                                        case "3S":
                                            dRow3value = "10K热敏电阻(3S)";
                                            break;
                                        case "T":
                                            dRow3value = "T型热电偶";
                                            break;
                                        case "K":
                                            dRow3value = "K型热电偶";
                                            break;
                                        case "0":
                                            dRow3value = "无";
                                            break;
                                    }
                                }
                                addchlinfo.Append(dRow3value);
                                addchlinfo.Append(" 通道线长度:");
                                string dRow4value = " ";
                                if (!string.IsNullOrEmpty(cepro.AdditionalChannelLinelength))
                                {
                                    valuearr = cepro.AdditionalChannelLinelength.Split(seprates, StringSplitOptions.None);
                                    if (valuearr.Length > 1)
                                    {
                                        dRow4value = valuearr[1];
                                    }
                                    else
                                    {
                                        switch (valuearr[0])
                                        {
                                            case "1":
                                                dRow4value = "0.5米（常规3U）";
                                                break;
                                            case "2":
                                                dRow4value = "2米";
                                                break;
                                            case "3":
                                                dRow4value = "3米";
                                                break;
                                        }
                                    }
                                }
                                addchlinfo.Append(dRow4value);
                                addchlinfo.Append(" 电压夹具:");
                                string dRow5value = " ";
                                if (!string.IsNullOrEmpty(cepro.AdditionalChannelFixture))
                                {
                                    valuearr = cepro.AdditionalChannelFixture.Split(seprates, StringSplitOptions.None);
                                    switch (valuearr[0])
                                    {
                                        case "1":
                                            dRow5value = "鳄鱼夹";
                                            break;
                                        case "2":
                                            dRow5value = "无";
                                            break;
                                        case "0":
                                            dRow5value = "线鼻子";
                                            break;
                                    }
                                    if (valuearr.Length > 1)
                                    {
                                        dRow5value += valuearr[1] + "mm";
                                    }
                                }
                                addchlinfo.Append(dRow5value);
                                //添加到单元格中
                                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 13, YCellMark = 2, ValueType = 0, ValueData = addchlinfo.ToString() });//辅助通道信息
                            }
                            else
                            {
                                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 13, YCellMark = 2, ValueType = 0, ValueData = "无" });//无辅助通道
                            }
                            #endregion
                            #region 压床
                            if (cepro.PressMacFlag != null && cepro.PressMacFlag == "1")
                            {
                                StringBuilder pressmacinfo = new StringBuilder();
                                pressmacinfo.Append("压床类型:");
                                string dRow1value = " ";
                                if (!string.IsNullOrEmpty(cepro.PressType))
                                {
                                    valuearr = cepro.PressType.Split(seprates, StringSplitOptions.None);
                                    if (valuearr.Length > 1)
                                    {
                                        switch (valuearr[1])
                                        {
                                            case "3":
                                                dRow1value = "无";
                                                break;
                                            case "2":
                                                dRow1value = "全自动";
                                                break;
                                            case "1":
                                                dRow1value = "半自动";
                                                break;
                                        }
                                        pressmacinfo.Append(dRow1value);
                                    }
                                    else
                                    {
                                        pressmacinfo.Append("(" + valuearr[0] + ")");
                                    }
                                }
                                pressmacinfo.Append("   压床通讯方式:");
                                if (!string.IsNullOrEmpty(cepro.PLCLine))
                                {
                                    switch (cepro.PLCLine)
                                    {
                                        case "NO":
                                            pressmacinfo.Append("无");
                                            break;
                                        default:
                                            pressmacinfo.Append(cepro.PLCLine);
                                            break;
                                    }
                                }
                                pressmacinfo.Append("   每层通道数:");
                                if (!string.IsNullOrEmpty(cepro.ChannelsPerLayer))
                                {
                                    pressmacinfo.Append(cepro.ChannelsPerLayer);
                                }
                                pressmacinfo.Append("   压床层数:");
                                if (!string.IsNullOrEmpty(cepro.PressLayers))
                                {
                                    pressmacinfo.Append(cepro.PressLayers);
                                }
                                pressmacinfo.Append("   单托盘通道数:");
                                if (!string.IsNullOrEmpty(cepro.ChannelsPerPallet))
                                {
                                    pressmacinfo.Append(cepro.ChannelsPerPallet);
                                }
                                pressmacinfo.Append("   单列通道数:");
                                if (!string.IsNullOrEmpty(cepro.ChannelsPerList))
                                {
                                    pressmacinfo.Append(cepro.ChannelsPerList);
                                }
                                if (!string.IsNullOrEmpty(cepro.MESBISFlag))
                                {
                                    pressmacinfo.Append("   ");
                                    valuearr = cepro.MESBISFlag.Split(seprates, StringSplitOptions.None);
                                    switch (valuearr[0])
                                    {
                                        case "0":
                                            break;
                                        case "NO":
                                            pressmacinfo.Append("无");
                                            break;
                                        default:
                                            pressmacinfo.Append(valuearr[0]);
                                            break;
                                    }
                                    if (valuearr.Length > 1)
                                    {
                                        pressmacinfo.Append(valuearr[1]);
                                    }
                                }
                                //添加到单元格中
                                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 14, YCellMark = 2, ValueType = 0, ValueData = pressmacinfo.ToString() });//压床信息
                            }
                            else
                            {
                                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 14, YCellMark = 2, ValueType = 0, ValueData = "无" });//无压床
                            }
                            #endregion
                            #region 高低温箱
                            if (cepro.HLTempFlag != null && cepro.HLTempFlag == "1")
                            {
                                StringBuilder hltempinfo = new StringBuilder();
                                hltempinfo.Append("通讯方式:");
                                if (!string.IsNullOrEmpty(cepro.HTTemp_PLCLine))
                                {
                                    switch (cepro.HTTemp_PLCLine)
                                    {
                                        case "NO":
                                            hltempinfo.Append("无");
                                            break;
                                        default:
                                            hltempinfo.Append(cepro.PLCLine);
                                            break;
                                    }
                                }
                                hltempinfo.Append(" 体积:");
                                if (!string.IsNullOrEmpty(cepro.HTTemp_Volume))
                                {
                                    hltempinfo.Append(cepro.HTTemp_Volume);
                                }
                                hltempinfo.Append(" 供应商");
                                if (!string.IsNullOrEmpty(cepro.HTTemp_Vendor))
                                {
                                    hltempinfo.Append(cepro.HTTemp_Vendor);
                                }
                                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 15, YCellMark = 2, ValueType = 0, ValueData = hltempinfo.ToString() });//无高低温箱
                            }
                            else
                            {
                                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 15, YCellMark = 2, ValueType = 0, ValueData = "无" });//无高低温箱
                            }
                            #endregion
                            #region 配件要求
                            if (!string.IsNullOrEmpty(cepro.EthernetCable))
                            {
                                EthernetCable = cepro.EthernetCable + "米";
                            }
                            if (!string.IsNullOrEmpty(cepro.PowerLine))
                            {
                                PowerLine = cepro.PowerLine + "米";
                            }

                            if (!string.IsNullOrEmpty(cepro.SwitchBoard))
                            {
                                switch (cepro.SwitchBoard)
                                {
                                    case "CP":
                                        SwitchBoard = "电脑";
                                        break;
                                    default:
                                        SwitchBoard = cepro.SwitchBoard;
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(cepro.ChannelElectricCurrentLine))
                            {
                                valuearr = cepro.ChannelElectricCurrentLine.Split(seprates, StringSplitOptions.None);
                                if (!string.IsNullOrEmpty(valuearr[0]))
                                {
                                    ChannelElectricCurrentLine += " 设备端线鼻子长度" + valuearr[0] + "mm";
                                }
                                if (valuearr.Length > 1)
                                {
                                    if (!string.IsNullOrEmpty(valuearr[1]))
                                    {
                                        ChannelElectricCurrentLine += " 压床端线鼻子" + valuearr[1] + "mm";
                                    }
                                }
                                if (valuearr.Length > 2)
                                {
                                    if (!string.IsNullOrEmpty(valuearr[2]))
                                    {
                                        ChannelElectricCurrentLine += " 长度" + valuearr[2] + "米";
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(cepro.ChannelVoltageLine))
                            {
                                valuearr = cepro.ChannelVoltageLine.Split(seprates, StringSplitOptions.None);
                                if (!string.IsNullOrEmpty(valuearr[0]))
                                {
                                    ChannelVoltageLine = "端子类型" + valuearr[0];
                                }
                                if (!string.IsNullOrEmpty(valuearr[1]))
                                {
                                    ChannelVoltageLine = "长度" + valuearr[1] + "米";
                                }
                            }
                            if (!string.IsNullOrEmpty(cepro.ChannelThermalCouple))
                            {
                                valuearr = cepro.ChannelThermalCouple.Split(seprates, StringSplitOptions.None);
                                if (!string.IsNullOrEmpty(valuearr[0]))
                                {
                                    ChannelThermalCouple = "端子类型" + valuearr[0];
                                }
                                if (!string.IsNullOrEmpty(valuearr[1]))
                                {
                                    ChannelThermalCouple = "长度" + valuearr[1] + "米";
                                }
                            }

                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 16, YCellMark = 2, ValueType = 0, ValueData = EthernetCable });//网线
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 16, YCellMark = 4, ValueType = 0, ValueData = PowerLine });//电源线
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 16, YCellMark = 6, ValueType = 0, ValueData = SwitchBoard });//交换机
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 16, YCellMark = 8, ValueType = 0, ValueData = ChannelThermalCouple });//通道温度线
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 17, YCellMark = 2, ValueType = 0, ValueData = ChannelElectricCurrentLine });//通道电流线
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 17, YCellMark = 4, ValueType = 0, ValueData = ChannelVoltageLine });//通道电压线

                            #endregion
                            #region 电池信息
                            if (!string.IsNullOrEmpty(cepro.CylindricalBatteryInfo))
                            {
                                valuearr = cepro.CylindricalBatteryInfo.Split(seprates, StringSplitOptions.None);
                                CylindricalBatteryInfo_Size = valuearr[0] + " " + valuearr[1];
                                CylindricalBatteryInfo_Struct = valuearr[2];
                            }
                            if (!string.IsNullOrEmpty(cepro.SoftPackingBatteryInfo))
                            {
                                valuearr = cepro.SoftPackingBatteryInfo.Split(seprates, StringSplitOptions.None);
                                SoftPackingBatteryInfo_Size = valuearr[0] + " " + valuearr[1];
                                if (valuearr.Length > 2)
                                {
                                    switch (valuearr[2])
                                    {
                                        case "1":
                                            SoftPackingBatteryInfo_Jtype = "两端出极耳";
                                            break;
                                        case "2":
                                            SoftPackingBatteryInfo_Jtype = "一端出极耳";
                                            break;
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(cepro.LiionBatteryInfo))
                            {
                                valuearr = cepro.LiionBatteryInfo.Split(seprates, StringSplitOptions.None);
                                LiionBatteryInfo_Size = valuearr[0] + " " + valuearr[1];
                                LiionBatteryInfo_struct = valuearr[2];
                                if (valuearr.Length > 2)
                                {
                                    switch (valuearr[2])
                                    {
                                        case "1":
                                            LiionBatteryInfo_Jtype = "两端出极耳";
                                            break;
                                        case "2":
                                            LiionBatteryInfo_Jtype = "一端出极耳";
                                            break;
                                    }
                                }
                            }
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 18, YCellMark = 3, ValueType = 0, ValueData = CylindricalBatteryInfo_Size });//圆柱电池-尺寸
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 18, YCellMark = 5, ValueType = 0, ValueData = CylindricalBatteryInfo_Struct });//圆柱电池-极柱结构
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 19, YCellMark = 3, ValueType = 0, ValueData = SoftPackingBatteryInfo_Size });//软包电池-尺寸
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 19, YCellMark = 4, ValueType = 0, ValueData = SoftPackingBatteryInfo_Jtype });//软包电池-极耳类型
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 20, YCellMark = 3, ValueType = 0, ValueData = LiionBatteryInfo_Size });//方形电池-尺寸
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 20, YCellMark = 4, ValueType = 0, ValueData = LiionBatteryInfo_Jtype });//方形电池-极耳类型
                            workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 20, YCellMark = 6, ValueType = 0, ValueData = LiionBatteryInfo_struct });//方形电池-极柱结构
                            #endregion
                        }
                    }
                }
                else
                {
                    //没有属性
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 8, YCellMark = 6, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["MinDischargeVoltage"].ToString()) ? " " : maintab.Rows[0]["MinDischargeVoltage"].ToString() });//设备最低放电电压
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 11, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(maintab.Rows[0]["DisOptions"].ToString()) ? " " : maintab.Rows[0]["DisOptions"].ToString() });//配电选项
                }
                #endregion
                #region BOM单
                DataTable bomtab = SelectContractReviewBomData(contractId, "1");
                //a.seq_id, b.ItemTypeName, a.ItemCode, c.ItemName, a.qty, a.UnitMsr,a.PurPrice, a.DocTotal, c.U_JGF AS MacPrice, IFNULL(c.U_JGF,0)*a.qty AS MacTotal,
                DataTable dTable = new DataTable();
                dTable.Columns.Add("C1", typeof(string));
                dTable.Columns.Add("C2", typeof(string));
                dTable.Columns.Add("C3", typeof(string));
                dTable.Columns.Add("C4", typeof(string));
                dTable.Columns.Add("C5", typeof(string));
                dTable.Columns.Add("C6", typeof(string));
                dTable.Columns.Add("C7", typeof(string));
                dTable.Columns.Add("C8", typeof(string));
                dTable.Columns.Add("C9", typeof(string));
                dTable.Columns.Add("C10", typeof(string));

                DataRow titlerow = dTable.NewRow();
                titlerow[0] = "#";
                titlerow[1] = "类别";
                titlerow[2] = "物料编码";
                titlerow[3] = "物料描述";
                titlerow[4] = "数量";
                titlerow[5] = "计量单位";
                titlerow[6] = "上一次采购价";
                titlerow[7] = "成本价";
                titlerow[8] = "加工单价";
                titlerow[9] = "加工费";
                dTable.Rows.Add(titlerow);
                int index = 1;
                foreach (DataRow tempRow in bomtab.Rows)
                {
                    DataRow newrow = dTable.NewRow();
                    newrow[0] = index.ToString();
                    newrow[1] = tempRow["ItemTypeName"];
                    newrow[2] = tempRow["ItemCode"];
                    newrow[3] = tempRow["ItemName"];
                    newrow[4] = string.IsNullOrEmpty(tempRow["qty"].ToString()) ? "" : double.Parse(tempRow["qty"].ToString()).ToString();
                    newrow[5] = tempRow["UnitMsr"];
                    newrow[6] = "******";//tempRow["PurPrice"];
                    newrow[7] = "******";//string.IsNullOrEmpty(tempRow["DocTotal"].ToString()) ? "" : double.Parse(tempRow["DocTotal"].ToString()).ToString("0.00");
                    newrow[8] = "******"; //tempRow["MacPrice"];
                    newrow[9] = "******";//string.IsNullOrEmpty(tempRow["MacTotal"].ToString()) ? "" : double.Parse(tempRow["MacTotal"].ToString()).ToString("0.00");
                    dTable.Rows.Add(newrow);
                    index++;
                }

                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 0, TableMark = 1, ValueType = 2, ValueData = dTable });

                #endregion
                string pdfName = string.Format("{0}.pdf", Guid.NewGuid().ToString());
                if (FileHelper.DOCTemplateToPDF(FileHelper.TempletFilePath.PhysicalPath + mbval, FileHelper.OrdersFilePath.PhysicalPath + pdfName, workMarks))
                {
                    //保存第一次PDF文件路径，避免每次生产新文件
                    SetPDFFilePathForContractReview(contractId, "PDF_FilePath_S", FileHelper.OrdersFilePath.VirtualPath + pdfName);
                    return host + FileHelper.OrdersFilePath.VirtualPath + pdfName;
                }
                else
                {
                    return "2";
                }
            }
            else
            {
                return "0";
            }
        }
        public async Task<byte[]> ContractExportShow_ForSaleNew(string sboid, string contractId)
        {
            DataTable maintab = GetContractReviewInfo(contractId, "1", "1", true);
            DataTable bomtab = SelectContractReviewBomData(contractId, "1");
            var logopath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "logo.png");
            var logostr = "";
            using (var fs = new FileStream(logopath, FileMode.Open))
            {
                var photo = new byte[fs.Length];
                fs.Position = 0;
                await fs.ReadAsync(photo, 0, photo.Length);
                logostr = Convert.ToBase64String(photo);
                Console.WriteLine(logostr);
            }
            var PrintForSale = new PrintForSale
            {
                contract_id = string.IsNullOrEmpty(maintab.Rows[0]["contract_id"].ToString()) ? " " : maintab.Rows[0]["contract_id"].ToString(),
                SlpName = string.IsNullOrEmpty(maintab.Rows[0]["SlpName"].ToString()) ? " " : maintab.Rows[0]["SlpName"].ToString(),
                ApplyDate = string.IsNullOrEmpty(maintab.Rows[0]["ApplyDate"].ToString()) ? " " : DateTime.Parse(maintab.Rows[0]["ApplyDate"].ToString()).ToShortDateString(),
                DeliverDate = string.IsNullOrEmpty(maintab.Rows[0]["DeliverDate"].ToString()) ? " " : DateTime.Parse(maintab.Rows[0]["DeliverDate"].ToString()).ToShortDateString(),
                ItemCode = (string.IsNullOrEmpty(maintab.Rows[0]["ItemCode"].ToString()) ? " " : maintab.Rows[0]["ItemCode"].ToString()) + (maintab.Rows[0]["IsNew"].ToString() == "1" ? "(新)" : ""),
                ItemDesc = string.IsNullOrEmpty(maintab.Rows[0]["ItemDesc"].ToString()) ? " " : maintab.Rows[0]["ItemDesc"].ToString(),
                CardCode = string.IsNullOrEmpty(maintab.Rows[0]["CardCode"].ToString()) ? " " : maintab.Rows[0]["CardCode"].ToString(),
                CardName = string.IsNullOrEmpty(maintab.Rows[0]["CardName"].ToString()) ? " " : maintab.Rows[0]["CardName"].ToString(),
                CustomType = string.IsNullOrEmpty(maintab.Rows[0]["CustomType"].ToString()) ? " " : GetCustomFldDescrByValue("saleContractReview", "1", maintab.Rows[0]["CustomType"].ToString()),
                CommRate = string.IsNullOrEmpty(maintab.Rows[0]["CommRate"].ToString()) ? " " : double.Parse(maintab.Rows[0]["CommRate"].ToString()).ToString("0.00"),
                Wattage = string.IsNullOrEmpty(maintab.Rows[0]["Wattage"].ToString()) ? " " : maintab.Rows[0]["Wattage"].ToString(),
                UnitMsr = string.IsNullOrEmpty(maintab.Rows[0]["UnitMsr"].ToString()) ? " " : maintab.Rows[0]["UnitMsr"].ToString(),
                U_JGF = string.IsNullOrEmpty(maintab.Rows[0]["U_JGF"].ToString()) ? " " : maintab.Rows[0]["U_JGF"].ToString(),
                U_FDY = string.IsNullOrEmpty(maintab.Rows[0]["U_FDY"].ToString()) ? " " : maintab.Rows[0]["U_FDY"].ToString(),
                Price = string.IsNullOrEmpty(maintab.Rows[0]["Price"].ToString()) ? " " : double.Parse(maintab.Rows[0]["Price"].ToString()).ToString("0.00"),
                Quantity = string.IsNullOrEmpty(maintab.Rows[0]["Quantity"].ToString()) ? " " : maintab.Rows[0]["Quantity"].ToString(),
                SumTotal = string.IsNullOrEmpty(maintab.Rows[0]["SumTotal"].ToString()) ? " " : double.Parse(maintab.Rows[0]["SumTotal"].ToString()).ToString("0.00"),
                CostTotal = string.IsNullOrEmpty(maintab.Rows[0]["Price"].ToString()) ? " " : double.Parse(maintab.Rows[0]["Price"].ToString()).ToString("0.00"),
                Maori = string.IsNullOrEmpty(maintab.Rows[0]["Price"].ToString()) ? " " : double.Parse(maintab.Rows[0]["Price"].ToString()).ToString("0.00"),
                DetectionCapability = string.IsNullOrEmpty(maintab.Rows[0]["DetectionCapability"].ToString()) ? " " : maintab.Rows[0]["DetectionCapability"].ToString(),
                CustomReq = string.IsNullOrEmpty(maintab.Rows[0]["CustomReq"].ToString()) ? " " : maintab.Rows[0]["CustomReq"].ToString(),
                Remarks = string.IsNullOrEmpty(maintab.Rows[0]["Remarks"].ToString()) ? " " : maintab.Rows[0]["Remarks"].ToString(),
                ProjectDesc = string.IsNullOrEmpty(maintab.Rows[0]["ProjectDesc"].ToString()) ? " " : maintab.Rows[0]["ProjectDesc"].ToString(),
                logo = logostr,
                QRcode = QRCoderHelper.CreateQRCodeToBase64(contractId),
                MinDischargeVoltage = maintab.Rows[0]["MinDischargeVoltage"].ToString() + "V",
                BomCost = new List<BomCost>()
            };
            switch (maintab.Rows[0]["DisOptions"].ToString())
            {
                case "AC110":
                    PrintForSale.DisOptions = "单相110V";
                    break;
                case "AC220":
                    PrintForSale.DisOptions = "单相220V";
                    break;
                case "TC220":
                    PrintForSale.DisOptions = "三相220V";
                    break;
                case "AC380":
                    PrintForSale.DisOptions = "三相380V";
                    break;
                case "AC420":
                    PrintForSale.DisOptions = "三相420V";
                    break;
                default:
                    PrintForSale.DisOptions = maintab.Rows[0]["DisOptions"].ToString();
                    break;
            }
            DataTable tempt = GetContractReviewProperty(contractId);
            string TemptPath = "";
            if (tempt.Rows.Count > 0)
            {
                string[] seprates = { "||" };
                if (!object.Equals(tempt.Rows[0]["ProductProperty"], DBNull.Value))
                {
                    byte[] thisPropbyte = (byte[])tempt.Rows[0]["ProductProperty"]; string[] valuearr;
                    if (tempt.Rows[0]["ProductType"].ToString() == "CT")
                    {
                        TemptPath = "PrintContractReview-CT.cshtml";
                        saleContractReview_CTPPE ctpro = DeSerialize<saleContractReview_CTPPE>(thisPropbyte);
                        #region 设备基本要求
                        if (!string.IsNullOrEmpty(ctpro.CaseColor))//机箱颜色
                        {
                            valuearr = ctpro.CaseColor.Split(seprates, StringSplitOptions.None);
                            if (valuearr.Length > 1)
                            {
                                PrintForSale.CaseColor = valuearr[1] + "色";
                            }
                            else
                            {
                                PrintForSale.CaseColor = "常规（暖灰）";
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.CaseSilkPrint))//机箱丝印
                        {
                            valuearr = ctpro.CaseSilkPrint.Split(seprates, StringSplitOptions.None);
                            if (valuearr.Length > 1)
                            {
                                PrintForSale.CaseSilkPRT = valuearr[1];
                            }
                            else
                            {
                                PrintForSale.CaseSilkPRT = "常规";
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.MinDischargeVoltage))//最低放电电压
                        {
                            valuearr = ctpro.MinDischargeVoltage.Split(seprates, StringSplitOptions.None);
                            if (valuearr.Length > 1)
                            {
                                PrintForSale.MinDischargeVoltage = valuearr[1] + "V";
                            }
                            else
                            {
                                PrintForSale.MinDischargeVoltage = valuearr[0] + "V";
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.AgeingTime))//设备老化时间
                        {
                            valuearr = ctpro.AgeingTime.Split(seprates, StringSplitOptions.None);
                            if (valuearr.Length > 1)
                            {
                                PrintForSale.AgeingTime = valuearr[1];
                            }
                            else
                            {
                                switch (valuearr[0])
                                {
                                    case "1":
                                        PrintForSale.AgeingTime = "公司默认标准";
                                        break;
                                    case "2":
                                        PrintForSale.AgeingTime = "1个星期";
                                        break;
                                    case "3":
                                        PrintForSale.AgeingTime = "半个月";
                                        break;
                                    default:
                                        PrintForSale.AgeingTime = valuearr[0];
                                        break;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.MainCaseNo))
                        {
                            valuearr = ctpro.MainCaseNo.Split(seprates, StringSplitOptions.None);
                            if (valuearr.Length > 1)
                            {
                                PrintForSale.MainCaseNo = "要求从" + valuearr[1] + "箱号开始";
                            }
                            else
                            {
                                PrintForSale.MainCaseNo = "无要求";
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.RequiredPrecision))
                        {
                            switch (ctpro.RequiredPrecision)
                            {
                                case "1":
                                    PrintForSale.RequiredPrecision = "0.1%FS";
                                    break;
                                case "2":
                                    PrintForSale.RequiredPrecision = "0.05%FS";
                                    break;
                                case "3":
                                    PrintForSale.RequiredPrecision = "0.02%FS";
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.MiddleMachineType))
                        {
                            switch (ctpro.MiddleMachineType)
                            {
                                case "1":
                                    PrintForSale.MiddleMachineType = "CT-ZWJ-3'S";
                                    break;
                                case "2":
                                    PrintForSale.MiddleMachineType = "CT-ZWJ-4'S";
                                    break;
                                case "3":
                                    PrintForSale.MiddleMachineType = "内置";
                                    break;
                                case "4":
                                    PrintForSale.MiddleMachineType = "无（原先已配）";
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.AdditionalCaseNo))
                        {
                            valuearr = ctpro.AdditionalCaseNo.Split(seprates, StringSplitOptions.None);
                            if (valuearr.Length > 1)
                            {
                                PrintForSale.AdditionalCaseNo = "要求从" + valuearr[1] + "箱号开始";
                            }
                            else
                            {
                                PrintForSale.AdditionalCaseNo = "无要求";
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.WiringMethod))
                        {
                            valuearr = ctpro.WiringMethod.Split(seprates, StringSplitOptions.None);
                            if (valuearr.Length > 1)
                            {
                                PrintForSale.WiringMethod = valuearr[1];
                            }
                            else
                            {
                                switch (valuearr[0])
                                {
                                    case "1":
                                        PrintForSale.WiringMethod = "设备前面出线（默认）";
                                        break;
                                    case "2":
                                        PrintForSale.WiringMethod = "设备背部出线";
                                        break;
                                    case "3":
                                        PrintForSale.WiringMethod = "设备底部出线";
                                        break;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.LanguageVer))
                        {
                            switch (ctpro.LanguageVer)
                            {
                                case "0":
                                    PrintForSale.LanguageVer = "中文版";
                                    break;
                                case "1":
                                    PrintForSale.LanguageVer = "中英文版";
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.DisOptions))
                        {
                            valuearr = ctpro.DisOptions.Split(seprates, StringSplitOptions.None);
                            if (valuearr.Length > 1)
                            {
                                PrintForSale.DisOptions = valuearr[1] + "V";
                                if (valuearr.Length > 2)
                                {
                                    PrintForSale.DisOptions += " " + valuearr[2];
                                }
                            }
                            else
                            {
                                switch (valuearr[0])
                                {
                                    case "AC110":
                                        PrintForSale.DisOptions = "单相110V";
                                        break;
                                    case "AC220":
                                        PrintForSale.DisOptions = "单相220V";
                                        break;
                                    case "TC220":
                                        PrintForSale.DisOptions = "三相220V";
                                        break;
                                    case "AC380":
                                        PrintForSale.DisOptions = "三相380V";
                                        break;
                                    case "AC420":
                                        PrintForSale.DisOptions = "三相420V";
                                        break;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.MainSpecialRequire))
                        {
                            string[] specReqarr = ctpro.MainSpecialRequire.Split(',');
                            for (int i = 0; i < specReqarr.Length; i++)
                            {
                                switch (specReqarr[i])
                                {
                                    case "0":
                                        PrintForSale.MainSpecialRequire += " 无";
                                        break;
                                    case "1":
                                        PrintForSale.MainSpecialRequire += " 极性切换功能";
                                        break;
                                    case "2":
                                        PrintForSale.MainSpecialRequire += " 恒压放电功能";
                                        break;
                                    case "3":
                                        PrintForSale.MainSpecialRequire += " 带报警灯";
                                        break;
                                    case "4":
                                        PrintForSale.MainSpecialRequire += " 无防反接功能";
                                        break;
                                    case "5":
                                        PrintForSale.MainSpecialRequire += " CAN通讯";
                                        break;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.MainOtherRequire))
                        {
                            PrintForSale.MainOtherRequire = ctpro.MainOtherRequire;
                        }

                        #endregion
                        #region 辅助通道
                        if (ctpro.AddChannelFlag != null && ctpro.AddChannelFlag == "1")
                        {
                            StringBuilder addchlinfo = new StringBuilder();
                            addchlinfo.Append("类型:");
                            string dRow1value = " ";
                            if (!string.IsNullOrEmpty(ctpro.AdditionalChannelType))
                            {
                                switch (ctpro.AdditionalChannelType)
                                {
                                    case "0":
                                        dRow1value = "无";
                                        break;
                                    case "1":
                                        dRow1value = "CA-4008-1U-VT";
                                        break;
                                    case "2":
                                        dRow1value = "CA-4008-1U-VT-KX";
                                        break;
                                    case "3":
                                        dRow1value = "CA-4008-1U-VT-TX";
                                        break;
                                    case "4":
                                        dRow1value = "CA-5008-1U-VT";
                                        break;
                                }
                            }
                            addchlinfo.Append(dRow1value);
                            addchlinfo.Append(" 电压:");
                            string dRow2value = " ";
                            if (!string.IsNullOrEmpty(ctpro.AdditionalChannelVoltage))
                            {
                                switch (ctpro.AdditionalChannelVoltage)
                                {
                                    case "0":
                                        dRow2value = "无";
                                        break;
                                    default:
                                        dRow2value = ctpro.AdditionalChannelVoltage + "V";
                                        break;
                                }
                            }
                            addchlinfo.Append(dRow2value);
                            addchlinfo.Append(" 温度:");
                            string dRow3value = " ";
                            if (!string.IsNullOrEmpty(ctpro.AdditionalChannelTemperature))
                            {
                                switch (ctpro.AdditionalChannelTemperature)
                                {
                                    case "4S":
                                        dRow3value = "2K热敏电阻(4S)";
                                        break;
                                    case "3S":
                                        dRow3value = "10K热敏电阻(3S)";
                                        break;
                                    case "T":
                                        dRow3value = "T型热电偶";
                                        break;
                                    case "K":
                                        dRow3value = "K型热电偶";
                                        break;
                                    case "0":
                                        dRow3value = "无";
                                        break;
                                }
                            }
                            addchlinfo.Append(dRow3value);
                            addchlinfo.Append(" 通道线长度:");
                            string dRow4value = " ";
                            if (!string.IsNullOrEmpty(ctpro.AdditionalChannelLinelength))
                            {
                                valuearr = ctpro.AdditionalChannelLinelength.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    dRow4value = valuearr[1];
                                }
                                else
                                {
                                    switch (valuearr[0])
                                    {
                                        case "1":
                                            dRow4value = "0.5米（常规3U）";
                                            break;
                                        case "2":
                                            dRow4value = "2米";
                                            break;
                                        case "3":
                                            dRow4value = "3米";
                                            break;
                                    }
                                }
                            }
                            addchlinfo.Append(dRow4value);
                            addchlinfo.Append(" 电压夹具:");
                            string dRow5value = " ";
                            if (!string.IsNullOrEmpty(ctpro.AdditionalChannelFixture))
                            {
                                valuearr = ctpro.AdditionalChannelFixture.Split(seprates, StringSplitOptions.None);
                                switch (valuearr[0])
                                {
                                    case "1":
                                        dRow5value = "鳄鱼夹";
                                        break;
                                    case "2":
                                        dRow5value = "无";
                                        break;
                                    case "0":
                                        dRow5value = "线鼻子";
                                        break;
                                }
                                if (valuearr.Length > 1)
                                {
                                    dRow5value += valuearr[1] + "mm";
                                }
                            }
                            addchlinfo.Append(dRow5value);
                            //添加到单元格中
                            PrintForSale.addchlinfo = addchlinfo.ToString();
                        }
                        else
                        {
                            PrintForSale.addchlinfo = "无";//无辅助通道
                        }
                        #endregion
                        #region 压床
                        if (ctpro.PressMacFlag != null && ctpro.PressMacFlag == "1")
                        {
                            StringBuilder pressmacinfo = new StringBuilder();
                            pressmacinfo.Append("压床类型:");
                            string dRow1value = " ";
                            if (!string.IsNullOrEmpty(ctpro.PressType))
                            {
                                valuearr = ctpro.PressType.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    switch (valuearr[1])
                                    {
                                        case "3":
                                            dRow1value = "无";
                                            break;
                                        case "2":
                                            dRow1value = "全自动";
                                            break;
                                        case "1":
                                            dRow1value = "半自动";
                                            break;
                                    }
                                    pressmacinfo.Append(dRow1value);
                                }
                                else
                                {
                                    pressmacinfo.Append("(" + valuearr[0] + ")");
                                }
                            }
                            pressmacinfo.Append("   压床通讯方式:");
                            if (!string.IsNullOrEmpty(ctpro.PLCLine))
                            {
                                switch (ctpro.PLCLine)
                                {
                                    case "NO":
                                        pressmacinfo.Append("无");
                                        break;
                                    default:
                                        pressmacinfo.Append(ctpro.PLCLine);
                                        break;
                                }
                            }
                            pressmacinfo.Append("   每层通道数:");
                            if (!string.IsNullOrEmpty(ctpro.ChannelsPerLayer))
                            {
                                pressmacinfo.Append(ctpro.ChannelsPerLayer);
                            }
                            pressmacinfo.Append("   压床层数:");
                            if (!string.IsNullOrEmpty(ctpro.PressLayers))
                            {
                                pressmacinfo.Append(ctpro.PressLayers);
                            }
                            pressmacinfo.Append("   单托盘通道数:");
                            if (!string.IsNullOrEmpty(ctpro.ChannelsPerPallet))
                            {
                                pressmacinfo.Append(ctpro.ChannelsPerPallet);
                            }
                            pressmacinfo.Append("   单列通道数:");
                            if (!string.IsNullOrEmpty(ctpro.ChannelsPerList))
                            {
                                pressmacinfo.Append(ctpro.ChannelsPerList);
                            }
                            if (!string.IsNullOrEmpty(ctpro.MESBISFlag))
                            {
                                pressmacinfo.Append("   ");
                                valuearr = ctpro.MESBISFlag.Split(seprates, StringSplitOptions.None);
                                switch (valuearr[0])
                                {
                                    case "0":
                                        break;
                                    case "NO":
                                        pressmacinfo.Append("无");
                                        break;
                                    default:
                                        pressmacinfo.Append(valuearr[0]);
                                        break;
                                }
                                if (valuearr.Length > 1)
                                {
                                    pressmacinfo.Append(valuearr[1]);
                                }
                            }
                            //添加到单元格中
                            PrintForSale.pressmacinfo = pressmacinfo.ToString();//压床信息
                        }
                        else
                        {
                            PrintForSale.pressmacinfo = "无";//无压床
                        }
                        #endregion
                        #region 高低温箱
                        if (ctpro.HLTempFlag != null && ctpro.HLTempFlag == "1")
                        {
                            StringBuilder hltempinfo = new StringBuilder();
                            hltempinfo.Append("通讯方式:");
                            if (!string.IsNullOrEmpty(ctpro.HTTemp_PLCLine))
                            {
                                switch (ctpro.HTTemp_PLCLine)
                                {
                                    case "NO":
                                        hltempinfo.Append("无");
                                        break;
                                    default:
                                        hltempinfo.Append(ctpro.PLCLine);
                                        break;
                                }
                            }
                            hltempinfo.Append(" 体积:");
                            if (!string.IsNullOrEmpty(ctpro.HTTemp_Volume))
                            {
                                hltempinfo.Append(ctpro.HTTemp_Volume);
                            }
                            hltempinfo.Append(" 供应商");
                            if (!string.IsNullOrEmpty(ctpro.HTTemp_Vendor))
                            {
                                hltempinfo.Append(ctpro.HTTemp_Vendor);
                            }
                            PrintForSale.hltempinfo = hltempinfo.ToString();//无高低温箱
                        }
                        else
                        {
                            PrintForSale.hltempinfo = "无";//无高低温箱
                        }
                        #endregion
                        #region 配件要求
                        if (!string.IsNullOrEmpty(ctpro.EthernetCable))
                        {
                            valuearr = ctpro.EthernetCable.Split(seprates, StringSplitOptions.None);
                            switch (valuearr[0])
                            {
                                case "1":
                                    PrintForSale.EthernetCable = "0.25米";
                                    if (valuearr.Length > 1)
                                    {
                                        PrintForSale.EthernetCable += valuearr[1] + "条";
                                    }
                                    break;
                                case "2":
                                    PrintForSale.EthernetCable = "0.5米";
                                    if (valuearr.Length > 1)
                                    {
                                        PrintForSale.EthernetCable += valuearr[1] + "条";
                                    }
                                    break;
                                case "3":
                                    PrintForSale.EthernetCable = "3米";
                                    if (valuearr.Length > 1)
                                    {
                                        PrintForSale.EthernetCable += valuearr[1] + "条";
                                    }
                                    break;
                                case "4":
                                    PrintForSale.EthernetCable = "5米";
                                    if (valuearr.Length > 1)
                                    {
                                        PrintForSale.EthernetCable += valuearr[1] + "条";
                                    }
                                    break;
                                case "0":
                                    if (valuearr.Length > 1)
                                    {
                                        PrintForSale.EthernetCable += valuearr[1] + "米";
                                    }
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.PowerLine))
                        {
                            valuearr = ctpro.PowerLine.Split(seprates, StringSplitOptions.None);
                            switch (valuearr[0])
                            {
                                case "1":
                                    PrintForSale.PowerLine = "1*1";
                                    if (valuearr.Length > 1)
                                    {
                                        PrintForSale.PowerLine += " " + valuearr[1] + "条";
                                    }
                                    if (valuearr.Length > 2)
                                    {
                                        string cver = "";
                                        switch (valuearr[2])
                                        {
                                            case "U":
                                                cver = "欧式";
                                                break;
                                            case "A":
                                                cver = "美式";
                                                break;
                                            case "E":
                                                cver = "英式";
                                                break;
                                        }
                                        PrintForSale.PowerLine += "(" + cver + ")";
                                    }
                                    break;
                                case "2":
                                    PrintForSale.PowerLine = "1*5";
                                    if (valuearr.Length > 1)
                                    {
                                        PrintForSale.PowerLine += " " + valuearr[1] + "条";
                                    }
                                    break;
                                case "3":
                                    PrintForSale.PowerLine = "1*6";
                                    if (valuearr.Length > 1)
                                    {
                                        PrintForSale.PowerLine += " " + valuearr[1] + "条";
                                    }
                                    break;
                                case "0":
                                    PrintForSale.PowerLine = "设备自带";
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.PlugAdaptor))
                        {
                            valuearr = ctpro.PlugAdaptor.Split(seprates, StringSplitOptions.None);
                            if (valuearr.Length > 1)
                            {
                                switch (valuearr[1])
                                {
                                    case "U":
                                        PrintForSale.PlugAdaptor = "欧式";
                                        break;
                                    case "A":
                                        PrintForSale.PlugAdaptor = "美式";
                                        break;
                                    case "E":
                                        PrintForSale.PlugAdaptor = "英式";
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(valuearr[0]))
                            {
                                PrintForSale.PlugAdaptor += " " + valuearr[0] + "个";
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.SwitchBoard))
                        {
                            switch (ctpro.SwitchBoard)
                            {
                                case "CP":
                                    PrintForSale.SwitchBoard = "电脑";
                                    break;
                                default:
                                    PrintForSale.SwitchBoard = ctpro.SwitchBoard;
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.ChannelElectricCurrentLine))
                        {
                            valuearr = ctpro.ChannelElectricCurrentLine.Split(seprates, StringSplitOptions.None);
                            if (valuearr.Length > 2)
                            {
                                switch (valuearr[1])
                                {
                                    case "1":
                                        PrintForSale.ChannelElectricCurrentLine = "线鼻子";
                                        if (!string.IsNullOrEmpty(valuearr[2]))
                                        {
                                            PrintForSale.ChannelElectricCurrentLine += valuearr[2] + "mm";
                                        }
                                        break;
                                    case "2":
                                        PrintForSale.ChannelElectricCurrentLine = "鳄鱼夹";
                                        if (!string.IsNullOrEmpty(valuearr[2]))
                                        {
                                            PrintForSale.ChannelElectricCurrentLine += valuearr[2] + "A";
                                        }
                                        break;
                                    case "3":
                                        PrintForSale.ChannelElectricCurrentLine = "聚合物夹具";
                                        if (!string.IsNullOrEmpty(valuearr[2]))
                                        {
                                            PrintForSale.ChannelElectricCurrentLine += valuearr[2] + "A";
                                        }
                                        break;
                                    case "4":
                                        PrintForSale.ChannelElectricCurrentLine = "顶针夹具";
                                        if (!string.IsNullOrEmpty(valuearr[2]))
                                        {
                                            PrintForSale.ChannelElectricCurrentLine += valuearr[2] + "A";
                                        }
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(valuearr[0]))
                            {
                                PrintForSale.ChannelElectricCurrentLine += " 长度" + valuearr[0] + "米";
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.ChannelVoltageLine))
                        {
                            valuearr = ctpro.ChannelVoltageLine.Split(seprates, StringSplitOptions.None);
                            if (valuearr.Length > 2)
                            {
                                switch (valuearr[1])
                                {
                                    case "1":
                                        PrintForSale.ChannelVoltageLine = "线鼻子";
                                        if (!string.IsNullOrEmpty(valuearr[2]))
                                        {
                                            PrintForSale.ChannelVoltageLine += valuearr[2] + "mm";
                                        }
                                        break;
                                    case "2":
                                        PrintForSale.ChannelVoltageLine = "鳄鱼夹";
                                        if (!string.IsNullOrEmpty(valuearr[2]))
                                        {
                                            PrintForSale.ChannelVoltageLine += valuearr[2] + "A";
                                        }
                                        break;
                                    case "3":
                                        PrintForSale.ChannelVoltageLine = "聚合物夹具";
                                        if (!string.IsNullOrEmpty(valuearr[2]))
                                        {
                                            PrintForSale.ChannelVoltageLine += valuearr[2] + "A";
                                        }
                                        break;
                                    case "4":
                                        PrintForSale.ChannelVoltageLine = "顶针夹具";
                                        if (!string.IsNullOrEmpty(valuearr[2]))
                                        {
                                            PrintForSale.ChannelVoltageLine += valuearr[2] + "A";
                                        }
                                        break;
                                }
                            }
                            if (!string.IsNullOrEmpty(valuearr[0]))
                            {
                                PrintForSale.ChannelVoltageLine += " 长度" + valuearr[0] + "米";
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.Pallet))
                        {
                            valuearr = ctpro.Pallet.Split(seprates, StringSplitOptions.None);
                            switch (valuearr[0])
                            {
                                case "1":
                                    PrintForSale.Pallet = "19寸托盘";
                                    break;
                                case "2":
                                    PrintForSale.Pallet = "24寸托盘";
                                    break;
                                case "0":
                                    if (valuearr.Length > 1)
                                    {
                                        PrintForSale.Pallet = valuearr[1];
                                    }
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.FixturePanel))
                        {
                            switch (ctpro.FixturePanel)
                            {
                                case "3":
                                    PrintForSale.FixturePanel = "3U夹具面板";
                                    break;
                                case "4":
                                    PrintForSale.FixturePanel = "5U夹具面板";
                                    break;
                                case "5":
                                    PrintForSale.FixturePanel = "纽扣夹具";
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.Gantry))
                        {
                            valuearr = ctpro.Gantry.Split(seprates, StringSplitOptions.None);
                            switch (valuearr[0])
                            {
                                case "1":
                                    PrintForSale.Gantry = "A604-19\"-LMJ-n";
                                    break;
                                case "2":
                                    PrintForSale.Gantry = "A604-19\"-LMJ-m";
                                    break;
                                case "0":
                                    if (valuearr.Length > 1)
                                    {
                                        PrintForSale.Gantry = valuearr[1];
                                    }
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.CustomBatteryElevator))
                        {
                            PrintForSale.CustomBatteryElevator = ctpro.CustomBatteryElevator;
                        }
                        if (!string.IsNullOrEmpty(ctpro.CustomAntiExplosionType))
                        {
                            PrintForSale.CustomAntiExplosionType = ctpro.CustomAntiExplosionType;
                        }
                        #endregion
                        #region 电池信息
                        if (!string.IsNullOrEmpty(ctpro.CylindricalBatteryInfo))
                        {
                            valuearr = ctpro.CylindricalBatteryInfo.Split(seprates, StringSplitOptions.None);
                            PrintForSale.CylindricalBatteryInfo_Size = valuearr[0] + " " + valuearr[1];
                            PrintForSale.CylindricalBatteryInfo_Struct = valuearr[2];
                        }
                        if (!string.IsNullOrEmpty(ctpro.SoftPackingBatteryInfo))
                        {
                            valuearr = ctpro.SoftPackingBatteryInfo.Split(seprates, StringSplitOptions.None);
                            PrintForSale.SoftPackingBatteryInfo_Size = valuearr[0] + " " + valuearr[1];
                            if (valuearr.Length > 2)
                            {
                                switch (valuearr[2])
                                {
                                    case "1":
                                        PrintForSale.SoftPackingBatteryInfo_Jtype = "两端出极耳";
                                        break;
                                    case "2":
                                        PrintForSale.SoftPackingBatteryInfo_Jtype = "一端出极耳";
                                        break;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(ctpro.LiionBatteryInfo))
                        {
                            valuearr = ctpro.LiionBatteryInfo.Split(seprates, StringSplitOptions.None);
                            PrintForSale.LiionBatteryInfo_Size = valuearr[0] + " " + valuearr[1];
                            PrintForSale.LiionBatteryInfo_struct = valuearr[2];
                            if (valuearr.Length > 2)
                            {
                                switch (valuearr[2])
                                {
                                    case "1":
                                        PrintForSale.LiionBatteryInfo_Jtype = "两端出极耳";
                                        break;
                                    case "2":
                                        PrintForSale.LiionBatteryInfo_Jtype = "一端出极耳";
                                        break;
                                }
                            }
                        }
                        #endregion
                        #region 其它特殊要求
                        PrintForSale.CTSepecialRequire = ctpro.CTSepecialRequire;//其它特殊要求
                        #endregion
                    }
                    else if (tempt.Rows[0]["ProductType"].ToString() == "CE")
                    {
                        TemptPath = "PrintContractReview-CE.cshtml";
                        saleContractReview_CEPPE cepro = DeSerialize<saleContractReview_CEPPE>(thisPropbyte);
                        #region 设备基本信息
                        if (!string.IsNullOrEmpty(cepro.CaseColor))//机箱颜色
                        {
                            valuearr = cepro.CaseColor.Split(seprates, StringSplitOptions.None);
                            if (valuearr.Length > 1)
                            {
                                PrintForSale.CaseColor = valuearr[1] + "色";
                            }
                            else
                            {
                                PrintForSale.CaseColor = "常规（暖灰）";
                            }
                        }
                        if (!string.IsNullOrEmpty(cepro.CaseSilkPrint))//机箱丝印
                        {
                            valuearr = cepro.CaseSilkPrint.Split(seprates, StringSplitOptions.None);
                            if (valuearr.Length > 1)
                            {
                                PrintForSale.CaseSilkPRT = valuearr[1];
                            }
                            else
                            {
                                PrintForSale.CaseSilkPRT = "常规";
                            }
                        }
                        if (!string.IsNullOrEmpty(cepro.MinDischargeVoltage))//最低放电电压
                        {
                            valuearr = cepro.MinDischargeVoltage.Split(seprates, StringSplitOptions.None);
                            if (valuearr.Length > 1)
                            {
                                PrintForSale.MinDischargeVoltage = valuearr[1] + "V";
                            }
                            else
                            {
                                PrintForSale.MinDischargeVoltage = valuearr[0] + "V";
                            }
                        }
                        if (!string.IsNullOrEmpty(cepro.AgeingTime))//设备老化时间
                        {
                            PrintForSale.AgeingTime = cepro.AgeingTime + "小时";

                        }
                        if (!string.IsNullOrEmpty(cepro.MainCaseNo))
                        {
                            valuearr = cepro.MainCaseNo.Split(seprates, StringSplitOptions.None);
                            if (valuearr.Length > 1)
                            {
                                PrintForSale.MainCaseNo = "要求从" + valuearr[1] + "箱号开始";
                            }
                            else
                            {
                                PrintForSale.MainCaseNo = "无要求";
                            }
                        }
                        if (!string.IsNullOrEmpty(cepro.RequiredPrecision))
                        {
                            switch (cepro.RequiredPrecision)
                            {
                                case "1":
                                    PrintForSale.RequiredPrecision = "0.1%FS";
                                    break;
                                case "2":
                                    PrintForSale.RequiredPrecision = "0.05%FS(温度25℃±5℃) ";
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(cepro.MiddleMachineType))
                        {
                            switch (cepro.MiddleMachineType)
                            {
                                case "1":
                                    PrintForSale.MiddleMachineType = "ZWJ-4S";
                                    break;
                                case "2":
                                    PrintForSale.MiddleMachineType = "ZWJ-3S";
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(cepro.AdditionalCaseNo))
                        {
                            valuearr = cepro.AdditionalCaseNo.Split(seprates, StringSplitOptions.None);
                            if (valuearr.Length > 1)
                            {
                                PrintForSale.AdditionalCaseNo = "要求从" + valuearr[1] + "箱号开始";
                            }
                            else
                            {
                                PrintForSale.AdditionalCaseNo = "无要求";
                            }
                        }
                        if (!string.IsNullOrEmpty(cepro.WiringMethod))
                        {
                            valuearr = cepro.WiringMethod.Split(seprates, StringSplitOptions.None);
                            if (valuearr.Length > 1)
                            {
                                PrintForSale.WiringMethod = valuearr[1];
                            }
                            else
                            {
                                switch (valuearr[0])
                                {
                                    case "1":
                                        PrintForSale.WiringMethod = "设备前面出线";
                                        break;
                                    case "2":
                                        PrintForSale.WiringMethod = "设备背部出线";
                                        break;
                                    case "3":
                                        PrintForSale.WiringMethod = "设备底部出线";
                                        break;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(cepro.LanguageVer))
                        {
                            switch (cepro.LanguageVer)
                            {
                                case "0":
                                    PrintForSale.LanguageVer = "中文版";
                                    break;
                                case "1":
                                    PrintForSale.LanguageVer = "英文版";
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(cepro.DisOptions))
                        {
                            valuearr = cepro.DisOptions.Split(seprates, StringSplitOptions.None);
                            switch (valuearr[0])
                            {
                                case "AC110":
                                    PrintForSale.DisOptions = "单相110V";
                                    break;
                                case "AC220":
                                    PrintForSale.DisOptions = "单相220V";
                                    break;
                                case "TC220":
                                    PrintForSale.DisOptions = "三相220V";
                                    break;
                                case "AC380":
                                    PrintForSale.DisOptions = "三相380V";
                                    break;
                                case "AC420":
                                    PrintForSale.DisOptions = "三相420V";
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(cepro.MainSpecialRequire))
                        {
                            string[] specReqarr = cepro.MainSpecialRequire.Split(',');
                            for (int i = 0; i < specReqarr.Length; i++)
                            {
                                switch (specReqarr[i])
                                {
                                    case "0":
                                        PrintForSale.MainSpecialRequire += "无";
                                        break;
                                    case "1":
                                        PrintForSale.MainSpecialRequire += "极性切换功能";
                                        break;
                                    case "2":
                                        PrintForSale.MainSpecialRequire += "恒压放电功能";
                                        break;
                                    case "3":
                                        PrintForSale.MainSpecialRequire += "带报警灯";
                                        break;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(cepro.AirExhaustingMethod))
                        {
                            switch (cepro.AirExhaustingMethod)
                            {
                                case "1":
                                    PrintForSale.AirExhaustingMethod = "前后";
                                    break;
                                case "2":
                                    PrintForSale.AirExhaustingMethod = "向上";
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(cepro.MainOtherRequire))
                        {
                            PrintForSale.MainOtherRequire = cepro.MainOtherRequire;
                        }

                        #endregion
                        #region 辅助通道
                        if (cepro.AddChannelFlag != null && cepro.AddChannelFlag == "1")
                        {
                            StringBuilder addchlinfo = new StringBuilder();
                            addchlinfo.Append("类型:");
                            string dRow1value = " ";
                            if (!string.IsNullOrEmpty(cepro.AdditionalChannelType))
                            {
                                switch (cepro.AdditionalChannelType)
                                {
                                    case "0":
                                        dRow1value = "无";
                                        break;
                                    case "1":
                                        dRow1value = "CA-4008-1U-VT";
                                        break;
                                    case "2":
                                        dRow1value = "CA-4008-1U-VT-KX";
                                        break;
                                    case "3":
                                        dRow1value = "CA-4008-1U-VT-TX";
                                        break;
                                    case "4":
                                        dRow1value = "CA-5008-1U-VT";
                                        break;
                                }
                            }
                            addchlinfo.Append(dRow1value);
                            addchlinfo.Append(" 电压:");
                            string dRow2value = " ";
                            if (!string.IsNullOrEmpty(cepro.AdditionalChannelVoltage))
                            {
                                switch (cepro.AdditionalChannelVoltage)
                                {
                                    case "0":
                                        dRow2value = "无";
                                        break;
                                    default:
                                        dRow2value = cepro.AdditionalChannelVoltage + "V";
                                        break;
                                }
                            }
                            addchlinfo.Append(dRow2value);
                            addchlinfo.Append(" 温度:");
                            string dRow3value = " ";
                            if (!string.IsNullOrEmpty(cepro.AdditionalChannelTemperature))
                            {
                                switch (cepro.AdditionalChannelTemperature)
                                {
                                    case "4S":
                                        dRow3value = "2K热敏电阻(4S)";
                                        break;
                                    case "3S":
                                        dRow3value = "10K热敏电阻(3S)";
                                        break;
                                    case "T":
                                        dRow3value = "T型热电偶";
                                        break;
                                    case "K":
                                        dRow3value = "K型热电偶";
                                        break;
                                    case "0":
                                        dRow3value = "无";
                                        break;
                                }
                            }
                            addchlinfo.Append(dRow3value);
                            addchlinfo.Append(" 通道线长度:");
                            string dRow4value = " ";
                            if (!string.IsNullOrEmpty(cepro.AdditionalChannelLinelength))
                            {
                                valuearr = cepro.AdditionalChannelLinelength.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    dRow4value = valuearr[1];
                                }
                                else
                                {
                                    switch (valuearr[0])
                                    {
                                        case "1":
                                            dRow4value = "0.5米（常规3U）";
                                            break;
                                        case "2":
                                            dRow4value = "2米";
                                            break;
                                        case "3":
                                            dRow4value = "3米";
                                            break;
                                    }
                                }
                            }
                            addchlinfo.Append(dRow4value);
                            addchlinfo.Append(" 电压夹具:");
                            string dRow5value = " ";
                            if (!string.IsNullOrEmpty(cepro.AdditionalChannelFixture))
                            {
                                valuearr = cepro.AdditionalChannelFixture.Split(seprates, StringSplitOptions.None);
                                switch (valuearr[0])
                                {
                                    case "1":
                                        dRow5value = "鳄鱼夹";
                                        break;
                                    case "2":
                                        dRow5value = "无";
                                        break;
                                    case "0":
                                        dRow5value = "线鼻子";
                                        break;
                                }
                                if (valuearr.Length > 1)
                                {
                                    dRow5value += valuearr[1] + "mm";
                                }
                            }
                            addchlinfo.Append(dRow5value);
                            //添加到单元格中
                            PrintForSale.addchlinfo = addchlinfo.ToString();//辅助通道信息
                        }
                        else
                        {
                            PrintForSale.addchlinfo = "无";//无辅助通道
                        }
                        #endregion
                        #region 压床
                        if (cepro.PressMacFlag != null && cepro.PressMacFlag == "1")
                        {
                            StringBuilder pressmacinfo = new StringBuilder();
                            pressmacinfo.Append("压床类型:");
                            string dRow1value = " ";
                            if (!string.IsNullOrEmpty(cepro.PressType))
                            {
                                valuearr = cepro.PressType.Split(seprates, StringSplitOptions.None);
                                if (valuearr.Length > 1)
                                {
                                    switch (valuearr[1])
                                    {
                                        case "3":
                                            dRow1value = "无";
                                            break;
                                        case "2":
                                            dRow1value = "全自动";
                                            break;
                                        case "1":
                                            dRow1value = "半自动";
                                            break;
                                    }
                                    pressmacinfo.Append(dRow1value);
                                }
                                else
                                {
                                    pressmacinfo.Append("(" + valuearr[0] + ")");
                                }
                            }
                            pressmacinfo.Append("   压床通讯方式:");
                            if (!string.IsNullOrEmpty(cepro.PLCLine))
                            {
                                switch (cepro.PLCLine)
                                {
                                    case "NO":
                                        pressmacinfo.Append("无");
                                        break;
                                    default:
                                        pressmacinfo.Append(cepro.PLCLine);
                                        break;
                                }
                            }
                            pressmacinfo.Append("   每层通道数:");
                            if (!string.IsNullOrEmpty(cepro.ChannelsPerLayer))
                            {
                                pressmacinfo.Append(cepro.ChannelsPerLayer);
                            }
                            pressmacinfo.Append("   压床层数:");
                            if (!string.IsNullOrEmpty(cepro.PressLayers))
                            {
                                pressmacinfo.Append(cepro.PressLayers);
                            }
                            pressmacinfo.Append("   单托盘通道数:");
                            if (!string.IsNullOrEmpty(cepro.ChannelsPerPallet))
                            {
                                pressmacinfo.Append(cepro.ChannelsPerPallet);
                            }
                            pressmacinfo.Append("   单列通道数:");
                            if (!string.IsNullOrEmpty(cepro.ChannelsPerList))
                            {
                                pressmacinfo.Append(cepro.ChannelsPerList);
                            }
                            if (!string.IsNullOrEmpty(cepro.MESBISFlag))
                            {
                                pressmacinfo.Append("   ");
                                valuearr = cepro.MESBISFlag.Split(seprates, StringSplitOptions.None);
                                switch (valuearr[0])
                                {
                                    case "0":
                                        break;
                                    case "NO":
                                        pressmacinfo.Append("无");
                                        break;
                                    default:
                                        pressmacinfo.Append(valuearr[0]);
                                        break;
                                }
                                if (valuearr.Length > 1)
                                {
                                    pressmacinfo.Append(valuearr[1]);
                                }
                            }
                            //添加到单元格中
                            PrintForSale.pressmacinfo = pressmacinfo.ToString();//压床信息
                        }
                        else
                        {
                            PrintForSale.pressmacinfo = "无";//无压床
                        }
                        #endregion
                        #region 高低温箱
                        if (cepro.HLTempFlag != null && cepro.HLTempFlag == "1")
                        {
                            StringBuilder hltempinfo = new StringBuilder();
                            hltempinfo.Append("通讯方式:");
                            if (!string.IsNullOrEmpty(cepro.HTTemp_PLCLine))
                            {
                                switch (cepro.HTTemp_PLCLine)
                                {
                                    case "NO":
                                        hltempinfo.Append("无");
                                        break;
                                    default:
                                        hltempinfo.Append(cepro.PLCLine);
                                        break;
                                }
                            }
                            hltempinfo.Append(" 体积:");
                            if (!string.IsNullOrEmpty(cepro.HTTemp_Volume))
                            {
                                hltempinfo.Append(cepro.HTTemp_Volume);
                            }
                            hltempinfo.Append(" 供应商");
                            if (!string.IsNullOrEmpty(cepro.HTTemp_Vendor))
                            {
                                hltempinfo.Append(cepro.HTTemp_Vendor);
                            }
                            PrintForSale.hltempinfo = hltempinfo.ToString();//无高低温箱
                        }
                        else
                        {
                            PrintForSale.hltempinfo = "无";//无高低温箱
                        }
                        #endregion
                        #region 配件要求
                        if (!string.IsNullOrEmpty(cepro.EthernetCable))
                        {
                            PrintForSale.EthernetCable = cepro.EthernetCable + "米";
                        }
                        if (!string.IsNullOrEmpty(cepro.PowerLine))
                        {
                            PrintForSale.PowerLine = cepro.PowerLine + "米";
                        }

                        if (!string.IsNullOrEmpty(cepro.SwitchBoard))
                        {
                            switch (cepro.SwitchBoard)
                            {
                                case "CP":
                                    PrintForSale.SwitchBoard = "电脑";
                                    break;
                                default:
                                    PrintForSale.SwitchBoard = cepro.SwitchBoard;
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(cepro.ChannelElectricCurrentLine))
                        {
                            valuearr = cepro.ChannelElectricCurrentLine.Split(seprates, StringSplitOptions.None);
                            if (!string.IsNullOrEmpty(valuearr[0]))
                            {
                                PrintForSale.ChannelElectricCurrentLine += " 设备端线鼻子长度" + valuearr[0] + "mm";
                            }
                            if (valuearr.Length > 1)
                            {
                                if (!string.IsNullOrEmpty(valuearr[1]))
                                {
                                    PrintForSale.ChannelElectricCurrentLine += " 压床端线鼻子" + valuearr[1] + "mm";
                                }
                            }
                            if (valuearr.Length > 2)
                            {
                                if (!string.IsNullOrEmpty(valuearr[2]))
                                {
                                    PrintForSale.ChannelElectricCurrentLine += " 长度" + valuearr[2] + "米";
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(cepro.ChannelVoltageLine))
                        {
                            valuearr = cepro.ChannelVoltageLine.Split(seprates, StringSplitOptions.None);
                            if (!string.IsNullOrEmpty(valuearr[0]))
                            {
                                PrintForSale.ChannelVoltageLine = "端子类型" + valuearr[0];
                            }
                            if (!string.IsNullOrEmpty(valuearr[1]))
                            {
                                PrintForSale.ChannelVoltageLine = "长度" + valuearr[1] + "米";
                            }
                        }
                        if (!string.IsNullOrEmpty(cepro.ChannelThermalCouple))
                        {
                            valuearr = cepro.ChannelThermalCouple.Split(seprates, StringSplitOptions.None);
                            if (!string.IsNullOrEmpty(valuearr[0]))
                            {
                                PrintForSale.ChannelThermalCouple = "端子类型" + valuearr[0];
                            }
                            if (!string.IsNullOrEmpty(valuearr[1]))
                            {
                                PrintForSale.ChannelThermalCouple = "长度" + valuearr[1] + "米";
                            }
                        }
                        #endregion
                        #region 电池信息
                        if (!string.IsNullOrEmpty(cepro.CylindricalBatteryInfo))
                        {
                            valuearr = cepro.CylindricalBatteryInfo.Split(seprates, StringSplitOptions.None);
                            PrintForSale.CylindricalBatteryInfo_Size = valuearr[0] + " " + valuearr[1];
                            PrintForSale.CylindricalBatteryInfo_Struct = valuearr[2];
                        }
                        if (!string.IsNullOrEmpty(cepro.SoftPackingBatteryInfo))
                        {
                            valuearr = cepro.SoftPackingBatteryInfo.Split(seprates, StringSplitOptions.None);
                            PrintForSale.SoftPackingBatteryInfo_Size = valuearr[0] + " " + valuearr[1];
                            if (valuearr.Length > 2)
                            {
                                switch (valuearr[2])
                                {
                                    case "1":
                                        PrintForSale.SoftPackingBatteryInfo_Jtype = "两端出极耳";
                                        break;
                                    case "2":
                                        PrintForSale.SoftPackingBatteryInfo_Jtype = "一端出极耳";
                                        break;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(cepro.LiionBatteryInfo))
                        {
                            valuearr = cepro.LiionBatteryInfo.Split(seprates, StringSplitOptions.None);
                            PrintForSale.LiionBatteryInfo_Size = valuearr[0] + " " + valuearr[1];
                            PrintForSale.LiionBatteryInfo_struct = valuearr[2];
                            if (valuearr.Length > 2)
                            {
                                switch (valuearr[2])
                                {
                                    case "1":
                                        PrintForSale.LiionBatteryInfo_Jtype = "两端出极耳";
                                        break;
                                    case "2":
                                        PrintForSale.LiionBatteryInfo_Jtype = "一端出极耳";
                                        break;
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
            foreach (DataRow tempRow in bomtab.Rows)
            {
                BomCost scon = new BomCost
                {
                    ItemTypeName = tempRow["ItemTypeName"].ToString(),
                    ItemCode = tempRow["ItemCode"].ToString(),
                    ItemName = tempRow["ItemName"].ToString(),
                    qty = string.IsNullOrEmpty(tempRow["qty"].ToString()) ? "" : double.Parse(tempRow["qty"].ToString()).ToString(),
                    UnitMsr = tempRow["UnitMsr"].ToString()
                };
                PrintForSale.BomCost.Add(scon);
            }
            return await ExportAllHandler.Exporterpdf(PrintForSale, TemptPath);

        }
        public bool SetPDFFilePathForContractReview(string ContractId, string fieldName, string PDFFilePath)
        {
            string sqlstr = string.Format(@"update {0}.sale_contract_review set {3}='{1}' where contract_id={2}", "nsap_bone", PDFFilePath, ContractId, fieldName);
            object theamount = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, sqlstr, CommandType.Text, null);
            return theamount == null ? false : (((decimal)theamount) > 0 ? true : false);
        }
        /// <summary>
        /// 查询合约评审 主表信息
        /// </summary>
        /// <returns></returns>
        public DataTable GetContractReviewInfo(string ContractId, string IsViewGross, string IsViewCostPrice, bool IsViewCustom)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT a.sbo_id, a.contract_id, {0}, a.ItemCode, c.ItemName AS ItemDesc, b.SlpName, a.UnitMsr, ", IsViewCustom ? "a.CardCode, a.CardName" : "'****' AS CardCode, '****' AS CardName");
            strSql.Append("a.is_new AS IsNew, a.comm_rate AS CommRate, a.walts AS Wattage, a.power_option AS DisOptions, a.min_discharge_volt AS MinDischargeVoltage, ");
            strSql.Append("a.custom_type AS CustomType, a.is_create_drawing_task AS IsCreateDrawingTask, a.custom_req AS CustomReq, a.detection_capability AS DetectionCapability, ");
            if (IsViewGross == "1")
            {
                strSql.Append("a.maori AS Maori, ");
            }
            else
            {
                strSql.Append("'*' AS Maori, ");
            }
            if (IsViewCostPrice == "1")
            {
                strSql.Append("a.cost_total AS CostTotal, ");
            }
            else
            {
                strSql.Append("'*' AS CostTotal, ");
            }
            strSql.Append("a.price AS Price, a.qty AS Quantity, a.sum_total AS SumTotal, a.apply_dt AS ApplyDate, a.deliver_dt AS DeliverDate, a.remarks AS Remarks, a.U_JGF,a.ProjectDesc,a.ProductType,c.U_FDY,a.PDF_FilePath,a.PDF_FilePath_S ");
            strSql.AppendFormat("FROM {0}.sale_contract_review a ", "nsap_bone");
            strSql.AppendFormat("LEFT JOIN {0}.crm_oslp b ON a.SlpCode=b.SlpCode AND a.sbo_id=b.sbo_id ", "nsap_bone");
            strSql.AppendFormat("LEFT JOIN {0}.store_oitm c ON a.itemcode=c.ItemCode AND a.sbo_id=c.sbo_id ", "nsap_bone");
            strSql.Append("WHERE a.contract_id=?ContractId");
            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> strPara = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?ContractId", ContractId),

            };
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, strPara);
        }

        public DataTable GetContractReviewProperty(string ContractId)
        {
            string sqlstr = string.Format("select a.ProductType,a.ProductProperty from {0}.sale_contract_review a where a.contract_id={1}", "nsap_bone", ContractId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sqlstr, CommandType.Text, null);

        }
        public string GetCustomFldDescrByValue(string TableID, string FieldID, string InValue)
        {
            string strSql = string.Format("SELECT Descr AS name FROM {0}.base_ufd1 WHERE TableID=?TableID AND FieldID=?FieldID and FldValue=?FldValue limit 1", "nsap_bone");
            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> strPara = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?TableID",TableID),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?FieldID",FieldID),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?FldValue",InValue)
            };
            object result = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, strPara);
            return result == null ? "" : result.ToString();
        }
        #endregion

        #region 查询合约评审 BOM单列表
        /// <summary>
        /// 查询合约评审 BOM单列表
        /// </summary>
        public DataTable SelectContractReviewBomData(string ContractId, string IsViewCostPrice)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT a.seq_id, b.ItemTypeName, a.ItemCode, c.ItemName, a.qty, a.UnitMsr, ");
            if (IsViewCostPrice == "1")
            {
                strSql.Append("a.PurPrice, a.DocTotal, c.U_JGF AS MacPrice, IFNULL(c.U_JGF,0)*a.qty AS MacTotal, ");
            }
            else
            {
                strSql.Append("'*' AS PurPrice, '*' AS DocTotal, '*' AS MacPrice, '*' AS MacTotal, ");
            }
            strSql.Append("'0' as IsNew, a.ItemTypeID ");
            strSql.AppendFormat("FROM {0}.sale_contract_review_detail a ", "nsap_bone");
            strSql.AppendFormat("LEFT JOIN {0}.store_itemtype b ON a.ItemTypeID=b.ItemTypeId ", "nsap_bone");
            strSql.AppendFormat("LEFT JOIN {0}.store_oitm c ON a.itemcode=c.ItemCode AND a.sbo_id=c.sbo_id ", "nsap_bone");
            strSql.Append("WHERE a.contract_id=?ContractId");
            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> strPara = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                 new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?ContractId", ContractId)
            };
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, strPara);
        }
        #endregion
        #region 销售订单生成PDF
        public string OrderExportShow(string sboid, string DocEntry, string Indicator, string host)
        {
            DataTable dtb = OrderExportView(sboid, DocEntry);
            if (dtb.Rows.Count > 0)
            {
                DataTable dtprint = GetPrintNo(sboid, DocEntry);
                string PrintNo = Guid.NewGuid().ToString();
                int PrintNumIndex = 0;
                if (dtprint.Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(dtprint.Rows[0][0].ToString()))
                    {
                        PrintNo = dtprint.Rows[0][0].ToString();
                    }
                    if (!string.IsNullOrEmpty(dtprint.Rows[0][1].ToString().Trim()))
                    {
                        PrintNumIndex = 1 + int.Parse(dtprint.Rows[0][1].ToString().Trim());
                    }
                    else
                    {
                        PrintNumIndex = 1;
                    }
                }
                else
                {
                    PrintNumIndex = 1;
                }

                if (UpdatePrintNo(sboid, DocEntry, PrintNo, PrintNumIndex.ToString()) == false)
                {
                    return "0";
                }
                string mbval = "";
                decimal docTotal = dtb.Rows[0][12].ToString() == "" ? 0 : decimal.Parse(dtb.Rows[0][12].ToString());
                decimal discSum = dtb.Rows[0]["DiscSum"].ToString() == "" ? 0 : decimal.Parse(dtb.Rows[0]["DiscSum"].ToString());
                decimal discSumFC = dtb.Rows[0]["DiscSumFC"].ToString() == "" ? 0 : decimal.Parse(dtb.Rows[0]["DiscSumFC"].ToString());
                decimal totalBefDisc = docTotal + discSum;
                DataTable dtNm = GetIndicators(string.IsNullOrEmpty(Indicator) ? " " : Indicator, sboid);
                if (dtNm.Rows.Count > 0)
                {
                    if (dtNm.Rows[0][1].ToString() == "01")
                    {
                        if (discSum > 0)
                        {
                            mbval = "销售合同-新威尔折扣.doc";
                        }
                        else
                        {
                            mbval = "销售合同-新威尔.doc";
                        }
                    }
                    else if (dtNm.Rows[0][1].ToString() == "02")
                    {
                        if (discSum > 0)
                        {
                            mbval = "销售合同-新能源折扣.doc";
                        }
                        else
                        {
                            mbval = "销售合同-新能源.doc";
                        }
                    }
                    else if (dtNm.Rows[0][1].ToString() == "05")
                    {
                        if (discSum > 0)
                        {
                            mbval = "销售合同-东莞新威折扣.doc";
                        }
                        else
                        {
                            mbval = "销售合同-东莞新威.doc";
                        }
                    }
                    else if (dtNm.Rows[0][1].ToString() == "07")
                    {
                        if (discSum > 0)
                        {
                            mbval = "销售合同-钮威折扣.doc";
                        }
                        else
                        {
                            mbval = "销售合同-钮威.doc";
                        }
                    }
                    else
                    {
                        return "3";
                    }
                }
                else
                {
                    if (discSum > 0)
                    {
                        mbval = "销售订单-现金折扣.doc";
                    }
                    else
                    {
                        mbval = "销售订单-现金.doc";
                    }
                }
                string jpgName = string.Format("{0}.jpg", Guid.NewGuid().ToString());
                string path = FileHelper.OrdersFilePath.PhysicalPath;
                QRCoderHelper.BuildBarcode(int.Parse(dtb.Rows[0][0].ToString()).ToString("d4"), FileHelper.OrdersFilePath.PhysicalPath + jpgName, 3);
                List<FileHelper.WordTemplate> workMarks = new List<FileHelper.WordTemplate>();
                string sb = dtb.Rows[0][15].ToString();
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 1, YCellMark = 5, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][0].ToString()) ? " " : dtb.Rows[0][0].ToString() });//单号
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 1, YCellMark = 6, ValueType = 1, ValueData = path + jpgName });//条形码
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 2, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][15].ToString()) ? " " : dtb.Rows[0][15].ToString() });//日期
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 3, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][9].ToString()) ? " " : dtb.Rows[0][9].ToString() });//销售员电话
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 1, XCellMark = 3, YCellMark = 5, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][8].ToString()) ? " " : dtb.Rows[0][8].ToString() });//销售员名称

                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][1].ToString()) ? " " : dtb.Rows[0][1].ToString() });//客户编号
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 2, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][7].ToString()) ? " " : dtb.Rows[0][7].ToString().Replace("<br>", "").Replace("\r", "") });//客户地址
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][2].ToString()) ? " " : dtb.Rows[0][2].ToString() });//客户名称
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 6, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][3].ToString()) ? " " : dtb.Rows[0][3].ToString() });//联系人
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 1, YCellMark = 8, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][4].ToString()) ? " " : dtb.Rows[0][4].ToString() });//电话
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 2, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][6].ToString()) ? " " : dtb.Rows[0][6].ToString() });//手机
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 2, XCellMark = 3, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][19].ToString()) ? " " : dtb.Rows[0][19].ToString().Replace("<br>", "").Replace("\r", "") });//交货地址



                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 1, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][11].ToString()) ? " " : dtb.Rows[0][11].ToString() });//付款条件
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 1, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][14].ToString().Trim()) ? " " : "" });//交货日期 Convert.ToDateTime(dtb.Rows[0][14].ToString().Trim()).ToString("yyyy.MM.dd") 
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 2, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][16].ToString()) ? " " : dtb.Rows[0][16].ToString() });//交货方式
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 2, YCellMark = 4, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][20].ToString()) ? " " : dtb.Rows[0][20].ToString() }); //验收期限
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 3, YCellMark = 2, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][10].ToString()) ? " " : dtb.Rows[0][10].ToString().Replace("<br>", "") });//备注
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 1, TableMark = 3, XCellMark = 4, YCellMark = 3, ValueType = 0, ValueData = string.IsNullOrEmpty(dtb.Rows[0][24].ToString()) ? " " : dtb.Rows[0][24].ToString() });//客户PO

                DataTable dTable = new DataTable();
                dTable.Columns.Add("C1", typeof(string));
                dTable.Columns.Add("C2", typeof(string));
                dTable.Columns.Add("C3", typeof(string));
                dTable.Columns.Add("C4", typeof(string));
                dTable.Columns.Add("C5", typeof(string));
                dTable.Columns.Add("C6", typeof(string));
                dTable.Columns.Add("C7", typeof(string));

                DataTable dtbs = OrderExportViews(sboid, DocEntry);
                string Currency = "";
                decimal totalall = 0;
                for (int i = 0; i < dtbs.Rows.Count; i++)
                {
                    DataRow dRow = dTable.NewRow();
                    if (Currency == "")
                    {
                        Currency = dtbs.Rows[i][7].ToString();
                    }
                    string r5d = Convert.ToDecimal(dtbs.Rows[i][5].ToString()).ToString("###,###.0000");
                    if (r5d == ".0000")
                    {
                        r5d = "0.0000";
                    }
                    string r6 = Convert.ToDecimal(dtbs.Rows[i][6].ToString()).ToString("###,###.00");
                    if (r6 == ".00")
                    {
                        r6 = "0.00";
                    }
                    totalall = totalall + decimal.Parse(r6);
                    dRow[0] = i + 1;
                    dRow[1] = string.IsNullOrEmpty(dtbs.Rows[i][1].ToString()) ? " " : dtbs.Rows[i][1].ToString(); //"CT-3008-5V5mA";物料编码
                    dRow[2] = string.IsNullOrEmpty(dtbs.Rows[i][2].ToString()) ? " " : dtbs.Rows[i][2].ToString();//"BTS-5V5mA-8通道-钢壳-四线扣式圆头夹具-3U19\"白色机箱";物料描述
                    dRow[3] = string.IsNullOrEmpty(dtbs.Rows[i][3].ToString()) ? " " : dtbs.Rows[i][3].ToString(); //"1";数量
                    dRow[4] = string.IsNullOrEmpty(dtbs.Rows[i][4].ToString()) ? " " : dtbs.Rows[i][4].ToString(); //"Pcs";单位
                    dRow[5] = string.IsNullOrEmpty(dtbs.Rows[i][5].ToString()) ? " " : r5d; //"3200.000000";单价
                    dRow[6] = string.IsNullOrEmpty(dtbs.Rows[i][6].ToString()) ? " " : r6; //"3200.00";金额

                    dTable.Rows.Add(dRow);
                }
                workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 0, TableMark = 1, ValueType = 2, ValueData = dTable });//明细数据

                if (mbval != "销售订单-现金.doc")
                {
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 2, TableMark = 1, XCellMark = 5, YCellMark = 4, ValueType = 0, ValueData = PrintNo });//打印编号                                                                                                                                                     
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 2, TableMark = 1, XCellMark = 5, YCellMark = 2, ValueType = 0, ValueData = "打印次数: " + PrintNumIndex });//打印次数 
                    if (discSum > 0)
                    {
                        workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 0, TableMark = 2, XCellMark = 3, YCellMark = 2, ValueType = 0, ValueData = "含税 " + dtb.Rows[0][23].ToString() });//含税
                        workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 0, TableMark = 2, XCellMark = 1, YCellMark = 4, ValueType = 0, ValueData = totalBefDisc > 0 ? "RMB " + totalBefDisc.ToString("###,###.00") : "" });//折扣前金额
                        workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 0, TableMark = 2, XCellMark = 2, YCellMark = 4, ValueType = 0, ValueData = discSum > 0 ? "RMB " + discSum.ToString("###,###.00") : "" });//折扣前金额
                        workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 0, TableMark = 2, XCellMark = 3, YCellMark = 4, ValueType = 0, ValueData = docTotal > 0 ? "RMB " + docTotal.ToString("###,###.00") : "" });//合计金额
                    }
                    else
                    {
                        workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 0, TableMark = 2, XCellMark = 1, YCellMark = 2, ValueType = 0, ValueData = "含税 " + dtb.Rows[0][23].ToString() });//含税
                        workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 0, TableMark = 2, XCellMark = 1, YCellMark = 4, ValueType = 0, ValueData = docTotal > 0 ? "RMB " + docTotal.ToString("###,###.00") : "" });//合计金额
                    }
                }
                else
                {
                    workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 2, TableMark = 1, XCellMark = 4, YCellMark = 4, ValueType = 0, ValueData = PrintNo });//打印编号                                                                                                                                                     
                                                                                                                                                                       //workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 2, TableMark = 1, XCellMark = 4, YCellMark = 2, ValueType = 0, ValueData = "打印次数: " + PrintNumIndex });//打印次数 
                    if (discSum > 0)
                    {
                        workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 0, TableMark = 2, XCellMark = 1, YCellMark = 4, ValueType = 0, ValueData = totalall == 0 ? "" : Currency + " " + totalall.ToString("###,###.00") });//合计金额
                        decimal totaldisc = discSum, totalamount = totalall;
                        if (Currency != "RMB")
                        {
                            totaldisc = discSumFC;
                            totalamount = totalall - totaldisc;
                        }
                        workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 0, TableMark = 2, XCellMark = 2, YCellMark = 4, ValueType = 0, ValueData = discSum > 0 ? Currency + " " + totaldisc.ToString("###,###.00") : "" });//折扣前金额
                        workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 0, TableMark = 2, XCellMark = 3, YCellMark = 4, ValueType = 0, ValueData = docTotal > 0 ? Currency + " " + totalamount.ToString("###,###.00") : "" });//合计金额
                    }
                    else
                    {
                        workMarks.Add(new FileHelper.WordTemplate() { MarkPosition = 0, TableMark = 2, XCellMark = 1, YCellMark = 3, ValueType = 0, ValueData = totalall == 0 ? "" : Currency + " " + totalall.ToString("###,###.00") });//合计金额
                    }
                }

                //string pdfName = string.Format("{0}.pdf", Guid.NewGuid().ToString());
                string pdfName = string.Format("SE-{0}.pdf", dtb.Rows[0][0].ToString());

                if (FileHelper.DOCTemplateToPDF(FileHelper.TempletFilePath.PhysicalPath + mbval, FileHelper.OrdersFilePath.PhysicalPath + pdfName, workMarks))
                {
                    return host + FileHelper.OrdersFilePath.VirtualPath + pdfName;
                }
                else
                {
                    return "2";
                }
            }
            else
            {
                return "0";
            }
        }
        /// <summary>
        /// 销售订单主数据导出
        /// </summary>
        public DataTable OrderExportView(string sboid, string DocEntry)
        {
            StringBuilder str = new StringBuilder();
            str.Append("SELECT distinct a.DocEntry,a.CardCode,a.CardName,b.Name,b.Tel1,b.Tel2,b.Cellolar,b.Address,c.SlpName,c.Memo,a.Comments,d.PymntGroup,");
            str.Append(" a.DocTotal,CONCAT(a.DocCur,' ',ROUND(IF(a.DocCur = 'RMB',a.DocTotal,IFNUll(a.DocTotalFC,0.000000)),2)) as curtotal ,DATE_FORMAT(a.DocDueDate,'%Y.%m.%d %H:%i') as DocDueDate,DATE_FORMAT(a.DocDate,'%Y.%m.%d') as DocDate,a.U_ShipName,b.Fax,a.U_YGMD,a.Address2,a.U_YSQX,a.BnkAccount,f.HouseBank,CONCAT(ROUND(a.U_SL,0),'%')U_SL,a.NumAtCard ");
            str.Append(" ,a.DiscSum,a.DiscSumFC,a.DocTotalFC,a.DocCur, DATE_FORMAT(k.log_dt,'%Y.%m.%d') as logdt, a.Indicator,ROUND(IF(a.DocCur = 'RMB',a.DiscSum,(a.DiscSum/a.DocRate)),2) as DiscSumT,ROUND(a.DiscPrcnt,2) as DiscPrcnt ");
            str.AppendFormat(" FROM {0}.sale_ordr a ", "nsap_bone");
            str.AppendFormat(" left join {0}.crm_ocpr b on a.CntctCode=b.CntctCode and a.sbo_id=b.sbo_id and a.CardCode=b.CardCode ", "nsap_bone");
            str.AppendFormat(" left join {0}.crm_oslp c on a.SlpCode=c.SlpCode and a.sbo_id=c.sbo_id ", "nsap_bone");
            str.AppendFormat(" left join {0}.crm_octg d on a.GroupNum=d.GroupNum AND a.sbo_id=d.sbo_id ", "nsap_bone");
            str.AppendFormat(" left join {0}.crm_ocrd f on a.DocEntry=f.CardCode and a.sbo_id=f.sbo_id ", "nsap_bone");
            str.AppendFormat(" left join (select h.sbo_id,h.sbo_itf_return,DATE_FORMAT(g.log_dt,'%Y.%m.%d') as log_dt ");
            str.AppendFormat(" from {0}.wfa_job h ", "nsap_base");
            str.AppendFormat(" left join {0}.wfa_log g on h.job_id= g.job_id ", "nsap_base");
            str.AppendFormat(" where h.sbo_itf_return = {0} and h.job_nm = '销售订单' order by g.log_dt desc limit 1) k on a.sbo_id = k.sbo_id and a.DocEntry = k.sbo_itf_return", DocEntry);
            str.AppendFormat(" where a.DocEntry={0} and a.sbo_id={1} ", DocEntry, sboid);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, str.ToString(), CommandType.Text, null);
        }
        /// <summary>
        ///获取打印编号
        /// </summary>
        public DataTable GetPrintNo(string SboId, string DocEntry)
        {
            string PrintSql = string.Format("SELECT PrintNo,PrintNumIndex FROM {0}.sale_ordr WHERE sbo_id = {1} AND DocEntry = {2}", "nsap_bone", int.Parse(SboId), int.Parse(DocEntry));
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, PrintSql.ToString(), CommandType.Text, null);

        }
        /// <summary>
        ///更新打印编号
        /// </summary>
        public bool UpdatePrintNo(string SboId, string DocEntry, string PrintNo, string PrintNumIndex)
        {
            string PrintSql = string.Format("UPDATE {0}.sale_ordr SET PrintNo='{1}',PrintNumIndex={4} WHERE DocEntry={2} and sbo_id={3} ", "nsap_bone", PrintNo, int.Parse(DocEntry), int.Parse(SboId), PrintNumIndex);
            bool result = false;
            result = UnitWork.ExecuteSql(PrintSql, ContextType.NsapBaseDbContext) > 0 ? true : false;
            return result;
        }
        /// <summary>
        /// 获取销售订单内部标识
        /// </summary>
        public DataTable GetIndicators(string Indicator, string sbo_id)
        {
            string strSql = string.Format(@"SELECT a.Name,a.Code FROM {0}.crm_oidc a
		                                    LEFT JOIN {0}.sale_ordr b ON a.Code=b.Indicator AND a.sbo_id=b.sbo_id
		                                    WHERE b.Indicator='{1}' AND b.sbo_id={2} ", "nsap_bone", Indicator, sbo_id);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }
        /// <summary>
        /// 销售订单行明细数据导出
        /// </summary>
        public DataTable OrderExportViews(string sboid, string DocEntry)
        {
            StringBuilder str = new StringBuilder();
            str.Append(" SELECT  ROW_NUMBER() OVER (ORDER BY b.sbo_id) RowNum, b.sbo_id,b.ItemCode,b.Dscription,ROUND(b.Quantity,2),b.unitMsr,ROUND(b.Price,6),ROUND(b.Quantity*b.Price,2),b.Currency ");
            str.AppendFormat(" from {0}.sale_rdr1  b ", "nsap_bone");
            str.AppendFormat(" LEFT JOIN {0}.sale_ordr a on b.DocEntry=a.DocEntry and b.sbo_id=a.sbo_id ", "nsap_bone");
            str.AppendFormat(" where a.DocEntry={0} and a.sbo_id={1} ", DocEntry, sboid);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, str.ToString(), CommandType.Text, null);
        }

        /// <summary>
        /// 销售订单总价
        /// </summary>
        public DataTable OrderExportTotalAmount(string sboid, string DocEntry)
        {
            StringBuilder str = new StringBuilder();
            str.Append(" SELECT  CONCAT(c.Currency,' ', ROUND( SUM( c.Quantity * c.Price ), 2 )) AS TotalAmount FROM (");
            str.Append(" SELECT  ROW_NUMBER() OVER (ORDER BY b.sbo_id) RowNum, b.sbo_id,b.ItemCode,b.Currency,b.Dscription,ROUND(b.Quantity,2) AS Quantity,b.unitMsr,ROUND(b.Price,6) AS Price,ROUND(b.Quantity*b.Price,2) ");
            str.AppendFormat(" from {0}.sale_rdr1  b ", "nsap_bone");
            str.AppendFormat(" LEFT JOIN {0}.sale_ordr a on b.DocEntry=a.DocEntry and b.sbo_id=a.sbo_id ", "nsap_bone");
            str.AppendFormat(" where a.DocEntry={0} and a.sbo_id={1} ) c", DocEntry, sboid);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, str.ToString(), CommandType.Text, null);
        }
        #endregion

        public DataTable GetCKCountsByCK(string itemCode, string whsCode, string sboId)
        {
            string sqlstr = string.Format(@"SELECT t1.sbo_id,t1.ItemCode,t1.ItemName,t1.OnHand,t1.IsCommited,t1.OnOrder,(t1.OnHand-t1.IsCommited+t1.OnOrder) as OnAvailable
                                        , t2.OnHand as whsOnHand,t2.IsCommited as whsIsCommited,t2.OnOrder as whsOnOrder,(t2.OnHand - t2.IsCommited + t2.OnOrder) as whsOnAvailable
                                        from {0}.store_oitm t1
                                        LEFT OUTER JOIN {0}.store_oitw t2 on t2.ItemCode = t1.ItemCode and t2.sbo_id = t1.sbo_Id and t2.whsCode = '{1}'
                                        where t1.sbo_id = {2} and t1.ItemCode = '{3}'", "nsap_bone", whsCode, sboId, itemCode.FilterSQL());
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sqlstr, CommandType.Text, null);
        }


        /// <summary>
        /// 批量上传（新）
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public List<UploadFileResp> BillAttachUploadNew(IFormFileCollection files, string host)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var result = new List<UploadFileResp>();
            foreach (var item in files)
            {
                var scon = new UploadFileResp();
                var fileName = ContentDispositionHeaderValue.Parse(item.ContentDisposition).FileName.Trim('"');
                scon.FileName = fileName;
                string filePath = FileHelper.FilePath.PhysicalPath;
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                string suffix = fileName.Split('.')[fileName.Split('.').Length - 1];
                fileName = Guid.NewGuid() + "." + suffix;
                string fileFullName = filePath + fileName;
                using (FileStream fs = System.IO.File.Create(fileFullName))
                {
                    //保存到本地
                    item.CopyTo(fs);
                    fs.Flush();
                }
                //scon.Id = new Guid().ToString();
                scon.FilePath = host + FileHelper.FilePath.VirtualPath + fileName;
                scon.FileType = suffix;
                scon.CreateUserName = loginUser.Name;
                result.Add(scon);
            }
            return result;
        }
        #region 获取联系人
        public string GetCntctCode(string Code, string Name, int SboId)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT CntctCode FROM {0}.crm_ocpr WHERE sbo_id=?sbo_id AND CardCode=?CardCode AND Name=?Name", "nsap_bone");

            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> sqlParameters = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?sbo_id",      SboId),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?CardCode",     Code),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?Name",    Name),

            };
            object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, sqlParameters);
            return obj == null ? "" : obj.ToString();
        }
        public string GetCntctCodeSql(string Code, string Name, int SboId)
        {
            DataTable dt = GetSboNamePwd(SboId);
            string sqlconn = ""; string sboname = "";
            if (dt.Rows.Count > 0)
            {
                sboname = dt.Rows[0][0].ToString() + ".dbo.";
            }
            if (SboId == 1) { sboname = ""; }
            string strSql = string.Format("SELECT CntctCode FROM " + sboname + "OCPR WHERE CardCode='{0}' AND Name='{1}'", Code, Name);
            object obj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, strSql, CommandType.Text, null);
            return obj == null ? "" : obj.ToString();
        }
        #endregion

        public string GetPagePowersByUrlWithClient(string url)
        {
            var UserID = _serviceBaseApp.GetUserNaspId();
            CurrentPage page = GetCurrentPage(UserID, url);
            Powers p = new Powers(page.AuthMap);
            StringBuilder sBuilder = new StringBuilder("{");
            sBuilder.AppendFormat("\"DownloadAttachment\":\"{0}\",", p.DownloadAttachment ? 1 : 0);
            sBuilder.AppendFormat("\"ViewAttachment\":\"{0}\",", p.ViewAttachment ? 1 : 0);
            sBuilder.AppendFormat("\"ViewCustom\":\"{0}\",", p.ViewCustom ? 1 : 0);
            sBuilder.AppendFormat("\"ViewSales\":\"{0}\",", p.ViewSales ? 1 : 0);
            sBuilder.AppendFormat("\"ViewPurchase\":\"{0}\",", p.ViewPurchase ? 1 : 0);
            sBuilder.AppendFormat("\"ViewGross\":\"{0}\",", p.ViewGross ? 1 : 0);
            sBuilder.AppendFormat("\"ViewCosts\":\"{0}\",", p.ViewCosts ? 1 : 0);
            sBuilder.AppendFormat("\"OperateExport\":\"{0}\",", p.OperateExport ? 1 : 0);
            sBuilder.AppendFormat("\"OperateAudit\":\"{0}\",", p.OperateAudit ? 1 : 0);
            sBuilder.AppendFormat("\"OperateDelete\":\"{0}\",", p.OperateDelete ? 1 : 0);
            sBuilder.AppendFormat("\"OperateUpdate\":\"{0}\",", p.OperateUpdate ? 1 : 0);
            sBuilder.AppendFormat("\"OperateAppend\":\"{0}\",", p.OperateAppend ? 1 : 0);
            sBuilder.AppendFormat("\"ViewFull\":\"{0}\",", p.ViewFull ? 1 : 0);
            sBuilder.AppendFormat("\"ViewSelfDepartment\":\"{0}\",", p.ViewSelfDepartment ? 1 : 0);
            sBuilder.AppendFormat("\"ViewSelf\":\"{0}\"", p.ViewSelf ? 1 : 0);
            sBuilder.Append("}");
            return sBuilder.ToString();
        }


        /// <summary>
        /// 根据业务伙伴代码获取相关数据
        /// </summary>
        public DataTable GetCardInfo(string CardCode, int SboID, bool isSql, bool ViewSelf, bool ViewSelfDepartment, bool ViewFull, int UserId, int DepId)
        {
            DataTable dt = GetSboNamePwd(SboID);
            string dRowData = string.Empty; string isOpen = "0"; string sboname = "0"; string sqlconn = "0";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); sboname = dt.Rows[0][0].ToString(); sqlconn = dt.Rows[0][5].ToString(); }
            string filterString = string.Empty;
            if (ViewSelfDepartment && !ViewFull)
            {
                DataTable rDataRows = GetSboSlpCodeIds(DepId, SboID);
                if (rDataRows.Rows.Count > 0)
                {
                    filterString += string.Format(" AND (a.SlpCode IN(");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][0]);
                    }
                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(") OR a.DfTcnician IN (");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][1]);
                    }
                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(") {0})", " OR a.SlpCode = -1 ");
                }

            }
            if (ViewSelf && !ViewFull && !ViewSelfDepartment)
            {
                DataTable rDataRowsSlp = GetSboSlpCodeId(UserId, SboID);
                if (rDataRowsSlp.Rows.Count > 0)
                {
                    string slpCode = rDataRowsSlp.Rows[0][0].ToString();
                    string slpTcnician = rDataRowsSlp.Rows[0][1].ToString();
                    string SlpCodeViewSelf = "";
                    if (CardCode.Substring(0, 1) == "V") { SlpCodeViewSelf = " OR a.SlpCode = -1"; } else { SlpCodeViewSelf = ""; }
                    filterString += string.Format(" AND (a.SlpCode = {0} OR a.DfTcnician={1} {2}) ", slpCode, slpTcnician, SlpCodeViewSelf);
                }
                else
                {
                    filterString += string.Format(" a.SlpCode =0  AND ");
                }
            }
            if (isSql && isOpen == "1")
            {
                return GetCardInfoSql(CardCode, sboname, sqlconn, filterString);
            }
            else
            {
                filterString += string.Format(" AND a.sbo_id={0}", SboID);
                return GetCardInfo(CardCode, filterString);
            }
        }
        public DataTable GetCardInfoSql(string CardCode, string sboname, string sqlconn, string filterString)
        {
            if (string.IsNullOrEmpty(sboname)) { sboname = ""; } else { sboname = sboname + ".dbo."; }
            string U_FPLB = string.Empty;
            if (IsExist("OCRD", "U_FPLB"))
            {
                U_FPLB = ",a.U_FPLB";
            }
            string strSql = string.Format("SELECT CardName,Currency,Building,MailBuildi{0},CntctPrsn,BillToDef AS PayToCode,ShipToDef AS ShipToCode,", U_FPLB);
            strSql += string.Format("(ISNULL(ZipCode,'')+ISNULL(b.Name,'')+ISNULL(c.Name,'')+ISNULL(City,'')+ISNULL(CONVERT(VARCHAR(1000),Building),'''')) AS Address,");
            strSql += string.Format("(ISNULL(MailZipCod,'')+ISNULL(d.Name,'')+ISNULL(e.Name,'')+ISNULL(MailCity,'')+ISNULL(CONVERT(VARCHAR(8000),MailBuildi),'''')) AS Address2,");
            strSql += string.Format("a.MailZipCod,a.State2,a.HsBnkIBAN,a.SlpCode AS U_YWY,a.QryGroup1 as IsTransport,U_is_reseller,U_EndCustomerName,U_EndCustomerContact FROM " + sboname + "OCRD a ");
            strSql += string.Format(" LEFT JOIN " + sboname + "OCRY b ON a.Country=b.Code");
            strSql += string.Format(" LEFT JOIN " + sboname + "OCST c ON a.State1=c.Code");
            strSql += string.Format(" LEFT JOIN " + sboname + "OCRY d ON a.MailCountr=d.Code");
            strSql += string.Format(" LEFT JOIN " + sboname + "OCST e ON a.State2=e.Code");
            strSql += string.Format(" WHERE CardCode='{0}'", CardCode);
            if (!string.IsNullOrEmpty(filterString)) { strSql += string.Format("{0}", filterString); }
            return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql, CommandType.Text, null);
        }
        /// <summary>
        /// 根据业务伙伴代码获取相关数据
        /// </summary>
        public DataTable GetCardInfo(string CardCode, string filterString)
        {
            string strSql = string.Format("SELECT CardName,Currency,Building,MailBuildi,U_FPLB,CntctPrsn,CONCAT(IFNULL(a.ZipCode,''),IFNULL(b.Name,''),");
            strSql += string.Format("IFNULL(c.Name,''),IFNULL(a.City,''),IFNULL(a.Building,'')) AS Address,CONCAT(IFNULL(a.MailZipCod,''),IFNULL(d.Name,''),");
            strSql += string.Format("IFNULL(e.Name,''),IFNULL(a.MailCity,''),IFNULL(a.MailBuildi,'')) AS Address2,a.MailZipCod,a.State2,a.HsBnkIBAN,a.QryGroup1 as IsTransport,U_is_reseller,U_EndCustomerName,U_EndCustomerContact");
            strSql += string.Format(",a.SlpCode");
            strSql += string.Format(" FROM {0}.crm_ocrd a", "nsap_bone");
            strSql += string.Format(" LEFT JOIN {0}.store_ocry b ON a.Country=b.Code", "nsap_bone");
            strSql += string.Format(" LEFT JOIN {0}.store_ocst c ON a.State1=c.Code", "nsap_bone");
            strSql += string.Format(" LEFT JOIN {0}.store_ocry d ON a.MailCountr=d.Code", "nsap_bone");
            strSql += string.Format(" LEFT JOIN {0}.store_ocst e ON a.State2=e.Code", "nsap_bone");

            strSql += string.Format(" WHERE CardCode='{0}'", CardCode);

            if (!string.IsNullOrEmpty(filterString)) { strSql += string.Format("{0}", filterString); }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }
        /// <summary>
        /// 审核备注
        /// </summary>
        /// <param name="docentry"></param>
        /// <param name="sboid"></param>
        /// <returns></returns>
        public string GetSaleQuotationRemarkById(string docentry, string sboid)
        {
            string sqlstr = string.Format("select AuditRemark from {0}.sale_oqut where sbo_id={1} and docentry={2} limit 1", "nsap_bone", sboid, docentry);
            object resultentry = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, sqlstr, CommandType.Text, null);
            return resultentry == null ? "" : resultentry.ToString();
        }

        /// <summary>
        /// 销售订单打印（新）
        /// </summary>
        /// <param name="sboid">账套Id</param>
        /// <param name="DocEntry">单据编号</param>
        /// <returns>成功返回字节流，失败抛出异常</returns>
        public async Task<byte[]> OrderExportShowNew(string sboid, string DocEntry)
        {
            DataTable dtb = OrderExportView(sboid, DocEntry);
            DataTable dtbs = OrderExportViews(sboid, DocEntry);
            DataTable dtprint = GetPrintNo(sboid, DocEntry);
            DataTable dtTotal = OrderExportTotalAmount(sboid, DocEntry);
            string PrintNo = Guid.NewGuid().ToString();
            int PrintNumIndex = 0;
            if (dtprint.Rows.Count > 0)
            {
                if (!string.IsNullOrEmpty(dtprint.Rows[0][0].ToString()))
                {
                    PrintNo = dtprint.Rows[0][0].ToString();
                }
                if (!string.IsNullOrEmpty(dtprint.Rows[0][1].ToString().Trim()))
                {
                    PrintNumIndex = 1 + int.Parse(dtprint.Rows[0][1].ToString().Trim());
                }
                else
                {
                    PrintNumIndex = 1;
                }
            }
            else
            {
                PrintNumIndex = 1;
            }

            if (UpdatePrintNo(sboid, DocEntry, PrintNo, PrintNumIndex.ToString()) == false)
            {
                return null;
            }

            var logopath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "logo.png");
            var logostr = "";
            using (var fs = new FileStream(logopath, FileMode.Open))
            {
                var photo = new byte[fs.Length];
                fs.Position = 0;
                await fs.ReadAsync(photo, 0, photo.Length);
                logostr = Convert.ToBase64String(photo);
                Console.WriteLine(logostr);
            }

            var indicator = dtb.Rows[0][30].ToString();
            var companyAddressData = new Category();
            var companyBankData = new Category();
            string companyName = "";
            string Chapter = "";
            string Chapterpath = "";
            string bankName = "";
            string bankNum = "";
            string telPhone = "";
            string DiscPrcnt = "";
            if (!string.IsNullOrEmpty(indicator))
            {
                //公司地址信息,在字典中维护
                companyAddressData = await UnitWork.Find<Category>(c => c.TypeId == "SYS_CompanyAddress" && c.DtValue == indicator).FirstOrDefaultAsync();
                companyBankData = await UnitWork.Find<Category>(c => c.TypeId == "SYS_BankMsg" && c.DtValue == indicator).FirstOrDefaultAsync();
                companyName = companyAddressData == null ? "" : companyAddressData.Name;
                bankName = companyBankData == null ? "" : companyBankData.Name;
                bankNum = companyBankData == null ? "" : companyBankData.Description;
                telPhone = companyBankData == null ? "" : companyBankData.DtCode;
                DiscPrcnt = "含税：" + (string.IsNullOrEmpty(dtb.Rows[0][23].ToString()) || (dtb.Rows[0][23].ToString() == "0") ? " " : dtb.Rows[0][23].ToString());
            }

            if (indicator == "01")
            {
                Chapterpath = Path.Combine(Directory.GetCurrentDirectory(), "Templates\\seal", NewareName + ".png");
            }
            else if (indicator == "02")
            {
                Chapterpath = Path.Combine(Directory.GetCurrentDirectory(), "Templates\\seal", NewllName + ".png");
            }
            else if (indicator == "05")
            {
                Chapterpath = Path.Combine(Directory.GetCurrentDirectory(), "Templates\\seal", DGNewareName + ".png");
            }
            else if (string.IsNullOrEmpty(indicator) && dtb.Rows[0][25].ToString() != "RMB")
            {
                Chapterpath = Path.Combine(Directory.GetCurrentDirectory(), "Templates\\seal", WBName + ".png");
                companyName = "NEWARE";
            }
            else
            {
                throw new Exception("报价单/订单所选择标识无打印模板，无法进行打印操作");
            }

            using (var fs = new FileStream(Chapterpath, FileMode.Open))
            {
                var photo = new byte[fs.Length];
                fs.Position = 0;
                await fs.ReadAsync(photo, 0, photo.Length);
                Chapter = Convert.ToBase64String(photo);
                Console.WriteLine(Chapter);
            }

            var PrintSalesOrder = new PrintSalesOrder
            {
                DocEntry = string.IsNullOrEmpty(dtb.Rows[0][0].ToString()) ? " " : dtb.Rows[0][0].ToString(),
                DateTime = string.IsNullOrEmpty(dtb.Rows[0][29].ToString()) ? " " : dtb.Rows[0][29].ToString(),
                SalseName = string.IsNullOrEmpty(dtb.Rows[0][8].ToString()) ? " " : dtb.Rows[0][8].ToString(),
                CardCode = string.IsNullOrEmpty(dtb.Rows[0][1].ToString()) ? " " : dtb.Rows[0][1].ToString(),
                Name = string.IsNullOrEmpty(dtb.Rows[0][3].ToString()) ? " " : dtb.Rows[0][3].ToString(),
                Tel = string.IsNullOrEmpty(dtb.Rows[0][4].ToString()) ? " " : dtb.Rows[0][4].ToString(),
                Fax = string.IsNullOrEmpty(dtb.Rows[0][17].ToString()) ? " " : dtb.Rows[0][17].ToString(),
                Cellolar = string.IsNullOrEmpty(dtb.Rows[0][9].ToString()) ? " " : dtb.Rows[0][9].ToString(),
                CardName = string.IsNullOrEmpty(dtb.Rows[0][2].ToString()) ? " " : dtb.Rows[0][2].ToString(),
                Address = string.IsNullOrEmpty(dtb.Rows[0][7].ToString()) ? " " : dtb.Rows[0][7].ToString(),
                Address2 = string.IsNullOrEmpty(dtb.Rows[0][19].ToString()) ? " " : dtb.Rows[0][19].ToString(),
                PymntGroup = string.IsNullOrEmpty(dtb.Rows[0][11].ToString()) ? " " : dtb.Rows[0][11].ToString(),
                Comments = string.IsNullOrEmpty(dtb.Rows[0][10].ToString()) ? " " : dtb.Rows[0][10].ToString().Replace("<br>", " "),
                DocTotal = string.IsNullOrEmpty(dtb.Rows[0][13].ToString())? " " : (dtb.Rows[0][31].ToString() == "0.00" ? "总计金额：   " + dtb.Rows[0][13].ToString().Split(' ')[0] + " " + _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(dtb.Rows[0][13].ToString().Split(' ')[1]), 2) : "折扣后金额：   " + dtb.Rows[0][13].ToString().Split(' ')[0] + " " + _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(dtb.Rows[0][13].ToString().Split(' ')[1]), 2)),
                DATEFORMAT = string.IsNullOrEmpty(dtb.Rows[0][14].ToString()) ? " " : dtb.Rows[0][14].ToString(),
                NumAtCard = string.IsNullOrEmpty(dtb.Rows[0][14].ToString()) ? " " : dtb.Rows[0][24].ToString(),
                AcceptanceDates = string.IsNullOrEmpty(dtb.Rows[0][20].ToString()) ? " " : dtb.Rows[0][20].ToString(),
                U_SL = string.IsNullOrEmpty(dtb.Rows[0][23].ToString()) ? " " : dtb.Rows[0][23].ToString(),
                TAmount = (string.IsNullOrEmpty(dtTotal.Rows[0][0].ToString()) || dtb.Rows[0][31].ToString() == "0.00") ? " " : "总计金额：   " + dtTotal.Rows[0][0].ToString().Split(' ')[0] + " " + _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(dtTotal.Rows[0][0].ToString().Split(' ')[1]), 2),
                DiscSum = (string.IsNullOrEmpty(dtb.Rows[0][31].ToString()) || dtb.Rows[0][31].ToString() == "0.00") ? " " : "折扣金额：   " + _serviceBaseApp.MoneyToCoin(dtb.Rows[0][31].ToDecimal(), 2),
                DisPlayStyle = (string.IsNullOrEmpty(dtb.Rows[0][31].ToString()) || dtb.Rows[0][31].ToString() == "0.00") ? "none" : "block",
                DiscPrcnt = DiscPrcnt,
                logo = logostr,
                QRcode = QRCoderHelper.CreateQRCodeToBase64(DocEntry),
                PrintNumIndex = PrintNumIndex.ToString(),
                PrintNo = PrintNo,
                ReimburseCosts = new List<ReimburseCost>()
            };

            for (int i = 0; i < dtbs.Rows.Count; i++)
            {
                ReimburseCost scon = new ReimburseCost
                {
                    ItemCode = string.IsNullOrEmpty(dtbs.Rows[i][2].ToString()) ? " " : dtbs.Rows[i][2].ToString(),
                    Dscription = string.IsNullOrEmpty(dtbs.Rows[i][3].ToString()) ? " " : dtbs.Rows[i][3].ToString(),
                    Quantity = string.IsNullOrEmpty(dtbs.Rows[i][4].ToString()) ? " " : dtbs.Rows[i][4].ToString(),
                    unitMsr = string.IsNullOrEmpty(dtbs.Rows[i][5].ToString()) ? " " : dtbs.Rows[i][5].ToString(),
                    Price = string.IsNullOrEmpty(dtbs.Rows[i][6].ToString()) ? " " : _serviceBaseApp.MoneyToCoin(dtbs.Rows[i][6].ToDecimal(), 4),
                    Money = string.IsNullOrEmpty(dtbs.Rows[i][7].ToString()) ? " " : _serviceBaseApp.MoneyToCoin(dtbs.Rows[i][7].ToDecimal(), 2)
                };

                PrintSalesOrder.ReimburseCosts.Add(scon);
            }

            var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "PrintSalesOrdersheader.html");
            var text = System.IO.File.ReadAllText(url);
            text = text.Replace("@Model.Data.logo", PrintSalesOrder.logo);
            text = text.Replace("@Model.Data.DocEntry", PrintSalesOrder.DocEntry);
            text = text.Replace("@Model.Data.DateTime", PrintSalesOrder.DateTime);
            text = text.Replace("@Model.Data.QRcode", PrintSalesOrder.QRcode);
            text = text.Replace("@Model.Data.SalseName", PrintSalesOrder.SalseName);
            text = text.Replace("@Model.Data.CardCode", PrintSalesOrder.CardCode);
            text = text.Replace("@Model.Data.Name", PrintSalesOrder.Name);
            text = text.Replace("@Model.Data.Tel", PrintSalesOrder.Tel);
            text = text.Replace("@Model.Data.Fax", PrintSalesOrder.Fax);
            text = text.Replace("@Model.Data.CardName", PrintSalesOrder.CardName);
            text = text.Replace("@Model.Data.Address", PrintSalesOrder.Address);
            text = text.Replace("@Model.Data.Name", PrintSalesOrder.Name);
            text = text.Replace("@Model.Data.Addrestwo", PrintSalesOrder.Address2);
            text = text.Replace("@Model.Data.SalseName", PrintSalesOrder.SalseName);
            text = text.Replace("@Model.Data.Cellolar", PrintSalesOrder.Cellolar);
            text = text.Replace("@Model.Data.DATEFORMAT", PrintSalesOrder.DATEFORMAT.Substring(0, 11));
            text = text.Replace("@Model.Data.PymntGroup", PrintSalesOrder.PymntGroup);
            text = text.Replace("@Model.Data.Comments", PrintSalesOrder.Comments);
            text = text.Replace("@Model.Data.NumAtCard", PrintSalesOrder.NumAtCard);
            text = text.Replace("@Model.Data.AcceptanceDates", PrintSalesOrder.AcceptanceDates);
            var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"PrintSalesOrdersheader{PrintSalesOrder.DocEntry}.html");
            System.IO.File.WriteAllText(tempUrl, text, Encoding.Unicode);
            var footUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "PrintSalesOrdersfooter.html");
            var foottext = System.IO.File.ReadAllText(footUrl);
            foottext = foottext.Replace("@Model.Data.Chapter", Chapter);
            foottext = foottext.Replace("@Model.Data.PrintNumIndex", PrintSalesOrder.PrintNumIndex);
            foottext = foottext.Replace("@Model.Data.PrintNo", PrintSalesOrder.PrintNo);
            foottext = foottext.Replace("@Model.Data.Company", companyName);
            foottext = foottext.Replace("@Model.Data.Address", companyAddressData == null ? "" : companyAddressData.Description);
            foottext = foottext.Replace("@Model.Data.BankName", bankName);
            foottext = foottext.Replace("@Model.Data.BankNum", bankNum);
            foottext = foottext.Replace("@Model.Data.TelPhone", telPhone);
            var foottempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"PrintSalesOrdersfooter{PrintSalesOrder.DocEntry}.html");
            System.IO.File.WriteAllText(foottempUrl, foottext, Encoding.Unicode);
            byte[] basecode = await ExportAllHandler.Exporterpdf(PrintSalesOrder, "PrintSalesOrders.cshtml", pdf =>
            {
                pdf.Orientation = Orientation.Portrait;
                pdf.IsWriteHtml = true;
                pdf.PaperKind = PaperKind.A4;
                pdf.IsEnablePagesCount = true;
                pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                pdf.FooterSettings = new FooterSettings() { HtmUrl = foottempUrl };
            });

            System.IO.File.Delete(tempUrl);
            System.IO.File.Delete(foottempUrl);
            return basecode;
        }

        #region 修改单据打印状态
        /// <summary>
        /// 修改单据打印状态
        /// </summary>
        public async Task<string> UpdatePrintStat(string SboId, string DocEntry, string TableName1, string TableName2)
        {
            bool result = false;
            string sqls = string.Format("UPDATE {0}.{3} SET Printed='Y' WHERE DocEntry={1} and sbo_id={2} ", "nsap_bone", DocEntry, SboId, TableName1);
            result = UnitWork.ExecuteSql(sqls.ToString(), ContextType.NsapBaseDbContext) > 0 ? true : false;
            if (result)
            {
                string sqlsbo = string.Format("SELECT sql_db,sql_conn FROM {0}.sbo_info WHERE sbo_id={1}", "nsap_base", SboId);
                DataTable dts = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sqlsbo.ToString(), CommandType.Text, null);
                if (dts.Rows.Count > 0)
                {
                    string sqla = string.Format("SELECT Printed FROM {1} WHERE DocEntry={0} ", DocEntry, TableName2);
                    DataTable dt = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, sqla.ToString(), CommandType.Text, null);
                    if (dt.Rows.Count > 0)
                    {
                        string sqlss = string.Format("UPDATE {1} SET Printed='Y' WHERE DocEntry={0} ", DocEntry, TableName2);
                        result = UnitWork.ExecuteSql(sqlss.ToString(), ContextType.SapDbContextType) > 0 ? true : false;
                    }
                }
            }
            if (result == false)
            {
                return "修改状态失败";
            }
            else
            {
                return "修改状态成功";
            }
        }
        #endregion
        #region 重置单据打印状态
        /// <summary>
        /// 重置单据打印状态
        /// </summary>
        public async Task<string> ResetPrintStat(string SboId, string DocEntry, string TableName1, string TableName2)
        {
            bool result = false;
            string sqls = string.Format("UPDATE {0}.{3} SET Printed='N' WHERE DocEntry={1} and sbo_id={2} ", "nsap_bone", DocEntry, SboId, TableName1);
            result = UnitWork.ExecuteSql(sqls.ToString(), ContextType.NsapBaseDbContext) > 0 ? true : false;
            if (result)
            {
                string sqlsbo = string.Format("SELECT sql_db,sql_conn FROM {0}.sbo_info WHERE sbo_id={1}", "nsap_base", SboId);
                DataTable dts = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sqlsbo.ToString(), CommandType.Text, null);
                if (dts.Rows.Count > 0)
                {
                    string sqla = string.Format("SELECT Printed FROM {1} WHERE DocEntry={0} ", DocEntry, TableName2);
                    DataTable dt = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, sqla.ToString(), CommandType.Text, null);
                    if (dt.Rows.Count > 0)
                    {
                        string sqlss = string.Format("UPDATE {1} SET Printed='N' WHERE DocEntry={0} ", DocEntry, TableName2);
                        result = UnitWork.ExecuteSql(sqlss.ToString(), ContextType.SapDbContextType) > 0 ? true : false;
                    }
                }
            }
            if (result == false)
            {
                return "重置状态失败";
            }
            else
            {
                return "重置状态成功";
            }
        }
        #endregion
        /// <summary>
        /// 查看视图【行明细 - 帐套关闭】
        /// </summary>
        /// <returns></returns>
        public DataTable SelectBillView(int pageSize, int pageIndex, string filterQuery, string sortname, string sortorder, string type, bool ViewFull, bool ViewSelf, int UserID, int SboID, bool ViewSelfDepartment, int DepID, bool ViewCustom, bool ViewSales, out int rowCount)
        {

            string sortString = string.Empty;
            string filterString = string.Empty;
            string line = string.Empty;
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());
            #region 搜索条件
            if (!string.IsNullOrEmpty(filterQuery))
            {
                string[] fields = filterQuery.Split('`');
                string[] p = fields[0].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("a.sbo_id = {0} AND ", p[1]);
                }
                p = fields[1].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("{0} LIKE '{1}' AND ", p[0], p[1].FilterSQL().Trim());
                }
                p = fields[2].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("({0} LIKE '%{1}%' OR a.CardName LIKE '%{1}%') AND ", p[0], p[1].FilterWildCard());
                }
                p = fields[3].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("({0} LIKE '%{1}%' OR b.Dscription LIKE '%{1}%') AND ", p[0], p[1].FilterWildCard());
                }
                p = fields[4].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    if (p[1] == "ON") { filterString += string.Format(" (a.DocStatus = 'O' AND a.Printed = 'N' AND a.CANCELED = 'N') AND "); }
                    if (p[1] == "OY") { filterString += string.Format(" (a.DocStatus = 'O' AND a.Printed = 'Y' AND a.CANCELED = 'N') AND "); }
                    if (p[1] == "CY") { filterString += string.Format(" a.CANCELED = 'Y' AND "); }
                    if (p[1] == "CN") { filterString += string.Format(" (a.DocStatus = 'C' AND a.CANCELED = 'N') AND "); }
                    if (p[1] == "NC") { filterString += string.Format(" a.CANCELED = 'N' AND "); }
                }
                p = fields[5].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("a.Comments LIKE '%{0}%' AND ", p[1].FilterWildCard());
                }
                p = fields[6].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("{0} LIKE '%{1}%' AND ", p[0], p[1].FilterSQL().Trim());
                }
                if ((type == "OPDN" || type == "OPOR" || type == "ORDR") && fields.Length > 7)
                {
                    p = fields[7].Split(':');
                    if (!string.IsNullOrEmpty(p[1]))
                    {
                        filterString += string.Format("{0} LIKE '%{1}%' AND ", p[0], p[1].FilterSQL().Trim());
                    }
                }
                if (type == "OPDN" && fields.Length > 8)
                {
                    p = fields[8].Split(':');
                    if (!string.IsNullOrEmpty(p[1]))
                    {
                        filterString += string.Format("{0}={1} AND ", p[0], p[1].FilterSQL().Trim());
                    }
                }
            }
            else
            {
                filterString += string.Format("a.sbo_id={0} AND ", SboID);
            }
            #endregion

            #region 根据不同的单据显示不同的内容
            if (!string.IsNullOrEmpty(type))
            {
                if (type == "OQUT") { type = "sale_oqut"; line = "sale_qut1"; }//销售报价单
                else if (type == "ORDR") { type = "sale_ordr"; line = "sale_rdr1"; }//销售订单
                else if (type == "ODLN") { type = "sale_odln"; line = "sale_dln1"; }//销售交货单
                else if (type == "OINV") { type = "sale_oinv"; line = "sale_inv1"; }//应收发票
                else if (type == "ORDN") { type = "sale_ordn"; line = "sale_rdn1"; }//销售退货单
                else if (type == "ORIN") { type = "sale_orin"; line = "sale_rin1"; }//应收贷项凭证
                else if (type == "OPQT") { type = "buy_opqt"; line = "buy_pqt1"; }//采购报价单
                else if (type == "OPOR") { type = "buy_opor"; line = "buy_por1"; }//采购订单
                else if (type == "OPDN") { type = "buy_opdn"; line = "buy_pdn1"; }//采购收货单
                else if (type == "OPCH") { type = "buy_opch"; line = "buy_pch1"; }//应付发票
                else if (type == "ORPD") { type = "buy_orpd"; line = "buy_rpd1"; }//采购退货单
                else if (type == "ORPC") { type = "buy_orpc"; line = "buy_rpc1"; }//应付贷项凭证
                else { type = "OQUT"; line = "QUT1"; }
            }
            #endregion

            #region 判断权限
            //if (!ViewFull)
            //{
            string arr_roles = GetRolesName(UserID);
            if ((line.Contains("buy")) && ((!arr_roles.Contains("物流文员")) && (!arr_roles.Contains("系统管理员"))))//若不含有物流文员角色，则则屏蔽运输采购单
            {
                filterString += string.Format(" d.QryGroup1='N' AND ");
            }
            //}
            if (ViewSelfDepartment && !ViewFull)
            {
                DataTable rDataRows = GetSboSlpCodeIds(DepID, SboID);
                if (rDataRows.Rows.Count > 0)
                {
                    filterString += string.Format(" (a.SlpCode IN(");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][0]);
                    }
                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(") OR d.DfTcnician IN (");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][1]);
                    }
                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(")) AND ");
                }

            }
            if (ViewSelf && !ViewFull && !ViewSelfDepartment)
            {
                DataTable rDataRowsSlp = GetSboSlpCodeId(UserID, SboID);
                if (rDataRowsSlp.Rows.Count > 0)
                {
                    string slpCode = rDataRowsSlp.Rows[0][0].ToString();
                    string DfTcnician = rDataRowsSlp.Rows[0][1].ToString();
                    filterString += string.Format(" (a.SlpCode = {0}) AND ", slpCode);// OR d.DfTcnician={1}   , DfTcnician  不允许售后查看业务员的单
                }
                else
                {
                    filterString = string.Format(" (a.SlpCode = {0}) AND ", 0);
                }
            }
            #endregion
            if (!string.IsNullOrEmpty(filterString))
                filterString = filterString.Substring(0, filterString.Length - 5);
            return SelectBillViewDetails(out rowCount, pageSize, pageIndex, filterString, sortString, type, line, ViewCustom, ViewSales);

        }
        /// <summary>
        /// 查询单据列表（MySql）
        /// </summary>
        public DataTable SelectBillViewDetails(out int rowCounts, int pageSize, int pageIndex, string filterQuery, string orderName, string type, string line, bool ViewCustom, bool ViewSales)
        {
            StringBuilder tableName = new StringBuilder();
            StringBuilder filedName = new StringBuilder();
            filedName.Append(" '',a.UpdateDate,a.DocEntry,a.CardCode,IF(" + ViewCustom + ",a.CardName,'******') CardName,b.ItemCode,b.Dscription,b.Quantity,IF(" + ViewSales + ",b.Price,'******') Price,IF(" + ViewSales + ",b.LineTotal,'******') LineTotal,IF(" + ViewSales + ",a.DocTotal,'******') DocTotal,IF(" + ViewSales + ",(a.DocTotal-a.PaidToDate),'******') OpenDocTotal,a.CreateDate,a.SlpCode,a.Comments,a.DocStatus,a.Printed,c.SlpName,a.CANCELED,a.Indicator,a.DocDueDate,e.Quantity eQuantity");
            if (line.ToLower() == "buy_por1")
            {
                filedName.Append(",b.ActualDocDueDate,b.LineNum,b.U_RelDoc");
            }
            if (line.ToLower() == "buy_pdn1")
            {
                filedName.Append(",case when b.BaseType=22 then b.BaseEntry else '' end as BaseEntry,'' as LineNum");
            }
            filedName.Append(",a.U_YGMD");
            if (line.ToLower() == "buy_pdn1")
            {
                filedName.Append(",f.CreateDate as por_CreateDate,f.DocDueDate as por_DocDueDate,'' as ActualDocDueDate,b.BaseLine,f1.U_RelDoc");
            }
            if (line.ToLower() == "sale_dln1")
            {
                filedName.Append(",f.CreateDate as rdr_CreateDate,f.DocDueDate as rdr_DocDueDate,b.u_reldoc,a.docdate");
            }
            if (line.ToLower() == "sale_rdr1")
            {
                filedName.Append(",b.LineNum,b.U_RelDoc ");
            }
            tableName.AppendFormat("{0}." + type + " a LEFT JOIN  {0}." + line + " b ON a.DocEntry=b.DocEntry AND a.sbo_id=b.sbo_id ", "nsap_bone");
            tableName.AppendFormat(" LEFT JOIN {0}.crm_oslp c ON a.SlpCode = c.SlpCode AND a.sbo_id=c.sbo_id", "nsap_bone");
            tableName.AppendFormat(" LEFT JOIN {0}.crm_ocrd d ON a.CardCode = d.CardCode AND a.sbo_id=d.sbo_id", "nsap_bone");
            tableName.AppendFormat(" LEFT JOIN {0}.buy_pdn1 e ON a.DocEntry = e.BaseEntry AND e.ItemCode = b.ItemCode AND e.BaseType = 22 AND a.sbo_id=e.sbo_id", "nsap_bone");//查询订单物料交货数
            if (line.ToLower() == "buy_pdn1")
            {
                tableName.AppendFormat(" LEFT JOIN {0}.buy_opor f ON b.BaseEntry = f.DocEntry", "nsap_bone");
                tableName.AppendFormat(" LEFT JOIN {0}.buy_POR1 f1 on b.BaseEntry=f1.docentry and b.BaseLine=f1.LineNum", "nsap_bone");
            }
            if (line.ToLower() == "sale_dln1")
            {
                tableName.AppendFormat(" LEFT JOIN {0}.sale_ordr f ON b.BaseEntry = f.DocEntry", "nsap_bone");
            }
            return SelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), pageSize, pageIndex, orderName, filterQuery, out rowCounts);
        }
    }
}
