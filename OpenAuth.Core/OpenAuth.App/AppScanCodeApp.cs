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
        /// 根据机序列号获取所有guid
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public async Task<TableData> GetGuidBySn(string serialNumber)
        {
            var result = new TableData();
            //根据物料编码区分下位机还是中位机
            var devInfo = UnitWork.Find<store_osrn>(p => p.MnfSerial.Equals(serialNumber)).FirstOrDefault();
            if (devInfo == null)
                return result;
            if (devInfo.ItemCode.ToLower().Contains("zwj"))
            {
                List<string> list = new List<string>();
                var MnfSerialList = (from a in UnitWork.Find<store_oitl>(null)
                                     join b in UnitWork.Find<store_itl1>(null) on new { a.LogEntry, a.ItemCode } equals new { b.LogEntry, b.ItemCode } into ab
                                     from b in ab.DefaultIfEmpty()
                                     join c in UnitWork.Find<store_osrn>(null) on new { b.ItemCode, b.SysNumber } equals new { c.ItemCode, c.SysNumber } into bc
                                     from c in bc.DefaultIfEmpty()
                                     where a.DocType == 17 && c.MnfSerial == serialNumber
                                     select c.MnfSerial).Distinct().ToList();
                if (MnfSerialList.Count > 0)
                {
                    result.Data = await (from a in UnitWork.Find<PcPlc>(null)
                                         join b in UnitWork.Find<NwcaliBaseInfo>(null) on a.NwcaliBaseInfoId equals b.Id
                                         where MnfSerialList.Contains(b.TesterSn) && a.ExpirationDate <= DateTime.Now
                                         select new { a.Guid, a.ExpirationDate, a.CalibrationDate }).OrderByDescending(c => c.CalibrationDate).Select(c => c.Guid).Distinct().ToListAsync();
                }
                return result;
            }
            else
            {
                var NwcaliBaseInfoList = await UnitWork.Find<NwcaliBaseInfo>(p => p.TesterSn.Equals(serialNumber)).Select(o => o.Id).ToListAsync();
                if (NwcaliBaseInfoList.Count > 0)
                {
                    result.Data = await UnitWork.Find<PcPlc>(p => NwcaliBaseInfoList.Contains(p.NwcaliBaseInfoId)).OrderByDescending(c => c.CalibrationDate).Select(o => o.Guid).Distinct().ToListAsync();
                }
                return result;
            }
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
                                       join d in UnitWork.Find<sale_dln1>(null) on  c.BaseEntry  equals d.DocEntry into cd
                                       from d in cd.DefaultIfEmpty()
                                       where c.BaseType == 17 && a.MnfSerial == sn
                                       select new { a.MnfSerial,a.ItemCode, c.BaseEntry,d.DocDate }).FirstOrDefaultAsync();
            return result;
        }

        /// <summary>
        /// 通过序列号查询销售交货明细
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="customer_code"></param>
        /// <returns></returns>
        public async Task<TableData> GetSalesDeliveryDetail(string sn,string customer_code)
        {
            var result = new TableData();
            List<string> list = new List<string>();
            result.Data = await (from a in UnitWork.Find<OSRIModel>(null)
                                 join b in UnitWork.Find<OITL>(null) on a.ItemCode equals b.ItemCode
                                 join c in UnitWork.Find<ITL1>(null) on new { b.LogEntry, SysNumber = a.SysSerial } equals new { c.LogEntry, c.SysNumber }
                                 join d in UnitWork.Find<ODLN>(null) on b.DocEntry equals d.DocEntry
                                 join e in UnitWork.Find<DLN1>(null) on new { d.DocEntry,a.ItemCode } equals new { e.DocEntry,e.ItemCode }
                                 where b.DocType == 15 && a.SuppSerial == sn && d.CardCode== customer_code
                                 select new { a.SuppSerial, d.CardCode, d.CardName, d.DocEntry,e.ItemCode, e.Quantity }).ToListAsync();
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
