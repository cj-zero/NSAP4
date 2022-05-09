﻿using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OpenAuth.App.Customer.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.App.Response;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Customer.Response;
using Infrastructure;
using Infrastructure.Extensions;
using OpenAuth.Repository.Domain.Customer;
using OpenAuth.Repository.Domain.Sap;

namespace OpenAuth.App.Customer
{
    public class CustomerLimitApp : OnlyUnitWorkBaeApp
    {
        public CustomerLimitApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth) { }

        /// <summary>
        /// 新增or修改组规则
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> AddOrUpdateGroupRule(AddOrUpdateGroupRulesReq req)
        {
            var response = new Infrastructure.Response();

            var userInfo = _auth.GetCurrentUser();
            var userName = userInfo.User.Name;

            var rules = req.Rules.Select(r => new CustomerLimitRule
            {
                CustomerType = int.Parse(r.CustomerTypeId),
                Limit = r.Limit,
                CreateUser = userName,
                CreateDatetime = DateTime.Now,
                UpdateUser = userName,
                UpdateDatetime = DateTime.Now,
            }).ToList();

            var customerLimit = new CustomerLimit
            {
                Name = req.GroupName,
                Describe = "",
                CreateUser = userName,
                CreateDatetime = DateTime.Now,
                UpdateUser = userName,
                UpdateDatetime = DateTime.Now,
            };

            //新增操作
            if (req.Id == null || req.Id <= 0)
            {
                using var tran = UnitWork.GetDbContext<CustomerLimit>().Database.BeginTransaction();
                try
                {
                    if (UnitWork.Find<CustomerLimit>(c => c.Name == req.GroupName).Any())
                    {
                        response.Message = "分组名称已存在";
                        response.Code = 500;
                        return response;
                    }

                    await UnitWork.AddAsync<CustomerLimit, int>(customerLimit);
                    await UnitWork.SaveAsync();
                    //把子类的规则id补上
                    rules.All(r =>
                    {
                        r.CustomerLimitId = customerLimit.Id;
                        return true;
                    });
                    await UnitWork.BatchAddAsync<CustomerLimitRule, int>(rules.ToArray());
                    await UnitWork.SaveAsync();
                    await tran.CommitAsync();
                }
                catch (Exception ex)
                {
                    await tran.RollbackAsync();
                    response.Code = 500;
                    response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                }
            }
            //修改操作
            else
            {
                using var tran = UnitWork.GetDbContext<CustomerLimit>().Database.BeginTransaction();
                try
                {
                    var instance = await UnitWork.Find<CustomerLimit>(null).FirstOrDefaultAsync();
                    instance.Name = req.GroupName;
                    instance.UpdateUser = userName;
                    instance.UpdateDatetime = DateTime.Now;

                    await UnitWork.UpdateAsync<CustomerLimit>(instance);

                    //规则先删除,再新增
                    await UnitWork.DeleteAsync<CustomerLimitRule>(c => c.CustomerLimitId == req.Id);
                    rules.All(r =>
                    {
                        r.CustomerLimitId = req.Id.Value;
                        return true;
                    });
                    await UnitWork.BatchAddAsync<CustomerLimitRule, int>(rules.ToArray());

                    await UnitWork.SaveAsync();
                    await tran.CommitAsync();
                }
                catch (Exception ex)
                {
                    await tran.RollbackAsync();
                    response.Code = 500;
                    response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                }
            }

            return response;
        }

        /// <summary>
        /// 查询组规则列表
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetGroupRules(QueryCustomerLimitReq req)
        {
            var result = new TableData();

            var query = UnitWork.Find<CustomerLimit>(null)
                .WhereIf(req.Id != null && req.Id > 0, c => c.Id == req.Id)
                .Include(x => x.CustomerLimitRules)
                .Include(x => x.CustomerLimitSalers)
                .OrderByDescending(x => x.UpdateDatetime)
                .Select(x => new QueryCustomerLimitResponse
                {
                    GroupId = x.Id,
                    GroupName = x.Name,
                    IsDelete = x.Isdelete,
                    RuleResponses = x.CustomerLimitRules.Select(r => new RuleResponse { CustomerType = r.CustomerType, Limit = r.Limit }),
                    SalerResponses = x.CustomerLimitSalers.Select(s => new SalerResponse { SalerId = s.SalerId, SalerName = s.SalerName })
                });

            result.Data = await query.Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            result.Count = await query.CountAsync();

            return result;
        }

        /// <summary>
        /// 删除规则组
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> DeleteGroupRule(DeleteGroupRuleReq req)
        {
            var response = new Infrastructure.Response();

            using var tran = UnitWork.GetDbContext<CustomerLimit>().Database.BeginTransaction();
            try
            {
                await UnitWork.DeleteAsync<CustomerLimit>(c => c.Id == req.Id);
                await UnitWork.DeleteAsync<CustomerLimitRule>(c => c.CustomerLimitId == req.Id);
                await UnitWork.DeleteAsync<CustomerLimitSaler>(c => c.CustomerLimitId == req.Id);

                await UnitWork.SaveAsync();
                await tran.CommitAsync();
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();

                response.Code = 500;
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
            }

            return response;
        }

        /// <summary>
        /// 启用or禁用组规则
        /// </summary>
        /// <returns></returns>
        public async Task<Infrastructure.Response> EnableGroupRole(EnableGroupRoleReq req)
        {
            var response = new Infrastructure.Response();

            var obj = UnitWork.Find<CustomerLimit>(c => c.Id == req.Id).FirstOrDefault();
            obj.Isdelete = req.Enable;
            await UnitWork.UpdateAsync(obj);
            await UnitWork.SaveAsync();

            return response;
        }

        /// <summary>
        /// 新增or修改用户组用户
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> UpdateGroupUser(AddOrUpdateGroupUsersReq req)
        {
            var response = new Infrastructure.Response();

            var userInfo = _auth.GetCurrentUser();
            var userName = userInfo.User.Name;

            var users = req.Users.Select(u => new CustomerLimitSaler
            {
                CustomerLimitId = req.Id,
                SalerId = u.UserId,
                SalerName = u.UserName,
                CreateUser = userName,
                CreateDatetime = DateTime.Now,
                UpdateUser = userName,
                UpdateDatetime = DateTime.Now,
            });

            using var tran = UnitWork.GetDbContext<CustomerLimit>().Database.BeginTransaction();
            try
            {
                await UnitWork.DeleteAsync<CustomerLimitSaler>(c => c.CustomerLimitId == req.Id);
                await UnitWork.BatchAddAsync<CustomerLimitSaler, int>(users.ToArray());

                await UnitWork.SaveAsync();
                await tran.CommitAsync();
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();

                response.Code = 500;
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
            }

            return response;
        }

        /// <summary>
        /// 拉取符合掉落规则的客户进入公海
        /// </summary>
        /// <returns></returns>
        public async Task AsyncCustomerStatusService()
        {
            //查询规则列表,按部门和客户类型分类,事件优先级:0-未报价>1-未下单>2-未交货
            var query = from c in UnitWork.Find<CustomerSeaRule>(null)
                        join ci in UnitWork.Find<CustomerSeaRuleItem>(null)
                        on c.Id equals ci.CustomerSeaRuleId
                        //true 代表启用
                        //目前的需求是先处理未报价客户未报价的规则
                        where c.Enable == true && ci.CustomerType == 1 && ci.OrderType == 0 
                        group new { c, ci } by new { ci.DepartmentName, ci.CustomerType } into g
                        select new
                        {
                            dept = g.Key.DepartmentName,
                            customerType = g.Key.CustomerType,
                            orderType = g.Min(x => x.ci.OrderType), //事件优先级越高,数值越小
                            day = g.Max(x => x.ci.Day) //优先级高的天数一定会比优先级低的大
                        };
            var ruleData = await query.ToListAsync();

            //提前通知天数
            var notifyDay = UnitWork.Find<CustomerSeaConf>(null).FirstOrDefault()?.NotifyDay; 
            //符合掉落公海规则的客户
            var customerLists = new List<CustomerList>();

            foreach (var rule in ruleData)
            {
                //根据部门查找业务员
                var slpInfo = await (from u in UnitWork.Find<base_user>(null)
                                     join ud in UnitWork.Find<base_user_detail>(d => new int[] { 0, 1 }.Contains(d.status)) //在职的员工,离职状态是2和3
                                     on u.user_id equals ud.user_id
                                     join d in UnitWork.Find<base_dep>(null)
                                     .WhereIf(rule.dept != "All", d => d.dep_alias == rule.dept) //all为公司全体
                                     on ud.dep_id equals d.dep_id
                                     join s in UnitWork.Find<sbo_user>(null)
                                     on u.user_id equals s.user_id
                                     select s.sale_id).Distinct().ToListAsync();
                //再根据业务员查找客户
                var query2 = from c in UnitWork.Find<OCRD>(null)
                             join s in UnitWork.Find<OSLP>(null)
                             on c.SlpCode equals s.SlpCode
                             where slpInfo.Select(s => s).Contains(s.SlpCode)
                             select new { c.CardCode, c.CardName, c.CreateDate, s.SlpCode, s.SlpName };
                //去掉白名单上的客户 
                var whiteList = await UnitWork.Find<SpecialCustomer>(s => s.Type == 1).Select(s => s.CustomerNo).ToListAsync();
                var query3 = query2.Where(c => !whiteList.Contains(c.CardCode));
                //根据客户类型查找客户
                if (rule.customerType == 1) //未报价客户
                {
                    var cardInfoData = await query3.Where(c => !UnitWork.Find<OQUT>(null).Any(q => q.CardCode == c.CardCode)).ToListAsync();
                    //未报价客户的行为类型只能是未报价
                    if (rule.orderType == 0)
                    {
                        //计算从分配给最新的业务员开始到现在过了多长时间
                        foreach (var c in cardInfoData)
                        {
                            DateTime? startTime = null;
                            //查找客户最近一次的业务员变更(查找有不同业务员的客户)
                            var acrdInfos = await UnitWork.Find<ACRD>(a => a.CardCode == c.CardCode).Select(c => new { c.SlpCode, c.UpdateDate }).ToListAsync();
                            var hasDiffClient = acrdInfos.Select(x => x.SlpCode).Distinct().Count() > 1;
                            //如果有不同的业务员,则开始时间取最近一次的客户分配给业务员的时间
                            if (hasDiffClient)
                            {
                                startTime = UnitWork.Find<ACRD>(a => a.CardCode == c.CardCode).Max(c => c.UpdateDate);
                            }
                            //否则说明该客户的业务员无变动,取该客户的创建时间
                            else
                            {
                                startTime = UnitWork.Find<OCRD>(a => a.CardCode == c.CardCode).Max(c => c.CreateDate);
                            }
                            //var lastClientChangeTime = (await UnitWork.Find<ACRD>(a => a.CardCode == c.CardCode).Select(c => new { c.SlpCode, c.UpdateDate }).ToListAsync()).GroupBy(a => a.SlpCode).Where(g => g.Select(x => x.SlpCode).Distinct().Count() > 1).Select(g => g.Max(x => x.UpdateDate)).FirstOrDefault();
                            ////判断是客户否存在业务员变更,存在则取最近一次的变更时间,不存在则取客户的创建时间
                            //var startTime = lastClientChangeTime ?? c.CreateDate;
                            //超过规则定义的天数则放入公海
                            if (startTime != null && (DateTime.Now - startTime).Value.Days > rule.day)
                            {
                                customerLists.Add(new CustomerList
                                {
                                    DepartMent = rule.dept,
                                
                                    CustomerNo = c.CardCode,
                                    CustomerName = c.CardName,
                                    CustomerSource = "",
                                    SlpCode = c.SlpCode,
                                    SalerName = c.SlpName,
                                    Label = "公海领取",
                                    LabelIndex = 3,
                                    CreateUser = "系统",
                                    CreateDateTime = DateTime.Now,
                                    UpdateUser = "系统",
                                    UpdateDateTime = DateTime.Now,
                                    IsDelete = false
                                });
                            }
                            //超过规则定义的,需提前n天通知
                            else if (startTime != null && (DateTime.Now - startTime).Value.Days > rule.day - notifyDay)
                            {
                                customerLists.Add(new CustomerList
                                {
                                    DepartMent = rule.dept,
                                    CustomerNo = c.CardCode,
                                    CustomerName = c.CardName,
                                    CustomerSource = "",
                                    SlpCode = c.SlpCode,
                                    SalerName = c.SlpName,
                                    Label = "即将掉入公海",
                                    LabelIndex = 4,
                                    CreateUser = "系统",
                                    CreateDateTime = DateTime.Now,
                                    UpdateUser = "系统",
                                    UpdateDateTime = DateTime.Now,
                                    IsDelete = false
                                });
                            }
                        }
                    }
                }
                //else if (rule.customerType == 2) //已成交客户
                //{
                //    cardInfo = cardInfo.Where(c => UnitWork.Find<ODLN>(null).Any(d => d.CardCode == c.CardCode));
                //    foreach (var c in cardInfo)
                //    {
                //        //获取该客户最近一次的做单时间
                //        DateTime? lastDate = null;
                //        if (rule.orderType == 0) //报价单
                //        {
                //            lastDate = await UnitWork.Find<OQUT>(q => q.CardCode == c.CardCode).OrderByDescending(q => q.DocEntry).Select(q => q.CreateDate).FirstOrDefaultAsync();
                //        }
                //        else if(rule.orderType == 1) //销售单
                //        {
                //            lastDate = await UnitWork.Find<ORDR>(q => q.CardCode == c.CardCode).OrderByDescending(q => q.DocEntry).Select(q => q.CreateDate).FirstOrDefaultAsync();
                //        }
                //        else if (rule.orderType == 2) //交货单
                //        {
                //            lastDate = await UnitWork.Find<ODLN>(q => q.CardCode == c.CardCode).OrderByDescending(q => q.DocEntry).Select(q => q.CreateDate).FirstOrDefaultAsync();
                //        }

                //        if (lastDate != null && (DateTime.Now - lastDate).Value.TotalDays > rule.day)
                //        {
                //            customerLists.Add(new CustomerList { CustomerNo = c.CardCode, CustomerName = c.CardName, DepartmentId = rule.dept, DepartmentName = rule.dept });
                //        }
                //    }
                //}

            }
            //数据库数据
            var databaseData = await UnitWork.Find<CustomerList>(null).Select(c => new { c.CustomerNo, c.Id }).ToListAsync();
            //表里面没有的数据要新增
            var insertData = customerLists.Select(c => c.CustomerNo).Except(databaseData.Select(d => d.CustomerNo));
            //两边都有的数据要更新
            var updateData = customerLists.Select(c => c.CustomerNo).Intersect(databaseData.Select(d => d.CustomerNo));
            //表里面多的要删除
            var deleteData = databaseData.Select(d => d.CustomerNo).Except(customerLists.Select(c => c.CustomerNo));

            await UnitWork.BatchAddAsync<CustomerList, int>(customerLists.Where(c => insertData.Contains(c.CustomerNo)).ToArray());
            await UnitWork.DeleteAsync<CustomerList>(c => deleteData.Contains(c.CustomerNo));
            foreach (var item in updateData)
            {
                var instance = await UnitWork.Find<CustomerList>(null).FirstOrDefaultAsync(c => c.CustomerNo == item);
                instance.LabelIndex = customerLists.FirstOrDefault(c => c.CustomerNo == item).LabelIndex;
                instance.Label = customerLists.FirstOrDefault(c => c.CustomerNo == item).Label;
                instance.SlpCode = customerLists.FirstOrDefault(c => c.CustomerNo == item).SlpCode;
                instance.SalerName = customerLists.FirstOrDefault(c => c.CustomerNo == item).SalerName;
                instance.UpdateDateTime = DateTime.Now;
                instance.UpdateUser = "系统";
                await UnitWork.UpdateAsync<CustomerList>(instance);
                //await UnitWork.SaveAsync();
            }

            await UnitWork.SaveAsync();
        }

    }
}
