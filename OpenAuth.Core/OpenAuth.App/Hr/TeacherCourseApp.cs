using Infrastructure.Extensions;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAuth.App.Hr;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App
{
    /// <summary>
    /// 直播视频 模块
    /// </summary>
    public class TeacherCourseApp : OnlyUnitWorkBaeApp
    {
        private IOptions<AppSetting> _appConfiguration;
        private HttpHelper _helper;


        /// <summary>
        ///  直播视频 模块
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        /// <param name="appConfiguration"></param>
        public TeacherCourseApp(IUnitWork unitWork, IAuth auth, IOptions<AppSetting> appConfiguration) : base(unitWork, auth)
        {
            _appConfiguration = appConfiguration;
            _helper = new HttpHelper(_appConfiguration.Value.AppPushMsgUrl);
        }

        #region App

        /// <summary>
        ///  推荐老师
        /// </summary>
        /// <param name="limt"></param>
        /// <returns></returns>
        public async Task<TableData> RecommendTeachers(int limt)
        {
            var result = new TableData();

            DateTime dt = DateTime.Now;  //当前时间
            DateTime startMonth = dt.AddDays(1 - dt.Day); //本月月初

            List<classroom_teacher_course> recentCourse = new List<classroom_teacher_course>();

            // 本月开课过或者即将要开课
            recentCourse = await UnitWork.Find<classroom_teacher_course>(null)
                .Where(c => c.AuditState == 2 && (c.StartTime >= dt || (c.EndTime >= startMonth && c.StartTime < dt)))
                .ToListAsync();

            if (recentCourse.Count < limt)
            {
                var supplementCount = limt - recentCourse.Count;
                var courseIds = recentCourse.Select(zw => zw.Id);
                var supplementCourse = await UnitWork.Find<classroom_teacher_course>(null)
                .Where(c => c.AuditState == 2 && !courseIds.Contains(c.Id)).OrderByDescending(c => c.EndTime)
                .Take(supplementCount*2).ToListAsync();
                recentCourse.AddRange(supplementCourse);
            }
            var teacherUserId = recentCourse.Select(zw => zw.AppUserId).Distinct().ToList();

            var query = await UnitWork.Find<classroom_teacher_apply_log>(null)
               .Where(c => c.AuditState == 2 )
               .WhereIf(teacherUserId.Count >0, c=> teacherUserId.Contains(c.AppUserId))
               .Select(c => new { c.Name, c.AppUserId, c.HeaderImg ,c.Experience })
               .ToListAsync();

            if(query.Count < limt)
            {
                var filterTeacherIds = query.Select(zw => zw.AppUserId).Distinct().ToList();
                var supplementCount = limt - query.Count;
                var supplementTeacher = await UnitWork.Find<classroom_teacher_apply_log>(null)
                        .Where(c => c.AuditState == 2 && !filterTeacherIds.Contains(c.AppUserId))
                        .Select(c => new { c.Name, c.AppUserId, c.HeaderImg, c.Experience }).OrderByDescending(c => c.Experience)
                        .Take(supplementCount)
                        .ToListAsync();
                query.AddRange(supplementTeacher);
            }

            var randomQuery = RandomExtract(query, limt);
            result.Data = randomQuery;

            return result;
        }

        /// <summary>
        ///  直播预告
        /// </summary>
        /// <param name="appUserId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        public async Task<TableData> TeacherCourseAdvanceNotice(int appUserId,int pageIndex, int pageSize)
        {
            var result = new TableData();
            DateTime midNight = DateTime.Now.Date; // 当天凌晨
            DateTime dt = DateTime.Now;

            // 获取预告+ 直播中(开课中)+当天结束的开课
            var query = (from a in UnitWork.Find<classroom_teacher_course>(null)
                        .Where(zw => zw.AuditState == 2
                        && (zw.EndTime > midNight))
                        select a); 

            result.Count = await query.CountAsync();
            var totalList = await query.ToListAsync();

            var pageData = totalList.Select( a=> new  teacher_course_sign
                {
                    Id = a.Id,
                    Title = a.Title,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    ForTheCrowd = a.ForTheCrowd,
                    TeachingMethod = a.TeachingMethod,
                    TeachingAddres = a.TeachingAddres,
                    AppUserId = a.AppUserId,
                    BackgroundImage = a.BackgroundImage,
                    VideoUrl = a.VideoUrl,
                    AuditState = a.AuditState,
                    CreateTime = a.CreateTime,
                    ViewedCount = a.ViewedCount,
                    Sign = getTeacherCourseSign(a.StartTime, a.EndTime, a.TeachingMethod, midNight, dt)
            }).OrderByDescending(zw => zw.Sign).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList(); ;

            // 二次排序
            var livingList = pageData.Where(zw => zw.Sign == (int)TeacherCourseSignEnum.Living).OrderByDescending(zw => zw.ViewedCount).ToList();
            var inClassList = pageData.Where(zw => zw.Sign == (int)TeacherCourseSignEnum.InClass).OrderBy(zw => zw.StartTime).ToList();
            var advanceNoticeList = pageData.Where(zw => zw.Sign == (int)TeacherCourseSignEnum.AdvanceNotice).OrderBy(zw => zw.StartTime).ToList();
            var endOfflineList = pageData.Where(zw => zw.Sign == (int)TeacherCourseSignEnum.EndOffline).OrderBy(zw => zw.StartTime).ToList();
            var endOnLineList = pageData.Where(zw => zw.Sign == (int)TeacherCourseSignEnum.EndOnLine).OrderBy(zw => zw.StartTime).ToList();
            //if(endOnLineList.Count >0)
            //{
            //    var teacherCourseIds = endOnLineList.Select(zw => zw.Id);
            //    var viewStatistics = await UnitWork.Find<classroom_teacher_course_play_log>(null)
            //        .Where(c => c.AppUserId == appUserId  && teacherCourseIds.Contains(c.TeacherCourseId))
            //        .GroupBy(t=>t.TeacherCourseId).Select(zw => new { TeacherCourseId = zw.Key, TotalPalyDuration =  zw.Sum(s=>s.PlayDuration) }).ToListAsync();

            //    var results = (from e in endOnLineList
            //                   join v in viewStatistics on e.Id equals v.TeacherCourseId into vtem
            //                   from ev in vtem.DefaultIfEmpty()
            //                   select new
            //                   {
            //                       e,
            //                       TotalPalyDuration = ev == null ? 0 : ev.TotalPalyDuration
            //                   }).ToList().OrderByDescending(zw=>zw.TotalPalyDuration).ToList();

            //    endOnLineList.Clear();
            //    endOnLineList = results.Select(zw => zw.e).ToList();
            //}
            var totalCourseList = new List<teacher_course_sign>();
            totalCourseList.AddRange(livingList);
            totalCourseList.AddRange(inClassList);
            totalCourseList.AddRange(advanceNoticeList);
            totalCourseList.AddRange(endOfflineList);
            totalCourseList.AddRange(endOnLineList);

            var teacherUserIds = totalCourseList.Select(zw => zw.AppUserId).Distinct().ToList();
            var teacherList = await UnitWork.Find<classroom_teacher_apply_log>(null)
                        .Where(c => c.AuditState == 2 && teacherUserIds.Contains(c.AppUserId))
                        .ToListAsync();

            List<TeacherCourseResp> obj = new List<TeacherCourseResp>();
            foreach (var item in totalCourseList)
            {
                var teacher = teacherList.FirstOrDefault(zw => zw.AppUserId == item.AppUserId);
                var newCourseResp = new TeacherCourseResp
                {
                    Id = item.Id,
                    Title = item.Title,
                    Name = teacher == null ? string.Empty : teacher.Name,
                    AppUserId = item.AppUserId,
                    Department = teacher == null ? string.Empty : teacher.Department,
                    ForTheCrowd = item.ForTheCrowd,
                    TeachingMethod = item.TeachingMethod,
                    TeachingAddres = item.TeachingAddres,
                    BackgroundImage = item.BackgroundImage,
                    VideoUrl = item.VideoUrl,
                    ViewedCount = item.ViewedCount,
                    StartTime = item.StartTime.ToString(Defaults.DateTimeFormat),
                    EndTime = item.EndTime.ToString(Defaults.DateTimeFormat),
                    StartHourMinute = item.StartTime.ToString(Defaults.DateHourFormat),
                    EndHourMinute = item.EndTime.ToString(Defaults.DateHourFormat),
                };
                obj.Add(newCourseResp);
            }
            result.Data = obj;
            return result;
        }

        /// <summary>
        ///  视频回放
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        public async Task<TableData> TeacherCoursePlayBack(int appUserId, int pageIndex, int pageSize)
        {
            var result = new TableData();
            DateTime dt = DateTime.Now;  //当前时间
            DateTime yesterday = dt.AddDays(-1);

            var query = (from a in UnitWork.Find<classroom_teacher_course>(null)
                         .Where(zw => zw.AuditState == 2 
                         && zw.TeachingMethod == 2
                         && zw.EndTime > yesterday)
                         select a);
            // 视频总数
            result.Count = await query.CountAsync();
            var pageData = await query.OrderByDescending(zw=>zw.EndTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            var teacherUserIds = pageData.Select(zw => zw.AppUserId).Distinct().ToList();
            var teacherList = await UnitWork.Find<classroom_teacher_apply_log>(null)
                        .Where(c => c.AuditState == 2 && teacherUserIds.Contains(c.AppUserId))
                        .ToListAsync();

            var teacherCourseIds = teacherList.Select(c => c.Id).ToList();
            // 观看记录
            var viewLogs = await UnitWork.Find<classroom_teacher_course_play_log>(null)
                    .Where(c => c.AppUserId == appUserId  && teacherCourseIds.Contains(c.TeacherCourseId)).ToListAsync();

            List<TeacherCourseResp> obj = new List<TeacherCourseResp>();
            foreach(var item in pageData)
            {
                var teacher = teacherList.FirstOrDefault(zw => zw.AppUserId == item.AppUserId);
                var log = viewLogs.FirstOrDefault(zw => zw.TeacherCourseId == item.Id);
                var newCourseResp = new TeacherCourseResp
                {
                    Id = item.Id,
                    Title = item.Title,
                    Name = teacher == null ? string.Empty : teacher.Name,
                    AppUserId = item.AppUserId,
                    Department = teacher == null ? string.Empty : teacher.Department,
                    ForTheCrowd = item.ForTheCrowd,
                    TeachingMethod = item.TeachingMethod,
                    TeachingAddres = item.TeachingAddres,
                    BackgroundImage = item.BackgroundImage,
                    VideoUrl = item.VideoUrl,
                    ViewedCount = item.ViewedCount,
                    StartTime = item.StartTime.ToString(Defaults.DateTimeFormat),
                    EndTime = item.EndTime.ToString(Defaults.DateTimeFormat),
                    StartHourMinute = item.StartTime.ToString(Defaults.DateHourFormat),
                    EndHourMinute = item.EndTime.ToString(Defaults.DateHourFormat),
                    PlayDuration = log == null ? 0 : log.PlayDuration,
                };
                obj.Add(newCourseResp);
            }
            result.Data = obj;
            return result;
        }

        /// <summary>
        /// 保存观看记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> SavePlayLog(TeacherCoursePlayLogReq req)
        {
            var result = new TableData();
            try
            {
                var query = await (from a in UnitWork.Find<classroom_teacher_course_play_log>(null)
                                    .Where(a => a.AppUserId == req.AppUserId && a.TeacherCourseId == req.TeacherCourseId)
                                   select a).FirstOrDefaultAsync();
                if (query == null)
                {
                    classroom_teacher_course_play_log log = new classroom_teacher_course_play_log();
                    log.TeacherCourseId = req.TeacherCourseId;
                    log.AppUserId = req.AppUserId;
                    log.PlayDuration = req.PlayDuration;
                    log.TotalDuration = req.TotalDuration;
                    log.CreateTime = DateTime.Now;
                    log.UpdateUser = null;
                    log.UpdateTime = null;
                    await UnitWork.AddAsync<classroom_teacher_course_play_log, int>(log);
                    await UnitWork.SaveAsync();

                }
                else
                {
                    query.PlayDuration = req.PlayDuration;
                    query.TotalDuration = req.TotalDuration;
                    query.UpdateTime = DateTime.Now;
                    query.UpdateUser = req.AppUserId;
                    await UnitWork.UpdateAsync(query);
                    await UnitWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                throw;
            }
            return result;
        }


        /// <summary>
        /// 讲师视频点击埋点
        /// </summary>
        /// <param name="id">讲师视频id</param>
        /// <returns></returns>
        public async Task<TableData> TeacherCourseBuriedPoint(int id)
        {
            var result = new TableData();
            var videoInfo = await UnitWork.Find<classroom_teacher_course>(null).Where(c => c.Id == id).FirstOrDefaultAsync();
            if(videoInfo != null)
            {
                videoInfo.ViewedCount++;
                await UnitWork.UpdateAsync(videoInfo);
                await UnitWork.SaveAsync();
            }
            return result;
        }

        /// <summary>
        ///  获取视频标记值
        /// </summary>
        /// <param name="startTime">开始时间 </param>
        /// <param name="endTime">结束时间</param>
        /// <param name="teachingMethod">1:线下 2:线上 </param>
        /// <param name="midNight">午夜凌晨 </param>
        ///  <param name="dt">当前时间 </param>
        /// <returns></returns>
        public int getTeacherCourseSign(DateTime startTime, DateTime endTime, int teachingMethod, DateTime midNight, DateTime dt)
        {
            // 优先级：直播中 4 > 开课中 3 > 预告 2 > 线上待录播 = 线下已结束 1
            var sortValue = (int)TeacherCourseSignEnum.Default;
            if (endTime < midNight)
            {
                return sortValue;
            }
            // 直播中/开课中
            if (dt >= startTime && dt <= endTime)
            {
                sortValue = teachingMethod == 2 ? (int)TeacherCourseSignEnum.Living : (int)TeacherCourseSignEnum.InClass;
            }
            else if (startTime > dt)
            {
                // 预告
                sortValue = (int)TeacherCourseSignEnum.AdvanceNotice;

            }
            else if (endTime > midNight && startTime < dt)
            {

                sortValue = teachingMethod == 2 ? (int)TeacherCourseSignEnum.EndOnLine : (int)TeacherCourseSignEnum.EndOffline; ;
            }
            return sortValue;
        }

        /// <summary>
        /// 随机
        /// </summary>
        /// <returns></returns>
        public List<T> RandomExtract<T>(List<T> list, int limit = 0)
        {
            if (limit > 0)
            {
                return (from a in list
                        orderby (Guid.NewGuid())
                        select a).Take(limit).ToList();
            }
            else
            {
                return (from a in list
                        orderby (Guid.NewGuid())
                        select a).ToList();
            }
        }

        #endregion
    }
}
