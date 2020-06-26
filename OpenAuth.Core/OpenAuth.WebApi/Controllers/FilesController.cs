﻿
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

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>  文件管理</summary>
    /// <remarks>   yubaolee, 2019-03-08. </remarks>

    [Route("api/[controller]/[action]")]
    [ApiController]
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
            result.data = objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select($"new ({propertyStr})");
            result.count = objs.Count();
            return result;
        }
        /// <summary>
        ///  批量上传文件接口
        /// </summary>
        /// <param name="files"></param>
        /// <returns>服务器存储的文件信息</returns>
        [HttpPost]
        [AllowAnonymous]
        public Response<IList<UploadFile>> Upload(IFormFileCollection files)
        {
            var result = new Response<IList<UploadFile>>();
            try
            {
                result.Result = _app.Add(files);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }

            return result;
        }
        [HttpGet("{dirName}/{fileName}")]
        [AllowAnonymous]
        public IActionResult Download(string dirName, string fileName)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, dirName, fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound($"fileName:{fileName} is not exists");
            var f = new FileExtensionContentTypeProvider();
            f.TryGetContentType(filePath, out string contentType);
            var fileStream = new FileStream(filePath, FileMode.Open);
            return File(fileStream, contentType);
        }
        [HttpGet("{fileId}")]
        [AllowAnonymous]
        public async Task<IActionResult> Download(string fileId, bool isThumbnail)
        {
            var file = await _app.GetFileAsync(fileId);
            
            var filePath = file.FilePath;
            if (isThumbnail)
                filePath = file.Thumbnail;
            if (!System.IO.File.Exists(filePath))
                return NotFound($"fileName:{file.FileName} is not exists");
            var f = new FileExtensionContentTypeProvider();
            f.TryGetContentType(filePath, out string contentType);
            var fileStream = new FileStream(filePath, FileMode.Open);
            return File(fileStream, contentType);
        }
    }
}
