﻿using Infrastructure;
using Infrastructure.Excel;
using Infrastructure.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Material.Response;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Sap.BusinessPartner;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npoi.Mapper;
using OpenAuth.Repository.Domain.Material;
using System.IO;
using Infrastructure.Export;
using DinkToPdf;
using DotNetCore.CAP;
using System.Threading;
using OpenAuth.Repository.Domain.NsapBone;
using Microsoft.Data.SqlClient;
using OpenAuth.Repository.Domain.Workbench;
using OpenAuth.App.Workbench;
using Infrastructure.Const;

namespace OpenAuth.App.Material
{
    /// <summary>
    /// 报价单操作
    /// </summary>
    public class QuotationApp : OnlyUnitWorkBaeApp
    {
        private readonly FlowInstanceApp _flowInstanceApp;

        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        public readonly WorkbenchApp _workbenchApp;

        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁

        private ICapPublisher _capBus;

        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> Load(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(request.AppId));
            }
            var result = new TableData();
            List<int> ServiceOrderids = new List<int>();
            if (!string.IsNullOrWhiteSpace(request.CardCode))
            {
                ServiceOrderids = await UnitWork.Find<ServiceOrder>(null).Where(q => q.TerminalCustomer.Contains(request.CardCode) || q.TerminalCustomerId.Contains(request.CardCode)).Select(s => s.Id).ToListAsync();

            }
            var Quotations = UnitWork.Find<Quotation>(null).Include(q => q.QuotationPictures).Include(q => q.QuotationOperationHistorys).WhereIf(request.QuotationId.ToString() != null, q => q.Id.ToString().Contains(request.QuotationId.ToString()))
                                .WhereIf(request.ServiceOrderSapId != null, q => q.ServiceOrderSapId.ToString().Contains(request.ServiceOrderSapId.ToString()))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUser), q => q.CreateUser.Contains(request.CreateUser))
                                .WhereIf(request.StartCreateTime != null, q => q.CreateTime > request.StartCreateTime)
                                .WhereIf(request.EndCreateTime != null, q => q.CreateTime < request.EndCreateTime)
                                .WhereIf(request.Status != null, q => q.Status == request.Status)
                                .WhereIf(request.QuotationStatus != null, q => q.QuotationStatus == request.QuotationStatus)
                                .WhereIf(request.SalesOrderId != null, q => q.SalesOrderId == request.SalesOrderId)
                                .WhereIf(ServiceOrderids.Count() > 0, q => ServiceOrderids.Contains(q.ServiceOrderId));
            var flowInstanceIds = await Quotations.Select(q => q.FlowInstanceId).ToListAsync();
            var flowinstanceObjs = from a in UnitWork.Find<FlowInstance>(f => flowInstanceIds.Contains(f.Id))
                                   join b in UnitWork.Find<FlowInstanceOperationHistory>(null) on a.Id equals b.InstanceId into ab
                                   from b in ab.DefaultIfEmpty()
                                   select new { a, b };
            var flowinstanceList = await flowinstanceObjs.ToListAsync();
            if (!loginContext.Roles.Any(r => r.Name.Equals("呼叫中心-查看")) && !loginContext.Roles.Any(r => r.Name.Equals("客服主管")) && !loginUser.Account.Equals(Define.SYSTEM_USERNAME))
            {

                if (request.PageStart != null && request.PageStart == 1)
                {
                    if (loginContext.Roles.Any(r => r.Name.Equals("销售员")))
                    {
                        switch (request.StartType)
                        {
                            case 1:
                                flowInstanceIds = flowinstanceList.Where(f => f.a.ActivityName == "销售员审批").Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                                break;
                            case 2:
                                flowInstanceIds = flowinstanceList.Where(f => f.b.CreateUserId.Equals(loginContext.User.Id) && f.b.Content == "销售员审批").Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                                break;
                            default:
                                flowInstanceIds = flowinstanceList.Where(f => (f.b.CreateUserId.Equals(loginContext.User.Id) || f.a.ActivityName == "销售员审批")).Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                                break;
                        }
                        ServiceOrderids = await UnitWork.Find<ServiceOrder>(null).Where(q => q.SalesManId.Equals(loginContext.User.Id)).WhereIf(ServiceOrderids.Count() > 0, s => ServiceOrderids.Contains(s.Id)).Select(s => s.Id).ToListAsync();
                        Quotations = Quotations.Where(q => ServiceOrderids.Contains(q.ServiceOrderId));
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("物料工程审批")))
                    {
                        switch (request.StartType)
                        {
                            case 1:
                                flowInstanceIds = flowinstanceList.Where(f => f.a.ActivityName == "工程审批").Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                                break;
                            case 2:
                                flowInstanceIds = flowinstanceList.Where(f => f.b.CreateUserId.Equals(loginContext.User.Id) && f.b.Content == "工程审批").Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                                break;
                            default:
                                flowInstanceIds = flowinstanceList.Where(f => (f.b.CreateUserId.Equals(loginContext.User.Id) || f.a.ActivityName == "工程审批")).Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                                break;
                        }
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")))
                    {
                        ServiceOrderids = await UnitWork.Find<ServiceOrder>(null).Where(q => q.SalesManId.Equals(loginContext.User.Id)).Select(s => s.Id).ToListAsync();
                        List<string> slpIds = null;
                        switch (request.StartType)
                        {
                            case 1:
                                flowInstanceIds = flowinstanceList.Where(f => f.a.ActivityName == "总经理审批").Select(f => f.a.Id).Distinct().ToList();
                                slpIds = flowinstanceList.Where(f => f.a.ActivityName == "销售员审批").Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (ServiceOrderids.Contains(q.ServiceOrderId) && slpIds.Contains(q.FlowInstanceId)));
                                break;
                            case 2:
                                flowInstanceIds = flowinstanceList.Where(f => f.b.CreateUserId.Equals(loginContext.User.Id) && f.b.Content == "总经理审批").Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                                break;
                            default:
                                flowInstanceIds = flowinstanceList.Where(f => (f.b.CreateUserId.Equals(loginContext.User.Id) || f.a.ActivityName == "总经理审批")).Select(f => f.a.Id).Distinct().ToList();
                                slpIds = flowinstanceList.Where(f => f.a.ActivityName == "销售员审批").Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (ServiceOrderids.Contains(q.ServiceOrderId) && slpIds.Contains(q.FlowInstanceId)));
                                break;
                        }
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("销售总助")))
                    {
                        switch (request.StartType)
                        {
                            case 1:
                                flowInstanceIds = flowinstanceList.Where(f => f.a.ActivityName == "销售总助审批").Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                                break;
                            case 2:
                                flowInstanceIds = flowinstanceList.Where(f => f.b.CreateUserId.Equals(loginContext.User.Id) && f.b.Content == "销售总助审批").Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                                break;
                            default:
                                flowInstanceIds = flowinstanceList.Where(f => (f.b.CreateUserId.Equals(loginContext.User.Id) || f.a.ActivityName == "销售总助审批")).Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                                break;
                        }
                    }
                    else
                    {
                        switch (request.StartType)
                        {
                            case 1:
                                flowInstanceIds = flowinstanceList.Where(f => f.a.ActivityName == "确认报价单").Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) && q.CreateUserId.Equals(loginUser.Id));
                                break;

                            case 2:
                                flowInstanceIds = flowinstanceList.Where(f => f.b.CreateUserId.Equals(loginContext.User.Id) && f.b.Content == "确认报价单").Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) && q.CreateUserId.Equals(loginUser.Id));
                                break;
                            default:
                                flowInstanceIds = flowinstanceList.Where(f => (f.b.CreateUserId.Equals(loginContext.User.Id) || f.a.ActivityName == "确认报价单")).Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) && q.CreateUserId.Equals(loginUser.Id));
                                break;
                        }
                    }

                }
                else if (request.PageStart != null && request.PageStart == 2)
                {
                    if (loginContext.Roles.Any(r => r.Name.Equals("物料财务")))
                    {
                        switch (request.StartType)
                        {
                            case 1:
                                flowInstanceIds = flowinstanceList.Where(f => f.a.ActivityName == "财务审批").Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                                break;
                            case 2:
                                flowInstanceIds = flowinstanceList.Where(f => f.b.CreateUserId.Equals(loginContext.User.Id) && f.b.Content == "财务审批").Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                                break;
                            default:
                                flowInstanceIds = flowinstanceList.Where(f => (f.b.CreateUserId.Equals(loginContext.User.Id) || f.a.ActivityName == "财务审批")).Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                                break;
                        }
                    }
                    else
                    {
                        switch (request.StartType)
                        {
                            case 1:
                                flowInstanceIds = flowinstanceList.Where(f => f.a.ActivityName == "回传销售订单").Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) && q.CreateUserId.Equals(loginUser.Id));
                                break;

                            case 2:
                                flowInstanceIds = flowinstanceList.Where(f => (f.b.CreateUserId.Equals(loginContext.User.Id) && f.b.Content == "回传销售订单")).Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) && q.CreateUserId.Equals(loginUser.Id));
                                break;
                            default:
                                flowInstanceIds = flowinstanceList.Where(f => (f.b.CreateUserId.Equals(loginContext.User.Id) || f.a.ActivityName == "回传销售订单")).Select(f => f.a.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) && q.CreateUserId.Equals(loginUser.Id));
                                break;
                        }
                    }
                }
                else if (request.PageStart != null && request.PageStart == 3)
                {
                    if (!loginContext.Roles.Any(r => r.Name.Equals("仓库")))
                    {
                        Quotations = Quotations.Where(q => q.CreateUserId.Equals(loginUser.Id));
                    }
                    switch (request.StartType)
                    {
                        case 1:
                            flowInstanceIds = flowinstanceList.Where(f => f.a.ActivityName == "待出库").Select(f => f.a.Id).Distinct().ToList();
                            Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                            break;
                        case 2:
                            flowInstanceIds = flowinstanceList.Where(f => f.a.ActivityName == "结束").Select(f => f.a.Id).Distinct().ToList();
                            Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                            break;
                        default:
                            flowInstanceIds = flowinstanceList.Where(f => f.a.ActivityName == "待出库" || f.a.ActivityName == "结束").Select(f => f.a.Id).Distinct().ToList();
                            Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                            break;
                    }
                    Quotations = Quotations.Where(q => (q.IsMaterialType != null || q.QuotationStatus == 11));
                }
                else
                {
                    if (!loginContext.Roles.Any(r => r.Name.Equals("物料稽查")))
                    {
                        Quotations = Quotations.Where(q => q.CreateUserId.Equals(loginUser.Id));
                    }
                }
            }

            var QuotationDate = await Quotations.OrderByDescending(q => q.UpDateTime).Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            List<string> fileids = new List<string>();
            QuotationDate.ForEach(q => fileids.AddRange(q.QuotationPictures.Select(p => p.PictureId).ToList()));

            var file = await UnitWork.Find<UploadFile>(f => fileids.Contains(f.Id)).ToListAsync();
            ServiceOrderids = QuotationDate.Select(q => q.ServiceOrderId).ToList();
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(null).Where(q => ServiceOrderids.Contains(q.Id)).Select(s => new { s.Id, s.TerminalCustomer, s.TerminalCustomerId, s.CustomerId }).ToListAsync();
            var query = from a in QuotationDate
                        join b in ServiceOrders on a.ServiceOrderId equals b.Id
                        select new { a, b };
            var terminalCustomerIds = query.Select(q => q.b.TerminalCustomerId).ToList();
            var ocrds = await UnitWork.Find<crm_ocrd>(o => terminalCustomerIds.Contains(o.CardCode)).ToListAsync();
            result.Data = query.Select(q => new
            {
                q.a.Id,
                q.a.ServiceOrderSapId,
                q.a.ServiceOrderId,
                q.b.TerminalCustomer,
                q.b.CustomerId,
                q.b.TerminalCustomerId,
                q.a.TotalMoney,
                q.a.CreateUser,
                q.a.Remark,
                q.a.SalesOrderId,
                CreateTime = Convert.ToDateTime(q.a.CreateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                UpDateTime = q.a.QuotationOperationHistorys.FirstOrDefault() != null ? Convert.ToDateTime(q.a.QuotationOperationHistorys.OrderByDescending(h => h.CreateTime).FirstOrDefault()?.CreateTime).ToString("yyyy.MM.dd HH:mm:ss") : Convert.ToDateTime(q.a.UpDateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                q.a.QuotationStatus,
                StatusName = flowinstanceObjs.Where(f => f.a.Id.Equals(q.a.FlowInstanceId)).FirstOrDefault()?.a.IsFinish == FlowInstanceStatus.Rejected ? "驳回" : flowinstanceObjs.Where(f => f.a.Id.Equals(q.a.FlowInstanceId)).FirstOrDefault()?.a.IsFinish == null || q.a.IsDraft == true ? "未提交" : flowinstanceObjs.Where(f => f.a.Id.Equals(q.a.FlowInstanceId)).FirstOrDefault()?.a.IsFinish == FlowInstanceStatus.Draft ? "撤回" : flowinstanceObjs.Where(f => f.a.Id.Equals(q.a.FlowInstanceId)).FirstOrDefault()?.a.ActivityName,
                q.a.Tentative,
                q.a.IsProtected,
                q.a.PrintWarehouse,
                Balance = ocrds.Where(o => o.CardCode.Equals(q.b.TerminalCustomerId)).FirstOrDefault()?.Balance,
                files = q.a.QuotationPictures.Select(p => new
                {
                    fileName = file.Where(f => f.Id.Equals(p.PictureId)).FirstOrDefault()?.FileName,
                    fileType = file.Where(f => f.Id.Equals(p.PictureId)).FirstOrDefault()?.FileType,
                    fileId = p?.PictureId
                }).ToList()
            }).OrderByDescending(q => Convert.ToDateTime(q.UpDateTime)).ToList();
            result.Count = await Quotations.CountAsync();

            return result;
        }

        /// <summary>
        /// 加载状态列表
        /// </summary>
        public async Task<TableData> GetQuotationOperationHistory(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var Quotations = await UnitWork.Find<Quotation>(null).Include(q => q.Expressages).Include(q => q.QuotationOperationHistorys).WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderId.ToString()), q => q.ServiceOrderId.Equals(request.ServiceOrderId))
                     .WhereIf(!string.IsNullOrWhiteSpace(request.QuotationId.ToString()), q => q.Id.Equals(request.QuotationId)).ToListAsync();
            Quotations.ForEach(q => q.QuotationOperationHistorys = q.QuotationOperationHistorys.OrderBy(o => o.CreateTime).ToList());
            result.Data = Quotations.Skip((request.page - 1) * request.limit).Take(request.limit).ToList();
            result.Count = Quotations.Count();
            return result;
        }

        /// <summary>
        /// 加载服务单列表
        /// </summary>
        public async Task<TableData> GetServiceOrderList(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(request.AppId));
            }
            var result = new TableData();
            var ServiceOrders = from a in UnitWork.Find<ServiceOrder>(s => s.VestInOrg <= 1)
                                join b in UnitWork.Find<ServiceWorkOrder>(s => s.Status < 7) on a.Id equals b.ServiceOrderId
                                select new { a, b };
            ServiceOrders = ServiceOrders.WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderId.ToString()), s => s.a.Id.Equals(request.ServiceOrderId))
                .WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderSapId.ToString()), s => s.a.U_SAP_ID.Equals(request.ServiceOrderSapId));
            var ServiceOrderList = (await ServiceOrders.Where(s => s.b.CurrentUserNsapId.Equals(loginUser.Id)).ToListAsync()).GroupBy(s => s.a.Id).Select(s => s.First());
            var CustomerIds = ServiceOrderList.Select(s => s.a.TerminalCustomerId).ToList();
            var CardAddress = from a in UnitWork.Find<OCRD>(null)
                              join c in UnitWork.Find<OCRY>(null) on a.Country equals c.Code into ac
                              from c in ac.DefaultIfEmpty()
                              join d in UnitWork.Find<OCST>(null) on a.State1 equals d.Code into ad
                              from d in ad.DefaultIfEmpty()
                              join e in UnitWork.Find<OCRY>(null) on a.MailCountr equals e.Code into ae
                              from e in ae.DefaultIfEmpty()
                              where CustomerIds.Contains(a.CardCode)
                              select new { a, c, d, e };
            var Address = await CardAddress.Select(q => new
            {
                q.a.CardCode,
                q.a.Balance,
                q.a.frozenFor,
                BillingAddress = $"{ q.a.ZipCode ?? "" }{ q.c.Name ?? "" }{ q.d.Name ?? "" }{ q.a.City ?? ""}{ q.a.Building ?? "" }",
                DeliveryAddress = $"{ q.a.MailZipCod ?? "" }{ q.e.Name ?? "" }{ q.d.Name ?? "" }{ q.a.MailCity ?? "" }{ q.a.MailBuildi ?? "" }"
            }).ToListAsync();
            result.Data = ServiceOrderList.Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select(q => new
                {
                    q.a.Id,
                    q.a.U_SAP_ID,
                    q.a.TerminalCustomer,
                    q.a.TerminalCustomerId,
                    q.a.NewestContacter,
                    q.a.NewestContactTel,
                    q.b.FromTheme,
                    q.a.SalesMan,
                    BillingAddress = Address.Where(a => a.CardCode.Equals(q.a.CustomerId)).FirstOrDefault()?.BillingAddress,//开票地址
                    DeliveryAddress = Address.Where(a => a.CardCode.Equals(q.a.CustomerId)).FirstOrDefault()?.DeliveryAddress, //收货地址
                    Balance = Address.Where(a => a.CardCode.Equals(q.a.CustomerId)).FirstOrDefault()?.Balance, //额度
                    frozenFor = Address.Where(a => a.CardCode.Equals(q.a.CustomerId)).FirstOrDefault()?.frozenFor == "N" ? "正常" : "冻结" //客户状态
                });
            result.Count = ServiceOrderList.Count();

            return result;
        }

        /// <summary>
        /// 获取序列号和设备
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetSerialNumberList(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(request.AppId));
            }
            var result = new TableData();

            var ServiceWorkOrderList = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId.Equals(request.ServiceOrderId))
                .WhereIf(string.IsNullOrWhiteSpace(request.CreateUserId), s => s.CurrentUserNsapId.Equals(loginUser.Id))
                .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUserId), s => s.CurrentUserNsapId.Equals(request.CreateUserId))
                .WhereIf(!string.IsNullOrWhiteSpace(request.MaterialType), s => s.MaterialCode.Substring(0, 2) == request.MaterialType)
                .WhereIf(!string.IsNullOrWhiteSpace(request.ManufacturerSerialNumbers), s => s.ManufacturerSerialNumber.Contains(request.ManufacturerSerialNumbers))
                .WhereIf(!string.IsNullOrWhiteSpace(request.MaterialCode), s => s.MaterialCode.Contains(request.MaterialCode))
                .Select(s => new { s.ManufacturerSerialNumber, s.MaterialCode, s.MaterialDescription, s.FromTheme }).ToListAsync();
            if (ServiceWorkOrderList != null && ServiceWorkOrderList.Count > 0)
            {
                #region 获取交货创建时间
                var MnfSerials = ServiceWorkOrderList.Select(s => s.ManufacturerSerialNumber).ToList();

                var manufacturerSerialNumber = from a in UnitWork.Find<OITL>(null)
                                               join b in UnitWork.Find<ITL1>(null) on new { a.LogEntry, a.ItemCode } equals new { b.LogEntry, b.ItemCode } into ab
                                               from b in ab.DefaultIfEmpty()
                                               join c in UnitWork.Find<OSRN>(null) on new { b.ItemCode, SysNumber = b.SysNumber.Value } equals new { c.ItemCode, c.SysNumber } into bc
                                               from c in bc.DefaultIfEmpty()
                                               where (a.DocType == 15 || a.DocType == 59) && MnfSerials.Contains(c.MnfSerial)
                                               select new { c.MnfSerial, a.DocEntry, a.BaseEntry, a.DocType, a.CreateDate, a.BaseType };
                #region 暂时废弃
                //var Equipments = from a in manufacturerSerialNumber
                //                 join b in UnitWork.Find<ODLN>(null) on a.DocEntry equals b.DocEntry into ab
                //                 from b in ab.DefaultIfEmpty()
                //                 select new { a.DocEntry, a.MnfSerial };
                //var EquipmentList = await Equipments.ToListAsync();
                //  var Equipments = await UnitWork.Query<SysEquipmentColumn>(@$"SELECT d.DocEntry,c.MnfSerial
                //      FROM oitl a left join itl1 b
                //      on a.LogEntry = b.LogEntry and a.ItemCode = b.ItemCode 
                //      left join osrn c on b.ItemCode = c.ItemCode and b.SysNumber = c.SysNumber
                //left join odln d on a.DocEntry=d.DocEntry
                //      where a.DocType =15 and c.MnfSerial in ({MnfSerialStr.ToString().Substring(0, MnfSerialStr.Length - 1)})").Select(s => new SysEquipmentColumn { MnfSerial = s.MnfSerial, DocEntry = s.DocEntry }).ToListAsync();

                //var DocEntryIds = Equipments.Select(e => e.DocEntry).ToList();

                //var buyopors = from a in UnitWork.Find<buy_opor>(null)
                //               join b in UnitWork.Find<sale_transport>(null) on a.DocEntry equals b.Buy_DocEntry
                //               where DocEntryIds.Contains(b.Base_DocEntry) && b.Base_DocType == 24
                //               select new { a, b };
                #endregion

                var MnfSerialList = await manufacturerSerialNumber.ToListAsync();
                var docdate = await manufacturerSerialNumber.Where(m => m.BaseType == 17).ToListAsync();
                var SalesOrderIds = docdate.Select(d => d.BaseEntry).ToList();
                var SalesOrderWarrantyDates = await UnitWork.Find<SalesOrderWarrantyDate>(s => SalesOrderIds.Contains(s.SalesOrderId) && s.IsPass == true).ToListAsync();
                var IsProtecteds = docdate.Select(e => new
                {
                    MnfSerial = e.MnfSerial,
                    DocDate = SalesOrderWarrantyDates.Where(s => s.SalesOrderId.Equals(e.BaseEntry)).Count() > 0 ? SalesOrderWarrantyDates.Where(s => s.SalesOrderId.Equals(e.BaseEntry)).FirstOrDefault()?.WarrantyPeriod : Convert.ToDateTime(e.CreateDate).AddMonths(13)
                }).ToList();
                #endregion
                List<QuotationProduct> quotationProducts = new List<QuotationProduct>();
                if (!string.IsNullOrWhiteSpace(request.QuotationId.ToString()))
                {
                    quotationProducts = await UnitWork.Find<QuotationProduct>(q => q.QuotationId == request.QuotationId).ToListAsync();
                }
                result.Data = ServiceWorkOrderList.Select(s => new ProductCodeListResp
                {
                    SalesOrder = MnfSerialList.Where(m => m.MnfSerial.Equals(s.ManufacturerSerialNumber) && m.BaseType == 17)?.Max(m => m.BaseEntry),
                    ProductionOrder = MnfSerialList.Where(m => m.MnfSerial.Equals(s.ManufacturerSerialNumber) && m.BaseType == 202)?.Max(m => m.BaseEntry),
                    ManufacturerSerialNumber = s.ManufacturerSerialNumber,
                    MaterialCode = s.MaterialCode,
                    MaterialDescription = s.MaterialDescription,
                    IsProtected = IsProtecteds.Where(i => i.MnfSerial.Equals(s.ManufacturerSerialNumber)).FirstOrDefault()?.DocDate > DateTime.Now ? true : false,
                    DocDate = IsProtecteds.Where(i => i.MnfSerial.Equals(s.ManufacturerSerialNumber)).OrderByDescending(s => s.DocDate).FirstOrDefault()?.DocDate,
                    FromTheme = s.FromTheme,
                    WarrantyTime = quotationProducts.Where(q => q.ProductCode.Equals(s.ManufacturerSerialNumber)).FirstOrDefault()?.WarrantyTime
                }).OrderBy(s => s.MaterialCode).ToList();
            }
            result.Count = ServiceWorkOrderList.Count();
            return result;
        }

        /// <summary>
        /// 获取物料列表
        /// </summary>
        public async Task<TableData> GetMaterialCodeList(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var Equipments = await EquipmentList(request);
            var quotations = await UnitWork.Find<Quotation>(q => q.ServiceOrderId.Equals(request.ServiceOrderId)).Include(q => q.QuotationProducts).ThenInclude(q => q.QuotationMaterials).ToListAsync();
            if (quotations != null && quotations.Count > 0)
            {
                List<QuotationMaterial> quotationMaterials = new List<QuotationMaterial>();
                quotations.ForEach(q =>
                    q.QuotationProducts.Where(p => p.ProductCode.Equals(request.ManufacturerSerialNumbers)).ForEach(p =>
                        quotationMaterials.AddRange(p.QuotationMaterials.ToList())
                    )
                );
                Equipments.ForEach(e =>
                      e.Quantity = e.Quantity - quotationMaterials.Where(q => q.MaterialCode.Equals(e.ItemCode)).Sum(q => q.Count)
                );
            }
            Equipments = Equipments.Where(e => e.Quantity > 0).ToList();
            var ItemCodes = Equipments.Select(e => e.ItemCode).ToList();
            var MaterialPrices = await UnitWork.Find<MaterialPrice>(m => ItemCodes.Contains(m.MaterialCode)).ToListAsync();

            //var categoryList = await UnitWork.Find<Category>(c => c.TypeId.Equals("SYS_WarehouseMaterial")).Select(c=>c.Name).ToListAsync();
            var EquipmentsList = Equipments.Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToList();
            EquipmentsList.ForEach(e =>
            {
                e.MnfSerial = request.ManufacturerSerialNumbers;
                var Prices = MaterialPrices.Where(m => m.MaterialCode.Equals(e.ItemCode)).FirstOrDefault();
                //4.0存在物料价格，取4.0的价格为售后结算价，销售价取售后结算价*销售价倍数，不存在就当前进货价*1.2 为售后结算价。销售价为售后结算价*3
                if (Prices != null)
                {
                    e.UnitPrice = Prices?.SettlementPrice == null || Prices?.SettlementPrice <= 0 ? e.lastPurPrc * Prices?.SettlementPriceModel : Prices?.SettlementPrice;
                    //var s = e.UnitPrice.ToDouble().ToString();
                    //if (s.IndexOf(".") > 0)
                    //{
                    //    s = s.Substring(s.IndexOf("."), s.Length - s.IndexOf("."));
                    //    if (s.Length > 1)
                    //    {
                    //        int lengeth = s.Substring(1, s.Length - 1).Length;
                    //        if (lengeth > 3) e.UnitPrice = e.UnitPrice + 0.005M;
                    //    }
                    //}
                    e.UnitPrice = decimal.Parse(e.UnitPrice.ToString("#0.0000"));
                    e.lastPurPrc = e.UnitPrice * Prices.SalesMultiple;
                }
                else
                {
                    e.UnitPrice = e.lastPurPrc * 1.2M;
                    //var s = e.UnitPrice.ToDouble().ToString();
                    //if (s.IndexOf(".") > 0)
                    //{
                    //    s = s.Substring(s.IndexOf("."), s.Length - s.IndexOf("."));
                    //    if (s.Length > 1)
                    //    {
                    //        int lengeth = s.Substring(1, s.Length - 1).Length;
                    //        if (lengeth > 3) e.UnitPrice = e.UnitPrice + 0.005M;
                    //    }
                    //}
                    e.UnitPrice = decimal.Parse(e.UnitPrice.ToString("#0.0000"));
                    e.lastPurPrc = e.UnitPrice * 3;
                }

            });

            result.Data = EquipmentsList;
            result.Count = Equipments.Count();
            return result;
        }

        /// <summary>
        /// 通用获取物料列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<List<SysEquipmentColumn>> EquipmentList(QueryQuotationListReq request)
        {
            SqlParameter[] parameter = new SqlParameter[]
            {
               new SqlParameter("ManufacturerSerialNumbers", request.ManufacturerSerialNumbers),
               new SqlParameter("WhsCode", request.WhsCode)
            };
            var Equipments = await UnitWork.Query<SysEquipmentColumn>(@$"select a.ItemCode,b.MnfSerial,c.ItemName,c.BuyUnitMsr,d.OnHand, d.WhsCode,a.BaseQty as Quantity ,c.lastPurPrc from WOR1 a 
						join (SELECT a.BaseEntry,c.MnfSerial,a.BaseType
            FROM oitl a left join itl1 b
            on a.LogEntry = b.LogEntry and a.ItemCode = b.ItemCode 
            left join osrn c on b.ItemCode = c.ItemCode and b.SysNumber = c.SysNumber
            where a.DocType in (15, 59) and c.MnfSerial = @ManufacturerSerialNumbers and a.BaseType=202) b on a.docentry = b.BaseEntry	
						join OITM c on a.itemcode = c.itemcode
						join OITW d on a.itemcode=d.itemcode 
						where d.WhsCode= @WhsCode", parameter).WhereIf(!string.IsNullOrWhiteSpace(request.PartCode), s => s.ItemCode.Contains(request.PartCode))
                        .WhereIf(!string.IsNullOrWhiteSpace(request.PartDescribe), s => request.PartDescribe.Contains(s.ItemName))
                        .Select(s => new SysEquipmentColumn { ItemCode = s.ItemCode, MnfSerial = s.MnfSerial, ItemName = s.ItemName, BuyUnitMsr = s.BuyUnitMsr, OnHand = s.OnHand, WhsCode = s.WhsCode, Quantity = s.Quantity, lastPurPrc = s.lastPurPrc }).ToListAsync();

            if (Equipments == null || Equipments.Count() <= 0)
            {
                request.MaterialCode = request.MaterialCode.Replace("'", "''");
                parameter = new SqlParameter[]
                {
                   new SqlParameter("MaterialCode", request.MaterialCode),
                   new SqlParameter("WhsCode", request.WhsCode)
                };
                Equipments = await UnitWork.Query<SysEquipmentColumn>(@$"select a.* ,c.lastPurPrc from (select a.Father as MnfSerial,a.Code as ItemCode,a.U_Desc as ItemName,a.U_DUnit as BuyUnitMsr,b.OnHand,b.WhsCode,a.Quantity
                        from ITT1 a join OITW b on a.Code=b.ItemCode  where a.Father=@MaterialCode and b.WhsCode=@WhsCode) a join OITM c on c.ItemCode=a.ItemCode", parameter)
                    .WhereIf(!string.IsNullOrWhiteSpace(request.PartCode), s => s.ItemCode.Contains(request.PartCode))
                    .WhereIf(!string.IsNullOrWhiteSpace(request.PartDescribe), s => request.PartDescribe.Contains(s.ItemName))
                    .Select(s => new SysEquipmentColumn { ItemCode = s.ItemCode, MnfSerial = s.MnfSerial, ItemName = s.ItemName, BuyUnitMsr = s.BuyUnitMsr, OnHand = s.OnHand, WhsCode = s.WhsCode, Quantity = s.Quantity, lastPurPrc = s.lastPurPrc }).ToListAsync();
            }
            if (request.IsWarranty == null || (bool)request.IsWarranty == false)
            {
                var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ShieldingMaterials")).Select(u => u.Name).ToListAsync();

                Equipments = Equipments.Where(e => !CategoryList.Contains(e.ItemCode)).ToList();
            }
            else
            {
                Equipments = Equipments.Where(e => e.ItemCode.Equals("S111-SERVICE-GSF")).ToList();
            }
            return Equipments;
        }

        /// <summary>
        /// 查询物料剩余库存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetMaterialCodeOnHand(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var materialCodeOnHand = (await UnitWork.Find<OITW>(o => o.ItemCode.Equals(request.MaterialCode) && o.WhsCode.Equals(request.WhsCode)).FirstOrDefaultAsync())?.OnHand;
            result.Data = new { OnHand = materialCodeOnHand };
            return result;
        }

        /// <summary>
        /// 获取报价单详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetDetails(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var Quotations = await GeneralDetails((int)request.QuotationId, request.IsUpdate);
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(Quotations.ServiceOrderId)).Include(s => s.ServiceWorkOrders).FirstOrDefaultAsync();
            var CustomerInformation = await UnitWork.Find<crm_ocrd>(o => o.CardCode.Equals(ServiceOrders.TerminalCustomerId)).Select(o => new { frozenFor = o.frozenFor == "N" ? "正常" : "冻结" }).FirstOrDefaultAsync();
            var QuotationMergeMaterials = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(request.QuotationId)).ToListAsync();
            QuotationMergeMaterials = QuotationMergeMaterials.OrderBy(q => q.MaterialCode).ToList();
            Quotations.ServiceRelations = (await UnitWork.Find<User>(u => u.Id.Equals(Quotations.CreateUserId)).FirstOrDefaultAsync()).ServiceRelations;
            var ocrds = await UnitWork.Find<crm_ocrd>(o => ServiceOrders.TerminalCustomerId.Equals(o.CardCode)).FirstOrDefaultAsync();
            var result = new TableData();
            if (Quotations.Status == 2)
            {
                var ExpressageList = await UnitWork.Find<Expressage>(e => e.QuotationId.Equals(Quotations.Id)).Include(e => e.ExpressagePicture).Include(e => e.LogisticsRecords).ToListAsync();
                List<LogisticsRecord> LogisticsRecords = new List<LogisticsRecord>();

                var fileids = new List<string>();
                foreach (var item in ExpressageList)
                {
                    fileids.AddRange(item.ExpressagePicture.Select(p => p.PictureId).ToList());
                    LogisticsRecords.AddRange(item.LogisticsRecords.ToList());
                }

                var files = await UnitWork.Find<UploadFile>(f => fileids.Contains(f.Id)).ToListAsync();
                var MergeMaterials = from a in QuotationMergeMaterials
                                     join b in LogisticsRecords on a.Id equals b.QuotationMaterialId
                                     select new { a, b };

                var Expressages = ExpressageList.Select(e => new
                {
                    ExpressagePicture = e.ExpressagePicture.Select(p => new
                    {
                        p.PictureId,
                        p.Id,
                        p.ExpressageId,
                        FileName = files.FirstOrDefault(f => f.Id.Equals(p.PictureId))?.FileName,
                        FileType = files.FirstOrDefault(f => f.Id.Equals(p.PictureId))?.FileType,
                    }),
                    e.ExpressInformation,
                    e.ExpressNumber,
                    e.Id,
                    e.Freight,
                    e.QuotationId,
                    e.Remark,
                    e.ReturnNoteId,
                    LogisticsRecords = MergeMaterials.Where(m => m.b.ExpressageId.Equals(e.Id)).Select(m => new
                    {
                        m.a.MaterialCode,
                        m.a.MaterialDescription,
                        m.a.Count,
                        m.a.Unit,
                        m.a.SentQuantity,
                        m.b.Quantity,
                        m.a.WhsCode
                    }).ToList()
                }).ToList();
                result.Data = new
                {
                    Balance = ocrds?.Balance,
                    Expressages,
                    Quotations = Quotations,
                    QuotationMergeMaterials,
                    ServiceOrders,
                    CustomerInformation
                };
            }
            else
            {
                result.Data = new
                {
                    Balance = ocrds?.Balance,
                    Quotations = Quotations,
                    QuotationMergeMaterials,
                    ServiceOrders,
                    CustomerInformation
                };
            }
            return result;
        }

        /// <summary>
        /// 报价单详情操作
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <param name="IsUpdate"></param>
        /// <returns></returns>
        public async Task<AddOrUpdateQuotationReq> GeneralDetails(int QuotationId, bool? IsUpdate)
        {
            var Quotations = await UnitWork.Find<Quotation>(q => q.Id == QuotationId).Include(q => q.QuotationPictures).Include(q => q.QuotationProducts).ThenInclude(q => q.QuotationMaterials).ThenInclude(q => q.QuotationMaterialPictures).Include(q => q.QuotationOperationHistorys).FirstOrDefaultAsync();
            var quotationsMap = Quotations.MapTo<AddOrUpdateQuotationReq>();
            List<string> materialCodes = new List<string>();
            List<string> WhsCode = new List<string>();
            Quotations.QuotationProducts.ForEach(q =>
            {
                WhsCode.AddRange(q.QuotationMaterials.Select(m => m.WhsCode).ToList());
                materialCodes.AddRange(q.QuotationMaterials.Select(m => m.MaterialCode).ToList());
            });
            var ItemCodes = await UnitWork.Find<OITW>(o => materialCodes.Contains(o.ItemCode) && WhsCode.Contains(o.WhsCode)).Select(o => new { o.ItemCode, o.WhsCode, o.OnHand }).ToListAsync();
            List<QuotationMaterialReq> quotationMaterials = new List<QuotationMaterialReq>();
            if (IsUpdate != null && (bool)IsUpdate)
            {
                var oITMS = await UnitWork.Find<OITM>(o => materialCodes.Contains(o.ItemCode)).Select(o => new QuotationMaterialReq { MaterialCode = o.ItemCode, SalesPrice = o.LastPurPrc }).ToListAsync();
                var MaterialPrices = await UnitWork.Find<MaterialPrice>(m => materialCodes.Contains(m.MaterialCode)).ToListAsync();
                oITMS.ForEach(o =>
                {
                    var Prices = MaterialPrices.Where(m => m.MaterialCode.Equals(o.MaterialCode)).FirstOrDefault();
                    //4.0存在物料价格，取4.0的价格为售后结算价，销售价取售后结算价*销售价倍数，不存在就当前进货价*1.2 为售后结算价。销售价为售后结算价*3
                    if (Prices != null)
                    {
                        o.UnitPrice = Prices?.SettlementPrice == null || Prices?.SettlementPrice <= 0 ? o.SalesPrice * Prices?.SettlementPriceModel : Prices?.SettlementPrice;

                        o.UnitPrice = decimal.Parse(o.UnitPrice.ToString("#0.0000"));
                        o.SalesPrice = o.UnitPrice * Prices.SalesMultiple;
                    }
                    else
                    {
                        o.UnitPrice = o.SalesPrice * 1.2M;
                        o.UnitPrice = decimal.Parse(o.UnitPrice.ToString("#0.0000"));
                        o.SalesPrice = o.UnitPrice * 3;
                    }
                });
                quotationMaterials.AddRange(oITMS.ToList());
            }
            quotationsMap.QuotationProducts.ForEach(p =>
                p.QuotationMaterials.ForEach(m =>
                {
                    m.WhsCode = m.WhsCode;
                    m.WarehouseQuantity = ItemCodes.Where(i => i.ItemCode.Equals(m.MaterialCode) && i.WhsCode.Equals(m.WhsCode)).FirstOrDefault()?.OnHand;
                    m.TotalPrice = m.TotalPrice == 0 && m.MaterialType != "3" && m.MaterialType != "4" ? decimal.Parse(Convert.ToDecimal((m.UnitPrice * 3 * (m.Discount / 100) * m.Count)).ToString("#0.00")) : m.TotalPrice;
                    m.SalesPrice = m.SalesPrice == 0 && m.MaterialType != "3" ? decimal.Parse(Convert.ToDecimal(m.UnitPrice * 3).ToString("#0.00")) : m.SalesPrice;
                    if (m.DiscountPrices < 0) m.DiscountPrices = m.SalesPrice == 0 && m.MaterialType != "3" && m.MaterialType != "3" ? decimal.Parse(Convert.ToDecimal(m.UnitPrice * 3 * (m.Discount / 100)).ToString("#0.00")) : decimal.Parse(Convert.ToDecimal(m.SalesPrice * (m.Discount / 100)).ToString("#0.00"));
                    if (IsUpdate != null && (bool)IsUpdate) m.UnitPrice = quotationMaterials.Where(q => q.MaterialCode.Equals(m.MaterialCode)).FirstOrDefault()?.UnitPrice;
                    if (IsUpdate != null && (bool)IsUpdate) m.SalesPrice = quotationMaterials.Where(q => q.MaterialCode.Equals(m.MaterialCode)).FirstOrDefault()?.SalesPrice;
                }
                )
            );

            var SecondId = (await UnitWork.Find<Relevance>(r => r.FirstId.Equals(quotationsMap.CreateUserId) && r.Key.Equals(Define.USERORG)).FirstOrDefaultAsync()).SecondId;
            quotationsMap.OrgName = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => o.Id.Equals(SecondId)).Select(o => o.Name).FirstOrDefaultAsync();

            List<QuotationMaterialReq> QuotationMergeMaterial = new List<QuotationMaterialReq>();
            List<ProductCodeListResp> serialNumberList = (await GetSerialNumberList(new QueryQuotationListReq { ServiceOrderId = quotationsMap.ServiceOrderId, CreateUserId = quotationsMap.CreateUserId })).Data;
            var count = 0;
            var isBool = (quotationsMap.ServiceChargeJH != null && quotationsMap.ServiceChargeJH > 0) || (quotationsMap.ServiceChargeJHCost != null && quotationsMap.ServiceChargeJHCost > 0);
            isBool = isBool == true ? true : (quotationsMap.ServiceChargeSM != null && quotationsMap.ServiceChargeSM > 0) || (quotationsMap.ServiceChargeSMCost != null && quotationsMap.ServiceChargeSMCost > 0);
            isBool = isBool == true ? true : (quotationsMap.TravelExpense != null && quotationsMap.TravelExpense > 0) || (quotationsMap.TravelExpenseCost != null && quotationsMap.TravelExpenseCost > 0);
            if (isBool && (IsUpdate == null || IsUpdate == false))
            {
                var productCodeList = quotationsMap.QuotationProducts.Select(q => q.ProductCode).ToList();
                var products = serialNumberList.Where(s => !productCodeList.Contains(s.ManufacturerSerialNumber)).Select(s => new QuotationProductReq
                {
                    MaterialCode = s.MaterialCode,
                    ProductCode = s.ManufacturerSerialNumber,
                    IsProtected = s.IsProtected,
                    MaterialDescription = s.MaterialDescription,
                    WarrantyExpirationTime = s.DocDate,
                    FromTheme = s.FromTheme,
                    QuotationMaterials = new List<QuotationMaterialReq>()
                }).ToList();
                quotationsMap.QuotationProducts.AddRange(products);
                count = quotationsMap.QuotationProducts.Count();
                if ((quotationsMap.ServiceChargeJH != null && quotationsMap.ServiceChargeJH > 0) || (quotationsMap.ServiceChargeJHCost != null && quotationsMap.ServiceChargeJHCost > 0))
                {
                    QuotationMergeMaterial.Add(new QuotationMaterialReq
                    {
                        Id = Guid.NewGuid().ToString(),
                        MaterialCode = "S111-SERVICE-GSF-JH",
                        MaterialDescription = "寄回维修费 20210518",
                        Unit = "PCS",
                        UnitPrice = quotationsMap.ServiceChargeJHCost,
                        SalesPrice = quotationsMap.ServiceChargeJH,
                        Count = quotationsMap.ServiceChargeManHourJH != null ? Convert.ToDecimal(quotationsMap.ServiceChargeManHourJH) / count : 1,
                        TotalPrice = quotationsMap.ServiceChargeManHourJH != null ? (Convert.ToDecimal(quotationsMap.ServiceChargeManHourJH) / count) * quotationsMap.ServiceChargeJH : quotationsMap.ServiceChargeJH / count,
                        Discount = 100,
                        DiscountPrices = quotationsMap.ServiceChargeJH,
                        QuotationMaterialPictures = new List<QuotationMaterialPictureReq>(),
                        MaterialType = quotationsMap.IsMaterialType == "3" ? "4" : "2"
                    });
                }
                if ((quotationsMap.ServiceChargeSM != null && quotationsMap.ServiceChargeSM > 0) || (quotationsMap.ServiceChargeSMCost != null && quotationsMap.ServiceChargeSMCost > 0))
                {
                    QuotationMergeMaterial.Add(new QuotationMaterialReq
                    {
                        Id = Guid.NewGuid().ToString(),
                        MaterialCode = "S111-SERVICE-GSF-SM",
                        MaterialDescription = "上门维修费 20210518",
                        Unit = "PCS",
                        UnitPrice = quotationsMap.ServiceChargeSMCost,
                        SalesPrice = quotationsMap.ServiceChargeSM,
                        Count = quotationsMap.ServiceChargeManHourSM != null ? Convert.ToDecimal(quotationsMap.ServiceChargeManHourSM) / count : 1,
                        TotalPrice = quotationsMap.ServiceChargeManHourSM != null ? (Convert.ToDecimal(quotationsMap.ServiceChargeManHourSM) / count) * quotationsMap.ServiceChargeSM : quotationsMap.ServiceChargeSM / count,
                        Discount = 100,
                        QuotationMaterialPictures = new List<QuotationMaterialPictureReq>(),
                        DiscountPrices = quotationsMap.ServiceChargeSM,
                        MaterialType = quotationsMap.IsMaterialType == "3" ? "4" : "2"
                    });
                }
                if ((quotationsMap.TravelExpense != null && quotationsMap.TravelExpense > 0) || (quotationsMap.TravelExpenseCost != null && quotationsMap.TravelExpenseCost > 0))
                {
                    QuotationMergeMaterial.Add(new QuotationMaterialReq
                    {
                        Id = Guid.NewGuid().ToString(),
                        MaterialCode = "S111-SERVICE-CLF",
                        MaterialDescription = "差旅费",
                        Unit = "PCS",
                        UnitPrice = quotationsMap.TravelExpenseCost,
                        SalesPrice = quotationsMap.TravelExpense,
                        Count = quotationsMap.TravelExpenseManHour != null ? Convert.ToDecimal(quotationsMap.TravelExpenseManHour) / count : 1,
                        TotalPrice = quotationsMap.TravelExpenseManHour != null ? (Convert.ToDecimal(quotationsMap.TravelExpenseManHour) / count) * quotationsMap.TravelExpense : quotationsMap.TravelExpense / count,
                        Discount = 100,
                        QuotationMaterialPictures = new List<QuotationMaterialPictureReq>(),
                        DiscountPrices = quotationsMap.TravelExpense,
                        MaterialType = quotationsMap.IsMaterialType == "3" ? "4" : "2"
                    });

                }

            }

            quotationsMap.QuotationProducts.ForEach(q =>
            {
                q.FromTheme = serialNumberList.Where(s => s.ManufacturerSerialNumber.Equals(q.ProductCode)).FirstOrDefault()?.FromTheme;
                q.QuotationMaterials.AddRange(QuotationMergeMaterial.ToList());
                q.QuotationMaterials = q.QuotationMaterials.OrderBy(m => m.MaterialCode).ToList();
            });
            quotationsMap.QuotationOperationHistorys = quotationsMap.QuotationOperationHistorys.Where(q => q.ApprovalStage != "-1").OrderBy(o => o.CreateTime).ThenByDescending(o => o.Action).ToList();
            var operationHistorys = Quotations.QuotationOperationHistorys.Select(q => new OperationHistoryResp { ApprovalResult = q.ApprovalResult, ApprovalStage = q.ApprovalStage, Content = q.Action, CreateTime = q.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"), Remark = q.Remark, CreateUserName = q.CreateUser, IntervalTime = q.IntervalTime.ToString() }).ToList();
            quotationsMap.FlowPathResp = await _flowInstanceApp.FlowPathRespList(operationHistorys, Quotations.FlowInstanceId);
            return quotationsMap;
        }

        /// <summary>
        /// 查询报价单详情物料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetDetailsMaterial(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var Quotations = await UnitWork.Find<Quotation>(q => q.Id.Equals(request.QuotationId)).Include(q => q.QuotationPictures).Include(q => q.QuotationProducts).ThenInclude(p => p.QuotationMaterials).FirstOrDefaultAsync();
            var result = new TableData();
            if (!string.IsNullOrWhiteSpace(request.MaterialCode))
            {
                Quotations.QuotationProducts = Quotations.QuotationProducts.Where(q => q.MaterialCode.Contains(request.MaterialCode)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(request.ManufacturerSerialNumbers))
            {
                Quotations.QuotationProducts = Quotations.QuotationProducts.Where(q => q.ProductCode.Contains(request.ManufacturerSerialNumbers)).ToList();
            }
            result.Data = Quotations.QuotationProducts;

            return result;
        }

        /// <summary>
        /// 按条件查询所有物料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> MaterialCodeList(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //限制查不出bom的数据，暂时屏蔽
            //var equipmentList = await EquipmentList(request);
            //var codeList = equipmentList.Select(e => e.ItemCode).ToList();
            var result = new TableData();
            var query = from a in UnitWork.Find<OITM>(null).WhereIf(!string.IsNullOrWhiteSpace(request.PartCode), q => q.ItemCode.Contains(request.PartCode))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.PartDescribe), q => q.ItemName.Contains(request.PartDescribe))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.ReplacePartCode), q => !q.ItemCode.Equals(request.ReplacePartCode))
                            //.WhereIf(codeList.Count > 0, q => !codeList.Contains(q.ItemCode))
                        join b in UnitWork.Find<OITW>(null) on a.ItemCode equals b.ItemCode into ab
                        from b in ab.DefaultIfEmpty()
                        where b.WhsCode == request.WhsCode
                        select new SysEquipmentColumn { ItemCode = a.ItemCode, ItemName = a.ItemName, lastPurPrc = a.LastPurPrc, BuyUnitMsr = a.SalUnitMsr, OnHand = b.OnHand, WhsCode = b.WhsCode };
            //退料获取可替换物料编码
            if (!string.IsNullOrWhiteSpace(request.ItemCode))
            {
                var code = request.ItemCode.Substring(0, request.ItemCode.IndexOf("-") + 1);
                query = query.Where(q => q.ItemCode.Substring(0, q.ItemCode.IndexOf("-") + 1).Equals(code) && !q.ItemCode.Equals(request.ItemCode));
            }

            //是否延保
            if (request.IsWarranty != null && (bool)request.IsWarranty)
            {
                query = query.Where(e => e.ItemCode.Equals("S111-SERVICE-GSF"));
            }
            else
            {
                var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ShieldingMaterials")).Select(u => u.Name).ToListAsync();
                query = query.Where(e => !CategoryList.Contains(e.ItemCode));
            }
            result.Count = await query.CountAsync();
            var Equipments = await query.Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            var ItemCodes = Equipments.Select(e => e.ItemCode).ToList();
            var MaterialPrices = await UnitWork.Find<MaterialPrice>(m => ItemCodes.Contains(m.MaterialCode)).ToListAsync();
            Equipments.ForEach(e =>
            {
                var Prices = MaterialPrices.Where(m => m.MaterialCode.Equals(e.ItemCode)).FirstOrDefault();
                //4.0存在物料价格，取4.0的价格为售后结算价，不存在就当前进货价*1.2 为售后结算价。销售价均为售后结算价*3
                if (Prices != null)
                {
                    e.UnitPrice = Prices?.SettlementPrice == null || Prices?.SettlementPrice <= 0 ? e.lastPurPrc * Prices?.SettlementPriceModel : Prices?.SettlementPrice;
                    //var s = e.UnitPrice.ToDouble().ToString();
                    //if (s.IndexOf(".") > 0)
                    //{
                    //    s = s.Substring(s.IndexOf("."), s.Length - s.IndexOf("."));
                    //    if (s.Length > 1)
                    //    {
                    //        int lengeth = s.Substring(1, s.Length - 1).Length;
                    //        if (lengeth > 3) e.UnitPrice = e.UnitPrice + 0.005M;
                    //    }
                    //}
                    e.UnitPrice = decimal.Parse(e.UnitPrice.ToString("#0.0000"));
                    e.lastPurPrc = e.UnitPrice * Prices?.SalesMultiple;
                }
                else
                {
                    e.UnitPrice = e.lastPurPrc * 1.2M;
                    //var s = e.UnitPrice.ToDouble().ToString();
                    //if (s.IndexOf(".") > 0)
                    //{
                    //    s = s.Substring(s.IndexOf("."), s.Length - s.IndexOf("."));
                    //    if (s.Length > 1)
                    //    {
                    //        int lengeth = s.Substring(1, s.Length - 1).Length;
                    //        if (lengeth > 3) e.UnitPrice = e.UnitPrice + 0.005M;
                    //    }
                    //}
                    e.UnitPrice = decimal.Parse(e.UnitPrice.ToString("#0.0000"));
                    e.lastPurPrc = e.UnitPrice * 3;
                }

            });
            result.Data = Equipments.ToList();
            return result;
        }

        /// <summary>
        /// 获取待合并报价单
        /// </summary>
        /// <param name="ServiceOrderId"></param>
        /// <returns></returns>
        public async Task<TableData> GetUnreadQuotations(int ServiceOrderId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var Quotations = await UnitWork.Find<Quotation>(q => q.ServiceOrderId.Equals(ServiceOrderId) && q.ErpOrApp == 2).ToListAsync();
            var QuotationIds = Quotations.Select(q => q.Id).ToList();
            var QuotationProducts = await UnitWork.Find<QuotationProduct>(q => QuotationIds.Contains((int)q.QuotationId)).Include(q => q.QuotationMaterials).ToListAsync();
            Quotations.ForEach(q => q.QuotationProducts = QuotationProducts.Where(p => p.QuotationId.Equals(q.Id)).ToList());
            result.Data = new
            {
                Quotations,
            };

            return result;
        }

        /// <summary>
        /// 获取该服务单所有报价单零件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetQuotationMaterialCode(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(request.AppId));
            }
            var result = new TableData();
            var QuotationIds = await UnitWork.Find<Quotation>(q => q.ServiceOrderId.Equals(request.ServiceOrderId) && q.CreateUserId.Equals(loginUser.Id)).Select(q => q.Id).ToListAsync();

            var QuotationMergeMaterials = await UnitWork.Find<QuotationMergeMaterial>(q => QuotationIds.Contains((int)q.QuotationId) && q.MaterialType == 1).ToListAsync();
            //获取当前服务单所有退料明细汇总
            var query = from a in UnitWork.Find<ReturnnoteMaterial>(null)
                        join b in UnitWork.Find<ReturnNote>(null) on a.ReturnNoteId equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        where b.ServiceOrderId == request.ServiceOrderId && a.Count > 0
                        select new { a.QuotationMaterialId, a.Count };
            var returnMaterials = (await query.ToListAsync()).GroupBy(g => g.QuotationMaterialId).Select(s => new { Qty = s.Sum(s => s.Count), Id = s.Key }).ToList();
            List<ReturnMaterialListResp> data = new List<ReturnMaterialListResp>();
            foreach (var item in QuotationMergeMaterials)
            {
                var res = item.MapTo<ReturnMaterialListResp>();
                int everQty = (int)(returnMaterials.Where(w => w.Id == item.Id).FirstOrDefault() == null ? 0 : returnMaterials.Where(w => w.Id == item.Id).FirstOrDefault()?.Qty);
                res.SurplusQty = (int)item.Count - (returnMaterials.Where(w => w.Id == item.Id).FirstOrDefault() == null ? 0 : (int)returnMaterials.Where(w => w.Id == item.Id).FirstOrDefault().Qty);
                data.Add(res);
            }
            result.Data = data;
            return result;
        }

        /// <summary>
        /// 新增报价单
        /// </summary>
        /// <param name="obj"></param>
        public async Task<string> Add(AddOrUpdateQuotationReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppId));
            }
            var Message = await Condition(obj);
            var QuotationObj = await CalculatePrice(obj);
            var dbContext = UnitWork.GetDbContext<Quotation>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    QuotationObj.QuotationProducts.ForEach(q => q.QuotationMaterials.ForEach(m =>
                    {
                        m.Id = Guid.NewGuid().ToString();
                        if (m.QuotationMaterialPictures != null && m.QuotationMaterialPictures.Count() > 0)
                        {
                            m.QuotationMaterialPictures.ForEach(p => p.Id = Guid.NewGuid().ToString());
                        }
                    }));
                    QuotationObj.CreateTime = DateTime.Now;
                    QuotationObj.CreateUser = loginUser.Name;
                    QuotationObj.CreateUserId = loginUser.Id;
                    QuotationObj.Status = 1;
                    QuotationObj.QuotationStatus = 3;
                    QuotationObj.PrintWarehouse = 1;
                    QuotationObj.UpDateTime = DateTime.Now;
                    QuotationObj = await UnitWork.AddAsync<Quotation, int>(QuotationObj);
                    await UnitWork.SaveAsync();
                    if (!obj.IsDraft)
                    {
                        QuotationObj.QuotationStatus = 3.1M;
                        await MergeMaterial(QuotationObj);
                        #region 创建审批流程
                        var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("物料报价单"));
                        var afir = new AddFlowInstanceReq();
                        afir.SchemeId = mf.FlowSchemeId;
                        afir.FrmType = 2;
                        afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();

                        var IsProtected = QuotationObj.IsProtected != null && QuotationObj.IsProtected == true ? "1" : "2";
                        string IsWarranty = null;
                        if (QuotationObj.IsMaterialType == 4)
                        {
                            IsWarranty = "1";
                        }
                        else
                        {
                            IsWarranty = "2";
                        }
                        afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\",\"IsProtected\":\"" + IsProtected + "\",\"IsWarranty\":\"" + IsWarranty + "\",\"WarrantyType\":\"" + QuotationObj.WarrantyType + "\"}";
                        afir.CustomName = $"物料报价单" + DateTime.Now;
                        QuotationObj.FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                        #endregion
                        await UnitWork.AddAsync<QuotationOperationHistory>(new QuotationOperationHistory
                        {
                            Action = QuotationObj.ErpOrApp == 1 ? QuotationObj.CreateUser + "通过ERP提交审批" : QuotationObj.CreateUser + "通过APP提交审批",
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            CreateTime = DateTime.Now,
                            QuotationId = QuotationObj.Id,
                            ApprovalStage = "3"
                        });
                        await UnitWork.UpdateAsync<Quotation>(QuotationObj);
                        //增加全局待处理
                        var serviceOrederObj = await UnitWork.Find<ServiceOrder>(s => s.Id == obj.ServiceOrderId).FirstOrDefaultAsync();
                        await _workbenchApp.AddOrUpdate(new WorkbenchPending
                        {
                            OrderType = 1,
                            TerminalCustomer = serviceOrederObj.TerminalCustomer,
                            TerminalCustomerId = serviceOrederObj.TerminalCustomerId,
                            ServiceOrderId = serviceOrederObj.Id,
                            ServiceOrderSapId = (int)serviceOrederObj.U_SAP_ID,
                            UpdateTime = QuotationObj.UpDateTime,
                            Remark = QuotationObj.Remark,
                            FlowInstanceId = QuotationObj.FlowInstanceId,
                            TotalMoney = QuotationObj.TotalMoney,
                            Petitioner = loginUser.Name,
                            SourceNumbers = QuotationObj.Id,
                            PetitionerId = loginUser.Id,
                        });
                        await UnitWork.SaveAsync();
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("添加报价单失败。" + ex.Message);
                }
            }
            return Message;
        }

        /// <summary>
        /// 修改报价单
        /// </summary>
        /// <param name="obj"></param>
        public async Task<string> Update(AddOrUpdateQuotationReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppId));
            }
            var Message = await Condition(obj);
            var QuotationObj = await CalculatePrice(obj);
            var dbContext = UnitWork.GetDbContext<Quotation>();
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    #region 删除
                    await UnitWork.DeleteAsync<QuotationProduct>(q => q.QuotationId == QuotationObj.Id);
                    await UnitWork.DeleteAsync<QuotationMergeMaterial>(q => q.QuotationId.Equals(QuotationObj.Id));
                    await UnitWork.SaveAsync();
                    #endregion

                    #region 新增
                    if (QuotationObj.QuotationProducts != null && QuotationObj.QuotationProducts.Count > 0)
                    {
                        var QuotationProductMap = QuotationObj.QuotationProducts.MapToList<QuotationProduct>();
                        QuotationProductMap.ForEach(q =>
                        {
                            q.QuotationMaterials.ForEach(m =>
                            {
                                m.Id = Guid.NewGuid().ToString();
                                if (m.QuotationMaterialPictures != null && m.QuotationMaterialPictures.Count() > 0)
                                {
                                    m.QuotationMaterialPictures.ForEach(p => { p.Id = Guid.NewGuid().ToString(); });
                                }
                            });
                        });
                        await UnitWork.BatchAddAsync<QuotationProduct>(QuotationProductMap.ToArray());
                    }
                    await UnitWork.SaveAsync();

                    #endregion

                    if (obj.IsDraft)
                    {
                        await UnitWork.UpdateAsync<Quotation>(u => u.Id == QuotationObj.Id, u => new Quotation
                        {
                            QuotationStatus = 3,
                            CollectionAddress = QuotationObj.CollectionAddress,
                            ShippingAddress = QuotationObj.ShippingAddress,
                            DeliveryMethod = QuotationObj.DeliveryMethod,
                            InvoiceCompany = QuotationObj.InvoiceCompany,
                            TotalMoney = QuotationObj.TotalMoney,
                            TotalCostPrice = QuotationObj.TotalCostPrice,
                            Remark = QuotationObj.Remark,
                            IsDraft = QuotationObj.IsDraft,
                            IsProtected = QuotationObj.IsProtected,
                            TravelExpense = QuotationObj.TravelExpense,
                            Status = 1,
                            ServiceChargeJH = QuotationObj.ServiceChargeJH,
                            ServiceChargeSM = QuotationObj.ServiceChargeSM,
                            TaxRate = QuotationObj.TaxRate,
                            InvoiceCategory = QuotationObj.InvoiceCategory,
                            AcceptancePeriod = QuotationObj.AcceptancePeriod,
                            Prepay = QuotationObj.Prepay,
                            PaymentAfterWarranty = QuotationObj.PaymentAfterWarranty,
                            CashBeforeFelivery = QuotationObj.CashBeforeFelivery,
                            PayOnReceipt = QuotationObj.PayOnReceipt,
                            CollectionDA = QuotationObj.CollectionDA,
                            ShippingDA = QuotationObj.ShippingDA,
                            AcquisitionWay = QuotationObj.AcquisitionWay,
                            IsMaterialType = QuotationObj.IsMaterialType,
                            ServiceChargeManHourJH = QuotationObj.ServiceChargeManHourJH,
                            ServiceChargeManHourSM = QuotationObj.ServiceChargeManHourSM,
                            TravelExpenseManHour = QuotationObj.TravelExpenseManHour,
                            ServiceChargeJHCost = QuotationObj.ServiceChargeJHCost,
                            ServiceChargeSMCost = QuotationObj.ServiceChargeSMCost,
                            TravelExpenseCost = QuotationObj.TravelExpenseCost,
                            PrintWarehouse = 1,
                            MoneyMeans = QuotationObj.MoneyMeans,
                            UpDateTime = DateTime.Now
                            //todo:要修改的字段赋值
                        });
                        await UnitWork.SaveAsync();
                    }
                    else
                    {
                        await MergeMaterial(QuotationObj);
                        if (string.IsNullOrWhiteSpace(QuotationObj.FlowInstanceId))
                        {
                            #region 创建审批流程
                            var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("物料报价单"));
                            var afir = new AddFlowInstanceReq();
                            afir.SchemeId = mf.FlowSchemeId;
                            afir.FrmType = 2;
                            afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                            var IsProtected = QuotationObj.IsProtected != null && QuotationObj.IsProtected == true ? "1" : "2";
                            string IsWarranty = null;
                            if (QuotationObj.IsMaterialType == 4)
                            {
                                IsWarranty = "1";
                            }
                            else
                            {
                                IsWarranty = "2";
                            }
                            afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\",\"IsProtected\":\"" + IsProtected + "\",\"IsWarranty\":\"" + IsWarranty + "\",\"WarrantyType\":\"" + QuotationObj.WarrantyType + "\"}";
                            afir.CustomName = $"物料报价单" + DateTime.Now;
                            QuotationObj.FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                            #endregion
                        }
                        else
                        {
                            await _flowInstanceApp.Start(new StartFlowInstanceReq { FlowInstanceId = QuotationObj.FlowInstanceId });
                        }
                        await UnitWork.UpdateAsync<Quotation>(u => u.Id == QuotationObj.Id, u => new Quotation
                        {
                            QuotationStatus = 3.1M,
                            CollectionAddress = QuotationObj.CollectionAddress,
                            ShippingAddress = QuotationObj.ShippingAddress,
                            DeliveryMethod = QuotationObj.DeliveryMethod,
                            InvoiceCompany = QuotationObj.InvoiceCompany,
                            TotalMoney = QuotationObj.TotalMoney,
                            TotalCostPrice = QuotationObj.TotalCostPrice,
                            Remark = QuotationObj.Remark,
                            IsDraft = QuotationObj.IsDraft,
                            IsProtected = QuotationObj.IsProtected,
                            TravelExpense = QuotationObj.TravelExpense,
                            Status = 1,
                            ServiceChargeJH = QuotationObj.ServiceChargeJH,
                            ServiceChargeSM = QuotationObj.ServiceChargeSM,
                            Prepay = QuotationObj.Prepay,
                            TaxRate = QuotationObj.TaxRate,
                            InvoiceCategory = QuotationObj.InvoiceCategory,
                            AcceptancePeriod = QuotationObj.AcceptancePeriod,
                            PaymentAfterWarranty = QuotationObj.PaymentAfterWarranty,
                            CashBeforeFelivery = QuotationObj.CashBeforeFelivery,
                            PayOnReceipt = QuotationObj.PayOnReceipt,
                            CollectionDA = QuotationObj.CollectionDA,
                            ShippingDA = QuotationObj.ShippingDA,
                            AcquisitionWay = QuotationObj.AcquisitionWay,
                            IsMaterialType = QuotationObj.IsMaterialType,
                            ServiceChargeManHourJH = QuotationObj.ServiceChargeManHourJH,
                            ServiceChargeManHourSM = QuotationObj.ServiceChargeManHourSM,
                            TravelExpenseManHour = QuotationObj.TravelExpenseManHour,
                            ServiceChargeJHCost = QuotationObj.ServiceChargeJHCost,
                            ServiceChargeSMCost = QuotationObj.ServiceChargeSMCost,
                            TravelExpenseCost = QuotationObj.TravelExpenseCost,
                            PrintWarehouse = 1,
                            MoneyMeans = QuotationObj.MoneyMeans,
                            UpDateTime = DateTime.Now,
                            FlowInstanceId = QuotationObj.FlowInstanceId,
                            //todo:要修改的字段赋值
                        });
                        await UnitWork.AddAsync<QuotationOperationHistory>(new QuotationOperationHistory
                        {
                            Action = QuotationObj.ErpOrApp == 1 ? QuotationObj.CreateUser + "通过ERP提交审批" : QuotationObj.CreateUser + "通过APP提交审批",
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            CreateTime = DateTime.Now,
                            QuotationId = QuotationObj.Id,
                            ApprovalStage = "3"
                        });
                        //增加全局待处理
                        var serviceOrederObj = await UnitWork.Find<ServiceOrder>(s => s.Id == obj.ServiceOrderId).FirstOrDefaultAsync();
                        await _workbenchApp.AddOrUpdate(new WorkbenchPending
                        {
                            OrderType = 1,
                            TerminalCustomer = serviceOrederObj.TerminalCustomer,
                            TerminalCustomerId = serviceOrederObj.TerminalCustomerId,
                            ServiceOrderId = serviceOrederObj.Id,
                            ServiceOrderSapId = (int)serviceOrederObj.U_SAP_ID,
                            UpdateTime = DateTime.Now,
                            Remark = QuotationObj.Remark,
                            FlowInstanceId = QuotationObj.FlowInstanceId,
                            TotalMoney = QuotationObj.TotalMoney,
                            Petitioner = loginUser.Name,
                            SourceNumbers = QuotationObj.Id,
                            PetitionerId = loginUser.Id,
                        });
                        await UnitWork.SaveAsync();

                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("添加报价单失败,请重试。" + ex.Message);
                }
                return Message;
            }
        }

        /// <summary>
        /// 合并零件表
        /// </summary>
        /// <param name="QuotationObj"></param>
        /// <returns></returns>
        public async Task MergeMaterial(Quotation QuotationObj)
        {
            #region 合并零件表
            List<QuotationMaterial> QuotationMaterials = new List<QuotationMaterial>();
            QuotationObj.QuotationProducts.ToList().ForEach(q => QuotationMaterials.AddRange(q.QuotationMaterials));

            //var MaterialsT = from a in QuotationMaterials
            //                 group a by new { a.MaterialCode, a.MaterialDescription, a.Unit, a.SalesPrice, a.UnitPrice, a.Discount, a.MaterialType, a.DiscountPrices, a.WhsCode } into g
            //                 select new QueryQuotationMergeMaterialListReq
            //                 {
            //                     MaterialCode = g.Key.MaterialCode,
            //                     MaterialDescription = g.Key.MaterialDescription,
            //                     Unit = g.Key.Unit,
            //                     SalesPrice = g.Key.SalesPrice,
            //                     CostPrice = g.Key.UnitPrice,
            //                     Count = g.Sum(a => a.Count),
            //                     TotalPrice = (g.Key.DiscountPrices * g.Sum(a => a.Count)),
            //                     IsProtected = QuotationObj.IsMaterialType==2? false : true,
            //                     QuotationId = QuotationObj.Id,
            //                     Margin = (g.Key.DiscountPrices * g.Sum(a => a.Count)) - (g.Key.UnitPrice * g.Sum(a => a.Count)),
            //                     Discount = g.Key.Discount,
            //                     SentQuantity = 0,
            //                     MaterialType = (int)g.Key.MaterialType,
            //                     DiscountPrices = g.Key.DiscountPrices,
            //                     WhsCode = g.Key.WhsCode
            //                 };
            var quotationMaterialsList = QuotationMaterials.GroupBy(q => new { q.MaterialCode, q.MaterialDescription, q.Unit, q.MaterialType, q.DiscountPrices, q.WhsCode }).Select(q => new { mergeMaterial = q.First(), count = q.Sum(s => s.Count) }).ToList();
            var MaterialsT = quotationMaterialsList.Select(q => new QueryQuotationMergeMaterialListReq
            {
                MaterialCode = q.mergeMaterial.MaterialCode,
                MaterialDescription = q.mergeMaterial.MaterialDescription,
                Unit = q.mergeMaterial.Unit,
                SalesPrice = q.mergeMaterial.SalesPrice,
                CostPrice = q.mergeMaterial.UnitPrice,
                Count = q.count,
                TotalPrice = (q.mergeMaterial.DiscountPrices * q.count),
                IsProtected = QuotationObj.IsMaterialType == 2 ? false : true,
                QuotationId = QuotationObj.Id,
                Margin = (q.mergeMaterial.DiscountPrices * q.count) - (q.mergeMaterial.UnitPrice * q.count),
                Discount = q.mergeMaterial.Discount,
                SentQuantity = 0,
                MaterialType = (int)q.mergeMaterial.MaterialType,
                DiscountPrices = q.mergeMaterial.DiscountPrices,
                WhsCode = q.mergeMaterial.WhsCode
            });
            var QuotationMergeMaterialList = MaterialsT.ToList();

            if ((QuotationObj.ServiceChargeJH != null && QuotationObj.ServiceChargeJH > 0) || (QuotationObj.ServiceChargeJHCost != null && QuotationObj.ServiceChargeJHCost > 0))
            {
                QuotationMergeMaterialList.Add(new QueryQuotationMergeMaterialListReq
                {
                    MaterialCode = "S111-SERVICE-GSF-JH",
                    MaterialDescription = "寄回维修费 20210518",
                    Unit = "PCS",
                    SalesPrice = QuotationObj.ServiceChargeJH,
                    CostPrice = QuotationObj.ServiceChargeJHCost,
                    Count = QuotationObj.ServiceChargeManHourJH,
                    TotalPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeJH * QuotationObj.ServiceChargeManHourJH).ToString("#0.00")),
                    IsProtected = QuotationObj.IsMaterialType == 2 ? false : true,
                    QuotationId = QuotationObj.Id,
                    Margin = QuotationObj.ServiceChargeJH != null && QuotationObj.ServiceChargeJH > 0 ? QuotationObj.ServiceChargeJH * QuotationObj.ServiceChargeManHourJH : -(QuotationObj.ServiceChargeManHourJH * QuotationObj.ServiceChargeJHCost),
                    Discount = 100,
                    SentQuantity = 0,
                    MaterialType = 2,
                    DiscountPrices = QuotationObj.ServiceChargeJH,
                    WhsCode = "37"
                });
            }
            if ((QuotationObj.ServiceChargeSM != null && QuotationObj.ServiceChargeSM > 0) || (QuotationObj.ServiceChargeSMCost != null && QuotationObj.ServiceChargeSMCost > 0))
            {
                QuotationMergeMaterialList.Add(new QueryQuotationMergeMaterialListReq
                {
                    MaterialCode = "S111-SERVICE-GSF-SM",
                    MaterialDescription = "上门维修费 20210518",
                    Unit = "PCS",
                    SalesPrice = QuotationObj.ServiceChargeSM,
                    CostPrice = QuotationObj.ServiceChargeSMCost,
                    Count = QuotationObj.ServiceChargeManHourSM,
                    TotalPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeSM * QuotationObj.ServiceChargeManHourSM).ToString("#0.00")),
                    IsProtected = QuotationObj.IsMaterialType == 2 ? false : true,
                    QuotationId = QuotationObj.Id,
                    Margin = QuotationObj.ServiceChargeSM != null && QuotationObj.ServiceChargeSM > 0 ? QuotationObj.ServiceChargeSM * QuotationObj.ServiceChargeManHourSM : -(QuotationObj.ServiceChargeManHourSM * QuotationObj.ServiceChargeSMCost),
                    Discount = 100,
                    SentQuantity = 0,
                    MaterialType = 2,
                    DiscountPrices = QuotationObj.ServiceChargeSM,
                    WhsCode = "37"
                });
            }
            if ((QuotationObj.TravelExpense != null && QuotationObj.TravelExpense > 0) || (QuotationObj.TravelExpenseCost != null && QuotationObj.TravelExpenseCost > 0))
            {
                QuotationMergeMaterialList.Add(new QueryQuotationMergeMaterialListReq
                {
                    MaterialCode = "S111-SERVICE-CLF",
                    MaterialDescription = "差旅费",
                    Unit = "PCS",
                    SalesPrice = QuotationObj.TravelExpense,
                    CostPrice = QuotationObj.TravelExpenseCost,
                    Count = QuotationObj.TravelExpenseManHour,
                    TotalPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.TravelExpense * QuotationObj.TravelExpenseManHour).ToString("#0.00")),
                    IsProtected = QuotationObj.IsMaterialType == 2 ? false : true,
                    QuotationId = QuotationObj.Id,
                    Margin = QuotationObj.TravelExpense != null && QuotationObj.TravelExpense > 0 ? QuotationObj.TravelExpense * QuotationObj.TravelExpenseManHour : -(QuotationObj.TravelExpenseManHour * QuotationObj.TravelExpenseCost),
                    Discount = 100,
                    SentQuantity = 0,
                    MaterialType = 2,
                    DiscountPrices = QuotationObj.TravelExpense,
                    WhsCode = "37"
                });
            }
            var QuotationMergeMaterialListMap = QuotationMergeMaterialList.MapToList<QuotationMergeMaterial>();
            await UnitWork.BatchAddAsync<QuotationMergeMaterial>(QuotationMergeMaterialListMap.ToArray());
            await UnitWork.SaveAsync();
            #endregion
        }

        /// <summary>
        /// 撤回报价单
        /// </summary>
        /// <param name="QuotationId"></param>
        public async Task Revocation(int QuotationId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var quotationObj = await UnitWork.Find<Quotation>(q => q.Id == QuotationId && q.QuotationStatus <= 5).FirstOrDefaultAsync();
            if (quotationObj == null)
            {
                throw new Exception("该报价单状态不可撤销。");
            }
            await UnitWork.UpdateAsync<Quotation>(q => q.Id == QuotationId, q => new Quotation
            {
                QuotationStatus = 2
            });
            var delQuotationMergeMaterial = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(QuotationId)).ToListAsync();
            await UnitWork.BatchDeleteAsync<QuotationMergeMaterial>(delQuotationMergeMaterial.ToArray());
            var selqoh = await UnitWork.Find<QuotationOperationHistory>(r => r.QuotationId.Equals(QuotationId)).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
            QuotationOperationHistory qoh = new QuotationOperationHistory();
            qoh.CreateUser = loginContext.User.Name;
            qoh.CreateUserId = loginContext.User.Id;
            qoh.CreateTime = DateTime.Now;
            qoh.QuotationId = QuotationId;
            qoh.ApprovalResult = "撤回";
            qoh.Action = "撤回报价单";
            qoh.ApprovalStage = "2";
            qoh.IntervalTime = selqoh != null ? Convert.ToInt32((DateTime.Now - Convert.ToDateTime(selqoh.CreateTime)).TotalSeconds) : 0;
            await UnitWork.AddAsync<QuotationOperationHistory>(qoh);
            if (!string.IsNullOrWhiteSpace(quotationObj.FlowInstanceId))
            {
                await _flowInstanceApp.ReCall(new RecallFlowInstanceReq { FlowInstanceId = quotationObj.FlowInstanceId });
            }
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 修改报价单物料，物流
        /// </summary>
        /// <param name="obj"></param>
        public async Task<TableData> UpdateMaterial(AddOrUpdateQuotationReq obj)
        {
            var expressageobj = new Expressage();
            var expressageMap = obj.ExpressageReqs.MapTo<Expressage>();
            var loginUser = new User();
            if (expressageMap.ExpressNumber == "自动出库")
            {
                loginUser = await UnitWork.Find<User>(u => u.Account.Equals("Admin")).FirstOrDefaultAsync();
            }
            else
            {
                var loginContext = _auth.GetCurrentUser();
                if (loginContext == null)
                {
                    throw new CommonException("登录已过期", Define.INVALID_TOKEN);
                }
                loginUser = loginContext.User;
                if (!loginContext.Roles.Any(r => r.Name.Equals("仓库")))
                {
                    throw new Exception("无仓库人员权限，不可出库。");
                }
            }
            #region 判断条件
            var quotationObj = await UnitWork.Find<Quotation>(q => q.Id == expressageMap.QuotationId).Include(q => q.QuotationMergeMaterials).FirstOrDefaultAsync();
            var mergeMaterialList = quotationObj.QuotationMergeMaterials.Select(q => new { q.MaterialCode, q.Id, q.WhsCode }).ToList();
            if (quotationObj.SalesOrderId == null || quotationObj.SalesOrderId <= 0)
            {
                throw new Exception("暂未生成销售订单，不可出库，请联系管理员。");
            }
            //判定是否存在成品
            mergeMaterialList.ForEach(m =>
            {
                if (m.MaterialCode.Trim().Substring(0, 1) == "C")
                {
                    throw new Exception("本出库单存在成品物料，请到ERP3.0进行交货操作。");
                }
            });
            string message = null;
            //判定库存数量
            var mergeMaterialIds = obj.QuotationMergeMaterialReqs.Select(q => q.Id).ToList();
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ShieldingMaterials")).Select(u => u.Name).ToListAsync();
            mergeMaterialList = mergeMaterialList.Where(q => mergeMaterialIds.Contains(q.Id) && !CategoryList.Contains(q.MaterialCode)).ToList();
            var mergeMaterials = mergeMaterialList.Select(m => m.MaterialCode).ToList();
            var whscodes = mergeMaterialList.Select(m => m.WhsCode).Distinct();
            var onHand = await UnitWork.Find<OITW>(o => mergeMaterials.Contains(o.ItemCode) && whscodes.Contains(o.WhsCode)).Select(o => new { o.ItemCode, o.OnHand, o.WhsCode }).ToListAsync();
            onHand.ForEach(o =>
            {
                var mergeMaterialid = mergeMaterialList.Where(m => m.MaterialCode.Equals(o.ItemCode) && m.WhsCode.Equals(o.WhsCode)).FirstOrDefault()?.Id;
                var num = obj.QuotationMergeMaterialReqs.Where(q => q.Id == mergeMaterialid).FirstOrDefault()?.SentQuantity;
                if (num != null && num > o.OnHand)
                {
                    message += o.ItemCode + "  ";
                }
            }
             );
            if (!string.IsNullOrWhiteSpace(message))
            {
                throw new Exception(message + "数量降为负库存，不可交货");
            }
            #endregion
            var result = new TableData();
            var dbContext = UnitWork.GetDbContext<Quotation>();
            List<LogisticsRecord> LogisticsRecords = new List<LogisticsRecord>();
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    //用信号量代替锁
                    await semaphoreSlim.WaitAsync();
                    try
                    {
                        if (string.IsNullOrWhiteSpace(expressageMap.ExpressNumber))
                        {
                            var time = DateTime.Now;
                            expressageMap.ExpressNumber = "ZT" + time.Year.ToString() + time.Month.ToString() + time.Day.ToString() + time.Hour.ToString() + time.Minute.ToString() + time.Second.ToString();
                        }
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                    expressageMap.CreateTime = DateTime.Now;
                    expressageobj = await UnitWork.AddAsync<Expressage>(expressageMap);
                    var ExpressagePictures = new List<ExpressagePicture>();
                    obj.ExpressageReqs.ExpressagePictures.ForEach(p => ExpressagePictures.Add(new ExpressagePicture { ExpressageId = expressageobj.Id, PictureId = p, Id = Guid.NewGuid().ToString() }));
                    await UnitWork.BatchAddAsync<ExpressagePicture>(ExpressagePictures.ToArray());
                    foreach (var item in obj.QuotationMergeMaterialReqs)
                    {
                        LogisticsRecords.Add(new LogisticsRecord
                        {
                            CreateTime = DateTime.Now,
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            Quantity = item.SentQuantity,
                            QuotationId = item.QuotationId,
                            QuotationMaterialId = item.Id,
                            ExpressageId = expressageobj.Id
                        });
                        if (item.SentQuantity > 0)
                        {
                            var QuotationMergeMaterialobj = await UnitWork.Find<QuotationMergeMaterial>(q => q.Id.Equals(item.Id)).FirstOrDefaultAsync();
                            QuotationMergeMaterialobj.SentQuantity += item.SentQuantity;
                            await UnitWork.UpdateAsync<QuotationMergeMaterial>(QuotationMergeMaterialobj);
                        }
                    }
                    await UnitWork.BatchAddAsync<LogisticsRecord>(LogisticsRecords.ToArray());
                    await UnitWork.SaveAsync();
                    var Expressages = await UnitWork.Find<Expressage>(e => e.QuotationId.Equals(obj.ExpressageReqs.QuotationId)).Include(e => e.ExpressagePicture).Include(e => e.LogisticsRecords).OrderByDescending(e => e.CreateTime).ToListAsync();
                    LogisticsRecords = new List<LogisticsRecord>();
                    Expressages.ForEach(e => LogisticsRecords.AddRange(e.LogisticsRecords));
                    var QuotationMergeMaterialLists = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(obj.ExpressageReqs.QuotationId)).ToListAsync();
                    #region 写入sap和反写记录


                    int isEXwarehouse = QuotationMergeMaterialLists.Where(q => q.SentQuantity != q.Count).Count();
                    List<QuotationOperationHistory> qoh = new List<QuotationOperationHistory>();
                    var selqoh = await UnitWork.Find<QuotationOperationHistory>(r => r.QuotationId.Equals(obj.ExpressageReqs.QuotationId)).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();

                    if (selqoh.ApprovalStage != "12")
                    {
                        qoh.Add(new QuotationOperationHistory
                        {
                            Id = Guid.NewGuid().ToString(),
                            Action = "开始出库",
                            ApprovalResult = "出库成功",
                            ApprovalStage = "12",
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            CreateTime = DateTime.Now,
                            QuotationId = obj.ExpressageReqs.QuotationId,
                            IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(selqoh.CreateTime)).TotalSeconds)
                        });
                        //if (!string.IsNullOrWhiteSpace(quotationObj.FlowInstanceId) && quotationObj.QuotationStatus != 12)
                        //{
                        //    await _flowInstanceApp.Verification(VerificationReqModle);
                        //}
                    }
                    if (isEXwarehouse == 0)
                    {
                        await UnitWork.UpdateAsync<Quotation>(q => q.Id.Equals(obj.ExpressageReqs.QuotationId), q => new Quotation { QuotationStatus = 11, UpDateTime = DateTime.Now });
                        qoh.Add(new QuotationOperationHistory
                        {
                            Id = Guid.NewGuid().ToString(),
                            Action = "出库完成",
                            ApprovalResult = "出库成功",
                            ApprovalStage = "11",
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            CreateTime = DateTime.Now,
                            QuotationId = obj.ExpressageReqs.QuotationId,
                            IntervalTime = qoh.Count > 0 ? 0 : Convert.ToInt32((DateTime.Now - Convert.ToDateTime(selqoh.CreateTime)).TotalSeconds)

                        });
                        if (!string.IsNullOrWhiteSpace(quotationObj.FlowInstanceId))
                        {
                            await _flowInstanceApp.Verification(new VerificationReq
                            {
                                NodeRejectStep = "",
                                NodeRejectType = "0",
                                FlowInstanceId = quotationObj.FlowInstanceId,
                                VerificationFinally = "1",
                                VerificationOpinion = "出库成功",
                            });
                        }
                    }
                    else
                    {
                        await UnitWork.UpdateAsync<Quotation>(q => q.Id.Equals(obj.ExpressageReqs.QuotationId), q => new Quotation { QuotationStatus = 12, UpDateTime = DateTime.Now });
                    }
                    await UnitWork.BatchAddAsync<QuotationOperationHistory>(qoh.ToArray());
                    await UnitWork.SaveAsync();
                    #endregion
                    #region 返回成功值
                    var MergeMaterials = from a in QuotationMergeMaterialLists
                                         join b in LogisticsRecords on a.Id equals b.QuotationMaterialId
                                         select new { a, b };
                    result.Data = new
                    {
                        start = isEXwarehouse == 0 ? 7 : 0,
                        Expressages = Expressages.Select(e => new
                        {
                            e.ExpressagePicture,
                            e.ExpressInformation,
                            e.ExpressNumber,
                            e.Id,
                            e.Freight,
                            e.QuotationId,
                            e.Remark,
                            e.ReturnNoteId,
                            LogisticsRecords = MergeMaterials.Where(m => m.b.ExpressageId.Equals(e.Id)).Select(m => new
                            {
                                m.a.MaterialCode,
                                m.a.MaterialDescription,
                                m.a.Count,
                                m.a.Unit,
                                m.a.SentQuantity,
                                m.b.Quantity,
                                m.a.WhsCode
                            }).ToList()
                        }).ToList()
                    };
                    #endregion
                    if (obj.IsMaterialType =="4")
                    {
                        var quotation=await UnitWork.Find<Quotation>(q => q.Id == obj.Id).Include(q=>q.QuotationProducts).ThenInclude(q=>q.QuotationMaterials).FirstOrDefaultAsync();
                        var prodctCodes=quotation.QuotationProducts.Select(q => q.ProductCode).ToList();
                        var warrantyDates= await UnitWork.Find<SalesOrderWarrantyDate>(s => prodctCodes.Contains(s.MnfSerial)).ToListAsync();
                        foreach (var item in quotation.QuotationProducts)
                        {
                            await UnitWork.UpdateAsync<SalesOrderWarrantyDate>(s => s.MnfSerial.Equals(item.ProductCode), s => new SalesOrderWarrantyDate { WarrantyPeriod = item.WarrantyTime });
                            await UnitWork.AddAsync<SalesOrderWarrantyDateRecord>(new SalesOrderWarrantyDateRecord
                            {
                                Id=Guid.NewGuid().ToString(),
                                SalesOrderWarrantyDateId = warrantyDates.Where(w => w.MnfSerial.Equals(item.ProductCode)).FirstOrDefault()?.Id,
                                CreateTime = DateTime.Now,
                                QuotationId = quotation.Id,
                                WarrantyExpense = item.QuotationMaterials != null && item.QuotationMaterials.Count() > 0 ? item.QuotationMaterials.Sum(q => q.DiscountPrices * q.Count) : 0,
                                CreateUser=quotation.CreateUser,
                                CreateUserId=quotation.CreateUserId,
                                WarrantyPeriod=item.WarrantyTime
                            });
                        }
                    }
                    transaction.Commit();
                    _capBus.Publish("Serve.SalesOfDelivery.Create", obj);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("添加物流失败,请重试。" + ex.Message);
                }
            }
            return result;
        }
        /// <summary>
        /// 维修费差旅费自动交货
        /// </summary>
        /// <returns></returns>
        public async Task TimeOfDelivery(int QuotationId)
        {
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ShieldingMaterials")).Select(u => u.Name).ToListAsync();

            var quotations = await UnitWork.Find<Quotation>(q => q.Id == QuotationId && q.Status == 2).Include(q => q.QuotationMergeMaterials)
                .Where(q => (q.QuotationMergeMaterials.Where(m => !CategoryList.Contains(m.MaterialCode)).Count() <= 0 || q.IsMaterialType == 4) && q.SalesOrderId != null).FirstOrDefaultAsync();
            if (quotations != null)
            {
                var pictures = "68cc3412-492b-4f39-b7de-3ab3a957017b";
                if (quotations.IsMaterialType == 4)
                {
                    pictures = "9fda9864-6d40-46bc-a94b-3f2d45d2d3c7";
                }
                else
                {
                    if ((quotations.ServiceChargeJH > 0 || quotations.ServiceChargeSM > 0) && quotations.TravelExpense > 0)
                    {
                        pictures = "701d519b-5c0a-4369-adf4-8c0a2b7f0b16";
                    }
                    else if (quotations.TravelExpense > 0)
                    {
                        pictures = "01a62877-1961-4f0e-9f39-2dab2cb2eb4a";
                    }
                }

                AddOrUpdateQuotationReq obj = new AddOrUpdateQuotationReq();
                obj.Id = quotations.Id;
                obj.IsMaterialType = quotations.IsMaterialType.ToString();
                obj.ExpressageReqs = new ExpressageReq
                {
                    ExpressNumber = "自动出库",
                    Freight = "0",
                    QuotationId = quotations.Id,
                    ExpressagePictures = new List<string>() { pictures }
                };
                obj.QuotationMergeMaterialReqs = new List<QuotationMergeMaterialReq>();
                quotations.QuotationMergeMaterials.ForEach(q =>
                {
                    obj.QuotationMergeMaterialReqs.Add(new QuotationMergeMaterialReq
                    {
                        DiscountPrices = q.DiscountPrices,
                        MaterialDescription = q.MaterialDescription,
                        MaterialCode = q.MaterialCode,
                        WhsCode = q.WhsCode,
                        SentQuantity = q.Count,
                        QuotationId = q.QuotationId,
                        Id = q.Id,
                        MaterialType = q.MaterialType.ToString()
                    });
                });
                if (quotations.IsMaterialType != null)
                {
                    await UpdateMaterial(obj);
                }
            }
        }

        /// <summary>
        /// 审批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Accraditation(AccraditationQuotationReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(req.AppId));
            }

            QuotationOperationHistory qoh = new QuotationOperationHistory();

            var obj = await UnitWork.Find<Quotation>(q => q.Id == req.Id).Include(q => q.QuotationProducts).Include(q => q.QuotationMergeMaterials).FirstOrDefaultAsync();
            var flowInstanceObj = await UnitWork.Find<FlowInstance>(f => f.Id.Equals(obj.FlowInstanceId)).FirstOrDefaultAsync();
            qoh.ApprovalStage = obj.QuotationStatus.ToString();

            VerificationReq VerificationReqModle = new VerificationReq();
            if (req.IsReject)
            {
                VerificationReqModle = new VerificationReq
                {
                    NodeRejectStep = "",
                    NodeRejectType = "1",
                    FlowInstanceId = obj.FlowInstanceId,
                    VerificationFinally = "3",
                    VerificationOpinion = req.Remark,
                };
                if (!string.IsNullOrWhiteSpace(obj.FlowInstanceId))
                {
                    await _flowInstanceApp.Verification(VerificationReqModle);
                }
                obj.QuotationStatus = 1;
                qoh.ApprovalResult = "驳回";
                qoh.ApprovalStage = "1";
                var delQuotationMergeMaterial = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(obj.Id)).ToListAsync();
                await UnitWork.BatchDeleteAsync<QuotationMergeMaterial>(delQuotationMergeMaterial.ToArray());
            }
            else
            {
                if ((loginContext.Roles.Any(r => r.Name.Equals("销售员")) || loginContext.Roles.Any(r => r.Name.Equals("总经理"))) && flowInstanceObj.ActivityName == "销售员审批")
                {
                    qoh.Action = "销售员审批";
                    obj.QuotationStatus = 4;
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("物料工程审批")) && flowInstanceObj.ActivityName == "工程审批")
                {
                    qoh.Action = "工程审批";
                    obj.QuotationStatus = 5;
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")) && flowInstanceObj.ActivityName == "总经理审批")
                {
                    qoh.Action = "总经理审批";
                    if (obj.IsMaterialType == 1 || obj.IsMaterialType == 3)
                    {
                        if (req.IsTentative == true)
                        {
                            obj.QuotationStatus = 5;
                            obj.Tentative = true;
                        }
                        else
                        {
                            obj.Tentative = false;
                            obj.QuotationStatus = 10;
                            obj.Status = 2;
                            #region 报价单同步到SAP，ERP3.0
                            _capBus.Publish("Serve.SellOrder.Create", obj.Id);
                            #endregion
                            
                        }

                    }
                    else
                    {
                        obj.QuotationStatus = 6;
                    }

                }
                else if (obj.CreateUserId.Equals(loginUser.Id) && flowInstanceObj.ActivityName == "确认报价单")
                {
                    qoh.Action = "客户确认报价单";
                    obj.QuotationStatus = 7;
                    #region 报价单同步到SAP，ERP3.0 
                    _capBus.Publish("Serve.SellOrder.Create", obj.Id);
                    #endregion
                }
                else if (obj.CreateUserId.Equals(loginUser.Id) && flowInstanceObj.ActivityName == "回传销售订单")
                {
                    qoh.Action = "回传销售订单";
                    obj.QuotationStatus = 8;
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("物料财务")) && flowInstanceObj.ActivityName == "财务审批")
                {
                    qoh.Action = "财务审批";
                    if (req.IsTentative == true)
                    {
                        obj.QuotationStatus = 8;
                        obj.Tentative = true;
                    }
                    else
                    {
                        obj.QuotationStatus = 10;
                        obj.Status = 2;
                    }

                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("销售总助")) && flowInstanceObj.ActivityName == "销售总助审批")
                {
                    qoh.Action = "销售总助审批";
                    obj.QuotationStatus = 5;
                    if (obj.WarrantyType == 1)
                    {
                        var prodctCodes = obj.QuotationProducts.Select(q => q.ProductCode).ToList();
                        var warrantyDates = await UnitWork.Find<SalesOrderWarrantyDate>(s => prodctCodes.Contains(s.MnfSerial)).ToListAsync();
                        foreach (var item in obj.QuotationProducts)
                        {
                            await UnitWork.UpdateAsync<SalesOrderWarrantyDate>(s => s.MnfSerial.Equals(item.ProductCode), s => new SalesOrderWarrantyDate { WarrantyPeriod = item.WarrantyTime });
                            await UnitWork.AddAsync<SalesOrderWarrantyDateRecord>(new SalesOrderWarrantyDateRecord
                            {
                                Id = Guid.NewGuid().ToString(),
                                SalesOrderWarrantyDateId = warrantyDates.Where(w => w.MnfSerial.Equals(item.ProductCode)).FirstOrDefault()?.Id,
                                CreateTime = DateTime.Now,
                                QuotationId = obj.Id,
                                WarrantyExpense = item.QuotationMaterials!=null&&item.QuotationMaterials.Count()>0?item.QuotationMaterials.Sum(q => q.DiscountPrices * q.Count):0,
                                CreateUser = obj.CreateUser,
                                CreateUserId = obj.CreateUserId,
                                WarrantyPeriod = item.WarrantyTime
                            });
                        }
                    }
                }
                else
                {
                    throw new Exception("暂无审批该流程权限，不可审批");
                }
                //else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")) && flowInstanceObj.ActivityName == "总经理审批")
                //{
                //    qoh.Action = "总经理审批";
                //    obj.QuotationStatus = 10;
                //    obj.Status = 2;
                //}
                if (req.IsTentative == true)
                {
                    obj.QuotationStatus = decimal.Parse(qoh.ApprovalStage);
                    obj.Tentative = true;
                    qoh.ApprovalResult = "暂定";
                }
                else
                {
                    qoh.ApprovalResult = "同意";
                    VerificationReqModle = new VerificationReq
                    {
                        NodeRejectStep = "",
                        NodeRejectType = "0",
                        FlowInstanceId = obj.FlowInstanceId,
                        VerificationFinally = "1",
                        VerificationOpinion = req.Remark,
                    };
                    if (!string.IsNullOrWhiteSpace(obj.FlowInstanceId))
                    {
                        await _flowInstanceApp.Verification(VerificationReqModle);
                    }
                }

            }
            await UnitWork.UpdateAsync<Quotation>(q => q.Id == obj.Id, q => new Quotation
            {
                UpDateTime = DateTime.Now,
                Tentative = obj.Tentative,
                QuotationStatus = obj.QuotationStatus,
                Status = obj.Status,
            });
            if (req.PictureIds != null && req.PictureIds.Count > 0)
            {
                List<QuotationPicture> QuotationPictures = new List<QuotationPicture>();
                req.PictureIds.ForEach(p => QuotationPictures.Add(new QuotationPicture { Id = Guid.NewGuid().ToString(), PictureId = p, QuotationId = obj.Id }));
                await UnitWork.BatchAddAsync<QuotationPicture>(QuotationPictures.ToArray());
            }
            var selqoh = await UnitWork.Find<QuotationOperationHistory>(r => r.QuotationId.Equals(obj.Id)).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
            qoh.CreateUser = loginContext.User.Name;
            qoh.CreateUserId = loginContext.User.Id;
            qoh.CreateTime = DateTime.Now;
            qoh.QuotationId = obj.Id;
            qoh.Remark = req.Remark;
            qoh.IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(selqoh.CreateTime)).TotalSeconds);
            await UnitWork.AddAsync<QuotationOperationHistory>(qoh);
            //修改全局待处理
            await UnitWork.UpdateAsync<WorkbenchPending>(w => w.SourceNumbers == obj.Id && w.OrderType == 1, w => new WorkbenchPending
            {
                UpdateTime = obj.UpDateTime,
            });
            await UnitWork.SaveAsync();
            if (obj.Status == 2) 
            {
                await TimeOfDelivery(obj.Id);
            }
        }

        /// <summary>
        /// 删除报价单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Delete(QueryQuotationListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            await UnitWork.DeleteAsync<Quotation>(q => q.Id == req.QuotationId);
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="AppId"></param>
        /// <returns></returns>
        private async Task<User> GetUserId(int AppId)
        {
            var userid = await UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(AppId)).Select(u => u.UserID).FirstOrDefaultAsync();

            return await UnitWork.Find<User>(u => u.Id.Equals(userid)).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 导入设备零件价格
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public async Task ImportMaterialPrice(ExcelHandler handler)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var MaterialPriceList = handler.GetListData<MaterialPrice>(mapper =>
            {
                var data = mapper
                .Map<MaterialPrice>(0, a => a.MaterialCode)
                .Map<MaterialPrice>(1, a => a.SettlementPrice)
                .Map<MaterialPrice>(2, a => a.SettlementPriceModel)
                .Map<MaterialPrice>(3, a => a.SalesMultiple)
                .Take<MaterialPrice>(0);
                return data.Select(d => d.Value).SkipWhile(v => v is null).ToList();
            });
            MaterialPriceList = MaterialPriceList.Where(m => !string.IsNullOrWhiteSpace(m.MaterialCode)).ToList();
            MaterialPriceList.ForEach(m =>
            {
                m.CreateUserId = loginContext.User.Id;
                m.CreateUser = loginContext.User.Name;
                m.CreateTime = DateTime.Now;
            });
            var materialCodes = MaterialPriceList.Select(m => m.MaterialCode).ToList();
            var materialPrices = await UnitWork.Find<MaterialPrice>(m => materialCodes.Contains(m.MaterialCode)).ToListAsync();
            await UnitWork.BatchDeleteAsync<MaterialPrice>(materialPrices.ToArray());
            await UnitWork.BatchAddAsync<MaterialPrice>(MaterialPriceList.ToArray());
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 通用条件
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task<string> Condition(AddOrUpdateQuotationReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppId));
            }
            #region 判断技术员余额
            //var ReturnNoteList = await UnitWork.Find<ReturnNote>(r => r.CreateUserId.Equals(loginUser.Id)).Include(r => r.ReturnnoteMaterials).ToListAsync();

            //List<int> returnNoteIds = ReturnNoteList.Select(s => s.Id).Distinct().ToList();
            ////计算剩余未结清金额
            //var notClearAmountList = (await UnitWork.Find<ReturnnoteMaterial>(w => returnNoteIds.Contains((int)w.ReturnNoteId) && w.Check == 1).ToListAsync()).GroupBy(g => g.MaterialCode).Select(s => new { s.Key, GoodCount = s.Sum(s => s.GoodQty), SecondCount = s.Sum(s => s.SecondQty), DiscountPrices = s.ToList().FirstOrDefault().DiscountPrices, TotalCount = s.ToList().FirstOrDefault().TotalCount }).ToList();
            //var totalprice = notClearAmountList.Sum(s => s.DiscountPrices * (s.TotalCount - s.GoodCount - s.SecondCount));
            //if (totalprice > 4000)
            //{
            //    throw new Exception("欠款已超出额度，不可领料。");
            //}
            #endregion

            #region 判断是否存在相同物料
            List<string> MaterialCode = new List<string>();
            List<QuotationMaterialReq> QuotationMaterialReps = new List<QuotationMaterialReq>();
            List<QuotationMergeMaterial> QuotationMergeMaterials = new List<QuotationMergeMaterial>();
            StringBuilder MaterialName = new StringBuilder();

            foreach (var item in obj.QuotationProducts)
            {
                MaterialCode.AddRange(item.QuotationMaterials.Select(q => q.MaterialCode).ToList());
                QuotationMaterialReps.AddRange(item.QuotationMaterials.ToList());
            }
            var Quotations = await UnitWork.Find<Quotation>(q => q.Status > 3 && q.Status < 6).Include(q => q.QuotationMergeMaterials).Where(q => q.QuotationMergeMaterials.Any(m => MaterialCode.Contains(m.MaterialCode))).ToListAsync();

            if (Quotations != null && Quotations.Count > 0)
            {
                Quotations.ForEach(q => QuotationMergeMaterials.AddRange(q.QuotationMergeMaterials.Where(m => MaterialCode.Contains(m.MaterialCode)).ToList()));
            }
            if (QuotationMergeMaterials != null && QuotationMergeMaterials.Count > 0)
            {
                var MaterialCodeCount = QuotationMergeMaterials.GroupBy(q => q.MaterialCode).Select(q => new { q.Key, Count = q.Select(s => s.Count).Sum() });
                foreach (var item in MaterialCodeCount)
                {
                    var QuotationMaterialRepsCount = QuotationMaterialReps.Where(q => q.MaterialCode.Equals(item.Key)).Select(q => new { q.WarehouseQuantity, q.Count }).ToList();
                    if (QuotationMaterialRepsCount.Sum(q => q.Count) + item.Count > QuotationMaterialRepsCount.FirstOrDefault()?.WarehouseQuantity)
                    {
                        MaterialName.Append(QuotationMaterialRepsCount.FirstOrDefault()?.WarehouseQuantity + ",");
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(MaterialName.ToString()))
            {
                return MaterialName.ToString().Substring(0, MaterialName.ToString().Length - 2) + "已存在多笔订单且库存数量不满足，请尽快付款。";
            }
            #endregion

            #region 判断是否已经开始退料 则不允许领料
            //var isExist = (await UnitWork.Find<ReturnNote>(w => w.ServiceOrderId == obj.ServiceOrderId && w.CreateUserId == loginUser.Id).ToListAsync()).Count > 0 ? true : false;
            //if (isExist)
            //{
            //    throw new Exception("该服务单已开始退料，不可领料。");
            //}
            #endregion
            #region  判断序列号数据是否存在
            if (obj.IsMaterialType == "4")
            {
                var productCodes = obj.QuotationProducts.Where(q => q.WarrantyExpirationTime != null).Select(q => q.ProductCode).ToList();
                var dateCount = await UnitWork.Find<SalesOrderWarrantyDate>(s => productCodes.Contains(s.MnfSerial)).CountAsync();
                if (dateCount < productCodes.Count())
                {
                    throw new Exception("暂时不可更改此服务单，请联系管理员");
                }
            }
            #endregion
            //判定字段是否同时存在
            if (!(!string.IsNullOrWhiteSpace(obj.TaxRate) && !string.IsNullOrWhiteSpace(obj.InvoiceCategory) && !string.IsNullOrWhiteSpace(obj.InvoiceCompany)) && !(string.IsNullOrWhiteSpace(obj.TaxRate) && string.IsNullOrWhiteSpace(obj.InvoiceCategory) && string.IsNullOrWhiteSpace(obj.InvoiceCompany)))
            {
                throw new Exception("请核对是否存在未填写字段");
            }
            var nsapUserId = await UnitWork.Find<NsapUserMap>(n => n.UserID.Equals(loginContext.User.Id)).Select(n => n.NsapUserId).FirstOrDefaultAsync();
            //判定人员是否有销售员code
            var userId = (await UnitWork.Find<NsapUserMap>(n => n.UserID.Equals(loginContext.User.Id)).FirstOrDefaultAsync())?.NsapUserId;
            var slpCode = (await UnitWork.Find<sbo_user>(s => s.user_id == userId && s.sbo_id == Define.SBO_ID).FirstOrDefaultAsync())?.sale_id;

            if (slpCode == null || slpCode == 0)
            {
                throw new Exception("暂无销售权限，请联系呼叫中心");
            }
            return null;
        }

        /// <summary>
        /// 计算价格
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private async Task<Quotation> CalculatePrice(AddOrUpdateQuotationReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;

            var QuotationObj = obj.MapTo<Quotation>();
            if (QuotationObj.IsMaterialType != 4 && QuotationObj.WarrantyType == 2)
            {
                QuotationObj.QuotationProducts = QuotationObj.QuotationProducts.Where(q => q.QuotationMaterials.Count > 0).ToList();
            }
            QuotationObj.ErpOrApp = 1;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppId));
                QuotationObj.ErpOrApp = 2;
                throw new Exception("APP暂时不可领料，请前往ERP4.0进行领料。");
            }
            QuotationObj.TotalMoney = 0;
            QuotationObj.TotalCostPrice = 0;
            QuotationObj.Tentative = false;
            QuotationObj.PrintNo = Guid.NewGuid().ToString();
            QuotationObj.PrintTheNumber = 0;
            QuotationObj.IsProtected = QuotationObj.IsMaterialType == 2 || (QuotationObj.IsMaterialType == 4 && QuotationObj.WarrantyType == 2) ? true : false;
            QuotationObj.QuotationProducts.ForEach(q =>
            {
                q.QuotationMaterials.ForEach(m =>
                {
                    if (m.MaterialType != 4 && m.MaterialType != 3 && m.SalesPrice > 0 && Convert.ToDouble(m.DiscountPrices / m.SalesPrice) < 0.4)
                    {
                        throw new Exception("金额有误请重新输入");
                    }
                    m.SalesPrice = m.MaterialType != 3 ? m.SalesPrice : 0;
                    m.DiscountPrices = m.MaterialType != 3 && m.MaterialType != 4 ? m.DiscountPrices : 0;
                    m.Discount = m.MaterialType != 3 && m.MaterialType != 4 ? m.SalesPrice != 0 ? (m.DiscountPrices / m.SalesPrice) * 100 : 100 : 100;
                    m.TotalPrice = m.MaterialType != 3 ? Convert.ToDecimal(Convert.ToDecimal(m.DiscountPrices * m.Count).ToString("#0.00")) : 0;
                    QuotationObj.TotalCostPrice += m.MaterialType != 3 ? Convert.ToDecimal(Convert.ToDecimal(m.DiscountPrices * m.Count).ToString("#0.00")) : 0;
                    QuotationObj.TotalMoney += m.MaterialType != 3 ? Convert.ToDecimal(Convert.ToDecimal(m.DiscountPrices * m.Count).ToString("#0.00")) : 0;
                });
            });
            #region 判定通用物料
            if (QuotationObj.ServiceChargeJH != null && QuotationObj.ServiceChargeJH > 0 && QuotationObj.ServiceChargeManHourJH != null && QuotationObj.ServiceChargeManHourJH > 0 && QuotationObj.IsMaterialType == 2)
            {
                QuotationObj.TotalMoney += Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeJH * QuotationObj.ServiceChargeManHourJH).ToString("#0.00"));
                QuotationObj.ServiceChargeJHCost = 0;
            }
            else if (QuotationObj.ServiceChargeJHCost != null && QuotationObj.ServiceChargeJHCost > 0 && QuotationObj.IsMaterialType == 3)
            {
                QuotationObj.ServiceChargeJH = 0;
            }
            else
            {
                QuotationObj.ServiceChargeJH = null;
                QuotationObj.ServiceChargeJHCost = null;
                QuotationObj.ServiceChargeManHourJH = null;
            }
            if (QuotationObj.ServiceChargeSM != null && QuotationObj.ServiceChargeSM > 0 && QuotationObj.ServiceChargeManHourSM != null && QuotationObj.ServiceChargeManHourSM > 0 && QuotationObj.IsMaterialType == 2)
            {
                QuotationObj.TotalMoney += Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeSM * QuotationObj.ServiceChargeManHourSM).ToString("#0.00"));
                QuotationObj.ServiceChargeSMCost = 0;
            }
            else if (QuotationObj.ServiceChargeSMCost != null && QuotationObj.ServiceChargeSMCost > 0 && QuotationObj.IsMaterialType == 3)
            {
                QuotationObj.ServiceChargeSM = 0;
            }
            else
            {
                QuotationObj.ServiceChargeSMCost = null;
                QuotationObj.ServiceChargeSM = null;
                QuotationObj.ServiceChargeManHourSM = null;
            }
            if (QuotationObj.TravelExpense != null && QuotationObj.TravelExpense > 0 && QuotationObj.TravelExpenseManHour != null && QuotationObj.TravelExpenseManHour > 0 && QuotationObj.IsMaterialType == 2)
            {
                QuotationObj.TotalMoney += Convert.ToDecimal(Convert.ToDecimal(QuotationObj.TravelExpense * QuotationObj.TravelExpenseManHour).ToString("#0.00"));
                QuotationObj.TravelExpenseCost = 0;
            }
            else if (QuotationObj.TravelExpenseCost != null && QuotationObj.TravelExpenseCost > 0 && QuotationObj.IsMaterialType == 3)
            {
                QuotationObj.TravelExpense = 0;
            }
            else
            {
                QuotationObj.TravelExpenseCost = null;
                QuotationObj.TravelExpense = null;
                QuotationObj.TravelExpenseManHour = null;
            }
            #endregion
            if (QuotationObj.IsMaterialType == 4)
            {
                QuotationObj.QuotationProducts = QuotationObj.QuotationProducts.Where(q => q.WarrantyTime != null).ToList();
            }
            return QuotationObj;
        }

        /// <summary>
        /// 获取合并后数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetMergeMaterial(QueryQuotationListReq req)
        {
            var result = new TableData();
            var QuotationMergeMaterials = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(req.QuotationId))
                .WhereIf(!string.IsNullOrWhiteSpace(req.MaterialCode), q => req.MaterialCode.Contains(q.MaterialCode)).ToListAsync();
            //result.Count = await QuotationMergeMaterials.CountAsync();
            var MaterialsList = QuotationMergeMaterials.Select(q => q.MaterialCode).ToList();
            var WhsCode = QuotationMergeMaterials.Select(q => q.WhsCode).Distinct().ToList();
            var ItemCodes = await UnitWork.Find<OITW>(o => MaterialsList.Contains(o.ItemCode) && WhsCode.Contains(o.WhsCode)).Select(o => new { o.ItemCode, o.WhsCode, o.OnHand }).ToListAsync();
            result.Data = QuotationMergeMaterials.Select(q => new
            {
                WhsCode = q.WhsCode,
                WarehouseQuantity = ItemCodes.Where(i => i.ItemCode.Equals(q.MaterialCode) && i.WhsCode.Equals(q.WhsCode)).FirstOrDefault()?.OnHand,
                q.MaterialCode,
                q.MaterialDescription,
                q.MaterialType,
                q.QuotationId,
                q.SentQuantity,
                q.Count,
                q.Unit,
                q.Id
            }).OrderBy(q => q.MaterialCode).ToList();
            return result;
        }

        /// <summary>
        /// 打印销售订单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        public async Task<byte[]> PrintSalesOrder(string QuotationId)
        {
            var quotationId = int.Parse(QuotationId);
            var model = await UnitWork.Find<Quotation>(q => q.Id.Equals(quotationId) && q.QuotationStatus < 10).Include(q => q.QuotationMergeMaterials).Include(q => q.QuotationOperationHistorys).FirstOrDefaultAsync();
            if (model != null || model == null)
            {
                throw new Exception("暂未开放销售订单打印，请前往3.0打印。");
                //throw new Exception("已出库，不可打印。");
            }
            var serverOrder = await UnitWork.Find<ServiceOrder>(q => q.Id.Equals(model.ServiceOrderId)).FirstOrDefaultAsync();
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_AcquisitionWay") || u.TypeId.Equals("SYS_DeliveryMethod")).Select(u => new { u.Name, u.TypeId, u.DtValue, u.Description }).ToListAsync();
            var createTime = Convert.ToDateTime(model.QuotationOperationHistorys.Where(q => q.ApprovalStage == "6.0").FirstOrDefault()?.CreateTime).ToString("yyyy.MM.dd");
            var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "SalesOrderHeader.html");
            var text = System.IO.File.ReadAllText(url);
            text = text.Replace("@Model.SalesOrderId", model.SalesOrderId.ToString());
            text = text.Replace("@Model.CreateTime", createTime);
            text = text.Replace("@Model.SalesUser", model?.CreateUser.ToString());
            text = text.Replace("@Model.QRcode", QRCoderHelper.CreateQRCodeToBase64(model.SalesOrderId.ToString()));
            text = text.Replace("@Model.CustomerId", serverOrder?.TerminalCustomerId.ToString());
            text = text.Replace("@Model.CollectionAddress", model?.CollectionAddress.ToString());
            text = text.Replace("@Model.ShippingAddress", model?.ShippingAddress.ToString());
            text = text.Replace("@Model.CustomerName", serverOrder?.TerminalCustomer.ToString());
            text = text.Replace("@Model.NewestContacter", serverOrder?.NewestContacter.ToString());
            text = text.Replace("@Model.NewestContactTel", serverOrder?.NewestContactTel.ToString());
            text = text.Replace("@Model.DeliveryMethod", CategoryList.Where(c => c.DtValue.Equals(model?.DeliveryMethod) && c.TypeId.Equals("SYS_DeliveryMethod")).FirstOrDefault()?.Name);
            text = text.Replace("@Model.AcquisitionWay", CategoryList.Where(c => c.DtValue.Equals(model?.AcquisitionWay) && c.TypeId.Equals("SYS_AcquisitionWay")).FirstOrDefault()?.Name);
            text = text.Replace("@Model.DeliveryDate", Convert.ToDateTime(model?.DeliveryDate).ToString("yyyy.MM.dd"));
            text = text.Replace("@Model.AcceptancePeriod", Convert.ToDateTime(model?.DeliveryDate).AddDays(model.AcceptancePeriod == null ? 0 : (double)model.AcceptancePeriod).ToString("yyyy.MM.dd"));
            text = text.Replace("@Model.Remark", model?.Remark);
            string InvoiceCompany = "", Location = "", website = "", seal = "", width = "", height = "";

            if (Convert.ToInt32(model.InvoiceCompany) == 1)
            {
                InvoiceCompany = "深圳市新威尔电子有限公司 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 交通银行股份有限公司&nbsp;&nbsp;深圳梅林支行 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;443066388018001726113";
                Location = "深圳市福田区梅林街道梅都社区中康路 128 号卓越梅林中心广场(北区)3 号楼 1206 电话：0755-83108866 免费服务专线：800-830-8866";
                website = "www.neware.com.cn &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
                seal = "新威尔";
                width = "350px";
                height = "350px";
            }
            else if (Convert.ToInt32(model.InvoiceCompany) == 2)
            {
                InvoiceCompany = "东莞新威检测技术有限公司 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 交通银行股份有限公司 &nbsp;&nbsp; 东莞塘厦支行 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;483007618018810043352";
                Location = "广东省东莞市塘厦镇龙安路5号5栋101室";
                seal = "东莞新威";
                width = "182px";
                height = "193px";
                text = text.Replace("@Model.logo", "hidden='hidden'");
            }
            var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"SalesOrderHeader{model.Id}.html");
            System.IO.File.WriteAllText(tempUrl, text, Encoding.Unicode);
            var footUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "SalesOrderFooter.html");
            var foottext = System.IO.File.ReadAllText(footUrl);
            foottext = foottext.Replace("@Model.Corporate", InvoiceCompany);
            foottext = foottext.Replace("@Model.PrintNo", model.PrintNo);
            foottext = foottext.Replace("@Model.Location", Location);
            foottext = foottext.Replace("@Model.Website", website);
            foottext = foottext.Replace("@Model.PrintTheNumber", (model.PrintTheNumber + 1).ToString());
            foottext = foottext.Replace("@Model.seal", seal);
            foottext = foottext.Replace("@Model.width", width);
            foottext = foottext.Replace("@Model.height", height);
            var foottempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"SalesOrderFooter{model.Id}.html");
            System.IO.File.WriteAllText(foottempUrl, foottext, Encoding.Unicode);
            var materials = model.QuotationMergeMaterials.Select(q => new PrintSalesOrderResp
            {
                MaterialCode = q.MaterialCode,
                MaterialDescription = q.MaterialDescription,
                Count = q.Count.ToString(),
                Unit = q.Unit,
                SalesPrice = q.MaterialType == 1 ? 0.00M : (decimal)q.DiscountPrices,
                TotalPrice = q.MaterialType == 1 ? 0.00M : (decimal)q.TotalPrice
            }).OrderBy(m => m.MaterialCode).ToList();
            var datas = await ExportAllHandler.Exporterpdf(materials, "PrintSalesOrder.cshtml", pdf =>
            {
                pdf.IsWriteHtml = true;
                pdf.PaperKind = PaperKind.A4;
                pdf.Orientation = Orientation.Portrait;
                pdf.IsEnablePagesCount = true;
                pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                pdf.FooterSettings = new FooterSettings() { HtmUrl = foottempUrl };
            });
            System.IO.File.Delete(tempUrl);
            System.IO.File.Delete(foottempUrl);
            await UnitWork.UpdateAsync<Quotation>(q => q.Id.Equals(quotationId), q => new Quotation { PrintTheNumber = q.PrintTheNumber + 1 });
            await UnitWork.SaveAsync();
            return datas;
        }

        /// <summary>
        /// 打印报价单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        public async Task<byte[]> PrintQuotation(string QuotationId)
        {
            var quotationId = int.Parse(QuotationId);
            var model = await UnitWork.Find<Quotation>(q => q.Id.Equals(quotationId)).Include(q => q.QuotationMergeMaterials).Include(q => q.QuotationOperationHistorys).FirstOrDefaultAsync();
            var serverOrder = await UnitWork.Find<ServiceOrder>(q => q.Id.Equals(model.ServiceOrderId)).FirstOrDefaultAsync();
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_AcquisitionWay") || u.TypeId.Equals("SYS_DeliveryMethod")).Select(u => new { u.Name, u.TypeId, u.DtValue, u.Description }).ToListAsync();

            var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Quotationheader.html");
            var text = System.IO.File.ReadAllText(url);
            text = text.Replace("@Model.QuotationId", model.Id.ToString());
            text = text.Replace("@Model.CreateTime", model.CreateTime.ToString("yyyy.MM.dd hh:mm"));
            text = text.Replace("@Model.SalesUser", model?.CreateUser.ToString());
            text = text.Replace("@Model.QRcode", QRCoderHelper.CreateQRCodeToBase64(model.Id.ToString()));
            text = text.Replace("@Model.CustomerId", serverOrder?.TerminalCustomerId.ToString());
            text = text.Replace("@Model.CollectionAddress", model?.CollectionAddress.ToString());
            text = text.Replace("@Model.ShippingAddress", model?.ShippingAddress.ToString());
            text = text.Replace("@Model.CustomerName", serverOrder?.TerminalCustomer.ToString());
            text = text.Replace("@Model.NewestContacter", serverOrder?.NewestContacter.ToString());
            text = text.Replace("@Model.NewestContactTel", serverOrder?.NewestContactTel.ToString());
            text = text.Replace("@Model.DeliveryMethod", CategoryList.Where(c => c.DtValue.Equals(model?.DeliveryMethod) && c.TypeId.Equals("SYS_DeliveryMethod")).FirstOrDefault()?.Name);
            text = text.Replace("@Model.AcquisitionWay", CategoryList.Where(c => c.DtValue.Equals(model?.AcquisitionWay) && c.TypeId.Equals("SYS_AcquisitionWay")).FirstOrDefault()?.Name);
            text = text.Replace("@Model.DeliveryDate", Convert.ToDateTime(model?.DeliveryDate).ToString("yyyy.MM.dd"));
            text = text.Replace("@Model.AcceptancePeriod", Convert.ToDateTime(model?.DeliveryDate).AddDays(model.AcceptancePeriod == null ? 0 : (double)model.AcceptancePeriod).ToString("yyyy.MM.dd"));
            text = text.Replace("@Model.Remark", model?.Remark);
            var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"Quotationheader{model.Id}.html");
            System.IO.File.WriteAllText(tempUrl, text);
            var footerUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Quotationfooter.html");
            var materials = model.QuotationMergeMaterials.Select(q => new PrintSalesOrderResp
            {
                MaterialCode = q.MaterialCode,
                MaterialDescription = q.MaterialDescription,
                Count = q.Count.ToString(),
                Unit = q.Unit,
                SalesPrice = q.MaterialType == 1 ? 0.00M : (decimal)q.DiscountPrices,
                TotalPrice = q.MaterialType == 1 ? 0.00M : (decimal)q.TotalPrice
            }).OrderBy(q => q.MaterialCode).ToList();
            var datas = await ExportAllHandler.Exporterpdf(materials, "PrintQuotation.cshtml", pdf =>
            {
                pdf.IsWriteHtml = true;
                pdf.PaperKind = PaperKind.A4;
                pdf.Orientation = Orientation.Portrait;
                pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                pdf.FooterSettings = new FooterSettings() { HtmUrl = footerUrl };
            });
            System.IO.File.Delete(tempUrl);
            return datas;
        }

        /// <summary>
        /// 打印交货单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task PrintStockRequisition(List<QuotationMergeMaterialReq> req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var model = await UnitWork.Find<Quotation>(q => q.Id == req.FirstOrDefault().QuotationId && q.QuotationStatus >= 10).Include(q => q.QuotationOperationHistorys).FirstOrDefaultAsync();
            var QuotationMergeMaterial = new List<QuotationMergeMaterial>();
            if (loginContext.Roles.Any(r => r.Name.Equals("仓库")) && req.Count > 0)
            {
                var ids = req.Select(m => m.Id).ToList();
                QuotationMergeMaterial = await UnitWork.Find<QuotationMergeMaterial>(q => ids.Contains(q.Id)).ToListAsync();
                QuotationMergeMaterial.ForEach(q =>
                {
                    q.Count = req.Where(m => m.Id == q.Id).FirstOrDefault().SentQuantity;
                });
            }
            else
            {
                QuotationMergeMaterial = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId == req.FirstOrDefault().QuotationId).ToListAsync();
            }
            if (model != null)
            {
                var serverOrder = await UnitWork.Find<ServiceOrder>(q => q.Id.Equals(model.ServiceOrderId)).FirstOrDefaultAsync();
                //var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_AcquisitionWay") || u.TypeId.Equals("SYS_DeliveryMethod")).Select(u => new { u.Name, u.TypeId, u.DtValue, u.Description }).ToListAsync();
                var SecondId = (await UnitWork.Find<Relevance>(r => r.FirstId.Equals(model.CreateUserId) && r.Key.Equals(Define.USERORG)).FirstOrDefaultAsync()).SecondId;
                var orgName = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => o.Id.Equals(SecondId)).Select(o => o.Name).FirstOrDefaultAsync();

                //var createTime = Convert.ToDateTime(model.QuotationOperationHistorys.Where(q => q.ApprovalStage.Equals("4")).FirstOrDefault()?.CreateTime).ToString("yyyy.MM.dd");
                var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "StockRequisitionHeader.html");
                var text = System.IO.File.ReadAllText(url);
                text = text.Replace("@Model.PickingList", model.Id.ToString());
                text = text.Replace("@Model.CreateTime", DateTime.Now.ToString("yyyy.MM.dd"));
                text = text.Replace("@Model.QRcode", QRCoderHelper.CreateQRCodeToBase64(model.Id.ToString()));
                text = text.Replace("@Model.OrgName", orgName);
                var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"StockRequisitionHeader{model.Id}.html");
                System.IO.File.WriteAllText(tempUrl, text, Encoding.Unicode);
                var footUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "StockRequisitionFooter.html");
                var foottext = System.IO.File.ReadAllText(footUrl);
                foottext = foottext.Replace("@Model.User", loginContext.User.Name);
                var foottempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"StockRequisitionFooter{model.Id}.html");
                System.IO.File.WriteAllText(foottempUrl, foottext, Encoding.Unicode);
                var materialList = QuotationMergeMaterial.Select(m => m.MaterialCode).ToList();
                var locationList = await UnitWork.Query<v_storeitemstock>(@$"select ItemCode,layer_no,unit_no,shelf_nm from v_storeitemstock").Where(v => materialList.Contains(v.ItemCode)).Select(v => new v_storeitemstock { ItemCode = v.ItemCode, layer_no = v.shelf_nm + "-" + v.layer_no + "-" + v.unit_no }).ToListAsync();

                var materials = QuotationMergeMaterial.Select(q => new PrintSalesOrderResp
                {
                    MaterialCode = q.MaterialCode,
                    MaterialDescription = q.MaterialDescription,
                    Count = q.Count.ToString(),
                    Unit = q.Unit,
                    ServiceOrderSapId = model.ServiceOrderSapId.ToString(),
                    SalesOrder = model.SalesOrderId.ToString(),
                    WhsCode = q.WhsCode,
                    Location = locationList.Where(l => l.ItemCode.Equals(q.MaterialCode)).FirstOrDefault()?.layer_no
                }).OrderBy(q => q.MaterialCode).ToList();

                var datas = await ExportAllHandler.Exporterpdf(materials, "StockRequisitionList.cshtml", pdf =>
                {
                    pdf.IsWriteHtml = true;
                    pdf.PaperKind = PaperKind.A5;
                    pdf.IsEnablePagesCount = true;
                    pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                    pdf.FooterSettings = new FooterSettings() { HtmUrl = foottempUrl, Right = "[page]/[toPage]" };
                });
                System.IO.File.Delete(tempUrl);
                System.IO.File.Delete(foottempUrl);
                await RedisHelper.AppendAsync(req.FirstOrDefault().QuotationId.ToString(), datas);
            }
            else
            {
                throw new Exception("暂无此领料单，请核对后重试。");
            }
        }

        /// <summary>
        /// 打印交货单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        public async Task<byte[]> PrintStockRequisition(string QuotationId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var b = await RedisHelper.GetAsync<byte[]>(QuotationId);
            await RedisHelper.DelAsync(QuotationId);
            var IsPrintWarehouse = (await UnitWork.Find<Quotation>(q => q.Id == int.Parse(QuotationId)).FirstOrDefaultAsync())?.PrintWarehouse;
            if (IsPrintWarehouse == null || IsPrintWarehouse != 3)
            {
                await UnitWork.UpdateAsync<Quotation>(q => q.Id == int.Parse(QuotationId), q => new Quotation { PrintWarehouse = 3 });
            }
            await UnitWork.AddAsync<QuotationOperationHistory>(new QuotationOperationHistory
            {
                Action = "仓库打印",
                ApprovalStage = "-1",
                CreateTime = DateTime.Now,
                CreateUser = loginContext.User.Name,
                CreateUserId = loginContext.User.Id,
                QuotationId = int.Parse(QuotationId)
            });
            await UnitWork.SaveAsync();
            return b;
        }

        /// <summary>
        /// 打印装箱清单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <param name="IsTrue"></param>
        /// <returns></returns>
        public async Task<byte[]> PrintPickingList(string QuotationId, bool? IsTrue)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            List<LogisticsRecord> logisticsRecords = new List<LogisticsRecord>();
            string Action = "技术员打印";
            if (!(bool)IsTrue)
            {
                Action = "仓库打印";
                var expressageList = await UnitWork.Find<Expressage>(e => e.Id.Equals(QuotationId)).Include(e => e.LogisticsRecords).FirstOrDefaultAsync();
                QuotationId = expressageList.QuotationId.ToString();
                logisticsRecords = expressageList.LogisticsRecords.ToList();
            }
            var quotationId = int.Parse(QuotationId);
            var model = await UnitWork.Find<Quotation>(q => q.Id.Equals(quotationId)).Include(q => q.QuotationMergeMaterials).FirstOrDefaultAsync();
            var serverOrder = await UnitWork.Find<ServiceOrder>(q => q.Id.Equals(model.ServiceOrderId)).FirstOrDefaultAsync();
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_AcquisitionWay") || u.TypeId.Equals("SYS_DeliveryMethod")).Select(u => new { u.Name, u.TypeId, u.DtValue, u.Description }).ToListAsync();

            var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "PickingListHeader.html");
            var text = System.IO.File.ReadAllText(url);
            text = text.Replace("@Model.QuotationId", model.Id.ToString());
            text = text.Replace("@Model.SalesOrderId", model.SalesOrderId.ToString());
            text = text.Replace("@Model.CreateTime", DateTime.Now.ToString("yyyy.MM.dd hh:mm"));//model.CreateTime.ToString("yyyy.MM.dd hh:mm")
            text = text.Replace("@Model.SalesUser", model?.CreateUser.ToString());
            text = text.Replace("@Model.QRcode", QRCoderHelper.CreateQRCodeToBase64(model.Id.ToString()));
            text = text.Replace("@Model.CustomerId", serverOrder?.TerminalCustomerId.ToString());
            text = text.Replace("@Model.CollectionAddress", model?.CollectionAddress.ToString());
            text = text.Replace("@Model.ShippingAddress", model?.ShippingAddress.ToString());
            text = text.Replace("@Model.CustomerName", serverOrder?.TerminalCustomer.ToString());
            text = text.Replace("@Model.NewestContacter", serverOrder?.NewestContacter.ToString());
            text = text.Replace("@Model.NewestContactTel", serverOrder?.NewestContactTel.ToString());
            text = text.Replace("@Model.DeliveryMethod", CategoryList.Where(c => c.DtValue.Equals(model?.DeliveryMethod) && c.TypeId.Equals("SYS_DeliveryMethod")).FirstOrDefault()?.Name);
            text = text.Replace("@Model.AcquisitionWay", CategoryList.Where(c => c.DtValue.Equals(model?.AcquisitionWay) && c.TypeId.Equals("SYS_AcquisitionWay")).FirstOrDefault()?.Name);
            text = text.Replace("@Model.DeliveryDate", Convert.ToDateTime(model?.DeliveryDate).ToString("yyyy.MM.dd"));
            text = text.Replace("@Model.AcceptancePeriod", Convert.ToDateTime(model?.DeliveryDate).AddDays(model.AcceptancePeriod == null ? 0 : (double)model.AcceptancePeriod).ToString("yyyy.MM.dd"));
            text = text.Replace("@Model.Remark", model?.Remark);
            var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"PickingListHeader{model.Id}.html");
            System.IO.File.WriteAllText(tempUrl, text, Encoding.Unicode);
            var footerUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"PickingListFooter.html");
            text = System.IO.File.ReadAllText(footerUrl);
            text = text.Replace("@Model.UserName", loginContext.User.Name);
            footerUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"PickingListFooter{model.Id}.html");
            System.IO.File.WriteAllText(footerUrl, text, Encoding.Unicode);
            if (logisticsRecords.Count > 0)
            {
                var ids = logisticsRecords.Select(l => l.QuotationMaterialId).ToList();
                model.QuotationMergeMaterials = model.QuotationMergeMaterials.Where(q => ids.Contains(q.Id)).Select(q => new QuotationMergeMaterial
                {
                    MaterialCode = q.MaterialCode,
                    MaterialDescription = q.MaterialDescription,
                    Count = logisticsRecords.Where(l => l.QuotationMaterialId.Equals(q.Id)).FirstOrDefault()?.Quantity,
                    Unit = q.Unit,
                    WhsCode = q.WhsCode
                }).ToList();
            }
            var materials = model.QuotationMergeMaterials.Select(q => new PrintSalesOrderResp
            {
                MaterialCode = q.MaterialCode,
                MaterialDescription = q.MaterialDescription,
                Count = q.Count.ToString(),
                Unit = q.Unit,
                WhsCode = q.WhsCode
            }).OrderBy(q => q.MaterialCode).ToList();
            var datas = await ExportAllHandler.Exporterpdf(materials, "PrintPickingList.cshtml", pdf =>
            {
                pdf.IsWriteHtml = true;
                pdf.PaperKind = PaperKind.A4;
                pdf.Orientation = Orientation.Portrait;
                pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                pdf.FooterSettings = new FooterSettings() { HtmUrl = footerUrl };
            });
            System.IO.File.Delete(tempUrl);
            System.IO.File.Delete(footerUrl);
            var IsPrintWarehouse = (await UnitWork.Find<Quotation>(q => q.Id == int.Parse(QuotationId)).FirstOrDefaultAsync())?.PrintWarehouse;
            if (IsPrintWarehouse == null || IsPrintWarehouse != 3)
            {
                await UnitWork.UpdateAsync<Quotation>(q => q.Id == int.Parse(QuotationId), q => new Quotation { PrintWarehouse = 2 });
            }
            await UnitWork.AddAsync<QuotationOperationHistory>(new QuotationOperationHistory
            {
                Action = Action,
                ApprovalStage = "-1",
                CreateTime = DateTime.Now,
                CreateUser = loginContext.User.Name,
                CreateUserId = loginContext.User.Id,
                QuotationId = int.Parse(QuotationId)
            });
            await UnitWork.SaveAsync();
            return datas;
        }

        /// <summary>
        /// 同步销售订单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        public async Task SyncSalesOrder(string QuotationId)
        {
            _capBus.Publish("Serve.SellOrder.ERPCreate", int.Parse(QuotationId));
        }

        /// <summary>
        /// 同步销售交货
        /// </summary>
        /// <param name="SalesOfDeliveryId"></param>
        /// <returns></returns>
        public async Task SyncSalesOfDelivery(string SalesOfDeliveryId)
        {
            _capBus.Publish("Serve.SalesOfDelivery.ERPCreate", int.Parse(SalesOfDeliveryId));
        }

        /// <summary>
        /// 清空交货记录
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        public async Task EmptyDeliveryRecord(string QuotationId)
        {
            var expressages = await UnitWork.Find<Expressage>(e => e.QuotationId == int.Parse(QuotationId)).Include(e => e.ExpressagePicture).Include(e => e.LogisticsRecords).ToListAsync();
            var picture = new List<ExpressagePicture>();
            expressages.ForEach(e => picture.AddRange(e.ExpressagePicture));
            var logisticsRecords = new List<LogisticsRecord>();
            expressages.ForEach(e => logisticsRecords.AddRange(e.LogisticsRecords));
            await UnitWork.BatchDeleteAsync<ExpressagePicture>(picture.ToArray());
            await UnitWork.BatchDeleteAsync<LogisticsRecord>(logisticsRecords.ToArray());
            await UnitWork.BatchDeleteAsync<Expressage>(expressages.ToArray());
            await UnitWork.UpdateAsync<Quotation>(q => q.Id == int.Parse(QuotationId), q => new Quotation { QuotationStatus = 10 });
            await UnitWork.UpdateAsync<QuotationMergeMaterial>(q => q.QuotationId == int.Parse(QuotationId), q => new QuotationMergeMaterial { SentQuantity = 0 });
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 取消销售订单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        public async Task CancellationSalesOrder(string QuotationId)
        {
            _capBus.Publish("Serve.SellOrder.Cancel", int.Parse(QuotationId));
            var quotationObj = await UnitWork.Find<Quotation>(q => q.Id == int.Parse(QuotationId)).FirstOrDefaultAsync();
            if (!string.IsNullOrWhiteSpace(quotationObj.FlowInstanceId))
            {
                await _flowInstanceApp.ReCall(new RecallFlowInstanceReq { FlowInstanceId = quotationObj.FlowInstanceId });
            }
        }

        /// <summary>
        /// 同步销售订单
        /// </summary>
        /// <returns></returns>
        public async Task SyncSalesOrderStatus()
        {
            var salesOrderIds = await UnitWork.Find<Quotation>(q => string.IsNullOrWhiteSpace(q.SalesOrderId.ToString()) && q.QuotationStatus != -1M && q.CreateTime > Convert.ToDateTime("2021.05.25")).Select(q => q.SalesOrderId).ToListAsync();
            var oRDRS = await UnitWork.Find<ORDR>(o => salesOrderIds.Contains(o.DocEntry) && (o.DocStatus == "C" || o.CANCELED == "Y")).Select(o => new { o.DocEntry, o.DocStatus, o.CANCELED }).ToListAsync();
            var cANCELEDORDR = oRDRS.Where(o => o.CANCELED == "Y").ToList();
            if (cANCELEDORDR.Count() > 0)
            {
                var cANCELEDORDRIds = cANCELEDORDR.Select(c => c.DocEntry).ToList();
                await UnitWork.UpdateAsync<Quotation>(q => q.QuotationStatus != -1M && cANCELEDORDRIds.Contains((int)q.SalesOrderId), q => new Quotation { QuotationStatus = -1 });
            }
            var statusORDR = oRDRS.Where(o => o.DocStatus == "C").ToList();
            if (statusORDR.Count() > 0)
            {
                var statusORDRIds = statusORDR.Select(c => c.DocEntry).ToList();
                await UnitWork.UpdateAsync<Quotation>(q => q.QuotationStatus != 11M && statusORDRIds.Contains((int)q.SalesOrderId), q => new Quotation { QuotationStatus = 11 });
            }
            await UnitWork.SaveAsync();
        }

        public QuotationApp(IUnitWork unitWork, ICapPublisher capBus, FlowInstanceApp flowInstanceApp, WorkbenchApp workbenchApp, ModuleFlowSchemeApp moduleFlowSchemeApp, IAuth auth) : base(unitWork, auth)
        {
            _flowInstanceApp = flowInstanceApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _capBus = capBus;
            _workbenchApp = workbenchApp;
        }

    }
}
