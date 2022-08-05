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


















    }
}
