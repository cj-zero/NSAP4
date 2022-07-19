using System;
using System.Linq;
using Infrastructure;
using Infrastructure.Extensions;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class DataPrivilegeRuleApp : BaseApp<DataPrivilegeRule>
    {
        private RevelanceManagerApp _revelanceApp;
        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryDataPrivilegeRuleListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("DataPrivilegeRule");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<DataPrivilegeRule>(null)
                        .WhereIf(request.IsNew != null, c => c.IsNew == request.IsNew)
                        .WhereIf(request.Enable != null, c => c.Enable == request.Enable)
                        .WhereIf(!string.IsNullOrWhiteSpace(request.SourceCode), c => c.SourceCode.Contains(request.SourceCode))
                        .WhereIf(!string.IsNullOrWhiteSpace(request.SourceName), c => c.SourceName.Contains(request.SourceName))
                        .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUserName), c => c.SourceName.Contains(request.CreateUserName))
                        .WhereIf(!string.IsNullOrWhiteSpace(request.Description), c => c.SourceName.Contains(request.Description));
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id.Contains(request.key) || u.SourceCode.Contains(request.key) || u.Description.Contains(request.key));
            }

            var propertyStr = string.Join(',', properties.Select(u => u.Key));
            result.columnHeaders = properties;
            result.Data = objs.OrderByDescending(u => u.No)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select($"new ({propertyStr})");
            result.Count = objs.Count();
            return result;
        }

        public TableData LoadNew(QueryDataPrivilegeRuleListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //var properties = loginContext.GetProperties("DataPrivilegeRule");

            //if (properties == null || properties.Count == 0)
            //{
            //    throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            //}


            var result = new TableData();
            var objs = UnitWork.Find<DataPrivilegeRule>(null)
                        .WhereIf(request.IsNew != null, c => c.IsNew == request.IsNew)
                        .WhereIf(request.Enable != null, c => c.Enable == request.Enable)
                        .WhereIf(!string.IsNullOrWhiteSpace(request.SourceCode), c => c.SourceCode.Contains(request.SourceCode))
                        .WhereIf(!string.IsNullOrWhiteSpace(request.SourceName), c => c.SourceName.Contains(request.SourceName))
                        .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUserName), c => c.SourceName.Contains(request.CreateUserName))
                        .WhereIf(!string.IsNullOrWhiteSpace(request.Description), c => c.SourceName.Contains(request.Description));
         
            //var where = GetDataPrivilegeNew("DataPermissions");
            //var sql = $"select * from DataPrivilegeRule where {where}";
            //var query = UnitWork.Query<DataPrivilegeRule>(sql).ToList();


            //var propertyStr = string.Join(',', properties.Select(u => u.Key));
            //result.columnHeaders = properties;
            result.Data = objs.OrderByDescending(u => u.No)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit);
            result.Count = objs.Count();
            return result;
        }

        public void Add(AddOrUpdateDataPriviReq req)
        {
            if (Repository.IsExist(u => u.SourceCode == req.SourceCode))
            {
                throw new Exception($"已经存在{req.SourceCode}的数据规则，如果想调整规制请直接修改");
            }
            var obj = req.MapTo<DataPrivilegeRule>();
            obj.CreateUserId = _auth.GetCurrentUser().User.Id;
            obj.CreateTime = DateTime.Now;
            Repository.Add(obj);
        }
        public void AddNew(AddOrUpdateDataPriviReq req)
        {
            var loginUser = _auth.GetCurrentUser().User;
            var no = UnitWork.Find<DataPrivilegeRule>(c => c.IsNew == true).OrderByDescending(c => c.No).Select(c => c.No).FirstOrDefault();
            var obj = req.MapTo<DataPrivilegeRule>();
            obj.CreateUserId = loginUser.Id;
            obj.CreateTime = DateTime.Now;
            obj.CreateUserName = loginUser.Name;
            obj.UpdateTime = DateTime.Now;
            obj.UpdateUserId= loginUser.Id;
            obj.UpdateUserName = loginUser.Name;
            obj.IsNew = true;
            obj.No = no > 0 ? no + 1 : 1;
            Repository.Add(obj);
        }

        public void Update(AddOrUpdateDataPriviReq obj)
        {
            UnitWork.Update<DataPrivilegeRule>(u => u.Id == obj.Id, u => new DataPrivilegeRule
            {
                SortNo = obj.SortNo,
                SourceCode = obj.SourceCode,
                SourceName = obj.SourceName,
                UpdateTime = DateTime.Now,
                UpdateUserId = _auth.GetCurrentUser().User.Id,
                Description = obj.Description,
                PrivilegeRules = obj.PrivilegeRules,
                Enable = obj.Enable
                //todo:要修改的字段赋值
            });
    
        }

        public void UpdateEnable(AddOrUpdateDataPriviReq obj)
        {
            UnitWork.Update<DataPrivilegeRule>(u => u.Id == obj.Id, u => new DataPrivilegeRule
            {
                Enable = obj.Enable
                //todo:要修改的字段赋值
            });

        }

        public DataPrivilegeRuleApp(IUnitWork unitWork, IRepository<DataPrivilegeRule> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
        }

        public DataPrivilegeRule GetByModuleName(string moduleName)
        {
            return Repository.FindSingle(u=>u.SourceCode == moduleName);
        }

        public void Clear()
        {
            Repository.Delete(u =>true);
        }

        public void Test()
        {
            var test = GetDataPrivilegeNew("stepMange");
        }
    }
}