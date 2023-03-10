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
                .WhereIf(!string.IsNullOrWhiteSpace(req.ManufacturerSerialNumber), q => q.MnfSerial.Contains(req.ManufacturerSerialNumber))
                .WhereIf(!string.IsNullOrWhiteSpace(req.SalesMan), q => q.SalesOrderName.Equals(req.SalesMan));
            if (req.State != null && req.State == 2)
            {
                SalesOrderWarrantyDates = SalesOrderWarrantyDates.Where(q => q.IsPass == false);
            }
            if (!loginContext.Roles.Any(r => r.Name.Equals("总助")))
            {
                var userId = (await UnitWork.Find<NsapUserMap>(n => n.UserID.Equals(loginContext.User.Id)).FirstOrDefaultAsync())?.NsapUserId;
                var slpCode = (await UnitWork.Find<sbo_user>(s => s.user_id == userId && s.sbo_id == Define.SBO_ID).FirstOrDefaultAsync())?.sale_id;
                SalesOrderWarrantyDates = SalesOrderWarrantyDates
                                          //.WhereIf(string.IsNullOrWhiteSpace(slpCode.ToString()), q => q.SalesOrderName.Equals(loginContext.User.Name))
                                          .WhereIf(!string.IsNullOrWhiteSpace(slpCode.ToString()), q => q.SlpCode == slpCode || q.SalesOrderWarrantyDateRecords.Any(s=>s.CreateUserId.Equals(loginContext.User.Id)));
            }
            result.Count = await SalesOrderWarrantyDates.CountAsync();
            var warrantyDates = await SalesOrderWarrantyDates.Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            warrantyDates.ForEach(s => s.SalesOrderWarrantyDateRecords=s.SalesOrderWarrantyDateRecords.OrderByDescending(w => w.CreateTime).ToList());
            result.Data = warrantyDates;
            return result;
        }

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
                CreateTime = DateTime.Now,
                CreateUser = loginContext.User.Name,
                CreateUserId = loginContext.User.Id
            });
            await UnitWork.SaveAsync();


        }

        /// <summary>
        /// 保修时间同步
        /// </summary>
        /// <returns></returns>
        public void SynchronizationSalesOrder(string SalesOrderId=null)
        {
            var docEntry = UnitWork.Find<SalesOrderWarrantyDate>(null).OrderByDescending(s => s.SalesOrderId).Select(s => s.SalesOrderId).FirstOrDefault();
            docEntry = docEntry == null ? 0 : docEntry;

            var query = from a in UnitWork.Find<ORDR>(null)
                        join b in UnitWork.Find<OITL>(null) on new { BaseEntry = a.DocEntry, BaseType = 17 } equals new { BaseEntry = (int)b.BaseEntry, BaseType = (int)b.BaseType } into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<ITL1>(null) on new { b.LogEntry, b.ItemCode } equals new { c.LogEntry, c.ItemCode }
                        join d in UnitWork.Find<OSRN>(null) on new { c.ItemCode, SysNumber = c.SysNumber.Value } equals new { d.ItemCode, d.SysNumber } into cd
                        from d in cd.DefaultIfEmpty()
                        join e in UnitWork.Find<OSLP>(null) on a.SlpCode equals e.SlpCode into ae
                        from e in ae.DefaultIfEmpty()
                        where b.DocType == 15 && a.DocEntry > docEntry && a.DocEntry < (docEntry + 10000)
                        select new { d.MnfSerial, b.BaseEntry, b.CardCode, b.CardName, b.DocType, b.CreateDate, e.SlpName, e.SlpCode };

            var model = query.WhereIf(!string.IsNullOrWhiteSpace(SalesOrderId),q=>q.BaseEntry==int.Parse(SalesOrderId)).Where(q => !string.IsNullOrWhiteSpace(q.MnfSerial)).Select(m => new SalesOrderWarrantyDate
            {
                SalesOrderId = m.BaseEntry,
                CustomerId = m.CardCode,
                CustomerName = m.CardName,
                SalesOrderName = m.SlpName,
                MnfSerial = m.MnfSerial,
                DeliveryDate = m.CreateDate,
                SlpCode = m.SlpCode,
                WarrantyPeriod = Convert.ToDateTime(m.CreateDate).AddMonths(13),
                IsPass = true
            }).ToList();
            model = model.GroupBy(m => new { m.MnfSerial,m.CustomerId}).Select(m => m.OrderByDescending(m=>m.WarrantyPeriod).First()).ToList();
            model.ForEach(m => m.CreateTime = DateTime.Now);
            var warrantyDates= UnitWork.Find<SalesOrderWarrantyDate>(s=> model.Select(m=>m.MnfSerial).ToList().Contains(s.MnfSerial)).ToList();
            List<string> mnfSerials = new List<string>();
            if (warrantyDates.Count()>0) 
            {
                warrantyDates.ForEach(w => 
                {
                    if (w.WarrantyPeriod < model.Where(m => m.MnfSerial.Equals(w.MnfSerial)).FirstOrDefault().WarrantyPeriod)
                    {
                        var modelObj = model.Where(m => m.MnfSerial.Equals(w.MnfSerial)).FirstOrDefault();
                        UnitWork.Update<SalesOrderWarrantyDate>(s => s.Id == w.Id, s => new SalesOrderWarrantyDate
                        {
                            CustomerId = modelObj.CustomerId,
                            CreateTime = modelObj.CreateTime,
                            CustomerName = modelObj.CustomerName,
                            SlpCode = modelObj.SlpCode,
                            SalesOrderId = modelObj.SalesOrderId,
                            SalesOrderName = modelObj.SalesOrderName,
                            DeliveryDate = modelObj.DeliveryDate,
                            MnfSerial = modelObj.MnfSerial,
                            IsPass = modelObj.IsPass,
                            WarrantyPeriod = modelObj.WarrantyPeriod,
                            Remark = modelObj.Remark
                        });
                        mnfSerials.Add(w.MnfSerial);
                    }
                    else 
                    {
                        mnfSerials.Add(w.MnfSerial);
                    }
                });
                model = model.Where(m => !mnfSerials.Contains(m.MnfSerial)).ToList();
            }
            UnitWork.BatchAdd<SalesOrderWarrantyDate>(model.ToArray());
            UnitWork.Save();
        }

    }
}
