using Infrastructure;
using System;
using System.Linq;

namespace OpenAuth.App.SSO
{
    public class AppInfoService 
    {
        private readonly AppManager _appManager;
        public AppInfo Get(string appKey)
        {
            var appInfos = _appManager.Get(a => a.AppKey.Equals(appKey));
            //可以从数据库读取
            return appInfos.MapTo<AppInfo>();
        }

        private AppInfo[] _applist = new[]
        {
            new AppInfo
            {
                AppKey = "openauth",
                Icon = "/Areas/SSO/Content/images/logo.png",
                Disable = true,
                Description = "基于DDDLite的权限管理系统",
                ReturnUrl = "http://localhost:56813",
                Name = "OpenAuth.Core",
                CreateTime = DateTime.Now,
            },
            new AppInfo
            {
                AppKey = "openauthtest",
                Icon = "/Areas/SSO/Content/images/logo.png",
                Disable = true,
                Description = "这只是个模拟的测试站点",
                ReturnUrl = "http://localhost:53050",
                Name = "OpenAuth.Core测试站点",
                CreateTime = DateTime.Now,
            }
        };

        public AppInfoService(AppManager appManager)
        {
            _appManager = appManager;
        }
    }
}