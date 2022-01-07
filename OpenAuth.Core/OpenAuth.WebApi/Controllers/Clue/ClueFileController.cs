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
    /// 线索附件
    /// </summary>
    [Route("api/Clue/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Clue")]
    public class ClueFileController : Controller
    {
        private readonly ClueApp _clueApp;
        private readonly FileApp _app;
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        public ClueFileController(IUnitWork UnitWork, FileApp app, ServiceBaseApp _serviceBaseApp, IAuth _auth, ClueApp clueApp)
        {
            this.UnitWork = UnitWork;
            this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            _clueApp = clueApp;
            this._app = app;
        }
        /// <summary>
        /// 附件列表
        /// </summary>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ClueFileByIdAsync")]
        public async Task<Response<List<ClueFileListDto>>> ClueFileByIdAsync(int ClueId)
        {
            var result = new Response<List<ClueFileListDto>>();
            try
            {
                result.Result = await _clueApp.ClueFileByIdAsync(ClueId);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        ///  批量上传文件
        /// </summary>
        /// <param name="files"></param>
        /// <returns>服务器存储的文件信息</returns>
        [HttpPost]
        [Route("ClueFileUpload")]
        public async Task<Response<IList<UploadFileResp>>> ClueFileUpload([FromForm] IFormFileCollection files)
        {
            var result = new Response<IList<UploadFileResp>>();
            try
            {
                result.Result = await _app.Add(files);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                //Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        ///  批量上传保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns>服务器存储的文件信息</returns>Fsave
        [HttpPost]
        [Route("AddClueFileUploadAsync")]
        public async Task<Response<bool>> AddClueFileUploadAsync(AddClueFileUploadReq addClueFileUploadReq)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _clueApp.AddClueFileUploadAsync(addClueFileUploadReq);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                //Log.Logger.Error($"地址：{Request.Path}，参数：'', 错误：{result.Message}");
            }
            return result;
        }
    }
}
