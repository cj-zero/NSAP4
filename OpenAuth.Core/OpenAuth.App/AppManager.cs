using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;

namespace OpenAuth.App
{
    /// <summary>
    /// 分类管理
    /// </summary>
    public class AppManager : BaseApp<Application>
    {
        public void Add(Application Application)
        {
            if (string.IsNullOrEmpty(Application.Id))
            {
                Application.Id = Guid.NewGuid().ToString();
            }
            Repository.Add(Application);
        }
        public async Task AddAsync(AddOrUpdateAppReq req)
        {
            if (string.IsNullOrEmpty(req.Id))
            {
                req.Id = Guid.NewGuid().ToString();
            }
            await Repository.AddAsync(req.MapTo<Application>());
        }

        public void Update(AddOrUpdateAppReq req)
        {
            Repository.Update(req.MapTo<Application>());
        }
        public async Task UpdateAsync(AddOrUpdateAppReq req)
        {
            await Repository.UpdateAsync(req.MapTo<Application>());
        }


        public List<Application> GetList(QueryAppListReq request)
        {
            var applications =  UnitWork.Find<Application>(null) ;
           
            return applications.ToList();
        }

        public AppManager(IUnitWork unitWork, IRepository<Application> repository,IAuth auth) : base(unitWork, repository, auth)
        {
        }
    }
}