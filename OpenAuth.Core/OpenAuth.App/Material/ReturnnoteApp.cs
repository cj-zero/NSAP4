using Infrastructure;
using KuaiDi100.Common.Request;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenAuth.App.Interface;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.App
{
    public class ReturnNoteApp : OnlyUnitWorkBaeApp
    {
        public ReturnNoteApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {

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
            var newNoteInfo = new ReturnNote { ServiceOrderId = req.ServiceOrderId, ServiceOrderSapId = req.SapId, Status = 1, CreateTime = DateTime.Now, CreateUserId = userInfo.Id, CreateUser = userInfo.Name };
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
                //根据快递单号查询快递公司编码
                string comCode = AutoNum.query(req.TrackNumber);
                string com = JsonConvert.DeserializeObject<dynamic>(comCode)[0].comCode;
                QueryTrackParam trackReq = new QueryTrackParam
                {
                    com = com,
                    num = req.TrackNumber,
                    resultv2 = "2"
                };
                string response = QueryTrack.queryTrackInfo(trackReq);
                string message = JsonConvert.DeserializeObject<dynamic>(response).message;
                if ("ok".Equals(message))
                {
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
            var returnNoteInfo = await UnitWork.Find<ReturnNote>(r => r.CreateUserId == userInfo.Id && r.ServiceOrderId == ServiceOrderId).ToListAsync();
            var query = from a in UnitWork.Find<ReturnNote>(null)
                        join b in UnitWork.Find<ReturnnoteMaterial>(null) on a.Id equals b.ReturnNoteId into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<ReturnNoteMaterialPicture>(null) on b.Id equals c.ReturnnoteMaterialId into abc
                        from c in abc.DefaultIfEmpty()
                        where a.ServiceOrderId == ServiceOrderId && a.CreateUserId == userInfo.Id
                        select new { a, b, c };
            var returnNoteList = await query.Select(s => new { s.b.MaterialCode, s.b.Id, s.b.Count, s.b.TotalCount, s.c.PictureId, s.b.Check }).ToListAsync();
            result.Data = returnNoteList;
            return result;
        }

        /// <summary>
        /// 获取物流信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<TableData> GetExpressInfo(string Id)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var ExpressInfo = await UnitWork.Find<Expressage>(w => w.Id == Id).FirstOrDefaultAsync();
            string tracknum = ExpressInfo.ExpressNumber;
            //根据快递单号查询快递公司编码
            string comCode = AutoNum.query(tracknum);
            string com = JsonConvert.DeserializeObject<dynamic>(comCode)[0].comCode;
            QueryTrackParam trackReq = new QueryTrackParam
            {
                com = com,
                num = tracknum,
                resultv2 = "2"
            };
            string response = QueryTrack.queryTrackInfo(trackReq);
            string message = JsonConvert.DeserializeObject<dynamic>(response).message;
            if ("ok".Equals(message))
            {
                result.Data = response;
                var express = new Expressage
                {
                    QuotationId = ExpressInfo.QuotationId,
                    ReturnNoteId = ExpressInfo.ReturnNoteId,
                    ExpressNumber = tracknum,
                    ExpressInformation = response
                };
                await UnitWork.AddAsync(express);
                await UnitWork.SaveAsync();
            }
            else
            {
                result.Code = 500;
                result.Message = message;
                return result;
            }
            return result;
        }
    }
}