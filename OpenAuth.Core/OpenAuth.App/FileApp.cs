using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.HuaweiOBS;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAuth.App.Files;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;

namespace OpenAuth.App
{
    /// <summary>
    /// 文件
    /// </summary>
    public class FileApp : BaseApp<UploadFile>
    {
        private ILogger<FileApp> _logger;
        private string _filePath;
        private string _dbFilePath;   //数据库中的文件路径
        private string _dbThumbnail;   //数据库中的缩略图路径
        private IFileStore _fileStore;

        public FileApp( IOptions<AppSetting> setOptions, IUnitWork unitWork, IRepository<UploadFile> repository, ILogger<FileApp> logger, IAuth auth, IFileStore fileStore)
            :base(unitWork, repository, auth)
        {
            _logger = logger;
            _filePath = setOptions.Value.UploadPath;
            if (string.IsNullOrEmpty(_filePath))
            {
                _filePath = AppContext.BaseDirectory;
            }
            _fileStore = fileStore;
        }

        public async Task<List<UploadFileResp>> Add(IFormFileCollection files)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new List<UploadFileResp>();
            foreach (var file in files)
            {
                result.Add(await Add(file));
            }

            return result;
        }

        public async Task<UploadFileResp> Add(IFormFile file)
        {
            if (file != null)
            {
                _logger.LogInformation("收到新文件: " + file.FileName);
                _logger.LogInformation("收到新文件: " + file.Length);
            }
            else
            {
                _logger.LogWarning("收到新文件为空");
            }
            var uploadResult = await _fileStore.UploadFile(file);
            uploadResult.CreateUserName = _auth.GetCurrentUser().User.Name;
            uploadResult.CreateUserId = Guid.Parse(_auth.GetCurrentUser().User.Id);
            var a = await Repository.AddAsync(uploadResult);
            return uploadResult.MapTo<UploadFileResp>();
            //if (file != null && file.Length > 0 && file.Length < 10485760)
            //{
            //    using (var binaryReader = new BinaryReader(file.OpenReadStream()))
            //    {
            //        var fileName = Path.GetFileName(file.FileName);
            //        var data = binaryReader.ReadBytes((int)file.Length);
            //        UploadFile(fileName, data);

            //        var filedb = new UploadFile
            //        {
            //            FilePath = _dbFilePath,
            //            Thumbnail = _dbThumbnail,
            //            FileName = fileName,
            //            FileSize = file.Length,
            //            FileType = Path.GetExtension(fileName),
            //            Extension = Path.GetExtension(fileName)
            //        };
            //        Repository.Add(filedb);
            //        return filedb;
            //    }
            //}
            //else
            //{
            //    throw new Exception("文件过大");
            //}
        }

        public async Task<List<UploadFileResp>> Add(IFormFileCollection files,string bucketName)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new List<UploadFileResp>();
            foreach (var file in files)
            {

                if (file != null)
                {
                    _logger.LogInformation("收到新文件: " + file.FileName);
                    _logger.LogInformation("收到新文件: " + file.Length);
                }
                else
                {
                    _logger.LogWarning("收到新文件为空");
                }
                var uploadResult = await _fileStore.UploadFile(file, bucketName);
                uploadResult.CreateUserName = _auth.GetCurrentUser().User.Name;
                uploadResult.CreateUserId = Guid.Parse(_auth.GetCurrentUser().User.Id);
                var a = await Repository.AddAsync(uploadResult);
                result.Add(uploadResult.MapTo<UploadFileResp>());
            }

            return result;
        }

        private void UploadFile(string fileName, byte[] fileBuffers)
        {
            string folder = DateTime.Now.ToString("yyyyMMdd");

            //判断文件是否为空
            if (string.IsNullOrEmpty(fileName))
            {
                throw new Exception("文件名不能为空");
            }

            //判断文件是否为空
            if (fileBuffers.Length < 1)
            {
                throw new Exception("文件不能为空");
            }

            var uploadPath = Path.Combine(_filePath , folder );
            _logger.LogInformation("文件写入：" + uploadPath);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var ext = Path.GetExtension(fileName).ToLower();
            string newName = GenerateId.GenerateOrderNumber() + ext;

            using (var fs = new FileStream(Path.Combine(uploadPath , newName), FileMode.Create))
            {
                fs.Write(fileBuffers, 0, fileBuffers.Length);
                fs.Close();

                //生成缩略图
                if (ext.Contains(".jpg") || ext.Contains(".jpeg") || ext.Contains(".png") || ext.Contains(".bmp") || ext.Contains(".gif"))
                {
                    string thumbnailName = GenerateId.GenerateOrderNumber() + ext;
                    ImgHelper.MakeThumbnail(Path.Combine(uploadPath , newName), Path.Combine(uploadPath , thumbnailName));
                    _dbThumbnail = Path.Combine(folder , thumbnailName);
                }


                _dbFilePath = Path.Combine(folder , newName);
            }
        }

        /// <summary>
        /// 获取文件详情
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public async Task<UploadFile> GetFileAsync(string fileId)
        {
            var file = await Repository.Find(f => f.Id.Equals(fileId)).FirstOrDefaultAsync();
            if (file is null)
                return null;
            //file.FilePath = Path.Combine(_filePath, file.FilePath);
            //if (!string.IsNullOrWhiteSpace(file.Thu mbnail))
            //{
            //    file.Thumbnail = Path.Combine(_filePath, file.Thumbnail);
            //}
            return file;
        }
        /// <summary>
        /// 获取文件流
        /// </summary>
        /// <returns></returns>
        public async Task<Stream> GetFileStreamAsync(string bucketName, string fileName)
        {
            //file.FilePath = Path.Combine(_filePath, file.FilePath);
            //if (!string.IsNullOrWhiteSpace(file.Thumbnail))
            //{
            //    file.Thumbnail = Path.Combine(_filePath, file.Thumbnail);
            //}
            var stream = await _fileStore.DownloadFile(bucketName, fileName);
            return stream;
        }

        /// <summary>
        /// 判断桶中是否已经存在文件名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool IsExistsFileName(string fileName)
        {
            var obsHelper = new HuaweiOBSHelper();
            var response = obsHelper.IsExistsObject(fileName, null);

            return response;
        }

        /// <summary>
        /// 上传文件到华为云obs
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="version"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<UploadFileResp> UploadFileToHuaweiOBS(IFormFile file)
        {
            var obsHelper = new HuaweiOBSHelper();
            var fileName = "bts-rom/" + file.FileName;
            var stream = file?.OpenReadStream();
            var response = obsHelper.PutObject(fileName, null, stream, out string objectKey);
            var result = new UploadFileResp
            {
                FileName = objectKey,
                FilePath = response.ObjectUrl
            };

            return result;
        }

        /// <summary>
        /// 上传烤机工步文件到华为云obs
        /// </summary>
        /// <param name="file">工步文件</param>                                                                       
        /// <returns></returns>
        public async Task<UploadFileResp> UploadStepFileToHuaweiOBS(IFormFile file)
        {
            var obsHelper = new HuaweiOBSHelper();
            var fileName = "stepFile/" + file.FileName;
            var stream = file?.OpenReadStream();
            var response = obsHelper.PutObject(fileName, null, stream, out string objectKey);
            var result = new UploadFileResp
            {
                FileName = objectKey,
                FilePath = response.ObjectUrl
            };
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">上传路径</param>
        /// <param name="file">以文件形式上传</param>
        /// <param name="stream">以流形式上传</param>
        /// <returns></returns>
        public async Task<UploadFileResp> UploadFileToHuaweiOBS(string fileName, IFormFile file = null, Stream stream = null)
        {
            var obsHelper = new HuaweiOBSHelper();
            //var fileName = "stepFile/" + file.FileName;
            if (stream == null)
            {
                stream = file?.OpenReadStream();
            }
            var response = obsHelper.PutObject(fileName, null, stream, out string objectKey);
            var result = new UploadFileResp
            {
                FileName = objectKey,
                FilePath = response.ObjectUrl
            };
            return result;
        }

        /// <summary>
        /// 下载对象
        /// </summary>
        /// <param name="bucketName">桶容器名称</param>
        /// <param name="objectKey">文件对象关键字</param>
        /// <param name="filePath"></param>
        public void GetFileFromHuaweiOBS(string objectKey, string? bucketName, string filePath)
        {
            var obsHelper = new HuaweiOBSHelper();
            obsHelper.GetObject(objectKey, bucketName, filePath);
        }
    }
}