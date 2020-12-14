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
            var result = new TableData();
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(null).WhereIf(!string.IsNullOrWhiteSpace(request.CardCode), q => q.CustomerId.Contains(request.CardCode) || q.CustomerName.Contains(request.CardCode)).Select(s => new { s.Id, s.TerminalCustomer, s.TerminalCustomerId }).ToListAsync();
            var ServiceOrderids = ServiceOrders.Select(s => s.Id).ToList();
            var Quotations = UnitWork.Find<Quotation>(null).WhereIf(request.QuotationId.ToString() != null, q => q.Id.ToString().Contains(request.QuotationId.ToString()))
                                .WhereIf(request.ServiceOrderSapId != null, q => q.ServiceOrderSapId.ToString().Contains(request.ServiceOrderSapId.ToString()))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUser), q => q.CreateUser.Contains(request.CreateUser))
                                .WhereIf(request.StartCreateTime != null, q => q.CreateTime > request.StartCreateTime)
                                .WhereIf(request.EndCreateTime != null, q => q.CreateTime < request.EndCreateTime)
                                .WhereIf(request.Status != null, q => q.Status == request.Status)
                                .Where(q => ServiceOrderids.Contains(q.ServiceOrderId));


            #region 分页条件
            switch (request.StartType)
            {
                case 1://草稿箱
                    Quotations = Quotations.Where(q => q.IsDraft == true);
                    break;

                case 2://审批中
                    Quotations = Quotations.Where(q => q.QuotationStatus > 3 && q.QuotationStatus < 11);
                    break;

                case 3://已领料
                    Quotations = Quotations.Where(q => q.QuotationStatus == 12);
                    break;

                case 4://已驳回
                    Quotations = Quotations.Where(q => q.QuotationStatus == 1);
                    break;
                default:
                    break;
            }
            #endregion

            if (request.IsSalesOrderList != null && (bool)request.IsSalesOrderList)
            {
                Quotations = Quotations.Where(q => q.SalesOrderId != null && q.QuotationStatus > 8 && q.QuotationStatus < 11);
            }

            var QuotationDate = await Quotations.Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();

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
                q.a.Reamrk,
                q.a.SalesOrderId,
                CreateTime = q.a.CreateTime.ToString("yyyy-MM-dd"),
                q.a.QuotationStatus
            }).ToList();
            result.Count = await Quotations.CountAsync();

            return result;
        }

        /// <summary>
        /// 待审批列表
        /// </summary>
        public async Task<TableData> ApprovalPendingLoad(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(null).WhereIf(!string.IsNullOrWhiteSpace(request.CardCode), q => q.CustomerId.Contains(request.CardCode) || q.CustomerName.Contains(request.CardCode)).Select(s => new { s.Id, s.TerminalCustomer, s.TerminalCustomerId }).ToListAsync();
            var ServiceOrderids = ServiceOrders.Select(s => s.Id).ToList();
            var Quotations = UnitWork.Find<Quotation>(null).WhereIf(request.QuotationId.ToString() != null, q => q.Id.ToString().Contains(request.QuotationId.ToString()))
                                .WhereIf(request.ServiceOrderSapId != null, q => q.ServiceOrderSapId.ToString().Contains(request.ServiceOrderSapId.ToString()))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUser), q => q.CreateUser.Contains(request.CreateUser))
                                .WhereIf(request.StartCreateTime != null, q => q.CreateTime > request.StartCreateTime)
                                .WhereIf(request.EndCreateTime != null, q => q.CreateTime < request.EndCreateTime)
                                .Where(q => ServiceOrderids.Contains(q.ServiceOrderId));


            #region 条件
            switch (request.StartType)
            {
                case 1://待审批
                    List<int> Condition = new List<int>();
                    if (loginContext.Roles.Any(r => r.Name.Equals("售后主管")))
                    {
                        Condition.Add(4);
                        //ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 4);
                    }
                    if (loginContext.Roles.Any(r => r.Name.Equals("仓库主管")))
                    {
                        Condition.Add(5);
                        //ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 5);
                    }
                    if (loginContext.Roles.Any(r => r.Name.Equals("总经理")))
                    {
                        Condition.Add(7);
                        //ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 6);
                    }
                    if (loginContext.Roles.Any(r => r.Name.Equals("物料财务")))
                    {
                        Condition.Add(6);
                        //ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 6);
                    }
                    Quotations = Quotations.Where(r => Condition.Contains((int)r.QuotationStatus) || (r.CreateUserId.Contains(loginContext.User.Id) && r.QuotationStatus == 8));
                    break;
                case 2://已驳回
                    if (loginContext.Roles.Any(r => r.Name.Equals("售后主管")))
                    {
                        var eohids = await UnitWork.Find<QuotationOperationHistory>(r => r.ApprovalStage >= 4 && r.ApprovalResult == "驳回").Select(r => r.QuotationId).Distinct().ToListAsync();
                        Quotations = Quotations.Where(r => eohids.Contains(r.Id) && r.QuotationStatus == 2);
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("仓库主管")))
                    {
                        var eohids = await UnitWork.Find<QuotationOperationHistory>(r => r.ApprovalStage >= 5 && r.ApprovalResult == "驳回").Select(r => r.QuotationId).Distinct().ToListAsync();
                        Quotations = Quotations.Where(r => eohids.Contains(r.Id) && r.QuotationStatus == 2);
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("物料财务")))
                    {
                        var eohids = await UnitWork.Find<QuotationOperationHistory>(r => r.ApprovalStage >= 5 && r.ApprovalResult == "驳回").Select(r => r.QuotationId).Distinct().ToListAsync();
                        Quotations = Quotations.Where(r => eohids.Contains(r.Id) && r.QuotationStatus == 2);
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")))
                    {
                        var eohids = await UnitWork.Find<QuotationOperationHistory>(r => r.ApprovalStage >= 6 && r.ApprovalResult == "驳回").Select(r => r.QuotationId).Distinct().ToListAsync();
                        Quotations = Quotations.Where(r => eohids.Contains(r.Id) && r.QuotationStatus == 2);
                    }
                    else
                    {
                        Quotations = Quotations.Where(r => r.QuotationStatus == -1);
                    }
                    break;
                case 3://已通过
                    if (loginContext.Roles.Any(r => r.Name.Equals("售后主管")))
                    {
                        Quotations = Quotations.Where(r => r.QuotationStatus > 4 && r.QuotationStatus <= 8);
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("仓库主管")))
                    {
                        Quotations = Quotations.Where(r => r.QuotationStatus > 5 && r.QuotationStatus <= 8);
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("物料财务")))
                    {
                        Quotations = Quotations.Where(r => r.QuotationStatus > 6 && r.QuotationStatus <= 8);
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")))
                    {
                        Quotations = Quotations.Where(r => r.QuotationStatus > 7 && r.QuotationStatus <= 8);
                    }
                    else
                    {
                        Quotations = Quotations.Where(r => r.CreateUserId.Contains(loginContext.User.Id) && r.QuotationStatus == 8);
                    }
                    break;
                default:
                    break;
            }
            #endregion

            var QuotationDate = await Quotations.Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();

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
                q.a.Reamrk,
                CreateTime = q.a.CreateTime.ToString("yyyy-MM-dd"),
                q.a.QuotationStatus
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
                                join b in UnitWork.Find<ServiceWorkOrder>(null) on a.Id equals b.ServiceOrderId
                                select new { a, b };

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
                    q.b.FromTheme,
                    q.a.SalesMan,
                    BillingAddress = Address.Where(a => a.CardCode.Equals(q.a.CustomerId)).FirstOrDefault()?.BillingAddress,//开票地址
                    DeliveryAddress = Address.Where(a => a.CardCode.Equals(q.a.CustomerId)).FirstOrDefault()?.DeliveryAddress //收货地址
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
                #region 判断保内保外
                var MnfSerials = ServiceWorkOrderList.Select(s => s.ManufacturerSerialNumber).ToList();

                StringBuilder MnfSerialStr = new StringBuilder();
                MnfSerials.ForEach(item =>
                {
                    MnfSerialStr.Append("'" + item + "',");
                });
                var manufacturerSerialNumber = from a in UnitWork.Find<OITL>(null)
                                    join b in UnitWork.Find<ITL1>(null) on new { a.LogEntry, a.ItemCode } equals new { b.LogEntry, b.ItemCode } into ab
                                    from b in ab.DefaultIfEmpty()
                                    join c in UnitWork.Find<OSRN>(null) on new { b.ItemCode, SysNumber = b.SysNumber.Value } equals new { c.ItemCode, c.SysNumber } into bc
                                    from c in bc.DefaultIfEmpty()
                                    where (a.DocType == 15 || a.DocType == 59) && c.MnfSerial.Contains(MnfSerialStr.ToString().Substring(0, MnfSerialStr.Length - 1))
                                    select new { c.MnfSerial, a.DocEntry,a.BaseEntry,a.DocType };

                var Equipments = from a in manufacturerSerialNumber
                                 join b in UnitWork.Find<ODLN>(null) on a.DocEntry equals b.DocEntry into ab
                                 from b in ab.DefaultIfEmpty()
                                 select new { a.DocEntry,a.MnfSerial};
                var MnfSerialList = await manufacturerSerialNumber.ToListAsync();
                var EquipmentList = await Equipments.ToListAsync();
                //  var Equipments = await UnitWork.Query<SysEquipmentColumn>(@$"SELECT d.DocEntry,c.MnfSerial
                //      FROM oitl a left join itl1 b
                //      on a.LogEntry = b.LogEntry and a.ItemCode = b.ItemCode 
                //      left join osrn c on b.ItemCode = c.ItemCode and b.SysNumber = c.SysNumber
                //left join odln d on a.DocEntry=d.DocEntry
                //      where a.DocType =15 and c.MnfSerial in ({MnfSerialStr.ToString().Substring(0, MnfSerialStr.Length - 1)})").Select(s => new SysEquipmentColumn { MnfSerial = s.MnfSerial, DocEntry = s.DocEntry }).ToListAsync();

                var DocEntryIds = Equipments.Select(e => e.DocEntry).ToList();

                var buyopors = from a in UnitWork.Find<buy_opor>(null)
                               join b in UnitWork.Find<sale_transport>(null) on a.DocEntry equals b.Buy_DocEntry
                               where DocEntryIds.Contains(b.Base_DocEntry) && b.Base_DocType == 24
                               select new { a, b };
                var docdate = await buyopors.ToListAsync();
                var IsProtecteds = EquipmentList.Select(e => new
                {
                    MnfSerial = e.MnfSerial,
                    DocDate = Convert.ToDateTime(docdate.Where(d => d.b.Base_DocEntry.Equals(e.DocEntry)).FirstOrDefault()?.a.DocDate).AddYears(1)
                }).ToList();
                #endregion

                result.Data = ServiceWorkOrderList.Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select(s => new
                {
                    SalesOrder= MnfSerialList.Where(m=>m.MnfSerial.Equals(s.ManufacturerSerialNumber) && m.DocType==17)?.Max(m=>m.BaseEntry),
                    ProductionOrder = MnfSerialList.Where(m => m.MnfSerial.Equals(s.ManufacturerSerialNumber) && m.DocType == 202)?.Max(m => m.BaseEntry),
                    ManufacturerSerialNumber = s.ManufacturerSerialNumber,
                    MaterialCode = s.MaterialCode,
                    MaterialDescription = s.MaterialDescription,
                    IsProtected = IsProtecteds.Where(i => i.MnfSerial.Equals(s.ManufacturerSerialNumber)).FirstOrDefault()?.DocDate > DateTime.Now ? true : false,
                    DocDate=IsProtecteds.Where(i => i.MnfSerial.Equals(s.ManufacturerSerialNumber)).FirstOrDefault()?.DocDate
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
            where a.DocType in (15, 59) and c.MnfSerial ='{request.ManufacturerSerialNumbers}' and BaseType=202) b on a.docentry = b.BaseEntry	
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
            var Quotations = await UnitWork.Find<Quotation>(q => q.Id.Equals(QuotationId)).Include(q => q.QuotationProducts).ThenInclude(p => p.QuotationMaterials).Include(q => q.QuotationOperationHistorys).FirstOrDefaultAsync();
            if (loginContext.Roles.Any(r => r.Name.Equals("售后主管")))
            {
                Quotations.IsRead = 1;
                await UnitWork.UpdateAsync<Quotation>(Quotations);
            }

            var result = new TableData();
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(Quotations.ServiceOrderId)).Select(s => new { s.Id, s.U_SAP_ID, s.TerminalCustomer, s.TerminalCustomerId, s.SalesMan }).FirstOrDefaultAsync();
            var QuotationMergeMaterials = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(QuotationId)).ToListAsync();
            if (loginContext.Roles.Any(r => r.Name.Equals("总经理")))
            {
                QuotationMergeMaterials = QuotationMergeMaterials.Where(q => q.IsProtected == true).ToList();
            }
            if (Quotations.Status == 2)
            {
                var ExpressageList = await UnitWork.Find<Expressage>(e => e.QuotationId.Equals(Quotations.Id)).Include(e => e.ExpressagePicture).ToListAsync();
                var fileids = new List<string>();
                foreach (var item in ExpressageList)
                {
                    fileids.AddRange(item.ExpressagePicture.Select(p => p.PictureId).ToList());
                }
                var file = await UnitWork.Find<UploadFile>(f => fileids.Contains(f.Id)).ToListAsync();

                var Expressages = ExpressageList.Select(e => new
                {
                    ExpressagePicture = e.ExpressagePicture.Select(p => new
                    {
                        p.PictureId,
                        p.Id,
                        p.ExpressageId,
                        FileName = file.FirstOrDefault(f => f.Id.Equals(p.PictureId))?.FileName,
                        FileType = file.FirstOrDefault(f => f.Id.Equals(p.PictureId))?.FileType,
                    }),
                    e.ExpressInformation,
                    e.ExpressNumber,
                    e.Id,
                    e.QuotationId,
                    e.Remark,
                    e.ReturnNoteId
                }).ToList();
                result.Data = new
                {
                    Expressages,
                    Quotations,
                    QuotationMergeMaterials,
                    ServiceOrders
                };
            }
            else
            {
                result.Data = new
                {
                    Quotations,
                    QuotationMergeMaterials,
                    ServiceOrders
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
            var Quotations = await UnitWork.Find<Quotation>(q => q.ServiceOrderId.Equals(ServiceOrderId) && q.IsRead == 0 && q.ErpOrApp == 2).ToListAsync();
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
        public async Task Add(AddOrUpdateQuotationReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
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
                        //保内保外
                        if (!(bool)QuotationObj.IsProtected)
                        {
                            //保外申请报价单
                            QuotationObj.QuotationStatus = 4;
                            if (IsProtected)
                            {
                                afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\",\"IsProtected\":\"0\",\"IsRead\":\"1\"}";
                            }
                            else
                            {
                                afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\",\"IsProtected\":\"0\",\"IsRead\":\"0\"}";
                            }
                        }
                        else
                        {
                            //保内申请报价单
                            QuotationObj.QuotationStatus = 5;
                            afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\",\"IsProtected\":\"1\",\"IsRead\":\"0\"}";
                        }
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
                        #region 合并零件表
                        List<QuotationMaterial> QuotationMaterialsT = new List<QuotationMaterial>();
                        QuotationObj.QuotationProducts.Where(q => q.IsProtected == true).ToList().ForEach(q => QuotationMaterialsT.AddRange(q.QuotationMaterials));
                        List<QuotationMaterial> QuotationMaterialsF = new List<QuotationMaterial>();
                        QuotationObj.QuotationProducts.Where(q => q.IsProtected == false).ToList().ForEach(q => QuotationMaterialsF.AddRange(q.QuotationMaterials));

                        var MaterialsT = from a in QuotationMaterialsT
                                         group a by new { a.MaterialCode, a.MaterialDescription, a.Unit, a.UnitPrice } into g
                                         select new QueryQuotationMergeMaterialListReq
                                         {
                                             MaterialCode = g.Key.MaterialCode,
                                             MaterialDescription = g.Key.MaterialDescription,
                                             Unit = g.Key.Unit,
                                             SalesPrice = g.Key.UnitPrice,
                                             Count = g.Sum(a => a.Count),
                                             TotalPrice = g.Sum(a => a.TotalPrice),
                                             IsProtected = true,
                                             QuotationId = QuotationObj.Id
                                         };

                        var MaterialsF = from a in QuotationMaterialsF
                                         group a by new { a.MaterialCode, a.MaterialDescription, a.Unit, a.UnitPrice } into g
                                         select new QueryQuotationMergeMaterialListReq
                                         {
                                             MaterialCode = g.Key.MaterialCode,
                                             MaterialDescription = g.Key.MaterialDescription,
                                             Unit = g.Key.Unit,
                                             SalesPrice = g.Key.UnitPrice,
                                             Count = g.Sum(a => a.Count),
                                             TotalPrice = g.Sum(a => a.TotalPrice),
                                             IsProtected = false,
                                             QuotationId = QuotationObj.Id
                                         };

                        var QuotationMergeMaterialList = MaterialsT.ToList();
                        QuotationMergeMaterialList.AddRange(MaterialsF.ToList());
                        var QuotationMergeMaterialListMap = QuotationMergeMaterialList.MapToList<QuotationMergeMaterial>();
                        await UnitWork.BatchAddAsync<QuotationMergeMaterial>(QuotationMergeMaterialListMap.ToArray());
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
        }

        /// <summary>
        /// 修改报价单
        /// </summary>
        /// <param name="obj"></param>
        public async Task Update(AddOrUpdateQuotationReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
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
                    if (obj.QuotationIds != null && obj.QuotationIds.Count > 0)
                    {
                        var DelQuotation = await UnitWork.Find<Quotation>(q => obj.QuotationIds.Contains(q.Id)).Include(q => q.QuotationProducts).Include(q => q.QuotationOperationHistorys).ToListAsync();
                        List<QuotationProduct> DelQuotationProduct = new List<QuotationProduct>();
                        List<QuotationOperationHistory> DelQuotationOperationHistorys = new List<QuotationOperationHistory>();
                        foreach (var item in DelQuotation)
                        {
                            DelQuotationProduct.AddRange(item.QuotationProducts);
                            DelQuotationOperationHistorys.AddRange(item.QuotationOperationHistorys);
                        }
                        var DelQuotationProductIds = DelQuotationProduct.Select(q => q.Id).ToList();

                        var DelQuotationMaterial = await UnitWork.Find<QuotationMaterial>(q => DelQuotationProductIds.Contains(q.QuotationProductId)).ToListAsync();

                        await UnitWork.BatchDeleteAsync<QuotationMaterial>(DelQuotationMaterial.ToArray());
                        await UnitWork.BatchDeleteAsync<Quotation>(DelQuotation.ToArray());
                        await UnitWork.BatchDeleteAsync<QuotationProduct>(DelQuotationProduct.ToArray());
                        await UnitWork.BatchDeleteAsync<QuotationOperationHistory>(DelQuotationOperationHistorys.ToArray());
                    }
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
                            Reamrk = QuotationObj.Reamrk,
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
                        //保内保外
                        if (!(bool)QuotationObj.IsProtected)
                        {
                            //保外申请报价单
                            QuotationObj.QuotationStatus = 4;
                            if (IsProtected)
                            {
                                afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\",\"IsProtected\":\"0\",\"IsRead\":\"1\"}";
                            }
                            else
                            {
                                afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\",\"IsProtected\":\"0\",\"IsRead\":\"0\"}";
                            }
                        }
                        else
                        {
                            //保内申请报价单
                            QuotationObj.QuotationStatus = 5;
                            afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\",\"IsProtected\":\"1\",\"IsRead\":\"0\"}";
                        }
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
                            Reamrk = QuotationObj.Reamrk,
                            IsDraft = QuotationObj.IsDraft,
                            IsProtected = QuotationObj.IsProtected,
                            Status = QuotationObj.Status,
                            FlowInstanceId = FlowInstanceId
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
                        #region 合并零件表
                        List<QuotationMaterial> QuotationMaterialsT = new List<QuotationMaterial>();
                        QuotationObj.QuotationProducts.Where(q => q.IsProtected == true).ToList().ForEach(q => QuotationMaterialsT.AddRange(q.QuotationMaterials));
                        List<QuotationMaterial> QuotationMaterialsF = new List<QuotationMaterial>();
                        QuotationObj.QuotationProducts.Where(q => q.IsProtected == false).ToList().ForEach(q => QuotationMaterialsF.AddRange(q.QuotationMaterials));

                        var MaterialsT = from a in QuotationMaterialsT
                                         group a by new { a.MaterialCode, a.MaterialDescription, a.Unit, a.UnitPrice } into g
                                         select new QueryQuotationMergeMaterialListReq
                                         {
                                             MaterialCode = g.Key.MaterialCode,
                                             MaterialDescription = g.Key.MaterialDescription,
                                             Unit = g.Key.Unit,
                                             SalesPrice = g.Key.UnitPrice,
                                             Count = g.Sum(a => a.Count),
                                             TotalPrice = g.Sum(a => a.TotalPrice),
                                             IsProtected = true,
                                             QuotationId = QuotationObj.Id
                                         };

                        var MaterialsF = from a in QuotationMaterialsF
                                         group a by new { a.MaterialCode, a.MaterialDescription, a.Unit, a.UnitPrice } into g
                                         select new QueryQuotationMergeMaterialListReq
                                         {
                                             MaterialCode = g.Key.MaterialCode,
                                             MaterialDescription = g.Key.MaterialDescription,
                                             Unit = g.Key.Unit,
                                             SalesPrice = g.Key.UnitPrice,
                                             Count = g.Sum(a => a.Count),
                                             TotalPrice = g.Sum(a => a.TotalPrice),
                                             IsProtected = false,
                                             QuotationId = QuotationObj.Id
                                         };

                        var QuotationMergeMaterialList = MaterialsT.ToList();
                        QuotationMergeMaterialList.AddRange(MaterialsF.ToList());
                        var QuotationMergeMaterialListMap = QuotationMergeMaterialList.MapToList<QuotationMergeMaterial>();
                        await UnitWork.BatchAddAsync<QuotationMergeMaterial>(QuotationMergeMaterialListMap.ToArray());
                        await UnitWork.SaveAsync();
                        #endregion
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

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("添加报销单失败,请重试。" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 修改报价单物料，物流
        /// </summary>
        /// <param name="obj"></param>
        public async Task UpdateMaterial(AddOrUpdateQuotationReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var dbContext = UnitWork.GetDbContext<Quotation>();
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    obj.Expressages = obj.Expressages.Where(e => string.IsNullOrWhiteSpace(e.Id)).ToList();
                    obj.Expressages.ForEach(e => { e.ReturnNoteId = null; e.QuotationId = obj.Id; e.Id = Guid.NewGuid().ToString(); });
                    var ExpressageMap = obj.Expressages.MapToList<Expressage>();
                    await UnitWork.BatchAddAsync<Expressage>(ExpressageMap.ToArray());
                    var QuotationMergeMaterialMap = obj.QuotationMergeMaterials.MapToList<QuotationMergeMaterial>();
                    await UnitWork.BatchUpdateAsync<QuotationMergeMaterial>(QuotationMergeMaterialMap.ToArray());
                    await UnitWork.SaveAsync();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("添加报销单失败,请重试。" + ex.Message);
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

            var obj = await UnitWork.Find<Quotation>(q => q.Id == req.Id).Include(q => q.QuotationProducts).FirstOrDefaultAsync();

            qoh.ApprovalStage = obj.QuotationStatus;
            obj.InvoiceCompany = req.InvoiceCompany;

            if (loginContext.Roles.Any(r => r.Name.Equals("售后主管")) && obj.QuotationStatus == 4)
            {
                qoh.Action = "售后主管审批";
            }
            else if (loginContext.Roles.Any(r => r.Name.Equals("仓库主管")) && obj.QuotationStatus == 5)
            {
                qoh.Action = "仓库主管审批";
            }
            else if (loginContext.Roles.Any(r => r.Name.Equals("物料财务")) && obj.QuotationStatus == 6)
            {
                qoh.Action = "报价单财务审批";
            }
            else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")) && obj.QuotationStatus == 7)
            {
                qoh.Action = "总经理审批";
            }
            else if (loginUser.Id.Equals(obj.CreateUserId) && obj.QuotationStatus == 8)
            {
                qoh.Action = "生成销售订单";
            }
            else if (loginUser.Id.Equals(obj.CreateUserId) && obj.QuotationStatus == 9)
            {
                qoh.Action = "销售订单成立";
            }
            else if (loginContext.Roles.Any(r => r.Name.Equals("物料财务")) && obj.QuotationStatus == 10)
            {
                qoh.Action = "报价单财务审批";
            }

            if (req.IsReject)
            {
                List<string> ids = new List<string>();
                ids.Add(obj.FlowInstanceId);
                await _flowInstanceApp.DeleteAsync(ids.ToArray());
                obj.QuotationStatus = 1;
                obj.FlowInstanceId = "";
                qoh.ApprovalResult = "驳回";
                var delQuotationMergeMaterial = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(obj.Id)).ToListAsync();
                await UnitWork.BatchDeleteAsync<QuotationMergeMaterial>(delQuotationMergeMaterial.ToArray());
            }
            else
            {
                VerificationReq VerificationReqModle = new VerificationReq
                {
                    NodeRejectStep = "",
                    NodeRejectType = "0",
                    FlowInstanceId = obj.FlowInstanceId,
                    VerificationFinally = "1",
                    VerificationOpinion = "同意",
                };
                qoh.ApprovalResult = "同意";
                if (loginContext.Roles.Any(r => r.Name.Equals("售后主管")) && obj.QuotationStatus == 4)
                {
                    obj.QuotationStatus = 5;
                    _flowInstanceApp.Verification(VerificationReqModle);
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("仓库主管")) && obj.QuotationStatus == 5)
                {
                    if (!(bool)obj.IsProtected)
                    {
                        obj.QuotationStatus = 6;
                    }
                    else
                    {
                        obj.QuotationStatus = 7;
                    }
                    _flowInstanceApp.Verification(VerificationReqModle);
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("物料财务")) && obj.QuotationStatus == 6)
                {
                    obj.QuotationStatus = 7;
                    obj.SalesOrderId = 111;
                    _flowInstanceApp.Verification(VerificationReqModle);
                    bool IsProtected = false;
                    obj.QuotationProducts.ForEach(q =>
                    {
                        if (q.IsProtected == true) IsProtected = (bool)q.IsProtected;
                    });
                    if (!IsProtected)
                    {
                        obj.QuotationStatus = 8;
                    };
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")) && obj.QuotationStatus == 7)
                {
                    if (!(bool)obj.IsProtected)
                    {
                        obj.QuotationStatus = 8;
                    }
                    else
                    {
                        obj.Status = 2;
                        obj.QuotationStatus = 10;
                    }
                    _flowInstanceApp.Verification(VerificationReqModle);
                }
                else if (loginUser.Id.Equals(obj.CreateUserId) && obj.QuotationStatus == 8)
                {
                    obj.QuotationStatus = 9;
                    _flowInstanceApp.Verification(new VerificationReq
                    {
                        NodeRejectStep = "",
                        NodeRejectType = "0",
                        FlowInstanceId = obj.FlowInstanceId,
                        VerificationFinally = "1",
                        VerificationOpinion = "同意",
                    });
                }
                else if (loginUser.Id.Equals(obj.CreateUserId) && obj.QuotationStatus == 9)
                {
                    obj.QuotationStatus = 10;
                    _flowInstanceApp.Verification(VerificationReqModle);
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("物料财务")) && obj.QuotationStatus == 10)
                {
                    obj.QuotationStatus = 11;
                    obj.Status = 2;
                    _flowInstanceApp.Verification(VerificationReqModle);
                }
                else
                {
                    throw new Exception("审批失败，暂无权限审批当前流程。");
                }
            }
            await UnitWork.UpdateAsync<Quotation>(obj);
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
        /// 判断是否是保内(暂弃)
        /// </summary>
        /// <param name="QuotationProducts"></param>
        /// <returns></returns>
        private async Task<dynamic> JudgeIsProtected(List<string> MnfSerials)
        {
            //      StringBuilder MnfSerial = new StringBuilder();
            //      MnfSerials.ForEach(item =>
            //      {
            //          MnfSerial.Append("'" + item + "',");
            //      });
            //      var Equipments = await UnitWork.Query<SysEquipmentColumn>(@$"SELECT d.DocEntry,c.MnfSerial
            //      FROM oitl a left join itl1 b
            //      on a.LogEntry = b.LogEntry and a.ItemCode = b.ItemCode 
            //      left join osrn c on b.ItemCode = c.ItemCode and b.SysNumber = c.SysNumber
            //left join odln d on a.DocEntry=d.DocEntry
            //      where a.DocType =15 and c.MnfSerial in ({MnfSerial.ToString().Substring(0, MnfSerial.Length - 1)})").Select(s => new SysEquipmentColumn { MnfSerial = s.MnfSerial, DocEntry = s.DocEntry }).ToListAsync();

            //      var DocEntryIds = Equipments.Select(e => e.DocEntry).ToList();

            //      var buyopors = from a in UnitWork.Find<buy_opor>(null)
            //                     join b in UnitWork.Find<sale_transport>(null) on a.DocEntry equals b.Buy_DocEntry
            //                     where DocEntryIds.Contains(b.Base_DocEntry) && b.Base_DocType == 24
            //                     select new { a, b };
            //      var docdate = await buyopors.ToListAsync();
            //      var IsProtecteds = Equipments.Select(e => new
            //      {
            //          MnfSerial = e.MnfSerial,
            //          DocDate = Convert.ToDateTime(docdate.Where(d => d.b.Base_DocEntry.Equals(e.DocEntry)).FirstOrDefault()?.a.DocDate).AddYears(1)
            //      }).ToList();

            return null;
        }

        #region 销售订单保修时间

        //private async Task<TableData> GetSalesOrder(SalesOrderWarrantyDateReq req)
        //{
        //    TableData result = new TableData();
        //    var query = from a in UnitWork.Find<ORDR>(null)
        //                join b in UnitWork.Find<OSLP>(null) on a.SlpCode equals b.SlpCode
        //                select new { a, b};
        //    query = query.WhereIf(!string.IsNullOrWhiteSpace(req.Customer), q => q.a.CardCode.Contains(req.Customer) || q.a.CardName.Contains(req.Customer))
        //                .WhereIf(!string.IsNullOrWhiteSpace(req.SalesOrderId.ToString()), q => q.a.DocEntry.Equals(req.SalesOrderId))
        //                .WhereIf(!string.IsNullOrWhiteSpace(req.SalesMan), q => q.b.SlpName.Equals(req.SalesMan));
        //    result.Data = await query.ToListAsync();
        //    return null;
        //}

        #endregion

        public QuotationApp(IUnitWork unitWork, FlowInstanceApp flowInstanceApp, ModuleFlowSchemeApp moduleFlowSchemeApp, IAuth auth) : base(unitWork, auth)
        {
            _flowInstanceApp = flowInstanceApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
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
                .Map<MaterialPrice>(0, a=>a.MaterialCode)
                .Map<MaterialPrice>(1,a=>a.SettlementPrice)
                .Map<MaterialPrice>(2,a=>a.SettlementPriceModel)
                .Take<MaterialPrice> (0);
                return data.Select(d => d.Value).SkipWhile(v => v is null).ToList();
            });
            MaterialPriceList.ForEach(m =>
            {
                m.CreateId = loginContext.User.Id;
                m.CreateName = loginContext.User.Name;
                m.CreateTime = DateTime.Now;
            });

            await UnitWork.BatchAddAsync(MaterialPriceList.ToArray());
            await UnitWork.SaveAsync();
        }
    }
}
