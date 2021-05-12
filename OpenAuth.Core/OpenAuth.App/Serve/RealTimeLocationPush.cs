using Infrastructure.Cache;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
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

            DateTime start = Convert.ToDateTime(DateTime.Now.ToString("D").ToString());
            DateTime end = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("D").ToString()).AddSeconds(-1);
            DateTime monthAgo = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("D").ToString());

            var pppUserMap = UnitWork.Find<AppUserMap>(null).ToList();
            //按指定时间过滤（筛选轨迹）
            //var realTimeLocation = UnitWork.Find<RealTimeLocation>(null)
            //                                .WhereIf(dateWhere, c => c.CreateTime >= req.StartDate && c.CreateTime <= req.EndDate)
            //                                .WhereIf(!dateWhere, c => c.CreateTime >= start && c.CreateTime <= end).ToList();

            //一个月前定位历史（筛选最新在线时间等）
            var realTimeLocationHis = UnitWork.Find<RealTimeLocation>(null)
                                            .Where(c => c.CreateTime >= monthAgo && c.CreateTime <= end).ToList();

            //var locaotionInfo = from a in realTimeLocation
            //                    join b in pppUserMap on a.AppUserId equals b.AppUserId
            //                    select new { a, b.UserID };


            var locaotionInfoHistory = from a in realTimeLocationHis
                                       join b in pppUserMap on a.AppUserId equals b.AppUserId
                                       select new { a, b.UserID };


            var data = await UnitWork.Find<User>(c => userIds.Contains(c.Id)).ToListAsync();
            var da1 = data.Select(c =>
            {
                //var loca = locaotionInfo.Where(q => q.UserID.Equals(c.Id)).OrderByDescending(q => q.a.CreateTime).ToList();
                //当天是否有定位记录
                var currentLoca = locaotionInfoHistory.Where(q => q.UserID.Equals(c.Id) && q.a.CreateTime.ToString("yyyy-MM-dd").Equals(DateTime.Now.ToString("yyyy-MM-dd"))).ToList();
                var currLocation = currentLoca.OrderByDescending(q => q.a.CreateTime).FirstOrDefault();
                var onlineState = "离线";
                var interv = "30天前";
                decimal? longi = 0, lati = 0;
                double? totalHour = 0;
                //获取轨迹

                //System.Collections.Generic.List<Trajectory> HistoryPositions = currentLoca?.GroupBy(c => c.a.CreateTime.ToString("yyyy-MM-dd")).Select(c => new Trajectory
                //{
                //    Date = c.Key,
                //    Pos = c.Select(p => new Position { Longitude = p.a.BaiduLongitude, Latitude = p.a.BaiduLatitude }).ToList()
                //}).ToList();

                //当天轨迹

                //System.Collections.Generic.List<Position> CurrPositions = loca?.Select(c => new Position { Longitude = c.a.Longitude, Latitude = c.a.Latitude }).ToList();

                if (currLocation != null && currLocation.a.CreateTime != null)
                {
                    //当天 / 最新定位

                    longi = currLocation.a.BaiduLongitude;
                    lati = currLocation.a.BaiduLatitude;

                    var now = DateTime.Now;
                    TimeSpan ts = now.Subtract(currLocation.a.CreateTime);
                    totalHour = Math.Round(ts.TotalHours, 2);
                    if (ts.Hours > 0)
                        interv = ts.Hours + "小时前";
                    else
                    {
                        onlineState = "在线";
                        interv = ts.Minutes + "分钟前";
                    }
                }
                else//当天无，则取历史最新
                {
                    var his = locaotionInfoHistory.Where(q => q.UserID.Equals(c.Id)).OrderByDescending(q => q.a.CreateTime).FirstOrDefault();
                    if (his != null)
                    {
                        currLocation = his;
                        //历史最新定位
                        longi = his.a.BaiduLongitude;
                        lati = his.a.BaiduLatitude;

                        var now = DateTime.Now;
                        TimeSpan ts = now.Subtract(his.a.CreateTime);
                        totalHour = Math.Round(ts.TotalHours, 2);
                        interv = ts.Hours + "小时前";
                        if (ts.Days > 0) interv = ts.Days + "天前";
                    }
                }
                var sorder = serviceWorkOrder.Where(q => q.CurrentUserNsapId.Equals(c.Id)).Select(q => q.ServiceOrderId).Take(3).ToArray();
                var orderIds = sorder.Length > 0 ? string.Join(",", sorder) : "";

                return new LocalInfoResp
                {
                    Name = c.Name,
                    Address = currLocation?.a.Province + currLocation?.a.City + currLocation?.a.Area + currLocation?.a.Addr,
                    Mobile = c.Mobile,
                    Status = onlineState,
                    Interval = interv,
                    Longitude = longi,
                    Latitude = lati,
                    TotalHour = totalHour,
                    //CurrPositions = CurrPositions,
                   // HistoryPositions = HistoryPositions,
                    ServiceOrderId = orderIds,
                    SignInDate = currentLoca.Count > 0 ? currentLoca.Min(q => q.a.CreateTime) : (DateTime?)null,
                    SignOutDate = currentLoca.Count > 1 ? currentLoca.Max(q => q.a.CreateTime) : (DateTime?)null
                };

            });
            #endregion

            #region 技术员离线消息
            //技术员离线or在线
            var userList = da1.Where(c => c.Status == "离线" && c.TotalHour >= 1).Select(c => c.Name).ToList();
            //var userLocation = locaotionInfoHistory.GroupBy(c => c.UserID).Select(c => new OffLineList
            //{
            //    Id = c.Key,
            //    Time = c.OrderByDescending(c => c.a.CreateTime).Select(c => c.a.CreateTime).FirstOrDefault()
            //});

            //var userList = userLocation.Where(c => DateTime.Now.Subtract(c.Time).TotalHours >= 1).Select(c => c.Id).ToList();
            var oldList = _cacheContext.Get<List<string>>("OffLineUserList");
            if (oldList != null)
            {
                _cacheContext.Remove("OffLineUserList");
                _cacheContext.Set<List<string>>("OffLineUserList", userList, end);
                //未推送过的人员Id
                userList = userList.Except(oldList).ToList();
            }
            else
                _cacheContext.Set<List<string>>("OffLineUserList", userList, end);


            //var noPushUserInfo = await UnitWork.Find<User>(c => userList.Contains(c.Id)).Select(c => c.Name).ToArrayAsync();

            if (userList.Count>0)
            {
                var users = await _userManagerApp.LoadByRoleName(new string[] { "呼叫中心", "总经理" });
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

                await _hubContext.Clients.Groups(new List<string>() { "呼叫中心", "总经理" }).SendAsync("TechnicianOfflineMessage", "系统", message);//离线消息
            }
            #endregion

            await _hubContext.Clients.Groups(new List<string>() { "呼叫中心", "总经理" }).SendAsync("SmartScreenInfo", "系统", da1.ToList());//大屏数据
        }
    }

    [Serializable]
    public class OffLineList
    {
        public string Id { get; set; }
        public DateTime Time { get; set; }
    }
}
