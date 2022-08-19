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
using OpenAuth.Repository;
using System.Collections;
using System.Data;
using OpenAuth.App.Order;
using NSAP.Entity.Client;

namespace OpenAuth.App.Customer
{
    public class CustomerListApp : OnlyUnitWorkBaeApp
    {
        private readonly UserManagerApp _userManagerApp;
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
        private readonly ServiceBaseApp _serviceBaseApp;
        public CustomerListApp(IUnitWork unitWork, IAuth auth,
            UserManagerApp userManagerApp, ServiceSaleOrderApp serviceSaleOrderApp, ServiceBaseApp serviceBaseApp) : base(unitWork, auth)
        {
            _userManagerApp = userManagerApp;
            _serviceSaleOrderApp = serviceSaleOrderApp;
            _serviceBaseApp = serviceBaseApp;
        }

        /// <summary>
        /// 查询客户列表
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetCustomers(QueryCustomerListReq req)
        {
            var result = new TableData();

            List<string> SpecialCodeList = UnitWork.Find<SpecialCustomer>(q => !q.Isdelete).Select(q => q.CustomerNo).ToList();

            var query = from c in UnitWork.Find<OCRD>(null)
                         .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), c => c.CardCode == req.CardCode)
                         .WhereIf(!string.IsNullOrWhiteSpace(req.CardName), c => c.CardName.Contains(req.CardName))
                         .Where(q => !SpecialCodeList.Contains(q.CardCode))
                        join s in UnitWork.Find<OSLP>(null)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.SlpName), s => s.SlpName.Contains(req.SlpName))
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
            var compSectorData = await UnitWork.Find<crm_ocrd>(x => data.Select(d => d.CardCode).Contains(x.CardCode) && x.sbo_id == Define.SBO_ID).Select(x => new { x.U_CompSector, x.CardCode }).ToListAsync();
            //销售员部门数据
            var deptData = await (from s in UnitWork.Find<sbo_user>(null)
                                  join ud in UnitWork.Find<base_user_detail>(null) on s.user_id equals ud.user_id
                                  join d in UnitWork.Find<base_dep>(null) on ud.dep_id equals d.dep_id
                                  where s.sbo_id == Define.SBO_ID
                                  && data.Select(x => x.SlpCode).Contains(s.sale_id.Value)
                                  select new
                                  {
                                      slpCode = s.sale_id,
                                      dept = d.dep_alias,
                                  }).Distinct().ToListAsync();
            //var response = new List<QueryCustomerListResponse>();
            //data.ForEach(d =>
            //{
            //    response.Add(new QueryCustomerListResponse
            //    {
            //        CardCode = d.CardCode,
            //        CardName = d.CardName,
            //        SlpCode = d.SlpCode,
            //        SlpName = d.SlpName,
            //        DeptCode = userInfo?.OrgId,
            //        DeptName = userInfo?.OrgName,
            //        GroupName = d.GroupName,
            //        CompSector = compSectorData.FirstOrDefault(c => c.CardCode == d.CardCode)?.U_CompSector
            //    });
            //});
            var response = from q in data
                           join c in compSectorData on q.CardCode equals c.CardCode into temp1
                           join d in deptData on q.SlpCode equals d.slpCode into temp2
                           from t1 in temp1.DefaultIfEmpty()
                           from t2 in temp2.DefaultIfEmpty()
                           select new QueryCustomerListResponse
                           {
                               CardCode = q.CardCode,
                               CardName = q.CardName,
                               SlpCode = q.SlpCode,
                               SlpName = q.SlpName,
                               DeptCode = t2 == null ? null : t2.dept,
                               DeptName = t2 == null ? null : t2.dept,
                               GroupName = q.GroupName,
                               CompSector = t1 == null ? null : t1.U_CompSector
                           };

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

            //判断是否是公海管理员(普通业务员只能看到盲盒类型的数据)
            var isAdmin = _auth.GetCurrentUser().Roles.Any(r => r.Name == "公海管理员");
            //查出黑名单用户
            var query = from x in UnitWork.Find<SpecialCustomer>(c => c.Type == req.Type && c.Isdelete == false)
                        select new
                        {
                            x.Id,
                            x.CustomerNo,
                            x.CustomerName,
                            x.SalerName,
                            x.DepartmentName,
                            x.Type,
                            x.Remark,
                            x.UpdateUser,
                            x.UpdateDatetime
                        };
            var dataquery = await query.OrderBy(q => q.CustomerNo).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            //查询历史领取记录获取次数
            var receivedata = await UnitWork.Find<CustomerSalerHistory>(c => query.Select(x => x.CustomerNo).Contains(c.CustomerNo) && c.IsSaleHistory == true)
                .GroupBy(c => c.CustomerNo)
                .Select(g => new
                {
                    CardCode = g.Key,
                    ReceiveTime = g.Count()
                }).ToListAsync();

            var fallinquery = await UnitWork.Find<Repository.Domain.Serve.CustomerMoveHistory>(c => query.Select(x => x.CustomerNo).Contains(c.CardCode)).ToListAsync();
            var fallindata = fallinquery
                .OrderByDescending(x => x.CreateTime)
                .GroupBy(c => c.CardCode)
                .Select(g => new
                {
                    CardCode = g.Key,
                    fallinCount = g.Count(),
                    Remark = g.FirstOrDefault().Remark
                });


            var data = from s in dataquery
                       join q in receivedata on s.CustomerNo equals q.CardCode into temp1
                       join f in fallindata on s.CustomerNo equals f.CardCode into temp2
                       from t1 in temp1.DefaultIfEmpty()
                       from t2 in temp2.DefaultIfEmpty()
                       select new
                       {
                           Id = s.Id,
                           CustomerNo = s.CustomerNo,
                           CustomerName = s.CustomerName,
                           SalerName = s.SalerName,
                           DepartmentName = s.DepartmentName,
                           Type = s.Type,
                           Count = isAdmin ? (t1 == null ? 0 : t1.ReceiveTime) : 0,
                           fallinCount = isAdmin ? (t2 == null ? 0 : t2.fallinCount) : 0,
                           fallResult = t2 == null ? "" : t2.Remark,
                           Remark = s.Remark,
                           UpdateUser = s.UpdateUser,
                           UpdateDatetime = s.UpdateDatetime
                       };

            result.Data = data;
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

            //判断是否是公海管理员(普通业务员只能看到盲盒类型的数据)
            var isAdmin = _auth.GetCurrentUser().Roles.Any(r => r.Name == "公海管理员");

            //登陆人部门
            var dept = _userManagerApp.GetUserOrgInfo(_auth.GetCurrentUser().User.Id)?.Result?.OrgName;

            var customers = new List<string>();
            if (req.StartCount != null && req.EndCount != null)
            {
                string sql = $@"select t1.cardcode
                                from(
	                                select cardcode
	                                from customer_move_history
	                                group by cardcode
	                                having count(*) >= {req.StartCount}
                                ) as t1
                                join(
	                                select cardcode
	                                from customer_move_history
	                                group by cardcode
	                                having count(*) <= {req.EndCount}
                                ) as t2
                                on t1.cardcode = t2.cardcode;";
                var table = UnitWork.ExcuteSqlTable(ContextType.Nsap4ServeDbContextType, sql, System.Data.CommandType.Text);
                if (table != null && table.Rows.Count > 0)
                {
                    customers = table.AsEnumerable().Select(t => t.Field<string>("cardcode")).ToList();
                }
            }

            //查询已经掉入公海的客户(去掉黑名单上的客户)
            var query = from c in UnitWork.Find<CustomerList>(c => c.LabelIndex == 3)
                .WhereIf(customers.Count > 0, c => customers.Contains(c.CustomerNo))
                .WhereIf(isAdmin == false, c => c.DepartMent == dept)
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), c => c.CustomerNo == req.CardCode)
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardName), c => c.CustomerName.Contains(req.CardName))
                .WhereIf(!string.IsNullOrWhiteSpace(req.DepartMent), c => c.DepartMent == req.DepartMent)
                .WhereIf(req.CreateStartTime != null && req.CreateEndTime != null, c => c.CustomerCreateDate >= req.CreateStartTime && c.CustomerCreateDate < req.CreateEndTime.Value.AddDays(1))
                .WhereIf(req.FallIntoStartTime != null && req.FallIntoEndTime != null, c => c.CreateDateTime >= req.FallIntoStartTime &&
                     c.CreateDateTime < req.FallIntoEndTime.Value.AddDays(1))
                        join s in UnitWork.Find<SpecialCustomer>(s => s.Type == 0) on c.CustomerNo equals s.CustomerNo into temp
                        from t in temp.DefaultIfEmpty()
                        where t.CustomerNo == null
                        select new
                        {
                            c.CustomerNo,
                            c.CustomerName,
                            c.DepartMent,
                            c.SlpCode,
                            c.SlpName,
                            c.CreateUser,
                            CreateDateTime = c.CustomerCreateDate,
                            FallIntoTime = c.UpdateDateTime,
                        };

            //先把要查询的数据加载到内存
            var queryData = await query.OrderBy(q => q.CustomerNo).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            //上一次交易时间(取交货时间)
            var lastOrderTimeData = await UnitWork.Find<ODLN>(d => queryData.Select(x => x.CustomerNo).Contains(d.CardCode))
                .GroupBy(d => d.CardCode).Select(g => new
                {
                    CardCode = g.Key,
                    LastOrderTime = g.Max(x => x.CreateDate)
                }).ToListAsync();
            //总交易金额
            var totalMoneydata = (await (from o in UnitWork.Find<ODLN>(null)
                                         join d in UnitWork.Find<DLN1>(null) on o.DocEntry equals d.DocEntry
                                         where queryData.Select(x => x.CustomerNo).Contains(o.CardCode)
                                         select new
                                         {
                                             CardCode = o.CardCode,
                                             LineTotal = d.LineTotal
                                         }).ToListAsync())
                               .GroupBy(x => x.CardCode).Select(g => new { CardCode = g.Key, TotalMoney = g.Sum(x => x.LineTotal) }).ToList();
            //领取次数
            var receiveTimedata = await UnitWork.Find<CustomerSalerHistory>(c => queryData.Select(x => x.CustomerNo).Contains(c.CustomerNo))
                .GroupBy(c => c.CustomerNo)
                .Select(g => new
                {
                    CardCode = g.Key,
                    ReceiveTime = g.Count()
                }).ToListAsync();
            var data = from q in queryData
                       join l in lastOrderTimeData on q.CustomerNo equals l.CardCode into temp1
                       join t in totalMoneydata on q.CustomerNo equals t.CardCode into temp2
                       join r in receiveTimedata on q.CustomerNo equals r.CardCode into temp3
                       from t1 in temp1.DefaultIfEmpty()
                       from t2 in temp2.DefaultIfEmpty()
                       from t3 in temp3.DefaultIfEmpty()
                       select new QueryCustomerSeaListResponse
                       {
                           CustomerNo = q.CustomerNo,
                           CustomerName = q.CustomerName,
                           DisplayCustomerNo = isAdmin ? q.CustomerNo : "******",
                           DisplayCustomerName = isAdmin ? q.CustomerName : "******",
                           DepartMent = isAdmin ? q.DepartMent : "******",
                           SlpCode = q.SlpCode,
                           SlpName = isAdmin ? q.SlpName : "******",
                           CreateDateTime = q.CreateDateTime.Value,
                           FallIntoTime = q.FallIntoTime.Value,
                           LastOrderTime = isAdmin ? (t1 == null ? "" : t1.LastOrderTime.ToString("yyyyMMdd")) : null,
                           TotalMoney = isAdmin ? (t2 == null ? 0 : t2.TotalMoney) : null,
                           ReceiveTime = isAdmin ? (t3 == null ? 0 : t3.ReceiveTime) : 0
                       };


            result.Data = data;
            result.Count = await query.CountAsync();

            return result;
        }

        /// <summary>
        /// 获取客户的详细信息
        /// </summary>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        public async Task<TableData> GetCustomerDetail(string cardCode)
        {
            var result = new TableData();
            var isAdmin = _auth.GetCurrentUser().Roles.Any(r => r.Name == "公海管理员");
            if (!isAdmin)
            {
                return result;
            }

            var query = from c in UnitWork.Find<OCRD>(null)
                        join s in UnitWork.Find<OSLP>(null) on c.SlpCode equals s.SlpCode into temp1
                        from t1 in temp1.DefaultIfEmpty()
                        join h in UnitWork.Find<OHEM>(null) on c.DfTcnician equals h.empID into temp2
                        from t2 in temp2.DefaultIfEmpty()
                        where c.CardCode == cardCode
                        select new QueryCustomerDetailResponse
                        {
                            CardCode = c.CardCode, //客户代码
                            CardName = c.CardName, //客户类型
                            SlpName = t1 == null ? "" : t1.SlpName, //客户归属
                            DfTcnician = t2 == null ? "" : (t2.lastName + t2.firstName), //售后主管
                            Is_reseller = c.U_is_reseller, //是否是中间商
                            EndCustomerName = c.U_EndCustomerName, //终端用户名
                            EndCustomerContact = c.U_EndCustomerContact, //终端联系人
                            IntrntSite = c.IntrntSite, //网址
                            FreeText = c.Free_Text, //备注
                            Balance = c.Balance, //科目余额
                            TotalBalance = c.Balance, //总科目余额
                            OrdersBal = c.OrdersBal, //未清订单金额
                            DNotesBal = c.DNotesBal, //未清交货单金额
                            CreateTime = c.CreateDate.Value, //创建时间
                            UpdateTime = c.UpdateDate.Value, //更新时间
                        };

            var data = await query.FirstOrDefaultAsync();

            var query2 = await (UnitWork.Find<crm_ocrd>(c => c.CardCode == cardCode).Select(c => new
            {
                c.U_TradeType, //贸易类型
                c.U_ClientSource, //客户来源
                c.U_StaffScale, //人员规模
                c.U_CardTypeStr, //客户类型
                c.U_CompSector, //所属行业
            })).FirstOrDefaultAsync();

            data.U_CompSector = query2?.U_CompSector ?? "";
            data.U_TradeType = query2?.U_TradeType ?? "";
            data.U_ClientSource = query2?.U_ClientSource ?? "";
            data.U_StaffScale = query2?.U_StaffScale ?? "";
            data.U_CardTypeStr = query2?.U_CardTypeStr ?? "";

            if (!isAdmin)
            {
                data.CardCode = "******";
                data.CardName = "******";
                data.EndCustomerName = "******";
                data.EndCustomerContact = "******";
                data.IntrntSite = "******";
                data.FreeText = "******";
                data.Balance = null;
                data.TotalBalance = null;
                data.OrdersBal = null;
                data.DNotesBal = null;
            }

            result.Data = data;

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
                         join ud in UnitWork.Find<base_user_detail>(null) on u.user_id equals ud.user_id
                         join s in UnitWork.Find<sbo_user>(null)
                         .WhereIf(req.SlpCode != null && req.SlpCode > 0, u => u.sale_id == req.SlpCode)
                         on u.user_id equals s.user_id
                         where s.sbo_id == Define.SBO_ID && new int[] { 0, 1 }.Contains(ud.status)
                         select new
                         {
                             slpcode = s.sale_id,
                             slpname = u.user_nm
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
            response.Message = "";
            var userInfo = _auth.GetCurrentUser();
            if (userInfo == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //根据用户姓名查询slpcode
            var data = await (from u in UnitWork.Find<base_user>(null)
                              join d in UnitWork.Find<base_user_detail>(null) on u.user_id equals d.user_id
                              join s in UnitWork.Find<sbo_user>(null) on u.user_id equals s.user_id
                              select new { s.sale_id, u.user_nm, d.try_date }).ToListAsync();
            var slpInfo = (from n in data
                           join o in UnitWork.Find<OSLP>(null) on n.sale_id equals o.SlpCode
                           where n.user_nm == userInfo.User.Name
                           select new { n.sale_id, n.user_nm, n.try_date, o.SlpName }).FirstOrDefault();
            if (slpInfo == null)
            {
                response.Code = 500;
                response.Message = "业务员信息不存在";

                return response;
            }

            var customers = new List<CustomerList>();
            //公海设置
            var config = await UnitWork.Find<CustomerSeaConf>(null).FirstOrDefaultAsync();
            //以用户和日期作为key,限制用户每天领取的数量
            var key = $"{slpInfo.sale_id}:{DateTime.Now.Date.ToString("yyyyMMdd")}:";
            if (!RedisHelper.Exists(key))
            {
                //有效时间设为1天,单位为秒
                RedisHelper.Set(key, 0, 24 * 60 * 60);
            }
            //判断业务员当天领取的次数是否符合规范
            if (req.Customers.Count() > config.ReceiveMaxLimit)
            {
                response.Message = $"每个业务员每天只能领取{config?.ReceiveMaxLimit}次公海客户";
                response.Code = 500;
                return response;
            }

            //认领规则如果开启
            if (config?.ReceiveEnable == true)
            {
                var diffDate = (DateTime.Now - slpInfo.try_date).Days;
                //如果不在这个区间,则不能领取(业务上是入职时间太短或者太长都不能领取)
                if (!(diffDate > config.ReceiveJobMin && diffDate < config.ReceiveJobMax))
                {
                    response.Message = $"抱歉,您的入职时长不满足领取规定";
                    response.Code = 500;
                    return response;
                }
                //判断每天领取个数是否满足规定
                var dayNum = await RedisHelper.GetAsync<int>(key);
                if (dayNum + req.Customers.Count() > config?.ReceiveMaxLimit)
                {
                    response.Message = $"每个业务员每天只能领取{config?.ReceiveMaxLimit}个公海客户";
                    response.Code = 500;
                    return response;
                }
            }

            //查询业务员的客户数限制
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
            #region 逻辑判断
            foreach (var item in req.Customers)
            {
                //判断是否是公海客户,如果不是或者没有则不能进行领取
                if (!UnitWork.Find<CustomerList>(null).Any(c => c.CustomerNo == item.CustomerNo && c.LabelIndex == 3))
                {
                    response.Message += $"客户已被领取,请重新查询或刷新页面 \n";
                    continue;
                }
                var customer = await UnitWork.Find<CustomerList>(c => c.CustomerNo == item.CustomerNo).FirstOrDefaultAsync();
                //抢回限制如果是开启的
                if (config?.BackEnable == true)
                {
                    //如果领取的销售员跟原销售员是同一人
                    if (customer.SlpCode == slpInfo.sale_id)
                    {
                        //判断天数是否符合要求
                        if ((DateTime.Now - customer.CreateDateTime).Days <= config.BackDay)
                        {
                            response.Message += $"原业务员在{config.BackDay}天内不能抢回公海客户{item.CustomerNo} \n";
                            continue;
                        }
                    }
                }


                //是否是未成交客户
                var isNoQuotationCustomer = await (from c in UnitWork.Find<OCRD>(null)
                                                   join u in UnitWork.Find<OQUT>(null) on c.CardCode equals u.CardCode into temp
                                                   from t in temp.DefaultIfEmpty()
                                                   where c.CardCode == item.CustomerNo && t.CardCode == null
                                                   select c.CardCode).AnyAsync();
                //已报价未转销售单
                var isHasQuotationCustomer = await (from c in UnitWork.Find<OCRD>(null)
                                                    join u in UnitWork.Find<OQUT>(null) on c.CardCode equals u.CardCode
                                                    join r in UnitWork.Find<ORDR>(null) on c.CardCode equals r.CardCode into temp
                                                    from t in temp.DefaultIfEmpty()
                                                    where c.CardCode == item.CustomerNo && t.CardCode == null
                                                    select c.CardCode).AnyAsync();

                //已销售未转交货
                var isHasOrderCustomer = await (from c in UnitWork.Find<OCRD>(null)
                                                join r in UnitWork.Find<ORDR>(null) on c.CardCode equals r.CardCode
                                                join d in UnitWork.Find<ODLN>(null) on c.CardCode equals d.CardCode into temp
                                                from t in temp.DefaultIfEmpty()
                                                where c.CardCode == item.CustomerNo && t.CardCode == null
                                                select c.CardCode).AnyAsync();

                //是否是已成交客户
                var isFinishCUstomer = UnitWork.Find<ODLN>(d => d.CardCode == item.CustomerNo).Any();

                if (customerLimitRule != null)
                {
                    //如果客户是未报价客户,并且有未报价客户数量规则限制
                    if (isNoQuotationCustomer && customerLimitRule.CustomerType == 1)
                    {
                        var NoQuotationCount = await (from c in UnitWork.Find<OCRD>(null)
                                                      join s in UnitWork.Find<OSLP>(null) on c.SlpCode equals s.SlpCode
                                                      join u in UnitWork.Find<OQUT>(null) on c.CardCode equals u.CardCode into temp
                                                      from t in temp.DefaultIfEmpty()
                                                      where s.SlpName == slpInfo.SlpName && t.CardCode == null
                                                      select c.CardCode).Distinct().CountAsync();
                        if (NoQuotationCount >= customerLimitRule.Limit)
                        {
                            response.Message += "超过该用户的未报价客户数限制";
                            response.Code = 500;
                            return response;
                        }
                    }
                    //如果客户是已成交客户,并且有已成交客户数量规则限制
                    else if (isFinishCUstomer && customerLimitRule.CustomerType == 2)
                    {
                        var finishCount = await (from c in UnitWork.Find<OCRD>(null)
                                                 join s in UnitWork.Find<OSLP>(null) on c.SlpCode equals s.SlpCode
                                                 join d in UnitWork.Find<ODLN>(null) on c.CardCode equals d.CardCode
                                                 where s.SlpName == slpInfo.SlpName
                                                 select c.CardCode).Distinct().CountAsync();
                        if (finishCount >= customerLimitRule.Limit)
                        {
                            response.Message += "超过该用户的已成交客户数限制";
                            response.Code = 500;
                            return response;
                        }
                    }
                    //如果客户是报价单未转订单
                    else if (isHasQuotationCustomer && customerLimitRule.CustomerType == 3)
                    {
                        var count = await (from c in UnitWork.Find<OCRD>(null)
                                           join s in UnitWork.Find<OSLP>(null) on c.SlpCode equals s.SlpCode
                                           join u in UnitWork.Find<OQUT>(null) on c.CardCode equals u.CardCode
                                           join r in UnitWork.Find<ORDR>(null) on c.CardCode equals r.CardCode into temp
                                           from t in temp.DefaultIfEmpty()
                                           where s.SlpName == slpInfo.SlpName
                                           select c.CardCode).Distinct().CountAsync();
                        if (count > customerLimitRule.Limit)
                        {
                            response.Message += "超过该客户的已报价未转销售客户数限制";
                            response.Code = 500;
                            return response;
                        }
                    }
                    //如果是订单未转交货单
                    else if (isHasOrderCustomer && customerLimitRule.CustomerType == 4)
                    {
                        var count = await (from c in UnitWork.Find<OCRD>(null)
                                           join s in UnitWork.Find<OSLP>(null) on c.SlpCode equals s.SlpCode
                                           join r in UnitWork.Find<ORDR>(null) on c.CardCode equals r.CardCode
                                           join d in UnitWork.Find<ODLN>(null) on c.CardCode equals d.CardCode into temp
                                           from t in temp.DefaultIfEmpty()
                                           where s.SlpName == slpInfo.SlpName
                                           select c.CardCode).Distinct().CountAsync();
                        if (count > customerLimitRule.Limit)
                        {
                            response.Message += "超过该客户的已销售未转交货客户数限制";
                            response.Code = 500;
                            return response;
                        }
                    }
                    //都不是的话看是否有全部客户数限制
                    else if (customerLimitRule.CustomerType == 0)
                    {
                        var totalCount = await (from c in UnitWork.Find<OCRD>(null)
                                                join s in UnitWork.Find<OSLP>(null) on c.SlpCode equals s.SlpCode
                                                where s.SlpName == slpInfo.SlpName
                                                select c.CardCode).Distinct().CountAsync();
                        if (totalCount >= customerLimitRule.Limit)
                        {
                            response.Message += "超过该用户的最大客户数限制";
                            response.Code = 500;
                            return response;
                        }
                    }
                }

                customers.Add(customer);
            }

            #endregion
            //判断有问题
            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                response.Code = 500;
                return response;
            }

            using var tran = UnitWork.GetDbContext<CustomerLimit>().Database.BeginTransaction();
            try
            {
                foreach (var item in req.Customers)
                {
                    var history = new CustomerSalerHistory()
                    {
                        CustomerNo = item.CustomerNo,
                        CustomerName = item.CustomerName,
                        SlpCode = slpInfo.sale_id.Value,
                        SlpName = slpInfo.SlpName,
                        SlpDepartment = customers.Find(c => c.CustomerNo == item.CustomerNo).DepartMent,
                        CreateTime = DateTime.Now,
                        ReceiveTime = DateTime.Now,
                        ReleaseTime = customers.Find(c => c.CustomerNo == item.CustomerNo).CreateDateTime,
                        FallIntoTime = customers.Find(c => c.CustomerNo == item.CustomerNo).CreateDateTime,
                        IsSaleHistory = req.IsSaleHistory,
                        CreateUserId = userInfo.User.Id
                    };
                    int lastInstance = 0;
                    var isExists = UnitWork.Find<ACRD>(c => c.CardCode == item.CustomerNo).Any();
                    if (isExists)
                    {
                        lastInstance = UnitWork.Find<ACRD>(c => c.CardCode == item.CustomerNo).Max(x => x.LogInstanc) + 1;
                    }
                    else
                    {
                        lastInstance = 1;
                    }

                    //修改客户的销售员和更新修改时间

                    string FuncID = _serviceSaleOrderApp.GetJobTypeByAddress("client/clientAssignSeller.aspx");

                    var loginUser = userInfo.User;
                    var userId = loginUser.User_Id.Value;
                    var sboid = _serviceBaseApp.GetUserNaspSboID(userId);


                    clientOCRD client = new clientOCRD();
                    client.CardCode = item.CustomerNo;
                    client.SlpCode = slpInfo.sale_id.ToString();
                    client.SboId = "1";         //帐套
                    byte[] job_data = ByteExtension.ToSerialize(client);
                    string job_id = _serviceSaleOrderApp.WorkflowBuild("业务伙伴分配销售员", Convert.ToInt32(FuncID), userId, job_data, "业务伙伴分配销售员", 1, "", "", 0, 0, 0, "BOneAPI", "NSAP.B1Api.BOneOCRDAssign");
                    if (int.Parse(job_id) > 0)
                    {
                        string result = _serviceSaleOrderApp.WorkflowSubmit(int.Parse(job_id), userId, "业务伙伴分配销售员", "", 0);
                        //如果成功,则将客户从公海中移出(如果有的话)
                        if (result == "2")
                        {
                            //加入历史归属表
                            history.LogInstance = lastInstance;
                            await UnitWork.AddAsync<CustomerSalerHistory>(history);
                            //领取后,将客户从公海中移出
                            await UnitWork.DeleteAsync<CustomerList>(c => c.CustomerNo == item.CustomerNo);
                            await UnitWork.SaveAsync();
                        }
                    }
                    //var instance = await UnitWork.Find<OCRD>(c => c.CardCode == item.CustomerNo).FirstOrDefaultAsync();
                    //instance.SlpCode = slpInfo.sale_id;
                    //instance.UpdateDate = DateTime.Now;
                    //await UnitWork.UpdateAsync<OCRD>(instance);
                    ////3.0的客户归属表中新增一条记录
                    //await UnitWork.AddAsync<ACRD>(new ACRD { DocEntry = instance.DocEntry, CardCode = item.CustomerNo, LogInstanc = lastInstance, CardName = item.CustomerName, SlpCode = slpInfo.sale_id, CreateDate = DateTime.Now, UpdateDate = DateTime.Now });

                }
                await tran.CommitAsync();
                RedisHelper.IncrBy(key, req.Customers.Count); //领取成功后,用户当天的客户数量加1
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
            response.Message = "";

            var userInfo = _auth.GetCurrentUser();
            if (!userInfo.Roles.Any(r => r.Name == "公海管理员"))
            {
                response.Code = 500;
                response.Message = "非公海管理员不能进行分配操作";

                return response;
            }

            var customers = new List<CustomerList>();
            //查询业务员的客户数限制
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
            #region 逻辑判断
            foreach (var item in req.Customers)
            {
                var customer = await UnitWork.Find<CustomerList>(c => c.CustomerNo == item.CustomerNo).FirstOrDefaultAsync();
                if (customer == null)
                {
                    response.Message += $"客户{item.CustomerNo}不存在或已被领取 \n";
                    continue;
                }

                //是否是未报价客户
                var isNoQuotationCustomer = await (from c in UnitWork.Find<OCRD>(null)
                                                   join u in UnitWork.Find<OQUT>(null) on c.CardCode equals u.CardCode into temp
                                                   from t in temp.DefaultIfEmpty()
                                                   where c.CardCode == item.CustomerNo && t.CardCode == null
                                                   select c.CardCode).AnyAsync();
                //已报价未转销售单
                var isHasQuotationCustomer = await (from c in UnitWork.Find<OCRD>(null)
                                                    join u in UnitWork.Find<OQUT>(null) on c.CardCode equals u.CardCode
                                                    join r in UnitWork.Find<ORDR>(null) on c.CardCode equals r.CardCode into temp
                                                    from t in temp.DefaultIfEmpty()
                                                    where c.CardCode == item.CustomerNo && t.CardCode == null
                                                    select c.CardCode).AnyAsync();
                //已销售未转交货
                var isHasOrderCustomer = await (from c in UnitWork.Find<OCRD>(null)
                                                join r in UnitWork.Find<ORDR>(null) on c.CardCode equals r.CardCode
                                                join d in UnitWork.Find<ODLN>(null) on c.CardCode equals d.CardCode into temp
                                                from t in temp.DefaultIfEmpty()
                                                where c.CardCode == item.CustomerNo && t.CardCode == null
                                                select c.CardCode).AnyAsync();
                //是否是已成交客户
                var isFinishCUstomer = await UnitWork.Find<ODLN>(d => d.CardCode == item.CustomerNo).AnyAsync();

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
                    //如果客户是报价单未转订单
                    else if (isHasQuotationCustomer && customerLimitRule.CustomerType == 3)
                    {
                        var count = await (from c in UnitWork.Find<OCRD>(null)
                                           join s in UnitWork.Find<OSLP>(null) on c.SlpCode equals s.SlpCode
                                           join u in UnitWork.Find<OQUT>(null) on c.CardCode equals u.CardCode
                                           join r in UnitWork.Find<ORDR>(null) on c.CardCode equals r.CardCode into temp
                                           from t in temp.DefaultIfEmpty()
                                           where s.SlpName == req.SlpName
                                           select c.CardCode).Distinct().CountAsync();
                        if (count > customerLimitRule.Limit)
                        {
                            response.Message += "超过该客户的已报价未转销售客户数限制";
                            response.Code = 500;
                            return response;
                        }
                    }
                    //如果是订单未转交货单
                    else if (isHasOrderCustomer && customerLimitRule.CustomerType == 4)
                    {
                        var count = await (from c in UnitWork.Find<OCRD>(null)
                                           join s in UnitWork.Find<OSLP>(null) on c.SlpCode equals s.SlpCode
                                           join r in UnitWork.Find<ORDR>(null) on c.CardCode equals r.CardCode
                                           join d in UnitWork.Find<ODLN>(null) on c.CardCode equals d.CardCode into temp
                                           from t in temp.DefaultIfEmpty()
                                           where s.SlpName == req.SlpName
                                           select c.CardCode).Distinct().CountAsync();
                        if (count > customerLimitRule.Limit)
                        {
                            response.Message += "超过该客户的已销售未转交货客户数限制";
                            response.Code = 500;
                            return response;
                        }
                    }
                    //都不是的话看是否有全部客户数限制
                    else if (customerLimitRule.CustomerType == 0)
                    {
                        var totalCount = await (from c in UnitWork.Find<OCRD>(null)
                                                join s in UnitWork.Find<OSLP>(null) on c.SlpCode equals s.SlpCode
                                                where s.SlpName == req.SlpName
                                                select c.CardCode).Distinct().CountAsync();
                        if (totalCount >= customerLimitRule.Limit)
                        {
                            response.Code = 500;
                            response.Message = "超过该客户的最大客户数限制";
                            return response;
                        }
                    }
                }

                customers.Add(customer);
            }
            #endregion
            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                response.Code = 500;
                return response;
            }

            using var tran = UnitWork.GetDbContext<CustomerLimit>().Database.BeginTransaction();
            try
            {
                foreach (var item in req.Customers)
                {
                    var history = new CustomerSalerHistory()
                    {
                        CustomerNo = item.CustomerNo,
                        CustomerName = item.CustomerName,
                        SlpCode = req.SlpCode,
                        SlpName = req.SlpName,
                        SlpDepartment = customers.Find(c => c.CustomerNo == item.CustomerNo).DepartMent,
                        CreateTime = DateTime.Now,
                        ReceiveTime = DateTime.Now,
                        ReleaseTime = customers.Find(c => c.CustomerNo == item.CustomerNo).CreateDateTime,
                        FallIntoTime = customers.Find(c => c.CustomerNo == item.CustomerNo).CreateDateTime,
                        IsSaleHistory = req.IsSaleHistory
                    };

                    int lastInstance = 0;
                    var isExists = UnitWork.Find<ACRD>(c => c.CardCode == item.CustomerNo).Any();
                    if (isExists) { lastInstance = UnitWork.Find<ACRD>(c => c.CardCode == item.CustomerNo).Max(x => x.LogInstanc) + 1; }
                    else { lastInstance = 1; }



                    //修改客户的销售员

                    string FuncID = _serviceSaleOrderApp.GetJobTypeByAddress("client/clientAssignSeller.aspx");

                    var loginUser = userInfo.User;
                    var userId = loginUser.User_Id.Value;
                    var sboid = _serviceBaseApp.GetUserNaspSboID(userId);


                    clientOCRD client = new clientOCRD();
                    client.CardCode = item.CustomerNo;
                    client.SlpCode = req.SlpCode.ToString();
                    client.SboId = "1";         //帐套
                    byte[] job_data = ByteExtension.ToSerialize(client);
                    string job_id = _serviceSaleOrderApp.WorkflowBuild("业务伙伴分配销售员", Convert.ToInt32(FuncID), userId, job_data, "业务伙伴分配销售员", 1, "", "", 0, 0, 0, "BOneAPI", "NSAP.B1Api.BOneOCRDAssign");
                    if (int.Parse(job_id) > 0)
                    {
                        string result = _serviceSaleOrderApp.WorkflowSubmit(int.Parse(job_id), userId, "业务伙伴分配销售员", "", 0);
                        //如果成功,则将客户从公海中移出(如果有的话)
                        if (result == "2")
                        {
                            //加入历史归属表
                            history.LogInstance = lastInstance;
                            await UnitWork.AddAsync<CustomerSalerHistory>(history);
                            //领取后,将客户从公海中移出
                            await UnitWork.DeleteAsync<CustomerList>(c => c.CustomerNo == item.CustomerNo);
                            await UnitWork.SaveAsync();
                        }
                    }

                    //var instance = await UnitWork.Find<OCRD>(c => c.CardCode == item.CustomerNo).FirstOrDefaultAsync();
                    //instance.SlpCode = req.SlpCode;
                    //await UnitWork.UpdateAsync<OCRD>(instance);
                    ////3.0的客户归属表中新增一条记录
                    //await UnitWork.AddAsync<ACRD>(new ACRD { DocEntry = instance.DocEntry, CardCode = item.CustomerNo, LogInstanc = lastInstance, CardName = item.CustomerName, SlpCode = req.SlpCode, CreateDate = DateTime.Now, UpdateDate = DateTime.Now });
                    //await UnitWork.SaveAsync();
                }

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


        #region
        /// <summary>
        /// 根据客户代码查询历史归属记录(黑名单)
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetCustomerHistoryLists(QueryCustomerSalerListReq req)
        {
            var result = new TableData();
            var queryCustomerSalers = new List<QueryCustomerSalerListResponse>();
            DateTime dateStart = new DateTime(1970, 1, 1, 8, 0, 0);
            string addsql = $@"select sale_id,j.sync_dt from wfa_job j left join sbo_user u on j.user_id = u.user_id where job_nm ='添加业务伙伴:" + req.CardName + "' or card_name = '" + req.CardName + "'";
            var addtable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, addsql, System.Data.CommandType.Text);
            int saleid = (addtable != null && addtable.Rows.Count > 0) ? Convert.ToInt32(addtable.Rows[0]["sale_id"]) : 0;
            var slpinfo = (from o in UnitWork.Find<OSLP>(null)
                           where o.SlpCode == saleid
                           select new
                           {
                               o.SlpCode,
                               o.SlpName
                           }).FirstOrDefault();
            if (addtable != null && addtable.Rows.Count > 0)
            {
                queryCustomerSalers.Add(new QueryCustomerSalerListResponse
                {
                    SlpCode = saleid,
                    SalerName = slpinfo.SlpName,
                    type = 0,
                    movein_type = "创建",
                    remark = "",
                    CreateTime = Convert.ToDateTime(addtable.Rows[0]["sync_dt"]),
                    t = Convert.ToInt32((Convert.ToDateTime(addtable.Rows[0]["sync_dt"]) - dateStart).TotalSeconds)
                });
            }

            //查询客户领取掉落记录表
            string sql = $@"select a.* FROM (select SlpCode,SlpName,movein_type,remark,CreateTime,case WHEN movein_type = '按规则掉入' then 3 else 4 END type from customer_move_history where CardCode = '{req.CardCode}'
                            UNION select SlpCode,SlpName,case when Is_SaleHistory = 1 then'领取' else '分配' end ,'',CreateTime,case when Is_SaleHistory = 1 then 1 else 2 end type from Customer_Saler_History where customerNo = '{req.CardCode}') as a order by a.CreateTime";
            var table = UnitWork.ExcuteSqlTable(ContextType.Nsap4ServeDbContextType, sql, System.Data.CommandType.Text);

            if (table != null && table.Rows.Count > 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {

                    queryCustomerSalers.Add(new QueryCustomerSalerListResponse
                    {
                        SlpCode = Convert.ToInt32(table.Rows[i]["SlpCode"]),
                        SalerName = table.Rows[i]["SlpName"].ToString(),
                        type = table.Rows[i]["type"].ToInt(),
                        movein_type = table.Rows[i]["movein_type"].ToString(),
                        remark = table.Rows[i]["remark"].ToString(),
                        CreateTime = Convert.ToDateTime(table.Rows[i]["CreateTime"]),
                        t = Convert.ToInt32((Convert.ToDateTime(table.Rows[i]["CreateTime"]) - dateStart).TotalSeconds)
                    });
                }
            }

            result.Data = queryCustomerSalers;
            result.Count = queryCustomerSalers.Count();

            return result;
        }

        #endregion
    }
}
