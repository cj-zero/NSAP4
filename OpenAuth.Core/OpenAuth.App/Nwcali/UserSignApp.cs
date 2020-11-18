using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class UserSignApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> Load(QueryUserSignListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //var properties = loginContext.GetProperties("usersign");

            //if (properties == null || properties.Count == 0)
            //{
            //    throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            //}


            var result = new TableData();
            var objs = UnitWork.Find<UserSign>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.UserName.Contains(request.key));
            }

            //var propertyStr = string.Join(',', properties.Select(u => u.Key));
            //result.columnHeaders = properties;
            result.Data = await objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            result.Count = objs.Count();
            return result;
        }

        public async Task Add(AddOrUpdateUserSignReq req)
        {
            var obj = req.MapTo<UserSign>();
            //todo:补充或调整自己需要的字段
            var user = _auth.GetCurrentUser().User;
            await UnitWork.AddAsync<UserSign,int>(obj);
        }

         public async Task Update(AddOrUpdateUserSignReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            await UnitWork.UpdateAsync<UserSign>(u => u.Id == obj.Id, u => new UserSign
            {
                UserId = obj.UserId,
                UserName = obj.UserName,
                PictureId = obj.PictureId,
                //todo:补充或调整自己需要的字段
            });
        }

        public async Task Delete(List<int> ids)
        {
            await UnitWork.DeleteAsync<UserSign>(u => ids.Contains(u.Id));
        }

        public UserSignApp(IUnitWork unitWork, RevelanceManagerApp app, IAuth auth) : base(unitWork,auth)
        {
            _revelanceApp = app;
        }
    }
}