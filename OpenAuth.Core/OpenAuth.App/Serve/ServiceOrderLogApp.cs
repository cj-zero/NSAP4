using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Npoi.Mapper;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Response;
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

            objs.WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderId),s=>s.ServiceOrderId.Equals(request.ServiceOrderId));
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
        public async Task BatchAddAsync(AddOrUpdateServiceOrderLogReq req, List<int> ids)
        {
            var objs = new List<ServiceOrderLog>();
            ids.ForEach(i =>
            {
                var obj = req.MapTo<ServiceOrderLog>();
                //todo:补充或调整自己需要的字段
                obj.CreateTime = DateTime.Now;
                var user = _auth.GetCurrentUser().User;
                obj.CreateUserId = user.Id;
                obj.CreateUserName = user.Name;
                obj.ServiceWorkOrderId = i;
                objs.Add(obj);
            });
            await Repository.BatchAddAsync(objs.ToArray());
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

        /// <summary>
        /// 根据服务单id查找关于该服务单日志
        /// </summary>
        /// <param name="ServiceOrderId"></param>
        /// <returns></returns>
        public async Task<TableData> GetServiceOrderLog(int ServiceOrderId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var ids = await UnitWork.Find<ServiceWorkOrder>(s=>s.ServiceOrderId.Equals(ServiceOrderId)).Select(s => s.Id).ToListAsync();
            var ServiceOrderLogs = await UnitWork.Find<ServiceOrderLog>(s => s.ServiceOrderId.Equals(ServiceOrderId) || ids.Contains((int)s.ServiceWorkOrderId)).ToListAsync();
            ServiceOrderLogs = ServiceOrderLogs.GroupBy(o=>new { o.Action,o.CreateTime}).Select(o=>o.First()).ToList();
            var loglist = ServiceOrderLogs.Select(s => new ServiceOrderLogResp
            {
                CreateTime = s.CreateTime,
                CreateUserName = s.CreateUserName,
                Action = s.Action
            });
            result.Data = loglist.OrderByDescending(u => u.CreateTime).ToList();
            return result;
        }
        public ServiceOrderLogApp(IUnitWork unitWork, IRepository<ServiceOrderLog> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}