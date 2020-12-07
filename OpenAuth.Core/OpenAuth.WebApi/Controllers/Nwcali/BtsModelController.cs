using System;
using System.Collections.Generic;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// btsmodel操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BtsModelController : ControllerBase
    {
        private readonly BtsModelApp _app;

        //添加
        [HttpPost]
        [Authorize(Roles = "BtsModelAdmin")]
        public Response Add(AddOrUpdatebtsmodelReq obj)
        {
            var result = new Response();
            _app.Add(obj);


            return result;
        }

        //添加
        [HttpPost]
        [Authorize(Roles = "BtsModelAdmin")]
        public Response BatchAdd(List<BtsModel> objs)
        {
            var result = new Response();
            _app.BatchAdd(objs);


            return result;
        }

        //修改
        [HttpPost]
        [Authorize(Roles = "BtsModelAdmin")]
        public Response Update(AddOrUpdatebtsmodelReq obj)
        {
            var result = new Response();
            _app.Update(obj);


            return result;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public TableData Load([FromQuery] QuerybtsmodelListReq request)
        {
            return _app.Load(request);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "BtsModelAdmin")]
        public Response Delete([FromBody] List<int> ids)
        {
            var result = new Response();
            _app.Delete(ids);

            return result;
        }

        public BtsModelController(BtsModelApp app)
        {
            _app = app;
        }
    }
}
