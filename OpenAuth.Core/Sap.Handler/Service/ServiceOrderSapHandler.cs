using DotNetCore.CAP;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using SAPbobsCOM;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAuth.Repository.Domain.Sap;
using System.Reactive;

namespace Sap.Handler.Service
{
    public class ServiceOrderSapHandler : ICapSubscribe
    {
        private readonly IUnitWork UnitWork;
        private readonly Company company;

        public ServiceOrderSapHandler(IUnitWork unitWork, Company company)
        {
            UnitWork = unitWork;
            this.company = company;
        }

        [CapSubscribe("Serve.ServcieOrder.Create")]
        public async Task HandleServiceOrder(int theServiceOrderId)
        {
            var query = UnitWork.Find<ServiceOrder>(s => s.Id.Equals(theServiceOrderId))
                .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType).FirstOrDefault();
               
            var thisSorder =  query.MapTo<ServiceOrder>();
            if (thisSorder.ServiceWorkOrders.Count > 0) {
                //同步到SAP
                var thisSwork = thisSorder.ServiceWorkOrders[0];
                int eCode;
                string eMesg; 
                StringBuilder allerror = new StringBuilder();
                string docNum = string.Empty;
                try
                {
                    SAPbobsCOM.ServiceCalls sc = (SAPbobsCOM.ServiceCalls)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                    SAPbobsCOM.KnowledgeBaseSolutions kbs = (SAPbobsCOM.KnowledgeBaseSolutions)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oKnowledgeBaseSolutions);
                    #region 赋值

                    sc.CustomerCode = thisSorder.CustomerId;
                    sc.CustomerName = thisSorder.CustomerName;
                    sc.Subject = thisSwork.FromTheme;
                    //sc.ContactCode = 15;
                   if (!string.IsNullOrWhiteSpace(thisSwork.ContractId) && thisSwork.ContractId.Trim() != "-1")
                    {
                        sc.ContractID = Convert.ToInt32(thisSwork.ContractId);
                    }
                    //sc.ManufacturerSerialNum = thisSwork.ManufacturerSerialNumber;
                    //sc.InternalSerialNum = thisSwork.InternalSerialNumber;

                    //if (thisSorder.FromId != null && thisSorder.FromId != -1)
                    //{
                    //    sc.Origin = (int)thisSorder.FromId;
                    //}
                    if (!string.IsNullOrEmpty(thisSwork.MaterialCode) && IsValidItemCode(thisSwork.MaterialCode))
                    {
                        sc.ItemCode = thisSwork.MaterialCode;
                        sc.ItemDescription = thisSwork.MaterialDescription;
                    }
                    sc.Status = -3;// 待处理 
                    if (thisSwork.Priority != null && thisSwork.Priority == 3)
                    {
                        sc.Priority = BoSvcCallPriorities.scp_High;
                    }
                    else if (thisSwork.Priority != null && thisSwork.Priority == 2)
                    {
                        sc.Priority = BoSvcCallPriorities.scp_Medium;
                    }
                    else
                    {
                        sc.Priority = BoSvcCallPriorities.scp_Low;
                    }
                    //if (thisSwork.FromType != null)
                    //{
                    //    sc.CallType = (int)thisSwork.FromType;
                    //}
                    if (thisSwork.ProblemType != null)
                    {
                        sc.ProblemType = thisSwork.ProblemType.PrblmTypID;
                    }
                    sc.Description = thisSwork.Remark;
                    //sc.TechnicianCode = thisWorkOrder.ServiceOrder.SupervisorId;
                    //sc.City = thisWorkOrder.ServiceOrder.City;
                    //sc.Room = thisWorkOrder.ServiceOrder.Addr;
                    //sc.State = thisWorkOrder.ServiceOrder.Province;
                    //sc.Country = thisWorkOrder.ServiceOrder.
                    #endregion
                    int res = sc.Add();
                    if (res != 0)
                    {
                        company.GetLastError(out eCode, out eMesg);
                        allerror.Append("添加服务呼叫到SAP时异常！错误代码：" + eCode + "错误信息：" + eMesg);
                    }
                    else
                    {
                        company.GetNewObjectCode(out docNum);
                    }
                }
                catch (Exception e)
                {
                    allerror.Append("调用SBO接口添加服务呼叫时异常：" + e.ToString() + "");
                }

                if (!string.IsNullOrEmpty(docNum))
                {
                    //如果同步成功则修改serviceOrder
                    await UnitWork.UpdateAsync<ServiceOrder>(s => s.Id.Equals(theServiceOrderId), e => new ServiceOrder
                    {
                        U_SAP_ID = System.Convert.ToInt32(docNum)
                    });
                    var ServiceWorkOrders = await UnitWork.Find<ServiceWorkOrder>(u => u.ServiceOrderId.Equals(theServiceOrderId)).AsNoTracking().ToListAsync();
                    int num = 0;
                    ServiceWorkOrders.ForEach(u => u.WorkOrderNumber = docNum + "-" + ++num);
                    UnitWork.BatchUpdate<ServiceWorkOrder>(ServiceWorkOrders.ToArray());
                    await UnitWork.SaveAsync();
                    Log.Logger.Warning($"同步成功，SAP_ID：{docNum}", typeof(ServiceOrderSapHandler));
                }
                if (!string.IsNullOrWhiteSpace(allerror.ToString()))
                {
                    Log.Logger.Error(allerror.ToString(), typeof(ServiceOrderSapHandler));
                }
            }
        }

        [CapSubscribe("Serve.ServcieOrder.CreateFromAPP")]
        public async Task HandleServiceOrderAPP(int theServiceOrderId)
        {
            var query = UnitWork.Find<ServiceOrder>(s => s.Id.Equals(theServiceOrderId)).Include(s=>s.ServiceOrderSNs).FirstOrDefault();

            var thisSorder = query.MapTo<ServiceOrder>();
            //同步到SAP
            int eCode;
            string eMesg;
            StringBuilder allerror = new StringBuilder();
            string docNum = string.Empty;
            try
            {
                SAPbobsCOM.ServiceCalls sc = (SAPbobsCOM.ServiceCalls)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                SAPbobsCOM.KnowledgeBaseSolutions kbs = (SAPbobsCOM.KnowledgeBaseSolutions)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oKnowledgeBaseSolutions);
                #region 赋值

                sc.CustomerCode = thisSorder.CustomerId;
                sc.CustomerName = thisSorder.CustomerName;
                sc.Subject = thisSorder.Services.Length>250? thisSorder.Services.Substring(0, 250):thisSorder.Services ;
                //if (thisSorder.FromId != null && thisSorder.FromId != -1)
                //{
                //    sc.Origin = (int)thisSorder.FromId;
                //}
                if (thisSorder.ServiceOrderSNs!=null && thisSorder.ServiceOrderSNs.Count > 0)
                {
                    var thisSN = thisSorder.ServiceOrderSNs[0];
                    if (!string.IsNullOrEmpty(thisSN.ItemCode) && IsValidItemCode(thisSN.ItemCode))
                    {
                        sc.ItemCode = thisSN.ItemCode;
                        sc.ManufacturerSerialNum = thisSN.ManufSN;
                    }
                }
                sc.Status = -3;// 待处理 
                sc.Priority = BoSvcCallPriorities.scp_Low;
                //if (thisSwork.FromType != null)
                //{
                //    sc.CallType = (int)thisSwork.FromType;
                //}
                //if (thisSorder.ProblemTypeId != null)
                //{

                //    sc.ProblemType = thisSorder.PRO;
                //}
                if (!string.IsNullOrEmpty(thisSorder.ProblemTypeId))
                {
                    var queryp = UnitWork.Find<ProblemType>(s => s.Id.Equals(thisSorder.ProblemTypeId)).FirstOrDefault();
                    var pbltype = queryp.MapTo<ProblemType>();
                    if (pbltype!=null)
                    {
                        sc.ProblemType = pbltype.PrblmTypID;
                    }
                }
                sc.Description = thisSorder.Services;

                #endregion
                int res = sc.Add();
                if (res != 0)
                {
                    company.GetLastError(out eCode, out eMesg);
                    allerror.Append("添加服务呼叫到SAP时异常！错误代码：" + eCode + "错误信息：" + eMesg);
                }
                else
                {
                    company.GetNewObjectCode(out docNum);
                }
            }
            catch (Exception e)
            {
                allerror.Append("调用SBO接口添加服务呼叫时异常：" + e.ToString() + "");
            }

            if (!string.IsNullOrEmpty(docNum))
            {
                //如果同步成功则修改serviceOrder
                await UnitWork.UpdateAsync<ServiceOrder>(s => s.Id.Equals(theServiceOrderId), e => new ServiceOrder
                {
                    U_SAP_ID = System.Convert.ToInt32(docNum)
                });
                await UnitWork.SaveAsync();
                Log.Logger.Warning($"同步成功，SAP_ID：{docNum}", typeof(ServiceOrderSapHandler));
            }
            if (!string.IsNullOrWhiteSpace(allerror.ToString()))
            {
                Log.Logger.Error(allerror.ToString(), typeof(ServiceOrderSapHandler));
            }
        }


        [CapSubscribe("Serve.ServcieOrder.CreateWorkNumber")]
        public async Task HandleCreateWorkNumber(int ServiceOrderId)
        {
            try
            {
                var ServiceOrder = UnitWork.Find<ServiceOrder>(s => s.Id.Equals(ServiceOrderId)).AsNoTracking().FirstOrDefault();
                var ServiceWorkOrders = await UnitWork.Find<ServiceWorkOrder>(u => u.ServiceOrderId.Equals(ServiceOrderId)).AsNoTracking().ToListAsync();
                int num = 0;
                ServiceWorkOrders.ForEach(u => u.WorkOrderNumber = ServiceOrder.U_SAP_ID + "-" + ++num);
                UnitWork.BatchUpdate<ServiceWorkOrder>(ServiceWorkOrders.ToArray());
                await UnitWork.SaveAsync();
            }
            catch (Exception e)
            {
                Log.Logger.Warning($"同步ID：{ServiceOrderId}失败,错误信息：{e.Message}", typeof(ServiceOrderSapHandler));
            }
            
        }


        /// <summary>
        /// 判断物料编码在客户端是否存在
        /// </summary>
        /// <param name="materialCode">物料编码</param>
        /// <returns></returns>
        public bool IsValidItemCode(string materialCode)
        {
            var query = UnitWork.Find<OITM>(o => o.ItemCode.Equals(materialCode)).Select(q => new { q.ItemCode });
            if (query.Count() > 0)
            {
                return true;
            }
            return false;
        }
    }
}
