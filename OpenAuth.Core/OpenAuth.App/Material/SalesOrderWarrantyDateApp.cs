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

            var SalesOrderWarrantyDates = UnitWork.Find<SalesOrderWarrantyDate>(null).Include(s => s.SalesOrderWarrantyDateRecords)
                .WhereIf(!string.IsNullOrWhiteSpace(req.Customer), q => q.CustomerId.Contains(req.Customer) || q.CustomerName.Contains(req.Customer))
                .WhereIf(!string.IsNullOrWhiteSpace(req.SalesOrderId.ToString()), q => q.SalesOrderId.Equals(req.SalesOrderId))
                .WhereIf(!string.IsNullOrWhiteSpace(req.SalesMan), q => q.SalesOrderName.Equals(req.SalesMan))
               ;
            if (!loginContext.Roles.Any(r => r.Name.Equals("总助")))
            {
                SalesOrderWarrantyDates = SalesOrderWarrantyDates.Where(q => q.SalesOrderName.Equals(loginContext.User.Name));
            }
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
                IsPass = false
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
                IsPass = req.IsPass
            });
            await UnitWork.AddAsync<SalesOrderWarrantyDateRecord>(new SalesOrderWarrantyDateRecord
            {
                Id = Guid.NewGuid().ToString(),
                SalesOrderWarrantyDateId = req.Id,
                Action = (bool)req.IsPass ? "通过" : "驳回",
                CreateTime = DateTime.Now,
                CreateUser = loginContext.User.Name,
                CreateUserId = loginContext.User.Id
            });
            await UnitWork.SaveAsync();


        }

        /// <summary>
        /// 保修时间同步
        /// </summary>
        /// <param name="DocEntry"></param>
        /// <returns></returns>
        public async Task SynchronizationSalesOrder()
        {
            var docEntry = await UnitWork.Find<SalesOrderWarrantyDate>(null).OrderByDescending(s => s.SalesOrderId).Select(s => s.SalesOrderId).FirstOrDefaultAsync();
            var query = from a in UnitWork.Find<ORDR>(null)
                        join b in UnitWork.Find<OSLP>(null) on a.SlpCode equals b.SlpCode
                        join c in UnitWork.Find<ODLN>(null) on a.DocEntry equals c.DocEntry
                        select new { a, b, c };
            docEntry = docEntry == null?0:docEntry;
            var model = await query.Where(q => q.a.DocEntry > docEntry).ToListAsync();

            var salesOrderWarrantyDates = model.Select(m => new SalesOrderWarrantyDate
            {
                SalesOrderId = m.a.DocEntry,
                CustomerId = m.a.CardCode,
                CustomerName = m.a.CardName,
                SalesOrderName = m.b.SlpName,
                DeliveryDate = m.c.CreateDate,
                CreateTime = DateTime.Now,
                WarrantyPeriod = Convert.ToDateTime(m.c.CreateDate).AddMonths(13),
                IsPass = true
            }).ToList();
            await UnitWork.BatchAddAsync<SalesOrderWarrantyDate>(salesOrderWarrantyDates.ToArray());
            await UnitWork.SaveAsync();
        }

    }
}
