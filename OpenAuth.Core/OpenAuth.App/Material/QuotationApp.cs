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

namespace OpenAuth.App.Material
{
    /// <summary>
    /// 报价单操作
    /// </summary>
    public class QuotationApp : OnlyUnitWorkBaeApp
    {
        private readonly FlowInstanceApp _flowInstanceApp;

        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;

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


            #region 页面条件
            switch (request.StartType)
            {
                case 1://保存
                    Quotations = Quotations.Where(q => q.IsDraft == true);
                    break;

                case 2://审批中
                    Quotations = Quotations.Where(q => q.QuotationStatus > 3 && q.QuotationStatus < 6);
                    break;
            }
            #endregion

            if (!loginContext.Roles.Any(r => r.Name.Equals("物料财务")))
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
                CreateTime = q.a.CreateTime.ToString("yyyy-MM-dd"),
                q.a.QuotationStatus,
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

        #region (废弃)
        /// <summary>
        /// 待审批列表
        /// </summary>
        //public async Task<TableData> ApprovalPendingLoad(QueryQuotationListReq request)
        //{
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    var result = new TableData();
        //    var ServiceOrders = await UnitWork.Find<ServiceOrder>(null).WhereIf(!string.IsNullOrWhiteSpace(request.CardCode), q => q.CustomerId.Contains(request.CardCode) || q.CustomerName.Contains(request.CardCode)).Select(s => new { s.Id, s.TerminalCustomer, s.TerminalCustomerId }).ToListAsync();
        //    var ServiceOrderids = ServiceOrders.Select(s => s.Id).ToList();
        //    var Quotations = UnitWork.Find<Quotation>(null).WhereIf(request.QuotationId.ToString() != null, q => q.Id.ToString().Contains(request.QuotationId.ToString()))
        //                        .WhereIf(request.ServiceOrderSapId != null, q => q.ServiceOrderSapId.ToString().Contains(request.ServiceOrderSapId.ToString()))
        //                        .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUser), q => q.CreateUser.Contains(request.CreateUser))
        //                        .WhereIf(request.StartCreateTime != null, q => q.CreateTime > request.StartCreateTime)
        //                        .WhereIf(request.EndCreateTime != null, q => q.CreateTime < request.EndCreateTime)
        //                        .Where(q => ServiceOrderids.Contains(q.ServiceOrderId));



        //    var QuotationDate = await Quotations.Skip((request.page - 1) * request.limit)
        //        .Take(request.limit).ToListAsync();

        //    var query = from a in QuotationDate
        //                join b in ServiceOrders on a.ServiceOrderId equals b.Id
        //                select new { a, b };

        //    result.Data = query.Select(q => new
        //    {
        //        q.a.Id,
        //        q.a.ServiceOrderSapId,
        //        q.a.ServiceOrderId,
        //        q.b.TerminalCustomer,
        //        q.b.TerminalCustomerId,
        //        q.a.TotalMoney,
        //        q.a.CreateUser,
        //        q.a.Reamrk,
        //        CreateTime = q.a.CreateTime.ToString("yyyy-MM-dd"),
        //        q.a.QuotationStatus
        //    }).ToList();
        //    result.Count = await Quotations.CountAsync();
        //    return result;
        //}
        #endregion

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
                                join b in UnitWork.Find<ServiceWorkOrder>(null) on a.Id equals b.ServiceOrderId
                                select new { a, b};
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

            var ServiceWorkOrderList = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId.Equals(request.ServiceOrderId) && s.CurrentUserNsapId.Equals(loginUser.Id) && !s.MaterialCode.Equals("其他设备"))
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

            #region 暂时废弃
            //var ServiceWorkOrderList = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId.Equals(request.ServiceOrderId)).Select(s => new { s.ManufacturerSerialNumber, s.MaterialCode }).ToListAsync();
            //string ManufacturerSerialNumbers = "";
            //ServiceWorkOrderList.ForEach(m => ManufacturerSerialNumbers += "'" + m.ManufacturerSerialNumber + "',");
            //ManufacturerSerialNumbers = ManufacturerSerialNumbers.Substring(0, ManufacturerSerialNumbers.Length - 1);

            //var Datas = DataList.GroupBy(d => d.a.MnfSerial).ToList();
            //List<MaterialCodeListReap> MaterialCodeListReaps = new List<MaterialCodeListReap>();
            //foreach (var item in Datas)
            //{
            //    MaterialCodeListReap mcr = new MaterialCodeListReap();
            //    mcr.MnfSerial = item.Key;
            //    mcr.MaterialDetailList = item.Select(i => new MaterialDetails { ItemCode = i.c.ItemCode, OnHand = i.c.OnHand, WhsCode = i.c.WhsCode, Quantity = i.b.BaseQty }).ToList();
            //    MaterialCodeListReaps.Add(mcr);
            //}
            //var ManufacturerSerialNumberList = Datas.Where(d => !ManufacturerSerialNumbers.Contains(d.Key)).Select(d => d.Key).ToList();
            //if (ManufacturerSerialNumberList != null && ManufacturerSerialNumberList.Count() > 0)
            //{
            //    var MaterialCodes = ServiceWorkOrderList.Where(s => ManufacturerSerialNumberList.Contains(s.ManufacturerSerialNumber)).Select(s => s.MaterialCode).ToList();
            //    var itt1list = await UnitWork.Find<ITT1>(o => MaterialCodes.Contains(o.Father)).Select(o => new { o.Father, o.Code, o.Quantity }).ToListAsync();
            //    var codes = itt1list.Select(i => i.Code).ToList();
            //    oitwlist = await UnitWork.Find<OITW>(o => codes.Contains(o.ItemCode) && o.WhsCode == "37").Select(o => new { o.ItemCode, o.OnHand, o.WhsCode }).ToListAsync();

            //    var OrderList = from a in ServiceWorkOrderList
            //                    join b in itt1list on a.MaterialCode equals b.Father
            //                    join c in oitwlist on b.Code equals c.ItemCode
            //                    select new { a, b, c };
            //    var ListData = OrderList.GroupBy(d => d.a.ManufacturerSerialNumber).ToList();
            //    foreach (var item in ListData)
            //    {
            //        MaterialCodeListReap mcr = new MaterialCodeListReap();
            //        mcr.MnfSerial = item.Key;
            //        mcr.MaterialDetailList = item.Select(i => new MaterialDetails { ItemCode = i.c.ItemCode, WhsCode = i.c.WhsCode, OnHand = i.c.OnHand, Quantity = i.b.Quantity }).ToList();
            //        MaterialCodeListReaps.Add(mcr);
            //    }
            //}
            #endregion

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

            if (Equipments == null && Equipments.Count() <= 0)
            {
                Equipments = await UnitWork.Query<SysEquipmentColumn>(@$"select a.Father as MnfSerial,a.Code as ItemCode,a.U_Desc as ItemName,a.U_DUnit as BuyUnitMsr,b.OnHand,b.WhsCode,a.Quantity,c.lastPurPrc
                        from ITT1 a join OITW b on a.Code=b.ItemCode where a.Father='{request.MaterialCode}' and b.WhsCode=37")
                    .WhereIf(!string.IsNullOrWhiteSpace(request.PartCode), s => s.ItemCode.Contains(request.PartCode))
                    .Select(s => new SysEquipmentColumn { ItemCode = s.ItemCode, MnfSerial = s.MnfSerial, ItemName = s.ItemName, BuyUnitMsr = s.BuyUnitMsr, OnHand = s.OnHand, WhsCode = s.WhsCode, Quantity = s.Quantity, lastPurPrc = s.lastPurPrc }).ToListAsync();
            }

            var ItemCodes = Equipments.Select(e => e.ItemCode).ToList();
            var MaterialPrices = await UnitWork.Find<MaterialPrice>(m => ItemCodes.Contains(m.MaterialCode)).ToListAsync();

            Equipments.ForEach(e =>
            {
                e.MnfSerial = request.ManufacturerSerialNumbers;
                var Prices = MaterialPrices.Where(m => m.MaterialCode.Equals(e.ItemCode)).FirstOrDefault();
                if (Prices != null)
                {
                    e.lastPurPrc = Prices?.SettlementPrice <= 0 ? e.lastPurPrc * Prices?.SettlementPriceModel : Prices?.SettlementPrice;
                }
            });

            result.Data = Equipments.Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToList();

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
                result.Data = new
                {
                    Expressages,
                    Quotations,
                    QuotationMergeMaterials,
                    ServiceOrders,
                    CustomerInformation
                };
            }
            else
            {
                result.Data = new
                {
                    Quotations,
                    QuotationMergeMaterials,
                    ServiceOrders,
                    CustomerInformation
                };
            }


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
            QuotationObj.ErpOrApp = 1;
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppId));
                QuotationObj.ErpOrApp = 2;
            }

            QuotationObj.IsProtected = true;
            QuotationObj.QuotationProducts.ForEach(q =>
            {
                if (!(bool)q.IsProtected)
                {
                    QuotationObj.IsProtected = false;
                }
            });
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
                        //创建物料报价单审批流程
                        var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("物料报价单"));
                        var afir = new AddFlowInstanceReq();
                        afir.SchemeId = mf.FlowSchemeId;
                        afir.FrmType = 2;
                        afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                        afir.CustomName = $"物料报价单" + DateTime.Now;
                        afir.OrgId = "";
                        bool IsProtected = false;
                        obj.QuotationProducts.ForEach(q =>
                        {
                            if (q.IsProtected == true) IsProtected = (bool)q.IsProtected;
                        });
                        QuotationObj.QuotationStatus = 4;
                        afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\"}";

                        #region//保内保外
                        //if (!(bool)QuotationObj.IsProtected)
                        //{
                        //    //保外申请报价单
                        //    QuotationObj.QuotationStatus = 4;
                        //    if (IsProtected)
                        //    {
                        //        afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\",\"IsProtected\":\"0\",\"IsRead\":\"1\"}";
                        //    }
                        //    else
                        //    {
                        //        afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\",\"IsProtected\":\"0\",\"IsRead\":\"0\"}";
                        //    }
                        //}
                        //else
                        //{
                        //    //保内申请报价单

                        //    afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\",\"IsProtected\":\"1\",\"IsRead\":\"0\"}";
                        //}
                        #endregion

                        var FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                        QuotationObj.FlowInstanceId = FlowInstanceId;
                        await UnitWork.UpdateAsync<Quotation>(QuotationObj);
                        await UnitWork.AddAsync<QuotationOperationHistory>(new QuotationOperationHistory
                        {
                            Action = "提交报价单",
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            CreateTime = DateTime.Now,
                            QuotationId = QuotationObj.Id
                        });
                        await UnitWork.SaveAsync();
                    }

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
                                         CostPrice = 0,
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
                    var QuotationMergeMaterialListMap = QuotationMergeMaterialList.MapToList<QuotationMergeMaterial>();
                    await UnitWork.BatchAddAsync<QuotationMergeMaterial>(QuotationMergeMaterialListMap.ToArray());
                    await UnitWork.SaveAsync();
                    #endregion

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
            QuotationObj.ErpOrApp = 1;
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppId));
                QuotationObj.ErpOrApp = 2;
            }
            QuotationObj.IsProtected = true;
            QuotationObj.QuotationProducts.ForEach(q =>
            {
                if (!(bool)q.IsProtected)
                {
                    QuotationObj.IsProtected = false;
                }
            });
            var dbContext = UnitWork.GetDbContext<Quotation>();
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    //if (obj.QuotationIds != null && obj.QuotationIds.Count > 0)
                    //{
                    //    var DelQuotation = await UnitWork.Find<Quotation>(q => obj.QuotationIds.Contains(q.Id)).Include(q => q.QuotationProducts).Include(q => q.QuotationOperationHistorys).ToListAsync();
                    //    List<QuotationProduct> DelQuotationProduct = new List<QuotationProduct>();
                    //    List<QuotationOperationHistory> DelQuotationOperationHistorys = new List<QuotationOperationHistory>();
                    //    foreach (var item in DelQuotation)
                    //    {
                    //        DelQuotationProduct.AddRange(item.QuotationProducts);
                    //        DelQuotationOperationHistorys.AddRange(item.QuotationOperationHistorys);
                    //    }
                    //    var DelQuotationProductIds = DelQuotationProduct.Select(q => q.Id).ToList();

                    //    var DelQuotationMaterial = await UnitWork.Find<QuotationMaterial>(q => DelQuotationProductIds.Contains(q.QuotationProductId)).ToListAsync();

                    //    await UnitWork.BatchDeleteAsync<QuotationMaterial>(DelQuotationMaterial.ToArray());
                    //    await UnitWork.BatchDeleteAsync<Quotation>(DelQuotation.ToArray());
                    //    await UnitWork.BatchDeleteAsync<QuotationProduct>(DelQuotationProduct.ToArray());
                    //    await UnitWork.BatchDeleteAsync<QuotationOperationHistory>(DelQuotationOperationHistorys.ToArray());
                    //}
                    if (obj.IsDraft)
                    {
                        QuotationObj.Status = 1;
                        QuotationObj.QuotationStatus = 3;
                        await UnitWork.UpdateAsync<Quotation>(u => u.Id == QuotationObj.Id, u => new Quotation
                        {
                            QuotationStatus = QuotationObj.QuotationStatus,
                            CollectionAddress = QuotationObj.CollectionAddress,
                            ShippingAddress = QuotationObj.ShippingAddress,
                            DeliveryMethod = QuotationObj.DeliveryMethod,
                            InvoiceCompany = QuotationObj.InvoiceCompany,
                            TotalMoney = QuotationObj.TotalMoney,
                            Remark = QuotationObj.Remark,
                            IsDraft = QuotationObj.IsDraft,
                            IsProtected = QuotationObj.IsProtected,
                            Status = QuotationObj.Status,
                            //todo:要修改的字段赋值
                        });
                        await UnitWork.SaveAsync();
                    }
                    else
                    {
                        QuotationObj.Status = 1;
                        //创建物料报价单审批流程
                        var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("物料报价单"));
                        var afir = new AddFlowInstanceReq();
                        afir.SchemeId = mf.FlowSchemeId;
                        afir.FrmType = 2;
                        afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                        bool IsProtected = false;
                        obj.QuotationProducts.ForEach(q =>
                        {
                            if (q.IsProtected == true) IsProtected = (bool)q.IsProtected;
                        });
                        QuotationObj.QuotationStatus = 4;
                        afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\"}";

                        #region//保内保外
                        //if (!(bool)QuotationObj.IsProtected)
                        //{
                        //    //保外申请报价单
                        //    QuotationObj.QuotationStatus = 4;
                        //    if (IsProtected)
                        //    {
                        //        afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\",\"IsProtected\":\"0\",\"IsRead\":\"1\"}";
                        //    }
                        //    else
                        //    {
                        //        afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\",\"IsProtected\":\"0\",\"IsRead\":\"0\"}";
                        //    }
                        //}
                        //else
                        //{
                        //    //保内申请报价单
                        //    QuotationObj.QuotationStatus = 5;
                        //    afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\",\"IsProtected\":\"1\",\"IsRead\":\"0\"}";
                        //}
                        #endregion

                        afir.CustomName = $"物料报价单" + DateTime.Now;
                        afir.OrgId = "";
                        var FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);

                        await UnitWork.UpdateAsync<Quotation>(u => u.Id == QuotationObj.Id, u => new Quotation
                        {
                            QuotationStatus = QuotationObj.QuotationStatus,
                            CollectionAddress = QuotationObj.CollectionAddress,
                            ShippingAddress = QuotationObj.ShippingAddress,
                            DeliveryMethod = QuotationObj.DeliveryMethod,
                            InvoiceCompany = QuotationObj.InvoiceCompany,
                            TotalMoney = QuotationObj.TotalMoney,
                            Remark = QuotationObj.Remark,
                            IsDraft = QuotationObj.IsDraft,
                            IsProtected = QuotationObj.IsProtected,
                            Status = QuotationObj.Status,
                            FlowInstanceId = FlowInstanceId,
                            SalesOrderId = new Random().Next(1, 9999)
                            //todo:要修改的字段赋值
                        });
                        await UnitWork.AddAsync<QuotationOperationHistory>(new QuotationOperationHistory
                        {
                            Action = "提交报价单",
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            CreateTime = DateTime.Now,
                            QuotationId = QuotationObj.Id
                        });
                        await UnitWork.SaveAsync();

                    }



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
                                         CostPrice = 0,
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
                    var QuotationMergeMaterialListMap = QuotationMergeMaterialList.MapToList<QuotationMergeMaterial>();
                    await UnitWork.BatchAddAsync<QuotationMergeMaterial>(QuotationMergeMaterialListMap.ToArray());
                    await UnitWork.SaveAsync();
                    #endregion

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("添加报销单失败,请重试。" + ex.Message);
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
            var Expressageobj = new Expressage();
            var dbContext = UnitWork.GetDbContext<Quotation>();
            List<LogisticsRecord> LogisticsRecords = new List<LogisticsRecord>();
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var ExpressageMap = obj.ExpressageReqs.MapTo<Expressage>();
                    ExpressageMap.CreateTime = DateTime.Now;
                    Expressageobj = await UnitWork.AddAsync<Expressage>(ExpressageMap);
                    var ExpressagePictures = new List<ExpressagePicture>();
                    obj.ExpressageReqs.ExpressagePictures.ForEach(p => ExpressagePictures.Add(new ExpressagePicture { ExpressageId = Expressageobj.Id, PictureId = p, Id = Guid.NewGuid().ToString() }));
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
                            ExpressageId = Expressageobj.Id
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
            var Expressages = await UnitWork.Find<Expressage>(e => e.QuotationId.Equals(obj.ExpressageReqs.QuotationId)).Include(e => e.ExpressagePicture).Include(e => e.LogisticsRecords).OrderByDescending(e=>e.CreateTime).ToListAsync();
            LogisticsRecords = new List<LogisticsRecord>();
            Expressages.ForEach(e => LogisticsRecords.AddRange(e.LogisticsRecords));
            var QuotationMergeMaterialLists = await UnitWork.Find<QuotationMergeMaterial>(q =>q.QuotationId.Equals(obj.ExpressageReqs.QuotationId)).ToListAsync();

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
                await UnitWork.UpdateAsync<Quotation>(q => q.Id.Equals(obj.ExpressageReqs.QuotationId), q => new Quotation { QuotationStatus = 7 });
                await UnitWork.SaveAsync();
            }
            var result = new TableData();
            var MergeMaterials = from a in QuotationMergeMaterialLists
                                 join b in LogisticsRecords on a.Id equals b.QuotationMaterialId
                                 select new { a, b };
            result.Data = new
            {
                start = isEXwarehouse == 0 ? 7 : 0,
                Expressages= Expressages.Select(e => new
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
            if (loginUser.Id.Equals(obj.CreateUserId) && obj.QuotationStatus == 4)
            {
                qoh.Action = "销售订单成立";
            }
            else if (loginContext.Roles.Any(r => r.Name.Equals("物料财务")) && obj.QuotationStatus == 5)
            {
                qoh.Action = "报价单财务审批";
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
                //obj.QuotationStatus = 1;
                //qoh.ApprovalResult = "驳回";
                //var delQuotationMergeMaterial = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(obj.Id)).ToListAsync();
                //await UnitWork.BatchDeleteAsync<QuotationMergeMaterial>(delQuotationMergeMaterial.ToArray());
            }
            else
            {
                VerificationReqModle = new VerificationReq
                {
                    NodeRejectStep = "",
                    NodeRejectType = "0",
                    FlowInstanceId = obj.FlowInstanceId,
                    VerificationFinally = "1",
                    VerificationOpinion = "同意",
                };
                qoh.ApprovalResult = "同意";
                if (loginUser.Id.Equals(obj.CreateUserId) && obj.QuotationStatus == 4)
                {
                    obj.QuotationStatus = 5;
                    _flowInstanceApp.Verification(VerificationReqModle);
                    if ((bool)obj.IsProtected)
                    {
                        obj.QuotationStatus = 6;
                        obj.Status = 2;
                        //_flowInstanceApp.Verification(VerificationReqModle);
                    }
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("物料财务")) && obj.QuotationStatus == 5)
                {
                    obj.QuotationStatus = 6;
                    obj.Status = 2;
                    _flowInstanceApp.Verification(VerificationReqModle);
                }
                else
                {
                    throw new Exception("审批失败，暂无权限审批当前流程。");
                }
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

            await UnitWork.BatchAddAsync(MaterialPriceList.ToArray());
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
            var Money = await UnitWork.Find<MaterialsSettlement>(m => m.ProposerId.Equals(loginUser.Id)).ToListAsync();
            if (Money != null && Money.Count > 0)
            {
                var TotalMoney = Money.Sum(m => m.TotalMoney);
                var Totalpayamount = Money.Sum(m => m.Totalpayamount);
                if ((TotalMoney - Totalpayamount) > 4000)
                {
                    throw new Exception("欠款已超出额度，不可领料。");
                }
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
        /// 生成销售订单
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task<string> CreateSalesOrder(AddOrUpdateQuotationReq obj)
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

            return "1111";
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
        /// <param name="QuotationId"></param>
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

        #region 判断是否是保内
        /// <summary>
        /// 判断是否是保内(暂弃)
        /// </summary>
        /// <param name="QuotationProducts"></param>
        /// <returns></returns>
        //private async Task<dynamic> JudgeIsProtected(List<string> MnfSerials)
        //{
        //    //      StringBuilder MnfSerial = new StringBuilder();
        //    //      MnfSerials.ForEach(item =>
        //    //      {
        //    //          MnfSerial.Append("'" + item + "',");
        //    //      });
        //    //      var Equipments = await UnitWork.Query<SysEquipmentColumn>(@$"SELECT d.DocEntry,c.MnfSerial
        //    //      FROM oitl a left join itl1 b
        //    //      on a.LogEntry = b.LogEntry and a.ItemCode = b.ItemCode 
        //    //      left join osrn c on b.ItemCode = c.ItemCode and b.SysNumber = c.SysNumber
        //    //left join odln d on a.DocEntry=d.DocEntry
        //    //      where a.DocType =15 and c.MnfSerial in ({MnfSerial.ToString().Substring(0, MnfSerial.Length - 1)})").Select(s => new SysEquipmentColumn { MnfSerial = s.MnfSerial, DocEntry = s.DocEntry }).ToListAsync();

        //    //      var DocEntryIds = Equipments.Select(e => e.DocEntry).ToList();

        //    //      var buyopors = from a in UnitWork.Find<buy_opor>(null)
        //    //                     join b in UnitWork.Find<sale_transport>(null) on a.DocEntry equals b.Buy_DocEntry
        //    //                     where DocEntryIds.Contains(b.Base_DocEntry) && b.Base_DocType == 24
        //    //                     select new { a, b };
        //    //      var docdate = await buyopors.ToListAsync();
        //    //      var IsProtecteds = Equipments.Select(e => new
        //    //      {
        //    //          MnfSerial = e.MnfSerial,
        //    //          DocDate = Convert.ToDateTime(docdate.Where(d => d.b.Base_DocEntry.Equals(e.DocEntry)).FirstOrDefault()?.a.DocDate).AddYears(1)
        //    //      }).ToList();

        //    return null;
        //}
        #endregion

        public QuotationApp(IUnitWork unitWork, FlowInstanceApp flowInstanceApp, ModuleFlowSchemeApp moduleFlowSchemeApp, IAuth auth) : base(unitWork, auth)
        {
            _flowInstanceApp = flowInstanceApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
        }

    }
}
