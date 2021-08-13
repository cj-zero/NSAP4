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

namespace OpenAuth.App.Order
{
    /// <summary>
    /// 销售订单业务
    /// </summary>
    public partial class ServiceSaleOrderApp : OnlyUnitWorkBaeApp
    {
        private readonly RevelanceManagerApp _revelanceApp;
        private readonly AppServiceOrderLogApp _appServiceOrderLogApp;
        private readonly ServiceOrderLogApp _ServiceOrderLogApp;
        private IOptions<AppSetting> _appConfiguration;
        private ICapPublisher _capBus;
        private readonly ServiceFlowApp _serviceFlowApp;
        ServiceBaseApp _serviceBaseApp;
        public ServiceSaleOrderApp(IUnitWork unitWork, RevelanceManagerApp app, ServiceBaseApp serviceBaseApp, ServiceOrderLogApp serviceOrderLogApp, IAuth auth, AppServiceOrderLogApp appServiceOrderLogApp, IOptions<AppSetting> appConfiguration, ICapPublisher capBus, ServiceOrderLogApp ServiceOrderLogApp, ServiceFlowApp serviceFlowApp) : base(unitWork, auth)
        {
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
                sortString = string.Format("{0} {1}", "a.docentry", query.SortName, query.SortOrder);
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
            //if (type == "ODLN")
            //{
            //    p = fields[6].Split(':');
            //    if (!string.IsNullOrEmpty(p[1]))
            //    {
            //        filterString += string.Format("a.GroupNum = {1} AND ", p[0], p[1]);
            //    }
            //    string[] p7 = fields[7].Split(':');
            //    string[] p8 = fields[8].Split(':');
            //    if (!string.IsNullOrEmpty(p8[1]))
            //    {
            //        filterString += " (a.CreateDate BETWEEN '" + p7[1].FilterSQL().Trim() + "' AND '" + p8[1].FilterSQL().Trim() + "') AND ";
            //    }
            //}
            if (type == "ORDR")
            {
                if (!string.IsNullOrWhiteSpace(query.Indicator))
                {
                    filterString += string.Format("a.Indicator = '{0}' AND ", query.Indicator);
                }
            }
            //查询关联订单
            //if (fields.Length > 6)
            //{
            //    p = fields[6].Split(':');
            //    if (!string.IsNullOrEmpty(p[1]))
            //    {
            //        if (type == "OPDN")
            //        {
            //            filterString += string.Format("EXISTS (select 1 from {0}.dbo.PDN1 p1 LEFT JOIN {0}.dbo.POR1 p2 on p1.BaseEntry=p2.DocEntry and p1.BaseLine=p2.LineNum where p1.docentry=a.docentry and p2.U_RelDoc like '%{1}%') AND ", sboname, p[1].FilterSQL().Trim());
            //        }
            //        if (type == "OPOR")
            //        {
            //            filterString += string.Format("EXISTS (select 1 from {0}.dbo.POR1 p2  where p2.docentry=a.docentry and p2.U_RelDoc like '%{1}%') AND ", sboname, p[1].FilterSQL().Trim());
            //        }
            //    }
            //}
            //if (type == "OPDN" && fields.Length > 7)
            //{
            //    p = fields[7].Split(':');
            //    if (!string.IsNullOrEmpty(p[1]))
            //    {
            //        filterString += string.Format("EXISTS (select 1 from {0}.dbo.PDN1 p2  where p2.docentry=a.docentry and p2.BaseEntry={1}) AND ", sboname, p[1].FilterSQL().Trim());
            //    }
            //}
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
                //视图查询数据
                tableData = SelectOrdersInfo(out rowCount, pageSize, pageIndex, filterString, sortString, type, line, ViewCustom, ViewSales, sqlcont, sboname);
                //if (type.ToLower() == "odln")
                //{
                //    //thistab.Columns.Add("BuyDocEntry", typeof(string));
                //    //thistab.Columns.Add("TransportName", typeof(string));
                //    //thistab.Columns.Add("TransportID", typeof(string));
                //    //thistab.Columns.Add("TransportSum", typeof(string));
                //    foreach (DataRow odlnrow in thistab.Rows)
                //    {
                //        string docnum = odlnrow["DocEntry"].ToString();
                //        DataTable thist = NSAP.Data.Sales.BillDelivery.GetSalesDelivery_PurchaseOrderList(docnum, SboID.ToString());
                //        string buyentry = "";
                //        string transname = "";
                //        string transid = "";
                //        double transsum = 0.00;
                //        string tempname = "";
                //        string transDocTotal = "";
                //        for (int i = 0; i < thist.Rows.Count; i++)
                //        {
                //            transsum += double.Parse(thist.Rows[i]["DocTotal"].ToString());// 交货对应采购单总金额
                //                                                                           //快递单号，对应采购单编号
                //            if (string.IsNullOrEmpty(buyentry))
                //            {
                //                buyentry = thist.Rows[i]["Buy_DocEntry"].ToString();
                //                transid = string.IsNullOrEmpty(thist.Rows[i]["LicTradNum"].ToString()) ? "000000000" : thist.Rows[i]["LicTradNum"].ToString();
                //                tempname = thist.Rows[i]["CardName"].ToString();
                //                transname = tempname;
                //                transDocTotal = thist.Rows[i]["DocTotal"].ToString();
                //            }
                //            else
                //            {
                //                buyentry += ";" + thist.Rows[i]["Buy_DocEntry"].ToString();
                //                transid += ";" + (string.IsNullOrEmpty(thist.Rows[i]["LicTradNum"].ToString()) ? "000000000" : thist.Rows[i]["LicTradNum"].ToString());
                //                //物流公司名称如果连续重复，则只显示第一个
                //                if (tempname != thist.Rows[i]["CardName"].ToString())
                //                    tempname = thist.Rows[i]["CardName"].ToString();
                //                else
                //                    tempname = "";
                //                transname += ";;" + tempname;
                //                transDocTotal += ";" + thist.Rows[i]["DocTotal"].ToString();
                //            }

                //        }
                //        odlnrow["BuyDocEntry"] = buyentry;
                //        odlnrow["TransportName"] = transname;
                //        odlnrow["TransportID"] = transid;
                //        odlnrow["TransportSum"] = transsum.ToString() + ";" + transDocTotal;
                //    }
                //}
                //if (type.ToLower() == "ordr")//对应发票状态
                //{
                //    thistab.Columns.Add("billStatus", typeof(string));
                //    thistab.Columns.Add("bonusStatus", typeof(string));
                //    thistab.Columns.Add("proStatus", typeof(string));
                //    thistab.Columns.Add("IndicatorName", typeof(string));
                //    thistab.Columns.Add("EmpAcctWarn", typeof(string));
                //    string bonustypeid = NSAP.Data.Client.ClientInfo.GetJobTypeByUrl("sales/SalesBonus.aspx");
                //    string bonusatypeid = NSAP.Data.Client.ClientInfo.GetJobTypeByUrl("sales/BonusAfterSales.aspx");
                //    string protypeid = NSAP.Data.Client.ClientInfo.GetJobTypeByUrl("product/ProductionOrder.aspx");
                //    string protypeid_cp = NSAP.Data.Client.ClientInfo.GetJobTypeByUrl("product/ProductionOrder_CP.aspx");
                //    string typeidstr = bonustypeid + "," + bonusatypeid + "," + protypeid + "," + protypeid_cp;
                //    foreach (DataRow ordrrow in thistab.Rows)
                //    {
                //        string orderid = ordrrow["DocEntry"].ToString();
                //        ordrrow["billStatus"] = NSAP.Data.Sales.BillDelivery.GetBillStatusByOrderId(orderid, SboID.ToString());
                //        DataTable jobtab = NSAP.Data.Sales.BillDelivery.GetJobStateForDoc(orderid, typeidstr, SboID.ToString());
                //        DataRow[] bonusrows = jobtab.Select("job_type_id=" + bonustypeid + " or job_type_id=" + bonusatypeid);
                //        DataRow[] prorows = jobtab.Select("job_type_id=" + protypeid + " or job_type_id=" + protypeid_cp, "upd_dt desc");
                //        ordrrow["bonusStatus"] = "";
                //        ordrrow["proStatus"] = "";
                //        if (bonusrows.Length > 0)
                //        {
                //            ordrrow["bonusStatus"] = bonusrows[0]["job_state"].ToString();
                //        }
                //        if (prorows.Length > 0)
                //        {
                //            ordrrow["proStatus"] = prorows[0]["job_state"].ToString();
                //        }
                //        ordrrow["IndicatorName"] = NSAP.Data.Sales.BillDelivery.GetBillIndicatorByOrderId(orderid, SboID.ToString());
                //        ordrrow["EmpAcctWarn"] = NSAP.Data.Sales.BillDelivery.GetEmptyAcctByOrderId(orderid, SboID.ToString());
                //    }
                //}
                //if (type.ToLower() == "opor")
                //{
                //    foreach (DataRow temprow in thistab.Rows)
                //    {
                //        string indicator = temprow["Indicator"].ToString();
                //        string taxstr = NSAP.Data.Finance.BillApplication.GetTaxNoByPO(temprow["DocEntry"].ToString(), SboID.ToString());
                //        temprow["PurchaseBillNo"] = taxstr;
                //        temprow["IndicatorName"] = NSAP.Data.Finance.BillApplication.GetToCompanyName(SboID.ToString(), indicator);
                //    }
                //}
                //if (type.ToLower() == "opdn")
                //{
                //    double totalamount = 0.00;
                //    foreach (DataRow temprow in thistab.Rows)
                //    {
                //        totalamount += double.Parse(temprow["Doctotal"].ToString());
                //    }
                //    string pageExtend = string.Format("总金额：{0}", totalamount.ToString("0.00"));
                //    return thistab.FelxgridDataToJSON(pageIndex.ToString(), rowCount.ToString(), pageExtend);
                //}
                //else
                //{
                //    return thistab.FelxgridDataToJSON(pageIndex.ToString(), rowCount.ToString());
                //}
            }
            else
            {
                //  return SelectBillViewInfo(pageSize, pageIndex, filterQuery, sortname, sortorder, type, ViewFull, ViewSelf, UserID, uSboId, ViewSelfDepartment, DepID, ViewCustom, ViewSales);
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
                filedName.Append(",''  as AttachFlag ");
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
                //new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("strFrom",tableName.ToString()),
                //new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("strSelect",filedName.ToString()),
                //new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("pageSize",pageSize),
                //new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("pageIndex",pageIndex),
                //new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("strOrder",orderName),
                //new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("strWhere",filterQuery),
                //new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("isStats",1),
                //new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("rowCount",0),
            };
            SqlParameter isStats = new SqlParameter("@isStats", SqlDbType.Int);
            isStats.Value = 1;
            sqlParameters.Add(isStats);
            SqlParameter paramOut = new SqlParameter("@rowCount", SqlDbType.Int);
            paramOut.Value = 0;
            paramOut.Direction = ParameterDirection.Output;
            sqlParameters.Add(paramOut);
            DataTable dt = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, $"sp_common_pager", CommandType.StoredProcedure, sqlParameters);
            tableData.Count = Convert.ToInt32(paramOut.Value);
            rowCounts = Convert.ToInt32(sqlParameters[7].Value);
            // dt = Sql.SAPSelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), pageSize, pageIndex, orderName, filterQuery, out rowCounts);
            if (type.ToLower() == "ordr" || type.ToLower() == "opor")
            {
                string bonetype = type.ToLower();
                string sql = "SELECT  a.PrintNo,a.PrintNumIndex  FROM nsap_bone.sale_ordr a  where a.DocEntry=1941 and a.sbo_id=1";
                if ("opor" == type.ToLower())
                {
                    sql = "SELECT  a.PrintNo,a.PrintNumIndex  FROM nsap_bone.buy_opor a  where a.DocEntry=1941 and a.sbo_id=1";
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataTable dtPrintnum = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);//取编号
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
                    var typeObj = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, $"SELECT a.type_id value FROM nsap_oa.file_type a LEFT JOIN nsap_base.base_func b ON a.func_id=b.func_id LEFT JOIN nsap_base.base_page c ON c.page_id=b.page_id WHERE c.page_url='{"sales/SalesQuotation.aspx"}'", CommandType.Text, null).FirstOrDefault();
                    string fileType = typeObj == null ? "-1" : typeObj.Value.ToString();

                    string strSql2 = string.Format("SELECT 1 value FROM nsap_oa.file_main AS T0 ");
                    strSql2 += string.Format("LEFT JOIN nsap_oa.file_type AS T1 ON T0.file_type_id = T1.type_id ");
                    strSql2 += string.Format("WHERE T0.file_type_id = {0} AND T0.docEntry = {1} limit 1", int.Parse(fileType), int.Parse(temprow["DocEntry"].ToString()));
                    ResultOrderDto fileflag = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, strSql2, CommandType.Text, null).FirstOrDefault();
                    temprow["AttachFlag"] = fileflag == null ? "0" : fileflag.Value.ToString();
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
            int userID = _serviceBaseApp.GetUserNaspId();
            int sboID = _serviceBaseApp.GetUserNaspSboID(userID);
            int funcId = 50;
            string logstring = "";
            string jobname = "";
            string result = "";
            try
            {
                billDelivery billDelivery = BulidBillDelivery(orderReq.Order);
                if (orderReq.IsCopy)
                {
                    funcId = _serviceBaseApp.GetFuncsByUserID("sales/SalesOrder.aspx", userID);
                    logstring = "根据销售报价单下销售订单";
                    jobname = "销售订单";
                    SalesOrderSave_ORDR(orderReq);
                }
                else
                {
                    funcId = _serviceBaseApp.GetFuncsByUserID("sales/SalesQuotation.aspx", userID);
                    byte[] job_data = ByteExtension.ToSerialize(billDelivery);
                    string className = "NSAP.B1Api.BOneOQUT";
                    logstring = "新建销售报价单";
                    jobname = "销售报价单";
                    if (orderReq.Ations == OrderAtion.Draft)
                    {
                        result = OrderWorkflowBuild(jobname, funcId, userID, job_data, orderReq.Order.Remark, sboID, orderReq.Order.CardCode, orderReq.Order.CardName, (double.Parse(orderReq.Order.DocTotal) > 0 ? double.Parse(orderReq.Order.DocTotal) : 0), int.Parse(orderReq.Order.BillBaseType), int.Parse(orderReq.Order.BillBaseEntry), "BOneAPI", className);
                    }
                    else if (orderReq.Ations == OrderAtion.Submit)
                    {
                        result = OrderWorkflowBuild(jobname, funcId, userID, job_data, orderReq.Order.Remark, sboID, orderReq.Order.CardCode, orderReq.Order.CardName, (double.Parse(orderReq.Order.DocTotal) > 0 ? double.Parse(orderReq.Order.DocTotal) : 0), int.Parse(orderReq.Order.BillBaseType), int.Parse(orderReq.Order.BillBaseEntry), "BOneAPI", className);
                        if (int.Parse(result) > 0)
                        {
                            var par = SaveJobPara(result, orderReq.IsTemplate);
                            if (par)
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
                                    result = Eshop_OrderStatusFlow(thisinfo, int.Parse(orderReq.Order.U_New_ORDRID));
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
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
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
        public bool SaveJobPara(string jobID, bool setNumber)
        {
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

            WfaJobPara wfaJobPara = new WfaJobPara()
            {
                job_id = int.Parse(jobID),
                para_idx = 1,
                para_val = setNumber ? "" : "1",
                upd_dt = DateTime.Now
            };
            UnitWork.Add<WfaJobPara, int>(wfaJobPara);
            return true;
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
        public bool UpdateAudit(int jobId, byte[] jobData, string remarks, string doc_total, string card_code, string card_name)
        {
            bool isSave = false;
            //string strSql = string.Format("UPDATE {0}.wfa_job SET job_data=?job_data,remarks=?remarks,job_state=?job_state,doc_total=?doc_total,", Sql.BaseDatabaseName);
            //strSql += string.Format("card_code=?card_code,card_name=?card_name WHERE job_id = ?job_id", Sql.BaseDatabaseName);
            //IDataParameter[] parameters =
            //{
            //    Sql.Action.GetParameter("?job_data", jobData),
            //    Sql.Action.GetParameter("?remarks", remarks),
            //    Sql.Action.GetParameter("?job_state", "0"),
            //    Sql.Action.GetParameter("?doc_total", doc_total==""?"0":doc_total),
            //    Sql.Action.GetParameter("?card_code", card_code),
            //    Sql.Action.GetParameter("?card_name", card_name),
            //    Sql.Action.GetParameter("?job_id",  jobId)
            //};
            //isSave = Sql.Action.ExecuteNonQuery(Sql.UTF8ConnectionString, CommandType.Text, strSql, parameters) > 0 ? true : false;
            return isSave;
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
            if (!string.IsNullOrEmpty(query.SortName) && !string.IsNullOrEmpty(query.SortOrder))
            {
                sortString = string.Format("{0} {1}", query.SortName.Replace("itemcode", "m.itemcode"), query.SortOrder.ToUpper());
            }
            if (!string.IsNullOrEmpty(query.ItemCode))
            {
                filterString += string.Format("(m.ItemCode LIKE '%{0}%' OR m.ItemName LIKE '%{0}%') AND ", query.ItemCode);
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
            filedName.Append(" '',m.ItemCode,m.ItemName,IFNULL(c.high_price,0) AS high_price,IFNULL(c.low_price,0) AS low_price,w.OnHand,m.OnHand AS SumOnHand,m.IsCommited,m.OnOrder,(w.OnHand-w.IsCommited+w.OnOrder) AS OnAvailable,");
            filedName.Append("(m.OnHand-m.IsCommited+m.OnOrder) AS Available,w.WhsCode,IFNULL(U_TDS,'0') AS U_TDS,IFNULL(U_DL,0) AS U_DL,");
            filedName.Append("IFNULL(U_DY,0) AS U_DY,m.U_JGF,m.LastPurPrc,IFNULL(c.item_cfg_id,0),IFNULL(c.pic_path,m.PicturName),");
            filedName.Append("((CASE m.QryGroup1 WHEN 'N' then 0 else 0.5 END)");
            filedName.Append("+(CASE m.QryGroup2 WHEN 'N' then 0 else 3 END)");
            filedName.Append("+(CASE m.QryGroup3 WHEN 'N' then 0 else 2 END)) AS QryGroup,c.item_desp,IFNULL(m.U_US,0),IFNULL(m.U_FS,0),m.QryGroup3,m.SVolume,m.SWeight1,");
            filedName.Append("(CASE m.QryGroup1 WHEN 'N' THEN 0 ELSE '0.5' END) AS QryGroup1,");
            filedName.Append("(CASE m.QryGroup2 WHEN 'N' THEN 0 ELSE '3' END) AS QryGroup2,");
            filedName.Append("(CASE m.QryGroup3 WHEN 'N' THEN 0 ELSE '2' END) AS _QryGroup3,m.U_JGF1,IFNULL(m.U_YFCB,'0'),m.MinLevel,m.PurPackUn,c.item_counts,m.buyunitmsr");
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
                OwnerCode = "1",//当前操作人COde
                SboId = sboID.ToString(),
                UserId = userID.ToString(),
                Printed = "N",
                LicTradNum = "0",
                FuncId = funcId.ToString(),
                DocStatus = "O",
                CurSource = "",
                billBaseType = "-1",
                billBaseEntry = "-1",
                Address = order.Address,
                Address2 = order.Address2,
                BeforeDiscSum = order.BeforeDiscSum,
                CardCode = order.CardCode,
                CardName = order.CardName,
                CntctCode = order.CntctCode,
                Comments = order.Comments,
                CustomFields = $"U_ShipName≮1≯≮0≯U_SCBM≮1≯P3-陈友祥",
                DiscPrcnt = order.DiscPrcnt,
                GoodsToDay = order.GoodsToDay,
                DiscSum = order.DiscSum,
                DocCur = order.DocCur,
                DocDate = order.DocDate,
                DocDueDate = order.DocDueDate,
                DocRate = order.DocRate,
                DocTotal = order.DocTotal,
                DocType = order.DocType,
                GoodsToDate = order.GoodsToDate,
                GoodsToPro = "0.00",
                GroupNum = order.GroupNum,
                Indicator = order.Indicator,
                NumAtCard = order.NumAtCard,
                PartSupply = order.PartSupply,
                PayBefShip = order.PayBefShip,
                PayToCode = order.PayToCode,
                PeyMethod = order.PeyMethod,
                PrepaData = order.PrepaData,
                PrepaPro = order.PrepaPro,
                Remark = order.Remark,
                ShipToCode = order.ShipToCode,
                SlpCode = order.SlpCode,
                U_YWY = order.U_YWY,
                TaxDate = order.TaxDate,
                TotalExpns = order.TotalExpns,
                TrnspCode = order.TrnspCode,
                U_FPLB = order.U_FPLB,
                U_SL = order.U_SL,
                VatGroup = order.VatGroup,
                VatSum = order.VatSum,
                WhsCode = order.WhsCode,
                attachmentData = new List<billAttchment>(),
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
                    BaseEntry = item.BaseEntry,
                    BaseLine = item.BaseLine,
                    BaseRef = item.BaseRef,
                    BaseType = item.BaseType,
                    DiscPrcnt = item.DiscPrcnt,
                    Dscription = item.Dscription,
                    ItemCfgId = item.ItemCfgId,
                    ItemCode = item.ItemCode,
                    LineTotal = item.LineTotal,
                    OnHand = item.OnHand,
                    Price = item.Price,
                    PriceAfVAT = item.PriceAfVAT,
                    PriceBefDi = item.PriceBefDi,
                    Quantity = item.Quantity,
                    StockPrice = item.StockPrice,
                    TargetType = item.TargetType,
                    TotalFrgn = item.TotalFrgn,
                    TrgetEntry = item.TrgetEntry,
                    U_DL = item.U_DL,
                    U_DY = item.U_DY,
                    U_PDXX = item.U_PDXX,
                    U_SCTCJE = item.U_SCTCJE,
                    U_TDS = item.U_TDS,
                    U_XSTCBL = item.U_XSTCBL,
                    U_YF = item.U_YF,
                    U_YFTCJE = item.U_YFTCJE,
                    WhsCode = item.WhsCode,
                    IsExistMo = item.IsExistMo,
                    U_SHJSDJ = item.U_SHJSDJ,
                    U_SHJSJ = item.U_SHJSJ,
                    U_SHTC = item.U_SHTC,
                    U_ZS = item.U_ZS,
                };
                billDelivery.billSalesDetails.Add(billSalesDetail);
            }
            return billDelivery;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>

        /// <returns></returns>
        public bool UpdateSalesDocAttachment(BillDelivery model) {
            #region 删除原附件数据
            string sql = string.Format("DELETE FROM nsap_oa.file_main WHERE docEntry={0} AND file_type_id = {1}", model.docEntry, model.filetypeId);
            UnitWork.ExcuteSqlTable(ContextType.NsapOaDbContextType, sql, CommandType.Text, null);
            #endregion
            #region 新增附件数据
            foreach (var item in model.deatil) {

                StringBuilder nSql = new StringBuilder();
                nSql.AppendFormat("INSERT INTO nsap_oa.file_main (file_sn,file_nm,file_type_id,docEntry,job_id,file_ver,acct_id,issue_reason,file_path,content,remarks,file_status,upd_dt,view_file_path,sbo_id) VALUES");
                nSql.AppendFormat("(1,'{0}',{1},{2},0,'A0',8,'','{3}','','',0,'{4}','{5}',{6});", item.filename, model.filetypeId, model.docEntry, item.filepath, DateTime.Now, item.filepath, item.filesboid);
                UnitWork.ExcuteSqlTable(ContextType.NsapOaDbContextType, nSql.ToString(), CommandType.Text, null);
            }
            #endregion
            return true;
        }


        #region 合约评审
        public string GridRelationContractList(int pageSize, int pageIndex, string filterQuery, string sortname, string sortorder, string itemCode, string cardCode, string sboID) {
            int rowCount = 0;
            string sortString = string.Empty;
            string filterString = string.Empty;
            int rowcounts = 0;
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
                sortString = string.Format(" {0} {1}", sortname, sortorder.ToUpper());
            filterString = string.Format(" sbo_id={0} and itemcode='{1}' and CardCode='{2}'", sboID, itemCode.FilterSQL(), cardCode);
            #region 搜索条件
            if (!string.IsNullOrEmpty(filterQuery)) {
                string[] fields = filterQuery.Split('`');
                string[] p = fields[0].Split(':');
                if (!string.IsNullOrEmpty(p[1])) {
                    filterString += string.Format(" and contract_id={0} ", p[1].FilterSQL().Trim());
                }
            }
            #endregion
            return GridRelationContractListNos(out rowCount, pageSize, pageIndex, filterString, sortString);
        }
        public string GridRelationContractListNos(out int rowsCount, int pageSize, int pageIndex, string filterQuery, string orderName) {
            string tablename = string.Format(" {0}.sale_contract_review", "nsap_bone");
            string fieldname = " sbo_id,contract_id,price,qty,sum_total,deliver_dt,walts,comm_rate,custom_req";
            return SelectPagingHaveRowsCount(tablename, fieldname, pageSize, pageIndex, orderName, filterQuery, out rowsCount);
        }
        public string SelectPagingHaveRowsCount(string tableName, string fieldName, int pageSize, int pageIndex, string strOrder, string strWhere, out int rowsCount) {
            return SelectPaging(tableName, fieldName, pageSize, pageIndex, strOrder, strWhere, 1, out rowsCount);
        }
        private string SelectPaging(string tableName, string fieldName, int pageSize, int pageIndex, string strOrder, string strWhere, int isTotal, out int rowsCount) {
            string code = "";
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
            code = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, "nsap_base.sp_common_pager", CommandType.StoredProcedure, sqlParameters)?.ToString();
            rowsCount = isTotal == 1 ? Convert.ToInt32(sqlParameters[7].Value) : 0;
            return code;

        }
        #endregion
        #region 复制生产订单
        public string CopyProductToSaleSelect(int pageSize, int pageIndex, string filterQuery, string sortname, string sortorder, int sboID) {
            int rowCount = 0;
            string sortString = string.Empty;
            string filterString = string.Empty;
            string line = string.Empty;
            filterString = " a.sbo_id = " + sboID;
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder)) {
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());
            }
            #region 搜索条件
            if (!string.IsNullOrEmpty(filterQuery)) {
                string[] fields = filterQuery.Split('`');
                string[] p = fields[0].Split(':');
                if (!string.IsNullOrEmpty(p[1])) {
                    filterString += " AND a.DocEntry like'%" + p[1].FilterWildCard() + "%'";
                };
                p = fields[1].Split(':');
                if (!string.IsNullOrEmpty(p[1])) {
                    filterString += " AND a.ItemCode like'%" + p[1].FilterWildCard() + "%'";
                };
                p = fields[2].Split(':');
                if (!string.IsNullOrEmpty(p[1])) {
                    filterString += " AND a.U_WO_LTDW like'%" + p[1].FilterWildCard() + "%'";
                };
            }
            filterString += " AND c.dep_id =58 ";
            #endregion
            return CopyProductToSaleSelectNos(out rowCount, pageSize, pageIndex, filterString, sortString, sboID);
        }

        public string CopyProductToSaleSelectNos(out int rowCounts, int pageSize, int pageIndex, string filterQuery, string orderName, int sboID) {
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
        public string SelectMaterialsInventoryData(string ItemCode, string SboId, bool IsOpenSap, string Operating) {
            DataTable dt = SelectMaterialsInventoryDataNos(ItemCode.FilterESC(), SboId, IsOpenSap, Operating);
            if (Operating == "search" || Operating == "edit") {
                dt.Rows.Add(ItemCode, "", "", "SUM", dt.Compute("SUM(OnHand)", "true"), dt.Compute("SUM(IsCommited)", "true"), dt.Compute("SUM(OnOrder)", "true"), dt.Compute("SUM(Available)", "true"), "0", "0", "0", "");
            }
            return dt.Rows.Count.ToString();
        }
        /// <summary>
        /// 查询物料的库存数据
        /// </summary>
        /// <returns></returns>
        public DataTable SelectMaterialsInventoryDataNos(string ItemCode, string SboId, bool IsOpenSap, string Operating) {
            StringBuilder strSql = new StringBuilder();
            if (IsOpenSap) {
                if (Operating == "search" || Operating == "edit") {
                    strSql.Append("SELECT b.ItemCode,b.WhsCode,a.WhsName,b.Locked,b.OnHand,b.IsCommited,b.OnOrder,");
                    strSql.Append("(b.OnHand+b.OnOrder-b.IsCommited) as Available,b.MinStock,b.MaxStock,b.AvgPrice,b.U_KW ");
                    strSql.Append("FROM OWHS a LEFT JOIN OITW b ON a.WhsCode=b.WhsCode ");
                    strSql.Append("WHERE b.ItemCode=@ItemCode ORDER BY b.WhsCode ASC");
                    SqlParameter[] para = {
                        new SqlParameter("@ItemCode", ItemCode)
                    };
                    return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, para);
                } else {
                    strSql.Append("SELECT '' AS ItemCode,WhsCode,WhsName,'N' AS Locked,'0' AS OnHand,'0' AS IsCommited,");
                    strSql.Append("'0' AS OnOrder,'0' AS Available,'' AS MinStock,'' AS MaxStock,'0' AS AvgPrice,");
                    strSql.Append("'' AS U_KW FROM OWHS ORDER BY WhsCode ASC");
                    return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text);
                }
            } else {
                if (Operating == "search" || Operating == "edit") {
                    strSql.Append("SELECT b.ItemCode,b.WhsCode,a.WhsName,b.Locked,b.OnHand,b.IsCommited,b.OnOrder,");
                    strSql.Append("(b.OnHand+b.OnOrder-b.IsCommited) as Available,b.MinStock,b.MaxStock,b.AvgPrice,b.U_KW ");
                    strSql.AppendFormat("FROM {0}.store_OWHS a LEFT JOIN {0}.store_OITW b ON a.sbo_id=b.sbo_id AND a.WhsCode=b.WhsCode ", "nsap_bone");
                    strSql.Append("WHERE b.sbo_id=?sbo_id AND b.ItemCode=?ItemCode ORDER BY b.WhsCode ASC");
                    SqlParameter[] para = {
                        new SqlParameter("?sbo_id",SboId),
                        new SqlParameter("?ItemCode",ItemCode)


                    };
                    return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, para);

                } else {
                    strSql.Append("SELECT '' AS ItemCode,WhsCode,WhsName,'N' AS Locked,'0' AS OnHand,'0' AS IsCommited,");
                    strSql.Append("'0' AS OnOrder,'0' AS Available,'0' AS MinStock,'0' AS MaxStock,'0' AS AvgPrice,'' AS U_KW ");
                    strSql.AppendFormat("FROM {0}.store_OWHS WHERE sbo_id=?sbo_id ORDER BY WhsCode ASC", "nsap_bone");
                    IDataParameter[] para = {
                        new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?sbo_id",    SboId)
                    };
                    return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, para);

                }
            }
        }

        #endregion
    }
}
