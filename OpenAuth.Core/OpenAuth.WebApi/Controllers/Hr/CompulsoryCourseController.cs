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
        /// 修改课程包状态
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

        /// <summary>
        /// 删除课包
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> DeleteCoursePackage(CoursePpackageReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.DeleteCoursePackage(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        #endregion

        #region 课程相关
        /// <summary>
        /// 课程列表
        /// </summary>
        /// <param name="name"></param>
        /// <param name="createUser"></param>
        /// <param name="learningCycle"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="state"></param>
        /// <param name="source">课程来源(1:主管推课  2:职前  3:入职  4:晋升  5:转正 6:变动)</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CourseList(string name,string createUser, int? learningCycle, DateTime? startTime, DateTime? endTime, bool? state,int? source, int pageIndex, int pageSize)
        {
            var result = new TableData();
            try
            {
                result = await _app.CourseList(name,createUser,learningCycle,startTime,endTime,state, source,pageIndex, pageSize);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 创建课程
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> CreateCourse(AddOrEditCourseReq req)
        {
            var result = new TableData();
            try
            {
                if (string.IsNullOrWhiteSpace(req.name))
                {
                    result.Code = 500;
                    result.Message = "请填写课程称!";
                    return result;
                }
                result = await _app.CreateCourse(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 编辑课程
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> EditCourse(AddOrEditCourseReq req)
        {
            var result = new TableData();
            try
            {
                if (string.IsNullOrWhiteSpace(req.name))
                {
                    result.Code = 500;
                    result.Message = "请填写课程名称!";
                    return result;
                }
                result = await _app.EditCourse(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 修改课程状态
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> ChangeCourseState(AddOrEditCourseReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.ChangeCourseState(req);
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
