using Infrastructure;
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

namespace OpenAuth.App.Material
{
    /// <summary>
    /// 报价单操作
    /// </summary>
    public class QuotationApp : OnlyUnitWorkBaeApp
    {
        //private readonly FlowInstanceApp _flowInstanceApp;

        //private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;

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
                ServiceOrderids = await UnitWork.Find<ServiceOrder>(null).Where(q => q.CustomerId.Contains(request.CardCode) || q.CustomerName.Contains(request.CardCode)).Select(s => s.Id).ToListAsync();

            }
            var Quotations = UnitWork.Find<Quotation>(null).Include(q => q.QuotationPictures).WhereIf(request.QuotationId.ToString() != null, q => q.Id.ToString().Contains(request.QuotationId.ToString()))
                                .WhereIf(request.ServiceOrderSapId != null, q => q.ServiceOrderSapId.ToString().Contains(request.ServiceOrderSapId.ToString()))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUser), q => q.CreateUser.Contains(request.CreateUser))
                                .WhereIf(request.StartCreateTime != null, q => q.CreateTime > request.StartCreateTime)
                                .WhereIf(request.EndCreateTime != null, q => q.CreateTime < request.EndCreateTime)
                                .WhereIf(request.Status != null, q => q.Status == request.Status)
                                .WhereIf(request.QuotationStatus != null, q => q.QuotationStatus == request.QuotationStatus)
                                .WhereIf(request.SalesOrderId != null, q => q.SalesOrderId == request.SalesOrderId)
                                .WhereIf(ServiceOrderids.Count() > 0, q => ServiceOrderids.Contains(q.ServiceOrderId));
            if (!loginContext.Roles.Any(r => r.Name.Equals("客服主管")) && !loginUser.Account.Equals(Define.SYSTEM_USERNAME))
            {
                if (request.PageStart != null && request.PageStart == 1)
                {

                    if (loginContext.Roles.Any(r => r.Name.Equals("销售员")))
                    {
                        switch (request.StartType)
                        {
                            case 1:
                                Quotations = Quotations.Where(q => q.QuotationStatus == 3.1M);
                                break;

                            case 2:
                                Quotations = Quotations.Where(q => q.QuotationStatus > 3.1M);
                                break;
                            default:
                                Quotations = Quotations.Where(q => q.QuotationStatus >= 3.1M);
                                break;
                        }
                        ServiceOrderids = await UnitWork.Find<ServiceOrder>(null).Where(q => q.SalesManId.Equals(loginContext.User.Id)).Select(s => s.Id).ToListAsync();
                        Quotations = Quotations.Where(q => ServiceOrderids.Contains(q.ServiceOrderId));
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("物料工程审批")))
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
                        ServiceOrderids = await UnitWork.Find<ServiceOrder>(null).Where(q => q.SalesManId.Equals(loginContext.User.Id)).Select(s => s.Id).ToListAsync();
                        switch (request.StartType)
                        {
                            case 1:
                                Quotations = Quotations.Where(q => q.QuotationStatus == 5 || (ServiceOrderids.Contains(q.ServiceOrderId) && q.QuotationStatus == 3.1M));
                                break;

                            case 2:
                                Quotations = Quotations.Where(q => q.QuotationStatus > 5 || (ServiceOrderids.Contains(q.ServiceOrderId) && q.QuotationStatus > 3.1M));
                                break;
                            default:
                                Quotations = Quotations.Where(q => q.QuotationStatus >= 5 || (ServiceOrderids.Contains(q.ServiceOrderId) && q.QuotationStatus >= 3.1M));
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
                            Quotations = Quotations.Where(q => q.QuotationStatus == 10 || q.QuotationStatus == 12);
                            break;

                        case 2://已出库
                            Quotations = Quotations.Where(q => q.QuotationStatus == 11);
                            break;
                        default:
                            Quotations = Quotations.Where(q => q.QuotationStatus >= 10);
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

            var QuotationDate = await Quotations.Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            List<string> fileids = new List<string>();
            QuotationDate.ForEach(q => fileids.AddRange(q.QuotationPictures.Select(p => p.PictureId).ToList()));

            var file = await UnitWork.Find<UploadFile>(f => fileids.Contains(f.Id)).ToListAsync();
            ServiceOrderids = QuotationDate.Select(q => q.ServiceOrderId).ToList();
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(null).Where(q => ServiceOrderids.Contains(q.Id)).Select(s => new { s.Id, s.TerminalCustomer, s.TerminalCustomerId }).ToListAsync();
            var query = from a in QuotationDate
                        join b in ServiceOrders on a.ServiceOrderId equals b.Id
                        select new { a, b };
            var terminalCustomerIds = query.Select(q => q.b.TerminalCustomerId).ToList();
            var ocrds = await UnitWork.Find<OCRD>(o => terminalCustomerIds.Contains(o.CardCode)).ToListAsync();
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
                CreateTime = Convert.ToDateTime(q.a.CreateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                UpDateTime = Convert.ToDateTime(q.a.UpDateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                q.a.QuotationStatus,
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
            var ServiceOrders = from a in UnitWork.Find<ServiceOrder>(s=>s.VestInOrg<=1)
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

                result.Data = ServiceWorkOrderList.Select(s => new ProductCodeListResp
                {
                    SalesOrder = MnfSerialList.Where(m => m.MnfSerial.Equals(s.ManufacturerSerialNumber) && m.BaseType == 17)?.Max(m => m.BaseEntry),
                    ProductionOrder = MnfSerialList.Where(m => m.MnfSerial.Equals(s.ManufacturerSerialNumber) && m.BaseType == 202)?.Max(m => m.BaseEntry),
                    ManufacturerSerialNumber = s.ManufacturerSerialNumber,
                    MaterialCode = s.MaterialCode,
                    MaterialDescription = s.MaterialDescription,
                    IsProtected = IsProtecteds.Where(i => i.MnfSerial.Equals(s.ManufacturerSerialNumber)).FirstOrDefault()?.DocDate > DateTime.Now ? true : false,
                    DocDate = IsProtecteds.Where(i => i.MnfSerial.Equals(s.ManufacturerSerialNumber)).OrderByDescending(s => s.DocDate).FirstOrDefault()?.DocDate,
                    FromTheme = s.FromTheme
                }).ToList();
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
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ShieldingMaterials")).Select(u => u.Name).ToListAsync();

            Equipments = Equipments.Where(e => !CategoryList.Contains(e.ItemCode)).ToList();
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
            var CustomerInformation = await UnitWork.Find<OCRD>(o => o.CardCode.Equals(ServiceOrders.TerminalCustomerId)).Select(o => new { o.BackOrder, frozenFor = o.frozenFor == "N" ? "正常" : "冻结" }).FirstOrDefaultAsync();
            var QuotationMergeMaterials = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(request.QuotationId)).ToListAsync();
            QuotationMergeMaterials = QuotationMergeMaterials.OrderBy(q => q.MaterialCode).ToList();
            Quotations.QuotationOperationHistorys = Quotations.QuotationOperationHistorys.Where(q => q.ApprovalStage != "-1").OrderBy(q => q.CreateTime).ThenByDescending(o => o.Action).ToList();
            Quotations.ServiceRelations = (await UnitWork.Find<User>(u => u.Id.Equals(Quotations.CreateUserId)).FirstOrDefaultAsync()).ServiceRelations;
            var ocrds = await UnitWork.Find<OCRD>(o => ServiceOrders.TerminalCustomerId.Equals(o.CardCode)).FirstOrDefaultAsync();
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
            var Quotations = await UnitWork.Find<Quotation>(q => q.Id == QuotationId).Include(q => q.QuotationPictures).Include(q => q.QuotationProducts).ThenInclude(p => p.QuotationMaterials).Include(q => q.QuotationOperationHistorys).FirstOrDefaultAsync();
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
                    m.TotalPrice = m.TotalPrice == 0 && m.MaterialType != "3" ? decimal.Parse(Convert.ToDecimal((m.UnitPrice * 3 * (m.Discount / 100) * m.Count)).ToString("#0.00")) : m.TotalPrice;
                    m.SalesPrice = m.SalesPrice == 0 && m.MaterialType != "3" ? decimal.Parse(Convert.ToDecimal(m.UnitPrice * 3).ToString("#0.00")) : m.SalesPrice;
                    if (m.DiscountPrices < 0) m.DiscountPrices = m.SalesPrice == 0 && m.MaterialType != "3" ? decimal.Parse(Convert.ToDecimal(m.UnitPrice * 3 * (m.Discount / 100)).ToString("#0.00")) : decimal.Parse(Convert.ToDecimal(m.SalesPrice * (m.Discount / 100)).ToString("#0.00"));
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
            if (((quotationsMap.ServiceChargeJH != null && quotationsMap.ServiceChargeJH > 0)|| (quotationsMap.ServiceChargeSM != null && quotationsMap.ServiceChargeSM > 0) || (quotationsMap.TravelExpense != null && quotationsMap.TravelExpense > 0)) && (IsUpdate == null || IsUpdate == false))
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
                if (quotationsMap.ServiceChargeJH != null && quotationsMap.ServiceChargeJH > 0)
                {
                    QuotationMergeMaterial.Add(new QuotationMaterialReq
                    {
                        MaterialCode = "S111-SERVICE-GSF-JH",
                        MaterialDescription = "寄回维修费 20210518",
                        Unit = "PCS",
                        SalesPrice = quotationsMap.ServiceChargeJH,
                        Count = quotationsMap.ServiceChargeManHourJH != null ? Convert.ToDecimal(quotationsMap.ServiceChargeManHourJH) / count : 1,
                        TotalPrice = quotationsMap.ServiceChargeManHourJH != null ? (Convert.ToDecimal(quotationsMap.ServiceChargeManHourJH) / count) * quotationsMap.ServiceChargeJH : quotationsMap.ServiceChargeJH / count,
                        Discount = 100,
                        DiscountPrices = quotationsMap.ServiceChargeJH,
                        MaterialType = "2"
                    });
                }
                if (quotationsMap.ServiceChargeSM != null && quotationsMap.ServiceChargeSM > 0)
                {
                    QuotationMergeMaterial.Add(new QuotationMaterialReq
                    {
                        MaterialCode = "S111-SERVICE-GSF-SM",
                        MaterialDescription = "上门维修费 20210518",
                        Unit = "PCS",
                        SalesPrice = quotationsMap.ServiceChargeSM,
                        Count = quotationsMap.ServiceChargeManHourSM != null ? Convert.ToDecimal(quotationsMap.ServiceChargeManHourSM) / count : 1,
                        TotalPrice = quotationsMap.ServiceChargeManHourSM != null ? (Convert.ToDecimal(quotationsMap.ServiceChargeManHourSM) / count) * quotationsMap.ServiceChargeSM : quotationsMap.ServiceChargeSM / count,
                        Discount = 100,
                        DiscountPrices = quotationsMap.ServiceChargeSM,
                        MaterialType = "2"
                    });
                }
                if (quotationsMap.TravelExpense != null && quotationsMap.TravelExpense > 0)
                {
                    QuotationMergeMaterial.Add(new QuotationMaterialReq
                    {
                        MaterialCode = "S111-SERVICE-CLF",
                        MaterialDescription = "差旅费",
                        Unit = "PCS",
                        SalesPrice = quotationsMap.TravelExpense,
                        Count = quotationsMap.TravelExpenseManHour != null ? Convert.ToDecimal(quotationsMap.TravelExpenseManHour) / count : 1,
                        TotalPrice = quotationsMap.TravelExpenseManHour != null ? (Convert.ToDecimal(quotationsMap.TravelExpenseManHour) / count) * quotationsMap.TravelExpense : quotationsMap.TravelExpense / count,
                        Discount = 100,
                        DiscountPrices = quotationsMap.TravelExpense,
                        MaterialType = "2"
                    });

                }

            }
            quotationsMap.QuotationProducts.ForEach(q =>
            {
                q.FromTheme = serialNumberList.Where(s => s.ManufacturerSerialNumber.Equals(q.ProductCode)).FirstOrDefault()?.FromTheme;
                q.QuotationMaterials.AddRange(QuotationMergeMaterial.ToList());
                q.QuotationMaterials = q.QuotationMaterials.OrderBy(m => m.MaterialCode).ToList();
            });

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
            result.Count = await query.CountAsync();
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ShieldingMaterials")).Select(u => u.Name).ToListAsync();

            var Equipments = await query.Where(e => !CategoryList.Contains(e.ItemCode)).Skip((request.page - 1) * request.limit)
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
            obj.QuotationProducts = obj.QuotationProducts.Where(q => q.QuotationMaterials.Count > 0).ToList();
            var QuotationObj = await CalculatePrice(obj);
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
                    QuotationObj.PrintWarehouse = 1;
                    QuotationObj.UpDateTime = DateTime.Now;
                    QuotationObj = await UnitWork.AddAsync<Quotation, int>(QuotationObj);
                    await UnitWork.SaveAsync();
                    if (!obj.IsDraft)
                    {
                        QuotationObj.QuotationStatus = 3.1M;

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
                        #endregion

                        await UnitWork.UpdateAsync<Quotation>(QuotationObj);
                        #region 合并零件表
                        List<QuotationMaterial> QuotationMaterials = new List<QuotationMaterial>();
                        QuotationObj.QuotationProducts.ToList().ForEach(q => QuotationMaterials.AddRange(q.QuotationMaterials));


                        var MaterialsT = from a in QuotationMaterials
                                         group a by new { a.MaterialCode, a.MaterialDescription, a.Unit, a.SalesPrice, a.UnitPrice, a.Discount, a.MaterialType, a.DiscountPrices, a.WhsCode } into g
                                         select new QueryQuotationMergeMaterialListReq
                                         {
                                             MaterialCode = g.Key.MaterialCode,
                                             MaterialDescription = g.Key.MaterialDescription,
                                             Unit = g.Key.Unit,
                                             SalesPrice = g.Key.SalesPrice,
                                             CostPrice = g.Key.UnitPrice,
                                             Count = g.Sum(a => a.Count),
                                             TotalPrice = (g.Key.DiscountPrices * g.Sum(a => a.Count)),
                                             IsProtected = false,
                                             QuotationId = QuotationObj.Id,
                                             Margin = (g.Key.DiscountPrices * g.Sum(a => a.Count)) - (g.Key.UnitPrice * g.Sum(a => a.Count)),
                                             Discount = g.Key.Discount,
                                             SentQuantity = 0,
                                             MaterialType = (int)g.Key.MaterialType,
                                             DiscountPrices = g.Key.DiscountPrices,
                                             WhsCode = g.Key.WhsCode
                                         };

                        var QuotationMergeMaterialList = MaterialsT.ToList();

                        if (QuotationObj.ServiceChargeJH != null && QuotationObj.ServiceChargeJH > 0)
                        {
                            QuotationMergeMaterialList.Add(new QueryQuotationMergeMaterialListReq
                            {
                                MaterialCode = "S111-SERVICE-GSF-JH",
                                MaterialDescription = "寄回维修费 20210518",
                                Unit = "PCS",
                                SalesPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeJH * QuotationObj.ServiceChargeManHourJH).ToString("#0.00")),
                                CostPrice = 0,
                                Count = 1,
                                TotalPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeJH * QuotationObj.ServiceChargeManHourJH).ToString("#0.00")),
                                IsProtected = false,
                                QuotationId = QuotationObj.Id,
                                Margin = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeJH * QuotationObj.ServiceChargeManHourJH).ToString("#0.00")),
                                Discount = 100,
                                SentQuantity = 0,
                                MaterialType = 2,
                                DiscountPrices = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeJH * QuotationObj.ServiceChargeManHourJH).ToString("#0.00")),
                                WhsCode = "37"
                            });
                        }
                        if (QuotationObj.ServiceChargeSM != null && QuotationObj.ServiceChargeSM > 0)
                        {
                            QuotationMergeMaterialList.Add(new QueryQuotationMergeMaterialListReq
                            {
                                MaterialCode = "S111-SERVICE-GSF-SM",
                                MaterialDescription = "上门维修费 20210518",
                                Unit = "PCS",
                                SalesPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeSM * QuotationObj.ServiceChargeManHourSM).ToString("#0.00")),
                                CostPrice = 0,
                                Count = 1,
                                TotalPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeSM * QuotationObj.ServiceChargeManHourSM).ToString("#0.00")),
                                IsProtected = false,
                                QuotationId = QuotationObj.Id,
                                Margin = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeSM * QuotationObj.ServiceChargeManHourSM).ToString("#0.00")),
                                Discount = 100,
                                SentQuantity = 0,
                                MaterialType = 2,
                                DiscountPrices = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeSM * QuotationObj.ServiceChargeManHourSM).ToString("#0.00")),
                                WhsCode = "37"
                            });
                        }
                        if (QuotationObj.TravelExpense != null && QuotationObj.TravelExpense > 0)
                        {
                            QuotationMergeMaterialList.Add(new QueryQuotationMergeMaterialListReq
                            {
                                MaterialCode = "S111-SERVICE-CLF",
                                MaterialDescription = "差旅费",
                                Unit = "PCS",
                                SalesPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.TravelExpense * QuotationObj.TravelExpenseManHour).ToString("#0.00")),
                                CostPrice = 0,
                                Count = 1,
                                TotalPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.TravelExpense * QuotationObj.TravelExpenseManHour).ToString("#0.00")),
                                IsProtected = false,
                                QuotationId = QuotationObj.Id,
                                Margin = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.TravelExpense * QuotationObj.TravelExpenseManHour).ToString("#0.00")),
                                Discount = 100,
                                SentQuantity = 0,
                                MaterialType = 2,
                                DiscountPrices = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.TravelExpense * QuotationObj.TravelExpenseManHour).ToString("#0.00")),
                                WhsCode = "37"
                            });
                        }
                        var QuotationMergeMaterialListMap = QuotationMergeMaterialList.MapToList<QuotationMergeMaterial>();
                        await UnitWork.BatchAddAsync<QuotationMergeMaterial>(QuotationMergeMaterialListMap.ToArray());
                        await UnitWork.AddAsync<QuotationOperationHistory>(new QuotationOperationHistory
                        {
                            Action = QuotationObj.ErpOrApp == 1 ? QuotationObj.CreateUser + "通过ERP提交审批" : QuotationObj.CreateUser + "通过APP提交审批",
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            CreateTime = DateTime.Now,
                            QuotationId = QuotationObj.Id,
                            ApprovalStage = "3"
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
                            PrintWarehouse = 1,
                            MoneyMeans = QuotationObj.MoneyMeans,
                            UpDateTime=DateTime.Now
                            //todo:要修改的字段赋值
                        });
                        await UnitWork.SaveAsync();


                    }
                    else
                    {
                        #region 合并零件表
                        List<QuotationMaterial> QuotationMaterials = new List<QuotationMaterial>();
                        QuotationObj.QuotationProducts.ToList().ForEach(q => QuotationMaterials.AddRange(q.QuotationMaterials));


                        var MaterialsT = from a in QuotationMaterials
                                         group a by new { a.MaterialCode, a.MaterialDescription, a.Unit, a.SalesPrice, a.UnitPrice, a.Discount, a.MaterialType, a.DiscountPrices, a.WhsCode } into g
                                         select new QueryQuotationMergeMaterialListReq
                                         {
                                             MaterialCode = g.Key.MaterialCode,
                                             MaterialDescription = g.Key.MaterialDescription,
                                             Unit = g.Key.Unit,
                                             SalesPrice = g.Key.SalesPrice,
                                             CostPrice = g.Key.UnitPrice,
                                             Count = g.Sum(a => a.Count),
                                             TotalPrice = (g.Key.DiscountPrices * g.Sum(a => a.Count)),
                                             IsProtected = false,
                                             QuotationId = QuotationObj.Id,
                                             Margin = (g.Key.DiscountPrices * g.Sum(a => a.Count)) - (g.Key.UnitPrice * g.Sum(a => a.Count)),
                                             Discount = g.Key.Discount,
                                             SentQuantity = 0,
                                             MaterialType = (int)g.Key.MaterialType,
                                             DiscountPrices = g.Key.DiscountPrices,
                                             WhsCode = g.Key.WhsCode
                                         };

                        var QuotationMergeMaterialList = MaterialsT.ToList();

                        if (QuotationObj.ServiceChargeJH != null && QuotationObj.ServiceChargeJH > 0)
                        {
                            QuotationMergeMaterialList.Add(new QueryQuotationMergeMaterialListReq
                            {
                                MaterialCode = "S111-SERVICE-GSF-JH",
                                MaterialDescription = "寄回维修费 20210518",
                                Unit = "PCS",
                                SalesPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeJH * QuotationObj.ServiceChargeManHourJH).ToString("#0.00")),
                                CostPrice = 0,
                                Count = 1,
                                TotalPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeJH * QuotationObj.ServiceChargeManHourJH).ToString("#0.00")),
                                IsProtected = false,
                                QuotationId = QuotationObj.Id,
                                Margin = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeJH * QuotationObj.ServiceChargeManHourJH).ToString("#0.00")),
                                Discount = 100,
                                SentQuantity = 0,
                                MaterialType = 2,
                                DiscountPrices = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeJH * QuotationObj.ServiceChargeManHourJH).ToString("#0.00")),
                                WhsCode = "37"
                            });
                        }
                        if (QuotationObj.ServiceChargeSM != null && QuotationObj.ServiceChargeSM > 0)
                        {
                            QuotationMergeMaterialList.Add(new QueryQuotationMergeMaterialListReq
                            {
                                MaterialCode = "S111-SERVICE-GSF-SM",
                                MaterialDescription = "上门维修费 20210518",
                                Unit = "PCS",
                                SalesPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeSM * QuotationObj.ServiceChargeManHourSM).ToString("#0.00")),
                                CostPrice = 0,
                                Count = 1,
                                TotalPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeSM * QuotationObj.ServiceChargeManHourSM).ToString("#0.00")),
                                IsProtected = false,
                                QuotationId = QuotationObj.Id,
                                Margin = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeSM * QuotationObj.ServiceChargeManHourSM).ToString("#0.00")),
                                Discount = 100,
                                SentQuantity = 0,
                                MaterialType = 2,
                                DiscountPrices = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeSM * QuotationObj.ServiceChargeManHourSM).ToString("#0.00")),
                                WhsCode = "37"
                            });
                        }
                        if (QuotationObj.TravelExpense != null && QuotationObj.TravelExpense > 0)
                        {
                            QuotationMergeMaterialList.Add(new QueryQuotationMergeMaterialListReq
                            {
                                MaterialCode = "S111-SERVICE-CLF",
                                MaterialDescription = "差旅费",
                                Unit = "PCS",
                                SalesPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.TravelExpense * QuotationObj.TravelExpenseManHour).ToString("#0.00")),
                                CostPrice = 0,
                                Count = 1,
                                TotalPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.TravelExpense * QuotationObj.TravelExpenseManHour).ToString("#0.00")),
                                IsProtected = false,
                                QuotationId = QuotationObj.Id,
                                Margin = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.TravelExpense * QuotationObj.TravelExpenseManHour).ToString("#0.00")),
                                Discount = 100,
                                SentQuantity = 0,
                                MaterialType = 2,
                                DiscountPrices = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.TravelExpense * QuotationObj.TravelExpenseManHour).ToString("#0.00")),
                                WhsCode = "37"
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
                        ////QuotationObj.FlowInstanceId = FlowInstanceId;
                        //QuotationObj.QuotationStatus = 4;
                        //await UnitWork.UpdateAsync<Quotation>(QuotationObj);
                        #endregion

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
                            PrintWarehouse = 1,
                            MoneyMeans = QuotationObj.MoneyMeans,
                            UpDateTime = DateTime.Now
                            //FlowInstanceId = FlowInstanceId,
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
            var num = await UnitWork.Find<Quotation>(q => q.Id == QuotationId && q.QuotationStatus <= 5).CountAsync();
            if (num <= 0)
            {
                throw new Exception("该报价单状态不可撤销。");
            }
            await UnitWork.UpdateAsync<Quotation>(q => q.Id == QuotationId, q => new Quotation
            {
                QuotationStatus = 2,
                FlowInstanceId = ""
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
            mergeMaterialList = mergeMaterialList.Where(q => mergeMaterialIds.Contains(q.Id) && !q.MaterialCode.Equals("S111-SERVICE-GSF") && !q.MaterialCode.Equals("S111-SERVICE-CLF")).ToList();
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
            }


            if (isEXwarehouse == 0)
            {
                await UnitWork.UpdateAsync<Quotation>(q => q.Id.Equals(obj.ExpressageReqs.QuotationId), q => new Quotation { QuotationStatus = 11 });
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
            }
            else
            {
                await UnitWork.UpdateAsync<Quotation>(q => q.Id.Equals(obj.ExpressageReqs.QuotationId), q => new Quotation { QuotationStatus = 12 });
            }
            await UnitWork.BatchAddAsync<QuotationOperationHistory>(qoh.ToArray());
            await UnitWork.SaveAsync();
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
                        m.b.Quantity,
                        m.a.WhsCode
                    }).ToList()
                }).ToList()
            };
            return result;
        }
        /// <summary>
        /// 维修费差旅费定时交货
        /// </summary>
        /// <returns></returns>
        public async Task TimeOfDelivery()
        {
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ShieldingMaterials")).Select(u => u.Name).ToListAsync();

            var quotations = await UnitWork.Find<Quotation>(q => q.QuotationStatus == 10 && q.CreateTime > Convert.ToDateTime("2021.05.25")).Include(q => q.QuotationMergeMaterials)
                .Where(q => q.QuotationMergeMaterials.Where(m => !CategoryList.Contains(m.MaterialCode)).Count() <= 0 && q.SalesOrderId != null).ToListAsync();
            foreach (var item in quotations)
            {

                var pictures = "68cc3412-492b-4f39-b7de-3ab3a957017b";
                if ((item.ServiceChargeJH > 0 || item.ServiceChargeSM > 0) && item.TravelExpense > 0)
                {
                    pictures = "701d519b-5c0a-4369-adf4-8c0a2b7f0b16";
                }
                else if (item.TravelExpense > 0)
                {
                    pictures = "01a62877-1961-4f0e-9f39-2dab2cb2eb4a";
                }
                AddOrUpdateQuotationReq obj = new AddOrUpdateQuotationReq();
                obj.ExpressageReqs = new ExpressageReq
                {
                    ExpressNumber = "自动出库",
                    Freight = "0",
                    QuotationId = item.Id,
                    ExpressagePictures = new List<string>() { pictures }
                };
                obj.QuotationMergeMaterialReqs = item.QuotationMergeMaterials.MapToList<QuotationMergeMaterialReq>();
                int num = 0;
                obj.QuotationMergeMaterialReqs.ForEach(q => q.SentQuantity = 1);
                if (num == 0 && item.IsMaterialType != null)
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

            qoh.ApprovalStage = obj.QuotationStatus.ToString();

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
                qoh.ApprovalStage = "1";
                var delQuotationMergeMaterial = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(obj.Id)).ToListAsync();
                await UnitWork.BatchDeleteAsync<QuotationMergeMaterial>(delQuotationMergeMaterial.ToArray());
            }
            else
            {
                if ((loginContext.Roles.Any(r => r.Name.Equals("销售员")) || loginContext.Roles.Any(r => r.Name.Equals("总经理"))) && obj.QuotationStatus == 3.1M)
                {
                    qoh.Action = "销售员审批";
                    obj.QuotationStatus = 4;
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("物料工程审批")) && obj.QuotationStatus == 4)
                {
                    qoh.Action = "工程审批";
                    obj.QuotationStatus = 5;
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")) && obj.QuotationStatus == 5)
                {
                    qoh.Action = "总经理审批";
                    if ((bool)obj.IsMaterialType && ((obj.ServiceChargeJH == null && obj.ServiceChargeSM == null && obj.TravelExpense == null) || (obj.ServiceChargeJH <= 0 && obj.ServiceChargeSM <= 0 && obj.TravelExpense <= 0)))
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
                            _capBus.Publish("Serve.SellOrder.ERPCreate", obj.Id);
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
                    #region 报价单同步到SAP，ERP3.0 
                    _capBus.Publish("Serve.SellOrder.Create", obj.Id);
                    _capBus.Publish("Serve.SellOrder.ERPCreate", obj.Id);
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
                        obj.QuotationStatus = 10;
                        obj.Status = 2;
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
                    obj.QuotationStatus = decimal.Parse(qoh.ApprovalStage);
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
            qoh.IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(selqoh.CreateTime)).TotalSeconds);
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
            var isExist = (await UnitWork.Find<ReturnNote>(w => w.ServiceOrderId == obj.ServiceOrderId && w.CreateUserId == loginUser.Id).ToListAsync()).Count > 0 ? true : false;
            if (isExist)
            {
                throw new Exception("该服务单已开始退料，不可领料。");
            }
            #endregion

            //判定字段是否同时存在
            if (!(!string.IsNullOrWhiteSpace(obj.TaxRate) && !string.IsNullOrWhiteSpace(obj.InvoiceCategory) && !string.IsNullOrWhiteSpace(obj.InvoiceCompany)) && !(string.IsNullOrWhiteSpace(obj.TaxRate) && string.IsNullOrWhiteSpace(obj.InvoiceCategory) && string.IsNullOrWhiteSpace(obj.InvoiceCompany)))
            {
                throw new Exception("请核对是否存在未填写字段");
            }

            //判定人员是否有销售员code
            var slpcode = (await UnitWork.Find<OSLP>(o => o.SlpName.Equals(loginUser.Name)).FirstOrDefaultAsync())?.SlpCode;
            if (slpcode == null || slpcode == 0)
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
            QuotationObj.IsProtected = true;
            QuotationObj.QuotationProducts.ForEach(q =>
            {
                q.QuotationMaterials.ForEach(m =>
                {
                    if (m.MaterialType != 3 && m.SalesPrice > 0 && Convert.ToDouble(m.DiscountPrices / m.SalesPrice) < 0.4)
                    {
                        throw new Exception("金额有误请重新输入");
                    }
                    m.Discount = m.MaterialType != 3 ? m.Discount : 100;
                    m.SalesPrice = m.MaterialType != 3 ? m.SalesPrice : 0;
                    m.DiscountPrices = m.MaterialType != 3 ? m.DiscountPrices : 0;
                    m.TotalPrice = m.MaterialType != 3 ? Convert.ToDecimal(Convert.ToDecimal(m.DiscountPrices * m.Count).ToString("#0.00")) : 0;
                    QuotationObj.TotalCostPrice += m.MaterialType != 3 ? Convert.ToDecimal(Convert.ToDecimal(m.DiscountPrices * m.Count).ToString("#0.00")) : 0;
                    QuotationObj.TotalMoney += m.MaterialType != 3 ? Convert.ToDecimal(Convert.ToDecimal(m.DiscountPrices * m.Count).ToString("#0.00")) : 0;
                });
            });
            if (QuotationObj.ServiceChargeJH != null && QuotationObj.ServiceChargeJH > 0 && QuotationObj.ServiceChargeManHourJH != null && QuotationObj.ServiceChargeManHourJH > 0)
            {
                QuotationObj.TotalMoney += Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeJH * QuotationObj.ServiceChargeManHourJH).ToString("#0.00"));
            }
            else
            {
                QuotationObj.ServiceChargeJH = null;
                QuotationObj.ServiceChargeManHourJH = null;
            }
            if (QuotationObj.ServiceChargeSM != null && QuotationObj.ServiceChargeSM > 0 && QuotationObj.ServiceChargeManHourSM != null && QuotationObj.ServiceChargeManHourSM > 0)
            {
                QuotationObj.TotalMoney += Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeSM * QuotationObj.ServiceChargeManHourSM).ToString("#0.00"));
            }
            else
            {
                QuotationObj.ServiceChargeSM = null;
                QuotationObj.ServiceChargeManHourSM = null;
            }
            if (QuotationObj.TravelExpense != null && QuotationObj.TravelExpense > 0 && QuotationObj.TravelExpenseManHour != null && QuotationObj.TravelExpenseManHour > 0)
            {
                QuotationObj.TotalMoney += Convert.ToDecimal(Convert.ToDecimal(QuotationObj.TravelExpense * QuotationObj.TravelExpenseManHour).ToString("#0.00"));
            }
            else
            {
                QuotationObj.TravelExpense = null;
                QuotationObj.TravelExpenseManHour = null;
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
        }

        /// <summary>
        /// 同步销售订单
        /// </summary>
        /// <returns></returns>
        public async Task SyncSalesOrderStatus()
        {
            var salesOrderIds = await UnitWork.Find<Quotation>(q => string.IsNullOrWhiteSpace(q.SalesOrderId.ToString()) && q.QuotationStatus != -1M && q.CreateTime>Convert.ToDateTime("2021.05.25")).Select(q => q.SalesOrderId).ToListAsync();
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
        //, FlowInstanceApp flowInstanceApp, ModuleFlowSchemeApp moduleFlowSchemeApp
        public QuotationApp(IUnitWork unitWork, ICapPublisher capBus, ModuleFlowSchemeApp moduleFlowSchemeApp, IAuth auth) : base(unitWork, auth)
        {
            //_flowInstanceApp = flowInstanceApp;
            //_moduleFlowSchemeApp = moduleFlowSchemeApp;
            _capBus = capBus;
        }

    }
}
