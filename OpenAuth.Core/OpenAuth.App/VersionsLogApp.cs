using System;
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
    public class VersionsLogApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public async  Task<TableData> Load(PageReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            var objs = await UnitWork.Find<VersionsLog>(v=>v.IsDelete==false).OrderByDescending(u => u.CreateTime)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();

            result.Data = objs;
            result.Count = objs.Count();
            return result;
        }
        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> VersionsNumberList()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            var objs = await UnitWork.Find<VersionsLog>(v => v.IsDelete == false).Select(v=>v.VersionsNumber).ToListAsync();

            result.Data = objs;
            result.Count = objs.Count();
            return result;
        }

        public async Task Add(AddOrUpdateVersionsLogReq req)
        {
            var obj = req.MapTo<VersionsLog>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            obj.IsDelete = false;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            obj.CreateUser = user.Name;
            await UnitWork.AddAsync<VersionsLog>(obj);
            await UnitWork.SaveAsync();
        }

         public async Task Delete(AddOrUpdateVersionsLogReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            if (user.Account == Define.SYSTEM_USERNAME)
            {
                await UnitWork.UpdateAsync<VersionsLog>(v => v.VersionsNumber == obj.VersionsNumber, v => new VersionsLog { IsDelete = true });

                await UnitWork.SaveAsync();
            }
            else 
            {
                throw new Exception("无权限删除，请联系管理员");
            }
            
        }
            

        public VersionsLogApp(IUnitWork unitWork,RevelanceManagerApp app, IAuth auth) : base(unitWork,auth)
        {
            _revelanceApp = app;
        }
    }
}