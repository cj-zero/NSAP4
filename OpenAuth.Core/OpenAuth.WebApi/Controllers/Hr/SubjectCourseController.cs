using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Hr;
using OpenAuth.App.Hr.Request;
using OpenAuth.App.Response;
using System;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Hr
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Hr")]
    public class SubjectCourseController : ControllerBase
    {
        private SubjectCourseApp _app;
        /// <summary>
        /// 专题
        /// </summary>
        /// <param name="app"></param>
        public SubjectCourseController(SubjectCourseApp app)
        {
            _app = app;
        }

        /// <summary>
        /// 专题列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> GetSubjectListByErp(GetSubjectListByErpReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.GetSubjectListByErp(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 新增/修改专题
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> AddOrModifySubjectByErp(AddOrEditSubjectReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.AddOrModifySubjectByErp(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 删除专题
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> DelSubjectByErp(int subjectId)
        {
            var result = new TableData();
            try
            {
                result = await _app.DelSubjectByErp(subjectId);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 调整专题排序
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AdjustSubjectByErp(int oldId, int newId)
        {
            var result = new TableData();
            try
            {
                result = await _app.AdjustSubjectByErp(oldId, newId);
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
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> GetSubjectCourseListByErp(GetSubjectCourseListByErpReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.GetSubjectCourseListByErp(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }        [HttpPost]
        /// <summary>
        /// 新增/修改专题课程列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> AddOrModifySubjectCourseByErp(AddOrEditSubjectCourseReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.AddOrModifySubjectCourseByErp(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 删除专题课程
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> DelSubjectCourseByErp(int subjectId)
        {
            var result = new TableData();
            try
            {
                result = await _app.DelSubjectCourseByErp(subjectId);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 调整专题课程排序
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AdjustSubjectCourseByErp(int oldId, int newId)
        {
            var result = new TableData();
            try
            {
                result = await _app.AdjustSubjectCourseByErp(oldId, newId);
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
