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

        #region 课程包相关
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

        /// <summary>
        /// 课程包列表
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
            if (model!=null)
            {
                model.Name = req.name;
                model.Remark = req.remark;
                model.ModifyTime = DateTime.Now;
                await UnitWork.UpdateAsync(model);
                await UnitWork.SaveAsync();
            }
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
            if (model!=null)
            {
                if (model.State == true)
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
            }
            return result;
        }

        /// <summary>
        /// 删除课程包
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> DeleteCoursePackage(CoursePpackageReq req)
        {
            var result = new TableData();
            var isExist = await UnitWork.Find<classroom_course_package_user>(null).AnyAsync(c => c.CoursePackageId == req.id);
            if (isExist)
            {
                result.Code = 500;
                result.Message = "课程包已分配人员无法删除!";
                return result;
            }
            var model = await UnitWork.Find<classroom_course_package>(null).FirstOrDefaultAsync(c => c.Id == req.id);
            await UnitWork.DeleteAsync(model);
            await UnitWork.SaveAsync();
            return result;
        }



        #endregion

        #region 课程相关

        /// <summary>
        /// 课程列表
        /// </summary>
        /// <param name="name"></param>
        /// <param name="createUser"></param>
        /// <param name="learningCycle"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="state"></param>
        /// <param name="source"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<TableData> CourseList(string name, string createUser, int? learningCycle, DateTime? startTime, DateTime? endTime, bool? state, int? source,int pageIndex, int pageSize)
        {
            var result = new TableData();
            result.Data = await UnitWork.Find<classroom_course>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
                .WhereIf(!string.IsNullOrWhiteSpace(createUser), c => c.CreateUser.Contains(createUser))
                .WhereIf(learningCycle != null && learningCycle>0, c => c.LearningCycle ==learningCycle)
                .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                .WhereIf(endTime != null, c => c.CreateTime <= endTime)
                .WhereIf(state != null, c => c.State == state)
                .WhereIf(source!=null && source!=0,c=>c.Source==source)
                .Select(c => new { c.Id, c.Name,c.Source,c.LearningCycle, c.CreateUser, c.CreateTime, c.State})
                .OrderByDescending(c => c.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .ToListAsync();
            result.Count = await UnitWork.Find<classroom_course>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
                .WhereIf(!string.IsNullOrWhiteSpace(createUser), c => c.CreateUser.Contains(createUser))
                .WhereIf(learningCycle != null && learningCycle > 0, c => c.LearningCycle == learningCycle)
                .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                .WhereIf(endTime != null, c => c.CreateTime <= endTime)
                .WhereIf(state != null, c => c.State == state)
                .WhereIf(source != null && source != 0, c => c.Source == source)
                .CountAsync();
            return result;
        }

        /// <summary>
        /// 新增课程
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> CreateCourse(AddOrEditCourseReq req)
        {
            var result = new TableData();
            var user = _auth.GetCurrentUser().User;
            var coursePack = await UnitWork.Find<classroom_course>(null).Where(c => c.Name.Equals(req.name)).AnyAsync();
            if (coursePack)
            {
                result.Code = 500;
                result.Message = "课程名称已存在!";
                return result;
            }
            classroom_course model = new classroom_course();
            model.Name = req.name;
            model.State = true;
            model.Source = req.source;
            model.LearningCycle = req.learningCycle;
            model.CreateTime = DateTime.Now;
            model.CreateUser = user.Name;
            await UnitWork.AddAsync<classroom_course, int>(model);
            await UnitWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// 编辑课程
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> EditCourse(AddOrEditCourseReq req)
        {
            var result = new TableData();
            var coursePack = await UnitWork.Find<classroom_course>(null).Where(c => c.Id != req.id && c.Name.Equals(req.name)).AnyAsync();
            if (coursePack)
            {
                result.Code = 500;
                result.Message = "课程名称已存在!";
                return result;
            }
            var model = await UnitWork.Find<classroom_course>(null).FirstOrDefaultAsync(c => c.Id == req.id);
            if (model!=null)
            {
                model.Name = req.name;
                model.Source = req.source;
                model.LearningCycle = req.learningCycle;
                await UnitWork.UpdateAsync(model);
                await UnitWork.SaveAsync();
            }
            return result;
        }

        /// <summary>
        /// 修改课程状态
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> ChangeCourseState(AddOrEditCourseReq req)
        {
            var result = new TableData();
            var model = await UnitWork.Find<classroom_course>(null).FirstOrDefaultAsync(c => c.Id == req.id);
            if (model!=null)
            {
                if (model.State == true)
                {
                    model.State = false;
                }
                else
                {
                    model.State = true;
                }
                await UnitWork.UpdateAsync(model);
                await UnitWork.SaveAsync();
            }
            return result;
        }


        #endregion
        #endregion

    }
}
