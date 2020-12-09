using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
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
        public async Task<TableData> GetReturnRepairList(QueryRerurnRepairListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var querybase = from a in UnitWork.Find<ReturnRepair>(null)
                            join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id
                            select new { b.U_SAP_ID, b.CustomerId, b.CustomerName, a.ServiceOrderId, a.Id, a.MaterialType, a.CreateTime };
            var baseInfo = await querybase.WhereIf(Convert.ToInt32(req.SapId) > 0, w => w.U_SAP_ID == Convert.ToInt32(req.SapId))            //服务Id
                           .WhereIf(!string.IsNullOrEmpty(req.Customer), w => w.CustomerId.Contains(req.Customer) || w.CustomerName.Contains(req.Customer))//客户
                           .WhereIf(!string.IsNullOrEmpty(req.StartDate), w => w.CreateTime >= Convert.ToDateTime(req.StartDate))//创建时间（开始）
                           .WhereIf(!string.IsNullOrEmpty(req.EndDate), w => w.CreateTime < Convert.ToDateTime(req.EndDate))//创建时间（结束）
                           .ToListAsync();
            var returnReturnIds = baseInfo.Select(s => s.Id).Distinct().ToList();
            var fileList = await UnitWork.Find<UploadFile>(null).ToListAsync();
            var resultList = (await UnitWork.Find<Express>(e => returnReturnIds.Contains((int)e.ReturnRepairId)).Include(s => s.ExpressPictures)
                .WhereIf(!string.IsNullOrEmpty(req.ExpressNum), w => w.ExpressNumber.Equals(req.ExpressNum))            //快递单号
                .WhereIf(!string.IsNullOrEmpty(req.Creater), w => w.Creater.Equals(req.Creater))//寄件人
                .WhereIf(Convert.ToInt32(req.ExpressState) == 1, w => w.IsCheck == 0)
                .WhereIf(Convert.ToInt32(req.ExpressState) == 2, w => w.IsCheck == 1)
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
                    TypeName = s.Type == 1 ? "寄回" : "寄出",
                    CreateTime = s.CreateTime?.ToString("yyyy-MM-dd"),
                    s.CheckTime,
                    U_SAP_ID = baseInfo.Where(w => w.Id == s.ReturnRepairId).Select(s => s.U_SAP_ID).FirstOrDefault(),
                    CustomerId = baseInfo.Where(w => w.Id == s.ReturnRepairId).Select(s => s.CustomerId).FirstOrDefault(),
                    CustomerName = baseInfo.Where(w => w.Id == s.ReturnRepairId).Select(s => s.CustomerName).FirstOrDefault(),
                    ServiceOrderId = baseInfo.Where(w => w.Id == s.ReturnRepairId).Select(s => s.ServiceOrderId).FirstOrDefault(),

                    MaterialType = baseInfo.Where(w => w.Id == s.ReturnRepairId).Select(s => s.MaterialType).FirstOrDefault(),
                    s.ExpressNumber,
                    s.ExpressInformation,
                    s.Creater,
                    s.Remark,

                    ExpressAccessorys = GetAccessorys(s.ExpressPictures, fileList),
                    s.ReturnRepairId
                }).ToList();
            result.Count = resultList.Count;
            result.Data = resultList.GroupBy(g => g.Id).Select(s => new { ExpressId = s.Key, ExpressInfo = s.ToList() }).Skip((req.page - 1) * req.limit).Take(req.limit).ToList();

            return result;
        }

        /// <summary>
        /// 获取附件信息集合
        /// </summary>
        /// <param name="accessoryIds"></param>
        /// <param name="fileList"></param>
        /// <returns></returns>
        private dynamic GetAccessorys(List<string> accessoryIds, List<UploadFile> fileList)
        {
            List<dynamic> result = new List<dynamic>();

            foreach (var item in accessoryIds)
            {
                Dictionary<string, object> accessoryInfo = new Dictionary<string, object>();
                var fileInfo = fileList.Where(w => w.Id == item).FirstOrDefault();
                if (fileInfo != null)
                {
                    accessoryInfo.Add("fileId", item);
                    accessoryInfo.Add("fileName", fileInfo.FileName);
                    accessoryInfo.Add("fileType", fileInfo.FileType);
                    result.Add(accessoryInfo);
                }
            }
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
                switch (req.Type)
                {
                    case 1:
                        await UnitWork.UpdateAsync<Expressage>(w => w.Id == req.ExpressId, u => new Expressage { ExpressInformation = response, ExpressNumber = req.TrackNumber });
                        break;
                    case 2:
                        await UnitWork.UpdateAsync<Express>(w => w.Id == req.ExpressId, u => new Express { ExpressInformation = response, CheckTime = checkTime, IsCheck = isCheck, CreateTime = CreateTime, ExpressNumber = req.TrackNumber, Creater = userInfo.Name, CreateUserId = userInfo.Id });
                        break;
                }
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

        /// <summary>
        /// 寄出设备
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task AddExpress(AddExpressReq req)
        {
            //获取当前用户nsap用户信息
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //1.添加物流信息
            if (string.IsNullOrEmpty(req.TrackNum))
            {
                throw new CommonException("请输入快递单号", Define.ExpressNum_IsNull);
            }
            //先不判断物流单号信息是否正确
            var express = new Express
            {
                ReturnRepairId = req.ReturnRepairId,
                ExpressNumber = req.TrackNum,
                Creater = loginContext.User.Name,
                CreateUserId = loginContext.User.Id,
                CreateTime = DateTime.Now,
                IsCheck = 0,
                Type = 2,
                Remark = req.Remark
            };
            var expressageInfo = await UnitWork.AddAsync(express);
            await UnitWork.SaveAsync();
            //判断物流单号不为空
            var result = await _expressageApp.GetExpressInfo(req.TrackNum);
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
            //添加附件信息
            if (req.Accessorys.Count > 0)
            {
                foreach (var item in req.Accessorys)
                {
                    var accessoryInfo = new ExpressPicture { ExpressId = expressageInfo.Id, PictureId = item };
                    await UnitWork.AddAsync(accessoryInfo);
                }
            }
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 撤回快递
        /// </summary>
        /// <param name="ExpresssId"></param>
        /// <returns></returns>
        public async Task WithDrawExpress(string ExpressId)
        {
            //获取当前用户nsap用户信息
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var expressInfo = await UnitWork.Find<Express>(w => w.Id == ExpressId).FirstOrDefaultAsync();
            if (expressInfo == null)
            {
                throw new CommonException("当前快递信息已不存在", Define.Express_NotFound);
            }

            TimeSpan timeSpan = (TimeSpan)(DateTime.Now - expressInfo?.CreateTime);
            if (timeSpan.TotalMinutes > 5)
            {
                throw new CommonException("无法撤回，寄出时间已超5分钟", Define.IS_OverTime);
            }
            //删除快递单
            await UnitWork.DeleteAsync<Express>(w => w.Id == ExpressId);
            await UnitWork.DeleteAsync<ExpressPicture>(w => w.ExpressId == ExpressId);
            await UnitWork.SaveAsync();
        }
    }
}