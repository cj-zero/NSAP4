using Infrastructure.Extensions;
using NSAP.Entity.Sales;
using OpenAuth.App.Order.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.NsapBone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenAuth.App.Order
{
    public partial class ServiceSaleOrderApp
    {
        #region 商城订单审核流程
        /// <summary>
        /// 添加订单流程
        /// </summary>
        /// <returns></returns>
        public string Eshop_AddOrderStatusFlow(WfaEshopStatus addWfaEshopStatusDto)
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
                foreach (var item in addWfaEshopStatusDto.Items)
                {
                    wfa_eshop_oqutdetail wfaEshopOqutdetail = new wfa_eshop_oqutdetail()
                    {
                        item_code = item.ItemCode,
                        item_desc = item.ItemDesc,
                        item_qty = item.ItemQty
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
        /// <returns></returns>
        public string Eshop_UpdateOrderStatusFlow(WfaEshopStatus wfaEshopStatusDto)
        {
            try
            {
                wfa_eshop_status wfa_Eshop_Status = UnitWork.FindSingle<wfa_eshop_status>(zw => zw.document_id == wfaEshopStatusDto.DocumentId);
                UnitWork.Update<wfa_eshop_status>(wfa_Eshop_Status);
                UnitWork.Delete<wfa_eshop_oqutdetail>(zw => zw.document_id == wfaEshopStatusDto.DocumentId);
                List<wfa_eshop_oqutdetail> wfaEshopOqutdetails = new List<wfa_eshop_oqutdetail>();
                foreach (var item in wfaEshopStatusDto.Items)
                {
                    wfa_eshop_oqutdetail wfaEshopOqutdetail = new wfa_eshop_oqutdetail()
                    {
                        document_id = wfaEshopStatusDto.DocumentId,
                        item_code = item.ItemCode,
                        item_desc = item.ItemDesc,
                        item_qty = item.ItemQty
                    };
                    UnitWork.Add<wfa_eshop_oqutdetail, int>(wfaEshopOqutdetail);
                }
                UnitWork.Save();
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
        /// <returns></returns>
        public string Eshop_OrderStatusFlow(WfaEshopStatus wfaEshopStatus, int oldorderno)
        {
            wfa_eshop_status WfaEshopStatus = UnitWork.FindSingle<wfa_eshop_status>(zw => zw.job_id == wfaEshopStatus.JobId);
            if (WfaEshopStatus.document_id > 0)
            {
                return Eshop_UpdateOrderStatusFlow(wfaEshopStatus);
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
                        return Eshop_UpdateOrderStatusFlow(wfaEshopStatus);
                    }
                }
                return Eshop_AddOrderStatusFlow(wfaEshopStatus);
            }
        }
        #endregion

        #region 根据销售报价单下销售订单
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
            billDelivery billDelivery = BulidBillDelivery(orderReq.Order);
            byte[] job_data = ByteExtension.ToSerialize(billDelivery);
            #region 售后人员(部门名称“售后”开头）下的销售订单如果没有设备（物料编号C开头),则审批流程改成呼叫中心审批
            bool shslp = false; bool shc = false;
            //判断销售员是否是售后部门
            if (!string.IsNullOrEmpty(orderReq.Order.SlpCode.ToString()))
            {
                string depnm = _serviceBaseApp.GetSalesDepname(orderReq.Order.SlpCode.ToString(), sboID.ToString());
                if (depnm.IndexOf("售后") == 0)
                {
                    shslp = true;
                }
            }
            //判断销售明细里面物料是否存在设备
            foreach (OrderItem orderDetails in orderReq.Order.OrderItems)
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
            int basetype = int.Parse(orderReq.Order.BillBaseType);
            if (!string.IsNullOrEmpty(orderReq.Order.U_EshopNo))
            {
                basetype = -2;//商城订单统一基本类别 方便审批列表标识出来
            }
            if (orderReq.Ations == OrderAtion.Draft)
            {
                result = OrderWorkflowBuild(jobname, funcId, userID, job_data, orderReq.Order.Remark, sboID, orderReq.Order.CardCode, orderReq.Order.CardName, (double.Parse(orderReq.Order.DocTotal.ToString()) > 0 ? double.Parse(orderReq.Order.DocTotal.ToString()) : 0), int.Parse(orderReq.Order.BillBaseType), int.Parse(orderReq.Order.BillBaseEntry), "BOneAPI", className);
            }
            if (orderReq.Ations == OrderAtion.Submit)
            {
                result = OrderWorkflowBuild(jobname, funcId, userID, job_data, orderReq.Order.Remark, sboID, orderReq.Order.CardCode, orderReq.Order.CardName, (double.Parse(orderReq.Order.DocTotal.ToString()) > 0 ? double.Parse(orderReq.Order.DocTotal.ToString()) : 0), basetype, int.Parse(orderReq.Order.BillBaseEntry), "BOneAPI", className);
                if (int.Parse(result) > 0)
                {
                    var par = SaveJobPara(result, orderReq.IsTemplate);
                    if (par)
                    {
                        string _jobID = result;
                        if ("0" != WorkflowSubmit(int.Parse(result), userID, orderReq.Order.Remark, "", 0))
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
                result = WorkflowSubmit(orderReq.JobId, userID, orderReq.Order.Remark, "", 0);
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
                UnitWork.Delete<store_drawing_job>(wfaEshopStatus);
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


    }
}
