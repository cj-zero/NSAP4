using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.App.Workbench.Request;
using OpenAuth.App.Workbench.Response;
using OpenAuth.Repository.Domain;
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
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public PendingApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
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
                                    select new { a.Name, OrgName = c.Name, c.CascadeId }).OrderByDescending(u => u.CascadeId).FirstOrDefaultAsync();
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
                    CreateTime = w.CreateTime.ToString("yyyy.MM.dd HH:mm:ss"),
                    MaterialCode = w.MaterialCode,
                    WorkOrderNumber = w.WorkOrderNumber,
                    FromTheme = w.FromTheme,
                    MaterialDescription = w.MaterialDescription,
                    Remark = w.Remark
                }).ToList()
            }).FirstOrDefaultAsync();
            serviceOrder.Petitioner = petitioner.OrgName + "-" + petitioner.Name;
            serviceOrder.ServiceDailyReports = serviceDailyReportList.Select(s => new ServiceDailyReportResp
            {
                CreateTime = s.CreateTime.ToString("yyyy.MM.dd HH:mm:ss"),
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
            var quotationObj = await UnitWork.Find<Quotation>(q => q.Id == QuotationId).Include(q => q.QuotationProducts).ThenInclude(q => q.QuotationMaterials).ThenInclude(q => q.QuotationMaterialPictures).Include(q => q.QuotationOperationHistorys)
                .FirstOrDefaultAsync();
            if (quotationObj == null) 
            {
                return null;
            }
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
                UpDateTime = quotationObj.UpDateTime.ToString("yyyy.MM.dd HH:mm:ss"),
                PayOnReceipt = quotationObj.PayOnReceipt,
                TotalCostPrice = quotationObj.TotalCostPrice,
                TotalMoney = quotationObj.TotalMoney,
                QuotationOperationHistorys = quotationObj.QuotationOperationHistorys.Select(o => new OperationHistoryResp
                {
                    ApprovalResult = o.ApprovalResult,
                    Content = o.Action,
                    CreateTime = o.CreateTime.ToString("yyyy.MM.dd HH:mm:ss"),
                    CreateUserName = o.CreateUser,
                    IntervalTime = o.IntervalTime.ToString(),
                    Remark = o.Remark
                }).OrderBy(o=>o.CreateTime).ToList(),
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
                        Count = m.Count,
                        WhsCode = m.WhsCode,
                        MaterialType = m.MaterialType,
                        MaxQuantity = m.MaxQuantity,
                        NewMaterialCode = m.NewMaterialCode,
                        Remark = m.Remark,
                        ReplaceMaterialCode = m.ReplaceMaterialCode,
                        Unit = m.Unit,
                        TotalPrice = m.TotalPrice,
                        UnitPrice = m.UnitPrice,
                        Files = m.QuotationMaterialPictures.Select(p => new FileResp
                        {
                            FileId = p.PictureId,
                            FileName = p.FileName,
                            FileType = p.FileType
                        }).ToList()
                    }).OrderBy(m => m.MaterialCode).ToList()
                }).OrderBy(p=>p.MaterialCode).ToList()
            };
            //var schemeContent=await UnitWork.Find<FlowInstance>(f => f.Id.Equals(quotationObj.FlowInstanceId)).Select(f=>f.SchemeContent).FirstOrDefaultAsync();
            //var schemeContentJson = JsonHelper.Instance.Deserialize<FlowInstanceJson>(schemeContent);
            //List<FlowInstanceNodes> flowInstanceNodes = new List<FlowInstanceNodes>();
            //var stratid = schemeContentJson.nodes.Where(s => s.id.Contains("start")).FirstOrDefault().id;
            //schemeContentJson.nodes.ForEach(s =>
            //{
                
            //    flowInstanceNodes.Add(new FlowInstanceNodes { Name= });

            //});

            //quotationDetails.FlowPathResp= schemeContentJson.nodes.Select(s=>new FlowPathResp { 
            //    ActivityName=s.Name,
            //}).ToList();
            return quotationDetails;
        }
        /// <summary>
        /// 退料单详情
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnnoteDetailsResp> ReturnnoteDetails(int ReturnnoteId)
        {
            //var returnnoteDetails=await UnitWork.Find<ReturnNote>(r => r.Id == int.Parse(ReturnnoteId)).Include(r=>r.ReturnnoteMaterials).ThenInclude(r=>r.ReturnNoteMaterialPictures)
            //    .Select(r=>new ReturnnoteDetailsResp { 

            //    })
            //    .FirstOrDefaultAsync();
            return null;
        }
        /// <summary>
        /// 结算单详情
        /// </summary>
        /// <returns></returns>
        public async Task<OutsourcDetailsResp> OutsourcDetails(int OutsourcId)
        {
            var outsourcObj = await UnitWork.Find<outsourc>(o => o.Id == OutsourcId).Include(o => o.outsourcexpenses).ThenInclude(o => o.outsourcexpensespictures)
                .FirstOrDefaultAsync();
            var outsourcDetails = new OutsourcDetailsResp
            {
                OutsourcId = outsourcObj.Id.ToString(),
                ServiceMode = outsourcObj.ServiceMode,
                Remark = outsourcObj.Remark,
                TotalMoney = outsourcObj.TotalMoney,
                UpdateTime = outsourcObj.UpdateTime.ToString("yyyy.MM.dd HH:mm:ss"),
                OutsourcExpenses = outsourcObj.outsourcexpenses.Select(e => new OutsourcExpensesResp
                {
                    Id=e.Id,
                    FromLat = e.FromLat,
                    Days = e.Days,
                    From = e.From,
                    FromLng = e.FromLng,
                    ManHour = e.ManHour,
                    ExpensesType = e.ExpensesType,
                    Money = e.Money,
                    To = e.To,
                    ToLat = e.ToLat,
                    ToLng = e.ToLng,
                    Files = e.outsourcexpensespictures.Select(p => new FileResp
                    {
                        FileId = p.PictureId,
                        FileName = p.FileName,
                        FileType = p.FileType
                    }).ToList()
                }).OrderBy(r=>r.ExpensesType).ToList()
            };
            outsourcDetails.OutsourcOperationHistory = await UnitWork.Find<FlowInstanceOperationHistory>(f => f.InstanceId.Equals(outsourcObj.FlowInstanceId)).OrderBy(f => f.CreateDate)
                .Select(f => new OperationHistoryResp
                {
                    ApprovalResult = f.ApprovalResult,
                    Content = f.Content,
                    CreateTime = f.CreateDate.ToString("yyyy.MM.dd HH:mm:ss"),
                    CreateUserName = f.CreateUserName,
                    IntervalTime = f.IntervalTime.ToString(),
                    Remark = f.Remark
                }
                ).OrderBy(f=>f.CreateTime).ToListAsync();

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
            if (reimburseObj.ReimburseFares != null && reimburseObj.ReimburseFares.Count > 0)
            {
                var rfids = reimburseObj.ReimburseFares.Select(r => r.Id).ToList();
                filemodel = await UnitWork.Find<ReimburseAttachment>(r => rfids.Contains(r.ReimburseId) && r.ReimburseType == 2).ToListAsync();
            }
            if (reimburseObj.ReimburseAccommodationSubsidies != null && reimburseObj.ReimburseAccommodationSubsidies.Count > 0)
            {
                var rasids = reimburseObj.ReimburseAccommodationSubsidies.Select(r => r.Id).ToList();
                filemodel.AddRange(await UnitWork.Find<ReimburseAttachment>(r => rasids.Contains(r.ReimburseId) && r.ReimburseType == 3).ToListAsync());
            }
            if (reimburseObj.ReimburseOtherCharges != null && reimburseObj.ReimburseOtherCharges.Count > 0)
            {
                var rocids = reimburseObj.ReimburseOtherCharges.Select(r => r.Id).ToList();
                filemodel.AddRange(await UnitWork.Find<ReimburseAttachment>(r => rocids.Contains(r.ReimburseId) && r.ReimburseType == 4).ToListAsync());
            }
            fileids.AddRange(filemodel.Select(f => f.FileId).ToList());
            var file = await UnitWork.Find<UploadFile>(f => fileids.Contains(f.Id)).ToListAsync();
            var reimburseDetails = new ReimburseDetailsResp
            {
                ReimburseId = reimburseObj.Id,
                UpdateTime = reimburseObj.UpdateTime,
                Remark = reimburseObj.Remark,
                TotalMoney = reimburseObj.TotalMoney,
                ReimburseMainId= reimburseObj.MainId,
                ReimburseFares = reimburseObj.ReimburseFares.Select(f => new ReimburseFareResp
                {
                    Id=f.Id,
                    SerialNumber = f.SerialNumber,
                    CreateTime = f.CreateTime.ToString("yyyy.MM.dd HH:mm:ss"),
                    ExpenseOrg = f.ExpenseOrg,
                    From = f.From,
                    FromLat = f.FromLat,
                    FromLng = f.FromLng,
                    InvoiceNumber = f.InvoiceNumber,
                    InvoiceTime = f.InvoiceTime.ToString("yyyy.MM.dd HH:mm:ss"),
                    Money = f.Money,
                    Remark = f.Remark,
                    To = f.To,
                    ToLat = f.ToLat,
                    ToLng = f.ToLng,
                    TrafficType = f.TrafficType,
                    Transport = f.Transport,
                    Files = filemodel.Where(m => m.ReimburseId == f.Id && m.ReimburseType == 2).Select(m => new FileResp
                    {
                        FileId = m.FileId,
                        FileName = file.Where(s => s.Id.Equals(m.Id)).FirstOrDefault().FileName,
                        FileType = file.Where(s => s.Id.Equals(m.Id)).FirstOrDefault().FileType,
                    }).ToList()
                }).OrderBy(r=>r.InvoiceTime).ToList(),
                ReimburseAccommodationSubsidies = reimburseObj.ReimburseAccommodationSubsidies.Select(a => new ReimburseAccommodationSubsidyResp
                {
                    Id = a.Id,
                    SellerName = a.SellerName,
                    CreateTime = a.CreateTime.ToString("yyyy.MM.dd HH:mm:ss"),
                    Days = a.Days,
                    ExpenseOrg = a.ExpenseOrg,
                    InvoiceNumber = a.InvoiceNumber,
                    InvoiceTime = a.InvoiceTime.ToString("yyyy.MM.dd HH:mm:ss"),
                    Money = a.Money,
                    TotalMoney = a.TotalMoney,
                    Remark = a.Remark,
                    SerialNumber = a.SerialNumber,
                    Files = filemodel.Where(m => m.ReimburseId == a.Id && m.ReimburseType == 3).Select(m => new FileResp
                    {
                        FileId = m.FileId,
                        FileName = file.Where(s => s.Id.Equals(m.Id)).FirstOrDefault().FileName,
                        FileType = file.Where(s => s.Id.Equals(m.Id)).FirstOrDefault().FileType,
                    }).ToList()
                }).OrderBy(r => r.InvoiceTime).ToList(),
                ReimburseOtherCharges = reimburseObj.ReimburseOtherCharges.Select(o => new ReimburseOtherChargesResp
                {
                    Id = o.Id,
                    CreateTime = o.CreateTime.ToString("yyyy.MM.dd HH:mm:ss"),
                    ExpenseCategory = o.ExpenseCategory,
                    ExpenseOrg = o.ExpenseOrg,
                    SerialNumber = o.SerialNumber,
                    InvoiceNumber = o.InvoiceNumber,
                    InvoiceTime = o.InvoiceTime.ToString("yyyy.MM.dd HH:mm:ss"),
                    Money = o.Money,
                    Remark = o.Remark,
                    Files = filemodel.Where(m => m.ReimburseId == o.Id && m.ReimburseType == 4).Select(m => new FileResp
                    {
                        FileId = m.FileId,
                        FileName = file.Where(s => s.Id.Equals(m.Id)).FirstOrDefault().FileName,
                        FileType = file.Where(s => s.Id.Equals(m.Id)).FirstOrDefault().FileType,
                    }).ToList()
                }).OrderBy(r => r.InvoiceTime).ToList(),
                ReimburseTravellingAllowances = reimburseObj.ReimburseTravellingAllowances.Select(t => new ReimburseTravellingAllowanceResp
                {
                    Id = t.Id,
                    SerialNumber = t.SerialNumber,
                    CreateTime = t.CreateTime.ToString("yyyy.MM.dd HH:mm:ss"),
                    Days = t.Days,
                    ExpenseOrg = t.ExpenseOrg,
                    Money = t.Money,
                    Remark = t.Remark
                }).OrderBy(r => r.CreateTime).ToList(),
                ReimurseOperationHistories = reimburseObj.ReimurseOperationHistories.Select(o => new OperationHistoryResp
                {
                    Content = o.Action,
                    ApprovalResult = o.ApprovalResult,
                    CreateTime = o.CreateTime.ToString("yyyy.MM.dd HH:mm:ss"),
                    CreateUserName = o.CreateUser,
                    Remark = o.Remark,
                    IntervalTime = o.IntervalTime.ToString()
                }).OrderBy(r => r.CreateTime).ToList()
            };
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
                    var quotation = await UnitWork.Find<Quotation>(q => q.ServiceOrderId.Equals(pendingObj.ServiceOrderId) && q.QuotationStatus == 11).OrderByDescending(q => q.CreateTime).FirstOrDefaultAsync();
                    if (quotation != null)
                    {
                        var returnnoteId = await UnitWork.Find<ReturnNote>(q => q.SalesOrderId.Equals(quotation.SalesOrderId)).OrderByDescending(q => q.CreateTime).Select(q => q.Id).FirstOrDefaultAsync();
                        quotationDetails = await QuotationDetails(quotation.Id);
                        returnnoteDetails = await ReturnnoteDetails(returnnoteId);
                    }
                    outsourcDetails = await OutsourcDetails(pendingObj.SourceNumbers);
                    break;
                case 4:
                    quotation = await UnitWork.Find<Quotation>(q => q.ServiceOrderId.Equals(pendingObj.ServiceOrderId) && q.QuotationStatus == 11).OrderByDescending(q => q.CreateTime).FirstOrDefaultAsync();
                    if (quotation != null)
                    {
                        var returnnoteId = await UnitWork.Find<ReturnNote>(q => q.SalesOrderId.Equals(quotation.SalesOrderId)).OrderByDescending(q => q.CreateTime).Select(q => q.Id).FirstOrDefaultAsync();
                        quotationDetails = await QuotationDetails(quotation.Id);
                        returnnoteDetails = await ReturnnoteDetails(returnnoteId);
                    }
                    reimburseDetails = await ReimburseDetails(pendingObj.SourceNumbers);
                    break;
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
        public async Task AddOrUpdate(WorkbenchPending obj)
        {
            //增加全局待处理
            var workbenchPendingObj = await UnitWork.Find<WorkbenchPending>(w => w.SourceNumbers == obj.SourceNumbers && w.OrderType == obj.OrderType).FirstOrDefaultAsync();
            if (workbenchPendingObj != null)
            {
                await UnitWork.UpdateAsync<WorkbenchPending>(w => w.ApprovalNumber == workbenchPendingObj.ApprovalNumber, w => new WorkbenchPending
                {
                    UpdateTime = obj.UpdateTime,
                    Remark = obj.Remark,
                });
            }
            else
            {
                await UnitWork.AddAsync<WorkbenchPending>(new WorkbenchPending
                {
                    OrderType = obj.OrderType,
                    TerminalCustomer = obj.TerminalCustomer,
                    TerminalCustomerId = obj.TerminalCustomerId,
                    ServiceOrderId = obj.ServiceOrderId,
                    ServiceOrderSapId = obj.ServiceOrderSapId,
                    UpdateTime = obj.UpdateTime,
                    Remark = obj.Remark,
                    FlowInstanceId = obj.FlowInstanceId,
                    TotalMoney = obj.TotalMoney,
                    Petitioner = obj.Petitioner,
                    SourceNumbers = obj.SourceNumbers,
                    PetitionerId = obj.PetitionerId

                });
            }
            await UnitWork.SaveAsync();
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
            var query = from a in UnitWork.Find<WorkbenchPending>(null)
                        join b in UnitWork.Find<FlowInstance>(null) on a.FlowInstanceId equals b.Id
                        where b.MakerList.Contains(loginContext.User.Id)
                        select new { a, b };
            query = query.WhereIf(!string.IsNullOrWhiteSpace(req.ApprovalNumber), q => q.a.ApprovalNumber == int.Parse(req.ApprovalNumber))
                        .WhereIf(!string.IsNullOrWhiteSpace(req.Petitioner), q => q.a.Petitioner.Contains(req.Petitioner))
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
                q.a.OrderType,
                q.b.ActivityName,
                q.a.UpdateTime
            }).ToListAsync();
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
                }
            }
            reult.Data = pending.Where(p => !salesManIds.Contains(p.ApprovalNumber)).Skip((req.page - 1) * req.limit).Take(req.limit).ToList();
            reult.Count = await query.CountAsync();
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

    }
}
