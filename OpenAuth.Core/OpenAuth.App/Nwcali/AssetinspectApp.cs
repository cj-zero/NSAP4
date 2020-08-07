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
    /// <summary>
    /// 
    /// </summary>
    public class AssetinspectApp : BaseApp<AssetInspect>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载送检记录列表
        /// </summary>
        public TableData Load(QueryassetinspectListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("AssetInspect");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<AssetInspect>(null);
            objs = objs.Where(u => u.AssetId == request.AssetId);
            var propertyStr = string.Join(',', properties.Select(u => u.Key));
            result.columnHeaders = properties;
            result.Data = objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select($"new ({propertyStr})");
            result.Count = objs.Count();
            return result;
        }
        /// <summary>
        /// 添加送检记录
        /// </summary>
        /// <param name="req"></param>
        public void Add(AddOrUpdateassetinspectReq req,ref string InspectId)
        {
            var obj = req.MapTo<AssetInspect>();
            UnitWork.AddAsync<AssetInspect>(obj);
            //Repository.Add(obj);
            InspectId = obj.Id;
        }

        public AssetinspectApp(IUnitWork unitWork, IRepository<AssetInspect> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}