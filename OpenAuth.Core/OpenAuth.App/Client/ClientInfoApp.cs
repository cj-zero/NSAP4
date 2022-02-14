extern alias MySqlConnectorAlias;
using OpenAuth.App.Clue.Request;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Extensions;
using Newtonsoft.Json;
using NSAP.Entity.Client;
using NSAP.Entity.Sales;
using OpenAuth.App.Client.Request;
using OpenAuth.App.Order;
using OpenAuth.Repository;
using billAttchment = NSAP.Entity.Sales.billAttchment;
using clientCRD1 = NSAP.Entity.Client.clientCRD1;
using clientOCPR = NSAP.Entity.Client.clientOCPR;

namespace OpenAuth.App.Client
{
    public class ClientInfoApp : OnlyUnitWorkBaeApp
    {
        ServiceBaseApp _serviceBaseApp;
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
        public ClientInfoApp(ServiceSaleOrderApp serviceSaleOrderApp, ServiceBaseApp serviceBaseApp, IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
            _serviceBaseApp = serviceBaseApp;
            _serviceSaleOrderApp = serviceSaleOrderApp;
        }
        /// <summary>
        /// 客户新增草稿 修改草稿
        /// </summary>
        /// <param name="addClientInfoReq"></param>
        /// <param name="isEdit"></param>
        /// <returns></returns>
        public async Task<string> AddClientAsync(AddClientInfoReq addClientInfoReq, bool isEdit)
        {
            string result = "";
            int userID = _serviceBaseApp.GetUserNaspId();
            clientOCRD OCRD = BulidClientJob(addClientInfoReq.clientInfo);
            OCRD.SboId = "1";
            if (OCRD.CardType == "S")
            {
                addClientInfoReq.funcId = Convert.ToInt32(_serviceSaleOrderApp.GetJobTypeByAddress("client/supplierAudit.aspx"));
            }
            if (!string.IsNullOrEmpty(OCRD.CardNameCore.Trim())) { OCRD.U_Name = OCRD.CardNameCore; }
            string rJobNm = string.Format("{0}{1}", isEdit ? "修改" : "添加", OCRD.CardType == "S" ? "供应商" : "业务伙伴");
            byte[] job_data = ByteExtension.ToSerialize(OCRD);
            if (addClientInfoReq.submitType == "Temporary")
            {
                result = _serviceSaleOrderApp.WorkflowBuild(rJobNm, addClientInfoReq.funcId, userID, job_data, OCRD.FreeText.FilterESC(), Convert.ToInt32(OCRD.SboId), OCRD.CardCode, OCRD.CardName, 0, 0, 0, "BOneAPI", "NSAP.B1Api.BOneOCRD");
                bool updParaCardCode = UpdateWfaJobPara(result, 1, OCRD.CardCode);
                bool updParaCardName = UpdateWfaJobPara(result, 2, OCRD.CardName);
                bool updParaOperateType = UpdateWfaJobPara(result, 3, OCRD.ClientOperateType);
                bool updParaAppChange = UpdateWfaJobPara(result, 4, OCRD.IsApplicationChange);
            }
            else if (addClientInfoReq.submitType == "Submit")
            {
                result = _serviceSaleOrderApp.WorkflowBuild(rJobNm, addClientInfoReq.funcId, userID, job_data, OCRD.FreeText.FilterESC(), Convert.ToInt32(OCRD.SboId), OCRD.CardCode, OCRD.CardName, 0, 0, 0, "BOneAPI", "NSAP.B1Api.BOneOCRD");
                bool updParaCardCode = UpdateWfaJobPara(result, 1, OCRD.CardCode);
                bool updParaCardName = UpdateWfaJobPara(result, 2, OCRD.CardName);
                bool updParaOperateType = UpdateWfaJobPara(result, 3, OCRD.ClientOperateType);

                string JobId = result;
                bool bChangeTcnician = false;
                string ret = "";
                if (int.Parse(result) > 0)
                {
                    //改掉在无相似业务伙伴情况下 直接提交走审批流，但在草稿提交直接通过  
                    if (rJobNm == "添加业务伙伴")
                    {
                        bool updParaAppChange = UpdateWfaJobPara(result, 4, OCRD.IsApplicationChange);//该参数决定流程跳转
                        result = _serviceSaleOrderApp.WorkflowSubmit(int.Parse(result), userID, OCRD.FreeText, "", 0);
                        ret = SaveCrmAuditInfo1(JobId, userID, rJobNm);
                    }
                    else if (rJobNm == "修改业务伙伴")
                    {
                        result = _serviceSaleOrderApp.WorkflowSubmit(int.Parse(result), userID, OCRD.FreeText, "", 0);
                        if (result == "1")
                        {
                            string sfTcnician = OCRD.DfTcnicianCode;
                            string sCardCode = OCRD.CardCode;

                            switch (GetSavefTcnician_sql(sCardCode, sfTcnician))
                            {
                                case "1":
                                    ret = SaveCrmAuditInfo1(JobId, userID, rJobNm);
                                    break;
                                case "0":
                                    SetSavefTcnicianStep_sql(sCardCode, sfTcnician, Convert.ToInt32(JobId));
                                    bChangeTcnician = true;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                if (bChangeTcnician)
                {
                    bool updParaAppChange = UpdateWfaJobPara(JobId, 4, bChangeTcnician.ToString());
                }
                else
                {
                    bool updParaAppChange = UpdateWfaJobPara(JobId, 4, OCRD.IsApplicationChange);
                }
            }
            return result;
        }
        /// <summary>
        /// 查询视图集合
        /// </summary>
        public DataTable SelectClientList(int limit, int page, string query, string sortname, string sortorder, int sboid, int userId, bool rIsViewSales, bool rIsViewSelf, bool rIsViewSelfDepartment, bool rIsViewFull, int depID, out int rowCount)
        {
            bool IsSaler = false, IsPurchase = false, IsTech = false, IsClerk = false;//业务员，采购员，技术员，文员
            string rSalCode = GetUserInfoById(sboid.ToString(), userId.ToString(), "1");
            string rPurCode = GetUserInfoById(sboid.ToString(), userId.ToString(), "2");
            string rTcnicianCode = GetUserInfoById(sboid.ToString(), userId.ToString(), "3");
            if (Convert.ToInt32(rSalCode) > 0)
            {
                IsSaler = true;
            }
            if (Convert.ToInt32(rPurCode) > 0)
            {
                IsPurchase = true;
            }
            if (Convert.ToInt32(rTcnicianCode) > 0)
            {
                IsTech = true;
            }


            bool IsOpenSap = _serviceSaleOrderApp.GetSapSboIsOpen(sboid.ToString());
            string sortString = string.Empty;
            StringBuilder filterString = new StringBuilder();
            filterString.Append(IsOpenSap ? " 1=1 " : string.Format(" sbo_id={0} ", sboid.ToString()));
            if (!rIsViewFull)
            {
                #region 查看本部门
                if (rIsViewSelfDepartment)
                {
                    string filter_str = string.Empty;
                    if (IsPurchase)//采购员
                    {
                        bool isMechanical = OCRDisSpecial(rPurCode, "2", sboid.ToString());
                        if (isMechanical)
                        {
                            filter_str = string.Format(" (CardCode IN ('V00733','V00735','V00836') OR (SlpCode='-1' and (QryGroup2='Y' OR QryGroup3='Y')))", rPurCode);

                        }
                        else
                        {

                            filter_str = string.Format(" (CardCode IN ('V00733','V00735','V00836') OR (SlpCode='-1' and QryGroup2='N')) ", rPurCode);
                        }
                    }

                    filterString.AppendFormat(" {0} AND DfTcnician ={1} ", filter_str, rTcnicianCode);
                }
                #endregion
                #region 查看自己
                if (rIsViewSelf && !rIsViewSelfDepartment)
                {
                    if (!IsSaler && !IsPurchase && !IsTech && !IsClerk)
                    {
                        filterString.AppendFormat(" AND 1<>1 ");
                    }
                    else
                    {

                        int flag = 0;
                        filterString.AppendFormat(" AND ( ");
                        if (IsSaler)//业务员
                        {
                            flag = 1;
                            filterString.AppendFormat(" (SlpCode={0} and CardCode like 'C%') ", rSalCode);
                        }
                        if (IsPurchase)//采购员
                        {
                            if (flag == 1)
                            {
                                filterString.AppendFormat(" OR (SlpCode={0} and CardCode like 'V%') ", rPurCode);
                            }
                            else
                            {
                                flag = 1;

                                string filter_str = string.Empty;
                                bool isMechanical = OCRDisSpecial(rPurCode, "2", sboid.ToString());
                                if (isMechanical)
                                {
                                    filter_str = string.Format(" (CardCode IN ('V00733','V00735','V00836') OR SlpCode ={0} OR ( SlpCode='-1' and (QryGroup2='Y' OR QryGroup3='Y')) ) ", rPurCode);

                                }
                                else
                                {

                                    filter_str = string.Format(" ( CardCode IN ('V00733','V00735','V00836') OR SlpCode ={0} OR ( SlpCode='-1' and QryGroup2='N') ) ", rPurCode);
                                }
                                //filterString.AppendFormat(" (SlpCode={0} OR SlpCode='-1' and CardCode like 'V%') ", rPurCode);
                                filterString.AppendFormat(" ({0} and CardCode like 'V%') ", filter_str);
                            }
                        }
                        if (IsTech)//技术员
                        {
                            if (flag == 1)
                            {
                                filterString.AppendFormat(" OR (DfTcnician={0} and CardCode like 'C%') ", rTcnicianCode);
                            }
                            else
                            {
                                flag = 1;
                                filterString.AppendFormat(" DfTcnician={0} and CardCode like 'C%' ", rTcnicianCode);
                            }
                        }

                        filterString.AppendFormat(" ) ");
                    }
                }
                #endregion
            }
            else
            {
                if ((IsSaler || IsTech || IsClerk) && !IsPurchase)//业务员或技术员,文员
                {
                    filterString.Append(" AND CardCode LIKE 'C%' ");
                }
                else if (IsPurchase && !IsSaler && !IsTech && !IsClerk)//采购员
                {
                    filterString.Append("AND CardCode LIKE 'V%' ");
                }
            }
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());
            if (!string.IsNullOrEmpty(query))
            {
                string[] whereArray = query.Split('`');
                for (int i = 1; i < whereArray.Length; i++)
                {
                    string[] p = whereArray[i].Split(':');
                    if (!string.IsNullOrEmpty(p[1]))
                    {
                        filterString.AppendFormat("AND {0} LIKE '%{1}%' ", p[0], p[1].Trim().FilterSQL().Replace("*", "%"));
                    }
                }
            }
            StringBuilder filedName = new StringBuilder();
            StringBuilder tableName = new StringBuilder();
            DataTable clientTable = new DataTable();

            if (!IsOpenSap) { filedName.Append("sbo_id,"); }
            filedName.Append("CardCode, CardName, SlpName, Technician, CntctPrsn, Address, Phone1, Cellular, ");
            if (rIsViewSales)
            {
                filedName.Append("Balance,  BalanceTotal, DNotesBal, OrdersBal, OprCount, ");
            }
            else
            {
                filedName.Append("'****' AS Balance, '******************************' AS BalanceTotal, '****' AS DNotesBal, '****' AS OrdersBal, '****' AS OprCount, ");
            }
            filedName.Append("UpdateDate , ");
            filedName.Append(" validFor,validFrom,validTo,ValidComm,frozenFor,frozenFrom,frozenTo,FrozenComm ,GroupName,Free_Text");
            filedName.Append(",case when INVTotal90P>0 and Due90>0 then (Due90/INVTotal90P)*100 else 0 end as Due90Percent");

            if (IsOpenSap)
            {
                tableName.Append("(SELECT A.CardCode,A.CardName,B.SlpName,(ISNULL(E.lastName,'')+ISNULL(E.firstName,'')) as Technician,");
                tableName.Append("A.CntctPrsn,(ISNULL(F.Name,'')+ISNULL(G.Name,'')+ISNULL(A.City,'')+ISNULL(CONVERT(NVARCHAR(100),A.Building),'')) AS Address, ");
                tableName.Append("A.Phone1,A.Cellular,");//,A.Balance,ISNULL(A.Balance,0) + ISNULL(H.doctoal,0) AS BalanceTotal
                tableName.Append("A.DNotesBal,A.OrdersBal,A.OprCount,A.UpdateDate,A.SlpCode,A.DfTcnician ");
                tableName.Append(",isnull(A.Balance,0) as Balance,0.00000000 as BalanceTotal ");
                tableName.Append(" , A.validFor,A.validFrom,A.validTo,A.ValidComm,A.frozenFor,A.frozenFrom,A.frozenTo,A.FrozenComm,A.QryGroup2,A.QryGroup3 ");
                tableName.Append(",C.GroupName,A.Free_Text");
                //90天内未清收款金额
                tableName.Append(",isnull(A.Balance,0)+isnull((select SUM(openBal) from ORCT WHERE CANCELED = 'N' AND openBal<>0 and datediff(DAY, docdate, getdate())<= 90 AND CardCode = A.CardCode),0)");
                //90天内未清发票金额
                tableName.Append("-isnull((select SUM(DocTotal - PaidToDate) from OINV WHERE CANCELED = 'N' and DocTotal-PaidToDate > 0 and datediff(DAY, docdate, getdate())<= 90 and CardCode = A.CardCode),0) ");
                //90天内未清贷项金额
                tableName.Append("+isnull((select SUM(DocTotal - PaidToDate) from ORIN where CANCELED = 'N' and DocTotal-PaidToDate > 0 and datediff(DAY, docdate, getdate())<= 90 and CardCode = A.CardCode),0) as Due90");
                //90天前未清发票的发票总额
                tableName.Append(",(select SUM(DocTotal) from OINV WHERE CANCELED = 'N' and DocTotal-PaidToDate > 0 and datediff(DAY, docdate, getdate())> 90 and CardCode = A.CardCode) as INVTotal90P ");
                //
                tableName.Append(" FROM OCRD A ");
                tableName.Append("LEFT JOIN OSLP B ON B.SlpCode=A.SlpCode ");
                tableName.Append("LEFT JOIN OCRG C ON C.GroupCode=A.GroupCode ");
                tableName.Append("LEFT JOIN OIDC D ON D.Code=A.Indicator ");
                tableName.Append("LEFT JOIN OHEM E ON E.empID=A.DfTcnician ");
                tableName.Append("LEFT JOIN OCRY F ON F.Code=A.Country ");
                tableName.Append("LEFT JOIN OCST G ON G.Code=A.State1 ");
                //tableName.Append("LEFT JOIN NSAP.dbo.test_kmye H ON A.CardCode=H.cardcode) T "); //科目余额总账表
                tableName.Append(") T");
                //tableName.Append("LEFT JOIN NSAP.dbo.biz_clerk_tech I ON A.CardCode=I.Cardcode "); //文员，技术员对照表
                clientTable = _serviceSaleOrderApp.SAPSelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), limit, page, sortString, filterString.ToString(), out rowCount);
            }
            else
            {
                tableName.Append("(SELECT A.sbo_id,A.CardCode,A.CardName,B.SlpName,CONCAT(IFNULL(E.lastName,''),IFNULL(E.firstName,'')) as Technician,");
                tableName.Append("A.CntctPrsn,CONCAT(IFNULL(F.Name,''),IFNULL(G.Name,''),IFNULL(A.City,''),IFNULL(A.Building,'')) AS Address, ");
                tableName.Append("A.Phone1,A.Cellular, ");//,A.Balance,H.Balance AS BalanceTotal
                tableName.Append("A.DNotesBal,A.OrdersBal,A.OprCount,A.upd_dt AS UpdateDate,A.SlpCode,A.DfTcnician ");
                tableName.Append(",IFNULL(A.Balance,0) as Balance,0.00 as BalanceTotal ");
                tableName.Append(" , A.validFor,A.validFrom,A.validTo,A.ValidComm,A.frozenFor,A.frozenFrom,A.frozenTo,A.FrozenComm,A.QryGroup2,A.QryGroup3 ");
                tableName.Append(",C.GroupName,A.Free_Text");
                //90天内未清收款金额
                tableName.Append(",IFNULL(A.Balance,0)+IFNULL((select SUM(openBal) from FINANCE_ORCT WHERE CANCELED = 'N' AND openBal<>0 and datediff(NOW(), docdate)<= 90 AND CardCode = A.CardCode),0)");
                //90天内未清发票金额
                tableName.Append("-IFNULL((select SUM(DocTotal - PaidToDate) from SALE_OINV WHERE CANCELED = 'N' and DocTotal-PaidToDate > 0 and datediff(NOW(), docdate)<= 90 and CardCode = A.CardCode),0)");
                //90天内未清贷项金额
                tableName.Append("+IFNULL((select SUM(DocTotal - PaidToDate) from SALE_ORIN where CANCELED = 'N' and DocTotal-PaidToDate > 0 and datediff(NOW(), docdate)<= 90 and CardCode = A.CardCode),0) as Due90");
                //90天前未清发票的发票总额
                tableName.Append(",(select SUM(DocTotal) from SALE_OINV WHERE CANCELED = 'N' and DocTotal-PaidToDate > 0 and datediff(NOW(), docdate)> 90 and CardCode = A.CardCode) as INVTotal90P ");

                tableName.AppendFormat(" FROM {0}.crm_OCRD A  ", "nsap_bone");
                tableName.AppendFormat("LEFT JOIN {0}.crm_OSLP B ON B.SlpCode=A.SlpCode AND B.sbo_id=A.sbo_id ", "nsap_bone");
                tableName.AppendFormat("LEFT JOIN {0}.crm_OCRG C ON C.GroupCode=A.GroupCode AND C.sbo_id=A.sbo_id ", "nsap_bone");
                tableName.AppendFormat("LEFT JOIN {0}.crm_OIDC D ON D.Code=A.Indicator AND D.sbo_id=A.sbo_id ", "nsap_bone");
                tableName.AppendFormat("LEFT JOIN {0}.crm_OHEM E ON E.empID=A.DfTcnician AND E.sbo_id=A.sbo_id ", "nsap_bone");
                tableName.AppendFormat("LEFT JOIN {0}.crm_OCRY F ON F.Code=A.Country ", "nsap_bone");
                tableName.AppendFormat("LEFT JOIN {0}.crm_OCST G ON G.Code=A.State1 ", "nsap_bone");
                //tableName.AppendFormat("LEFT JOIN {0}.crm_balance_sum H ON H.CardCode=A.CardCode) T ", Sql.BOneDatabaseName);
                tableName.Append(") T");
                //tableName.AppendFormat("LEFT JOIN {0}.crm_clerk_tech I ON I.sbo_id=A.sbo_id AND I.CardCode=A.CardCode ", Sql.BOneDatabaseName);
                clientTable = _serviceSaleOrderApp.SelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), limit, page, sortString, filterString.ToString(), out rowCount);
            }
            //统计业务伙伴总的科目余额
            DataTable sbotable = new DataTable();
            sbotable = _serviceSaleOrderApp.DropListSboId();
            foreach (DataRow clientrow in clientTable.Rows)
            {
                decimal totalbalance = 0;
                foreach (DataRow sborow in sbotable.Rows)
                {
                    string sbobalancestr = GetClientSboBalance(clientrow["CardCode"].ToString(), sborow["id"].ToString());
                    decimal sbobalance = 0;
                    if (!string.IsNullOrEmpty(sbobalancestr) && Decimal.TryParse(sbobalancestr, out sbobalance))
                        totalbalance += sbobalance;
                    if (clientTable.Columns.Contains("sbo_id") && clientrow["sbo_id"].ToString() == sborow["id"].ToString())
                        clientrow["Balance"] = sbobalance;
                }
                clientrow["BalanceTotal"] = totalbalance;
            }
            return clientTable;

        }
        /// <summary>
        /// 查詢指定業務夥伴的科目余额（老系统用excel导入的crm_ocrd_oldsbo_balance)
        /// </summary>
        /// <param name="CardCode">客戶代碼</param>
        /// <param name="SboId">賬套</param>
        /// <returns></returns>
        public string GetClientSboBalance(string CardCode, string SboId)
        {
            bool sapflag = _serviceSaleOrderApp.GetSapSboIsOpen(SboId);
            if (sapflag)
            {

                string strSql = string.Format("SELECT Balance FROM OCRD WHERE CardCode = '{0}'", CardCode);
                object sapbobj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, strSql, CommandType.Text, null);
                return sapbobj == null ? "" : sapbobj.ToString();
            }
            else
            {
                string returnstr = "";
                string strSql = string.Format("SELECT Balance FROM {0}.crm_ocrd_oldsbo_balance WHERE sbo_id={1} and CardCode='{2}'", "nsap_bone", SboId, CardCode);

                object balobj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);

                if (balobj != null) { returnstr = balobj.ToString(); }
                return returnstr;
            }
        }
        #region 采购员   客户组分配
        private bool OCRDisSpecial(string rPurCode, string v1, string v2)
        {
            bool ret = false;
            string strSql = string.Format(" SELECT count(*)  FROM {0}.crm_OCQG_assign WHERE sbo_id=?sbo_id AND SlpCode=?SlpCode AND GroupCode=?GroupCode ", "nsap_bone");
            IDataParameter[] strPara = {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?sbo_id", v2),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?SlpCode", rPurCode),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?GroupCode", v1)

            };
            object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text, strPara);
            if (obj != null)
            {
                int num = Convert.ToInt32(obj.ToString());
                if (num > 0)
                {
                    ret = true;
                }

            }
            return ret;
        }
        #endregion
        private string GetUserInfoById(string SboId, string UserId, string SeeType)
        {
            string rRoleNm = "", rFiledName = "";
            if (SeeType == "1")
            {
                rFiledName = "A.sale_id";
                rRoleNm = "销售";
            }
            else if (SeeType == "2")
            {
                rFiledName = "A.sale_id";
                rRoleNm = "采购";
            }
            else if (SeeType == "3")
            {
                rFiledName = "A.tech_id";
                rRoleNm = "技术";
            }
            else
            {
                return "0";
            }
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT {0} FROM {1}.sbo_user A ", rFiledName, "nsap_base");
            strSql.AppendFormat("LEFT JOIN {0}.base_user_role B ON A.user_id=B.user_id  ", "nsap_base");
            strSql.AppendFormat("LEFT JOIN {0}.base_role C ON B.role_id=C.role_id ", "nsap_base");
            strSql.AppendFormat("WHERE A.sbo_id={0} AND A.user_id={1} AND C.role_nm LIKE '%{2}%' AND {3}>0 GROUP BY A.user_id ", SboId, UserId, rRoleNm, rFiledName);
            object strObj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
            return strObj == null ? "0" : strObj.ToString();
        }

        public DataTable GetSellerInfo(string sboId, string userId)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT a.sale_id,b.SlpName FROM {0}.sbo_user a ", "nsap_base");
            strSql.AppendFormat("LEFT JOIN {0}.crm_oslp b ON a.sale_id=b.SlpCode AND a.sbo_id=b.sbo_id ", "nsap_bone");
            strSql.AppendFormat("WHERE a.sbo_id={0} AND a.user_id={1}", sboId, userId);

            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
        }

        /// <summary>
        /// 构建客户草稿
        /// </summary>
        /// <param name="clientInfo"></param>
        /// <returns></returns>
        public clientOCRD BulidClientJob(ClientInfo clientInfo)
        {
            int userID = _serviceBaseApp.GetUserNaspId();
            int sboID = _serviceBaseApp.GetUserNaspSboID(userID);
            int funcId = 50;
            clientOCRD billDelivery = new clientOCRD()
            {
                CardName = clientInfo.CardName,//供应商名称
                CardCode = !string.IsNullOrEmpty(clientInfo.CardCode) ? clientInfo.CardCode : "",//业务伙伴代码
                CardFName = clientInfo.CardFName,//外文名称
                CardType = clientInfo.CardType,//货币类型
                GroupCode = clientInfo.GroupCode,//组代码
                CmpPrivate = clientInfo.CmpPrivate,//公司个人
                AddrType = clientInfo.AddrType,//默认开票到地址类型
                MailAddrTy = clientInfo.MailAddrTy,//默认运达到地址类型
                Address = clientInfo.Address,//默认开票到地址标识
                MailAddres = clientInfo.MailAddres,//默认运达到地址标识
                ZipCode = clientInfo.ZipCode,//收票方邮政编码
                MailZipCod = clientInfo.MailZipCod,//送达方邮政编码
                Phone1 = clientInfo.Phone1,//电话1
                Phone2 = clientInfo.Phone2,//电话2
                Cellular = clientInfo.Cellular,//移动电话
                Fax = clientInfo.Fax,//传真
                E_Mail = clientInfo.E_Mail,//电子邮件
                IntrntSite = clientInfo.IntrntSite,//网站
                CntctPrsn = clientInfo.CntctPrsn,//联系人
                Notes = clientInfo.Notes,//备注
                Balance = clientInfo.Balance,//科目金额
                DNotesBal = clientInfo.DNotesBal,//未清交货单余额
                OrdersBal = clientInfo.OrdersBal,//未清订单余额
                OprCount = clientInfo.OprCount,//未清机会
                GroupNum = clientInfo.GroupNum,//付款条款代码
                LicTradNum = clientInfo.LicTradNum,//国税编号
                ListNum = clientInfo.ListNum,//价格清单编号
                DNoteBalSy = clientInfo.DNoteBalSy,//未清系统货币DN余额
                OrderBalSy = clientInfo.OrderBalSy,//未清系统货币订单余额
                FreeText = clientInfo.FreeText,//自由文本
                SlpCode = clientInfo.SlpCode,//销售代表代码
                Currency = clientInfo.Currency,//业务伙伴货币
                Building = clientInfo.Building,//收款方详细地址
                MailBuildi = clientInfo.MailBuildi,//收货方详细地址
                City = clientInfo.City,//收款方城市
                MailCity = clientInfo.MailCity,//收货城市
                State1 = clientInfo.State1,//收款方省
                State2 = clientInfo.State2,//收货方所在省
                Country = clientInfo.Country,//收款方国家
                MailCountr = clientInfo.MailCountr,//收货国家
                DflAccount = clientInfo.DflAccount,//科目
                DflBranch = clientInfo.DflBranch,//缺省分属
                BankCode = clientInfo.BankCode,//银行名称
                AddID = clientInfo.AddID,//附加标识编号
                FatherType = clientInfo.FatherType,//父类汇总类型
                DscntObjct = clientInfo.DscntObjct,//折扣对象
                DscntRel = clientInfo.DscntRel,//折扣比率
                DataSource = clientInfo.DataSource,//数据源
                Priority = clientInfo.Priority,//优先级别OBPP
                CreditCard = clientInfo.CreditCard,//信用卡
                CrCardNum = clientInfo.CrCardNum,//信用卡编号
                CardValid = clientInfo.CardValid,//信用卡有效性
                UserSign = clientInfo.UserSign,//用户签名
                LocMth = clientInfo.LocMth,//本币对帐
                ValidFor = clientInfo.ValidFor,//活跃期间
                ValidFrom = clientInfo.ValidFrom,//活跃开始日期
                ValidTo = clientInfo.ValidTo,//活跃结束日期 
                FrozenFor = clientInfo.FrozenFor,//冻结期间
                FrozenFrom = clientInfo.FrozenFrom,//冻结从
                FrozenTo = clientInfo.FrozenTo,//冻结至
                ValidComm = clientInfo.ValidComm,//可用备注
                FrozenComm = clientInfo.FrozenComm,//冻结注释
                VatGroup = clientInfo.VatGroup,//计税组
                LogInstanc = clientInfo.LogInstanc,//日志实例
                ObjType = clientInfo.ObjType,//对象类型
                Indicator = clientInfo.Indicator,//标识
                ShipType = clientInfo.ShipType,//装运类型
                DebPayAcct = clientInfo.DebPayAcct,// 应收/应付帐款
                DocEntry = clientInfo.DocEntry,//单据编号
                HouseBank = clientInfo.HouseBank,//开户行
                HousBnkCry = clientInfo.HousBnkCry,//开户行国家
                HousBnkAct = clientInfo.HousBnkAct,//开户行科目
                HousBnkBrn = clientInfo.HousBnkBrn,//开户行分行
                ProjectCod = clientInfo.ProjectCod,//项目代码
                VatIdUnCmp = clientInfo.VatIdUnCmp,//统一国税编号
                AgentCode = clientInfo.AgentCode,//代理商代码
                TolrncDays = clientInfo.TolrncDays,//容差天数
                SelfInvoic = clientInfo.SelfInvoic,//本票
                DeferrTax = clientInfo.DeferrTax,//递延税
                LetterNum = clientInfo.LetterNum,//免税信函号
                MaxAmount = clientInfo.MaxAmount,//最大免税金额
                FromDate = clientInfo.FromDate,//免税有效期从
                ToDate = clientInfo.ToDate,//免税有效期至
                WTLiable = clientInfo.WTLiable,//应征预扣税
                CrtfcateNO = clientInfo.CrtfcateNO,//证书号
                ExpireDate = clientInfo.ExpireDate,//到期日
                NINum = clientInfo.NINum,//登记号
                Industry = clientInfo.Industry,//行业 - 待用
                IndustryC = clientInfo.IndustryC,//行业
                Business = clientInfo.Business,//业务
                AliasName = clientInfo.AliasName,//别名
                DfTcnician = clientInfo.DfTcnician,//售后主管
                Territory = clientInfo.Territory,//地域OTER
                GTSRegNum = clientInfo.GTSRegNum,//金税登记号
                GTSBankAct = clientInfo.GTSBankAct,//金税开户行及账号
                GTSBilAddr = clientInfo.GTSBilAddr,//金税开票地址
                HsBnkSwift = clientInfo.HsBnkSwift,//开户行BIC/SWIFT码
                HsBnkIBAN = clientInfo.HsBnkIBAN,//开户行BIC/SWIFT码
                DflSwift = clientInfo.DflSwift,//缺省银行BIC/SWIFT码
                U_PYSX = clientInfo.U_PYSX,//拼音缩写
                U_Name = clientInfo.U_Name,//简称
                U_FName = clientInfo.U_FName,//外文简称
                U_FPLB = clientInfo.U_FPLB,//发票类别
                SalesVolume = clientInfo.SalesVolume,//销售量
                ServiceFees = clientInfo.ServiceFees,//服务费
                WaitAssign = clientInfo.WaitAssign,//是否待分配客户
                CreateDate = clientInfo.CreateDate,//创建时间
                UpdateDate = clientInfo.UpdateDate,//修改时间
                BillToDef = clientInfo.BillToDef,//默认开票地址
                ShipToDef = clientInfo.ShipToDef,//默认收货地址
                QryGroup1 = clientInfo.QryGroup1,// 属性 1
                QryGroup2 = clientInfo.QryGroup2,// 属性 2
                QryGroup3 = clientInfo.QryGroup3,// 属性 3
                QryGroup4 = clientInfo.QryGroup4,// 属性 4
                QryGroup6 = clientInfo.QryGroup6,// 属性 6
                ClientOperateType = clientInfo.ClientOperateType,//业务伙伴操作类型
                CustomFields = !string.IsNullOrEmpty(clientInfo.CustomFields) ? clientInfo.CustomFields.Replace(" ", "").Replace("　", "") : "",//  $"U_ShipName≮1≯≮0≯U_SCBM≮1≯P3-陈友祥",
                IsActive = clientInfo.IsActive,//业务伙伴状态
                CardNamePrefix = clientInfo.CardNamePrefix,//前缀
                CardNameCore = clientInfo.CardNameCore,//核心名称
                CardNameSuffix = clientInfo.CardNameSuffix,//后缀
                FuncId = clientInfo.FuncId,//页面ID
                SlpName = clientInfo.SlpName,//销售员名称
                DfTcnicianCode = clientInfo.DfTcnicianCode,// 售后主管【技术员编号】
                DfTcnicianHead = clientInfo.DfTcnicianHead,//售后 技术员
                DefAddrBill = clientInfo.DefAddrBill,//默认开票到地址[国家·省·市·详细地址]
                DefAddrShip = clientInfo.DefAddrShip,//默认运达到地址[国家·省·市·详细地址]
                IsApplicationChange = clientInfo.IsApplicationChange,//申请变更
                ChangeType = clientInfo.ChangeType,//变更类型
                ChangeCardCode = clientInfo.ChangeCardCode,//变更的业务伙伴编码
                is_reseller = clientInfo.is_reseller,//是否中间商
                EndCustomerName = clientInfo.EndCustomerName,//终端用户名
                EndCustomerContact = clientInfo.EndCustomerContact,//终端用户联系人
                ContactList = new List<clientOCPR>(),//联系人列表
                AddrList = new List<clientCRD1>(),//地址列表
                FilesDetails = new List<billAttchment>(),//附件列表
                EshopUserId = clientInfo.EshopUserId,

                //  AcctList = new List<clientAcct1>()
            };
            foreach (var item in clientInfo.ContactList)
            {
                clientOCPR clientOCPR = new clientOCPR()
                {
                    SeqId = item.SeqId,//序号
                    SboId = item.SboId,//帐套ID
                    CntctCode = item.CntctCode,//联系人代码
                    CardCode = item.CardCode,//业务伙伴代码
                    Name = item.Name,//联系人名称
                    Position = item.Position,//职位
                    Address = item.Address,//地址
                    Tel1 = item.Tel1,//电话1
                    Tel2 = item.Tel2,//电话2
                    Cellolar = item.Cellolar,//移动电话
                    Fax = item.Fax,//传真
                    E_MailL = item.E_MailL,//电子邮件
                    Pager = item.Pager,//传呼机
                    Notes1 = item.Notes1,//备注1
                    Notes2 = item.Notes2,//备注2
                    DataSource = item.DataSource,//数据源
                    UserSign = item.UserSign,//用户签名
                    Password = item.Password,//密码
                    LogInstanc = item.LogInstanc,//日志实例
                    ObjType = item.ObjType,//对象类型
                    BirthPlace = item.BirthPlace,//出生地
                    BirthDate = item.BirthDate,//生日
                    Gender = item.Gender,//性别
                    Profession = item.Profession,//职业
                    UpdateDate = item.UpdateDate,//更新日期
                    UpdateTime = item.UpdateTime,//更新时间
                    Title = item.Title,//标题
                    BirthCity = item.BirthCity,//出生城市
                    FirstName = item.FirstName,//名
                    MiddleName = item.MiddleName,//中间名
                    LastName = item.LastName,//姓
                    U_ACCT = item.U_ACCT,//账号
                    U_BANK = item.U_BANK,//开户行
                    Active = item.Active,//是/否 可用
                    IsDefault = item.IsDefault//是否 默认
                };
                billDelivery.ContactList.Add(clientOCPR);
            }
            foreach (var item in clientInfo.AddrList)
            {
                clientCRD1 clientCRD1 = new clientCRD1()
                {
                    SeqId = item.SeqId,//序号
                    SboId = item.SboId,//帐套ID
                    CardCode = item.CardCode,//业务伙伴代码
                    Address = item.Address,//地址
                    ZipCode = item.ZipCode,//邮政编码
                    City = item.City,//城市
                    County = item.County,//地区
                    Country = item.Country,//国家
                    State = item.State,//省
                    CountryId = item.CountryId,//国家 - 编号
                    StateId = item.StateId,//省 - 编号
                    LogInstanc = item.LogInstanc,//日志实例
                    ObjType = item.ObjType,//对象类型
                    LicTradNum = item.LicTradNum,//国税编号
                    LineNum = item.LineNum,//行编号
                    TaxCode = item.TaxCode,//税码
                    Building = item.Building,//大楼/楼层/房间
                    AdresType = item.AdresType,//地址类型
                    Address2 = item.Address2,//地址2
                    Address3 = item.Address3,//地址3
                    Active = item.Active,//是/否 可用
                    IsDefault = item.IsDefault//是否 默认
                };
                billDelivery.AddrList.Add(clientCRD1);
            }
            foreach (var item in clientInfo.FilesDetails)
            {
                billAttchment billAttchment = new billAttchment()
                {
                    fileId = item.fileId,//附件ID
                    filetype = item.filetype,//附件类型
                    filetypeId = item.filetypeId,//附件类型Id
                    filename = item.filename,//附件名称
                    realName = item.realName,//附件名称
                    remarks = item.remarks,//附件备注
                    filepath = item.filepath,//附件下载路径
                    attachPath = item.attachPath,//附件预览路径
                    filetime = item.filetime,//上传时间
                    username = item.username,//操作者
                    fileUserId = item.fileUserId//用户Id
                };
                billDelivery.FilesDetails.Add(billAttchment);
            }

            billDelivery.EshopUserId = clientInfo.EshopUserId;
            return billDelivery;
        }
        #region 修改流程任务参数值
        /// <summary>
        /// 修改流程任务参数值
        /// </summary>
        public bool UpdateWfaJobPara(string jobId, int para_idx, string para_val)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("INSERT INTO {0}.wfa_job_para(job_id,para_idx,para_val)", "nsap_base");
            strSql.AppendFormat("VALUES({0},{1},'{2}') ON DUPLICATE KEY UPDATE para_val=VALUES(para_val)", jobId, para_idx, para_val);

            int rows = UnitWork.ExecuteSql(strSql.ToString(), ContextType.NsapBaseDbContext);
            return rows > 0 ? true : false;
        }
        #endregion
        #region 新增第一步自动保存业务伙伴审核的录入方案
        /// <summary>
        /// 保存业务伙伴审核的录入方案
        /// </summary>
        public string SaveCrmAuditInfo1(string JobId, int userID, string rJobNm)
        {
            string AuditType = "Add";
            clientOCRD client = new clientOCRD();
            client = _serviceSaleOrderApp.DeSerialize<clientOCRD>((byte[])(GetAuditInfo(JobId)));
            client.ChangeType = AuditType;
            client.ChangeCardCode = "";
            byte[] job_data = ByteExtension.ToSerialize(client);

            string res = string.Empty;
            res = UpdateAuditJob(JobId, rJobNm, client.FreeText.FilterESC(), job_data, false) ? "1" : "0";
            if (res == "1")
            {
                res = _serviceSaleOrderApp.WorkflowSubmit(Convert.ToInt32(JobId), userID, "", "", 0);
            }
            return res;
        }





        #endregion
        #region 根据jobId获取审核任务信息
        /// <summary>
        /// 根据jobId获取审核任务信息
        /// </summary>
        public object GetAuditInfo(string jobId)
        {
            string sql = string.Format("SELECT job_data FROM {0}.wfa_job WHERE job_id=?{1} ", "nsap_bone", jobId);
            return UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
        }
        #endregion
        #region 根据jobId获取审核任务信息
        /// <summary>
        /// 根据jobId获取审核任务信息
        /// </summary>
        public clientOCRD GetAuditInfoNew(string jobId)
        {
            clientOCRD bill = _serviceSaleOrderApp.DeSerialize<clientOCRD>((byte[])(GetAuditInfo(jobId)));
            return bill;
        }
        #endregion
        /// <summary>
        /// 修改审核数据
        /// </summary>
        public bool UpdateAuditJob(string jobId, string jobName, string remarks, byte[] wfaJob, bool isEditStatus)
        {
            StringBuilder strSql = new StringBuilder();
            int rows = 0;
            if (isEditStatus)
            {
                strSql.AppendFormat("UPDATE IGNORE {0}.wfa_job SET job_nm={1},", "nsap_base", jobName);
                strSql.AppendFormat("job_data={0},remarks={1},job_state={2} WHERE job_id={3}", wfaJob, remarks, 0, jobId);
                rows = UnitWork.ExecuteSql(strSql.ToString(), ContextType.NsapBaseDbContext);
            }
            else
            {
                strSql.AppendFormat("UPDATE IGNORE {0}.wfa_job SET ", "nsap_base");
                strSql.AppendFormat("job_nm={0},job_data={1},remarks={2} WHERE job_id={3}", jobName, wfaJob, remarks, jobId);
                rows = UnitWork.ExecuteSql(strSql.ToString(), ContextType.NsapBaseDbContext);
            }
            return rows > 0 ? true : false;
        }
        /// <summary>
        /// 是否选择新的售后主管
        /// </summary>
        /// <param name="sCardCode"></param>
        /// <param name="sfTcnician"></param>
        /// <returns></returns>
        public string GetSavefTcnician_sql(string sCardCode, string sfTcnician)
        {
            IDataParameter[] strPara = new IDataParameter[]
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pCardCode", sCardCode),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pDfTcnician", sfTcnician)

            };
            string ret = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, string.Format("{0}.sp_userTemp_GetState", "nsap_base"), CommandType.StoredProcedure, strPara).ToString();
            return ret;
        }
        public string SetSavefTcnicianStep_sql(string sCardCode, string sfTcnician, int JobId)
        {

            IDataParameter[] strPara = new IDataParameter[]
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pCardCode", sCardCode),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pDfTcnician", sfTcnician),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pJobId", JobId)

            };
            string ret = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, string.Format("{0}.sp_SetTcnicianStep", "nsap_base"), CommandType.StoredProcedure, strPara).ToString();
            return ret;

        }

    }
}
