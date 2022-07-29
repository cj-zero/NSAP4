using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Hr;
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
    /// 讲师相关
    /// </summary>
    public class LecturerApp : OnlyUnitWorkBaeApp
    {
        /// <summary>
        /// 讲师相关
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public LecturerApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
        }

        #region erp
        /// <summary>
        /// 讲师申请记录
        /// </summary>
        /// <param name="name"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="auditState"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<TableData> TeacherApplyHistory(string name, DateTime? startTime, DateTime? endTime, int? auditState, int pageIndex, int pageSize)
        {
            var result = new TableData();
            result.Data = await UnitWork.Find<classroom_apply_teacher_log>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
                .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                .WhereIf(endTime != null, c => c.CreateTime <= endTime)
                .WhereIf(auditState != null && auditState > 0, c => c.AuditState == auditState)
                .Select(c => new { c.Id, c.Name, c.Age, c.AuditState, c.Department, c.CanTeachCourse, c.BeGoodATTerritory, c.CreateTime })
                .OrderByDescending(c => c.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .ToListAsync();
            result.Count = await UnitWork.Find<classroom_apply_teacher_log>(null)
           .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
           .WhereIf(startTime != null, c => c.CreateTime >= startTime)
           .WhereIf(endTime != null, c => c.CreateTime <= endTime)
           .WhereIf(auditState != null && auditState > 0, c => c.AuditState == auditState)
           .CountAsync();
            return result;
        }

        /// <summary>
        /// 讲师申请审核
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> TeacherApplyAudit(TeacherApplyAuditReq req)
        {
            var result = new TableData();
            var user = _auth.GetCurrentUser().User;
            var query = await UnitWork.Find<classroom_apply_teacher_log>(null).FirstOrDefaultAsync(c => c.Id == req.id);
            if (query != null)
            {
                query.AuditState = req.auditState;
                query.OperationUser = user.Name;
                query.ModifyTime = DateTime.Now;
            }
            await UnitWork.UpdateAsync(query);
            await UnitWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// 讲师列表
        /// </summary>
        /// <param name="name"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="grade"></param>
        /// <returns></returns>
        public async Task<TableData> TeacherList(string name, DateTime? startTime, DateTime? endTime, int? grade, int pageIndex, int pageSize)
        {
            var result = new TableData();
            result.Data = await UnitWork.Find<classroom_apply_teacher_log>(null)
                .Where(c=>c.AuditState!=1 && c.AuditState!=3)
                .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
                .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                .WhereIf(endTime != null, c => c.CreateTime <= endTime)
                .WhereIf(grade != null && grade != 0, c => c.Grade == grade)
                .Select(c => new { c.Name,c.Age,c.Department,c.BeGoodATTerritory,c.CanTeachCourse,c.CreateTime,c.Grade,c.Experience,c.Id,c.AuditState})
                .OrderBy(c => c.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .ToListAsync();
            result.Count = await UnitWork.Find<classroom_apply_teacher_log>(null)
                .Where(c => c.AuditState != 1 && c.AuditState != 3)
                .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
                .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                .WhereIf(endTime != null, c => c.CreateTime <= endTime)
                .WhereIf(grade != null && grade != 0, c => c.Grade == grade)
                .CountAsync();
            return result;
        }
        #endregion
    }
}
