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
    [ApiController]
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
        /// 讲师发布中心
        /// </summary>
        /// <param name="appUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> IssuedCenter(int appUserId)
        {
            var result = new TableData();
            try
            {
                result = await _app.IssuedCenter(appUserId);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
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
        public async Task<TableData> TeacherCourseApply(TeacherCourseApplyReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.TeacherCourseApply(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 讲师详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> TeacherDetail(int id)
        {
            var result = new TableData();
            try
            {
                result = await _app.TeacherDetail(id);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 编辑讲师信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> EditTeacher(TeacherApplyReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.EditTeacher(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 讲师介绍
        /// </summary>
        /// <param name="appUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> TeacherIntroduction(int appUserId)
        {
            var result = new TableData();
            try
            {
                result = await _app.TeacherIntroduction(appUserId);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 荣誉墙
        /// </summary>
        /// <param name="appUserId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> HonorWall(int appUserId, int pageIndex=1,int pageSize=10)
        {
            var result = new TableData();              
            try
            {
                result = await _app.HonorWall(appUserId,pageIndex,pageSize);
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
