using Infrastructure.Cache;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.SignalR;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Serve
{
    public class RealTimeLocationPush : OnlyUnitWorkBaeApp
    {
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly SysMessageApp _sysMessageApp;
        private readonly ICacheContext _cacheContext;
        private readonly UserManagerApp _userManagerApp;

        public RealTimeLocationPush(IUnitWork unitWork, 
            IHubContext<MessageHub> hubContext, 
            IAuth auth, 
            SysMessageApp sysMessageApp,
            ICacheContext cacheContext,
            UserManagerApp userManagerApp) : base(unitWork, auth)
        {
            _hubContext = hubContext;
            _sysMessageApp = sysMessageApp;
            _cacheContext = cacheContext;
            _userManagerApp = userManagerApp;
        }

        /// <summary>
        /// 推送智慧大屏数据/消息
        /// </summary>
        /// <returns></returns>
        public async Task PushDataMessage()
        {
            #region 大屏数据
            //技术员角色
            var roleId = await UnitWork.Find<Role>(c => c.Name.Equals("售后技术员")).Select(c => c.Id).FirstOrDefaultAsync();
            var userIds = await UnitWork.Find<Relevance>(c => c.Key.Equals(Define.USERROLE) && c.SecondId.Equals(roleId)).Select(c => c.FirstId).ToListAsync();
            //未完成服务id的
            var serviceWorkOrder = UnitWork.Find<ServiceWorkOrder>(c => c.Status < 7);
            var noComplete = await serviceWorkOrder
                                    .Where(c => !string.IsNullOrWhiteSpace(c.CurrentUserNsapId))
                                    .Select(c => c.CurrentUserNsapId)
                                    .Distinct()
                                    .ToListAsync();
            userIds.AddRange(noComplete);

            DateTime end = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("D").ToString()).AddSeconds(-1);

            var pppUserMap = await UnitWork.Find<AppUserMap>(null).ToListAsync();


            //所有人最新定位信息
            var realTimeLocationHis = await UnitWork.FromSql<RealTimeLocation>(@$"SELECT * from nsap4_serve.realtimelocation where Id in  (
                                        SELECT max(Id) as Id from nsap4_serve.realtimelocation GROUP BY AppUserId
                                        ) ORDER BY CreateTime desc").ToListAsync();


            var locaotionInfoHistory = from a in realTimeLocationHis
                                       join b in pppUserMap on a.AppUserId equals b.AppUserId
                                       select new { a, b.UserID };

            var now = DateTime.Now;
            var data = await UnitWork.Find<User>(c => userIds.Contains(c.Id)).ToListAsync();
            var da1 = data.Select(c =>
            {
                //当天是否有定位记录
                var currentLoca = locaotionInfoHistory.Where(q => q.UserID.Equals(c.Id)).FirstOrDefault();
                var currentDate = locaotionInfoHistory.Where(q => q.UserID.Equals(c.Id) && q.a.CreateTime.ToString("yyyy-MM-dd").Equals(DateTime.Now.ToString("yyyy-MM-dd"))).ToList();

                var onlineState = "离线";
                var interv = "30天前";
                decimal? longi = 0, lati = 0;
                double? totalHour = 0;

                if (currentLoca != null)
                {
                    //当天 / 最新定位
                    longi = currentLoca.a.BaiduLongitude;
                    lati = currentLoca.a.BaiduLatitude;

                    TimeSpan ts = now.Subtract(currentLoca.a.CreateTime);
                    totalHour = Math.Round(ts.TotalHours, 2);
                    if (totalHour > 24)
                        interv = ts.Days + "天前";
                    else if (totalHour >= 1 && totalHour <= 24)
                        interv = ts.Hours + "小时前";
                    else
                    {
                        onlineState = "在线";
                        interv = ts.Minutes + "分钟前";
                    }
                }

                return new LocalInfoResp
                {
                    Name = c.Name,
                    Address = currentLoca?.a.Province + currentLoca?.a.City + currentLoca?.a.Area + currentLoca?.a.Addr,
                    Mobile = c.Mobile,
                    Status = onlineState,
                    Interval = interv,
                    Longitude = longi,
                    Latitude = lati,
                    TotalHour = totalHour,
                    SignInDate = currentDate.Count > 0 ? currentDate.Min(q => q.a.CreateTime) : (DateTime?)null,
                    SignOutDate = currentDate.Count > 1 ? currentDate.Max(q => q.a.CreateTime) : (DateTime?)null
                };

            });
            #endregion

            #region 技术员离线消息
            //技术员离线or在线
            var userList = da1.Where(c => c.Status == "离线" && c.TotalHour >= 1).Select(c => c.Name).ToList();
            var oldList = _cacheContext.Get<List<string>>("OffLineUserList");
            if (oldList != null)
            {
                var aa=_cacheContext.Remove("OffLineUserList");
                _cacheContext.Set<List<string>>("OffLineUserList", userList, end);
                //未推送过的人员Id
                userList = userList.Except(oldList).ToList();
            }
            else
                _cacheContext.Set<List<string>>("OffLineUserList", userList, end);


            //var noPushUserInfo = await UnitWork.Find<User>(c => userList.Contains(c.Id)).Select(c => c.Name).ToArrayAsync();

            if (userList.Count>0)
            {
                var users = await _userManagerApp.LoadByRoleName(new string[] { "呼叫中心"});
                var message = $"技术员:{string.Join(",", userList.ToArray())}已离线一小时，请及时处理。";
                foreach (var user in users)
                {
                    //保存推送记录
                    await _sysMessageApp.AddAsync(new Repository.Domain.SysMessage
                    {
                        Title = "技术员离线消息通知",
                        Content = message,
                        CreateId = Guid.Empty.ToString(),
                        CreateTime = DateTime.Now,
                        FromId = Guid.Empty.ToString(),
                        FromName = "系统通知",
                        ToId = user.Id,
                        ToName = user.Name,
                        TypeName = "技术员离线消息通知",
                        TypeId = Guid.Empty.ToString()
                    });
                }

                await _hubContext.Clients.Groups(new List<string>() { "呼叫中心"}).SendAsync("TechnicianOfflineMessage", "系统", message);//离线消息
            }
            #endregion

            await _hubContext.Clients.Groups(new List<string>() { "呼叫中心"}).SendAsync("SmartScreenInfo", "系统", da1.ToList());//大屏数据
        }
    }

}
