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

            DateTime end = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("D").ToString()).AddSeconds(-1);
            DateTime monthAgo = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("D").ToString());
            var pppUserMap = await UnitWork.Find<AppUserMap>(null).ToListAsync();

            //所有人最新定位信息
            var realTimeLocationHis = await UnitWork.FromSql<RealTimeLocation>(@$"SELECT * from nsap4_serve.realtimelocation where Id in  (
                                        SELECT max(Id) as Id from nsap4_serve.realtimelocation GROUP BY AppUserId
                                        ) ORDER BY CreateTime desc").ToListAsync();


            var locaotionInfoHistory = from a in realTimeLocationHis
                                       join b in pppUserMap on a.AppUserId equals b.AppUserId
                                       select new { a, b.UserID };


            var now = DateTime.Now;
            var data = await UnitWork.Find<User>(c => userIds.Contains(c.Id)).WhereIf(!string.IsNullOrWhiteSpace(req.Name), c => c.Name.Equals(req.Name)).ToListAsync();
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
                     TotalHour= totalHour,
                     //ServiceOrderId = orderIds,
                     SignInDate = currentDate.Count > 0 ? currentDate.Min(q => q.a.CreateTime) : (DateTime?)null,
                     SignOutDate = currentDate.Count > 1 ? currentDate.Max(q => q.a.CreateTime) : (DateTime?)null
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

            var sorderwork = await UnitWork.Find<ServiceWorkOrder>(c => c.Status < 7).Where(c=>c.CurrentUserNsapId==userId).Select(c => c.ServiceOrderId).Distinct().ToListAsync();
            var sorder = await UnitWork.Find<ServiceOrder>(c => sorderwork.Contains(c.Id)).Take(3).Select(c => c.U_SAP_ID).ToArrayAsync();
            var orderIds = sorder.Length > 0 ? string.Join(",", sorder) : "";

            req.StartDate = req.StartDate ?? DateTime.Now;
            req.EndDate = req.EndDate ?? DateTime.Now;
            var start = Convert.ToDateTime(req.StartDate.Value.Date.ToString());
            DateTime end = Convert.ToDateTime(req.EndDate.ToDateTime().AddDays(1).ToString("D").ToString()).AddSeconds(-1);
            var pppUserMap = UnitWork.Find<AppUserMap>(c=>c.UserID==userId).FirstOrDefault();
            //按指定时间过滤（筛选轨迹）
            var realTimeLocation = await UnitWork.Find<RealTimeLocation>(null)
                                            .Where(c =>(c.CreateTime >= start && c.CreateTime <= end) && c.AppUserId==pppUserMap.AppUserId)
                                            .OrderByDescending(c => c.CreateTime)
                                            .ToListAsync();


            var data = realTimeLocation?.GroupBy(c=>c.CreateTime.ToString("yyyy-MM-dd")).Select(c=>new Trajectory
            {
                Date = c.Key,
                Pos = c.Select(p => new Position { Longitude = p.BaiduLongitude, Latitude = p.BaiduLatitude }).ToList()
            }).ToList();

            HistoryPositions history = new HistoryPositions();
            history.ServiceOrderId = orderIds;
            history.Trajectory = data;


            result.Data = history;
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