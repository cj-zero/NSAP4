using Infrastructure;
using OpenAuth.App.Interface;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;

namespace OpenAuth.App.Material
{
    public class SalesOrderWarrantyDateApp : OnlyUnitWorkBaeApp
    {
        public SalesOrderWarrantyDateApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
        }

        /// <summary>
        /// 获取销售订单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> Load(SalesOrderWarrantyDateReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();

            var SalesOrderWarrantyDates = UnitWork.Find<SalesOrderWarrantyDate>(null).Include(s=>s.SalesOrderWarrantyDateRecords)
                .WhereIf(!string.IsNullOrWhiteSpace(req.Customer), q => q.CustomerId.Contains(req.Customer) || q.CustomerName.Contains(req.Customer))
                .WhereIf(!string.IsNullOrWhiteSpace(req.SalesOrderId.ToString()), q => q.SalesOrderId.Equals(req.SalesOrderId))
                .WhereIf(!string.IsNullOrWhiteSpace(req.SalesMan), q => q.SalesOrderName.Equals(req.SalesMan));
            result.Count = await SalesOrderWarrantyDates.CountAsync();
            result.Data = await SalesOrderWarrantyDates.Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();

            return result;
        }
        #region 添加
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        //public async Task Add(AddOrUpdatesalesorderwarrantydateReq req)
        //{
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    var SalesOrderWarrantyDateMap = req.MapTo<SalesOrderWarrantyDate>();
        //    var SalesOrderWarrantyDates = await UnitWork.AddAsync<SalesOrderWarrantyDate>(SalesOrderWarrantyDateMap);
        //    await UnitWork.AddAsync<SalesOrderWarrantyDateRecord>(new SalesOrderWarrantyDateRecord
        //    {
        //        Id = Guid.NewGuid().ToString(),
        //        SalesOrderWarrantyDateId = SalesOrderWarrantyDates.Id,
        //        Action = loginContext.User.Name + "修改保修时间为" + req.WarrantyPeriod,
        //        CreateTime = DateTime.Now,
        //        CreateUser = loginContext.User.Name,
        //        CreateUserId = loginContext.User.Id
        //    });
        //    await UnitWork.SaveAsync();
        //}
        #endregion

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task UpDate(AddOrUpdatesalesorderwarrantydateReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            await UnitWork.UpdateAsync<SalesOrderWarrantyDate>(s => s.Id.Equals(req.Id), s => new SalesOrderWarrantyDate
            {
                WarrantyPeriod = req.WarrantyPeriod,
                IsPass=false
            });
            await UnitWork.AddAsync<SalesOrderWarrantyDateRecord>(new SalesOrderWarrantyDateRecord
            {
                Id = Guid.NewGuid().ToString(),
                SalesOrderWarrantyDateId = req.Id,
                Action = loginContext.User.Name + "修改保修时间为" + req.WarrantyPeriod,
                CreateTime = DateTime.Now,
                CreateUser = loginContext.User.Name,
                CreateUserId = loginContext.User.Id
            });
            await UnitWork.SaveAsync();

        }

        /// <summary>
        /// 审批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Approval(AddOrUpdatesalesorderwarrantydateReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            await UnitWork.UpdateAsync<SalesOrderWarrantyDate>(s => s.Id.Equals(req.Id), s => new SalesOrderWarrantyDate
            {
                IsPass=req.IsPass
            });
            await UnitWork.AddAsync<SalesOrderWarrantyDateRecord>(new SalesOrderWarrantyDateRecord
            {
                Id = Guid.NewGuid().ToString(),
                SalesOrderWarrantyDateId = req.Id,
                Action = (bool)req.IsPass?"通过" :"驳回",
                CreateTime = DateTime.Now,
                CreateUser = loginContext.User.Name,
                CreateUserId = loginContext.User.Id
            });
            await UnitWork.SaveAsync();


        }

        //保修时间同步
        //private async Task<TableData> GetSalesOrder(SalesOrderWarrantyDateReq req)
        //{
        //    TableData result = new TableData();
        //    var query = from a in UnitWork.Find<ORDR>(null)
        //                join b in UnitWork.Find<OSLP>(null) on a.SlpCode equals b.SlpCode
        //                join c in UnitWork.Find<ODLN>(null) on a.DocEntry equals c.DocEntry
        //                select new { a, b,c };
        //    query = query.WhereIf(!string.IsNullOrWhiteSpace(req.Customer), q => q.a.CardCode.Contains(req.Customer) || q.a.CardName.Contains(req.Customer))
        //                .WhereIf(!string.IsNullOrWhiteSpace(req.SalesOrderId.ToString()), q => q.a.DocEntry.Equals(req.SalesOrderId))
        //                .WhereIf(!string.IsNullOrWhiteSpace(req.SalesMan), q => q.b.SlpName.Equals(req.SalesMan));
        //    result.Data = await query.ToListAsync();
        //    return null;
        //}

    }
}
