using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Extensions;
using DotNetCore.CAP;
using Infrastructure.Const;
using OpenAuth.App.Material.Response;
using System.Linq.Dynamic.Core;
using OpenAuth.App.Material;
using OpenAuth.Repository.Domain.Workbench;
using OpenAuth.App.Workbench;

namespace OpenAuth.App
{
    public class ReturnNoteApp : OnlyUnitWorkBaeApp
    {
        private readonly FlowInstanceApp _flowInstanceApp;
        //private readonly ExpressageApp _expressageApp;
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        private ICapPublisher _capBus;
        //private readonly QuotationApp _quotation;
        private readonly PendingApp _pending;
        private readonly WorkbenchApp _workbenchApp;

        public ReturnNoteApp(IUnitWork unitWork, FlowInstanceApp flowInstanceApp, WorkbenchApp workbenchApp, PendingApp pending, ModuleFlowSchemeApp moduleFlowSchemeApp, IAuth auth, ICapPublisher capBus) : base(unitWork, auth)
        {
            _flowInstanceApp = flowInstanceApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            //_expressageApp = expressageApp;
            _capBus = capBus;
            //_quotation = quotation;
            _workbenchApp = workbenchApp;
            _pending = pending;
        }

        #region app和erp通用
        /// <summary>
        /// 查询退料信息列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> Load(ReturnMaterialReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            var loginUser = loginContext.User;
            List<string> Lines = new List<string>();
            List<string> flowInstanceIds = new List<string>();
            var lineId = "";
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            if (loginUser.Account == Define.USERAPP && req.AppUserId > 0)
            {
                loginUser = await GetUserId((int)req.AppUserId);
            }
            var returnNotes = UnitWork.Find<ReturnNote>(null).Include(r => r.ReturnNotePictures).Include(r=>r.ReturnNoteProducts).ThenInclude(r => r.ReturnNoteMaterials)
                                .WhereIf(!string.IsNullOrWhiteSpace(req.SapId.ToString()), r => r.ServiceOrderSapId == req.SapId)
                                .WhereIf(!string.IsNullOrWhiteSpace(req.SalesOrderId.ToString()), r => r.SalesOrderId == req.SalesOrderId)
                                .WhereIf(!string.IsNullOrWhiteSpace(req.StartDate.ToString()), r => r.CreateTime > req.StartDate)
                                .WhereIf(!string.IsNullOrWhiteSpace(req.EndDate.ToString()), r => r.CreateTime < Convert.ToDateTime(req.EndDate).AddDays(1));
            #region 筛选条件
            //var schemeContent = await .FirstOrDefaultAsync();
            var SchemeContent = await UnitWork.Find<FlowScheme>(f => f.SchemeName.Equals("退料单审批")).Select(f => f.SchemeContent).FirstOrDefaultAsync();
            SchemeContentJson schemeJson = JsonHelper.Instance.Deserialize<SchemeContentJson>(SchemeContent);
            switch (req.PageType)
            {
                case 1:
                    if (loginContext.Roles.Any(r => r.Name.Equals("储运人员")))
                    {
                        lineId = schemeJson.Nodes.Where(n => n.name.Equals("储运收货")).FirstOrDefault()?.id;
                    }
                    break;
                case 2:
                    if (loginContext.Roles.Any(r => r.Name.Equals("品质")))
                    {
                        lineId = schemeJson.Nodes.Where(n => n.name.Equals("品质检验")).FirstOrDefault()?.id;
                    }
                    break;
                case 3:
                    if (loginContext.Roles.Any(r => r.Name.Equals("总经理")))
                    {
                        lineId = schemeJson.Nodes.Where(n => n.name.Equals("总经理审批")).FirstOrDefault()?.id;
                    }
                    break;
                case 4:
                    if (loginContext.Roles.Any(r => r.Name.Equals("仓库")))
                    {
                        lineId = schemeJson.Nodes.Where(n => n.name.Equals("仓库入库")).FirstOrDefault()?.id;
                    }
                    break;
                default:
                    returnNotes = returnNotes.Where(r => r.CreateUserId.Equals(loginUser.Id));
                    break;
            }
            if (!string.IsNullOrWhiteSpace(lineId) && req.PageType != null && req.PageType > 0)
            {
                if (req.PageStatus == 1)
                {
                    Lines.Add(lineId);
                }
                else //if (req.PageStatus == 2)
                {
                    List<string> lineIds = new List<string>();
                    var lineIdTo = lineId;
                    foreach (var item in schemeJson.Lines)
                    {
                        if (schemeJson.Lines.Where(l => l.from.Equals(lineIdTo)).FirstOrDefault()?.to != null)
                        {
                            lineIdTo = schemeJson.Lines.Where(l => l.from.Equals(lineIdTo)).FirstOrDefault()?.to;
                            lineIds.Add(lineIdTo);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (req.PageStatus == 2)
                    {
                        Lines.AddRange(lineIds);
                    }
                    else
                    {
                        Lines.Add(lineId);
                        Lines.AddRange(lineIds);
                    }
                }
                if (Lines.Count > 0)
                {
                    flowInstanceIds = await UnitWork.Find<FlowInstance>(f => Lines.Contains(f.ActivityId)).Select(s => s.Id).ToListAsync();
                    returnNotes = returnNotes.Where(r => flowInstanceIds.Contains(r.FlowInstanceId));
                }
            }
            if (!string.IsNullOrWhiteSpace(req.Status))
            {
                if (req.Status == "驳回")
                {
                    flowInstanceIds.AddRange(await UnitWork.Find<FlowInstance>(f => f.IsFinish == FlowInstanceStatus.Rejected).Select(s => s.Id).ToListAsync());
                }
                else
                {
                    flowInstanceIds.AddRange(await UnitWork.Find<FlowInstance>(f => f.ActivityName.Equals(req.Status)).Select(s => s.Id).ToListAsync());
                }
                if (req.Status == "开始")
                {
                    returnNotes = returnNotes.Where(r => flowInstanceIds.Contains(r.FlowInstanceId) || string.IsNullOrEmpty(r.FlowInstanceId));
                }
                else
                {
                    returnNotes = returnNotes.Where(r => flowInstanceIds.Contains(r.FlowInstanceId));
                }

            }
            #endregion

            var result = new TableData();
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ReturnNoteTypeName")).Select(u => new { u.Name, u.DtValue }).ToListAsync();
            result.Count = await returnNotes.CountAsync();
            var returnNoteList = await returnNotes.OrderByDescending(r => r.UpdateTime).ToListAsync();
            flowInstanceIds = returnNoteList.Select(r => r.FlowInstanceId).ToList();
            var flowInstanceList = await UnitWork.Find<FlowInstance>(f => flowInstanceIds.Contains(f.Id)).ToListAsync();
            List<ReturnNoteMainResp> returnNoteMainRespList = new List<ReturnNoteMainResp>();
            if (!string.IsNullOrWhiteSpace(req.Customer))
            {
                var serviceOrders = await UnitWork.Find<ServiceOrder>(s => s.TerminalCustomer.Contains(req.Customer) && s.TerminalCustomerId.Contains(req.Customer)).Select(s => new { s.Id, s.TerminalCustomer, s.TerminalCustomerId, s.NewestContacter, s.NewestContactTel }).ToListAsync();
                var serviceOrderIds = serviceOrders.Select(s => s.Id).ToList();
                result.Count = returnNoteList.Where(r => serviceOrderIds.Contains(r.ServiceOrderId)).Count();
                returnNoteList = returnNoteList.Where(r => serviceOrderIds.Contains(r.ServiceOrderId)).Skip((req.page - 1) * req.limit).Take(req.limit).ToList();
                returnNoteMainRespList = returnNoteList.Select(r => new ReturnNoteMainResp
                {
                    returnNoteId = r.Id,
                    ServiceOrderId = r.ServiceOrderId,
                    SalesOrderId = r.SalesOrderId,
                    ServiceOrderSapId = r.ServiceOrderSapId,
                    CreateUser = r.CreateUser,
                    CreateTime = Convert.ToDateTime(r.CreateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                    UpdateTime = Convert.ToDateTime(r.UpdateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                    TotalMoney = r.TotalMoney,
                    Status = flowInstanceList.Where(f => f.Id.Equals(r.FlowInstanceId)).FirstOrDefault()?.IsFinish != FlowInstanceStatus.Rejected ? flowInstanceList.Where(f => f.Id.Equals(r.FlowInstanceId)).FirstOrDefault()?.ActivityName : "驳回",
                    IsUpDate = flowInstanceList.Where(f => f.Id.Equals(r.FlowInstanceId)).FirstOrDefault()?.IsFinish != FlowInstanceStatus.Running ? flowInstanceList.Where(f => f.Id.Equals(r.FlowInstanceId)).FirstOrDefault()?.IsFinish != FlowInstanceStatus.Running ? true : false : false,
                    IsLiquidated = r.IsLiquidated,
                    Remark = r.Remark,
                    InvoiceDocEntry = r.ReturnNoteProducts.FirstOrDefault().ReturnNoteMaterials.FirstOrDefault()?.InvoiceDocEntry,
                    TerminalCustomer = serviceOrders.Where(s => s.Id == r.ServiceOrderId).FirstOrDefault()?.TerminalCustomer,
                    TerminalCustomerId = serviceOrders.Where(s => s.Id == r.ServiceOrderId).FirstOrDefault()?.TerminalCustomerId,
                }).ToList();
            }
            else
            {
                returnNoteList = returnNoteList.Skip((req.page - 1) * req.limit).Take(req.limit).ToList();
                var serviceOrderIds = returnNoteList.Select(r => r.ServiceOrderId).ToList();
                var serviceOrders = await UnitWork.Find<ServiceOrder>(s => serviceOrderIds.Contains(s.Id)).Select(s => new { s.Id, s.TerminalCustomer, s.TerminalCustomerId }).ToListAsync();
                returnNoteMainRespList = returnNoteList.Select(r => new ReturnNoteMainResp
                {
                    returnNoteId = r.Id,
                    ServiceOrderId = r.ServiceOrderId,
                    SalesOrderId = r.SalesOrderId,
                    ServiceOrderSapId = r.ServiceOrderSapId,
                    CreateUser = r.CreateUser,
                    CreateTime = Convert.ToDateTime(r.CreateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                    UpdateTime = Convert.ToDateTime(r.UpdateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                    TotalMoney = r.TotalMoney,
                    Status = flowInstanceList.Where(f => f.Id.Equals(r.FlowInstanceId)).FirstOrDefault()?.IsFinish != FlowInstanceStatus.Rejected ? flowInstanceList.Where(f => f.Id.Equals(r.FlowInstanceId)).FirstOrDefault()?.ActivityName : "驳回",
                    IsUpDate = flowInstanceList.Where(f => f.Id.Equals(r.FlowInstanceId)).FirstOrDefault()?.IsFinish != FlowInstanceStatus.Running ? flowInstanceList.Where(f => f.Id.Equals(r.FlowInstanceId)).FirstOrDefault()?.IsFinish != FlowInstanceStatus.Running ? true : false : false,
                    IsLiquidated = r.IsLiquidated,
                    Remark = r.Remark,
                    InvoiceDocEntry = r.ReturnNoteProducts.FirstOrDefault().ReturnNoteMaterials.FirstOrDefault()?.InvoiceDocEntry,
                    TerminalCustomer = serviceOrders.Where(s => s.Id == r.ServiceOrderId).FirstOrDefault()?.TerminalCustomer,
                    TerminalCustomerId = serviceOrders.Where(s => s.Id == r.ServiceOrderId).FirstOrDefault()?.TerminalCustomerId,
                }).ToList();

            }
            returnNoteMainRespList.ForEach(r => { r.StatusName = r.Status != null ? CategoryList.Where(c => c.DtValue.Equals(r.Status)).FirstOrDefault()?.Name : "未提交"; r.Status = r.Status != null ? r.Status : "开始"; });
            result.Data = returnNoteMainRespList;
            return result;
        }

        /// <summary>
        /// 获取可退料的应收发票
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetOinvList(ReturnMaterialReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && req.AppUserId > 0)
            {
                loginUser = await GetUserId((int)req.AppUserId);
            }
            var result = new TableData();
            //查询当前技术员所有可退料服务Id
            var query = from a in UnitWork.Find<QuotationMergeMaterial>(null)
                        join b in UnitWork.Find<Quotation>(null) on a.QuotationId equals b.Id
                        where b.CreateUserId == loginUser.Id && b.QuotationStatus == 11 && b.SalesOrderId != null
                        select new { b };
            query = query.WhereIf(!string.IsNullOrWhiteSpace(req.SalesOrderId.ToString()), q => q.b.SalesOrderId == req.SalesOrderId)
                .WhereIf(!string.IsNullOrWhiteSpace(req.ServiceOrderId.ToString()), q => q.b.ServiceOrderId == req.ServiceOrderId);
            var queryList = await query.Select(q => new { q.b.ServiceOrderId, q.b.SalesOrderId, q.b.CreateUser }).Distinct().ToListAsync();
            //获取服务单id集合
            var serviceOrderIds = queryList.Select(s => s.ServiceOrderId).ToList();
            //获取销售订单id集合
            var salesOrderIds = queryList.Select(s => s.SalesOrderId).ToList();
            //查询交货记录
            var saledln1 = await UnitWork.Find<sale_dln1>(s => salesOrderIds.Contains(s.BaseEntry) && s.BaseType == 17).WhereIf(!string.IsNullOrWhiteSpace(req.InvoiceDocEntry.ToString()), s => s.DocEntry == req.InvoiceDocEntry).Select(s => new { s.DocEntry, s.BaseEntry }).Distinct().ToListAsync();
            var saledln1Ids = saledln1.Select(s => s.DocEntry).ToList();
            //查询应收发票
            var saleinv1 = await UnitWork.Find<sale_inv1>(s => saledln1Ids.Contains((int)s.BaseEntry) && s.BaseType == 15 && s.LineStatus == "O").Select(s => new { s.DocEntry, s.BaseEntry, s.DocDate, s.U_A_ADATE }).Distinct().ToListAsync();
            var oinvIds = saleinv1.Select(s => s.DocEntry).ToList();
            var saleoinvs = await UnitWork.Find<sale_oinv>(s => oinvIds.Contains(s.DocEntry)).Select(s => new { s.DocEntry, s.DocTotal, s.UpdateDate }).Distinct().ToListAsync();
            var salerin1s = await UnitWork.Find<sale_rin1>(s => oinvIds.Contains((uint)s.BaseEntry) && s.BaseType == 13).Select(s => new { s.BaseEntry, TotalMoney = s.Price * s.Quantity }).ToListAsync();

            //var inv1Ids = saleinv1.Select(s => (int)s.DocEntry).Distinct().ToList();
            //查询已退料单
            //var returnNotes = await UnitWork.Find<ReturnNote>(r => inv1Ids.Contains((int)r.InvoiceDocEntry)).Include(r => r.ReturnnoteMaterials).ToListAsync();
            //var docentrys = saleinv1.Select(s => (int)s.DocEntry).ToList();
            //查询服务单
            var serviceOrders = await UnitWork.Find<ServiceOrder>(s => serviceOrderIds.Contains(s.Id)).WhereIf(!string.IsNullOrWhiteSpace(req.Customer), s => s.TerminalCustomerId.Contains(req.Customer) || s.TerminalCustomer.Contains(req.Customer)).Select(s => new { s.Id, s.U_SAP_ID, s.TerminalCustomer, s.TerminalCustomerId, s.NewestContacter, s.NewestContactTel }).ToListAsync();

            var oinvList = from a in saleinv1
                           join b in saledln1 on a.BaseEntry equals b.DocEntry into ab
                           from b in ab.DefaultIfEmpty()
                           join c in queryList on b.BaseEntry equals c.SalesOrderId into bc
                           from c in bc.DefaultIfEmpty()
                           join d in serviceOrders on c.ServiceOrderId equals d.Id into cd
                           from d in cd.DefaultIfEmpty()
                           join e in saleoinvs on a.DocEntry equals e.DocEntry into ae
                           from e in ae.DefaultIfEmpty()
                           select new { a, b, c, d, e };

            //关联所有数据
            result.Data = oinvList.Skip((req.page - 1) * req.limit).Take(req.limit).Select(q => new
            {
                InvoiceDocEntry = q.a?.DocEntry,
                SalesOrderId = q.c?.SalesOrderId,
                U_SAP_ID = q.d?.U_SAP_ID,
                ServiceOrderId = q.d?.Id,
                DocTotal = q.e.DocTotal,
                OutstandingAmount = salerin1s.Where(s => s.BaseEntry == q.a.DocEntry)?.Sum(s => s.TotalMoney) != null ? q.e.DocTotal - salerin1s.Where(s => s.BaseEntry == q.a.DocEntry)?.Sum(s => s.TotalMoney) : q.e.DocTotal,
                UpdateTime = Convert.ToDateTime(q.e.UpdateDate).ToString("yyyy.MM.dd HH:mm:ss"),
                CreateTime = Convert.ToDateTime(q.a?.DocDate).ToString("yyyy.MM.dd HH:mm:ss"),
                q.c?.CreateUser,
                q.d?.TerminalCustomer,
                q.d?.TerminalCustomerId,
                q.d?.NewestContacter,
                q.d?.NewestContactTel,
            }).ToList();

            return result;
        }
        /// <summary>
        /// 获取序列号信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetSerialNumberList(ReturnMaterialReq req) 
        {
            var result = new TableData();
            result.Data = await UnitWork.Find<Quotation>(q => q.SalesOrderId == req.SalesOrderId).Include(q => q.QuotationProducts).Select(q=>q.QuotationProducts.Select(p=>new {p.ProductCode ,p.MaterialCode,p.MaterialDescription}).ToList()).FirstOrDefaultAsync();
            return result;
        }
        /// <summary>
        /// 获取序列号下物料信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> SerialNumberMaterialList(ReturnMaterialReq req)
        {
            var result = new TableData();
            var quotationProducts = await UnitWork.Find<Quotation>(q => q.SalesOrderId == req.SalesOrderId).Include(q => q.QuotationProducts).ThenInclude(q => q.QuotationMaterials).Select(q => q.QuotationProducts).FirstOrDefaultAsync();
            var quotationMaterials = quotationProducts.Where(q => q.ProductCode.Equals(req.ProductCode)).Select(q=>q.QuotationMaterials).FirstOrDefault();
            var MaterialList = quotationMaterials.Select(q => new { q.MaterialCode, q.MaterialDescription, q.Count }).ToList();
            return result;
        }
        /// <summary>
        /// 获取应收发票的物料信息集合
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetMaterialList(ReturnMaterialReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && req.AppUserId > 0)
            {
                loginUser = await GetUserId((int)req.AppUserId);
            }
            var result = new TableData();
            //查询当前技术员所有可退料服务Id
            var quotationObj = await UnitWork.Find<Quotation>(q => q.SalesOrderId == req.SalesOrderId && q.CreateUserId.Equals(loginUser.Id)).Include(q => q.QuotationProducts).ThenInclude(q=>q.QuotationMaterials).Include(q=>q.QuotationMergeMaterials).Where(q=>q.QuotationProducts.Any(p=>p.ProductCode.Equals(req.ProductCode))).FirstOrDefaultAsync();
            var quotationProductObj = quotationObj.QuotationProducts.FirstOrDefault();
            var quotationMergeMaterials = quotationObj.QuotationMergeMaterials.ToList();
            //查询应收发票  && s.LineStatus == "O"
            var saleinv1s = await UnitWork.Find<sale_inv1>(s => s.DocEntry == req.InvoiceDocEntry).ToListAsync();
            //是否存在退料记录
            var materials = await UnitWork.Find<ReturnNoteMaterial>(r => req.InvoiceDocEntry == r.InvoiceDocEntry).ToListAsync();

            List<sale_inv1> saleinv1List = new List<sale_inv1>();
            saleinv1s.ForEach(s=> {
                s.Quantity = s.Quantity - materials.Where(m => m.ReplaceMaterialCode.Equals(s.ItemCode)).Count();
                var quotationMaterialCount = quotationProductObj.QuotationMaterials.Where(q => q.MaterialCode.Equals(s.ItemCode) && Convert.ToDecimal(q.DiscountPrices).ToString("#0.00") == Convert.ToDecimal(s.Price).ToString("#0.00")).Count() - materials.Where(m => m.ReplaceMaterialCode.Equals(s.ItemCode)).Count();
                if (quotationMaterialCount > 0 && s.Quantity>0) 
                {
                    var num = s.Quantity;
                    if (quotationMaterialCount < s.Quantity)
                    {
                        num = quotationMaterialCount;
                    }
                    for (int i = 0; i < num; i++)
                    {
                        s.Quantity = 1;
                        saleinv1List.Add(s);
                    }
                }
            });
            
            result.Data = saleinv1List.Select(s => new ReturnMaterialListResp
            {
                MaterialCode = "",
                MaterialDescription = "",
                Moeny=Convert.ToDecimal(s.Price),
                QuotationMaterialId= quotationMergeMaterials.Where(q=>q.MaterialCode.Equals(s.ItemCode)&& Convert.ToDecimal(q.DiscountPrices).ToString("#0.00") == Convert.ToDecimal(s.Price).ToString("#0.00")).FirstOrDefault()?.Id,
                SNandPn="",
                ReplaceSNandPN="",
                ReplaceMaterialCode= s.ItemCode,
                ReplaceMaterialDescription= s.Dscription
            }).ToList();
            return result;
        }

        /// <summary>
        /// 查询退料信息列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetDetails(ReturnMaterialReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && req.AppUserId > 0)
            {
                loginUser = await GetUserId((int)req.AppUserId);
            }
            //.Include(r => r.ReturnnoteOperationHistorys)
            var returnNotes = await UnitWork.Find<ReturnNote>(r => r.Id == req.returnNoteId).Include(r => r.ReturnNotePictures).Include(r=>r.ReturnNoteProducts).ThenInclude(r => r.ReturnNoteMaterials).ThenInclude(r => r.ReturnNoteMaterialPictures).FirstOrDefaultAsync();
            var History = await UnitWork.Find<FlowInstanceOperationHistory>(f => f.InstanceId.Equals(returnNotes.FlowInstanceId)).OrderBy(f => f.CreateDate).ToListAsync();
            List<ReturnNoteMaterial> returnnoteMaterials = new List<ReturnNoteMaterial>();
            returnNotes.ReturnNoteProducts.ForEach(r => { returnnoteMaterials.AddRange(r.ReturnNoteMaterials); });
            //查询当前技术员所有可退料服务Id
            var quotationObj = await UnitWork.Find<Quotation>(q => q.SalesOrderId == returnNotes.SalesOrderId).Include(q => q.QuotationMergeMaterials).FirstOrDefaultAsync();
            var InvoiceDocEntry = returnnoteMaterials.Select(r => r.InvoiceDocEntry).FirstOrDefault();
            var result = new TableData();
            //var serviceOrders = await UnitWork.Find<ServiceOrder>(s => s.Id == returnNotes.ServiceOrderId).Select(s => new { s.Id, s.TerminalCustomer, s.TerminalCustomerId, s.NewestContacter, s.NewestContactTel, s.U_SAP_ID }).FirstOrDefaultAsync();
            //查询应收发票  && s.LineStatus == "O"
            var saleinv1 = await UnitWork.Find<sale_inv1>(s => s.DocEntry == InvoiceDocEntry && s.sbo_id == Define.SBO_ID).ToListAsync();
            var DocTotal = saleinv1.Sum(s => s.LineTotal);
            //是否存在退料记录
            var materials = await UnitWork.Find<ReturnNoteMaterial>(r => r.ReturnNoteId != returnNotes.Id && InvoiceDocEntry == r.InvoiceDocEntry).ToListAsync();

            List<string> fileIds = new List<string>();
            var numberIds = returnnoteMaterials.Select(r => r.Id).ToList();

            var fileList = await UnitWork.Find<UploadFile>(f => fileIds.Contains(f.Id)).ToListAsync();
            
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ReturnNoteTypeName")).Select(u => new { u.Name, u.DtValue }).ToListAsync();
            var flowInstanceObj = await UnitWork.Find<FlowInstance>(f => f.Id.Equals(returnNotes.FlowInstanceId)).FirstOrDefaultAsync();
            var returnnoteOperationHistorys = History.Select(h => new OperationHistoryResp
            {
                CreateTime = h.CreateDate.ToString("yyyy.MM.dd HH:mm:ss"),
                Remark = h.Remark,
                IntervalTime = h.IntervalTime.ToString(),
                CreateUserName = h.CreateUserName,
                Content = h.Content,
                ApprovalResult = h.ApprovalResult,
                ApprovalStage = h.ApprovalStage
            }).ToList();
            if (req.IsUpDate != null && (bool)req.IsUpDate) 
            {
                foreach (var item in returnNotes.ReturnNoteProducts)
                {
                    List<ReturnMaterialListResp> MaterialList = (await GetMaterialList(new ReturnMaterialReq { InvoiceDocEntry=item.ReturnNoteMaterials.FirstOrDefault()?.InvoiceDocEntry,ProductCode=item.ProductCode,SalesOrderId= returnNotes .SalesOrderId})).Data;
                    var returnNoteMaterials = MaterialList.Select(m => new ReturnNoteMaterial {
                        ReplaceSNandPN=m.ReplaceSNandPN,
                        SNandPN=m.SNandPn,
                        Moeny=m.Moeny,
                        ReplaceMaterialCode=m.ReplaceMaterialCode, 
                        MaterialCode=m.MaterialCode,
                        MaterialDescription=m.MaterialDescription,
                        QuotationMaterialId=m.QuotationMaterialId
                    }).ToList();
                    returnNotes.ReturnNoteProducts.Where(r => r.Id.Equals(item.Id)).FirstOrDefault().ReturnNoteMaterials.AddRange(returnNoteMaterials);
                }
            }
            List<FlowPathResp> flowPathResp = new List<FlowPathResp>();
            if (!string.IsNullOrWhiteSpace(returnNotes.FlowInstanceId)) 
            {
                flowPathResp = await _flowInstanceApp.FlowPathRespList(returnnoteOperationHistorys, returnNotes.FlowInstanceId);
            }
            var qoutationReq = await _pending.QuotationDetails(quotationObj.Id);
            var serviceOrders = await _pending.ServiceOrderDetails(returnNotes.SalesOrderId, returnNotes.CreateUserId);
            result.Data = new
            {
                InvoiceDocEntry,
                DocTotal = DocTotal,
                Status = flowInstanceObj?.IsFinish == FlowInstanceStatus.Rejected ? "驳回" : flowInstanceObj?.ActivityName == null ? "开始" : flowInstanceObj?.ActivityName,
                returnNoteId = returnNotes.Id,
                returnNotes,
                serviceOrders,
                flowPathResp,
                returnnoteOperationHistorys,
                Quotations = qoutationReq
            };
            return result;
        }

        /// <summary>
        /// 添加退料单
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task Add(AddOrUpdateReturnnoteReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var loginOrg = loginContext.Orgs;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppUserId));
                loginOrg = await GetOrgs(loginUser.Id);
            }
            //事务保证数据一致
            var dbContext = UnitWork.GetDbContext<Quotation>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var returnnotrObj = obj.MapTo<ReturnNote>();
                    returnnotrObj.CreateTime = DateTime.Now;
                    returnnotrObj.CreateUser = loginUser.Name;
                    returnnotrObj.CreateUserId = loginUser.Id;
                    returnnotrObj.UpdateTime = DateTime.Now;
                    returnnotrObj.IsLiquidated = false;
                    returnnotrObj.TotalMoney = await CalculatePrice(obj);
                    returnnotrObj = await UnitWork.AddAsync<ReturnNote, int>(returnnotrObj);
                    await UnitWork.SaveAsync();
                    if (!obj.IsDraft)
                    {
                        //创建退料流程
                        var mf = _moduleFlowSchemeApp.Get(m => m.Module.Name.Equals("物料退料单"));
                        var afir = new AddFlowInstanceReq();
                        afir.SchemeId = mf.FlowSchemeId;
                        afir.FrmType = 2;
                        afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                        afir.CustomName = $"退料单";
                        afir.FrmData = "{\"ReturnnoteId\":\"" + returnnotrObj.Id + "\"}";
                        afir.OrgId = loginOrg.FirstOrDefault()?.Id;
                        var FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                        await UnitWork.UpdateAsync<ReturnNote>(r => r.Id == returnnotrObj.Id, r => new ReturnNote { FlowInstanceId = FlowInstanceId });
                        //增加全局待处理
                        var serviceOrederObj = await UnitWork.Find<ServiceOrder>(s => s.Id == obj.ServiceOrderId).FirstOrDefaultAsync();
                        await _workbenchApp.AddOrUpdate(new WorkbenchPending
                        {
                            OrderType = 2,
                            TerminalCustomer = serviceOrederObj.TerminalCustomer,
                            TerminalCustomerId = serviceOrederObj.TerminalCustomerId,
                            ServiceOrderId = serviceOrederObj.Id,
                            ServiceOrderSapId = (int)serviceOrederObj.U_SAP_ID,
                            UpdateTime = returnnotrObj.UpdateTime,
                            Remark = returnnotrObj.Remark,
                            FlowInstanceId = FlowInstanceId,
                            TotalMoney = returnnotrObj.TotalMoney,
                            Petitioner = loginUser.Name,
                            SourceNumbers = returnnotrObj.Id,
                            PetitionerId = loginUser.Id
                        });
                    }
                    List<string> materialIds = new List<string>();
                    returnnotrObj.ReturnNoteProducts.ForEach(r =>
                    {
                        materialIds.AddRange(r.ReturnNoteMaterials.Select(m => m.Id).ToList());
                    });
                    await UnitWork.UpdateAsync<ReturnNoteMaterial>(r => materialIds.Contains(r.Id), r => new ReturnNoteMaterial { ReturnNoteId = returnnotrObj.Id });
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("添加退料单失败。请重试" + ex.Message);
                }
            }

        }

        /// <summary>
        /// 修改退料单
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task Update(AddOrUpdateReturnnoteReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var loginOrg = loginContext.Orgs;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppUserId));
                loginOrg = await GetOrgs(loginUser.Id);
            }
            var dbContext = UnitWork.GetDbContext<ReturnNote>();
            //事务
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //先删后增
                    #region 删除
                    var returnNoteObj = await UnitWork.Find<ReturnNote>(r => r.Id == obj.ReturnNoteId).Include(r => r.ReturnNotePictures).FirstOrDefaultAsync();
                    await UnitWork.DeleteAsync<ReturnNoteProduct>(r => r.ReturnNoteId == returnNoteObj.Id);
                    if (returnNoteObj.ReturnNotePictures != null && returnNoteObj.ReturnNotePictures.Count > 0)
                    {
                        await UnitWork.BatchDeleteAsync<ReturnNotePicture>(returnNoteObj.ReturnNotePictures.ToArray());
                    }
                    await UnitWork.SaveAsync();
                    #endregion
                    #region 新增
                    obj.ReturnNoteProducts.ForEach(r => { r.ReturnNoteId = obj.ReturnNoteId;r.ReturnNoteMaterials.ForEach(m => m.ReturnNoteId = obj.ReturnNoteId); } );
                    await UnitWork.BatchAddAsync<ReturnNoteProduct>(obj.ReturnNoteProducts.ToArray());
                    obj.ReturnNotePictures.ForEach(r => r.ReturnNoteId = obj.ReturnNoteId);
                    await UnitWork.BatchAddAsync<ReturnNotePicture>(obj.ReturnNotePictures.ToArray());
                    await UnitWork.SaveAsync();
                    #endregion
                    //var returnnotrStatus = 0;
                    var FlowInstanceId = "";
                    if (!obj.IsDraft)
                    {
                        //returnnotrStatus = 3;//未提交
                        if (string.IsNullOrWhiteSpace(returnNoteObj.FlowInstanceId))
                        {
                            //创建结算流程
                            var mf = _moduleFlowSchemeApp.Get(m => m.Module.Name.Equals("物料退料单"));
                            var afir = new AddFlowInstanceReq();
                            afir.SchemeId = mf.FlowSchemeId;
                            afir.FrmType = 2;
                            afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                            afir.CustomName = $"退料单";
                            afir.FrmData = "{\"ReturnnoteId\":\"" + obj.ReturnNoteId + "\"}";
                            afir.OrgId = loginOrg.FirstOrDefault()?.Id;
                            FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                        }
                        else
                        {
                            FlowInstanceId = returnNoteObj.FlowInstanceId;
                            await _flowInstanceApp.Start(new StartFlowInstanceReq() { FlowInstanceId = returnNoteObj.FlowInstanceId });
                        }
                        //增加全局待处理
                        var serviceOrederObj = await UnitWork.Find<ServiceOrder>(s => s.Id == obj.ServiceOrderId).FirstOrDefaultAsync();
                        await _workbenchApp.AddOrUpdate(new WorkbenchPending
                        {
                            OrderType = 2,
                            TerminalCustomer = serviceOrederObj.TerminalCustomer,
                            TerminalCustomerId = serviceOrederObj.TerminalCustomerId,
                            ServiceOrderId = serviceOrederObj.Id,
                            ServiceOrderSapId = (int)serviceOrederObj.U_SAP_ID,
                            UpdateTime = DateTime.Now,
                            Remark = returnNoteObj.Remark,
                            FlowInstanceId = FlowInstanceId,
                            TotalMoney = returnNoteObj.TotalMoney,
                            Petitioner = loginUser.Name,
                            SourceNumbers = returnNoteObj.Id,
                            PetitionerId = loginUser.Id
                        });
                    }
                    obj.TotalMoney = await CalculatePrice(obj);
                    await UnitWork.UpdateAsync<ReturnNote>(r => r.Id == obj.ReturnNoteId, r => new ReturnNote
                    {
                        // Status = returnnotrStatus,
                        UpdateTime = DateTime.Now,
                        FreightCharge = obj.FreightCharge,
                        DeliveryMethod = int.Parse(obj.DeliveryMethod),
                        Remark = obj.Remark,
                        ExpressNumber = obj.ExpressNumber,
                        TotalMoney = obj.TotalMoney,
                        FlowInstanceId = FlowInstanceId
                    });

                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("添加退料单失败。请重试" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 删除退料单
        /// </summary>
        /// <param name="returnNoteId"></param>
        /// <returns></returns>
        public async Task Delete(int returnNoteId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var returnNoteObj = await UnitWork.Find<ReturnNote>(r => r.Id == returnNoteId).FirstOrDefaultAsync();
            if (returnNoteObj.FlowInstanceId != null) 
            {
                var flowInstanceObj = await UnitWork.Find<FlowInstance>(f => f.Id.Equals(returnNoteObj.FlowInstanceId)).FirstOrDefaultAsync();
                if (flowInstanceObj.IsFinish == FlowInstanceStatus.Finished || flowInstanceObj.IsFinish == FlowInstanceStatus.Running)
                {
                    throw new Exception("此退料单已完成或正在进行中不可删除。");
                }
                await UnitWork.DeleteAsync<FlowInstance>(flowInstanceObj);
            }
            
            //删除所有关联数据
            if (returnNoteObj != null)
            {
                await UnitWork.DeleteAsync<ReturnNote>(returnNoteObj);
                await UnitWork.SaveAsync();
            }
            else
            {
                throw new Exception("删除失败，无该退料单。");
            }

        }

        /// <summary>
        /// 审批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Accraditation(AccraditationReturnNoteReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var returnNotes = await UnitWork.Find<ReturnNote>(r => r.Id == req.Id).Include(r => r.ReturnNoteProducts).ThenInclude(r=>r.ReturnNoteMaterials).FirstOrDefaultAsync();
            if (returnNotes == null)
            {
                throw new Exception("退料单为空，请核对。");
            }
            var flowInstanceObj = await UnitWork.Find<FlowInstance>(f => f.Id.Equals(returnNotes.FlowInstanceId)).FirstOrDefaultAsync();
            if (loginContext.Roles.Any(r => r.Name.Equals("物料品质")) && flowInstanceObj.ActivityName.Equals("品质检验"))
            {
                if (!req.IsReject)
                {
                    foreach (var item in req.returnnoteMaterials)
                    {
                        await UnitWork.UpdateAsync<ReturnNoteMaterial>(r=>r.Id.Equals(item.MaterialsId),r=>new ReturnNoteMaterial { IsGood=item.IsGood});
                    }
                }
            }
            if (loginContext.Roles.Any(r => r.Name.Equals("仓库")) && flowInstanceObj.ActivityName.Equals("仓库入库"))
            {
                if (!req.IsReject)
                {
                    foreach (var item in req.returnnoteMaterials)
                    {
                        await UnitWork.UpdateAsync<ReturnNoteMaterial>(r => r.Id.Equals(item.MaterialsId), r => new ReturnNoteMaterial { GoodWhsCode=item.GoodWhsCode,SecondWhsCode = item.SecondWhsCode });
                    }
                }
            }
            VerificationReq VerificationReqModle = new VerificationReq
            {
                NodeRejectStep = "",
                NodeRejectType = "0",
                FlowInstanceId = returnNotes.FlowInstanceId,
                VerificationFinally = "1",
                VerificationOpinion = req.Remark,
            };
            if (req.IsReject)
            {
                VerificationReqModle.VerificationFinally = "3";
                VerificationReqModle.VerificationOpinion = req.Remark;
                VerificationReqModle.NodeRejectType = "1";
                await _flowInstanceApp.Verification(VerificationReqModle);
            }
            else
            {
                await _flowInstanceApp.Verification(VerificationReqModle);
            }
            await UnitWork.UpdateAsync<ReturnNote>(r => r.Id == returnNotes.Id, r => new ReturnNote
            {
                UpdateTime = DateTime.Now
            });
            //修改全局待处理
            await UnitWork.UpdateAsync<WorkbenchPending>(w => w.SourceNumbers == returnNotes.Id && w.OrderType == 2, w => new WorkbenchPending
            {
                UpdateTime = DateTime.Now,
            });
            await UnitWork.SaveAsync();
            if (loginContext.Roles.Any(r => r.Name.Equals("仓库")) && flowInstanceObj.ActivityName.Equals("仓库入库"))
            {
                if (!req.IsReject)
                {
                    await WarehousePutMaterialsIn(req);
                }
            }
        }

        /// <summary>
        /// 计算价格
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private async Task<decimal> CalculatePrice(AddOrUpdateReturnnoteReq obj)
        {
            var returnNoteMaterials = obj.ReturnNoteProducts.Select(r => r.ReturnNoteMaterials).ToList();
            decimal TotalMoney = 0;
            obj.ReturnNoteProducts.ForEach(r =>
            {
                TotalMoney = r.ReturnNoteMaterials.Sum(m => m.Moeny);
            });
            return TotalMoney;
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
        /// 获取用户部门
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        private async Task<List<OpenAuth.Repository.Domain.Org>> GetOrgs(string UserId)
        {
            var orgids = await UnitWork.Find<Relevance>(
                    u => u.FirstId == UserId && u.Key == Define.USERORG).Select(u => u.SecondId).ToListAsync();
            var orgs = (await UnitWork.Find<OpenAuth.Repository.Domain.Org>(u => orgids.Contains(u.Id)).ToListAsync()).Min(o => o.CascadeId);
            return await UnitWork.Find<OpenAuth.Repository.Domain.Org>(u => u.CascadeId.Contains(orgs)).ToListAsync();
        }

        /// <summary>
        /// 仓库入库
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task WarehousePutMaterialsIn(AccraditationReturnNoteReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var returnNoteObj = await UnitWork.Find<ReturnNote>(r => r.Id == req.Id).Include(r=>r.ReturnNoteProducts).ThenInclude(r=>r.ReturnNoteMaterials).FirstOrDefaultAsync();
            List<ReturnNoteMaterial> returnnoteMaterials = new List<ReturnNoteMaterial>();
            returnNoteObj.ReturnNoteProducts.ForEach(r =>
            {
                returnnoteMaterials.AddRange(r.ReturnNoteMaterials.ToList());
            });
            var returnnoteMaterialList= returnnoteMaterials.GroupBy(r => new { r.MaterialCode, r.MaterialDescription, r.Moeny, r.IsGood, r.GoodWhsCode, r.SecondWhsCode }).Select(r=>new { 
                r.First().MaterialCode,
                r.First().MaterialDescription,
                r.First().IsGood,
                r.First().InvoiceDocEntry,
                r.First().GoodWhsCode,
                r.First().SecondWhsCode,
                r.First().Moeny,
                r.First().ReturnNoteId,
                r.First().QuotationMaterialId,
                Count = r.Count(),

            }).ToList();

            List<QuotationMergeMaterialReq> returnMergeMaterialGoodReqs = new List<QuotationMergeMaterialReq>();
            List<QuotationMergeMaterialReq> returnMergeMaterialSecondReqs = new List<QuotationMergeMaterialReq>();

            if (returnnoteMaterialList != null)
            {
                foreach (var item in returnnoteMaterialList)
                {
                    if ((bool)item.IsGood)
                    {
                        returnMergeMaterialGoodReqs.Add(new QuotationMergeMaterialReq { 
                            InventoryQuantity=item.Count,
                            ReturnNoteId=item.ReturnNoteId,
                            WhsCode=item.GoodWhsCode,
                            MaterialCode=item.MaterialCode,
                            MaterialDescription=item.MaterialDescription,
                            Id=item.QuotationMaterialId
                        });
                    }
                    else 
                    {
                        returnMergeMaterialSecondReqs.Add(new QuotationMergeMaterialReq
                        {
                            InventoryQuantity = item.Count,
                            ReturnNoteId = item.ReturnNoteId,
                            WhsCode = item.SecondWhsCode,
                            MaterialCode = item.MaterialCode,
                            MaterialDescription = item.MaterialDescription,
                            Id = item.QuotationMaterialId
                        });
                    }   
                }

                //foreach (var item in req.returnnoteMaterials)
                //{
                //    var returnnoteMaterialObj = returnNoteObj.ReturnnoteMaterials.Where(r => r.Id.Equals(item.MaterialsId)).FirstOrDefault();
                //    if (item.GoodQty > 0)
                //    {
                //        var putInMaterialInfo = new QuotationMergeMaterialReq { Id = returnnoteMaterialObj.QuotationMaterialId, InventoryQuantity = item.GoodQty, ReturnNoteId = req.Id, WhsCode = item.GoodWhsCode, MaterialCode = returnnoteMaterialObj.MaterialCode, MaterialDescription = returnnoteMaterialObj.MaterialDescription };
                //        returnMergeMaterialGoodReqs.Add(putInMaterialInfo);
                //    }
                //    if (item.SecondQty > 0)
                //    {
                //        var putInMaterialInfo = new QuotationMergeMaterialReq { Id = returnnoteMaterialObj.QuotationMaterialId, InventoryQuantity = item.SecondQty, ReturnNoteId = req.Id, WhsCode = item.SecondWhsCode, MaterialCode = returnnoteMaterialObj.MaterialCode, MaterialDescription = returnnoteMaterialObj.MaterialDescription };
                //        returnMergeMaterialSecondReqs.Add(putInMaterialInfo);
                //    }
                //}


            }
            //推送到SAP
            if (returnMergeMaterialGoodReqs.Count > 0)
            {
                _capBus.Publish("Serve.ReceiptCreditVouchers.Create", new AddOrUpdateQuotationReq { InvoiceDocEntry = returnnoteMaterials.FirstOrDefault()?.InvoiceDocEntry, SalesOrderId = returnNoteObj.SalesOrderId, QuotationMergeMaterialReqs = returnMergeMaterialGoodReqs });
            }
            if (returnMergeMaterialSecondReqs.Count > 0)
            {
                _capBus.Publish("Serve.ReceiptCreditVouchers.Create", new AddOrUpdateQuotationReq { InvoiceDocEntry = returnnoteMaterials.FirstOrDefault()?.InvoiceDocEntry, SalesOrderId = returnNoteObj.SalesOrderId, QuotationMergeMaterialReqs = returnMergeMaterialSecondReqs });
            }
        }
        #endregion
    }
}