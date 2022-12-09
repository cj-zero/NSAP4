using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.ClientRelation;
using OpenAuth.App.ClientRelation.Response;
using OpenAuth.App.Material;
using System.Threading.Tasks;
using System;
using OpenAuth.Repository.Domain.View;
using System.Collections.Generic;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using System.IO;
using Newtonsoft.Json.Linq;
using Magicodes.ExporterAndImporter.Core.Models;
using OpenAuth.App.Response;

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
        /// 校验提交统计是否合法
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response<List<string>>> LegitCheckUtility([FromBody] LegitCheckRequest req)
        {
            var result = new Response<List<string>>();
            try
            {
                result.Result = await _app.LegitCheckUtility(req);
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
        /// 考勤任务明细
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response<DutyDetailsRsp>> DutyDetailsTableUtility([FromBody] DutyChartRequest req)
        {
            var result = new Response<DutyDetailsRsp>();
            try
            {
                result.Result = await _app.DutyDetailsTableUtility(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 获取对应月份归档记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<ArchiveData>> GetArchives(string  req)
        {
            var result = new Response<ArchiveData>();
            try
            {
                result.Result = await _app.GetArchives(req);
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

            return File(data, "application/octet-stream","Rate.xlsx");
        }

        /// <summary>
        /// 导出评分明细表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExportRateDetailTableUtility([FromBody] DetailExportData req)
        {
            IExportFileByTemplate exporter = new ExcelExporter();
            var fileName = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "DutyDetail.xlsx");
            //var exportData = new DetailExportData();
            //exportData.detports.Add(new DetailExport {  Name="jack"});
            var data =  await exporter.ExportBytesByTemplate(req, fileName);
            return File(data, "application/octet-stream", "RateDetail.xlsx");
        }

        /// <summary>
        /// 保存评分表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response<bool>> SaveRateDetails([FromBody] DetailExportSaveData req)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _app.SaveRateDetails(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }


        /// <summary>
        /// 获取物料数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public  TableData GetDataA (string req)
        {
            //overtime code 
            var result = new TableData();
            try
            {
                result =  _app.GetDataA(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 获取物料下级数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public  TableData GetDataB(string req)
        {
            //overtime code 
            var result = new TableData();
            try
            {
                result =  _app.GetDataB(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }


    }
}
