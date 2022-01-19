
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>  文件管理</summary>
    /// <remarks>   yubaolee, 2019-03-08. </remarks>

    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "General")]
    public class FilesController :ControllerBase
    {

        private readonly FileApp _app;
        private readonly IAuth _auth;
        private readonly IUnitWork UnitWork;

        public FilesController(FileApp app, IAuth auth, IUnitWork unitWork)
        {
            _app = app;
            _auth = auth;
            UnitWork = unitWork;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public async Task<TableData> LoadAsync([FromQuery]QueryFileListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("uploadfile");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<UploadFile>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id.Contains(request.key));
            }

            var propertyStr = string.Join(',', properties.Select(u => u.Key));
            result.columnHeaders = properties;
            result.Data = objs.OrderByDescending(u => u.CreateTime)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select($"new ({propertyStr})");
            result.Count = objs.Count();
            return result;
        }
        /// <summary>
        ///  批量上传文件接口
        /// </summary>
        /// <param name="files"></param>
        /// <returns>服务器存储的文件信息</returns>
        [HttpPost]
        //[AllowAnonymous]
        public async Task<Response<IList<UploadFileResp>>> Upload([FromForm]IFormFileCollection files)
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
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="bucketNames"></param>
        /// <returns></returns>
        [HttpPost]
        //[AllowAnonymous]
        public async Task<Response<IList<UploadFileResp>>> UploadFor([FromForm] IFormFileCollection files)
        {
            var result = new Response<IList<UploadFileResp>>();
            try
            {
                var bucketNames = Request.Form["bucketNames"];
                result.Result = await _app.Add(files, bucketNames);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        ///  App批量上传文件接口
        /// </summary>
        /// <param name="files"></param>
        /// <returns>服务器存储的文件信息</returns>
        [HttpPost]
        //[AllowAnonymous]
        public async Task<Response<IList<UploadFileResp>>> AppUpload()
        {
            var files = Request.Form.Files;
            var result = new Response<IList<UploadFileResp>>();
            try
            {
                if (files.Count() <= 0)
                {
                    result.Code = 500;
                    result.Message = "传入文件为空，请检查。";
                }
                else 
                {
                    result.Result = await _app.Add(files);
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }

            return result;
        }

        [HttpGet("{dirName}/{fileName}")]
        public IActionResult Download(string dirName, string fileName)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, dirName, fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound($"fileName:{fileName} is not exists");
            var f = new FileExtensionContentTypeProvider();
            f.TryGetContentType(filePath, out string contentType);
            if (string.IsNullOrWhiteSpace(contentType))
                contentType = "application/octet-stream";
            var fileStream = new FileStream(filePath, FileMode.Open);
            return File(fileStream, contentType, fileName);
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> Download(string fileId, bool isThumbnail)
        {
            var file = await _app.GetFileAsync(fileId);
            if(file is null)
                return NotFound($"fileId:{fileId} is not exists");
            //var filePath = file.FilePath;
            //if (isThumbnail)
            //    filePath = file.Thumbnail;
            //if (!System.IO.File.Exists(filePath))
            //    return NotFound($"fileName:{file.FileName} is not exists");

            var f = new FileExtensionContentTypeProvider();
            f.TryGetContentType(file.FileName, out string contentType);
            if (string.IsNullOrWhiteSpace(contentType))
                contentType = "application/octet-stream";
            var fileStream = await _app.GetFileStreamAsync(file.BucketName, file.FilePath);
            return File(fileStream, contentType);//, file.FileName
        }

        /// <summary>
        /// 无鉴权下载
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="isThumbnail"></param>
        /// <returns></returns>
        [HttpGet("{fileId}")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadAllow(string fileId)
        {
            var file = await _app.GetFileAsync(fileId);
            if (file is null)
                return NotFound($"fileId:{fileId} is not exists");
            //var filePath = file.FilePath;
            //if (isThumbnail)
            //    filePath = file.Thumbnail;
            //if (!System.IO.File.Exists(filePath))
            //    return NotFound($"fileName:{file.FileName} is not exists");

            var f = new FileExtensionContentTypeProvider();
            f.TryGetContentType(file.FileName, out string contentType);
            if (string.IsNullOrWhiteSpace(contentType))
                contentType = "application/octet-stream";
            var fileStream = await _app.GetFileStreamAsync(file.BucketName, file.FilePath);
            return File(fileStream, contentType);//, file.FileName
        }

        /// <summary>
        /// 上传文件到华为云obs
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="version"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableRequestSizeLimit] 
        public async Task<List<UploadFileResp>> UploadFileToHuaweiOBS([FromForm] string prefix, [FromForm] string version, IFormFile file)
        {
            var result = await _app.UploadFileToHuaweiOBS(prefix, version, file);
            return result;
        }
    }
}
