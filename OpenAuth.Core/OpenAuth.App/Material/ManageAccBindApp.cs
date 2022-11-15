using Microsoft.Extensions.Logging;
using OpenAuth.App.ClientRelation;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material
{
    public class ManageAccBindApp : OnlyUnitWorkBaeApp
    {
        ServiceBaseApp _serviceBaseApp;
        private ILogger<ManageAccBindApp> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public ManageAccBindApp(IUnitWork unitWork, IAuth auth, ILogger<ManageAccBindApp> logger, ServiceBaseApp serviceBaseApp) : base(unitWork, auth)
        {
            _serviceBaseApp = serviceBaseApp;
            _logger = logger;
        }




    }

}
