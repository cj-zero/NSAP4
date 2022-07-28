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

        #region web端
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
                .Select(c => new { c.Id,c.Name, c.Age, c.AuditState, c.Department, c.CanTeachCourse, c.BeGoodATTerritory, c.CreateTime })
                .OrderByDescending(c=>c.Id)
                .Skip((pageIndex-1)*pageSize).Take(pageSize)
                .ToListAsync();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="createUser"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="state"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<TableData> CoursePackageList(string name, string createUser, DateTime? startTime, DateTime? endTime, bool? state, int pageIndex, int pageSize)
        {
            var result = new TableData();
            result.Data = await UnitWork.Find<classroom_course_package>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
                .WhereIf(!string.IsNullOrWhiteSpace(createUser),c=>c.CreateUser.Contains(createUser))
                .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                .WhereIf(endTime != null, c => c.CreateTime <= endTime)
                .WhereIf(state != null, c => c.State == state)
                .Select(c => new { c.Id, c.Name, c.CreateUser, c.CreateTime, c.State, c.Remark })
                .OrderByDescending(c => c.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .ToListAsync();
            result.Count = await UnitWork.Find<classroom_course_package>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
                .WhereIf(!string.IsNullOrWhiteSpace(createUser), c => c.CreateUser.Contains(createUser))
                .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                .WhereIf(endTime != null, c => c.CreateTime <= endTime)
                .WhereIf(state != null, c => c.State == state)
                .CountAsync();
            return result;
        }

        /// <summary>
        /// 创建课程包
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> CreateCoursePackage(CoursePpackageReq req)
        {
            var result = new TableData();
            var user = _auth.GetCurrentUser().User;
            var coursePack = await UnitWork.Find<classroom_course_package>(null).Where(c => c.Name.Equals(req.name)).AnyAsync();
            if (coursePack)
            {
                result.Code = 500;
                result.Message = "课程包名称已存在!";
                return result;
            }
            classroom_course_package model = new classroom_course_package();
            model.Name = req.name;
            model.State = true;
            model.Remark = req.remark;
            model.CreateTime = DateTime.Now;
            model.CreateUser = user.Name;
            model.ModifyTime = DateTime.Now;
            await UnitWork.AddAsync<classroom_course_package,int>(model);
            await UnitWork.SaveAsync();
            return result;
        }
        /// <summary>
        /// 编辑课程包
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> EditCoursePackage(CoursePpackageReq req)
        {
            var result = new TableData();
            var coursePack = await UnitWork.Find<classroom_course_package>(null).Where(c => c.Id!=req.id && c.Name.Equals(req.name)).AnyAsync();
            if (coursePack)
            {
                result.Code = 500;
                result.Message = "课程包名称已存在!";
                return result;
            }
            var model= await UnitWork.Find<classroom_course_package>(null).FirstOrDefaultAsync(c=>c.Id==req.id);
            model.Name = req.name;
            model.Remark = req.remark;
            model.ModifyTime = DateTime.Now;
            await UnitWork.UpdateAsync(model);
            await UnitWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// 修改课包状态
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> ChangeCoursePackageState(CoursePpackageReq req)
        {
            var result = new TableData();
            var model = await UnitWork.Find<classroom_course_package>(null).FirstOrDefaultAsync(c => c.Id == req.id);
            if (model.State==true)
            {
                model.State = false;
            }
            else
            {
                model.State = true;
            }
            model.ModifyTime = DateTime.Now;
            await UnitWork.UpdateAsync(model);
            await UnitWork.SaveAsync();
            return result;
        }
        #endregion
    }
}
