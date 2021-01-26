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
namespace OpenAuth.App
{
    public class ReturnNoteApp : OnlyUnitWorkBaeApp
    {
        private readonly FlowInstanceApp _flowInstanceApp;
        private readonly ExpressageApp _expressageApp;
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        public ReturnNoteApp(IUnitWork unitWork, FlowInstanceApp flowInstanceApp, ModuleFlowSchemeApp moduleFlowSchemeApp, ExpressageApp expressageApp, IAuth auth) : base(unitWork, auth)
        {
            _flowInstanceApp = flowInstanceApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _expressageApp = expressageApp;
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
            //获取当前用户nsap用户信息
            var userInfo = (await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.AppUserId).Include(a => a.User).FirstOrDefaultAsync())?.User;
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            //判断是否已存在最后一次退料
            var isExist = (await UnitWork.Find<ReturnNote>(w => w.ServiceOrderId == req.ServiceOrderId && w.IsLast == 1 && w.CreateUserId == userInfo.Id).ToListAsync()).Count > 0 ? true : false;
            if (isExist)
            {
                throw new CommonException("已完成退料，无法继续退料", Define.IS_Return_Finish);
            }
            int returnNoteId = 0;
            //判断是否已有退料单 若有则不新增
            var returnNoteInfo = await UnitWork.Find<ReturnNote>(w => w.CreateUserId == userInfo.Id && w.ServiceOrderSapId == req.SapId).FirstOrDefaultAsync();
            if (returnNoteInfo == null)
            {
                //1.新增退料单主表
                var newNoteInfo = new ReturnNote { ServiceOrderId = req.ServiceOrderId, ServiceOrderSapId = req.SapId, Status = 1, CreateTime = DateTime.Now, CreateUserId = userInfo.Id, CreateUser = userInfo.Name, IsLast = req.IsLastReturn, StockOutId = req.StockOutId };
                var o = await UnitWork.AddAsync<ReturnNote, int>(newNoteInfo);
                returnNoteId = o.Id;
            }
            else
            {
                await UnitWork.UpdateAsync<ReturnNote>(w => w.Id == returnNoteId, u => new ReturnNote { IsLast = req.IsLastReturn });
                returnNoteId = returnNoteInfo.Id;
            }
            await UnitWork.SaveAsync();
            //2.添加退料明细信息
            List<string> detaiList = new List<string>();
            foreach (ReturnMaterialDetail item in req.ReturnMaterialDetail)
            {
                var newDetailInfo = new ReturnnoteMaterial
                {
                    ReturnNoteId = returnNoteId,
                    MaterialCode = item.MaterialCode,
                    MaterialDescription = item.MaterialDescription,
                    Count = item.ReturnQty,
                    TotalCount = item.TotalQty,
                    Check = 0,
                    CostPrice = item.CostPrice
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
                            ExpressInformation = response
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
            }
            else
            {
                var express = new Expressage
                {
                    QuotationId = null,
                    ReturnNoteId = returnNoteId,
                    ExpressNumber = "无",
                    ExpressInformation = string.Empty,
                    CreateTime = DateTime.Now
                };
                await UnitWork.AddAsync(express);
                expressId = express.Id;
            }
            //5.反写ExpressId至退料明细
            await UnitWork.UpdateAsync<ReturnnoteMaterial>(w => detaiList.Contains(w.Id), u => new ReturnnoteMaterial { ExpressId = expressId });
            //创建物料报价单审批流程
            if (req.IsLastReturn == 1)
            {
                var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("待退料单"));
                var afir = new AddFlowInstanceReq();
                afir.SchemeId = mf.FlowSchemeId;
                afir.FrmType = 2;
                afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                afir.CustomName = $"退料单审批" + DateTime.Now;
                afir.OrgId = "";
                //保外申请报价单
                afir.FrmData = $"{{\"ReturnnoteId\":\"{returnNoteId}\"}}";
                var flowinstanceid = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                await UnitWork.UpdateAsync<ReturnNote>(r => r.Id == returnNoteId, r => new ReturnNote
                {
                    FlowInstanceId = flowinstanceid
                });
            }
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
            await UnitWork.UpdateAsync<ReturnNote>(r => r.FlowInstanceId == returnNote.FlowInstanceId, u => new ReturnNote { Status = 2, Remark = req.Remark });
            //流程通过
            _flowInstanceApp.Verification(new VerificationReq
            {
                NodeRejectStep = "",
                NodeRejectType = "0",
                FlowInstanceId = returnNote.FlowInstanceId,
                VerificationFinally = "1",
                VerificationOpinion = "同意",
            });
            //判断是否最后一次退料并且所有退料都已核验通过
            var isExistLast = (await UnitWork.Find<ReturnNote>(w => w.ServiceOrderSapId == returnNote.ServiceOrderSapId && w.CreateUserId == returnNote.CreateUserId && returnNote.Status == 2 && returnNote.IsLast == 1).ToListAsync()).Count;
            if (isExistLast > 0)
            {
                //更新退料单为可结算状态
                await UnitWork.UpdateAsync<ReturnNote>(w => w.ServiceOrderSapId == returnNote.ServiceOrderSapId && w.CreateUserId == returnNote.CreateUserId, u => new ReturnNote { IsCanClear = 1 });
            }
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 仓库验货（保存）
        /// </summary>
        /// <param name="ReturnMaterials"></param>
        /// <returns></returns>
        public async Task SaveReceiveInfo(ReturnNoteAuditReq req)
        {
            //保存验收结果
            if (req.ReturnMaterials != null && req.ReturnMaterials.Count > 0)
            {
                foreach (var item in req.ReturnMaterials)
                {
                    await UnitWork.UpdateAsync<ReturnnoteMaterial>(r => r.Id == item.Id, u => new ReturnnoteMaterial { Check = item.IsPass, WrongCount = item.WrongCount, ReceivingRemark = item.ReceiveRemark });
                }
            }
            //保存签收备注
            await UnitWork.UpdateAsync<ReturnNote>(r => r.Id == req.Id, u => new ReturnNote { Remark = req.Remark });
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
            var result = new TableData();
            //获取退料列表
            var query = from a in UnitWork.Find<ReturnNote>(null)
                        join b in UnitWork.Find<ReturnnoteMaterial>(null) on a.Id equals b.ReturnNoteId into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<ReturnNoteMaterialPicture>(null) on b.Id equals c.ReturnnoteMaterialId into abc
                        from c in abc.DefaultIfEmpty()
                        join d in UnitWork.Find<Expressage>(null) on a.Id equals d.ReturnNoteId into abcd
                        from d in abcd.DefaultIfEmpty()
                        where a.ServiceOrderId == ServiceOrderId && a.CreateUserId == userInfo.Id
                        select new { a, b, c, d };
            var returnNoteList = await query.Select(s => new { s.b.MaterialCode, s.b.Id, s.b.Count, s.b.TotalCount, s.c.PictureId, s.b.Check, returnNoteId = s.a.Id, s.b.WrongCount, s.b.ReceivingRemark, s.b.ShippingRemark, s.d.ExpressNumber, s.a.Status, s.a.IsLast, s.a.Remark }).OrderByDescending(o => o.returnNoteId).ToListAsync();
            result.Data = returnNoteList;
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
        /// 获取退料单列表(ERP)
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
              .WhereIf(!string.IsNullOrWhiteSpace(req.Status), q => q.Status == Convert.ToInt32(req.Status))
              .OrderBy(s => s.Id).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            //获取服务单列表
            var serviceOrderList = await UnitWork.Find<ServiceOrder>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(req.SapId), q => q.U_SAP_ID.Equals(Convert.ToInt32(req.SapId)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.Customer), q => q.CustomerName.Equals(req.Customer))
                .ToListAsync();
            var returnNoteList = returnNote.Select(s => new { s.Id, CustomerId = serviceOrderList.Where(w => w.Id == s.ServiceOrderId).Select(s => s.CustomerId).FirstOrDefault(), CustomerName = serviceOrderList.Where(w => w.Id == s.ServiceOrderId).Select(s => s.CustomerName).FirstOrDefault(), s.ServiceOrderId, s.CreateUser, CreateDate = s.CreateTime.ToString("yyyy.MM.dd"), s.ServiceOrderSapId, s.IsCanClear, s.Remark }).ToList();
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
                {"returnNoteCode" ,returnNote.Id},{"creater",returnNote.CreateUser },{ "createTime",returnNote.CreateTime },{"salMan",serviceOrder.SalesMan},{ "serviceSapId",serviceOrder.U_SAP_ID},{ "customerCode",serviceOrder.CustomerId},{ "customerName",serviceOrder.CustomerName},{ "status",returnNote.Status},{ "isLast",returnNote.IsLast},{ "remark",(string.IsNullOrEmpty(returnNote.Remark)? string.Empty:returnNote.Remark)},{ "stockOutId",returnNote.StockOutId},{ "contacter",serviceOrder.Contacter},{"contacterTel",serviceOrder.ContactTel }
            };
            outData.Add("mainInfo", mainInfo);
            //获取物流信息
            var expressList = await UnitWork.Find<Expressage>(w => w.ReturnNoteId == Id).OrderByDescending(o => o.CreateTime).Select(s => new { s.ExpressNumber, s.ExpressInformation, s.Remark }).ToListAsync();
            outData.Add("expressList", expressList);
            //获取退料单中所有的物流集合
            var query = from a in UnitWork.Find<ReturnnoteMaterial>(null)
                        join b in UnitWork.Find<Expressage>(null) on a.ExpressId equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<ReturnNoteMaterialPicture>(null) on a.Id equals c.ReturnnoteMaterialId into abc
                        from c in abc.DefaultIfEmpty()
                        where a.ReturnNoteId == Id
                        orderby b.CreateTime
                        select new { a.MaterialCode, a.MaterialDescription, a.Count, a.Check, a.ReceivingRemark, a.ShippingRemark, b.Id, c.PictureId };
            var detailList = (await query.ToListAsync()).GroupBy(g => g.Id).Select(s => new { s.Key, detail = s.ToList() }).ToList();
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
            var returnNoteList = returnNote.Select(s => new { CustomerId = serviceOrderList.Where(w => w.Id == s.ServiceOrderId).Select(s => s.CustomerId).FirstOrDefault(), CustomerName = serviceOrderList.Where(w => w.Id == s.ServiceOrderId).Select(s => s.CustomerName).FirstOrDefault(), s.ServiceOrderId, s.CreateUser, CreateDate = s.CreateTime.ToString("yyyy.mm.dd"), s.ServiceOrderSapId, s.CreateUserId, s.Id }).ToList().GroupBy(g => new { g.Id }).Select(s => new { s.Key, detail = s.ToList() }).ToList();
            result.Data = returnNoteList;
            return result;
        }

        /// <summary>
        /// 获取退料结算详情(ERP)
        /// </summary>
        /// <param name="serviceSapId"></param>
        /// <param name="createrId"></param>
        /// <returns></returns>
        public async Task<TableData> GetClearReturnNoteDetail(int serviceSapId, string createrId)
        {
            Dictionary<string, dynamic> outData = new Dictionary<string, dynamic>();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            //获取当前服务单信息
            var serviceOrderInfo = await UnitWork.Find<ServiceOrder>(w => w.U_SAP_ID == serviceSapId).FirstOrDefaultAsync();
            outData.Add("U_SAP_ID", serviceOrderInfo.U_SAP_ID);
            outData.Add("SalesMan", serviceOrderInfo.SalesMan);
            outData.Add("CustomerId", serviceOrderInfo.CustomerId);
            outData.Add("CustomerName", serviceOrderInfo.CustomerName);
            outData.Add("Contacter", serviceOrderInfo.Contacter);
            outData.Add("ContactTel", serviceOrderInfo.ContactTel);
            //获取退料主表信息
            var returnNoteInfo = await UnitWork.Find<ReturnNote>(w => w.ServiceOrderSapId == serviceSapId && w.CreateUserId == createrId).FirstOrDefaultAsync();
            outData.Add("CreateTime", returnNoteInfo.CreateTime.ToString("yyyy-MM-dd HH:mm"));
            outData.Add("CreateUser", returnNoteInfo.CreateUser);
            outData.Add("ReturnNoteId", returnNoteInfo.Id);
            outData.Add("StockOutId", returnNoteInfo.StockOutId);
            //获取退料单中所有的物流集合
            var query = from a in UnitWork.Find<ReturnnoteMaterial>(null)
                        join b in UnitWork.Find<Expressage>(null) on a.ExpressId equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        where a.ReturnNoteId == returnNoteInfo.Id
                        orderby b.CreateTime
                        select new { a.MaterialCode, a.MaterialDescription, a.Count, b.ExpressagePicture, a.Check, a.ReceivingRemark, a.ShippingRemark, b.ExpressNumber, b.Id, b.ExpressInformation, b.Remark };
            var detailList = (await query.ToListAsync()).GroupBy(g => g.Id).Select(s => new { s.Key, detail = s.ToList() }).ToList();

            //var expressList = await UnitWork.Find<Expressage>(w => w.ReturnNoteId == returnNoteInfo.Id).OrderByDescending(o => o.CreateTime).Select(s => new { s.ExpressNumber, s.ExpressInformation, s.Remark }).ToListAsync();
            //outData.Add("ExpressList", returnNoteInfo.Id);
            //var MaterialList = (await UnitWork.Find<ReturnnoteMaterial>(w => returnNoteIds.Contains((int)w.ReturnNoteId)).ToListAsync()).GroupBy(g => g.MaterialCode).Select(s => new
            //{
            //    MaterialCode = s.Key,
            //    MaterDescription = s.Where(w => w.MaterialCode == s.Key).FirstOrDefault().MaterialDescription,
            //    AlreadyReturnQty = s.Where(w => w.MaterialCode == s.Key).Sum(k => k.Count),
            //    TotalReturnCount = s.Where(w => w.MaterialCode == s.Key).Sum(k => k.TotalCount),
            //    CostPrice = s.Where(w => w.MaterialCode == s.Key).FirstOrDefault().CostPrice
            //}).ToList();
            outData.Add("DetailList", detailList);
            result.Data = outData;
            return result;
        }

    }
}