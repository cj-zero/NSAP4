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
                .Take(supplementCount).ToListAsync();
                recentCourse.AddRange(supplementCourse);
            }
            var teacherUserId = recentCourse.Select(zw => zw.AppUserId).ToList();

            var query = await UnitWork.Find<classroom_teacher_apply_log>(null)
               .Where(c => c.AuditState == 2 )
               .WhereIf(teacherUserId.Count >0, c=> teacherUserId.Contains(c.AppUserId))
               .Select(c => new { c.Name, c.AppUserId, c.HeaderImg ,c.Experience })
               .ToListAsync();

            if(query.Count < limt)
            {
                var supplementCount = limt - query.Count;
                var supplementTeacher = await UnitWork.Find<classroom_teacher_apply_log>(null)
                        .Where(c => c.AuditState == 2 && !teacherUserId.Contains(c.AppUserId))
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
