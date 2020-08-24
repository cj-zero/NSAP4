using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.nwcali;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 实验室资产管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
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
        public TableData Load([FromQuery]QueryassetListReq request)
        {
            var result = new TableData();
            try
            {
                return _app.Load(request);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        #region 获取单个自资产详情
        /// <summary>
        /// 获取单个自资产详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<Asset>> GetAsset(string id)
        {
            var result = new Response<Asset>();
            try
            {
                result.Result = await _app.GetAsset(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }
        #endregion
        /// <summary>
        /// 添加资产
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public Response Add(AddOrUpdateassetReq obj)
        {
            var result = new Response();
            try
            {
                _app.Add(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 修改资产
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public Response Update(AddOrUpdateassetReq obj)
        {
            var result = new Response();
            try
            {
                _app.Update(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 查询字典
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public TableData GetListCategoryName()
        {
            var result = new TableData();
            try
            {
                string ids = "SYS_AssetStatus,SYS_AssetCategory,SYS_AssetSJType,SYS_CategoryNondeterminacy,SYS_AssetSJWay";
                return _app.GetListCategoryName(ids);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 按名称模糊查询部门
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        public TableData GetListOrg(string name)
        {
            var result = new TableData();
            try
            {
                return _app.GetListOrg(name);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
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
        public TableData GetListUser(string name,string Orgid)
        {
            var result = new TableData();
            try
            {
                return _app.GetListUser(name, Orgid);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
            
        }


        /// <summary>
        /// 加载送检列表
        /// </summary>
        [HttpGet]
        public TableData AssetInspectsLoad(string AssetId)
        {
            var result = new TableData();
            try
            {
                return _app.AssetInspectsLoad(AssetId);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 加载操作列表
        /// </summary>
        [HttpGet]
        public async Task<TableData> AssetOperationsLoad(string AssetId)
        {
            var result = new TableData();
            try
            {
                return await _app.AssetOperationsLoad(AssetId);
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
