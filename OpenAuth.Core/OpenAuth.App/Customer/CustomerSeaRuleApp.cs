using OpenAuth.App.Customer.Request;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OpenAuth.Repository.Domain.Customer;
using OpenAuth.App.Response;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Customer.Response;
using Infrastructure.Extensions;

namespace OpenAuth.App.Customer
{
    public class CustomerSeaRuleApp: OnlyUnitWorkBaeApp
    {
        public CustomerSeaRuleApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth) { }

        /// <summary>
        /// 新增公海规则
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> AddCustomerSeaRule(AddOrUpdateCustomerSeaRuleReq req)
        {
            var response = new Infrastructure.Response();
            response.Message = "";

            if (req.RuleDetails.Count <= 0)
            {
                response.Code = 500;
                response.Message = "掉入规则没填写";
                return response;
            }

            //判断规则名是否存在
            if(UnitWork.Find<CustomerSeaRule>(null).Any(c=>c.RuleName == req.RuleName))
            {
                response.Code = 500;
                response.Message = "规则名已存在";
                return response;
            }

            //判断部门、客户类型、事件类型3者组合是否已存在
            foreach(var dept in req.Departments)
            {
                foreach(var rule in req.RuleDetails)
                {
                    var isExists = UnitWork.Find<CustomerSeaRuleItem>(c => c.DepartmentName == dept.DepartmentName && c.RuleType == rule.RuleType).Any();
                    if (isExists)
                    {
                       response.Message += $"{dept.DepartmentName}已存在规则,一个部门只能存在一个规则 \n";
                    }
                }
            }
            if(!string.IsNullOrWhiteSpace(response.Message))
            {
                response.Code = 500;
                return response;
            }

            var userInfo = _auth.GetCurrentUser();
            using var tran = UnitWork.GetDbContext<CustomerSeaRule>().Database.BeginTransaction();
            try
            {
                var customerSeaRule = new CustomerSeaRule
                {
                    RuleName = req.RuleName,
                    Enable = req.Enable,
                    CreateUser = userInfo?.User?.Name,
                    CreateDatetime = DateTime.Now,
                    UpdateUser = userInfo?.User?.Name,
                    UpdateDatetime = DateTime.Now,
                    IsDelete = false
                };
                //先保存,获取主表的主键
                await UnitWork.AddAsync<CustomerSeaRule>(customerSeaRule);
                await UnitWork.SaveAsync();
                //将部门和规则转换为1对1的关系
                foreach(var dept in req.Departments)
                {
                    foreach(var rule in req.RuleDetails)
                    {
                        var ruleItem = new CustomerSeaRuleItem
                        {
                            CustomerSeaRuleId = customerSeaRule.Id,
                            DepartmentId = dept.DepartmentId,
                            DepartmentName = dept.DepartmentName,
                            RuleType = rule.RuleType,
                            Day = rule.Day,
                            CreateUser = userInfo?.User?.Name,
                            CreateDatetime = DateTime.Now,
                            UpdateUser = userInfo?.User?.Name,
                            UpdateDatetime = DateTime.Now,
                        };

                        await UnitWork.AddAsync<CustomerSeaRuleItem>(ruleItem);
                    }
                }

                await UnitWork.SaveAsync();
                await tran.CommitAsync();
                response.Message = "操作成功";
            }
            catch(Exception ex)
            {
                await tran.RollbackAsync();
                response.Code = 500;
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
            }

            return response;
        }

        /// <summary>
        /// 获取公海规则信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetCustomerSeaRules(QueryCustomerSeaRulesReq req)
        {
            var result = new TableData();

            var query = UnitWork.Find<CustomerSeaRule>(null)
                .WhereIf(req.Id != null && req.Id > 0, c => c.Id == req.Id)
                .Include(c => c.CustomerSeaRuleItems)
                .OrderByDescending(c => c.CreateDatetime)
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit)
                .Select(c => new QueryCustomerSeaRuleResponse
                {
                    RuleId = c.Id,
                    RuleName = c.RuleName,
                    DepartInfos = c.CustomerSeaRuleItems.Select(x => new DepartInfo
                    {
                        DepartId = x.DepartmentId,
                        DepartName = x.DepartmentName
                    }),
                    RuleDetailInfos = c.CustomerSeaRuleItems.Select(x => new RuleDetailInfo
                    {
                        RuleType = x.RuleType,
                        Day = x.Day
                    }),
                    Enable = c.Enable
                });

            var data = await query.ToListAsync();
            result.Data = data.Select(d => new QueryCustomerSeaRuleResponse
            {
                RuleId = d.RuleId,
                RuleName = d.RuleName,
                DepartInfos = d.DepartInfos.Distinct(),
                RuleDetailInfos = d.RuleDetailInfos.Distinct(),
                Enable = d.Enable
            });
            result.Count = await query.CountAsync();

            return result;
        }

        /// <summary>
        /// 根据id删除公海规则
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> DeleteCustomerSeaRule(QueryCustomerSeaRulesReq req)
        {
            var response = new Infrastructure.Response();
            using var tran = UnitWork.GetDbContext<CustomerSeaRule>().Database.BeginTransaction();
            try
            {
                await UnitWork.DeleteAsync<CustomerSeaRule>(c => c.Id == req.Id);
                await UnitWork.DeleteAsync<CustomerSeaRuleItem>(c => c.CustomerSeaRuleId == req.Id);

                await UnitWork.SaveAsync();
                await tran.CommitAsync();
            }
            catch(Exception ex)
            {
                await tran.RollbackAsync();
                response.Code = 500;
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
            }

            return response;
        }

        /// <summary>
        /// 根据id修改公海规则
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> UpdateCustomerSeaRule(AddOrUpdateCustomerSeaRuleReq req)
        {
            var response = new Infrastructure.Response();

            var userInfo = _auth.GetCurrentUser();
            using var tran = UnitWork.GetDbContext<CustomerSeaRule>().Database.BeginTransaction();
            try
            {
                var customerSeaRuleObject = await UnitWork.Find<CustomerSeaRule>(c => c.Id == req.Id).FirstOrDefaultAsync();
                if(customerSeaRuleObject == null)
                {
                    response.Message = "规则不存在";
                    return response;
                }
                customerSeaRuleObject.RuleName = req.RuleName;
                customerSeaRuleObject.Enable = req.Enable;
                customerSeaRuleObject.UpdateDatetime = DateTime.Now;
                customerSeaRuleObject.UpdateUser = userInfo?.User?.Name;
                await UnitWork.UpdateAsync<CustomerSeaRule>(customerSeaRuleObject);
                //先删除,再插入
                await UnitWork.DeleteAsync<CustomerSeaRuleItem>(c => c.CustomerSeaRuleId == req.Id);
                //将部门和规则转换为1对1的关系
                foreach (var dept in req.Departments)
                {
                    foreach (var rule in req.RuleDetails)
                    {
                        var ruleItem = new CustomerSeaRuleItem
                        {
                            CustomerSeaRuleId = customerSeaRuleObject.Id,
                            DepartmentId = dept.DepartmentId,
                            DepartmentName = dept.DepartmentName,
                            RuleType = rule.RuleType,
                            Day = rule.Day,
                            CreateUser = userInfo?.User?.Name,
                            CreateDatetime = DateTime.Now,
                            UpdateUser = userInfo?.User?.Name,
                            UpdateDatetime = DateTime.Now,
                        };

                        await UnitWork.AddAsync<CustomerSeaRuleItem>(ruleItem);
                    }
                }

                await UnitWork.SaveAsync();
                await tran.CommitAsync();
            }
            catch(Exception ex)
            {
                await tran.RollbackAsync();
                response.Code = 500;
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
            }

            return response;
        }

        /// <summary>
        /// 根据id启用or禁用公海规则
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> EnableCustomerSeaRule(EnableCustomerSeaRuleReq req)
        {
            var response = new Infrastructure.Response();
            var customerSeaRuleObject = await UnitWork.Find<CustomerSeaRule>(c => c.Id == req.Id).FirstOrDefaultAsync();
            if (customerSeaRuleObject != null)
            {
                customerSeaRuleObject.Enable = req.Enable;
                await UnitWork.UpdateAsync<CustomerSeaRule>(customerSeaRuleObject);
                await UnitWork.SaveAsync();
            }

            return response;
        }

        /// <summary>
        /// 获取叶子结点的部门信息列表
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetLeafOrgList()
        {
            var result = new TableData();
            //只查询S开头的部门(销售or售后)
            var query = UnitWork.Find<Repository.Domain.Org>(o => o.IsLeaf == true && o.Status == 0 && o.Name.StartsWith("S"))
                .Select(o => new { deptId = o.Name, deptName = o.Name }).Distinct();

            result.Data = await query.OrderBy(q => q.deptName).ToListAsync();
            result.Count = await query.CountAsync();

            return result;
        }
    }
}
