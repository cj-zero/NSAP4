using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using OpenAuth.App.Dictionary;
using OpenAuth.App.Response;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.App.Request;

namespace OpenAuth.WebApi.Controllers.Dictionary
{
    /// <summary>
    /// 新威词典
    /// </summary>
    [Route("api/Dictionary/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Dictionary")]
    public class NewareDictionaryController : ControllerBase
    {
        IAuth _auth;
        IUnitWork UnitWork;
        private NewareDictionaryApp _newareDictionaryApp;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="UnitWork"></param>
        /// <param name="auth"></param>
        /// <param name="newareDictionaryApp"></param>
        public NewareDictionaryController(NewareDictionaryApp newareDictionaryApp, IUnitWork UnitWork, IAuth auth)
        {
            this.UnitWork = UnitWork;
            this._auth = auth;
            _newareDictionaryApp = newareDictionaryApp;
        }

        /// <summary>
        /// 获取新威词典信息列表
        /// </summary>
        /// <param name="request">新威词典查询实体数据</param>
        /// <returns>成功返回词典列表信息，失败返回异常信息</returns>
        [HttpPost]
        public async Task<TableData> GetNewareDictionaryList(QueryDictionaryReq request)
        {
            var result = new TableData();
            try
            {
                return await _newareDictionaryApp.GetNewareDictionaryList(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 新威词典详情
        /// </summary>
        /// <param name="id">词典id</param>
        /// <returns>成功返回词典详情信息，失败返回异常信息</returns>
        [HttpGet]
        public async Task<TableData> GetNewareDictionaryDetail(int id)
        {
            var result = new TableData();
            try
            {
                return await _newareDictionaryApp.GetNewareDictionaryDetail(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="obj">新威词典实体数据</param>
        /// <returns>成功返回操作成功，失败返回异常信息</returns>
        [HttpPost]
        public async Task<Response> Add(NewareDictionary obj)
        {
            var result = new Response();
            try
            {
                var Message = await _newareDictionaryApp.Add(obj);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="obj">新威词典实体数据</param>
        /// <returns>成功返回操作成功，失败返回异常信息</returns>
        [HttpPost]
        public async Task<Response> Update(NewareDictionary obj)
        {
            var result = new Response();
            try
            {
                var Message = await _newareDictionaryApp.UpDate(obj);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 删除词典
        /// </summary>
        /// <param name="id">词典id</param>
        /// <returns>成功返回操作成功，失败返回异常信息</returns>
        [HttpPost]
        public async Task<Response> DeleteDictionary(int id)
        {
            var result = new Response();
            try
            {
                var Message = await _newareDictionaryApp.DeleteDictionary(id);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 批量删除词典
        /// </summary>
        /// <param name="idList">词典Id集合</param>
        /// <returns>成功返回操作成功，失败返回异常信息</returns>
        [HttpPost]
        public async Task<Response> DeleteDictionarys(List<int> idList)
        {
            var result = new Response();
            try
            {
                var Message = await _newareDictionaryApp.DeleteDictionarys(idList);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{idList.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }


    }
}