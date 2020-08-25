using NetOffice.Extensions.Conversion;
using OpenAuth.App.SignalR;
using OpenAuth.Repository.Domain;
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
        private readonly SignalRMessageApp _signalrmessage;
        private readonly UserManagerApp _userManagerApp;
        private readonly SysMessageApp _sysMessageApp;

        private SysLogApp _sysLogApp;
        private readonly OpenJobApp _openJobApp;

        public ServiceOrderTimeoutJob(ServiceOrderApp serviceOrderApp, OpenJobApp openJobApp, UserManagerApp userManagerApp, SysMessageApp sysMessageApp, SysLogApp sysLogApp, SignalRMessageApp signalrmessage)
        {
            _serviceOrderApp = serviceOrderApp;
            _openJobApp = openJobApp;
            _userManagerApp = userManagerApp;
            _sysMessageApp = sysMessageApp;
            _sysLogApp = sysLogApp;
            _signalrmessage = signalrmessage;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);

            if (_serviceOrderApp is null)
            {
                _sysLogApp.Add(new SysLog
                {
                    TypeName = "定时任务",
                    TypeId = "AUTOJOB",
                    Content = "ServiceOrderApp对象为空"
                });
            }
            if (_userManagerApp is null)
            {
                _sysLogApp.Add(new SysLog
                {
                    TypeName = "定时任务",
                    TypeId = "AUTOJOB",
                    Content = "UserManagerApp对象为空"
                });
            }
            if (_sysMessageApp is null)
            {
                _sysLogApp.Add(new SysLog
                {
                    TypeName = "定时任务",
                    TypeId = "AUTOJOB",
                    Content = "SysMessageApp对象为空"
                });
            }

            //todo:这里可以加入自己的自动任务逻辑
            var list = await _serviceOrderApp.FindTimeoutOrder();
            if(list.Count > 0)
            {
                var users = await _userManagerApp.LoadByRoleName("呼叫中心");
                foreach (var user in users)
                {
                    await _sysMessageApp.AddAsync(new Repository.Domain.SysMessage
                    {
                        Title = "超时服务单未处理",
                        Content = $"服务单:{string.Join(",", list.Select(s => s.U_SAP_ID.ToString()).ToArray())}超时未处理，请及时处理。若已处理请忽略。",
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
                await _signalrmessage.SendSystemMessage(SignalRSendType.Role, $"服务单:{string.Join(",", list.Select(s => s.U_SAP_ID.ToString()).ToArray())}超时未处理，请及时处理。若已处理请忽略。", new List<string>() { "呼叫中心" });
            }
            _openJobApp.RecordRun(jobId);
        }
    }
}
