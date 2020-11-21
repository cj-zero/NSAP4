using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using KuaiDi100.Common.Request;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class ReturnRepairApp : OnlyUnitWorkBaeApp
    {
        private readonly ExpressageApp _expressageApp;
        public ReturnRepairApp(IUnitWork unitWork, ExpressageApp expressageApp,
          IAuth auth) : base(unitWork, auth)
        {
            _expressageApp = expressageApp;
        }

        /// <summary>
        /// 提交物流信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task AddReturnRepair(AddReturnRepairReq req)
        {
            //获取当前用户nsap用户信息
            var userInfo = (await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.AppUserId).Include(a => a.User).FirstOrDefaultAsync())?.User;
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            //1.新增返厂维修单主表
            var newRepairInfo = new ReturnRepair { ServiceOrderId = req.ServiceOrderId, ServiceOrderSapId = req.ServiceOrderSapId, MaterialType = req.MaterialType, CreateTime = DateTime.Now, CreateUserId = userInfo.Id, CreateUser = userInfo.Name };
            var o = await UnitWork.AddAsync<ReturnRepair, int>(newRepairInfo);
            await UnitWork.SaveAsync();
            //2.添加物流信息
            if (req.TrackInfos != null && req.TrackInfos.Count > 0)
            {
                foreach (var item in req.TrackInfos)
                {
                    int isAlreadyCheck = 0;
                    var expressageInfo = await UnitWork.Find<Express>(w => w.ReturnRepairId == o.Id && w.ExpressNumber == item.Key).FirstOrDefaultAsync();
                    if (expressageInfo != null)
                    {
                        isAlreadyCheck = (int)expressageInfo.IsCheck;
                    }
                    else
                    {
                        //先不判断物流单号信息是否正确
                        var express = new Express
                        {
                            ReturnRepairId = o.Id,
                            ExpressNumber = item.Key,
                            Creater = userInfo.Name,
                            CreateUserId = userInfo.Id,
                            CreateTime = DateTime.Now,
                            IsCheck = 0,
                            Type = 1,
                            Remark = item.Value
                        };
                        expressageInfo = await UnitWork.AddAsync(express);
                        await UnitWork.SaveAsync();
                    }
                    //判断物流单号不为空且未签收再进行物流查询
                    if (isAlreadyCheck == 0)
                    {
                        var result = await _expressageApp.GetExpressInfo(item.Key);
                        if (result.Code == 200)
                        {
                            var response = (string)result.Data;
                            var returndata = JsonConvert.DeserializeObject<dynamic>(result.Data);
                            int isCheck = returndata.ischeck;
                            var detail = returndata.data;
                            DateTime? checkTime = null;//收货时间
                            DateTime? CreateTime = null;//发货时间
                            if (isCheck == 1)
                            {
                                checkTime = Convert.ToDateTime(detail[0]["time"].ToString());
                            }
                            CreateTime = Convert.ToDateTime(detail[detail.Count - 1]["time"].ToString());
                            await UnitWork.UpdateAsync<Express>(w => w.Id == expressageInfo.Id, u => new Express { ExpressInformation = response, CheckTime = checkTime, IsCheck = isCheck, CreateTime = CreateTime });
                        }
                    }
                }
            }
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 获取维修进度
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <param name="materialType"></param>
        /// <param name="appUserId"></param>
        /// <returns></returns>
        public async Task<TableData> GetReturnRepairProcess(int serviceOrderId, string materialType, int appUserId)
        {
            var result = new TableData();
            //获取当前用户nsap用户信息
            var userInfo = (await UnitWork.Find<AppUserMap>(a => a.AppUserId == appUserId).Include(a => a.User).FirstOrDefaultAsync())?.User;
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            var query = from a in UnitWork.Find<Express>(null)
                        join b in UnitWork.Find<ReturnRepair>(null) on a.ReturnRepairId equals b.Id
                        where b.ServiceOrderId == serviceOrderId && b.MaterialType == materialType && b.CreateUserId == userInfo.Id
                        select new { a, b };
            var resultList = (await query.Select(s => new { s.a.Id, s.a.IsCheck, s.a.Type, s.a.CreateTime, s.a.CheckTime }).OrderBy(o => o.CreateTime).ToListAsync()).GroupBy(g => g.Type).Select(s => new { type = s.Key, ExpressInfo = s.ToList() }).ToList();
            result.Data = resultList;
            return result;
        }

        /// <summary>
        /// 获取返厂维修列表(网页端)
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetReturnRepairList()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var querybase = from a in UnitWork.Find<ReturnRepair>(null)
                            join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id
                            select new { a, b.U_SAP_ID, b.CustomerId, b.CustomerName, a.ServiceOrderId, a.Id };
            var baseInfo = await querybase.ToListAsync();
            var resultList = (await UnitWork.Find<Express>(null).Include(s => s.ExpressPictures)
                .Select(s => new
                {
                    s.Id,
                    s.IsCheck,
                    s.Type,
                    s.CreateTime,
                    s.CheckTime,
                    s.ReturnRepairId,
                    s.ExpressNumber,
                    s.ExpressInformation,
                    s.Creater,
                    s.Remark,
                    ExpressPictures = s.ExpressPictures.Select(s => s.PictureId).ToList()
                }).OrderBy(o => o.CreateTime).ToListAsync())
                .Select(s => new
                {
                    s.Id,
                    s.IsCheck,
                    s.Type,
                    CreateTime = s.CreateTime?.ToString("yyyy-MM-dd"),
                    s.CheckTime,
                    U_SAP_ID = baseInfo.Where(w => w.Id == s.ReturnRepairId).Select(s => s.U_SAP_ID),
                    CustomerId = baseInfo.Where(w => w.Id == s.ReturnRepairId).Select(s => s.CustomerId),
                    CustomerName = baseInfo.Where(w => w.Id == s.ReturnRepairId).Select(s => s.CustomerName),
                    s.ExpressNumber,
                    s.ExpressInformation,
                    s.Creater,
                    s.Remark,
                    s.ExpressPictures
                }).ToList().GroupBy(g => g.Id).Select(s => new { ExpressId = s.Key, ExpressInfo = s.ToList() }).ToList();
            result.Data = resultList;
            return result;
        }

        /// <summary>
        /// 修改快递单号
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> EditExpressNum(EditExpressNumReq req)
        {
            string response = string.Empty;
            var result = new TableData();
            //获取当前用户nsap用户信息
            var userInfo = (await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.AppUserId).Include(a => a.User).FirstOrDefaultAsync())?.User;
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            //查询物流信息
            var r = await _expressageApp.GetExpressInfo(req.TrackNumber);
            if (r.Code == 200)
            {
                response = (string)r.Data;
                var returndata = JsonConvert.DeserializeObject<dynamic>(response);
                int isCheck = returndata.ischeck;
                var detail = returndata.data;
                DateTime? checkTime = null;//收货时间
                DateTime? CreateTime = null;//发货时间
                if (isCheck == 1)
                {
                    checkTime = Convert.ToDateTime(detail[0]["time"].ToString());
                }
                CreateTime = Convert.ToDateTime(detail[detail.Count - 1]["time"].ToString());
                await UnitWork.UpdateAsync<Express>(w => w.Id == req.ExpressId, u => new Express { ExpressInformation = response, CheckTime = checkTime, IsCheck = isCheck, CreateTime = CreateTime, ExpressNumber = req.TrackNumber, Creater = userInfo.Name, CreateUserId = userInfo.Id });
                await UnitWork.SaveAsync();
                result.Data = response;
            }
            else
            {
                result.Code = r.Code;
                result.Message = r.Message;
            }
            return result;
        }

        /// <summary>
        /// 获取物流信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<TableData> GetExpressInfo(string Id)
        {
            Dictionary<string, object> outData = new Dictionary<string, object>();
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var ExpressInfo = await UnitWork.Find<Express>(w => w.Id == Id).FirstOrDefaultAsync();
            string tracknum = ExpressInfo.ExpressNumber;
            outData.Add("tracknum", tracknum);
            int isCheck = (int)ExpressInfo.IsCheck;
            if (isCheck == 1)
            {
                outData.Add("expressinfo", ExpressInfo.ExpressInformation);
                result.Data = outData;
                return result;
            }
            else
            {
                var r = await _expressageApp.GetExpressInfo(tracknum);
                if (r.Code == 200)
                {
                    var response = (string)r.Data;
                    var returndata = JsonConvert.DeserializeObject<dynamic>(response);
                    int isAlreadyCheck = returndata.ischeck;
                    var detail = returndata.data;
                    DateTime? checkTime = null;//收货时间
                    DateTime? CreateTime = null;//发货时间
                    if (isCheck == 1)
                    {
                        checkTime = Convert.ToDateTime(detail[0]["time"].ToString());
                    }
                    CreateTime = Convert.ToDateTime(detail[detail.Count - 1]["time"].ToString());
                    await UnitWork.UpdateAsync<Express>(w => w.Id == Id, u => new Express { ExpressInformation = response, CheckTime = checkTime, IsCheck = isAlreadyCheck, CreateTime = CreateTime });
                    await UnitWork.SaveAsync();
                    outData.Add("expressinfo", response);
                }
                else
                {
                    outData.Add("expressinfo", "");
                }
            }
            result.Data = outData;
            return result;
        }
    }
}