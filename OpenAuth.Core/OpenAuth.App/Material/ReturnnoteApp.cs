using Infrastructure;
using KuaiDi100.Common.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
using OpenAuth.App.Material.Response;
using DotNetCore.CAP;

namespace OpenAuth.App
{
    public class ReturnNoteApp : OnlyUnitWorkBaeApp
    {
        private readonly FlowInstanceApp _flowInstanceApp;
        private readonly ExpressageApp _expressageApp;
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        private ICapPublisher _capBus;

        public ReturnNoteApp(IUnitWork unitWork, FlowInstanceApp flowInstanceApp, ModuleFlowSchemeApp moduleFlowSchemeApp, ExpressageApp expressageApp, IAuth auth, ICapPublisher capBus) : base(unitWork, auth)
        {
            _flowInstanceApp = flowInstanceApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _expressageApp = expressageApp;
            _capBus = capBus;
        }
        /// <summary>
        /// 退料
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task ReturnMaterials(ReturnMaterialReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            string userId = loginContext.User.Id;
            string userName = loginContext.User.Name;
            //获取当前用户nsap用户信息
            if (req.AppUserId > 0)
            {
                var userInfo = (await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.AppUserId).Include(a => a.User).FirstOrDefaultAsync())?.User;
                if (userInfo == null)
                {
                    throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
                }
                userId = userInfo.Id;
                userName = userInfo.Name;
            }
            //判断是否已存在最后一次退料
            var isExist = (await UnitWork.Find<ReturnNote>(w => w.ServiceOrderId == req.ServiceOrderId && w.IsLast == 1 && w.CreateUserId == userId).ToListAsync()).Count > 0 ? true : false;
            if (isExist)
            {
                throw new CommonException("已完成退料，无法继续退料", Define.IS_Return_Finish);
            }
            //计算本次退料单 金额总和
            var totalAmt = await UnitWork.Find<Quotation>(w => req.StockOutIds.Contains(w.Id)).SumAsync(s => s.TotalMoney);
            int returnNoteId = 0;
            //判断是否已有退料单 若有则不新增
            var returnNoteInfo = await UnitWork.Find<ReturnNote>(w => w.CreateUserId == userId && w.ServiceOrderSapId == req.SapId).FirstOrDefaultAsync();
            if (returnNoteInfo == null)
            {
                //1.新增退料单主表
                var newNoteInfo = new ReturnNote { ServiceOrderId = req.ServiceOrderId, ServiceOrderSapId = req.SapId, Status = 1, CreateTime = DateTime.Now, CreateUserId = userId, CreateUser = userName, StockOutId = string.Join(",", req.StockOutIds), TotalMoney = totalAmt };
                await UnitWork.AddAsync<ReturnNote, int>(newNoteInfo);
                await UnitWork.SaveAsync();
                returnNoteId = newNoteInfo.Id;
            }
            else
            {
                returnNoteId = returnNoteInfo.Id;
            }
            await UnitWork.SaveAsync();
            //2.添加退料明细信息
            var querySum = from a in UnitWork.Find<ReturnnoteMaterial>(null)
                           join b in UnitWork.Find<ReturnNote>(null) on a.ReturnNoteId equals b.Id into ab
                           from b in ab.DefaultIfEmpty()
                           where b.ServiceOrderId == req.ServiceOrderId && a.Count > 0
                           select new { a.QuotationMaterialId, a.Count };
            var returnMaterials = (await querySum.ToListAsync()).GroupBy(g => g.QuotationMaterialId).Select(s => new { Qty = s.Sum(s => s.Count), Id = s.Key }).ToList();
            List<string> detaiList = new List<string>();
            foreach (ReturnMaterialDetail item in req.ReturnMaterialDetail)
            {
                var newDetailInfo = new ReturnnoteMaterial
                {
                    ReturnNoteId = returnNoteId,
                    MaterialCode = item.MaterialCode,
                    MaterialDescription = item.MaterialDescription,
                    Count = item.ReturnQty == null ? 0 : item.ReturnQty,
                    TotalCount = item.TotalQty,
                    Check = item.ReturnQty > 0 ? 0 : 1,
                    CostPrice = item.CostPrice,
                    QuotationMaterialId = item.QuotationMaterialId,
                    IsGoodFinish = 0,
                    IsSecondFinish = 0
                };
                var detail = await UnitWork.AddAsync<ReturnnoteMaterial, int>(newDetailInfo);
                await UnitWork.SaveAsync();
                detaiList.Add(detail.Id);
                //3.添加退料物料图片
                if (!string.IsNullOrEmpty(item.PictureId))
                {
                    var newPictureInfo = new ReturnNoteMaterialPicture { ReturnnoteMaterialId = detail.Id, PictureId = item.PictureId };
                    await UnitWork.AddAsync(newPictureInfo);
                    await UnitWork.SaveAsync();
                }
                int everQty = (int)(returnMaterials.Where(w => w.Id == item.QuotationMaterialId).FirstOrDefault() == null ? 0 : returnMaterials.Where(w => w.Id == item.QuotationMaterialId).FirstOrDefault()?.Qty);
                item.SurplusQty = (int)(item.TotalQty - everQty - item.ReturnQty);
            }
            //4.添加物流信息
            string expressId = string.Empty;
            if (!string.IsNullOrEmpty(req.TrackNumber))
            {
                var result = await _expressageApp.GetExpressInfo(req.TrackNumber);
                if (result.Code == 200)
                {
                    var response = (string)result.Data;
                    var expressageInfo = await UnitWork.Find<Expressage>(w => w.ReturnNoteId == returnNoteId && w.ExpressNumber == req.TrackNumber).FirstOrDefaultAsync();
                    if (expressageInfo == null)
                    {
                        var express = new Expressage
                        {
                            QuotationId = null,
                            ReturnNoteId = returnNoteId,
                            ExpressNumber = req.TrackNumber,
                            ExpressInformation = response,
                            Freight = req.Freight
                        };
                        await UnitWork.AddAsync(express);
                        expressId = express.Id;
                    }
                    else
                    {
                        expressId = expressageInfo.Id;
                        await UnitWork.UpdateAsync<Expressage>(w => w.Id == expressageInfo.Id, u => new Expressage { ExpressInformation = response });
                    }
                }
                else
                {
                    var express = new Expressage
                    {
                        QuotationId = null,
                        ReturnNoteId = returnNoteId,
                        ExpressNumber = req.TrackNumber,
                        ExpressInformation = string.Empty,
                        CreateTime = DateTime.Now,
                        Freight = req.Freight
                    };
                    await UnitWork.AddAsync(express);
                    expressId = express.Id;
                }
            }
            else
            {
                var express = new Expressage
                {
                    QuotationId = null,
                    ReturnNoteId = returnNoteId,
                    ExpressNumber = "无",
                    ExpressInformation = string.Empty,
                    CreateTime = DateTime.Now,
                    Freight = req.Freight
                };
                await UnitWork.AddAsync(express);
                expressId = express.Id;
            }
            //判断是否上传了物流图片
            if (req.ExpressPictureIds != null)
            {
                var ExpressagePictures = new List<ExpressagePicture>();
                req.ExpressPictureIds.ForEach(p => ExpressagePictures.Add(new ExpressagePicture { ExpressageId = expressId, PictureId = p, Id = Guid.NewGuid().ToString() }));
                await UnitWork.BatchAddAsync(ExpressagePictures.ToArray());
            }
            //5.反写ExpressId至退料明细
            await UnitWork.UpdateAsync<ReturnnoteMaterial>(w => detaiList.Contains(w.Id), u => new ReturnnoteMaterial { ExpressId = expressId });
            //创建物料报价单审批流程
            //if (req.IsLastReturn == 1)
            //{
            //    var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("待退料单"));
            //    var afir = new AddFlowInstanceReq();
            //    afir.SchemeId = mf.FlowSchemeId;
            //    afir.FrmType = 2;
            //    afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
            //    afir.CustomName = $"退料单审批" + DateTime.Now;
            //    afir.OrgId = "";
            //    //保外申请报价单
            //    afir.FrmData = $"{{\"ReturnnoteId\":\"{returnNoteId}\"}}";
            //    var flowinstanceid = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
            //    await UnitWork.UpdateAsync<ReturnNote>(r => r.Id == returnNoteId, r => new ReturnNote
            //    {
            //        FlowInstanceId = flowinstanceid
            //    });
            //}
            await UnitWork.UpdateAsync<ReturnNote>(w => w.Id == returnNoteId, u => new ReturnNote { IsLast = req.IsLastReturn, TotalMoney = totalAmt, Status = req.IsLastReturn == 1 ? 2 : 1 });
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 退料审批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Accraditation(ReturnNoteAuditReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取退料单表头信息
            var returnNote = await UnitWork.Find<ReturnNote>(w => w.Id == req.Id).FirstOrDefaultAsync();
            //仓库验货
            await SaveReceiveInfo(req);
            //验收通过
            await UnitWork.UpdateAsync<ReturnNote>(r => r.Id == req.Id, u => new ReturnNote { Remark = req.Remark });
            ////流程通过
            //_flowInstanceApp.Verification(new VerificationReq
            //{
            //    NodeRejectStep = "",
            //    NodeRejectType = "0",
            //    FlowInstanceId = returnNote.FlowInstanceId,
            //    VerificationFinally = "1",
            //    VerificationOpinion = "同意",
            //});
            //判断是否最后一次退料并且所有退料都已核验
            if (returnNote.IsLast == 1)
            {
                //更新退料单为已完成退料
                await UnitWork.UpdateAsync<ReturnNote>(w => w.Id == req.Id, u => new ReturnNote { Status = 2 });
            }
            //物流单状态更新为已仓库收货
            await UnitWork.UpdateAsync<Expressage>(w => w.Id == req.ExpressageId, u => new Expressage { Status = 1 });
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 仓库验货（保存）
        /// </summary>
        /// <param name="ReturnMaterials"></param>
        /// <returns></returns>
        public async Task SaveReceiveInfo(ReturnNoteAuditReq req)
        {
            //获取退料单表头信息
            var returnNote = await UnitWork.Find<ReturnNote>(w => w.Id == req.Id).FirstOrDefaultAsync();
            //保存验收结果
            if (req.ReturnMaterials != null && req.ReturnMaterials.Count > 0)
            {
                foreach (var item in req.ReturnMaterials)
                {
                    await UnitWork.UpdateAsync<ReturnnoteMaterial>(r => r.Id == item.Id, u => new ReturnnoteMaterial { Check = item.IsPass, ReceivingRemark = item.ReceiveRemark });
                }
            }
            //保存签收备注
            await UnitWork.UpdateAsync<ReturnNote>(r => r.Id == req.Id, u => new ReturnNote { Remark = req.Remark });
            //判断是否最后一次退料并且所有退料都已核验
            var count = (await UnitWork.Find<ReturnnoteMaterial>(w => w.ReturnNoteId == req.Id && w.Check == 0).ToListAsync()).Count;
            if (returnNote.IsLast == 1 && count == 0)
            {
                //更新退料单为已完成退料
                await UnitWork.UpdateAsync<ReturnNote>(w => w.Id == req.Id, u => new ReturnNote { Status = 2 });
            }
            //物流单状态更新为已仓库收货
            await UnitWork.UpdateAsync<Expressage>(w => w.Id == req.ExpressageId, u => new Expressage { Status = 1 });
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 获取退料结果
        /// </summary>
        /// <param name="appUserId"></param>
        /// <param name="ServiceOrderId"></param>
        /// <returns></returns>
        public async Task<TableData> GetReturnNoteInfo(int appUserId, int ServiceOrderId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = (await UnitWork.Find<AppUserMap>(a => a.AppUserId == appUserId).Include(a => a.User).FirstOrDefaultAsync())?.User;
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            var returnNote = await UnitWork.Find<ReturnNote>(w => w.CreateUserId == userInfo.Id && w.ServiceOrderId == ServiceOrderId).FirstOrDefaultAsync();
            var result = new TableData();
            //获取退料列表
            var query = from a in UnitWork.Find<ReturnnoteMaterial>(null)
                        join b in UnitWork.Find<Expressage>(null) on a.ExpressId equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<ReturnNoteMaterialPicture>(null) on a.Id equals c.ReturnnoteMaterialId into abc
                        from c in abc.DefaultIfEmpty()
                        where a.ReturnNoteId == returnNote.Id && a.Count > 0
                        orderby b.CreateTime
                        select new { a.MaterialCode, a.MaterialDescription, a.Count, a.TotalCount, b.ExpressNumber, a.Check, a.ReceivingRemark, a.ShippingRemark, ExpressId = b.Id, c.PictureId, a.Id };
            var detailList = (await query.ToListAsync()).GroupBy(g => new { g.ExpressId, g.ExpressNumber }).Select(s => new { s.Key.ExpressId, s.Key.ExpressNumber, returnNote.IsLast, returnNote.Status, returnNote.Remark, detail = s.ToList() }).ToList();
            result.Data = detailList;
            return result;
        }

        /// <summary>
        /// 获取物流信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<TableData> GetExpressageInfo(string Id)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var ExpressInfo = await UnitWork.Find<Expressage>(w => w.Id == Id).FirstOrDefaultAsync();
            string tracknum = ExpressInfo.ExpressNumber;
            var r = await _expressageApp.GetExpressInfo(tracknum);
            if (r.Code == 200)
            {
                var response = (string)r.Data;
                await UnitWork.UpdateAsync<Expressage>(w => w.Id == Id, u => new Expressage { ExpressInformation = response });
                await UnitWork.SaveAsync();
                result.Data = response;
            }
            else
            {
                return result;
            }

            return result;
        }

        /// <summary>
        /// 获取退料单列表(ERP 技术员退料)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetReturnNoteList(GetReturnNoteListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            //获取退料列表
            var returnNote = await UnitWork.Find<ReturnNote>(null)
              .WhereIf(!string.IsNullOrWhiteSpace(req.Id), q => q.Id.Equals(Convert.ToInt32(req.Id)))
              .WhereIf(!string.IsNullOrWhiteSpace(req.CreaterName), q => q.CreateUser.Equals(req.CreaterName))
              .WhereIf(!string.IsNullOrWhiteSpace(req.BeginDate), q => q.CreateTime >= Convert.ToDateTime(req.BeginDate))
              .WhereIf(!string.IsNullOrWhiteSpace(req.EndDate), q => q.CreateTime < Convert.ToDateTime(req.EndDate))
              .WhereIf(!string.IsNullOrWhiteSpace(req.QutationId), q => q.StockOutId.Contains(req.QutationId))
              .OrderBy(s => s.Id).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            //获取服务单列表
            var serviceOrderList = await UnitWork.Find<ServiceOrder>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(req.SapId), q => q.U_SAP_ID.Equals(Convert.ToInt32(req.SapId)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.Customer), q => q.CustomerName.Equals(req.Customer))
                .ToListAsync();
            var returnNoteList = returnNote.Select(s => new { s.Id, CustomerId = serviceOrderList.Where(w => w.Id == s.ServiceOrderId).Select(s => s.CustomerId).FirstOrDefault(), CustomerName = serviceOrderList.Where(w => w.Id == s.ServiceOrderId).Select(s => s.CustomerName).FirstOrDefault(), s.ServiceOrderId, s.CreateUser, CreateDate = s.CreateTime.ToString("yyyy.MM.dd"), s.ServiceOrderSapId, s.IsCanClear, s.Remark, s.TotalMoney }).ToList();
            result.Data = returnNoteList;
            return result;
        }

        /// <summary>
        /// 获取退料单详情(ERP)
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<TableData> GetReturnNoteDetail(int Id)
        {
            Dictionary<string, object> outData = new Dictionary<string, object>();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            //获取退料单主表详情
            var returnNote = await UnitWork.Find<ReturnNote>(w => w.Id == Id).FirstOrDefaultAsync();
            //获取服务单详情
            var serviceOrder = await UnitWork.Find<ServiceOrder>(w => w.Id == returnNote.ServiceOrderId && w.U_SAP_ID == returnNote.ServiceOrderSapId).FirstOrDefaultAsync();
            var mainInfo = new Dictionary<string, object>()
            {
                {"returnNoteCode" ,returnNote.Id},{"creater",returnNote.CreateUser },{ "createTime",returnNote.CreateTime },{"salMan",serviceOrder.SalesMan},{ "serviceSapId",serviceOrder.U_SAP_ID},{ "customerCode",serviceOrder.TerminalCustomerId},{ "customerName",serviceOrder.TerminalCustomer},{ "status",returnNote.Status},{ "isLast",returnNote.IsLast},{ "remark",(string.IsNullOrEmpty(returnNote.Remark)? string.Empty:returnNote.Remark)},{ "stockOutId",returnNote.StockOutId},{ "contacter",serviceOrder.NewestContacter},{"contacterTel",serviceOrder.NewestContactTel },{"serviceOrderId",serviceOrder.Id }
            };
            outData.Add("mainInfo", mainInfo);
            //获取物流信息
            var expressList = await UnitWork.Find<Expressage>(w => w.ReturnNoteId == Id).Include(i => i.ExpressagePicture).OrderByDescending(o => o.CreateTime).Select(s => new { s.ExpressNumber, s.ExpressInformation, s.Remark, s.Id, s.Freight, s.ExpressagePicture }).ToListAsync();
            outData.Add("expressList", expressList);
            //获取当前服务单所有退料明细汇总
            var querySum = from a in UnitWork.Find<ReturnnoteMaterial>(null)
                           join b in UnitWork.Find<ReturnNote>(null) on a.ReturnNoteId equals b.Id into ab
                           from b in ab.DefaultIfEmpty()
                           where b.ServiceOrderId == returnNote.ServiceOrderId && a.Count > 0
                           select new { a.QuotationMaterialId, a.Count };
            var returnMaterials = (await querySum.ToListAsync()).GroupBy(g => g.QuotationMaterialId).Select(s => new { Qty = s.Sum(s => s.Count), Id = s.Key }).ToList();
            //获取退料单中所有的物流集合
            var query = from a in UnitWork.Find<ReturnnoteMaterial>(null)
                        join b in UnitWork.Find<Expressage>(null) on a.ExpressId equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<ReturnNoteMaterialPicture>(null) on a.Id equals c.ReturnnoteMaterialId into abc
                        from c in abc.DefaultIfEmpty()
                        where a.ReturnNoteId == Id && a.Count > 0
                        orderby b.CreateTime
                        select new ReturnMaterialDetailResp { MaterialCode = a.MaterialCode, MaterialDescription = a.MaterialDescription, Count = (int)a.Count, Check = (int)a.Check, ReceivingRemark = a.ReceivingRemark, ShippingRemark = a.ShippingRemark, ExpressId = b.Id, PictureId = c.PictureId, Id = a.Id, GoodQty = (int)a.GoodQty, SecondQty = (int)a.SecondQty, TotalCount = (int)a.TotalCount, MaterialId = a.QuotationMaterialId };
            var data = await query.ToListAsync();
            foreach (var item in data)
            {
                int everQty = (int)(returnMaterials.Where(w => w.Id == item.MaterialId).FirstOrDefault() == null ? 0 : returnMaterials.Where(w => w.Id == item.MaterialId).FirstOrDefault()?.Qty);
                item.SurplusQty = item.TotalCount - everQty;
            }
            var detailList = data.GroupBy(g => g.ExpressId).Select(s => new { s.Key, detail = s.ToList() }).ToList();
            outData.Add("materialList", detailList);
            result.Data = outData;
            return result;
        }

        /// <summary>
        /// 获取退料结算列表(ERP)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetClearReturnNoteList(GetClearReturnNoteListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            //获取已完成退料并且所有退料单仓库核验通过的退料单集合
            //获取退料列表
            var returnNote = await UnitWork.Find<ReturnNote>(w => w.IsLast == 1 && w.Status == 2 && w.IsCanClear == 1)
              .WhereIf(!string.IsNullOrWhiteSpace(req.CreaterName), q => q.CreateUser.Equals(req.CreaterName))
              .WhereIf(!string.IsNullOrWhiteSpace(req.BeginDate), q => q.CreateTime >= Convert.ToDateTime(req.BeginDate))
              .WhereIf(!string.IsNullOrWhiteSpace(req.EndDate), q => q.CreateTime < Convert.ToDateTime(req.EndDate))
              .OrderBy(s => s.Id).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            //获取服务单列表
            var serviceOrderList = await UnitWork.Find<ServiceOrder>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(req.SapId), q => q.U_SAP_ID.Equals(Convert.ToInt32(req.SapId)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.Customer), q => q.CustomerName.Contains(req.Customer) || q.CustomerId.Contains(req.Customer))
                .ToListAsync();
            //获取退料单Id集合
            List<int> returnNoteIds = returnNote.Select(s => s.Id).Distinct().ToList();
            //计算剩余未结清金额
            var notClearAmountList = (await UnitWork.Find<ReturnnoteMaterial>(w => returnNoteIds.Contains((int)w.ReturnNoteId)).ToListAsync()).GroupBy(g => new { g.ReturnNoteId, g.MaterialCode }).Select(s => new { s.Key.ReturnNoteId, s.Key.MaterialCode, Count = s.Sum(s => s.Count), TotalPassCount = s.Sum(s => s.SecondQty + s.GoodQty), Costprice = s.ToList().FirstOrDefault().CostPrice, TotalCount = s.ToList().FirstOrDefault().TotalCount }).ToList();
            var AmountList = notClearAmountList.GroupBy(g => g.ReturnNoteId).Select(s => new { s.Key, Amount = s.Sum(s => s.Costprice * (s.TotalCount - s.TotalPassCount)) }).ToList();

            var returnNoteList = returnNote.Select(s => new { CustomerId = serviceOrderList.Where(w => w.Id == s.ServiceOrderId).Select(s => s.TerminalCustomerId).FirstOrDefault(), CustomerName = serviceOrderList.Where(w => w.Id == s.ServiceOrderId).Select(s => s.TerminalCustomer).FirstOrDefault(), s.ServiceOrderId, s.CreateUser, CreateDate = s.CreateTime.ToString("yyyy.mm.dd"), s.ServiceOrderSapId, s.CreateUserId, s.Id, notClearAmount = Math.Round((decimal)AmountList.Where(w => w.Key == s.Id).FirstOrDefault().Amount, 2), Status = Math.Round((decimal)AmountList.Where(w => w.Key == s.Id).FirstOrDefault().Amount, 2) > 0 ? "未清" : "已清", s.Remark }).ToList().GroupBy(g => new { g.Id }).Select(s => new { s.Key, detail = s.ToList() }).ToList();
            result.Data = returnNoteList;
            return result;
        }

        /// <summary>
        /// 获取退料结算详情(ERP)
        /// </summary>
        /// <param name="Id">退料单Id</param>
        /// <returns></returns>
        public async Task<TableData> GetClearReturnNoteDetail(int Id)
        {
            Dictionary<string, dynamic> outData = new Dictionary<string, dynamic>();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            //获取退料主表信息
            var returnNoteInfo = await UnitWork.Find<ReturnNote>(w => w.Id == Id).FirstOrDefaultAsync();
            outData.Add("CreateTime", returnNoteInfo.CreateTime.ToString("yyyy-MM-dd HH:mm"));
            outData.Add("CreateUser", returnNoteInfo.CreateUser);
            outData.Add("ReturnNoteId", returnNoteInfo.Id);
            outData.Add("StockOutId", returnNoteInfo.StockOutId);
            //获取当前服务单信息
            var serviceOrderInfo = await UnitWork.Find<ServiceOrder>(w => w.U_SAP_ID == returnNoteInfo.ServiceOrderSapId).FirstOrDefaultAsync();
            outData.Add("U_SAP_ID", serviceOrderInfo.U_SAP_ID);
            outData.Add("SalesMan", serviceOrderInfo.SalesMan);
            outData.Add("CustomerId", serviceOrderInfo.TerminalCustomerId);
            outData.Add("CustomerName", serviceOrderInfo.TerminalCustomer);
            outData.Add("Contacter", serviceOrderInfo.NewestContacter);
            outData.Add("ContactTel", serviceOrderInfo.NewestContactTel);
            //获取领料单详情
            List<int> quotationIds = new List<int>();
            if (!string.IsNullOrEmpty(returnNoteInfo.StockOutId))
            {
                var arr = returnNoteInfo.StockOutId.Split(",");
                for (int i = 0; i < arr.Length; i++)
                {
                    if (!quotationIds.Contains(Convert.ToInt32(arr[i])))
                    {
                        quotationIds.Add(Convert.ToInt32(arr[i]));
                    }
                }
            }
            var qutationMaterials = (await UnitWork.Find<QuotationMergeMaterial>(q => quotationIds.Contains((int)q.QuotationId) && q.IsProtected == true).ToListAsync()).GroupBy(g => g.MaterialCode).Select(s => new { s.Key, Qty = s.Sum(s => s.Count) }).ToList();
            //计算剩余未结清金额
            decimal? notClearAmount = 0;
            //获取物料信息
            var MaterialList = (await UnitWork.Find<ReturnnoteMaterial>(w => w.ReturnNoteId == returnNoteInfo.Id).ToListAsync()).GroupBy(g => g.MaterialCode).Select(s => new
            {
                MaterialCode = s.Key,
                MaterDescription = s.Where(w => w.MaterialCode == s.Key).FirstOrDefault().MaterialDescription,
                AlreadyReturnQty = s.Where(w => w.MaterialCode == s.Key).Sum(k => k.Count),
                TotalReturnCount = qutationMaterials.Where(w => w.Key == s.Key).FirstOrDefault().Qty,
                NotClearAmount = s.Where(w => w.MaterialCode == s.Key).FirstOrDefault().CostPrice * (qutationMaterials.Where(w => w.Key == s.Key).FirstOrDefault().Qty - s.Where(w => w.MaterialCode == s.Key).Sum(k => k.GoodQty) - s.Where(w => w.MaterialCode == s.Key).Sum(k => k.SecondQty)),
                Status = s.Where(w => w.MaterialCode == s.Key).FirstOrDefault().CostPrice * (qutationMaterials.Where(w => w.Key == s.Key).FirstOrDefault().Qty - s.Where(w => w.MaterialCode == s.Key).Sum(k => k.GoodQty) - s.Where(w => w.MaterialCode == s.Key).Sum(k => k.SecondQty)) > 0 ? "未清" : "已清"
            }).ToList();
            MaterialList.ForEach(f => notClearAmount += f.NotClearAmount);
            outData.Add("NotClearAmount", Math.Round((decimal)notClearAmount, 2));
            outData.Add("DetailList", MaterialList);
            result.Data = outData;
            return result;
        }

        /// <summary>
        /// 获取可退料的服务单集合(ERP)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetServiceOrderInfo(PageReq req)
        {
            Dictionary<string, dynamic> outData = new Dictionary<string, dynamic>();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            //查询当前技术员所有可退料服务Id
            var query = from a in UnitWork.Find<QuotationMergeMaterial>(null)
                        join b in UnitWork.Find<Quotation>(null) on a.QuotationId equals b.Id
                        where b.CreateUserId == loginContext.User.Id && a.IsProtected == true
                        select new { b.ServiceOrderId };
            var serviceorderIds = (await query.ToListAsync()).Select(s => s.ServiceOrderId).Distinct().ToList();
            //排除已生成退料单的服务Id
            var returnOrderIds = await UnitWork.Find<ReturnNote>(w => w.CreateUserId == loginContext.User.Id).Select(s => s.ServiceOrderId).Distinct().ToListAsync();
            if (returnOrderIds.Count > 0)
            {
                returnOrderIds.ForEach(f => serviceorderIds.Remove(f));
            }
            var data = await UnitWork.Find<ServiceOrder>(s => serviceorderIds.Contains(s.Id)).Select(s => new { s.Id, CustomerId = s.TerminalCustomerId, CustomerName = s.TerminalCustomer, Contacter = s.NewestContacter, ContactTel = s.NewestContactTel, s.U_SAP_ID }).ToListAsync();
            result.Data = data.Skip((req.page - 1) * req.limit).Take(req.limit).ToList();
            result.Count = data.Count;
            return result;
        }

        /// <summary>
        /// 品质检验(ERP)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task CheckOutMaterials(CheckOutMaterialsReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //查询需要做检验的退料明细
            var returnMaterialDetails = await UnitWork.Find<ReturnnoteMaterial>(w => req.DetailIds.Contains(w.Id)).ToListAsync();
            foreach (var item in req.checkOutMaterials)
            {
                var detail = returnMaterialDetails.Where(w => w.Id == item.Id).FirstOrDefault();
                detail.GoodQty = item.GoodQty;
                detail.SecondQty = item.SecondQty;
            }
            await UnitWork.BatchUpdateAsync(returnMaterialDetails.ToArray());
            //更新为已品质检验
            await UnitWork.UpdateAsync<Expressage>(w => w.Id == req.ExpressageId, u => new Expressage { Status = 2 });
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 获取退料单列表(ERP 仓库收货/品质入库/仓库入库)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetReturnNoteListByExpress(GetReturnNoteListByExpressReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            ////获取退料列表
            var returnNoteIds = await UnitWork.Find<ReturnNote>(null)
              .WhereIf(!string.IsNullOrWhiteSpace(req.Id), q => q.Id.Equals(Convert.ToInt32(req.Id)))
              .WhereIf(!string.IsNullOrWhiteSpace(req.CreaterName), q => q.CreateUser.Equals(req.CreaterName))
              .WhereIf(!string.IsNullOrWhiteSpace(req.BeginDate), q => q.CreateTime >= Convert.ToDateTime(req.BeginDate))
              .WhereIf(!string.IsNullOrWhiteSpace(req.EndDate), q => q.CreateTime < Convert.ToDateTime(req.EndDate))
              .WhereIf(!string.IsNullOrWhiteSpace(req.QutationId), q => q.StockOutId.Contains(req.QutationId))
              .OrderBy(s => s.Id).Select(s => s.Id).Distinct().ToListAsync();
            ////获取服务单列表
            var serviceOrderList = await UnitWork.Find<ServiceOrder>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(req.SapId), q => q.U_SAP_ID.Equals(Convert.ToInt32(req.SapId)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.Customer), q => q.CustomerName.Equals(req.Customer))
                .ToListAsync();
            var query = from a in UnitWork.Find<Expressage>(null)
                        join b in UnitWork.Find<ReturnNote>(null) on a.ReturnNoteId equals b.Id
                        where returnNoteIds.Contains((int)a.ReturnNoteId) && a.Status == Convert.ToInt32(req.Status) && (!string.IsNullOrEmpty(req.TrackNumber) ? a.ExpressNumber == req.TrackNumber : true)
                        select new ReturnNoteMainResp { ExpressNumber = a.ExpressNumber, ExpressId = a.Id, Id = b.Id, ServiceOrderId = b.ServiceOrderId, CreateUser = b.CreateUser, CreateDate = b.CreateTime.ToString("yyyy.MM.dd"), ServiceOrderSapId = b.ServiceOrderSapId, IsCanClear = b.IsCanClear, Remark = b.Remark, TotalMoney = (decimal)b.TotalMoney };
            var data = await query.Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            foreach (var item in data)
            {
                item.CustomerId = serviceOrderList.Where(w => w.Id == item.ServiceOrderId).FirstOrDefault()?.TerminalCustomerId;
                item.CustomerName = serviceOrderList.Where(w => w.Id == item.ServiceOrderId).FirstOrDefault()?.TerminalCustomer;
            }
            result.Data = data;
            return result;
        }

        /// <summary>
        /// 获取退料单详情（根据物流单号）
        /// </summary>
        /// <param name="ExpressageId"></param>
        /// <returns></returns>
        public async Task<TableData> GetReturnNoteDetailByExpress(string ExpressageId)
        {
            Dictionary<string, object> outData = new Dictionary<string, object>();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            //获取物流信息
            var expressList = await UnitWork.Find<Expressage>(w => w.Id == ExpressageId).Include(i => i.ExpressagePicture).Select(s => new { s.ExpressNumber, s.ExpressInformation, s.Remark, s.Id, s.Freight, s.ReturnNoteId, s.ExpressagePicture }).FirstOrDefaultAsync();
            //获取退料单主表详情
            var returnNote = await UnitWork.Find<ReturnNote>(w => w.Id == expressList.ReturnNoteId).FirstOrDefaultAsync();
            //获取服务单详情
            var serviceOrder = await UnitWork.Find<ServiceOrder>(w => w.Id == returnNote.ServiceOrderId && w.U_SAP_ID == returnNote.ServiceOrderSapId).FirstOrDefaultAsync();
            var mainInfo = new Dictionary<string, object>()
            {
                {"returnNoteCode" ,returnNote.Id},{"creater",returnNote.CreateUser },{ "createTime",returnNote.CreateTime },{"salMan",serviceOrder.SalesMan},{ "serviceSapId",serviceOrder.U_SAP_ID},{ "customerCode",serviceOrder.TerminalCustomerId},{ "customerName",serviceOrder.TerminalCustomer},{ "status",returnNote.Status},{ "isLast",returnNote.IsLast},{ "remark",(string.IsNullOrEmpty(returnNote.Remark)? string.Empty:returnNote.Remark)},{ "stockOutId",returnNote.StockOutId},{ "contacter",serviceOrder.NewestContacter},{"contacterTel",serviceOrder.NewestContactTel },{"serviceOrderId",serviceOrder.Id }
            };
            outData.Add("mainInfo", mainInfo);
            outData.Add("expressList", expressList);
            //获取当前服务单所有退料明细汇总
            var querySum = from a in UnitWork.Find<ReturnnoteMaterial>(null)
                           join b in UnitWork.Find<ReturnNote>(null) on a.ReturnNoteId equals b.Id into ab
                           from b in ab.DefaultIfEmpty()
                           where b.ServiceOrderId == returnNote.ServiceOrderId && a.Count > 0
                           select new { a.QuotationMaterialId, a.Count };
            var returnMaterials = (await querySum.ToListAsync()).GroupBy(g => g.QuotationMaterialId).Select(s => new { Qty = s.Sum(s => s.Count), Id = s.Key }).ToList();

            //获取退料单中所有的物流集合
            var query = from a in UnitWork.Find<ReturnnoteMaterial>(null)
                        join b in UnitWork.Find<ReturnNoteMaterialPicture>(null) on a.Id equals b.ReturnnoteMaterialId into ab
                        from b in ab.DefaultIfEmpty()
                        where a.ExpressId == ExpressageId && a.Count > 0
                        select new ReturnMaterialDetailResp { MaterialCode = a.MaterialCode, MaterialDescription = a.MaterialDescription, Count = (int)a.Count, Check = (int)a.Check, ReceivingRemark = a.ReceivingRemark, ShippingRemark = a.ShippingRemark, ExpressId = b.Id, PictureId = b.PictureId, Id = a.Id, GoodQty = (int)a.GoodQty, SecondQty = (int)a.SecondQty, TotalCount = (int)a.TotalCount, MaterialId = a.QuotationMaterialId, IsGoodFinish = (int)a.IsGoodFinish, IsSecondFinish = (int)a.IsSecondFinish };
            var data = await query.ToListAsync();
            foreach (var item in data)
            {
                int everQty = (int)(returnMaterials.Where(w => w.Id == item.MaterialId).FirstOrDefault() == null ? 0 : returnMaterials.Where(w => w.Id == item.MaterialId).FirstOrDefault()?.Qty);
                item.SurplusQty = item.TotalCount - everQty;
            }
            outData.Add("materialList", data);
            result.Data = outData;
            return result;
        }

        /// <summary>
        /// 仓库入库
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task WarehousePutMaterialsIn(WarehousePutMaterialsInReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            List<QuotationMergeMaterialReq> returnMergeMaterialReqs = new List<QuotationMergeMaterialReq>();
            List<string> GoodDetailIds = new List<string>();
            List<string> SecondDetailIds = new List<string>();
            if (req.putInMaterials != null)
            {
                foreach (var item in req.putInMaterials)
                {
                    if (item.Qty > 0)
                    {
                        var putInMaterialInfo = new QuotationMergeMaterialReq { Id = item.MaterialId, InventoryQuantity = item.Qty, ReturnNoteId = req.ReturnNoteId, WhsCode = item.WhsCode.ToString() };
                        returnMergeMaterialReqs.Add(putInMaterialInfo);
                    }
                    if (item.WhsCode == 37)
                    {
                        GoodDetailIds.Add(item.Id);
                    }
                    else if (item.WhsCode == 39)
                    {
                        SecondDetailIds.Add(item.Id);
                    }
                }
            }
            //推送到SAP
            _capBus.Publish("Serve.AfterSaleReturn.Create", new AddOrUpdateQuotationReq { QuotationMergeMaterialReqs = returnMergeMaterialReqs });
            //更新退料明细状态
            await UnitWork.UpdateAsync<ReturnnoteMaterial>(w => GoodDetailIds.Contains(w.Id), u => new ReturnnoteMaterial { IsGoodFinish = 1 });
            await UnitWork.UpdateAsync<ReturnnoteMaterial>(w => SecondDetailIds.Contains(w.Id), u => new ReturnnoteMaterial { IsSecondFinish = 1 });
            await UnitWork.SaveAsync();
            //判断是否全部入库 仓库状态更新为已仓库入库
            var returnnoteMaterials = await UnitWork.Find<ReturnnoteMaterial>(w => w.ExpressId == req.ExpressageId).ToListAsync();
            var goodNotFinishCount = returnnoteMaterials.Where(w => w.GoodQty > 0 && w.IsGoodFinish == 0).ToList().Count;
            var secondNotFinishCount = returnnoteMaterials.Where(w => w.SecondQty > 0 && w.IsSecondFinish == 0).ToList().Count;
            if (secondNotFinishCount == 0 && goodNotFinishCount == 0)
            {
                await UnitWork.UpdateAsync<Expressage>(w => w.Id == req.ExpressageId, u => new Expressage { Status = 3 });
            }
            //判断是否所有退料都已入库
            var isExist = (await UnitWork.Find<Expressage>(w => w.ReturnNoteId == req.ReturnNoteId && w.Status < 3).ToListAsync()).Count > 0 ? false : true;
            if (isExist)
            {
                await UnitWork.UpdateAsync<ReturnNote>(w => w.Id == req.ReturnNoteId, u => new ReturnNote { IsCanClear = 1 });
            }
            await UnitWork.SaveAsync();
        }

    }
}