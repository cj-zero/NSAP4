using Infrastructure;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using OpenAuth.Repository.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.App;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 系统配置信息
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "General")]
    public class SysConfController :ControllerBase
    {
        private IUnitWork _UnitWork;
        private IOptions<AppSetting> _appConfiguration;

        public SysConfController(IOptions<AppSetting> appConfiguration, IUnitWork unitWork)
        {
            _appConfiguration = appConfiguration;
            _UnitWork = unitWork;
        }

        /// <summary>
        /// 是否Identity认证
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<AppSetHelp> IsIdentityAuth()
        {
            AppSetHelp appSetHelp = new AppSetHelp();
            appSetHelp.Result = _appConfiguration.Value.IsIdentityAuth;
            var vl = _UnitWork.Find<VersionsLog>(null).OrderByDescending(r => r.CreateTime).FirstOrDefault();
            if (vl != null)
            {
                appSetHelp.Version = vl.VersionsNumber;
                appSetHelp.VersionTime = vl.CreateTime.ToString("yyyy.MM.dd");
                appSetHelp.Code = 200;
                appSetHelp.Message = "操作成功";
            }

            return appSetHelp;
        }
    }
}
