using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Clue.ModelDto;
using OpenAuth.App.Clue.Request;
using OpenAuth.App.Interface;
using OpenAuth.App.Meeting.ModelDto;
using OpenAuth.App.ProductModel.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Clue
{
    /// <summary>
    /// 线索分类字典
    /// </summary>
    [Route("api/Clue/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Clue")]
    public class ClueClassificationController : Controller
    {
        private readonly ClueApp _clueApp;
        private readonly FileApp _app;
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        public ClueClassificationController(IUnitWork UnitWork, FileApp app, ServiceBaseApp _serviceBaseApp, IAuth _auth, ClueApp clueApp)
        {
            this.UnitWork = UnitWork;
            this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            _clueApp = clueApp;
            this._app = app;
        }
        /// <summary>
        /// 分类字典列表
        /// </summary>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ClassificationAsync")]
        public async Task<Response<List<ClassificationDto>>> ClassificationAsync()
        {
            var result = new Response<List<ClassificationDto>> ();
            try
            {
                result.Result = await _clueApp.ClassificationAsync();
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="addClassificationReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddClassificationAsync")]
        public async Task<Response<string>> AddClassificationAsync(AddClassificationReq addClassificationReq)
        {
            var result = new Response<string>();
            try
            {
                result.Result = await _clueApp.AddClassificationAsync(addClassificationReq);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
    }
}
