using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
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
            var objs = await UnitWork.Find<RealTimeLocation>(w => w.AppUserId == (int)map.AppUserId && w.CreateTime >= Convert.ToDateTime(startDate) && w.CreateTime <= Convert.ToDateTime(endDate)).OrderBy(o => o.CreateTime).Select(s => new { Latitude=s.BaiduLatitude, Longitude=s.BaiduLongitude, s.CreateTime }).ToListAsync();
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
            //var roleId = await UnitWork.Find<Role>(c => c.Name.Equals("售后技术员")).Select(c => c.Id).FirstOrDefaultAsync();
            //var userIds = await UnitWork.Find<Relevance>(c => c.Key.Equals(Define.USERROLE) && c.SecondId.Equals(roleId)).Select(c => c.FirstId).ToListAsync();
            ////未完成服务id的
            //var serviceWorkOrder = UnitWork.Find<ServiceWorkOrder>(c => c.Status < 7);
            //var noComplete = await serviceWorkOrder
            //                        .Where(c => !string.IsNullOrWhiteSpace(c.CurrentUserNsapId))
            //                        .Select(c => c.CurrentUserNsapId)
            //                        .Distinct()
            //                        .ToListAsync();
            //userIds.AddRange(noComplete);

            DateTime start = DateTime.Now.Date;
            DateTime end = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("D").ToString()).AddSeconds(-1);
            DateTime monthAgo = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("D").ToString());
            var pppUserMap = await UnitWork.Find<AppUserMap>(null).ToListAsync();

            //所有人最新定位信息
            var realTimeLocationHis = await UnitWork.FromSql<RealTimeLocation>(@$"SELECT * from realtimelocation where Id in  (
                                        SELECT max(Id) as Id from realtimelocation GROUP BY AppUserId
                                        ) ORDER BY CreateTime desc")
                                        .ToListAsync();


            var locaotionInfoHistory = from a in realTimeLocationHis
                                       join b in pppUserMap on a.AppUserId equals b.AppUserId
                                       select new { a, b.UserID };

            List<string> userIds = new List<string>();
            if (loginContext.Roles.Any(c => c.Name == "智慧大屏查看-部门主管") && loginContext.User.Account!=Define.SYSTEM_USERNAME)
            {
                var orgId = loginContext.Orgs.OrderByDescending(c => c.CascadeId).FirstOrDefault().Id;
                var userid = await UnitWork.Find<Relevance>(c => c.Key == Define.USERORG && c.SecondId == orgId).Select(c => c.FirstId).ToListAsync();
                userIds.AddRange(userid);
            }


            var now = DateTime.Now;
            var data = await (from a in UnitWork.Find<AppUserMap>(null)
                              join b in UnitWork.Find<User>(c => c.Status == 0).WhereIf(req.Name.Count > 0, c => req.Name.Contains(c.Name)).WhereIf(userIds.Count > 0, c => userIds.Contains(c.Id)) on a.UserID equals b.Id
                              select new User { Id = b.Id, Name = b.Name, Mobile = b.Mobile }).ToListAsync();

            var da1 = data.Select(c =>
             {
                 //当天是否有定位记录
                 var currentLoca = locaotionInfoHistory.Where(q => q.UserID.Equals(c.Id)).FirstOrDefault();
                 var onlineState = "离线";
                 var interv = "30天前";
                 decimal? longi = 0, lati = 0;
                 double? totalHour = 720;

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
                     Province= currentLoca?.a.Province,
                     City = currentLoca?.a.City,
                     Area = currentLoca?.a.Area,
                     Mobile = c.Mobile,
                     Status = onlineState,
                     Interval = interv,
                     Longitude = longi,
                     Latitude = lati,
                     TotalHour= totalHour,
                 };

             });
            

            if (!string.IsNullOrWhiteSpace(req.Province)) da1=da1.Where(c => c.Province==req.Province && !string.IsNullOrWhiteSpace(c.Province));
            if (!string.IsNullOrWhiteSpace(req.City)) da1 = da1.Where(c => c.City == req.City && !string.IsNullOrWhiteSpace(c.City));
            if (!string.IsNullOrWhiteSpace(req.Area)) da1=da1.Where(c => c.Area == req.Area && !string.IsNullOrWhiteSpace(c.Area));


            result.Count = da1.Count();
            result.Data = da1.OrderBy(c => c.TotalHour);
            return result;
        }

        /// <summary>
        /// 查询技术员轨迹
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> HistoryTrajectory(QueryTrajectoryReq req)
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
            if (pppUserMap==null)
            {
                result.Code = 500;
                result.Message = "该用户暂未绑定APP";
                return result;
            }
            //按指定时间过滤（筛选轨迹）
            var realTimeLocation = await UnitWork.Find<RealTimeLocation>(null)
                                            .Where(c =>(c.CreateTime >= start && c.CreateTime <= end) && c.AppUserId==pppUserMap.AppUserId && !string.IsNullOrWhiteSpace(c.Province))
                                            .OrderByDescending(c => c.CreateTime)
                                            .ToListAsync();
            //当天定位数据
            var currentDate = await UnitWork.Find<RealTimeLocation>(c => c.AppUserId == pppUserMap.AppUserId && !string.IsNullOrWhiteSpace(c.Province) && c.CreateTime>=DateTime.Now.Date && c.CreateTime< DateTime.Now.AddDays(1).Date).ToListAsync();
            //var currentDate = await UnitWork.FromSql<RealTimeLocation>(@$"SELECT * from realtimelocation where AppUserId={pppUserMap.AppUserId} AND TO_DAYS(CreateTime)=TO_DAYS(NOW())").ToListAsync();

            var data = realTimeLocation?.GroupBy(c=>c.CreateTime.ToString("yyyy-MM-dd")).Select(c=>new Trajectory
            {
                Date = c.Key,
                Pos = c.Select(p => new Position 
                { 
                    Longitude = p.BaiduLongitude, 
                    Latitude = p.BaiduLatitude,
                    Address= p?.Province + p?.City + p?.Area + p?.Addr,
                    PosDate=p?.CreateTime
                }).OrderBy(q=>q.PosDate).ToList()
            }).ToList();


            HistoryPositions history = new HistoryPositions();
            history.ServiceOrderId = orderIds;
            history.Trajectory = data;
            history.SignInDate = currentDate.Count > 0 ? currentDate.Min(q => q.CreateTime) : (DateTime?)null;
            history.SignOutDate = currentDate.Count > 1 ? currentDate.Max(q => q.CreateTime) : (DateTime?)null;


            result.Data = history;
            return result;
        }

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<byte[]> ExcelAttendanceInfo(QueryLocationInfoReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var relevances = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG).ToListAsync();
            var orgs = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).ToListAsync();

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

            var pppUserMap = await UnitWork.Find<AppUserMap>(null).ToListAsync();

            req.StartDate = req.StartDate ?? DateTime.Now;
            req.EndDate = req.EndDate ?? DateTime.Now;
            var start = Convert.ToDateTime(req.StartDate.Value.Date.ToString());
            DateTime end = Convert.ToDateTime(req.EndDate.ToDateTime().AddDays(1).ToString("D").ToString()).AddSeconds(-1);

            var realTimeLocation = await UnitWork.Find<RealTimeLocation>(c => (c.CreateTime >= start && c.CreateTime <= end) && !string.IsNullOrWhiteSpace(c.Province))
                .WhereIf(!string.IsNullOrWhiteSpace(req.Province), c => c.Province == req.Province)
                .WhereIf(!string.IsNullOrWhiteSpace(req.City), c => c.City == req.City).Select(c => new
                {
                    c.AppUserId,
                    c.Province,
                    c.City,
                    CreateTime = c.CreateTime.ToString("yyyy-MM-dd")
                }).ToListAsync();

            var query = from a in realTimeLocation
                        join b in pppUserMap on a.AppUserId equals b.AppUserId
                        select new { a, b.UserID };

            //考勤记录
            var clock = await UnitWork.Find<AttendanceClock>(c => c.ClockDate >= req.StartDate.Value.Date && c.ClockDate < req.EndDate.Value.Date.AddDays(1)).ToListAsync();
            var clockinfo = clock.GroupBy(c => new { c.Name, c.AppUserId, c.ClockDate, c.Org }).Select(c => new
            {
                c.Key.AppUserId,
                ClockDate=c.Key.ClockDate,
                Count = c.Count() >= 2 ? 1 : 0//两条打卡算考勤
            }).ToList();

            //定位数据
            var group = query.Select(g => new
            {
                g.UserID,
                g.a.CreateTime,
                g.a.Province,
                g.a.City,
                Count = clockinfo.Where(q => q.AppUserId == g.a.AppUserId).Sum(q => q.Count)
            }).ToList();

            var data = await UnitWork.Find<User>(c => userIds.Contains(c.Id)).WhereIf(req.Name.Count > 0, c => req.Name.Contains(c.Name)).ToListAsync();
            List<RealTimeLocationExcelDto> excelDtos = new List<RealTimeLocationExcelDto>();
            foreach (var datauser in data)
            {
                var secondId = relevances.Where(r => r.FirstId == datauser.Id).Select(r => r.SecondId).ToList();
                var orgname = orgs.Where(org => secondId.Contains(org.Id)).OrderByDescending(org => org.CascadeId).Select(org => org.Name).FirstOrDefault();
                foreach (var item in group.Where(o => datauser.Id == o.UserID))
                {
                    excelDtos.Add(new RealTimeLocationExcelDto
                    {
                        Org = orgname,
                        Name = datauser.Name,
                        CreateDate = item.CreateTime,
                        Province = item.Province,
                        City = item.City,
                        Count = item.Count
                    });
                }
            }

            IExporter exporter = new ExcelExporter();
            var bytes = await exporter.ExportAsByteArray(excelDtos);
            return bytes;
        }

        /// <summary>
        /// 分析报表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> OnlineDurationReport(QueryLocationInfoReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();

            //技术员角色
            //var roleId = await UnitWork.Find<Role>(c => c.Name.Equals("售后技术员")).Select(c => c.Id).FirstOrDefaultAsync();
            //var userIds = await UnitWork.Find<Relevance>(c => c.Key.Equals(Define.USERROLE) && c.SecondId.Equals(roleId)).Select(c => c.FirstId).ToListAsync();
            ////未完成服务id的
            //var serviceWorkOrder = UnitWork.Find<ServiceWorkOrder>(c => c.Status < 7);
            //var noComplete = await serviceWorkOrder
            //                        .Where(c => !string.IsNullOrWhiteSpace(c.CurrentUserNsapId))
            //                        .Select(c => c.CurrentUserNsapId)
            //                        .Distinct()
            //                        .ToListAsync();
            //userIds.AddRange(noComplete);

            var pppUserMap = await UnitWork.Find<AppUserMap>(null).ToListAsync();

            req.StartDate = req.StartDate ?? DateTime.Now;
            req.EndDate = req.EndDate ?? DateTime.Now;
            var start = Convert.ToDateTime(req.StartDate.Value.Date.ToString());
            DateTime end = Convert.ToDateTime(req.EndDate.ToDateTime().AddDays(1).ToString("D").ToString()).AddSeconds(-1);

            var realTimeLocation = await UnitWork.Find<RealTimeLocation>(c => c.CreateTime >= start && c.CreateTime <= end && !string.IsNullOrWhiteSpace(c.Province))
                .WhereIf(!string.IsNullOrWhiteSpace(req.Province), c => c.Province == req.Province)
                .WhereIf(!string.IsNullOrWhiteSpace(req.City), c => c.City == req.City)
                .ToListAsync();

            //数据权限
            List<string> userIds = new List<string>();
            if (loginContext.Roles.Any(c => c.Name == "智慧大屏查看-部门主管") && loginContext.User.Account != Define.SYSTEM_USERNAME)
            {
                var orgId = loginContext.Orgs.OrderByDescending(c => c.CascadeId).FirstOrDefault().Id;
                var userid = await UnitWork.Find<Relevance>(c => c.Key == Define.USERORG && c.SecondId == orgId).Select(c => c.FirstId).ToListAsync();
                userIds.AddRange(userid);
            }

            //部门
            var OrgNames = from b in UnitWork.Find<Relevance>(r => r.Key == Define.USERORG).WhereIf(userIds.Count > 0, r => userIds.Contains(r.FirstId))
                           join c in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on b.SecondId equals c.Id into bc
                           from c in bc.DefaultIfEmpty()
                           join u in UnitWork.Find<User>(null).WhereIf(userIds.Count > 0, r => userIds.Contains(r.Id))
                                    .WhereIf(req.Name.Count > 0, c => req.Name.Contains(c.Name)) on b.FirstId equals u.Id into bu
                           from u in bu.DefaultIfEmpty()
                           where b.Key.Equals(Define.USERORG)
                           select new { OrgId = c.Id, OrgName = c.Name, c.CascadeId, UserId = u.Id, UserName = u.Name };
            var OrgNameList = await OrgNames.OrderByDescending(o => o.CascadeId).ToListAsync();
            OrgNameList = OrgNameList.GroupBy(o => o.UserId).Select(o => o.First()).ToList();


            var query = from a in realTimeLocation
                        join b in pppUserMap on a.AppUserId equals b.AppUserId
                        join c in OrgNameList on b.UserID equals c.UserId
                        select new GroupByInfoResp
                        {
                            UserId = c.UserId,
                            UserName = c.UserName,
                            OrgName = c.OrgName,
                            Province = a.Province,
                            City = a.City,
                            CreateTime = a.CreateTime
                        };

            List<RealTimeLocationReportResp> reportResps = new List<RealTimeLocationReportResp>();

            var list1 = query.GroupBy(c => c.OrgName).Select(c =>
            {
                var list = c.OrderByDescending(o => o.CreateTime).ToList();
                return new RealTimeLocationReportSubtableResp
                {
                    StatName = c.Key,
                    Duration = GetDuration(list)
                };
            }).ToList();
            reportResps.Add(new RealTimeLocationReportResp { StatType = "Department", StatList = list1 });

            var list2 = query.GroupBy(c => new { c.UserId, c.UserName }).Select(c =>
            {
                var list = c.OrderByDescending(o => o.CreateTime).ToList();
                return new RealTimeLocationReportSubtableResp
                {
                    StatId = c.Key.UserId,
                    StatName = c.Key.UserName,
                    Duration = GetDuration(list)
                };
            }).ToList();
            reportResps.Add(new RealTimeLocationReportResp { StatType = "Staff", StatList = list2 });

            var list3 = query.GroupBy(c => c.Province).Select(c =>
            {
                var list = c.OrderByDescending(o => o.CreateTime).ToList();
                return new RealTimeLocationReportSubtableResp
                {
                    StatName = c.Key,
                    Duration = GetDuration(list)
                };
            }).ToList();
            reportResps.Add(new RealTimeLocationReportResp { StatType = "Province", StatList = list3 });

            var list4 = query.GroupBy(c => c.City).Select(c =>
            {
                var list = c.OrderByDescending(o => o.CreateTime).ToList();
                return new RealTimeLocationReportSubtableResp
                {
                    StatName = c.Key,
                    Duration = GetDuration(list)
                };
            }).ToList();
            reportResps.Add(new RealTimeLocationReportResp { StatType = "City", StatList = list4 });


            var maxhour = list2.Count > 0 ? list2.Max(c => c.Duration) : 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (maxhour>= array[i,0] && maxhour< array[i,1])
                {
                    maxhour = array[i, 1];
                    break;
                }
            }

            var inter = maxhour / 10;//间隔
            double temp = 0;
            List<RealTimeLocationReportSubtableResp> list5 = new List<RealTimeLocationReportSubtableResp>();
            for (int i = 0; i < 10; i++)
            {
                //时长间隔内数据
                var countinfo = list2.Where(c => c.Duration >= temp && c.Duration < (temp + inter)).ToList();
                //if (countinfo.Count>0)
                //{
                    list5.Add(new RealTimeLocationReportSubtableResp
                    {
                        StatName = $"{(temp + inter)}",
                        Duration = countinfo.Count,//人数
                        ReportList = countinfo.Select(c => new RealTimeLocationReportSubtableResp
                        {
                            StatName = c.StatName //姓名
                        }).ToList()
                    });
                //}
                temp += inter;
            }
            reportResps.Add(new RealTimeLocationReportResp { StatType = "NumberOfEmployees", StatList = list5 });
            result.Data = reportResps;
            return result;
        }

        private static readonly int[,] array = new int[,] {
            { 0,10 },{ 10,20 },{ 20,30 },{ 30,40 },{ 40,50 },{ 50,100 },{ 100,200 },{ 200,400 },{ 400, 600 },{ 600, 800 },{ 800, 1000 },
            { 1000,2000 },{ 2000,3000 },{ 3000,4000 },{ 4000,5000 },{ 5000,6000 },
            { 6000,7000 },{ 7000,8000 },{ 8000,9000 },{ 9000,10000 },
        };

        private double GetDuration(List<GroupByInfoResp> list)
        {
            double total = 0;
            if (list.Count==1) return 0.02;//1分钟约等于0.02时
            for (int i = 0; i < list.Count - 1; i++)
            {
                var date1 = list[i].CreateTime;
                var date2 = list[i + 1].CreateTime;
                var timespan = date1.Subtract(date2);

                if (timespan.TotalMinutes > 10)
                    total += 1;
                else
                    total += timespan.TotalMinutes;
            }
            TimeSpan ts = TimeSpan.FromMinutes(total);
            return Math.Round(ts.TotalHours, 2);
        }

        /// <summary>
        /// 获取所有客户
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetCustomer() 
        {
            TableData result = new TableData();
            var data = await UnitWork.Find<crm_ocrd>(null).Select(c => new { Name = c.CardName, Code = c.CardCode }).Distinct().ToListAsync();
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