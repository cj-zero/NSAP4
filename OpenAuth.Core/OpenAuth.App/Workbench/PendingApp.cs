using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAuth.App.Interface;
using OpenAuth.App.Material;
using OpenAuth.App.Response;
using OpenAuth.App.Sap.BusinessPartner;
using OpenAuth.App.Workbench.Request;
using OpenAuth.App.Workbench.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Material;
using OpenAuth.Repository.Domain.Sap;
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
        public readonly FlowInstanceApp _flowInstanceApp;
        public readonly UserManagerApp _userManagerApp;
        private readonly OrgManagerApp _orgApp;
        private readonly BusinessPartnerApp _businessPartnerApp;
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        /// <param name="quotationApp"></param>
        public PendingApp(IUnitWork unitWork, IAuth auth, QuotationApp quotationApp, UserManagerApp userManagerApp, FlowInstanceApp flowInstanceApp, OrgManagerApp orgApp, BusinessPartnerApp businessPartnerApp) : base(unitWork, auth)
        {
            _quotationApp = quotationApp;
            _flowInstanceApp = flowInstanceApp;
            _userManagerApp = userManagerApp;
            _orgApp = orgApp;
            _businessPartnerApp = businessPartnerApp;
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
            var orgrole = await _orgApp.GetOrgNameAndRoleIdentity(PetitionerId);

            var serviceOrder = await UnitWork.Find<ServiceOrder>(s => s.Id == ServiceOrderId).Include(s => s.ServiceWorkOrders).Select(s => new ServiceOrderResp
            {
                ServiceOrderId = s.Id.ToString(),
                ServiceOrderSapId = s.U_SAP_ID.ToString(),
                SalesManId = s.SalesManId,
                SalesMan = s.SalesMan,
                SupervisorId = s.SupervisorId,
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
            serviceOrder.Balance = await UnitWork.Find<OCRD>(o => serviceOrder.TerminalCustomerId.Contains(o.CardCode)).Select(o => o.Balance.ToString()).FirstOrDefaultAsync();
            //serviceOrder.Petitioner = petitioner.OrgName + "-" + petitioner.Name;
            serviceOrder.Petitioner = petitioner.Name;
            serviceOrder.PetitionerDept = petitioner.OrgName;
            serviceOrder.PetitionerId = petitioner.Id;
            serviceOrder.RoleIdentity = orgrole.RoleIdentity;
            serviceOrder.ServiceDailyReports = serviceDailyReportList.Select(s => new ServiceDailyReportResp
            {
                CreateTime = Convert.ToDateTime(s.CreateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                ManufacturerSerialNumber = s.ManufacturerSerialNumber,
                MaterialCode = s.MaterialCode,
                ProcessCode = GetServiceTroubleAndSolution(s.ProcessDescription, "code"),
                ProcessDescription = GetServiceTroubleAndSolution(s.ProcessDescription, "description"),
                TroubleCode = GetServiceTroubleAndSolution(s.TroubleDescription, "code"),
                TroubleDescription = GetServiceTroubleAndSolution(s.TroubleDescription, "description")
            }).ToList();
            return serviceOrder;
        }
        /// <summary>
        /// 报价单详情
        /// </summary>
        /// <returns></returns>
        public async Task<QuotationDetailsResp> QuotationDetails(int QuotationId)
        {
            var quotationObj = await _quotationApp.GeneralDetails(QuotationId, null);
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
            var fileList = await UnitWork.Find<UploadFile>(f => (quotationPictures.Select(q => q.PictureId).ToList()).Contains(f.Id)).ToListAsync();
            List<LogisticsRecord> LogisticsRecords = new List<LogisticsRecord>();
            var QuotationMergeMaterials = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId == QuotationId).ToListAsync();
            var ExpressageList = await UnitWork.Find<Expressage>(e => e.QuotationId == QuotationId).Include(e => e.LogisticsRecords).ToListAsync();
            foreach (var item in ExpressageList)
            {
                LogisticsRecords.AddRange(item.LogisticsRecords.ToList());
            }

            var MergeMaterials = from a in QuotationMergeMaterials
                                 join b in LogisticsRecords on a.Id equals b.QuotationMaterialId
                                 select new LogisticsRecordResp
                                 {
                                     MaterialCode = a.MaterialCode,
                                     MaterialDescription = a.MaterialDescription,
                                     Count = a.Count,
                                     Unit = a.Unit,
                                     SentQuantity = a.SentQuantity,
                                     Quantity = b.Quantity,
                                     WhsCode = a.WhsCode
                                 };

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
                WarrantyType = quotationObj.WarrantyType,
                Tentative = quotationObj.Tentative,
                UpDateTime = Convert.ToDateTime(quotationObj.UpDateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                PayOnReceipt = quotationObj.PayOnReceipt,
                TotalCostPrice = quotationObj.TotalCostPrice,
                TotalMoney = quotationObj.TotalMoney,
                QuotationStatus = quotationObj.QuotationStatus.ToString(),
                FlowInstanceId = quotationObj.FlowInstanceId,
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
                    WarrantyTime = p.WarrantyTime,
                    QuotationMaterials = p.QuotationMaterials.Select(m => new QuotationMaterialResp
                    {
                        MaterialCode = m.MaterialCode,
                        MaterialDescription = m.MaterialDescription,
                        Discount = m.Discount,
                        DiscountPrices = m.DiscountPrices,
                        SalesPrice = m.SalesPrice,
                        Count = m.Count.ToString(),//Convert.ToDecimal(m.Count).ToString("#0.00"),
                        WhsCode = m.WhsCode,
                        MaterialType = Convert.ToInt32(m.MaterialType),
                        MaxQuantity = m.MaxQuantity,
                        NewMaterialCode = m.NewMaterialCode,
                        Remark = m.Remark,
                        ReplaceMaterialCode = m.ReplaceMaterialCode,
                        Unit = m.Unit,
                        TotalPrice = m.TotalPrice,
                        UnitPrice = m.UnitPrice,
                        WarehouseQuantity = m.WarehouseQuantity.ToString(),
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
                    FileName = fileList.Where(f => f.Id.Equals(q.PictureId)).FirstOrDefault()?.FileName,
                    FileType = fileList.Where(f => f.Id.Equals(q.PictureId)).FirstOrDefault()?.FileType
                }).ToList(),
                LogisticsRecords = MergeMaterials.ToList()
            };
            if (!string.IsNullOrWhiteSpace(quotationObj.FlowInstanceId)) 
            {
                quotationDetails.FlowPathResp = await _flowInstanceApp.FlowPathRespList(quotationDetails.QuotationOperationHistorys, quotationObj.FlowInstanceId);
            }
            return quotationDetails;
        }
        /// <summary>
        /// 退料单详情
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnnoteDetailsResp> ReturnnoteDetails(int ReturnnoteId)
        {
            var returNnoteObj = await UnitWork.Find<ReturnNote>(r => r.Id == ReturnnoteId).Include(r => r.ReturnNotePictures).Include(r => r.ReturnNoteProducts).ThenInclude(r=>r.ReturnNoteMaterials).FirstOrDefaultAsync();

            var  fileIds = returNnoteObj.ReturnNotePictures.Select(r=>r.PictureId).ToList();
            var fileList = await UnitWork.Find<UploadFile>(f => fileIds.Contains(f.Id)).ToListAsync();
            var returnnoteDetails = new ReturnnoteDetailsResp
            {
                ReturnnoteId = returNnoteObj.Id.ToString(),
                DeliveryMethod = returNnoteObj.DeliveryMethod,
                ExpressNumber = returNnoteObj.ExpressNumber,
                FreightCharge = returNnoteObj.FreightCharge,
                Remark = returNnoteObj.Remark,
                TotalMoney = returNnoteObj.TotalMoney,
                UpdateTime = returNnoteObj.UpdateTime.ToString("yyyy.MM.dd HH:mm:ss"),
                ReturnNoteProducts = returNnoteObj.ReturnNoteProducts.Select(r => new ReturnNoteProductResp
                {
                    MaterialCode = r.MaterialCode,
                    MaterialDescription = r.MaterialDescription,
                    ProductCode = r.ProductCode,
                    ReturnNoteId = r.ReturnNoteId,
                    Money = r.Money,
                    ReturnNoteMaterials = r.ReturnNoteMaterials.Select(m => new ReturnNoteMaterialResp
                    {
                        Id = m.Id,
                        Sort = m.Sort,
                        LineNum = m.LineNum,
                        MaterialType = m.MaterialType,
                        MaterialCode = m.MaterialCode,
                        InvoiceDocEntry = m.InvoiceDocEntry,
                        MaterialDescription = m.MaterialDescription,
                        SecondWhsCode = m.SecondWhsCode,
                        ShippingRemark = m.ShippingRemark,
                        SNandPN = m.SNandPN,
                        ReplaceSNandPN = m.ReplaceSNandPN,
                        ReplaceMaterialDescription = m.ReplaceMaterialDescription,
                        GoodWhsCode = m.GoodWhsCode,
                        IsGood = m.IsGood,
                        Money = m.Money,
                        QuotationMaterialId = m.QuotationMaterialId,
                        ReceivingRemark = m.ReceivingRemark,
                        ReplaceMaterialCode = m.ReplaceMaterialCode,
                        ReturnNoteProductId = m.ReturnNoteProductId
                    }).OrderBy(m => m.ReplaceMaterialCode).ToList()
                }).ToList(),
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
            if(!string.IsNullOrWhiteSpace(returNnoteObj.FlowInstanceId)) returnnoteDetails.FlowPathResp = await _flowInstanceApp.FlowPathRespList(returnnoteDetails.ReturnNoteHistoryResp, returNnoteObj.FlowInstanceId);
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
            outsourcDetails.FlowPathResp = await _flowInstanceApp.FlowPathRespList(outsourcDetails.OutsourcOperationHistory, outsourcObj.FlowInstanceId);
            return outsourcDetails;
        }
        /// <summary>
        /// 报销单详情
        /// </summary>
        /// <returns></returns>
        public async Task<ReimburseDetailsResp> ReimburseDetails(int ReimburseId, string customerId)
        {
            var reimburseObj = await UnitWork.Find<ReimburseInfo>(r => r.MainId == ReimburseId).Include(r => r.ReimburseFares)
                .Include(r => r.ReimburseOtherCharges).Include(r => r.ReimburseTravellingAllowances).Include(r => r.ReimurseOperationHistories).Include(r => r.ReimburseAccommodationSubsidies).FirstOrDefaultAsync();
            reimburseObj.ReimburseAttachments = await UnitWork.Find<ReimburseAttachment>(r => r.ReimburseId == reimburseObj.Id && r.ReimburseType == 0).ToListAsync();
            List<string> fileids = new List<string>();
            List<ReimburseAttachment> filemodel = new List<ReimburseAttachment>();
            List<ReimburseExpenseOrg> expenseOrg = new List<ReimburseExpenseOrg>();
            var userOrgInfo = await _userManagerApp.GetUserOrgInfo(reimburseObj.CreateUserId);
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
            var orgMoney = expenseOrg.Where(c => c.OrgId == userOrgInfo.OrgId).Sum(c => c.Money);
            fileids.AddRange(filemodel.Select(f => f.FileId).ToList());
            fileids.AddRange(reimburseObj.ReimburseAttachments.Select(r => r.FileId).ToList());
            var file = await UnitWork.Find<UploadFile>(f => fileids.Contains(f.Id)).ToListAsync();
            var ocrds = await _businessPartnerApp.GetDetails(customerId);
            var userinfo = await _userManagerApp.GetUserOrgInfo("", ocrds?.TechName);
            var reimburseDetails = new ReimburseDetailsResp
            {
                CreateUserId = reimburseObj.CreateUserId,
                ReimburseId = reimburseObj.Id,
                UpdateTime = reimburseObj.UpdateTime,
                Remark = reimburseObj.Remark,
                TotalMoney = reimburseObj.TotalMoney,
                OrgMoney = orgMoney,
                Org = userOrgInfo?.OrgName,
                CusBelong = userinfo?.OrgName + "-" + userinfo?.Name,
                CusBelongId = userinfo?.Id,
                ReimburseMainId = reimburseObj.MainId,
                Files = reimburseObj.ReimburseAttachments.Select(r => new FileResp
                {
                    FileId = r.FileId,
                    FileName = file.Where(s => s.Id.Equals(r.FileId)).FirstOrDefault().FileName,
                    FileType = file.Where(s => s.Id.Equals(r.FileId)).FirstOrDefault().FileType,
                }).ToList(),
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
            reimburseDetails.FlowPathResp = await _flowInstanceApp.FlowPathRespList(reimburseDetails.ReimurseOperationHistories, reimburseObj.FlowInstanceId);
            return reimburseDetails;
        }

        /// <summary>
        /// 内部联络单详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<dynamic> InternalcontactDetails(string id)
        {

            var detail = await UnitWork.Find<InternalContact>(c => c.IW == id)
                            .Include(c => c.InternalContactAttchments)
                            .Include(c => c.InternalContactBatchNumbers)
                            .Include(c => c.InternalContactDeptInfos)
                            .Include(c => c.InternalContactMaterials)
                            .Include(c => c.InternalContactTasks)
                            .Include(c => c.InternalContactServiceOrders)
                            .FirstOrDefaultAsync();
            var internalContactProductions = await UnitWork.Find<InternalContactProduction>(c => c.InternalContactId == detail.Id).ToListAsync();
            //操作历史
            var operationHistories = await UnitWork.Find<FlowInstanceOperationHistory>(c => c.InstanceId == detail.FlowInstanceId)
                .OrderBy(c => c.CreateDate).Select(h => new
                {
                    CreateDate = Convert.ToDateTime(h.CreateDate).ToString("yyyy.MM.dd HH:mm:ss"),
                    h.Remark,
                    IntervalTime = h.IntervalTime != null && h.IntervalTime > 0 ? h.IntervalTime / 60 : null,
                    h.CreateUserName,
                    h.Content,
                    h.ApprovalResult,
                }).ToListAsync();

            var reviceOrgList = detail.InternalContactDeptInfos.Where(c => c.Type == 1).Select(c => new
            {
                c.OrgName,
                Detail = c.HandleTime != null ? "已查收" : "",
                ReciveTime = c.HandleTime
            });
            var execOrgList = detail.InternalContactDeptInfos.Where(c => c.Type == 2).Select(c => new
            {
                c.OrgName,
                Detail = c.Content,
                ExecTime = c.HandleTime
            });

            return new
            {
                detail.Id,
                detail.IW,
                detail.Theme,
                detail.IsTentative,
                CardCodes = !string.IsNullOrWhiteSpace(detail.CardCode) ? detail.CardCode.Split(",") : new string[] { },
                CardNames = !string.IsNullOrWhiteSpace(detail.CardCode) ? detail.CardName.Split(",") : new string[] { },
                detail.Status,
                detail.RdmsNo,
                detail.SaleOrderNo,
                detail.AdaptiveModel,
                detail.ProductionNo,
                AdaptiveRanges = detail.AdaptiveRange.Split(","),
                Reasons = detail.Reason.Split(","),
                MaterialOrder = !string.IsNullOrWhiteSpace(detail.MaterialOrder) ? detail.MaterialOrder.Split(",") : new string[] { },
                //BatchNumbers = detail.InternalContactBatchNumbers,
                BatchNumbers = detail.InternalContactBatchNumbers,
                detail.CheckApproveId,
                detail.CheckApprove,
                detail.DevelopApproveId,
                detail.DevelopApprove,
                InternalContactReceiveDepts = detail.InternalContactDeptInfos.Where(o => o.Type == 1).Select(c => new { c.OrgId, c.OrgName, c.UserId, c.UserName }).ToList(),
                InternalContactExecDepts = detail.InternalContactDeptInfos.Where(o => o.Type == 2).Select(c => new { c.OrgId, c.OrgName, c.UserId, c.UserName }).ToList(),
                detail.Content,
                reviceOrgList,
                execOrgList,
                InternalContactMaterials = detail.InternalContactMaterials,
                operationHistories,
                detail.InternalContactTasks,
                detail.InternalContactServiceOrders,
                InternalContactProductions = internalContactProductions
            };
        }

        /// <summary>
        /// 获取待处理订单详情
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> PendingDetails(PendingReq req)
        {
            var reult = new TableData();
            var pendingObj = await UnitWork.Find<WorkbenchPending>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(req.ApprovalNumber), w => w.ApprovalNumber == int.Parse(req.ApprovalNumber))
                .WhereIf(!string.IsNullOrWhiteSpace(req.SourceNumbers), w => w.SourceNumbers == int.Parse(req.SourceNumbers))
                .FirstOrDefaultAsync();
            if (pendingObj == null && !string.IsNullOrWhiteSpace(req.ApprovalNumber))//来源结算getdetail
            {
                pendingObj = new WorkbenchPending
                {
                    //ApprovalNumber = int.Parse(req.ApprovalNumber),
                    OrderType = 3,
                    ServiceOrderId = req.ServiceOrderId == null ? 0 : Convert.ToInt32(req.ServiceOrderId),
                    PetitionerId = req.Petitioner,
                    SourceNumbers=Convert.ToInt32(req.SourceNumbers)
                };
            }
            ServiceOrderResp serviceOrderDetails = null;
            if (pendingObj.ServiceOrderId!=0) 
                serviceOrderDetails = await ServiceOrderDetails(pendingObj.ServiceOrderId, pendingObj.PetitionerId);
            if (pendingObj.OrderType != 5)//内联单没有抬头
            {
                //为职员加上部门抬头
                var salesManOrgInfo = await _userManagerApp.GetUserOrgInfo(serviceOrderDetails?.SalesManId);
                serviceOrderDetails.SalesManDept = salesManOrgInfo != null ? salesManOrgInfo.OrgName : "";
                var superVisorOrgInfo = await _userManagerApp.GetUserOrgInfo(serviceOrderDetails.SupervisorId);
                serviceOrderDetails.SupervisorDept = superVisorOrgInfo != null ? superVisorOrgInfo.OrgName : "";
            }

            List<QuotationDetailsResp> quotationDetails = new List<QuotationDetailsResp>();
            List<ReturnnoteDetailsResp> returnnoteDetails = new List<ReturnnoteDetailsResp>();
            OutsourcDetailsResp outsourcDetails = null;
            ReimburseDetailsResp reimburseDetails = null;
            dynamic internalcontactDetails = null;
            List<Quotation> quotation = new List<Quotation>();
            switch (pendingObj.OrderType)
            {
                case 1:
                    quotationDetails.Add(await QuotationDetails(pendingObj.SourceNumbers));
                    break;
                case 2:
                    var quotationId = (await (from a in UnitWork.Find<ReturnNote>(r => r.Id == pendingObj.SourceNumbers)
                                              join b in UnitWork.Find<Quotation>(null) on a.SalesOrderId equals b.SalesOrderId
                                              select new { b.Id }).FirstOrDefaultAsync()).Id;
                    quotationDetails.Add(await QuotationDetails(quotationId));
                    returnnoteDetails.Add(await ReturnnoteDetails(pendingObj.SourceNumbers));
                    break;
                case 3:
                    var returnnoteObj = await UnitWork.Find<ReturnNote>(q => q.ServiceOrderId == pendingObj.ServiceOrderId).OrderByDescending(q => q.CreateTime).Select(q => new { q.Id, q.SalesOrderId }).ToListAsync();
                    if (returnnoteObj != null)
                    {
                        //quotation = await UnitWork.Find<Quotation>(q => q.SalesOrderId.Equals(returnnoteObj.SalesOrderId)).OrderByDescending(q => q.CreateTime).ToListAsync();
                        foreach (var item in returnnoteObj)
                        {
                            returnnoteDetails.Add(await ReturnnoteDetails(item.Id));
                        }
                    }

                    var quotaId = (await UnitWork.Find<Outsourc>(null).FirstOrDefaultAsync(x => x.Id == int.Parse(req.SourceNumbers))).QuotationId;
                    quotation = await UnitWork.Find<Quotation>(q => q.Id == quotaId && q.ServiceOrderId.Equals(pendingObj.ServiceOrderId) && q.QuotationStatus >= 3.1M).OrderByDescending(q => q.CreateTime).ToListAsync();
                    if (quotation != null && quotation.Count()>0)
                    {
                        foreach (var item in quotation)
                        {
                            quotationDetails.Add(await QuotationDetails(item.Id));
                        }
                    }
                    outsourcDetails=await OutsourcDetails(pendingObj.SourceNumbers);
                    break;
                case 4:
                    returnnoteObj = await UnitWork.Find<ReturnNote>(q => q.ServiceOrderId == pendingObj.ServiceOrderId).OrderByDescending(q => q.CreateTime).Select(q => new { q.Id, q.SalesOrderId }).ToListAsync();
                    if (returnnoteObj != null)
                    {
                        //quotation = await UnitWork.Find<Quotation>(q => q.SalesOrderId.Equals(returnnoteObj.SalesOrderId)).OrderByDescending(q => q.CreateTime).ToListAsync();
                        foreach (var item in returnnoteObj)
                        {
                            returnnoteDetails.Add(await ReturnnoteDetails(item.Id));
                        }
                    }
                    quotation = await UnitWork.Find<Quotation>(q => q.ServiceOrderId.Equals(pendingObj.ServiceOrderId)&&q.QuotationStatus >= 3.1M).OrderByDescending(q => q.CreateTime).ToListAsync();
                    if (quotation != null && quotation.Count() > 0)
                    {
                        foreach (var item in quotation)
                        {
                            quotationDetails.Add(await QuotationDetails(item.Id));
                        }
                    }
                    reimburseDetails = await ReimburseDetails(pendingObj.SourceNumbers, serviceOrderDetails?.TerminalCustomerId);
                    break;
                case 5:
                    internalcontactDetails = await InternalcontactDetails(pendingObj.SourceNumbers.ToString());
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
                reimburseDetails,
                internalcontactDetails
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
            var SelOrgName = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).Select(o => new { o.Id, o.Name, o.CascadeId }).ToListAsync();
            var Relevances = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG).Select(r => new { r.FirstId, r.SecondId }).ToListAsync();

            if (req.PageType == 1)
            {
                //待处理
                var query = from a in UnitWork.Find<WorkbenchPending>(c => c.OrderType != 5)
                            join b in UnitWork.Find<FlowInstance>(null) on a.FlowInstanceId equals b.Id into ab
                            from b in ab.DefaultIfEmpty()
                            where (b.MakerList.Contains(loginContext.User.Id) || (b.MakerList == "1" && b.CustomName.Contains("物料报价单"))) && b.ActivityName != "待出库" && b.ActivityName != "开始" && b.ActivityName != "执行中" && b.ActivityName != "提交"
                            select new { a, b };
                query = query.WhereIf(!string.IsNullOrWhiteSpace(req.ApprovalNumber), q => q.a.ApprovalNumber == int.Parse(req.ApprovalNumber))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.Petitioner), q => q.a.Petitioner.Contains(req.Petitioner))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.OrderType), q => q.a.OrderType == int.Parse(req.OrderType))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.TerminalCustomer), q => q.a.TerminalCustomer.Contains(req.TerminalCustomer))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.TerminalCustomerId), q => q.a.TerminalCustomerId.Contains(req.TerminalCustomerId))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.SourceNumbers), q => q.a.SourceNumbers == int.Parse(req.SourceNumbers))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.StartTime.ToString()), q => q.a.UpdateTime > req.StartTime)
                            .WhereIf(!string.IsNullOrWhiteSpace(req.EndTime.ToString()), q => q.a.UpdateTime > Convert.ToDateTime(req.EndTime).AddDays(1));
                if (loginContext.Roles.Any(r => r.Name == "销售员"))
                {
                    var serviceOrderIds = await UnitWork.Find<ServiceOrder>(s=>s.SalesManId.Equals(loginContext.User.Id)).Select(s=>s.Id).ToListAsync();
                    query = query.Where(q => (serviceOrderIds.Contains(q.a.ServiceOrderId) && q.b.ActivityName == "销售员审批" && q.a.OrderType == 1) || q.b.ActivityName != "销售员审批" && q.a.OrderType != 1);
                }
                query=query.Where(q => ((q.b.ActivityName == "确认报价单"|| q.b.ActivityName == "回传销售订单")&& q.a.OrderType==1 && q.a.PetitionerId.Equals(loginContext.User.Id)) || (q.b.ActivityName != "确认报价单"&& q.b.ActivityName != "回传销售订单"));
                var pending = await query.OrderByDescending(o => o.a.UpdateTime).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
                reult.Data = pending.Select(p => new
                {
                    p.a.TotalMoney,
                    p.a.ApprovalNumber,
                    p.a.TerminalCustomer,
                    p.a.TerminalCustomerId,
                    p.a.SourceNumbers,
                    p.a.ServiceOrderSapId,
                    p.a.Remark,
                    Petitioner= SelOrgName.Where(s => s.Id.Equals(Relevances.Where(r => r.FirstId.Equals(p.a.PetitionerId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name==null? p.a.Petitioner: SelOrgName.Where(s => s.Id.Equals(Relevances.Where(r => r.FirstId.Equals(p.a.PetitionerId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name + "-" + p.a.Petitioner,
                    p.a.PetitionerId,
                    p.a.OrderType,
                    p.b.ActivityName,
                    p.a.UpdateTime
                }).ToList();
                reult.Count = await query.CountAsync();
            }
            else if (req.PageType == 2)
            {
                //已处理
                var query = from a in UnitWork.Find<WorkbenchPending>(c => c.OrderType != 5)
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
                            .WhereIf(!string.IsNullOrWhiteSpace(req.SourceNumbers), q => q.a.SourceNumbers == int.Parse(req.SourceNumbers))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.EndTime.ToString()), q => q.a.UpdateTime > Convert.ToDateTime(req.EndTime).AddDays(1));
                var pending = await query.OrderByDescending(q => q.a.UpdateTime).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
                reult.Data = pending.Select(q => new
                {
                    Petitioner = SelOrgName.Where(s => s.Id.Equals(Relevances.Where(r => r.FirstId.Equals(q.a.PetitionerId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name==null? q.a.Petitioner : SelOrgName.Where(s => s.Id.Equals(Relevances.Where(r => r.FirstId.Equals(q.a.PetitionerId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name + "-" + q.a.Petitioner,
                    q.a.TotalMoney,
                    q.a.ApprovalNumber,
                    q.a.TerminalCustomer,
                    q.a.TerminalCustomerId,
                    q.a.SourceNumbers,
                    q.a.ServiceOrderSapId,
                    q.a.Remark,
                    q.a.OrderType,
                    q.a.PetitionerId,
                    ActivityName = q.b.ActivityName == "开始" || q.b.ActivityName == "提交" ? "驳回" : q.b.ActivityName,
                    q.a.UpdateTime
                }).ToList();
                reult.Count = await query.CountAsync();
            }
            return reult;
        }

        /// <summary>
        /// 解析完工报告json
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<string> GetServiceTroubleAndSolution(string data, string objectCode)
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(data))
            {
                JArray jArray = (JArray)JsonConvert.DeserializeObject(data);
                foreach (var item in jArray)
                {
                    result.Add(item[objectCode] == null ? "" : item[objectCode].ToString());
                }
            }
            return result;
        }



    }
}
