using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.App
{
    /// <summary>
    /// 
    /// </summary>
    public class AppScanCodeApp : OnlyUnitWorkBaeApp
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public AppScanCodeApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {

        }
        /// <summary>
        /// 根据序列号获取所有guid
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public async Task<TableData> GetGuidBySn(string serialNumber)
        {
            var result = new TableData();
            var NwcaliBaseInfoList = await UnitWork.Find<NwcaliBaseInfo>(p => p.TesterSn.Equals(serialNumber)).Select(o => o.Id).ToListAsync();
            if (NwcaliBaseInfoList.Count > 0)
            {
                result.Data = await UnitWork.Find<PcPlc>(p => NwcaliBaseInfoList.Contains(p.NwcaliBaseInfoId)).OrderByDescending(c => c.CalibrationDate).Select(o => o.Guid).Distinct().ToListAsync();
            }
            return result;
        }

        /// <summary>
        /// 根据guid集合获取序列号集合
        /// </summary>
        /// <param name="guids"></param>
        /// <returns></returns>
        public async Task<TableData> GetSnListByGuidList(string guids)
        {
            var result = new TableData();
            var guidList = guids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            result.Data = await (from a in UnitWork.Find<PcPlc>(null)
                                 join b in UnitWork.Find<NwcaliBaseInfo>(null) on a.NwcaliBaseInfoId equals b.Id
                                 where guidList.Contains(a.Guid)
                                 select new { a.Guid, b.TesterSn }).Distinct().ToListAsync();
            return result;
        }
        /// <summary>
        /// 根据序列号获取订单信息
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public async Task<TableData> GetOrderInfoBySn(string sn)
        {
            var result = new TableData();
            List<string> list = new List<string>();
            result.Data = await (from a in UnitWork.Find<store_osrn>(null)
                                 join b in UnitWork.Find<store_itl1>(null) on new { a.ItemCode, a.SysNumber } equals new { b.ItemCode, b.SysNumber } into ab
                                 from b in ab.DefaultIfEmpty()
                                 join c in UnitWork.Find<store_oitl>(null) on new { b.ItemCode, b.LogEntry } equals new { c.ItemCode, c.LogEntry } into bc
                                 from c in bc.DefaultIfEmpty()
                                 join d in UnitWork.Find<sale_dln1>(null) on c.BaseEntry equals d.DocEntry into cd
                                 from d in cd.DefaultIfEmpty()
                                 where c.BaseType == 17 && a.MnfSerial == sn
                                 select new { a.MnfSerial, a.ItemCode, c.BaseEntry, d.DocDate }).FirstOrDefaultAsync();
            return result;
        }

        /// <summary>
        /// 通过序列号查询销售交货明细
        /// </summary>
        /// <param name="manufSN"></param>
        /// <param name="customer_code"></param>
        /// <returns></returns>
        public async Task<TableData> GetSalesDeliveryDetail(string manufSN, string customer_code)
        {
            var result = new TableData();
            result.Data = await (from a in UnitWork.Find<OINS>(null)
                                 join b in UnitWork.Find<DLN1>(null) on new { DocEntry = a.deliveryNo, ItemCode = a.itemCode } equals new { b.DocEntry, b.ItemCode }
                                 where a.manufSN == manufSN && a.customer == customer_code
                                 group new { b.ItemCode, b.DocEntry, b.Quantity }
                                 by new { b.ItemCode, b.DocEntry } into c
                                 select new { c.Key.ItemCode, c.Key.DocEntry, Quantity = c.Sum(c => c.Quantity) })
                                 .ToListAsync();
            return result;
        }

        /// <summary>
        /// 获取交货单对应物料的序列号列表
        /// </summary>
        /// <param name="deliveryNo">销售交货单号</param>
        /// <param name="itemCode">物料编码</param>
        /// <param name="customer_code">客户代码</param>
        /// <returns></returns>
        public async Task<TableData> GetManufSNList(int deliveryNo, string itemCode, string customer_code)
        {
            var result = new TableData();
            var isExistService = await UnitWork.Find<OINS>(c => c.deliveryNo == deliveryNo && c.itemCode == itemCode && c.customer == customer_code).Select(c => c.manufSN).Distinct().ToListAsync();
            result.Data = isExistService;
            return result;
        }

        /// <summary>
        /// 出厂序列号获取客户信息
        /// </summary>
        /// <param name="manufSN"></param>
        /// <returns></returns>
        public async Task<TableData> GetCustomerInfoBySn(string manufSN)
        {
            var result = new TableData();
            result.Data = await (from a in UnitWork.Find<OINS>(null)
                                 join b in UnitWork.Find<OCRD>(null) on a.customer equals b.CardCode
                                 where a.manufSN == manufSN
                                 select new { b.CardName, b.CardCode, b.CardType, a.manufSN })
                                 .FirstOrDefaultAsync();
            return result;
        }

        /// <summary>
        /// 获取客户信息
        /// </summary>
        /// <param name="customer_code"></param>
        /// <returns></returns>
        public async Task<TableData> GetCustomerInfo(string customer_code)
        {
            var result = new TableData();
            result.Data = await UnitWork.Find<OCRD>(c => c.CardCode == customer_code).Select(c => new { c.U_is_reseller, c.CardType, c.CardName, c.CardCode }).FirstOrDefaultAsync();
            return result;
        }

        /// <summary>
        /// 判断用户是否有服务单
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<TableData> IsExistServerOrder(int userId)
        {
            var result = new TableData();
            var isExistService = (await UnitWork.Find<ServiceOrder>(w => w.AppUserId == userId).ToListAsync())?.Count > 0 ? true : false;
            result.Data = isExistService;
            return result;
        }
    }
}
