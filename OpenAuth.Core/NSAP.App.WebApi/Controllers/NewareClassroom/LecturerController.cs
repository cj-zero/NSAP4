using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Hr;
using OpenAuth.App.Response;
using System;
using System.Threading.Tasks;

namespace NSAP.App.WebApi.Controllers.NewareClassroom
{
    /// <summary>
    /// App讲师相关
    /// </summary>
    public class LecturerController : BaseController
    {
        private LecturerApp _app;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        public LecturerController(LecturerApp app)
        {
            _app = app;
        }

        /// <summary>
        /// 讲师提交申请
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> TeacherApply(TeacherApplyReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.TeacherApply(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 开课申请
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> TeacherCourseApply()
        {
            var result = new TableData();
            try
            {
                result = await _app.TeacherCourseApply();
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
