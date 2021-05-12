using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class RealTimeLocationApp : BaseApp<RealTimeLocation>
    {
        private RevelanceManagerApp _revelanceApp;
        public RealTimeLocationApp(IUnitWork unitWork, IRepository<RealTimeLocation> repository,
    RevelanceManagerApp app, IAuth auth) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> Load(int ServiceOrderId, string UserId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var map = await UnitWork.Find<AppUserMap>(w => w.UserID == UserId).FirstOrDefaultAsync();
            if (map == null)
            {
                throw new CommonException("当前用户未绑定App", Define.INVALID_TOKEN);
            }
            string startDate = string.Empty;
            string endDate = string.Empty;
            //获取当前服务单下技术员填写的日报信息
            var dailyReports = await UnitWork.Find<ServiceDailyReport>(w => w.ServiceOrderId == ServiceOrderId && w.CreateUserId == UserId).Select(s => s.CreateTime).ToListAsync();
            if (dailyReports != null)
            {
                startDate = dailyReports.Min()?.ToString("yyyy-MM-dd 00:00:00");
                endDate = dailyReports.Max()?.ToString("yyyy-MM-dd 23:59:59");
            }
            else
            {
                //获取完工报告下的出差开始时间与出差结束时间
                var completeReports = await UnitWork.Find<CompletionReport>(w => w.ServiceOrderId == ServiceOrderId && w.CreateUserId == UserId).FirstOrDefaultAsync();
                if (completeReports != null)
                {
                    if (completeReports.BusinessTripDate == null || completeReports.EndDate == null)
                    {
                        startDate = completeReports.CreateTime?.ToString("yyyy-MM-dd 00:00:00");
                        endDate = completeReports.CreateTime?.ToString("yyyy-MM-dd 23:59:59");
                    }
                    else
                    {
                        startDate = completeReports.BusinessTripDate?.ToString("yyyy-MM-dd 00:00:00");
                        endDate = completeReports.EndDate?.ToString("yyyy-MM-dd 23:59:59");
                    }

                }
            }
            var result = new TableData();
            var objs = await UnitWork.Find<RealTimeLocation>(w => w.AppUserId == (int)map.AppUserId && w.CreateTime >= Convert.ToDateTime(startDate) && w.CreateTime <= Convert.ToDateTime(endDate)).OrderBy(o => o.CreateTime).Select(s => new { s.Latitude, s.Longitude, s.CreateTime }).ToListAsync();
            var data = objs.GroupBy(g => g.CreateTime.Date).Select(s => new { date = s.Key, list = s.ToList() }).ToList();
            result.Count = objs.Count();
            result.Data = data;
            return result;
        }

        /// <summary>
        /// 添加定位信息
        /// </summary>
        /// <param name="req"></param>
        public async Task Add(AddOrUpdaterealtimelocationReq req)
        {
            var obj = req.MapTo<RealTimeLocation>();
            obj.CreateTime = DateTime.Now;
            //转百度坐标
            var zuob = Gcj02ToBd09(obj.Latitude.ToDouble(), obj.Longitude.ToDouble());
            obj.BaiduLatitude = zuob[0].ToDecimal();
            obj.BaiduLongitude = zuob[1].ToDecimal();
            //todo:补充或调整自己需要的字段
            //判断是否已存在记录 若存在则做更新操作
            //var locations = await UnitWork.Find<RealTimeLocation>(r => r.AppUserId == req.AppUserId).FirstOrDefaultAsync();
            //if (locations != null)
            //{
            //    obj.Id = locations.Id;
            //    Repository.Update(obj);
            //}
            //else
            //{
            Repository.Add(obj);
            //}
        }

        /// <summary>
        /// 监控大屏数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> LoadLocationInfo(QueryLocationInfoReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();

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

            //var dateWhere = req.StartDate != null && req.EndDate != null ? true : false;
            //DateTime start = Convert.ToDateTime(DateTime.Now.ToString("D").ToString());
            DateTime end = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("D").ToString()).AddSeconds(-1);
            DateTime monthAgo = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("D").ToString());
            var pppUserMap = UnitWork.Find<AppUserMap>(null).ToList();
            ////按指定时间过滤（筛选轨迹）
            //var realTimeLocation = UnitWork.Find<RealTimeLocation>(null)
            //                                .WhereIf(dateWhere, c => c.CreateTime >= req.StartDate && c.CreateTime <= req.EndDate)
            //                                .WhereIf(!dateWhere, c => c.CreateTime >= start && c.CreateTime <= end).ToList();

            //一个月前定位历史（筛选最新在线时间等）
            var realTimeLocationHis= UnitWork.Find<RealTimeLocation>(null)
                                            .Where(c => c.CreateTime >= monthAgo && c.CreateTime <= end).ToList();

            //var locaotionInfo = from a in realTimeLocation
            //                    join b in pppUserMap on a.AppUserId equals b.AppUserId
            //                    select new { a, b.UserID };
            

            var locaotionInfoHistory = from a in realTimeLocationHis
                                       join b in pppUserMap on a.AppUserId equals b.AppUserId
                                       select new { a, b.UserID };


            var data = await UnitWork.Find<User>(c => userIds.Contains(c.Id)).WhereIf(!string.IsNullOrWhiteSpace(req.Name), c => c.Name.Equals(req.Name)).ToListAsync();
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

                 //System.Collections.Generic.List<Trajectory> HistoryPositions = loca?.GroupBy(c => c.a.CreateTime.ToString("yyyy-MM-dd")).Select(c => new Trajectory
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
                         totalHour =Math.Round(ts.TotalHours,2);
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
                     TotalHour= totalHour,
                     //CurrPositions = CurrPositions,
                     //HistoryPositions = HistoryPositions,
                     ServiceOrderId = orderIds,
                     SignInDate = currentLoca.Count > 0 ? currentLoca.Min(q => q.a.CreateTime) : (DateTime?)null,
                     SignOutDate = currentLoca.Count > 1 ? currentLoca.Max(q => q.a.CreateTime) : (DateTime?)null
                 };

             });

            if (string.IsNullOrWhiteSpace(req.Name) && !string.IsNullOrWhiteSpace(req.Status)) da1.Where(c => c.Status == req.Status);

            result.Data = da1;
            return result;
        }


        public async Task<TableData> HistoryTrajectory(QueryLocationInfoReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();

            var userId = await UnitWork.Find<User>(c => c.Name==req.Name).Select(c=>c.Id).FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(userId))
            {
                result.Code = 500;
                result.Message = "查询不到该技术员";
                return result;
            }
            req.StartDate = req.StartDate ?? DateTime.Now;
            req.EndDate = req.EndDate ?? DateTime.Now;
            var start = Convert.ToDateTime(req.StartDate.Value.Date.ToString());
            //DateTime start = new DateTime(req.StartDate.Value.Year, req.StartDate.Value.Month, req.StartDate.Value.Day);
            DateTime end = Convert.ToDateTime(req.EndDate.ToDateTime().AddDays(1).ToString("D").ToString()).AddSeconds(-1);
            //DateTime monthAgo = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("D").ToString());
            var pppUserMap = UnitWork.Find<AppUserMap>(c=>c.UserID==userId).FirstOrDefault();
            //按指定时间过滤（筛选轨迹）
            var realTimeLocation = await UnitWork.Find<RealTimeLocation>(null)
                                            .Where(c =>(c.CreateTime >= start && c.CreateTime <= end) && c.AppUserId==pppUserMap.AppUserId)
                                            .OrderByDescending(c => c.CreateTime)
                                            .ToListAsync();


            var data = realTimeLocation?.GroupBy(c=>c.CreateTime.ToString("yyyy-MM-dd")).Select(c=>new HistoryPositions
            {
                Date = c.Key,
                Pos = c.Select(p => new Position { Longitude = p.BaiduLongitude, Latitude = p.BaiduLatitude }).ToList()
            }).ToList();

            result.Data = data;
            return result;
        }
        /// <summary>
        /// 获取所有客户
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetCustomer() 
        {
            TableData result = new TableData();
            var data = await UnitWork.Find<crm_ocrd>(null).Select(c => new { Name = c.CardName, Code = c.CardCode }).ToListAsync();
            result.Data = data;
            return result;
        }

        public async Task UpdateLoca()
        {
            var loca = await UnitWork.Find<RealTimeLocation>(c => string.IsNullOrWhiteSpace(c.BaiduLatitude.ToString())).Take(5000).ToListAsync();
            foreach (var item in loca)
            {
                var zuob = Gcj02ToBd09(item.Latitude.ToDouble(), item.Longitude.ToDouble());
                item.BaiduLatitude = zuob[0].ToDecimal();
                item.BaiduLongitude = zuob[1].ToDecimal();
            }
            await UnitWork.BatchUpdateAsync(loca.ToArray());
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 火星坐标系 (GCJ-02) 与百度坐标系 (BD-09) 的转换算法 将 GCJ-02 坐标转换成 BD-09 坐标 
        /// 高德谷歌转为百度
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        public static double[] Gcj02ToBd09(double lat, double lon)
        {
            var x_pi= 3.14159265358979324 * 3000.0 / 180.0;
            double x = lon, y = lat;
            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * x_pi);
            double tempLon = z * Math.Cos(theta) + 0.0065;
            double tempLat = z * Math.Sin(theta) + 0.006;
            double[] gps = { tempLat, tempLon };
            return gps;
        }
    }
}