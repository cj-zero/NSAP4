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
    public class BeforeSaleFilesApp : BaseApp<BeforeSaleFiles>
    {
        private RevelanceManagerApp _revelanceApp;

        public BeforeSaleFilesApp(IUnitWork unitWork, IRepository<BeforeSaleFiles> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
        }
        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryBeforeSaleFilesListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //var properties = loginContext.GetProperties("beforesalefiles");

            //if (properties == null || properties.Count == 0)
            //{
            //    throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            //}


            var result = new TableData();
            var objs = UnitWork.Find<BeforeSaleFiles>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id.Contains(request.key));
            }


            //var propertyStr = string.Join(',', properties.Select(u => u.Key));
            //result.columnHeaders = properties;
            result.Data = objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToList();// Select($"new ({propertyStr})");
            result.Count = objs.Count();
            return result;
        }

        public void Add(AddOrUpdateBeforeSaleFilesReq req)
        {
            var obj = req.MapTo<BeforeSaleFiles>();
            //todo:补充或调整自己需要的字段
            var user = _auth.GetCurrentUser().User;
            Repository.Add(obj);
        }

         public void Update(AddOrUpdateBeforeSaleFilesReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<BeforeSaleFiles>(u => u.Id == obj.Id, u => new BeforeSaleFiles
            {
                BeforeSaleDemandId = obj.BeforeSaleDemandId,
                FileId = obj.FileId,
                Type = obj.Type
                //todo:补充或调整自己需要的字段
            });

        }           
    }
}