using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class SolutionApp : BaseApp<Solution>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QuerySolutionListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("solution");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<Solution>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id.Contains(request.key) || u.Subject.Contains(request.key)).WhereIf(int.TryParse(request.key, out int code), u => u.SltCode == code);
            }


            var propertyStr = string.Join(',', properties.Select(u => u.Key));
            result.columnHeaders = properties;
            result.Data = objs.OrderByDescending(u => u.SltCode)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select($"new ({propertyStr})");
            result.Count = objs.Count();
            return result;
        }

        public async Task Add(AddOrUpdateSolutionReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var maxCode = await Repository.Find(null).Select(s => s.SltCode).MaxAsync();
            req.SltCode = ++maxCode;
            var obj = req.MapTo<Solution>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = loginContext.User;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            await Repository.AddAsync(obj);
        }

         public void Update(AddOrUpdateSolutionReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<Solution>(u => u.Id == obj.Id, u => new Solution
            {
                SltCode = obj.SltCode,
                Subject = obj.Subject,
                Cause = obj.Cause,
                Symptom = obj.Symptom,
                Descriptio = obj.Descriptio,
                Status = obj.Status,
                UpdateTime = DateTime.Now,
                UpdateUserId = user.Id,
                UpdateUserName = user.Name
                //todo:补充或调整自己需要的字段
            });

        }
            

        public SolutionApp(IUnitWork unitWork, IRepository<Solution> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}