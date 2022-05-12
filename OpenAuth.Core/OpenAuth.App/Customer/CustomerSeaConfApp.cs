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
        public CustomerSeaConfApp(IUnitWork unitWork, IAuth auth, OpenJobApp openJobApp) : base(unitWork, auth)
        {
            _openJobApp = openJobApp;
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
                    //这是否个启用控制着两个定时任务:1.是否启用拉取符合条件的客户进入公海 2.向销售员发送提醒信息
                    objectItem.Enable = req.Enable.Value;
                    var job = UnitWork.FindSingle<OpenJob>(o => o.JobCall == "OpenAuth.App.Jobs.CustomerSeaJob");
                    var job2 = UnitWork.FindSingle<OpenJob>(o => o.JobCall == "OpenAuth.App.Jobs.PushMessage");
                    if (req.Enable == false) //停止
                    {
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
                    var job = UnitWork.FindSingle<OpenJob>(o => o.JobCall == "OpenAuth.App.Jobs.RecoveryCustomer");
                    if (req.Enable == false) //停止
                    {
                        _openJobApp.ChangeJobStatus(new App.Request.ChangeJobStatusReq
                        {
                            Id = job.Id,
                            Status = 0
                        });
                    }
                    else //启动
                    {
                        _openJobApp.ChangeJobStatus(new App.Request.ChangeJobStatusReq
                        {
                            Id = job.Id,
                            Status = 0
                        });
                    }
                }
                //是否启用公海认领分配规则
                else if (req.ReceiveEnable != null)
                {
                    objectItem.ReceiveEnable = req.ReceiveEnable.Value;
                }
                //是否启用主动掉入公海限制
                else if (req.AutomaticEnable != null)
                {
                    objectItem.AutomaticEnable = req.AutomaticEnable.Value;
                }
                //是否启用掉入公海后抢回限制
                else if (req.BackEnable != null)
                {
                    objectItem.BackEnable = req.BackEnable.Value;
                }

                await UnitWork.UpdateAsync(objectItem);
                await UnitWork.SaveAsync();
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
    }
}
