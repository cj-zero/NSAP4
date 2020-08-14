using System;
using System.Collections.Generic;
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
    public class CertOperationHistoryApp : BaseApp<CertOperationHistory>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryCertOperationHistoryListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("certoperationhistory");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<CertOperationHistory>(null);
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

        public void Add(AddOrUpdateCertOperationHistoryReq req)
        {
            var obj = req.MapTo<CertOperationHistory>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            obj.CreateUser = user.Name;
            Repository.Add(obj);
        }
        public async Task AddAsync(AddOrUpdateCertOperationHistoryReq req)
        {
            var obj = req.MapTo<CertOperationHistory>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            obj.CreateUser = user.Name;
            await Repository.AddAsync(obj);
        }

        public async Task<List<CertOperationHistory>> GetCertOperationHistory(string id)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            return await UnitWork.Find<CertOperationHistory>(c=>c.CertInfoId == id).ToListAsync();
        }

        public void Update(AddOrUpdateCertOperationHistoryReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<CertOperationHistory>(u => u.Id == obj.Id, u => new CertOperationHistory
            {
                CertInfoId = obj.CertInfoId,
                CreateUserId = obj.CreateUserId,
                CreateUser = obj.CreateUser,
                CreateTime = obj.CreateTime,
                Action = obj.Action,
                //todo:补充或调整自己需要的字段
            });

        }
            

        public CertOperationHistoryApp(IUnitWork unitWork, IRepository<CertOperationHistory> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}