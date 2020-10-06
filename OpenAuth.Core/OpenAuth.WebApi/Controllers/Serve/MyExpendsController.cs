using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;

namespace OpenAuth.WebApi.Controllers.Serve
{
    /// <summary>
    /// 我的费用
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    public class MyExpendsController : ControllerBase
    {
        private readonly MyExpendsApp _app;

        public MyExpendsController(MyExpendsApp app)
        {
            _app = app;
        }

        /// <summary>
        /// 查看我的费用
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Load([FromQuery] QueryMyExpendsListReq obj)
        {
            var result = new TableData();
            try
            {
                return await _app.Load(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="MyExpendsId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Details(int MyExpendsId)
        {
            var result = new TableData();
            try
            {
                return await _app.Details(MyExpendsId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 添加我的费用
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Add(AddOrUpdateMyExpendsReq obj)
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
            }

            return result;
        }

        /// <summary>
        /// 修改我的费用
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Update(AddOrUpdateMyExpendsReq obj)
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
            }

            return result;
        }


        /// <summary>
        /// 删除我的费用
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Delete(List<int> ids)
        {
            var result = new Response();
            try
            {
                await _app.Delete(ids);
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
