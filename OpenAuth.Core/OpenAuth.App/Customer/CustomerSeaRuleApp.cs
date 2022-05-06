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

            if(UnitWork.Find<CustomerSeaRule>(null).Any(c=>c.RuleName == req.RuleName))
            {
                response.Message = "规则名已存在";
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
                            CustomerType = rule.CustomerType,
                            Day = rule.Day,
                            OrderType = rule.OrderType,
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
                .OrderByDescending(c => c.UpdateDatetime)
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
                        CustomerType = x.CustomerType,
                        Day = x.Day,
                        OrderType = x.OrderType
                    }),
                    Enable = c.Enable
                });

            result.Data = await query.ToListAsync();
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
                await UnitWork.DeleteAsync<CustomerSeaRuleItem>(c => c.Id == req.Id);

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
                            CustomerType = rule.CustomerType,
                            Day = rule.Day,
                            OrderType = rule.OrderType,
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

            var query = UnitWork.Find<Repository.Domain.Org>(o => o.IsLeaf == true && o.Status == 0)
                .Select(o => new { deptId = o.Id, deptName = o.Name });

            result.Data = await query.OrderBy(q => q.deptName).ToListAsync();
            result.Count = await query.CountAsync();

            return result;
        }
    }
}
