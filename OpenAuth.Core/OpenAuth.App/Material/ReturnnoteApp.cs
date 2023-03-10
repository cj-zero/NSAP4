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
using OpenAuth.Repository.Domain.Material;
using System.Text;
using OpenAuth.App.SignalR;
using Microsoft.AspNetCore.SignalR;
using System.IO;
using Infrastructure.Export;
using DinkToPdf;

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
        private readonly OrgManagerApp _orgApp;
        private readonly UserManagerApp _userManagerApp;

        private readonly IHubContext<MessageHub> _hubContext;

        public ReturnNoteApp(IUnitWork unitWork, FlowInstanceApp flowInstanceApp, WorkbenchApp workbenchApp, PendingApp pending,
            ModuleFlowSchemeApp moduleFlowSchemeApp, IAuth auth, ICapPublisher capBus, OrgManagerApp orgApp, UserManagerApp userManagerApp, IHubContext<MessageHub> hubContext) : base(unitWork, auth)
        {
            _flowInstanceApp = flowInstanceApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            //_expressageApp = expressageApp;
            _capBus = capBus;
            //_quotation = quotation;
            _workbenchApp = workbenchApp;
            _pending = pending;
            _orgApp = orgApp;
            _userManagerApp = userManagerApp;
            _hubContext = hubContext;
        }

        #region app???erp??????
        /// <summary>
        /// ????????????????????????
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
                throw new CommonException("???????????????", Define.INVALID_TOKEN);
            }
            if (loginUser.Account == Define.USERAPP && req.AppUserId > 0)
            {
                loginUser = await GetUserId((int)req.AppUserId);
            }
            List<int> serviceOrderIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(req.Customer))
            {
                var serviceOrderList = await UnitWork.Find<ServiceOrder>(s => s.TerminalCustomer.Contains(req.Customer) || s.TerminalCustomerId.Contains(req.Customer)).Select(s => new { s.Id }).Distinct().ToListAsync();
                serviceOrderIds = serviceOrderList.Select(s => s.Id).ToList();
            }
            var returnNotes = UnitWork.Find<ReturnNote>(null).Include(r => r.ReturnNotePictures).Include(r => r.ReturnNoteProducts).ThenInclude(r => r.ReturnNoteMaterials)
                                .WhereIf(req.returnNoteId != null, r => r.Id == req.returnNoteId)
                                .WhereIf(!string.IsNullOrWhiteSpace(req.CreateUserName), r => r.CreateUser.Contains(req.CreateUserName))
                                .WhereIf(!string.IsNullOrWhiteSpace(req.SapId.ToString()), r => r.ServiceOrderSapId == req.SapId)
                                .WhereIf(!string.IsNullOrWhiteSpace(req.SalesOrderId.ToString()), r => r.SalesOrderId == req.SalesOrderId)
                                .WhereIf(!string.IsNullOrWhiteSpace(req.MaterialCode), r => r.ReturnNoteProducts.Any(x => x.ReturnNoteMaterials.Any(m => m.MaterialCode == req.MaterialCode)))
                                .WhereIf(!string.IsNullOrWhiteSpace(req.StartDate.ToString()), r => r.CreateTime > req.StartDate)
                                .WhereIf(serviceOrderIds.Count() > 0, r => serviceOrderIds.Contains(r.ServiceOrderId))
                                .WhereIf(!string.IsNullOrWhiteSpace(req.EndDate.ToString()), r => r.CreateTime < Convert.ToDateTime(req.EndDate).AddDays(1));
            if (!string.IsNullOrWhiteSpace(req.Status))
            {
                if (req.Status == "??????")
                {
                    flowInstanceIds.AddRange(await UnitWork.Find<FlowInstance>(f => f.IsFinish == FlowInstanceStatus.Rejected).Select(s => s.Id).ToListAsync());
                }
                else
                {
                    flowInstanceIds.AddRange(await UnitWork.Find<FlowInstance>(f => f.ActivityName.Equals(req.Status)).Select(s => s.Id).ToListAsync());
                }
                if (req.Status == "??????")
                {
                    returnNotes = returnNotes.Where(r => flowInstanceIds.Contains(r.FlowInstanceId) || string.IsNullOrEmpty(r.FlowInstanceId));
                }
                else
                {
                    returnNotes = returnNotes.Where(r => flowInstanceIds.Contains(r.FlowInstanceId));
                }
            }
            if (loginUser.Account!=Define.SYSTEM_USERNAME && !loginContext.Roles.Any(r => r.Name.Equals("????????????-????????????ID")))
            {
                #region ????????????
                //var schemeContent = await .FirstOrDefaultAsync();
                var SchemeContent = await UnitWork.Find<FlowScheme>(f => f.SchemeName.Equals("???????????????")).Select(f => f.SchemeContent).FirstOrDefaultAsync();
                SchemeContentJson schemeJson = JsonHelper.Instance.Deserialize<SchemeContentJson>(SchemeContent);
                switch (req.PageType)
                {
                    case 1:
                        if (loginContext.Roles.Any(r => r.Name.Equals("????????????")))
                        {
                            lineId = schemeJson.Nodes.Where(n => n.name.Equals("????????????")).FirstOrDefault()?.id;
                        }
                        break;
                    case 2:
                        if (loginContext.Roles.Any(r => r.Name.Equals("????????????")))
                        {
                            lineId = schemeJson.Nodes.Where(n => n.name.Equals("????????????")).FirstOrDefault()?.id;
                        }
                        break;
                    case 3:
                        if (loginContext.Roles.Any(r => r.Name.Equals("?????????")) || loginContext.Roles.Any(r => r.Name.Equals("?????????")))
                        {
                            lineId = schemeJson.Nodes.Where(n => n.name.Equals("???????????????")).FirstOrDefault()?.id;
                        }
                        break;
                    case 4:
                        if (loginContext.Roles.Any(r => r.Name.Equals("??????")))
                        {
                            lineId = schemeJson.Nodes.Where(n => n.name.Equals("????????????")).FirstOrDefault()?.id;
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
                #endregion
            }

            var result = new TableData();
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ReturnNoteTypeName")).Select(u => new { u.Name, u.DtValue }).ToListAsync();
            result.Count = await returnNotes.CountAsync();
            var returnNoteList = await returnNotes.OrderByDescending(r => r.UpdateTime).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            flowInstanceIds = returnNoteList.Select(r => r.FlowInstanceId).ToList();
            var flowInstanceList = await UnitWork.Find<FlowInstance>(f => flowInstanceIds.Contains(f.Id)).ToListAsync();
           
            serviceOrderIds = returnNoteList.Select(r => r.ServiceOrderId).ToList();
            var serviceOrders = await UnitWork.Find<ServiceOrder>(s => serviceOrderIds.Contains(s.Id)).Select(s => new { s.Id, s.TerminalCustomer, s.TerminalCustomerId }).ToListAsync();
            var userIds = returnNoteList.Select(r => r.CreateUserId).ToList();
            var SelOrgName = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).Select(o => new { o.Id, o.Name, o.CascadeId }).ToListAsync();
            var Relevances = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && userIds.Contains(r.FirstId)).Select(r => new { r.FirstId, r.SecondId }).ToListAsync();

            var returnNoteMainRespList = returnNoteList.Select(r => new ReturnNoteMainResp
            {
                returnNoteId = r.Id,
                ServiceOrderId = r.ServiceOrderId,
                SalesOrderId = r.SalesOrderId,
                ServiceOrderSapId = r.ServiceOrderSapId,
                Reason = r.Reason,
                CreateUser = SelOrgName.Where(s => s.Id.Equals(Relevances.Where(w => w.FirstId.Equals(r.CreateUserId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name == null ? r.CreateUser : SelOrgName.Where(s => s.Id.Equals(Relevances.Where(w => w.FirstId.Equals(r.CreateUserId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name + "-" + r.CreateUser,
                CreateTime = Convert.ToDateTime(r.CreateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                UpdateTime = Convert.ToDateTime(r.UpdateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                TotalMoney = r.TotalMoney,
                Status = flowInstanceList.Where(f => f.Id.Equals(r.FlowInstanceId)).FirstOrDefault()?.IsFinish != FlowInstanceStatus.Rejected ? flowInstanceList.Where(f => f.Id.Equals(r.FlowInstanceId)).FirstOrDefault()?.ActivityName : "??????",
                IsUpDate = flowInstanceList.Where(f => f.Id.Equals(r.FlowInstanceId)).FirstOrDefault()?.IsFinish != FlowInstanceStatus.Running ? flowInstanceList.Where(f => f.Id.Equals(r.FlowInstanceId)).FirstOrDefault()?.IsFinish == FlowInstanceStatus.Finished ? false : true : false,
                IsLiquidated = r.IsLiquidated,
                Remark = r.Remark,
                InvoiceDocEntry = r.ReturnNoteProducts.FirstOrDefault()?.ReturnNoteMaterials.FirstOrDefault()?.InvoiceDocEntry,
                TerminalCustomer = serviceOrders.Where(s => s.Id == r.ServiceOrderId).FirstOrDefault()?.TerminalCustomer,
                TerminalCustomerId = serviceOrders.Where(s => s.Id == r.ServiceOrderId).FirstOrDefault()?.TerminalCustomerId,
            }).ToList();
            returnNoteMainRespList.ForEach(r => { r.StatusName = r.Status != null ? CategoryList.Where(c => c.DtValue.Equals(r.Status)).FirstOrDefault()?.Name : "?????????"; r.Status = r.Status != null ? r.Status : "??????"; });
            result.Data = returnNoteMainRespList;
            return result;
        }

        /// <summary>
        /// ??????????????????????????????
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetOinvList(ReturnMaterialReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("???????????????", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && req.AppUserId > 0)
            {
                loginUser = await GetUserId((int)req.AppUserId);
            }
            var result = new TableData();
            //??????????????????????????????????????????Id
            var query = from a in UnitWork.Find<QuotationMergeMaterial>(null)
                        join b in UnitWork.Find<Quotation>(null) on a.QuotationId equals b.Id
                        where b.CreateUserId == loginUser.Id && b.QuotationStatus == 11 && b.SalesOrderId != null
                        select new { b };
            query = query.WhereIf(!string.IsNullOrWhiteSpace(req.SalesOrderId.ToString()), q => q.b.SalesOrderId == req.SalesOrderId)
                .WhereIf(!string.IsNullOrWhiteSpace(req.ServiceOrderId.ToString()), q => q.b.ServiceOrderId == req.ServiceOrderId);
            var queryList = await query.Select(q => new { q.b.ServiceOrderId, q.b.SalesOrderId, q.b.CreateUser, q.b.Id }).Distinct().ToListAsync();
            //???????????????id??????
            var serviceOrderIds = queryList.Select(s => s.ServiceOrderId).ToList();
            //??????????????????id??????
            var salesOrderIds = queryList.Select(s => s.SalesOrderId).ToList();
            //??????????????????
            var saledln1 = await UnitWork.Find<sale_dln1>(s => salesOrderIds.Contains(s.BaseEntry) && s.BaseType == 17).Select(s => new { s.DocEntry, s.BaseEntry }).Distinct().ToListAsync();
            var saledln1Ids = saledln1.Select(s => s.DocEntry).ToList();
            //??????????????????
            var saleinv1 = await UnitWork.Find<sale_inv1>(s => saledln1Ids.Contains((int)s.BaseEntry) && s.BaseType == 15 && s.LineStatus == "O").WhereIf(!string.IsNullOrWhiteSpace(req.InvoiceDocEntry.ToString()), s => s.DocEntry == req.InvoiceDocEntry).Select(s => new { s.DocEntry, s.BaseEntry, s.DocDate, s.U_A_ADATE }).Distinct().ToListAsync();
            var oinvIds = saleinv1.Select(s => s.DocEntry).ToList();
            var saleoinvs = await UnitWork.Find<sale_oinv>(s => oinvIds.Contains(s.DocEntry)).Select(s => new { s.DocEntry, s.DocTotal, s.UpdateDate }).Distinct().ToListAsync();
            var salerin1s = await UnitWork.Find<sale_rin1>(s => oinvIds.Contains((uint)s.BaseEntry) && s.BaseType == 13).Select(s => new { s.BaseEntry, TotalMoney = s.Price * s.Quantity }).ToListAsync();

            //var inv1Ids = saleinv1.Select(s => (int)s.DocEntry).Distinct().ToList();
            //??????????????????
            //var returnNotes = await UnitWork.Find<ReturnNote>(r => inv1Ids.Contains((int)r.InvoiceDocEntry)).Include(r => r.ReturnnoteMaterials).ToListAsync();
            //var docentrys = saleinv1.Select(s => (int)s.DocEntry).ToList();
            //???????????????
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
            result.Count = oinvList.Count();
            //??????????????????
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
                QuotaionId = q.c?.Id,
                q.d?.TerminalCustomer,
                q.d?.TerminalCustomerId,
                q.d?.NewestContacter,
                q.d?.NewestContactTel,
            }).ToList();

            return result;
        }
        /// <summary>
        /// ?????????????????????
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetSerialNumberList(ReturnMaterialReq req)
        {
            var result = new TableData();
            if (req.SalesOrderId!=null)
            {
                result.Data = await UnitWork.Find<Quotation>(q => q.SalesOrderId == req.SalesOrderId).Include(q => q.QuotationProducts).Select(q => q.QuotationProducts.Select(p => new { p.ProductCode, p.MaterialCode, p.MaterialDescription }).ToList()).FirstOrDefaultAsync();
            }
            else if (req.QuotationId!=null)
            {
                result.Data = await UnitWork.Find<Quotation>(q => q.Id == req.QuotationId).Include(q => q.QuotationProducts).Select(q => q.QuotationProducts.Select(p => new { p.ProductCode, p.MaterialCode, p.MaterialDescription }).ToList()).FirstOrDefaultAsync();
            }
            return result;
        }
        /// <summary>
        /// ??????????????????????????????
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> SerialNumberMaterialList(ReturnMaterialReq req)
        {
            var result = new TableData();
            var quotationObj = await UnitWork.Find<Quotation>(q => q.SalesOrderId == req.SalesOrderId).Include(q => q.QuotationMergeMaterials).Include(q => q.QuotationProducts).ThenInclude(q => q.QuotationMaterials).FirstOrDefaultAsync();
            var quotationProducts = quotationObj.QuotationProducts;
            var quotationMergeMaterials = quotationObj.QuotationMergeMaterials;
            var quotationMaterials = quotationProducts.Where(q => q.ProductCode.Equals(req.ProductCode)).Select(q => q.QuotationMaterials).FirstOrDefault();
            var MaterialList = quotationMaterials.Select(q => new { q.MaterialCode, q.MaterialDescription, q.DiscountPrices, q.Count }).ToList();
            List<SerialNumberMaterial> serialNumberMaterials = new List<SerialNumberMaterial>();
            MaterialList.ForEach(m =>
            {
                var mergeMaterialId = quotationMergeMaterials.Where(q => q.DiscountPrices == m.DiscountPrices && q.MaterialCode == m.MaterialCode).FirstOrDefault()?.Id;
                for (int i = 0; i < m.Count; i++)
                {
                    serialNumberMaterials.Add(new SerialNumberMaterial { MaterialCode = m.MaterialCode, MaterialDescription = m.MaterialDescription,QuotationMaterialId= mergeMaterialId });
                }
            });
            result.Data = serialNumberMaterials;
            return result;
        }
        /// <summary>
        /// ???????????????????????????????????????
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetMaterialList(ReturnMaterialReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("???????????????", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && req.AppUserId > 0)
            {
                loginUser = await GetUserId((int)req.AppUserId);
            }
            var result = new TableData();
            //??????????????????????????????????????????Id
            var quotationObj = await UnitWork.Find<Quotation>(q => q.SalesOrderId == req.SalesOrderId && q.CreateUserId.Equals(loginUser.Id)).Include(q => q.QuotationProducts).ThenInclude(q => q.QuotationMaterials).Include(q => q.QuotationMergeMaterials).Where(q => q.QuotationProducts.Any(p => p.ProductCode.Equals(req.ProductCode))).FirstOrDefaultAsync();
            var quotationProductObj = quotationObj.QuotationProducts.Where(c => c.ProductCode == req.ProductCode).FirstOrDefault();
            var quotationMergeMaterials = quotationObj.QuotationMergeMaterials.ToList();
            //??????????????????  && s.LineStatus == "O"
            var saleinv1s = await UnitWork.Find<sale_inv1>(s => s.DocEntry == req.InvoiceDocEntry).ToListAsync();
            //????????????????????????
            //var returnNoteProducts = await UnitWork.Find<ReturnNoteProduct>(r => r.ProductCode.Equals(req.ProductCode)).Include(r => r.ReturnNoteMaterials).Where(r => r.ReturnNoteMaterials.Any(m => req.InvoiceDocEntry == m.InvoiceDocEntry)).ToListAsync();
            List<ReturnNoteMaterial> materials = new List<ReturnNoteMaterial>();
            //returnNoteProducts.ForEach(r =>
            //{
            //    materials.AddRange(r.ReturnNoteMaterials.ToList());
            //});
            List<sale_inv1> saleinv1List = new List<sale_inv1>();
            //saleinv1s.ForEach(s =>
            //{
            //    s.Quantity = s.Quantity - materials.Where(m => m.ReplaceMaterialCode.Equals(s.ItemCode)).Count();
            //    var quotationMaterialCount = quotationProductObj.QuotationMaterials.Where(q => q.MaterialCode.Equals(s.ItemCode) && Convert.ToDecimal(q.DiscountPrices).ToString("#0.00") == Convert.ToDecimal(s.Price).ToString("#0.00")).Count() - materials.Where(m => m.ReplaceMaterialCode.Equals(s.ItemCode)).Count();
            //    if (quotationMaterialCount > 0 && s.Quantity > 0)
            //    {
            //        var num = s.Quantity;
            //        if (quotationMaterialCount < s.Quantity)
            //        {
            //            num = quotationMaterialCount;
            //        }
            //        for (int i = 0; i < num; i++)
            //        {
            //            s.Quantity = 1;
            //            saleinv1List.Add(s);
            //        }
            //    }
            //});
            var productMaterials = quotationProductObj.QuotationMaterials.ToList();
            var replaceRecord = await UnitWork.Find<MaterialReplaceRecord>(c => c.QuotationId == quotationObj.Id && c.ProductCode == req.ProductCode).ToListAsync();
            List<ReturnMaterialListResp> listResps = new List<ReturnMaterialListResp>();
            saleinv1s.ForEach(s =>
            {
                //?????????????????????
                //var productMaterialsCount = productMaterials.Where(p => p.MaterialCode == s.ItemCode && Convert.ToDecimal(p.DiscountPrices).ToString("#0.00") == Convert.ToDecimal(s.Price).ToString("#0.00")).FirstOrDefault()?.Count;
                //?????????????????????
                var hasCount = materials.Where(m => m.ReplaceMaterialCode.Equals(s.ItemCode)).Count();
                //??????????????????
                //productMaterialsCount = productMaterialsCount - hasCount;
                //s.Quantity = s.Quantity - hasCount;
                s.Quantity = s.Quantity - hasCount;//?????????????????????????????????
                var quotationMaterialCount = quotationProductObj.QuotationMaterials.Where(q => q.MaterialCode.Equals(s.ItemCode) && Convert.ToDecimal(q.DiscountPrices).ToString("#0.00") == Convert.ToDecimal(s.Price).ToString("#0.00")).FirstOrDefault()?.Count ?? 0 - materials.Where(m => m.ReplaceMaterialCode.Equals(s.ItemCode)).Count();//??????????????????????????????
                var sort = 0;
                if (productMaterials.Any(c => c.MaterialCode == s.ItemCode))//?????????????????????????????????????????????????????????
                {
                    if (s.Quantity > 0 && quotationMaterialCount > 0)
                    {
                        var num = s.Quantity;
                        if (quotationMaterialCount < s.Quantity)
                        {
                            num = quotationMaterialCount;//?????????????????????????????????????????????
                        }
                        for (int i = 1; i <= num; i++)
                        {
                            var linenum = hasCount + i;
                            listResps.Add(new ReturnMaterialListResp
                            {
                                Sort = sort + i,
                                LineNum = linenum,
                                MaterialType = quotationMergeMaterials.Where(q => q.MaterialCode.Equals(s.ItemCode) && Convert.ToDecimal(q.DiscountPrices).ToString("#0.00") == Convert.ToDecimal(s.Price).ToString("#0.00")).FirstOrDefault()?.MaterialType,
                                //???????????????????????????
                                MaterialCode = replaceRecord.Where(r => r.MaterialCode == s.ItemCode && r.LineNum == linenum).FirstOrDefault() == null ? "" : replaceRecord.Where(r => r.MaterialCode == s.ItemCode && r.LineNum == linenum).FirstOrDefault().ReplaceMaterialCode,
                                MaterialDescription = replaceRecord.Where(r => r.MaterialCode == s.ItemCode && r.LineNum == linenum).FirstOrDefault() == null ? "" : replaceRecord.Where(r => r.MaterialCode == s.ItemCode && r.LineNum == linenum).FirstOrDefault().ReplaceMaterialDescription,
                                Money = Convert.ToDecimal(s.Price),
                                QuotationMaterialId = quotationMergeMaterials.Where(q => q.MaterialCode.Equals(s.ItemCode) && Convert.ToDecimal(q.DiscountPrices).ToString("#0.00") == Convert.ToDecimal(s.Price).ToString("#0.00")).FirstOrDefault()?.Id,
                                SNandPN = replaceRecord.Where(r => r.MaterialCode == s.ItemCode && r.LineNum == linenum).FirstOrDefault() == null ? "" : replaceRecord.Where(r => r.MaterialCode == s.ItemCode && r.LineNum == linenum).FirstOrDefault().ReplaceSNandPN,
                                ReplaceSNandPN = replaceRecord.Where(r => r.MaterialCode == s.ItemCode && r.LineNum == linenum).FirstOrDefault() == null ? "" : replaceRecord.Where(r => r.MaterialCode == s.ItemCode && r.LineNum == linenum).FirstOrDefault().SNandPN,
                                ReplaceMaterialCode = s.ItemCode,
                                ReplaceMaterialDescription = s.Dscription
                            });
                            //s.Quantity = 1;
                            //saleinv1List.Add(s);
                        }
                    }
                }
            });
            listResps = listResps.Where(c => c.MaterialType != 3).ToList();//?????????????????????????????????
            result.Data = listResps;
            result.Count = listResps.Count();
            //result.Data = saleinv1List.Select(s => new ReturnMaterialListResp
            //{
            //    MaterialCode = replaceRecord.Where(r => r.MaterialCode == s.ItemCode).FirstOrDefault()?.ReplaceMaterialCode,
            //    MaterialDescription = replaceRecord.Where(r => r.MaterialCode == s.ItemCode).FirstOrDefault()?.ReplaceMaterialDescription,
            //    Money = Convert.ToDecimal(s.Price),
            //    QuotationMaterialId = quotationMergeMaterials.Where(q => q.MaterialCode.Equals(s.ItemCode) && Convert.ToDecimal(q.DiscountPrices).ToString("#0.00") == Convert.ToDecimal(s.Price).ToString("#0.00")).FirstOrDefault()?.Id,
            //    SNandPN = replaceRecord.Where(r => r.MaterialCode == s.ItemCode).FirstOrDefault()?.ReplaceSNandPN,
            //    ReplaceSNandPN = replaceRecord.Where(r => r.MaterialCode == s.ItemCode).FirstOrDefault()?.SNandPN,
            //    ReplaceMaterialCode = s.ItemCode,
            //    ReplaceMaterialDescription = s.Dscription
            //}).ToList();
            return result;
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetQuotationList(ReturnMaterialReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("???????????????", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && req.AppUserId > 0)
            {
                loginUser = await GetUserId((int)req.AppUserId);
            }
            var result = new TableData();

            List<int> ServiceOrderids = new List<int>();
            List<string> quotationIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(req.Customer))
            {
                ServiceOrderids = await UnitWork.Find<ServiceOrder>(c => c.TerminalCustomer.Contains(req.Customer) || c.TerminalCustomerId.Contains(req.Customer)).Select(c => c.Id).ToListAsync();
            }
            if (!string.IsNullOrWhiteSpace(req.ProductCode))
            {
                quotationIds = await UnitWork.Find<QuotationProduct>(c => c.ProductCode.Contains(req.ProductCode)).Select(c => c.QuotationId.ToString()).ToListAsync();
            }

            var quotation = UnitWork.Find<Quotation>(c => c.Status == 2 && c.QuotationStatus != -1)
                                    .Include(c => c.QuotationProducts).Include(c => c.QuotationMergeMaterials).Include(c => c.Expressages)
                                    .WhereIf(!string.IsNullOrWhiteSpace(req.QuotationId.ToString()), c => c.Id == req.QuotationId)
                                    .WhereIf(!string.IsNullOrWhiteSpace(req.CreateUserName), c => c.CreateUser == req.CreateUserName)
                                    .WhereIf(!string.IsNullOrWhiteSpace(req.SapId.ToString()), c => c.ServiceOrderSapId == req.SapId)
                                    .WhereIf(req.SalesOrderId != null, c => c.SalesOrderId == req.SalesOrderId)
                                    .WhereIf(ServiceOrderids.Count > 0, c => ServiceOrderids.Contains(c.ServiceOrderId))
                                    .WhereIf(quotationIds.Count > 0, c => quotationIds.Contains(c.Id.ToString()))
                                    ;

            if (!loginContext.Roles.Any(r => r.Name.Equals("????????????-??????")) && !loginContext.Roles.Any(r => r.Name.Equals("????????????")) && !loginUser.Account.Equals(Define.SYSTEM_USERNAME))
            {
                if (!loginContext.Roles.Any(r => r.Name.Equals("??????")))
                {
                    if (loginContext.Roles.Any(r => r.Name.Equals("????????????")))//????????????????????????????????????????????????
                    {
                        var orgId = loginContext.Orgs.OrderByDescending(c => c.CascadeId).FirstOrDefault()?.Id;
                        var orgUserIds = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.SecondId == orgId && c.Key == Define.USERORG).Select(c => c.FirstId).ToListAsync();
                        quotation = quotation.Where(q => orgUserIds.Contains(q.CreateUserId));
                    }
                    else
                    {
                        quotation = quotation.Where(q => q.CreateUserId.Equals(loginUser.Id));
                    }
                }
                //switch (request.StartType)
                //{
                //    case 1:
                //        flowInstanceIds = flowinstanceList.Where(f => f.a.ActivityName == "?????????").Select(f => f.a.Id).Distinct().ToList();
                //        Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus == 10));
                //        break;
                //    case 2:
                //        flowInstanceIds = flowinstanceList.Where(f => f.a.ActivityName == "??????").Select(f => f.a.Id).Distinct().ToList();
                //        Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus == 11));
                //        break;
                //    default:
                //        flowInstanceIds = flowinstanceList.Where(f => f.a.ActivityName == "?????????" || f.a.ActivityName == "??????").Select(f => f.a.Id).Distinct().ToList();
                //        Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus >= 10));
                //        break;
                //}
                //Quotations = Quotations.Where(q => (q.IsMaterialType != null || q.QuotationStatus == 11));
            }
            result.Count = quotation.Count();
            var quotationObj = await quotation.OrderByDescending(c => c.UpDateTime).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            var serviceOrderIds = quotation.Select(c => c.ServiceOrderId).ToList();
            var ids = quotation.Select(c => c.Id).ToList();

            var materialReplaceRecord = await UnitWork.Find<MaterialReplaceRecord>(c => ids.Contains(c.QuotationId)).ToListAsync();

            var serviceOrder = await UnitWork.Find<ServiceOrder>(c => serviceOrderIds.Contains(c.Id))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.Customer),c=>c.TerminalCustomer.Contains(req.Customer) || c.TerminalCustomerId.Contains(req.Customer))
                            .Select(c => new { c.Id, c.TerminalCustomer, c.TerminalCustomerId })
                            .ToListAsync();

            var query = from a in quotationObj
                        join b in serviceOrder on a.ServiceOrderId equals b.Id
                        select new { a, b };
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ShieldingMaterials")).Select(u => u.Name).ToListAsync();

            result.Data = query.Select(c => new
            {
                c.a.Id,
                c.a.ServiceOrderId,
                c.a.ServiceOrderSapId,
                c.a.SalesOrderId,
                c.b.TerminalCustomerId,
                c.b.TerminalCustomer,
                c.a.TotalMoney,
                c.a.AcquisitionWay,
                Expressages = c.a.Expressages,
                DeviceNum = c.a.QuotationProducts.Count(),
                c.a.CreateTime,
                c.a.CreateUser,
                //modify by yangsiming @2022.04.14 MaterialType=3,???????????????????????????????????????
                Status = materialReplaceRecord.Where(m => m.QuotationId == c.a.Id).Count() > 0 ? (materialReplaceRecord.Where(m => m.QuotationId == c.a.Id && m.MaterialType != 3).Count() == c.a.QuotationMergeMaterials.Where(c => c.MaterialType != 3 && !CategoryList.Contains(c.MaterialCode)).Sum(s => s.Count) ? "?????????" : "????????????") : "?????????"
            });
            return result;
        }

        /// <summary>
        /// ????????????????????????
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task CreateMaterialReplaceRecord(AddOrUpdateMaterialReplaceRecordReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("???????????????", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var loginOrg = loginContext.Orgs;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppUserId));
                loginOrg = await GetOrgs(loginUser.Id);
            }

            var materialObj = obj.MaterialReplaceRecordReqs.MapToList<MaterialReplaceRecord>();
            //?????????????????????????????????
            var quotationId = obj.MaterialReplaceRecordReqs.Where(m => m != null).Select(x => x.QuotationId).FirstOrDefault();
            await UnitWork.DeleteAsync<MaterialReplaceRecord>(m => m.QuotationId == quotationId);
            await UnitWork.BatchAddAsync(materialObj.ToArray());
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// ???????????????????????????????????????
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetMaterialListBySerialNumber(ReturnMaterialReq req)
        {
            var result = new TableData();
            var quotationProducts = await UnitWork.Find<QuotationProduct>(c => c.QuotationId == req.QuotationId && c.ProductCode == req.ProductCode).Include(c => c.QuotationMaterials).FirstOrDefaultAsync();
            var replacedMaterials = await UnitWork.Find<MaterialReplaceRecord>(c => c.QuotationId == req.QuotationId).ToListAsync();
            List<MaterialReplaceRecordReq> materials = new List<MaterialReplaceRecordReq>();
            quotationProducts.QuotationMaterials.ForEach(c =>
            {
                for (int i = 0; i < c.Count; i++)
                {
                    materials.Add(new MaterialReplaceRecordReq
                    {
                        LineNum = (i + 1),
                        MaterialType = c.MaterialType,
                        MaterialCode = c.MaterialCode,
                        MaterialDescription = c.MaterialDescription,
                        SNandPN = replacedMaterials.Where(r => r.ProductCode == quotationProducts.ProductCode && r.MaterialCode == c.MaterialCode && r.LineNum == (i + 1)).FirstOrDefault()?.SNandPN,
                        ReplaceMaterialCode = replacedMaterials.Where(r => r.ProductCode == quotationProducts.ProductCode && r.MaterialCode == c.MaterialCode && r.LineNum == (i + 1)).FirstOrDefault()?.ReplaceMaterialCode,
                        ReplaceMaterialDescription = replacedMaterials.Where(r => r.ProductCode == quotationProducts.ProductCode && r.MaterialCode == c.MaterialCode && r.LineNum == (i + 1)).FirstOrDefault()?.ReplaceMaterialDescription,
                        ReplaceSNandPN = replacedMaterials.Where(r => r.ProductCode == quotationProducts.ProductCode && r.MaterialCode == c.MaterialCode && r.LineNum == (i + 1)).FirstOrDefault()?.ReplaceSNandPN,
                        Count = c.UnitPrice,
                        Status = c.MaterialType == 3 ? "??????" : !string.IsNullOrWhiteSpace(replacedMaterials.Where(r => r.ProductCode == quotationProducts.ProductCode && r.MaterialCode == c.MaterialCode && r.LineNum == (i + 1)).FirstOrDefault()?.SNandPN) ? "?????????" : "?????????"
                    });
                }
            });
            result.Data = new
            {
                quotationProducts.ProductCode,
                quotationProducts.MaterialCode,
                quotationProducts.MaterialDescription,
                MaterialsData = materials
            };
            return result;
        }

        /// <summary>
        /// ????????????????????????
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetDetails(ReturnMaterialReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("???????????????", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && req.AppUserId > 0)
            {
                loginUser = await GetUserId((int)req.AppUserId);
            }
            //.Include(r => r.ReturnnoteOperationHistorys)
            var returnNotes = await UnitWork.Find<ReturnNote>(r => r.Id == req.returnNoteId).Include(r => r.ReturnNotePictures).Include(r => r.ReturnNoteProducts).ThenInclude(r => r.ReturnNoteMaterials).ThenInclude(r => r.ReturnNoteMaterialPictures).FirstOrDefaultAsync();
            var createrOrgInfo = await _userManagerApp.GetUserOrgInfo(returnNotes.CreateUserId);
            returnNotes.OrgName = createrOrgInfo != null ? createrOrgInfo.OrgName : "";

            var History = await UnitWork.Find<FlowInstanceOperationHistory>(f => f.InstanceId.Equals(returnNotes.FlowInstanceId)).OrderBy(f => f.CreateDate).ToListAsync();
            List<ReturnNoteMaterial> returnnoteMaterials = new List<ReturnNoteMaterial>();
            returnNotes.ReturnNoteProducts.ForEach(r => { returnnoteMaterials.AddRange(r.ReturnNoteMaterials); });
            //??????????????????????????????????????????Id
            var quotationObj = await UnitWork.Find<Quotation>(q => q.SalesOrderId == returnNotes.SalesOrderId).Include(q => q.QuotationMergeMaterials).FirstOrDefaultAsync();
            var InvoiceDocEntry = returnnoteMaterials.Select(r => r.InvoiceDocEntry).FirstOrDefault();
            var result = new TableData();
            //var serviceOrders = await UnitWork.Find<ServiceOrder>(s => s.Id == returnNotes.ServiceOrderId).Select(s => new { s.Id, s.TerminalCustomer, s.TerminalCustomerId, s.NewestContacter, s.NewestContactTel, s.U_SAP_ID }).FirstOrDefaultAsync();
            //??????????????????  && s.LineStatus == "O"
            var saleinv1 = await UnitWork.Find<sale_inv1>(s => s.DocEntry == InvoiceDocEntry && s.sbo_id == Define.SBO_ID).ToListAsync();
            var DocTotal = saleinv1.Sum(s => s.LineTotal);
            //????????????????????????
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
                    //List<ReturnMaterialListResp> MaterialList = (await GetMaterialList(new ReturnMaterialReq { InvoiceDocEntry = item.ReturnNoteMaterials.FirstOrDefault()?.InvoiceDocEntry, ProductCode = item.ProductCode, SalesOrderId = returnNotes.SalesOrderId })).Data;
                    //var returnNoteMaterials = MaterialList.Select(m => new ReturnNoteMaterial
                    //{
                    //    //Sort = m.Sort,
                    //    LineNum = m.LineNum,
                    //    MaterialType = m.MaterialType,
                    //    ReplaceSNandPN = m.ReplaceSNandPN,
                    //    SNandPN = m.SNandPN,
                    //    Money = m.Money,
                    //    ReplaceMaterialCode = m.ReplaceMaterialCode,
                    //    ReplaceMaterialDescription = m.ReplaceMaterialDescription,
                    //    MaterialCode = m.MaterialCode,
                    //    MaterialDescription = m.MaterialDescription,
                    //    QuotationMaterialId = m.QuotationMaterialId
                    //}).ToList();
                    //returnNotes.ReturnNoteProducts.Where(r => r.Id.Equals(item.Id)).FirstOrDefault().ReturnNoteMaterials.AddRange(returnNoteMaterials);
                    var hasMaterials = returnNotes.ReturnNoteProducts.Where(r => r.Id.Equals(item.Id)).FirstOrDefault().ReturnNoteMaterials;
                    //hasMaterials = hasMaterials.Concat(returnNoteMaterials).OrderBy(c => c.ReplaceMaterialCode).ToList();
                    hasMaterials = hasMaterials.OrderBy(c => c.ReplaceMaterialCode).ToList();
                    var sort = 0;
                    var groupbyMaterials = hasMaterials.GroupBy(c => c.ReplaceMaterialCode).Select(c => new { c.Key, Item = c.Select(i=>i).ToList() }).ToList();
                    groupbyMaterials.ForEach(g =>
                    {
                        sort = 0;
                        g.Item.ForEach(f =>
                        {
                            f.Sort = ++sort;
                        });
                    });
                    returnNotes.ReturnNoteProducts.Where(r => r.Id.Equals(item.Id)).FirstOrDefault().ReturnNoteMaterials = hasMaterials;
                    //returnNotes.ReturnNoteProducts.Where(r => r.Id.Equals(item.Id)).FirstOrDefault().ReturnNoteMaterials.AddRange(returnNoteMaterials);
                }
            }
            List<FlowPathResp> flowPathResp = new List<FlowPathResp>();
            if (!string.IsNullOrWhiteSpace(returnNotes.FlowInstanceId))
            {
                flowPathResp = await _flowInstanceApp.FlowPathRespList(returnnoteOperationHistorys, returnNotes.FlowInstanceId);
            }
            var qoutationReq = await _pending.QuotationDetails(quotationObj.Id);
            var serviceOrders = await _pending.ServiceOrderDetails(returnNotes.ServiceOrderId, returnNotes.CreateUserId);
            //???????????????????????????
            var salesManOrgInfo = await _userManagerApp.GetUserOrgInfo(serviceOrders.SalesManId);
            serviceOrders.SalesManDept = salesManOrgInfo != null ? salesManOrgInfo.OrgName : "";
            var superVisorOrgInfo = await _userManagerApp.GetUserOrgInfo(serviceOrders.SupervisorId);
            serviceOrders.SupervisorDept = superVisorOrgInfo != null ? superVisorOrgInfo.OrgName : "";

            var status = flowInstanceObj?.IsFinish == FlowInstanceStatus.Rejected ? "??????" : flowInstanceObj?.ActivityName == null ? "??????" : flowInstanceObj?.ActivityName;
            var isPermission = IsPermission(status);
            returnNotes.ReturnNoteProducts.ForEach(c => {
                c.ReturnNoteMaterials = c.ReturnNoteMaterials.OrderBy(c => c.ReplaceMaterialCode).ThenBy(c => c.Sort).ToList();
            });

            var orgrole = await _orgApp.GetOrgNameAndRoleIdentity(returnNotes.CreateUserId);

            result.Data = new
            {
                InvoiceDocEntry,
                DocTotal = DocTotal,
                returnNoteId = returnNotes.Id,
                Status= status,
                RoleIdentity = orgrole.RoleIdentity,
                IsPermission = isPermission,
                returnNotes,
                serviceOrders,
                flowPathResp,
                returnnoteOperationHistorys,
                Quotations = qoutationReq
            };
            return result;
        }

        /// <summary>
        /// ???????????????????????????
        /// </summary>
        /// <param name="Status"></param>
        /// <returns></returns>
        private bool IsPermission(string Status)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("???????????????", Define.INVALID_TOKEN);
            }
            switch (Status)
            {
                case "????????????":
                    if (loginContext.Roles.Any(r => r.Name.Equals("????????????")))
                    {
                        return true;
                    }
                    break;
                case "????????????":
                    if (loginContext.Roles.Any(r => r.Name.Equals("????????????")))
                    {
                        return true;
                    }
                    break;
                case "???????????????":
                    if (loginContext.Roles.Any(r => r.Name.Equals("?????????")) || loginContext.Roles.Any(r => r.Name.Equals("?????????")))
                    {
                        return true;
                    }
                    break;
                case "????????????":
                    if (loginContext.Roles.Any(r => r.Name.Equals("??????")))
                    {
                        return true;
                    }
                    break;
                default:
                    break;
            }
            return false;
        }
        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task Add(AddOrUpdateReturnnoteReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("???????????????", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var loginOrg = loginContext.Orgs;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppUserId));
                loginOrg = await GetOrgs(loginUser.Id);
            }
            var catetory = await UnitWork.Find<Category>(c => c.TypeId == "SYS_ReturnAllowXLH").Select(c => c.DtValue).ToListAsync();
            obj.ReturnNoteProducts.ForEach(c =>
            {
                //modify by yangsiming @2022/3/23 ???????????????????????????????????????,?????????????????????????????????????????????,?????????????????????,???????????????????????????
                var planCount = GetMaterialList(new ReturnMaterialReq { SalesOrderId = obj.SalesOrderId, InvoiceDocEntry = obj.InvoiceDocEntry, ProductCode = c.ProductCode }).Result.Count;
                var realCount = c.ReturnNoteMaterials?.Count();
                if (planCount != realCount && !catetory.Contains(c.ProductCode))
                {
                    throw new Exception($"?????????{c.ProductCode},?????????????????????");
                }

                var sort = 0;
                var hasMaterials = c.ReturnNoteMaterials.OrderBy(c => c.ReplaceMaterialCode).ToList();
                var groupbyMaterials = hasMaterials.GroupBy(c => c.ReplaceMaterialCode).Select(c => new { c.Key, Item = c.Select(i => i).ToList() }).ToList();
                groupbyMaterials.ForEach(g =>
                {
                    sort = 0;
                    var first = g.Item.FirstOrDefault()?.MaterialCode;
                    if (!g.Item.All(c => c.MaterialCode == first))
                    {
                        throw new Exception($"??????{g.Key}?????????????????????????????????????????????????????????????????????");
                    }
                    //??????????????????
                    g.Item.ForEach(f =>
                    {
                        f.Sort = ++sort;
                    });
                });
                c.ReturnNoteMaterials = hasMaterials;
            });
            //????????????????????????
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
                    //??????????????????
                    //returnnotrObj.ReturnNoteProducts.ForEach(c =>
                    //{
                    //    var sort = 0;
                    //    var hasMaterials = c.ReturnNoteMaterials.OrderBy(c => c.ReplaceMaterialCode).ToList();
                    //    var groupbyMaterials = hasMaterials.GroupBy(c => c.ReplaceMaterialCode).Select(c => new { Item = c.Select(i => i).ToList() }).ToList();
                    //    groupbyMaterials.ForEach(g =>
                    //    {
                    //        sort = 0;
                    //        g.Item.ForEach(f =>
                    //        {
                    //            f.Sort = ++sort;
                    //        });
                    //    });
                    //    c.ReturnNoteMaterials = hasMaterials;
                    //});
                    returnnotrObj = await UnitWork.AddAsync<ReturnNote, int>(returnnotrObj);
                    await UnitWork.SaveAsync();
                    if (!obj.IsDraft)
                    {
                        //??????????????????
                        var mf = _moduleFlowSchemeApp.Get(m => m.Module.Name.Equals("???????????????"));
                        var afir = new AddFlowInstanceReq();
                        afir.SchemeId = mf.FlowSchemeId;
                        afir.FrmType = 2;
                        afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                        afir.CustomName = $"?????????";
                        afir.FrmData = "{\"ReturnnoteId\":\"" + returnnotrObj.Id + "\"}";
                        afir.OrgId = loginOrg.FirstOrDefault()?.Id;
                        var FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                        await UnitWork.UpdateAsync<ReturnNote>(r => r.Id == returnnotrObj.Id, r => new ReturnNote { FlowInstanceId = FlowInstanceId });
                        //?????????????????????
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
                    #region ?????????????????? ?????????
                    var quotationObj = await UnitWork.Find<Quotation>(c => c.SalesOrderId == obj.SalesOrderId).Include(c => c.QuotationProducts).ThenInclude(c => c.QuotationMaterials).FirstOrDefaultAsync();
                    var productCode = obj.ReturnNoteProducts.Select(c => c.ProductCode).ToList();
                    var replaceRecord = await UnitWork.Find<MaterialReplaceRecord>(c => c.QuotationId == quotationObj.Id && productCode.Contains(c.ProductCode)).ToListAsync();
                    List<MaterialReplaceRecord> materialReplaces = new List<MaterialReplaceRecord>();
                    obj.ReturnNoteProducts.ForEach(c =>
                    {
                        c.ReturnNoteMaterials.ForEach(m =>
                        {
                            var item = replaceRecord.Where(r => r.ProductCode == c.ProductCode && r.MaterialCode == m.ReplaceMaterialCode && r.LineNum == m.LineNum).FirstOrDefault();
                            if (item != null)
                            {
                                item.ReplaceMaterialCode = m.MaterialCode;
                                item.ReplaceMaterialDescription = m.MaterialDescription;
                                item.ReplaceSNandPN = m.SNandPN;
                                item.SNandPN = m.ReplaceSNandPN;
                                materialReplaces.Add(item);
                            }
                        });
                    });
                    if (materialReplaces.Count > 0)
                    {
                        await UnitWork.BatchUpdateAsync(materialReplaces.ToArray());
                    }
                    #endregion
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
                    throw new Exception("?????????????????????????????????" + ex.Message);
                }
            }

        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task Update(AddOrUpdateReturnnoteReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("???????????????", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var loginOrg = loginContext.Orgs;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppUserId));
                loginOrg = await GetOrgs(loginUser.Id);
            }
            obj.ReturnNoteProducts.ForEach(c =>
            {
                //modify by yangsiming @2022/3/23 ???????????????????????????????????????,?????????????????????????????????????????????,?????????????????????,???????????????????????????
                var planCount = GetMaterialList(new ReturnMaterialReq { SalesOrderId = obj.SalesOrderId, InvoiceDocEntry = obj.InvoiceDocEntry, ProductCode = c.ProductCode }).Result.Count;
                var realCount = c.ReturnNoteMaterials?.Count();
                if (planCount != realCount)
                {
                    throw new Exception($"?????????{c.ProductCode},?????????????????????");
                }

                var sort = 0;
                var hasMaterials = c.ReturnNoteMaterials.OrderBy(c => c.ReplaceMaterialCode).ToList();
                var groupbyMaterials = hasMaterials.GroupBy(c => c.ReplaceMaterialCode).Select(c => new { c.Key, Item = c.Select(i => i).ToList() }).ToList();
                groupbyMaterials.ForEach(g =>
                {
                    sort = 0;
                    var first = g.Item.FirstOrDefault()?.MaterialCode;
                    if (!g.Item.All(c => c.MaterialCode == first))
                    {
                        throw new Exception($"??????{g.Key}?????????????????????????????????????????????????????????????????????");
                    }
                    //??????????????????
                    g.Item.ForEach(f =>
                    {
                        f.Sort = ++sort;
                    });
                });
                c.ReturnNoteMaterials = hasMaterials;
            });

            var dbContext = UnitWork.GetDbContext<ReturnNote>();
            //??????
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //????????????
                    #region ??????
                    var returnNoteObj = await UnitWork.Find<ReturnNote>(r => r.Id == obj.ReturnNoteId).Include(r => r.ReturnNotePictures).FirstOrDefaultAsync();
                    await UnitWork.DeleteAsync<ReturnNoteProduct>(r => r.ReturnNoteId == returnNoteObj.Id);
                    if (returnNoteObj.ReturnNotePictures != null && returnNoteObj.ReturnNotePictures.Count > 0)
                    {
                        await UnitWork.BatchDeleteAsync<ReturnNotePicture>(returnNoteObj.ReturnNotePictures.ToArray());
                    }
                    await UnitWork.SaveAsync();
                    #endregion
                    #region ??????
                    obj.ReturnNoteProducts.ForEach(r => { r.ReturnNoteId = obj.ReturnNoteId; r.ReturnNoteMaterials.ForEach(m => m.ReturnNoteId = obj.ReturnNoteId); });
                    await UnitWork.BatchAddAsync<ReturnNoteProduct>(obj.ReturnNoteProducts.ToArray());
                    obj.ReturnNotePictures.ForEach(r => r.ReturnNoteId = obj.ReturnNoteId);
                    await UnitWork.BatchAddAsync<ReturnNotePicture>(obj.ReturnNotePictures.ToArray());
                    await UnitWork.SaveAsync();
                    #endregion
                    //var returnnotrStatus = 0;
                    var FlowInstanceId = "";
                    if (!obj.IsDraft)
                    {
                        //returnnotrStatus = 3;//?????????
                        if (string.IsNullOrWhiteSpace(returnNoteObj.FlowInstanceId))
                        {
                            //??????????????????
                            var mf = _moduleFlowSchemeApp.Get(m => m.Module.Name.Equals("???????????????"));
                            var afir = new AddFlowInstanceReq();
                            afir.SchemeId = mf.FlowSchemeId;
                            afir.FrmType = 2;
                            afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                            afir.CustomName = $"?????????";
                            afir.FrmData = "{\"ReturnnoteId\":\"" + obj.ReturnNoteId + "\"}";
                            afir.OrgId = loginOrg.FirstOrDefault()?.Id;
                            FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                        }
                        else
                        {
                            FlowInstanceId = returnNoteObj.FlowInstanceId;
                            await _flowInstanceApp.Start(new StartFlowInstanceReq() { FlowInstanceId = returnNoteObj.FlowInstanceId });
                        }
                        //?????????????????????
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
                    throw new Exception("?????????????????????????????????" + ex.Message);
                }
            }
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="returnNoteId"></param>
        /// <returns></returns>
        public async Task Delete(int returnNoteId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("???????????????", Define.INVALID_TOKEN);
            }
            var returnNoteObj = await UnitWork.Find<ReturnNote>(r => r.Id == returnNoteId).FirstOrDefaultAsync();
            if (returnNoteObj.FlowInstanceId != null)
            {
                var flowInstanceObj = await UnitWork.Find<FlowInstance>(f => f.Id.Equals(returnNoteObj.FlowInstanceId)).FirstOrDefaultAsync();
                if (flowInstanceObj.IsFinish == FlowInstanceStatus.Finished || flowInstanceObj.IsFinish == FlowInstanceStatus.Running)
                {
                    throw new Exception("??????????????????????????????????????????????????????");
                }
                await UnitWork.DeleteAsync<FlowInstance>(flowInstanceObj);
            }

            //????????????????????????
            if (returnNoteObj != null)
            {
                await UnitWork.DeleteAsync<ReturnNote>(returnNoteObj);
                await UnitWork.SaveAsync();
            }
            else
            {
                throw new Exception("?????????????????????????????????");
            }

        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Accraditation(AccraditationReturnNoteReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("???????????????", Define.INVALID_TOKEN);
            }
            var returnNotes = await UnitWork.Find<ReturnNote>(r => r.Id == req.Id).Include(r => r.ReturnNoteProducts).ThenInclude(r => r.ReturnNoteMaterials).FirstOrDefaultAsync();
            if (returnNotes == null)
            {
                throw new Exception("??????????????????????????????");
            }
            var flowInstanceObj = await UnitWork.Find<FlowInstance>(f => f.Id.Equals(returnNotes.FlowInstanceId)).FirstOrDefaultAsync();
            if (loginContext.Roles.Any(r => r.Name.Equals("????????????")) && flowInstanceObj.ActivityName.Equals("????????????"))
            {
                if (!req.IsReject)
                {
                    foreach (var item in req.returnnoteMaterials)
                    {
                        await UnitWork.UpdateAsync<ReturnNoteMaterial>(r => r.Id.Equals(item.MaterialsId), r => new ReturnNoteMaterial { IsGood = item.IsGood });
                    }
                }
            }
            if (loginContext.Roles.Any(r => r.Name.Equals("??????")) && flowInstanceObj.ActivityName.Equals("????????????"))
            {
                if (!req.IsReject)
                {
                    foreach (var item in req.returnnoteMaterials)
                    {
                        if (string.IsNullOrWhiteSpace(item.GoodWhsCode) && string.IsNullOrWhiteSpace(item.SecondWhsCode))
                        {
                            throw new Exception("?????????????????????");
                        }
                        await UnitWork.UpdateAsync<ReturnNoteMaterial>(r => r.Id.Equals(item.MaterialsId), r => new ReturnNoteMaterial { GoodWhsCode = item.GoodWhsCode, SecondWhsCode = item.SecondWhsCode });
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
            //?????????????????????
            await UnitWork.UpdateAsync<WorkbenchPending>(w => w.SourceNumbers == returnNotes.Id && w.OrderType == 2, w => new WorkbenchPending
            {
                UpdateTime = DateTime.Now,
            });
            await UnitWork.SaveAsync();
            if (loginContext.Roles.Any(r => r.Name.Equals("??????")) && flowInstanceObj.ActivityName.Equals("????????????"))
            {
                if (!req.IsReject)
                {
                    await WarehousePutMaterialsIn(req);
                }
            }
        }

        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private async Task<decimal> CalculatePrice(AddOrUpdateReturnnoteReq obj)
        {
            var returnNoteMaterials = obj.ReturnNoteProducts.Select(r => r.ReturnNoteMaterials).ToList();
            decimal TotalMoney = 0;
            obj.ReturnNoteProducts.ForEach(r =>
            {
                r.Money = r.ReturnNoteMaterials.Sum(m => m.Money);
                TotalMoney = r.Money;
            });
            return TotalMoney;
        }

        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="AppId"></param>
        /// <returns></returns>
        private async Task<User> GetUserId(int AppId)
        {
            var userid = UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(AppId)).Select(u => u.UserID).FirstOrDefault();

            return await UnitWork.Find<User>(u => u.Id.Equals(userid)).FirstOrDefaultAsync();
        }

        /// <summary>
        /// ??????????????????
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
        /// ????????????
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task WarehousePutMaterialsIn(AccraditationReturnNoteReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("???????????????", Define.INVALID_TOKEN);
            }
            var returnNoteObj = await UnitWork.Find<ReturnNote>(r => r.Id == req.Id).Include(r => r.ReturnNoteProducts).ThenInclude(r => r.ReturnNoteMaterials).FirstOrDefaultAsync();
            List<ReturnNoteMaterial> returnnoteMaterials = new List<ReturnNoteMaterial>();
            returnNoteObj.ReturnNoteProducts.ForEach(r =>
            {
                returnnoteMaterials.AddRange(r.ReturnNoteMaterials.ToList());
            });
            var returnnoteMaterialList = returnnoteMaterials.GroupBy(r => new { r.MaterialCode, r.MaterialDescription, r.Money, r.IsGood, r.GoodWhsCode, r.SecondWhsCode }).Select(r => new
            {
                r.First().MaterialCode,
                r.First().MaterialDescription,
                r.First().IsGood,
                r.First().InvoiceDocEntry,
                r.First().GoodWhsCode,
                r.First().SecondWhsCode,
                r.First().Money,
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
                        returnMergeMaterialGoodReqs.Add(new QuotationMergeMaterialReq
                        {
                            InventoryQuantity = item.Count,
                            ReturnNoteId = item.ReturnNoteId,
                            WhsCode = item.GoodWhsCode,
                            MaterialCode = item.MaterialCode,
                            MaterialDescription = item.MaterialDescription,
                            Id = item.QuotationMaterialId
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
            //?????????SAP
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

        /// <summary>
        /// ????????????????????????????????????(????????????????????????)??????????????????7????????????
        /// </summary>
        /// <returns></returns>
        public async Task SendUnSubmitMessage()
        {
            //??????????????????????????????????????????
            //????????????????????????,????????????????????????id
            var flowInstanceIds = await (from s in UnitWork.Find<FlowScheme>(null)
                                         join i in UnitWork.Find<FlowInstance>(null)
                                         on s.Id equals i.SchemeId
                                         where s.SchemeName == "???????????????" && i.ActivityName == "??????"
                                         select i.Id).Distinct().ToListAsync();

            //????????????id??????????????????(??????id?????????????????????,??????id????????????????????????,??????????????????)
            var returnNotes = await UnitWork.Find<ReturnNote>(null).Where(r => (flowInstanceIds.Contains(r.FlowInstanceId) || string.IsNullOrWhiteSpace(r.FlowInstanceId))).Select(r => new { r.Id, r.ServiceOrderSapId, r.SalesOrderId, r.CreateUser }).ToListAsync();

            //????????????????????????
            var groupData = returnNotes.GroupBy(r => r.CreateUser);
            //????????????????????????????????????
            foreach(var item in groupData)
            {
                var user = item.Key;
                var message = new StringBuilder();
                foreach (var d in item)
                {
                    var returnNoteId = d.Id;
                    var serviceSapId = d.ServiceOrderSapId;
                    var salesOrderId = d.SalesOrderId;
                    message.AppendLine($"????????????:{returnNoteId}");
                }
                message.AppendLine("?????????7????????????,???????????????.");

                await _hubContext.Clients.User(user).SendAsync("ReceiveMessage", "??????", message.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task SendUnSubmitCount()
        {
            //??????????????????????????????????????????
            //????????????????????????,????????????????????????id
            var flowInstanceIds = await (from s in UnitWork.Find<FlowScheme>(null)
                                         join i in UnitWork.Find<FlowInstance>(null)
                                         on s.Id equals i.SchemeId
                                         where s.SchemeName == "???????????????" && i.ActivityName == "??????"
                                         select i.Id).Distinct().ToListAsync();

            //????????????id??????????????????(??????id?????????????????????,??????id????????????????????????,??????????????????)
            var returnNotes = await UnitWork.Find<ReturnNote>(null).Where(r => (flowInstanceIds.Contains(r.FlowInstanceId) || string.IsNullOrWhiteSpace(r.FlowInstanceId))).Select(r => new { r.Id, r.ServiceOrderSapId, r.SalesOrderId, r.CreateUser }).ToListAsync();

            //????????????????????????
            var groupData = returnNotes.GroupBy(r => r.CreateUser);
            //????????????????????????????????????
            foreach (var item in groupData)
            {
                var user = item.Key;
                await _hubContext.Clients.User(user).SendAsync("ReturnNoteUnSubmitCount", "??????", item.Count());
            }
        }

        public async Task<byte[]> PrintReturnnote(string ReturnnoteId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("???????????????", Define.INVALID_TOKEN);
            }

            var model = await UnitWork.Find<ReturnNote>(c => c.Id == int.Parse(ReturnnoteId)).FirstOrDefaultAsync();
            var serverOrder = await UnitWork.Find<ServiceOrder>(q => q.Id.Equals(model.ServiceOrderId)).FirstOrDefaultAsync();
            var returnNoteMaterials = await UnitWork.Find<ReturnNoteMaterial>(c => c.ReturnNoteId == model.Id).ToListAsync();

            var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "ReturnnoteHeader.html");
            var text = System.IO.File.ReadAllText(url);
            text = text.Replace("@Model.Id", model.Id.ToString());
            text = text.Replace("@Model.SalesOrderId", model.SalesOrderId.ToString());
            text = text.Replace("@Model.CreateTime", DateTime.Now.ToString("yyyy.MM.dd HH:mm"));//model.CreateTime.ToString("yyyy.MM.dd hh:mm")
            text = text.Replace("@Model.CreateUser", model?.CreateUser.ToString());
            text = text.Replace("@Model.QRcode", QRCoderHelper.CreateQRCodeToBase64(model.Id.ToString()));
            text = text.Replace("@Model.CustomerId", serverOrder?.TerminalCustomerId.ToString());
            text = text.Replace("@Model.CustomerName", serverOrder?.TerminalCustomer.ToString());
            text = text.Replace("@Model.NewestContacter", serverOrder?.NewestContacter.ToString());
            text = text.Replace("@Model.NewestContactTel", serverOrder?.NewestContactTel.ToString());
            text = text.Replace("@Model.TotalMoney", model.TotalMoney.ToString("0.00"));
            text = text.Replace("@Model.InvoiceDocEntry", returnNoteMaterials.FirstOrDefault()?.InvoiceDocEntry.ToString());
            var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"ReturnnoteHeader{model.Id}.html");
            System.IO.File.WriteAllText(tempUrl, text, Encoding.Unicode);
            var materials = returnNoteMaterials.GroupBy(c => c.MaterialCode).Select(c => new
            {
                MaterialCode = c.First().MaterialCode,
                MaterialDescription = c.First().MaterialDescription,
                Count = c.Count(),
                Unit = "PCS",
                //WhsCode = c.First().WhsCode
                Good = c.Where(w => w.IsGood == true).Count(),
                Second = c.Where(w => w.IsGood == false).Count()
            });
            var datas = await ExportAllHandler.Exporterpdf(materials, "ReturnnoteList.cshtml", pdf =>
            {
                pdf.IsWriteHtml = true;
                pdf.PaperKind = PaperKind.A4;
                pdf.Orientation = Orientation.Portrait;
                pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                //pdf.FooterSettings = new FooterSettings() { HtmUrl = footerUrl };
            });
            System.IO.File.Delete(tempUrl);
            //System.IO.File.Delete(footerUrl);
            return datas;
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <param name="ReturnnoteId"></param>
        /// <returns></returns>
        public async Task<TableData> GetReturnNoteList(int ReturnnoteId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("???????????????", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            var model = await UnitWork.Find<ReturnNote>(c => c.Id == ReturnnoteId).FirstOrDefaultAsync();
            var returnNoteMaterials = await UnitWork.Find<ReturnNoteMaterial>(c => c.ReturnNoteId == model.Id).ToListAsync();

            var materials = returnNoteMaterials.GroupBy(c => c.MaterialCode).Select(c => new
            {
                MaterialCode = c.First().MaterialCode,
                MaterialDescription = c.First().MaterialDescription,
                Count = c.Count(),
                Unit = "PCS",
                //WhsCode = c.First().WhsCode
                Good = c.Where(w => w.IsGood == true).Count(),
                Second = c.Where(w => w.IsGood == false).Count()
            });
            result.Data = materials;
            return result;
        }
    }
}