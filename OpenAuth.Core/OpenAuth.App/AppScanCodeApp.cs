using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
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
    public class AppScanCodeApp: OnlyUnitWorkBaeApp
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
        /// 根据序列号获取下位机有效的校准证书guid
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public async Task<TableData> GetCertInfoBySn(string serialNumber)
        {
            var result = new TableData();
            var NwcaliBaseInfoList = await UnitWork.Find<NwcaliBaseInfo>(p => p.TesterSn.Equals(serialNumber)).Select(o => o.Id).ToListAsync();
            if (NwcaliBaseInfoList.Count>0)
            {
                result.Data = await UnitWork.Find<PcPlc>(p => NwcaliBaseInfoList.Contains(p.NwcaliBaseInfoId)).OrderByDescending(c => c.CalibrationDate).Select(o => o.Guid).ToListAsync();
            }
            return result;
        }
        /// <summary>
        /// 根据中位机序列号获取中位机下所有guid
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public async Task<TableData> GetGuidBySn(string serialNumber)
        {
            var result = new TableData();
            List<string> list = new List<string>();
            var MnfSerialList = (from a in UnitWork.Find<store_oitl>(null)
                                 join b in UnitWork.Find<store_itl1>(null) on new { a.LogEntry, a.ItemCode } equals new { b.LogEntry, b.ItemCode } into ab
                                 from b in ab.DefaultIfEmpty()
                                 join c in UnitWork.Find<store_osrn>(null) on new { b.ItemCode, b.SysNumber } equals new { c.ItemCode, c.SysNumber } into bc
                                 from c in bc.DefaultIfEmpty()
                                 where a.DocType == 17 && c.MnfSerial == serialNumber
                                 select  c.MnfSerial ).Distinct().ToList();
            if (MnfSerialList.Count>0)
            {
                result.Data= await (from a in UnitWork.Find<PcPlc>(null)
                                   join b in UnitWork.Find<NwcaliBaseInfo>(null) on a.NwcaliBaseInfoId equals b.Id
                                   where MnfSerialList.Contains(b.TesterSn) && a.ExpirationDate <= DateTime.Now
                                   select new { a.Guid, a.ExpirationDate, a.CalibrationDate }).OrderByDescending(c => c.CalibrationDate).Select(c => c.Guid).Distinct().ToListAsync();
            }
            return result;
        }
    }
}
