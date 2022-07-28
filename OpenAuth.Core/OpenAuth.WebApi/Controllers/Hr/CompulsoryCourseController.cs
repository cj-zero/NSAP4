using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Hr;
using OpenAuth.App.Response;
using System;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Hr")]
    public class CompulsoryCourseController: ControllerBase
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
        /// 讲师申请记录
        /// </summary>
        /// <param name="name"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="auditState"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> TeacherApplyHistory(string name, DateTime? startTime, DateTime? endTime, int? auditState, int pageIndex, int pageSize)
        {
            var result = new TableData();
            try
            {
                result = await _app.TeacherApplyHistory(name, startTime, endTime, auditState, pageIndex, pageSize);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        #region 课程包相关
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
        public async Task<TableData> CoursePackageList(string name,string createUser, DateTime? startTime, DateTime? endTime,bool state, int pageIndex, int pageSize)
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
        public async Task<TableData> CreateCoursePackage(CoursePpackageReq req)
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
        public async Task<TableData> EditCoursePackage(CoursePpackageReq req)
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
        /// 编辑课程包
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> ChangeCoursePackageState(CoursePpackageReq req)
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
        #endregion
    }
}
