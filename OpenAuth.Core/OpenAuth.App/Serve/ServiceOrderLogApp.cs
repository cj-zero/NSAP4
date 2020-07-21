using System;
using System.Linq;
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
    public class ServiceOrderLogApp : BaseApp<ServiceOrderLog>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> Load(QueryServiceOrderLogListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //var properties = loginContext.GetProperties("serviceorderlog");

            //if (properties == null || properties.Count == 0)
            //{
            //    throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            //}


            var result = new TableData();
            var objs = UnitWork.Find<ServiceOrderLog>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id.Contains(request.key));
            }


            //var propertyStr = string.Join(',', properties.Select(u => u.Key));
            //result.columnHeaders = properties;
            result.Data = await objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            result.Count = objs.Count();
            return result;
        }

        public void Add(AddOrUpdateServiceOrderLogReq req)
        {
            var obj = req.MapTo<ServiceOrderLog>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            Repository.Add(obj);
        }
        public async Task AddAsync(AddOrUpdateServiceOrderLogReq req)
        {
            var obj = req.MapTo<ServiceOrderLog>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            await Repository.AddAsync(obj);
        }

         public void Update(AddOrUpdateServiceOrderLogReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<ServiceOrderLog>(u => u.Id == obj.Id, u => new ServiceOrderLog
            {
                ServiceOrderId = obj.ServiceOrderId,
                ServiceWorkOrderId = obj.ServiceWorkOrderId,
                Action = obj.Action,
                ActionType = obj.ActionType,
                CreateTime = obj.CreateTime,
                CreateUserId = obj.CreateUserId,
                CreateUserName = obj.CreateUserName,
                //todo:补充或调整自己需要的字段
            });

        }
            

        public ServiceOrderLogApp(IUnitWork unitWork, IRepository<ServiceOrderLog> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}