using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Hr;
using OpenAuth.App.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 必修课相关
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Hr")]
    public class CompulsoryCourseController : ControllerBase
    {
        private CompulsoryCourseApp _app;
        /// <summary>
        /// 必修课
        /// </summary>
        /// <param name="app"></param>
        public CompulsoryCourseController(CompulsoryCourseApp app)
        {
            _app = app;
        }


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
        [HttpGet]
        public async Task<TableData> CoursePackageList(string name, string createUser, DateTime? startTime, DateTime? endTime, bool state, int pageIndex, int pageSize)
        {
            var result = new TableData();
            try
            {
                result = await _app.CoursePackageList(name, createUser, startTime, endTime, state, pageIndex, pageSize);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 创建课程包
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> CreateCoursePackage(CoursePackageReq req)
        {
            var result = new TableData();
            try
            {
                if (string.IsNullOrWhiteSpace(req.name))
                {
                    result.Code = 500;
                    result.Message = "请填写课包名称!";
                    return result;
                }
                result = await _app.CreateCoursePackage(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 编辑课程包
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> EditCoursePackage(CoursePackageReq req)
        {
            var result = new TableData();
            try
            {
                if (string.IsNullOrWhiteSpace(req.name))
                {
                    result.Code = 500;
                    result.Message = "请填写课包名称!";
                    return result;
                }
                result = await _app.EditCoursePackage(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 修改课程包状态
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> ChangeCoursePackageState(CoursePackageReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.ChangeCoursePackageState(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 删除课包
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> DeleteCoursePackage(CoursePackageReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.DeleteCoursePackage(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        #endregion

        #region  课程包课程相关
        /// <summary>
        /// 课包课程列表
        /// </summary>
        /// <param name="coursePackageId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CoursePackageCourseList(int coursePackageId)
        {
            var result = new TableData();
            try
            {
                result = await _app.CoursePackageCourseList(coursePackageId);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 课程包添加课程
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> AddCourseIntoCoursePackage(CourseForCoursePackageReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.AddCourseIntoCoursePackage(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 删除课程包课程
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> DeleteCourseIntoCoursePackage(CourseForCoursePackageReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.DeleteCourseIntoCoursePackage(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 修改课程包课程排序
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> ChangeCourseSort(List<CourseSortResp> req)
        {
            var result = new TableData();
            try
            {
                result = await _app.ChangeCourseSort(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        #endregion

        #region 课程包人员相关

        /// <summary>
        /// 课程包添加人员
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> AddCoursePackageUser(CoursePackageUserReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.AddCoursePackageUser(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
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
        [HttpGet]
        public async Task<TableData> CoursePackageUserList(int coursePackageId, string name, decimal? schedule, DateTime? startTime, DateTime? endTime, int pageIndex = 1, int pageSize = 10)
        {
            var result = new TableData();
            try
            {
                result = await _app.CoursePackageUserList(coursePackageId, name, schedule, startTime, endTime, pageIndex, pageSize);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 删除课程包人员
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> DeleteCoursePackageUser(CoursePackageUserReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.DeleteCoursePackageUser(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        #endregion

        #region  课程
        /// <summary>
        /// 课程列表
        /// </summary>
        /// <param name="name"></param>
        /// <param name="createUser"></param>
        /// <param name="learningCycle"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="state"></param>
        /// <param name="source">课程来源(1:主管推课  2:职前  3:入职  4:晋升  5:转正 6:变动)</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CourseList(string name, string createUser, int? learningCycle, DateTime? startTime, DateTime? endTime, bool? state, int? source, int pageIndex, int pageSize)
        {
            var result = new TableData();
            try
            {
                result = await _app.CourseList(name, createUser, learningCycle, startTime, endTime, state, source, pageIndex, pageSize);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 创建课程
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> CreateCourse(AddOrEditCourseReq req)
        {
            var result = new TableData();
            try
            {
                if (string.IsNullOrWhiteSpace(req.name))
                {
                    result.Code = 500;
                    result.Message = "请填写课程称!";
                    return result;
                }
                result = await _app.CreateCourse(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 编辑课程
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> EditCourse(AddOrEditCourseReq req)
        {
            var result = new TableData();
            try
            {
                if (string.IsNullOrWhiteSpace(req.name))
                {
                    result.Code = 500;
                    result.Message = "请填写课程名称!";
                    return result;
                }
                result = await _app.EditCourse(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 删除课程
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> DeleteCourse(AddOrEditCourseReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.DeleteCourse(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 修改课程状态
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> ChangeCourseState(AddOrEditCourseReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.ChangeCourseState(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
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
        [HttpPost]
        public async Task<TableData> AddCourseVideo(AddOrEditCourseVideoReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.AddCourseVideo(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
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
        [HttpGet]
        public async Task<TableData> CourseVideoList(string name, string createUser, DateTime? startTime, DateTime? endTime, bool? state)
        {
            var result = new TableData();
            try
            {
                result = await _app.CourseVideoList(name, createUser, startTime, endTime, state);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;

        }
        /// <summary>
        /// 删除课程视频
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> DeleteCourseVideo(DeleteCourseVideoReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.DeleteCourseVideo(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        #endregion

        #region 课程视频习题
        /// <summary>
        /// 课程视频题目列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CourseVideoSubjectList(int id)
        {
            var result = new TableData();
            try
            {
                result = await _app.CourseVideoSubjectList(id);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 课程视频添加题目
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> CourseVideoAddSubject(CourseVideoAddSubjectReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.CourseVideoAddSubject(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 删除课程视频题目
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> DeleteCourseVideoSubject(CourseVideoAddSubjectReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.DeleteCourseVideoSubject(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        #endregion


        #region  课程答题情况
        /// <summary>
        /// 用户课程视频列表
        /// </summary>
        /// <param name="appUserId"></param>
        /// <param name="coursePackageId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> UserCourseVideoList(int appUserId, int coursePackageId)
        {
            var result = new TableData();
            try
            {
                result = await _app.UserCourseVideoList(appUserId, coursePackageId);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 考试试卷结果详情
        /// </summary>
        /// <param name="examId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> UserVideoExamResult(int examId, int pageIndex = 1, int pageSize = 10)
        {
            var result = new TableData();
            try
            {
                pageIndex = pageIndex <= 0 ? 1 : pageIndex;
                pageSize = pageSize <= 0 ? 10 : pageSize;
                result = await _app.UserVideoExamResult(examId, pageIndex, pageSize);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        #endregion
    }
}
