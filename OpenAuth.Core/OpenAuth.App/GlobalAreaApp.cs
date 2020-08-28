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
    public class GlobalAreaApp : BaseApp<GlobalArea>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryGlobalAreaListReq request)
        {
            var result = new TableData();
            
            var GlobalArealist= UnitWork.Find<GlobalArea>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(request.ReqId), u => u.Pid == request.ReqId)
                .WhereIf(string.IsNullOrWhiteSpace(request.ReqId),u=> u.AreaLevel.Equals("1"));
            
            result.Data = GlobalArealist.ToList();
            return result;
        }

       

        public GlobalAreaApp(IUnitWork unitWork, IRepository<GlobalArea> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
        }
    }
}