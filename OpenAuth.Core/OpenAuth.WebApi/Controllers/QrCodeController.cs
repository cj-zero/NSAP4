using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain.Sap;
using Quartz.Impl.Calendar;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 二维码
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "General")]
    public class QrCodeController : ControllerBase
    {
        private readonly UserManagerApp _app;
        private readonly HttpClienService _httpClienService;

        public IConfiguration Configuration { get; }
        public QrCodeController(UserManagerApp app, IConfiguration configuration,
            HttpClienService httpClienService)
        {
            _app = app;
            Configuration = configuration;
            this._httpClienService = httpClienService;
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
        /// 从passport获取二维码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public Response<Dictionary<string, string>> GetV2()
        {
            var response = new Response<Dictionary<string, string>>();
            //获取当前服务地址及端口
            string qrUrl = "http://passport.neware.work/api/Account2/getLoginQRCode";
            var resultJson = HttpGet(qrUrl);
            var result = JsonConvert.DeserializeObject<dynamic>(resultJson);
            if (result.code != "200")
            {
                response.Code = 500;
                response.Message = "passport请求失败";
                return response;
            }
            string codeId = result.data.codeId;
            string qrcode = result.data.codeContent;
            var bitmap = QRCoderHelper.GetPTQRCode(qrcode, 5);
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Jpeg);
            var bas64str = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("img", bas64str);
            dic.Add("codeId", codeId);
            response.Result = dic;
            return response;
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
                    response.Message = "您还未绑定Erp4.0帐号";
                    return response;
                }
                //将信息存到redis中 5分钟有效期
                RedisHelper.Set(rd, userid, new TimeSpan(0, 0, 5, 0));
            }
            catch (Exception ex)
            {
                response.Code = 500;
                response.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{userid},{rd}， 错误：{response.Message}");
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{rd}， 错误：{response.Message}");
            }

            return response;
        }
        /// <summary>
        /// 验证登录状态
        /// </summary>
        /// <param name="codeId">二维码随机码</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public Response<Dictionary<string, string>> ValidateLoginStateV2(string codeId)
        {
            var response = new Response<Dictionary<string, string>>();
            string appUserId = string.Empty;
            string url = $"http://passport.neware.work/api/Account2/ValidateLoginQRCode?codeId={codeId}";
            var resultjson = HttpGet(url);
            var result = JsonConvert.DeserializeObject<dynamic>(resultjson);
            if (result.code != "200")
            {
                response.Code = 205;
                response.Message = "用户未登录";
                return response;
            }
            
            int passportId = result.data.passportId;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string passportToken = result.data.identityToken;
            dic.Add("passportToken", passportToken);
            try
            {
                //查询用户信息
                var token = _app.GetTokenByPassportId(passportId);
                if (string.IsNullOrEmpty(token))
                {
                    response.Code = 205;
                    response.Message = "用户未登录";
                    return response;
                }
                dic.Add("erpToken", token);
                response.Result = dic;
            }
            catch (Exception ex)
            {
                response.Code = 500;
                response.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{codeId}， 错误：{response.Message}");
            }

            return response;
        }

        private string HttpGet(string url)
        {
            string result;
            //string url = $"https://passport.neware.work/v/?rm={rd}";
            try
            {
                var webClient = new WebClient { Encoding = Encoding.UTF8 };

                result = webClient.DownloadString(url);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }
    }
}

