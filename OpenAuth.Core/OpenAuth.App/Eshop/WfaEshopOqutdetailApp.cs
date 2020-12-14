﻿using System;
using System.Linq;
using Infrastructure;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class wfa_eshop_oqutdetailApp : BaseApp<wfa_eshop_oqutdetail>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(Querywfa_eshop_oqutdetailListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("wfa_eshop_oqutdetail");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<wfa_eshop_oqutdetail>(null);
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

        public void Add(AddOrUpdatewfa_eshop_oqutdetailReq req)
        {
            var obj = req.MapTo<wfa_eshop_oqutdetail>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            Repository.Add(obj);
        }

         public void Update(AddOrUpdatewfa_eshop_oqutdetailReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<wfa_eshop_oqutdetail>(u => u.Id == obj.Id, u => new wfa_eshop_oqutdetail
            {
                document_id = obj.document_id,
                item_code = obj.item_code,
                item_desc = obj.item_desc,
                item_qty = obj.item_qty,
                UpdateTime = DateTime.Now,
                UpdateUserId = user.Id,
                UpdateUserName = user.Name
                //todo:补充或调整自己需要的字段
            });

        }
            

        public wfa_eshop_oqutdetailApp(IUnitWork unitWork, IRepository<wfa_eshop_oqutdetail> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}