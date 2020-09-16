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
    public class MyExpendsApp : BaseApp<MyExpends>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryMyExpendsListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("myexpends");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<MyExpends>(null);
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

        public void Add(AddOrUpdateMyExpendsReq req)
        {
            var obj = req.MapTo<MyExpends>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            Repository.Add(obj);
        }

         public void Update(AddOrUpdateMyExpendsReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<MyExpends>(u => u.Id == obj.Id, u => new MyExpends
            {
                FeeType = obj.FeeType,
                SerialNumber = obj.SerialNumber,
                TrafficType = obj.TrafficType,
                Transport = obj.Transport,
                From = obj.From,
                To = obj.To,
                Money = obj.Money,
                InvoiceNumber = obj.InvoiceNumber,
                Remark = obj.Remark,
                CreateTime = obj.CreateTime,
                Days = obj.Days,
                TotalMoney = obj.TotalMoney,
                ExpenseCategory = obj.ExpenseCategory,
                UpdateTime = DateTime.Now,
                UpdateUserId = user.Id,
                UpdateUserName = user.Name
                //todo:补充或调整自己需要的字段
            });

        }
            

        public MyExpendsApp(IUnitWork unitWork, IRepository<MyExpends> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}