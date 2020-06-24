using System;
using System.Linq;
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
    public class CertinfoApp : BaseApp<Certinfo>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> LoadAsync(QueryCertinfoListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("certinfo");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<Certinfo>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id.Contains(request.key));
            }
            if (!string.IsNullOrEmpty(request.CertNo))
            {
                objs = objs.Where(u => u.CertNo.Contains(request.CertNo));
            }
            var list = await objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();

            var fId = list.Select(l => l.FlowInstanceId);
            var fs = await UnitWork.Find<FlowInstance>(f => fId.Contains(f.Id)).ToListAsync();
            list.ForEach(c =>
            {
                c.FlowInstance = fs.Find(f => f.Id.Equals(c.FlowInstanceId));
            });
            var view = list.Select(c =>
            {
                return new CertinfoView { Id = c.Id, CertNo = c.CertNo, ActivityName = c.FlowInstance?.ActivityName, CreateTime = c.CreateTime };
            });
            properties.RemoveAll(a => a.Key.Equals("FlowInstance"));
            //properties.Add(new KeyDescription() { Key = "FlowInstanceActivityName", Browsable = true, Description = "FlowInstanceActivityName", Type = "String" });
            var propertyStr = string.Join(',', properties.Select(u => u.Key));
            result.columnHeaders = properties;
            result.data = view;//list.AsQueryable().Select($"new ({propertyStr},FlowInstance)");
            result.count = objs.Count();
            return result;
        }

        public void Add(AddOrUpdateCertinfoReq req)
        {
            var obj = req.MapTo<Certinfo>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            //obj.CreateUserId = user.Id;
            //obj.CreateUserName = user.Name;
            Repository.Add(obj);
        }
        public async Task AddAsync(AddOrUpdateCertinfoReq req, CancellationToken cancellationToken = default)
        {
            var obj = req.MapTo<Certinfo>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            //obj.CreateUserId = user.Id;
            //obj.CreateUserName = user.Name;
            await Repository.AddAsync(obj, cancellationToken);
        }

        public void Update(AddOrUpdateCertinfoReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<Certinfo>(u => u.Id == obj.Id, u => new Certinfo
            {
                CertNo = obj.CertNo,
                CertPath = obj.CertPath,
                PdfPath = obj.PdfPath,
                BaseInfoPath = obj.BaseInfoPath,
                CreateTime = obj.CreateTime,
                //UpdateTime = DateTime.Now,
                //UpdateUserId = user.Id,
                //UpdateUserName = user.Name
                //todo:补充或调整自己需要的字段
            });

        }
        public async Task UpdateAsync(AddOrUpdateCertinfoReq obj, CancellationToken cancellationToken = default)
        {
            var user = _auth.GetCurrentUser().User;
            await UnitWork.UpdateAsync<Certinfo>(u => u.Id == obj.Id, u => new Certinfo
            {
                CertNo = obj.CertNo,
                CertPath = obj.CertPath,
                PdfPath = obj.PdfPath,
                BaseInfoPath = obj.BaseInfoPath,
                CreateTime = obj.CreateTime,
                FlowInstanceId = obj.FlowInstanceId,
                //UpdateTime = DateTime.Now,
                //UpdateUserId = user.Id,
                //UpdateUserName = user.Name
                //todo:补充或调整自己需要的字段
            }, cancellationToken);
            await UnitWork.SaveAsync();
        }
            

        public CertinfoApp(IUnitWork unitWork, IRepository<Certinfo> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}