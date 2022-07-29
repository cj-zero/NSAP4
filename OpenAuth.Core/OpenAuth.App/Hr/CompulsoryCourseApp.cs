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

        #region 课程包

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
                .WhereIf(!string.IsNullOrWhiteSpace(createUser), c => c.CreateUser.Contains(createUser))
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
        public async Task<TableData> CreateCoursePackage(CoursePackageReq req)
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
            await UnitWork.AddAsync<classroom_course_package, int>(model);
            await UnitWork.SaveAsync();
            return result;
        }
        /// <summary>
        /// 编辑课程包
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> EditCoursePackage(CoursePackageReq req)
        {
            var result = new TableData();
            var coursePack = await UnitWork.Find<classroom_course_package>(null).Where(c => c.Id != req.id && c.Name.Equals(req.name)).AnyAsync();
            if (coursePack)
            {
                result.Code = 500;
                result.Message = "课程包名称已存在!";
                return result;
            }
            var model = await UnitWork.Find<classroom_course_package>(null).FirstOrDefaultAsync(c => c.Id == req.id);
            if (model != null)
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
        public async Task<TableData> ChangeCoursePackageState(CoursePackageReq req)
        {
            var result = new TableData();
            var model = await UnitWork.Find<classroom_course_package>(null).FirstOrDefaultAsync(c => c.Id == req.id);
            if (model != null)
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
        public async Task<TableData> DeleteCoursePackage(CoursePackageReq req)
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

        #region 课程包课程

        /// <summary>
        /// 课程包课程列表
        /// </summary>
        /// <param name="coursePackageId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<TableData> CoursePackageCourseList(int coursePackageId)
        {
            var result = new TableData();
            var query = await (from a in UnitWork.Find<classroom_course_package>(null)
                               join b in UnitWork.Find<classroom_course_package_map>(null) on a.Id equals b.CoursePackageId
                               join c in UnitWork.Find<classroom_course>(null) on b.CourseId equals c.Id
                               where a.Id == coursePackageId
                               select new { c.Name, c.Source, c.LearningCycle, c.State, b.Sort, b.Id })
                               .OrderBy(c => c.Sort)
                               .ToListAsync();
            result.Count = query.Count;
            result.Data = query;
            return result;
        }

        /// <summary>
        /// 课程包添加课程
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> AddCourseIntoCoursePackage(CourseForCoursePackageReq req)
        {
            var result = new TableData();
            List<classroom_course_package_map> list = new List<classroom_course_package_map>();
            var courseIds = await UnitWork.Find<classroom_course_package_map>(null).Where(c => req.CourseIds.Contains(c.CourseId) && c.CoursePackageId == req.CoursePackageId).Select(c => c.CourseId).ToListAsync();
            int sort = await UnitWork.Find<classroom_course_package_map>(null).Where(c => c.CoursePackageId == req.CoursePackageId).Select(c => c.Sort).MaxAsync();
            foreach (var item in req.CourseIds)
            {
                if (courseIds.Contains(item))
                {
                    continue;
                }
                sort++;
                classroom_course_package_map model = new classroom_course_package_map();
                model.CoursePackageId = req.CoursePackageId;
                model.CourseId = item;
                model.CreateTime = DateTime.Now;
                model.Sort = sort;
                list.Add(model);
            }
            await UnitWork.BatchAddAsync<classroom_course_package_map, int>(list.ToArray());
            await UnitWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// 删除课程包课程
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> DeleteCourseIntoCoursePackage(CourseForCoursePackageReq req)
        {
            var result = new TableData();
            var courseList = await UnitWork.Find<classroom_course_package_map>(null).Where(c => req.CourseIds.Contains(c.CourseId) && c.CoursePackageId == req.CoursePackageId).ToListAsync();
            await UnitWork.BatchDeleteAsync(courseList.ToArray());
            await UnitWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// 修改课程包课程排序
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> ChangeCourseSort(List<CourseSortResp> req)
        {
            var result = new TableData();
            var ids = req.Select(c => c.id).Distinct().ToList();
            var courseList = await UnitWork.Find<classroom_course_package_map>(null).Where(c => ids.Contains(c.CourseId)).ToListAsync();
            foreach (var item in courseList)
            {
                item.Sort = req.Where(c => c.id == item.Id).Select(c => c.sort).FirstOrDefault();
            }
            await UnitWork.BatchUpdateAsync(courseList.ToArray());
            await UnitWork.SaveAsync();
            return result;
        }

        #endregion

        #region 课程包人员
        /// <summary>
        /// 课程包添加人员
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> AddCoursePackageUser(CoursePackageUserReq req)
        {
            var result = new TableData();
            List<classroom_course_package_user> list = new List<classroom_course_package_user>();
            var userIds = await UnitWork.Find<classroom_course_package_user>(null).Where(c => req.Ids.Contains(c.AppUserId) && c.CoursePackageId == req.CoursePackageId).Select(c => c.AppUserId).ToListAsync();
            foreach (var item in req.Ids)
            {
                if (userIds.Contains(item))
                {
                    continue;
                }
                classroom_course_package_user model = new classroom_course_package_user();
                model.AppUserId = item;
                model.CoursePackageId = req.CoursePackageId;
                model.CreateTime = DateTime.Now;
                model.Schedule = 0;
                list.Add(model);
            }
            await UnitWork.BatchAddAsync<classroom_course_package_user, int>(list.ToArray());
            await UnitWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// 课程包人员列表
        /// </summary>
        /// <param name="coursePackageId"></param>
        /// <param name="name"></param>
        /// <param name="schedule"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<TableData> CoursePackageUserList(int coursePackageId, string name, decimal? schedule, DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize)
        {
            var result = new TableData();
            result.Data = await (from a in UnitWork.Find<classroom_course_package_user>(null)
                                 join b in UnitWork.Find<AppUserMap>(null) on a.AppUserId equals b.AppUserId
                                 join c in UnitWork.Find<User>(null) on b.UserID equals c.Id
                                 where c.Status == 0 && a.CoursePackageId == coursePackageId
                                 select new { c.Name, a.Schedule, a.CreateTime, a.EndTime, c.Id })
                .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
                .WhereIf(schedule != null && schedule >= 0, c => c.Schedule == schedule)
                .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                .WhereIf(endTime != null, c => c.CreateTime <= endTime)
                .OrderByDescending(c => c.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .ToListAsync();
            result.Count = await (from a in UnitWork.Find<classroom_course_package_user>(null)
                                  join b in UnitWork.Find<AppUserMap>(null) on a.AppUserId equals b.AppUserId
                                  join c in UnitWork.Find<User>(null) on b.UserID equals c.Id
                                  where c.Status == 0 && a.CoursePackageId == coursePackageId
                                  select new { c.Name, a.Schedule, a.CreateTime, a.EndTime, c.Id })
                .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
                .WhereIf(schedule != null && schedule >= 0, c => c.Schedule == schedule)
                .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                .WhereIf(endTime != null, c => c.CreateTime <= endTime).CountAsync();
            return result;
        }


        /// <summary>
        /// 删除课程包人员
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> DeleteCoursePackageUser(CoursePackageUserReq req)
        {
            var result = new TableData();
            var userList = await UnitWork.Find<classroom_course_package_user>(null).Where(c => req.Ids.Contains(c.AppUserId) && c.CoursePackageId == req.CoursePackageId).ToListAsync();
            await UnitWork.BatchDeleteAsync(userList.ToArray());
            await UnitWork.SaveAsync();
            return result;
        }
        #endregion

        #endregion

        #region 课程相关

        #region 课程

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
        public async Task<TableData> CourseList(string name, string createUser, int? learningCycle, DateTime? startTime, DateTime? endTime, bool? state, int? source, int pageIndex, int pageSize)
        {
            var result = new TableData();
            result.Data = await UnitWork.Find<classroom_course>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
                .WhereIf(!string.IsNullOrWhiteSpace(createUser), c => c.CreateUser.Contains(createUser))
                .WhereIf(learningCycle != null && learningCycle > 0, c => c.LearningCycle == learningCycle)
                .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                .WhereIf(endTime != null, c => c.CreateTime <= endTime)
                .WhereIf(state != null, c => c.State == state)
                .WhereIf(source != null && source != 0, c => c.Source == source)
                .Select(c => new { c.Id, c.Name, c.Source, c.LearningCycle, c.CreateUser, c.CreateTime, c.State })
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
            if (model != null)
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
            if (model != null)
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

        #region 课程视频
        #endregion

        #region 课程视频习题

        #endregion

        #endregion

        #endregion

    }
}
