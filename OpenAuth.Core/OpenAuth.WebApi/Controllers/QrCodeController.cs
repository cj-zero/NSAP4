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
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
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

        public IConfiguration Configuration { get; }
        public QrCodeController(UserManagerApp app, IConfiguration configuration)
        {
            _app = app;
            Configuration = configuration;
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
            if (!CommonHelper.IsDebug)
            {
                url = HttpUtility.UrlEncode(Configuration.GetSection("QrcodeCallBack").Value + "/api/QrCode/SaveLoginState?rd=" + rd);
            }
            Qrcode qrcode = new Qrcode()
            {
                scene = "NSAPLogin",
                parameter = "callback=" + url
            };

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
                int appuserId = int.Parse(userid);
                //判断是否存在关联
                if (!_app.IsHaveNsapAccount(appuserId))
                {
                    response.Code = 204;
                    response.Message = "您还未开通nSAP访问权限";
                    return response;
                }
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
            string appUserId = RedisHelper.Get(rd);
            if (string.IsNullOrEmpty(appUserId))
            {
                response.Code = 205;
                response.Message = "用户未登录";
                return response;
            }
            try
            {
                //查询用户信息
                var token = _app.GetTokenByAppUserId(int.Parse(appUserId));
                if (string.IsNullOrEmpty(token))
                {
                    response.Code = 205;
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
