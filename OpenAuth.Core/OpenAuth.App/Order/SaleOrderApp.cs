using OpenAuth.App.Order.Request;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenAuth.App.Order
{
    public partial class ServiceSaleOrderApp
    {
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
    }
}
