using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Material;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Response;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Material
{
    /// <summary>
    /// BTS中位机和下位机软件版本管理相关
    /// </summary>
    [Route("api/Material/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Material")]
    public class ZWJAndXWJController : ControllerBase
    {
        private readonly ZWJAndXwjMGMTApp _app;
        public ZWJAndXWJController(ZWJAndXwjMGMTApp app)
        {
            _app = app;
        }

        #region 中位机软件版本管理
        /// <summary>
        /// 新增中位机软件版本记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> AddZWJSoftwareVersion(AddOrUpdateZWJSoftwareInfoReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.AddZWJSoftwareVersion(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 查询中位机软件版本记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetZWJSoftwareVersions([FromQuery] QueryZWJSoftwareListReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.GetZWJSoftwareVersions(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 修改中位机软件版本记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> UpdateZWJSoftwareVersion(AddOrUpdateZWJSoftwareInfoReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.UpdateZWJSoftwareVersion(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 删除中位机软件版本记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> DeleteZWJSoftwareVersion(int id)
        {
            var result = new TableData();
            try
            {
                result = await _app.DeleteZWJSoftwareVersion(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }
        #endregion

        #region 下位机软件版本管理
        /// <summary>
        /// 新增下位机软件版本记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> AddXWJSoftwareVersion(AddOrUpdateXWJSoftwareInfoReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.AddXWJSoftwareVersion(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 查询下位机软件版本记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetXWJSoftwareVersions([FromQuery] QueryXWJSoftwareListReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.GetXWJSoftwareVersions(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 修改下位机软件版本记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> UpdateXWJSoftwareVersion(AddOrUpdateXWJSoftwareInfoReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.UpdateXWJSoftwareVersion(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 删除下位机软件版本记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> DeleteXWJSoftwareVersion(int id)
        {
            var result = new TableData();
            try
            {
                result = await _app.DeleteXWJSoftwareVersion(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        #endregion

        #region 下位机版本映射
        /// <summary>
        /// 获取下位机软件版本别名
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetXWJSoftwareVersionAlias()
        {
            var result = new TableData();
            try
            {
                result = await _app.GetXWJSoftwareVersionAlias();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{""}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 新增下位机版本映射
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> AddXWJHardwareMap(AddOrUpdateXWJMapReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.AddXWJHardwareMap(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 查询下位机版本映射记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetXWJHarewareMaps([FromQuery] QueryXWJHarewareListReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.GetXWJHarewareMaps(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 修改下位机版本映射
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> UpdateXWJHarewareMap(AddOrUpdateXWJMapReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.UpdateXWJHarewareMap(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 删除下位机版本映射
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> DeleteXWJHarewareMap(int id)
        {
            var result = new TableData();
            try
            {
                result = await _app.DeleteXWJHarewareMap(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }
        #endregion
    }
}
