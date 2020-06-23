using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class ModuleFlowSchemeApp: BaseApp<ModuleFlowScheme>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryModuleFlowSchemeListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("moduleflowscheme");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<ModuleFlowScheme>(null).Include(m => m.Module).Include(m => m.FlowScheme).Select(s => new { s.Id, s.ModuleId, ModuleName = s.Module.Name, s.FlowSchemeId, s.FlowScheme.SchemeName });
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id.Contains(request.key));
            }
            properties.RemoveAll(p => p.Key.Equals("Module") || p.Key.Equals("FlowScheme"));
            properties.Add(new KeyDescription() { Key = "ModuleName", Browsable = true, Description = "模块名称", Type = "String" });
            properties.Add(new KeyDescription() { Key = "SchemeName", Browsable = true, Description = "流程名称", Type = "String" });

            var propertyStr = string.Join(',', properties.Select(u => u.Key));
            result.columnHeaders = properties;
            result.data = objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select($"new ({propertyStr},ModuleName,SchemeName)");
            result.count = objs.Count();
            return result;
        }

        public void Add(AddOrUpdateModuleFlowSchemeReq req)
        {
            var obj = req.MapTo<ModuleFlowScheme>();
            //todo:补充或调整自己需要的字段
            var user = _auth.GetCurrentUser().User;
            Repository.Add(obj);
        }
        public async Task AddAsync(AddOrUpdateModuleFlowSchemeReq req, CancellationToken cancellationToken = default)
        {
            var obj = req.MapTo<ModuleFlowScheme>();
            //todo:补充或调整自己需要的字段
            var user = _auth.GetCurrentUser().User;
            await Repository.AddAsync(obj, cancellationToken);
        }

        public void Update(AddOrUpdateModuleFlowSchemeReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<ModuleFlowScheme>(u => u.Id == obj.Id, u => new ModuleFlowScheme
            {
                //todo:补充或调整自己需要的字段
            });

        }

        public ModuleFlowSchemeApp(IUnitWork unitWork, IRepository<ModuleFlowScheme> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}