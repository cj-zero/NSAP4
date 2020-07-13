using NetOffice.Extensions.Conversion;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    public class ServiceOrderTimeoutJob : IJob
    {
        private readonly ServiceOrderApp _serviceOrderApp;
        private readonly UserManagerApp _userManagerApp;
        private readonly SysMessageApp _sysMessageApp;

        private readonly OpenJobApp _openJobApp;

        public ServiceOrderTimeoutJob(ServiceOrderApp serviceOrderApp, OpenJobApp openJobApp, UserManagerApp userManagerApp, SysMessageApp sysMessageApp)
        {
            _serviceOrderApp = serviceOrderApp;
            _openJobApp = openJobApp;
            _userManagerApp = userManagerApp;
            _sysMessageApp = sysMessageApp;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            //todo:这里可以加入自己的自动任务逻辑
            var list = await _serviceOrderApp.FindTimeoutOrder();
            if(list.Count > 0)
            {
                var users = await _userManagerApp.LoadByRoleName("售后");
                foreach (var user in users)
                {
                    await _sysMessageApp.AddAsync(new Repository.Domain.SysMessage
                    {
                        Title = "超时服务单未处理",
                        Content = $"服务单:{string.Join(",", list.Select(s => s.Id.ToString()).ToArray())}超过24小时未处理，请及时处理。若已处理请忽略。",
                        CreateId = Guid.Empty.ToString(),
                        CreateTime = DateTime.Now,
                        FromId = Guid.Empty.ToString(),
                        FromName = "系统通知",
                        ToId = user.Id,
                        ToName = user.Name,
                        TypeName = "呼叫服务通知",
                        TypeId = Guid.Empty.ToString()
                    });
                }
                
            }
            _openJobApp.RecordRun(jobId);
        }
    }
}
