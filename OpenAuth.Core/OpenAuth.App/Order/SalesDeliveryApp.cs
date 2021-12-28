extern alias MySqlConnectorAlias;
using Infrastructure.Extensions;
using OpenAuth.App.Interface;
using OpenAuth.App.Order.Request;
using OpenAuth.Repository;
using OpenAuth.Repository.Interface;
using System.Collections.Generic;
using System.Data;
using System.Text;
using NSAP.Entity.Sales;
using System.Threading.Tasks;
using OpenAuth.App.Order.ModelDto;
using OpenAuth.Repository.Extensions;
using System.Linq;
using Infrastructure;
using System;

namespace OpenAuth.App.Order
{
    /// <summary>
    /// 销售订单业务
    /// </summary>
    public partial class SalesDeliveryApp : OnlyUnitWorkBaeApp
    {
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;


        public SalesDeliveryApp(IUnitWork unitWork, IAuth auth, ServiceSaleOrderApp serviceSaleOrderApp) : base(unitWork, auth)
        {
            this._serviceSaleOrderApp = serviceSaleOrderApp;

        }
        public async Task<string> SalesDeliverySave(SalesDeliverySaveReq salesDeliverySaveReq, int UserID, int FuncID, string jobdata,
            string jobname, int SboID, string IsTemplate)
        {
            string result = "", className = "";
            if (jobname == "销售交货") { className = "NSAP.B1Api.BOneODLN"; }
            billDelivery Model = _serviceSaleOrderApp.GetDeliverySalesInfoNewNos(salesDeliverySaveReq.JobId.ToString(), 7);
            Model.DocStatus = "O";
            Model.Comments += "基于销售订单" + salesDeliverySaveReq.JobId;
            Model.CustomFields = salesDeliverySaveReq.CustomFields;
            #region 必须都有关联订单，并且购买数量与关联订单数量一致,采购订单所有物料高于2次的采购历史，并且价格不高于历史最低价，则不需审批直接通过。
            bool PurPassAudit = false;
            #endregion
            byte[] job_data = ByteExtension.ToSerialize(Model);
            #region 售后人员(部门名称“售后”开头）下的销售订单如果没有设备（物料编号C开头),则审批流程改成呼叫中心审批
            #endregion
            //审批流单据金额
            double jobDocTotal = (double.Parse(Model.DocTotal) > 0 ? double.Parse(Model.DocTotal) : 0);
            if (!string.IsNullOrEmpty(salesDeliverySaveReq.Ations.ToString()))
            {
                if (salesDeliverySaveReq.Ations == OrderAtion.Draft)
                {
                    result = _serviceSaleOrderApp.WorkflowBuild(jobname, FuncID, UserID, job_data, Model.Remark, int.Parse(Model.SboId), Model.CardCode, Model.CardName, jobDocTotal, int.Parse(Model.billBaseType), int.Parse(Model.billBaseEntry), "BOneAPI", className);
                }
                if (salesDeliverySaveReq.Ations == OrderAtion.Submit)
                {
                    result = _serviceSaleOrderApp.WorkflowBuild(jobname, FuncID, UserID, job_data, Model.Remark, int.Parse(Model.SboId), Model.CardCode, Model.CardName, jobDocTotal, int.Parse(Model.billBaseType), int.Parse(Model.billBaseEntry), "BOneAPI", className);
                    if (int.Parse(result) > 0)
                    {
                        int user_id = 0;

                        var par = _serviceSaleOrderApp.SaveJobPara(result, IsTemplate);
                        if (par == "1")
                        {
                            string _jobID = result;
                            if ("0" != _serviceSaleOrderApp.WorkflowSubmit(int.Parse(result), UserID, Model.Remark, "", user_id))
                            {
                                if (!(jobname == "采购订单" && Model.DocType == "S"))
                                {
                                    SaveProOrder(Model, int.Parse(_jobID)).ToString();
                                }
                                if (Model.serialNumber.Count > 0)
                                {
                                    UpdateSerialNumber(Model.serialNumber, int.Parse(_jobID));
                                }
                            }
                            else { result = "0"; }
                        }
                        else { result = "0"; }
                    }

                }
                if (salesDeliverySaveReq.Ations == OrderAtion.Resubmit)
                {
                    if (jobdata == "1")
                    {
                        if (bool.Parse(_serviceSaleOrderApp.UpdateAudit(salesDeliverySaveReq.JobId, job_data, Model.Remark, Model.DocTotal, Model.CardCode, Model.CardName)))
                        {
                            result = _serviceSaleOrderApp.WorkflowSubmit(salesDeliverySaveReq.JobId, UserID, Model.Remark, "", 0);
                        }
                    }
                    else
                    {
                        result = _serviceSaleOrderApp.WorkflowSubmit(salesDeliverySaveReq.JobId, UserID, Model.Remark, "", 0);
                    }
                }
            }
            return result;
        }
        public int SaveProOrder(billDelivery Model, int jobid)
        {
            string SaleOrder = "0";
            string sfileCpbm = "";
            string sNum = "";
            string sTp = "";
            int bresult = 0;
            foreach (var item in Model.billSalesDetails)
            {
                sfileCpbm += item.ItemCode.Replace('★', '"') + ",";
                sNum += item.Quantity + ",";
                if (item.U_TDS == "0" && item.U_DL == "0")
                {
                    sTp += "0" + ",";
                }
                else
                {
                    sTp += "2" + ",";
                }
            }

            if (sfileCpbm != "" && sNum != "")
            {
                sfileCpbm = sfileCpbm.TrimEnd(',');
                sNum = sNum.TrimEnd(',');
                sTp = sTp.TrimEnd(',');
                bresult = WorkBillJob(Model.SboId, jobid, "0", sfileCpbm, SaleOrder, sNum, Model.WhsCode, sTp);
            }
            else
            {
                bresult = 1;
            }
            return bresult;
        }
        public int WorkBillJob(string sbo_id, int jobID, string proNum, string sfileCpbm, string jobidNew, string sNum, string WhsCode, string sTp)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat(" DELETE from {0}.store_drawing_job where  job_idMe ={1} and Typeid =3 AND SalesId={2} ;", "nsap_bone", jobID.ToString(), jobidNew);
            sql.AppendFormat(" INSERT INTO {0}.store_drawing_job(sbo_id, job_idMe, productNum, itemcode, SalesId , projhsl,WhsCode, Typeid, TypeTP, upd_date ) VALUES ", "nsap_bone");
            string[] itemcode = sfileCpbm.Split(',');
            string[] projhsl = sNum.Split(',');
            string[] _sTp = sTp.Split(',');
            for (int i = 0; i < itemcode.Length; i++)
            {
                sql.AppendFormat("( {0}, {1}, {2}, '{3}','{4}', {5} ,'{6}',{7} ,{8},  CURRENT_TIMESTAMP() ) ,",
                    sbo_id, jobID.ToString(), proNum, itemcode[i].Replace("\"", "\\\"").Replace("\'", "\\\'"), jobidNew, projhsl[i], WhsCode, "3", _sTp[i]);
            }
            sql.Remove(sql.Length - 1, 1);
            sql.AppendFormat(" ;");
            int rows = 0;
            try
            {
                rows = UnitWork.ExecuteSql(sql.ToString(), ContextType.NsapBaseDbContext);
            }
            catch
            {
                ;
            }

            return rows > 0 ? 1 : 0;
        }
        //修改已选择序列号状态
        public bool UpdateSerialNumber(IList<billSerialNumber> osrnlist, int submitjobid)
        {
            string strSql = string.Empty;
            int res = 0;
            foreach (billSerialNumber osrn in osrnlist)
            {
                foreach (billSerialNumberChooseItem serial in osrn.Details)
                {
                    strSql = string.Format("INSERT INTO {0}.store_osrn_alreadyexists (ItemCode,SysNumber,DistNumber,MnfSerial,IsChange,JobId) VALUES (?ItemCode,?SysNumber,?DistNumber,?MnfSerial,?IsChange,?JobId) ON Duplicate KEY UPDATE DistNumber=VALUES(DistNumber),MnfSerial=VALUES(MnfSerial),IsChange=VALUES(IsChange),JobId=VALUES(JobId)", "nsap_bone");

                    List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> sqlParameters = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?ItemCode",  osrn.ItemCode),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?SysNumber",  serial.SysSerial),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?DistNumber",  serial.IntrSerial),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?MnfSerial",  serial.SuppSerial),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?IsChange","1"),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?JobId",submitjobid)
            };
                    //strSql = string.Format("UPDATE {0}.store_osrn_alreadyexists SET IsChange=1 WHERE ItemCode = '{1}' AND SysNumber ='{2}'", Sql.BOneDatabaseName, osrn.ItemCode.Replace("'", "''"), serial.SysSerial);
                    res = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, sqlParameters).Rows.Count;
                }
            }
            return res > 0 ? true : false;
        }
        /// <summary>
        /// 交货详情主数据
        /// </summary>
        /// <param name="DocNum"></param>
        /// <param name="SboId"></param>
        /// <param name="tablename"></param>
        /// <param name="ViewCustom"></param>
        /// <param name="ViewSales"></param>
        /// <returns></returns>
        public async Task<Main> QuerySaleDetailsNew(string DocNum, int SboId, string tablename, bool ViewCustom, bool ViewSales)
        {
            string U_YWY = string.Empty;
            if (await IsExistMySql(tablename, "U_YWY"))
            {
                U_YWY = ",U_YWY";
            }
            string strSql = string.Format("SELECT CardCode,IF(" + ViewCustom + ",CardName,'******' ) AS CardName,IF(" + ViewCustom + ",CntctCode,0) AS CntctCode,IF(" + ViewCustom + ",NumAtCard,'******' ) AS NumAtCard,IF(" + ViewCustom + ",DocCur,'') AS DocCur,IF(" + ViewCustom + ",DocRate,0) AS DocRate");
            strSql += string.Format(",DocNum,DocType,IF(" + ViewSales + ",DocTotal,0) AS DocTotal,DocDate,DocDueDate,TaxDate,SupplCode,ShipToCode,PayToCode,Address,Address2,Comments,SlpCode,TrnspCode,GroupNum,PeyMethod,VatPercent,LicTradNum,Indicator,PartSupply,ReqDate,CANCELED");
            strSql += string.Format(",DpmPrcnt,Printed,DocStatus,OwnerCode,U_FPLB,U_SL,U_YGMD{0},sbo_id as SboId", U_YWY);
            string[] tabArray = tablename.Split(new char[] { '_' });
            if (tabArray[0].ToString() != "buy")
            {
                strSql += string.Format(",U_CallID,U_CallName,U_SerialNumber ");
            }
            if (tabArray[0].ToString() == "sale")
            {
                if (tabArray.Length > 1 && tabArray[1].ToString() == "orin")
                {
                    strSql += string.Format(",U_New_ORDRID");
                }
                strSql += string.Format(",IF(" + ViewSales + ",DocTotalFC,0) AS DocTotalFC,IF(" + ViewSales + ",DiscSum,0) AS DiscSum,IF(" + ViewSales + ",DiscSumFC,0) AS DiscSumFC,IF(" + ViewSales + ",DiscPrcnt,0) AS DiscPrcnt");
            }
            strSql += string.Format(" FROM {0}." + tablename + "", "nsap_bone");
            strSql += string.Format(" WHERE DocEntry={0} AND sbo_id={1}", DocNum, SboId);
            DataTable dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            //return dt;
            return dt.Tolist<Main>().FirstOrDefault();
        }



        public async Task<bool> IsExistMySql(string tablename, string filename)
        {
            bool result = false;
            string strSql = string.Format("SELECT COUNT(*) FROM information_schema.columns WHERE table_schema='nsap_bone' AND table_name ='{0}' AND column_name='{1}'", tablename, filename);
            object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            if (obj.ToString() == "0" || obj.ToString() == null)
            { result = false; }
            else { result = true; }
            return result;
        }

        public async Task<Manager> DropPopupOwnerCodeNew(int SboID, string id)
        {
            string strSql = " SELECT empID AS id,CONCAT(lastName,+firstName) AS name FROM " + "nsap_bone" + ".crm_ohem WHERE sbo_id=" + SboID + "";
            if (id != "0")
            {
                strSql += "  AND empID='" + id + "'";
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).Tolist<Manager>().FirstOrDefault();
        }
        public async Task<Sales> DropPopupSlpCodeNew(int SboID, string id)
        {
            string strSql = " SELECT SlpCode AS id,SlpName AS name FROM " + "nsap_bone" + ".crm_oslp WHERE sbo_id=" + SboID + "";
            if (id != "0")
            {
                strSql += " AND SlpCode='" + id + "'";
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).Tolist<Sales>().FirstOrDefault();

        }
        public async Task<List<Mark>> DropPopupIndicatorNew(int sbo_id, string id)
        {
            string strSql = string.Format(" SELECT Code as id,Name AS name FROM {0}.crm_oidc WHERE sbo_id={1}", "nsap_bone", sbo_id);
            if (id != "" && id != "0")
            {
                strSql += string.Format(" AND Code='{0}'", id);
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).Tolist<Mark>();


        }
        /// <summary>
        /// 装运类型
        /// </summary>
        /// <param name="SboId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ShipType> DropPopupTrnspCodeNew(int SboId, string id)
        {
            string strSql = " SELECT TrnspCode AS id,TrnspName AS name FROM " + "nsap_bone" + ".crm_oshp WHERE sbo_id=" + SboId + "";
            if (id != "0")
            {
                strSql += " AND TrnspCode='" + id + "'";
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).Tolist<ShipType>().FirstOrDefault();

        }
        /// <summary>
        /// 付款条款（新）
        /// </summary>
        public async Task<PaymentCond> GetGroupNumNew(int sbo_id, string id)
        {
            string strSql = string.Format(" SELECT GroupNum AS id,PymntGroup AS name FROM {0}.crm_octg WHERE sbo_id={1}", "nsap_bone", sbo_id);
            if (id != "0")
            {
                strSql += string.Format(" AND GroupNum = '{0}'", id);
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).Tolist<PaymentCond>().FirstOrDefault();

        }
        /// <summary>
        /// 货币
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sboId"></param>
        /// <returns></returns>
        public async Task<DocCur> DropPopupDocCurNew(string id, int sboId)
        {
            string strSql = " SELECT CurrCode AS id,CurrName AS name FROM " + "nsap_bone" + ".crm_ocrn WHERE sbo_id = " + sboId.ToString() + "";
            if (id != "0")
            {
                strSql += " AND CurrCode='" + id + "'";
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).Tolist<DocCur>().FirstOrDefault();
        }
        /// <summary>
        /// 获取业务伙伴的所有联系人
        /// </summary>
        public async Task<CntctCode> DropPopupCntctPrsnNew(string Code, int SboId, string id)
        {
            string sql = string.Format("SELECT b.CntctCode AS id,b.Name AS name FROM  " + "nsap_bone" + ".crm_ocrd a LEFT JOIN  " + "nsap_bone" + ".crm_ocpr b ON a.CardCode=b.CardCode WHERE a.CardCode='{0}'", Code);
            if (id != "0")
            {
                sql += string.Format(" AND b.CntctCode='{0}'", id);
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql, CommandType.Text, null).Tolist<CntctCode>().FirstOrDefault();

        }
        public async Task<ShipToCode> GetAddressNew(string AdresType, string CardCode, int SboId)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT Address AS name,CONCAT(IFNULL(ZipCode,''),IFNULL(b.Name,''),IFNULL(c.Name,''),IFNULL(City,''),IFNULL(Building,'')) AS id,a.ZipCode,a.State ");
            sql.AppendFormat(" FROM {0}.crm_crd1 a", "nsap_bone");
            sql.AppendFormat(" LEFT JOIN {0}.store_ocry b ON a.Country=b.Code", "nsap_bone");
            sql.AppendFormat(" LEFT JOIN {0}.store_ocst c ON a.State=c.Code", "nsap_bone");
            sql.AppendFormat(" WHERE AdresType='{0}' AND CardCode='{1}' ", AdresType, CardCode);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql.ToString(), CommandType.Text, null).Tolist<ShipToCode>().FirstOrDefault();

        }
        /// <summary>
        /// 查询指定业务伙伴的科目余额
        /// </summary>
        public async Task<string> SelectBalanceNew(string cardcode, string SboId)
        {
            bool IsOpenSap = _serviceSaleOrderApp.GetSapSboIsOpen(SboId);
            if (IsOpenSap)
            {
                string strSql = string.Format("SELECT ISNULL(Balance,0) AS Balance FROM OCRD WHERE CardCode='{0}'", cardcode);
                object obj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, strSql, CommandType.Text, null);
                return obj == null ? "0" : obj.ToString();
            }
            else
            {
                string strSql = string.Format("SELECT IFNULL(Balance,0) AS Balance FROM {0}.crm_OCRD WHERE sbo_id={1} AND CardCode='{2}'", "nsap_bone", SboId, cardcode);

                object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
                return obj == null ? "0" : obj.ToString();
            }
        }
        /// <summary>
        /// 查询销售员所有客户总的科目余额
        /// </summary>
        public async Task<string> GetSumBalDueNew(string slpCode, string type, string SboId)
        {
            bool IsOpenSap = _serviceSaleOrderApp.GetSapSboIsOpen(SboId);
            if (IsOpenSap)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("SELECT sum(ISNULL(b.Balance,0)) AS Balance FROM OCRD b");
                strSql.AppendFormat(" WHERE b.SlpCode='{0}' and (b.cardtype='C' or b.cardtype='L')", slpCode);
                object obj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
                return obj == null ? "0" : obj.ToString();
            }
            else
            {
                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat("SELECT SUM(a.Debit-a.Credit) AS Total FROM {0}.finance_jdt1 a ", "nsap_bone");
                strSql.AppendFormat("LEFT JOIN {0}.crm_ocrd b ON a.sbo_id=b.sbo_id AND a.ShortName=b.CardCode AND a.ShortName LIKE '{1}%' ", "nsap_bone", type);
                strSql.AppendFormat(" WHERE b.sbo_id={0} AND b.SlpCode={1}", SboId, slpCode);

                object obj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
                return obj == null ? "0" : obj.ToString();
            }
        }


        public async Task<DataTable> GetItemCodeListNew(string DocNum, string tablename, bool ViewSales, int SboID)
        {
            string strSql = string.Format(" SELECT d.DocEntry,d.ItemCode,Dscription,Quantity,IF(" + ViewSales + ",Price,0) AS Price,IF(" + ViewSales + ",d.U_XSTCBL,0) AS U_XSTCBL,IF(" + ViewSales + ",d.U_YWF,0) AS U_YWF,IF(" + ViewSales + ",d.U_FWF,0) AS U_FWF,");
            strSql += string.Format("IF(" + ViewSales + ",LineTotal,0) AS LineTotal,IF(" + ViewSales + ",StockPrice,0) AS StockPrice,d.WhsCode,w.OnHand,");
            strSql += string.Format("(CASE m.QryGroup1 WHEN 'N' THEN 0 ELSE '0.5' END) AS QryGroup1,");
            strSql += string.Format("(CASE m.QryGroup2 WHEN 'N' THEN 0 ELSE '3' END) AS QryGroup2,");
            strSql += string.Format("(CASE m.QryGroup3 WHEN 'N' THEN 0 ELSE '2' END) AS _QryGroup3,d.OpenQty,m.U_JGF,m.U_JGF1,");
            strSql += string.Format("IFNULL(m.U_YFCB,0) AS U_YFCB,IFNULL(d.U_SHJSDJ,0) AS U_SHJSDJ,IFNULL(d.U_SHJSJ,0) AS U_SHJSJ,IFNULL(d.U_SHTC,0) AS U_SHTC,");
            strSql += string.Format("d.LineNum AS BaseLine,d.DocEntry AS BaseEntry,IFNULL(d.U_SHJSJ,0) AS U_SHJSJ,IFNULL(d.U_SHTC,0) AS U_SHTC,U_ZS,IF(" + ViewSales + ",d.PriceBefDi,0) as PriceBefDi,IF(" + ViewSales + ",DiscPrcnt,0) as DiscPrcnt");
            strSql += string.Format(",IF(" + ViewSales + ",TotalFrgn,0) as LineTotalFC");
            strSql += string.Format(" FROM {0}." + tablename + " d", "nsap_bone");
            strSql += string.Format(" LEFT JOIN {0}.store_oitw w ON d.ItemCode=w.ItemCode AND d.WhsCode=w.WhsCode AND d.sbo_id=w.sbo_id", "nsap_bone");
            strSql += string.Format(" LEFT JOIN {0}.store_oitm m ON d.ItemCode=m.ItemCode AND d.sbo_id=m.sbo_id", "nsap_bone");
            strSql += string.Format(" WHERE d.DocEntry={0} AND d.sbo_id={1}", DocNum, SboID);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
        }
        public async Task<DataTable> QuerySalesCustomNew(string DocNum, int SboId, string tablename)
        {


            string customs = await IsMysqlCustomField(tablename);
            if (customs != "")
            {
                string strSql = string.Format("SELECT " + customs + " FROM {0}." + tablename + " ", "nsap_bone");
                strSql += string.Format(" WHERE DocNum={0} AND sbo_id={1}", DocNum, SboId);
                return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            }
            else
            {
                return new DataTable();
            }
        }
        public async Task<string> IsMysqlCustomField(string tableName)
        {
            string strSql = string.Format("SELECT GROUP_CONCAT(column_name,'') FROM information_schema.columns WHERE table_schema='{0}' AND table_name ='{1}' AND column_name in (SELECT AliasID FROM {0}.base_cufd WHERE TableID='{1}')", "nsap_bone", tableName);
            return UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null).ToString();
        }
        public async Task<DataTable> DropPopupWhsCodeNew(int sbo_id, string id)
        {
            string strSql = string.Format(" SELECT WhsCode AS id,WhsName AS name FROM {0}.store_owhs WHERE sbo_id={1}", "nsap_bone", sbo_id);
            if (id != "0")
            {
                strSql += string.Format(" AND WhsCode='{0}'", id);
            }
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }
        /// <summary>
        /// 获取附件信息
        /// </summary>
        public async Task<DataTable> GetFilesList(string DocNum, string TypeId, int SboId)
        {
            string strSql = string.Format("SELECT a.file_id,b.type_nm,a.file_nm,a.remarks,a.file_path,a.upd_dt,c.user_nm,a.view_file_path,a.file_type_id,a.acct_id ");
            strSql += string.Format(" FROM {0}.file_main a", "nsap_bone");
            strSql += string.Format(" LEFT JOIN {0}.file_type b ON a.file_type_id=b.type_id", "nsap_bone");
            strSql += string.Format(" LEFT JOIN {0}.base_user c ON a.acct_id=c.user_id", "nsap_bone");
            if (TypeId == "5")//销售订单附件附带销售提成附件
            {
                strSql += string.Format(" WHERE a.docEntry='{0}' AND a.file_type_id in ({1},37) AND sbo_id={2}", DocNum, TypeId, SboId);
            }
            else
            {//非销售订单附件
                strSql += string.Format(" WHERE a.docEntry='{0}' AND a.file_type_id='{1}' AND sbo_id={2}", DocNum, TypeId, SboId);
            }

            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
        }


        /// <summary>
        /// 草稿保存 提交
        /// </summary>
        /// <param name="salesSaveDraftReq"></param>
        /// <param name="userID"></param>
        /// <param name="sboID"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<string> SalesSaveDraft(SalesSaveDraftReq salesSaveDraftReq, int userID, int sboID)
        {
            string res = "0";
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            if (salesSaveDraftReq.JobId != 0)
            {
                DataTable objTable = _serviceSaleOrderApp.GetAuditObjWithFlowChart(salesSaveDraftReq.JobId.ToString());
                if (objTable.Rows.Count > 0)
                {
                    foreach (DataRow objRow in objTable.Rows)
                    {
                        if (!objRow[0].ToString().Contains(loginContext.User.Name))
                        {
                            return "单据已提交，请勿重复提交";
                        }
                    }
                }
            }
            if (salesSaveDraftReq.Order.FileList != null && salesSaveDraftReq.Order.FileList.Count > 0)
            {
                salesSaveDraftReq.Order.FileList.ForEach(zw =>
                {
                    zw.fileUserId = userID.ToString();
                });
            }
            billDelivery billDelivery = _serviceSaleOrderApp.BulidBillDelivery(salesSaveDraftReq.Order);
            bool PurPassAudit = false;
            if (salesSaveDraftReq.jobType == "opor" && billDelivery.DocType == "I" && billDelivery.billSalesDetails.Count > 0)
            {
                bool zhitong = true;
                foreach (billSalesDetails thedetail in billDelivery.billSalesDetails)
                {//必须都有关联订单，并且购买数量与关联订单数量一致,采购数量+可用量>0必须审批。成品编码必须审批
                    if (thedetail.ItemCode.StartsWith("C"))
                    {
                        zhitong = false;
                        break;
                    }
                    if (!string.IsNullOrEmpty(thedetail.U_RelDoc))
                    {
                        double itemqtyvalid = 0.00;
                        DataTable avatab = await GetOnhandAndAvailable(thedetail.ItemCode.FilterSQL());
                        if (avatab != null && avatab.Rows.Count > 0)
                        {
                            double.TryParse(avatab.Rows[0]["Available"].ToString(), out itemqtyvalid);
                        }
                        itemqtyvalid += double.Parse(thedetail.Quantity);
                        if (double.Parse(thedetail.Quantity) != await GetRelQty(thedetail.ItemCode.FilterSQL(), thedetail.U_RelDoc)
                            || itemqtyvalid > 0.00)
                        {
                            zhitong = false;
                            break;
                        }
                    }
                    else
                    {
                        zhitong = false;
                        break;
                    }
                    //采购订单所有物料高于2次的采购历史，并且价格不高于历史最低价
                    if (!await GetPORValidFlagForAudit(thedetail.ItemCode.FilterSQL(), "3", thedetail.Price))
                    {
                        zhitong = false;
                        break;
                    }
                }
                //总金额多于5000需审批
                double doctotal = 0.00, docrate = 0.00;
                double.TryParse(billDelivery.DocTotal, out doctotal); double.TryParse(billDelivery.DocRate, out docrate);
                if (doctotal * docrate > 5000)
                {
                    zhitong = false;
                }
                if (zhitong)
                {
                    PurPassAudit = true;
                    billDelivery.U_New_ORDRID = "Z";
                }
            }
            byte[] job_data = ByteExtension.ToSerialize(billDelivery);
            if (salesSaveDraftReq.Ations == OrderAtion.DraftUpdate)
            {
                if (bool.Parse(_serviceSaleOrderApp.UpdateAudit(salesSaveDraftReq.JobId, job_data, billDelivery.Remark, billDelivery.DocTotal, billDelivery.CardCode, billDelivery.CardName)))
                {
                    res = "1";
                }
            }
            else if (salesSaveDraftReq.Ations == OrderAtion.DrafSubmit)
            {
                if (bool.Parse(_serviceSaleOrderApp.UpdateAudit(salesSaveDraftReq.JobId, job_data, billDelivery.Remark, billDelivery.DocTotal, billDelivery.CardCode, billDelivery.CardName)))
                {
                    int user_id = 0;
                    if (salesSaveDraftReq.jobType == "opdn" || salesSaveDraftReq.jobType == "orpc" || salesSaveDraftReq.jobType == "orpd")
                    {
                        if (int.Parse(await GetUserIdFromBuy(billDelivery.SlpCode, sboID)) > 0)
                        {
                            user_id = int.Parse(await GetUserIdFromBuy(billDelivery.SlpCode, sboID));
                        }
                        else { user_id = userID; }
                    }
                    if (salesSaveDraftReq.jobType == "orin" || salesSaveDraftReq.jobType == "ordn")
                    {
                        string saleAfterUser = await GetDfTcnician(billDelivery.CardCode, sboID);
                        if (!int.TryParse(saleAfterUser, out user_id))
                        { user_id = userID; }
                    }
                    //运输采购单，或达到条件采购订单不需审批直接通过
                    if ((salesSaveDraftReq.jobType == "opor" && billDelivery.IsTransport == "Y") || PurPassAudit)
                    {
                        res = await UpdateWorkFlowState(salesSaveDraftReq.JobId.ToString());
                        if (res != "0")
                        {
                            return "2";//提示审批通过
                        }
                    }
                    //采购订单，保存第一步跳转参数，（服务单或存在第一次采购物料)
                    if (salesSaveDraftReq.jobType == "opor")
                    {
                        //保存单据类型
                        await UpdateWfaJobPara(salesSaveDraftReq.JobId.ToString(), 2, billDelivery.DocType);
                        bool needf = false;
                        foreach (billSalesDetails tempdet in billDelivery.billSalesDetails)
                        {
                            if (!string.IsNullOrEmpty(tempdet.ItemCode) && !await ExistPorFlag(tempdet.ItemCode.FilterSQL()))
                            {
                                needf = true;
                                break;
                            }
                        }
                        var par = await SaveJobPara(salesSaveDraftReq.JobId.ToString(), needf ? "1" : "0");
                    }

                    res = _serviceSaleOrderApp.WorkflowSubmit(int.Parse(salesSaveDraftReq.JobId.ToString()), userID, billDelivery.Remark, "", user_id);
                    if (int.Parse(res) > 0 && billDelivery.serialNumber.Count > 0)
                    {
                        _serviceSaleOrderApp.UpdateSerialNumber(billDelivery.serialNumber, int.Parse(salesSaveDraftReq.JobId.ToString()));
                    }
                }
            }
            return res;
        }
        public async Task<DataTable> GetOnhandAndAvailable(string ItemCode)
        {
            string strSql = string.Format("SELECT OnHand,IsCommited,OnOrder,MinLevel,(OnHand-IsCommited+OnOrder) AS Available FROM OITM");
            strSql += string.Format(" WHERE ItemCode='{0}'", ItemCode);
            return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql, CommandType.Text, null);
        }
        /// <summary>
        /// 获取物料关联订单的总数量
        /// </summary>
        /// <param name="itemcode"></param>
        /// <param name="relstr"></param>
        /// <returns></returns>
        public async Task<double> GetRelQty(string itemcode, string relstr)
        {
            double relqty = 0.00;
            string oworstr = "", ordrstr = "";
            if (relstr.Contains("生产订单:"))
            {
                oworstr = relstr.Substring(relstr.IndexOf("生产订单:") + 5);
            }
            if (relstr.Contains("销售订单:"))
            {
                if (relstr.Contains(";"))
                {
                    ordrstr = relstr.Substring(5, relstr.IndexOf(";") - 5);
                }
                else
                {
                    ordrstr = relstr.Substring(5);
                }
            }
            string relsql = "";
            if (!string.IsNullOrEmpty(oworstr))
            {
                relsql += string.Format(@" select (v1.PlannedQty-v1.IssuedQty) as openqty from WOR1 v1
                                        left outer join OWOR v0 on v0.DocEntry=v1.DocEntry
                                        where v0.Type='S'AND (v0.Status='R' OR v0.Status='P') AND v1.PlannedQty-v1.IssuedQty>0  
                                        and v1.ItemCode='{0}' and v0.docentry in ({1})", itemcode, oworstr);
            }
            if (!string.IsNullOrEmpty(ordrstr))
            {
                relsql += !string.IsNullOrEmpty(relsql) ? "UNION ALL" : "";
                relsql += string.Format(@" select t1.OpenQty as openqty from RDR1 t1 
                                        left outer join ORDR t0 on t0.DocEntry=t1.DocEntry
                                        where t0.CANCELED='N' and t1.LineStatus='O' and t1.ItemCode='{0}' and t0.docentry in ({1})", itemcode, ordrstr);
            }
            if (!string.IsNullOrEmpty(relsql))
            {
                relsql = "select sum(openqty) from (" + relsql + ") v1";
                object qtyobj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, relsql, CommandType.Text, null);
                double.TryParse(qtyobj.ToString(), out relqty);
            }
            return relqty;
        }
        /// <summary>
        /// 判断采购订单物料达到条件否（比如：采购次数不低于3次并且采购价格不高于历史最低价）
        /// </summary>
        /// <param name="itemcode"></param>
        /// <param name="con1"></param>
        /// <param name="con2"></param>
        /// <returns></returns>
        public async Task<bool> GetPORValidFlagForAudit(string itemcode, string con1, string con2)
        {
            string sqlstr = string.Format(@"select 1 from por1 t1 inner join opor t0 on t0.docentry = t1.docentry
                                                where((linestatus = 'C' AND trgetentry is not null) or linestatus = 'O') and t0.canceled = 'N' and t1.price>0
                                                and t1.itemcode = '{0}' HAVING COUNT(t1.DOCENTRY)>={1} and Min(t1.price)>={2}", itemcode, con1, con2);
            object relobj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, sqlstr, CommandType.Text, null);
            return relobj == null ? false : true;
        }
        #region 获取采购员对应的用户
        /// <summary>
        /// 获取采购员对应的用户
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetUserIdFromBuy(string sale_id, int sbo_id)
        {
            string strSql = string.Format("SELECT user_id FROM {0}.sbo_user WHERE sale_id={1} AND sbo_id={2} LIMIT 1", "nsap", sale_id, sbo_id);
            object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            return obj == null ? "0" : obj.ToString();
        }
        #endregion
        #region 获取业务伙伴收售后主管
        /// <summary>
        /// 获取业务伙伴收售后主管
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetDfTcnician(string CardCode, int sbo_id)
        {
            //string strSql = string.Format("SELECT user_id FROM {0}.sbo_user WHERE tech_id=", Sql.BaseDatabaseName);
            //strSql += string.Format("IFNULL((SELECT DfTcnician FROM {0}.crm_ocrd WHERE CardCode=?CardCode AND sbo_id=?sbo_id),0) AND sbo_id=?sbo_id ORDER BY upd_dt DESC LIMIT 1", Sql.BOneDatabaseName);

            string strSql = string.Format(@"SELECT t0.user_id FROM {0}.sbo_user t0
                                            INNER JOIN {0}.base_user u on u.user_id=t0.user_id
                                            LEFT JOIN {1}.crm_ohem h on h.empid=t0.tech_id and h.sbo_id=t0.sbo_id
                                            WHERE u.user_nm=CONCAT(h.lastName,h.firstName) and t0.sbo_id={3} 
                                           and t0.tech_id=IFNULL((SELECT DfTcnician FROM {1}.crm_ocrd WHERE CardCode={2} AND sbo_id={3}),0) ORDER BY t0.upd_dt DESC LIMIT 1", "nsap_base", "nsap_bone", CardCode, sbo_id);

            object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
            return obj == null ? "0" : obj.ToString();
        }
        #endregion
        #region updata work flow state
        public async Task<string> UpdateWorkFlowState(string job_id)
        {
            string sql = string.Format("UPDATE {0}.wfa_job SET job_state=3 WHERE job_id={1};", "nsap_base", job_id);
            object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
            if (obj == null)
                return "0";
            else
                return Convert.ToInt32(obj.ToString()) > 0 ? "1" : "0";
        }

        #endregion
        #region 修改流程任务参数值
        /// <summary>
        /// 修改流程任务参数值
        /// </summary>
        public async Task<bool> UpdateWfaJobPara(string jobId, int para_idx, string para_val)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("INSERT INTO {0}.wfa_job_para(job_id,para_idx,para_val)", "nsap_base");
            strSql.AppendFormat("VALUES({0},{1},{2}) ON DUPLICATE KEY UPDATE para_val=VALUES(para_val)", jobId, para_idx, para_val);
            int rows = UnitWork.ExecuteSql(strSql.ToString(), ContextType.NsapBaseDbContext);
            return rows > 0 ? true : false;
        }
        #endregion
        /// <summary>
        /// 判断物料是否采购过
        /// </summary>
        /// <param name="itemcode"></param>
        /// <returns></returns>
        public async Task<bool> ExistPorFlag(string itemcode)
        {
            string sqlstr = string.Format(@"select 1 from por1 t1 inner join opor t0 on t0.docentry = t1.docentry
                                                where((linestatus = 'C' AND trgetentry is not null) or linestatus = 'O') and t0.canceled = 'N'
                                                and t1.itemcode = '{0}'", itemcode);
            object relobj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, sqlstr, CommandType.Text, null);
            return relobj == null ? false : true;
        }
        #region 保存审核参数
        /// <summary>
        /// 保存审核参数
        /// </summary>
        /// <returns></returns>
        public async Task<string> SaveJobPara(string jobID, string setNumber)
        {
            string strSql = string.Format("INSERT INTO {0}.wfa_job_para (job_id,para_idx,para_val) VALUES({1},{2},{3})", "nsap_base", jobID, 1, setNumber == "" ? "1" : setNumber);
            strSql += string.Format(" ON Duplicate KEY UPDATE ");
            strSql += string.Format("para_val=VALUES(para_val)");

            return UnitWork.ExecuteSql(strSql, ContextType.NsapBaseDbContext) > 0 ? "1" : "0";
        }
        #endregion
    }
}
