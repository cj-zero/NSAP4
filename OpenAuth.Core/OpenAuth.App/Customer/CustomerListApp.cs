using System.Linq;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.App.Customer.Request;
using OpenAuth.App.Customer.Response;
using OpenAuth.App.Response;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain.Customer;

namespace OpenAuth.App.Customer
{
    public class CustomerListApp : OnlyUnitWorkBaeApp
    {
        private readonly UserManagerApp _userManagerApp;
        public CustomerListApp(IUnitWork unitWork, IAuth auth,
            UserManagerApp userManagerApp) : base(unitWork, auth)
        {
            _userManagerApp = userManagerApp;
        }

        /// <summary>
        /// 查询客户列表
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetCustomers(QueryCustomerListReq req)
        {
            var result = new TableData();

            var query = from c in UnitWork.Find<OCRD>(null)
                        .WhereIf(!string.IsNullOrWhiteSpace( req.CardCode),c=>c.CardCode == req.CardCode)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.CardName),c=>c.CardName.Contains(req.CardName))
                        join s in UnitWork.Find<OSLP>(null) 
                        .WhereIf(!string.IsNullOrWhiteSpace(req.SlpName),s=>s.SlpName.Contains(req.SlpName))
                        on c.SlpCode equals s.SlpCode
                        join g in UnitWork.Find<OCRG>(null) on (int)c.GroupCode equals g.GroupCode
                        select new
                        {
                            c.CardCode,
                            c.CardName,
                            s.SlpCode,
                            s.SlpName,
                            g.GroupName
                        };
            //先把数据加载到内存
            var data = await query.OrderBy(q => q.CardCode).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            //跨库查询,把要查询的数据一次查完,优化查询速度
            //客户所属行业数据
            var compSectorData = await UnitWork.Find<crm_ocrd>(x => data.Select(d => d.CardCode).Contains(x.CardCode)).Select(x => new { x.U_CompSector, x.CardCode }).ToListAsync();

            var response = new List<QueryCustomerListResponse>();
            data.ForEach(d =>
            {
                var userInfo = _userManagerApp.GetUserOrgInfo(null, d.SlpName).Result;
                response.Add(new QueryCustomerListResponse
                {
                    CardCode = d.CardCode,
                    CardName = d.CardName,
                    SlpCode = d.SlpCode,
                    SlpName = d.SlpName,
                    DeptCode = userInfo?.OrgId,
                    DeptName = userInfo?.OrgName,
                    GroupName = d.GroupName,
                    CompSector = compSectorData.FirstOrDefault(c => c.CardCode == d.CardCode)?.U_CompSector
                });
            });

            result.Data = response;
            result.Count = await query.CountAsync();

            return result;
        }

        /// <summary>
        /// 新增客户白名单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> AddCustomer(List<AddCustomerListReq> model)
        {
            var result = new Infrastructure.Response();

            foreach (var item in model)
            {
                //映射
                var instance = item.MapTo<SpecialCustomer>();
                //判断名单中是否已存在该客户
                var isExists = UnitWork.Find<SpecialCustomer>(null).Any(c => c.CustomerNo == instance.CustomerNo && c.Isdelete == false);
                if (isExists)
                {
                    result.Message = "客户已存在名单中";
                    result.Code = 500;
                    return result;
                }

                var userInfo = _auth.GetCurrentUser().User;
                instance.CreateUser = userInfo.Name;
                instance.CreateDatetime = DateTime.Now;
                instance.UpdateUser = userInfo.Name;
                instance.UpdateDatetime = DateTime.Now;

                await UnitWork.AddAsync<SpecialCustomer, int>(instance);
            }

            await UnitWork.SaveAsync();

            return result;
        }

        /// <summary>
        /// 获取黑白名单列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetWhiteOrBlackList(QueryWhiteOrBlackListReq req)
        {
            var result = new TableData();

            var query = UnitWork.Find<SpecialCustomer>(c => c.Type == req.Type && c.Isdelete == false).Select(x => new
            {
                x.Id,
                x.CustomerNo,
                x.CustomerName,
                x.SalerName,
                x.DepartmentName,
                x.Type
            });

            result.Data = await query.OrderBy(q => q.CustomerNo).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            result.Count = await query.CountAsync();

            return result;
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> DeleteCustomer(int id)
        {
            var result = new Infrastructure.Response();

            await UnitWork.DeleteAsync<SpecialCustomer>(c => c.Id == id);
            await UnitWork.SaveAsync();

            return result;
        }

        /// <summary>
        /// 根据客户代码查询历史归属记录
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetCustomerSalerLists(QueryCustomerSalerListReq req)
        {
            var result = new TableData();

            //查询客户销售员变更记录表
            var queryCustomerSalers = new List<QueryCustomerSalerListResponse>();

            //如果变更记录表不存在客户记录,则直接查询OCRD表
            if (!UnitWork.Find<ACRD>(null).Any(a => a.CardCode == req.CardCode))
            {
                var slpcode = await (from o in UnitWork.Find<OCRD>(null)
                                     join s in UnitWork.Find<OSLP>(null) on o.SlpCode equals s.SlpCode
                                     where o.CardCode == req.CardCode
                                     orderby o.CreateDate
                                     select new
                                     {
                                         o.CardCode,
                                         s.SlpName,
                                         o.CreateDate,
                                     }).FirstOrDefaultAsync();
                queryCustomerSalers.Add(new QueryCustomerSalerListResponse
                {
                    SalerName = slpcode.SlpName,
                    CreateTime = slpcode.CreateDate,
                    ReceiveDate = slpcode.CreateDate
                });

            }
            //否则查询客户销售员变更记录表
            else
            {

                //根据客户编码查询客户的销售员记录
                var query = await (from a in UnitWork.Find<ACRD>(null)
                                   join s in UnitWork.Find<OSLP>(null) on a.SlpCode equals s.SlpCode
                                   where a.CardCode == req.CardCode
                                   orderby a.LogInstanc
                                   select new
                                   {
                                       a.CardCode,
                                       a.LogInstanc,
                                       a.SlpCode,
                                       s.SlpName,
                                       a.CreateDate,
                                       a.UpdateDate
                                   }).ToListAsync();


                if (query.Select(q => q.SlpCode).Distinct().Count() == 1)
                {
                    queryCustomerSalers.Add(new QueryCustomerSalerListResponse
                    {
                        SlpCode = query[0].SlpCode.Value,
                        SalerName = query[0].SlpName,
                        LogInstanc = query[0].LogInstanc,
                        CreateTime = query[0].CreateDate,
                        ReceiveDate = query[0].CreateDate
                    });
                }
                else
                {
                    for (int i = 0; i < query.Count - 1; i++)
                    {
                        if (query[i].SlpCode != query[i + 1].SlpCode)
                        {
                            queryCustomerSalers.Add(new QueryCustomerSalerListResponse
                            {
                                SlpCode = query[i].SlpCode.Value,
                                SalerName = query[i].SlpName,
                                LogInstanc = query[i].LogInstanc,
                                CreateTime = query[i].CreateDate,
                                ReceiveDate = query[i].CreateDate
                            });
                            queryCustomerSalers.Add(new QueryCustomerSalerListResponse
                            {
                                SlpCode = query[i + 1].SlpCode.Value,
                                SalerName = query[i + 1].SlpName,
                                LogInstanc = query[i + 1].LogInstanc,
                                CreateTime = query[i + 1].CreateDate,
                                ReceiveDate = query[i + 1].CreateDate
                            });
                        }
                    }
                }
            }

            //查询公海分配记录
            var history = await UnitWork.Find<CustomerSalerHistory>(c => c.CustomerNo == req.CardCode).Select(c => new { c.LogInstance, c.FallIntoTime }).ToListAsync();

            var data = from a in queryCustomerSalers
                       join h in history on a.LogInstanc equals h.LogInstance into temp
                       from t in temp.DefaultIfEmpty()
                       select new QueryCustomerSalerListResponse
                       {
                           SalerName = a.SalerName,
                           CreateTime = a.CreateTime,
                           ReceiveDate = a.ReceiveDate,
                           ReleaseDate = t == null ? null : t.FallIntoTime,
                           FallIntoDate = t == null ? null : t.FallIntoTime,
                       };

            result.Data = data;
            result.Count = data.Count();

            return result;
        }

        /// <summary>
        /// 获取掉入公海的客户列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetCustomerSeaLists(QueryCustomerSeaReq req)
        {
            var result = new TableData();

            //查询已经掉入公海的客户
            var query = UnitWork.Find<CustomerList>(c => c.LabelIndex == 3)
                .WhereIf(!string.IsNullOrWhiteSpace(req.DepartMent), c => c.DepartMent == req.DepartMent)
                .Select(c => new
                {
                    //Num = (req.page - 1) * req.limit + 1,
                    c.CustomerNo,
                    c.CustomerName,
                    c.DepartMent,
                    CustomerSource = "",
                    c.CreateUser,
                    c.CreateDateTime,
                    FallIntoTime = c.CreateDateTime
                });

            result.Data = (await query.OrderBy(q => q.CustomerNo).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync())
                .Select((c, index) => new
                {
                    Num = (req.page - 1) * req.limit + index + 1,
                    c.CustomerNo,
                    c.CustomerName,
                    c.DepartMent,
                    c.CustomerSource,
                    c.CreateUser,
                    c.CreateDateTime,
                    c.FallIntoTime
                });
            result.Count = await query.CountAsync();

            return result;
        }

        /// <summary>
        /// 获取在职的销售员列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetSlpInfo(QuerySlpInfoReq req)
        {
            var result = new TableData();

            var query = (from u in UnitWork.Find<base_user>(null)
                         .WhereIf(!string.IsNullOrWhiteSpace(req.SlpName), u => u.user_nm.Contains(req.SlpName))
                         join ud in UnitWork.Find<base_user_detail>(d => new int[] { 0, 1 }.Contains(d.status)) //在职的员工,离职状态是2和3
                         on u.user_id equals ud.user_id
                         join s in UnitWork.Find<sbo_user>(null)
                         .WhereIf(req.SlpCode != null && req.SlpCode > 0, u => u.sale_id == req.SlpCode)
                         on u.user_id equals s.user_id
                         group new { u, ud, s } by new { s.user_id } into g
                         select new
                         {
                             slpcode = g.Min(x => x.s.sale_id),
                             slpname = g.Max(x => x.u.user_nm)
                         }).Distinct();

            result.Data = await query.ToListAsync();
            result.Count = await query.CountAsync();

            return result;
        }

        /// <summary>
        /// 从公海中领取客户
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> ReceiveCustomerTest(ReceiveCustomerReq req)
        {
            var response = new Infrastructure.Response();

            var userInfo = _auth.GetCurrentUser();
            //根据用户姓名查询slpcode
            var slpInfo = await (from u in UnitWork.Find<base_user>(null)
                                 join s in UnitWork.Find<sbo_user>(null) on u.user_id equals s.user_id
                                 where u.user_nm == userInfo.User.Name
                                 select new { s.sale_id, u.user_nm }).FirstOrDefaultAsync();
            if (slpInfo == null)
            {
                response.Code = 500;
                response.Message = "销售员信息不存在";

                return response;
            }

            #region 逻辑判断
                          //判断是否是公海客户,如果不是或者没有则不能进行领取
            if (!UnitWork.Find<CustomerList>(null).Any(c => c.CustomerNo == req.CustomerNo))
            {
                response.Message = "非公海客户不能进行领取";
                response.Code = 500;
                return response;
            }

            var customer = await UnitWork.Find<CustomerList>(c => c.CustomerNo == req.CustomerNo).FirstOrDefaultAsync();
            //如果领取的销售员跟原销售员是同一人
            if (customer.SlpCode == slpInfo.sale_id)
            {
                //公海设置
                var config = await UnitWork.Find<CustomerSeaConf>(null).FirstOrDefaultAsync();
                //抢回限制如果是开启的
                if (config?.BackEnable == true)
                {
                    //判断天数是否符合要求
                    if ((DateTime.Now - customer.CreateDateTime).Days <= config.BackDay)
                    {
                        response.Message = $"原业务员在{config.BackDay}天内不能抢回公海客户";
                        response.Code = 500;
                        return response;
                    }
                }
            }

            //判断是否有最大客户数限制
            var customerLimitRule = await (from cl in UnitWork.Find<CustomerLimit>(null)
                                           join clr in UnitWork.Find<CustomerLimitRule>(null)
                                           on cl.Id equals clr.CustomerLimitId
                                           join clu in UnitWork.Find<CustomerLimitSaler>(null)
                                           on cl.Id equals clu.CustomerLimitId
                                           //这个字段用的有点奇怪,是否启用的意思,true代表已启用
                                           where cl.Isdelete == true && clu.SalerName == slpInfo.user_nm
                                           //按照销售员、客户类型分组，数量取最大的限制数量
                                           group new { cl, clr, clu } by new { clu.SalerName, clr.CustomerType } into g
                                           select new
                                           {
                                               SalerName = g.Key.SalerName,
                                               CustomerType = g.Key.CustomerType,
                                               Limit = g.Max(x => x.clr.Limit)
                                           }).FirstOrDefaultAsync();
            var isNoQuotationCustomer = await (from c in UnitWork.Find<OCRD>(null)
                                               join u in UnitWork.Find<OQUT>(null) on c.CardCode equals u.CardCode into temp
                                               from t in temp.DefaultIfEmpty()
                                               where c.CardCode == req.CustomerNo && t.CardCode == null
                                               select c.CardCode).AnyAsync();
            var isFinishCUstomer = UnitWork.Find<ODLN>(d => d.CardCode == req.CustomerNo).Any();

            if (customerLimitRule != null)
            {
                //如果客户是未报价客户,并且有未报价客户数量规则限制
                if (isNoQuotationCustomer && customerLimitRule.CustomerType == 1)
                {
                    var NoQuotationCount = await (from c in UnitWork.Find<OCRD>(null)
                                                  join s in UnitWork.Find<OSLP>(null) on c.SlpCode equals s.SlpCode
                                                  join u in UnitWork.Find<OQUT>(null) on c.CardCode equals u.CardCode into temp
                                                  from t in temp.DefaultIfEmpty()
                                                  where s.SlpName == slpInfo.user_nm && t.CardCode == null
                                                  select c.CardCode).Distinct().CountAsync();
                    if (NoQuotationCount >= customerLimitRule.Limit)
                    {
                        response.Code = 500;
                        response.Message = "超过该用户的最大客户数限制";
                        return response;
                    }
                }
                //如果客户是已成交客户,并且有已成交客户数量规则限制
                else if (isFinishCUstomer && customerLimitRule.CustomerType == 2)
                {
                    var finishCount = await (from c in UnitWork.Find<OCRD>(null)
                                             join s in UnitWork.Find<OSLP>(null) on c.SlpCode equals s.SlpCode
                                             join d in UnitWork.Find<ODLN>(null) on c.CardCode equals d.CardCode
                                             where s.SlpName == slpInfo.user_nm
                                             select c.CardCode).Distinct().CountAsync();
                    if (finishCount >= customerLimitRule.Limit)
                    {
                        response.Code = 500;
                        response.Message = "超过该客户的最大客户数限制";
                        return response;
                    }
                }
            }

            #endregion

            var history = new CustomerSalerHistory()
            {
                CustomerNo = req.CustomerNo,
                CustomerName = req.CustomerName,
                SlpCode = slpInfo.sale_id.Value,
                SlpName = slpInfo.user_nm,
                CreateTime = DateTime.Now,
                ReceiveTime = DateTime.Now,
                ReleaseTime = customer.CreateDateTime,
                FallIntoTime = customer.CreateDateTime,
                IsSaleHistory = req.IsSaleHistory
            };

            using var tran = UnitWork.GetDbContext<CustomerLimit>().Database.BeginTransaction();
            try
            {
                int lastInstance = 0;
                var isExists = UnitWork.Find<ACRD>(c => c.CardCode == req.CustomerNo).Any();
                if (isExists) { lastInstance = UnitWork.Find<ACRD>(c => c.CardCode == req.CustomerNo).Max(x => x.LogInstanc) + 1; }
                else { lastInstance = 1; }

                //加入历史归属表
                history.LogInstance = lastInstance;
                await UnitWork.AddAsync<CustomerSalerHistory>(history);
                //领取后,将客户从公海中移出
                await UnitWork.DeleteAsync<CustomerList>(c => c.CustomerNo == req.CustomerNo);

                //修改客户的销售员
                var instance = await UnitWork.Find<OCRD>(c => c.CardCode == req.CustomerNo).FirstOrDefaultAsync();
                instance.SlpCode = slpInfo.sale_id;
                await UnitWork.UpdateAsync<OCRD>(instance);
                //3.0的客户归属表中新增一条记录
                await UnitWork.AddAsync<ACRD>(new ACRD { DocEntry = instance.DocEntry, CardCode = req.CustomerNo, LogInstanc = lastInstance, CardName = req.CustomerName, SlpCode = slpInfo.sale_id, CreateDate = DateTime.Now, UpdateDate = DateTime.Now });
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
        /// 管理员分配客户给业务员
        /// </summary>
        /// <returns></returns>
        public async Task<Infrastructure.Response> DistributeCustomer(DistributeCustomerReq req)
        {
            var response = new Infrastructure.Response();

            var userInfo = _auth.GetCurrentUser();
            if (!userInfo.Roles.Any(r => r.Name == "管理员"))
            {
                response.Code = 500;
                response.Message = "非管理员不能进行分配操作";

                return response;
            }

            #region 判断逻辑
            //判断是否有最大客户数限制
            var customerLimitRule = await (from cl in UnitWork.Find<CustomerLimit>(null)
                                           join clr in UnitWork.Find<CustomerLimitRule>(null)
                                           on cl.Id equals clr.CustomerLimitId
                                           join clu in UnitWork.Find<CustomerLimitSaler>(null)
                                           on cl.Id equals clu.CustomerLimitId
                                           //这个字段用的有点奇怪,是否启用的意思,true代表已启用
                                           where cl.Isdelete == true && clu.SalerName == req.SlpName
                                           //按照销售员、客户类型分组，数量取最大的限制数量
                                           group new { cl, clr, clu } by new { clu.SalerName, clr.CustomerType } into g
                                           select new
                                           {
                                               SalerName = g.Key.SalerName,
                                               CustomerType = g.Key.CustomerType,
                                               Limit = g.Max(x => x.clr.Limit)
                                           }).FirstOrDefaultAsync();
            //是否是未报价客户
            var isNoQuotationCustomer = await (from c in UnitWork.Find<OCRD>(null)
                                               join u in UnitWork.Find<OQUT>(null) on c.CardCode equals u.CardCode into temp
                                               from t in temp.DefaultIfEmpty()
                                               where c.CardCode == req.CustomerNo && t.CardCode == null
                                               select c.CardCode).AnyAsync();
            //是否是已成交客户
            var isFinishCUstomer = UnitWork.Find<ODLN>(d => d.CardCode == req.CustomerNo).Any();

            if (customerLimitRule != null)
            {
                //如果客户是未报价客户,并且有未报价客户数量规则限制
                if (isNoQuotationCustomer && customerLimitRule.CustomerType == 1)
                {
                    var NoQuotationCount = await (from c in UnitWork.Find<OCRD>(null)
                                                  join s in UnitWork.Find<OSLP>(null) on c.SlpCode equals s.SlpCode
                                                  join u in UnitWork.Find<OQUT>(null) on c.CardCode equals u.CardCode into temp
                                                  from t in temp.DefaultIfEmpty()
                                                  where s.SlpName == req.SlpName && t.CardCode == null
                                                  select c.CardCode).Distinct().CountAsync();
                    if (NoQuotationCount >= customerLimitRule.Limit)
                    {
                        response.Code = 500;
                        response.Message = "超过该用户的最大客户数限制";
                        return response;
                    }
                }
                //如果客户是已成交客户,并且有已成交客户数量规则限制
                else if (isFinishCUstomer && customerLimitRule.CustomerType == 2)
                {
                    var finishCount = await (from c in UnitWork.Find<OCRD>(null)
                                             join s in UnitWork.Find<OSLP>(null) on c.SlpCode equals s.SlpCode
                                             join d in UnitWork.Find<ODLN>(null) on c.CardCode equals d.CardCode
                                             where s.SlpName == req.SlpName
                                             select c.CardCode).Distinct().CountAsync();
                    if (finishCount >= customerLimitRule.Limit)
                    {
                        response.Code = 500;
                        response.Message = "超过该客户的最大客户数限制";
                        return response;
                    }
                }
            }
            #endregion

            var customer = await UnitWork.Find<CustomerList>(c => c.CustomerNo == req.CustomerNo).FirstOrDefaultAsync();
            if (customer == null)
            {
                response.Code = 500;
                response.Message = "客户不存在或已被领取";
                return response;
            }

            var history = new CustomerSalerHistory()
            {
                CustomerNo = req.CustomerNo,
                CustomerName = req.CustomerName,
                SlpCode = req.SlpCode,
                SlpName = req.SlpName,
                CreateTime = DateTime.Now,
                ReceiveTime = DateTime.Now,
                ReleaseTime = customer.CreateDateTime,
                FallIntoTime = customer.CreateDateTime,
                IsSaleHistory = req.IsSaleHistory
            };

            using var tran = UnitWork.GetDbContext<CustomerLimit>().Database.BeginTransaction();
            try
            {
                int lastInstance = 0;
                var isExists = UnitWork.Find<ACRD>(c => c.CardCode == req.CustomerNo).Any();
                if (isExists) { lastInstance = UnitWork.Find<ACRD>(c => c.CardCode == req.CustomerNo).Max(x => x.LogInstanc) + 1; }
                else { lastInstance = 1; }

                //加入历史归属表
                history.LogInstance = lastInstance;
                await UnitWork.AddAsync<CustomerSalerHistory>(history);
                //领取后,将客户从公海中移出
                await UnitWork.DeleteAsync<CustomerList>(c => c.CustomerNo == req.CustomerNo);

                //修改客户的销售员
                var instance = await UnitWork.Find<OCRD>(c => c.CardCode == req.CustomerNo).FirstOrDefaultAsync();
                instance.SlpCode = req.SlpCode;
                await UnitWork.UpdateAsync<OCRD>(instance);
                //3.0的客户归属表中新增一条记录
                await UnitWork.AddAsync<ACRD>(new ACRD { DocEntry = instance.DocEntry, CardCode = req.CustomerNo, LogInstanc = lastInstance, CardName = req.CustomerName, SlpCode = req.SlpCode, CreateDate = DateTime.Now, UpdateDate = DateTime.Now });
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
    }
}
