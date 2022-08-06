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
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> GetEmployeeApplyList(GetEmployeeApplyListReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.GetEmployeeApplyList(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 职工申请审核
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> EmployeeApplyAudit(EmployeeApplyAuditReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.EmployeeApplyAudit(req);
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
