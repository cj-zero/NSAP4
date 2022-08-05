using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Hr;
using OpenAuth.App.Hr.Request;
using OpenAuth.App.Response;
using System;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Hr
{

    /// <summary>
    /// 专题课程相关
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Hr")]
    public class SubjectCourseController : ControllerBase
    {
        private SubjectCourseApp _app;

        #region 专题
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
        [HttpPost]
        public async Task<TableData> DelSubjectByErp(DeleteModelReq<int> req)
        {
            var result = new TableData();
            try
            {
                result = await _app.DelSubjectByErp(req.Id);
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
        [HttpPost]
        public async Task<TableData> AdjustSubjectByErp(SortExchange<int> req)
        {
            var result = new TableData();
            try
            {
                result = await _app.AdjustSubjectByErp(req.OldId, req.NewId);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        #endregion


        #region 专题课程
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
        }        
        /// <summary>
        /// 新增/修改专题课程
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
        [HttpPost]
        public async Task<TableData> DelSubjectCourseByErp(DeleteModelReq<int> req)
        {
            var result = new TableData();
            try
            {
                result = await _app.DelSubjectCourseByErp(req.Id);
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
        [HttpPost]
        public async Task<TableData> AdjustSubjectCourseByErp(SortExchange<int> req)
        {
            var result = new TableData();
            try
            {
                result = await _app.AdjustSubjectCourseByErp(req.OldId, req.NewId);
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
