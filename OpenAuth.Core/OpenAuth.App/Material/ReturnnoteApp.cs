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
            //1.新增退料单主表
            var newNoteInfo = new ReturnNote { ServiceOrderId = req.ServiceOrderId, ServiceOrderSapId = req.SapId, Status = 1, CreateTime = DateTime.Now, CreateUserId = userInfo.Id, CreateUser = userInfo.Name, IsLast = req.IsLastReturn };
            var o = await UnitWork.AddAsync<ReturnNote, int>(newNoteInfo);
            await UnitWork.SaveAsync();
            //2.添加退料明细信息
            foreach (ReturnMaterialDetail item in req.ReturnMaterialDetail)
            {
                var newDetailInfo = new ReturnnoteMaterial
                {
                    ReturnNoteId = o.Id,
                    MaterialCode = item.MaterialCode,
                    MaterialDescription = item.MaterialDescription,
                    Count = item.ReturnQty,
                    TotalCount = item.TotalQty,
                    Check = 0
                };
                var detail = await UnitWork.AddAsync<ReturnnoteMaterial, int>(newDetailInfo);
                await UnitWork.SaveAsync();
                //3.添加退料物料图片
                var newPictureInfo = new ReturnNoteMaterialPicture { ReturnnoteMaterialId = detail.Id, PictureId = item.PictureId };
                await UnitWork.AddAsync(newPictureInfo);
                await UnitWork.SaveAsync();
            }
            //4.添加物流信息
            if (!string.IsNullOrEmpty(req.TrackNumber))
            {
                var result = await _expressageApp.GetExpressInfo(req.TrackNumber);
                if (result.Code == 200)
                {
                    var response = (string)result.Data;
                    var expressageInfo = await UnitWork.Find<Expressage>(w => w.ReturnNoteId == o.Id).FirstOrDefaultAsync();
                    if (expressageInfo == null)
                    {
                        var express = new Expressage
                        {
                            QuotationId = null,
                            ReturnNoteId = o.Id,
                            ExpressNumber = req.TrackNumber,
                            ExpressInformation = response
                        };
                        await UnitWork.AddAsync(express);
                    }
                    else
                    {
                        await UnitWork.UpdateAsync<Expressage>(w => w.Id == expressageInfo.Id, u => new Expressage { ExpressInformation = response });
                    }
                }
            }
            //创建物料报价单审批流程
            var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("待退料单"));
            var afir = new AddFlowInstanceReq();
            afir.SchemeId = mf.FlowSchemeId;
            afir.FrmType = 2;
            afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
            afir.CustomName = $"退料单审批" + DateTime.Now;
            afir.OrgId = "";
            //保外申请报价单
            afir.FrmData = $"{{\"ReturnnoteId\":\"{o.Id}\"}}";
            var flowinstanceid = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
            await UnitWork.UpdateAsync<ReturnNote>(r => r.Id == o.Id, r => new ReturnNote
            {
                FlowInstanceId = flowinstanceid
            });
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
            await SaveReceiveInfo(req.ReturnMaterials);
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
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 仓库验货（保存）
        /// </summary>
        /// <param name="ReturnMaterials"></param>
        /// <returns></returns>
        public async Task SaveReceiveInfo(List<ReturnMaterial> ReturnMaterials)
        {
            if (ReturnMaterials != null && ReturnMaterials.Count > 0)
            {
                foreach (var item in ReturnMaterials)
                {
                    await UnitWork.UpdateAsync<ReturnnoteMaterial>(r => r.Id == item.Id, u => new ReturnnoteMaterial { Check = item.IsPass, WrongCount = item.WrongCount, ReceivingRemark = item.ReceiveRemark });
                }
            }
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
            var returnNoteList = (await query.Select(s => new { s.b.MaterialCode, s.b.Id, s.b.Count, s.b.TotalCount, s.c.PictureId, s.b.Check, returnNoteId = s.a.Id, s.b.WrongCount, s.b.ReceivingRemark, s.b.ShippingRemark, s.d.ExpressNumber, s.a.Status, s.a.IsLast, s.a.Remark }).OrderByDescending(o => o.returnNoteId).ToListAsync()).GroupBy(g => g.returnNoteId).Select(s => new { ReturnNoteId = s.Key, Detail = s.ToList() }).ToList();
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
        /// 获取退料单列表
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
            var returnNoteList = returnNote.Select(s => new { s.Id, CustomerId = serviceOrderList.Where(w => w.Id == s.ServiceOrderId).Select(s => s.CustomerId).FirstOrDefault(), CustomerName = serviceOrderList.Where(w => w.Id == s.ServiceOrderId).Select(s => s.CustomerName).FirstOrDefault(), s.ServiceOrderId, s.CreateUser, CreateDate = s.CreateTime.ToString("yyyy.mm.dd"), s.ServiceOrderSapId }).ToList();
            result.Data = returnNoteList;
            return result;
        }

        /// <summary>
        /// 获取退料单详情（nsap）
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<TableData> GetReturnNoteDetail(int Id)
        {
            Dictionary<string, object> outDta = new Dictionary<string, object>();
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
                {"returnNoteCode" ,returnNote.Id},{"creater",returnNote.CreateUser },{ "createTime",returnNote.CreateTime },{"salMan",serviceOrder.SalesMan},{ "serviceSapId",serviceOrder.U_SAP_ID},{ "customerCode",serviceOrder.CustomerId},{ "customerName",serviceOrder.CustomerName},{ "status",returnNote.Status},{ "isLast",returnNote.IsLast}
            };
            outDta.Add("mainInfo", mainInfo);
            //获取物流信息
            var expressList = await UnitWork.Find<Expressage>(w => w.ReturnNoteId == Id).ToListAsync();
            outDta.Add("expressList", expressList);
            //获取退料列表
            var query = from a in UnitWork.Find<ReturnNote>(null)
                        join b in UnitWork.Find<ReturnnoteMaterial>(null) on a.Id equals b.ReturnNoteId into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<ReturnNoteMaterialPicture>(null) on b.Id equals c.ReturnnoteMaterialId into abc
                        from c in abc.DefaultIfEmpty()
                        where a.Id == Id
                        select new { a, b, c };
            var returnNoteList = await query.Select(s => new { s.b.MaterialCode, s.b.MaterialDescription, s.b.Id, s.b.Count, s.b.TotalCount, s.c.PictureId, s.b.Check, returnNoteId = s.a.Id, s.b.WrongCount, s.b.ReceivingRemark, s.b.ShippingRemark }).OrderByDescending(o => o.returnNoteId).ToListAsync();
            outDta.Add("returnNoteList", returnNoteList);
            result.Data = outDta;
            return result;
        }


    }
}