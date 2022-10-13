using OpenAuth.App.ClientRelation;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    /// <summary>
    /// 客户审核通过后同步关系
    /// </summary>
    public class ClientRelationJob : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly ClientRelationApp _clientRelationApp;

        public ClientRelationJob(OpenJobApp openJobApp, ClientRelationApp  clientRelationApp)
        {
            _openJobApp = openJobApp;
            _clientRelationApp = clientRelationApp;
        }


        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            await _clientRelationApp.SyncRelations();
            _openJobApp.RecordRun(jobId);
        }


    }
}
