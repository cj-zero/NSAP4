using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAuth.App.Interface;
using OpenAuth.App.Material;
using OpenAuth.App.Response;
using OpenAuth.App.Workbench.Request;
using OpenAuth.App.Workbench.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Material;
using OpenAuth.Repository.Domain.Settlement;
using OpenAuth.Repository.Domain.Workbench;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Workbench
{
    /// <summary>
    /// 提交给我的
    /// </summary>
    public class PendingApp : OnlyUnitWorkBaeApp
    {
        public readonly QuotationApp _quotationApp;
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        /// <param name="quotationApp"></param>
        public PendingApp(IUnitWork unitWork, IAuth auth, QuotationApp quotationApp) : base(unitWork, auth)
        {
            _quotationApp = quotationApp;
        }
        /// <summary>
        /// 服务单详情
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceOrderResp> ServiceOrderDetails(int ServiceOrderId, string PetitionerId)
        {
            var petitioner = await (from a in UnitWork.Find<User>(u => u.Id.Equals(PetitionerId))
                                    join b in UnitWork.Find<Relevance>(r => r.Key == Define.USERORG) on a.Id equals b.FirstId into ab
                                    from b in ab.DefaultIfEmpty()
                                    join c in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on b.SecondId equals c.Id into bc
                                    from c in bc.DefaultIfEmpty()
                                    select new { a.Name, a.Id, OrgName = c.Name, c.CascadeId }).OrderByDescending(u => u.CascadeId).FirstOrDefaultAsync();
            var serviceDailyReportList = await UnitWork.Find<ServiceDailyReport>(s => ServiceOrderId == s.ServiceOrderId).ToListAsync();

            var serviceOrder = await UnitWork.Find<ServiceOrder>(s => s.Id == ServiceOrderId).Include(s => s.ServiceWorkOrders).Select(s => new ServiceOrderResp
            {
                ServiceOrderId = s.Id.ToString(),
                ServiceOrderSapId = s.U_SAP_ID.ToString(),
                SalesMan = s.SalesMan,
                Supervisor = s.Supervisor,
                NewestContacter = s.NewestContacter,
                NewestContactTel = s.NewestContactTel,
                TerminalCustomer = s.TerminalCustomer,
                TerminalCustomerId = s.TerminalCustomerId,
                Address = s.Address,
                ServiceWorkOrders = s.ServiceWorkOrders.Select(w => new ServiceWorkOrderResp
                {
                    ManufacturerSerialNumber = w.ManufacturerSerialNumber,
                    CreateTime = Convert.ToDateTime(w.CreateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                    MaterialCode = w.MaterialCode,
                    WorkOrderNumber = w.WorkOrderNumber,
                    FromTheme = w.FromTheme,
                    MaterialDescription = w.MaterialDescription,
                    Remark = w.Remark
                }).ToList()
            }).FirstOrDefaultAsync();
            serviceOrder.Petitioner = petitioner.OrgName + "-" + petitioner.Name;
            serviceOrder.PetitionerId = petitioner.Id;
            serviceOrder.ServiceDailyReports = serviceDailyReportList.Select(s => new ServiceDailyReportResp
            {
                CreateTime = Convert.ToDateTime(s.CreateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                ManufacturerSerialNumber = s.ManufacturerSerialNumber,
                MaterialCode = s.MaterialCode,
                ProcessDescription = GetServiceTroubleAndSolution(s.ProcessDescription),
                TroubleDescription = GetServiceTroubleAndSolution(s.TroubleDescription)
            }).ToList();
            return serviceOrder;
        }
        /// <summary>
        /// 报价单详情
        /// </summary>
        /// <returns></returns>
        public async Task<QuotationDetailsResp> QuotationDetails(int QuotationId)
        {
            var quotationObj = await _quotationApp.GeneralDetails(QuotationId,null);
            if (quotationObj == null)
            {
                return null;
            }
            var quotationPictures = await UnitWork.Find<QuotationPicture>(q => q.QuotationId == quotationObj.Id).ToListAsync();
            List<string> materialCodes = new List<string>();
            List<string> WhsCode = new List<string>();
            quotationObj.QuotationProducts.ForEach(q =>
            {
                WhsCode.AddRange(q.QuotationMaterials.Select(m => m.WhsCode).ToList());
                materialCodes.AddRange(q.QuotationMaterials.Select(m => m.MaterialCode).ToList());
            });
            var fileList = await UnitWork.Find<UploadFile>(f => (quotationPictures.Select(q=>q.PictureId).ToList()).Contains(f.Id)).ToListAsync();
           
            var quotationDetails = new QuotationDetailsResp
            {
                QuotationId = quotationObj.Id,
                AcceptancePeriod = quotationObj.AcceptancePeriod,
                AcquisitionWay = quotationObj.AcquisitionWay,
                InvoiceCategory = quotationObj.InvoiceCategory,
                TaxRate = quotationObj.TaxRate,
                CashBeforeFelivery = quotationObj.CashBeforeFelivery,
                SalesOrderId = quotationObj.SalesOrderId,
                DeliveryDate = quotationObj.DeliveryDate,
                MoneyMeans = quotationObj.MoneyMeans,
                PaymentAfterWarranty = quotationObj.PaymentAfterWarranty,
                DeliveryMethod = quotationObj.DeliveryMethod,
                InvoiceCompany = quotationObj.InvoiceCompany,
                IsMaterialType = quotationObj.IsMaterialType,
                Prepay = quotationObj.Prepay,
                Remark = quotationObj.Remark,
                Tentative = quotationObj.Tentative,
                UpDateTime = Convert.ToDateTime(quotationObj.UpDateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                PayOnReceipt = quotationObj.PayOnReceipt,
                TotalCostPrice = quotationObj.TotalCostPrice,
                TotalMoney = quotationObj.TotalMoney,
                QuotationStatus = quotationObj.QuotationStatus.ToString(),
                QuotationOperationHistorys = quotationObj.QuotationOperationHistorys.Select(o => new OperationHistoryResp
                {
                    ApprovalResult = o.ApprovalResult,
                    ApprovalStage = o.ApprovalStage,
                    Content = o.Action,
                    CreateTime = Convert.ToDateTime(o.CreateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                    CreateUserName = o.CreateUser,
                    IntervalTime = o.IntervalTime.ToString(),
                    Remark = o.Remark
                }).OrderBy(o => o.CreateTime).ToList(),
                QuotationProducts = quotationObj.QuotationProducts.Select(p => new QuotationProductResp
                {
                    IsProtected = p.IsProtected,
                    ProductCode = p.ProductCode,
                    MaterialCode = p.MaterialCode,
                    MaterialDescription = p.MaterialDescription,
                    WarrantyExpirationTime = p.WarrantyExpirationTime,
                    QuotationMaterials = p.QuotationMaterials.Select(m => new QuotationMaterialResp
                    {
                        MaterialCode = m.MaterialCode,
                        MaterialDescription = m.MaterialDescription,
                        Discount = m.Discount,
                        DiscountPrices = m.DiscountPrices,
                        SalesPrice = m.SalesPrice,
                        Count =Convert.ToInt32(m.Count),
                        WhsCode = m.WhsCode,
                        MaterialType = Convert.ToInt32(m.MaterialType),
                        MaxQuantity = m.MaxQuantity,
                        NewMaterialCode = m.NewMaterialCode,
                        Remark = m.Remark,
                        ReplaceMaterialCode = m.ReplaceMaterialCode,
                        Unit = m.Unit,
                        TotalPrice = m.TotalPrice,
                        UnitPrice = m.UnitPrice,
                        WarehouseQuantity= m.WarehouseQuantity.ToString(),
                        Files = m.QuotationMaterialPictures.Select(p => new FileResp
                        {
                            FileId = p.PictureId,
                            FileName = p.FileName,
                            FileType = p.FileType
                        }).ToList()
                    }).OrderBy(m => m.MaterialCode).ToList()
                }).OrderBy(p => p.MaterialCode).ToList(),
                FlowPathResp = new List<FlowPathResp>(),
                Files = quotationPictures.Select(q => new FileResp
                {
                    FileId = q.PictureId,
                    FileName = fileList.Where(f=>f.Id.Equals(q.PictureId)).FirstOrDefault()?.FileName,
                    FileType = fileList.Where(f => f.Id.Equals(q.PictureId)).FirstOrDefault()?.FileType
                }).ToList()
            };
            quotationDetails.FlowPathResp = await FlowPathRespList(quotationDetails.QuotationOperationHistorys, quotationObj.FlowInstanceId,int.Parse(quotationObj.IsMaterialType));
            return quotationDetails;
        }
        /// <summary>
        /// 退料单详情
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnnoteDetailsResp> ReturnnoteDetails(int ReturnnoteId)
        {
            var returNnoteObj = await UnitWork.Find<ReturnNote>(r => r.Id == ReturnnoteId).Include(r => r.ReturnNotePictures).Include(r => r.ReturnnoteMaterials)
                .ThenInclude(r => r.ReturnNoteMaterialPictures).FirstOrDefaultAsync();

            List<string> fileIds = new List<string>();
            var numberIds = returNnoteObj.ReturnnoteMaterials.Select(r => r.Id).ToList();
            var numbers = await UnitWork.Find<ReturnnoteMaterialNumber>(r => numberIds.Contains(r.ReturnnoteMaterialId)).ToListAsync();
            returNnoteObj.ReturnnoteMaterials.ForEach(r => { fileIds.AddRange(r.ReturnNoteMaterialPictures.Select(n => n.PictureId).ToList()); r.ReturnnoteMaterialNumbers = numbers.Where(n => n.ReturnnoteMaterialId == r.Id).ToList(); });
            fileIds.AddRange(returNnoteObj.ReturnNotePictures.Select(r => r.PictureId).ToList());
            var fileList = await UnitWork.Find<UploadFile>(f => fileIds.Contains(f.Id)).ToListAsync();
            var materialIds = returNnoteObj.ReturnnoteMaterials.Select(r => r.QuotationMaterialId).ToList();
            var quotationMaterials = await UnitWork.Find<QuotationMergeMaterial>(q => materialIds.Contains(q.Id)).ToListAsync();
            var returnnoteDetails = new ReturnnoteDetailsResp
            {
                ReturnnoteId = returNnoteObj.Id.ToString(),
                DeliveryMethod = returNnoteObj.DeliveryMethod,
                ExpressNumber = returNnoteObj.ExpressNumber,
                FreightCharge = returNnoteObj.FreightCharge,
                Remark = returNnoteObj.Remark,
                TotalMoney = returNnoteObj.TotalMoney,
                UpdateTime = returNnoteObj.UpdateTime.ToString("yyyy.MM.dd HH:mm:ss"),
                ReturnNoteMaterials = returNnoteObj.ReturnnoteMaterials.Select(r => new ReturnNoteMaterialResp
                {
                    MaterialsId = r.Id,
                    Count = r.Count,
                    SecondQty = r.SecondQty,
                    ReceivingRemark = r.ReceivingRemark,
                    ShippingRemark = r.ShippingRemark,
                    GoodQty = r.GoodQty,
                    GoodWhsCode = r.GoodWhsCode,
                    InvoiceDocEntry = r.InvoiceDocEntry,
                    MaterialDescription = r.MaterialDescription,
                    MaterialCode = r.MaterialCode,
                    SecondWhsCode = r.SecondWhsCode,
                    ReplacePartCode = quotationMaterials.Where(q => q.Id.Equals(r.QuotationMaterialId)).FirstOrDefault()?.MaterialCode,
                    ReplacePartDescription = quotationMaterials.Where(q => q.Id.Equals(r.QuotationMaterialId)).FirstOrDefault()?.MaterialDescription,
                    TotalPrice = r.Count * quotationMaterials.Where(q => q.Id.Equals(r.QuotationMaterialId)).FirstOrDefault()?.DiscountPrices,
                    Price = quotationMaterials.Where(q => q.Id.Equals(r.QuotationMaterialId)).FirstOrDefault()?.DiscountPrices,
                    ReturnnoteMaterialNumberResps = r.ReturnnoteMaterialNumbers.Select(n => new ReturnnoteMaterialNumberResp
                    {
                        RemoveNumber = n.ReturnNumber,
                        ReturnNumber = n.ReturnNumber,
                        ReturnnoteMaterialId = n.ReturnnoteMaterialId
                    }).ToList(),
                    ReturnNoteId = r.ReturnNoteId,
                    Files = r.ReturnNoteMaterialPictures.Select(p => new FileResp
                    {
                        FileId = p.PictureId,
                        FileName = fileList.Where(f => f.Id.Equals(p.PictureId)).FirstOrDefault()?.FileName,
                        FileType = fileList.Where(f => f.Id.Equals(p.PictureId)).FirstOrDefault()?.FileType,
                    }).ToList()
                }).ToList()
            };
            var History = await UnitWork.Find<FlowInstanceOperationHistory>(f => f.InstanceId.Equals(returNnoteObj.FlowInstanceId)).OrderBy(f => f.CreateDate).ToListAsync();
            returnnoteDetails.ReturnNoteHistoryResp = History.Select(h => new OperationHistoryResp
            {
                CreateTime = h.CreateDate.ToString("yyyy.MM.dd HH:mm:ss"),
                Remark = h.Remark,
                IntervalTime = h.IntervalTime.ToString(),
                CreateUserName = h.CreateUserName,
                Content = h.Content,
                ApprovalResult = h.ApprovalResult,
                ApprovalStage = h.ApprovalStage
            }).ToList();
            returnnoteDetails.FlowPathResp = await FlowPathRespList(returnnoteDetails.ReturnNoteHistoryResp, returNnoteObj.FlowInstanceId);
            returnnoteDetails.ReturnNotePictures = returNnoteObj.ReturnNotePictures.Select(r => new FileResp
            {
                FileId = r.PictureId,
                FileName = fileList.Where(f => f.Id.Equals(r.PictureId)).FirstOrDefault()?.FileName,
                FileType = fileList.Where(f => f.Id.Equals(r.PictureId)).FirstOrDefault()?.FileType
            }).ToList();
            return returnnoteDetails;
        }
        /// <summary>
        /// 结算单详情
        /// </summary>
        /// <returns></returns>
        public async Task<OutsourcDetailsResp> OutsourcDetails(int OutsourcId)
        {
            var outsourcObj = await UnitWork.Find<Outsourc>(o => o.Id == OutsourcId).Include(o => o.OutsourcExpenses).ThenInclude(o => o.outsourcexpensespictures)
                .FirstOrDefaultAsync();
            var expenseIds = outsourcObj.OutsourcExpenses.Select(o => o.Id).ToList();
            var expenseOrgs = await UnitWork.Find<OutsourcExpenseOrg>(o => expenseIds.Contains(o.ExpenseId)).ToListAsync();
            var outsourcDetails = new OutsourcDetailsResp
            {
                OutsourcId = outsourcObj.Id.ToString(),
                ServiceMode = outsourcObj.ServiceMode,
                Remark = outsourcObj.Remark,
                TotalMoney = outsourcObj.TotalMoney,
                UpdateTime = Convert.ToDateTime(outsourcObj.UpdateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                OutsourcExpenses = outsourcObj.OutsourcExpenses.Select(e => new OutsourcExpensesResp
                {
                    Id = e.Id,
                    FromLat = e.FromLat,
                    Days = e.Days,
                    From = e.From,
                    FromLng = e.FromLng,
                    ManHour = e.ManHour,
                    ExpenseType = e.ExpenseType,
                    Money = e.Money,
                    To = e.To,
                    ToLat = e.ToLat,
                    ToLng = e.ToLng,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    OutsourcExpenseOrgs = expenseOrgs.Where(o => o.ExpenseId.Equals(e.Id)).ToList(),
                    Files = e.outsourcexpensespictures.Select(p => new FileResp
                    {
                        FileId = p.PictureId,
                        FileName = p.FileName,
                        FileType = p.FileType
                    }).ToList()
                }).OrderBy(r => r.StartTime).ToList()
            };
            var outsourcOperationHistorys = await UnitWork.Find<FlowInstanceOperationHistory>(f => f.InstanceId.Equals(outsourcObj.FlowInstanceId)).OrderBy(f => f.CreateDate)
                .ToListAsync();
            outsourcDetails.OutsourcOperationHistory = outsourcOperationHistorys.Select(f => new OperationHistoryResp
            {
                ApprovalStage = f.ApprovalStage,
                ApprovalResult = f.ApprovalResult,
                Content = f.Content,
                CreateTime = f?.CreateDate.ToString("yyyy.MM.dd HH:mm:ss"),
                CreateUserName = f?.CreateUserName,
                IntervalTime = f?.IntervalTime.ToString(),
                Remark = f.Remark
            }).OrderBy(f => f.CreateTime).ToList();
            outsourcDetails.FlowPathResp = await FlowPathRespList(outsourcDetails.OutsourcOperationHistory, outsourcObj.FlowInstanceId);
            return outsourcDetails;
        }
        /// <summary>
        /// 报销单详情
        /// </summary>
        /// <returns></returns>
        public async Task<ReimburseDetailsResp> ReimburseDetails(int ReimburseId)
        {
            var reimburseObj = await UnitWork.Find<ReimburseInfo>(r => r.MainId == ReimburseId).Include(r => r.ReimburseFares)
                .Include(r => r.ReimburseOtherCharges).Include(r => r.ReimburseTravellingAllowances).Include(r => r.ReimurseOperationHistories).Include(r => r.ReimburseAccommodationSubsidies).FirstOrDefaultAsync();

            List<string> fileids = new List<string>();
            List<ReimburseAttachment> filemodel = new List<ReimburseAttachment>();
            List<ReimburseExpenseOrg> expenseOrg = new List<ReimburseExpenseOrg>();
            if (reimburseObj.ReimburseTravellingAllowances != null && reimburseObj.ReimburseTravellingAllowances.Count > 0)
            {
                var rtaids = reimburseObj.ReimburseTravellingAllowances.Select(r => r.Id).ToList();
                expenseOrg.AddRange(await UnitWork.Find<ReimburseExpenseOrg>(r => rtaids.Contains(r.ExpenseId) && r.ExpenseType == 1).ToListAsync());
            }
            if (reimburseObj.ReimburseFares != null && reimburseObj.ReimburseFares.Count > 0)
            {
                var rfids = reimburseObj.ReimburseFares.Select(r => r.Id).ToList();
                filemodel = await UnitWork.Find<ReimburseAttachment>(r => rfids.Contains(r.ReimburseId) && r.ReimburseType == 2).ToListAsync();
                expenseOrg.AddRange(await UnitWork.Find<ReimburseExpenseOrg>(r => rfids.Contains(r.ExpenseId) && r.ExpenseType == 2).ToListAsync());
            }
            if (reimburseObj.ReimburseAccommodationSubsidies != null && reimburseObj.ReimburseAccommodationSubsidies.Count > 0)
            {
                var rasids = reimburseObj.ReimburseAccommodationSubsidies.Select(r => r.Id).ToList();
                filemodel.AddRange(await UnitWork.Find<ReimburseAttachment>(r => rasids.Contains(r.ReimburseId) && r.ReimburseType == 3).ToListAsync());
                expenseOrg.AddRange(await UnitWork.Find<ReimburseExpenseOrg>(r => rasids.Contains(r.ExpenseId) && r.ExpenseType == 3).ToListAsync());
            }
            if (reimburseObj.ReimburseOtherCharges != null && reimburseObj.ReimburseOtherCharges.Count > 0)
            {
                var rocids = reimburseObj.ReimburseOtherCharges.Select(r => r.Id).ToList();
                filemodel.AddRange(await UnitWork.Find<ReimburseAttachment>(r => rocids.Contains(r.ReimburseId) && r.ReimburseType == 4).ToListAsync());
                expenseOrg.AddRange(await UnitWork.Find<ReimburseExpenseOrg>(r => rocids.Contains(r.ExpenseId) && r.ExpenseType == 4).ToListAsync());
            }
            fileids.AddRange(filemodel.Select(f => f.FileId).ToList());
            var file = await UnitWork.Find<UploadFile>(f => fileids.Contains(f.Id)).ToListAsync();
            var reimburseDetails = new ReimburseDetailsResp
            {
                ReimburseId = reimburseObj.Id,
                UpdateTime = reimburseObj.UpdateTime,
                Remark = reimburseObj.Remark,
                TotalMoney = reimburseObj.TotalMoney,
                ReimburseMainId = reimburseObj.MainId,
                ReimburseFares = reimburseObj.ReimburseFares.Select(f => new ReimburseFareResp
                {
                    Id = f.Id,
                    SerialNumber = f.SerialNumber,
                    CreateTime = f.CreateTime.ToString("yyyy.MM.dd HH:mm:ss"),
                    ExpenseOrg = f.ExpenseOrg,
                    From = f.From,
                    FromLat = f.FromLat,
                    FromLng = f.FromLng,
                    InvoiceNumber = f.InvoiceNumber,
                    InvoiceTime = Convert.ToDateTime(f.InvoiceTime).ToString("yyyy.MM.dd HH:mm:ss"),
                    Money = f.Money,
                    Remark = f.Remark,
                    To = f.To,
                    ToLat = f.ToLat,
                    ToLng = f.ToLng,
                    TrafficType = f.TrafficType,
                    Transport = f.Transport,
                    ExpenseType = "2",
                    Files = filemodel.Where(m => m.ReimburseId == f.Id && m.ReimburseType == 2).Select(m => new FileResp
                    {
                        FileId = m.FileId,
                        FileName = file.Where(s => s.Id.Equals(m.FileId)).FirstOrDefault().FileName,
                        FileType = file.Where(s => s.Id.Equals(m.FileId)).FirstOrDefault().FileType,
                        AttachmentType = m.AttachmentType.ToString()
                    }).ToList(),
                    ReimburseExpenseOrgs = expenseOrg.Where(e => e.ExpenseType == 2 && e.ExpenseId == f.Id).ToList()
                }).OrderBy(r => r.InvoiceTime).ToList(),
                ReimburseAccommodationSubsidies = reimburseObj.ReimburseAccommodationSubsidies.Select(a => new ReimburseAccommodationSubsidyResp
                {
                    Id = a.Id,
                    SellerName = a.SellerName,
                    CreateTime = a.CreateTime.ToString("yyyy.MM.dd HH:mm:ss"),
                    Days = a.Days,
                    ExpenseOrg = a.ExpenseOrg,
                    InvoiceNumber = a.InvoiceNumber,
                    InvoiceTime = Convert.ToDateTime(a.InvoiceTime).ToString("yyyy.MM.dd HH:mm:ss"),
                    Money = a.Money,
                    TotalMoney = a.TotalMoney,
                    Remark = a.Remark,
                    SerialNumber = a.SerialNumber,
                    ExpenseType = "3",
                    Files = filemodel.Where(m => m.ReimburseId == a.Id && m.ReimburseType == 3).Select(m => new FileResp
                    {
                        FileId = m.FileId,
                        FileName = file.Where(s => s.Id.Equals(m.FileId)).FirstOrDefault().FileName,
                        FileType = file.Where(s => s.Id.Equals(m.FileId)).FirstOrDefault().FileType,
                        AttachmentType = m.AttachmentType.ToString()
                    }).ToList(),
                    ReimburseExpenseOrgs = expenseOrg.Where(e => e.ExpenseType == 3 && e.ExpenseId == a.Id).ToList()
                }).OrderBy(r => r.InvoiceTime).ToList(),
                ReimburseOtherCharges = reimburseObj.ReimburseOtherCharges.Select(o => new ReimburseOtherChargesResp
                {
                    Id = o.Id,
                    CreateTime = o.CreateTime.ToString("yyyy.MM.dd HH:mm:ss"),
                    ExpenseCategory = o.ExpenseCategory,
                    ExpenseOrg = o.ExpenseOrg,
                    SerialNumber = o.SerialNumber,
                    InvoiceNumber = o.InvoiceNumber,
                    InvoiceTime = Convert.ToDateTime(o.InvoiceTime).ToString("yyyy.MM.dd HH:mm:ss"),
                    Money = o.Money,
                    Remark = o.Remark,
                    ExpenseType = "4",
                    Files = filemodel.Where(m => m.ReimburseId == o.Id && m.ReimburseType == 4).Select(m => new FileResp
                    {
                        FileId = m.FileId,
                        FileName = file.Where(s => s.Id.Equals(m.FileId)).FirstOrDefault().FileName,
                        FileType = file.Where(s => s.Id.Equals(m.FileId)).FirstOrDefault().FileType,
                        AttachmentType = m.AttachmentType.ToString()
                    }).ToList(),
                    ReimburseExpenseOrgs = expenseOrg.Where(e => e.ExpenseType == 4 && e.ExpenseId == o.Id).ToList()
                }).OrderBy(r => r.InvoiceTime).ToList(),
                ReimburseTravellingAllowances = reimburseObj.ReimburseTravellingAllowances.Select(t => new ReimburseTravellingAllowanceResp
                {
                    Id = t.Id,
                    SerialNumber = t.SerialNumber,
                    CreateTime = Convert.ToDateTime(t.CreateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                    Days = t.Days,
                    ExpenseOrg = t.ExpenseOrg,
                    Money = t.Money,
                    Remark = t.Remark,
                    ExpenseType = "1",
                    ReimburseExpenseOrgs = expenseOrg.Where(e => e.ExpenseType == 1 && e.ExpenseId == t.Id).ToList()
                }).OrderBy(r => r.CreateTime).ToList(),
                ReimurseOperationHistories = reimburseObj.ReimurseOperationHistories.Select(o => new OperationHistoryResp
                {
                    ApprovalStage = o.ApprovalStage.ToString(),
                    Content = o.Action,
                    ApprovalResult = o.ApprovalResult,
                    CreateTime = Convert.ToDateTime(o.CreateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                    CreateUserName = o.CreateUser,
                    Remark = o.Remark,
                    IntervalTime = o.IntervalTime.ToString()
                }).OrderBy(r => r.CreateTime).ToList()
            };
            reimburseDetails.FlowPathResp = await FlowPathRespList(reimburseDetails.ReimurseOperationHistories, reimburseObj.FlowInstanceId);
            return reimburseDetails;
        }

        /// <summary>
        /// 获取待处理订单详情
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> PendingDetails(PendingReq req)
        {
            var reult = new TableData();
            var pendingObj = await UnitWork.Find<WorkbenchPending>(w => w.ApprovalNumber == int.Parse(req.ApprovalNumber)).FirstOrDefaultAsync();
            var serviceOrderDetails = await ServiceOrderDetails(pendingObj.ServiceOrderId, pendingObj.PetitionerId);
            QuotationDetailsResp quotationDetails = null;
            ReturnnoteDetailsResp returnnoteDetails = null;
            OutsourcDetailsResp outsourcDetails = null;
            ReimburseDetailsResp reimburseDetails = null;
            var quotation = new Quotation();
            switch (pendingObj.OrderType)
            {
                case 1:
                    quotationDetails = await QuotationDetails(pendingObj.SourceNumbers);
                    break;
                case 2:
                    var quotationId = (await (from a in UnitWork.Find<ReturnNote>(r => r.Id == pendingObj.SourceNumbers)
                                              join b in UnitWork.Find<Quotation>(null) on a.SalesOrderId equals b.SalesOrderId
                                              select new { b.Id }).FirstOrDefaultAsync()).Id;
                    quotationDetails = await QuotationDetails(quotationId);
                    returnnoteDetails = await ReturnnoteDetails(pendingObj.SourceNumbers);
                    break;
                case 3:
                    var returnnoteObj = await UnitWork.Find<ReturnNote>(q => q.ServiceOrderId == pendingObj.ServiceOrderId).OrderByDescending(q => q.CreateTime).Select(q => new { q.Id, q.SalesOrderId }).FirstOrDefaultAsync();
                    if (returnnoteObj != null)
                    {
                        quotation = await UnitWork.Find<Quotation>(q => q.SalesOrderId.Equals(returnnoteObj.SalesOrderId)).OrderByDescending(q => q.CreateTime).FirstOrDefaultAsync();
                        returnnoteDetails = await ReturnnoteDetails(returnnoteObj.Id);
                    }
                    else
                    {
                        quotation = await UnitWork.Find<Quotation>(q => q.ServiceOrderId.Equals(pendingObj.ServiceOrderId) && q.QuotationStatus == 11).OrderByDescending(q => q.CreateTime).FirstOrDefaultAsync();
                    }
                    if (quotation != null)
                    {
                        quotationDetails = await QuotationDetails(quotation.Id);
                    }
                    outsourcDetails = await OutsourcDetails(pendingObj.SourceNumbers);
                    break;
                case 4:
                    returnnoteObj = await UnitWork.Find<ReturnNote>(q => q.ServiceOrderId == pendingObj.ServiceOrderId).OrderByDescending(q => q.CreateTime).Select(q => new { q.Id, q.SalesOrderId }).FirstOrDefaultAsync();
                    if (returnnoteObj != null)
                    {
                        quotation = await UnitWork.Find<Quotation>(q => q.SalesOrderId.Equals(returnnoteObj.SalesOrderId)).OrderByDescending(q => q.CreateTime).FirstOrDefaultAsync();
                        returnnoteDetails = await ReturnnoteDetails(returnnoteObj.Id);
                    }
                    else 
                    {
                        quotation = await UnitWork.Find<Quotation>(q => q.ServiceOrderId.Equals(pendingObj.ServiceOrderId) && q.QuotationStatus==11).OrderByDescending(q => q.CreateTime).FirstOrDefaultAsync();
                    }
                    if (quotation != null)
                    {
                        quotationDetails = await QuotationDetails(quotation.Id);
                    }
                    reimburseDetails = await ReimburseDetails(pendingObj.SourceNumbers);
                    break;
            }
            if (pendingObj.OrderType == 3 || pendingObj.OrderType == 4)
            {
                var completionReport = await UnitWork.Find<CompletionReport>(c => c.ServiceOrderId.ToString() == serviceOrderDetails.ServiceOrderId && c.CreateUserId.Equals(pendingObj.PetitionerId)).FirstOrDefaultAsync();
                serviceOrderDetails.Destination = completionReport.Destination;
                serviceOrderDetails.Becity = completionReport.Becity;
            }
            reult.Data = new
            {
                serviceOrderDetails,
                quotationDetails,
                returnnoteDetails,
                outsourcDetails,
                reimburseDetails
            };
            return reult;
        }

        /// <summary>
        ///判断增加还是修改待处理
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> Load(PendingReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var reult = new TableData();

            if (req.PageType == 1)
            {
                //待处理
                var query = from a in UnitWork.Find<WorkbenchPending>(null)
                            join b in UnitWork.Find<FlowInstance>(null) on a.FlowInstanceId equals b.Id
                            where (b.MakerList.Contains(loginContext.User.Id) || (b.MakerList == "1" && b.CustomName.Contains("物料报价单"))) && b.ActivityName!="待出库" && b.ActivityName != "开始"
                            select new { a, b };
                query = query.WhereIf(!string.IsNullOrWhiteSpace(req.ApprovalNumber), q => q.a.ApprovalNumber == int.Parse(req.ApprovalNumber))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.Petitioner), q => q.a.Petitioner.Contains(req.Petitioner))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.OrderType), q => q.a.OrderType == int.Parse(req.OrderType))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.TerminalCustomer), q => q.a.TerminalCustomer.Contains(req.TerminalCustomer))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.TerminalCustomerId), q => q.a.TerminalCustomerId.Contains(req.TerminalCustomerId))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.StartTime.ToString()), q => q.a.UpdateTime > req.StartTime)
                            .WhereIf(!string.IsNullOrWhiteSpace(req.EndTime.ToString()), q => q.a.UpdateTime > Convert.ToDateTime(req.EndTime).AddDays(1));
                var pending = await query.Select(q => new
                {
                    q.a.TotalMoney,
                    q.a.ApprovalNumber,
                    q.a.TerminalCustomer,
                    q.a.TerminalCustomerId,
                    q.a.SourceNumbers,
                    q.a.ServiceOrderSapId,
                    q.a.Remark,
                    q.a.Petitioner,
                    q.a.PetitionerId,
                    q.a.OrderType,
                    q.b.ActivityName,
                    q.a.UpdateTime
                }).OrderByDescending(o=>o.UpdateTime).ToListAsync();
                List<int> salesManIds = new List<int>();
                foreach (var p in pending)
                {
                    if (p.OrderType == 1 && p.ActivityName == "销售员审批")
                    {
                        var salesManId = (await UnitWork.Find<ServiceOrder>(s => s.U_SAP_ID == p.ServiceOrderSapId).FirstOrDefaultAsync())?.SalesManId;
                        if (!loginContext.User.Id.Equals(salesManId))
                        {
                            salesManIds.Add(p.ApprovalNumber);
                        }
                    }else if(p.OrderType == 1 && p.ActivityName == "确认报价单")
                    {
                        if (!loginContext.User.Id.Equals(p.PetitionerId))
                        {
                            salesManIds.Add(p.ApprovalNumber);
                        }
                    }
                    else if (p.OrderType == 1 && p.ActivityName == "回传销售订单")
                    {
                        if (!loginContext.User.Id.Equals(p.PetitionerId))
                        {
                            salesManIds.Add(p.ApprovalNumber);
                        }
                    }
                }
                reult.Data = pending.Where(p => !salesManIds.Contains(p.ApprovalNumber)).Skip((req.page - 1) * req.limit).Take(req.limit).ToList();
                reult.Count = pending.Where(p => !salesManIds.Contains(p.ApprovalNumber)).Count();
            }
            else if (req.PageType == 2)
            {
                //已处理
                var query = from a in UnitWork.Find<WorkbenchPending>(null)
                            join b in UnitWork.Find<FlowInstance>(null) on a.FlowInstanceId equals b.Id into ab
                            from b in ab.DefaultIfEmpty()
                            join c in UnitWork.Find<FlowInstanceOperationHistory>(null).Select(f => new { f.InstanceId, f.CreateUserId }).Distinct() on b.Id equals c.InstanceId into bc
                            from c in bc.DefaultIfEmpty()
                            where c.CreateUserId.Equals(loginContext.User.Id)
                            select new { a, b };
                query = query.WhereIf(!string.IsNullOrWhiteSpace(req.ApprovalNumber), q => q.a.ApprovalNumber == int.Parse(req.ApprovalNumber))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.Petitioner), q => q.a.Petitioner.Contains(req.Petitioner))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.OrderType), q => q.a.OrderType == int.Parse(req.OrderType))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.TerminalCustomer), q => q.a.TerminalCustomer.Contains(req.TerminalCustomer))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.TerminalCustomerId), q => q.a.TerminalCustomerId.Contains(req.TerminalCustomerId))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.StartTime.ToString()), q => q.a.UpdateTime > req.StartTime)
                            .WhereIf(!string.IsNullOrWhiteSpace(req.EndTime.ToString()), q => q.a.UpdateTime > Convert.ToDateTime(req.EndTime).AddDays(1));
                reult.Data = await query.Select(q => new
                {
                    q.a.TotalMoney,
                    q.a.ApprovalNumber,
                    q.a.TerminalCustomer,
                    q.a.TerminalCustomerId,
                    q.a.SourceNumbers,
                    q.a.ServiceOrderSapId,
                    q.a.Remark,
                    q.a.Petitioner,
                    q.a.OrderType,
                    ActivityName = q.b.ActivityName == "开始" ? "驳回" : q.b.ActivityName,
                    q.a.UpdateTime
                }).OrderByDescending(q=>q.UpdateTime).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
                reult.Count = await query.CountAsync();
            }
            return reult;
        }

        /// <summary>
        /// 解析完工报告json
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<string> GetServiceTroubleAndSolution(string data)
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(data))
            {
                JArray jArray = (JArray)JsonConvert.DeserializeObject(data);
                foreach (var item in jArray)
                {
                    result.Add(item["description"].ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// 获取生命周期
        /// </summary>
        /// <param name="reqp"></param>
        /// <param name="FlowInstanceId"></param>
        /// <param name="IsMaterialType"></param>
        /// <returns></returns>
        private async Task<List<FlowPathResp>> FlowPathRespList(List<OperationHistoryResp> reqp, string FlowInstanceId, int IsMaterialType = 0)
        {
            var flowInstance = await UnitWork.Find<FlowInstance>(f => f.Id.Equals(FlowInstanceId)).Select(f => new { f.SchemeContent ,f.ActivityName}).FirstOrDefaultAsync();
            var schemeContentJson = JsonHelper.Instance.Deserialize<FlowInstanceJson>(flowInstance.SchemeContent);
            List<FlowInstanceNodes> flowInstanceNodes = new List<FlowInstanceNodes>();
            List<FlowPathResp> flowPathResps = new List<FlowPathResp>();
            int number = 1;
            flowInstanceNodes.Add(new FlowInstanceNodes { Name = "提交", Number = number });
            string toId = schemeContentJson.lines.Where(s => s.from.Contains("start")).FirstOrDefault()?.to;
            var query = from a in schemeContentJson.lines
                        join b in schemeContentJson.nodes on a.@from equals b.id
                        select new { a.to, b.id, b.Name, a.Compares };
            query = query.Where(q => !q.id.Contains("start") && !q.id.Contains("end")).ToList();
            foreach (var item in query)
            {
                if (query.Where(n => n.id.Equals(toId)).FirstOrDefault() != null)
                {
                    string toName = "";
                    if (IsMaterialType != 0)
                    {
                        if (IsMaterialType == 1 || IsMaterialType == 3)
                        {
                            toName = query.Where(n => n.id.Equals(toId) && (n.Compares == null || string.IsNullOrWhiteSpace(n.Compares.FirstOrDefault()?.Value) || n.Compares.Select(c => c.Value).FirstOrDefault() == "2")).FirstOrDefault().Name;
                            toId = query.Where(n => n.id.Equals(toId) && (n.Compares == null || string.IsNullOrWhiteSpace(n.Compares.FirstOrDefault()?.Value) || n.Compares.Select(c => c.Value).FirstOrDefault() == "2")).FirstOrDefault().to;
                        }
                        else
                        {
                            toName = query.Where(n => n.id.Equals(toId) && (n.Compares == null || string.IsNullOrWhiteSpace(n.Compares.FirstOrDefault()?.Value) || n.Compares.Select(c => c.Value).FirstOrDefault() == "1")).FirstOrDefault().Name;
                            toId = query.Where(n => n.id.Equals(toId) && (n.Compares == null || string.IsNullOrWhiteSpace(n.Compares.FirstOrDefault()?.Value) || n.Compares.Select(c => c.Value).FirstOrDefault() == "1")).FirstOrDefault().to;
                        }
                    }
                    else
                    {
                        toName = query.Where(n => n.id.Equals(toId)).FirstOrDefault().Name;
                        toId = query.Where(n => n.id.Equals(toId)).FirstOrDefault().to;
                    }
                    flowInstanceNodes.Add(new FlowInstanceNodes { Name = toName, Number = ++number });

                }
            }
            flowInstanceNodes.Add(new FlowInstanceNodes { Name = "结束", Number = ++number });
            flowInstanceNodes = flowInstanceNodes.OrderBy(f => f.Number).ToList();
            string historys = null;
            flowInstanceNodes.ForEach(f =>
            {
                var operationHistorys = reqp.Where(q => q.Content.Contains(f.Name) || (f.Name == "审批结束" && (q.Content == "已支付" || q.Content == "出库完成" || q.Content == "结束"))||(f.Name == "待出库"&& q.Content == "开始出库")).OrderByDescending(q=>q.CreateTime).FirstOrDefault();

                if (historys == null || (operationHistorys?.CreateTime != null && DateTime.Parse(historys) < DateTime.Parse(operationHistorys.CreateTime)))
                {
                    historys = operationHistorys?.CreateTime;
                    flowPathResps.Add(new FlowPathResp
                    {
                        ActivityName = f.Name,
                        Number = f.Number,
                        CreateTime = operationHistorys?.CreateTime,
                        IntervalTime = operationHistorys?.IntervalTime,
                        IsNode = true
                    });
                }
                else
                {
                    if (flowInstance.ActivityName == "结束")
                    {
                        flowPathResps.Add(new FlowPathResp
                        {
                            ActivityName = f.Name,
                            Number = f.Number,
                            IsNode = true
                        });
                    }
                    else 
                    {
                        flowPathResps.Add(new FlowPathResp
                        {
                            ActivityName = f.Name,
                            Number = f.Number,
                            IsNode = false
                        });
                    }
                    
                }

            });
            return flowPathResps;
        }


    }
}
