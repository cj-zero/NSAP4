using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Excel;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 知识库操作
    /// </summary>
    [Route("api/Serve/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Serve")]
    public class KnowledgeBasesController : ControllerBase
    {
        private readonly KnowledgeBaseApp _app;

        //获取详情
        [HttpGet]
        public Response<KnowledgeBase> Get(string id)
        {
            var result = new Response<KnowledgeBase>();
            try
            {
                result.Result = _app.Get(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id}, 错误：{result.Message}");
            }

            return result;
        }

        //添加
        [HttpPost]
        public Response Add(KnowledgeBase obj)
        {
            var result = new Response();
            _app.Add(obj);

            return result;
        }

        //修改
        [HttpPost]
        public Response Update(KnowledgeBase obj)
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public async Task<TableData> Load([FromQuery] QueryKnowledgeBaseListReq request)
        {
            return await _app.Load(request);
        }

        /// <summary>
        /// 加载列表(树型)
        /// </summary>
        [HttpGet]
        public async Task<Response<IEnumerable<TreeItem<KnowledgeBase>>>> LoadTree([FromQuery] QueryKnowledgeBaseListReq request)
        {
            return await _app.LoadTree(request);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        [HttpPost]
        public Response Delete([FromBody] string[] ids)
        {
            var result = new Response();
            try
            {
                _app.Delete(ids);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ids.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }


        /// <summary>
        /// 加载新知识库列表
        /// </summary>
        [HttpGet]
        public async Task<TableData> NewLoad([FromQuery]QueryKnowledgeBaseListReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.NewLoad(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }
        /// <summary>
        /// 添加新知识库
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> NewAdd(KnowledgeBase obj)
        {
            var result = new Response();
            try
            {
                await _app.NewAdd(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }
        /// <summary>
        /// 修改新知识库
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> NewUpdate(KnowledgeBase obj)
        {
            var result = new Response();
            try
            {
                await _app.NewUpdate(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }
        /// <summary>
        /// 删除新知识库
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> NewDelete(QueryKnowledgeBaseListReq request)
        {
            var result = new Response();
            try
            {
                await _app.NewDelete(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }
        /// <summary>
        /// 导入知识库
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> ImportRepository()
        {
            var result = new Response();
            try
            {
                var file = Request.Form.Files[0];
                var handler = new ExcelHandler(file.OpenReadStream());
                await _app.ImportRepository(handler);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
        public KnowledgeBasesController(KnowledgeBaseApp app)
        {
            _app = app;
        }
    }
}
