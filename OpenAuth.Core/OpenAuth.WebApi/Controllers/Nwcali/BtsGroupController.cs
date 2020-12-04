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
    /// btsgroup操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BtsGroupController : ControllerBase
    {
        private readonly BtsGroupApp _app;


        //添加
        [HttpPost]
        public Response Add(AddOrUpdatebtsgroupReq obj)
        {
            var result = new Response();
            _app.Add(obj);


            return result;
        }

        //修改
        [HttpPost]
        public Response Update(AddOrUpdatebtsgroupReq obj)
        {
            var result = new Response();
            _app.Update(obj);

            return result;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public TableData Load([FromQuery] QuerybtsgroupListReq request)
        {
            return _app.Load(request);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        [HttpPost]
        public Response Delete([FromBody] List<int> ids)
        {
            var result = new Response();
            _app.Delete(ids);

            return result;
        }

        public BtsGroupController(BtsGroupApp app)
        {
            _app = app;
        }
    }
}
