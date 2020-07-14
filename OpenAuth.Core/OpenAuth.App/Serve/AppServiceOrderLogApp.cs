using System;
using System.Linq;
using Infrastructure;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class AppServiceOrderLogApp : BaseApp<AppServiceOrderLog>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryAppServiceOrderLogListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("appserviceorderlog");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<AppServiceOrderLog>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id.Contains(request.key));
            }


            var propertyStr = string.Join(',', properties.Select(u => u.Key));
            result.columnHeaders = properties;
            result.data = objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select($"new ({propertyStr})");
            result.count = objs.Count();
            return result;
        }

        public void Add(AddOrUpdateAppServiceOrderLogReq req)
        {
            var obj = req.MapTo<AppServiceOrderLog>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            Repository.Add(obj);
        }

         public void Update(AddOrUpdateAppServiceOrderLogReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<AppServiceOrderLog>(u => u.Id == obj.Id, u => new AppServiceOrderLog
            {
                Title = obj.Title,
                Details = obj.Details,
                CreateTime = obj.CreateTime,
                CreateUserId = obj.CreateUserId,
                CreateUserName = obj.CreateUserName,
                ServiceOrderId = obj.ServiceOrderId,
                ServiceWorkOrder = obj.ServiceWorkOrder,
                UpdateTime = DateTime.Now,
                UpdateUserId = user.Id,
                UpdateUserName = user.Name
                //todo:补充或调整自己需要的字段
            });

        }
            

        public AppServiceOrderLogApp(IUnitWork unitWork, IRepository<AppServiceOrderLog> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}