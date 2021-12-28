extern alias MySqlConnectorAlias;

using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NSAP.Entity.Sales;
using OpenAuth.App.Order.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.NsapBone;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Order
{
    public partial class ServiceSaleOrderApp
    {
        #region 商城订单审核流程
        /// <summary>
        /// 添加订单流程
        /// </summary>
        /// <returns></returns>
        public string Eshop_AddOrderStatusFlow(WfaEshopStatus addWfaEshopStatusDto, IList<NSAP.Entity.Sales.billSalesDetails> itemdetails)
        {
            try
            {
                List<wfa_eshop_oqutdetail> wfaEshopOqutdetails = new List<wfa_eshop_oqutdetail>();
                wfa_eshop_status wfaEshopStatus = new wfa_eshop_status()
                {
                    job_id = addWfaEshopStatusDto.JobId,
                    user_id = addWfaEshopStatusDto.UserId,
                    slp_code = addWfaEshopStatusDto.SlpCode,
                    quotation_entry = addWfaEshopStatusDto.JobId,
                    order_entry = addWfaEshopStatusDto.JobId,
                    card_code = addWfaEshopStatusDto.CardCode,
                    card_name = addWfaEshopStatusDto.CardName,
                    complete_phase = addWfaEshopStatusDto.CompletePhase,
                    cur_status = addWfaEshopStatusDto.CurStatus,
                    order_phase = addWfaEshopStatusDto.OrderPhase,
                    first_createdate = addWfaEshopStatusDto.FirstCreateDate,
                    order_lastdate = addWfaEshopStatusDto.FirstCreateDate,
                    shipping_lastdate = addWfaEshopStatusDto.FirstCreateDate,
                    complete_lastdate = addWfaEshopStatusDto.CompleteLastDate,
                };
                foreach (var item in itemdetails)
                {
                    wfa_eshop_oqutdetail wfaEshopOqutdetail = new wfa_eshop_oqutdetail()
                    {
                        item_code = item.ItemCode,
                        item_desc = item.Dscription,
                        item_qty = decimal.Parse(item.Quantity)
                    };
                    wfaEshopOqutdetails.Add(wfaEshopOqutdetail);
                }
                wfaEshopStatus.wfa_eshop_oqutdetails = wfaEshopOqutdetails;
                UnitWork.Add<wfa_eshop_status, int>(wfaEshopStatus);
                UnitWork.Save();
                return "1";
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// 更新订单流程
        /// </summary>
        /// <param name="wfaEshopStatusDto"></param>
        /// <param name="itemdetails"></param>
        /// <returns></returns>
        public string Eshop_UpdateOrderStatusFlow(WfaEshopStatus wfaEshopStatusDto, IList<NSAP.Entity.Sales.billSalesDetails> itemdetails)
        {
            try
            {
                wfa_eshop_status wfa_Eshop_Status = UnitWork.FindSingle<wfa_eshop_status>(zw => zw.document_id == wfaEshopStatusDto.DocumentId);
                if (wfa_Eshop_Status != null)
                {
                    UnitWork.Update<wfa_eshop_status>(wfa_Eshop_Status);
                    UnitWork.Delete<wfa_eshop_oqutdetail>(zw => zw.document_id == wfaEshopStatusDto.DocumentId);
                    List<wfa_eshop_oqutdetail> wfaEshopOqutdetails = new List<wfa_eshop_oqutdetail>();
                    foreach (var item in itemdetails)
                    {
                        wfa_eshop_oqutdetail wfaEshopOqutdetail = new wfa_eshop_oqutdetail()
                        {
                            document_id = wfa_Eshop_Status.document_id,
                            item_code = item.ItemCode,
                            item_desc = item.Dscription,
                            item_qty = decimal.Parse(item.Quantity)
                        };
                        UnitWork.Add<wfa_eshop_oqutdetail, int>(wfaEshopOqutdetail);
                    }
                    UnitWork.Save();
                }
                return "1";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///取消订单流程
        /// </summary>
        /// <param name="old_orderno">old order no </param>
        /// <param name="document_id">out exists document id</param>
        public void Eshop_CancelOrderStatusFlow(int old_orderno, out int document_id)
        {
            wfa_eshop_status wfaEshopStatus = UnitWork.FindSingle<wfa_eshop_status>(zw => zw.order_entry == old_orderno);
            if (wfaEshopStatus != null)
            {
                document_id = wfaEshopStatus.document_id;
                if (wfaEshopStatus.document_id > 0)
                {
                    wfa_eshop_canceledstatus wfaEshopCanceledstatus = new wfa_eshop_canceledstatus()
                    {
                        document_id = wfaEshopStatus.document_id,
                        Line_idx = 0,
                        job_id = wfaEshopStatus.job_id,
                        user_id = wfaEshopStatus.job_id,
                        slp_code = wfaEshopStatus.job_id,
                        cur_status = wfaEshopStatus.job_id,
                        quotation_entry = wfaEshopStatus.job_id,
                        order_entry = wfaEshopStatus.job_id,
                        card_code = wfaEshopStatus.card_code,
                        card_name = wfaEshopStatus.card_name,
                        order_phase = wfaEshopStatus.order_phase,
                        shipping_phase = wfaEshopStatus.shipping_phase,
                        complete_phase = wfaEshopStatus.complete_phase,
                        first_createdate = wfaEshopStatus.first_createdate,
                        order_lastdate = wfaEshopStatus.order_lastdate,
                        shipping_lastdate = wfaEshopStatus.shipping_lastdate,
                        complete_lastdate = wfaEshopStatus.complete_lastdate,
                    };
                    UnitWork.Add<wfa_eshop_canceledstatus, int>(wfaEshopCanceledstatus);
                    UnitWork.Save();
                }
            }
            document_id = 0;
        }
        /// <summary>
        /// 商城流程
        /// </summary>
        /// <param name="wfaEshopStatus"></param>
        /// <param name="oldorderno"></param>
        /// <param name="itemdetails"></param>
        /// <returns></returns>
        public string Eshop_OrderStatusFlow(WfaEshopStatus wfaEshopStatus, IList<NSAP.Entity.Sales.billSalesDetails> itemdetails, int oldorderno)
        {
            wfa_eshop_status wfaEshop = UnitWork.FindSingle<wfa_eshop_status>(zw => zw.job_id == wfaEshopStatus.JobId);
            if (wfaEshop != null && wfaEshop.document_id > 0)
            {
                wfaEshopStatus.DocumentId = wfaEshop.document_id;
                return Eshop_UpdateOrderStatusFlow(wfaEshopStatus, itemdetails);
            }
            else
            {
                if (oldorderno > 0)
                {
                    int old_docId;
                    Eshop_CancelOrderStatusFlow(oldorderno, out old_docId);
                    if (old_docId > 0)
                    {
                        wfaEshopStatus.DocumentId = old_docId;
                        return Eshop_UpdateOrderStatusFlow(wfaEshopStatus, itemdetails);
                    }
                }
                return Eshop_AddOrderStatusFlow(wfaEshopStatus, itemdetails);
            }
        }
        #endregion

        #region 根据销售报价单下销售订单
        /// <summary>
        /// 根据销售报价单下销售订单
        /// </summary>
        /// <param name="orderReq"></param>
        /// <returns></returns>
        public string SalesOrderSave_ORDR(SalesOrderSaveReq orderReq)
        {

            int userID = _serviceBaseApp.GetUserNaspId();
            int sboID = _serviceBaseApp.GetUserNaspSboID(userID);
            int funcId = 33;
            string result = "";
            string className = "NSAP.B1Api.BOneORDR";
            string jobname = "销售订单";
            //查询
            billDelivery billDelivery = GetDeliverySalesInfoNewNos(orderReq.JobId.ToString(), 13);
            string josn = JsonConvert.SerializeObject(billDelivery);
            if (billDelivery is null)
            {
                result = "单据不存在";
                return result;
            }
            if (IsExistDoc(orderReq.JobId.ToString(), "23", sboID.ToString()))
            {
                result = "该销售报价单转销售订单已提交";
                return result;
            }
            billDelivery.billBaseEntry = orderReq.JobId.ToString();
            billDelivery.billBaseType = "23";
            billDelivery.Remark = !string.IsNullOrWhiteSpace(orderReq.Remark) ? orderReq.Remark : "";
            billDelivery.DocStatus = "O";
            int i = 0;
            foreach (var item in billDelivery.billSalesDetails)
            {
                item.BaseEntry = orderReq.JobId.ToString();
                item.BaseLine = i.ToString();
                item.BaseType = "23";
                i++;
            }
            if (orderReq.Comments == "")
            {
                billDelivery.Comments = "基于报价单" + orderReq.JobId.ToString();

            }
            else
            {
                billDelivery.Comments = orderReq.Comments + "基于报价单" + orderReq.JobId.ToString();

            }
            //billDelivery. = "销售订单";
            byte[] job_data = ByteExtension.ToSerialize(billDelivery);
            #region 售后人员(部门名称“售后”开头）下的销售订单如果没有设备（物料编号C开头),则审批流程改成呼叫中心审批
            bool shslp = false; bool shc = false;
            //判断销售员是否是售后部门
            if (!string.IsNullOrEmpty(billDelivery.SlpCode))
            {
                string depnm = _serviceBaseApp.GetSalesDepname(billDelivery.SlpCode, sboID.ToString());
                if (depnm.IndexOf("售后") == 0)
                {
                    shslp = true;
                }
            }
            //判断销售明细里面物料是否存在设备
            foreach (var orderDetails in billDelivery.billSalesDetails)
            {
                if (!string.IsNullOrEmpty(orderDetails.ItemCode))
                {
                    if (orderDetails.ItemCode.StartsWith("C", StringComparison.CurrentCultureIgnoreCase))
                    {
                        shc = true;
                        break;
                    }
                }
            }
            if (shslp && !shc)
            {
                jobname = "售后订单";
                funcId = _serviceBaseApp.GetFuncsByUserID("sales/SalesOrder_AfterSale.aspx", userID);
            }
            #endregion
            int basetype = int.Parse(billDelivery.billBaseType);
            if (!string.IsNullOrEmpty(billDelivery.U_EshopNo))
            {
                basetype = -2;//商城订单统一基本类别 方便审批列表标识出来
            }
            if (orderReq.Ations == OrderAtion.Draft)
            {
                result = OrderWorkflowBuild(jobname, funcId, userID, job_data, billDelivery.Remark, sboID, billDelivery.CardCode, billDelivery.CardName, (double.Parse(billDelivery.DocTotal.ToString()) > 0 ? double.Parse(billDelivery.DocTotal.ToString()) : 0), int.Parse(billDelivery.billBaseType), int.Parse(billDelivery.billBaseEntry), "BOneAPI", className);
            }
            if (orderReq.Ations == OrderAtion.Submit)
            {
                result = OrderWorkflowBuild(jobname, funcId, userID, job_data, billDelivery.Remark, sboID, billDelivery.CardCode, billDelivery.CardName, (double.Parse(billDelivery.DocTotal.ToString()) > 0 ? double.Parse(billDelivery.DocTotal.ToString()) : 0), basetype, int.Parse(billDelivery.billBaseEntry), "BOneAPI", className);
                if (int.Parse(result) > 0)
                {
                    var par = SaveJobPara(result, "");
                    if (par == "1")
                    {
                        string _jobID = result;
                        if ("0" != WorkflowSubmit(int.Parse(result), userID, billDelivery.Remark, "", 0))
                        {
                           SaveProOrder(billDelivery, int.Parse(_jobID)).ToString();
                            if (billDelivery.serialNumber.Count > 0)
                            {
                                UpdateSerialNumber(billDelivery.serialNumber, int.Parse(_jobID));
                            }
                        }
                        else { result = "0"; }
                    }
                    else { result = "0"; }
                }
            }
            if (orderReq.Ations == OrderAtion.Resubmit)
            {
                result = WorkflowSubmit(orderReq.JobId, userID, billDelivery.Remark, "", 0);
            }
            if (orderReq.Ations == OrderAtion.DraftUpdate)
            {
                result = UpdateAudit(orderReq.JobId, job_data, billDelivery.Remark, billDelivery.DocTotal, billDelivery.CardCode, billDelivery.CardName);

            }
            if (orderReq.Ations == OrderAtion.DrafSubmit)
            {
                result = UpdateAudit(orderReq.JobId, job_data, billDelivery.Remark, billDelivery.DocTotal, billDelivery.CardCode, billDelivery.CardName);
                if (result != null)
                {
                    var par = SaveJobPara(orderReq.JobId.ToString(), "");
                    if (par == "1")
                    {
                        string _jobID = orderReq.JobId.ToString();
                        if ("0" != WorkflowSubmit(orderReq.JobId, userID, billDelivery.Remark, "", 0))
                        {
                            result = SaveProOrder(billDelivery, int.Parse(_jobID)).ToString();
                            if (billDelivery.serialNumber.Count > 0)
                            {
                                if (UpdateSerialNumber(billDelivery.serialNumber, int.Parse(_jobID))) { result = "1"; }
                            }
                        }
                        else { result = "0"; }
                    }
                    else { result = "0"; }
                }
            }
            return result;
        }

        public string CloseDocFlow(int basetype, int docnum, int funcid, string jobname, int userID, string className, string sboId)
        {
            string result = "";
            billDelivery Model = new billDelivery();
            Model.DocNum = docnum.ToString();
            Model.SboId = sboId;
            byte[] job_data = ByteExtension.ToSerialize(Model);
            result = WorkflowBuild(jobname, funcid, userID, job_data, "", int.Parse(sboId), "", "", 0, basetype, docnum, "BOneAPI", className);
            if (int.Parse(result) > 0)
            {
                result = WorkflowSubmit(int.Parse(result), userID, "", "", 0);
            }
            return result;
        }

        public async Task<string> GetPurchaseItemByOrderNo(string orderNo, string sboId, string orderTitle)
        {
            string strsql = string.Format(@"SELECT t1.docentry,t1.itemcode,t1.U_RelDoc from {0}.buy_por1 t1
                                                 left join {0}.buy_opor t0 on t0.sbo_id=t1.sbo_id and t0.docentry=t1.docentry 
                                                 where t0.CANCELED='N' AND t1.sbo_id={1} and t1.U_RelDoc like '%{2}%'", "nsap_bone", sboId, orderTitle);
            DataTable dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strsql, CommandType.Text, null);
            if (dt.Rows.Count > 0)
            {
                string returnstr = string.Empty;
                foreach (DataRow tempr in dt.Rows)
                {
                    string[] temparr = tempr["U_RelDoc"].ToString().Split(';');
                    if (temparr.Length > 0)
                    {
                        foreach (string tempstr in temparr)
                        {
                            if (tempstr.Split(':')[0] == orderTitle)
                            {
                                string[] reldocarr = tempstr.Split(':')[1].Split(',');
                                foreach (string relstr in reldocarr)//找到关联订单号里面存在现编辑中订单号
                                {
                                    if (relstr == orderNo)
                                    {
                                        returnstr += (string.IsNullOrEmpty(returnstr) ? "" : ",") + tempr["docentry"].ToString() + "(" + tempr["itemcode"] + ")";
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                return returnstr;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 根据销售报价单下销售订单
        /// </summary>
        /// <param name="orderReq"></param>
        /// <returns></returns>
        public string SalesOrderSave_ORDR(AddOrderReq orderReq)
        {
            int userID = _serviceBaseApp.GetUserNaspId();
            int sboID = _serviceBaseApp.GetUserNaspSboID(userID);
            int funcId = 50;
            string result = "";
            string className = "NSAP.B1Api.BOneORDR";
            string jobname = "";
            //查询
            billDelivery billDelivery = BulidBillDelivery(orderReq.Order);
            byte[] job_data = ByteExtension.ToSerialize(billDelivery);
            #region 售后人员(部门名称“售后”开头）下的销售订单如果没有设备（物料编号C开头),则审批流程改成呼叫中心审批
            bool shslp = false; bool shc = false;
            //判断销售员是否是售后部门
            if (!string.IsNullOrEmpty(billDelivery.SlpCode))
            {
                string depnm = _serviceBaseApp.GetSalesDepname(billDelivery.SlpCode, sboID.ToString());
                if (depnm.IndexOf("售后") == 0)
                {
                    shslp = true;
                }
            }
            //判断销售明细里面物料是否存在设备
            foreach (var orderDetails in billDelivery.billSalesDetails)
            {
                if (!string.IsNullOrEmpty(orderDetails.ItemCode))
                {
                    if (orderDetails.ItemCode.StartsWith("C", StringComparison.CurrentCultureIgnoreCase))
                    {
                        shc = true;
                        break;
                    }
                }
            }
            if (shslp && !shc)
            {
                jobname = "售后订单";
                funcId = _serviceBaseApp.GetFuncsByUserID("sales/SalesOrder_AfterSale.aspx", userID);
            }
            #endregion
            int basetype = int.Parse(billDelivery.billBaseType);
            if (!string.IsNullOrEmpty(billDelivery.U_EshopNo))
            {
                basetype = -2;//商城订单统一基本类别 方便审批列表标识出来
            }
            if (orderReq.Ations == OrderAtion.Draft)
            {
                result = OrderWorkflowBuild(jobname, funcId, userID, job_data, billDelivery.Remark, sboID, billDelivery.CardCode, billDelivery.CardName, (double.Parse(billDelivery.DocTotal.ToString()) > 0 ? double.Parse(billDelivery.DocTotal.ToString()) : 0), int.Parse(billDelivery.billBaseType), int.Parse(billDelivery.billBaseEntry), "BOneAPI", className);
            }
            if (orderReq.Ations == OrderAtion.Submit)
            {
                result = OrderWorkflowBuild(jobname, funcId, userID, job_data, billDelivery.Remark, sboID, billDelivery.CardCode, billDelivery.CardName, (double.Parse(billDelivery.DocTotal.ToString()) > 0 ? double.Parse(billDelivery.DocTotal.ToString()) : 0), basetype, int.Parse(billDelivery.billBaseEntry), "BOneAPI", className);
                if (int.Parse(result) > 0)
                {
                    var par = SaveJobPara(result, "");
                    if (par == "1")
                    {
                        string _jobID = result;
                        if ("0" != WorkflowSubmit(int.Parse(result), userID, billDelivery.Remark, "", 0))
                        {
                            result = SaveProOrder(billDelivery, int.Parse(_jobID)).ToString();
                            if (billDelivery.serialNumber.Count > 0)
                            {
                                if (UpdateSerialNumber(billDelivery.serialNumber, int.Parse(_jobID))) { result = "1"; }
                            }
                        }
                        else { result = "0"; }
                    }
                    else { result = "0"; }
                }
            }
            if (orderReq.Ations == OrderAtion.Resubmit)
            {
                result = WorkflowSubmit(orderReq.JobId, userID, billDelivery.Remark, "", 0);
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
                bresult = WorkOrderJob(Model.SboId, jobid, "0", sfileCpbm, SaleOrder, sNum, Model.WhsCode, sTp) ? 1 : 0;
            }
            else
            {
                bresult = 1;
            }
            return bresult;
        }



        //修改已选择序列号状态
        public bool UpdateSerialNumber(IList<billSerialNumber> serialNumbers, int submitjobid)
        {
            try
            {
                foreach (billSerialNumber item in serialNumbers)
                {
                    List<store_osrn_alreadyexists> list = new List<store_osrn_alreadyexists>();
                    foreach (billSerialNumberChooseItem serial in item.Details)
                    {
                        store_osrn_alreadyexists storeOsrn = new store_osrn_alreadyexists();
                        storeOsrn.ItemCode = item.ItemCode;
                        storeOsrn.SysNumber = int.Parse(serial.SysSerial);
                        storeOsrn.DistNumber = serial.IntrSerial;
                        storeOsrn.MnfSerial = serial.SuppSerial;
                        storeOsrn.IsChange = "1";
                        storeOsrn.JobId = submitjobid;
                        UnitWork.Add<store_osrn_alreadyexists, string>(storeOsrn);
                    }
                }
                UnitWork.Save();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool WorkOrderJob(string sbo_id, int jobID, string proNum, string sfileCpbm, string jobidNew, string sNum, string WhsCode, string sTp)
        {
            try
            {
                store_drawing_job wfaEshopStatus = UnitWork.FindSingle<store_drawing_job>(zw => zw.job_idMe == jobID && zw.Typeid == 3 && zw.SalesId == int.Parse(jobidNew));
                if (wfaEshopStatus != null)
                {
                    UnitWork.Delete<store_drawing_job>(wfaEshopStatus);
                }
                string[] itemcode = sfileCpbm.Split(',');
                string[] projhsl = sNum.Split(',');
                string[] _sTp = sTp.Split(',');
                for (int i = 0; i < itemcode.Length; i++)
                {
                    store_drawing_job storeDrawingJob = new store_drawing_job()
                    {
                        sbo_id = int.Parse(sbo_id),
                        job_idMe = jobID,
                        productNum = int.Parse(proNum),
                        itemcode = itemcode[i].Replace("\"", "\\\"").Replace("\'", "\\\'"),
                        SalesId = int.Parse(jobidNew),
                        projhsl = projhsl[i],
                        WhsCode = WhsCode,
                        Typeid = 3,
                        TypeTP = int.Parse(_sTp[i]),
                        upd_date = DateTime.Now
                    };
                    UnitWork.Add<store_drawing_job, int>(storeDrawingJob);
                }
                UnitWork.Save();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 销售报价单审核流程
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobID"></param>
        /// <param name="userID"></param>
        /// <param name="recommend"></param>
        /// <param name="auditOpinionid"></param>
        /// <param name="IsUpdate"></param>
        /// <param name="vStock"></param>
        /// <returns></returns>
        public string AuditResubmitNextNew(int jobID, int userID, string recommend, string auditOpinionid, string IsUpdate, string vStock, string Comments, string Remark, string CustomFields, List<billSerialNumberChooseItem> ChoosedSerialNumberList, List<billSerialNumber> serialNumber)
        {
            string res = "";
            byte[] job_data = new byte[] { };
            billDelivery Model = DeSerialize<billDelivery>((byte[])(GetSalesInfo(jobID.ToString())));
            if (!string.IsNullOrWhiteSpace(Comments))
            {
                Model.Comments = Comments;

            }
            if (!string.IsNullOrWhiteSpace(Remark))
            {
                Model.Remark = Remark;
            }
            Model.DocStatus = "C";
            if (!string.IsNullOrWhiteSpace(CustomFields))
            {
                Model.CustomFields = CustomFields;
            }
            if (ChoosedSerialNumberList.Count > 0 && ChoosedSerialNumberList != null)
            {
                foreach (var item in Model.billSalesDetails)
                {
                    item.ChoosedSerialNumberList = ChoosedSerialNumberList;
                }
            }
            if (serialNumber.Count > 0 && serialNumber != null)
            {
                Model.serialNumber = serialNumber;
            }
            if (IsUpdate == "1")
            {
                job_data = ByteExtension.ToSerialize(Model);
            }
            //防止工作流多个人同时操作一个工作流，会跳过步骤的情况
            if (!_serviceBaseApp.IsValidSubmit(jobID, userID))
            {
                return "0";
            }
            if (auditOpinionid == "agree")
            {
                int v = 0;
                if (vStock == "1")
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("ItemCode", typeof(string));
                    dt.Columns.Add("Onhand", typeof(float));
                    dt.Columns.Add("WhsCode", typeof(string));
                    foreach (billSalesDetails details in Model.billSalesDetails)
                    {
                        DataRow row = dt.NewRow();
                        row["ItemCode"] = details.ItemCode;
                        row["Onhand"] = float.Parse(details.Quantity == "" ? "0" : details.Quantity);
                        row["WhsCode"] = details.WhsCode;
                        DataRow[] duplicaterow = dt.Select(string.Format("ItemCode='{0}' and WhsCode='{1}'", details.ItemCode.FilterSQL(), details.WhsCode));
                        if (duplicaterow.Length > 0)
                        {
                            duplicaterow[0]["Onhand"] = float.Parse(duplicaterow[0]["Onhand"].ToString()) + float.Parse(details.Quantity == "" ? "0" : details.Quantity);
                        }
                        else
                        {
                            dt.Rows.Add(row);
                        }
                    }
                    if (dt.Rows.Count > 0)
                    {

                        DataTable dtTable = GetItemOnhand(dt);
                        DataTable jTable = dt.Join(dtTable, new DataColumn[] { dt.Columns["ItemCode"], dt.Columns["WhsCode"] }, new DataColumn[] { dtTable.Columns["ItemCode"], dtTable.Columns["WhsCode"] });
                        DataRow[] vRows = jTable.Select("Onhand>ItemOnhand and InvntItem=1");
                        DataTable itemCodeList = new DataTable();
                        itemCodeList.Columns.Add("ItemCode", typeof(string));
                        itemCodeList.Columns.Add("isCountHas", typeof(float));
                        for (int i = 0; i < vRows.Length; i++)
                        {
                            v++;
                            DataRow dRow = itemCodeList.NewRow();
                            dRow[0] = vRows[i]["ItemCode"];
                            dRow[1] = vRows[i]["Onhand"];
                            itemCodeList.Rows.Add(dRow);
                        }
                        if (v > 0)
                        {
                            res = itemCodeList.DataTableToJSON();
                            string rJobTypeNm = _serviceBaseApp.GetJobTypeNm(jobID);
                            if (rJobTypeNm == "销售提成" || rJobTypeNm == "销售提成审核" || rJobTypeNm == "售后提成" || rJobTypeNm == "售后提成审核")
                            {
                                v = 0;
                            }
                        }
                    }
                }
                if (v == 0)
                {
                    string rJobTypeNm = _serviceBaseApp.GetJobTypeNm(jobID);
                    //if (rJobTypeNm == "采购收货" || rJobTypeNm == "采购收货M2")
                    //{
                    //    bool updParaWhsCode = NSAP.Data.Store.MaterialMasterData.UpdateWfaJobPara(jobID.ToString(), 1, Model.WhsCode);
                    //    int is_pass_num = 0;
                    //    for (int i = 0; i < Model.IQCDetails.Count; i++)
                    //    {
                    //        if (Model.IQCDetails[i].Inspect_dimension == "1" && Model.IQCDetails[i].Inspect_function == "1" && Model.IQCDetails[i].Inspect_appearance == "1" && Model.IQCDetails[i].Inspect_other == "1" && Model.IQCDetails[i].Inspect_result == "1")
                    //        {
                    //            is_pass_num++;

                    //        }

                    //    }
                    //    if (is_pass_num == Model.IQCDetails.Count)
                    //    {
                    //        NSAP.Data.Store.MaterialMasterData.UpdateWfaJobPara(jobID.ToString(), 2, "OK");
                    //    }
                    //    else
                    //    {
                    //        NSAP.Data.Store.MaterialMasterData.UpdateWfaJobPara(jobID.ToString(), 2, "NG");
                    //    }
                    //}
                    if (IsUpdate == "1")
                    {
                        if (bool.Parse(UpdateAuditA(jobID, job_data, Model.Remark, Model.DocTotal, Model.CardCode, Model.CardName)))
                        {
                            res = WorkflowSubmit(jobID, userID, recommend, "", 0);
                        }
                        else { res = "0"; }
                    }
                    else
                    {
                        res = WorkflowSubmit(jobID, userID, recommend, "", 0);
                    }
                }
            }
            else if (auditOpinionid == "reject")
            {
                if (IsUpdate == "1")
                {
                    if (bool.Parse(UpdateAudit(jobID, job_data, Model.Remark, Model.DocTotal, Model.CardCode, Model.CardName)))
                    {
                        res = WorkflowReject(jobID, userID, recommend, "", 0);
                        if (res == "1")
                        {
                            foreach (billSerialNumber osrn in Model.serialNumber)
                            {
                                foreach (billSerialNumberChooseItem serial in osrn.Details)
                                {
                                    if (DeleteSerialNumber(osrn.ItemCode.FilterSQL(), serial.SysSerial))
                                    {
                                        res = "1";
                                    }
                                    else
                                    {
                                        res = "0";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        res = "0";
                    }
                }
                else
                {
                    res = WorkflowReject(jobID, userID, recommend, "", 0);
                    if (res == "1")
                    {
                        foreach (billSerialNumber osrn in Model.serialNumber)
                        {
                            foreach (billSerialNumberChooseItem serial in osrn.Details)
                            {
                                if (DeleteSerialNumber(osrn.ItemCode.FilterSQL(), serial.SysSerial))
                                {
                                    res = "1";
                                }
                                else
                                {
                                    res = "0";
                                }
                            }
                        }
                    }
                }
                //撤销服务呼叫绑定
                if (Model.billBaseType == "-1" && Model.U_CallID != null && Model.U_CallID != "")
                {
                    UpdateUsftjbjFromOscl(Model.U_CallID, Model.SboId, "0");
                }
            }
            else if (auditOpinionid == "pending")
            {
                if (IsUpdate == "1")
                {
                    if (bool.Parse(UpdateAudit(jobID, job_data, Model.Remark, Model.DocTotal, Model.CardCode, Model.CardName)))
                    {
                        res = SavePanding(jobID, userID, recommend);
                    }
                    else { res = "0"; }
                }
                else
                {
                    res = SavePanding(jobID, userID, recommend);
                }
            }
            return res;
        }


        #endregion


        /// <summary>
        /// 修改审核数据（销售交货）
        /// </summary>
        public string UpdateAuditA(int jobId, byte[] jobData, string remarks, string doc_total, string card_code, string card_name)
        {
            string isSave = "";
            string strSql = string.Format("UPDATE {0}.wfa_job SET job_data=?job_data,remarks='{1}',doc_total={2},", "nsap_base", remarks, doc_total == "" ? "0" : doc_total);
            strSql += string.Format("card_code='{0}',card_name='{1}' WHERE job_id ={2}", card_code, card_name, jobId);
            List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> sqlParameters = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?job_data",  jobData),

            };
            isSave = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, sqlParameters).ToString();
            return isSave == "" ? "true" : "false";
        }
    }
}
