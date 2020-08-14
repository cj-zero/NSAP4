using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class CertPlcApp : BaseApp<Certplc>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryCertPlcListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("certplc");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<Certplc>(null);
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

        public void Add(AddOrUpdateCertPlcReq req)
        {
            var obj = req.MapTo<Certplc>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            Repository.Add(obj);
        }
        public async Task AddAsync(AddOrUpdateCertPlcReq req, CancellationToken cancellationToken = default)
        {
            var obj = req.MapTo<Certplc>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            await Repository.AddAsync(obj, cancellationToken);
        }

         public void Update(AddOrUpdateCertPlcReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<Certplc>(u => u.Id == obj.Id, u => new Certplc
            {
                CertNo = obj.CertNo,
                PlcGuid = obj.PlcGuid,
                CreateTime = obj.CreateTime,
                //todo:补充或调整自己需要的字段
            });

        }
            

        public CertPlcApp(IUnitWork unitWork, IRepository<Certplc> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}