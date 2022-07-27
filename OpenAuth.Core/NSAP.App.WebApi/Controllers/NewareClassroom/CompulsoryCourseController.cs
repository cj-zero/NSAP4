using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Response;
using System;
using System.Threading.Tasks;

namespace NSAP.App.WebApi.Controllers
{
    /// <summary>
    /// 必修课模块
    /// </summary>
    [ApiController]
    public class CompulsoryCourseController : BaseController
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
        /// <param name="Name"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="AuditState"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> TeacherApplyHistory(string Name,DateTime? startTime,DateTime? endTime,int? AuditState,int PageIndex,int PageSize)
        {
            var result = new TableData();
            try
            {
                result = await _app.TeacherApplyHistory(Name, startTime, endTime, AuditState, PageIndex, PageSize);
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
