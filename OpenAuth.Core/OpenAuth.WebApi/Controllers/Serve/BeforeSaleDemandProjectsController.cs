using System;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// BeforeSaleDemandProject操作
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Serve")]
    public class BeforeSaleDemandProjectsController : ControllerBase
    {
        private readonly BeforeSaleDemandProjectApp _app;

        ////获取详情
        ///
        [HttpGet]
        public async Task<TableData> GetDetails(int id)
        {
            return await _app.GetDetails(id);
        }


        /// <summary>
        /// 获取项目进度甘特图数据
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<TableData>> GetGanttData([FromQuery] QueryBeforeSaleDemandProjectListReq query)
        {
            var result = new Response<TableData>();
            try
            {
                result.Result = await _app.GetGanttData(query);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{query.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }

        //添加
        [HttpPost]
        public Response Add(AddOrUpdateBeforeSaleDemandProjectReq obj)
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

        //修改
       [HttpPost]
        public Response Update(AddOrUpdateBeforeSaleDemandProjectReq obj)
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
        /// 加载列表
        /// </summary>
        [HttpGet]
        public async Task<TableData> Load([FromQuery]QueryBeforeSaleDemandProjectListReq request)
        {
            return await _app.Load(request);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
       [HttpPost]
        public Response Delete([FromBody]string[] ids)
        {
            var result = new Response();
            try
            {
                //_app.Delete(ids);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        public BeforeSaleDemandProjectsController(BeforeSaleDemandProjectApp app) 
        {
            _app = app;
        }
    }
}
