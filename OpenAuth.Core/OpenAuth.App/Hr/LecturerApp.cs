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
            result.Data = await UnitWork.Find<classroom_teacher_apply_log>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
                .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                .WhereIf(endTime != null, c => c.CreateTime <= endTime)
                .WhereIf(auditState != null && auditState > 0, c => c.AuditState == auditState)
                .Select(c => new { c.Id, c.Name, c.Age, c.AuditState, c.Department, c.CanTeachCourse, c.BeGoodATTerritory, c.CreateTime })
                .OrderByDescending(c => c.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .ToListAsync();
            result.Count = await UnitWork.Find<classroom_teacher_apply_log>(null)
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
            var query = await UnitWork.Find<classroom_teacher_apply_log>(null).FirstOrDefaultAsync(c => c.Id == req.id);
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
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<TableData> TeacherList(string name, DateTime? startTime, DateTime? endTime, int? grade, int pageIndex, int pageSize)
        {
            var result = new TableData();
            result.Data = await UnitWork.Find<classroom_teacher_apply_log>(null)
                .Where(c=>c.AuditState!=1 && c.AuditState!=3)
                .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
                .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                .WhereIf(endTime != null, c => c.CreateTime <= endTime)
                .WhereIf(grade != null && grade != 0, c => c.Grade == grade)
                .Select(c => new { c.Name,c.Age,c.Department,c.BeGoodATTerritory,c.CanTeachCourse,c.CreateTime,c.Grade,c.Experience,c.Id,c.AuditState})
                .OrderBy(c => c.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .ToListAsync();
            result.Count = await UnitWork.Find<classroom_teacher_apply_log>(null)
                .Where(c => c.AuditState != 1 && c.AuditState != 3)
                .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
                .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                .WhereIf(endTime != null, c => c.CreateTime <= endTime)
                .WhereIf(grade != null && grade != 0, c => c.Grade == grade)
                .CountAsync();
            return result;
        }

        /// <summary>
        /// 讲师等级修改
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> ChangeTeacherGrade(EditTeacherReq req)
        {
            var result = new TableData();
            var user = _auth.GetCurrentUser().User;
            var model = await UnitWork.Find<classroom_teacher_apply_log>(null).FirstOrDefaultAsync(c => c.Id == req.id);
            if (model!=null)
            {
                model.Grade = req.Grade;
                model.Experience = req.Experience;
                model.ModifyTime = DateTime.Now;
                model.OperationUser = user.Name;
                await UnitWork.UpdateAsync(model);
                await UnitWork.SaveAsync();
            }
            return result;
        }

        /// <summary>
        /// 讲师开课列表
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="title"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="auditState"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<TableData> TeacherCourseList(string userName, string title, DateTime? startTime, DateTime? endTime, int? auditState, int pageIndex, int pageSize)
        {
            var result = new TableData();
            List<int> appUserIds = new List<int>();
            result.Data=await (from a in UnitWork.Find<classroom_teacher_course>(null)
                        join b in UnitWork.Find<classroom_teacher_apply_log>(null) on a.AppUserId equals b.AppUserId
                        select new {b.Name,a.Id,a.ForTheCrowd,a.Title,a.TeachingMethod,a.TeachingAddres,a.StartTime,a.EndTime,a.VideoUrl,a.AuditState,a.CreateTime})
                .WhereIf(!string.IsNullOrWhiteSpace(userName), c => c.Name.Contains(userName))
                .WhereIf(!string.IsNullOrWhiteSpace(title), c => c.Title.Contains(title))
                .WhereIf(startTime != null, c => c.StartTime >= startTime)
                .WhereIf(endTime != null, c => c.EndTime <= endTime)
                .WhereIf(auditState != null && auditState != 0, c => c.AuditState == auditState)
                .OrderByDescending(c => c.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .ToListAsync();

            result.Count = await (from a in UnitWork.Find<classroom_teacher_course>(null)
                                  join b in UnitWork.Find<classroom_teacher_apply_log>(null) on a.AppUserId equals b.AppUserId
                                  select new { b.Name, a.Id, a.ForTheCrowd, a.Title, a.TeachingMethod, a.TeachingAddres, a.StartTime, a.EndTime, a.VideoUrl, a.AuditState, a.CreateTime })
                .WhereIf(!string.IsNullOrWhiteSpace(userName), c => c.Name.Contains(userName))
                .WhereIf(!string.IsNullOrWhiteSpace(title), c => c.Title.Contains(title))
                .WhereIf(startTime != null, c => c.StartTime >= startTime)
                .WhereIf(endTime != null, c => c.EndTime <= endTime)
                .WhereIf(auditState != null && auditState != 0, c => c.AuditState == auditState)
                .CountAsync();
            return result;
        }

        /// <summary>
        /// 讲师开课审核
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> TeacherCourseAudit(TeacherCourseAuditReq req)
        {
            var result = new TableData();
            var user = _auth.GetCurrentUser().User;
            var query = await UnitWork.Find<classroom_teacher_course>(null).FirstOrDefaultAsync(c => c.Id == req.id);
            if (query != null)
            {
                query.AuditState = req.auditState;
            }
            await UnitWork.UpdateAsync(query);
            await UnitWork.SaveAsync();
            return result;
        }
        /// <summary>
        /// 修改讲师课程
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> EditTeacherCourse(classroom_teacher_course req)
        {
            var result = new TableData();
            var user = _auth.GetCurrentUser().User;
            var query = await UnitWork.Find<classroom_teacher_course>(null).FirstOrDefaultAsync(c => c.Id == req.Id);
            if (query != null)
            {
                query.VideoUrl = req.VideoUrl;
            }
            await UnitWork.UpdateAsync(query);
            await UnitWork.SaveAsync();
            return result;
        }
        #endregion


        #region App
        /// <summary>
        /// 讲师提交申请
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> TeacherApply(TeacherApplyReq req)
        {
            var result = new TableData();
            var query = await UnitWork.Find<classroom_teacher_apply_log>(null).FirstOrDefaultAsync(c => c.AppUserId == req.AppUserId && c.AuditState!=3);
            if (query != null)
            {
                if (query.AuditState==4)
                {
                    result.Code = 500;
                    result.Message = "您已被管理员封禁!";
                    return result;
                }
                else if (query.AuditState == 1)
                {
                    result.Code = 500;
                    result.Message = "您已提交过申请,管理员正在审核!";
                    return result;
                }
            }
            else
            {
                classroom_teacher_apply_log model = new classroom_teacher_apply_log();
                model.Name = req.Name;
                model.Age= req.Age;
                model.Mobile= req.Mobile;
                model.HeaderImg= req.HeaderImg;
                model.Department= req.Department;
                model.CanTeachCourse= req.CanTeachCourse;
                model.BeGoodATTerritory= req.BeGoodATTerritory;
                model.CreateTime = DateTime.Now;
                model.ModifyTime = DateTime.Now;
                model.AppUserId = req.AppUserId;
                await UnitWork.UpdateAsync(query);
                await UnitWork.SaveAsync();
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> TeacherCourseApply()
        {
            var result = new TableData();
            return result;
        }
        #endregion
    }
}
