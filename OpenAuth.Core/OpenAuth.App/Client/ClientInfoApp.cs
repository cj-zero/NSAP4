extern alias MySqlConnectorAlias;
using OpenAuth.App.Clue.Request;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using NSAP.Entity.Client;
using NSAP.Entity.Sales;
using OpenAuth.App.Client.Request;
using OpenAuth.App.Order;
using OpenAuth.Repository;
using billAttchment = NSAP.Entity.Sales.billAttchment;
using clientCRD1 = NSAP.Entity.Client.clientCRD1;
using clientOCPR = NSAP.Entity.Client.clientOCPR;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Serve;
using clientAcct1 = NSAP.Entity.Client.clientAcct1;
using OpenAuth.Repository.Domain.Customer;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.App.Client.Response;
using Microsoft.AspNetCore.SignalR;
using OpenAuth.App.SignalR;

namespace OpenAuth.App.Client
{
    public class ClientInfoApp : OnlyUnitWorkBaeApp
    {
        ServiceBaseApp _serviceBaseApp;
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
        private readonly IHubContext<MessageHub> _hubContext;
        public ClientInfoApp(ServiceSaleOrderApp serviceSaleOrderApp, ServiceBaseApp serviceBaseApp, IUnitWork unitWork, IAuth auth, IHubContext<MessageHub> hubContext) : base(unitWork, auth)
        {
            _serviceBaseApp = serviceBaseApp;
            _serviceSaleOrderApp = serviceSaleOrderApp;
            _hubContext = hubContext;
        }
        #region 客户新增草稿 修改草稿
        /// <summary>
        /// 客户新增草稿 修改草稿
        /// </summary>
        /// <param name="addClientInfoReq"></param>
        /// <param name="isEdit"></param>
        /// <returns></returns>
        public async Task<string> AddClientAsync(AddClientInfoReq addClientInfoReq, bool isEdit)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            addClientInfoReq.clientInfo.SlpName = loginUser.Name;
            string result = "";
            int userID = _serviceBaseApp.GetUserNaspId();
            clientOCRD OCRD = BulidClientJob(addClientInfoReq.clientInfo);
            OCRD.SboId = "1";
            if (OCRD.CardType == "S")
            {
                addClientInfoReq.funcId = Convert.ToInt32(_serviceSaleOrderApp.GetJobTypeByAddress("client/supplierAudit.aspx"));
            }
            if (OCRD.CardNameCore != null && !string.IsNullOrEmpty(OCRD.CardNameCore.Trim())) { OCRD.U_Name = OCRD.CardNameCore; }
            string rJobNm = string.Format("{0}{1}", isEdit ? "修改" : "添加", OCRD.CardType == "S" ? "供应商" : "业务伙伴");
            byte[] job_data = ByteExtension.ToSerialize(OCRD);
            if (addClientInfoReq.submitType == "Temporary")
            {
                result = _serviceSaleOrderApp.WorkflowBuild(rJobNm, addClientInfoReq.funcId, userID, job_data, OCRD.FreeText.FilterESC(), Convert.ToInt32(OCRD.SboId), OCRD.CardCode, OCRD.CardName, 0, 0, addClientInfoReq.baseEntry, "BOneAPI", "NSAP.B1Api.BOneOCRD");
                bool updParaCardCode = UpdateWfaJobPara(result, 1, OCRD.CardCode);
                bool updParaCardName = UpdateWfaJobPara(result, 2, OCRD.CardName);
                bool updParaOperateType = UpdateWfaJobPara(result, 3, OCRD.ClientOperateType);
                bool updParaAppChange = UpdateWfaJobPara(result, 4, OCRD.IsApplicationChange);
            }
            else if (addClientInfoReq.submitType == "Submit")
            {
                result = _serviceSaleOrderApp.WorkflowBuild(rJobNm, addClientInfoReq.funcId, userID, job_data, OCRD.FreeText.FilterESC(), Convert.ToInt32(OCRD.SboId), OCRD.CardCode, OCRD.CardName, 0, 0, addClientInfoReq.baseEntry, "BOneAPI", "NSAP.B1Api.BOneOCRD");
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
        #endregion
        #region 查询列表
        /// <summary>
        /// 查询列表
        /// </summary>
        public DataTable SelectClientList(int limit, int page, string query, string sortname, string sortorder,
            int sboid, int userId, bool rIsViewSales, bool rIsViewSelf, bool rIsViewSelfDepartment, bool rIsViewFull,
            int depID, string label, string contectTel, string slpName, string isReseller, int? Day, string CntctPrsn,string address, out int rowCount)
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
            //modify by yangis @2022.06.24
            if(!string.IsNullOrWhiteSpace(slpName))
            {
                filterString.Append($" and slpname like '%{slpName}%' ");
            }
            if (!string.IsNullOrWhiteSpace(isReseller))
            {
                filterString.Append($" and U_is_reseller = '{isReseller}' ");
            }
            if (!string.IsNullOrWhiteSpace(contectTel))
            {
                filterString.Append($" and phone1 like '%{contectTel}%' ");
            }
            if (!string.IsNullOrWhiteSpace(CntctPrsn))
            {
                filterString.Append($" and T.cardcode in ( select cardcode from OCPR where name like '%{CntctPrsn}%') ");
            }
            if (!string.IsNullOrWhiteSpace(address))
            {
                filterString.Append($" and T.cardcode in ( SELECT CardCode FROM CRD1 WHERE Building like '" + address + "') ");
            }
            //黑名单客户也不在客户列表上显示
            var blacklist = UnitWork.Find<SpecialCustomer>(c => c.Type == 0).Select(c => c.CustomerNo).ToList();
            if (blacklist.Count() > 0)
            {
                var selectCardCode = new StringBuilder("");
                string codes = "''";
                foreach (var item in blacklist)
                {
                    selectCardCode.Append($",'{item}'");
                    if (!string.IsNullOrWhiteSpace(selectCardCode.ToString()))
                    {
                        codes = selectCardCode.ToString().Substring(1);
                    }
                }

                filterString.Append($" and T.CardCode not in ({codes}) ");
            }

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
                            filter_str = string.Format(" (T.CardCode IN ('V00733','V00735','V00836') OR (SlpCode='-1' and (QryGroup2='Y' OR QryGroup3='Y')))", rPurCode);

                        }
                        else
                        {

                            filter_str = string.Format(" (T.CardCode IN ('V00733','V00735','V00836') OR (SlpCode='-1' and QryGroup2='N')) ", rPurCode);
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
                            filterString.AppendFormat(" (SlpCode={0} and T.CardCode like 'C%') ", rSalCode);
                        }
                        if (IsPurchase)//采购员
                        {
                            if (flag == 1)
                            {
                                filterString.AppendFormat(" OR (SlpCode={0} and T.CardCode like 'V%') ", rPurCode);
                            }
                            else
                            {
                                flag = 1;

                                string filter_str = string.Empty;
                                bool isMechanical = OCRDisSpecial(rPurCode, "2", sboid.ToString());
                                if (isMechanical)
                                {
                                    filter_str = string.Format(" (T.CardCode IN ('V00733','V00735','V00836') OR SlpCode ={0} OR ( SlpCode='-1' and (QryGroup2='Y' OR QryGroup3='Y')) ) ", rPurCode);

                                }
                                else
                                {

                                    filter_str = string.Format(" ( T.CardCode IN ('V00733','V00735','V00836') OR SlpCode ={0} OR ( SlpCode='-1' and QryGroup2='N') ) ", rPurCode);
                                }
                                //filterString.AppendFormat(" (SlpCode={0} OR SlpCode='-1' and CardCode like 'V%') ", rPurCode);
                                filterString.AppendFormat(" ({0} and T.CardCode like 'V%') ", filter_str);
                            }
                        }
                        if (IsTech)//技术员
                        {
                            if (flag == 1)
                            {
                                filterString.AppendFormat(" OR (DfTcnician={0} and T.CardCode like 'C%') ", rTcnicianCode);
                            }
                            else
                            {
                                flag = 1;
                                filterString.AppendFormat(" DfTcnician={0} and T.CardCode like 'C%' ", rTcnicianCode);
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
                    filterString.Append(" AND T.CardCode LIKE 'C%' ");
                }
                else if (IsPurchase && !IsSaler && !IsTech && !IsClerk)//采购员
                {
                    filterString.Append("AND T.CardCode LIKE 'V%' ");
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
                        filterString.AppendFormat("AND T.{0} LIKE '%{1}%' ", p[0], p[1].Trim().FilterSQL().Replace("*", "%"));
                    }
                }
            }
            StringBuilder filedName = new StringBuilder();
            StringBuilder tableName = new StringBuilder();
            DataTable clientTable = new DataTable();

            if (!IsOpenSap) { filedName.Append("sbo_id,"); }
            filedName.Append("T.CardCode,T1.UpdateDate DistributionDate, CardName, SlpName, Technician, CntctPrsn, Address, Phone1, Cellular,U_is_reseller, ");
            if (rIsViewSales)
            {
                filedName.Append("Balance,  BalanceTotal, DNotesBal, OrdersBal, OprCount, ");
            }
            else
            {
                filedName.Append("'****' AS Balance, '******************************' AS BalanceTotal, '****' AS DNotesBal, '****' AS OrdersBal, '****' AS OprCount, ");
            }
            filedName.Append("CreateDate,T.UpdateDate , ");
            filedName.Append(" validFor,validFrom,validTo,ValidComm,frozenFor,frozenFrom,frozenTo,FrozenComm ,GroupName,Free_Text");
            filedName.Append(",case when INVTotal90P>0 and Due90>0 then (Due90/INVTotal90P)*100 else 0 end as Due90Percent");
            var CardCodes = "";
            if (IsOpenSap)
            {
                tableName.Append("(SELECT A.CardCode,A.CardName,B.SlpName,(ISNULL(E.lastName,'')+ISNULL(E.firstName,'')) as Technician,");
                tableName.Append("A.CntctPrsn,(ISNULL(F.Name,'')+ISNULL(G.Name,'')+ISNULL(A.City,'')+ISNULL(CONVERT(NVARCHAR(100),A.Building),'')) AS Address, ");
                tableName.Append("A.Phone1,A.Cellular,A.U_is_reseller,");//,A.Balance,ISNULL(A.Balance,0) + ISNULL(H.doctoal,0) AS BalanceTotal
                tableName.Append("A.DNotesBal,A.OrdersBal,A.OprCount,A.CreateDate,A.UpdateDate,A.SlpCode,A.DfTcnician ");
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

                //筛选标签
                if (!string.IsNullOrWhiteSpace(label))
                {
                    //全部客户
                    if (label == "0") { }
                    //未报价客户
                    if (label == "1" || Day.Value != 0)
                    {
                        //在报价单中不存在的客户
                        tableName.Append(" LEFT JOIN OQUT AS q on A.CardCode = q.CardCode ");
                        tableName.Append(" WHERE q.CardCode IS NULL ");
                        if (Day.Value != 0)
                        {
                            tableName.Append(" and A.CardCode in(SELECT CardCode from (SELECT CardCode, max(UpdateDate) UpdateDate from( select CardCode, SlpCode, min(UpdateDate) UpdateDate from ");
                            tableName.Append("(select CardCode, SlpCode, ISNULL(UpdateDate,CreateDate) UpdateDate from OCRD UNION select CardCode, SlpCode, UpdateDate from ACRD ) a GROUP BY CardCode, SlpCode) b ");
                            tableName.Append(" where DATEDIFF(day, b.UpdateDate, GETDATE()) > " + Day + " GROUP BY CardCode) c) ");
                        }
                    }
                    //已成交客户
                    else if (label == "2")
                    {
                        //在交货单中存在的客户
                        tableName.Append(@" where exists(select 1 from ODLN as n where n.CardCode = A.CardCode) ");
                    }
                    //公海领取(掉入公海的客户被重新分配和领取,但是分配和领取之后没有做过报价单,做过单了就属于正常用户)
                    else if (label == "3")
                    {
                        //这些客户还没有做过单
                        tableName.Append(@" LEFT JOIN OQUT AS q on A.CardCode = q.CardCode ");
                        tableName.Append(" WHERE q.CardCode IS NULL ");
                        //并且在历史归属表中存在但是公海中不存在的客户(说明已被领取)
                        var cardCodes = (from h in UnitWork.Find<CustomerSalerHistory>(q => q.IsSaleHistory == true)
                                         join c in UnitWork.Find<CustomerList>(null) on h.CustomerNo equals c.CustomerNo into temp
                                         from t in temp.DefaultIfEmpty()
                                         where t.CustomerNo == null
                                         select h.CustomerNo).Distinct().ToList();
                        var selectCardCode = new StringBuilder("");
                        string codes = "''";
                        foreach (var item in cardCodes)
                        {
                            selectCardCode.Append($",'{item}'");
                        }
                        if (!string.IsNullOrWhiteSpace(selectCardCode.ToString()))
                        {
                            codes = selectCardCode.ToString().Substring(1);
                        }

                        tableName.Append($" AND A.CardCode IN ({codes}) ");
                    }
                    //即将掉入公海
                    else if (label == "4")
                    {
                        var cardCodes = UnitWork.Find<CustomerList>(c => c.LabelIndex == 4).Select(c => c.CustomerNo).ToList();
                        var selectCardCode = new StringBuilder("");
                        string codes = "''";
                        foreach (var item in cardCodes)
                        {
                            selectCardCode.Append($",'{item}'");
                        }
                        if (!string.IsNullOrWhiteSpace(selectCardCode.ToString()))
                        {
                            codes = selectCardCode.ToString().Substring(1);
                        }

                        tableName.Append($" WHERE A.CardCode IN ({codes}) ");
                    }
                }
                //tableName.Append("LEFT JOIN NSAP.dbo.test_kmye H ON A.CardCode=H.cardcode) T "); //科目余额总账表
                tableName.Append(") T");
                tableName.Append(" LEFT JOIN (SELECT CardCode, max(UpdateDate) UpdateDate from( select CardCode, SlpCode, min(UpdateDate) UpdateDate from (select CardCode, SlpCode, ISNULL(UpdateDate,CreateDate) UpdateDate from OCRD UNION select CardCode, SlpCode, UpdateDate from ACRD ) a GROUP BY CardCode, SlpCode) b  GROUP BY CardCode) T1 on T.CardCode = T1.CardCode ");
                //tableName.Append("LEFT JOIN NSAP.dbo.biz_clerk_tech I ON A.CardCode=I.Cardcode "); //文员，技术员对照表

                //modify by yangsiming @2022.06.16 调用存储过程,当sql语句太长,超过4000个字符,后面的会被截掉,造成报错
                //clientTable = _serviceSaleOrderApp.SAPSelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), limit, page, sortString, filterString.ToString(), out rowCount);
                var sql2 = $" select {filedName.ToString()} from {tableName.ToString()} where {filterString.ToString()} order by {sortString} offset {(page - 1) * limit} rows fetch next {limit} rows only ";
                clientTable = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, sql2, CommandType.Text);
                var sql3 = $" select count(*) from {tableName.ToString()} where {filterString.ToString()}; ";
                rowCount = int.Parse(UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, sql3, CommandType.Text).Rows[0][0].ToString());
                clientTable.Columns.Add("U_ClientSource", typeof(string));//客户来源
                clientTable.Columns.Add("U_CompSector", typeof(string));//所属行业
                clientTable.Columns.Add("U_TradeType", typeof(string));//贸易类型
                clientTable.Columns.Add("FollowUpTime", typeof(string));//最后跟进时间
                clientTable.Columns.Add("FollowUpDay", typeof(string));//未跟进天数
                clientTable.Columns.Add("U_CardTypeStr", typeof(string));//新版客户类型
                clientTable.Columns.Add("base_entry", typeof(int));//新版客户类型

                var Array = from DataRow dr in clientTable.Rows select dr[1].ToString();
                var heet = "";
                foreach (var item in Array)
                {
                    heet += "'" + item + "',";
                }

                CardCodes = heet.TrimEnd(',');
                if (!string.IsNullOrWhiteSpace(CardCodes))
                {
                    //var sql = string.Format("SELECT U_TradeType,U_ClientSource,U_CompSector,U_CardTypeStr,CardCode FROM crm_ocrd WHERE CardCode IN ({0})", CardCodes);
                    //var ClientSource = UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, sql, CommandType.Text, null);

                    //var strsql = string.Format("SELECT A.base_entry,CardCode FROM nsap_base.wfa_job   A LEFT JOIN nsap_bone.crm_ocrd B ON B.CardCode=A.sbo_itf_return WHERE A.job_type_id=72 AND B.CardCode IN ({0})", CardCodes);
                    //var Baseentry = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strsql, CommandType.Text, null);

                    string sql = string.Format(@"SELECT DISTINCT A.base_entry,U_TradeType,U_ClientSource,U_CompSector,U_CardTypeStr,CardCode FROM nsap_base.wfa_job   A 
                        LEFT JOIN nsap_bone.crm_ocrd B ON B.CardCode = A.sbo_itf_return  WHERE B.CardCode IN ({0})", CardCodes);
                    var ClientSource = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
                    List<ClueFollowUp> clueFollowUpList = new List<ClueFollowUp>();
                    if (ClientSource.Rows.Count > 0)
                    {
                        List<int> baseEntryids = (from d in ClientSource.AsEnumerable() select d.Field<int>("base_entry")).ToList();
                        clueFollowUpList = UnitWork.Find<ClueFollowUp>(q => baseEntryids.Contains(q.Id)).ToList();
                    }
                    foreach (DataRow clientrow in clientTable.Rows)
                    {
                        clientrow["FollowUpDay"] = "暂无跟进天数";
                        clientrow["FollowUpTime"] = "暂无跟进时间";
                        if (ClientSource.Rows.Count > 0)
                        {
                            foreach (DataRow clientSource in ClientSource.Rows)
                            {
                                if (clientrow["CardCode"].ToString() == clientSource["CardCode"].ToString())
                                {
                                    clientrow["U_ClientSource"] = clientSource["U_ClientSource"];
                                    clientrow["U_CompSector"] = clientSource["U_CompSector"];
                                    clientrow["U_TradeType"] = clientSource["U_TradeType"];
                                    clientrow["U_CardTypeStr"] = clientSource["U_CardTypeStr"];
                                    clientrow["base_entry"] = clientSource["base_entry"].ToInt();
                                    break;
                                }
                            }
                            //foreach (DataRow item in Baseentry.Rows)
                            //{
                            //    var ClueId = item["base_entry"].ToInt();
                            //    var FollowUpTime = UnitWork.FindSingle<ClueFollowUp>(q => q.Id == ClueId);
                            //    //var strsql4 = string.Format("SELECT FollowUpTime FROM erp4_serve.cluefollowup   WHERE ClueId ={0}   ORDER BY  FollowUpTime  DESC LIMIT 1", item["base_entry"]);
                            //    //var FollowUpTime = UnitWork.ExcuteSqlTable(ContextType.Nsap4ServeDbContextType, strsql4, CommandType.Text, null);
                            //    if (FollowUpTime != null)
                            //    {
                            //        clientrow["FollowUpTime"] = FollowUpTime.FollowUpTime;
                            //        var subTime = (DateTime.Now.Subtract(FollowUpTime.FollowUpTime));
                            //        clientrow["FollowUpDay"] = $"{subTime.Days}天";
                            //    }
                            //}
                        }
                        var ClueId = clientrow["base_entry"].ToInt();
                        foreach (var FollowUpTime in clueFollowUpList)
                        {
                            if (FollowUpTime.ClueId == ClueId)
                            {
                                clientrow["FollowUpTime"] = FollowUpTime.FollowUpTime;
                                var subTime = (DateTime.Now.Subtract(FollowUpTime.FollowUpTime));
                                clientrow["FollowUpDay"] = $"{subTime.Days}天";
                                break;
                            }
                        }
                    }
                }
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
                tableName.AppendFormat("LEFT JOIN {0}.wfa_job H ON H.sbo_itf_return=A.CardCode ", "nsap_base");
                tableName.AppendFormat("LEFT JOIN {0}.clue I ON I.Id=H.base_entry", "nsap_serve");
                tableName.AppendFormat("LEFT JOIN {0}.cluefollowup J ON J.ClueId=I.Id ORDER BY b.FollowUpTime DESC LIMIT 1 ", "nsap_serve");
                //tableName.AppendFormat("LEFT JOIN {0}.crm_balance_sum H ON H.CardCode=A.CardCode) T ", "nsap_bone");
                tableName.Append(") T");
                //tableName.AppendFormat("LEFT JOIN {0}.crm_clerk_tech I ON I.sbo_id=A.sbo_id AND I.CardCode=A.CardCode ", "nsap_bone");
                clientTable = _serviceSaleOrderApp.SelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), limit, page, sortString, filterString.ToString(), out rowCount);
            }


            //统计业务伙伴总的科目余额
            DataTable sbotable = new DataTable();
            sbotable = _serviceSaleOrderApp.DropListSboId();
            var Arrays = from DataRow dr in sbotable.Rows select dr[0].ToString();
            var heets = "";
            foreach (var item in Arrays)
            {
                heets += "'" + item + "',";
            }
            var ids = heets.TrimEnd(',');
            if (!string.IsNullOrWhiteSpace(CardCodes))
            {
                string strmySql = string.Format("SELECT sbo_id,CardCode,Balance FROM nsap_bone.crm_ocrd_oldsbo_balance WHERE CardCode IN ({0})", CardCodes);
                var sbobalancestr = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strmySql, CommandType.Text, null);
                if (sbobalancestr.Rows.Count > 0)
                {
                    foreach (DataRow clientrow in clientTable.Rows)
                    {
                        decimal totalbalance = 0;
                        foreach (DataRow item in sbobalancestr.Rows)
                        {
                            decimal sbobalance = 0;
                            if (!string.IsNullOrEmpty(item["Balance"].ToString()) && Decimal.TryParse(item["Balance"].ToString(), out sbobalance) && clientrow["CardCode"].ToString() == item["CardCode"].ToString())
                                totalbalance += sbobalance;
                            if (clientTable.Columns.Contains("sbo_id") && clientrow["sbo_id"].ToString() == item["id"].ToString() && clientrow["CardCode"].ToString() == item["CardCode"].ToString())
                                clientrow["Balance"] = sbobalance;
                        }
                        clientrow["BalanceTotal"] = totalbalance;
                    }
                }

                DataTable Balancetb = new DataTable();
                foreach (DataRow item in sbotable.Rows)
                {
                    bool sapflag = _serviceSaleOrderApp.GetSapSboIsOpen(item["id"].ToString());
                    if (sapflag)
                    {
                        string strSql = string.Format("SELECT Balance,CardCode FROM OCRD WHERE CardCode in ({0}) and Balance>0", CardCodes);
                        Balancetb = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql, CommandType.Text, null);
                        break;
                    }
                }
                if (Balancetb.Rows.Count > 0)
                {
                    foreach (DataRow clientrows in clientTable.Rows)
                    {
                        foreach (DataRow sborow in Balancetb.Rows)
                        {
                            if (clientrows["CardCode"].ToString() == sborow["CardCode"].ToString())
                            {
                                decimal totalbalance = 0;
                                string sbobalancestrs = sborow["Balance"].ToString();
                                decimal sbobalance = 0;
                                if (!string.IsNullOrEmpty(sbobalancestrs) && Decimal.TryParse(sbobalancestrs, out sbobalance) && sbobalancestrs != "0.000000")
                                    totalbalance += sbobalance;
                                clientrows["BalanceTotal"] = totalbalance;
                                break;
                            }
                        }
                    }
                }
            }
            return clientTable;
        }

        /// <summary>
        /// 统计各个状态的客户数量
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetCustomerCount()
        {
            var result = new TableData();

            int? slpCode = null; //销售员的销售代码
            var userInfo = _auth.GetCurrentUser().User;

            //这3个人可以查看全部,其他只能查看自己的客户
            if (!new string[] { "韦京生", "郭睿心", "骆灵芝" }.Contains(userInfo.Name))
            {
                slpCode = await (from u in UnitWork.Find<base_user>(null)
                                 join s in UnitWork.Find<sbo_user>(null)
                                 on u.user_id equals s.user_id
                                 where s.sbo_id == Define.SBO_ID && u.user_nm == userInfo.Name
                                 select s.sale_id).FirstOrDefaultAsync();
            }

            //黑名单客户也不在客户列表上显示
            var blacklist = UnitWork.Find<SpecialCustomer>(c => c.Type == 0).Select(c => c.CustomerNo).ToList();

            //总数
            var query0 = await UnitWork.Find<OCRD>(null)
                .WhereIf(slpCode != null, c => c.SlpCode == slpCode)
                .WhereIf(blacklist.Count() > 0, c => !blacklist.Contains(c.CardCode))
                .CountAsync();

            //未报价客户
            var query1 = await (from c in UnitWork.Find<OCRD>(null)
                                .WhereIf(slpCode != null, c => c.SlpCode == slpCode)
                                .WhereIf(blacklist.Count() > 0, c => !blacklist.Contains(c.CardCode))
                                join a in UnitWork.Find<OQUT>(null) on c.CardCode equals a.CardCode into temp
                                from t in temp.DefaultIfEmpty()
                                where t.CardCode == null
                                select c.CardCode).CountAsync();
            //已成交客户
            var query2 = await (from c in UnitWork.Find<OCRD>(null)
                                .WhereIf(slpCode != null, c => c.SlpCode == slpCode)
                                .WhereIf(blacklist.Count() > 0, c => !blacklist.Contains(c.CardCode))
                                join d in UnitWork.Find<ODLN>(null) on c.CardCode equals d.CardCode
                                select c.CardCode).Distinct().CountAsync();

            //公海领取(被领取后还没做过单的用户)
            //在历史归属表中存在但是公海中不存在的客户(说明已被领取)
            var cardCodes = (from h in UnitWork.Find<CustomerSalerHistory>(null)
                             join c in UnitWork.Find<CustomerList>(null) on h.CustomerNo equals c.CustomerNo into temp
                             from t in temp.DefaultIfEmpty()
                             where t.CustomerNo == null
                             select h.CustomerNo).Distinct().ToList();
            //还没做过单的客户
            var query3 = await (from c in UnitWork.Find<OCRD>(c => cardCodes.Contains(c.CardCode))
                                .WhereIf(slpCode != null, c => c.SlpCode == slpCode)
                                .WhereIf(blacklist.Count() > 0, c => !blacklist.Contains(c.CardCode))
                                join a in UnitWork.Find<OQUT>(null) on c.CardCode equals a.CardCode into temp
                                from t in temp.DefaultIfEmpty()
                                where t.CardCode == null
                                select c.CardCode).CountAsync();
            //即将掉入公海客户
            var query4 = await UnitWork.Find<CustomerList>(c => c.LabelIndex == 4)
                .WhereIf(slpCode != null, c => c.SlpCode == slpCode)
                .WhereIf(blacklist.Count() > 0, c => !blacklist.Contains(c.CustomerNo))
                .CountAsync();

            result.Data = new GetCustomerCount() { Count0 = query0, Count1 = query1, Count2 = query2, Count3 = query3, Count4 = query4 };

            return result;
        }

        #region 同步
        public async Task<bool> Synchronous(int clueId)
        {
            var clue = await UnitWork.FindSingleAsync<Repository.Domain.Serve.Clue>(q => q.Id == clueId);
            if (clue == null)
            {
                return false;
            }
            clue.Status = 1;
            await UnitWork.UpdateAsync(clue);
            await UnitWork.SaveAsync();
            return true;
        }
        #endregion
        #endregion
        #region 查詢指定業務夥伴的科目余额
        /// <summary>
        /// 查詢指定業務夥伴的科目余额（老系统用excel导入的crm_ocrd_oldsbo_balance)
        /// </summary>
        /// <param name="CardCode">客戶代碼</param>
        /// <param name="SboId">賬套</param>
        /// <returns></returns>
        public DataTable GetClientSboBalanceNew(string CardCode, string SboId)
        {
            DataTable dt = new DataTable();
            string strmySql = string.Format("SELECT sbo_id,CardCode,Balance FROM {0}.crm_ocrd_oldsbo_balance WHERE sbo_id IN({1}) and CardCode IN ({2})", "nsap_bone", SboId, CardCode);
            dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strmySql, CommandType.Text, null);
            return dt;
        }
        public string GetClientSboBalance(string CardCode, string SboId)
        {
            var sapbobjs = "";
            bool sapflag = _serviceSaleOrderApp.GetSapSboIsOpen(SboId);
            if (sapflag)
            {

                string strSql = string.Format("SELECT Balance FROM OCRD WHERE CardCode = '{0}'", CardCode);
                object sapbobj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, strSql, CommandType.Text, null);
                sapbobjs = sapbobj == null ? "" : sapbobj.ToString();
            }

            return sapbobjs;
        }
        #endregion
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
        #region 构建客户草稿
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
                SboId = clientInfo.SboId,
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
                CreateDate = !string.IsNullOrWhiteSpace(clientInfo.CreateDate) ? clientInfo.CreateDate : DateTime.Now.ToString(),//创建时间
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
                SlpName = !string.IsNullOrWhiteSpace(clientInfo.SlpName) ? clientInfo.SlpName.Replace(" \"", "").Replace("\\", "") : "",//销售员名称
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
                AcctList = new List<clientAcct1>(),
                U_SuperClient = clientInfo.U_SuperClient,//上级客户
                U_StaffScale = clientInfo.U_StaffScale,//人员规模
                U_ClientSource = clientInfo.U_ClientSource,//客户来源
                U_TradeType = clientInfo.U_TradeType,//贸易类型
                U_CompSector = clientInfo.U_CompSector,//所属行业
                U_CardTypeStr = clientInfo.U_CardTypeStr//新版客户类型
            };
            foreach (var item in clientInfo.ContactList)
            {
                if (item.IsDefault == "1")
                {
                    billDelivery.CntctPrsn = item.Name; //联系人
                    billDelivery.Phone1 = item.Tel1;//电话1
                    billDelivery.Phone2 = item.Tel2;//电话2
                    billDelivery.Cellular = item.Cellolar; //移动电话
                }
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
                    fileUserId = userID.ToString()//用户Id
                };
                billDelivery.FilesDetails.Add(billAttchment);
            }

            billDelivery.EshopUserId = clientInfo.EshopUserId;
            return billDelivery;
        }
        #endregion
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
            string sql = string.Format("SELECT job_data FROM {0}.wfa_job WHERE job_id={1}", "nsap_base", jobId);
            return UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
        }
        #endregion
        #region 根据jobId获取审核任务信息
        /// <summary>
        /// 根据jobId获取审核任务信息
        /// </summary>
        public clientOCRD GetAuditInfoNew(string jobId)
        {
            clientOCRD bill = _serviceSaleOrderApp.DeSerialize<clientOCRD>((byte[])GetAuditInfo(jobId));
            return bill;
        }
        #endregion
        #region 修改审核数据
        /// <summary>
        /// 修改审核数据
        /// </summary>
        public bool UpdateAuditJob(string jobId, string jobName, string remarks, byte[] wfaJob, bool isEditStatus)
        {
            StringBuilder strSql = new StringBuilder();
            int rows;
            if (isEditStatus)
            {
                strSql.AppendFormat("UPDATE IGNORE {0}.wfa_job SET job_nm='{1}',", "nsap_base", jobName);
                strSql.AppendFormat("job_data=?job_data,remarks='{0}',job_state={1} WHERE job_id={2}", remarks, 0, jobId);
                List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> strPara = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
                {
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?job_data",     wfaJob)

                };
                rows = UnitWork.ExecuteNonQuery(ContextType.NsapBaseDbContext, CommandType.Text, strSql.ToString(), strPara);
            }
            else
            {
                strSql.AppendFormat("UPDATE IGNORE {0}.wfa_job SET ", "nsap_base");
                strSql.Append("job_nm=?job_nm,job_data=?job_data,remarks=?remarks WHERE job_id=?job_id");
                List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> strPara = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
                {
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?job_nm",     jobName),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?job_data",     wfaJob),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?remarks",     remarks),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?job_id",     jobId)

                };
                rows = UnitWork.ExecuteNonQuery(ContextType.NsapBaseDbContext, CommandType.Text, strSql.ToString(), strPara);
            }

            return rows > 0 ? true : false;
        }
        #endregion

        #region 修改审核数据（修改客户名称）
        /// <summary>
        /// 修改审核数据（修改客户名称）
        /// </summary>
        public bool UpdateAuditJob(string jobId, string jobName, string cardName, string remarks, byte[] wfaJob, bool isEditStatus)
        {
            StringBuilder strSql = new StringBuilder();
            bool result = true;
            try
            {
                if (isEditStatus)
                {
                    strSql.AppendFormat("UPDATE IGNORE {0}.wfa_job SET job_nm='{1}',", "nsap_base", jobName);
                    strSql.AppendFormat("job_data=?job_data,card_name='{0}',remarks='{1}',job_state={2} WHERE job_id={3}", cardName, remarks, 0, jobId);
                    List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> strPara = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
                {
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?job_data",    wfaJob)

                };
                    object rows = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, strPara);
                }
                else
                {
                    strSql.AppendFormat("UPDATE IGNORE {0}.wfa_job SET ", "nsap_base");
                    strSql.AppendFormat("job_nm='{0}',job_data=?job_data,card_name='{1}',remarks='{2}' WHERE job_id={3}", jobName, cardName, remarks, jobId);
                    List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> strPara = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
                {
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?job_data",    wfaJob)

                };
                    object rows = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, strPara);
                }
            }
            catch (Exception ex)
            {
                result = false;
                throw;
            }
            return result;
        }
        #endregion
        #region 是否选择新的售后主管
        /// <summary>
        /// 是否选择新的售后主管
        /// </summary>
        /// <param name="sCardCode"></param>
        /// <param name="sfTcnician"></param>
        /// <returns></returns>
        public string GetSavefTcnician_sql(string sCardCode, string sfTcnician)
        {
            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> strPara = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pCardCode", sCardCode),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pDfTcnician", sfTcnician)

            };
            string ret = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, string.Format("{0}.sp_userTemp_GetState", "nsap_base"), CommandType.StoredProcedure, strPara).ToString();
            return ret;
        }
        public string SetSavefTcnicianStep_sql(string sCardCode, string sfTcnician, int JobId)
        {

            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> strPara = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pCardCode", sCardCode),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pDfTcnician", sfTcnician),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pJobId", JobId)

            };
            string ret = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, string.Format("{0}.sp_SetTcnicianStep", "nsap_base"), CommandType.StoredProcedure, strPara).ToString();
            return ret;

        }
        #endregion
        #region 查询业务伙伴详细
        /// <summary>
        /// 查询业务伙伴详细
        /// </summary>
        /// <returns></returns>
        public DataTable SelectCrmClientInfo(string CardCode, string SboId, bool IsOpenSap, bool IsViewSales)
        {

            string CustomFields = "";
            DataTable dt = _serviceSaleOrderApp.GetCustomFields("crm_OCRD");
            if (IsOpenSap)
            {
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (_serviceSaleOrderApp.QueryExistsStoreItemTypeField("OCRD", dt.Rows[i][0].ToString(), true))
                        {
                            CustomFields += "," + dt.Rows[i][0].ToString();
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
                        if (_serviceSaleOrderApp.QueryExistsStoreItemTypeField("crm_OCRD", dt.Rows[i][0].ToString(), false))
                        {
                            CustomFields += "," + dt.Rows[i][0].ToString();
                        }
                    }
                }
            }
            StringBuilder strSql = new StringBuilder("SELECT ");
            if (!IsOpenSap) { strSql.Append("a.sbo_id,"); }
            strSql.Append("a.CardCode, a.CardName, CardFName, a.CardType, a.GroupCode, a.CmpPrivate, a.Notes, ");
            if (IsViewSales) { strSql.Append("a.Balance, "); } else { strSql.Append("'****' AS Balance, "); }
            strSql.Append("a.GroupNum, a.LicTradNum, a.Free_Text AS 'FreeText', a.SlpCode, a.Currency,a.DNotesBal,a.OrdersBal, ");
            strSql.Append("a.Cellular, a.E_Mail, a.Fax, a.CntctPrsn, a.Phone1, a.Phone2, ");
            strSql.Append("a.QryGroup1, a.QryGroup2, a.QryGroup3, a.QryGroup4, a.QryGroup5, a.QryGroup6, a.QryGroup7, a.QryGroup8, a.QryGroup9, a.QryGroup10, ");
            strSql.Append("a.QryGroup11, a.QryGroup12, a.QryGroup13, a.QryGroup14, a.QryGroup15, a.QryGroup16, a.QryGroup17, a.QryGroup18, a.QryGroup19, a.QryGroup20, ");
            strSql.Append("a.QryGroup21, a.QryGroup22, a.QryGroup23, a.QryGroup24, a.QryGroup25, a.QryGroup26, a.QryGroup27, a.QryGroup28, a.QryGroup29, a.QryGroup30, ");
            strSql.Append("a.QryGroup31, a.QryGroup32, a.QryGroup33, a.QryGroup34, a.QryGroup35, a.QryGroup36, a.QryGroup37, a.QryGroup38, a.QryGroup39, a.QryGroup40, ");
            strSql.Append("a.QryGroup41, a.QryGroup42, a.QryGroup43, a.QryGroup44, a.QryGroup45, a.QryGroup46, a.QryGroup47, a.QryGroup48, a.QryGroup49, a.QryGroup50, ");
            strSql.Append("a.QryGroup51, a.QryGroup52, a.QryGroup53, a.QryGroup54, a.QryGroup55, a.QryGroup56, a.QryGroup57, a.QryGroup58, a.QryGroup59, a.QryGroup60, ");
            strSql.Append("a.QryGroup61, a.QryGroup62, a.QryGroup63, a.QryGroup64, a.DocEntry, ");
            strSql.Append("a.validFor AS ValidFor, a.validFrom AS ValidFrom, a.validTo AS ValidTo, a.ValidComm, ");
            strSql.Append("a.frozenFor AS FrozenFor, a.frozenFrom AS FrozenFrom, a.frozenTo AS FrozenTo, a.FrozenComm, ");
            strSql.Append("a.VatGroup, a.Indicator, a.ShipType, a.AliasName, ");
            //默认开票到 DefAddrBill, BillToDef, AddrType, Country, State1, City, Building, ZipCode, County 
            strSql.Append("a.Address, a.BillToDef, a.AddrType, a.Country, a.State1, a.City, a.Building, a.ZipCode, ");
            //默认运达到 DefAddrShip, ShipToDef,MailAddrTy,MailCountr,State2,MailCity,MailBuildi,MailZipCod, MailCounty
            strSql.Append("a.MailAddres, a.ShipToDef, a.MailAddrTy, a.MailCountr, a.State2, a.MailCity, a.MailBuildi, a.MailZipCod, ");
            strSql.Append("a.DfTcnician as DfTcnicianCode, a.Territory, a.IndustryC, ");//a.StreetNo, a.MailStrNo, 
            if (IsOpenSap) { strSql.Append("ISNULL(c.lastName,'')+ISNULL(c.firstName,'') as DfTcnician, "); } else { strSql.Append("CONCAT(IFNULL(c.lastName,''),IFNULL(c.firstName,'')) as DfTcnician, "); }
            strSql.Append("a.GTSRegNum, a.GTSBankAct, a.GTSBilAddr,b.SlpName, d.DocDueDate, a.IntrntSite, ");
            strSql.AppendFormat("U_Prefix AS CardNamePrefix,U_Name AS CardNameCore,U_Suffix AS CardNameSuffix,U_is_reseller AS is_reseller,U_EndCustomerName AS EndCustomerName,U_EndCustomerContact AS EndCustomerContact  {0} ", CustomFields);
            DataTable dtRet;

            if (IsOpenSap)
            {
                strSql.Append("FROM OCRD a LEFT JOIN OSLP b ON a.SlpCode=b.SlpCode LEFT JOIN OHEM c ON a.DfTcnician=c.empID ");
                strSql.AppendFormat("LEFT JOIN (SELECT TOP 1 DocDueDate,CardCode FROM ODLN WHERE CardCode='{0}' AND DocTotal>=2000 ORDER BY DocDueDate DESC) d ON a.CardCode=d.CardCode ", CardCode);
                strSql.AppendFormat("WHERE a.CardCode='{0}' ", CardCode);
                var dd = strSql.ToString();
                dtRet = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
                dtRet.Columns.Add("U_ClientSource", typeof(string));//客户来源
                dtRet.Columns.Add("U_CompSector", typeof(string));//所属行业
                dtRet.Columns.Add("U_TradeType", typeof(string));//贸易类型
                dtRet.Columns.Add("U_CardTypeStr", typeof(string));//新版客户类型
                dtRet.Columns.Add("U_StaffScale", typeof(string));//人员规模
                foreach (DataRow clientrow in dtRet.Rows)
                {
                    var sql = string.Format(
                        "SELECT U_TradeType,U_ClientSource,U_CompSector,U_CardTypeStr,U_StaffScale FROM crm_ocrd WHERE CardCode='{0}'",
                        clientrow["CardCode"]);
                    var ClientSource =
                        UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, sql, CommandType.Text, null);
                    if (ClientSource.Rows.Count > 0)
                    {
                        foreach (DataRow clientSource in ClientSource.Rows)
                        {
                            clientrow["U_ClientSource"] = clientSource["U_ClientSource"];
                            clientrow["U_CompSector"] = clientSource["U_CompSector"];
                            clientrow["U_TradeType"] = clientSource["U_TradeType"];
                            clientrow["U_CardTypeStr"] = clientSource["U_CardTypeStr"];
                            clientrow["U_StaffScale"] = clientSource["U_StaffScale"];
                        }
                    }
                }
            }
            else
            {
                strSql.AppendFormat("FROM {0}.crm_OCRD a LEFT JOIN {0}.crm_OSLP b ON a.sbo_id=b.sbo_id AND a.SlpCode=b.SlpCode ", "nsap_bone");
                strSql.AppendFormat("LEFT JOIN {0}.crm_OHEM c ON a.sbo_id=c.sbo_id AND a.DfTcnician=c.empID ", "nsap_bone");
                strSql.AppendFormat("LEFT JOIN  (SELECT DocDueDate,CardCode FROM {0}.sale_ODLN WHERE CardCode='{0}' AND DocTotal>=2000 ORDER BY DocDueDate DESC LIMIT 1) d ON a.CardCode=d.CardCode ", "nsap_bone");
                strSql.AppendFormat("WHERE a.sbo_id={0} AND a.CardCode='{1}' ", SboId, CardCode);


                dtRet = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);

            }

            string sDfTcnician = dtRet.Rows[0]["DfTcnicianCode"].ToString();

            SetSavefTcnician_sql(CardCode, sDfTcnician, SboId);
            return dtRet;
        }
        #endregion
        #region 是否选择新的售后主管
        public void SetSavefTcnician_sql(string sCardCode, string sfTcnician, string SboId)
        {
            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> strPara = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pCardCode",    sCardCode),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pDfTcnician",     string.IsNullOrEmpty(sfTcnician)?"0":sfTcnician),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pSboId",     SboId)

            };
            UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, string.Format("{0}.sp_userTemp_Add", "nsap_base"), CommandType.StoredProcedure, strPara);

        }
        #endregion
        #region 查询业务伙伴的联系人
        /// <summary>
        /// 查询业务伙伴的联系人
        /// </summary>
        /// <returns></returns>
        public DataTable SelectClientContactData(string CardCode, string SboId, bool IsOpenSap, bool IsViewFull)
        {
            //string StrWhere = IsViewFull ? "" : " AND Active='Y' ";
            string StrWhere = "";
            StringBuilder strSql = new StringBuilder();
            if (IsOpenSap)
            {
                strSql.Append("SELECT CardCode,CntctCode,Active,'0' AS IsDefault,Name,Gender,Title,Position,[Address],Notes1,Notes2,Tel1,Tel2,Cellolar,Fax,E_MailL,BirthDate,U_ACCT,U_BANK FROM OCPR ");
                strSql.AppendFormat("WHERE CardCode='{0}' {1} ", CardCode, StrWhere);

                return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
            }
            else
            {
                strSql.Append("SELECT CardCode,CntctCode,Active,'0' AS IsDefault,Name,Gender,Title,Position,Address,Notes1,Notes2,Tel1,Tel2,Cellolar,Fax,E_MailL,BirthDate,U_ACCT,U_BANK ");
                strSql.AppendFormat("FROM {0}.crm_OCPR WHERE sbo_id=?sbo_id AND CardCode=?CardCode {1} ORDER BY Name ASC", "nsap_bone", StrWhere);

                List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> strPara = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
                {
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?sbo_id",    SboId),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?CardCode",   CardCode)
                };
                return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text,
                    strPara);
            }
        }
        #endregion
        #region 查询业务伙伴的地址
        /// <summary>
        /// 查询业务伙伴的地址
        /// </summary>
        /// <returns></returns>
        public DataTable SelectClientAddrData(string CardCode, string SboId, bool IsOpenSap)
        {
            StringBuilder strSql = new StringBuilder();
            if (IsOpenSap)
            {
                strSql.Append("SELECT CardCode,LineNum,U_Active AS Active,'0' AS IsDefault,a.AdresType,a.Address,b.Name AS Country,c.Name AS 'State',a.City,a.Building,a.ZipCode,a.Country AS CountryId,a.State AS StateId ");
                strSql.Append("FROM CRD1 a LEFT JOIN OCRY b ON a.Country=b.Code LEFT JOIN OCST c ON a.State=c.Code ");
                strSql.AppendFormat("WHERE a.CardCode='{0}' ", CardCode);

                return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
            }
            else
            {
                strSql.Append("SELECT CardCode,LineNum,U_Active AS Active,'0' AS IsDefault,a.AdresType,a.Address,b.Name AS Country,c.Name AS 'State',a.City,a.Building,a.ZipCode,a.Country AS CountryId,a.State AS StateId ");
                strSql.AppendFormat("FROM {0}.crm_CRD1 a LEFT JOIN {0}.crm_OCRY b ON a.Country=b.Code LEFT JOIN {0}.crm_OCST c ON a.State=c.Code ", "nsap_bone");
                strSql.Append("WHERE a.sbo_id=?sbo_id AND a.CardCode=?CardCode ORDER BY a.Address ASC");


                List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> para = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
                {
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?sbo_id",    SboId),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?CardCode",     CardCode)

                };
                return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, para);
            }
        }
        #endregion
        #region 查询所有技术员
        /// <summary>
        /// 查询所有技术员
        /// </summary>
        /// <returns></returns>
        public DataTable GetTcnicianInfo(int pageSize, int pageIndex, string filterQuery, string sortname, string sortorder, string SboId, string Type, out int rowsCount)
        {
            string sortString = string.Empty;
            string filterString = "1=1 ";
            if (Type == "1") { filterString += "AND role_nm LIKE '%技术主管%' "; }
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());
            if (!string.IsNullOrEmpty(filterQuery))
            {
                string[] whereArray = filterQuery.Split('`');
                for (int i = 0; i < whereArray.Length; i++)
                {
                    string[] p = whereArray[i].Split(':');
                    if (!string.IsNullOrEmpty(p[1]))
                    {
                        filterString += string.Format("AND {0} LIKE '%{1}%' ", p[0], p[1].Trim().FilterSQL().Replace("*", "%"));
                    }
                }
            }
            DataTable dt;
            StringBuilder filedName = new StringBuilder();
            StringBuilder tableName = new StringBuilder();
            filedName.Append("tech_id,user_nm,TcnicianNm,role_nm,dep_alias,post_nm,telphone");
            tableName.Append("(SELECT h.tech_id,a.user_nm,CONCAT(IFNULL(lastName,''),IFNULL(firstName,'')) AS TcnicianNm,");
            tableName.Append("GROUP_CONCAT(DISTINCT f.role_nm ORDER BY f.role_nm SEPARATOR ',') as role_nm,c.dep_alias,d.post_nm,a.telphone ");
            tableName.AppendFormat("FROM {0}.base_user a LEFT JOIN {0}.base_user_detail b ON a.user_id=b.user_id ", "nsap_base");
            tableName.AppendFormat("LEFT JOIN {0}.base_dep c ON b.dep_id=c.dep_id LEFT JOIN {0}.base_post d ON b.post_id=d.post_id ", "nsap_base");
            tableName.AppendFormat("LEFT JOIN {0}.base_user_role e ON a.user_id=e.user_id LEFT JOIN {0}.base_role f ON e.role_id=f.role_id ", "nsap_base");
            tableName.AppendFormat("LEFT JOIN {0}.sbo_user h ON h.sbo_id={1} AND a.user_id=h.user_id ", "nsap_base", SboId);
            tableName.AppendFormat("LEFT JOIN {0}.crm_ohem i ON h.sbo_id=i.sbo_id AND h.tech_id=i.empID WHERE h.tech_id>0 GROUP BY a.user_id) T ", "nsap_bone");
            return SelectPagingNoneRowsCount(tableName.ToString(), filedName.ToString(), pageSize, pageIndex, sortString, filterString, out rowsCount);

        }
        public DataTable SelectPagingNoneRowsCount(string tableName, string fieldName, int pageSize, int pageIndex, string strOrder, string strWhere, out int rowsCount)
        {
            return SelectPaging(tableName, fieldName, pageSize, pageIndex, strOrder, strWhere, 0, out rowsCount);
        }
        public DataTable SelectPaging(string tableName, string fieldName, int pageSize, int pageIndex, string strOrder, string strWhere, int isTotal, out int rowsCount)
        {

            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> para = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pTableName",    tableName),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pFieldName",     fieldName),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pPageSize",     pageSize),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pPageIndex",     pageIndex),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pStrOrder",     strOrder),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pStrWhere",     strWhere),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?pIsTotal",     isTotal),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?rowsCount",     0)

            };
            para[7].Direction = ParameterDirection.Output;
            DataTable dataTable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, string.Format("{0}.sp_common_pager", "nsap_base"), CommandType.StoredProcedure, para);
            rowsCount = isTotal == 1 ? Convert.ToInt32(para[7].Value) : 0;
            return dataTable;
        }
        #endregion

        #region 修改流程任务
        /// <summary>
        /// 修改流程任务
        /// </summary>
        public string UpdateClientJob(UpdateClientJobReq updateClientJobReq)
        {
            string result = "";
            var UserId = _serviceBaseApp.GetUserNaspId();
            clientOCRD OCRD = BulidClientJob(updateClientJobReq.clientInfo);
            //根据客户类型生成业务伙伴编码
            if (!string.IsNullOrWhiteSpace(OCRD.CardNameCore.Trim())) { OCRD.U_Name = OCRD.CardNameCore; }
            string rJobNm = string.Format("{0}{1}", OCRD.ClientOperateType == "edit" ? "修改" : "添加", OCRD.CardType == "S" ? "供应商" : "业务伙伴");
            byte[] job_data = ByteExtension.ToSerialize(OCRD);
            if (updateClientJobReq.submitType == "Temporary")
            {
                result = UpdateAuditJob(updateClientJobReq.JobId, rJobNm, OCRD.CardName, OCRD.FreeText.FilterESC(), job_data, true) ? "1" : "0";
                bool updParaCardCode = UpdateWfaJobPara(updateClientJobReq.JobId, 1, OCRD.CardCode);
                bool updParaCardName = UpdateWfaJobPara(updateClientJobReq.JobId, 2, OCRD.CardName);
                bool updParaOperateType = UpdateWfaJobPara(updateClientJobReq.JobId, 3, OCRD.ClientOperateType);
                bool updParaAppChange = UpdateWfaJobPara(updateClientJobReq.JobId, 4, OCRD.IsApplicationChange);
            }
            else if (updateClientJobReq.submitType == "Resubmit")
            {
                bool res = UpdateAuditJob(updateClientJobReq.JobId, rJobNm, OCRD.CardName, OCRD.FreeText.FilterESC(), job_data, false);
                bool updParaCardCode = UpdateWfaJobPara(updateClientJobReq.JobId, 1, OCRD.CardCode);
                bool updParaCardName = UpdateWfaJobPara(updateClientJobReq.JobId, 2, OCRD.CardName);
                bool updParaOperateType = UpdateWfaJobPara(updateClientJobReq.JobId, 3, OCRD.ClientOperateType);
                bool updParaAppChange = UpdateWfaJobPara(updateClientJobReq.JobId, 4, OCRD.IsApplicationChange);
                if (res)
                {
                    result = _serviceSaleOrderApp.WorkflowSubmit(int.Parse(updateClientJobReq.JobId), UserId, OCRD.FreeText, "", 0);
                    if (result == "1")
                    {
                        if (rJobNm == "添加业务伙伴")
                        {
                            result = SaveCrmAuditInfo1(updateClientJobReq.JobId, UserId, rJobNm);
                        }
                        else if (rJobNm == "修改业务伙伴")
                        {
                            string sfTcnician = OCRD.DfTcnicianCode;
                            string sCardCode = OCRD.CardCode;

                            switch (GetSavefTcnician_sql(sCardCode, sfTcnician))
                            {
                                case "1":
                                    result = SaveCrmAuditInfo1(updateClientJobReq.JobId, UserId, rJobNm);
                                    break;
                                case "0":
                                    SetSavefTcnicianStep_sql(sCardCode, sfTcnician, Convert.ToInt32(updateClientJobReq.JobId));
                                    bool bChangeTcnician = true;
                                    UpdateWfaJobPara(updateClientJobReq.JobId, 4, bChangeTcnician.ToString());
                                    break;
                                default:
                                    break;
                            }
                            //result =SaveCrmAuditInfo1(JobId, UserID, rJobNm);
                        }
                    }

                }
            }
            else if (updateClientJobReq.submitType == "Edit")
            {
                result = UpdateAuditJob(updateClientJobReq.JobId, rJobNm, OCRD.CardName, OCRD.FreeText.FilterESC(), job_data, false) ? "1" : "0";
                bool updParaCardCode = UpdateWfaJobPara(updateClientJobReq.JobId, 1, OCRD.CardCode);
                bool updParaCardName = UpdateWfaJobPara(updateClientJobReq.JobId, 2, OCRD.CardName);
                bool updParaOperateType = UpdateWfaJobPara(updateClientJobReq.JobId, 3, OCRD.ClientOperateType);
                bool updParaAppChange = UpdateWfaJobPara(updateClientJobReq.JobId, 4, OCRD.IsApplicationChange);
            }
            return result;
        }
        #endregion

        #region 保存业务伙伴审核的录入方案
        /// <summary>
        /// 保存业务伙伴审核的录入方案
        /// </summary>
        public string SaveCrmAuditInfo(string AuditType, string CardCode, string DfTcnician, string JobId)
        {
            clientOCRD client = new clientOCRD();
            client = _serviceSaleOrderApp.DeSerialize<clientOCRD>((byte[])GetAuditInfo(JobId));
            client.ChangeType = AuditType;
            client.ChangeCardCode = CardCode;
            if (AuditType == "Edit")
            {
                client.DfTcnicianCode = DfTcnician;
            }
            string rJobNm = string.Format("{0}{1}", client.ClientOperateType == "edit" ? "修改" : "添加", client.CardType == "S" ? "供应商" : "业务伙伴");
            byte[] job_data = ByteExtension.ToSerialize(client);
            return UpdateAuditJob(JobId, rJobNm, client.FreeText.FilterESC(), job_data, false) ? "1" : "0";

        }
        #endregion
        #region 审核
        /// <summary>
        /// 审核
        /// </summary>
        public string AuditResubmitNext(int jobID, int userID, string recommend, string auditOpinionid)
        {
            string res = "";
            if (auditOpinionid == "agree")
            {
                res = _serviceSaleOrderApp.WorkflowSubmit(jobID, userID, recommend, "", 0);
            }
            else if (auditOpinionid == "reject")
            {
                res = _serviceSaleOrderApp.WorkflowReject(jobID, userID, recommend, "", 0);
            }
            else if (auditOpinionid == "pending")
            {
                res = _serviceSaleOrderApp.SavePanding(jobID, userID, recommend);
            }
            return res;
        }
        #region 查询 国家·省·市
        /// <summary>
        /// 查询 国家·省·市
        /// </summary>
        /// <returns></returns>
        public DataTable GetStateProvincesInfo(int pageSize, int pageIndex, string filterQuery, string sortname, string sortorder, string AddrType, string CountryId, string StateId, out int rowCounts)
        {

            string sortString = string.Empty;
            string filterString = " 1=1 ";
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());
            if (!string.IsNullOrEmpty(filterQuery))
            {
                string[] whereArray = filterQuery.Split('`');
                for (int i = 0; i < whereArray.Length; i++)
                {
                    string[] p = whereArray[i].Split(':');
                    if (!string.IsNullOrEmpty(p[1]))
                    {
                        string[] qfieldarr = p[0].Split(',');
                        string likestr = "";
                        foreach (string thefield in qfieldarr)
                        {
                            if (string.IsNullOrEmpty(likestr))
                            {
                                likestr += string.Format("{0} LIKE '%{1}%' ", thefield, p[1].Trim().FilterSQL().Replace("*", "%"));
                            }
                            else
                            {
                                likestr += string.Format(" OR {0} LIKE '%{1}%' ", thefield, p[1].Trim().FilterSQL().Replace("*", "%"));
                            }
                        }
                        if (!string.IsNullOrEmpty(likestr))
                        {
                            filterString += "AND (" + likestr + ")";
                        }
                    }
                }
            }
            StringBuilder filedName = new StringBuilder();
            StringBuilder tableName = new StringBuilder();

            filedName.Append("Code, Name");
            if (AddrType == "1")
            {
                tableName.AppendFormat("(SELECT Code, Name FROM {0}.crm_OCRY) T", "nsap_bone");
            }
            else if (AddrType == "2")
            {
                tableName.AppendFormat("(SELECT Code, Name FROM {0}.crm_OCST WHERE Country='{1}') T", "nsap_bone", CountryId);
            }
            else
            {
                tableName.AppendFormat("(SELECT city_id AS Code, city AS Name FROM {0}.crm_city WHERE parent_id={1}) T", "nsap_bone", StateId);
            }
            return _serviceSaleOrderApp.SelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), pageSize, pageIndex, sortString, filterQuery, out rowCounts);
        }
        #endregion 
        #endregion

        #region 查询指定城市的邮编
        /// <summary>
        /// 查询指定城市的邮编
        /// </summary>
        public string GetClientCityZipCode(string CityId)
        {
            string strSql = string.Format("SELECT zip_code FROM {0}.crm_city WHERE city_id={1}", "nsap_bone", CityId);
            return UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null).ToString();
        }
        #endregion
        #region 查询是否提交过指定客户的变更信息
        /// <summary>
        /// 查询是否提交过指定客户的变更信息
        /// </summary>
        /// <returns></returns>
        public string GetAuditCardNameExsit(string OperaType, string SboId, string UserId, string CardCode, string CardName)
        {
            string JobType = _serviceSaleOrderApp.GetJobTypeByUrl("client/ClientInfo.aspx");

            StringBuilder strSql = new StringBuilder();
            if (OperaType == "add")
            {
                strSql.AppendFormat("SELECT COUNT(*) FROM {0}.wfa_job ", "nsap_base");
                strSql.Append("WHERE job_type_id=?JobType AND sbo_id=?SboId AND user_id=?UserId AND job_state=?JobState AND card_name=?CardName ");
                List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> strPara = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
                {
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?JobType",    JobType),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?SboId",     SboId),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?UserId",     UserId),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?JobState",     1),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?CardName",     CardName),

                };
                return Convert.ToInt32(UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, strPara).ToString()) > 0 ? "true" : "false";
            }
            else
            {
                strSql.AppendFormat("SELECT COUNT(*) FROM {0}.wfa_job ", "nsap_base");
                strSql.Append("WHERE job_type_id=?JobType AND sbo_id=?SboId AND user_id=?UserId AND job_state=?JobState ");
                if (!string.IsNullOrEmpty(CardCode))
                {
                    strSql.AppendFormat(" AND (card_code='{0}' OR card_name=?CardName)", CardCode);
                }
                else
                {
                    strSql.Append(" AND card_name=?CardName");
                }
                List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> strPara = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
                {
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?JobType",    JobType),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?SboId",     SboId),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?UserId",     UserId),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?JobState",     1),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?CardName",     CardName),

                };
                return Convert.ToInt32(UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, strPara).ToString()) > 0 ? "true" : "false";
            }
        }
        #endregion
        #region 查询审核中的客户名是否存在相同
        /// <summary>
        /// 查询审核中的客户名是否存在相同
        /// </summary>
        /// <returns></returns>
        public string GetAuditCardNameSimilar(string SboId, string CardName)
        {
            string JobType = _serviceSaleOrderApp.GetJobTypeByUrl("client/ClientInfo.aspx");

            //SELECT COUNT(*) FROM wfa_job A LEFT JOIN wfa_job_para B ON A.job_id=B.job_id AND B.para_idx=3 
            //WHERE job_type_id=?JobType AND sbo_id=?SboId AND job_state=?JobState AND B.para_val=?OperaType AND card_name=?CardName
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT COUNT(*) FROM {0}.wfa_job ", "nsap_base");
            strSql.Append("WHERE job_type_id=?JobType AND sbo_id=?SboId AND job_state=?JobState AND card_name=?CardName ");

            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> strPara = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?JobType",    JobType),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?SboId",     SboId),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?JobState",     1),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?CardName",     CardName),

            };
            return Convert.ToInt32(UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, strPara).ToString()) > 0 ? "true" : "false";
        }
        #endregion
        #region 检验公司名称简称是否存在相似
        /// <summary>
        /// 检验公司名称简称是否存在相似
        /// </summary>
        public string CheckCardNameSimilar(string SboId, string FieldName, string CardName, bool IsEdit, string CardCode)
        {
            StringBuilder strSql = new StringBuilder();
            bool IsOpenSap = false; //NSAP.Data.Store.MaterialMasterData.GetSapSboIsOpen(SboId);  //因为当前sqlserver库排序规则符合要求
            if (IsOpenSap)
            {

                strSql.AppendFormat("SELECT COUNT(*) FROM OCRD WHERE ({0} LIKE '%{1}%' OR '{1}' LIKE ('%'+{0}+'%')) AND LEN({0})>0 ", FieldName, CardName);
                if (IsEdit) { strSql.AppendFormat(" AND CardCode<>{0} ", CardCode); }
                return UnitWork.ExecuteScalar(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null).ToString();
            }
            else
            {
                strSql.AppendFormat("SELECT COUNT(*) FROM {0}.crm_OCRD WHERE sbo_id=?sbo_id AND ({1} LIKE '%{2}%' OR '{2}' LIKE CONCAT('%',{1},'%') AND LENGTH({1})>0) ", "nsap_bone", FieldName, CardName);
                if (IsEdit) { strSql.Append(" AND CardCode<>?CardCode "); }

                List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> strPara = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
                {
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?sbo_id",    SboId),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?CardCode",     CardCode),

                };
                return Convert.ToInt32(UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, strPara).ToString()) > 0 ? "true" : "false";
            }
        }
        #endregion
        #region 提交给我的相似客户
        public DataTable CheckCardSimilar(string Query, string JobId, bool IsSearchAll)
        {
            DataTable dataTable = new DataTable();
            clientOCRD OCRDModel = new clientOCRD();
            OCRDModel = _serviceSaleOrderApp.DeSerialize<clientOCRD>((byte[])(GetAuditInfo(JobId)));
            if (OCRDModel == null)
            {
                return dataTable;
            }
            if (OCRDModel.CardType.ToUpper() == "C" || OCRDModel.CardType.ToUpper() == "L")
            {
                if (!IsSearchAll)
                {
                    dataTable = SelectClientSimilarData(OCRDModel, OCRDModel.SboId, Query, IsSearchAll);
                }
                else
                {
                    if (!string.IsNullOrEmpty(Query))
                    {
                        dataTable = SelectClientSimilarData(OCRDModel, OCRDModel.SboId, Query, IsSearchAll);
                    }
                }
            }
            else
            {
                if (!IsSearchAll)
                {
                    dataTable = SelectSupplierSimilarData(OCRDModel, OCRDModel.SboId, Query, IsSearchAll);
                }
                else
                {
                    if (!string.IsNullOrEmpty(Query))
                    {
                        dataTable = SelectSupplierSimilarData(OCRDModel, OCRDModel.SboId, Query, IsSearchAll);
                    }
                }
            }
            return dataTable;
        }
        #endregion
        #region 搜索业务伙伴相似数据(New)
        /// <summary>
        /// 搜索业务伙伴相似数据
        /// </summary>
        public DataTable SelectClientSimilarData(clientOCRD Model, string SboId, string Query, bool IsSearchAll)
        {
            string pubSql = " SELECT  D.sbo_id,D.CardCode,D.CardName,D.CardFName,P.SlpName,D.CntctPrsn,";
            pubSql += "CONCAT(IFNULL(Y.Name,''),IFNULL(T.Name,''),IFNULL(D.MailCity,''),IFNULL(D.MailBuildi,'')) AS Address,";
            pubSql += "D.Phone1,D.Cellular,D.Balance,D.U_Name,D.U_EndCustomerName,D.U_EndCustomerContact,{0} FROM {1}.crm_OCRD D ";
            pubSql += "LEFT JOIN {1}.crm_OCRY Y ON Y.Code=D.MailCountr LEFT JOIN {1}.crm_OCST T ON T.Code=D.State2 ";
            pubSql += "LEFT JOIN {1}.crm_OSLP P ON P.SlpCode=D.SlpCode AND P.sbo_id=D.sbo_id ";
            string strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,0 AS Similarity11,0 AS Similarity12 ";

            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT   sbo_id,CardCode,CardName,CardFName,SlpName,CntctPrsn,Address,Phone1,Cellular,Balance,U_Name,U_EndCustomerName,U_EndCustomerContact,");
            strSql.Append("Similarity1, Similarity2, Similarity3, Similarity4, Similarity5, Similarity6, Similarity7, Similarity8, Similarity9, Similarity10,Similarity11,Similarity12,DfTcnician FROM ( ");

            if (IsSearchAll)  //根据搜索条件全局搜索
            {
                strSql.Append("SELECT  sbo_id,CardCode ,CardName,CardFName,SlpName,CntctPrsn,Address,Phone1,Cellular,Balance,U_Name,U_EndCustomerName,U_EndCustomerContact,");
                strSql.Append("SUM(Similarity1) AS Similarity1,SUM(Similarity2) AS Similarity2,SUM(Similarity3) AS Similarity3,SUM(Similarity4) AS Similarity4,SUM(Similarity5) AS Similarity5,");
                strSql.Append("SUM(Similarity6) AS Similarity6,SUM(Similarity7) AS Similarity7,SUM(Similarity8) AS Similarity8,SUM(Similarity9) AS Similarity9,SUM(Similarity10) AS Similarity10,SUM(Similarity11) AS Similarity11,SUM(Similarity12) AS Similarity12,DfTcnician ");
                strSql.Append(" FROM (");

                string[] whereArray = Query.Split('`');
                int rLineNum = 0;
                for (int i = 0; i < whereArray.Length; i++)
                {
                    #region 按条件搜索
                    string[] p = whereArray[i].Split(':');
                    if (!string.IsNullOrEmpty(p[1]))
                    {
                        if (i == 0)         //业务伙伴名称
                        {
                            strSimilarity = "1 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,0 AS Similarity11,0 AS Similarity12, 1 AS Grade,D.DfTcnician ";
                            strSql.Append(string.Format(pubSql, strSimilarity, "nsap_bone"));
                            strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardCode NOT LIKE 'V%' AND D.CardName LIKE '%{1}%' ", SboId, p[1].Trim().FilterSQL());
                            rLineNum++;
                        }
                        else if (i == 1)     //外文名称
                        {
                            strSimilarity = "0 AS Similarity1, 1 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,0 AS Similarity11,0 AS Similarity12, 2 AS Grade,D.DfTcnician ";
                            strSql.Append(rLineNum > 0 ? " UNION ALL " : "");
                            strSql.Append(string.Format(pubSql, strSimilarity, "nsap_bone"));
                            strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardCode NOT LIKE 'V%' AND D.CardFName LIKE '%{1}%' ", SboId, p[1].Trim().FilterSQL());
                            rLineNum++;
                        }
                        else if (i == 2)     //电话
                        {
                            strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 1 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,0 AS Similarity11,0 AS Similarity12, 3 AS Grade,D.DfTcnician ";
                            strSql.Append(rLineNum > 0 ? " UNION ALL " : "");
                            strSql.Append(string.Format(pubSql, strSimilarity, "nsap_bone"));
                            strSql.AppendFormat(" INNER JOIN (SELECT DISTINCT sbo_id,CardCode FROM {0}.crm_TEL WHERE sbo_id={1} AND CardCode NOT LIKE 'V%' AND Phone LIKE '%{2}%') A ON A.CardCode=D.CardCode AND A.sbo_id=D.sbo_id ", "nsap_bone", SboId, p[1].Trim().Replace("*", "%"));
                            rLineNum++;
                        }
                        else if (i == 3)     //联系人
                        {
                            strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 1 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,0 AS Similarity11,0 AS Similarity12, 4 AS Grade,D.DfTcnician ";
                            strSql.Append(rLineNum > 0 ? " UNION ALL " : "");
                            strSql.Append(string.Format(pubSql, strSimilarity, "nsap_bone"));
                            strSql.AppendFormat(" LEFT JOIN {0}.crm_OCPR R ON D.CardCode=R.CardCode AND R.sbo_id=D.sbo_id WHERE D.sbo_id={1} AND D.CardCode NOT LIKE 'V%' AND R.Name LIKE '%{2}%' ", "nsap_bone", SboId, p[1].Trim().FilterSQL());
                            rLineNum++;
                        }
                        else if (i == 4)     //联系人地址
                        {
                            strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 1 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,0 AS Similarity11,0 AS Similarity12, 5 AS Grade,D.DfTcnician ";
                            strSql.Append(rLineNum > 0 ? " UNION ALL " : "");
                            strSql.Append(string.Format(pubSql, strSimilarity, "nsap_bone"));
                            strSql.AppendFormat(" INNER JOIN (SELECT DISTINCT sbo_id,CardCode FROM {0}.crm_OCPR WHERE sbo_id={1} AND CardCode NOT LIKE 'V%' AND Address LIKE '%{2}%' ", "nsap_bone", SboId, p[1].Trim().FilterSQL());
                            strSql.AppendFormat(" UNION SELECT DISTINCT sbo_id,CardCode FROM {0}.crm_CRD1 WHERE sbo_id={1} AND CardCode NOT LIKE 'V%' AND Building LIKE '%{2}%') A ON A.CardCode=D.CardCode AND A.sbo_id=D.sbo_id ", "nsap_bone", SboId, p[1].Trim().FilterSQL());
                            rLineNum++;
                        }
                        else if (i == 5)         //终端用户名
                        {
                            strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,1 AS Similarity11,0 AS Similarity12, 11 AS Grade,D.DfTcnician ";
                            strSql.Append(string.Format(pubSql, strSimilarity, "nsap_bone"));
                            strSql.AppendFormat("WHERE D.sbo_id={0} AND D.U_EndCustomerName NOT LIKE 'V%' AND D.U_EndCustomerName LIKE '%{1}%' ", SboId, p[1].Trim().FilterSQL());
                            rLineNum++;
                        }
                        else if (i == 6)         //终端联系人
                        {
                            strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,0 AS Similarity11,1 AS Similarity12, 12 AS Grade,D.DfTcnician ";
                            strSql.Append(string.Format(pubSql, strSimilarity, "nsap_bone"));
                            strSql.AppendFormat("WHERE D.sbo_id={0} AND D.U_EndCustomerContact NOT LIKE 'V%' AND D.U_EndCustomerContact LIKE '%{1}%' ", SboId, p[1].Trim().FilterSQL());
                            rLineNum++;
                        }
                    }
                    #endregion
                }
                strSql.Append(") AS E GROUP BY CardCode,CardName,CardFName,SlpName,CntctPrsn,Address,Phone1,Cellular,Balance,U_Name,U_EndCustomerName,U_EndCustomerContact ");
                strSql.Append(" ORDER BY SUM(Similarity1+Similarity2+Similarity3+Similarity4+Similarity5+Similarity6+Similarity7+Similarity8+Similarity9+Similarity10+Similarity11+Similarity12) DESC,MIN(Grade) ASC LIMIT 50 ");
                if (rLineNum == 0)
                {
                    DataTable dt = new DataTable();
                    return dt;
                }
            }
            else
            {  //搜索当前业务伙伴相似
                strSql.Append("SELECT sbo_id,CardCode,CardName,CardFName,SlpName,CntctPrsn,Address,Phone1,Cellular,Balance,U_Name,U_EndCustomerName,U_EndCustomerContact,");
                strSql.Append("SUM(Similarity1) AS Similarity1,SUM(Similarity2) AS Similarity2,SUM(Similarity3) AS Similarity3,SUM(Similarity4) AS Similarity4,SUM(Similarity5) AS Similarity5,");
                strSql.Append("SUM(Similarity6) AS Similarity6,SUM(Similarity7) AS Similarity7,SUM(Similarity8) AS Similarity8,SUM(Similarity9) AS Similarity9,SUM(Similarity10) AS Similarity10,SUM(Similarity11) AS Similarity11,SUM(Similarity12) AS Similarity12,DfTcnician ");
                strSql.Append(" FROM (");
                #region CardName
                //==
                strSimilarity = "10 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,0 AS Similarity11,0 AS Similarity12, 1 AS Grade ,D.DfTcnician ";
                strSql.Append(string.Format(pubSql, strSimilarity, "nsap_bone"));
                strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardName='{1}' ", SboId, Model.CardName.FilterSQL());
                //LIKE
                strSimilarity = "1 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,0 AS Similarity11,0 AS Similarity12, 1 AS Grade ,D.DfTcnician ";
                strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardName like '%{1}%' AND D.CardName<>'{1}' ", SboId, Model.CardName.FilterSQL());
                #endregion

                #region U_Name
                strSimilarity = "0 AS Similarity1, 1 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,0 AS Similarity11,0 AS Similarity12, 2 AS Grade ,D.DfTcnician ";
                if (!string.IsNullOrEmpty(Model.U_Name))
                {
                    strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                    strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardType!='S' AND ((D.CardName LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',D.CardName,'%')) AND D.CardName IS NOT NULL) AND ((D.U_Name LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',D.U_Name,'%')) AND D.U_Name IS NOT NULL) AND ((D.CardFName LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',D.CardFName,'%')) AND D.CardFName IS NOT NULL) ", SboId, Model.U_Name.FilterSQL());
                }
                else
                {
                    strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                    strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardType!='S' AND (D.U_Name='' OR D.U_Name is null) ", SboId);
                }
                #endregion

                #region OCRD表
                if (!string.IsNullOrEmpty(Model.Phone1))  //Phone1
                {
                    strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 1 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,0 AS Similarity11,0 AS Similarity12, 3 AS Grade,D.DfTcnician ";
                    strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                    strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                    strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardCode NOT LIKE 'V%' AND (Tel.Phone LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',Tel.Phone,'%')) ", SboId, crmDealTel(Model.Phone1.Trim()));
                }
                //if (!string.IsNullOrEmpty(Model.Phone2))  //Phone2
                //{
                //    strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 1 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10, 4 AS Grade ";
                //    strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                //    strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                //    strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardCode NOT LIKE 'V%' AND (Tel.Phone LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',Tel.Phone,'%')) ", SboId, crmDealTel(Model.Phone2.Trim()));
                //}








                if (!string.IsNullOrEmpty(Model.Cellular))  //Cellular
                {
                    strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 1 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,0 AS Similarity11,0 AS Similarity12, 5 AS Grade,D.DfTcnician  ";
                    strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                    strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                    strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardCode NOT LIKE 'V%' AND (Tel.Phone LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',Tel.Phone,'%')) ", SboId, crmDealTel(Model.Cellular.Trim()));
                }
                if (!string.IsNullOrEmpty(Model.Fax))       //Fax
                {
                    strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 1 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,0 AS Similarity11,0 AS Similarity12, 6 AS Grade,D.DfTcnician  ";
                    strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                    strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                    strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardCode NOT LIKE 'V%' AND (Tel.Phone LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',Tel.Phone,'%')) ", SboId, crmDealTel(Model.Fax.Trim()));
                }
                #endregion

                #region OCPR表
                foreach (clientOCPR ocpr in Model.ContactList)  //00000
                {
                    if (!string.IsNullOrEmpty(ocpr.Tel1))  //Tel1
                    {
                        strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 1 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,0 AS Similarity11,0 AS Similarity12, 7 AS Grade,D.DfTcnician";
                        strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                        strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                        strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardCode NOT LIKE 'V%' AND (Tel.Phone LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',Tel.Phone,'%')) ", SboId, crmDealTel(ocpr.Tel1.Trim()));
                    }
                    //if (!string.IsNullOrEmpty(ocpr.Tel2))  //Tel2
                    //{
                    //    strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 1 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10, 8 AS Grade ";
                    //    strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                    //    strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                    //    strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardCode NOT LIKE 'V%' AND (Tel.Phone LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',Tel.Phone,'%')) ", SboId, crmDealTel(ocpr.Tel2.Trim()));
                    //}
                    if (!string.IsNullOrEmpty(ocpr.Cellolar))  //Cellolar
                    {
                        strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 1 AS Similarity9, 0 AS Similarity10,0 AS Similarity11,0 AS Similarity12, 9 AS Grade,D.DfTcnician";
                        strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                        strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                        strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardCode NOT LIKE 'V%' AND (Tel.Phone LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',Tel.Phone,'%')) ", SboId, crmDealTel(ocpr.Cellolar.Trim()));
                    }
                    if (!string.IsNullOrEmpty(ocpr.Fax))   //Fax
                    {
                        strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 1 AS Similarity10,0 AS Similarity11,0 AS Similarity12, 10 AS Grade,D.DfTcnician ";
                        strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                        strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                        strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardCode NOT LIKE 'V%' AND (Tel.Phone LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',Tel.Phone,'%')) ", SboId, crmDealTel(ocpr.Fax.Trim()));
                    }
                }
                if (!string.IsNullOrEmpty(Model.EndCustomerName))  //EndCustomerName
                {
                    //==
                    strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,10 AS Similarity11,0 AS Similarity12, 11 AS Grade ,D.DfTcnician ";
                    strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                    strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                    strSql.AppendFormat("WHERE D.sbo_id={0} AND D.U_EndCustomerName IS NOT NULL AND D.U_EndCustomerName='{1}' ", SboId, Model.EndCustomerName.FilterSQL());
                    //LIKE
                    strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,1 AS Similarity11,0 AS Similarity12, 11 AS Grade ,D.DfTcnician ";
                    strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                    strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                    strSql.AppendFormat("WHERE D.sbo_id={0} AND D.U_EndCustomerName IS NOT NULL AND D.U_EndCustomerName like '%{1}%' AND D.U_EndCustomerName<>'{1}' ", SboId, Model.EndCustomerName.FilterSQL());

                }
                if (!string.IsNullOrEmpty(Model.EndCustomerContact))  //EndCustomerContact
                {
                    //==
                    strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,0 AS Similarity11,10 AS Similarity12, 12 AS Grade ,D.DfTcnician ";
                    strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                    strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                    strSql.AppendFormat("WHERE D.sbo_id={0} AND D.U_EndCustomerContact IS NOT NULL AND D.U_EndCustomerContact='{1}' ", SboId, Model.EndCustomerContact.FilterSQL());
                    //LIKE
                    strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10,0 AS Similarity11,1 AS Similarity12, 12 AS Grade ,D.DfTcnician ";
                    strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                    strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                    strSql.AppendFormat("WHERE D.sbo_id={0} AND D.U_EndCustomerContact IS NOT NULL AND D.U_EndCustomerContact like '%{1}%' AND D.U_EndCustomerContact<>'{1}' ", SboId, Model.EndCustomerContact.FilterSQL());
                }
                #endregion
                strSql.Append(") AS E ");
                #region 搜索条件
                strSql.AppendFormat("WHERE sbo_id={0} ", SboId);
                if (!string.IsNullOrEmpty(Query))
                {
                    string[] queryArray = Query.Split('`');
                    for (int i = 0; i < queryArray.Length; i++)
                    {
                        string[] p = queryArray[i].Split(':');
                        if (!string.IsNullOrEmpty(p[1]))
                        {
                            strSql.AppendFormat("AND {0} LIKE '%{1}%' ", p[0], p[1].Trim().FilterSQL());
                        }
                    }
                }
                #endregion
                strSql.Append(" GROUP BY CardCode,CardName,CardFName,SlpName,CntctPrsn,Address,Phone1,Cellular,Balance,U_Name,U_EndCustomerName,U_EndCustomerContact ");
                strSql.Append(" ORDER BY SUM(Similarity1+Similarity2+Similarity3+Similarity4+Similarity5+Similarity6+Similarity7+Similarity8+Similarity9+Similarity10+Similarity11+Similarity12) DESC,MIN(Grade) ASC LIMIT 50 ");
            }
            strSql.Append(") T ");

            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
        }
        #endregion
        #region 处理电话号码
        /// <summary>
        /// 处理电话号码
        /// </summary>
        public static string crmDealTel(string Tel)
        {
            string tmp = Tel.Trim();
            //int num = tmp.LastIndexOf("-");
            //if (num > 0)
            //{
            //    if (tmp.Substring(num + 1, tmp.Length - num - 1).Length < 7)
            //    {
            //        tmp = tmp.Substring(0, num);
            //    }
            //}
            return tmp.Replace("-", "").Replace("－", "");
        }
        #endregion
        #region 搜索供应商相似数据
        /// <summary>
        /// 搜索业务伙伴相似数据
        /// </summary>
        public DataTable SelectSupplierSimilarData(clientOCRD Model, string SboId, string Query, bool IsSearchAll)
        {
            string pubSql = " SELECT D.sbo_id,D.CardCode,D.CardName,D.CardFName,P.SlpName,D.CntctPrsn,";
            pubSql += "CONCAT(IFNULL(Y.Name,''),IFNULL(T.Name,''),IFNULL(D.MailCity,''),IFNULL(D.MailBuildi,'')) AS Address,";
            pubSql += "D.Phone1,D.Cellular,D.Balance,D.U_Name,{0} FROM {1}.crm_OCRD D ";
            pubSql += "LEFT JOIN {1}.crm_OCRY Y ON Y.Code=D.MailCountr LEFT JOIN {1}.crm_OCST T ON T.Code=D.State2 ";
            pubSql += "LEFT JOIN {1}.crm_OSLP P ON P.SlpCode=D.SlpCode AND P.sbo_id=D.sbo_id ";
            string strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10 ";

            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT sbo_id,CardCode,CardName,CardFName,SlpName,CntctPrsn,Address,Phone1,Cellular,Balance,U_Name,");
            strSql.Append("Similarity1, Similarity2, Similarity3, Similarity4, Similarity5, Similarity6, Similarity7, Similarity8, Similarity9, Similarity10,DfTcnician FROM ( ");

            if (IsSearchAll)  //根据搜索条件全局搜索
            {
                strSql.Append("SELECT sbo_id,CardCode,CardName,CardFName,SlpName,CntctPrsn,Address,Phone1,Cellular,Balance,U_Name,");
                strSql.Append("SUM(Similarity1) AS Similarity1,SUM(Similarity2) AS Similarity2,SUM(Similarity3) AS Similarity3,SUM(Similarity4) AS Similarity4,SUM(Similarity5) AS Similarity5,");
                strSql.Append("SUM(Similarity6) AS Similarity6,SUM(Similarity7) AS Similarity7,SUM(Similarity8) AS Similarity8,SUM(Similarity9) AS Similarity9,SUM(Similarity10) AS Similarity10,DfTcnician ");
                strSql.Append(" FROM (");

                string[] whereArray = Query.Split('`');
                int rLineNum = 0;
                for (int i = 0; i < whereArray.Length; i++)
                {
                    #region 按条件搜索
                    string[] p = whereArray[i].Split(':');
                    if (!string.IsNullOrEmpty(p[1]))
                    {
                        if (i == 0)         //业务伙伴名称
                        {
                            strSimilarity = "1 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10, 1 AS Grade,D.DfTcnician ";
                            strSql.Append(string.Format(pubSql, strSimilarity, "nsap_bone"));
                            strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardCode LIKE 'V%' AND D.CardName LIKE '%{1}%' ", SboId, p[1].Trim().FilterSQL());
                            rLineNum++;
                        }
                        else if (i == 1)     //外文名称
                        {
                            strSimilarity = "0 AS Similarity1, 1 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10, 2 AS Grade,D.DfTcnician ";
                            strSql.Append(rLineNum > 0 ? " UNION ALL " : "");
                            strSql.Append(string.Format(pubSql, strSimilarity, "nsap_bone"));
                            strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardCode LIKE 'V%' AND D.CardFName LIKE '%{1}%' ", SboId, p[1].Trim().FilterSQL());
                            rLineNum++;
                        }
                        else if (i == 2)     //电话
                        {
                            strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 1 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10, 3 AS Grade,D.DfTcnician ";
                            strSql.Append(rLineNum > 0 ? " UNION ALL " : "");
                            strSql.Append(string.Format(pubSql, strSimilarity, "nsap_bone"));
                            strSql.AppendFormat(" INNER JOIN (SELECT DISTINCT sbo_id,CardCode FROM {0}.crm_TEL WHERE sbo_id={1} AND CardCode LIKE 'V%' AND Phone LIKE '%{2}%') A ON A.CardCode=D.CardCode AND A.sbo_id=D.sbo_id ", "nsap_bone", SboId, p[1].Trim().Replace("*", "%"));
                            rLineNum++;
                        }
                        else if (i == 3)     //联系人
                        {
                            strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 1 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10, 4 AS Grade,D.DfTcnician ";
                            strSql.Append(rLineNum > 0 ? " UNION ALL " : "");
                            strSql.Append(string.Format(pubSql, strSimilarity, "nsap_bone"));
                            strSql.AppendFormat(" LEFT JOIN {0}.crm_OCPR R ON D.CardCode=R.CardCode AND R.sbo_id=D.sbo_id WHERE D.sbo_id={1} AND D.CardCode LIKE 'V%' AND R.Name LIKE '%{2}%' ", "nsap_bone", SboId, p[1].Trim().FilterSQL());
                            rLineNum++;
                        }
                        else if (i == 4)     //联系人地址
                        {
                            strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 1 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10, 5 AS Grade,D.DfTcnician ";
                            strSql.Append(rLineNum > 0 ? " UNION ALL " : "");
                            strSql.Append(string.Format(pubSql, strSimilarity, "nsap_bone"));
                            strSql.AppendFormat(" INNER JOIN (SELECT DISTINCT sbo_id,CardCode FROM {0}.crm_OCPR WHERE sbo_id={1} AND CardCode NOT LIKE 'V%' AND Address LIKE '%{2}%' ", "nsap_bone", SboId, p[1].Trim().FilterSQL());
                            strSql.AppendFormat(" UNION SELECT DISTINCT sbo_id,CardCode FROM {0}.crm_CRD1 WHERE sbo_id={1} AND CardCode LIKE 'V%' AND Building LIKE '%{2}%') A ON A.CardCode=D.CardCode AND A.sbo_id=D.sbo_id ", "nsap_bone", SboId, p[1].Trim().FilterSQL());
                            rLineNum++;
                        }
                    }
                    #endregion
                }
                strSql.Append(") AS E GROUP BY CardCode,CardName,CardFName,SlpName,CntctPrsn,Address,Phone1,Cellular,Balance,U_Name ");
                strSql.Append(" ORDER BY SUM(Similarity1+Similarity2+Similarity3+Similarity4+Similarity5+Similarity6+Similarity7+Similarity8+Similarity9+Similarity10) DESC,MIN(Grade) ASC LIMIT 50 ");
                if (rLineNum == 0)
                {
                    DataTable dt = new DataTable();
                    return dt;
                }
            }
            else
            {  //搜索当前业务伙伴相似
                strSql.Append("SELECT sbo_id,CardCode,CardName,CardFName,SlpName,CntctPrsn,Address,Phone1,Cellular,Balance,U_Name,");
                strSql.Append("SUM(Similarity1) AS Similarity1,SUM(Similarity2) AS Similarity2,SUM(Similarity3) AS Similarity3,SUM(Similarity4) AS Similarity4,SUM(Similarity5) AS Similarity5,");
                strSql.Append("SUM(Similarity6) AS Similarity6,SUM(Similarity7) AS Similarity7,SUM(Similarity8) AS Similarity8,SUM(Similarity9) AS Similarity9,SUM(Similarity10) AS Similarity10,DfTcnician ");
                strSql.Append(" FROM (");
                #region CardName
                //==
                strSimilarity = "10 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10, 1 AS Grade,D.DfTcnician ";
                strSql.Append(string.Format(pubSql, strSimilarity, "nsap_bone"));
                strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardName='{1}' ", SboId, Model.CardName.FilterSQL());
                //LIKE
                strSimilarity = "1 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10, 1 AS Grade,D.DfTcnician ";
                strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardName like '%{1}%' AND D.CardName<>'{1}' ", SboId, Model.CardName.FilterSQL());
                #endregion

                #region U_Name
                //strSimilarity = "0 AS Similarity1, 1 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10, 2 AS Grade ";
                //if (!string.IsNullOrEmpty(Model.U_Name))
                //{
                //    strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                //    strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardType!='S' AND ((D.CardName LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',D.CardName,'%')) AND D.CardName IS NOT NULL) AND ((D.U_Name LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',D.U_Name,'%')) AND D.U_Name IS NOT NULL) AND ((D.CardFName LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',D.CardFName,'%')) AND D.CardFName IS NOT NULL) ", SboId, Model.U_Name);
                //}
                //else
                //{
                //    strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                //    strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardType!='S' AND (D.U_Name='' OR D.U_Name is null) ", SboId);
                //}
                #endregion

                #region OCRD表
                if (!string.IsNullOrEmpty(Model.Phone1))  //Phone1
                {
                    strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 1 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10, 3 AS Grade,D.DfTcnician ";
                    strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                    strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                    strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardCode LIKE 'V%' AND (Tel.Phone LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',Tel.Phone,'%')) ", SboId, crmDealTel(Model.Phone1.Trim()));
                }
                //if (!string.IsNullOrEmpty(Model.Phone2))  //Phone2
                //{
                //    strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 1 AS Similarity4, 0 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10, 4 AS Grade ";
                //    strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                //    strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                //    strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardCode NOT LIKE 'V%' AND (Tel.Phone LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',Tel.Phone,'%')) ", SboId, crmDealTel(Model.Phone2.Trim()));
                //}
                if (!string.IsNullOrEmpty(Model.Cellular))  //Cellular
                {
                    strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 1 AS Similarity5, 0 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10, 5 AS Grade,D.DfTcnician ";
                    strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                    strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                    strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardCode LIKE 'V%' AND (Tel.Phone LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',Tel.Phone,'%')) ", SboId, crmDealTel(Model.Cellular.Trim()));
                }
                if (!string.IsNullOrEmpty(Model.Fax))       //Fax
                {
                    strSimilarity = "0 AS Similarity1, 0 AS Similarity2, 0 AS Similarity3, 0 AS Similarity4, 0 AS Similarity5, 1 AS Similarity6, 0 AS Similarity7, 0 AS Similarity8, 0 AS Similarity9, 0 AS Similarity10, 6 AS Grade,D.DfTcnician ";
                    strSql.Append(" UNION ALL " + string.Format(pubSql, strSimilarity, "nsap_bone"));
                    strSql.AppendFormat("LEFT JOIN {0}.crm_Tel Tel on Tel.CardCode = D.CardCode and Tel.sbo_id=D.sbo_id ", "nsap_bone");
                    strSql.AppendFormat("WHERE D.sbo_id={0} AND D.CardName IS NOT NULL AND D.CardCode LIKE 'V%' AND (Tel.Phone LIKE '%{1}%' OR '{1}' LIKE CONCAT('%',Tel.Phone,'%')) ", SboId, crmDealTel(Model.Fax.Trim()));
                }
                #endregion


                strSql.Append(") AS E ");
                #region 搜索条件
                strSql.AppendFormat("WHERE sbo_id={0} ", SboId);
                if (!string.IsNullOrEmpty(Query))
                {
                    string[] queryArray = Query.Split('`');
                    for (int i = 0; i < queryArray.Length; i++)
                    {
                        string[] p = queryArray[i].Split(':');
                        if (!string.IsNullOrEmpty(p[1]))
                        {
                            strSql.AppendFormat("AND {0} LIKE '%{1}%' ", p[0], p[1].Trim().FilterSQL());
                        }
                    }
                }
                #endregion
                strSql.Append(" GROUP BY CardCode,CardName,CardFName,SlpName,CntctPrsn,Address,Phone1,Cellular,Balance,U_Name ");
                strSql.Append(" ORDER BY SUM(Similarity1+Similarity2+Similarity3+Similarity4+Similarity5+Similarity6+Similarity7+Similarity8+Similarity9+Similarity10) DESC,MIN(Grade) ASC LIMIT 50 ");
            }
            strSql.Append(") T ");

            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
        }
        #endregion
        #region 查询业务伙伴附件
        /// <summary>
        /// 查询业务伙伴附件
        /// </summary>
        public DataTable SelectClientFiles(string FileType, string SboId, string IssueReason)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT file_id,sbo_id,file_sn,file_nm,file_type_id,docEntry,job_id,file_ver,acct_id,issue_reason,");
            strSql.Append("file_path,view_file_path,content,remarks,file_status,upd_dt,user_nm,type_nm ");

            strSql.Append("FROM (SELECT a.file_id,a.sbo_id,a.file_sn,a.file_nm,a.file_type_id,a.docEntry,a.job_id,a.file_ver,a.acct_id,");
            strSql.Append("a.issue_reason,a.file_path,a.view_file_path,a.content,a.remarks,a.file_status,a.upd_dt,b.user_nm,c.type_nm ");
            strSql.AppendFormat("FROM {0}.file_main a LEFT JOIN {1}.base_user b ON a.acct_id=b.user_id ", "nsap_oa", "nsap_base");
            strSql.AppendFormat("LEFT JOIN {0}.file_type c ON a.file_type_id=c.type_id) T ", "nsap_oa");

            strSql.AppendFormat("WHERE file_type_id={0} AND sbo_id={1} AND issue_reason='{2}' AND file_status<>{3} ", FileType, SboId, IssueReason, 3);


            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
        }
        #endregion
        #region 查询业务伙伴报价单
        /// <summary>
        /// 查询业务伙伴报价单
        /// </summary>
        public DataTable SelectOqut(SelectOqutReq selectOqutReq)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(
                "SELECT  A.DocEntry,B.SlpName,A.DocTotal, (A.DocTotal-A.PaidToDate)  AS OpenDocTotal,A.CreateDate,A.DocStatus,A.Printed  ");
            strSql.AppendFormat("FROM OQUT A LEFT JOIN OSLP B ON A.SlpCode=B.SlpCode WHERE A.CardCode='{0}'", selectOqutReq.CardCode);
            if (!string.IsNullOrWhiteSpace(selectOqutReq.Docentry))
            {
                strSql.AppendFormat("AND A.DocEntry='{0}' ", selectOqutReq.Docentry);
            }
            if (!string.IsNullOrWhiteSpace(selectOqutReq.Slpname))
            {
                strSql.AppendFormat("AND B.SlpName='{0}' ", selectOqutReq.Slpname);
            }
            if (selectOqutReq.Status == "ON")
            {
                strSql.AppendFormat("AND (a.DocStatus = 'O' AND a.Printed = 'N' AND a.CANCELED = 'N') ");
            }
            if (selectOqutReq.Status == "OY")
            {
                strSql.AppendFormat("AND (a.DocStatus = 'O' AND a.Printed = 'Y' AND a.CANCELED = 'N') ");
            }
            if (selectOqutReq.Status == "CY")
            {
                strSql.AppendFormat("AND a.CANCELED = 'Y'  ");
            }
            if (selectOqutReq.Status == "CN")
            {
                strSql.AppendFormat("AND (a.DocStatus = 'C' AND a.CANCELED = 'N') ");
            }

            //时间区间
            if (!string.IsNullOrWhiteSpace(selectOqutReq.StartTime) && !string.IsNullOrWhiteSpace(selectOqutReq.EndTime))
            {

                strSql.AppendFormat("a.UpdateDate BETWEEN '{0}' AND '{1}' AND ", selectOqutReq.StartTime, selectOqutReq.EndTime);
            }
            strSql.AppendFormat("ORDER BY A.DocEntry DESC");

            return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
        }
        #endregion
        #region 查询业务伙销售订单
        /// <summary>
        /// 查询业务伙销售订单
        /// </summary>
        public DataTable SelectOrdr(SelectOrdrReq selectOrdrReq)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(
                "SELECT  A.DocEntry,B.SlpName,A.DocTotal, (A.DocTotal-A.PaidToDate)  AS OpenDocTotal,A.CreateDate,A.DocStatus,A.Printed  ");
            strSql.AppendFormat("FROM ORDR A LEFT JOIN OSLP B ON A.SlpCode=B.SlpCode WHERE A.CardCode='{0}'", selectOrdrReq.CardCode);
            if (!string.IsNullOrWhiteSpace(selectOrdrReq.Docentry))
            {
                strSql.AppendFormat("AND A.DocEntry='{0}' ", selectOrdrReq.Docentry);
            }
            if (!string.IsNullOrWhiteSpace(selectOrdrReq.Slpname))
            {
                strSql.AppendFormat("AND B.SlpName='{0}' ", selectOrdrReq.Slpname);
            }
            if (selectOrdrReq.Status == "ON")
            {
                strSql.AppendFormat("AND (a.DocStatus = 'O' AND a.Printed = 'N' AND a.CANCELED = 'N') ");
            }
            if (selectOrdrReq.Status == "OY")
            {
                strSql.AppendFormat("AND (a.DocStatus = 'O' AND a.Printed = 'Y' AND a.CANCELED = 'N') ");
            }
            if (selectOrdrReq.Status == "CY")
            {
                strSql.AppendFormat("AND a.CANCELED = 'Y'  ");
            }
            if (selectOrdrReq.Status == "CN")
            {
                strSql.AppendFormat("AND (a.DocStatus = 'C' AND a.CANCELED = 'N') ");
            }

            //时间区间
            if (!string.IsNullOrWhiteSpace(selectOrdrReq.StartTime) && !string.IsNullOrWhiteSpace(selectOrdrReq.EndTime))
            {

                strSql.AppendFormat("a.UpdateDate BETWEEN '{0}' AND '{1}' AND ", selectOrdrReq.StartTime, selectOrdrReq.EndTime);
            }
            strSql.AppendFormat("ORDER BY A.DocEntry DESC");

            return UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
        }
        #endregion

        /// <summary>
        /// 判断客户是否存在审核中的报价单
        /// </summary>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        public async Task<TableData> IsExistsReviewJob(string cardCode)
        {
            var tableData = new TableData();

            var query = UnitWork.Find<wfa_job>(j => j.job_state == 1 && j.job_nm == "销售报价单" && j.card_code == cardCode)
                .Select(j => new
                {
                    j.job_id
                });

            tableData.Data = await query.ToListAsync();
            tableData.Count = await query.CountAsync();

            return tableData;
        }

        /// <summary>
        /// 业务员将客户主动移入公海
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> MoveInCustomerSea(MoveInCustomerSeaReq req)
        {
            var result = new Infrastructure.Response();

            //判断是否是公海管理员
            var isCustomerSeaAdmin = _auth.GetCurrentUser().Roles.Any(r => r.Name == "公海管理员");
            //操作人名称
            var userName = _auth.GetCurrentUser()?.User?.Name;

            //根据客户代码查询所属销售员信息
            var slpInfo = await (from c in UnitWork.Find<OCRD>(null)
                                 join s in UnitWork.Find<OSLP>(null) on c.SlpCode equals s.SlpCode
                                 where c.CardCode == req.CardCode
                                 select new
                                 {
                                     s.SlpCode,
                                     s.SlpName,
                                     c.CreateDate
                                 }).FirstOrDefaultAsync();

            //销售员所属部门
            var dept = await (from u in UnitWork.Find<base_user>(null)
                              join ud in UnitWork.Find<base_user_detail>(null) on u.user_id equals ud.user_id
                              join d in UnitWork.Find<base_dep>(null) on ud.dep_id equals d.dep_id
                              where u.user_nm == slpInfo.SlpName
                              select d.dep_alias).FirstOrDefaultAsync();

            //如果是非管理员,则判断该客户是否属于此销售员(防止销售员把其他销售员的客户移入,现实中不太可能,这里做一次校验)
            if (!isCustomerSeaAdmin && slpInfo.SlpName != userName)
            {
                result.Code = 500;
                result.Message = "此客户不属于此销售员";
                return result;
            }

            //判断客户是否是从公海领取的客户
            var isFromCustomerSea = await UnitWork.Find<CustomerSalerHistory>(c => c.CustomerNo == req.CardCode && c.SlpCode == slpInfo.SlpCode).FirstOrDefaultAsync();
            if (isFromCustomerSea != null)
            {
                var customerSeaConfig = await UnitWork.Find<CustomerSeaConf>(null).FirstOrDefaultAsync();
                //如果是则判断是否符合领取后多少天才能重新放入的规则
                if (customerSeaConfig.AutomaticEnable)
                {
                    var startDate = isFromCustomerSea.CreateTime;
                    var diff = (DateTime.Now - startDate).Value.TotalDays;
                    if (diff <= customerSeaConfig.BackDay)
                    {
                        result.Code = 500;
                        result.Message = $"从公海领取的客户,{customerSeaConfig.AutomaticDayLimit}天内不能放回公海";
                        return result;
                    }
                }
            }
            //如果是这个原因,则加入黑名单
            if (req.Remark == "公司已注销"|| req.Remark == "失信客户")
            {
                using var tran = UnitWork.GetDbContext<CustomerList>().Database.BeginTransaction();
                try
                {
                    await UnitWork.AddAsync<SpecialCustomer, int>(new SpecialCustomer
                    {
                        CustomerNo = req.CardCode,
                        CustomerName = req.CardName,
                        SalerId = slpInfo.SlpCode.ToString(),
                        SalerName = slpInfo.SlpName,
                        DepartmentId = dept,
                        DepartmentName = dept,
                        Type = 0,
                        CreateUser = userName,
                        CreateDatetime = DateTime.Now,
                        UpdateUser = userName,
                        UpdateDatetime = DateTime.Now,
                        Remark = req.Remark
                    });
                    //await UnitWork.AddAsync<CustomerMoveHistory, int>(new CustomerMoveHistory
                    //{
                    //    CardCode = req.CardCode,
                    //    CardName = req.CardName,
                    //    SlpName = slpInfo.SlpName,
                    //    MoveInType = "主动移入",
                    //    Remark = req.Remark,
                    //    CreateTime = DateTime.Now,
                    //    CreateUser = userName,
                    //    UpdateTime = DateTime.Now,
                    //    UpdateUser = userName
                    //});
                    await UnitWork.SaveAsync();
                    await tran.CommitAsync();
                }
                catch(Exception ex)
                {
                    result.Code = 500;
                    result.Message = ex.Message ?? ex.InnerException?.Message ?? "";
                    await tran.RollbackAsync();
                }
            }
            //其他原因则加入公海
            else
            {
                var customerLists = new List<CustomerList>();
                customerLists.Add(new CustomerList
                {
                    DepartMent = dept,
                    CustomerNo = req.CardCode,
                    CustomerName = req.CardName,
                    CustomerSource = "",
                    CustomerCreateDate = slpInfo.CreateDate,
                    SlpCode = slpInfo.SlpCode,
                    SlpName = slpInfo.SlpName,
                    Label = "已经掉入公海",
                    LabelIndex = 3,
                    CreateUser = userName,
                    CreateDateTime = DateTime.Now,
                    UpdateUser = userName,
                    UpdateDateTime = DateTime.Now,
                    IsDelete = false
                });

                using var tran = UnitWork.GetDbContext<CustomerList>().Database.BeginTransaction();
                try
                {
                    await UnitWork.BatchAddAsync<CustomerList, int>(customerLists.ToArray());
                    await UnitWork.UpdateAsync<OCRD>(c => customerLists.Select(x => x.CustomerNo).Contains(c.CardCode), x => new OCRD
                    {
                        SlpCode = null
                    });
                    await UnitWork.AddAsync<CustomerMoveHistory, int>(new CustomerMoveHistory
                    {
                        CardCode = req.CardCode,
                        CardName = req.CardName,
                        SlpCode = slpInfo.SlpCode,
                        SlpName = slpInfo.SlpName,
                        MoveInType = "主动移入",
                        Remark = req.Remark,
                        CreateTime = DateTime.Now,
                        CreateUser = userName,
                        UpdateTime = DateTime.Now,
                        UpdateUser = userName
                    });
                    await UnitWork.SaveAsync();
                    await tran.CommitAsync();
                }
                catch (Exception ex)
                {
                    result.Code = 500;
                    result.Message = ex.Message ?? ex.InnerException?.Message ?? "";
                    await tran.RollbackAsync();
                }
            }


            return result;
        }

        #region 客户详情页

        #region 客户跟进记录
        /// <summary>
        /// 新增跟进
        /// </summary>
        /// <param name="addClueFollowUpReq"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> AddClientFollowAsync(ClientFollowUp clientFollowUp, bool isAdd)
        {
            var response = new Infrastructure.Response();
            response.Message = "";
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var userId = loginUser.User_Id.Value;
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            int SlpCode = Convert.ToInt16(GetUserInfoById(sboid.ToString(), userId.ToString(), "1"));
            if (isAdd)
            {
                clientFollowUp.SlpCode = SlpCode;
                clientFollowUp.CreateUser = loginUser.User_Id.Value;
                clientFollowUp.CreateDate = DateTime.Now;
                clientFollowUp.IsDelete = false;
                await UnitWork.AddAsync<ClientFollowUp, int>(clientFollowUp);
            }
            else
            {
                ClientFollowUp info = UnitWork.Find<ClientFollowUp>(q => q.Id == clientFollowUp.Id && !q.IsDelete).FirstOrDefault();
                info.CardCode = clientFollowUp.CardCode;
                info.CardName = clientFollowUp.CardName;
                info.SlpCode = SlpCode;
                info.SlpName = clientFollowUp.SlpName;
                info.Contacts = clientFollowUp.Contacts;
                info.FollowType = clientFollowUp.FollowType;
                info.NextTime = clientFollowUp.NextTime;// NextFollowTime;
                info.Remark = clientFollowUp.Remark;
                info.FileId = clientFollowUp.FileId;
                info.ImgId = clientFollowUp.ImgId;
                info.ImgName = clientFollowUp.ImgName;
                info.FileName = clientFollowUp.FileName;
                info.UpdateUser = loginUser.User_Id.Value;
                info.UpdateDate = DateTime.Now;
                await UnitWork.UpdateAsync<ClientFollowUp>(info);
            }
            await UnitWork.SaveAsync();
            response.Message = "操作成功";
            return response;
        }


        /// <summary>
        /// 跟进列表
        /// </summary>
        /// <param name="CardCode"></param>
        /// <returns></returns>
        public async Task<List<ClientFollowUp>> ClientFollowUpByIdAsync(string CardCode)
        {
            var result = new List<ClientFollowUp>();

            var loginUser = _auth.GetCurrentUser().User;
            if (loginUser.Name == "韦京生" || loginUser.Name == "郭睿心" || loginUser.Name == "骆灵芝")
            {
                result = UnitWork.Find<ClientFollowUp>(q => q.CardCode == CardCode && !q.IsDelete).OrderByDescending(t => t.CreateDate).MapToList<ClientFollowUp>();
            }
            else
            {
                var userId = _serviceBaseApp.GetUserNaspId();
                var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
                int SlpCode = Convert.ToInt16(GetUserInfoById(sboid.ToString(), userId.ToString(), "1"));
                result = UnitWork.Find<ClientFollowUp>(q => q.CardCode == CardCode && q.SlpCode == SlpCode && !q.IsDelete).OrderByDescending(t => t.CreateDate).MapToList<ClientFollowUp>();
            }
            return result;
        }

        /// <summary>
        /// 删除跟进
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> DeleteFollowByCodeAsync(int Id)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var clientFollowUp = await UnitWork.FindSingleAsync<ClientFollowUp>(q => q.Id == Id);

            clientFollowUp.IsDelete = true;
            await UnitWork.UpdateAsync(clientFollowUp);
            await UnitWork.SaveAsync();

            return true;
        }

        /// <summary>
        /// 常用语列表
        /// </summary>
        /// <param name="CardCode"></param>
        /// <returns></returns>
        public async Task<List<ClientFollowUpPhrase>> ClientFollowUpPhraseAsync(int type)
        {
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            int SlpCode = Convert.ToInt16(GetUserInfoById(sboid.ToString(), userId.ToString(), "1"));
            var result = new List<ClientFollowUpPhrase>();
            var ClientFollowUpPhrase = UnitWork.Find<ClientFollowUpPhrase>(q => q.SlpCode == SlpCode && q.Type == type).MapToList<ClientFollowUpPhrase>();
            return ClientFollowUpPhrase;
        }

        /// <summary>
        /// 添加常用语
        /// </summary>
        /// <param name="addClueFollowUpReq"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> AddClientFollowPhraseAsync(List<ClientFollowUpPhrase> list)
        {
            var response = new Infrastructure.Response();
            response.Message = "";
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var userId = loginUser.User_Id.Value;
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            int SlpCode = Convert.ToInt16(GetUserInfoById(sboid.ToString(), userId.ToString(), "1"));
            int type = list[0].Type;
            await UnitWork.DeleteAsync<ClientFollowUpPhrase>(q => q.SlpCode == SlpCode && q.Type == type);
            foreach (var item in list)
            {
                await UnitWork.AddAsync<ClientFollowUpPhrase, int>(new ClientFollowUpPhrase
                {
                    SlpCode = SlpCode,
                    Remark = item.Remark,
                    Type = item.Type,
                    CreateUser = loginUser.Name,
                    CreateDate = DateTime.Now
                });
            }
            await UnitWork.SaveAsync();
            response.Message = "操作成功";
            return response;
        }

        /// <summary>
        /// 向即将有跟进任务的业务员发送提醒信息
        /// </summary>
        /// <returns></returns>
        public async Task PushMessageToSlp()
        {
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddMinutes(1);
            //查询所有时间段内待跟进的任务
            var query = await (UnitWork.Find<ClientFollowUp>(c => c.NextTime >= startTime && c.NextTime <= endTime).Select(g => new
            {
                SlpName = g.SlpName,
                CardName = g.CardName
            })).ToListAsync();
            //查看有哪些业务员要发送提醒
            foreach (var slp in query)
            {
                await _hubContext.Clients.User(slp.SlpName).SendAsync("ReceiveMessage", "系统", $"您有1个客户待跟进，客户名称：" + slp.CardName);
            }

            var querySchedule = await (UnitWork.Find<ClientSchedule>(c => c.RemindTime >= startTime && c.RemindTime <= endTime).Select(g => new
            {
                SlpName = g.SlpName,
                Title = g.Title
            })).ToListAsync();
            //查看有哪些业务员要发送提醒
            foreach (var slp in querySchedule)
            {
                await _hubContext.Clients.User(slp.SlpName).SendAsync("ReceiveMessage", "系统", $"您有1个日程待跟进，日程标题：" + slp.Title);
            }
        }
        #endregion
        #endregion
    }
}

