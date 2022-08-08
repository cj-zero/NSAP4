using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Hr;
using OpenAuth.App.Hr.Request;
using OpenAuth.App.Response;
using System;
using System.Threading.Tasks;

namespace NSAP.App.WebApi.Controllers.NewareClassroom
{
    public class SubjectCourseController : BaseController
    {
        private readonly SubjectCourseApp _app;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        public SubjectCourseController(SubjectCourseApp app)
        {
            _app = app;
        }

        /// <summary>
        /// 专题列表
        /// </summary>
        /// <param name="appUserId"></param>
        /// <param name="name">专题名</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ClassroomSubjectList(int appUserId, string name)
        {
            var result = new TableData();
            try
            {
                result = await _app.ClassroomSubjectList(appUserId, name);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 专题课程列表
        /// </summary>
        /// <param name="appUserId"></param>
        /// <param name="subjectId">专题Id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ClassroomSubjectCourseList(int appUserId, int subjectId,string name)
        {
            var result = new TableData();
            try
            {
                result = await _app.ClassroomSubjectCourseList(appUserId, subjectId, name);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 修改课程观看记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> UpdateCourseRecord(SubjectCourseRecordReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.UpdateCourseRecord(req);
            }
            
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 修改专题观看次数
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> UpdateSubjectViewNumber(int subjectId)
        {
            var result = new TableData();
            try
            {
                result = await _app.UpdateSubjectViewNumber(subjectId);
            }

            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 修改课程观看次数
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> UpdateCourseViewNumber(int courseId)
        {
            var result = new TableData();
            try
            {
                result = await _app.UpdateCourseViewNumber(courseId);
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
