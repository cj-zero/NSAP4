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
        public async Task<Infrastructure.Response> AddCustomer(List< AddCustomerListReq> model)
        {
            var result = new Infrastructure.Response();

            foreach (var item in model)
            {
                //映射
                var instance = item.MapTo<CustomerList>();
                //判断名单中是否已存在该客户
                var isExists = UnitWork.Find<CustomerList>(null).Any(c => c.CustomerNo == instance.CustomerNo && c.Isdelete == false);
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

                await UnitWork.AddAsync<CustomerList, int>(instance);
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

            var query = UnitWork.Find<CustomerList>(c => c.Type == req.Type && c.Isdelete == false).Select(x => new
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

            await UnitWork.DeleteAsync<CustomerList>(c => c.Id == id);
            await UnitWork.SaveAsync();

            return result;
        }
    }
}
