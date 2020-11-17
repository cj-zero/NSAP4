using System;
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
        public ReturnRepairApp(IUnitWork unitWork,
          IAuth auth) : base(unitWork, auth)
        {

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
            int isAlreadyCheck = 0;
            var expressageInfo = await UnitWork.Find<Express>(w => w.ReturnRepairId == o.Id).FirstOrDefaultAsync();
            if (expressageInfo != null)
            {
                isAlreadyCheck = (int)expressageInfo.IsCheck;
            }
            //判断物流单号不为空且未签收再进行物流查询
            if (!string.IsNullOrEmpty(req.TrackNumber) && isAlreadyCheck == 0)
            {
                //根据快递单号查询快递公司编码
                string comCode = AutoNum.query(req.TrackNumber);
                if (comCode != "[]")
                {
                    string com = JsonConvert.DeserializeObject<dynamic>(comCode)[0].comCode;
                    QueryTrackParam trackReq = new QueryTrackParam
                    {
                        com = com,
                        num = req.TrackNumber,
                        resultv2 = "2"
                    };
                    string response = QueryTrack.queryTrackInfo(trackReq);
                    var returndata = JsonConvert.DeserializeObject<dynamic>(response);
                    string message = returndata.message;
                    int isCheck = returndata.ischeck;
                    var detail = returndata.data;
                    if ("ok".Equals(message))
                    {
                        string checkTime = string.Empty;
                        if (expressageInfo == null)
                        {
                            if (isCheck == 1)
                            {
                                checkTime = detail[0]["time"].ToString();
                            }
                            var express = new Express
                            {
                                ReturnRepairId = o.Id,
                                ExpressNumber = req.TrackNumber,
                                ExpressInformation = response,
                                Creater = userInfo.Name,
                                CreateUserId = userInfo.Id,
                                CreateTime = Convert.ToDateTime(detail[detail.Count - 1]["time"].ToString()),
                                IsCheck = isCheck,
                                Type = 1,
                                Remark = req.Remark,
                                CheckTime = Convert.ToDateTime(checkTime)
                            };
                            await UnitWork.AddAsync(express);
                        }
                        else
                        {
                            await UnitWork.UpdateAsync<Express>(w => w.Id == expressageInfo.Id, u => new Express { ExpressInformation = response });
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
            var resultList = (await query.Select(s => new { s.a.Id, s.a.IsCheck, s.a.Type, s.a.CreateTime, s.a.CheckTime }).OrderBy(o => o.CreateTime).ToListAsync()).GroupBy(g => g.Id).Select(s => new { ExpressId = s.Key, ExpressInfo = s.ToList() }).ToList();
            result.Data = resultList;
            return result;
        }
    }
}