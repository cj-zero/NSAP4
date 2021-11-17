using Microsoft.AspNetCore.Http;
using NetOffice.WordApi;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Files
{
    public interface IFileStore
    {
        Task<UploadFile> UploadFile(IFormFile file, string bucketNames="");

        Task<Stream> DownloadFile(string bucketName, string fileName);
    }
}
