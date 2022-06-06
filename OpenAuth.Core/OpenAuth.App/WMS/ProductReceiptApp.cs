using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using OpenAuth.App.WMS.Request;
using OpenAuth.Repository.Interface;
using OpenAuth.App.Workbench;
using Microsoft.Extensions.Options;
using OpenAuth.App.Interface;

namespace OpenAuth.App.WMS
{
    /// <summary>
    /// WMS生产收货操作
    /// </summary>
    public class ProductReceiptApp: OnlyUnitWorkBaeApp
    {
        private ICapPublisher _capBus;
        private readonly FlowInstanceApp _flowInstanceApp;
        private readonly IOptions<AppSetting> _appConfiguration;
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        public readonly WorkbenchApp _workbenchApp;
        private readonly OrgManagerApp _orgApp;
        private readonly UserManagerApp _userManagerApp;
        /// <summary>
        /// WMS同步生产收货
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task AddProductReceipt(AddOrUpdProductReceiptReq obj)
        {
            _capBus.Publish("WMS.ProductReceipt.Create", obj);
            await UnitWork.SaveAsync();
        }

        public ProductReceiptApp(IUnitWork unitWork, ICapPublisher capBus, FlowInstanceApp flowInstanceApp, WorkbenchApp workbenchApp,
           ModuleFlowSchemeApp moduleFlowSchemeApp, IOptions<AppSetting> appConfiguration, IAuth auth, OrgManagerApp orgApp,
           UserManagerApp userManagerApp) : base(unitWork, auth)
        {
            _appConfiguration = appConfiguration;
            _flowInstanceApp = flowInstanceApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _capBus = capBus;
            _workbenchApp = workbenchApp;
            _orgApp = orgApp;
            _userManagerApp = userManagerApp;
        }
    }
}
