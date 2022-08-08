using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Hr;
using OpenAuth.App.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NSAP.App.WebApi.Controllers.NewareClassroom
{
    /// <summary>
    ///  职工申请
    /// </summary>
    [ApiController]
    public class EmployeeApplyController : BaseController
    {
        private EmployeeApplyApp _app;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        public EmployeeApplyController(EmployeeApplyApp app)
        {
            _app = app;
        }


        /// <summary>
        /// 员工加入申请
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> EmployeeApply(EmployeeApplyReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.EmployeeApply(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

       
        /// <summary>
        /// 个人简历上传
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> UploadResume(IFormFile file)
        {
            var result = new TableData();
            try
            {
                if (file is null)
                {
                    result.Code = 500;
                    result.Message = "文件为空";
                    return result;
                }

                //文件后缀
                var fileExtension = Path.GetExtension(file.FileName);

                //判断后缀是否是pdf
                if (fileExtension == null)
                {
                    result.Code = 500;
                    result.Message = "上传的文件没有后缀";
                    return result;
                }
                const string fileFilt = ".pdf";
                if (fileFilt.IndexOf(fileExtension.ToLower(), StringComparison.Ordinal) <= -1)
                {
                    result.Code = 500;
                    result.Message = "请上传pdf格式的文档";
                    return result;
                }
                //判断文件大小    
                long length = file.Length;
                if (length > 1024 * 1024 * 10)
                {
                    result.Code = 500;
                    result.Message = "上传的文件不能大于10M";
                    return result;
                }
                result = await _app.UploadResumeToHuaweiOBS(file);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }











    }
}
