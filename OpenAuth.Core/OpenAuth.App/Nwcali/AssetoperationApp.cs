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
    public class AssetoperationApp : BaseApp<AssetOperation>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        //public TableData Load(QueryassetoperationListReq request)
        //{
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }

        //    var properties = loginContext.GetProperties("assetoperation");

        //    if (properties == null || properties.Count == 0)
        //    {
        //        throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
        //    }
        //    var result = new TableData();
        //    var objs = UnitWork.Find<AssetOperation>(null);
        //    objs = objs.Where(u => u.Id == request.AssetId);
        //    var propertyStr = string.Join(',', properties.Select(u => u.Key));
        //    result.columnHeaders = properties;
        //    result.Data = objs.OrderBy(u => u.Id)
        //        .Skip((request.page - 1) * request.limit)
        //        .Take(request.limit).Select($"new ({propertyStr})");
        //    result.Count = objs.Count();
        //    return result;
        //}
        public TableData Load(string AssetId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("assetoperation");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }
            var result = new TableData();
            var objs = UnitWork.Find<AssetOperation>(null);
            objs = objs.Where(u => u.AssetId.Contains(AssetId));
            result.Data = objs.OrderBy(u => u.Id).ToList();
            result.Count = objs.Count();
            return result;
        }
        public void Add(AddOrUpdateassetoperationReq req)
        {
            var obj = req.MapTo<AssetOperation>();
            //todo:补充或调整自己需要的字段
            obj.OperationCZDate = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.OperationCZName = user.Name;
            UnitWork.AddAsync<AssetOperation>(obj);
            //Repository.Add(obj);
        }
        public AssetoperationApp(IUnitWork unitWork, IRepository<AssetOperation> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}