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
    public class BtsModelApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QuerybtsmodelListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            var objs = UnitWork.Find<BtsModel>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Model.Contains(request.key));
            }

            result.Data = objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToList();
            result.Count = objs.Count();
            return result;
        }

        public void Add(AddOrUpdatebtsmodelReq req)
        {
            var obj = req.MapTo<BtsModel>();
            //todo:补充或调整自己需要的字段
            var user = _auth.GetCurrentUser().User;
            UnitWork.Add<BtsModel, int>(obj);
            UnitWork.Save();
        }
        public void BatchAdd(List<BtsModel> reqs)
        {
            //todo:补充或调整自己需要的字段
            var user = _auth.GetCurrentUser().User;
            UnitWork.BatchAdd<BtsModel, int>(reqs.ToArray());
            UnitWork.Save();
        }

         public void Update(AddOrUpdatebtsmodelReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<BtsModel>(u => u.Id == obj.Id, u => new BtsModel
            {
                Model = obj.Model,
                Remark = obj.Remark,
                //todo:补充或调整自己需要的字段
            });
            UnitWork.Save();

        }

        public void Delete(List<int> ids)
        {
            UnitWork.Delete<BtsModel>(m => ids.Contains(m.Id));
            UnitWork.Save();
        }

        public BtsModelApp(IUnitWork unitWork,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, auth)
        {
            _revelanceApp = app;
        }
    }
}