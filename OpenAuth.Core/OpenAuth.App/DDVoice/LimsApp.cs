using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Infrastructure;
using System.Threading.Tasks;
using OpenAuth.App.DDVoice.Common;
using OpenAuth.App.DDVoice.EntityHelp;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using OpenAuth.Repository.Domain;
using Infrastructure.Extensions;
using OpenAuth.App.Response;
using Infrastructure.Cache;

namespace OpenAuth.App.DDVoice
{
    public class LimsApp : OnlyUnitWorkBaeApp
    {
        private ILogger<LimsApp> _logger;
        private IUnitWork _UnitWork;
        private IAuth _auth;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public LimsApp(ILogger<LimsApp> logger, IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
            _logger = logger;
            _UnitWork = unitWork;
            _auth = auth;
        }

        /// <summary>
        /// 设置lims服务
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> SetLimsSer()
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //获取lims服务token
            string token = await GetAccessToken();
            if(string.IsNullOrEmpty(token))
            {
                result.Code = 500;
                result.Message = "Limis获取token失败";
                return result;
            }

            //获取passportId
            int? passportId = await UnitWork.Find<AppUserMap>(r => r.UserID == loginContext.User.Id).Select(r => r.PassPortId).FirstOrDefaultAsync();
            if (passportId == null)
            {
                result.Code = 500;
                result.Message = "当前用户PassPortId为空";
                return result;
            }
            else
            {
                LimsServerHelp limsServerHelp = new LimsServerHelp() { enterpriseId = 142, passportId = passportId };
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Authorization", "Bearer " + token);
               
                //调用lims服务接口
                LimsEntityResultHelp tokenResult = JsonConvert.DeserializeObject<LimsEntityResultHelp>(HttpHelpers.HttpPostAsync($"https://passport.neware.work/api/Account2/LoginFromServer",JsonConvert.SerializeObject(limsServerHelp), "application/json", 30, headers).Result);
                if (tokenResult.success)
                {
                    result.Data = tokenResult.data;
                    result.Code = 200;
                    result.Message = "设置成功";
                }
                else
                {
                    result.Code = 500;
                    result.Message = tokenResult.message;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取accesstoken
        /// </summary>
        /// <returns>成功返回token，失败返回空</returns>
        public async Task<string> GetAccessToken()
        {
            AccessHelp accessHelp = new AccessHelp() { clientId = "ERP3", clientSecret = "4547e48280b611ebb4e6484d7ea1663f", scope = "api" };

            //调用limis接口获取token
            AccessResultHelp tokenResult = JsonConvert.DeserializeObject<AccessResultHelp>(HttpHelpers.HttpPostAsync($"https://passport.neware.work/api/Account/GetAccessToken", JsonConvert.SerializeObject(accessHelp)).Result);

            //获取成功返回token，否则返回空
            if (tokenResult.success)
            {
                return tokenResult.data;
            }
            else
            {
                return "";
            }
        }
    }
}
