using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App
{
    /// <summary>
    /// 必修课模块
    /// </summary>
    public class CompulsoryCourseApp : OnlyUnitWorkBaeApp
    {
        /// <summary>
        /// 必修课模块
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public CompulsoryCourseApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
        }

        /// <summary>
        /// 讲师申请记录
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="AuditState"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        public async Task<TableData> TeacherApplyHistory(string Name, DateTime? startTime, DateTime? endTime, int? AuditState, int PageIndex, int PageSize)
        {
            var result = new TableData();
            result.Data = await UnitWork.Find<classroom_teacher>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(Name), c => c.Name.Contains(Name))
                .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                .WhereIf(endTime != null, c => c.CreateTime <= endTime)
                .WhereIf(AuditState != null && AuditState > 0, c => c.AuditState == AuditState)
                .Select(c => new { c.Id,c.Name, c.Age, c.AuditState, c.Department, c.CanTeachCourse, c.BeGoodATTerritory, c.CreateTime })
                .OrderByDescending(c=>c.Id)
                .Skip((PageIndex-1)*PageSize).Take(PageSize)
                .ToListAsync();
            return result;
        }
    }
}
