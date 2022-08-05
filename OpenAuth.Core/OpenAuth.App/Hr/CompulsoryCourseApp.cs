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
            List<object> list = new List<object>();
            var query = await (from a in UnitWork.Find<classroom_course_package_user>(null)
                               join b in UnitWork.Find<AppUserMap>(null) on a.AppUserId equals b.AppUserId
                               join c in UnitWork.Find<User>(null) on b.UserID equals c.Id
                               where c.Status == 0 && a.CoursePackageId == coursePackageId
                               select new { c.Name, a.CreateTime, c.Id, b.AppUserId })
                               .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
                               .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                               .WhereIf(endTime != null, c => c.CreateTime < endTime)
                               .OrderByDescending(c => c.Id)
                               .ToListAsync();
            var courseList = await (from a in UnitWork.Find<classroom_course_package_map>(null)
                                    join b in UnitWork.Find<classroom_course>(null) on a.CourseId equals b.Id
                                    where a.CoursePackageId == coursePackageId && b.State == true
                                    select new { b.Id, b.LearningCycle })
                                    .ToListAsync();
            var courseIds = courseList.Select(c => c.Id).ToList();
            var videoList = await UnitWork.Find<classroom_course_video>(null).Where(c => courseIds.Contains(c.CourseId)).Select(c => new { c.Id, c.CourseId }).ToListAsync();
            var examList = await UnitWork.Find<classroom_course_exam>(null)
                .Where(c => c.CoursePackageId == coursePackageId)
                .Select(c => new { c.CourseVideoId, c.IsPass, c.CourseId, c.CoursePackageId, c.AppUserId }).ToListAsync();
            var videoPlayList = await UnitWork.Find<classroom_video_play_log>(null)
                .Where(c => c.CoursePackageId == coursePackageId)
                .Select(c => new { c.PlayDuration, c.CourseVideoId, c.CoursePackageId, c.CourseId, c.AppUserId, c.TotalDuration })
                .ToListAsync();
            foreach (var item in query)
            {
                int i = 0;
                int totalDay = courseList.Sum(c => c.LearningCycle);
                DateTime endTimes = item.CreateTime.AddDays(totalDay);
                foreach (var ctem in courseList)
                {
                    var courseVideoList = videoList.Where(c => c.CourseId == ctem.Id).ToList();
                    int j = 0;
                    foreach (var vitem in videoPlayList)
                    {
                        var isPass = examList.Where(c => c.CourseId == ctem.Id && c.CourseVideoId == vitem.CourseVideoId && c.AppUserId == item.AppUserId && c.IsPass == true).Any();
                        var playResult = videoPlayList.Where(c => c.CourseId == ctem.Id && c.CourseVideoId == vitem.CourseVideoId && c.AppUserId == item.AppUserId).OrderByDescending(c => c.PlayDuration).FirstOrDefault();
                        var isFinish = playResult == null ? false : (playResult.PlayDuration / (double)playResult.TotalDuration > 0.8);
                        if (isFinish && isPass)
                        {
                            j++;
                        }
                    }
                    if (j == courseVideoList.Count)
                    {
                        i++;
                    }
                }
                var schedules = i / courseList.Count;
                if (schedule != null && schedule == schedules)
                {
                    list.Add(new { item.Name, item.CreateTime, endTimes, item.Id, item.AppUserId });
                }
                else
                {
                    list.Add(new { item.Name, item.CreateTime, endTimes, item.Id, item.AppUserId });
                }
            }
            result.Data = list.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            result.Count = list.Count;
            return result;
        }


        /// <summary>
        /// 删除课程包人员
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> DeleteCoursePackageUser(CoursePackageUserReq req)
        {
            var result = new TableData(); ;
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
        /// 删除课程
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> DeleteCourse(AddOrEditCourseReq req)
        {
            var result = new TableData();
            var isExist = await UnitWork.Find<classroom_course_package_map>(null).AnyAsync(c => c.CourseId == req.id);
            if (isExist)
            {
                result.Code = 500;
                result.Message = "课程正在使用无法删除!";
                return result;
            }
            var model = await UnitWork.Find<classroom_course>(null).FirstOrDefaultAsync(c => c.Id == req.id);
            await UnitWork.DeleteAsync(model);
            await UnitWork.SaveAsync();
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
        /// <param name="courseId"></param>
        /// <param name="name"></param>
        /// <param name="createUser"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task<TableData> CourseVideoList(int courseId, string name, string createUser, DateTime? startTime, DateTime? endTime, bool? state)
        {
            var result = new TableData();
            result.Data = await UnitWork.Find<classroom_course_video>(null)
                  .Where(c => c.CourseId == courseId)
                  .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
                  .WhereIf(!string.IsNullOrWhiteSpace(createUser), c => c.CreateUser.Contains(createUser))
                  .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                  .WhereIf(endTime != null, c => c.CreateTime <= endTime)
                  .WhereIf(state != null, c => c.State == state)
                  .Select(c => new { c.Id, c.Name, c.CreateTime, c.Duration, c.ViewedCount, c.State, c.VideoUrl,c.CreateUser })
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
        /// 课程视频题目列表
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
            if (data["ErrorCode"] != null || data["ErrorCode"].ToString() != "200")
            {
                result.Code = 500;
                result.Message = "视频题目获取失败!";
                return result;
            }
            result.Data = data["Data"];
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

        #region 课程答题情况
        /// <summary>
        /// 用户课程视频列表
        /// </summary>
        /// <param name="appUserId"></param>
        /// <param name="coursePackageId"></param>
        /// <returns></returns>
        public async Task<TableData> UserCourseVideoList(int appUserId, int coursePackageId)
        {
            var result = new TableData();
            List<object> list = new List<object>();
            var courseList = await (from a in UnitWork.Find<classroom_course_package_map>(null)
                                    join b in UnitWork.Find<classroom_course>(null) on a.CourseId equals b.Id
                                    where a.CoursePackageId == coursePackageId && b.State == true
                                    select new { b.Id, b.Name }).ToListAsync();
            var courseIds = courseList.Select(c => c.Id).ToList();
            var videoList = await UnitWork.Find<classroom_course_video>(null).Where(c => courseIds.Contains(c.CourseId)).ToListAsync();
            var examList = await UnitWork.Find<classroom_course_exam>(null)
                .Where(c => c.CoursePackageId == coursePackageId && c.AppUserId == appUserId)
                .Select(c => new { c.Id, c.CourseVideoId, c.IsPass, c.CourseId, c.CoursePackageId, c.SubmitTime, c.IsSubmit }).ToListAsync();
            var videoPlayList = await UnitWork.Find<classroom_video_play_log>(null)
                .Where(c => c.CoursePackageId == coursePackageId && c.AppUserId == appUserId)
                .Select(c => new { c.PlayDuration, c.CourseVideoId, c.CoursePackageId, c.CourseId })
                .ToListAsync();
            foreach (var item in videoList)
            {
                string videoName = item.Name;
                string courserName = courseList.Where(c => c.Id == item.CourseId).FirstOrDefault()?.Name;
                string submitTime = string.Empty;
                var playDurationInfo = videoPlayList.Where(c => c.CourseVideoId == item.Id && c.CourseId == item.Id).OrderByDescending(c => c.PlayDuration).Select(c => new { c.PlayDuration }).FirstOrDefault();
                var isPlayFinish = playDurationInfo == null ? false : (playDurationInfo.PlayDuration / (double)item.Duration > 0.8);
                var isPass = examList.Where(c => c.CourseId == item.CourseId && c.CourseVideoId == item.Id && c.IsPass == true).Any();
                var lastExam = examList.Where(c => c.CourseId == item.CourseId && c.CourseVideoId == item.Id && c.IsSubmit == true).OrderByDescending(c => c.Id).FirstOrDefault();
                list.Add(new { item.Id, isPass, isPlayFinish, courserName, videoName, examId = lastExam == null ? 0 : lastExam.Id, submitTime = lastExam == null ? "" : lastExam.SubmitTime.ToString("yyyy-MM-dd HH:mm:ss") });
            }
            result.Data = list;
            return result;
        }
        /// <summary>
        /// 考试试卷结果详情
        /// </summary>
        /// <param name="examId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<TableData> UserVideoExamResult(int examId, int pageIndex, int pageSize)
        {
            var result = new TableData();
            result.Data = await UnitWork.Find<classroom_course_exam_subject>(null)
                .Where(c => c.ExaminationId == examId)
                .OrderBy(c => c.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .ToListAsync();
            result.Count = await UnitWork.Find<classroom_course_exam_subject>(null).Where(c => c.ExaminationId == examId).CountAsync();
            return result;
        }

        #endregion

        #endregion


        #region App
        /// <summary>
        /// 必修课程列表
        /// </summary>
        /// <param name="appUserId"></param>
        /// <param name="state">课程状态 0:全部 1:已逾期 2:已完成 3:未开始 4:进行中</param>
        /// <param name="source">课程来源(1:主管推课  2:职前  3:入职  4:晋升  5:转正 6:变动)</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<TableData> CompulsoryCourseList(int appUserId, int? state, int? source, int pageIndex, int pageSize)
        {
            var result = new TableData();
            List<object> obj = new List<object>();
            DateTime dt = DateTime.Now;
            var query = await (from a in UnitWork.Find<classroom_course_package_user>(null)
                               join c in UnitWork.Find<classroom_course_package_map>(null) on a.CoursePackageId equals c.CoursePackageId
                               join d in UnitWork.Find<classroom_course>(null) on c.CourseId equals d.Id
                               where a.AppUserId == appUserId && d.State == true
                               select new { a.Id, d.Name, d.Source, d.State, d.LearningCycle, a.CreateTime, a.CoursePackageId, c.Sort, c.CourseId })
                               .WhereIf(source != null && source > 0, c => c.Source == source)
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
                    switch (state)
                    {
                        case 0:
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
                                        courseState = 3;
                                    }
                                    else
                                    {
                                        courseState = 4;
                                    }
                                }
                            }
                            obj.Add(new { row.Name, row.CreateTime, EndTime, Schedule, row.CoursePackageId, row.CourseId, courseState });
                            break;
                        case 1:
                            if (Schedule != 1 && dt >= EndTime)
                            {
                                obj.Add(new { row.Name, row.CreateTime, EndTime, Schedule, row.CoursePackageId, row.CourseId, courseState = 1 });
                            }
                            break;
                        case 2:
                            if (Schedule == 1)
                            {
                                obj.Add(new { row.Name, row.CreateTime, EndTime, Schedule, row.CoursePackageId, row.CourseId, courseState = 2 });
                            }
                            break;
                        case 3:
                            if (Schedule == 0 && playCount <= 0 && examCount <= 0)
                            {
                                obj.Add(new { row.Name, row.CreateTime, EndTime, Schedule, row.CoursePackageId, row.CourseId, courseState = 3 });
                            }
                            break;
                        case 4:
                            if (playCount >= 0 || examCount >= 0)
                            {
                                obj.Add(new { row.Name, row.CreateTime, EndTime, Schedule, row.CoursePackageId, row.CourseId, courseState = 4 });
                            }
                            break;

                    }
                }
            }
            result.Data = obj.Skip((pageIndex - 1) * pageSize).Take(pageSize);
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
                var courseInfo = await UnitWork.Find<classroom_course>(null).Where(c => c.Id == courseId && c.State == true).FirstOrDefaultAsync();
                if (courseInfo != null)
                {
                    var videoPlayList = await UnitWork.Find<classroom_video_play_log>(null)
                        .Where(c => c.CoursePackageId == coursePackageId && c.CourseId == courseId && c.AppUserId == appUserId)
                        .Select(c => new { c.PlayDuration, c.CourseVideoId, c.CoursePackageId, c.CourseId })
                        .ToListAsync();
                    var examList = await UnitWork.Find<classroom_course_exam>(null)
                        .Where(c => c.CoursePackageId == coursePackageId && c.CourseId == courseId && c.AppUserId == appUserId)
                       .Select(c => new { c.CourseVideoId, c.IsPass, c.CourseId, c.CoursePackageId }).ToListAsync();
                    var courseList = await UnitWork.Find<classroom_course_package_map>(null)
                        .Where(c => c.CoursePackageId == coursePackageId)
                        .OrderBy(c => c.Sort)
                        .Select(c => c.CourseId)
                        .ToListAsync();
                    var index = courseList.FindIndex(c => c == courseId) + 1;
                    var ids = courseList.Take(index);
                    int day = await UnitWork.Find<classroom_course>(null).Where(c => ids.Contains(c.Id)).SumAsync(c => c.LearningCycle);
                    var EndTime = packageInfo.CreateTime.AddDays(day);
                    var query = await UnitWork.Find<classroom_course_video>(null).Where(c => c.CourseId == courseId).OrderBy(c => c.Id).Select(c => new { c.Name, c.Id, c.Duration }).ToListAsync();
                    foreach (var item in query)
                    {
                        var courseVideoState = 1;
                        var playLog = videoPlayList.Where(c => c.CourseVideoId == item.Id).OrderByDescending(c => c.PlayDuration).Select(c => new { c.PlayDuration }).ToList();
                        int examCount = examList.Where(c => c.CourseVideoId == item.Id).Count();
                        if (playLog.Count <= 0 && examCount <= 0)
                        {
                            courseVideoState = 1;//待完成
                        }
                        else
                        {
                            var isPlayFinish = (playLog.FirstOrDefault() == null ? 0 : playLog.FirstOrDefault().PlayDuration) / (double)item.Duration > 0.8;
                            var isPass = examList.Where(c => c.CourseVideoId == item.Id && c.IsPass == true).Any();
                            if (isPlayFinish == true && isPass == true)
                            {
                                courseVideoState = 2;//已完成
                            }
                            else
                            {
                                courseVideoState = 3;//未完成
                            }
                        }
                        list.Add(new { item.Name, item.Id, packageInfo.CreateTime, courseVideoState, EndTime, coursePackageId, courseId });
                    }
                }
            }
            result.Data = list;
            return result;
        }

        /// <summary>
        /// 课程视频详情
        /// </summary>
        /// <param name="id">课程视频id</param>
        /// <param name="appUserId"></param>
        /// <param name="coursePackageId"></param>
        /// <param name="courseId">课程Id</param>
        /// <returns></returns>
        public async Task<TableData> CourseVideoDetails(int id, int appUserId, int coursePackageId, int courseId)
        {
            var result = new TableData();
            var videoInfo = await UnitWork.Find<classroom_course_video>(null).Where(c => c.CourseId == courseId && c.Id == id).FirstOrDefaultAsync();
            string videoUrl = videoInfo?.VideoUrl;
            bool isPlayFinish = false;
            decimal totalScore = 0;
            decimal maxScore = 0;
            bool isPass = false;
            var subjectIds = await UnitWork.Find<classroom_course_video_subject>(null).Where(c => c.CourseVideoId == id).Select(c => c.SubjectId).ToListAsync();
            var str = _helper.Post(new
            {
                ids = subjectIds
            }, (string.IsNullOrEmpty(_appConfiguration.Value.AppVersion) ? string.Empty : _appConfiguration.Value.AppVersion + "/") + "Exam/SubjectListByIds");
            JObject data = (JObject)JsonConvert.DeserializeObject(str);
            if (data["ErrorCode"] == null || data["ErrorCode"].ToString() != "200")
            {
                result.Code = 500;
                result.Message = "视频题目获取失败!";
                return result;
            }
            List<SubjectResp> subjectList = JsonConvert.DeserializeObject<List<SubjectResp>>(data["Data"].ToString());
            var playLog = await UnitWork.Find<classroom_video_play_log>(null).Where(c => c.CoursePackageId == coursePackageId && c.CourseId == courseId && c.CourseVideoId == id && c.AppUserId == appUserId).OrderByDescending(c => c.PlayDuration).FirstOrDefaultAsync();
            if (playLog != null)
            {
                isPlayFinish = (playLog.PlayDuration / (double)playLog.TotalDuration) > 0.8;
            }
            totalScore = subjectList.Select(c => c.score).Sum();
            var examInfo = await UnitWork.Find<classroom_course_exam>(null).Where(c => c.CoursePackageId == coursePackageId && c.CourseId == courseId && c.CourseVideoId == id && c.AppUserId == appUserId).OrderByDescending(c => c.TestScores).FirstOrDefaultAsync();
            if (examInfo != null)
            {
                maxScore = examInfo.TestScores;
                isPass = examInfo.IsPass;
            }
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
            var subjectIds = await UnitWork.Find<classroom_course_video_subject>(null).Where(c => c.CourseVideoId == id).Select(c => c.SubjectId).ToListAsync();
            var str = _helper.Post(new
            {
                ids = subjectIds
            }, (string.IsNullOrEmpty(_appConfiguration.Value.AppVersion) ? string.Empty : _appConfiguration.Value.AppVersion + "/") + "Exam/SubjectListByIds");
            JObject data = (JObject)JsonConvert.DeserializeObject(str);
            if (data["ErrorCode"] == null || data["ErrorCode"].ToString() != "200")
            {
                result.Code = 500;
                result.Message = "题目获取失败!";
                return result;
            }
            List<SubjectResp> subjectList = JsonConvert.DeserializeObject<List<SubjectResp>>(data["Data"].ToString());
            classroom_course_exam model = new classroom_course_exam();
            List<classroom_course_exam_subject> examSubjectList = new List<classroom_course_exam_subject>();
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
            foreach (var item in subjectList)
            {
                classroom_course_exam_subject exam_Subject = new classroom_course_exam_subject();
                var optionItems = JsonConvert.DeserializeObject<List<SubjectOptionResp>>(item.answer_options);
                exam_Subject.ExaminationId = exam.Id;
                exam_Subject.Type = item.type;
                exam_Subject.Content = item.content;
                exam_Subject.AnswerOptions = JsonConvert.SerializeObject(optionItems);
                exam_Subject.StandardAnswer = item.standard_answer;
                exam_Subject.Score = item.score;
                exam_Subject.CreateTime = DateTime.Now;
                exam_Subject.VideoUrl = item.video_url;
                exam_Subject.ImageUrl = item.image_url;
                exam_Subject.AnswerContent = "";
                exam_Subject.SubjectId = item.Id;
                exam_Subject.AnswerStatus = 0;
                exam_Subject.ModifyTime = DateTime.Now;
                examSubjectList.Add(exam_Subject);
            }
            await UnitWork.BatchAddAsync<classroom_course_exam_subject, int>(examSubjectList.ToArray());
            await UnitWork.SaveAsync();
            var list = await UnitWork.Find<classroom_course_exam_subject>(null)
                .Where(c => c.ExaminationId == exam.Id)
                .Select(c => new
                {
                    type = c.Type,
                    content = c.Content,
                    Items = JsonConvert.DeserializeObject(c.AnswerOptions),
                    subject_id = c.SubjectId,
                    video_url = c.VideoUrl,
                    image_url = c.ImageUrl,
                    id = c.Id
                }).OrderBy(c => c.id).ToListAsync();
            result.Data = new { ExaminationId = exam.Id, CourseVideoId = id, list };
            return result;
        }

        /// <summary>
        /// 提交试卷
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> SubmitExamPaper(SubmitExamReq req)
        {
            var result = new TableData();
            var ids = req.subjectList.Select(c => c.Id).ToList();
            var list = await UnitWork.Find<classroom_course_exam_subject>(null).Where(c => ids.Contains(c.Id) && c.ExaminationId == req.ExaminationId).ToListAsync();
            var examInfo = await UnitWork.Find<classroom_course_exam>(null).FirstOrDefaultAsync(c => c.Id == req.ExaminationId);
            foreach (var item in list)
            {
                var submitContent = req.subjectList.FirstOrDefault(c => c.Id == item.Id);
                string[] answerSheet = null;
                if (submitContent != null)
                {
                    if (item.Type != 3)
                    {
                        var standard_answer = item.StandardAnswer.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).OrderBy(x => x).ToArray();
                        answerSheet = submitContent.optionIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).OrderBy(x => x).ToArray();
                        if (Enumerable.SequenceEqual(standard_answer, answerSheet))
                        {
                            item.AnswerStatus = 1;
                            examInfo.TestScores += item.Score;
                        }
                        else
                            item.AnswerStatus = 2;
                        var optionItems = JsonConvert.DeserializeObject<List<SubjectOptionResp>>(item.AnswerOptions);
                        optionItems.ForEach(zw =>
                        {
                            if (answerSheet.Contains(zw.Id.ToString()))
                            {
                                zw.is_choice = true;
                            }
                        });
                        item.AnswerOptions = JsonConvert.SerializeObject(optionItems);
                    }
                    else
                    {
                        var standard_answer = item.StandardAnswer.Split(',');
                        answerSheet = submitContent.optionIds.Split(',');
                        if (Enumerable.SequenceEqual(standard_answer, answerSheet))
                        {
                            item.AnswerStatus = 1;
                            examInfo.TestScores += item.Score;
                        }
                        else
                        {
                            int correct_answers = 0;
                            for (int i = 0; i < standard_answer.Count(); i++)
                            {
                                if (i < answerSheet.Count())
                                {
                                    if (standard_answer[i].Trim().Equals(answerSheet[i].Trim()))
                                    {
                                        correct_answers++;
                                    }
                                }
                            }
                            item.AnswerStatus = 2;
                            examInfo.TestScores += (decimal)item.Score / standard_answer.Count() * correct_answers;
                        }
                        item.AnswerContent = submitContent.optionIds;
                    }
                }
            }
            examInfo.IsPass = examInfo.TestScores == examInfo.TotalScore;
            examInfo.IsSubmit = true;
            examInfo.SubmitTime = DateTime.Now;
            await UnitWork.UpdateAsync(examInfo);
            await UnitWork.BatchUpdateAsync(list.ToArray());
            await UnitWork.SaveAsync();
            result.Data = new { req.ExaminationId, examInfo.IsPass, examInfo.TestScores, examInfo.TotalScore, examInfo.SubmitTime };
            return result;
        }
        /// <summary>
        /// 考试结果
        /// </summary>
        /// <param name="examinationId"></param>
        /// <param name="courseVideoId"></param>
        /// <returns></returns>
        public async Task<TableData> ExamPaperResult(int examinationId, int courseVideoId)
        {
            var result = new TableData();
            var videoInfo = await UnitWork.Find<classroom_course_video>(null).FirstOrDefaultAsync(c => c.Id == courseVideoId);
            var examResult = await UnitWork.Find<classroom_course_exam>(null).FirstOrDefaultAsync(c => c.Id == examinationId);
            var subjectList = await UnitWork.Find<classroom_course_exam_subject>(null).Where(c => c.ExaminationId == examinationId).Select(c => new { c.Id, c.AnswerStatus }).ToListAsync();
            result.Data = new { name = videoInfo == null ? "" : videoInfo.Name + "练习", examResult.TotalScore, examResult.TestScores, subjectList, subjectCount = subjectList.Count };
            return result;
        }
        #endregion

    }
}
