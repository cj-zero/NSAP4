using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Npoi.Mapper;
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
        public async Task<TableData> Load(QueryGlobalAreaListReq request)
        {
            var result = new TableData();
            
            var GlobalArealist=  UnitWork.Find<GlobalArea>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(request.ReqId), u => u.Pid == request.ReqId)
                .WhereIf(string.IsNullOrWhiteSpace(request.ReqId),u=> u.AreaLevel.Equals("1"));
            
            result.Data = await GlobalArealist.ToListAsync();
            return result;
        }
        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> GetArea(QueryGlobalAreaListReq request)
        {
            var result = new TableData();

            var globalAreaAll = await UnitWork.Find<GlobalArea>(null).ToListAsync();
            if (!string.IsNullOrWhiteSpace(request.AreaName)) 
            {
                var globalAreaList = globalAreaAll.Where(u => u.AreaName.Contains(request.AreaName));
                List<string> areaId = new List<string>();
                List<List<GlobalArea>> globalAreaGroup = new List<List<GlobalArea>>();
                foreach (var item in globalAreaList)
                {
                    if (!string.IsNullOrWhiteSpace(item.Path)) 
                    {
                        areaId = item.Path.Split(',').ToList();
                    }
                    areaId.Add(item.Id);
                    globalAreaGroup.Add(globalAreaAll.Where(g => areaId.Contains(g.Id)).ToList());
                }
                result.Data = globalAreaGroup.ToList();
            }
            return result;
        }
        

        public GlobalAreaApp(IUnitWork unitWork, IRepository<GlobalArea> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
        }
    }
}