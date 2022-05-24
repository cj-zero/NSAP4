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
using Sap.Handler.Service.Request;
using System.Threading;

namespace Sap.Handler.Service
{
    public class ServiceOrderSapHandler : ICapSubscribe
    {
        private readonly IUnitWork UnitWork;
        private readonly Company company;
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁

        public ServiceOrderSapHandler(IUnitWork unitWork, Company company)
        {
            UnitWork = unitWork;
            this.company = company;
        }

        [CapSubscribe("Serve.ServcieOrder.Create")]
        public async Task HandleServiceOrder(int theServiceOrderId)
        {
            var query =await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(theServiceOrderId)).AsNoTracking()
                .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType).FirstOrDefaultAsync();
            Log.Logger.Information($"获取服务单信息完成，theServiceOrderId：{theServiceOrderId}，开始同步", typeof(ServiceOrderSapHandler));
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
                    //判断是客户还是供应商
                    string cardtype =await UnitWork.Find<OCRD>(w => w.CardCode.Equals(thisSorder.CustomerId)).Select(s =>s.CardType).FirstOrDefaultAsync();
                    if (!string.IsNullOrEmpty(cardtype) && cardtype == "S")
                    {
                        sc.ServiceBPType = ServiceTypeEnum.srvcPurchasing;
                    }
                    else
                    {
                        sc.ServiceBPType = ServiceTypeEnum.srvcSales;
                    }
                    var FromTheme = JsonHelper.Instance.Deserialize<List<FromThemeJson>>(thisSwork.FromTheme);
                    sc.Subject = FromTheme.FirstOrDefault()?.description;
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
                        Log.Logger.Information($"添加服务呼叫到SAP成功！", typeof(ServiceOrderSapHandler));
                    }
                }
                catch (Exception e)
                {
                    allerror.Append("调用SBO接口添加服务呼叫时异常：" + e.ToString() + "");
                }

                if (!string.IsNullOrEmpty(docNum))
                {
                    //用信号量代替锁
                    semaphoreSlim.Wait();
                    try
                    {
                        //如果同步成功则修改serviceOrder
                        UnitWork.Update<ServiceOrder>(s => s.Id.Equals(theServiceOrderId), e => new ServiceOrder
                        {
                            U_SAP_ID = System.Convert.ToInt32(docNum)
                        });
                        var ServiceWorkOrders = await UnitWork.Find<ServiceWorkOrder>(u => u.ServiceOrderId.Equals(theServiceOrderId)).AsNoTracking().ToListAsync();
                        int num = 0;
                        ServiceWorkOrders.ForEach(u => u.WorkOrderNumber = docNum + "-" + ++num);
                        UnitWork.BatchUpdate<ServiceWorkOrder>(ServiceWorkOrders.ToArray());
                        UnitWork.Save();
                        Log.Logger.Warning($"同步成功，SAP_ID：{docNum}", typeof(ServiceOrderSapHandler));
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error($"反写4.0失败，SAP_ID：{docNum}失败原因:{ex.Message}", typeof(ServiceOrderSapHandler));
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
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
                //判断是客户还是供应商
                string cardtype = await UnitWork.Find<OCRD>(w => w.CardCode.Equals(thisSorder.CustomerId)).Select(s => s.CardType).FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(cardtype) && cardtype == "S")
                {
                    sc.ServiceBPType = ServiceTypeEnum.srvcPurchasing;
                }
                else
                {
                    sc.ServiceBPType = ServiceTypeEnum.srvcSales;
                }
                if (string.IsNullOrWhiteSpace(thisSorder.Services))
                {
                    sc.Subject = "无";
                    sc.Description = "无";
                }
                else
                {
                    sc.Subject = thisSorder.Services.Length > 250 ? thisSorder.Services.Substring(0, 250) : thisSorder.Services;
                    sc.Description = thisSorder.Services;
                }
                //if (thisSorder.FromId != null && thisSorder.FromId != -1)
                //{
                //    sc.Origin = (int)thisSorder.FromId;
                //}
                if (thisSorder.ServiceOrderSNs != null && thisSorder.ServiceOrderSNs.Count > 0)
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
                    if (pbltype != null)
                    {
                        sc.ProblemType = pbltype.PrblmTypID;
                    }
                }

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
                var ServiceOrder = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(ServiceOrderId)).AsNoTracking().FirstOrDefaultAsync();
                var ServiceWorkOrders = await UnitWork.Find<ServiceWorkOrder>(u => u.ServiceOrderId.Equals(ServiceOrderId)).AsNoTracking().ToListAsync();
                int num = 0;
                ServiceWorkOrders.ForEach(u => u.WorkOrderNumber = ServiceOrder.U_SAP_ID + "-" + ++num);
                UnitWork.BatchUpdate<ServiceWorkOrder>(ServiceWorkOrders.ToArray());
                await UnitWork.SaveAsync();
            }
            catch (Exception e)
            {
                Log.Logger.Error($"同步ID：{ServiceOrderId}失败,错误信息：{e.Message}", typeof(ServiceOrderSapHandler));
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

        /// <summary>
        /// 新建联系人
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type">单据类型 1.服务单 2.物料单</param>
        /// <returns></returns>
        [CapSubscribe("Serve.OCPR.Create")]
        public async Task HandleCreateContact(AddCoustomerContact obj)
        {
            StringBuilder allerror = new StringBuilder();
            try
            {
                int res, eCode;
                string eMesg;
                SAPbobsCOM.BusinessPartners bp = (SAPbobsCOM.BusinessPartners)company.GetBusinessObject(BoObjectTypes.oBusinessPartners);
                bp.GetByKey(obj.CardCode);
                Log.Logger.Warning($"cardcode:{obj.CardCode}");

                var rLineNum = await UnitWork.Find<OCPR>(c => c.CardCode == obj.CardCode).CountAsync();
                Log.Logger.Warning($"rLineNum:{rLineNum}");
                bp.ContactEmployees.Add();
                bp.ContactEmployees.SetCurrentLine(rLineNum);
                bp.ContactEmployees.Active = BoYesNoEnum.tYES;
                bp.ContactEmployees.Name = obj.NewestContacter.Trim();       //联系人名称
                bp.ContactEmployees.Phone1 = obj.NewestContactTel;             //电话1
                bp.ContactEmployees.Address = obj.Address;
                res = bp.Update();
                if (res != 0)
                {
                    company.GetLastError(out eCode, out eMesg);
                    allerror.Append("添加客户联系人到SAP时异常！错误代码：" + eCode + "错误信息：" + eMesg);
                }
                else
                {
                    //同步至3.0
                    Log.Logger.Warning($"开始同步至3.0", typeof(ServiceOrderSapHandler));
                    await HandleERPCreateContact(obj.CardCode);
                    Log.Logger.Warning($"同步成功", typeof(ServiceOrderSapHandler));

                }
            }
            catch (Exception e)
            {
                allerror.Append("调用SBO接口添加客户联系人时异常：" + e.ToString() + "");
            }
            if (!string.IsNullOrWhiteSpace(allerror.ToString()))
            {
                Log.Logger.Error(allerror.ToString(), typeof(ServiceOrderSapHandler));
            }
        }

        /// <summary>
        /// 同步至3.0
        /// </summary>
        /// <returns></returns>
        [CapSubscribe("Serve.OCPR.ERPCreate")]
        public async Task HandleERPCreateContact(string cardcode)
        {
            try
            {
                var erpctList = await UnitWork.Find<crm_ocpr>(c => c.CardCode == cardcode).Select(c => c.CntctCode).ToListAsync();
                //新增的联系人
                var contactList = await UnitWork.Find<OCPR>(c => c.CardCode == cardcode && !erpctList.Contains(c.CntctCode)).Select(c => new
                {
                    c.CntctCode,
                    c.CardCode,
                    c.Name,
                    c.Position,
                    c.Address,
                    c.Tel1,
                    c.Tel2,
                    c.Cellolar,
                    c.Fax,
                    c.E_MailL,
                    c.Notes1,
                    c.Notes2,
                    c.BirthDate,
                    c.Gender,
                    c.Active,
                    c.U_ACCT,
                    c.U_BANK
                }).ToListAsync();

                List<crm_ocpr> ocpr = new List<crm_ocpr>();
                foreach (var c in contactList)
                {
                    crm_ocpr crm_Ocpr = new crm_ocpr
                    {
                        CntctCode = c.CntctCode,
                        CardCode = c.CardCode,
                        Name = c.Name,
                        Position = c.Position,
                        Address = c.Address,
                        Tel1 = c.Tel1,
                        Tel2 = c.Tel2,
                        Cellolar = c.Cellolar,
                        Fax = c.Fax,
                        E_MailL = c.E_MailL,
                        Notes1 = c.Notes1,
                        Notes2 = c.Notes2,
                        BirthDate = c.BirthDate,
                        Gender = c.Gender,
                        Active = c.Active,
                        U_ACCT = c.U_ACCT,
                        U_BANK = c.U_BANK
                    };
                    ocpr.Add(crm_Ocpr);
                    //await UnitWork.AddAsync<crm_ocpr,int>(crm_Ocpr);
                }
                await UnitWork.BatchAddAsync<crm_ocpr, int>(ocpr.ToArray());
                await UnitWork.SaveAsync();
                Log.Logger.Debug($"同步3.0成功，CntctCode：{contactList.Select(c => c.CntctCode).ToList()}", typeof(SellOrderSapHandler));
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"同步3.0失败，" + ex.Message, typeof(SellOrderSapHandler));
            }
        }
    }
}
