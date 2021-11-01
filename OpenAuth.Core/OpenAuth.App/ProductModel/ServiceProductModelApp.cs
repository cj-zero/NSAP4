using AutoMapper;
using Microsoft.Extensions.Logging;
using OpenAuth.App.Interface;
using OpenAuth.App.ProductModel.Request;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.ProductModel
{
    /// <summary>
    /// 产品选型
    /// </summary>
    public class ServiceProductModelApp : OnlyUnitWorkBaeApp
    {
        private IMapper _mapper;
        private readonly RevelanceManagerApp _revelanceApp;
        private readonly AppServiceOrderLogApp _appServiceOrderLogApp;
        private readonly ServiceOrderLogApp _ServiceOrderLogApp;
        private readonly ServiceFlowApp _serviceFlowApp;
        ServiceBaseApp _serviceBaseApp;
        private ILogger<ServiceProductModelApp> _logger;

        public ServiceProductModelApp(IMapper mapper, IUnitWork unitWork, ILogger<ServiceProductModelApp> logger, RevelanceManagerApp app, ServiceBaseApp serviceBaseApp, ServiceOrderLogApp serviceOrderLogApp, IAuth auth, AppServiceOrderLogApp appServiceOrderLogApp, ServiceOrderLogApp ServiceOrderLogApp, ServiceFlowApp serviceFlowApp) : base(unitWork, auth)
        {
            _logger = logger;
            _appServiceOrderLogApp = appServiceOrderLogApp;
            _ServiceOrderLogApp = ServiceOrderLogApp;
            _serviceFlowApp = serviceFlowApp;
            _serviceBaseApp = serviceBaseApp;
            _mapper = mapper;




        }

        public dynamic Load(ProductModelReq queryModel, out int rowcount)
        {
            throw new NotImplementedException();
        }
    }
}
