using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Hr;
using OpenAuth.App.Response;
using System;
using System.Threading.Tasks;

namespace NSAP.App.WebApi.Controllers
{
    /// <summary>
    /// 必修课模块
    /// </summary>
    [ApiController]
    public class CompulsoryCourseController : BaseController
    {
        private CompulsoryCourseApp _app;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        public CompulsoryCourseController(CompulsoryCourseApp app)
        {
            _app = app;
        }

        /// <summary>
        /// 必修课程列表
        /// </summary>
        /// <param name="appUserId"></param>
        /// <param name="state">课程状态 0:全部 1:已逾期 2:已完成 3:未开始 4:进行中</param>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CompulsoryCourseList(int appUserId, int? state, int source)
        {
            var result = new TableData();
            try
            {
                result = await _app.CompulsoryCourseList(appUserId, state, source);
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
        /// <param name="appUserId"></param>
        /// <param name="coursePackageId"></param>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CourseVideoList(int appUserId, int coursePackageId, int courseId)
        {
            var result = new TableData();
            try
            {
                result = await _app.CourseVideoList(appUserId, coursePackageId, courseId);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
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
        [HttpGet]
        public async Task<TableData> CourseVideoDetails(int id, int appUserId, int coursePackageId, int courseId)
        {
            var result = new TableData();
            try
            {
                result = await _app.CourseVideoDetails(id,appUserId, coursePackageId, courseId);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 保存观看记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> SavePlayLog(CourseVideoPlayLogReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.SavePlayLog(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 获取视频题目内容
        /// </summary>
        /// <param name="id"></param>
        /// <param name="appUserId"></param>
        /// <param name="coursePackageId"></param>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CreateExamPaper(int id, int appUserId, int coursePackageId, int courseId)
        {
            var result = new TableData();
            try
            {
                result = await _app.CreateExamPaper(id, appUserId,coursePackageId,courseId);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
    }
}
