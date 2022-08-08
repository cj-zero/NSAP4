using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Hr;
using OpenAuth.App.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NSAP.App.WebApi.Controllers.NewareClassroom
{

    /// <summary>
    ///  课堂首页
    /// </summary>
    [ApiController]
    public class ClassHomeController : BaseController
    {

        private ClassHomeApp _app;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="app"></param>
        public ClassHomeController(ClassHomeApp app)
        {
            this._app = app;
        }

        /// <summary>
        /// 课程首页模糊搜索
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> GetEmployeeApplyList(ClassHomeSearchReq req)
        {
            var result = new TableData();
            try
            {
               switch(req.type)
               {
                    case 1:
                        result = await _app.CompulsoryCourseList(req.appUserId, req.key ,req.pageIndex, req.pageSize);
                        break;
                    case 2:
                        result = await _app.ClassroomSubjectList(req.appUserId, req.key, req.pageIndex, req.pageSize);
                        break;
                    case 3:
                        result = await _app.TeacherCoursePlayBack(req.appUserId, req.key, req.pageIndex, req.pageSize);
                        break;
                    default:
                        break;
               }
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
