using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using OpenAuth.App;
using OpenAuth.App.Interface;
using Quartz.Impl.Calendar;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 二维码
    /// </summary>
    [Route("api/[controller]/[action]")]
    public class QrCodeController : ControllerBase
    {
        private readonly UserManagerApp _app;

        public QrCodeController(UserManagerApp app)
        {
            _app = app;
        }

        /// <summary>
        /// 获取二维码
        /// </summary>
        /// <param name="rd">随机数</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get(string rd)
        {
            //获取当前服务地址及端口
            string url = "http://localhost:" + Request.HttpContext.Connection.LocalPort;
            url = HttpUtility.UrlEncode(url + "/l/r=" + rd);
            Qrcode qrcode = new Qrcode()
            {
                scene = "NWLogin",
                parameter = "callback=" + url
            };
            Response.ContentType = "image/jpeg";

            var bitmap = QRCoderHelper.GetPTQRCode(JsonConvert.SerializeObject(qrcode), 5);
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Jpeg);
            return File(ms.ToArray(), "image/png");
        }

        /// <summary>
        /// 扫码成功保存登录状态
        /// </summary>
        /// <param name="userid">app用户Id</param>
        /// <param name="rd">随机数</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public Response SaveLoginState(string userid, string rd)
        {
            var response = new Response();
            try
            {
                //将信息存到redis中 5分钟有效期
                RedisHelper.Set(rd, userid, new TimeSpan(0, 0, 5, 0));
            }
            catch (Exception ex)
            {
                response.Code = 500;
                response.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return response;
        }

        /// <summary>
        /// 验证登录状态
        /// </summary>
        /// <param name="rd">二维码随机码</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public Response<string> ValidateLoginState(string rd)
        {
            var response = new Response<string>();
            int appUserId = int.Parse(RedisHelper.Get(rd));
            if (appUserId <= 0)
            {
                response.Code = 500;
                response.Message = "用户未登录";
                return response;
            }
            try
            {
                //查询用户信息
                var token = _app.GetTokenByAppUserId(appUserId);
                if (string.IsNullOrEmpty(token))
                {
                    response.Code = 500;
                    response.Message = "用户未登录";
                    return response;
                }
                response.Result = token;
            }
            catch (Exception ex)
            {
                response.Code = 500;
                response.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return response;
        }
    }
}
