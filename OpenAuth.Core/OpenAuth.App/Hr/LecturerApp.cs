using Common;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private IOptions<AppSetting> _appConfiguration;
        private HttpHelper _helper;
        private ILogger<LecturerApp> _logger;
        /// <summary>
        /// 讲师相关
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        /// <param name="appConfiguration"></param>
        /// <param name="logger"></param>
        public LecturerApp(IUnitWork unitWork, IAuth auth, IOptions<AppSetting> appConfiguration, ILogger<LecturerApp> logger) : base(unitWork, auth)
        {
            _appConfiguration = appConfiguration;
            _helper = new HttpHelper(_appConfiguration.Value.AppPushMsgUrl);
            _logger= logger;
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
                .Select(c => new { c.Id, c.Name, c.Age, c.AuditState, c.Department, c.CanTeachCourse, c.BeGoodAtTerritory, c.CreateTime })
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
            if (req.auditState == 3)
            {
                try
                {
                    string title = "讲师申请";
                    string content = "讲师申请失败!";
                    string payload = "{\"urlType\":1,\"url\":\"/pages/afterSale/course/publishCenter\"}";
                    var str = _helper.Post(new
                    {
                        userIds = new List<int> { query.AppUserId },
                        title = title,
                        content = content,
                        payload = payload
                    }, (string.IsNullOrEmpty(_appConfiguration.Value.AppVersion) ? string.Empty : _appConfiguration.Value.AppVersion + "/") + "Message/AppExternalMessagePush", "", "");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"讲师申请推送失败,AppUserId={query.AppUserId }! message={ex.Message}");
                }
            }
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
                .Where(c => c.AuditState != 1 && c.AuditState != 3)
                .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name.Contains(name))
                .WhereIf(startTime != null, c => c.CreateTime >= startTime)
                .WhereIf(endTime != null, c => c.CreateTime <= endTime)
                .WhereIf(grade != null && grade != 0, c => c.Grade == grade)
                .Select(c => new { c.Name, c.Age, c.Department, c.BeGoodAtTerritory, c.CanTeachCourse, c.CreateTime, c.Grade, c.Experience, c.Id, c.AuditState })
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
            if (model != null)
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
            result.Data = await (from a in UnitWork.Find<classroom_teacher_course>(null)
                                 join b in UnitWork.Find<classroom_teacher_apply_log>(null) on a.AppUserId equals b.AppUserId
                                 select new { b.Name, a.Id, a.ForTheCrowd, a.Title, a.TeachingMethod, a.TeachingAddres, a.StartTime, a.EndTime, a.VideoUrl, a.AuditState, a.CreateTime })
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
            if (req.auditState == 3)
            {
                try
                {
                    string title = "讲师开课申请";
                    string content = $"讲师【{query.Title}】课程开课申请失败!";
                    string payload = "{\"urlType\":1,\"url\":\"/pages/afterSale/course/publishCenter\"}";
                    var str = _helper.Post(new
                    {
                        userIds = new List<int> { query.AppUserId },
                        title = title,
                        content = content,
                        payload = payload
                    }, (string.IsNullOrEmpty(_appConfiguration.Value.AppVersion) ? string.Empty : _appConfiguration.Value.AppVersion + "/") + "Message/AppExternalMessagePush", "", "");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"讲师开课申请推送失败,AppUserId={query.AppUserId }! message={ex.Message}");
                }
            }
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
        /// 讲师发布中心
        /// </summary>
        /// <param name="appUserId"></param>
        /// <returns></returns>
        public async Task<TableData> IssuedCenter(int appUserId)
        {
            var result = new TableData();
            var query = await UnitWork.Find<classroom_teacher_apply_log>(null).Where(c => c.AppUserId == appUserId).OrderByDescending(c => c.Id).FirstOrDefaultAsync();
            List<classroom_teacher_course> list = new List<classroom_teacher_course>();
            int AuditState = 0;
            if (query != null)
            {
                if (query.AuditState == 2)
                {
                    list = await UnitWork.Find<classroom_teacher_course>(null)
                        .Where(c => c.AppUserId == appUserId)
                        .OrderByDescending(c => c.Id)
                        .ToListAsync();
                }
                AuditState = query.AuditState;
            }
            result.Data = new { AuditState, appUserId, Id = query == null ? 0 : query.Id, list };
            return result;
        }

        /// <summary>
        /// 讲师提交申请
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> TeacherApply(TeacherApplyReq req)
        {
            var result = new TableData();
            var query = await UnitWork.Find<classroom_teacher_apply_log>(null).Where(c => c.AppUserId == req.AppUserId).OrderByDescending(c => c.Id).FirstOrDefaultAsync();
            if (query != null)
            {
                if (query.AuditState == 4)
                {
                    result.Code = 500;
                    result.Message = "您已被管理员封禁,请联系管理员进行解封!";
                    return result;
                }
                else if (query.AuditState == 1)
                {
                    result.Code = 500;
                    result.Message = "您已提交过申请,管理员正在审核!";
                    return result;
                }
                else if (query.AuditState == 2)
                {
                    result.Code = 500;
                    result.Message = "已是讲师无需申请!";
                    return result;
                }
                else if (query.AuditState == 3)
                {
                    classroom_teacher_apply_log model = new classroom_teacher_apply_log();
                    model.Name = req.Name;
                    model.Age = req.Age;
                    model.Mobile = req.Mobile;
                    model.HeaderImg = req.HeaderImg;
                    model.Department = req.Department;
                    model.CanTeachCourse = req.CanTeachCourse;
                    model.BeGoodAtTerritory = req.BeGoodAtTerritory;
                    model.CreateTime = DateTime.Now;
                    model.ModifyTime = DateTime.Now;
                    model.AppUserId = req.AppUserId;
                    model.AuditState = 1;
                    await UnitWork.AddAsync<classroom_teacher_apply_log, int>(query);
                    await UnitWork.SaveAsync();
                }
            }
            else
            {
                classroom_teacher_apply_log model = new classroom_teacher_apply_log();
                model.Name = req.Name;
                model.Age = req.Age;
                model.Mobile = req.Mobile;
                model.HeaderImg = req.HeaderImg;
                model.Department = req.Department;
                model.CanTeachCourse = req.CanTeachCourse;
                model.BeGoodAtTerritory = req.BeGoodAtTerritory;
                model.CreateTime = DateTime.Now;
                model.ModifyTime = DateTime.Now;
                model.AppUserId = req.AppUserId;
                model.AuditState = 1;
                await UnitWork.AddAsync<classroom_teacher_apply_log, int>(model);
                await UnitWork.SaveAsync();
            }
            return result;
        }

        /// <summary>
        /// 开课申请
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> TeacherCourseApply(TeacherCourseApplyReq req)
        {
            var result = new TableData();
            var query = await UnitWork.Find<classroom_teacher_apply_log>(null).Where(c => c.AppUserId == req.AppUserId).OrderByDescending(c => c.Id).FirstOrDefaultAsync();
            if (query == null || query.AuditState != 2)
            {
                result.Code = 500;
                result.Message = "您当前已不是讲师,无法提交开课申请!";
                return result;
            }
            var isExit = await UnitWork.Find<classroom_teacher_course>(null).Where(c => c.Title == req.Title).CountAsync() > 0;
            if (isExit)
            {
                result.Code = 500;
                result.Message = "课程名称已被占用,请修改后提交!";
                return result;
            }
            classroom_teacher_course model = new classroom_teacher_course();
            model.Title = req.Title;
            model.StartTime = req.StartTime;
            model.EndTime = req.EndTime;
            model.ForTheCrowd = req.ForTheCrowd;
            model.TeachingMethod = req.TeachingMethod;
            model.TeachingAddres = req.TeachingAddres;
            model.AppUserId = req.AppUserId;
            model.BackgroundImage = req.BackgroundImage;
            model.VideoUrl = "";
            model.AuditState = 1;
            model.CreateTime = DateTime.Now;
            await UnitWork.AddAsync<classroom_teacher_course, int>(model);
            await UnitWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// 讲师详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TableData> TeacherDetail(int id)
        {
            var result = new TableData();
            result.Data = await UnitWork.Find<classroom_teacher_apply_log>(null).Where(c => c.Id == id).FirstOrDefaultAsync();
            return result;
        }

        /// <summary>
        /// 编辑讲师信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> EditTeacher(TeacherApplyReq req)
        {
            var result = new TableData();
            var model = await UnitWork.Find<classroom_teacher_apply_log>(null).Where(c => c.Id == req.Id).FirstOrDefaultAsync();
            if (model != null)
            {
                model.Name = req.Name;
                model.Age = req.Age;
                model.Mobile = req.Mobile;
                model.HeaderImg = req.HeaderImg;
                model.Department = req.Department;
                model.CanTeachCourse = req.CanTeachCourse;
                model.BeGoodAtTerritory = req.BeGoodAtTerritory;
                model.ModifyTime = DateTime.Now;
                await UnitWork.UpdateAsync(model);
                await UnitWork.SaveAsync();
            }
            return result;
        }

        /// <summary>
        /// 讲师介绍
        /// </summary>
        /// <param name="appUserId"></param>
        /// <returns></returns>
        public async Task<TableData> TeacherIntroduction(int appUserId)
        {
            var result = new TableData();
            var query = await UnitWork.Find<classroom_teacher_apply_log>(null).Where(c => c.AppUserId == appUserId && c.AuditState == 2).OrderByDescending(c => c.Id).FirstOrDefaultAsync();
            if (query != null)
            {
                var list = await UnitWork.Find<classroom_teacher_course>(null)
                    .Where(c => c.AppUserId == appUserId && c.AuditState == 2)
                    .Select(c => new { c.Title, c.StartTime, c.EndTime, c.TeachingMethod, c.TeachingAddres, c.ForTheCrowd, c.ViewedCount, c.VideoUrl })
                    .ToListAsync();
                int popularityValue = list.Sum(c => c.ViewedCount);
                result.Data = new
                {
                    query.Id,
                    query.Name,
                    query.BeGoodAtTerritory,
                    query.CanTeachCourse,
                    query.HeaderImg,
                    query.Grade,
                    popularityValue,
                    list
                };
            }
            return result;
        }

        /// <summary>
        /// 荣誉墙
        /// </summary>
        /// <param name="appUserId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<TableData> HonorWall(int appUserId, int pageIndex, int pageSize)
        {
            var result = new TableData();
            var query = await UnitWork.Find<classroom_teacher_apply_log>(null)
                .Where(c => c.AuditState == 2)
                .Select(c => new { c.Name, c.BeGoodAtTerritory, c.CanTeachCourse, c.Grade, c.Experience, c.AppUserId, c.HeaderImg }).OrderByDescending(c => c.Experience)
                .ToListAsync();
            object myHonor = null;
            var myHonorInfo = query.Where(c => c.AppUserId == appUserId).FirstOrDefault();
            if (myHonorInfo != null)
            {
                int index = query.FindIndex(c => c.AppUserId == appUserId) + 1;
                myHonor = new { myHonorInfo.Name, myHonorInfo.BeGoodAtTerritory, myHonorInfo.CanTeachCourse, myHonorInfo.Grade, myHonorInfo.Experience, myHonorInfo.AppUserId, index, myHonorInfo.HeaderImg };
            }
            var list = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            result.Data = new { myHonor, list };
            return result;
        }

     
        #endregion
    }
}
