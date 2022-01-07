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
    public class BeforeSaleProScheDulingApp : BaseApp<BeforeSaleProScheduling>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryBeforeSaleProSchedulingListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("beforesaleproscheduling");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<BeforeSaleProScheduling>(null);
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

        public void Add(AddOrUpdateBeforeSaleProScheDulingReq req)
        {
            var obj = req.MapTo<BeforeSaleProScheduling>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            Repository.Add(obj);
        }

         public void Update(AddOrUpdateBeforeSaleProScheDulingReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<BeforeSaleProScheduling>(u => u.Id.Equals(obj.Id), u => new BeforeSaleProScheduling
            {
                Stage = obj.Stage,
                UserId = obj.UserId,
                UserName = obj.UserName,
                StartDate = obj.StartDate,
                EndDate = obj.EndDate,
                CreateUserId = obj.CreateUserId,
                CreateTime = obj.CreateTime
                //todo:补充或调整自己需要的字段
            });

        }
            

        public BeforeSaleProScheDulingApp(IUnitWork unitWork, IRepository<BeforeSaleProScheduling> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}