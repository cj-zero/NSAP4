using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class BtsGroupApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QuerybtsgroupListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }



            var result = new TableData();
            var objs = UnitWork.Find<BtsGroup>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Group.Contains(request.key));
            }


            result.Data = objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToList();
            result.Count = objs.Count();
            return result;
        }

        public void Add(AddOrUpdatebtsgroupReq req)
        {
            var obj = req.MapTo<BtsGroup>();
            //todo:补充或调整自己需要的字段
            var user = _auth.GetCurrentUser().User;
            UnitWork.Add<BtsGroup,int>(obj);
            UnitWork.Save();
        }

         public void Update(AddOrUpdatebtsgroupReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<BtsGroup>(u => u.Id == obj.Id, u => new BtsGroup
            {
                Group = obj.Group,
                //todo:补充或调整自己需要的字段
            });
            UnitWork.Save();

        }
            

        public BtsGroupApp(IUnitWork unitWork,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, auth)
        {
            _revelanceApp = app;
        }

        public void Delete(List<int> ids)
        {
            UnitWork.Delete<BtsGroup>(g => ids.Contains(g.Id));
            UnitWork.Save();
        }
    }
}