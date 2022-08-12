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
    /// 职工申请相关
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Hr")]
    public class EmployeeApplyController : ControllerBase
    {

        private EmployeeApplyApp _app;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        public EmployeeApplyController(EmployeeApplyApp app)
        {
            _app = app;
        }

        /// <summary>
        /// 职工申请列表
        /// </summary>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="Name"></param>
        /// <param name="Mobile"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetEmployeeApplyList(DateTime? StartTime, DateTime? EndTime ,string Name ="", string Mobile="", int PageIndex=1, int PageSize = 20)
        {
            var result = new TableData();
            try
            {
                result = await _app.GetEmployeeApplyList( Name, Mobile, PageIndex, PageSize, StartTime , EndTime);
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
