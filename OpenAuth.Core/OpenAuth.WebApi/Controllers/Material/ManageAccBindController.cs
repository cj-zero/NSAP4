using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.ClientRelation;
using OpenAuth.App.ClientRelation.Response;
using OpenAuth.App.Material;
using System.Threading.Tasks;
using System;
using OpenAuth.Repository.Domain.View;
using System.Collections.Generic;

namespace OpenAuth.WebApi.Controllers.Material
{
    /// <summary>
    /// 工程部账号绑定相关
    /// </summary>
    [Route("api/Material/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Material")]
    public class ManageAccBindController :  Controller
    {
        private readonly ManageAccBindApp _app;
        /// <summary>
        /// construct func
        /// </summary>
        /// <param name="app"></param>
        public ManageAccBindController(ManageAccBindApp app)
        {
            _app = app;
        }

        /// <summary>
        /// 获取4.0用户数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response<UserManageUtilityRsp>> GetUserUtilityList([FromBody] UserManageUtilityRequest req)
        {
            var result = new  Response<UserManageUtilityRsp>();
            try
            {
                result.Result = await _app.GetUserUtilityList(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }


        /// <summary>
        /// 获取绑定4.0账号
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response<BindUtilityRep>> GetBindUtilityList([FromBody] BindUtilityRequest req)
        {
            var result = new Response<BindUtilityRep>();
            try
            {
                result.Result = await _app.GetBindUtilityList(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }


        /// <summary>
        /// 修改绑定4.0账号
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response<bool>> UpdateBindUtility([FromBody] BindUtilityUpdateRequest req)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _app.UpdateBindUtility(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 考勤柱状图数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response<DutyChartResponse>> DutyChartUtility([FromBody] DutyChartRequest req)
        {
            var result = new Response<DutyChartResponse>();
            try
            {
                result.Result = await _app.DutyChartUtility(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 导出评分表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExportRateTableUtility([FromBody] RateTableReq req)
        {
            var data = await _app.ExportRateTableUtility(req.texports);

            return File(data, "application/vnd.ms-excel");
        }



    }
}
