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
    public class ProblemTypeApp : BaseApp<ProblemType>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("problemtype");

            //if (properties == null || properties.Count == 0)
            //{
            //    throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            //}


            var result = new TableData();
            var objs = UnitWork.Find<ProblemType>(null).Where(o => o.ParentId.Trim().Length.Equals(0)).Select(f => new
            {
                f.Id,
                f.Name,
                f.InuseFlag,
                f.OrderIdx,
                f.Description,
                f.ParentId,
                ChildTypes = UnitWork.Find<ProblemType>(null).Where(o1 => o1.ParentId.Equals(f.Id)).ToList()
            }).ToList();


            var propertyStr = string.Join(',', properties.Select(u => u.Key));
            result.columnHeaders = properties;
            result.Data = objs;
            result.Count = objs.Count();
            return result;
        }

        public void Add(AddOrUpdateProblemTypeReq req)
        {
            var obj = req.MapTo<ProblemType>();
            //todo:补充或调整自己需要的字段
            //obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            //obj.CreateUserId = user.Id;
            //obj.CreateUserName = user.Name;
            Repository.Add(obj);
        }

        public void Update(AddOrUpdateProblemTypeReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<ProblemType>(u => u.Id == obj.Id, u => new ProblemType
            {
                Name = obj.Name,
                Description = obj.Description,
                InuseFlag = obj.InuseFlag,
                ParentId = obj.ParentId,
                OrderIdx = obj.OrderIdx,
                //prblmTypID = obj.prblmTypID,
                //parentTypeID = obj.parentTypeID,
                //UpdateTime = DateTime.Now,
                //UpdateUserId = user.Id,
                //UpdateUserName = user.Name
                //todo:补充或调整自己需要的字段
            });

        }

        /// <summary>
        /// 加载客户问题类型列表(APP只显示一级)
        /// </summary>
        public TableData AppLoad()
        {
            var result = new TableData();
            string[] ChildTypes = new string[0];
            var objs = UnitWork.Find<ProblemType>(null).Where(o => o.ParentId.Trim().Length.Equals(0)).Select(f => new
            {
                f.Id,
                f.Name,
                f.InuseFlag,
                f.OrderIdx,
                f.Description,
                f.ParentId,
                ChildTypes
            }).ToList();
            result.Data = objs;
            result.Count = objs.Count();
            return result;
        }


        public ProblemTypeApp(IUnitWork unitWork, IRepository<ProblemType> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
        }
    }
}