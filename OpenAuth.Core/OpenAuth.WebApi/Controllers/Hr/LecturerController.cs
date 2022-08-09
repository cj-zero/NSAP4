using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Hr;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using System;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Hr
{
    /// <summary>
    /// 讲师相关
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Hr")]
    public class LecturerController:ControllerBase
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

        /// <summary>
        /// 讲师申请审核
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> TeacherApplyAudit(TeacherApplyAuditReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.TeacherApplyAudit(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
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
        [HttpGet]
        public async Task<TableData> TeacherList(string name, DateTime? startTime, DateTime? endTime,int? grade, int pageIndex, int pageSize)
        {
            var result = new TableData();
            try
            {
                result = await _app.TeacherList(name, startTime, endTime, grade, pageIndex, pageSize);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 讲师等级修改
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> ChangeTeacherGrade(EditTeacherReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.ChangeTeacherGrade(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 讲师开课记录
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="title"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="auditState"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> TeacherCourseList(string userName,string title, DateTime? startTime, DateTime? endTime,int? auditState, int pageIndex, int pageSize)
        {
            var result = new TableData();
            try
            {
                result = await _app.TeacherCourseList(userName, title, startTime, endTime, auditState, pageIndex, pageSize);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 讲师开课审核
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> TeacherCourseAudit(TeacherCourseAuditReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.TeacherCourseAudit(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 讲师课程修改
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> EditTeacherCourse(classroom_teacher_course req)
        {
            var result = new TableData();
            try
            {
                result = await _app.EditTeacherCourse(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 讲师经验值计算
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CalculateTeacherExperience()
        {
            var result = new TableData();
            try
            {
                result = await _app.CalculateTeacherExperience();
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
