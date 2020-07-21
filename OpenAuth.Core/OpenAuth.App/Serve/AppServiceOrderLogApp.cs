using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using Org.BouncyCastle.Ocsp;

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
            result.Data = objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select($"new ({propertyStr})");
            result.Count = objs.Count();
            return result;
        }

        /// <summary>
        /// 查询服务单和工单的执行记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<OrderLogListResp>> GetOrderLog(GetOrderLogListReq request)
        {
            var list = new List<OrderLogListResp>();
            var objs = UnitWork.Find<AppServiceOrderLog>(null);
            var orderLogs = await objs.Where(a => a.ServiceOrderId.Equals(request.ServiceOrderId)).Select(a => new OrderLogListResp { Title = a.Title, Details = a.Details, CreateTime = a.CreateTime }).ToListAsync();
            list.AddRange(orderLogs);
            if(!(request.ServiceWorkOrderId is null))
            {
                var workOrderLogs = await objs.Where(a => a.ServiceWorkOrder.Equals(request.ServiceWorkOrderId.Value)).Select(a => new OrderLogListResp { Title = a.Title, Details = a.Details, CreateTime = a.CreateTime }).ToListAsync();
                list.AddRange(workOrderLogs);
            }
            return list.OrderByDescending(l => l.CreateTime).ToList();
        }

        /// <summary>
        /// 插入记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task AddAsync(AddOrUpdateAppServiceOrderLogReq req)
        {
            var obj = req.MapTo<AppServiceOrderLog>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            await Repository.AddAsync(obj);
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