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

namespace OpenAuth.App
{
    public class ReturnNoteApp : OnlyUnitWorkBaeApp
    {
        private readonly FlowInstanceApp _flowInstanceApp;
        private readonly ExpressageApp _expressageApp;
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        private ICapPublisher _capBus;
        private readonly QuotationApp _quotation;

        public ReturnNoteApp(IUnitWork unitWork, FlowInstanceApp flowInstanceApp, QuotationApp quotation, ModuleFlowSchemeApp moduleFlowSchemeApp, ExpressageApp expressageApp, IAuth auth, ICapPublisher capBus) : base(unitWork, auth)
        {
            _flowInstanceApp = flowInstanceApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _expressageApp = expressageApp;
            _capBus = capBus;
            _quotation = quotation;
        }

        #region App退料
        ///// <summary>
        ///// 退料
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //public async Task ReturnMaterials(ReturnMaterialReq req)
        //{
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    string userId = loginContext.User.Id;
        //    string userName = loginContext.User.Name;
        //    //获取当前用户nsap用户信息
        //    if (req.AppUserId > 0)
        //    {
        //        var userInfo = (await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.AppUserId).Include(a => a.User).FirstOrDefaultAsync())?.User;
        //        if (userInfo == null)
        //        {
        //            throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
        //        }
        //        userId = userInfo.Id;
        //        userName = userInfo.Name;
        //    }
        //    //判断是否已存在最后一次退料
        //    var isExist = (await UnitWork.Find<ReturnNote>(w => w.ServiceOrderId == req.ServiceOrderId && w.IsLast == 1 && w.CreateUserId == userId).ToListAsync()).Count > 0 ? true : false;
        //    if (isExist)
        //    {
        //        throw new CommonException("已完成退料，无法继续退料", Define.IS_Return_Finish);
        //    }
        //    //计算本次退料单 金额总和
        //    var totalAmt = await UnitWork.Find<Quotation>(w => req.StockOutIds.Contains(w.Id)).SumAsync(s => s.TotalMoney);
        //    int returnNoteId = 0;
        //    //判断是否已有退料单 若有则不新增
        //    var returnNoteInfo = await UnitWork.Find<ReturnNote>(w => w.CreateUserId == userId && w.ServiceOrderSapId == req.SapId).FirstOrDefaultAsync();
        //    if (returnNoteInfo == null)
        //    {
        //        //1.新增退料单主表
        //        var newNoteInfo = new ReturnNote { ServiceOrderId = req.ServiceOrderId, ServiceOrderSapId = req.SapId, Status = 1, CreateTime = DateTime.Now, CreateUserId = userId, CreateUser = userName, StockOutId = string.Join(",", req.StockOutIds), TotalMoney = totalAmt };
        //        await UnitWork.AddAsync<ReturnNote, int>(newNoteInfo);
        //        await UnitWork.SaveAsync();
        //        returnNoteId = newNoteInfo.Id;
        //    }
        //    else
        //    {
        //        returnNoteId = returnNoteInfo.Id;
        //    }
        //    await UnitWork.SaveAsync();
        //    //2.添加退料明细信息
        //    var querySum = from a in UnitWork.Find<ReturnnoteMaterial>(null)
        //                   join b in UnitWork.Find<ReturnNote>(null) on a.ReturnNoteId equals b.Id into ab
        //                   from b in ab.DefaultIfEmpty()
        //                   where b.ServiceOrderId == req.ServiceOrderId && a.Count > 0
        //                   select new { a.QuotationMaterialId, a.Count };
        //    var returnMaterials = (await querySum.ToListAsync()).GroupBy(g => g.QuotationMaterialId).Select(s => new { Qty = s.Sum(s => s.Count), Id = s.Key }).ToList();
        //    List<string> detaiList = new List<string>();
        //    foreach (ReturnMaterialDetail item in req.ReturnMaterialDetail)
        //    {
        //        var newDetailInfo = new ReturnnoteMaterial
        //        {
        //            ReturnNoteId = returnNoteId,
        //            MaterialCode = item.MaterialCode,
        //            MaterialDescription = item.MaterialDescription,
        //            Count = item.ReturnQty,
        //            TotalCount = item.TotalQty,
        //            Check = item.ReturnQty > 0 ? 0 : 1,
        //            CostPrice = item.CostPrice,
        //            QuotationMaterialId = item.QuotationMaterialId,
        //            IsGoodFinish = 0,
        //            IsSecondFinish = 0,
        //            DiscountPrices = item.DiscountPrices
        //        };
        //        var detail = await UnitWork.AddAsync<ReturnnoteMaterial, int>(newDetailInfo);
        //        await UnitWork.SaveAsync();
        //        detaiList.Add(detail.Id);
        //        //3.添加退料物料图片
        //        if (!string.IsNullOrEmpty(item.PictureId))
        //        {
        //            var newPictureInfo = new ReturnNoteMaterialPicture { ReturnnoteMaterialId = detail.Id, PictureId = item.PictureId };
        //            await UnitWork.AddAsync(newPictureInfo);
        //            await UnitWork.SaveAsync();
        //        }
        //        int everQty = (int)(returnMaterials.Where(w => w.Id == item.QuotationMaterialId).FirstOrDefault() == null ? 0 : returnMaterials.Where(w => w.Id == item.QuotationMaterialId).FirstOrDefault()?.Qty);
        //        item.SurplusQty = item.TotalQty - everQty - item.ReturnQty;
        //    }
        //    //4.添加物流信息
        //    string expressId = string.Empty;
        //    if (!string.IsNullOrEmpty(req.TrackNumber))
        //    {
        //        var result = await _expressageApp.GetExpressInfo(req.TrackNumber);
        //        if (result.Code == 200)
        //        {
        //            var response = (string)result.Data;
        //            var expressageInfo = await UnitWork.Find<Expressage>(w => w.ReturnNoteId == returnNoteId && w.ExpressNumber == req.TrackNumber).FirstOrDefaultAsync();
        //            if (expressageInfo == null)
        //            {
        //                var express = new Expressage
        //                {
        //                    QuotationId = null,
        //                    ReturnNoteId = returnNoteId,
        //                    ExpressNumber = req.TrackNumber,
        //                    ExpressInformation = response,
        //                    Freight = req.Freight
        //                };
        //                await UnitWork.AddAsync(express);
        //                expressId = express.Id;
        //            }
        //            else
        //            {
        //                expressId = expressageInfo.Id;
        //                await UnitWork.UpdateAsync<Expressage>(w => w.Id == expressageInfo.Id, u => new Expressage { ExpressInformation = response });
        //            }
        //        }
        //        else
        //        {
        //            var express = new Expressage
        //            {
        //                QuotationId = null,
        //                ReturnNoteId = returnNoteId,
        //                ExpressNumber = req.TrackNumber,
        //                ExpressInformation = string.Empty,
        //                CreateTime = DateTime.Now,
        //                Freight = req.Freight
        //            };
        //            await UnitWork.AddAsync(express);
        //            expressId = express.Id;
        //        }
        //    }
        //    else
        //    {
        //        var express = new Expressage
        //        {
        //            QuotationId = null,
        //            ReturnNoteId = returnNoteId,
        //            ExpressNumber = "无",
        //            ExpressInformation = string.Empty,
        //            CreateTime = DateTime.Now,
        //            Freight = req.Freight
        //        };
        //        await UnitWork.AddAsync(express);
        //        expressId = express.Id;
        //    }
        //    //判断是否上传了物流图片
        //    if (req.ExpressPictureIds != null)
        //    {
        //        var ExpressagePictures = new List<ExpressagePicture>();
        //        req.ExpressPictureIds.ForEach(p => ExpressagePictures.Add(new ExpressagePicture { ExpressageId = expressId, PictureId = p, Id = Guid.NewGuid().ToString() }));
        //        await UnitWork.BatchAddAsync(ExpressagePictures.ToArray());
        //    }
        //    //5.反写ExpressId至退料明细
        //    await UnitWork.UpdateAsync<ReturnnoteMaterial>(w => detaiList.Contains(w.Id), u => new ReturnnoteMaterial { ExpressId = expressId });
        //    //创建物料报价单审批流程
        //    //if (req.IsLastReturn == 1)
        //    //{
        //    //    var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("待退料单"));
        //    //    var afir = new AddFlowInstanceReq();
        //    //    afir.SchemeId = mf.FlowSchemeId;
        //    //    afir.FrmType = 2;
        //    //    afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
        //    //    afir.CustomName = $"退料单审批" + DateTime.Now;
        //    //    afir.OrgId = "";
        //    //    //保外申请报价单
        //    //    afir.FrmData = $"{{\"ReturnnoteId\":\"{returnNoteId}\"}}";
        //    //    var flowinstanceid = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
        //    //    await UnitWork.UpdateAsync<ReturnNote>(r => r.Id == returnNoteId, r => new ReturnNote
        //    //    {
        //    //        FlowInstanceId = flowinstanceid
        //    //    });
        //    //}
        //    await UnitWork.UpdateAsync<ReturnNote>(w => w.Id == returnNoteId, u => new ReturnNote { IsLast = req.IsLastReturn, TotalMoney = totalAmt, Status = req.IsLastReturn == 1 ? 2 : 1 });
        //    await UnitWork.SaveAsync();
        //}

        ///// <summary>
        ///// 退料审批
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //public async Task Accraditation(ReturnNoteAuditReq req)
        //{
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    //获取退料单表头信息
        //    var returnNote = await UnitWork.Find<ReturnNote>(w => w.Id == req.Id).FirstOrDefaultAsync();
        //    //仓库验货
        //    await SaveReceiveInfo(req);
        //    //验收通过
        //    await UnitWork.UpdateAsync<ReturnNote>(r => r.Id == req.Id, u => new ReturnNote { Remark = req.Remark });
        //    ////流程通过
        //    //_flowInstanceApp.Verification(new VerificationReq
        //    //{
        //    //    NodeRejectStep = "",
        //    //    NodeRejectType = "0",
        //    //    FlowInstanceId = returnNote.FlowInstanceId,
        //    //    VerificationFinally = "1",
        //    //    VerificationOpinion = "同意",
        //    //});
        //    //判断是否最后一次退料并且所有退料都已核验
        //    if (returnNote.IsLast == 1)
        //    {
        //        //更新退料单为已完成退料
        //        await UnitWork.UpdateAsync<ReturnNote>(w => w.Id == req.Id, u => new ReturnNote { Status = 2 });
        //    }
        //    //物流单状态更新为已仓库收货
        //    await UnitWork.UpdateAsync<Expressage>(w => w.Id == req.ExpressageId, u => new Expressage { Status = 1 });
        //    await UnitWork.SaveAsync();
        //}

        ///// <summary>
        ///// 仓库验货（保存）
        ///// </summary>
        ///// <param name="ReturnMaterials"></param>
        ///// <returns></returns>
        //public async Task SaveReceiveInfo(ReturnNoteAuditReq req)
        //{
        //    //获取退料单表头信息
        //    var returnNote = await UnitWork.Find<ReturnNote>(w => w.Id == req.Id).FirstOrDefaultAsync();
        //    //保存验收结果
        //    if (req.ReturnMaterials != null && req.ReturnMaterials.Count > 0)
        //    {
        //        foreach (var item in req.ReturnMaterials)
        //        {
        //            await UnitWork.UpdateAsync<ReturnnoteMaterial>(r => r.Id == item.Id, u => new ReturnnoteMaterial { Check = item.IsPass, ReceivingRemark = item.ReceiveRemark });
        //        }
        //    }
        //    //保存签收备注
        //    await UnitWork.UpdateAsync<ReturnNote>(r => r.Id == req.Id, u => new ReturnNote { Remark = req.Remark });
        //    //判断是否最后一次退料并且所有退料都已核验
        //    var count = (await UnitWork.Find<ReturnnoteMaterial>(w => w.ReturnNoteId == req.Id && w.Check == 0).ToListAsync()).Count;
        //    if (returnNote.IsLast == 1 && count == 0)
        //    {
        //        //更新退料单为已完成退料
        //        await UnitWork.UpdateAsync<ReturnNote>(w => w.Id == req.Id, u => new ReturnNote { Status = 2 });
        //    }
        //    //物流单状态更新为已仓库收货
        //    await UnitWork.UpdateAsync<Expressage>(w => w.Id == req.ExpressageId, u => new Expressage { Status = 1 });
        //    await UnitWork.SaveAsync();
        //}

        ///// <summary>
        ///// 获取退料结果
        ///// </summary>
        ///// <param name="appUserId"></param>
        ///// <param name="ServiceOrderId"></param>
        ///// <returns></returns>
        //public async Task<TableData> GetReturnNoteInfo(int appUserId, int ServiceOrderId)
        //{
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    //获取当前用户nsap用户信息
        //    var userInfo = (await UnitWork.Find<AppUserMap>(a => a.AppUserId == appUserId).Include(a => a.User).FirstOrDefaultAsync())?.User;
        //    if (userInfo == null)
        //    {
        //        throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
        //    }
        //    var returnNote = await UnitWork.Find<ReturnNote>(w => w.CreateUserId == userInfo.Id && w.ServiceOrderId == ServiceOrderId).FirstOrDefaultAsync();
        //    var result = new TableData();
        //    //获取退料列表
        //    var query = from a in UnitWork.Find<ReturnnoteMaterial>(null)
        //                join b in UnitWork.Find<Expressage>(null) on a.ExpressId equals b.Id into ab
        //                from b in ab.DefaultIfEmpty()
        //                join c in UnitWork.Find<ReturnNoteMaterialPicture>(null) on a.Id equals c.ReturnnoteMaterialId into abc
        //                from c in abc.DefaultIfEmpty()
        //                where a.ReturnNoteId == returnNote.Id && a.Count > 0
        //                orderby b.CreateTime
        //                select new { a.MaterialCode, a.MaterialDescription, a.Count, a.TotalCount, b.ExpressNumber, a.Check, a.ReceivingRemark, a.ShippingRemark, ExpressId = b.Id, c.PictureId, a.Id };
        //    var detailList = (await query.ToListAsync()).GroupBy(g => new { g.ExpressId, g.ExpressNumber }).Select(s => new { s.Key.ExpressId, s.Key.ExpressNumber, returnNote.IsLast, returnNote.Status, returnNote.Remark, detail = s.ToList() }).ToList();
        //    result.Data = detailList;
        //    return result;
        //}

        ///// <summary>
        ///// 获取物流信息
        ///// </summary>
        ///// <param name="Id"></param>
        ///// <returns></returns>
        //public async Task<TableData> GetExpressageInfo(string Id)
        //{
        //    var result = new TableData();
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    var ExpressInfo = await UnitWork.Find<Expressage>(w => w.Id == Id).FirstOrDefaultAsync();
        //    string tracknum = ExpressInfo.ExpressNumber;
        //    var r = await _expressageApp.GetExpressInfo(tracknum);
        //    if (r.Code == 200)
        //    {
        //        var response = (string)r.Data;
        //        await UnitWork.UpdateAsync<Expressage>(w => w.Id == Id, u => new Expressage { ExpressInformation = response });
        //        await UnitWork.SaveAsync();
        //        result.Data = response;
        //    }
        //    else
        //    {
        //        return result;
        //    }

        //    return result;
        //}

        //#endregion

        //#region 旧erp
        ///// <summary>
        ///// 获取退料单列表(ERP 技术员退料)
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //public async Task<TableData> GetReturnNoteList(GetReturnNoteListReq req)
        //{
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    var result = new TableData();
        //    //获取退料列表
        //    var returnNote = await UnitWork.Find<ReturnNote>(null).Include(i => i.Expressages)
        //      .Where(w => !(w.Expressages.All(a => a.Status == 3) && w.IsLast == 1))
        //      .WhereIf(!string.IsNullOrWhiteSpace(req.Id), q => q.Id.Equals(Convert.ToInt32(req.Id)))
        //      .WhereIf(!string.IsNullOrWhiteSpace(req.CreaterName), q => q.CreateUser.Equals(req.CreaterName))
        //      .WhereIf(!string.IsNullOrWhiteSpace(req.BeginDate), q => q.CreateTime >= Convert.ToDateTime(req.BeginDate))
        //      .WhereIf(!string.IsNullOrWhiteSpace(req.EndDate), q => q.CreateTime < Convert.ToDateTime(req.EndDate))
        //      .WhereIf(!string.IsNullOrWhiteSpace(req.QutationId), q => q.StockOutId.Contains(req.QutationId))
        //      .OrderBy(s => s.Id).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
        //    //获取服务单列表
        //    var serviceOrderList = await UnitWork.Find<ServiceOrder>(null)
        //        .WhereIf(!string.IsNullOrWhiteSpace(req.SapId), q => q.U_SAP_ID.Equals(Convert.ToInt32(req.SapId)))
        //        .WhereIf(!string.IsNullOrWhiteSpace(req.Customer), q => q.CustomerName.Equals(req.Customer))
        //        .ToListAsync();
        //    //获取退料单Id集合
        //    List<int> returnNoteIds = returnNote.Select(s => s.Id).Distinct().ToList();
        //    //获取退料单的状态
        //    var expressageList = await UnitWork.Find<Expressage>(w => returnNoteIds.Contains((int)w.ReturnNoteId)).OrderBy(o => o.Status).ToListAsync();
        //    var returnStatus = expressageList.GroupBy(g => g.ReturnNoteId).Select(s => new { status = s.FirstOrDefault().Status, s.Key }).ToList();
        //    var returnNoteList = returnNote.Select(s => new { s.Id, CustomerId = serviceOrderList.Where(w => w.Id == s.ServiceOrderId).Select(s => s.CustomerId).FirstOrDefault(), CustomerName = serviceOrderList.Where(w => w.Id == s.ServiceOrderId).Select(s => s.CustomerName).FirstOrDefault(), s.ServiceOrderId, s.CreateUser, CreateDate = s.CreateTime.ToString("yyyy.MM.dd"), s.ServiceOrderSapId, s.IsCanClear, s.Remark, s.TotalMoney, Status = returnStatus.Where(w => w.Key == s.Id).FirstOrDefault().status, StatusName = GetStatusName(returnStatus.Where(w => w.Key == s.Id).FirstOrDefault().status, s.IsLast) }).ToList();
        //    result.Data = returnNoteList;
        //    return result;
        //}

        //private string GetStatusName(int status, int isLast)
        //{
        //    string name = string.Empty;
        //    switch (status)
        //    {
        //        case 0:
        //            name = "仓库收货";
        //            break;
        //        case 1:
        //            name = "品质检验";
        //            break;
        //        case 2:
        //            name = "仓库入库";
        //            break;
        //        case 3:
        //            if (isLast == 0)
        //            {
        //                name = "技术员退料";
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //    return name;
        //}

        ///// <summary>
        ///// 获取退料单详情(ERP)
        ///// </summary>
        ///// <param name="Id"></param>
        ///// <returns></returns>
        //public async Task<TableData> GetReturnNoteDetail(int Id)
        //{
        //    Dictionary<string, object> outData = new Dictionary<string, object>();
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    var result = new TableData();
        //    //获取退料单主表详情
        //    var returnNote = await UnitWork.Find<ReturnNote>(w => w.Id == Id).FirstOrDefaultAsync();
        //    //获取服务单详情
        //    var serviceOrder = await UnitWork.Find<ServiceOrder>(w => w.Id == returnNote.ServiceOrderId && w.U_SAP_ID == returnNote.ServiceOrderSapId).FirstOrDefaultAsync();
        //    var mainInfo = new Dictionary<string, object>()
        //    {
        //        {"returnNoteCode" ,returnNote.Id},{"creater",returnNote.CreateUser },{ "createTime",returnNote.CreateTime },{"salMan",serviceOrder.SalesMan},{ "serviceSapId",serviceOrder.U_SAP_ID},{ "customerCode",serviceOrder.TerminalCustomerId},{ "customerName",serviceOrder.TerminalCustomer},{ "status",returnNote.Status},{ "isLast",returnNote.IsLast},{ "remark",(string.IsNullOrEmpty(returnNote.Remark)? string.Empty:returnNote.Remark)},{ "stockOutId",returnNote.StockOutId},{ "contacter",serviceOrder.NewestContacter},{"contacterTel",serviceOrder.NewestContactTel },{"serviceOrderId",serviceOrder.Id }
        //    };
        //    outData.Add("mainInfo", mainInfo);
        //    //获取物流信息
        //    var expressList = await UnitWork.Find<Expressage>(w => w.ReturnNoteId == Id).Include(i => i.ExpressagePicture).OrderByDescending(o => o.CreateTime).Select(s => new { s.ExpressNumber, s.ExpressInformation, s.Remark, s.Id, s.Freight, s.ExpressagePicture }).ToListAsync();
        //    outData.Add("expressList", expressList);
        //    //获取当前服务单所有退料明细汇总
        //    var querySum = from a in UnitWork.Find<ReturnnoteMaterial>(null)
        //                   join b in UnitWork.Find<ReturnNote>(null) on a.ReturnNoteId equals b.Id into ab
        //                   from b in ab.DefaultIfEmpty()
        //                   where b.ServiceOrderId == returnNote.ServiceOrderId && a.Count > 0
        //                   select new { a.QuotationMaterialId, a.Count };
        //    var returnMaterials = (await querySum.ToListAsync()).GroupBy(g => g.QuotationMaterialId).Select(s => new { Qty = s.Sum(s => s.Count), Id = s.Key }).ToList();
        //    //获取退料单中所有的物流集合
        //    var query = from a in UnitWork.Find<ReturnnoteMaterial>(null)
        //                join b in UnitWork.Find<Expressage>(null) on a.ExpressId equals b.Id into ab
        //                from b in ab.DefaultIfEmpty()
        //                join c in UnitWork.Find<ReturnNoteMaterialPicture>(null) on a.Id equals c.ReturnnoteMaterialId into abc
        //                from c in abc.DefaultIfEmpty()
        //                where a.ReturnNoteId == Id && a.Count > 0
        //                orderby b.CreateTime
        //                select new ReturnMaterialDetailResp { MaterialCode = a.MaterialCode, MaterialDescription = a.MaterialDescription, Count = (int)a.Count, Check = (int)a.Check, ReceivingRemark = a.ReceivingRemark, ShippingRemark = a.ShippingRemark, ExpressId = b.Id, PictureId = c.PictureId, Id = a.Id, GoodQty = (int)a.GoodQty, SecondQty = (int)a.SecondQty, TotalCount = (int)a.TotalCount, MaterialId = a.QuotationMaterialId };
        //    var data = await query.ToListAsync();
        //    foreach (var item in data)
        //    {
        //        int everQty = (int)(returnMaterials.Where(w => w.Id == item.MaterialId).FirstOrDefault() == null ? 0 : returnMaterials.Where(w => w.Id == item.MaterialId).FirstOrDefault()?.Qty);
        //        item.SurplusQty = item.TotalCount - everQty;
        //    }
        //    var detailList = data.GroupBy(g => g.ExpressId).Select(s => new { s.Key, detail = s.ToList() }).ToList();
        //    outData.Add("materialList", detailList);
        //    result.Data = outData;
        //    return result;
        //}

        ///// <summary>
        ///// 获取退料结算列表(ERP)
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //public async Task<TableData> GetClearReturnNoteList(GetClearReturnNoteListReq req)
        //{
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    var result = new TableData();
        //    //获取已完成退料并且所有退料单仓库核验通过的退料单集合
        //    //获取退料列表
        //    var returnNote = await UnitWork.Find<ReturnNote>(w => w.IsLast == 1 && w.Status == 2 && w.IsCanClear == 1)
        //      .WhereIf(!string.IsNullOrWhiteSpace(req.CreaterName), q => q.CreateUser.Equals(req.CreaterName))
        //      .WhereIf(!string.IsNullOrWhiteSpace(req.BeginDate), q => q.CreateTime >= Convert.ToDateTime(req.BeginDate))
        //      .WhereIf(!string.IsNullOrWhiteSpace(req.EndDate), q => q.CreateTime < Convert.ToDateTime(req.EndDate))
        //      .OrderBy(s => s.Id).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
        //    //获取服务单列表
        //    var serviceOrderList = await UnitWork.Find<ServiceOrder>(null)
        //        .WhereIf(!string.IsNullOrWhiteSpace(req.SapId), q => q.U_SAP_ID.Equals(Convert.ToInt32(req.SapId)))
        //        .WhereIf(!string.IsNullOrWhiteSpace(req.Customer), q => q.CustomerName.Contains(req.Customer) || q.CustomerId.Contains(req.Customer))
        //        .ToListAsync();
        //    //获取退料单Id集合
        //    List<int> returnNoteIds = returnNote.Select(s => s.Id).Distinct().ToList();
        //    //获取退料单的领料单Id集合
        //    var stkOutIdList = returnNote.Select(s => new { s.StockOutId, s.Id }).ToList();
        //    List<QutationRequesQty> qutationRequesQties = new List<QutationRequesQty>();
        //    foreach (var item in stkOutIdList)
        //    {
        //        if (!string.IsNullOrEmpty(item.StockOutId))
        //        {
        //            List<int> quotationIds = new List<int>();
        //            var arr = item.StockOutId.Split(",");
        //            for (int i = 0; i < arr.Length; i++)
        //            {
        //                if (!quotationIds.Contains(Convert.ToInt32(arr[i])))
        //                {
        //                    quotationIds.Add(Convert.ToInt32(arr[i]));
        //                }
        //            }
        //            var qutationMaterials = (await UnitWork.Find<QuotationMergeMaterial>(q => quotationIds.Contains((int)q.QuotationId) && q.MaterialType == 2).ToListAsync()).GroupBy(g => g.MaterialCode).Select(s => new QutationMaterialQty { MaterialCode = s.Key, Qty = s.Sum(s => s.Count) }).ToList();
        //            qutationRequesQties.Add(new QutationRequesQty { ReturnNoteId = item.Id, QutationMaterials = qutationMaterials });
        //        }
        //    }

        //    List<ReturnNotClearAmt> returnNotClearAmts = new List<ReturnNotClearAmt>();
        //    foreach (var item in returnNoteIds)
        //    {
        //        decimal? notClearAmount = 0;
        //        var MaterialList = (await UnitWork.Find<ReturnnoteMaterial>(w => w.ReturnNoteId == item).ToListAsync()).GroupBy(g => g.MaterialCode).Select(s => new
        //        {
        //            MaterialCode = s.Key,
        //            NotClearAmount = s.Where(w => w.MaterialCode == s.Key).FirstOrDefault().DiscountPrices * (qutationRequesQties.Where(w => w.ReturnNoteId == item).FirstOrDefault().QutationMaterials.Where(w => w.MaterialCode == s.Key).FirstOrDefault().Qty - s.Where(w => w.MaterialCode == s.Key).Sum(k => k.GoodQty) - s.Where(w => w.MaterialCode == s.Key).Sum(k => k.SecondQty))
        //        }).ToList();
        //        MaterialList.ForEach(f => notClearAmount += f.NotClearAmount);
        //        returnNotClearAmts.Add(new ReturnNotClearAmt { ReturnNoteId = item, Amt = notClearAmount });
        //    }

        //    var returnNoteList = returnNote.Select(s => new { CustomerId = serviceOrderList.Where(w => w.Id == s.ServiceOrderId).Select(s => s.TerminalCustomerId).FirstOrDefault(), CustomerName = serviceOrderList.Where(w => w.Id == s.ServiceOrderId).Select(s => s.TerminalCustomer).FirstOrDefault(), s.ServiceOrderId, s.CreateUser, CreateDate = s.CreateTime.ToString("yyyy.mm.dd"), s.ServiceOrderSapId, s.CreateUserId, s.Id, notClearAmount = Math.Round((decimal)returnNotClearAmts.Where(w => w.ReturnNoteId == s.Id).FirstOrDefault().Amt, 2), Status = Math.Round((decimal)returnNotClearAmts.Where(w => w.ReturnNoteId == s.Id).FirstOrDefault().Amt, 2) > 0 ? "未清" : "已清", s.Remark }).ToList().GroupBy(g => new { g.Id }).Select(s => new { s.Key, detail = s.ToList() }).ToList();
        //    result.Data = returnNoteList;
        //    return result;
        //}

        ///// <summary>
        ///// 获取退料结算详情(ERP)
        ///// </summary>
        ///// <param name="Id">退料单Id</param>
        ///// <returns></returns>
        //public async Task<TableData> GetClearReturnNoteDetail(int Id)
        //{
        //    Dictionary<string, dynamic> outData = new Dictionary<string, dynamic>();
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    var result = new TableData();
        //    //获取退料主表信息
        //    var returnNoteInfo = await UnitWork.Find<ReturnNote>(w => w.Id == Id).FirstOrDefaultAsync();
        //    outData.Add("CreateTime", returnNoteInfo.CreateTime.ToString("yyyy-MM-dd HH:mm"));
        //    outData.Add("CreateUser", returnNoteInfo.CreateUser);
        //    outData.Add("ReturnNoteId", returnNoteInfo.Id);
        //    outData.Add("StockOutId", returnNoteInfo.StockOutId);
        //    //获取当前服务单信息
        //    var serviceOrderInfo = await UnitWork.Find<ServiceOrder>(w => w.U_SAP_ID == returnNoteInfo.ServiceOrderSapId).FirstOrDefaultAsync();
        //    outData.Add("U_SAP_ID", serviceOrderInfo.U_SAP_ID);
        //    outData.Add("SalesMan", serviceOrderInfo.SalesMan);
        //    outData.Add("CustomerId", serviceOrderInfo.TerminalCustomerId);
        //    outData.Add("CustomerName", serviceOrderInfo.TerminalCustomer);
        //    outData.Add("Contacter", serviceOrderInfo.NewestContacter);
        //    outData.Add("ContactTel", serviceOrderInfo.NewestContactTel);
        //    //获取领料单详情
        //    List<int> quotationIds = new List<int>();
        //    if (!string.IsNullOrEmpty(returnNoteInfo.StockOutId))
        //    {
        //        var arr = returnNoteInfo.StockOutId.Split(",");
        //        for (int i = 0; i < arr.Length; i++)
        //        {
        //            if (!quotationIds.Contains(Convert.ToInt32(arr[i])))
        //            {
        //                quotationIds.Add(Convert.ToInt32(arr[i]));
        //            }
        //        }
        //    }
        //    var qutationMaterials = (await UnitWork.Find<QuotationMergeMaterial>(q => quotationIds.Contains((int)q.QuotationId) && q.MaterialType == 2).ToListAsync()).GroupBy(g => g.MaterialCode).Select(s => new { s.Key, Qty = s.Sum(s => s.Count) }).ToList();
        //    //计算剩余未结清金额
        //    decimal? notClearAmount = 0;
        //    //获取物料信息
        //    var MaterialList = (await UnitWork.Find<ReturnnoteMaterial>(w => w.ReturnNoteId == returnNoteInfo.Id).ToListAsync()).GroupBy(g => g.MaterialCode).Select(s => new
        //    {
        //        MaterialCode = s.Key,
        //        MaterDescription = s.Where(w => w.MaterialCode == s.Key).FirstOrDefault().MaterialDescription,
        //        AlreadyReturnQty = s.Where(w => w.MaterialCode == s.Key).Sum(k => k.Count),
        //        TotalReturnCount = qutationMaterials.Where(w => w.Key == s.Key).FirstOrDefault().Qty,
        //        NotClearAmount = s.Where(w => w.MaterialCode == s.Key).FirstOrDefault().DiscountPrices * (qutationMaterials.Where(w => w.Key == s.Key).FirstOrDefault().Qty - s.Where(w => w.MaterialCode == s.Key).Sum(k => k.GoodQty) - s.Where(w => w.MaterialCode == s.Key).Sum(k => k.SecondQty)),
        //        Status = s.Where(w => w.MaterialCode == s.Key).FirstOrDefault().DiscountPrices * (qutationMaterials.Where(w => w.Key == s.Key).FirstOrDefault().Qty - s.Where(w => w.MaterialCode == s.Key).Sum(k => k.GoodQty) - s.Where(w => w.MaterialCode == s.Key).Sum(k => k.SecondQty)) > 0 ? "未清" : "已清"
        //    }).ToList();
        //    MaterialList.ForEach(f => notClearAmount += f.NotClearAmount);
        //    outData.Add("NotClearAmount", Math.Round((decimal)notClearAmount, 2));
        //    outData.Add("DetailList", MaterialList);
        //    result.Data = outData;
        //    return result;
        //}

        ///// <summary>
        ///// 获取可退料的服务单集合(ERP)
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //public async Task<TableData> GetServiceOrderInfo(PageReq req)
        //{
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    var result = new TableData();
        //    //查询当前技术员所有可退料服务Id
        //    var query = from a in UnitWork.Find<QuotationMergeMaterial>(null)
        //                join b in UnitWork.Find<Quotation>(null) on a.QuotationId equals b.Id
        //                where b.CreateUserId == loginContext.User.Id && a.MaterialType == 1 && b.QuotationStatus == 11 && b.SalesOrderId!=null
        //                select new { a,b };
        //    var queryList = await query.ToListAsync();
        //    var salesOrderIds = queryList.Select(s => s.b.SalesOrderId).ToList();
        //    var saledln1 = await UnitWork.Find<sale_dln1>(s=> salesOrderIds.Contains(s.BaseEntry) && s.BaseType == 17).ToListAsync();
        //    var saledln1Ids = saledln1.Select(s => s.DocEntry).ToList();
        //    var saleinv1 = await UnitWork.Find<sale_inv1>(s => saledln1Ids.Contains((int)s.BaseEntry) && s.BaseType == 15 && s.LineStatus=="O").ToListAsync();

        //    //var oinvquery = from a in UnitWork.Find<sale_dln1>(null)
        //    //                join b in UnitWork.Find<sale_inv1>(null) on a.DocEntry equals b.BaseEntry into ab
        //    //                from b in ab.DefaultIfEmpty()
        //    //                where salesOrderIds.Contains(a.BaseEntry) && a.BaseType == 17 && b.BaseType == 15 
        //    //                select new { a, b };
        //    //var oinvqueryList = await oinvquery.ToListAsync();

        //    var docentrys = saleinv1.Select(s => (int)s.DocEntry).ToList();
        //    var oinvList = saleinv1.Select(o => new
        //    {
        //        SalesOrderId= saledln1.Where(s=>s.DocEntry==o.BaseEntry).FirstOrDefault()?.BaseEntry,
        //        o.ItemCode,
        //        Quantity= o.Quantity,
        //        o.Dscription,
        //        o.Price,
        //        o.DocEntry,
        //        MergeMaterialId= queryList.Where(q=>q.a.MaterialCode.Equals(o.ItemCode)&&q.b.SalesOrderId== saledln1.Where(s => s.DocEntry == o.BaseEntry).FirstOrDefault()?.BaseEntry).FirstOrDefault()?.a.Id
        //    }).ToList();
        //    result.Data = oinvList.GroupBy(o => o.DocEntry).Select(o=>new { docEntry=o.Key, oinvList=o }).ToList();
        //    return result;
        //}

        ///// <summary>
        ///// 品质检验(ERP)
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //public async Task CheckOutMaterials(CheckOutMaterialsReq req)
        //{
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    //查询需要做检验的退料明细
        //    var returnMaterialDetails = await UnitWork.Find<ReturnnoteMaterial>(w => req.DetailIds.Contains(w.Id)).ToListAsync();
        //    foreach (var item in req.checkOutMaterials)
        //    {
        //        var detail = returnMaterialDetails.Where(w => w.Id == item.Id).FirstOrDefault();
        //        detail.GoodQty = item.GoodQty;
        //        detail.SecondQty = item.SecondQty;
        //    }
        //    await UnitWork.BatchUpdateAsync(returnMaterialDetails.ToArray());
        //    //更新为已品质检验
        //    await UnitWork.UpdateAsync<Expressage>(w => w.Id == req.ExpressageId, u => new Expressage { Status = 2 });
        //    await UnitWork.SaveAsync();
        //}

        ///// <summary>
        ///// 获取退料单列表(ERP 仓库收货/品质入库/仓库入库)
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //public async Task<TableData> GetReturnNoteListByExpress(GetReturnNoteListByExpressReq req)
        //{
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    var result = new TableData();
        //    ////获取退料列表
        //    var returnNoteIds = await UnitWork.Find<ReturnNote>(null)
        //      .WhereIf(!string.IsNullOrWhiteSpace(req.Id), q => q.Id.Equals(Convert.ToInt32(req.Id)))
        //      .WhereIf(!string.IsNullOrWhiteSpace(req.CreaterName), q => q.CreateUser.Equals(req.CreaterName))
        //      .WhereIf(!string.IsNullOrWhiteSpace(req.BeginDate), q => q.CreateTime >= Convert.ToDateTime(req.BeginDate))
        //      .WhereIf(!string.IsNullOrWhiteSpace(req.EndDate), q => q.CreateTime < Convert.ToDateTime(req.EndDate))
        //      .WhereIf(!string.IsNullOrWhiteSpace(req.QutationId), q => q.StockOutId.Contains(req.QutationId))
        //      .OrderBy(s => s.Id).Select(s => s.Id).Distinct().ToListAsync();
        //    ////获取服务单列表
        //    var serviceOrderList = await UnitWork.Find<ServiceOrder>(null)
        //        .WhereIf(!string.IsNullOrWhiteSpace(req.SapId), q => q.U_SAP_ID.Equals(Convert.ToInt32(req.SapId)))
        //        .WhereIf(!string.IsNullOrWhiteSpace(req.Customer), q => q.CustomerName.Equals(req.Customer))
        //        .ToListAsync();
        //    var query = from a in UnitWork.Find<Expressage>(null)
        //                join b in UnitWork.Find<ReturnNote>(null) on a.ReturnNoteId equals b.Id
        //                where returnNoteIds.Contains((int)a.ReturnNoteId) && a.Status == Convert.ToInt32(req.Status) && (!string.IsNullOrEmpty(req.TrackNumber) ? a.ExpressNumber == req.TrackNumber : true)
        //                select new ReturnNoteMainResp { ExpressNumber = a.ExpressNumber, ExpressId = a.Id, Id = b.Id, ServiceOrderId = b.ServiceOrderId, CreateUser = b.CreateUser, CreateDate = b.CreateTime.ToString("yyyy.MM.dd"), ServiceOrderSapId = b.ServiceOrderSapId, IsCanClear = b.IsCanClear, Remark = b.Remark, TotalMoney = (decimal)b.TotalMoney };
        //    var data = await query.Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
        //    foreach (var item in data)
        //    {
        //        item.CustomerId = serviceOrderList.Where(w => w.Id == item.ServiceOrderId).FirstOrDefault()?.TerminalCustomerId;
        //        item.CustomerName = serviceOrderList.Where(w => w.Id == item.ServiceOrderId).FirstOrDefault()?.TerminalCustomer;
        //    }
        //    result.Data = data;
        //    return result;
        //}

        ///// <summary>
        ///// 获取退料单详情（根据物流单号）
        ///// </summary>
        ///// <param name="ExpressageId"></param>
        ///// <returns></returns>
        //public async Task<TableData> GetReturnNoteDetailByExpress(string ExpressageId)
        //{
        //    Dictionary<string, object> outData = new Dictionary<string, object>();
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    var result = new TableData();
        //    //获取物流信息
        //    var expressList = await UnitWork.Find<Expressage>(w => w.Id == ExpressageId).Include(i => i.ExpressagePicture).Select(s => new { s.ExpressNumber, s.ExpressInformation, s.Remark, s.Id, s.Freight, s.ReturnNoteId, s.ExpressagePicture }).FirstOrDefaultAsync();
        //    //获取退料单主表详情
        //    var returnNote = await UnitWork.Find<ReturnNote>(w => w.Id == expressList.ReturnNoteId).FirstOrDefaultAsync();
        //    //获取服务单详情
        //    var serviceOrder = await UnitWork.Find<ServiceOrder>(w => w.Id == returnNote.ServiceOrderId && w.U_SAP_ID == returnNote.ServiceOrderSapId).FirstOrDefaultAsync();
        //    var mainInfo = new Dictionary<string, object>()
        //    {
        //        {"returnNoteCode" ,returnNote.Id},{"creater",returnNote.CreateUser },{ "createTime",returnNote.CreateTime },{"salMan",serviceOrder.SalesMan},{ "serviceSapId",serviceOrder.U_SAP_ID},{ "customerCode",serviceOrder.TerminalCustomerId},{ "customerName",serviceOrder.TerminalCustomer},{ "status",returnNote.Status},{ "isLast",returnNote.IsLast},{ "remark",(string.IsNullOrEmpty(returnNote.Remark)? string.Empty:returnNote.Remark)},{ "stockOutId",returnNote.StockOutId},{ "contacter",serviceOrder.NewestContacter},{"contacterTel",serviceOrder.NewestContactTel },{"serviceOrderId",serviceOrder.Id }
        //    };
        //    outData.Add("mainInfo", mainInfo);
        //    outData.Add("expressList", expressList);
        //    //获取当前服务单所有退料明细汇总
        //    var querySum = from a in UnitWork.Find<ReturnnoteMaterial>(null)
        //                   join b in UnitWork.Find<ReturnNote>(null) on a.ReturnNoteId equals b.Id into ab
        //                   from b in ab.DefaultIfEmpty()
        //                   where b.ServiceOrderId == returnNote.ServiceOrderId && a.Count > 0
        //                   select new { a.QuotationMaterialId, a.Count };
        //    var returnMaterials = (await querySum.ToListAsync()).GroupBy(g => g.QuotationMaterialId).Select(s => new { Qty = s.Sum(s => s.Count), Id = s.Key }).ToList();

        //    //获取退料单中所有的物流集合
        //    var query = from a in UnitWork.Find<ReturnnoteMaterial>(null)
        //                join b in UnitWork.Find<ReturnNoteMaterialPicture>(null) on a.Id equals b.ReturnnoteMaterialId into ab
        //                from b in ab.DefaultIfEmpty()
        //                where a.ExpressId == ExpressageId && a.Count > 0
        //                select new ReturnMaterialDetailResp { MaterialCode = a.MaterialCode, MaterialDescription = a.MaterialDescription, Count = (int)a.Count, Check = (int)a.Check, ReceivingRemark = a.ReceivingRemark, ShippingRemark = a.ShippingRemark, ExpressId = b.Id, PictureId = b.PictureId, Id = a.Id, GoodQty = (int)a.GoodQty, SecondQty = (int)a.SecondQty, TotalCount = (int)a.TotalCount, MaterialId = a.QuotationMaterialId, IsGoodFinish = (int)a.IsGoodFinish, IsSecondFinish = (int)a.IsSecondFinish };
        //    var data = await query.ToListAsync();
        //    foreach (var item in data)
        //    {
        //        int everQty = (int)(returnMaterials.Where(w => w.Id == item.MaterialId).FirstOrDefault() == null ? 0 : returnMaterials.Where(w => w.Id == item.MaterialId).FirstOrDefault()?.Qty);
        //        item.SurplusQty = item.TotalCount - everQty;
        //    }
        //    outData.Add("materialList", data);
        //    result.Data = outData;
        //    return result;
        //}
        #endregion

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
            var returnNotes = UnitWork.Find<ReturnNote>(null).Include(r => r.ReturnNotePictures).Include(r => r.ReturnnoteMaterials).ThenInclude(r => r.ReturnNoteMaterialPictures)
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
            var returnNoteList = await returnNotes.ToListAsync();
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
                    InvoiceDocEntry = r.ReturnnoteMaterials.FirstOrDefault()?.InvoiceDocEntry,
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
                    InvoiceDocEntry = r.ReturnnoteMaterials.FirstOrDefault()?.InvoiceDocEntry,
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
            query = query.WhereIf(!string.IsNullOrWhiteSpace(req.SalesOrderId.ToString()), q => q.b.SalesOrderId == req.SalesOrderId);
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
            var quotationObj = await UnitWork.Find<Quotation>(q => q.SalesOrderId == req.SalesOrderId && q.CreateUserId.Equals(loginUser.Id)).Include(q => q.QuotationMergeMaterials).FirstOrDefaultAsync();

            //查询应收发票  && s.LineStatus == "O"
            var saleinv1 = await UnitWork.Find<sale_inv1>(s => s.DocEntry == req.InvoiceDocEntry).ToListAsync();

            //是否存在退料记录
            var materials = await UnitWork.Find<ReturnnoteMaterial>(r => req.InvoiceDocEntry == r.InvoiceDocEntry).ToListAsync();
            materials.ForEach(m =>
            {
                m.MaterialCode = quotationObj.QuotationMergeMaterials.Where(q => q.Id.Equals(m.QuotationMaterialId)).FirstOrDefault()?.MaterialCode;
            });
            result.Data = saleinv1.Select(s => new
            {
                MaterialCode = s.ItemCode,
                MaterialDescription = s.Dscription,
                s.Price,
                Quantity = s.Quantity,//总数量
                ResidueQuantity = materials.Where(m => m.MaterialCode.Equals(s.ItemCode) && m.InvoiceDocEntry == s.DocEntry).Sum(m => m.Count) > 0 ? s.Quantity - materials.Where(m => m.MaterialCode.Equals(s.ItemCode) && m.InvoiceDocEntry == s.DocEntry).Sum(m => m.Count) : s.Quantity,//应退数量
                QuotationMaterialId = quotationObj.QuotationMergeMaterials.Where(q => q.MaterialCode.Equals(s.ItemCode) && (s.Price == q.DiscountPrices || s.Price == decimal.Parse(Convert.ToDecimal(q.DiscountPrices).ToString("#0.00")))).FirstOrDefault()?.Id,
            }).Where(s => s.QuotationMaterialId != null).ToList();

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
            var returnNotes = await UnitWork.Find<ReturnNote>(r => r.Id == req.returnNoteId).Include(r => r.ReturnNotePictures).Include(r => r.ReturnnoteMaterials).ThenInclude(r => r.ReturnNoteMaterialPictures).FirstOrDefaultAsync();
            var History = await UnitWork.Find<FlowInstanceOperationHistory>(f => f.InstanceId.Equals(returnNotes.FlowInstanceId)).OrderBy(f => f.CreateDate).ToListAsync();

            //查询当前技术员所有可退料服务Id
            var quotationObj = await UnitWork.Find<Quotation>(q => q.SalesOrderId == returnNotes.SalesOrderId).Include(q => q.QuotationMergeMaterials).FirstOrDefaultAsync();
            var InvoiceDocEntry = returnNotes.ReturnnoteMaterials.Select(r => r.InvoiceDocEntry).FirstOrDefault();
            var result = new TableData();
            var serviceOrders = await UnitWork.Find<ServiceOrder>(s => s.Id == returnNotes.ServiceOrderId).Select(s => new { s.Id, s.TerminalCustomer, s.TerminalCustomerId, s.NewestContacter, s.NewestContactTel, s.U_SAP_ID }).FirstOrDefaultAsync();
            //查询应收发票  && s.LineStatus == "O"
            var saleinv1 = await UnitWork.Find<sale_inv1>(s => s.DocEntry == InvoiceDocEntry && s.sbo_id==Define.SBO_ID).ToListAsync();
            var DocTotal = saleinv1.Sum(s => s.LineTotal);
            //是否存在退料记录
            var materials = await UnitWork.Find<ReturnnoteMaterial>(r => r.ReturnNoteId!= returnNotes.Id&& InvoiceDocEntry == r.InvoiceDocEntry).ToListAsync();
            
            List<string> fileIds = new List<string>();
            returnNotes.ReturnnoteMaterials.ForEach(r => fileIds.AddRange(r.ReturnNoteMaterialPictures.Select(n => n.PictureId).ToList()));
            var fileList = await UnitWork.Find<UploadFile>(f => fileIds.Contains(f.Id)).ToListAsync();
            List<object> returnnoteMaterials = new List<object>();
            returnNotes.ReturnnoteMaterials.ForEach(r =>
                {
                    var QuotationMergeMaterialObj = quotationObj.QuotationMergeMaterials.Where(q => q.Id.Equals(r.QuotationMaterialId)).FirstOrDefault();
                    var Quantity = saleinv1.Where(s => s.ItemCode.Equals(QuotationMergeMaterialObj.MaterialCode) && ( Convert.ToDouble(s.Price) == Convert.ToDouble(QuotationMergeMaterialObj.DiscountPrices) || s.Price == decimal.Parse(Convert.ToDecimal(QuotationMergeMaterialObj.DiscountPrices).ToString("#0.00")))).FirstOrDefault()?.Quantity;
                    var num = materials.Where(m => m.QuotationMaterialId.Equals(r.QuotationMaterialId)).Sum(m => m.Count);
                    var ResidueQuantity = num > 0 ? Quantity - num : Quantity;
                    returnnoteMaterials.Add(new
                    {
                        r.Id,
                        r.InvoiceDocEntry,
                        r.GoodWhsCode,
                        r.GoodQty,
                        r.Count,
                        r.MaterialCode,
                        r.MaterialDescription,
                        r.QuotationMaterialId,
                        r.ReceivingRemark,
                        r.ReturnNoteId,
                        r.ProductCode,
                        r.ReplaceProductCode,
                        ReturnNoteMaterialPictures = r.ReturnNoteMaterialPictures.Select(p => new
                        {
                            FileName = fileList.Where(f => f.Id.Equals(p.PictureId)).FirstOrDefault()?.FileName,
                            FileType = fileList.Where(f => f.Id.Equals(p.PictureId)).FirstOrDefault()?.FileType,
                            p.PictureId
                        }),
                        r.SecondQty,
                        r.SecondWhsCode,
                        r.ShippingRemark,
                        TotalPrice = r.Count * QuotationMergeMaterialObj.DiscountPrices,
                        Price = QuotationMergeMaterialObj.DiscountPrices,
                        Quantity = Quantity,
                        ResidueQuantity = ResidueQuantity,
                        IfReplace = QuotationMergeMaterialObj.MaterialCode.Equals(r.MaterialCode) ? false : true,
                        ReplacePartCode =  QuotationMergeMaterialObj.MaterialCode,
                        ReplacePartDescription =  QuotationMergeMaterialObj.MaterialDescription
                    });

                }
            );
            if (req.IsUpDate != null && (bool)req.IsUpDate)
            {
                var ReturnnoteMaterialList = returnNotes.ReturnnoteMaterials.Select(r => new
                {
                    MaterialCode= quotationObj.QuotationMergeMaterials.Where(q=>q.Id.Equals(r.QuotationMaterialId)).FirstOrDefault()?.MaterialCode,
                    Price = quotationObj.QuotationMergeMaterials.Where(q => q.Id.Equals(r.QuotationMaterialId)).FirstOrDefault()?.DiscountPrices
                }).ToList();
                ReturnnoteMaterialList.ForEach(r =>
                {
                    saleinv1 = saleinv1.Where(s => s.ItemCode != r.MaterialCode && s.Price != r.Price && s.Price != decimal.Parse(Convert.ToDecimal(r.Price).ToString("#0.00"))).ToList();
                });
                materials.ForEach(m => m.MaterialCode = quotationObj.QuotationMergeMaterials.Where(q => q.Id.Equals(m.QuotationMaterialId)).FirstOrDefault()?.MaterialCode);
                saleinv1.ForEach(s =>
                {
                    returnnoteMaterials.Add(new
                    {
                        MaterialCode = s.ItemCode,
                        MaterialDescription = s.Dscription,
                        InvoiceDocEntry = s.DocEntry,
                        s.Price,
                        Quantity = s.Quantity,//总数量
                        ResidueQuantity = materials.Where(m => m.MaterialCode.Equals(s.ItemCode) && m.InvoiceDocEntry == s.DocEntry).Sum(m => m.Count) > 0 ? s.Quantity - materials.Where(m => m.MaterialCode.Equals(s.ItemCode) && m.InvoiceDocEntry == s.DocEntry).Sum(m => m.Count) : s.Quantity,//应退数量
                        QuotationMaterialId = quotationObj.QuotationMergeMaterials.Where(q => q.MaterialCode.Equals(s.ItemCode) && (s.Price == q.DiscountPrices || s.Price == decimal.Parse(Convert.ToDecimal(q.DiscountPrices).ToString("#0.00")))).FirstOrDefault()?.Id,
                    });
                });
            }
            returnNotes.ReturnnoteMaterials = null;
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ReturnNoteTypeName")).Select(u => new { u.Name, u.DtValue }).ToListAsync();
            var flowInstanceObj = await UnitWork.Find<FlowInstance>(f => f.Id.Equals(returnNotes.FlowInstanceId)).FirstOrDefaultAsync();
            var ReturnnoteOperationHistorys = History.Select(h => new
            {
                CreateDate = h.CreateDate.ToString("yyyy.MM.dd HH:mm:ss"),
                h.Remark,
                IntervalTime = h.IntervalTime != null && h.IntervalTime > 0 ? h.IntervalTime / 60 : null,
                h.CreateUserName,
                h.Content,
                h.ApprovalResult,
            });
            var qoutationReq = await _quotation.GeneralDetails(quotationObj.Id, null);
            result.Data = new
            {
                InvoiceDocEntry,
                DocTotal = DocTotal,
                Status = flowInstanceObj?.IsFinish == FlowInstanceStatus.Rejected ? "驳回" : flowInstanceObj?.ActivityName == null ? "开始" : flowInstanceObj?.ActivityName,
                returnNoteId = returnNotes.Id,
                returnNotes,
                returnnoteMaterials,
                serviceOrders,
                ReturnnoteOperationHistorys,
                Quotations= qoutationReq
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
                        await _quotation.AddOrUpdate(new WorkbenchPending
                        {
                            OrderType = 1,
                            TerminalCustomer = serviceOrederObj.TerminalCustomer,
                            TerminalCustomerId = serviceOrederObj.TerminalCustomerId,
                            ServiceOrderId = serviceOrederObj.Id,
                            ServiceOrderSapId = (int)serviceOrederObj.U_SAP_ID,
                            UpdateTime = returnnotrObj.UpdateTime,
                            Remark = returnnotrObj.Remark,
                            FlowInstanceId = returnnotrObj.FlowInstanceId,
                            TotalMoney = returnnotrObj.TotalMoney,
                            Petitioner = loginUser.Name,
                            SourceNumbers = returnnotrObj.Id
                        });
                    }
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
            var dbContext = UnitWork.GetDbContext<Quotation>();
            //事务
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //先删后增
                    #region 删除
                    var returnNoteObj = await UnitWork.Find<ReturnNote>(r => r.Id == obj.ReturnNoteId).Include(r => r.ReturnNotePictures).Include(r => r.ReturnnoteMaterials).ThenInclude(r => r.ReturnNoteMaterialPictures).FirstOrDefaultAsync();
                    var materialPictures = new List<ReturnNoteMaterialPicture>();
                    returnNoteObj.ReturnnoteMaterials.ForEach(r => materialPictures.AddRange(r.ReturnNoteMaterialPictures.ToList()));
                    if (materialPictures != null && materialPictures.Count > 0)
                    {
                        await UnitWork.BatchDeleteAsync<ReturnNoteMaterialPicture>(materialPictures.ToArray());
                    }
                    if (returnNoteObj.ReturnnoteMaterials != null && returnNoteObj.ReturnnoteMaterials.Count > 0)
                    {
                        await UnitWork.BatchDeleteAsync<ReturnnoteMaterial>(returnNoteObj.ReturnnoteMaterials.ToArray());
                    }
                    if (returnNoteObj.ReturnNotePictures != null && returnNoteObj.ReturnNotePictures.Count > 0)
                    {
                        await UnitWork.BatchDeleteAsync<ReturnNotePicture>(returnNoteObj.ReturnNotePictures.ToArray());
                    }
                    await UnitWork.SaveAsync();
                    #endregion
                    #region 新增
                    obj.ReturnnoteMaterials.ForEach(r => r.ReturnNoteId = obj.ReturnNoteId);
                    await UnitWork.BatchAddAsync<ReturnnoteMaterial>(obj.ReturnnoteMaterials.ToArray());
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
                        await _quotation.AddOrUpdate(new WorkbenchPending
                        {
                            OrderType = 1,
                            TerminalCustomer = serviceOrederObj.TerminalCustomer,
                            TerminalCustomerId = serviceOrederObj.TerminalCustomerId,
                            ServiceOrderId = serviceOrederObj.Id,
                            ServiceOrderSapId = (int)serviceOrederObj.U_SAP_ID,
                            UpdateTime = DateTime.Now,
                            Remark = returnNoteObj.Remark,
                            FlowInstanceId = FlowInstanceId,
                            TotalMoney = returnNoteObj.TotalMoney,
                            Petitioner = loginUser.Name,
                            SourceNumbers = returnNoteObj.Id
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

                    //if (!obj.IsDraft)
                    //{
                    //    await UnitWork.AddAsync<ReturnnoteOperationHistory>(new ReturnnoteOperationHistory
                    //    {
                    //        Action = "提交退料单",
                    //        ApprovalStage = "3",
                    //        CreateTime = DateTime.Now,
                    //        CreateUser = loginUser.Name,
                    //        CreateUserId = loginUser.Id,
                    //        ReturnNoteId = obj.ReturnNoteId
                    //    });
                    //}
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
            var returnNoteObj = await UnitWork.Find<ReturnNote>(r => r.Id == returnNoteId).Include(r => r.ReturnNotePictures).Include(r => r.ReturnnoteMaterials).ThenInclude(r => r.ReturnNoteMaterialPictures).FirstOrDefaultAsync();
            var flowInstanceObj = await UnitWork.Find<FlowInstance>(f => f.Id.Equals(returnNoteObj.FlowInstanceId)).FirstOrDefaultAsync();
            if (flowInstanceObj.IsFinish == FlowInstanceStatus.Finished || flowInstanceObj.IsFinish == FlowInstanceStatus.Running)
            {
                throw new Exception("此退料单已完成或正在进行中不可删除。");
            }
            var materialPictures = new List<ReturnNoteMaterialPicture>();
            returnNoteObj.ReturnnoteMaterials.ForEach(r => materialPictures.AddRange(r.ReturnNoteMaterialPictures.ToList()));
            //删除所有关联数据
            if (returnNoteObj != null)
            {
                if (materialPictures != null && materialPictures.Count > 0)
                {
                    await UnitWork.BatchDeleteAsync<ReturnNoteMaterialPicture>(materialPictures.ToArray());
                }
                if (returnNoteObj.ReturnnoteMaterials != null && returnNoteObj.ReturnnoteMaterials.Count > 0)
                {
                    await UnitWork.BatchDeleteAsync<ReturnnoteMaterial>(returnNoteObj.ReturnnoteMaterials.ToArray());
                }
                if (returnNoteObj.ReturnNotePictures != null && returnNoteObj.ReturnNotePictures.Count > 0)
                {
                    await UnitWork.BatchDeleteAsync<ReturnNotePicture>(returnNoteObj.ReturnNotePictures.ToArray());
                }
                await UnitWork.DeleteAsync<ReturnNote>(returnNoteObj);
                await UnitWork.DeleteAsync<FlowInstance>(flowInstanceObj);
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
            var returnNotes = await UnitWork.Find<ReturnNote>(r => r.Id == req.Id).Include(r => r.ReturnnoteMaterials).FirstOrDefaultAsync();
            if (returnNotes == null)
            {
                throw new Exception("退料单为空，请核对。");
            }
            var flowInstanceObj = await UnitWork.Find<FlowInstance>(f => f.Id.Equals(returnNotes.FlowInstanceId)).FirstOrDefaultAsync();
            if (loginContext.Roles.Any(r => r.Name.Equals("物料品质")) && flowInstanceObj.ActivityName.Equals("品质检验"))
            {
                if (!req.IsReject)
                {
                    var materialIds = req.returnnoteMaterials.Select(r => r.MaterialsId).ToList();
                    if (materialIds != null && materialIds.Count > 0)
                    {
                        var returnnoteMaterials = returnNotes.ReturnnoteMaterials.Where(r => materialIds.Contains(r.Id)).ToList();
                        returnnoteMaterials.ForEach(r =>
                        {
                            var materialObj = req.returnnoteMaterials.Where(m => m.MaterialsId.Equals(r.Id)).FirstOrDefault();
                            r.GoodQty = materialObj?.GoodQty > 0 ? materialObj?.GoodQty : 0;
                            r.SecondQty = materialObj?.SecondQty > 0 ? materialObj?.SecondQty : 0;
                        });
                        await UnitWork.BatchUpdateAsync<ReturnnoteMaterial>(returnnoteMaterials.ToArray());
                    }

                }
            }
            if (loginContext.Roles.Any(r => r.Name.Equals("仓库")) && flowInstanceObj.ActivityName.Equals("仓库入库"))
            {
                if (!req.IsReject)
                {
                    var materialIds = req.returnnoteMaterials.Select(r => r.MaterialsId).ToList();
                    if (materialIds != null && materialIds.Count > 0)
                    {
                        var returnnoteMaterials = returnNotes.ReturnnoteMaterials.Where(r => materialIds.Contains(r.Id)).ToList();
                        returnnoteMaterials.ForEach(r =>
                        {
                            var materialObj = req.returnnoteMaterials.Where(m => m.MaterialsId.Equals(r.Id)).FirstOrDefault();
                            r.GoodWhsCode = materialObj?.GoodWhsCode;
                            r.SecondWhsCode = materialObj?.SecondWhsCode;
                        });
                        await UnitWork.BatchUpdateAsync<ReturnnoteMaterial>(returnnoteMaterials.ToArray());
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
            #region 判定是否和报价单有关联
            obj.ReturnnoteMaterials.ForEach(r =>
            {
                if (string.IsNullOrWhiteSpace(r.QuotationMaterialId)) 
                {
                    throw new Exception("未找到关联销售订单，请联系管理员。");
                }
            });
            #endregion
            var quotationMaterialIds = obj.ReturnnoteMaterials.Select(r => r.QuotationMaterialId).ToList();
            var quotationMaterials = await UnitWork.Find<QuotationMergeMaterial>(q => quotationMaterialIds.Contains(q.Id)).Select(q => new { q.Id, q.DiscountPrices }).ToListAsync();
            decimal TotalMoney = 0;
            obj.ReturnnoteMaterials.ForEach(r =>
            {
                TotalMoney += Convert.ToDecimal(r.Count * quotationMaterials.Where(q => q.Id.Equals(r.QuotationMaterialId)).FirstOrDefault()?.DiscountPrices);
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
            var returnNoteObj = await UnitWork.Find<ReturnNote>(r => r.Id == req.Id).Include(r => r.ReturnnoteMaterials).FirstOrDefaultAsync();
            List<QuotationMergeMaterialReq> returnMergeMaterialGoodReqs = new List<QuotationMergeMaterialReq>();
            List<QuotationMergeMaterialReq> returnMergeMaterialSecondReqs = new List<QuotationMergeMaterialReq>();
            List<string> GoodDetailIds = new List<string>();
            List<string> SecondDetailIds = new List<string>();

            if (req.returnnoteMaterials != null)
            {
                foreach (var item in req.returnnoteMaterials)
                {
                    var returnnoteMaterialObj = returnNoteObj.ReturnnoteMaterials.Where(r => r.Id.Equals(item.MaterialsId)).FirstOrDefault();
                    if (item.GoodQty > 0)
                    {
                        var putInMaterialInfo = new QuotationMergeMaterialReq { Id = returnnoteMaterialObj.QuotationMaterialId, InventoryQuantity = item.GoodQty, ReturnNoteId = req.Id, WhsCode = item.GoodWhsCode, MaterialCode = returnnoteMaterialObj.MaterialCode, MaterialDescription = returnnoteMaterialObj.MaterialDescription };
                        returnMergeMaterialGoodReqs.Add(putInMaterialInfo);
                    }
                    if (item.SecondQty > 0)
                    {
                        var putInMaterialInfo = new QuotationMergeMaterialReq { Id = returnnoteMaterialObj.QuotationMaterialId, InventoryQuantity = item.SecondQty, ReturnNoteId = req.Id, WhsCode = item.SecondWhsCode, MaterialCode = returnnoteMaterialObj.MaterialCode, MaterialDescription = returnnoteMaterialObj.MaterialDescription };
                        returnMergeMaterialSecondReqs.Add(putInMaterialInfo);
                    }
                }
            }
            //推送到SAP
            if (returnMergeMaterialGoodReqs.Count > 0)
            {
                _capBus.Publish("Serve.ReceiptCreditVouchers.Create", new AddOrUpdateQuotationReq { InvoiceDocEntry = returnNoteObj.ReturnnoteMaterials.FirstOrDefault()?.InvoiceDocEntry, SalesOrderId = returnNoteObj.SalesOrderId, QuotationMergeMaterialReqs = returnMergeMaterialGoodReqs });
            }
            if (returnMergeMaterialSecondReqs.Count > 0)
            {
                _capBus.Publish("Serve.ReceiptCreditVouchers.Create", new AddOrUpdateQuotationReq { InvoiceDocEntry = returnNoteObj.ReturnnoteMaterials.FirstOrDefault()?.InvoiceDocEntry, SalesOrderId = returnNoteObj.SalesOrderId, QuotationMergeMaterialReqs = returnMergeMaterialSecondReqs });
            }
            ////推送到SAP
            //_capBus.Publish("Serve.AfterSaleReturn.Create", new AddOrUpdateQuotationReq { QuotationMergeMaterialReqs = returnMergeMaterialReqs });
            ////更新退料明细状态
            //await UnitWork.UpdateAsync<ReturnnoteMaterial>(w => GoodDetailIds.Contains(w.Id), u => new ReturnnoteMaterial { IsGoodFinish = 1 });
            //await UnitWork.UpdateAsync<ReturnnoteMaterial>(w => SecondDetailIds.Contains(w.Id), u => new ReturnnoteMaterial { IsSecondFinish = 1 });
            //await UnitWork.SaveAsync();
            ////判断是否全部入库 仓库状态更新为已仓库入库
            //var returnnoteMaterials = await UnitWork.Find<ReturnnoteMaterial>(w => w.ExpressId == req.ExpressageId).ToListAsync();
            //var goodNotFinishCount = returnnoteMaterials.Where(w => w.GoodQty > 0 && w.IsGoodFinish == 0).ToList().Count;
            //var secondNotFinishCount = returnnoteMaterials.Where(w => w.SecondQty > 0 && w.IsSecondFinish == 0).ToList().Count;
            //if (secondNotFinishCount == 0 && goodNotFinishCount == 0)
            //{
            //    await UnitWork.UpdateAsync<Expressage>(w => w.Id == req.ExpressageId, u => new Expressage { Status = 3 });
            //}
            ////判断是否所有退料都已入库
            //var isExist = (await UnitWork.Find<Expressage>(w => w.ReturnNoteId == req.ReturnNoteId && w.Status < 3).ToListAsync()).Count > 0 ? false : true;
            //if (isExist)
            //{
            //    await UnitWork.UpdateAsync<ReturnNote>(w => w.Id == req.ReturnNoteId, u => new ReturnNote { IsCanClear = 1 });
            //}
            //await UnitWork.SaveAsync();


        }
        #endregion
    }
}