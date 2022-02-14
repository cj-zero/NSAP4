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
using OpenAuth.Repository.Domain.Serve;

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
            var result = new Response<List<ClassificationDto>>();
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
        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetClassificationByIdAsync")]
        public async Task<Response<ClueClassification>> GetClassificationByIdAsync(int Id)
        {
            var result = new Response<ClueClassification>();
            try
            {
                result.Result = await _clueApp.GetClassificationByIdAsync(Id);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="updateClassificationReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateClassificationAsync")]
        public async Task<Response<string>> UpdateClassificationAsync(UpdateClassificationReq updateClassificationReq)
        {
            var result = new Response<string>();
            try
            {
                result.Result = await _clueApp.UpdateClassificationAsync(updateClassificationReq);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("DeleteClassificationByIdAsync")]
        public async Task<Response<string>> DeleteClassificationByIdAsync(int Id)
        {
            var result = new Response<string>();
            try
            {
                result.Result = await _clueApp.DeleteClassificationByIdAsync(Id);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 行业下拉
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("IndustryDropDownAsync")]
        public async Task<Response<List<ClassificationDto>>> IndustryDropDownAsync()
        {
            var result = new Response<List<ClassificationDto>>();
            try
            {
                result.Result = await _clueApp.IndustryDropDownAsync();
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
    }
}
