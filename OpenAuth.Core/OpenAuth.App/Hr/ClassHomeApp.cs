using Common;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.HuaweiOBS;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAuth.App.Hr;
using OpenAuth.App.Hr.Request;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Hr;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Hr
{

    /// <summary>
    ///  课堂首页
    /// </summary>
    public class ClassHomeApp : OnlyUnitWorkBaeApp
    {
        private IOptions<AppSetting> _appConfiguration;
        private HttpHelper _helper;
        private ILogger<ClassHomeApp> _logger;



        #region App

        /// <summary>
        /// 课堂首页
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        /// <param name="appConfiguration"></param>
        /// <param name="logger"></param>
        public ClassHomeApp(IUnitWork unitWork, IAuth auth, IOptions<AppSetting> appConfiguration, ILogger<ClassHomeApp> logger) : base(unitWork, auth)
        {
            _appConfiguration = appConfiguration;
            _helper = new HttpHelper(_appConfiguration.Value.AppPushMsgUrl);
            _logger = logger;
        }

        /// <summary>
        /// 必修课程列表
        /// </summary>
        /// <param name="appUserId">app用户id</param>
        /// <param name="key">关键词</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<TableData> CompulsoryCourseList(int appUserId, string key, int pageIndex, int pageSize)
        {
            var result = new TableData();
            List<object> obj = new List<object>();
            DateTime dt = DateTime.Now;
            var query = await (from a in UnitWork.Find<classroom_course_package_user>(null)
                               join c in UnitWork.Find<classroom_course_package_map>(null) on a.CoursePackageId equals c.CoursePackageId
                               join d in UnitWork.Find<classroom_course>(null) on c.CourseId equals d.Id
                               where a.AppUserId == appUserId && d.State == true
                               select new { a.Id, d.Name, d.Source, d.State, d.LearningCycle, a.CreateTime, a.CoursePackageId, c.Sort, c.CourseId })
                               .WhereIf(!string.IsNullOrWhiteSpace(key),c=>c.Name.Contains(key))
                               .OrderByDescending(c => c.Id)
                               .ToListAsync();

            List<int> coursePackageId = query.Select(c => c.CoursePackageId).Distinct().ToList();
            var courseIds = query.Select(c => c.CourseId).Distinct().ToList();
            var videoList = await UnitWork.Find<classroom_course_video>(null).Where(c => courseIds.Contains(c.CourseId)).ToListAsync();
            var videoPlayList = await UnitWork.Find<classroom_video_play_log>(null)
                .Where(c => coursePackageId.Contains(c.CoursePackageId) && c.AppUserId == appUserId)
                .Select(c => new { c.PlayDuration, c.CourseVideoId, c.CoursePackageId, c.CourseId })
                .ToListAsync();
            var examList = await UnitWork.Find<classroom_course_exam>(null)
               .Where(c => coursePackageId.Contains(c.CoursePackageId) && c.AppUserId == appUserId)
               .Select(c => new { c.CourseVideoId, c.IsPass, c.CourseId, c.CoursePackageId }).ToListAsync();
            var playList = await UnitWork.Find<classroom_video_play_log>(null).Where(c => coursePackageId.Contains(c.CoursePackageId) && c.AppUserId == appUserId).ToListAsync();
            foreach (var item in coursePackageId)
            {
                var list = query.Where(c => c.CoursePackageId == item).OrderBy(c => c.Sort).ToList();
                foreach (var row in list)
                {
                    int courseState = 0;
                    int i = 0;
                    decimal Schedule = 0;
                    var index = list.FindIndex(c => c.CourseId == row.CourseId) + 1;
                    var ids = list.Select(c => c.CourseId).Take(index);
                    int day = list.Where(c => ids.Contains(c.CourseId)).Sum(c => c.LearningCycle);
                    var EndTime = row.CreateTime.AddDays(day);
                    var courseVideoList = videoList.Where(c => c.CourseId == row.CourseId).ToList();
                    if (courseVideoList.Count > 0)
                    {
                        foreach (var vitem in courseVideoList)
                        {
                            var playDurationInfo = videoPlayList.Where(c => c.CourseVideoId == vitem.Id && c.CoursePackageId == item && c.CourseId == row.CourseId).OrderByDescending(c => c.PlayDuration).Select(c => new { c.PlayDuration }).FirstOrDefault();
                            var playDuration = playDurationInfo == null ? 0 : playDurationInfo.PlayDuration;
                            var isPlayFinish = (double)playDuration / vitem.Duration > 0.8;
                            var isPass = examList.Where(c => c.CourseId == row.CourseId && c.CourseVideoId == vitem.Id && c.IsPass == true).Any();
                            if (isPlayFinish == true && isPass == true)
                            {
                                i++;
                            }
                        }
                        Schedule = Math.Round((decimal)i / courseVideoList.Count, 2);
                    }
                    int playCount = playList.Where(c => c.CoursePackageId == item && c.CourseId == row.CourseId).Count();
                    int examCount = examList.Where(c => c.CoursePackageId == item && c.CourseId == row.CourseId).Count();

                    if (playCount <= 0 && examCount <= 0)
                    {
                        courseState = 3;
                    }
                    else
                    {
                        if (Schedule == 1)
                        {
                            courseState = 2;
                        }
                        else if (Schedule != 1)
                        {
                            if (dt >= EndTime)
                            {
                                courseState = 1;
                            }
                            else
                            {
                                courseState = 4;
                            }
                        }
                    }
                    obj.Add(new { row.Name, row.CreateTime, EndTime, Schedule, row.CoursePackageId, row.CourseId, courseState });
                }
            }
            result.Count = obj.Count;
            result.Data = obj.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList(); 
            return result;
        }

        /// <summary>
        /// 专题列表
        /// </summary>
        /// <param name="appUserId">app用户id</param>
        /// <param name="key">关键词</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<TableData> ClassroomSubjectList(int appUserId, string key, int pageIndex, int pageSize)
        {
            var result = new TableData();
            List<classroom_subject_dto> obj = new List<classroom_subject_dto>();

            var subjectList = await (from a in UnitWork.Find<classroom_subject>(null)
                                   .WhereIf(!string.IsNullOrWhiteSpace(key), a => a.Name.Contains(key))
                                   .Where(a => a.State == 1)
                                     select a).ToListAsync();

            var courseList = await (from a in UnitWork.Find<classroom_subject_course>(null)
                                    select a).ToListAsync();


            var userProgress = await (from a in UnitWork.Find<classroom_subject_course_user>(null)
                                      .Where(a => a.AppUserId == appUserId)
                                      select a).ToListAsync();

            foreach (var item in subjectList)
            {
                var courseCount = courseList.Where(a => a.SubjectId == item.Id).Count();
                if (courseCount == 0)
                {
                    continue;
                }
                var userProgressCount = userProgress.Where(a => a.SubjectId == item.Id && a.IsComplete == true).Count();

                classroom_subject_dto info = new classroom_subject_dto();
                info.Id = item.Id;
                info.ViewNumbers = item.ViewNumbers;
                info.Name = item.Name;
                info.State = item.State;
                info.CreateTime = item.CreateTime;
                info.Sort = item.Sort;
                info.CreateUser = item.CreateUser;
                if (courseCount == userProgressCount)
                {
                    info.Schedule = 100;
                    info.IsComplete = true;
                }
                else
                {
                    info.Schedule = userProgressCount * 100 / courseCount;
                    info.IsComplete = false;
                }
                obj.Add(info);
            }
            List<classroom_subject_dto> subList1 = obj.Where(a => a.IsComplete == true).OrderBy(a => a.Sort).ToList();
            List<classroom_subject_dto> subList2 = obj.Where(a => a.IsComplete == false && a.Schedule > 0).OrderByDescending(a => a.Schedule).ToList();
            List<classroom_subject_dto> subList3 = obj.Where(a => a.IsComplete == false && a.Schedule == 0).OrderBy(a => a.Sort).ToList();

            obj.Clear();
            obj.AddRange(subList2);
            obj.AddRange(subList3);
            obj.AddRange(subList1);
            result.Count = obj.Count();
            result.Data = obj.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return result;
        }


        /// <summary>
        /// 视频回放列表
        /// </summary>
        /// <param name="appUserId">app用户id</param>
        /// <param name="key">关键词</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>


        public async Task<TableData> TeacherCoursePlayBack(int appUserId, string key, int pageIndex, int pageSize)
        {
            var result = new TableData();
            DateTime dt = DateTime.Now;  //当前时间
            DateTime yesterday = dt.AddDays(-1);

            var query = (from a in UnitWork.Find<classroom_teacher_course>(null)
                         .Where(zw => zw.AuditState == 2
                         && zw.TeachingMethod == 2
                         && zw.EndTime > yesterday)
                         .WhereIf(!string.IsNullOrWhiteSpace(key), a => a.Title.Contains(key))
                         select a);
            // 视频总数
            result.Count = await query.CountAsync();
            var pageData = await query.OrderByDescending(zw => zw.EndTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            var teacherUserIds = pageData.Select(zw => zw.AppUserId).Distinct().ToList();
            var teacherList = await UnitWork.Find<classroom_teacher_apply_log>(null)
                        .Where(c => c.AuditState == 2 && teacherUserIds.Contains(c.AppUserId))
                        .ToListAsync();
            var teacherCourseIds = teacherList.Select(c => c.Id).ToList();
            // 观看记录
            var viewLogs = await UnitWork.Find<classroom_teacher_course_play_log>(null)
                    .Where(c => c.AppUserId == appUserId && teacherCourseIds.Contains(c.TeacherCourseId)).ToListAsync();

            List<TeacherCourseResp> obj = new List<TeacherCourseResp>();
            foreach (var item in pageData)
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
        #endregion

    }
}
