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
    public class GlobalAreaApp : BaseApp<GlobalArea>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryGlobalAreaListReq request)
        {
            var result = new TableData();
            
            var GlobalArealist= UnitWork.Find<GlobalArea>(null);

            if (request.StateCode != null)
            {
                GlobalArealist = GlobalArealist.Where(g => g.StateCode == request.StateCode && g.provinceCode != "0"&& g.cityCode == "0");
            }
            else if (request.provinceCode != null) 
            {
                GlobalArealist = GlobalArealist.Where(g => g.provinceCode == request.provinceCode && g.cityCode!="0" && g.CountyCode== "0");
            }
            else if (request.cityCode != null)
            {
                GlobalArealist = GlobalArealist.Where(g => g.cityCode == request.cityCode && g.CountyCode!="0");
            }
            else 
            {
                GlobalArealist = GlobalArealist.Where(g => g.provinceCode == "0");
            }

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