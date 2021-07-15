using DotNetCore.CAP;
using Infrastructure;
using Microsoft.Extensions.Options;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Order
{
    /// <summary>
    /// 销售订单业务
    /// </summary>
    public class ServiceSaleOrderApp : OnlyUnitWorkBaeApp
    {
        private readonly RevelanceManagerApp _revelanceApp;
        private readonly AppServiceOrderLogApp _appServiceOrderLogApp;
        private readonly ServiceOrderLogApp _ServiceOrderLogApp;
        private IOptions<AppSetting> _appConfiguration;
        private ICapPublisher _capBus;
        private readonly ServiceFlowApp _serviceFlowApp;
        public ServiceSaleOrderApp(IUnitWork unitWork, RevelanceManagerApp app, ServiceOrderLogApp serviceOrderLogApp, IAuth auth, AppServiceOrderLogApp appServiceOrderLogApp, IOptions<AppSetting> appConfiguration, ICapPublisher capBus, ServiceOrderLogApp ServiceOrderLogApp, ServiceFlowApp serviceFlowApp) : base(unitWork, auth)
        {
            _appConfiguration = appConfiguration;
            _revelanceApp = app;
            _appServiceOrderLogApp = appServiceOrderLogApp;
            _capBus = capBus;
            _ServiceOrderLogApp = ServiceOrderLogApp;
            _serviceFlowApp = serviceFlowApp;
        }
        /// <summary>
        /// 获取业务员信息
        /// </summary>
        /// <param name="sboId"></param>
        /// <returns></returns>
        public List<SelectOption> GetSalesSelect(int sboId)
        {
            var loginContext = _auth.GetCurrentUser();
            //if (loginContext == null)
            //{
            //    throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            //}
            //业务员Id
            var selectOption = UnitWork.Find<crm_oslp>(null).Select(zw => new SelectOption { Key = zw.SlpCode.ToString(), Option = zw.SlpName }).ToList();
            return selectOption;
        }
    }
}
