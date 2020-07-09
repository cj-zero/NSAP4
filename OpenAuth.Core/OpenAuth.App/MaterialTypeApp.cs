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
    public class MaterialTypeApp : BaseApp<MaterialType>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryMaterialTypeListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("materialtype");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<MaterialType>(null);
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

        public void Add(AddOrUpdatematerialtypeReq req)
        {
            var obj = req.MapTo<MaterialType>();
            //todo:补充或调整自己需要的字段
            //obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            //obj.CreateUserId = user.Id;
            //obj.CreateUserName = user.Name;
            Repository.Add(obj);
        }

         public void Update(AddOrUpdatematerialtypeReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<MaterialType>(u => u.Id == obj.Id, u => new MaterialType
            {
                TypeAlias = obj.TypeAlias,
                TypeName = obj.TypeName,
                ParentId = obj.ParentId,
                TypeLevel = obj.TypeLevel,
                OrderIdx = obj.OrderIdx,
                CodingExp = obj.CodingExp,
                DescExp = obj.DescExp,
                Valid = obj.Valid,
                UpdTime = obj.UpdTime,
                CodeRuleFlag = obj.CodeRuleFlag,
                UserId = obj.UserId,
                AttachFlag = obj.AttachFlag,
                MacTime = obj.MacTime,
                MacPrice = obj.MacPrice,
                ForBomAttFlag = obj.ForBomAttFlag,
                type_id = obj.type_id,
                parent_id = obj.parent_id
                //UpdateTime = DateTime.Now,
                //UpdateUserId = user.Id,
                //UpdateUserName = user.Name
                //todo:补充或调整自己需要的字段
            });

        }
            

        public MaterialTypeApp(IUnitWork unitWork, IRepository<MaterialType> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}