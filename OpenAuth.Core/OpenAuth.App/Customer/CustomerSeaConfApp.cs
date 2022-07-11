using OpenAuth.App.Customer.Request;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Domain.Customer;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Response;
using OpenAuth.App.Customer.Response;
using OpenAuth.Repository.Domain;
using Quartz;

namespace OpenAuth.App.Customer
{
    public class CustomerSeaConfApp : OnlyUnitWorkBaeApp
    {
        private readonly OpenJobApp _openJobApp;
        private IScheduler _scheduler;

        public CustomerSeaConfApp(IUnitWork unitWork, IAuth auth, OpenJobApp openJobApp, IScheduler scheduler) : base(unitWork, auth)
        {
            _openJobApp = openJobApp;
            _scheduler = scheduler;
        }

        /// <summary>
        /// 自动放入公海设置
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> AutoPutInCustomerSea(AutoPutInCustomerSeaObjectReq req)
        {
            var result = new Infrastructure.Response();
            var userInfo = _auth.GetCurrentUser();
            //判断设置是否存在
            var isExist = UnitWork.Find<CustomerSeaConf>(null).Any();
            //存在则更新部分记录
            if (isExist)
            {
                var objectItem = UnitWork.Find<CustomerSeaConf>(null).First();
                objectItem.PutTime = req.PutTime.TimeOfDay;
                objectItem.NotifyTime = req.NotifyTime.TimeOfDay;
                objectItem.NotifyDay = req.NotifyDay;

                //每次修改,都将任务设置为不开启,修改完之后再手动开启
                objectItem.Enable = false;

                objectItem.UpdateDatetime = DateTime.Now;
                objectItem.UpdateUser = userInfo.User.Name;

                await UnitWork.UpdateAsync<CustomerSeaConf>(objectItem);
            }
            //不存在则新增记录
            else
            {
                await UnitWork.AddAsync<CustomerSeaConf, int>(new CustomerSeaConf
                {
                    PutTime = req.PutTime.TimeOfDay,
                    NotifyTime = req.NotifyTime.TimeOfDay,
                    NotifyDay = req.NotifyDay,

                    CreateDatetime = DateTime.Now,
                    CreateUser = userInfo.User.Name,
                    UpdateDatetime = DateTime.Now,
                    UpdateUser = userInfo.User.Name,
                });
            }

            //修改拉取客户进入公海的时间(定时任务运行时间)
            var job1 = await UnitWork.FindSingleAsync<OpenJob>(o => o.JobCall == "OpenAuth.App.Jobs.CustomerSeaJob");
            job1.Cron = $"{req.PutTime.Second} {req.PutTime.Minute} {req.PutTime.Hour} * * ?";
            job1.Status = 0;
            await UnitWork.UpdateAsync<OpenJob>(job1);

            //修改向业务员发消息的时间(定时任务运行时间)
            var job2 = await UnitWork.FindSingleAsync<OpenJob>(o => o.JobCall == "OpenAuth.App.Jobs.PushMessage");
            job2.Cron = $"{req.NotifyTime.Second} {req.NotifyTime.Minute} {req.NotifyTime.Hour} * * ?";
            job2.Status = 0;
            await UnitWork.UpdateAsync<OpenJob>(job2);
            await UnitWork.SaveAsync();

            _openJobApp.ChangeJobStatus(new App.Request.ChangeJobStatusReq
            {
                Id = job1.Id,
                Status = 0,
            });
            _openJobApp.ChangeJobStatus(new App.Request.ChangeJobStatusReq
            {
                Id = job2.Id,
                Status = 0,
            });

            return result;
        }


        /// <summary>
        /// 公海回收机制设置
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> CustomerSeaRecovery(CustomerSeaRecoveryObjectReq req)
        {
            var result = new Infrastructure.Response();

            var userInfo = _auth.GetCurrentUser();
            //判断设置是否存在
            var isExist = UnitWork.Find<CustomerSeaConf>(null).Any();
            //存在则更新部分记录
            if (isExist)
            {
                var objectItem = UnitWork.Find<CustomerSeaConf>(null).First();
                objectItem.RecoverNoPrice = req.NoPriceDay;
                objectItem.RecoverNoOrder = req.NoOrderDay;

                objectItem.UpdateDatetime = DateTime.Now;
                objectItem.UpdateUser = userInfo.User.Name;

                //每次修改,都将任务设置为不开启,修改完之后再手动开启
                objectItem.RecoverEnable = false;

                await UnitWork.UpdateAsync<CustomerSeaConf>(objectItem);
                await UnitWork.SaveAsync();
            }
            //不存在则新增记录
            else
            {
                await UnitWork.AddAsync<CustomerSeaConf, int>(new CustomerSeaConf
                {
                    RecoverNoPrice = req.NoPriceDay,
                    RecoverNoOrder = req.NoOrderDay,

                    CreateDatetime = DateTime.Now,
                    CreateUser = userInfo.User.Name,
                    UpdateDatetime = DateTime.Now,
                    UpdateUser = userInfo.User.Name,
                });
                await UnitWork.SaveAsync();
            }

            //修改回收客户进入公海的时间(定时任务运行时间)
            var job1 = await UnitWork.FindSingleAsync<OpenJob>(o => o.JobCall == "OpenAuth.App.Jobs.RecoveryCustomer");
            job1.Status = 0;
            await UnitWork.UpdateAsync<OpenJob>(job1);
            await UnitWork.SaveAsync();

            _openJobApp.ChangeJobStatus(new App.Request.ChangeJobStatusReq
            {
                Id = job1.Id,
                Status = 0,
            });

            return result;
        }


        /// <summary>
        /// 公海认领规则
        /// </summary>
        /// <returns></returns>
        public async Task<Infrastructure.Response> ClaimRules(ClaimRulesObjectReq req)
        {
            var result = new Infrastructure.Response();

            var userInfo = _auth.GetCurrentUser();
            //判断设置是否存在
            var isExist = UnitWork.Find<CustomerSeaConf>(null).Any();
            //存在则更新部分记录
            if (isExist)
            {
                var objectItem = UnitWork.Find<CustomerSeaConf>(null).First();
                objectItem.ReceiveMaxLimit = req.ReceiveMaxLimit;
                objectItem.ReceiveJobMax = req.ReceiveJobMax;
                objectItem.ReceiveJobMin = req.ReceiveJobMin;

                objectItem.UpdateDatetime = DateTime.Now;
                objectItem.UpdateUser = userInfo.User.Name;

                await UnitWork.UpdateAsync<CustomerSeaConf>(objectItem);
                await UnitWork.SaveAsync();
            }
            //不存在则新增记录
            else
            {
                await UnitWork.AddAsync<CustomerSeaConf, int>(new CustomerSeaConf
                {
                    ReceiveMaxLimit = req.ReceiveMaxLimit,
                    ReceiveJobMax = req.ReceiveJobMax,
                    ReceiveJobMin = req.ReceiveJobMin,

                    CreateDatetime = DateTime.Now,
                    CreateUser = userInfo.User.Name,
                    UpdateDatetime = DateTime.Now,
                    UpdateUser = userInfo.User.Name,
                });
                await UnitWork.SaveAsync();
            }

            return result;
        }


        /// <summary>
        /// 主动掉入公海限制
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> AutomaticLimit(AutomaticObjectReq req)
        {
            var result = new Infrastructure.Response();

            var userInfo = _auth.GetCurrentUser();
            //判断设置是否存在
            var isExist = UnitWork.Find<CustomerSeaConf>(null).Any();
            //存在则更新部分记录
            if (isExist)
            {
                var objectItem = UnitWork.Find<CustomerSeaConf>(null).First();
                objectItem.AutomaticDayLimit = req.AutomaticDayLimit;
                objectItem.AutomaticLimit = req.AutomaticLimit;

                objectItem.UpdateDatetime = DateTime.Now;
                objectItem.UpdateUser = userInfo.User.Name;

                await UnitWork.UpdateAsync<CustomerSeaConf>(objectItem);
                await UnitWork.SaveAsync();
            }
            //不存在则新增记录
            else
            {
                await UnitWork.AddAsync<CustomerSeaConf, int>(new CustomerSeaConf
                {
                    AutomaticDayLimit = req.AutomaticDayLimit,
                    AutomaticLimit = req.AutomaticLimit,

                    CreateDatetime = DateTime.Now,
                    CreateUser = userInfo.User.Name,
                    UpdateDatetime = DateTime.Now,
                    UpdateUser = userInfo.User.Name,
                });
                await UnitWork.SaveAsync();
            }

            return result;
        }


        /// <summary>
        /// 掉入公海后抢回限制
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> GetBackLimit(GetBackLimitObjectReq req)
        {
            var result = new Infrastructure.Response();

            var userInfo = _auth.GetCurrentUser();
            //判断设置是否存在
            var isExist = UnitWork.Find<CustomerSeaConf>(null).Any();
            //存在则更新部分记录
            if (isExist)
            {
                var objectItem = UnitWork.Find<CustomerSeaConf>(null).First();
                objectItem.BackDay = req.BackDay;

                objectItem.UpdateDatetime = DateTime.Now;
                objectItem.UpdateUser = userInfo.User.Name;

                await UnitWork.UpdateAsync<CustomerSeaConf>(objectItem);
                await UnitWork.SaveAsync();
            }
            //不存在则新增记录
            else
            {
                await UnitWork.AddAsync<CustomerSeaConf, int>(new CustomerSeaConf
                {
                    BackDay = req.BackDay,

                    CreateDatetime = DateTime.Now,
                    CreateUser = userInfo.User.Name,
                    UpdateDatetime = DateTime.Now,
                    UpdateUser = userInfo.User.Name,
                });
                await UnitWork.SaveAsync();
            }

            return result;
        }


        /// <summary>
        /// 根据字段修改启用字段
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> EnableObject(EnableObject req)
        {
            var result = new Infrastructure.Response();

            var objectItem = await UnitWork.Find<CustomerSeaConf>(null).FirstOrDefaultAsync();
            if (objectItem != null)
            {
                //是否启用自动放入公海
                if (req.Enable != null)
                {
                    //这是否个启用控制着两个定时任务:1.是否启用拉取符合条件的客户进入公海
                    ////2.向销售员发送提醒信息
                    objectItem.Enable = req.Enable.Value;
                    await UnitWork.UpdateAsync(objectItem);

                    var job = UnitWork.FindSingle<OpenJob>(o => o.JobCall == "OpenAuth.App.Jobs.CustomerSeaJob");
                    var job2 = UnitWork.FindSingle<OpenJob>(o => o.JobCall == "OpenAuth.App.Jobs.PushMessage");

                    if (req.Enable == false) //停止
                    {
                        job.Status = 0;
                        //job2.Status = 0;
                        await UnitWork.SaveAsync();

                        _openJobApp.ChangeJobStatus(new App.Request.ChangeJobStatusReq
                        {
                            Id = job.Id,
                            Status = 0
                        });

                        _openJobApp.ChangeJobStatus(new App.Request.ChangeJobStatusReq
                        {
                            Id = job2.Id,
                            Status = 0
                        });
                    }
                    else //启动
                    {
                        job.Status = 1;
                        job2.Status = 1;
                        await UnitWork.SaveAsync();

                        _openJobApp.ChangeJobStatus(new App.Request.ChangeJobStatusReq
                        {
                            Id = job.Id,
                            Status = 1
                        });

                        _openJobApp.ChangeJobStatus(new App.Request.ChangeJobStatusReq
                        {
                            Id = job2.Id,
                            Status = 1
                        });
                    }
                }
                //是否启用公海回收机制
                else if (req.RecoverEnable != null)
                {
                    objectItem.RecoverEnable = req.RecoverEnable.Value;
                    await UnitWork.UpdateAsync(objectItem);

                    var job = UnitWork.FindSingle<OpenJob>(o => o.JobCall == "OpenAuth.App.Jobs.RecoveryCustomer");
                    if (req.RecoverEnable == false) //停止
                    {
                        job.Status = 0;
                        await UnitWork.SaveAsync();

                        _openJobApp.ChangeJobStatus(new App.Request.ChangeJobStatusReq
                        {
                            Id = job.Id,
                            Status = 0
                        });
                    }
                    else //启动
                    {
                        job.Status = 1;
                        await UnitWork.SaveAsync();

                        _openJobApp.ChangeJobStatus(new App.Request.ChangeJobStatusReq
                        {
                            Id = job.Id,
                            Status = 1
                        });
                    }
                }
                //是否启用公海认领分配规则
                else if (req.ReceiveEnable != null)
                {
                    objectItem.ReceiveEnable = req.ReceiveEnable.Value;
                    await UnitWork.UpdateAsync(objectItem);
                    await UnitWork.SaveAsync();
                }
                //是否启用主动掉入公海限制
                else if (req.AutomaticEnable != null)
                {
                    objectItem.AutomaticEnable = req.AutomaticEnable.Value;
                    await UnitWork.UpdateAsync(objectItem);
                    await UnitWork.SaveAsync();
                }
                //是否启用掉入公海后抢回限制
                else if (req.BackEnable != null)
                {
                    objectItem.BackEnable = req.BackEnable.Value;
                    await UnitWork.UpdateAsync(objectItem);
                    await UnitWork.SaveAsync();
                }
            }

            return result;
        }

        /// <summary>
        /// 查询公海设置信息
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetCustomerSeaConfig()
        {
            var result = new TableData();

            //存在则返回第一条记录
            if (UnitWork.Find<CustomerSeaConf>(null).Any())
            {
                result.Data = await UnitWork.Find<CustomerSeaConf>(null).FirstOrDefaultAsync();
                result.Count = await UnitWork.Find<CustomerSeaConf>(null).CountAsync();
            }
            else
            {
                result.Data = new QueryCustomerSeaConfigResponse
                {
                    PutTime = DateTime.Now.Date.TimeOfDay,
                    NotifyTime = DateTime.Now.Date.TimeOfDay,
                    NotifyDay = 0,
                    Enable = false,

                    RecoverNoPrice = 0,
                    RecoverNoOrder = 0,
                    RecoverEnable = false,

                    ReceiveMaxLimit = 0,
                    ReceiveJobMax = 0,
                    ReceiveJobMin = 0,
                    ReceiveEnable = false,

                    AutomaticDayLimit = 0,
                    AutomaticLimit = 0,
                    AutomaticEnable = false,

                    BackDay = 0,
                    BackEnable = false
                };
            }

            return result;
            //return await UnitWork.Find<CustomerSeaConf>(null).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 修改公海通用设置
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> UpdateObject(UpdateCustomerSeaConfigReq req)
        {
            var response = new Infrastructure.Response();

            var userInfo = _auth.GetCurrentUser();
            //判断设置是否存在
            var objectItem = await UnitWork.Find<CustomerSeaConf>(null).AsNoTracking().FirstOrDefaultAsync();
            if (objectItem != null)
            {
                //自动放入公海
                objectItem.PutTime = req.PutTime.TimeOfDay;
                objectItem.NotifyTime = req.NotifyTime.TimeOfDay;
                objectItem.NotifyDay = req.NotifyDay;
                objectItem.Enable = req.Enable;

                //公海认领分配
                objectItem.ReceiveMaxLimit = req.ReceiveMaxLimit;
                objectItem.ReceiveJobMax = req.ReceiveJobMax;
                objectItem.ReceiveJobMin = req.ReceiveJobMin;
                objectItem.ReceiveEnable = req.ReceiveEnable;

                //主动掉入规则
                objectItem.AutomaticDayLimit = req.AutomaticDayLimit;
                objectItem.AutomaticEnable = req.AutomaticEnable;

                //掉入公海抢回限制
                objectItem.BackDay = req.BackDay;
                objectItem.BackEnable = req.BackEnable;

                objectItem.UpdateDatetime = DateTime.Now;
                objectItem.UpdateUser = userInfo.User.Name;

                await UnitWork.UpdateAsync<CustomerSeaConf>(objectItem);
                await UnitWork.SaveAsync();
            }
            else
            {
                await UnitWork.AddAsync<CustomerSeaConf, int>(new CustomerSeaConf
                {
                    //自动放入公海
                    PutTime = req.PutTime.TimeOfDay,
                    NotifyTime = req.NotifyTime.TimeOfDay,
                    NotifyDay = req.NotifyDay,
                    Enable = req.Enable,

                    //公海认领分配
                    ReceiveMaxLimit = req.ReceiveMaxLimit,
                    ReceiveJobMax = req.ReceiveJobMax,
                    ReceiveJobMin = req.ReceiveJobMin,
                    ReceiveEnable = req.ReceiveEnable,

                    //掉入公海抢回限制
                    BackDay = req.BackDay,
                    BackEnable = req.BackEnable,

                    //主动掉入规则
                    AutomaticDayLimit = req.AutomaticDayLimit,
                    AutomaticEnable = req.AutomaticEnable,

                    //创建人信息
                    CreateDatetime = DateTime.Now,
                    CreateUser = userInfo.User.Name,
                    UpdateDatetime = DateTime.Now,
                    UpdateUser = userInfo.User.Name,

                });
                await UnitWork.SaveAsync();
            }

            //都先设为停止,再启动,否则规则不会立刻生效
            //拉取缓存中的客户进入表
            var job1 = await UnitWork.Find<OpenJob>(o => o.JobCall == "OpenAuth.App.Jobs.RecoveryCustomer").AsNoTracking().FirstOrDefaultAsync();
            //var job1 = await UnitWork.FindSingleAsync<OpenJob>(o => o.JobCall == "OpenAuth.App.Jobs.CustomerSeaJob");
            job1.Cron = $"{req.PutTime.Second} {req.PutTime.Minute} {req.PutTime.Hour} * * ?";
            job1.Status = 0;

            //向业务员发送所属客户即将掉入公海的提醒
            var job2 = await UnitWork.Find<OpenJob>(o => o.JobCall == "OpenAuth.App.Jobs.PushMessage").AsNoTracking().FirstOrDefaultAsync();
            job2.Cron = $"{req.NotifyTime.Second} {req.NotifyTime.Minute} {req.NotifyTime.Hour} * * ?";
            job2.Status = 0;

            //停止定时任务
            Action<OpenJob> deleteTriggerKey = (j) =>
            {
                TriggerKey triggerKey = new TriggerKey(j.Id);
                // 停止触发器
                _scheduler.PauseTrigger(triggerKey);
                // 移除触发器
                _scheduler.UnscheduleJob(triggerKey);
                // 删除任务
                _scheduler.DeleteJob(new JobKey(j.Id));
            };

            //启动定时任务
            Action<OpenJob> buildTriggerKey = (j) =>
            {
                var jobBuilderType = typeof(JobBuilder);
                var method = jobBuilderType.GetMethods().FirstOrDefault(
                        x => x.Name.Equals("Create", StringComparison.OrdinalIgnoreCase) &&
                             x.IsGenericMethod && x.GetParameters().Length == 0)
                    ?.MakeGenericMethod(Type.GetType(j.JobCall));

                var jobBuilder = (JobBuilder)method.Invoke(null, null);

                IJobDetail jobDetail = jobBuilder.WithIdentity(j.Id).Build();
                jobDetail.JobDataMap[Define.JOBMAPKEY] = j.Id;  //传递job信息
                ITrigger trigger = TriggerBuilder.Create()
                    .WithCronSchedule(j.Cron)
                    .WithIdentity(j.Id)
                    .StartNow()
                    .Build();
                _scheduler.ScheduleJob(jobDetail, trigger);
            };

            deleteTriggerKey(job1);
            deleteTriggerKey(job2);

            //启动
            if (req.Enable == true)
            {
                job1.Status = 1;
                job2.Status = 1;

                buildTriggerKey(job1);
                buildTriggerKey(job2);
            }

            await UnitWork.UpdateAsync(job1);
            await UnitWork.UpdateAsync(job2);
            await UnitWork.SaveAsync();

            return response;
        }
    }
}
