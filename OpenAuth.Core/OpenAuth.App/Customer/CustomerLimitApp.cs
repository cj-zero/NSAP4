using OpenAuth.App.Interface;
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
using Microsoft.AspNetCore.SignalR;
using OpenAuth.App.SignalR;
using OpenAuth.Repository;
using System.Data;
using OpenAuth.Repository.Domain.Serve;

namespace OpenAuth.App.Customer
{
    public class CustomerLimitApp : OnlyUnitWorkBaeApp
    {
        private readonly IHubContext<MessageHub> _hubContext;
        public CustomerLimitApp(IUnitWork unitWork, IAuth auth, IHubContext<MessageHub> hubContext) : base(unitWork, auth)
        {
            _hubContext = hubContext;
        }

        public async Task<Infrastructure.Response> AddGroupRule(AddOrUpdateGroupRulesReq req)
        {
            var response = new Infrastructure.Response();
            response.Message = "";
            //前端那边传过来数量为0的规则过滤掉
            req.Rules = req.Rules.Where(r => r.Limit != 0).ToList();
            //全部客户规则与其他规则不共存
            if (req.Rules.Any(r => r.CustomerTypeId == "0") && req.Rules.Any(r => r.CustomerTypeId != "0"))
            {
                response.Message = "全部客户规则与其他规则不能同时存在";
                response.Code = 500;
                return response;
            }

            //判断用户是否已存在规则组中
            foreach (var user in req.Users)
            {
                var ruleName = await (from l in UnitWork.Find<CustomerLimit>(null)
                                      join s in UnitWork.Find<CustomerLimitSaler>(null) on l.Id equals s.CustomerLimitId
                                      where s.SalerId == user.SlpCode
                                      select l.Name).FirstOrDefaultAsync();
                if (ruleName != null)
                {
                    response.Message += $"{user.UserName}已存在规则组:{ruleName} \n";
                }
            }
            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                response.Code = 500;
                return response;
            }

            //判断用户的客户数是否已超过规则的限制
            foreach (var user in req.Users)
            {
                //如果有全部客户数量限制
                if (req.Rules.Any(r => r.CustomerTypeId == "0"))
                {
                    var rule = req.Rules.First(r => r.CustomerTypeId == "0");
                    var count = await UnitWork.Find<OCRD>(c => c.SlpCode == user.SlpCode).CountAsync();
                    if (count > rule.Limit)
                    {
                        response.Message += $"{user.UserName}的全部客户数量限制为{rule.Limit}个,现有客户{count}个 \n";
                        continue;
                    }
                }
                //如果有未报价客户数量限制
                if (req.Rules.Any(r => r.CustomerTypeId == "1"))
                {
                    var rule = req.Rules.First(r => r.CustomerTypeId == "1");
                    var count = await (from c in UnitWork.Find<OCRD>(null)
                                       join u in UnitWork.Find<OQUT>(null) on c.CardCode equals u.CardCode into temp
                                       from t in temp.DefaultIfEmpty()
                                       where c.SlpCode == user.SlpCode && t.CardCode == null
                                       select c.CardCode).CountAsync();
                    if (count > rule.Limit)
                    {
                        response.Message += $"{user.UserName}的未报价客户数量限制为{rule.Limit}个,现有未报价客户{count}个 \n";
                        continue;
                    }
                }
                //如果有已报价未转订单客户数限制
                if (req.Rules.Any(r => r.CustomerTypeId == "2"))
                {
                    var rule = req.Rules.First(r => r.CustomerTypeId == "2");
                    //已报价未转销售单
                    var count = await (from c in UnitWork.Find<OCRD>(null)
                                       join u in UnitWork.Find<OQUT>(null) on c.CardCode equals u.CardCode
                                       join r in UnitWork.Find<ORDR>(null) on c.CardCode equals r.CardCode into temp
                                       from t in temp.DefaultIfEmpty()
                                       where c.SlpCode == user.SlpCode && t.CardCode == null
                                       select c.CardCode).CountAsync();
                    if (count > rule.Limit)
                    {
                        response.Message += $"{user.UserName}的已成交客户数限制为{rule.Limit}个,现有已成交客户{count}个 \n";
                        continue;
                    }
                }
                //如果有已有订单未有交货单客户数限制
                if (req.Rules.Any(r => r.CustomerTypeId == "3"))
                {
                    var rule = req.Rules.First(r => r.CustomerTypeId == "3");
                    //已销售未转交货
                    var count = await (from c in UnitWork.Find<OCRD>(null)
                                       join r in UnitWork.Find<ORDR>(null) on c.CardCode equals r.CardCode
                                       join d in UnitWork.Find<ODLN>(null) on c.CardCode equals d.CardCode into temp
                                       from t in temp.DefaultIfEmpty()
                                       where c.SlpCode == user.SlpCode && t.CardCode == null
                                       select c.CardCode).CountAsync();
                    if (count > rule.Limit)
                    {
                        response.Message += $"{user.UserName}的未转销售订单(已报价)客户数限制为{rule.Limit}个,现有未转销售订单(已报价)客户{count}个 \n";
                        continue;
                    }
                }
                //如果有已成交客户数限制
                if (req.Rules.Any(r => r.CustomerTypeId == "4"))
                {
                    var rule = req.Rules.First(r => r.CustomerTypeId == "4");
                    var count = await (from c in UnitWork.Find<OCRD>(null)
                                       join d in UnitWork.Find<ODLN>(null) on c.CardCode equals d.CardCode
                                       where c.SlpCode == user.SlpCode
                                       select c.CardCode).Distinct().CountAsync();
                    if (count > rule.Limit)
                    {
                        response.Message += $"{user.UserName}的未交货(已转销售订单)客户数限制为{rule.Limit}个,现有未交货(已转销售订单)客户{count}个 \n";
                        continue;
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                response.Code = 500;
                return response;
            }

            var userInfo = _auth.GetCurrentUser();
            var userName = userInfo.User.Name;
            //规则
            var customerLimit = new CustomerLimit
            {
                Name = req.GroupName,
                Describe = req.Remark,
                CreateUser = userName,
                CreateDatetime = DateTime.Now,
                UpdateUser = userName,
                UpdateDatetime = DateTime.Now,
                Isdelete = req.Enable
            };
            //规则明细
            var rules = req.Rules.Select(r => new CustomerLimitRule
            {
                CustomerType = int.Parse(r.CustomerTypeId),
                Limit = r.Limit,
                CreateUser = userName,
                CreateDatetime = DateTime.Now,
                UpdateUser = userName,
                UpdateDatetime = DateTime.Now,
            }).ToList();
            //规则用户
            var users = req.Users.Select(u => new CustomerLimitSaler
            {
                SalerId = u.SlpCode,
                SalerName = u.UserName,
                Dept = u.Dept,
                Account = u.Account,
                CreateUser = userName,
                CreateDatetime = DateTime.Now,
                UpdateUser = userName,
                UpdateDatetime = DateTime.Now,
            }).ToList();

            if (UnitWork.Find<CustomerLimit>(c => c.Name == req.GroupName).Any())
            {
                response.Message = "分组名称已存在";
                response.Code = 500;
                return response;
            }

            using var tran = UnitWork.GetDbContext<CustomerLimit>().Database.BeginTransaction();
            try
            {
                await UnitWork.AddAsync<CustomerLimit, int>(customerLimit);
                await UnitWork.SaveAsync();
                //把子类的规则id补上
                rules.All(r =>
                {
                    r.CustomerLimitId = customerLimit.Id;
                    return true;
                });
                users.All(u =>
                {
                    u.CustomerLimitId = customerLimit.Id;
                    return true;
                });
                await UnitWork.BatchAddAsync<CustomerLimitRule, int>(rules.ToArray());
                await UnitWork.BatchAddAsync<CustomerLimitSaler, int>(users.ToArray());
                await UnitWork.SaveAsync();
                await tran.CommitAsync();

                response.Message = customerLimit.Id.ToString();
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                response.Code = 500;
            }

            return response;
        }

        /// <summary>
        /// 修改组规则
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> UpdateGroupRule(AddOrUpdateGroupRulesReq req)
        {
            var response = new Infrastructure.Response();
            response.Message = "";

            //前端那边传过来数量为0的规则过滤掉
            req.Rules = req.Rules.Where(r => r.Limit != 0).ToList();
            //全部客户规则与其他规则不共存
            if (req.Rules.Any(r => r.CustomerTypeId == "0") && req.Rules.Any(r => r.CustomerTypeId != "0"))
            {
                response.Message = "全部客户规则与其他规则不能同时存在";
                response.Code = 500;
                return response;
            }

            //判断用户是否已存在规则组中
            foreach (var user in req.Users)
            {
                var ruleName = await (from l in UnitWork.Find<CustomerLimit>(null)
                                      join s in UnitWork.Find<CustomerLimitSaler>(null) on l.Id equals s.CustomerLimitId
                                      where s.SalerId == user.SlpCode
                                      select l.Name).FirstOrDefaultAsync();
                if (ruleName != null && ruleName != req.GroupName)
                {
                    response.Message += $"{user.UserName}已存在规则组:{ruleName} \n";
                }
            }
            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                response.Code = 500;
                return response;
            }

            //判断用户的客户数是否已超过规则的限制
            foreach (var user in req.Users)
            {
                //如果有全部客户数量限制
                if (req.Rules.Any(r => r.CustomerTypeId == "0"))
                {
                    var rule = req.Rules.First(r => r.CustomerTypeId == "0");
                    var count = await UnitWork.Find<OCRD>(c => c.SlpCode == user.SlpCode).CountAsync();
                    if (count > rule.Limit)
                    {
                        response.Message += $"{user.UserName}的全部客户数量限制为{rule.Limit}个,现有客户{count}个 \n";
                        continue;
                    }
                }
                //如果有未报价客户数量限制
                if (req.Rules.Any(r => r.CustomerTypeId == "1"))
                {
                    var rule = req.Rules.First(r => r.CustomerTypeId == "1");
                    var count = await (from c in UnitWork.Find<OCRD>(null)
                                       join u in UnitWork.Find<OQUT>(null) on c.CardCode equals u.CardCode into temp
                                       from t in temp.DefaultIfEmpty()
                                       where c.SlpCode == user.SlpCode && t.CardCode == null
                                       select c.CardCode).CountAsync();
                    if (count > rule.Limit)
                    {
                        response.Message += $"{user.UserName}的未报价客户数量限制为{rule.Limit}个,现有未报价客户{count}个 \n";
                        continue;
                    }
                }
                //如果有已报价未转订单客户数限制
                if (req.Rules.Any(r => r.CustomerTypeId == "2"))
                {
                    var rule = req.Rules.First(r => r.CustomerTypeId == "2");
                    //已报价未转销售单
                    var count = await (from c in UnitWork.Find<OCRD>(null)
                                       join u in UnitWork.Find<OQUT>(null) on c.CardCode equals u.CardCode
                                       join r in UnitWork.Find<ORDR>(null) on c.CardCode equals r.CardCode into temp
                                       from t in temp.DefaultIfEmpty()
                                       where c.SlpCode == user.SlpCode && t.CardCode == null
                                       select c.CardCode).CountAsync();
                    if (count > rule.Limit)
                    {
                        response.Message += $"{user.UserName}的已成交客户数限制为{rule.Limit}个,现有已成交客户{count}个 \n";
                        continue;
                    }
                }
                //如果有已有订单未有交货单客户数限制
                if (req.Rules.Any(r => r.CustomerTypeId == "3"))
                {
                    var rule = req.Rules.First(r => r.CustomerTypeId == "3");
                    //已销售未转交货
                    var count = await (from c in UnitWork.Find<OCRD>(null)
                                       join r in UnitWork.Find<ORDR>(null) on c.CardCode equals r.CardCode
                                       join d in UnitWork.Find<ODLN>(null) on c.CardCode equals d.CardCode into temp
                                       from t in temp.DefaultIfEmpty()
                                       where c.SlpCode == user.SlpCode && t.CardCode == null
                                       select c.CardCode).CountAsync();
                    if (count > rule.Limit)
                    {
                        response.Message += $"{user.UserName}的未转销售订单(已报价)客户数限制为{rule.Limit}个,现有未转销售订单(已报价)客户{count}个 \n";
                        continue;
                    }
                }
                //如果有已成交客户数限制
                if (req.Rules.Any(r => r.CustomerTypeId == "4"))
                {
                    var rule = req.Rules.First(r => r.CustomerTypeId == "4");
                    var count = await (from c in UnitWork.Find<OCRD>(null)
                                       join d in UnitWork.Find<ODLN>(null) on c.CardCode equals d.CardCode
                                       where c.SlpCode == user.SlpCode
                                       select c.CardCode).Distinct().CountAsync();
                    if (count > rule.Limit)
                    {
                        response.Message += $"{user.UserName}的未交货(已转销售订单)客户数限制为{rule.Limit}个,现有未交货(已转销售订单)客户{count}个 \n";
                        continue;
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                response.Code = 500;
                return response;
            }

            var userInfo = _auth.GetCurrentUser();
            var userName = userInfo.User.Name;
            //规则
            var customerLimit = new CustomerLimit
            {
                Name = req.GroupName,
                Describe = req.Remark,
                CreateUser = userName,
                CreateDatetime = DateTime.Now,
                UpdateUser = userName,
                UpdateDatetime = DateTime.Now,
                Isdelete = req.Enable
            };
            //规则明细
            var rules = req.Rules.Select(r => new CustomerLimitRule
            {
                CustomerType = int.Parse(r.CustomerTypeId),
                Limit = r.Limit,
                CreateUser = userName,
                CreateDatetime = DateTime.Now,
                UpdateUser = userName,
                UpdateDatetime = DateTime.Now,
            }).ToList();
            //规则用户
            var users = req.Users.Select(u => new CustomerLimitSaler
            {
                SalerId = u.SlpCode,
                SalerName = u.UserName,
                Dept = u.Dept,
                Account = u.Account,
                CreateUser = userName,
                CreateDatetime = DateTime.Now,
                UpdateUser = userName,
                UpdateDatetime = DateTime.Now,
            }).ToList();

            using var tran = UnitWork.GetDbContext<CustomerLimit>().Database.BeginTransaction();
            try
            {
                var instance = await UnitWork.Find<CustomerLimit>(c => c.Id == req.Id).FirstOrDefaultAsync();
                instance.Name = req.GroupName;
                instance.UpdateUser = userName;
                instance.UpdateDatetime = DateTime.Now;
                instance.Isdelete = req.Enable;

                await UnitWork.UpdateAsync<CustomerLimit>(instance);

                //规则先删除,再新增
                await UnitWork.DeleteAsync<CustomerLimitRule>(c => c.CustomerLimitId == req.Id);
                rules.All(r =>
                {
                    r.CustomerLimitId = req.Id.Value;
                    return true;
                });
                await UnitWork.BatchAddAsync<CustomerLimitRule, int>(rules.ToArray());

                //用户先删除,再新增
                await UnitWork.DeleteAsync<CustomerLimitSaler>(c => c.CustomerLimitId == req.Id);
                users.All(u =>
                {
                    u.CustomerLimitId = req.Id.Value;
                    return true;
                });
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
                .OrderByDescending(x => x.CreateDatetime)
                .Select(x => new QueryCustomerLimitResponse
                {
                    GroupId = x.Id,
                    GroupName = x.Name,
                    Description = x.Describe,
                    IsDelete = x.Isdelete,
                    RuleResponses = x.CustomerLimitRules.Select(r => new RuleResponse { CustomerType = r.CustomerType, Limit = r.Limit }),
                    SalerResponses = x.CustomerLimitSalers.Select(s => new SalerResponse
                    {
                        SalerId = s.SalerId,
                        SalerName = s.SalerName,
                        Dept = s.Dept,
                        Account = s.Account
                    })
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
        //public async Task<Infrastructure.Response> UpdateGroupUser(AddOrDeleteGroupUsersReq req)
        //{
        //    var response = new Infrastructure.Response();
        //    response.Message = "";

        //    var userInfo = _auth.GetCurrentUser();
        //    var userName = userInfo.User.Name;

        //    foreach(var item in req.Users)
        //    {
        //        var ruleName = await (from l in UnitWork.Find<CustomerLimit>(null)
        //                              join s in UnitWork.Find<CustomerLimitSaler>(null) on l.Id equals s.CustomerLimitId
        //                              where s.SalerId == item.UserId
        //                              select l.Name).FirstOrDefaultAsync();
        //        if (ruleName != null)
        //        {
        //            response.Message += $"{item.UserName}已存在规则组:{ruleName} \n";
        //        }
        //    }

        //    //var limits = from c in UnitWork.Find<CustomerLimit>(null)
        //    //             join l in UnitWork.Find<>
        //    foreach (var item in req.Users)
        //    {


        //    }

        //    if (!string.IsNullOrWhiteSpace(response.Message))
        //    {
        //        response.Code = 500;
        //        return response;
        //    }

        //    var users = req.Users.Select(u => new CustomerLimitSaler
        //    {
        //        CustomerLimitId = req.Id,
        //        SalerId = u.UserId,
        //        SalerName = u.UserName,
        //        CreateUser = userName,
        //        CreateDatetime = DateTime.Now,
        //        UpdateUser = userName,
        //        UpdateDatetime = DateTime.Now,
        //    });

        //    using var tran = UnitWork.GetDbContext<CustomerLimit>().Database.BeginTransaction();
        //    try
        //    {
        //        await UnitWork.DeleteAsync<CustomerLimitSaler>(c => c.CustomerLimitId == req.Id);
        //        await UnitWork.BatchAddAsync<CustomerLimitSaler, int>(users.ToArray());

        //        await UnitWork.SaveAsync();
        //        await tran.CommitAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        await tran.RollbackAsync();

        //        response.Code = 500;
        //        response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
        //    }

        //    return response;
        //}

        /// <summary>
        /// 新增规则组用户
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> AddGroupUser(AddOrDeleteGroupUsersReq req)
        {
            var response = new Infrastructure.Response();
            response.Message = "";
            //判断用户是否已存在规则组中
            foreach (var user in req.Users)
            {
                var ruleName = await (from l in UnitWork.Find<CustomerLimit>(null)
                                      join s in UnitWork.Find<CustomerLimitSaler>(null) on l.Id equals s.CustomerLimitId
                                      where s.SalerId == user.SlpCode
                                      select l.Name).FirstOrDefaultAsync();
                if (ruleName != null)
                {
                    response.Message += $"{user.UserName}已存在规则组:{ruleName} \n";
                }
            }
            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                response.Code = 500;
                return response;
            }

            //判断用户的客户数是否已超过规则的限制
            var rules = from c in UnitWork.Find<CustomerLimit>(null)
                        join r in UnitWork.Find<CustomerLimitRule>(null) on c.Id equals r.CustomerLimitId
                        where c.Id == req.Id
                        select new
                        {
                            r.CustomerType,
                            r.Limit,
                        };
            foreach (var user in req.Users)
            {
                //如果有全部客户数量限制
                if (rules.Any(r => r.CustomerType == 0))
                {
                    var rule = rules.First(r => r.CustomerType == 0);
                    var count = await UnitWork.Find<OCRD>(c => c.SlpCode == user.SlpCode).CountAsync();
                    if (count >= rule.Limit)
                    {
                        response.Message += $"{user.UserName}的全部客户数量限制为{rule.Limit}个 \n";
                        continue;
                    }
                }
                //如果有未报价客户数量限制
                if (rules.Any(r => r.CustomerType == 1))
                {
                    var rule = rules.First(r => r.CustomerType == 1);
                    var count = await (from c in UnitWork.Find<OCRD>(null)
                                       join u in UnitWork.Find<OQUT>(null) on c.CardCode equals u.CardCode into temp
                                       from t in temp.DefaultIfEmpty()
                                       where c.SlpCode == user.SlpCode && t.CardCode == null
                                       select c.CardCode).CountAsync();
                    if (count >= rule.Limit)
                    {
                        response.Message += $"{user.UserName}的未报价客户数量限制为{rule.Limit}个 \n";
                        continue;
                    }
                }
                //如果有已报价未转订单客户数限制
                if (rules.Any(r => r.CustomerType == 2))
                {
                    var rule = rules.First(r => r.CustomerType == 2);
                    //已报价未转销售单
                    var count = await (from c in UnitWork.Find<OCRD>(null)
                                       join u in UnitWork.Find<OQUT>(null) on c.CardCode equals u.CardCode
                                       join r in UnitWork.Find<ORDR>(null) on c.CardCode equals r.CardCode into temp
                                       from t in temp.DefaultIfEmpty()
                                       where c.SlpCode == user.SlpCode && t.CardCode == null
                                       select c.CardCode).CountAsync();
                    if (count >= rule.Limit)
                    {
                        response.Message += $"{user.UserName}的已成交客户数限制为{rule.Limit}个 \n";
                        continue;
                    }
                }
                //如果有已有订单未有交货单客户数限制
                if (rules.Any(r => r.CustomerType == 3))
                {
                    var rule = rules.First(r => r.CustomerType == 3);
                    //已销售未转交货
                    var count = await (from c in UnitWork.Find<OCRD>(null)
                                       join r in UnitWork.Find<ORDR>(null) on c.CardCode equals r.CardCode
                                       join d in UnitWork.Find<ODLN>(null) on c.CardCode equals d.CardCode into temp
                                       from t in temp.DefaultIfEmpty()
                                       where c.SlpCode == user.SlpCode && t.CardCode == null
                                       select c.CardCode).CountAsync();
                    if (count >= rule.Limit)
                    {
                        response.Message += $"{user.UserName}的未转销售订单(已报价)客户数限制为{rule.Limit}个 \n";
                        continue;
                    }
                }
                //如果有已成交客户数限制
                if (rules.Any(r => r.CustomerType == 4))
                {
                    var rule = rules.First(r => r.CustomerType == 4);
                    var count = await (from c in UnitWork.Find<OCRD>(null)
                                       join d in UnitWork.Find<ODLN>(null) on c.CardCode equals d.CardCode
                                       where c.SlpCode == user.SlpCode
                                       select c.CardCode).Distinct().CountAsync();
                    if (count >= rule.Limit)
                    {
                        response.Message += $"{user.UserName}的未交货(已转销售订单)客户数限制为{rule.Limit}个 \n";
                        continue;
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                response.Code = 500;
                return response;
            }

            var userInfo = _auth.GetCurrentUser();
            var userName = userInfo.User.Name;
            var users = req.Users.Select(u => new CustomerLimitSaler
            {
                CustomerLimitId = req.Id,
                SalerId = u.SlpCode,
                SalerName = u.UserName,
                Dept = u.Dept,
                Account = u.Account,
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
        /// 删除规则组用户
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> DeleteGroupUser(AddOrDeleteGroupUsersReq req)
        {
            var response = new Infrastructure.Response();

            await UnitWork.DeleteAsync<CustomerLimitSaler>(c => c.CustomerLimitId == req.Id && req.Users.Select(u => u.SlpCode).Contains(c.SalerId));
            await UnitWork.SaveAsync();

            return response;
        }

        /// <summary>
        /// 获取销售员信息
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetSlpInfos(QuerySlpReq req)
        {
            var result = new TableData();

            //已在规则组中的用户
            var limitUsers = await UnitWork.Find<CustomerLimitSaler>(null).Select(c => c.SalerId).ToListAsync();
            var query = from u in UnitWork.Find<base_user>(null)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.Account), u => u.log_nm == req.Account)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.Name), u => u.user_nm == req.Name)
                        join ud in UnitWork.Find<base_user_detail>(null) on u.user_id equals ud.user_id
                        join s in UnitWork.Find<sbo_user>(null) on u.user_id equals s.user_id
                        join d in UnitWork.Find<base_dep>(null)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.Dept), d => d.dep_alias == req.Dept)
                        on ud.dep_id equals d.dep_id
                        where s.sbo_id == Define.SBO_ID && new int[] { 0, 1 }.Contains(ud.status)
                        && !limitUsers.Contains(s.sale_id.Value)
                        select new
                        {
                            u.log_nm,
                            u.user_nm,
                            d.dep_alias,
                            d.dep_nm,
                            s.sale_id
                        };

            result.Data = await query.OrderBy(q => q.sale_id).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            result.Count = await query.CountAsync();

            return result;
        }


        #region 定时任务
        /// <summary>
        /// 拉取符合掉落规则的客户进入公海
        /// </summary>
        /// <returns></returns>
        public async Task AsyncCustomerStatusService()
        {
            //查询是否有通用规则的设置,如果没有或者开关没打开的话,不进行拉取
            var seaConfig = UnitWork.Find<CustomerSeaConf>(null).FirstOrDefault();
            if (seaConfig == null || seaConfig.Enable == false) { return; }

            var customerTypeData = new Dictionary<int, string>();
            customerTypeData.Add(0, "全部客户");
            customerTypeData.Add(1, "未报价客户");
            customerTypeData.Add(2, "已成交客户");

            var orderTypeData = new Dictionary<int, string>();
            orderTypeData.Add(0, "未报价");
            orderTypeData.Add(1, "未成交");
            orderTypeData.Add(2, "未交货");

            //查询规则列表,按部门和客户类型分类,事件优先级:0-未报价>1-未下单>2-未交货
            var ruleData = await (from c in UnitWork.Find<CustomerSeaRule>(null)
                                  join ci in UnitWork.Find<CustomerSeaRuleItem>(null)
                                  on c.Id equals ci.CustomerSeaRuleId
                                  //true代表启用,目前的需求是先处理未报价客户未报价的规则
                                  where c.Enable == true && ci.CustomerType == 1 && ci.OrderType == 0
                                  group new { c, ci } by new { ci.DepartmentName, ci.CustomerType } into g
                                  select new
                                  {
                                      dept = g.Key.DepartmentName,
                                      customerType = g.Key.CustomerType,
                                      orderType = g.Min(x => x.ci.OrderType), //事件优先级越高,数值越小
                                      day = g.Max(x => x.ci.Day) //优先级高的天数一定会比优先级低的大
                                  }).ToListAsync();
            if (ruleData.Count() <= 0)
            {
                return;
            }

            //提前通知天数
            var notifyDay = seaConfig?.NotifyDay;
            //符合掉落公海规则的客户
            var customerLists = new List<CustomerList>();
            //符合掉入规则的客户会先放入缓存,提醒日期过后还没做单,就会放入公海表中
            foreach (var rule in ruleData)
            {
                var dept = rule.dept; //部门
                var key = $"dept:{dept}";
                RedisHelper.SAdd("dept:", dept);

                //根据部门查找该部门下的业务员销售编号
                var slpInfo = await (from u in UnitWork.Find<base_user>(null)
                                     join ud in UnitWork.Find<base_user_detail>(null) 
                                     on u.user_id equals ud.user_id
                                     join d in UnitWork.Find<base_dep>(null)
                                     on ud.dep_id equals d.dep_id
                                     join s in UnitWork.Find<sbo_user>(null)
                                     on u.user_id equals s.user_id
                                     where s.sbo_id == Define.SBO_ID
                                     && d.dep_alias == dept
                                     //&& new int[] { 0, 1 }.Contains(ud.status) //在职的员工,离职状态是2和3
                                     && u.user_nm == "薛琼" //(先拿薛姐的客户试一试)
                                     select s.sale_id).Distinct().ToListAsync();

                //再根据销售编号查找客户
                var whiteList = await UnitWork.Find<SpecialCustomer>(null).Select(s => s.CustomerNo).ToListAsync(); //黑/白名单客户不会掉入公海池
                //查询客户(条件：是属于该部门的业务员，不在黑/白名单中，并且没有报价单)
                var customers = await (from c in UnitWork.Find<OCRD>(null)
                                       join s in UnitWork.Find<OSLP>(null) on c.SlpCode equals s.SlpCode
                                       join q in UnitWork.Find<OQUT>(null) on c.CardCode equals q.CardCode into temp
                                       from t in temp.DefaultIfEmpty()
                                       where slpInfo.Select(s => s).Contains(s.SlpCode)
                                       && !whiteList.Contains(c.CardCode)
                                       && t.CardCode == null
                                       orderby c.CreateDate
                                       select new { c.CardCode, c.CardName, c.CreateDate, s.SlpCode, s.SlpName }).Take(50).ToListAsync();
                foreach (var customer in customers)
                {
                    DateTime? startTime = null;
                    //查找客户最近一次的业务员变更(查找有不同业务员的客户)
                    var acrdInfos = await UnitWork.Find<ACRD>(a => a.CardCode == customer.CardCode).Select(c => new { c.SlpCode, c.UpdateDate }).ToListAsync();
                    var hasDiffClient = acrdInfos.Select(x => x.SlpCode).Distinct().Count() > 1;
                    //如果有不同的业务员,则开始时间取最近一次的客户分配给业务员的时间
                    if (hasDiffClient)
                    {
                        startTime = UnitWork.Find<ACRD>(a => a.CardCode == customer.CardCode).Max(c => c.UpdateDate);
                    }
                    //否则说明该客户的业务员无变动,取该客户的创建时间
                    else
                    {
                        startTime = customer.CreateDate;
                    }

                    //超过规则定义的天数则放入redis中,这部分的数据是即将掉入公海,放入日期加上通知天数就是掉入公海的时间
                    if (startTime != null && (DateTime.Now - startTime).Value.Days > rule.day)
                    {
                        customerLists.Add(new CustomerList { DepartMent = dept, CustomerNo = customer.CardCode });

                        var score = decimal.Parse(DateTime.Now.AddDays((double)notifyDay).ToString("yyyyMMdd"));
                        //如果有序集合中不存在成员,则新增
                        var memberScore = RedisHelper.ZScore(key, customer.CardCode);
                        if (memberScore == null)
                        {
                            var result = RedisHelper.ZAdd(key, (score, customer.CardCode));
                        }

                        RedisHelper.HSet($"customer:{customer.CardCode}", "Department", dept);
                        RedisHelper.HSet($"customer:{customer.CardCode}", "CustomerName", customer.CardName);
                        RedisHelper.HSet($"customer:{customer.CardCode}", "SlpCode", customer.SlpCode);
                        RedisHelper.HSet($"customer:{customer.CardCode}", "SlpName", customer.SlpName);
                        RedisHelper.HSet($"customer:{customer.CardCode}", "CreateDate", customer.CreateDate);
                        RedisHelper.HSet($"customer:{customer.CardCode}", "Remark", $"{rule.dept}," +
                            $"{customerTypeData.FirstOrDefault(c => c.Key == rule.customerType).Value}超过{rule.day}天{orderTypeData.FirstOrDefault(o => o.Key == rule.orderType).Value}");
                    }
                }
            }

            //数据处理
            var depts = customerLists.Select(c => c.DepartMent).Distinct();
            foreach (var dept in depts)
            {
                //返回缓存中这个部门下所有的客户
                var deptData = RedisHelper.ZRange($"dept:{dept}", 0, -1).ToList();
                //缓存中有,而本次扫描中没有的,说明客户不符合掉落规则(原业务员做了报价单,或者分配给了新的业务员等),这部分数据要从缓存中移除
                var deleData = deptData.Except(customerLists.Where(c => c.DepartMent == dept).Select(c => c.CustomerNo)).ToList();
                RedisHelper.ZRem($"dept:{dept}", deleData.ToArray());
                var customers = deleData.Select(d => $"customer:{d}");
                RedisHelper.Del(customers.ToArray()); 
            }
            //部门清除旧有的,加入本次符合规则的
            var deptsData = RedisHelper.SMembers("dept:");
            RedisHelper.SRem("dept:", deptsData);
            RedisHelper.SAdd("dept:", depts.ToArray());
        }

        public async Task Test()
        {
            var cardCodes = new string[] { "C01072" };
            var customers = await UnitWork.Find<OCRD>(null).Where(c => cardCodes.Contains(c.CardCode)).Select(c => new { c.CardCode, c.CreateDate }).ToListAsync();
            foreach(var customer in customers)
            {
                DateTime? startTime = null;
                //查找客户最近一次的业务员变更(查找有不同业务员的客户)
                var acrdInfos = await UnitWork.Find<ACRD>(a => a.CardCode == customer.CardCode).Select(c => new { c.SlpCode, c.UpdateDate }).ToListAsync();
                var hasDiffClient = acrdInfos.Select(x => x.SlpCode).Distinct().Count() > 1;
                //如果有不同的业务员,则开始时间取最近一次的客户分配给业务员的时间
                if (hasDiffClient)
                {
                    startTime = UnitWork.Find<ACRD>(a => a.CardCode == customer.CardCode).Max(c => c.UpdateDate);
                }
                //否则说明该客户的业务员无变动,取该客户的创建时间
                else
                {
                    startTime = customer.CreateDate;
                }
            }
        }

        /// <summary>
        /// 拉取缓存中符合掉落规则的客户进入公海
        /// </summary>
        /// <returns></returns>
        public async Task RecoveryCustomer()
        {
            var customerLists = new List<CustomerList>();
            //查询有哪些部门
            var depts = RedisHelper.SMembers("dept:");

            foreach (var dept in depts)
            {
                //查询这个部门有哪些客户
                var customersWithScore = RedisHelper.ZRangeWithScores($"dept:{dept}", 0, -1);
                foreach (var customer in customersWithScore)
                {
                        
                    //掉入日期
                    var date = DateTime.ParseExact(customer.score.ToString(), "yyyyMMdd", null).Date;
                    var customerCode = customer.member;
                    var customerName = RedisHelper.HGet($"customer:{customer.member}", "CustomerName");
                    var slpCode = RedisHelper.HGet($"customer:{customer.member}", "SlpCode");
                    var slpName = RedisHelper.HGet($"customer:{customer.member}", "SlpName");
                    var createDate = RedisHelper.HGet($"customer:{customer.member}", "CreateDate");
                    var remark = RedisHelper.HGet($"customer:{customer.member}", "Remark") ?? "";
                    //如果大于等于掉入日期,则掉入公海,并且删除缓存
                    if (DateTime.Now.Date >= date)
                    {
                        customerLists.Add(new CustomerList
                        {
                            DepartMent = dept,
                            CustomerNo = customerCode,
                            CustomerName = customerName,
                            CustomerSource = "",
                            CustomerCreateDate = DateTime.Parse(createDate),
                            SlpCode = int.Parse(slpCode),
                            SlpName = slpName,
                            Label = "已经掉入公海",
                            LabelIndex = 3,
                            CreateUser = "系统",
                            CreateDateTime = DateTime.Now,
                            UpdateUser = "系统",
                            UpdateDateTime = DateTime.Now,
                            IsDelete = false,
                            Remark = remark
                        });
                    }
                    //否则也加入公海,但是状态是即将掉入
                    else
                    {
                        customerLists.Add(new CustomerList
                        {
                            DepartMent = dept,
                            CustomerNo = customerCode,
                            CustomerName = customerName,
                            CustomerSource = "",
                            CustomerCreateDate = DateTime.Parse(createDate),
                            SlpCode = int.Parse(slpCode),
                            SlpName = slpName,
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
            
            //数据处理
            foreach (var item in customerLists)
            {
                var instance = await UnitWork.Find<CustomerList>(c => c.CustomerNo == item.CustomerNo).FirstOrDefaultAsync();
                //存在更新
                if (instance != null)
                {
                    instance.LabelIndex = item.LabelIndex;
                    instance.Label = item.Label;
                    instance.SlpCode = item.SlpCode;
                    instance.SlpName = item.SlpName;
                    instance.UpdateDateTime = DateTime.Now;
                    instance.UpdateUser = "系统";
                    await UnitWork.UpdateAsync(instance);
                }
                //不存在新增
                else
                {
                    await UnitWork.AddAsync<CustomerList, int>(item);
                }

                await UnitWork.SaveAsync();
            }

            //根据本次任务扫描的部门,查找已在公海池中的客户(根据部门处理数据,防止误删其他部门在公海的数据)
            var databaseData = await UnitWork.Find<CustomerList>(c => c.LabelIndex == 4 && customerLists.Select(x => x.DepartMent).Contains(c.DepartMent)).Select(c => c.CustomerNo).ToListAsync();
            //公海池中有,而本次扫描中没有的,说明客户不符合掉落规则(原业务员做了报价单,或者分配给了新的业务员等),这部分数据要从公海中移除
            var deleteData = databaseData.Except(customerLists.Select(c => c.CustomerNo));
            if (deleteData.Count() > 0)
            {
                await UnitWork.DeleteAsync<CustomerList>(c => deleteData.Contains(c.CustomerNo));
                await UnitWork.SaveAsync();
            }

            //已经掉入公海的客户,原来所属的销售员清空
            var customers = customerLists.Where(c => c.LabelIndex == 3);
            await UnitWork.UpdateAsync<OCRD>(c => customers.Select(x => x.CustomerNo).Contains(c.CardCode), x => new OCRD
            {
                SlpCode = null
            });
            //同时加入掉入记录表
            var moveinHistorys = customers.Select(c => new CustomerMoveHistory
            {
                CardCode = c.CustomerNo,
                CardName = c.CustomerName,
                SlpCode = c.SlpCode,
                SlpName = c.SlpName,
                MoveInType = "按规则掉入",
                Remark = c.Remark,
                CreateTime = DateTime.Now,
                CreateUser = "系统",
                UpdateTime = DateTime.Now,
                UpdateUser = "系统"
            });
            await UnitWork.BatchAddAsync<CustomerMoveHistory, int>(moveinHistorys.ToArray());
            await UnitWork.SaveAsync();

            //正式环境下发现有重复的数据,把重复的数据删除,只保留最小的那一个
            var sql = @"select c.Id
                        from customer_list as c
                        where c.Customer_No in(
	                        select Customer_No
	                        from customer_list
	                        group by Customer_No
	                        having count(*) > 1
                        )
                        and c.Id not in(
	                        select min(Id)
	                        from customer_list
	                        group by Customer_No
	                        having count(*) > 1
                        );";
            var duplicateData = UnitWork.ExcuteSqlTable(ContextType.Nsap4ServeDbContextType, sql, System.Data.CommandType.Text);
            if (duplicateData != null && duplicateData.Rows.Count > 0)
            {
                var duplicateId = duplicateData.AsEnumerable().Select(d => d.Field<int>("Id")).ToList();
                await UnitWork.DeleteAsync<CustomerList>(c => duplicateId.Contains(c.Id));
                await UnitWork.SaveAsync();
            }

            //数据库数据更新之后,删除已经掉入公海的客户缓存
            foreach (var customer in customers)
            {
                RedisHelper.Del($"customer:{customer.CustomerNo}");
                RedisHelper.ZRem($"dept:{customer.DepartMent}", customer.CustomerNo);
            }
        }

        /// <summary>
        /// 向即将掉入公海的客户业务员发送提醒信息
        /// </summary>
        /// <returns></returns>
        public async Task PushMessage()
        {
            var config = await UnitWork.Find<CustomerSeaConf>(null).Select(c => c.NotifyDay).FirstOrDefaultAsync();
            //查询所有即将掉入公海的客户
            var query = await (UnitWork.Find<CustomerList>(c => c.LabelIndex == 4).GroupBy(c => c.SlpName).Select(g => new
            {
                SlpName = g.Key,
                count = g.Count()
            })).ToListAsync();
            //查看有哪些业务员要发送提醒
            foreach(var slp in query)
            {
                //向原销售员发送提醒
                //var customers = new StringBuilder("");
                //foreach (var item in query.Where(q=>q.SlpName == slp.Key))
                //{
                //    customers.Append($",{item.CustomerNo}");

                //}
                //if (!string.IsNullOrWhiteSpace(customers.ToString()))
                //{
                //    await _hubContext.Clients.User(slp.Key).SendAsync("ReceiveMessage", "系统", $"您的客户{customers.ToString().Substring(1)}即将掉入公海,请及时跟进!");
                //}
                await _hubContext.Clients.User(slp.SlpName).SendAsync("ReceiveMessage", "系统", $"您有{slp.count}个客户将在{config}天后掉入公海,请及时跟进!");
            }

            //查询有哪些部门需要发送通知
            //var depts = RedisHelper.SMembers("dept:");
            //foreach (var dept in depts)
            //{
            //    //查询这个部门有哪些客户
            //    var customersWithScore = RedisHelper.ZRangeWithScores($"dept:{dept}", 0, -1);
            //    foreach (var customer in customersWithScore)
            //    {
            //        var slpName = RedisHelper.HGet($"customer:{customer.member}", "SlpName");
            //        var date = DateTime.ParseExact(customer.score.ToString(), "yyyyMMdd", null).ToString("yyyy-MM-dd");
            //        await _hubContext.Clients.User(slpName).SendAsync("ReceiveMessage", "系统", $"您的客户{customer.member}将在{date}掉入公海,请及时进行报价!");
            //    }
            //}
        }
        #endregion
    }
}
