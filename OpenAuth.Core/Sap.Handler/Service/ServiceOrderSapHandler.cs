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
                   if (thisSwork.ContractId.Trim() != "" && thisSwork.ContractId != null && thisSwork.ContractId.Trim() != "-1")
                    {
                        sc.ContractID = Convert.ToInt32(thisSwork.ContractId);
                    }
                    //sc.ManufacturerSerialNum = thisSwork.ManufacturerSerialNumber;
                    //sc.InternalSerialNum = thisSwork.InternalSerialNumber;

                    //if (thisSorder.FromId != null && thisSorder.FromId != -1)
                    //{
                    //    sc.Origin = (int)thisSorder.FromId;
                    //}
                    sc.ItemCode = thisSwork.MaterialCode;
                    sc.ItemDescription = thisSwork.MaterialDescription;
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
                    await UnitWork.SaveAsync();
                }
                else
                {
                    
                }
                if (!string.IsNullOrWhiteSpace(allerror.ToString()))
                {
                    Log.Logger.Error(allerror.ToString(), typeof(ServiceOrderSapHandler));
                }
            }
        }
    }
}
