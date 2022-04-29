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

namespace OpenAuth.App.Customer
{
    public class CustomerSeaConfApp : OnlyUnitWorkBaeApp
    {
        public CustomerSeaConfApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth) { }

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
                    PutTime = req.PutTime.TimeOfDay,
                    NotifyTime = req.NotifyTime.TimeOfDay,
                    NotifyDay = req.NotifyDay,

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
                if (req.Enable != null)
                {
                    objectItem.Enable = req.Enable.Value;
                }
                else if (req.RecoverEnable != null)
                {
                    objectItem.RecoverEnable = req.RecoverEnable.Value;
                }
                else if(req.ReceiveEnable != null)
                {
                    objectItem.ReceiveEnable = req.ReceiveEnable.Value;
                }
                else if(req.AutomaticEnable != null)
                {
                    objectItem.AutomaticEnable = req.AutomaticEnable.Value;
                }
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
