using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using Quartz;

namespace OpenAuth.App.HostedService
{
    public class QuartzService : IHostedService, IDisposable
    {
        private readonly ILogger<QuartzService> _logger;
        private IScheduler _scheduler;
        private readonly IUnitWork _unitWork;

        public QuartzService(ILogger<QuartzService> logger, IScheduler scheduler, IUnitWork unitWork)
        {
            _logger = logger;
            _scheduler = scheduler;
            _unitWork = unitWork;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("启动定时job，可以在这里配置读取数据库需要启动的任务，然后启动他们");
            await _scheduler.Start();
            await InitJob();
            //return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _scheduler.Shutdown();
            _logger.LogInformation("关闭定时job");
        }

        public void Dispose()
        {
            _scheduler.Shutdown().ConfigureAwait(false).GetAwaiter().GetResult();
            //throw new NotImplementedException();
        }

        private async Task InitJob()
        {
            var jobs = await _unitWork.Find<OpenJob>(j => j.Status == 1).ToListAsync();
            foreach (var job in jobs)
            {
                try
                {
                    _logger.LogInformation($"启动定时job: {job.Id}");
                    var jobBuilderType = typeof(JobBuilder);
                    var method = jobBuilderType.GetMethods().FirstOrDefault(
                            x => x.Name.Equals("Create", StringComparison.OrdinalIgnoreCase) &&
                                 x.IsGenericMethod && x.GetParameters().Length == 0)
                        ?.MakeGenericMethod(Type.GetType(job.JobCall));

                    var jobBuilder = (JobBuilder)method.Invoke(null, null);

                    IJobDetail jobDetail = jobBuilder.WithIdentity(job.Id).Build();
                    jobDetail.JobDataMap[Define.JOBMAPKEY] = job.Id;  //传递job信息
                    ITrigger trigger = TriggerBuilder.Create()
                        .WithCronSchedule(job.Cron)
                        .WithIdentity(job.Id)
                        .StartNow()
                        .Build();
                    await _scheduler.ScheduleJob(jobDetail, trigger);
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}