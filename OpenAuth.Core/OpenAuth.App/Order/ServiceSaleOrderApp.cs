﻿extern alias MySqlConnectorAlias;
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
            sortString = string.Format("{0} {1}", "a.docentry", "desc".ToUpper());
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
                    object fileflag = UnitWork.ExcuteSql<ResultOrderDto>(ContextType.NsapBaseDbContext, strSql2, CommandType.Text, null).FirstOrDefault();
                    temprow["AttachFlag"] = fileflag == null ? "0" : fileflag.ToString();
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
            //if (loginContext == null)
            //{
            //    throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            //}
            //业务员Id
            var selectOption = UnitWork.Find<crm_oslp>(null).Select(zw => new SelectOption { Key = zw.SlpCode.ToString(), Option = zw.SlpName }).ToList();
            return selectOption;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderReq"></param>
        /// <returns></returns>
        public string Save(AddOrUpdateOrderReq orderReq)
        {
            int UserID = _serviceBaseApp.GetUserNaspId();
            string funcId = "0";
            string logstring = "";
            string jobname = "";
            string result = "";
            try
            {
                int sboID = _serviceBaseApp.GetUserNaspSboID(UserID);
                byte[] job_data = ByteExtension.ToSerialize(orderReq.Order);
                if (orderReq.Copy == "1")
                {
                    funcId = _serviceBaseApp.GetFuncsByUserID("sales/SalesOrder.aspx", UserID).ToString();
                    logstring = "根据销售报价单下销售订单";
                    jobname = "销售订单";
                    //  billNo = NSAP.Biz.Sales.BillDelivery.SalesDeliverySave_ORDR(rData, ations, JobId, UserID, int.Parse(funcId), "0", jobname, SboID, IsTemplate);
                }
                else
                {
                    string className = "NSAP.B1Api.BOneOQUT";
                    funcId = _serviceBaseApp.GetFuncsByUserID("sales/SalesQuotation.aspx", UserID).ToString();
                    logstring = "新建销售报价单";
                    jobname = "销售报价单";
                    int FuncID = int.Parse(funcId);
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
                                string _jobID = result;
                                if ("0" != WorkflowSubmit(int.Parse(result), UserID, orderReq.Order.Remark, "", 0))
                                {
                                    #region 更新商城订单状态
                                    WfaEshopStatus thisinfo = new WfaEshopStatus();
                                    thisinfo.JobId = int.Parse(result);
                                    thisinfo.UserId = UserID;
                                    thisinfo.SlpCode = int.Parse(orderReq.Order.SboId);
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
                        result = WorkflowSubmit(orderReq.JobId, UserID, orderReq.Order.Remark, "", 0);
                    }
                }
                UnitWork.Save();
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
            if (carCode != "")
            {
                var crmOcrd = UnitWork.FindSingle<crm_ocrd>(zw => zw.sbo_id == sboID && zw.CardCode == carCode);
                if (crmOcrd != null)
                {
                    carName = crmOcrd.CardName;
                }
            }
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
        public bool SaveJobPara(string jobID, string setNumber)
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
                para_val = setNumber == "" ? "1" : setNumber,
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
    }
}
