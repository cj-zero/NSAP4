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

namespace OpenAuth.App.Material
{
    /// <summary>
    /// 报价单操作
    /// </summary>
    public class QuotationApp : OnlyUnitWorkBaeApp
    {
        private readonly FlowInstanceApp _flowInstanceApp;

        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;

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
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(null).WhereIf(!string.IsNullOrWhiteSpace(request.CardCode), q => q.CustomerId.Contains(request.CardCode) || q.CustomerName.Contains(request.CardCode)).Select(s => new { s.Id, s.TerminalCustomer, s.TerminalCustomerId }).ToListAsync();
            var ServiceOrderids = ServiceOrders.Select(s => s.Id).ToList();
            var Quotations = UnitWork.Find<Quotation>(null).Include(q => q.QuotationPictures).WhereIf(request.QuotationId.ToString() != null, q => q.Id.ToString().Contains(request.QuotationId.ToString()))
                                .WhereIf(request.ServiceOrderSapId != null, q => q.ServiceOrderSapId.ToString().Contains(request.ServiceOrderSapId.ToString()))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUser), q => q.CreateUser.Contains(request.CreateUser))
                                .WhereIf(request.StartCreateTime != null, q => q.CreateTime > request.StartCreateTime)
                                .WhereIf(request.EndCreateTime != null, q => q.CreateTime < request.EndCreateTime)
                                .WhereIf(request.Status != null, q => q.Status == request.Status)
                                .Where(q => ServiceOrderids.Contains(q.ServiceOrderId));

            if (request.PageStart != null && request.PageStart == 1)
            {

                if (loginContext.Roles.Any(r => r.Name.Equals("物料工程审批")))
                {
                    switch (request.StartType)
                    {
                        case 1:
                            Quotations = Quotations.Where(q => q.QuotationStatus == 4);
                            break;

                        case 2:
                            Quotations = Quotations.Where(q => q.QuotationStatus > 4);
                            break;
                        default:
                            Quotations = Quotations.Where(q => q.QuotationStatus >= 4);
                            break;
                    }
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")))
                {
                    switch (request.StartType)
                    {
                        case 1:
                            Quotations = Quotations.Where(q => q.QuotationStatus == 5);
                            break;

                        case 2:
                            Quotations = Quotations.Where(q => q.QuotationStatus > 5);
                            break;
                        default:
                            Quotations = Quotations.Where(q => q.QuotationStatus >= 5);
                            break;
                    }
                }
                else
                {
                    switch (request.StartType)
                    {
                        case 1:
                            Quotations = Quotations.Where(q => q.QuotationStatus == 6 && q.CreateUserId.Equals(loginUser.Id));
                            break;

                        case 2:
                            Quotations = Quotations.Where(q => q.QuotationStatus > 6 && q.CreateUserId.Equals(loginUser.Id));
                            break;
                        default:
                            Quotations = Quotations.Where(q => q.QuotationStatus >= 6 && q.CreateUserId.Equals(loginUser.Id));
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
                            Quotations = Quotations.Where(q => q.QuotationStatus == 8);
                            break;

                        case 2:
                            Quotations = Quotations.Where(q => q.QuotationStatus > 8);
                            break;
                        default:
                            Quotations = Quotations.Where(q => q.QuotationStatus >= 8);
                            break;
                    }
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")))
                {
                    switch (request.StartType)
                    {
                        case 1:
                            Quotations = Quotations.Where(q => q.QuotationStatus == 9);
                            break;

                        case 2:
                            Quotations = Quotations.Where(q => q.QuotationStatus > 9);
                            break;
                        default:
                            Quotations = Quotations.Where(q => q.QuotationStatus >= 9);
                            break;
                    }

                }
                else
                {
                    switch (request.StartType)
                    {
                        case 1:
                            Quotations = Quotations.Where(q => q.QuotationStatus == 7 && q.CreateUserId.Equals(loginUser.Id));
                            break;

                        case 2:
                            Quotations = Quotations.Where(q => q.QuotationStatus > 7 && q.CreateUserId.Equals(loginUser.Id));
                            break;
                        default:
                            Quotations = Quotations.Where(q => q.QuotationStatus >= 7 && q.CreateUserId.Equals(loginUser.Id));
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
                    case 1://未出库
                        Quotations = Quotations.Where(q => q.QuotationStatus == 10);
                        break;

                    case 2://已出库
                        Quotations = Quotations.Where(q => q.QuotationStatus == 11);
                        break;
                    default:
                        Quotations = Quotations.Where(q => q.QuotationStatus >= 10);
                        break;
                }
            }
            else
            {
                Quotations = Quotations.Where(q => q.CreateUserId.Equals(loginUser.Id));
            }
            var QuotationDate = await Quotations.Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            List<string> fileids = new List<string>();
            QuotationDate.ForEach(q => fileids.AddRange(q.QuotationPictures.Select(p => p.PictureId).ToList()));

            var file = await UnitWork.Find<UploadFile>(f => fileids.Contains(f.Id)).ToListAsync();

            var query = from a in QuotationDate
                        join b in ServiceOrders on a.ServiceOrderId equals b.Id
                        select new { a, b };

            result.Data = query.Select(q => new
            {
                q.a.Id,
                q.a.ServiceOrderSapId,
                q.a.ServiceOrderId,
                q.b.TerminalCustomer,
                q.b.TerminalCustomerId,
                q.a.TotalMoney,
                q.a.CreateUser,
                q.a.Remark,
                q.a.SalesOrderId,
                CreateTime = q.a.CreateTime.ToString("yyyy.MM.dd"),
                q.a.QuotationStatus,
                q.a.Tentative,
                q.a.IsProtected,
                files = q.a.QuotationPictures.Select(p => new
                {
                    fileName = file.Where(f => f.Id.Equals(p.PictureId)).FirstOrDefault().FileName,
                    fileType = file.Where(f => f.Id.Equals(p.PictureId)).FirstOrDefault().FileType,
                    fileId = p.PictureId
                }).ToList()
            }).ToList();
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
            var ServiceOrders = from a in UnitWork.Find<ServiceOrder>(null)
                                join b in UnitWork.Find<ServiceWorkOrder>(s => s.Status < 7) on a.Id equals b.ServiceOrderId
                                select new { a, b };
            ServiceOrders = ServiceOrders.WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderId.ToString()), s => s.a.Id.Equals(request.ServiceOrderId))
                .WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderSapId.ToString()), s => s.a.U_SAP_ID.Equals(request.ServiceOrderSapId));
            var ServiceOrderList = (await ServiceOrders.Where(s => s.b.CurrentUserNsapId.Equals(loginUser.Id)).ToListAsync()).GroupBy(s => s.a.Id).Select(s => s.First());
            var CustomerIds = ServiceOrderList.Select(s => s.a.CustomerId).ToList();
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

            var ServiceWorkOrderList = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId.Equals(request.ServiceOrderId) && s.CurrentUserNsapId.Equals(loginUser.Id) && !s.MaterialCode.Equals("无序列号"))
                .WhereIf(!string.IsNullOrWhiteSpace(request.MaterialType), s => s.MaterialCode.Substring(0, 2) == request.MaterialType)
                .WhereIf(!string.IsNullOrWhiteSpace(request.ManufacturerSerialNumbers), s => s.ManufacturerSerialNumber.Contains(request.ManufacturerSerialNumbers))
                .WhereIf(!string.IsNullOrWhiteSpace(request.MaterialCode), s => s.MaterialCode.Contains(request.MaterialCode))
                .Select(s => new { s.ManufacturerSerialNumber, s.MaterialCode, s.MaterialDescription }).ToListAsync();
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

                result.Data = ServiceWorkOrderList.Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select(s => new
                {
                    SalesOrder = MnfSerialList.Where(m => m.MnfSerial.Equals(s.ManufacturerSerialNumber) && m.BaseType == 17)?.Max(m => m.BaseEntry),
                    ProductionOrder = MnfSerialList.Where(m => m.MnfSerial.Equals(s.ManufacturerSerialNumber) && m.BaseType == 202)?.Max(m => m.BaseEntry),
                    ManufacturerSerialNumber = s.ManufacturerSerialNumber,
                    MaterialCode = s.MaterialCode,
                    MaterialDescription = s.MaterialDescription,
                    IsProtected = IsProtecteds.Where(i => i.MnfSerial.Equals(s.ManufacturerSerialNumber)).FirstOrDefault()?.DocDate > DateTime.Now ? true : false,
                    DocDate = IsProtecteds.Where(i => i.MnfSerial.Equals(s.ManufacturerSerialNumber)).FirstOrDefault()?.DocDate
                }).ToList();
            }
            else
            {
                result.Data = ServiceWorkOrderList;
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

            var Equipments = await UnitWork.Query<SysEquipmentColumn>(@$"select a.ItemCode,b.MnfSerial,c.ItemName,c.BuyUnitMsr,d.OnHand, d.WhsCode,a.BaseQty as Quantity ,c.lastPurPrc from WOR1 a 
						join (SELECT a.BaseEntry,c.MnfSerial,a.BaseType
            FROM oitl a left join itl1 b
            on a.LogEntry = b.LogEntry and a.ItemCode = b.ItemCode 
            left join osrn c on b.ItemCode = c.ItemCode and b.SysNumber = c.SysNumber
            where a.DocType in (15, 59) and c.MnfSerial ='{request.ManufacturerSerialNumbers}' and a.BaseType=202) b on a.docentry = b.BaseEntry	
						join OITM c on a.itemcode = c.itemcode
						join OITW d on a.itemcode=d.itemcode 
						where d.WhsCode=37").WhereIf(!string.IsNullOrWhiteSpace(request.PartCode), s => s.ItemCode.Contains(request.PartCode))
                        .Select(s => new SysEquipmentColumn { ItemCode = s.ItemCode, MnfSerial = s.MnfSerial, ItemName = s.ItemName, BuyUnitMsr = s.BuyUnitMsr, OnHand = s.OnHand, WhsCode = s.WhsCode, Quantity = s.Quantity, lastPurPrc = s.lastPurPrc }).ToListAsync();

            if (Equipments == null || Equipments.Count() <= 0)
            {
                request.MaterialCode = request.MaterialCode.Replace("'", "''");
                Equipments = await UnitWork.Query<SysEquipmentColumn>(@$"select a.* ,c.lastPurPrc from (select a.Father as MnfSerial,a.Code as ItemCode,a.U_Desc as ItemName,a.U_DUnit as BuyUnitMsr,b.OnHand,b.WhsCode,a.Quantity
                        from ITT1 a join OITW b on a.Code=b.ItemCode  where a.Father='{request.MaterialCode}' and b.WhsCode=37) a join OITM c on c.ItemCode=a.ItemCode")
                    .WhereIf(!string.IsNullOrWhiteSpace(request.PartCode), s => s.ItemCode.Contains(request.PartCode))
                    .Select(s => new SysEquipmentColumn { ItemCode = s.ItemCode, MnfSerial = s.MnfSerial, ItemName = s.ItemName, BuyUnitMsr = s.BuyUnitMsr, OnHand = s.OnHand, WhsCode = s.WhsCode, Quantity = s.Quantity, lastPurPrc = s.lastPurPrc }).ToListAsync();
            }
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



            var EquipmentsList = Equipments.Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToList();
            EquipmentsList.ForEach(e =>
            {
                e.MnfSerial = request.ManufacturerSerialNumbers;
                var Prices = MaterialPrices.Where(m => m.MaterialCode.Equals(e.ItemCode)).FirstOrDefault();
                //4.0存在物料价格，取4.0的价格为售后结算价，不存在就当前进货价*1.2 为售后结算价。销售价均为售后结算价*3
                if (Prices != null)
                {
                    e.UnitPrice = Prices?.SettlementPrice <= 0 ? e.lastPurPrc * Prices?.SettlementPriceModel : Prices?.SettlementPrice;
                    var s = e.UnitPrice.ToDouble().ToString();
                    if (s.IndexOf(".") > 0)
                    {
                        s = s.Substring(s.IndexOf("."), s.Length - s.IndexOf("."));
                        if (s.Length > 1)
                        {
                            int lengeth = s.Substring(1, s.Length - 1).Length;
                            if (lengeth > 3) e.UnitPrice = e.UnitPrice + 0.005M;
                        }
                    }
                    e.UnitPrice = Math.Round((decimal)e.UnitPrice, 2);
                }
                else
                {
                    e.UnitPrice = e.lastPurPrc * 1.2M;
                    var s = e.UnitPrice.ToDouble().ToString();
                    if (s.IndexOf(".") > 0)
                    {
                        s = s.Substring(s.IndexOf("."), s.Length - s.IndexOf("."));
                        if (s.Length > 1)
                        {
                            int lengeth = s.Substring(1, s.Length - 1).Length;
                            if (lengeth > 3) e.UnitPrice = e.UnitPrice + 0.005M;
                        }
                    }
                    e.UnitPrice = Math.Round((decimal)e.UnitPrice, 2);

                }
                e.lastPurPrc = e.UnitPrice * 3;
            });
            result.Data = EquipmentsList;
            result.Count = Equipments.Count();
            return result;
        }

        /// <summary>
        /// 获取报价单详情
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        public async Task<TableData> GetDetails(int QuotationId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var Quotations = await UnitWork.Find<Quotation>(q => q.Id.Equals(QuotationId)).Include(q => q.QuotationPictures).Include(q => q.QuotationProducts).ThenInclude(p => p.QuotationMaterials).Include(q => q.QuotationOperationHistorys).FirstOrDefaultAsync();
            var quotationsMap = Quotations.MapTo<AddOrUpdateQuotationReq>();
            List<string> materialCodes = new List<string>();
            Quotations.QuotationProducts.ForEach(q => {

                materialCodes.AddRange(q.QuotationMaterials.Select(m => m.MaterialCode).ToList());
            });
            var ItemCodes= await UnitWork.Find<OITW>(o => materialCodes.Contains(o.ItemCode) && o.WhsCode=="37").Select(o=>new { o.ItemCode,o.WhsCode,o.OnHand}).ToListAsync();
            quotationsMap.QuotationProducts.ForEach(p =>
                p.QuotationMaterials.ForEach(m =>
                    {
                        m.WarehouseNumber = ItemCodes.Where(i => i.ItemCode.Equals(m.MaterialCode)).FirstOrDefault()?.WhsCode;
                        m.WarehouseQuantity= ItemCodes.Where(i => i.ItemCode.Equals(m.MaterialCode)).FirstOrDefault()?.OnHand;
                    }
                )
            );

            var result = new TableData();
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(Quotations.ServiceOrderId)).Select(s => new { s.Id, s.U_SAP_ID, s.TerminalCustomer, s.TerminalCustomerId, s.SalesMan, s.NewestContacter, s.NewestContactTel }).FirstOrDefaultAsync();
            var CustomerInformation = await UnitWork.Find<OCRD>(o => o.CardCode.Equals(ServiceOrders.TerminalCustomerId)).Select(o => new { o.BackOrder, frozenFor = o.frozenFor == "N" ? "正常" : "冻结" }).FirstOrDefaultAsync();
            var QuotationMergeMaterials = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(QuotationId)).ToListAsync();

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
                        m.b.Quantity
                    }).ToList()
                }).ToList();
                quotationsMap.QuotationOperationHistorys = quotationsMap.QuotationOperationHistorys.OrderBy(q => q.CreateTime).ToList();
                result.Data = new
                {
                    Expressages,
                    Quotations=quotationsMap,
                    QuotationMergeMaterials,
                    ServiceOrders,
                    CustomerInformation
                };
            }
            else
            {
                quotationsMap.QuotationOperationHistorys = quotationsMap.QuotationOperationHistorys.OrderBy(q => q.CreateTime).ToList();
                result.Data = new
                {
                    Quotations=quotationsMap,
                    QuotationMergeMaterials,
                    ServiceOrders,
                    CustomerInformation
                };
            }


            return result;
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
            var result = new TableData();
            var query = from a in UnitWork.Find<OITM>(null).WhereIf(!string.IsNullOrWhiteSpace(request.PartCode), q => q.ItemCode.Contains(request.PartCode))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.PartDescribe), q => q.ItemName.Contains(request.PartDescribe))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.ReplacePartCode),q=>!q.ItemCode.Equals(request.ReplacePartCode))
                        join b in UnitWork.Find<OITW>(null) on a.ItemCode equals b.ItemCode into ab
                        from b in ab.DefaultIfEmpty()
                        where b.WhsCode == "37"
                        select new SysEquipmentColumn {ItemCode=a.ItemCode,ItemName=a.ItemName,lastPurPrc=a.LastPurPrc,BuyUnitMsr=a.SalUnitMsr,OnHand=b.OnHand,WhsCode=b.WhsCode };
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
                    e.UnitPrice = Prices?.SettlementPrice <= 0 ? e.lastPurPrc * Prices?.SettlementPriceModel : Prices?.SettlementPrice;
                    var s = e.UnitPrice.ToDouble().ToString();
                    if (s.IndexOf(".") > 0)
                    {
                        s = s.Substring(s.IndexOf("."), s.Length - s.IndexOf("."));
                        if (s.Length > 1)
                        {
                            int lengeth = s.Substring(1, s.Length - 1).Length;
                            if (lengeth > 3) e.UnitPrice = e.UnitPrice + 0.005M;
                        }
                    }
                    e.UnitPrice = Math.Round((decimal)e.UnitPrice, 2);
                }
                else
                {
                    e.UnitPrice = e.lastPurPrc * 1.2M;
                    var s = e.UnitPrice.ToDouble().ToString();
                    if (s.IndexOf(".") > 0)
                    {
                        s = s.Substring(s.IndexOf("."), s.Length - s.IndexOf("."));
                        if (s.Length > 1)
                        {
                            int lengeth = s.Substring(1, s.Length - 1).Length;
                            if (lengeth > 3) e.UnitPrice = e.UnitPrice + 0.005M;
                        }
                    }
                    e.UnitPrice = Math.Round((decimal)e.UnitPrice, 2);

                }
                e.lastPurPrc = e.UnitPrice * 3;
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

            var QuotationMergeMaterials = await UnitWork.Find<QuotationMergeMaterial>(q => QuotationIds.Contains((int)q.QuotationId) && q.IsProtected == true).ToListAsync();
            result.Data = QuotationMergeMaterials;
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

            var Message = await Condition(obj);

            var QuotationObj = obj.MapTo<Quotation>();
            QuotationObj.PrintNo = Guid.NewGuid().ToString();
            QuotationObj.Tentative = false;
            QuotationObj.ErpOrApp = 1;
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppId));
                QuotationObj.ErpOrApp = 2;
            }
            //判定人员是否有销售员code
            var slpcode = (await UnitWork.Find<OSLP>(o => o.SlpName.Equals(loginUser.Name)).FirstOrDefaultAsync())?.SlpCode;
            if (slpcode == null || slpcode == 0)
            {
                throw new Exception("暂无销售权限，请联系呼叫中心");
            }
            QuotationObj.IsProtected = true;
            QuotationObj.TotalMoney = 0;
            QuotationObj.TotalCostPrice = 0;
            QuotationObj.QuotationProducts.ForEach(q =>
            {
                if (!(bool)q.IsProtected)
                {
                    QuotationObj.IsProtected = false;
                    foreach (var item in q.QuotationMaterials)
                    {
                        QuotationObj.TotalMoney += item.SalesPrice * item.Count;
                    }
                }
                else
                {
                    foreach (var item in q.QuotationMaterials)
                    {
                        QuotationObj.TotalCostPrice += item.UnitPrice * item.Count;
                    }
                    q.QuotationMaterials.ForEach(q => q.SalesPrice = 0);
                }
            });
            if (QuotationObj.ServiceCharge != null && QuotationObj.ServiceCharge > 0)
            {
                QuotationObj.TotalMoney += QuotationObj.ServiceCharge;
            }
            var dbContext = UnitWork.GetDbContext<Quotation>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    QuotationObj.QuotationProducts.ForEach(q => q.QuotationMaterials.ForEach(m => m.Id = Guid.NewGuid().ToString()));
                    QuotationObj.CreateTime = DateTime.Now;
                    QuotationObj.CreateUser = loginUser.Name;
                    QuotationObj.CreateUserId = loginUser.Id;
                    QuotationObj.Status = 1;
                    QuotationObj.QuotationStatus = 3;
                    QuotationObj = await UnitWork.AddAsync<Quotation, int>(QuotationObj);
                    await UnitWork.SaveAsync();
                    if (!obj.IsDraft)
                    {
                        QuotationObj.QuotationStatus = 4;

                        #region 创建审批流程
                        //var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("物料报价单"));
                        //var afir = new AddFlowInstanceReq();
                        //afir.SchemeId = mf.FlowSchemeId;
                        //afir.FrmType = 2;
                        //afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();

                        //afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\",\"IsProtected\":\"" + QuotationObj.IsProtected + "\"}";

                        //afir.CustomName = $"物料报价单" + DateTime.Now;
                        //afir.OrgId = "";
                        //var FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                        //QuotationObj.FlowInstanceId = FlowInstanceId;
                        await UnitWork.UpdateAsync<Quotation>(QuotationObj);
                        #endregion

                        #region 合并零件表
                        List<QuotationMaterial> QuotationMaterialsT = new List<QuotationMaterial>();
                        QuotationObj.QuotationProducts.Where(q => q.IsProtected == true).ToList().ForEach(q => QuotationMaterialsT.AddRange(q.QuotationMaterials));
                        List<QuotationMaterial> QuotationMaterialsF = new List<QuotationMaterial>();
                        QuotationObj.QuotationProducts.Where(q => q.IsProtected == false).ToList().ForEach(q => QuotationMaterialsF.AddRange(q.QuotationMaterials));


                        var MaterialsT = from a in QuotationMaterialsT
                                         group a by new { a.MaterialCode, a.MaterialDescription, a.Unit, a.SalesPrice, a.UnitPrice, a.Discount } into g
                                         select new QueryQuotationMergeMaterialListReq
                                         {
                                             MaterialCode = g.Key.MaterialCode,
                                             MaterialDescription = g.Key.MaterialDescription,
                                             Unit = g.Key.Unit,
                                             SalesPrice = 0,
                                             CostPrice = g.Key.UnitPrice,
                                             Count = g.Sum(a => a.Count),
                                             TotalPrice = 0,//g.Sum(a => a.TotalPrice)
                                             IsProtected = true,
                                             QuotationId = QuotationObj.Id,
                                             Margin = 0,
                                             Discount = g.Key.Discount,
                                             SentQuantity = 0
                                         };

                        var MaterialsF = from a in QuotationMaterialsF
                                         group a by new { a.MaterialCode, a.MaterialDescription, a.Unit, a.SalesPrice, a.UnitPrice, a.Discount } into g
                                         select new QueryQuotationMergeMaterialListReq
                                         {
                                             MaterialCode = g.Key.MaterialCode,
                                             MaterialDescription = g.Key.MaterialDescription,
                                             Unit = g.Key.Unit,
                                             SalesPrice = g.Key.SalesPrice,
                                             CostPrice = g.Key.UnitPrice,
                                             Count = g.Sum(a => a.Count),
                                             TotalPrice = (g.Key.SalesPrice * g.Sum(a => a.Count)) * g.Key.Discount,//g.Sum(a => a.TotalPrice)
                                             IsProtected = false,
                                             QuotationId = QuotationObj.Id,
                                             Margin = ((g.Key.SalesPrice * g.Sum(a => a.Count)) * g.Key.Discount) - (g.Key.UnitPrice * g.Sum(a => a.Count)),
                                             Discount = g.Key.Discount,
                                             SentQuantity = 0
                                         };

                        var QuotationMergeMaterialList = MaterialsT.ToList();
                        QuotationMergeMaterialList.AddRange(MaterialsF.ToList());

                        if (QuotationObj.ServiceCharge != null && QuotationObj.ServiceCharge > 0)
                        {
                            QuotationMergeMaterialList.Add(new QueryQuotationMergeMaterialListReq
                            {
                                MaterialCode = "S111-SERVICE-GSF",
                                MaterialDescription = "服务费",
                                Unit = "PCS",
                                SalesPrice = QuotationObj.ServiceCharge,
                                CostPrice = 0,
                                Count = 1,
                                TotalPrice = QuotationObj.ServiceCharge,
                                IsProtected = false,
                                QuotationId = QuotationObj.Id,
                                Margin = QuotationObj.ServiceCharge,
                                Discount = 1,
                                SentQuantity = 1
                            });
                        }
                        var QuotationMergeMaterialListMap = QuotationMergeMaterialList.MapToList<QuotationMergeMaterial>();
                        await UnitWork.BatchAddAsync<QuotationMergeMaterial>(QuotationMergeMaterialListMap.ToArray());
                        await UnitWork.AddAsync<QuotationOperationHistory>(new QuotationOperationHistory
                        {
                            Action = "报价单提交审批",
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            CreateTime = DateTime.Now,
                            QuotationId = QuotationObj.Id
                        });
                        await UnitWork.SaveAsync();
                        #endregion

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

            var Message = await Condition(obj);
            var QuotationObj = obj.MapTo<Quotation>();
            QuotationObj.TotalMoney = 0;
            QuotationObj.TotalCostPrice = 0;
            QuotationObj.Tentative = false;
            QuotationObj.ErpOrApp = 1;
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppId));
                QuotationObj.ErpOrApp = 2;
            }
            //判定人员是否有销售员code
            var slpcode = (await UnitWork.Find<OSLP>(o => o.SlpName.Equals(loginUser.Name)).FirstOrDefaultAsync())?.SlpCode;
            if (slpcode == null || slpcode == 0)
            {
                throw new Exception("暂无销售权限，请联系呼叫中心");
            }
            QuotationObj.IsProtected = true;
            QuotationObj.QuotationProducts.ForEach(q =>
            {
                if (!(bool)q.IsProtected)
                {
                    QuotationObj.IsProtected = false;
                    foreach (var item in q.QuotationMaterials)
                    {
                        QuotationObj.TotalMoney += item.SalesPrice * item.Count;
                    }
                }
                else
                {
                    foreach (var item in q.QuotationMaterials)
                    {
                        QuotationObj.TotalCostPrice += item.UnitPrice * item.Count;
                    }
                    q.QuotationMaterials.ForEach(q => q.SalesPrice = 0);
                }
            });
            if (QuotationObj.ServiceCharge != null && QuotationObj.ServiceCharge > 0)
            {
                QuotationObj.TotalMoney += QuotationObj.ServiceCharge;
            }

            var dbContext = UnitWork.GetDbContext<Quotation>();
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    #region 删除

                    var QuotationProducts = await UnitWork.Find<QuotationProduct>(q => q.QuotationId.Equals(QuotationObj.Id)).Include(q => q.QuotationMaterials).ToListAsync();

                    if (QuotationProducts != null && QuotationProducts.Count > 0)
                    {
                        var QuotationMaterials = new List<QuotationMaterial>();
                        QuotationProducts.ForEach(q => QuotationMaterials.AddRange(q.QuotationMaterials));
                        if (QuotationMaterials != null && QuotationMaterials.Count > 0)
                        {
                            await UnitWork.BatchDeleteAsync<QuotationMaterial>(QuotationMaterials.ToArray());
                        }
                        await UnitWork.BatchDeleteAsync<QuotationProduct>(QuotationProducts.ToArray());
                    }

                    var QuotationMergeMaterials = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(QuotationObj.Id)).ToListAsync();

                    if (QuotationMergeMaterials != null && QuotationMergeMaterials.Count > 0)
                    {
                        await UnitWork.BatchDeleteAsync<QuotationMergeMaterial>(QuotationMergeMaterials.ToArray());
                    }

                    await UnitWork.SaveAsync();

                    #endregion

                    #region 新增
                    if (QuotationObj.QuotationProducts != null && QuotationObj.QuotationProducts.Count > 0)
                    {
                        var QuotationProductMap = QuotationObj.QuotationProducts.MapToList<QuotationProduct>();
                        QuotationProductMap.ForEach(q =>
                        {
                            q.QuotationMaterials.ForEach(m => m.Id = Guid.NewGuid().ToString());
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
                            Remark = QuotationObj.Remark,
                            IsDraft = QuotationObj.IsDraft,
                            IsProtected = QuotationObj.IsProtected,
                            Status = 1,
                            //todo:要修改的字段赋值
                        });
                        await UnitWork.SaveAsync();


                    }
                    else
                    {
                        #region 合并零件表
                        List<QuotationMaterial> QuotationMaterialsT = new List<QuotationMaterial>();
                        QuotationObj.QuotationProducts.Where(q => q.IsProtected == true).ToList().ForEach(q => QuotationMaterialsT.AddRange(q.QuotationMaterials));
                        List<QuotationMaterial> QuotationMaterialsF = new List<QuotationMaterial>();
                        QuotationObj.QuotationProducts.Where(q => q.IsProtected == false).ToList().ForEach(q => QuotationMaterialsF.AddRange(q.QuotationMaterials));

                        var MaterialsT = from a in QuotationMaterialsT
                                         group a by new { a.MaterialCode, a.MaterialDescription, a.Unit, a.SalesPrice, a.UnitPrice, a.Discount } into g
                                         select new QueryQuotationMergeMaterialListReq
                                         {
                                             MaterialCode = g.Key.MaterialCode,
                                             MaterialDescription = g.Key.MaterialDescription,
                                             Unit = g.Key.Unit,
                                             SalesPrice = 0,
                                             CostPrice = g.Key.UnitPrice,
                                             Count = g.Sum(a => a.Count),
                                             TotalPrice = 0,//g.Sum(a => a.TotalPrice)
                                             IsProtected = true,
                                             QuotationId = QuotationObj.Id,
                                             Margin = 0,
                                             Discount = g.Key.Discount,
                                             SentQuantity = 0
                                         };

                        var MaterialsF = from a in QuotationMaterialsF
                                         group a by new { a.MaterialCode, a.MaterialDescription, a.Unit, a.SalesPrice, a.UnitPrice, a.Discount } into g
                                         select new QueryQuotationMergeMaterialListReq
                                         {
                                             MaterialCode = g.Key.MaterialCode,
                                             MaterialDescription = g.Key.MaterialDescription,
                                             Unit = g.Key.Unit,
                                             SalesPrice = g.Key.SalesPrice,
                                             CostPrice = g.Key.UnitPrice,
                                             Count = g.Sum(a => a.Count),
                                             TotalPrice = (g.Key.SalesPrice * g.Sum(a => a.Count)) * g.Key.Discount,//g.Sum(a => a.TotalPrice)
                                             IsProtected = false,
                                             QuotationId = QuotationObj.Id,
                                             Margin = ((g.Key.SalesPrice * g.Sum(a => a.Count)) * g.Key.Discount) - (g.Key.UnitPrice * g.Sum(a => a.Count)),
                                             Discount = g.Key.Discount,
                                             SentQuantity = 0
                                         };

                        var QuotationMergeMaterialList = MaterialsT.ToList();
                        QuotationMergeMaterialList.AddRange(MaterialsF.ToList());
                        if (QuotationObj.ServiceCharge != null && QuotationObj.ServiceCharge > 0)
                        {
                            QuotationMergeMaterialList.Add(new QueryQuotationMergeMaterialListReq
                            {
                                MaterialCode = "S111-SERVICE-GSF",
                                MaterialDescription = "服务费",
                                Unit = "PCS",
                                SalesPrice = QuotationObj.ServiceCharge,
                                CostPrice = 0,
                                Count = 1,
                                TotalPrice = QuotationObj.ServiceCharge,
                                IsProtected = false,
                                QuotationId = QuotationObj.Id,
                                Margin = QuotationObj.ServiceCharge,
                                Discount = 1,
                                SentQuantity = 1
                            });
                        }
                        var QuotationMergeMaterialListMap = QuotationMergeMaterialList.MapToList<QuotationMergeMaterial>();
                        await UnitWork.BatchAddAsync<QuotationMergeMaterial>(QuotationMergeMaterialListMap.ToArray());
                        await UnitWork.SaveAsync();
                        #endregion

                        #region 创建审批流程
                        //var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("物料报价单"));
                        //var afir = new AddFlowInstanceReq();
                        //afir.SchemeId = mf.FlowSchemeId;
                        //afir.FrmType = 2;
                        //afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                        //bool IsProtected = false;
                        //obj.QuotationProducts.ForEach(q =>
                        //{
                        //    if (q.IsProtected == true) IsProtected = (bool)q.IsProtected;
                        //});
                        //afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\"+\"IsProtected\":\"" + QuotationObj.IsProtected + "\"}";

                        //afir.CustomName = $"物料报价单" + DateTime.Now;
                        //afir.OrgId = "";
                        //var FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                        //QuotationObj.FlowInstanceId = FlowInstanceId;
                        QuotationObj.QuotationStatus = 4;
                        await UnitWork.UpdateAsync<Quotation>(QuotationObj);
                        #endregion

                        await UnitWork.UpdateAsync<Quotation>(u => u.Id == QuotationObj.Id, u => new Quotation
                        {
                            QuotationStatus = 4,
                            CollectionAddress = QuotationObj.CollectionAddress,
                            ShippingAddress = QuotationObj.ShippingAddress,
                            DeliveryMethod = QuotationObj.DeliveryMethod,
                            InvoiceCompany = QuotationObj.InvoiceCompany,
                            TotalMoney = QuotationObj.TotalMoney,
                            Remark = QuotationObj.Remark,
                            IsDraft = QuotationObj.IsDraft,
                            IsProtected = QuotationObj.IsProtected,
                            Status = 1,
                            //FlowInstanceId = FlowInstanceId,
                            //todo:要修改的字段赋值
                        });
                        await UnitWork.AddAsync<QuotationOperationHistory>(new QuotationOperationHistory
                        {
                            Action = "报价单提交审批",
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            CreateTime = DateTime.Now,
                            QuotationId = QuotationObj.Id
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
        /// 修改报价单物料，物流
        /// </summary>
        /// <param name="obj"></param>
        public async Task<TableData> UpdateMaterial(AddOrUpdateQuotationReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var expressageobj = new Expressage();
            var expressageMap = obj.ExpressageReqs.MapTo<Expressage>();
            #region 判断库存量
            var mergeMaterialIds = obj.QuotationMergeMaterialReqs.Select(q => q.Id).ToList();
            var mergeMaterials = await UnitWork.Find<QuotationMergeMaterial>(q => mergeMaterialIds.Contains(q.Id)).Select(q => q.MaterialCode).ToListAsync();
            var onHand = await UnitWork.Find<OITW>(o => mergeMaterials.Contains(o.ItemCode) && o.WhsCode == "37").Select(o => new { o.ItemCode, o.OnHand }).ToListAsync();
            string message = null;
            onHand.Where(o => o.OnHand <= 0).ForEach(o =>
                 message += o.ItemCode + "  "
             );
            if (!string.IsNullOrWhiteSpace(message))
            {
                throw new Exception(message + "库存为零，不可交货");
            }
            #endregion
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
                            CreateUser = loginContext.User.Name,
                            CreateUserId = loginContext.User.Id,
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
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("添加物流失败,请重试。" + ex.Message);
                }
            }
            var Expressages = await UnitWork.Find<Expressage>(e => e.QuotationId.Equals(obj.ExpressageReqs.QuotationId)).Include(e => e.ExpressagePicture).Include(e => e.LogisticsRecords).OrderByDescending(e => e.CreateTime).ToListAsync();
            LogisticsRecords = new List<LogisticsRecord>();
            Expressages.ForEach(e => LogisticsRecords.AddRange(e.LogisticsRecords));
            var QuotationMergeMaterialLists = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(obj.ExpressageReqs.QuotationId)).ToListAsync();

            _capBus.Publish("Serve.SalesOfDelivery.Create", obj);

            int isEXwarehouse = 0;
            foreach (var item in QuotationMergeMaterialLists)
            {
                if (item.SentQuantity != item.Count)
                {
                    isEXwarehouse++;
                }
            }
            if (isEXwarehouse == 0)
            {
                await UnitWork.UpdateAsync<Quotation>(q => q.Id.Equals(obj.ExpressageReqs.QuotationId), q => new Quotation { QuotationStatus = 11 });
                await UnitWork.SaveAsync();
            }
            var result = new TableData();
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
                        m.b.Quantity
                    }).ToList()
                }).ToList()
            };
            return result;
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

            var obj = await UnitWork.Find<Quotation>(q => q.Id == req.Id).Include(q => q.QuotationProducts).FirstOrDefaultAsync();

            qoh.ApprovalStage = obj.QuotationStatus;
            if (loginContext.Roles.Any(r => r.Name.Equals("物料工程审批")) && obj.QuotationStatus == 4)
            {
                qoh.Action = "工程审批";
                obj.QuotationStatus = 5;
            }
            else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")) && obj.QuotationStatus == 5)
            {
                qoh.Action = "总经理审批";
                if ((bool)obj.IsProtected && obj.TotalMoney <= 0)
                {
                    if (req.IsTentative == true)
                    {
                        obj.QuotationStatus = 5;
                        obj.Tentative = true;
                    }
                    else
                    {
                        obj.Tentative = false;
                        obj.QuotationStatus = 9;
                        #region 同步到SAP 并拿到服务单主键
                        _capBus.Publish("Serve.SellOrder.Create", obj.Id);
                        #endregion
                    }

                }
                else
                {
                    obj.QuotationStatus = 6;
                }

            }
            else if (obj.CreateUserId.Equals(loginUser.Id) && obj.QuotationStatus == 6)
            {
                qoh.Action = "客户确认报价单";
                obj.QuotationStatus = 7;
                #region 同步到SAP 并拿到服务单主键

                _capBus.Publish("Serve.SellOrder.Create", obj.Id);
                #endregion
            }
            else if (obj.CreateUserId.Equals(loginUser.Id) && obj.QuotationStatus == 7)
            {
                qoh.Action = "销售订单成立";
                obj.QuotationStatus = 8;
            }
            else if (loginContext.Roles.Any(r => r.Name.Equals("物料财务")) && obj.QuotationStatus == 8)
            {
                qoh.Action = "财务审批";
                if (req.IsTentative == true)
                {
                    obj.QuotationStatus = 8;
                    obj.Tentative = true;
                }
                else
                {
                    obj.QuotationStatus = 9;
                }

            }
            else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")) && obj.QuotationStatus == 9)
            {
                qoh.Action = "总经理审批";
                obj.QuotationStatus = 10;
                obj.Status = 2;
            }
            else
            {
                throw new Exception("暂无审批该流程权限，不可审批");
            }
            VerificationReq VerificationReqModle = new VerificationReq();
            if (req.IsReject)
            {
                //VerificationReqModle = new VerificationReq
                //{
                //    NodeRejectStep = "start round mix-d23658c21ec6479f8e9226c69281648e",
                //    NodeRejectType = "2",
                //    FlowInstanceId = obj.FlowInstanceId,
                //    VerificationFinally = "3",
                //    VerificationOpinion = "",
                //};
                //_flowInstanceApp.Verification(VerificationReqModle);
                obj.QuotationStatus = 1;
                qoh.ApprovalResult = "驳回";
                obj.FlowInstanceId = "";
                var delQuotationMergeMaterial = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(obj.Id)).ToListAsync();
                await UnitWork.BatchDeleteAsync<QuotationMergeMaterial>(delQuotationMergeMaterial.ToArray());
            }
            else
            {
                //VerificationReqModle = new VerificationReq
                //{
                //    NodeRejectStep = "",
                //    NodeRejectType = "0",
                //    FlowInstanceId = obj.FlowInstanceId,
                //    VerificationFinally = "1",
                //    VerificationOpinion = "同意",
                //};
                if (req.IsTentative == true)
                {
                    obj.QuotationStatus = qoh.ApprovalStage;
                    obj.Tentative = true;
                    qoh.ApprovalResult = "暂定";
                }
                else
                {
                    qoh.ApprovalResult = "同意";
                }
                //_flowInstanceApp.Verification(VerificationReqModle);
            }
            await UnitWork.UpdateAsync<Quotation>(obj);
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
            qoh.IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(selqoh.CreateTime)).TotalMinutes);
            await UnitWork.AddAsync<QuotationOperationHistory>(qoh);
            await UnitWork.SaveAsync();
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
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(req.AppId));
            }
            var obj = await UnitWork.Find<Quotation>(q => q.Id.Equals(req.QuotationId)).Include(q => q.QuotationOperationHistorys)
                .Include(q => q.Expressages).Include(q => q.QuotationProducts).ThenInclude(p => p.QuotationMaterials).FirstOrDefaultAsync();
            var Materials = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(req.QuotationId)).ToListAsync();
            List<QuotationMaterial> QuotationMaterials = new List<QuotationMaterial>();
            foreach (var item in obj.QuotationProducts)
            {
                QuotationMaterials.AddRange(item.QuotationMaterials.ToList());
            }
            await UnitWork.BatchDeleteAsync<QuotationProduct>(obj.QuotationProducts.ToArray());
            await UnitWork.BatchDeleteAsync<QuotationMaterial>(QuotationMaterials.ToArray());
            await UnitWork.BatchDeleteAsync<Expressage>(obj.Expressages.ToArray());
            await UnitWork.BatchDeleteAsync<QuotationOperationHistory>(obj.QuotationOperationHistorys.ToArray());
            await UnitWork.BatchDeleteAsync<QuotationMergeMaterial>(Materials.ToArray());
            await UnitWork.DeleteAsync<Quotation>(obj);
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="AppId"></param>
        /// <returns></returns>
        private async Task<User> GetUserId(int AppId)
        {
            var userid = UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(AppId)).Select(u => u.UserID).FirstOrDefault();

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
                .Take<MaterialPrice>(0);
                return data.Select(d => d.Value).SkipWhile(v => v is null).ToList();
            });
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
            var ReturnNoteList = await UnitWork.Find<ReturnNote>(r => r.CreateUserId.Equals(loginUser.Id)).Include(r => r.ReturnnoteMaterials).ToListAsync();

            List<int> returnNoteIds = ReturnNoteList.Select(s => s.Id).Distinct().ToList();
            //计算剩余未结清金额
            var notClearAmountList = (await UnitWork.Find<ReturnnoteMaterial>(w => returnNoteIds.Contains((int)w.ReturnNoteId) && w.Check == 1).ToListAsync()).GroupBy(g => g.MaterialCode).Select(s => new { s.Key, Count = s.Sum(s => s.Count), TotalWrongCount = s.Sum(s => s.WrongCount), Costprice = s.ToList().FirstOrDefault().CostPrice, TotalCount = s.ToList().FirstOrDefault().TotalCount }).ToList();
            var totalprice = notClearAmountList.Sum(s => s.Costprice * (s.TotalCount - s.Count + s.TotalWrongCount));
            if (totalprice > 4000)
            {
                throw new Exception("欠款已超出额度，不可领料。");
            }
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

            return null;
        }

        /// <summary>
        /// 计算价格
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private async Task<Quotation> CalculatePrice(AddOrUpdateQuotationReq obj)
        {
            var quotationObj = obj.MapTo<Quotation>();

            List<string> itemCode = new List<string>();

            foreach (var item in quotationObj.QuotationProducts)
            {
                itemCode.AddRange(item.QuotationMaterials.Select(q => q.MaterialCode).ToList());
            }
            var purchasePrice = await UnitWork.Find<OITM>(o => itemCode.Contains(o.ItemCode)).Select(o => new { o.ItemCode, o.LastPurPrc }).ToListAsync();
            var materialPrices = await UnitWork.Find<MaterialPrice>(m => itemCode.Contains(m.MaterialCode)).ToListAsync();

            var query = from a in materialPrices
                        join b in purchasePrice on a.MaterialCode equals b.ItemCode
                        select new { a, b };

            quotationObj.QuotationProducts.ForEach(products =>
            {
                products.QuotationMaterials.ForEach(materials =>
                {
                    var materialCodes = query.Where(q => q.a.MaterialCode.Equals(materials.MaterialCode)).FirstOrDefault();
                    if (materialCodes != null)
                    {
                        var price = materialCodes.a?.SettlementPrice == null ? materialCodes.a.SettlementPriceModel * materialCodes.b.LastPurPrc : materialCodes.a?.SettlementPrice;
                        materials.SalesPrice = (price * 3) * materials.Discount;
                        materials.TotalPrice = materials.Count * materials.SalesPrice;
                        materials.UnitPrice = price;
                    }
                });

            });

            return quotationObj;
        }

        /// <summary>
        /// 获取合并后数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetMergeMaterial(QueryQuotationListReq req)
        {
            var result = new TableData();

            var QuotationMergeMaterials = UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(req.QuotationId))
                .WhereIf(!string.IsNullOrWhiteSpace(req.MaterialCode), q => req.MaterialCode.Contains(q.MaterialCode));
            //result.Count = await QuotationMergeMaterials.CountAsync();
            result.Data = await QuotationMergeMaterials.ToListAsync();
            return result;
        }

        /// <summary>
        /// 打印销售订单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        public async Task<byte[]> PrintSalesOrder(int QuotationId)
        {
            var model = await UnitWork.Find<Quotation>(q => q.Id.Equals(QuotationId)).Include(q => q.QuotationMergeMaterials).Include(q => q.QuotationOperationHistorys).FirstOrDefaultAsync();
            var serverOrder = await UnitWork.Find<ServiceOrder>(q => q.Id.Equals(model.ServiceOrderId)).FirstOrDefaultAsync();
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_AcquisitionWay") || u.TypeId.Equals("SYS_DeliveryMethod")).Select(u => new { u.Name, u.TypeId, u.DtValue, u.Description }).ToListAsync();

            var createTime = Convert.ToDateTime(model.QuotationOperationHistorys.Where(q => q.ApprovalStage.Equals(4)).FirstOrDefault()?.CreateTime).ToString("yyyy.MM.dd");
            var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "SalesOrderHeader.html");
            var text = System.IO.File.ReadAllText(url);
            text = text.Replace("@Model.SalesOrderId", model.SalesOrderId.ToString());
            text = text.Replace("@Model.CreateTime", createTime);
            text = text.Replace("@Model.SalesUser", model?.CreateUser.ToString());
            text = text.Replace("@Model.QRcode", QRCoderHelper.CreateQRCodeToBase64(model.Id.ToString()));
            text = text.Replace("@Model.CustomerId", serverOrder?.CustomerId.ToString());
            text = text.Replace("@Model.CollectionAddress", model?.CollectionAddress.ToString());
            text = text.Replace("@Model.ShippingAddress", model?.ShippingAddress.ToString());
            text = text.Replace("@Model.CustomerName", serverOrder?.CustomerName.ToString());
            text = text.Replace("@Model.NewestContacter", serverOrder?.NewestContacter.ToString());
            text = text.Replace("@Model.NewestContactTel", serverOrder?.NewestContactTel.ToString());
            text = text.Replace("@Model.DeliveryMethod", CategoryList.Where(c => c.DtValue.Equals(model?.DeliveryMethod) && c.TypeId.Equals("SYS_AcquisitionWay")).FirstOrDefault()?.Name);
            text = text.Replace("@Model.AcquisitionWay", CategoryList.Where(c => c.DtValue.Equals(model?.AcquisitionWay) && c.TypeId.Equals("SYS_DeliveryMethod")).FirstOrDefault()?.Name);
            text = text.Replace("@Model.DeliveryDate", Convert.ToDateTime(model?.DeliveryDate).ToString("yyyy.MM.dd"));
            text = text.Replace("@Model.AcceptancePeriod", Convert.ToDateTime(model?.DeliveryDate).AddDays(model.AcceptancePeriod == null ? 0 : (double)model.AcceptancePeriod).ToString("yyyy.MM.dd"));
            text = text.Replace("@Model.Remark", model?.Remark);
            var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"SalesOrderHeader{model.Id}.html");
            System.IO.File.WriteAllText(tempUrl, text);
            var footUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "SalesOrderFooter.html");
            var foottext = System.IO.File.ReadAllText(footUrl);
            string InvoiceCompany = "";
            if (Convert.ToInt32(model.InvoiceCompany) == 1)
            {
                InvoiceCompany = "深圳市新威尔电子有限公司 交通银行股份有限公司深圳梅林支行   443066388018001726113";
            }
            else if (Convert.ToInt32(model.InvoiceCompany) == 2)
            {
                InvoiceCompany = "东莞新威检测技术有限公司 交通银行股份有限公司东莞塘厦支行   483007618018810043352";
            }
            foottext = foottext.Replace("@Model.Corporate", InvoiceCompany);
            foottext = foottext.Replace("@Model.PrintNo", model.PrintNo);
            var foottempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"SalesOrderFooter{model.Id}.html");
            System.IO.File.WriteAllText(foottempUrl, foottext, Encoding.UTF8);
            var materials = model.QuotationMergeMaterials.Select(q => new PrintSalesOrderResp
            {
                MaterialCode = q.MaterialCode,
                MaterialDescription = q.MaterialDescription,
                Count = q.Count.ToString(),
                Unit = q.Unit,
                SalesPrice = Convert.ToDecimal(q.SalesPrice).ToString("N"),
                TotalPrice = (decimal)q.TotalPrice
            });
            var datas = await ExportAllHandler.Exporterpdf(materials, "PrintSalesOrder.cshtml", pdf =>
            {
                pdf.IsWriteHtml = true;
                pdf.PaperKind = PaperKind.A4;
                pdf.Orientation = Orientation.Portrait;
                pdf.HeaderSettings = new HeaderSettings() { FontName = "宋体", FontSize = 6, HtmUrl = tempUrl };
                pdf.FooterSettings = new FooterSettings() { FontName = "宋体", FontSize = 6, HtmUrl = foottempUrl };
            });
            System.IO.File.Delete(tempUrl);
            System.IO.File.Delete(foottempUrl);
            return datas;
        }

        /// <summary>
        /// 打印报价单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        public async Task<byte[]> PrintQuotation(int QuotationId)
        {
            var model = await UnitWork.Find<Quotation>(q => q.Id.Equals(QuotationId)).Include(q => q.QuotationMergeMaterials).Include(q => q.QuotationOperationHistorys).FirstOrDefaultAsync();
            var serverOrder = await UnitWork.Find<ServiceOrder>(q => q.Id.Equals(model.ServiceOrderId)).FirstOrDefaultAsync();
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_AcquisitionWay") || u.TypeId.Equals("SYS_DeliveryMethod")).Select(u => new { u.Name, u.TypeId, u.DtValue, u.Description }).ToListAsync();

            var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Quotationheader.html");
            var text = System.IO.File.ReadAllText(url);
            text = text.Replace("@Model.QuotationId", model.Id.ToString());
            text = text.Replace("@Model.CreateTime", model.CreateTime.ToString("yyyy.MM.dd hh:mm"));
            text = text.Replace("@Model.SalesUser", model?.CreateUser.ToString());
            text = text.Replace("@Model.QRcode", QRCoderHelper.CreateQRCodeToBase64(model.Id.ToString()));
            text = text.Replace("@Model.CustomerId", serverOrder?.CustomerId.ToString());
            text = text.Replace("@Model.CollectionAddress", model?.CollectionAddress.ToString());
            text = text.Replace("@Model.ShippingAddress", model?.ShippingAddress.ToString());
            text = text.Replace("@Model.CustomerName", serverOrder?.CustomerName.ToString());
            text = text.Replace("@Model.NewestContacter", serverOrder?.NewestContacter.ToString());
            text = text.Replace("@Model.NewestContactTel", serverOrder?.NewestContactTel.ToString());
            text = text.Replace("@Model.DeliveryMethod", CategoryList.Where(c => c.DtValue.Equals(model?.DeliveryMethod) && c.TypeId.Equals("SYS_AcquisitionWay")).FirstOrDefault()?.Name);
            text = text.Replace("@Model.AcquisitionWay", CategoryList.Where(c => c.DtValue.Equals(model?.AcquisitionWay) && c.TypeId.Equals("SYS_DeliveryMethod")).FirstOrDefault()?.Name);
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
                SalesPrice = q.SalesPrice.ToString("N"),
                TotalPrice = (decimal)q.TotalPrice
            });
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

        public QuotationApp(IUnitWork unitWork, FlowInstanceApp flowInstanceApp, ICapPublisher capBus, ModuleFlowSchemeApp moduleFlowSchemeApp, IAuth auth) : base(unitWork, auth)
        {
            _flowInstanceApp = flowInstanceApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _capBus = capBus;
        }

    }
}
