﻿using Infrastructure.Extensions;
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
    /// 必修课模块
    /// </summary>
    public class CompulsoryCourseApp : OnlyUnitWorkBaeApp
    {
        private IOptions<AppSetting> _appConfiguration;
        private HttpHelper _helper;
        /// <summary>
        /// 必修课模块
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        /// <param name="appConfiguration"></param>
        public CompulsoryCourseApp(IUnitWork unitWork, IAuth auth, IOptions<AppSetting> appConfiguration) : base(unitWork, auth)
        {
            _appConfiguration = appConfiguration;
            _helper = new HttpHelper(_appConfiguration.Value.AppPushMsgUrl);
        }

        #region erp

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
            result.Data = await (from a in UnitWork.Find<classroom_course_package>(null)
                                 join b in UnitWork.Find<classroom_course_package_map>(null) on a.Id equals b.CoursePackageId
                                 join c in UnitWork.Find<classroom_course>(null) on b.CourseId equals c.Id
                                 where a.Id == coursePackageId
                                 select new { c.Name, c.Source, c.LearningCycle, c.State, b.Sort, b.Id })
                               .OrderBy(c => c.Sort)
                               .ToListAsync();
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
                                 select new { c.Name, a.Schedule, a.CreateTime, c.Id })
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
                                  select new { c.Name, a.Schedule, a.CreateTime, c.Id })
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
        /// <summary>
        /// 课程添加视频
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> AddCourseVideo(AddOrEditCourseVideoReq req)
        {
            var result = new TableData();
            var user = _auth.GetCurrentUser().User;
            var query = await UnitWork.Find<classroom_course_video>(null).FirstOrDefaultAsync(c => c.Name.Equals(req.Name) && c.CourseId == req.CourseId);
            if (query != null)
            {
                result.Code = 500;
                result.Message = "视频名称已存在!";
                return result;
            }
            classroom_course_video model = new classroom_course_video();
            model.Name = req.Name;
            model.CourseId = req.CourseId;
            model.Duration = req.Duration;
            model.ViewedCount = 0;
            model.State = true;
            model.CreateUser = user.Name;
            model.CreateTime = DateTime.Now;
            model.VideoUrl = req.VideoUrl;
            await UnitWork.AddAsync<classroom_course_video, int>(model);
            await UnitWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// 课程视频列表
        /// </summary>
        /// <param name="name"></param>
        /// <param name="createUser"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task<TableData> CourseVideoList(string name, string createUser, DateTime? startTime, DateTime? endTime, bool? state)
        {
            var result = new TableData();
            result.Data = await UnitWork.Find<classroom_course_video>(null)
                  .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
                  .WhereIf(!string.IsNullOrWhiteSpace(createUser), c => c.CreateUser.Contains(createUser))
                  .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                  .WhereIf(endTime != null, c => c.CreateTime <= endTime)
                  .WhereIf(state != null, c => c.State == state)
                  .Select(c => new { c.Id, c.Name, c.CreateTime, c.Duration, c.ViewedCount, c.State, c.VideoUrl })
                  .OrderByDescending(c => c.Id)
                  .ToListAsync();
            return result;
        }
        /// <summary>
        /// 删除课程视频
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> DeleteCourseVideo(DeleteCourseVideoReq req)
        {
            var result = new TableData();
            var list = await UnitWork.Find<classroom_course_video>(null).Where(c => req.ids.Contains(c.Id)).ToListAsync();
            await UnitWork.BatchDeleteAsync(list.ToArray());
            await UnitWork.SaveAsync();
            return result;
        }
        #endregion

        #region 课程视频习题
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TableData> CourseVideoSubjectList(int id)
        {
            var result = new TableData();
            var subjectIds = await UnitWork.Find<classroom_course_video_subject>(null).Where(c => c.CourseVideoId == id).Select(c => c.SubjectId).ToListAsync();
            var str = _helper.Post(new
            {
                ids = subjectIds
            }, (string.IsNullOrEmpty(_appConfiguration.Value.AppVersion) ? string.Empty : _appConfiguration.Value.AppVersion + "/") + "Exam/SubjectListByIds");
            JObject data = (JObject)JsonConvert.DeserializeObject(str);
            if (data["ErrorCode"] != null && data["ErrorCode"].ToString() == "200")
            {
                result.Data = data["Data"];
            }
            return result;
        }
        /// <summary>
        /// 课程视频添加题目
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> CourseVideoAddSubject(CourseVideoAddSubjectReq req)
        {
            var result = new TableData();
            List<classroom_course_video_subject> list = new List<classroom_course_video_subject>();
            var subjectIds = await UnitWork.Find<classroom_course_video_subject>(null).Where(c => c.CourseVideoId == req.courseVideoId).Select(c => c.SubjectId).ToListAsync();
            foreach (var item in req.subjectIds)
            {
                if (!subjectIds.Contains(item))
                {
                    classroom_course_video_subject model = new classroom_course_video_subject();
                    model.CourseVideoId = req.courseVideoId;
                    model.SubjectId = item;
                    model.CreateTime = DateTime.Now;
                    list.Add(model);
                }
            }
            await UnitWork.BatchAddAsync<classroom_course_video_subject, int>(list.ToArray());
            await UnitWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// 删除课程视频题目
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> DeleteCourseVideoSubject(CourseVideoAddSubjectReq req)
        {
            var result = new TableData();
            var list = await UnitWork.Find<classroom_course_video_subject>(null).Where(c => c.CourseVideoId == req.courseVideoId && req.subjectIds.Contains(c.SubjectId)).ToListAsync();
            await UnitWork.BatchDeleteAsync(list.ToArray());
            await UnitWork.SaveAsync();
            return result;
        }
        #endregion

        #region 课程视频答题情况

        #endregion

        #endregion


        #region App
        /// <summary>
        /// 必修课程列表
        /// </summary>
        /// <param name="appUserId"></param>
        /// <param name="state">课程状态 0:全部 1:已逾期 2:已完成 3:未开始 4:进行中</param>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<TableData> CompulsoryCourseList(int appUserId, int? state, int? source)
        {
            var result = new TableData();
            List<object> obj = new List<object>();
            var query = await (from a in UnitWork.Find<classroom_course_package_user>(null)
                               join b in UnitWork.Find<classroom_course_package>(null) on a.CoursePackageId equals b.Id
                               join c in UnitWork.Find<classroom_course_package_map>(null) on b.Id equals c.CoursePackageId
                               join d in UnitWork.Find<classroom_course>(null) on c.CourseId equals d.Id
                               where a.AppUserId == appUserId && b.State == true && d.State == true
                               select new { a.Id, d.Name, d.Source, d.State, d.LearningCycle, a.CreateTime, a.CoursePackageId, c.Sort, c.CourseId })
                               .WhereIf(source != null, c => c.Source == source)
                               .OrderByDescending(c => c.Id)
                               .ToListAsync();
            List<int> coursePackageId = query.Select(c => c.CoursePackageId).ToList();
            foreach (var item in coursePackageId)
            {
                var list = query.Where(c => c.CoursePackageId == item).OrderBy(c => c.Sort).ToList();
                foreach (var row in list)
                {
                    var index = list.FindIndex(c => c.CourseId == row.CourseId) + 1;
                    int day = index * row.LearningCycle;
                    var EndTime = row.CreateTime.AddDays(day);
                    obj.Add(new { row.Name, row.CreateTime, EndTime, Schedule = "", row.CoursePackageId, row.CourseId });
                }
            }
            return result;
        }

        /// <summary>
        /// 课程视频列表
        /// </summary>
        /// <param name="appUserId"></param>
        /// <param name="coursePackageId"></param>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public async Task<TableData> CourseVideoList(int appUserId, int coursePackageId, int courseId)
        {
            var result = new TableData();
            List<object> list = new List<object>();
            var packageInfo = await UnitWork.Find<classroom_course_package_user>(null).Where(c => c.CoursePackageId == coursePackageId && c.AppUserId == appUserId).FirstOrDefaultAsync();
            if (packageInfo != null)
            {
                var courseInfo = await UnitWork.Find<classroom_course>(null).Where(c => c.Id == courseId).FirstOrDefaultAsync();
                if (courseInfo != null)
                {
                    var courseList = await UnitWork.Find<classroom_course_package_map>(null).Where(c => c.CoursePackageId == coursePackageId).OrderBy(c => c.Sort).Select(c => c.CourseId).ToListAsync();
                    var index = courseList.FindIndex(c => c == courseId) + 1;
                    int day = index * courseInfo.LearningCycle;
                    var EndTime = packageInfo.CreateTime.AddDays(day);
                    var query = await UnitWork.Find<classroom_course_video>(null).Where(c => c.CourseId == courseId).OrderBy(c => c.Id).Select(c => new { c.Name, c.Id }).ToListAsync();
                    foreach (var item in query)
                    {
                        list.Add(new { item.Name, item.Id, packageInfo.CreateTime, EndTime, coursePackageId, courseId });
                    }
                }
            }
            result.Data = list;
            return result;
        }

        /// <summary>
        /// 课程视频详情
        /// </summary>
        /// <param name="id"></param>
        /// <param name="appUserId"></param>
        /// <param name="coursePackageId"></param>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public async Task<TableData> CourseVideoDetails(int id, int appUserId, int coursePackageId, int courseId)
        {
            var result = new TableData();
            var videoInfo = await UnitWork.Find<classroom_course_video>(null).Where(c => c.CourseId == courseId && c.Id == id).FirstOrDefaultAsync();
            string videoUrl = videoInfo?.VideoUrl;
            bool isPlayFinish = false;
            decimal totalScore = 0;
            decimal maxScore = 0;
            var subjectIds = await UnitWork.Find<classroom_course_video_subject>(null).Where(c => c.CourseVideoId == id).Select(c => c.SubjectId).ToListAsync();
            var str = _helper.Post(new
            {
                ids = subjectIds
            }, (string.IsNullOrEmpty(_appConfiguration.Value.AppVersion) ? string.Empty : _appConfiguration.Value.AppVersion + "/") + "Exam/SubjectListByIds");
            JObject data = (JObject)JsonConvert.DeserializeObject(str);
            if (data["ErrorCode"] != null && data["ErrorCode"].ToString() == "200")
            {
                result.Data = data["Data"];
            }
            var playLog = await UnitWork.Find<classroom_video_play_log>(null).Where(c => c.CoursePackageId == coursePackageId && c.CourseId == courseId && c.CourseVideoId == id && c.AppUserId == appUserId).OrderByDescending(c => c.PlayDuration).FirstOrDefaultAsync();
            if (playLog != null)
            {
                isPlayFinish = (playLog.PlayDuration / (double)playLog.TotalDuration) > 0.8;
            }
            var isPass = totalScore == maxScore;
            result.Data = new { id, coursePackageId, courseId, videoUrl, isPlayFinish, totalScore, maxScore, isPass, videoInfo.Duration };
            return result;
        }

        /// <summary>
        /// 保存观看记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> SavePlayLog(CourseVideoPlayLogReq req)
        {
            var result = new TableData();
            var videoInfo = await UnitWork.Find<classroom_course_video>(null).Where(c => c.Id == req.CourseVideoId).FirstOrDefaultAsync();
            videoInfo.ViewedCount++;
            classroom_video_play_log log = new classroom_video_play_log();
            log.CoursePackageId = req.CoursePackageId;
            log.CourseId = req.CourseId;
            log.CourseVideoId = req.CourseVideoId;
            log.AppUserId = req.AppUserId;
            log.PlayDuration = req.PlayDuration;
            log.TotalDuration = req.TotalDuration;
            log.CreateTime = DateTime.Now;
            await UnitWork.AddAsync<classroom_video_play_log, int>(log);
            await UnitWork.UpdateAsync(videoInfo);
            await UnitWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// 创建答卷
        /// </summary>
        /// <param name="id"></param>
        /// <param name="appUserId"></param>
        /// <param name="coursePackageId"></param>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public async Task<TableData> CreateExamPaper(int id, int appUserId, int coursePackageId, int courseId)
        {
            var result = new TableData();
            List<object> list = new List<object>();
            var subjectIds = await UnitWork.Find<classroom_course_video_subject>(null).Where(c => c.CourseVideoId == id).Select(c => c.SubjectId).ToListAsync();
            var str = _helper.Post(new
            {
                ids = subjectIds
            }, (string.IsNullOrEmpty(_appConfiguration.Value.AppVersion) ? string.Empty : _appConfiguration.Value.AppVersion + "/") + "Exam/SubjectListByIds");
            JObject data = (JObject)JsonConvert.DeserializeObject(str);
            if (data["ErrorCode"] != null && data["ErrorCode"].ToString() == "200")
            {
                result.Data = data["Data"];
            }
            List<SubjectResp> subjectList = JsonConvert.DeserializeObject<List<SubjectResp>>(data["Data"].ToString());
            foreach (var subjectItem in data["Data"])
            {
                list.Add(new
                {
                    type = subjectItem["type"],
                    content = subjectItem["content"],
                    Items = JsonConvert.DeserializeObject(subjectItem["answer_options"].ToString()),
                    subject_id = subjectItem["Id"],
                    video_url = subjectItem["video_url"],
                    image_url = subjectItem["image_url"].ToString(),
                    score = subjectItem["score"].ToString()
                });
            }
            classroom_course_exam model = new classroom_course_exam();
            model.AppUserId = appUserId;
            model.CoursePackageId = coursePackageId;
            model.CourseId = courseId;
            model.CourseVideoId = id;
            model.TotalScore = subjectList.Select(c => c.score).Sum();
            model.IsPass = false;
            model.IsSubmit = false;
            model.TestScores = 0;
            model.SubmitTime = DateTime.Now;
            var exam = await UnitWork.AddAsync<classroom_course_exam, int>(model);
            await UnitWork.SaveAsync();
            result.Data = list;
            return result;
        }
        #endregion

    }
}
