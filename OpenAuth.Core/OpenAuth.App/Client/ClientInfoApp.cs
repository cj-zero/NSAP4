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
using OpenAuth.App.ClientRelation;
using DocumentFormat.OpenXml.Math;
using OpenAuth.App.Request;
using Microsoft.Extensions.Logging;
using EdgeCmd;

namespace OpenAuth.App.Client
{
    public class ClientInfoApp : OnlyUnitWorkBaeApp
    {
        ServiceBaseApp _serviceBaseApp;
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
        private readonly ClientRelationApp _clientRelationApp;
        private readonly IHubContext<MessageHub> _hubContext;
        private ILogger<ClientInfoApp> _logger;
        public ClientInfoApp(ServiceSaleOrderApp serviceSaleOrderApp, ClientRelationApp clientRelationApp, ServiceBaseApp serviceBaseApp, IUnitWork unitWork, IAuth auth, ILogger<ClientInfoApp> logger, IHubContext<MessageHub> hubContext) : base(unitWork, auth)
        {
            _serviceBaseApp = serviceBaseApp;
            _serviceSaleOrderApp = serviceSaleOrderApp;
            _hubContext = hubContext;
            _clientRelationApp = clientRelationApp;
            _logger = logger;
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
            //20221021 lims推广员使用线索转客户功能无法新增客户
            if (addClientInfoReq.baseEntry > 0 && addClientInfoReq.type == "Add")
            {
                var erpLims = UnitWork.FindSingle<LimsInfo>(u => u.UserId == loginUser.Id && u.Type == "LIMS");
                if (erpLims != null)
                {
                    _logger.LogWarning("lims推广员使用线索转客户功能无法新增客户,参数为：" + JsonConvert.SerializeObject(addClientInfoReq));
                    return "0";
                }
            }

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
                //添加4.0关系
                await _clientRelationApp.AddJobRelations(new ClientRelation.Request.AddJobRelReq
                {
                    Jobid = Convert.ToInt32(result),
                    Terminals = addClientInfoReq.Terminals,
                    Creator = loginUser.Name,
                    CreatorId = loginUser.Id,
                    Origin = 0
                });
                //新增更新草稿客户关系
                await _clientRelationApp.SaveScriptRelations(new ClientRelation.Request.JobScriptReq
                {
                    JobId = Convert.ToInt32(result),
                    ClientNo = "",
                    Flag = OCRD.is_reseller == "N" ? 0 : 1,
                    ClientName = OCRD.CardName,
                    EndCustomerName = addClientInfoReq.Terminals,
                    Operator = loginUser.Name,
                    Operatorid = loginUser.Id,
                    Initial = 0
                });

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
                        //添加4.0关系
                        await _clientRelationApp.AddJobRelations(new ClientRelation.Request.AddJobRelReq
                        {
                            Jobid = Convert.ToInt32(JobId),
                            Terminals = addClientInfoReq.Terminals,
                            Creator = loginUser.Name,
                            CreatorId = loginUser.Id,
                            Origin  = 0
                        });
                        await _clientRelationApp.SaveScriptRelations(new ClientRelation.Request.JobScriptReq
                        {
                            JobId = Convert.ToInt32(JobId),
                            ClientNo = "",
                            Flag = OCRD.is_reseller == "N" ? 0 : 1,
                            ClientName = OCRD.CardName,
                            EndCustomerName = addClientInfoReq.Terminals,
                            Operator = loginUser.Name,
                            Operatorid = loginUser.Id,
                            Initial = 0
                        });
                    }
                    else if (rJobNm == "修改业务伙伴")
                    {
                        //添加4.0关系
                        await _clientRelationApp.AddJobRelations(new ClientRelation.Request.AddJobRelReq
                        {
                            Jobid = Convert.ToInt32(JobId),
                            Terminals = addClientInfoReq.Terminals,
                            Creator = loginUser.Name,
                            CreatorId = loginUser.Id,
                            Origin =1
                        });

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
            //当前登录者业务员编码
            int slpCode = UnitWork.Find<sbo_user>(q => q.user_id == loginUser.User_Id).Select(q => q.sale_id).FirstOrDefault().Value;
            if (slpCode != addClientInfoReq.clientInfo.SlpCode.ToInt())
            {
                var crd1 = addClientInfoReq.clientInfo.AddrList.Where(q => q.isLims == true).ToList();
                var ocpr = addClientInfoReq.clientInfo.ContactList.Where(q => q.isLims == true).ToList();

                //如果是lims推广员
                SavelimsData(crd1, ocpr, addClientInfoReq.clientInfo.CardCode, slpCode);
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
            int depID, string label, string contectTel, string slpName, string isReseller, int? Day, string CntctPrsn, string address,
            string U_CardTypeStr, string U_ClientSource, string U_CompSector, string U_TradeType, string U_StaffScale,
            DateTime? CreateStartTime, DateTime? CreateEndTime, DateTime? DistributionStartTime, DateTime? DistributionEndTime,
            decimal? dNotesBalStart, decimal? dNotesBalEnd, decimal? ordersBalStart, decimal? ordersBalEnd,
            decimal? balanceStart, decimal? balanceEnd, decimal? balanceTotalStart, decimal? balanceTotalEnd, string CardName, out int rowCount)
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
            filterString.Append(string.Format(" T.sbo_id={0} ", sboid.ToString()));
            //modify by yangis @2022.06.24
            if (!string.IsNullOrWhiteSpace(CardName))
            {
                filterString.Append($" and (cardname like '%" + CardName + "%' or cardfname like '%" + CardName + "%' )");
            }
            if (!string.IsNullOrWhiteSpace(slpName))
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
                filterString.Append($" and T.cardcode in ( select cardcode from nsap_bone.crm_ocpr where name like '%{CntctPrsn}%') ");
            }
            if (!string.IsNullOrWhiteSpace(address))
            {
                filterString.Append($" and (T.cardcode in ( SELECT CardCode FROM nsap_bone.crm_crd1 WHERE Building like '" + address + "') or Address like '%" + address + "%') ");
            }
            if (Day != null)
            {
                DateTime date = DateTime.Now;
                filterString.Append($" and TO_DAYS(NOW()) - TO_DAYS(T1.UpdateDate) > {Day.Value} ");
            }
            //客户类型
            if (!string.IsNullOrWhiteSpace(U_CardTypeStr))
            {
                filterString.Append($" and U_CardTypeStr = '{U_CardTypeStr}'  ");
            }
            //客户来源
            if (!string.IsNullOrWhiteSpace(U_ClientSource))
            {
                filterString.Append($" and U_ClientSource = '{U_ClientSource}'  ");
            }
            //所属行业
            if (!string.IsNullOrWhiteSpace(U_CompSector))
            {
                filterString.Append($" and U_CompSector = '{U_CompSector}'  ");
            }
            //贸易类型
            if (!string.IsNullOrWhiteSpace(U_TradeType))
            {
                filterString.Append($" and U_TradeType = '{U_TradeType}' ");
            }
            //人员规模
            if (!string.IsNullOrWhiteSpace(U_StaffScale))
            {
                filterString.Append($" and U_StaffScale = '{U_StaffScale}' ");
            }
            //创建开始时间
            if (CreateStartTime != null)
            {
                filterString.Append($" and CreateDate >= '{CreateStartTime}' ");
            }
            //创建结束时间
            if (CreateEndTime != null)
            {
                filterString.Append($" and CreateDate <= '{CreateEndTime}' ");
            }
            //归属变更开始时间
            if (DistributionStartTime != null)
            {
                filterString.Append($" and T1.UpdateDate >= '{DistributionStartTime}' ");
            }
            //归属变更结束时间
            if (DistributionEndTime != null)
            {
                filterString.Append($" and T1.UpdateDate <= '{DistributionEndTime}' ");
            }
            //未清订单余额
            if (ordersBalStart != null)
            {
                filterString.Append($" and ordersBal >= {ordersBalStart} ");
            }
            //未清订单余额
            if (ordersBalEnd != null)
            {
                filterString.Append($" and ordersBal <= {ordersBalEnd} ");
            }
            //未清交货单余额
            if (dNotesBalStart != null)
            {
                filterString.Append($" and dNotesBal >= {dNotesBalStart} ");
            }
            //未清交货单余额
            if (dNotesBalEnd != null)
            {
                filterString.Append($" and dNotesBal <= {dNotesBalEnd} ");
            }
            //科目余额
            if (balanceStart != null)
            {
                filterString.Append($" and T3.Balance >= {balanceStart} ");
            }
            //科目余额
            if (balanceEnd != null)
            {
                filterString.Append($" and T3.Balance <= {balanceEnd} ");
            }
            //总科目余额
            if (balanceTotalStart != null)
            {
                filterString.Append($" and T4.BalanceTotal >= {balanceTotalStart} ");
            }
            //总科目余额
            if (balanceTotalEnd != null)
            {
                filterString.Append($" and T4.BalanceTotal <= {balanceTotalEnd} ");
            }
            //黑名单客户也不在客户列表上显示
            filterString.Append($" and T.CardCode not in ( select Customer_No from erp4_serve.special_customer where type = 0) ");
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
                            filterString.AppendFormat(" (SlpCode={0} or T.CardCode in (select CardCode from client_limsinfomap where LimsInfoId in( select LimsInfoId from client_limsinfo where SlpCode={0})) and t.sbo_id = 1) and T.CardCode like 'C%' ", rSalCode);
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
            filedName.Append("T.CardCode,T1.UpdateDate DistributionDate, FollowUpTime,TO_DAYS(NOW()) - TO_DAYS(FollowUpTime) FollowUpDay, CardName,SlpCode, SlpName, Technician, CntctPrsn, Address, Phone1, Cellular, relationFlag ,U_is_reseller, ");
            if (rIsViewSales)
            {
                filedName.Append("T3.Balance,  T4.BalanceTotal, DNotesBal, OrdersBal, OprCount, ");
            }
            else
            {
                filedName.Append("'****' AS Balance, '******************************' AS BalanceTotal, '****' AS DNotesBal, '****' AS OrdersBal, '****' AS OprCount, ");
            }
            filedName.Append("CreateDate,T.UpdateDate , ");
            filedName.Append(" GroupName,Free_Text,U_ClientSource,U_CompSector,U_TradeType,U_CardTypeStr,U_StaffScale ");
            if (IsOpenSap)
            {
                tableName.Append("(SELECT A.sbo_id,A.CardCode,A.CardName,A.CardFName,A.SlpCode,B.SlpName,CONCAT(IFNULL(E.lastName,''),IFNULL(E.firstName,'')) as Technician,");
                tableName.Append("A.CntctPrsn,CONCAT(IFNULL(F.Name,''),IFNULL(G.Name,''),IFNULL(A.City,''),IFNULL(A.Building,'')) AS Address, ");
                tableName.Append("A.Phone1,A.Cellular,A.U_is_reseller,");
                tableName.Append("A.DNotesBal,A.OrdersBal,A.OprCount,A.CreateDate,A.upd_dt UpdateDate,A.DfTcnician ");
                tableName.Append(", case  when LOCATE(\"C\", Y.SubNo)  > 0   ||  LOCATE(\"C\", Y.ParentNo) > 0   then 1  ELSE 0 end as relationFlag ");
                tableName.Append(" ,A.QryGroup2,A.QryGroup3 ");
                tableName.Append(",C.GroupName,A.Free_Text,A.U_ClientSource,A.U_CompSector,A.U_TradeType,A.U_CardTypeStr,A.U_StaffScale ");

                tableName.Append("FROM nsap_bone.crm_ocrd A ");
                tableName.Append("LEFT JOIN nsap_bone.crm_oslp B ON B.SlpCode=A.SlpCode  ");
                tableName.Append("LEFT JOIN nsap_bone.crm_ocrg C ON C.GroupCode=A.GroupCode  ");
                tableName.Append("LEFT JOIN nsap_bone.crm_oidc D ON D.Code=A.Indicator  ");
                tableName.Append("LEFT JOIN nsap_bone.crm_ohem E ON E.empID=A.DfTcnician and E.sbo_id = A.sbo_id ");
                tableName.AppendFormat("LEFT JOIN  (SELECT c.Id, c.SubNo ,c.ClientNo, c.IsActive, c.ParentNo, c.IsDelete, c.ScriptFlag,ROW_NUMBER() OVER (PARTITION BY ClientNo ORDER BY CreateDate  DESC) rn from {0}.clientrelation c)   Y ON Y.ClientNo = A.CardCode  AND Y.IsActive =1 AND Y.ScriptFlag =0 AND   Y.rn = 1  AND  Y.IsDelete = 0   ", "erp4");
                tableName.Append("LEFT JOIN nsap_bone.crm_ocry F ON F.Code=A.Country  ");
                tableName.Append("LEFT JOIN nsap_bone.crm_ocst G ON G.Code=A.State1 ");

                tableName.Append("FROM nsap_bone.crm_ocrd A ");
                tableName.Append("LEFT JOIN nsap_bone.crm_oslp B ON B.SlpCode=A.SlpCode  ");
                tableName.Append("LEFT JOIN nsap_bone.crm_ocrg C ON C.GroupCode=A.GroupCode  ");
                tableName.Append("LEFT JOIN nsap_bone.crm_oidc D ON D.Code=A.Indicator  ");
                tableName.Append("LEFT JOIN nsap_bone.crm_ohem E ON E.empID=A.DfTcnician and E.sbo_id = A.sbo_id ");
                tableName.Append("LEFT JOIN nsap_bone.crm_ocry F ON F.Code=A.Country  ");
                tableName.Append("LEFT JOIN nsap_bone.crm_ocst G ON G.Code=A.State1 ");

                //筛选标签
                if (!string.IsNullOrWhiteSpace(label))
                {
                    //全部客户
                    if (label == "0") { }
                    //未报价客户
                    if (label == "1" || Day != null)
                    {
                        //在报价单中不存在的客户
                        tableName.Append(" LEFT JOIN nsap_bone.sale_oqut AS q on A.CardCode = q.CardCode ");
                        tableName.Append(" WHERE q.CardCode IS NULL ");
                    }
                    //已成交客户
                    else if (label == "2")
                    {
                        //在交货单中存在的客户
                        tableName.Append(@" where exists(select 1 from nsap_bone.sale_odln as n where n.CardCode = A.CardCode) ");
                    }
                    //公海领取(掉入公海的客户被重新分配和领取,但是分配和领取之后没有做过报价单,做过单了就属于正常用户)
                    else if (label == "3")
                    {
                        //这些客户还没有做过单
                        tableName.Append(@" LEFT JOIN nsap_bone.sale_oqut AS q on A.CardCode = q.CardCode ");
                        tableName.Append(" WHERE q.CardCode IS NULL ");
                        //并且在历史归属表中存在但是公海中不存在的客户(说明已被领取)
                        tableName.Append($" AND A.CardCode IN (select distinct n.CustomerNo from erp4_serve.customer_saler_history n left join erp4_serve.customer_list m on n.CustomerNo = m.Customer_No where m.Customer_No is NULL) ");
                    }
                    //即将掉入公海
                    else if (label == "4")
                    {
                        tableName.Append(" LEFT JOIN erp4_serve.customer_list AS q on A.CardCode = q.Customer_No and q.Label_Index = 4 ");
                        tableName.Append(" WHERE q.Customer_No IS NOT NULL ");
                    }
                }
                tableName.Append(") T");
                //取公海领取记录表，得到客户的最新分配时间
                tableName.Append(" LEFT JOIN (SELECT CustomerNo CardCode, max(CreateTime) UpdateDate from erp4_serve.customer_saler_history GROUP BY CustomerNo) T1 on T.CardCode = T1.CardCode ");
                tableName.Append(" LEFT JOIN (SELECT CardCode, max(CreateDate) FollowUpTime from erp4_serve.client_followup GROUP BY CardCode) T2 on T.CardCode = T2.CardCode ");
                tableName.Append(" LEFT JOIN (SELECT CardCode,sbo_id,SUM(Balance) Balance FROM nsap_bone.crm_ocrd_oldsbo_balance GROUP BY CardCode) T3 on T.CardCode = T3.CardCode and T.sbo_id = T3.sbo_id ");
                tableName.Append(" LEFT JOIN (SELECT CardCode,SUM(Balance) BalanceTotal FROM nsap_bone.crm_ocrd_oldsbo_balance WHERE sbo_id IN(SELECT sbo_id  FROM nsap_base.sbo_info) GROUP BY CardCode) T4 on T.CardCode = T4.CardCode ");
                var sql2 = $" select {filedName.ToString()} from {tableName.ToString()} where {filterString.ToString()} order by {sortString} limit {(page - 1) * limit}, {limit} ";
                clientTable = UnitWork.ExcuteSqlTable(ContextType.Nsap4ServeDbContextType, sql2, CommandType.Text);
                var sql3 = $" select count(*) from {tableName.ToString()} where {filterString.ToString()}; ";
                rowCount = int.Parse(UnitWork.ExcuteSqlTable(ContextType.Nsap4ServeDbContextType, sql3, CommandType.Text).Rows[0][0].ToString());

            }
            else
            {
                tableName.Append("(SELECT A.sbo_id,A.CardCode,A.CardName,B.SlpName,CONCAT(IFNULL(E.lastName,''),IFNULL(E.firstName,'')) as Technician,");
                tableName.Append("A.CntctPrsn,CONCAT(IFNULL(F.Name,''),IFNULL(G.Name,''),IFNULL(A.City,''),IFNULL(A.Building,'')) AS Address, ");
                tableName.Append("A.Phone1,A.Cellular, ");//,A.Balance,H.Balance AS BalanceTotal
                tableName.Append("A.DNotesBal,A.OrdersBal,A.OprCount,A.upd_dt AS UpdateDate,A.SlpCode,A.DfTcnician ");
                tableName.Append(",IFNULL(A.Balance,0) as Balance,0.00 as BalanceTotal ");
                tableName.Append(", case  when LOCATE(\"C\", Y.SubNo)  > 0  ||  LOCATE(\"C\", Y.ParentNo) > 0  then 1  ELSE 0 end as relationFlag ");
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
                tableName.AppendFormat("LEFT JOIN  (SELECT c.Id, c.SubNo ,c.ClientNo, c.ParentNo, c.IsActive, c.IsDelete, c.ScriptFlag,ROW_NUMBER() OVER (PARTITION BY ClientNo ORDER BY CreateDate DESC) rn from {0}.clientrelation c)   Y ON Y.ClientNo = A.CardCode  AND Y.IsActive =1 AND Y.ScriptFlag =0 AND   Y.rn = 1  AND  Y.IsDelete = 0   ", "erp4");
                tableName.AppendFormat("LEFT JOIN {0}.cluefollowup J ON J.ClueId=I.Id ORDER BY b.FollowUpTime DESC LIMIT 1 ", "nsap_serve");
                //tableName.AppendFormat("LEFT JOIN {0}.crm_balance_sum H ON H.CardCode=A.CardCode) T ", "nsap_bone");
                tableName.Append(") T");
                //tableName.AppendFormat("LEFT JOIN {0}.crm_clerk_tech I ON I.sbo_id=A.sbo_id AND I.CardCode=A.CardCode ", "nsap_bone");
                clientTable = _serviceSaleOrderApp.SelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), limit, page, sortString, filterString.ToString(), out rowCount);
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
        public string GetClientSboBalanceNew(string CardCode, string SboId)
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
                string strmySql = string.Format("SELECT Balance FROM {0}.crm_ocrd_oldsbo_balance WHERE sbo_id IN({1}) and CardCode IN ('{2}')", "nsap_bone", SboId, CardCode);
                DataTable sapbobj = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strmySql, CommandType.Text, null);
                return sapbobj.Rows.Count == 0 ? "" : sapbobj.Rows[0][0].ToString();
            }

        }
        public string GetClientSboBalance(string CardCode, string SboId)
        {
            var sapbobjs = "";
            bool sapflag = _serviceSaleOrderApp.GetSapSboIsOpen(SboId);
            if (sapflag)
            {


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
                //QryGroup1 = clientInfo.QryGroup1,// 属性 1
                //QryGroup2 = clientInfo.QryGroup2,// 属性 2
                //QryGroup3 = clientInfo.QryGroup3,// 属性 3
                //QryGroup4 = clientInfo.QryGroup4,// 属性 4
                //QryGroup6 = clientInfo.QryGroup6,// 属性 6
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
        public List<DataTable> SelectClientContactData(string CardCode, string SboId, bool IsOpenSap, bool IsViewFull, string Type)
        {
            List<DataTable> dtList = new List<DataTable>();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            //当前登录用户业务员编码
            int slpCode = UnitWork.Find<sbo_user>(q => q.user_id == loginUser.User_Id).Select(q => q.sale_id).FirstOrDefault().Value;
            //该客户归属业务员编码
            int slpCode_client = UnitWork.Find<crm_ocrd>(q => q.CardCode == CardCode).Select(q => q.SlpCode).FirstOrDefault().Value;
            int sboId = SboId.ToInt();
            //查出该客户的所有联系人信息
            var data = (from n in UnitWork.Find<crm_ocpr>(q => q.CardCode == CardCode)
                        select new
                        {
                            CardCode = n.CardCode,
                            CntctCode = n.CntctCode,
                            Active = n.Active,
                            IsDefault = 0,
                            Name = n.Name,
                            Gender = n.Gender,
                            Title = n.Title,
                            Position = n.Position,
                            Address = n.Address,
                            Notes1 = n.Notes1,
                            Notes2 = n.Notes2,
                            Tel1 = n.Tel1,
                            Tel2 = n.Tel2,
                            Cellolar = n.Cellolar,
                            Fax = n.Fax,
                            E_MailL = n.E_MailL,
                            U_ACCT = n.U_ACCT,
                            U_BANK = n.U_BANK,
                            flag = false
                        }).ToList();
            dtList.Add(data.ToDataTable());
            var limsocpr = UnitWork.Find<LimsOCPR>(q => q.CardCode == CardCode && q.SlpCode == slpCode && q.Type == Type).ToList();
            var limsocprdata = (from n in limsocpr
                                select new
                                {
                                    CardCode = n.CardCode,
                                    SlpCode = n.SlpCode,
                                    CntctCode = n.CntctCode,
                                    Active = n.Active,
                                    IsDefault = 0,
                                    Name = n.Name,
                                    Gender = n.Gender,
                                    Title = n.Title,
                                    Position = n.Position,
                                    Address = n.Address,
                                    Notes1 = n.Notes1,
                                    Notes2 = n.Notes2,
                                    Tel1 = n.Tel1,
                                    Tel2 = n.Tel2,
                                    Cellolar = n.Cellolar,
                                    Fax = n.Fax,
                                    E_MailL = n.E_MailL,
                                    U_ACCT = n.U_ACCT,
                                    U_BANK = n.U_BANK,
                                    flag = true
                                }).ToList();
            dtList.Add(limsocprdata.ToDataTable());
            return dtList;
        }
        #endregion
        #region 查询业务伙伴的地址
        /// <summary>
        /// 查询业务伙伴的地址
        /// </summary>
        /// <returns></returns>
        public List<DataTable> SelectClientAddrData(string CardCode, string SboId, bool IsOpenSap, string Type)
        {
            List<DataTable> dtList = new List<DataTable>();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            //当前登录用户业务员编码
            int slpCode = UnitWork.Find<sbo_user>(q => q.user_id == loginUser.User_Id).Select(q => q.sale_id).FirstOrDefault().Value;
            //该客户归属业务员编码
            int slpCode_client = UnitWork.Find<crm_ocrd>(q => q.CardCode == CardCode).Select(q => q.SlpCode).FirstOrDefault().Value;
            int sboId = SboId.ToInt();
            //查出该客户的所有地址信息
            var data = (from n in UnitWork.Find<crm_crd1>(q => q.CardCode == CardCode)
                        join o in UnitWork.Find<crm_ocry>(null) on n.Country equals o.Code into temp1
                        from t1 in temp1.DefaultIfEmpty()
                        join c in UnitWork.Find<crm_ocst>(null) on n.State equals c.Code into temp2
                        from t2 in temp2.DefaultIfEmpty()
                        select new
                        {
                            CardCode = n.CardCode,
                            LineNum = n.LineNum.Value,
                            Active = n.U_Active,
                            IsDefault = 0,
                            AdresType = n.AdresType,
                            Address = n.Address,
                            Country = t1 == null ? "" : t1.Name,
                            State = t2 == null ? "" : t2.Name,
                            City = n.City,
                            Building = n.Building,
                            ZipCode = n.ZipCode,
                            CountryId = n.Country,
                            StateId = n.State,
                            flag = false
                        }).ToList();
            dtList.Add(data.ToDataTable());
            var limsCrd1 = UnitWork.Find<LimsCRD1>(q => q.CardCode == CardCode && q.SlpCode == slpCode && q.Type == Type).ToList();
            var limsCrd1data = (from n in limsCrd1
                                join o in UnitWork.Find<crm_ocry>(null) on n.Country equals o.Code into temp1
                                from t1 in temp1.DefaultIfEmpty()
                                join c in UnitWork.Find<crm_ocst>(null) on n.State equals c.Code into temp2
                                from t2 in temp2.DefaultIfEmpty()
                                select new
                                {
                                    CardCode = n.CardCode,
                                    SlpCode = n.SlpCode,
                                    LineNum = n.LineNum,
                                    Active = n.U_Active.ToString(),
                                    IsDefault = 0,
                                    AdresType = n.AdresType,
                                    Address = n.Address,
                                    Country = t1 == null ? "" : t1.Name,
                                    State = t2 == null ? "" : t2.Name,
                                    City = n.City,
                                    Building = n.Building,
                                    ZipCode = n.ZipCode,
                                    CountryId = n.Country,
                                    StateId = n.State,
                                    flag = true
                                }).ToList();
            dtList.Add(limsCrd1data.ToDataTable());
            return dtList;
        }
        #endregion
        #region 查询业务伙伴机会
        /// <summary>
        /// 查询业务伙伴机会
        /// </summary>
        /// <returns></returns>
        public DataTable SelectClientClueData(string CardCode, string SboId)
        {
            DataTable dt = new DataTable();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            string sql = "select SerialNumber,c.CreateUser SlpName,c.CreateTime clueDate,od.CreateDate clientDate from clue c";
            sql += " join nsap_bone.crm_ocrd od on c.CardCode = od.CardCode and od.sbo_id = " + SboId + " where od.CardCode = '" + CardCode + "'";
            dt = UnitWork.ExcuteSqlTable(ContextType.Nsap4ServeDbContextType, sql, CommandType.Text);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string username = dt.Rows[i]["SlpName"].ToString();
                string dept = "";
                //销售员部门数据
                var deptData = (from s in UnitWork.Find<base_user>(q => q.user_nm == username)
                                join ud in UnitWork.Find<base_user_detail>(null) on s.user_id equals ud.user_id
                                join d in UnitWork.Find<base_dep>(null) on ud.dep_id equals d.dep_id
                                select new
                                {
                                    dept = d.dep_alias
                                }).Distinct().ToList();
                if (deptData.Count > 0)
                {
                    dept = deptData[0].dept;
                }
                dt.Rows[i]["SlpName"] = dept + "-" + username;
            }
            return dt;
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
        public async Task<string> UpdateClientJob(UpdateClientJobReq updateClientJobReq)
        {
            string result = "";
            var loginContext = _auth.GetCurrentUser();
            var loginUser = loginContext.User;
            var UserId = _serviceBaseApp.GetUserNaspId();
            clientOCRD OCRD = BulidClientJob(updateClientJobReq.clientInfo);
            //根据客户类型生成业务伙伴编码
            if (!string.IsNullOrWhiteSpace(OCRD.CardNameCore == null ? OCRD.CardNameCore : OCRD.CardNameCore.Trim())) { OCRD.U_Name = OCRD.CardNameCore; }
            string rJobNm = string.Format("{0}{1}", OCRD.ClientOperateType == "edit" ? "修改" : "添加", OCRD.CardType == "S" ? "供应商" : "业务伙伴");
            byte[] job_data = ByteExtension.ToSerialize(OCRD);
            if (updateClientJobReq.submitType == "Temporary")
            {
                var resultTFlag = UpdateAuditJob(updateClientJobReq.JobId, rJobNm, OCRD.CardName, OCRD.FreeText.FilterESC(), job_data, true);
                result = resultTFlag ? "1" : "0";
                bool updParaCardCode = UpdateWfaJobPara(updateClientJobReq.JobId, 1, OCRD.CardCode);
                bool updParaCardName = UpdateWfaJobPara(updateClientJobReq.JobId, 2, OCRD.CardName);
                bool updParaOperateType = UpdateWfaJobPara(updateClientJobReq.JobId, 3, OCRD.ClientOperateType);
                bool updParaAppChange = UpdateWfaJobPara(updateClientJobReq.JobId, 4, OCRD.IsApplicationChange);
                //更新草稿客户关系
                if (resultTFlag)
                {
                    await _clientRelationApp.SaveScriptRelations(new ClientRelation.Request.JobScriptReq
                    {
                        JobId = Convert.ToInt32(updateClientJobReq.JobId),
                        ClientNo = "",
                        Flag = OCRD.is_reseller == "Y" ? 1 : 0,
                        ClientName = OCRD.CardName,
                        EndCustomerName = updateClientJobReq.Terminals,
                        Operator = loginUser.Name,
                        Operatorid = loginUser.Id,
                        Initial = 1
                    });
                }

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
                    //更新草稿客户关系
                    await _clientRelationApp.SaveScriptRelations(new ClientRelation.Request.JobScriptReq
                    {
                        JobId = Convert.ToInt32(updateClientJobReq.JobId),
                        ClientNo = "",
                        Flag = OCRD.is_reseller == "Y" ? 1 : 0,
                        ClientName = OCRD.CardName,
                        EndCustomerName = updateClientJobReq.Terminals,
                        Operator = loginUser.Name,
                        Operatorid = loginUser.Id,
                        Initial = 1
                    });
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
                var resultTElag = UpdateAuditJob(updateClientJobReq.JobId, rJobNm, OCRD.CardName, OCRD.FreeText.FilterESC(), job_data, false);
                result = resultTElag ? "1" : "0";
                bool updParaCardCode = UpdateWfaJobPara(updateClientJobReq.JobId, 1, OCRD.CardCode);
                bool updParaCardName = UpdateWfaJobPara(updateClientJobReq.JobId, 2, OCRD.CardName);
                bool updParaOperateType = UpdateWfaJobPara(updateClientJobReq.JobId, 3, OCRD.ClientOperateType);
                bool updParaAppChange = UpdateWfaJobPara(updateClientJobReq.JobId, 4, OCRD.IsApplicationChange);
                //更新草稿客户关系
                if (resultTElag)
                {
                    await _clientRelationApp.SaveScriptRelations(new ClientRelation.Request.JobScriptReq
                    {
                        JobId = Convert.ToInt32(updateClientJobReq.JobId),
                        ClientNo = "",
                        Flag = OCRD.is_reseller == "Y" ? 1 : 0,
                        ClientName = OCRD.CardName,
                        EndCustomerName = updateClientJobReq.Terminals,
                        Operator = loginUser.Name,
                        Operatorid = loginUser.Id,
                        Initial = 1
                    });

                }
            }
            //更新草稿推广员地址联系人
            //当前登录者业务员编码
            int slpCode = UnitWork.Find<sbo_user>(q => q.user_id == loginUser.User_Id).Select(q => q.sale_id).FirstOrDefault().Value;
            var crd1 = updateClientJobReq.clientInfo.AddrList.Where(q => q.isLims == true && q.slpCode == slpCode).ToList();
            var ocpr = updateClientJobReq.clientInfo.ContactList.Where(q => q.isLims == true && q.slpCode == slpCode).ToList();

            //如果是lims推广员
            SavelimsData(crd1, ocpr, updateClientJobReq.clientInfo.CardCode, slpCode);
            return result;
        }
        #endregion

        #region 保存业务伙伴审核的录入方案
        /// <summary>
        /// 保存业务伙伴审核的录入方案
        /// </summary>
        public async Task<Infrastructure.Response> SaveCrmAuditInfo(string AuditType, string CardCode, string DfTcnician, string JobId)
        {
            Infrastructure.Response rsp = new Infrastructure.Response();
            clientOCRD client = new clientOCRD();
            client = _serviceSaleOrderApp.DeSerialize<clientOCRD>((byte[])GetAuditInfo(JobId));
            client.ChangeType = AuditType;
            client.ChangeCardCode = CardCode;
            var originClient = UnitWork.FindSingle<OpenAuth.Repository.Domain.ClientRelation >(a => a.ClientNo == CardCode && a.IsDelete == 0 && a.IsActive == 1 && a.Flag !=2 && a.ScriptFlag == 0);
            if (AuditType == "Edit")
            {
                client.DfTcnicianCode = DfTcnician;
                //20221007  若审批是终端时 1. 中间商变更为终端，不允许 2. 终端变更，更改业务员，原先关系不解绑
                var currentuser = _auth.GetCurrentUser();
                var jobraw = Convert.ToInt32(JobId);
                var job = UnitWork.FindSingle<wfa_job>(a => a.job_id == jobraw);

                var newOper = UnitWork.FindSingle<User>(a => a.User_Id == job.user_id);
                if (originClient !=null && client.is_reseller == "N")
                {
                    if (originClient.Flag == 1)
                    {
                        //add log to explain why
                        _logger.LogError("不允许中间商变更为终端客户,请求参数为 jobid:" + JobId + " CardCode: " + CardCode);
                        rsp.Message = "审核失败，不允许中间商变更为终端客户";
                        rsp.Code = 200;
                        return rsp;
                    }
                    else
                    {
                        await _clientRelationApp.ResignRelations(new ClientRelation.Request.ResignRelReq
                        {
                            userid = currentuser.User.Id.ToString(),
                            username = currentuser.User.Name,
                            job_userid = newOper.Id,
                            job_username = newOper.Name,
                            jobid = (int)job.job_id,
                            ClientNo = CardCode,
                            ClientName = originClient.ClientName,
                            flag = 0,
                            OperateType = 1
                        });

                    }
                }
                else
                {
                    //20221007  若审批是中间商时 1. 终端变更为中间商，更改业务员  2. 中间商变更，更改业务员，原先关系不解绑
                    if (originClient != null && client.is_reseller == "Y")
                    {
                        await _clientRelationApp.ResignRelations(new ClientRelation.Request.ResignRelReq
                        {
                            userid = currentuser.User.Id.ToString(),
                            username = currentuser.User.Name,
                            job_userid = newOper.Id,
                            job_username = newOper.Name,
                            jobid = (int)job.job_id,
                            ClientNo = CardCode,
                            ClientName = originClient.ClientName,
                            flag = 1,
                            OperateType = 0
                        });
                    }


                }
            }
            string rJobNm = string.Format("{0}{1}", client.ChangeType == "edit" ? "修改" : "添加", client.CardType == "S" ? "供应商" : "业务伙伴");
            byte[] job_data = ByteExtension.ToSerialize(client);
            var finalResult = UpdateAuditJob(JobId, rJobNm, client.FreeText.FilterESC(), job_data, false) ? "1" : "0";
            rsp.Message = finalResult;
            rsp.Code = 200;
            return rsp;
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

        /// <summary>
        /// 审核通过修改lims推广员数据状态
        /// </summary>
        /// <param name="CardCode"></param>
        /// <param name="Type"></param>
        public void AuditlimsInfo(string CardCode, string Type)
        {
            if (Type == "agree")
            {
                var limsCrd1 = UnitWork.Find<LimsCRD1>(q => q.CardCode == CardCode && q.sbo_id == Define.SBO_ID && q.Type == "Temporary").ToList();
                //数据处理
                foreach (var item in limsCrd1)
                {
                    item.Type = "Submit";
                    UnitWork.Update(item);
                }
                UnitWork.Delete<LimsCRD1>(q => q.CardCode == CardCode && q.sbo_id == Define.SBO_ID && q.Type == "Submit");
                var limsOcpr = UnitWork.Find<LimsOCPR>(q => q.CardCode == CardCode && q.sbo_id == Define.SBO_ID && q.Type == "Temporary").ToList();
                //数据处理
                foreach (var item in limsOcpr)
                {
                    item.Type = "Submit";
                    UnitWork.Update(item);
                }
                UnitWork.Delete<LimsOCPR>(q => q.CardCode == CardCode && q.sbo_id == Define.SBO_ID && q.Type == "Submit");
                UnitWork.Save();
            }
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
                strSql.Append("WHERE job_type_id=?JobType AND sbo_id=?SboId AND job_state=?JobState ");// AND user_id=?UserId
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
                    //new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?UserId",     UserId),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?JobState",     1),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?CardName",     CardName),

                };
                int result = Convert.ToInt32(UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, strPara).ToString());
                List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> strPara1 = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
                {
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?JobType",    JobType),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?SboId",     SboId),
                    //new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?UserId",     UserId),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?JobState",     0),
                    new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?CardName",     CardName),

                };
                int result1 = Convert.ToInt32(UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, strPara1).ToString());
                return result + result1 > 0 ? "true" : "false";
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
        public StringBuilder getOqutSqlStr(SelectOqutReq selectOqutReq)
        {
            StringBuilder strSql = new StringBuilder();
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
                strSql.AppendFormat("a.UpdateDate BETWEEN '{0}' AND '{1}' ", selectOqutReq.StartTime, selectOqutReq.EndTime);
            }
            return strSql;
        }
        /// <summary>
        /// 查询业务伙伴报价单
        /// </summary>
        public OrderRq SelectOqut(SelectOqutReq selectOqutReq)
        {
            OrderRq orderRq = new OrderRq();
            DataTable dt = new DataTable();
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT  A.DocEntry,B.SlpName,A.DocTotal, (A.DocTotal-A.PaidToDate)  AS OpenDocTotal,A.CreateDate,A.DocStatus,A.Printed  ");
            strSql.Append(getOqutSqlStr(selectOqutReq));
            strSql.AppendFormat("ORDER BY A.DocEntry DESC ");
            int count = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null).Rows.Count;

            strSql.AppendFormat(" offset " + (selectOqutReq.page - 1) * selectOqutReq.limit + " rows fetch next " + selectOqutReq.limit + " rows only ");
            dt = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string username = dt.Rows[i]["SlpName"].ToString();
                string dept = "";
                //销售员部门数据
                var deptData = (from s in UnitWork.Find<base_user>(q => q.user_nm == username)
                                join ud in UnitWork.Find<base_user_detail>(null) on s.user_id equals ud.user_id
                                join d in UnitWork.Find<base_dep>(null) on ud.dep_id equals d.dep_id
                                select new
                                {
                                    dept = d.dep_alias
                                }).Distinct().ToList();
                if (deptData.Count > 0)
                {
                    dept = deptData[0].dept;
                }
                dt.Rows[i]["SlpName"] = dept + "-" + username;
            }

            StringBuilder strSqlMoney = new StringBuilder();
            strSqlMoney.Append("SELECT Sum(A.DocTotal) DocTotal, Sum(A.DocTotal-A.PaidToDate)  AS OpenDocTotal ");
            strSqlMoney.Append(getOqutSqlStr(selectOqutReq));
            DataTable dtMoney = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSqlMoney.ToString(), CommandType.Text, null);

            decimal Total = 0;
            decimal OpenDocTotal = 0;
            if (dtMoney != null)
            {
                for (int i = 0; i < dtMoney.Rows.Count; i++)
                {
                    Total += dtMoney.Rows[i]["DocTotal"].ToDecimal();
                    OpenDocTotal += dtMoney.Rows[i]["OpenDocTotal"].ToDecimal();
                }
            }
            orderRq.Total = Total;
            orderRq.OpenDocTotal = OpenDocTotal;
            orderRq.dt = dt;
            orderRq.count = count;
            return orderRq;
        }

        #endregion
        #region 查询业务伙销售订单
        public StringBuilder getOrdrSqlStr(SelectOrdrReq selectOrdrReq)
        {
            StringBuilder strSql = new StringBuilder();
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

                strSql.AppendFormat("a.UpdateDate BETWEEN '{0}' AND '{1}' ", selectOrdrReq.StartTime, selectOrdrReq.EndTime);
            }
            return strSql;
        }
        /// <summary>
        /// 查询业务伙销售订单
        /// </summary>
        public OrderRq SelectOrdr(SelectOrdrReq selectOrdrReq)
        {
            OrderRq orderRq = new OrderRq();
            DataTable dt = new DataTable();
            StringBuilder strSql = new StringBuilder();
            strSql.Append(
                "SELECT  A.DocEntry,B.SlpName,A.DocTotal, (A.DocTotal-A.PaidToDate)  AS OpenDocTotal,A.CreateDate,A.DocStatus,A.Printed  ");
            strSql.Append(getOrdrSqlStr(selectOrdrReq));
            strSql.AppendFormat("ORDER BY A.DocEntry DESC");

            int count = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null).Rows.Count;
            strSql.AppendFormat(" offset " + (selectOrdrReq.page - 1) * selectOrdrReq.limit + " rows fetch next " + selectOrdrReq.limit + " rows only ");
            dt = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string username = dt.Rows[i]["SlpName"].ToString();
                string dept = "";
                //销售员部门数据
                var deptData = (from s in UnitWork.Find<base_user>(q => q.user_nm == username)
                                join ud in UnitWork.Find<base_user_detail>(null) on s.user_id equals ud.user_id
                                join d in UnitWork.Find<base_dep>(null) on ud.dep_id equals d.dep_id
                                select new
                                {
                                    dept = d.dep_alias
                                }).Distinct().ToList();
                if (deptData.Count > 0)
                {
                    dept = deptData[0].dept;
                }
                dt.Rows[i]["SlpName"] = dept + "-" + username;
            }
            StringBuilder strSqlMoney = new StringBuilder();
            strSqlMoney.Append("SELECT Sum(A.DocTotal) DocTotal, Sum(A.DocTotal-A.PaidToDate)  AS OpenDocTotal ");
            strSqlMoney.Append(getOrdrSqlStr(selectOrdrReq));
            DataTable dtMoney = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSqlMoney.ToString(), CommandType.Text, null);
            decimal Total = 0;
            decimal OpenDocTotal = 0;
            if (dtMoney != null)
            {
                for (int i = 0; i < dtMoney.Rows.Count; i++)
                {
                    Total += dtMoney.Rows[i]["DocTotal"].ToDecimal();
                    OpenDocTotal += dtMoney.Rows[i]["OpenDocTotal"].ToDecimal();
                }
            }
            orderRq.Total = Total;
            orderRq.OpenDocTotal = OpenDocTotal;
            orderRq.dt = dt;
            orderRq.count = count;
            return orderRq;
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
            var userInfo = _auth.GetCurrentUser();
            if (userInfo == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //判断是否是公海管理员
            var isCustomerSeaAdmin = userInfo.Roles.Any(r => r.Name == "公海管理员");
            //操作人名称
            var userName = userInfo?.User?.Name;

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
            if (req.Remark == "公司已注销" || req.Remark == "失信客户")
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
                catch (Exception ex)
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

                    string FuncID = _serviceSaleOrderApp.GetJobTypeByAddress("client/clientAssignSeller.aspx");

                    clientOCRD client = new clientOCRD();
                    client.CardCode = req.CardCode;
                    client.SlpCode = "1054";
                    client.SboId = "1";         //帐套
                    byte[] job_data = ByteExtension.ToSerialize(client);
                    string job_id = _serviceSaleOrderApp.WorkflowBuild("业务伙伴分配销售员", Convert.ToInt32(FuncID), userInfo.User.User_Id.Value, job_data, "业务伙伴分配销售员", 1, "", "", 0, 0, 0, "BOneAPI", "NSAP.B1Api.BOneOCRDAssign");
                    if (int.Parse(job_id) > 0)
                    {
                        string re = _serviceSaleOrderApp.WorkflowSubmit(int.Parse(job_id), userInfo.User.User_Id.Value, "业务伙伴分配销售员", "", 0);
                        //如果成功,则将客户从公海中移出(如果有的话)
                        if (re == "2")
                        {
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
                        }
                    }
                    // remove 4.0 relation if it exists
                    await _clientRelationApp.RejectJobRelations(req.CardCode);
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
            var slpInfo = UnitWork.Find<OSLP>(q => q.SlpCode == SlpCode).FirstOrDefault();
            var SlpName = slpInfo == null ? "" : slpInfo.SlpName;
            if (isAdd)
            {
                clientFollowUp.SlpCode = SlpCode;
                clientFollowUp.SlpName = SlpName;
                clientFollowUp.CreateUser = loginUser.Name;
                clientFollowUp.CreateDate = DateTime.Now;
                clientFollowUp.IsDelete = false;
                clientFollowUp.IsRemind = false;
                await UnitWork.AddAsync<ClientFollowUp, int>(clientFollowUp);
            }
            else
            {
                ClientFollowUp info = UnitWork.Find<ClientFollowUp>(q => q.Id == clientFollowUp.Id && !q.IsDelete).FirstOrDefault();
                info.CardCode = clientFollowUp.CardCode;
                info.CardName = clientFollowUp.CardName;
                info.SlpCode = SlpCode;
                info.SlpName = SlpName;
                info.SlpName = clientFollowUp.SlpName;
                info.Contacts = clientFollowUp.Contacts;
                info.FollowType = clientFollowUp.FollowType;
                info.NextTime = clientFollowUp.NextTime;// NextFollowTime;
                info.Remark = clientFollowUp.Remark;
                info.FileId = clientFollowUp.FileId;
                info.ImgId = clientFollowUp.ImgId;
                info.ImgName = clientFollowUp.ImgName;
                info.FileName = clientFollowUp.FileName;
                info.UpdateUser = loginUser.Name;
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
            var query = await (UnitWork.Find<ClientFollowUp>(c => c.NextTime >= startTime && c.NextTime <= endTime && c.IsRemind == false && c.IsDelete == false).Select(g => new
            {
                SlpName = g.SlpName,
                CardName = g.CardName,
                Id = g.Id
            })).ToListAsync();
            //查看有哪些业务员要发送提醒
            foreach (var slp in query)
            {
                await _hubContext.Clients.User(slp.SlpName).SendAsync("ReceiveMessage", "系统", $"您有1个客户待跟进，客户名称：" + slp.CardName);
                int id = slp.Id;
                ClientFollowUp item = UnitWork.Find<ClientFollowUp>(q => q.Id == id).FirstOrDefault();
                item.IsRemind = true;
                UnitWork.Update<ClientFollowUp>(item);
            }

            var querySchedule = await (UnitWork.Find<ClientSchedule>(c => c.RemindTime >= startTime && c.RemindTime <= endTime && c.IsRemind == false && c.IsDelete == false).Select(g => new
            {
                SlpName = g.SlpName,
                Title = g.Title,
                Id = g.Id
            })).ToListAsync();
            //查看有哪些业务员要发送提醒
            foreach (var slp in querySchedule)
            {
                await _hubContext.Clients.User(slp.SlpName).SendAsync("ReceiveMessage", "系统", $"您有1个日程待跟进，日程标题：" + slp.Title);
                int id = slp.Id;
                ClientSchedule item = UnitWork.Find<ClientSchedule>(q => q.Id == id).FirstOrDefault();
                item.IsRemind = true;
                UnitWork.Update<ClientSchedule>(item);
            }
            UnitWork.Save();
        }
        #endregion
        #endregion

        #region LIMS推广员

        /// <summary>
        /// 获取所有节点状态
        /// </summary>
        /// <returns>返回节点状态信息</returns>
        public async Task<TableData> GetProductTypeList()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            var categorStatusList = await UnitWork.Find<Category>(u => u.TypeId.Equals("ProductType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
            result.Count = categorStatusList.Count();
            result.Data = categorStatusList;
            return result;
        }

        /// <summary>
        ///  推广员列表（产品推广员模块）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<TableData> QueryLIMSInfo(QueryLIMSInfoReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var result = new TableData();
            try
            {
                var map = UnitWork.Find<LimsInfoMap>(null);
                //销售员部门数据
                var deptData = await (from s in UnitWork.Find<sbo_user>(null)
                                      join ud in UnitWork.Find<base_user_detail>(null) on s.user_id equals ud.user_id
                                      join d in UnitWork.Find<base_dep>(null) on ud.dep_id equals d.dep_id
                                      where s.sbo_id == Define.SBO_ID
                                      select new
                                      {
                                          UserId = (int)s.user_id,
                                          dept = d.dep_alias,
                                      }).Distinct().ToListAsync();

                var objs = from n in await UnitWork.Find<LimsInfo>(q => q.Type == request.Type).ToListAsync()
                           join u in await UnitWork.Find<User>(null).ToListAsync() on n.UserId equals u.Id
                           join d in deptData on u.User_Id equals d.UserId
                           select new
                           {
                               Id = n.Id,
                               UserId = n.UserId,
                               Type = n.Type,
                               Count = n.Count,
                               u.Name,
                               d.dept,
                               u.Status,
                               CreateUser = n.CreateUser,
                               CreateDate = n.CreateDate,
                           };
                if (request.IsClientDetail)
                {
                    //已绑定客户的推广员
                    var userlist = UnitWork.Find<LimsInfoMap>(q => q.CardCode == request.CardCode).Select(q => q.LimsInfoId).ToList();
                    objs = objs.Where(q => !userlist.Contains(q.Id));
                }
                var LimsInfoList = objs.OrderByDescending(r => r.CreateDate).Skip((request.page - 1) * request.limit).Take(request.limit).ToList();

                var LimsInfoListResp = LimsInfoList.Select(r => new
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    Type = r.Type,
                    Name = r.Name,
                    dept = r.dept,
                    Status = r.Status,
                    Count = (map.Where(q => q.LimsInfoId == r.Id).Select(x => new
                    {
                        x.Id,
                        x.CardCode,
                        x.CardName
                    })).Count(),
                    CreateUser = r.CreateUser,
                    CreateDate = r.CreateDate,
                    limsInfoMapList = map.Where(q => q.LimsInfoId == r.Id).Select(x => new
                    {
                        x.Id,
                        x.CardCode,
                        x.CardName
                    })
                }).ToList();

                result.Count = objs.Count();
                result.Data = LimsInfoListResp;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message.ToString();
                result.Code = 500;
            }
            return result;
        }

        /// <summary>
        /// 通过推广员获取已绑定的客户列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetCardCodeById(QueryClientInfoReq req)
        {
            var result = new TableData();
            int id = req.id;
            var CardCodeList = UnitWork.Find<LimsInfoMap>(q => q.LimsInfoId == id).Select(q => q.CardCode).ToList();

            var query = from c in UnitWork.Find<OCRD>(q => CardCodeList.Contains(q.CardCode))
                        join s in UnitWork.Find<OSLP>(null)
                        on c.SlpCode equals s.SlpCode
                        select new
                        {
                            c.CardCode,
                            c.CardName,
                            s.SlpCode,
                            s.SlpName
                        };
            if (!string.IsNullOrWhiteSpace(req.key))
            {
                query = query.Where(q => q.CardCode.Contains(req.key) || q.CardName.Contains(req.key) || q.SlpName.Contains(req.key));
            }
            //先把数据加载到内存
            var data = await query.OrderBy(q => q.CardCode).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            //销售员部门数据
            var deptData = await (from s in UnitWork.Find<sbo_user>(null)
                                  join ud in UnitWork.Find<base_user_detail>(null) on s.user_id equals ud.user_id
                                  join d in UnitWork.Find<base_dep>(null) on ud.dep_id equals d.dep_id
                                  where s.sbo_id == Define.SBO_ID
                                  && data.Select(x => x.SlpCode).Contains(s.sale_id.Value)
                                  select new
                                  {
                                      slpCode = s.sale_id,
                                      dept = d.dep_alias,
                                  }).Distinct().ToListAsync();
            var response = from q in data
                           join d in deptData on q.SlpCode equals d.slpCode into temp1
                           from t1 in temp1.DefaultIfEmpty()
                           select new
                           {
                               CardCode = q.CardCode,
                               CardName = q.CardName,
                               SlpCode = q.SlpCode,
                               SlpName = q.SlpName,
                               DeptName = t1 == null ? null : t1.dept
                           };
            result.Data = response;
            result.Count = await query.CountAsync();

            return result;
        }

        /// <summary>
        /// 添加lims推广员
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> AddLims(AddLIMSInfo req)
        {
            var result = new Infrastructure.Response();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            List<LimsInfo> LimsInfoList = new List<LimsInfo>();
            foreach (var item in req.userIdList)
            {
                //获取业务员编码
                int User_Id = UnitWork.Find<User>(q => q.Id == item).Select(q => q.User_Id).FirstOrDefault().Value;
                int slpCode = UnitWork.Find<sbo_user>(q => q.user_id == User_Id).Select(q => q.sale_id).FirstOrDefault().Value;
                LimsInfoList.Add(new LimsInfo
                {
                    UserId = item,
                    SlpCode = slpCode,
                    Type = req.Type,
                    Count = req.Count,
                    CreateUser = loginUser.Name,
                    CreateDate = DateTime.Now
                });
            }
            await UnitWork.BatchAddAsync<LimsInfo, int>(LimsInfoList.ToArray());
            await UnitWork.SaveAsync();

            return result;
        }


        /// <summary>
        /// 绑定客户与推广员
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> AddLimsMap(AddLIMSInfoMap req)
        {
            var result = new Infrastructure.Response();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            List<LimsInfoMap> list = new List<LimsInfoMap>();
            for (int i = 0; i < req.LimsIdList.Count; i++)
            {
                int LimsInfoId = req.LimsIdList[i].ToInt();
                list.Add(new LimsInfoMap
                {
                    CardCode = req.CardCode,
                    CardName = req.CardName,
                    LimsInfoId = LimsInfoId,
                    CreateUser = loginUser.Name,
                    CreateDate = DateTime.Now
                });
            }
            await UnitWork.BatchAddAsync<LimsInfoMap, int>(list.ToArray());
            await UnitWork.SaveAsync();

            return result;
        }

        /// <summary>
        /// 通过客户编码获取推广员信息
        /// </summary>
        /// <param name="CardCode"></param>
        /// <returns></returns>
        public async Task<TableData> GetLIMSByCode(string CardCode, string Type, int page, int limit)
        {
            var tableData = new TableData();
            var Mapdata = (from n in await UnitWork.Find<LimsInfoMap>(q => q.CardCode == CardCode).ToListAsync()
                           join m in await UnitWork.Find<LimsInfo>(q => q.Type == Type).ToListAsync() on new { LimsInfoId = n.LimsInfoId.ToString() } equals new { LimsInfoId = m.Id.ToString() }
                           join u in await UnitWork.Find<User>(null).ToListAsync() on m.UserId equals u.Id
                           select new
                           {
                               n.Id,
                               m.Type,
                               u.User_Id,
                               u.Name,
                               n.CreateUser,
                               n.CreateDate
                           }).ToList();//on new { User_Id = n.User_Id.ToInt() } equals new { User_Id = ud.user_id.ToInt() }
            var userDept = (from ud in UnitWork.Find<base_user_detail>(null)
                            join d in UnitWork.Find<base_dep>(null) on ud.dep_id equals d.dep_id
                            select new
                            {
                                ud.user_id,
                                d.dep_nm
                            }).ToList();
            var data = (from n in Mapdata
                        join ud in userDept on new { User_Id = n.User_Id.ToInt() } equals new { User_Id = ud.user_id.ToInt() }
                        select new
                        {
                            Id = n.Id,
                            Type = n.Type,
                            Name = n.Name,
                            dep_nm = ud.dep_nm,
                            CreateUser = n.CreateUser,
                            CreateDate = n.CreateDate
                        }).ToList();
            var dataquery = data.OrderByDescending(q => q.CreateDate).Skip((page - 1) * limit).Take(limit).ToList();
            tableData.Data = dataquery;
            tableData.Count = data.Count();
            return tableData;
        }

        /// <summary>
        /// 在客户详情页删除推广员
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="CardCode"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<bool> DeleteLIMS(DeleteLIMSInfoMap req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            foreach (var item in req.LimsIdList)
            {
                await UnitWork.DeleteAsync<LimsInfoMap>(q => q.Id == item && q.CardCode == req.CardCode);
            }
            await UnitWork.SaveAsync();

            return true;
        }

        /// <summary>
        /// 在推广员模块删除推广员
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<bool> DeleteLIMSInfo(List<int> Ids)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            foreach (var item in Ids)
            {
                await UnitWork.DeleteAsync<LimsInfoMap>(q => q.LimsInfoId == item);
                await UnitWork.DeleteAsync<LimsInfo>(q => q.Id == item);
            }
            await UnitWork.SaveAsync();
            return true;
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetUserInfo(QueryLIMSInfoReq req)
        {
            var tableData = new TableData();
            //已设置为推广员的用户
            var userlist = UnitWork.Find<LimsInfo>(q => q.Type == req.Type).Select(q => q.UserId).ToList();
            //销售员部门数据
            var deptData = await (from s in UnitWork.Find<sbo_user>(q => q.sbo_id == Define.SBO_ID && q.sale_id != 0)
                                  join ud in UnitWork.Find<base_user_detail>(null) on s.user_id equals ud.user_id
                                  join d in UnitWork.Find<base_dep>(null) on ud.dep_id equals d.dep_id
                                  where s.sbo_id == Define.SBO_ID
                                  select new
                                  {
                                      UserId = (int)s.user_id,
                                      dept = d.dep_alias,
                                  }).Distinct().ToListAsync();
            var data = from n in await UnitWork.Find<User>(q => !userlist.Contains(q.Id)).ToListAsync()
                       join m in deptData on n.User_Id equals m.UserId
                       select new
                       {
                           n.Id,
                           n.Account,
                           n.Name,
                           n.Status,
                           m.dept,
                           n.CreateTime
                       };
            if (!string.IsNullOrWhiteSpace(req.key))
            {
                data = data.Where(q => q.Name.Contains(req.key) || q.dept.Contains(req.key));
            }
            var dataquery = data.OrderByDescending(q => q.CreateTime).Skip((req.page - 1) * req.limit).Take(req.limit).ToList();
            tableData.Data = dataquery;
            tableData.Count = data.Count();
            return tableData;
        }

        /// <summary>
        /// 获取登录用户的slpcode
        /// </summary>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<Infrastructure.Response> GetSlpCode()
        {
            var result = new Infrastructure.Response();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            int userId = 0;
            if (loginContext.User.User_Id != null)
            {
                userId = loginContext.User.User_Id.Value;
            }
            else
            {
                result.Code = 500;
                result.Message = "业务员账号未绑定4.0，请联系管理员";
                return result;
            }
            var sbouser = await UnitWork.Find<sbo_user>(q => q.user_id == userId).FirstOrDefaultAsync();
            if (sbouser != null && sbouser.sale_id != null)
            {
                result.Message = sbouser.sale_id.Value.ToString();
            }
            else
            {
                result.Code = 500;
                result.Message = "业务员账号未绑定3.0，请联系管理员";
                return result;
            }
            return result;
        }

        /// <summary>
        /// 保存lims推广员的联系人地址
        /// </summary>
        /// <param name="crd1"></param>
        /// <param name="ocpr"></param>
        /// <param name="CardCode"></param>
        /// <param name="slpCode"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public bool SavelimsData(List<clientCRD1Req> crd1, List<clientOCPRReq> ocpr, string CardCode, int slpCode)
        {
            if (crd1.Count > 0)
            {
                List<LimsCRD1> crd1List = new List<LimsCRD1>();
                foreach (var c in crd1)
                {
                    crd1List.Add(new LimsCRD1
                    {
                        sbo_id = Define.SBO_ID,
                        Address = c.Address,
                        CardCode = CardCode,
                        SlpCode = slpCode,
                        ZipCode = c.ZipCode,
                        City = c.City,
                        County = c.County,
                        Country = c.Country,
                        State = c.State,
                        LogInstanc = !string.IsNullOrWhiteSpace(c.LogInstanc) ? Convert.ToInt16(c.LogInstanc) : 0,
                        ObjType = c.ObjType,
                        LicTradNum = c.LicTradNum,
                        LineNum = !string.IsNullOrWhiteSpace(c.LineNum) ? Convert.ToInt32(c.LineNum) : 0,
                        TaxCode = c.TaxCode,
                        Building = c.Building,
                        AdresType = c.AdresType,
                        Address2 = c.Address2,
                        Address3 = c.Address3,
                        U_Active = c.Active,
                        Type = "Temporary"
                    });
                }
                UnitWork.BatchAdd<LimsCRD1, int>(crd1List.ToArray());
                UnitWork.Delete<LimsCRD1>(q => q.CardCode == CardCode && q.sbo_id == Define.SBO_ID && q.Type == "Temporary");
            }

            if (ocpr.Count > 0)
            {
                List<LimsOCPR> ocprList = new List<LimsOCPR>();
                foreach (var c in ocpr)
                {
                    ocprList.Add(new LimsOCPR
                    {
                        CntctCode = !string.IsNullOrWhiteSpace(c.CntctCode) ? Convert.ToInt32(c.CntctCode) : 0,
                        sbo_id = Define.SBO_ID,
                        CardCode = CardCode,
                        SlpCode = slpCode,
                        Name = c.Name,
                        Position = c.Position,
                        Address = c.Address,
                        Tel1 = c.Tel1,
                        Tel2 = c.Tel2,
                        Cellolar = c.Cellolar,
                        Fax = c.Fax,
                        E_MailL = c.E_MailL,
                        Pager = c.Pager,
                        Notes1 = c.Notes1,
                        Notes2 = c.Notes2,
                        DataSource = c.DataSource,
                        UserSign = !string.IsNullOrWhiteSpace(c.UserSign) ? Convert.ToInt32(c.UserSign) : 0,
                        Password = c.Password,
                        LogInstanc = !string.IsNullOrWhiteSpace(c.LogInstanc) ? Convert.ToInt32(c.LogInstanc) : 0,
                        ObjType = c.ObjType,
                        BirthPlace = c.BirthPlace,
                        Gender = c.Gender,
                        Profession = c.Profession,
                        Title = c.Title,
                        BirthCity = c.BirthCity,
                        Active = c.Active,
                        FirstName = c.FirstName,
                        MiddleName = c.MiddleName,
                        LastName = c.LastName,
                        U_ACCT = c.U_ACCT,
                        U_BANK = c.U_BANK,
                        Type = "Temporary"
                    });
                }
                UnitWork.BatchAdd<LimsOCPR, int>(ocprList.ToArray());
                UnitWork.Delete<LimsOCPR>(q => q.CardCode == CardCode && q.sbo_id == Define.SBO_ID && q.Type == "Temporary");
            }
            UnitWork.Save();
            return true;
        }

        /// <summary>
        /// 通过cardcode获取slpcode
        /// </summary>
        /// <param name="CardCode"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public int GetSlpCodeByCardCode(string CardCode)
        {
            int SlpCode = UnitWork.Find<crm_ocrd>(q => q.CardCode == CardCode).Select(q => q.SlpCode).FirstOrDefault().Value;
            return SlpCode;
        }

        /// <summary>
        /// 判断登录用户是否是推广员身份
        /// </summary>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<Infrastructure.Response> isLims()
        {
            int slpCode = 0;
            var result = new Infrastructure.Response();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            int userId = 0;
            if (loginContext.User.User_Id != null)
            {
                userId = loginContext.User.User_Id.Value;
            }
            else
            {
                result.Code = 500;
                result.Message = "业务员账号未绑定4.0，请联系管理员";
                return result;
            }
            var sbouser = await UnitWork.Find<sbo_user>(q => q.user_id == userId).FirstOrDefaultAsync();
            if (sbouser != null && sbouser.sale_id != null)
            {
                result.Message = sbouser.sale_id.Value.ToString();
            }
            else
            {
                result.Code = 500;
                result.Message = "业务员账号未绑定3.0，请联系管理员";
                return result;
            }
            var limsList = UnitWork.Find<LimsInfo>(q => q.SlpCode == slpCode).ToList();
            result.Message = limsList.Count > 0 ? "true" : "false";
            return result;
        }
        #endregion
    }
}

