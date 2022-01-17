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
using OpenAuth.Repository.Domain.Serve;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Clue
{
    /// <summary>
    /// 线索意向商品
    /// </summary>
    [Route("api/Clue/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Clue")]
    public class ClueIntentionProductController : Controller
    {
        private readonly ClueApp _clueApp;
        private readonly FileApp _app;
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        public ClueIntentionProductController(IUnitWork UnitWork, FileApp app, ServiceBaseApp _serviceBaseApp, IAuth _auth, ClueApp clueApp)
        {
            this.UnitWork = UnitWork;
            this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            _clueApp = clueApp;
            this._app = app;
        }
        /// <summary>
        /// 线索意向产品列表
        /// </summary>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ClueIntentionProductByIdAsync")]
        public async Task<Response<List<ClueIntentionProduct>>> ClueIntentionProductByIdAsync(int ClueId)
        {
            var result = new Response<List<ClueIntentionProduct>>();
            try
            {
                result.Result = await _clueApp.ClueIntentionProductByIdAsync(ClueId);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 新增意向商品
        /// </summary>
        /// <param name="addClueIntentionProductReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddClueIntentionProductAsync")]
        public async Task<Response<bool>> AddClueIntentionProductAsync(List<AddClueIntentionProductReq> addClueIntentionProductReq)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _clueApp.AddClueIntentionProductAsync(addClueIntentionProductReq);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 删除线索意向产品
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("DeleteClueIntentionProductByIdAsync")]
        public async Task<Response<bool>> DeleteClueIntentionProductByIdAsync(List<int> Ids)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _clueApp.DeleteClueIntentionProductByIdAsync(Ids);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
    }
}
