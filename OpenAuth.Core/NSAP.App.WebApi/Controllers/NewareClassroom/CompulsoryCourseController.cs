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
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> TeacherList()
        {
            var result = new TableData();
            try
            {
                result = await _app.TeacherList();
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
