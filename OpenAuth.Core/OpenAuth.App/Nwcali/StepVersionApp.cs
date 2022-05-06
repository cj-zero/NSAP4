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
    public class StepVersionApp : OnlyUnitWorkBaeApp
    {
        public StepVersionApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
        }
        /// <summary>
        /// 是否存在中位机默认版本
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> IsExistsDefaultStepVersion()
        {
            var result = new TableData();

            var data = await UnitWork.Find<StepVersion>(null).FirstOrDefaultAsync(x => x.DefaultVersion == true);
            result.Data = data?.Id;

            return result;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> Load(QueryStepversionListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //var properties = loginContext.GetProperties("stepversion");

            //if (properties == null || properties.Count == 0)
            //{
            //    throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            //}


            var result = new TableData();
            var objs = UnitWork.Find<StepVersion>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.SeriesName.Contains(request.key));
            }
            result.Data = await objs.OrderBy(u => u.Id)
              .Skip((request.page - 1) * request.limit)
              .Take(request.limit).ToListAsync();
            result.Count = objs.Count();
            return result;
        }

        public async Task<TableData> GetDetails(int id)
        {
            var result = new TableData();
            var query = UnitWork.Find<StepVersion>(null).Where(c => c.Id==id);

            result.Data = await query.ToListAsync();
            result.Count = await query.CountAsync();
            return result;
        }
        public async Task Add(AddOrUpdateStepVersionReq req)
        {
            var obj = req.MapTo<StepVersion>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            obj.CreateUser = user.Name;
            obj = await UnitWork.AddAsync<StepVersion, int>(obj);
            await UnitWork.SaveAsync();
        }

         public void Update(AddOrUpdateStepVersionReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<StepVersion>(u => u.Id == obj.Id, u => new StepVersion
            {
                StepName = obj.StepName,
                SeriesName = obj.SeriesName,
                StepVersionName = obj.StepVersionName,
                FilePath = obj.FilePath,
                FileName = obj.FileName,
                Remark = obj.Remark,
                CreateUserId = obj.CreateUserId,
                CreateUser = obj.CreateUser,
                CreateTime = obj.CreateTime,
                DefaultVersion = obj.DefaultVersion,
                PublishNum =obj.PublishNum+1,
                UpdateTime = DateTime.Now
            });
        }
        public async Task Delete(List<int> ids)
        {
            await UnitWork.DeleteAsync<StepVersion>(u => ids.Contains(u.Id));
        }
    }
}