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
    public class RealTimeLocationApp : BaseApp<RealTimeLocation>
    {
        private RevelanceManagerApp _revelanceApp;
        public RealTimeLocationApp(IUnitWork unitWork, IRepository<RealTimeLocation> repository,
    RevelanceManagerApp app, IAuth auth) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryrealtimelocationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("realtimelocation");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<RealTimeLocation>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id == int.Parse(request.key));
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
        /// 添加定位信息
        /// </summary>
        /// <param name="req"></param>
        public async Task Add(AddOrUpdaterealtimelocationReq req)
        {
            var obj = req.MapTo<RealTimeLocation>();
            obj.CreateTime = DateTime.Now;
            //todo:补充或调整自己需要的字段
            //判断是否已存在记录 若存在则做更新操作
            //var locations = await UnitWork.Find<RealTimeLocation>(r => r.AppUserId == req.AppUserId).FirstOrDefaultAsync();
            //if (locations != null)
            //{
            //    obj.Id = locations.Id;
            //    Repository.Update(obj);
            //}
            //else
            //{
            Repository.Add(obj);
            //}
        }
    }
}