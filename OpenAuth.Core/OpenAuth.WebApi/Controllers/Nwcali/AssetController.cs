using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.nwcali;
using OpenAuth.App.Nwcali.Response;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 实验室资产管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Nwcali")]
    public class AssetController : ControllerBase
    {
        private readonly AssetApp _app;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        public AssetController(AssetApp app)
        {
            _app = app;
        }

        /// <summary>
        /// 加载资产列表
        /// </summary>
        [HttpGet]
        public async Task<TableData> Load([FromQuery]QueryassetListReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.Load(request);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取失效日期大于当前时间的资产信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetExpiredAssets(string categoryId)
        {
            var result = new TableData();
            try
            {
                return await _app.GetExpiredAssets(categoryId);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{""}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取资产分类列表,可以根据资产分类名称筛选,不传则查询全部
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetAssetCategories(string categoryName = "")
        {
            var result = new TableData();

            try
            {
                return await _app.GetAssetCategories(categoryName);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{categoryName.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 根据资产分类id查询资产分类详情,可批量查询
        /// </summary>
        /// <param name="categoryIds"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetAssetCategoryDetails([FromQuery] string[] categoryIds)
        {
            var result = new TableData();

            try
            {
                return await _app.GetAssetCategoryDetails(categoryIds);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{categoryIds.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取单个自资产详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetAsset(int id)
        {
            var result = new TableData();
            try
            {
                return await  _app.GetAsset(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id}, 错误：{result.Message}");
            }

            return result;
        }


        /// <summary>
        /// 添加资产
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Add(AddOrUpdateassetReq obj)
        {
            var result = new Response();
            try
            {
                await _app.Add(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 修改资产
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Update(AddOrUpdateassetReq obj)
        {
            var result = new Response();
            try
            {
                await _app.Update(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 查询字典
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetListCategoryName()
        {
            var result = new TableData();
            try
            {
                string ids = "SYS_AssetStatus,SYS_AssetCategory,SYS_AssetSJType,SYS_CategoryNondeterminacy,SYS_AssetSJWay";
                return await _app.GetListCategoryName(ids);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 按名称模糊查询部门
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetListOrg(string name)
        {
            var result = new TableData();
            try
            {
                return await _app.GetListOrg(name);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{name}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 按名称模糊查询人员
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Orgid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetListUser(string name,string Orgid)
        {
            var result = new TableData();
            try
            {
                return await _app.GetListUser(name, Orgid);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{name},{Orgid}， 错误：{result.Message}");
            }
            return result;
            
        }


        /// <summary>
        /// 加载送检列表
        /// </summary>
        //[HttpGet]
        //public TableData AssetInspectsLoad(string AssetId)
        //{
        //    var result = new TableData();
        //    try
        //    {
        //        return _app.AssetInspectsLoad(AssetId);
        //    }
        //    catch (Exception ex)
        //    {

        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //    }
        //    return result;
        //}

        /// <summary>
        /// 加载操作列表
        /// </summary>
        //[HttpGet]
        //public async Task<TableData> AssetOperationsLoad(string AssetId)
        //{
        //    var result = new TableData();
        //    try
        //    {
        //        return await _app.AssetOperationsLoad(AssetId);
        //    }
        //    catch (Exception ex)
        //    {

        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //    }
        //    return result;
            
        //}

    }
}
