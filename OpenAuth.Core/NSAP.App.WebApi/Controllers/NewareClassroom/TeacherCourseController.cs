using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Hr;
using OpenAuth.App.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSAP.App.WebApi.Controllers.NewareClassroom
{

    /// <summary>
    /// 直播视频相关
    /// </summary>
    [ApiController]
    public class TeacherCourseController : BaseController
    {

        private TeacherCourseApp _app;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        public TeacherCourseController(TeacherCourseApp app)
        {
            this._app = app;
        }

        /// <summary>
        ///  推荐老师
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> RecommendTeachers( int limit = 6)
        {
            var result = new TableData();
            try
            {
                result = await _app.RecommendTeachers(limit);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        ///  直播预告
        /// </summary>
        /// <param name="appUserId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> TeacherCourseAdvanceNotice(int appUserId,int pageIndex = 1, int pageSize = 10)
        {
            var result = new TableData();
            try
            {
                result = await _app.TeacherCourseAdvanceNotice(appUserId,pageIndex, pageSize);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        ///  直播回放
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> TeacherCoursePlayBack(int pageIndex = 1, int pageSize = 10)
        {
            var result = new TableData();
            try
            {
                result = await _app.TeacherCoursePlayBack(pageIndex, pageSize);
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
        public async Task<TableData> SavePlayLog(TeacherCoursePlayLogReq req)
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
        /// 讲师视频点击埋点
        /// </summary>
        /// <param name="id">讲师视频id</param>
        /// <returns></returns>

        [HttpGet]
        public async Task<TableData> TeacherCourseBuriedPoint(int id)
        {
            var result = new TableData();
            try
            {
                result = await _app.TeacherCourseBuriedPoint(id);
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
