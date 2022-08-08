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
            result.Data = obj.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return result;
        }


















        #endregion









    }
}
