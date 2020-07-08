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
    public class AppUserMapApp : BaseApp<AppUserMap>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryAppUserMapListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("appusermap");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<AppUserMap>(null);
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

        public void Add(AddOrUpdateAppUserMapReq req)
        {
            var obj = req.MapTo<AppUserMap>();
            //todo:补充或调整自己需要的字段
            var user = _auth.GetCurrentUser().User;
            Repository.Add(obj);
        }

         public void Update(AddOrUpdateAppUserMapReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<AppUserMap>(u => u.Id == obj.Id, u => new AppUserMap
            {
                UserID = obj.UserID,
                AppUserId = obj.AppUserId,
                AppUserRole = obj.AppUserRole
                //todo:补充或调整自己需要的字段
            });

        }
            

        public AppUserMapApp(IUnitWork unitWork, IRepository<AppUserMap> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}