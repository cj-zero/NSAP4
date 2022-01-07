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
    public class BeforeSaleDemandProjectApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;
        public BeforeSaleDemandProjectApp(IUnitWork unitWork,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, auth)
        {
            _revelanceApp = app;
        }
        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryBeforeSaleDemandProjectListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //var properties = loginContext.GetProperties("BeforeSaleDemandProject");
            //if (properties == null || properties.Count == 0)
            //{
            //    throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            //}


            var result = new TableData();
            var objs = UnitWork.Find<BeforeSaleDemandProject>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id.Equals(request.key));
            }


            //var propertyStr = string.Join(',', properties.Select(u => u.Key));
            //result.columnHeaders = properties;
            result.Data = objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToList();//.Select($"new ({propertyStr})");
            result.Count = objs.Count();
            return result;
        }

        public void Add(AddOrUpdateBeforeSaleDemandProjectReq req)
        {
            var obj = req.MapTo<BeforeSaleDemandProject>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            //Repository.Add(obj);
        }

         public void Update(AddOrUpdateBeforeSaleDemandProjectReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<BeforeSaleDemandProject>(u => u.Id.Equals(obj.Id), u => new BeforeSaleDemandProject
            {
                ProjectNum = obj.ProjectNum,
                PromoterId = obj.PromoterId,
                PromoterName = obj.PromoterName,
                ReqUserId = obj.ReqUserId,
                ReqUserName = obj.ReqUserName,
                DevUserId = obj.DevUserId,
                DevUserName = obj.DevUserName,
                TestUserId = obj.TestUserId,
                TestUserName = obj.TestUserName,
                ActualStartDate = obj.ActualStartDate,
                SubmitDate = obj.SubmitDate,
                FlowInstanceId = obj.FlowInstanceId,
                Status = obj.Status,
                ProjcetUrl = obj.ProjcetUrl,
                ProjectDocURL = obj.ProjectDocURL,
                ActualDevStartDate = obj.ActualDevStartDate,
                ActualDevEndDate = obj.ActualDevEndDate,
                CreateUserName = obj.CreateUserName,
                CreateUserId = obj.CreateUserId,
                CreateTime = obj.CreateTime,
                UpdateTime = obj.UpdateTime
                //todo:补充或调整自己需要的字段
            });

        }
    }
}