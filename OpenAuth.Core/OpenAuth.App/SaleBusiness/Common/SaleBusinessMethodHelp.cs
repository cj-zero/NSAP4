using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Helpers;
using OpenAuth.App.Response;
using OpenAuth.App.Interface;
using OpenAuth.App.Order;
using OpenAuth.Repository;
using OpenAuth.App.SaleBusiness.Request;
using OpenAuth.Repository.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.App.SaleBusiness.Common;
using Microsoft.EntityFrameworkCore;

namespace OpenAuth.App.SaleBusiness.Common
{
    /// <summary>
    /// 公共方法帮助类
    /// </summary>
    public class SaleBusinessMethodHelp : OnlyUnitWorkBaeApp
    {
        private  IUnitWork _UnitWork;
        private  IAuth _auth;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public SaleBusinessMethodHelp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
            _UnitWork = unitWork;
            _auth = auth;
        }

        #region 公共方法
        /// <summary>
        /// 获取登录者信息
        /// </summary>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回登录者信息以及业务员编码</returns>
        public User GetLoginUser(out int? slpCode)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            User loginUser = loginContext.User;
            slpCode = (UnitWork.Find<sbo_user>(r => r.user_id == loginUser.User_Id).FirstOrDefault()).sale_id;
            return loginUser;
        }

        /// <summary>
        /// 获取时间范围内新增客户
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回新增客户概况信息</returns>
        public async Task<int> GetTimeRangCustomer(string startTime, string endTime, int? slpCode)
        {
            int count = 0;
            List<QueryOCRD> ocrdList = await UnitWork.Find<OCRD>(r => r.SlpCode == slpCode).Select(r => new QueryOCRD { CardCode = r.CardCode, CreateDate = r.CreateDate, SlpCode = r.SlpCode, UpdateDate = r.UpdateDate }).ToListAsync();
            foreach (QueryOCRD item in ocrdList)
            {
                var exitSlpList = await UnitWork.Find<ACRD>(r => r.CardCode == item.CardCode).Select(r => new { r.UpdateDate, r.CreateDate, r.CardCode, r.SlpCode }).ToListAsync();
                if (exitSlpList.Count == 0)
                {
                    if (item.CreateDate >= Convert.ToDateTime(startTime) && item.CreateDate <= Convert.ToDateTime(endTime))
                    {
                        count = count + 1;
                    }
                }
                else
                {
                    var exitEqualList = exitSlpList.Where(r => r.SlpCode == item.SlpCode).OrderByDescending(r => r.UpdateDate).ToList();
                    if (exitEqualList.Count() > 0)
                    {
                        var exitNotList = exitSlpList.Where(r => r.SlpCode != item.SlpCode).OrderByDescending(r => r.UpdateDate).ToList();
                        if (exitNotList.Count() > 0)
                        {
                            exitSlpList = exitSlpList.Where(r => r.UpdateDate > exitNotList.FirstOrDefault().UpdateDate).OrderBy(r => r.UpdateDate).ToList();
                            if (exitSlpList.Count() > 0)
                            {
                                if (exitSlpList.FirstOrDefault().UpdateDate >= Convert.ToDateTime(startTime) && exitSlpList.FirstOrDefault().UpdateDate <= Convert.ToDateTime(endTime))
                                {
                                    count = count + 1;
                                }
                            }
                            else
                            {
                                if (item.UpdateDate >= Convert.ToDateTime(startTime) && item.UpdateDate <= Convert.ToDateTime(endTime))
                                {
                                    count = count + 1;
                                }
                            }
                        }
                        else
                        {
                            if (exitEqualList.Min(r => r.UpdateDate) >= Convert.ToDateTime(startTime) && exitEqualList.Min(r => r.UpdateDate) <= Convert.ToDateTime(endTime))
                            {
                                count = count + 1;
                            }
                        }
                    }
                    else
                    {
                        if (item.UpdateDate >= Convert.ToDateTime(startTime) && item.UpdateDate <= Convert.ToDateTime(endTime))
                        {
                            count = count + 1;
                        }
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// 获取时间范围集合与x轴坐标
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="timeType">时间类型</param>
        /// <returns>返回时间范围集合和x轴坐标</returns>
        public QueryTableData TimeRangeType(string startTime, string endTime, string timeType)
        {
            var result = new TableData();
            List<QueryTime> qtList = new List<QueryTime>();
            List<string> xNum = new List<string>();
          
            QueryTableData qta = new QueryTableData();
            qta.IsSuccess = true;
            if (timeType == "0")
            {
                DateTime sTime = Convert.ToDateTime(startTime + " 00:00:00");
                DateTime eTime = Convert.ToDateTime(endTime + " 23:59:59");
                TimeSpan ts = eTime - sTime;
                if (ts.Days <= 31)
                {
                    while (sTime <= eTime)
                    {
                        QueryTime qt = new QueryTime();
                        qt.startTime = sTime.ToString("yyyy-MM-dd 00:00:00");
                        xNum.Add(sTime.ToString("yyyy.MM.dd"));
                        qt.endTime = sTime.ToString("yyyy-MM-dd 23:59:59");
                        sTime = sTime.AddDays(1);
                        qtList.Add(qt);
                    }
                }
                else
                {
                    qta.IsSuccess = false;
                    qta.Message = "当查看维度为天，时间范围不能超过31天";
                }
            }
            else if (timeType == "1")
            {
                DateTime sTime = Convert.ToDateTime(startTime + "-01");
                DateTime eTime = Convert.ToDateTime(endTime + "-01");
                int ts = eTime.Month - sTime.Month;
                if (ts <= 12)
                {
                    while (sTime <= eTime)
                    {
                        QueryTime qt = new QueryTime();
                        qt.startTime = sTime.ToString("yyyy-MM-dd 00:00:00");

                        qt.endTime = (sTime.AddDays(1 - sTime.Day).AddMonths(1).AddDays(-1)).ToString("yyyy-MM-dd 23:59:59");
                        xNum.Add(sTime.ToString("yyyy.MM"));
                        sTime = sTime.AddMonths(1);
                        qtList.Add(qt);
                    }
                }
                else
                {
                    qta.IsSuccess = false;
                    qta.Message = "当查看维度为月，时间范围不能超过12个月";
                }
            }
            else
            {
                DateTime sTime = Convert.ToDateTime(startTime + "-01-01");
                DateTime eTime = Convert.ToDateTime(endTime + "-01-01");
                int ts = eTime.Year - sTime.Year;
                if (ts <= 10)
                {
                    while (sTime <= eTime)
                    {
                        QueryTime qt = new QueryTime();
                        qt.startTime = sTime.ToString("yyyy-01-01 00:00:00");
                        qt.endTime = sTime.ToString("yyyy-12-31 23:59:59");
                        xNum.Add(sTime.ToString("yyyy"));
                        sTime = sTime.AddYears(1);
                        qtList.Add(qt);
                    }
                }
                else
                {
                    qta.IsSuccess = false;
                    qta.Message = "当查看维度为年，时间范围不能超过10年";
                }
            }

            qta.queryTimes = qtList;
            qta.xNum = xNum;
            return qta;
        }

        /// <summary>
        /// 获取开始时间，结束时间
        /// </summary>
        /// <param name="timeRange">时间范围：0代表本周，1代表本月，2代表本季，3代表本年</param>
        /// <returns>返回时间范围集合</returns>
        public QueryTime TimeRange(string timeRange)
        {
            QueryTime qt = new QueryTime();
            DateTime dt = DateTime.Now;
            if (timeRange == "0")//本周
            {
                qt.startTime = dt.AddDays(1 - Convert.ToInt32(dt.DayOfWeek.ToString("d"))).ToString("yyyy-MM-dd") + " 00:00:00";
                qt.endTime = dt.AddDays(1 - Convert.ToInt32(dt.DayOfWeek.ToString("d"))).AddDays(6).ToString("yyyy-MM-dd") + " 23:59:59";
            }
            else if (timeRange == "1")//本月
            {
                qt.startTime = dt.AddDays(1 - (dt.Day)).ToString("yyyy-MM-dd") + " 00:00:00";
                qt.endTime = dt.AddDays(1 - (dt.Day)).AddDays(DateTime.DaysInMonth(dt.Date.Year, dt.Date.Month) - 1).ToString("yyyy-MM-dd") + " 23:59:59";
            }
            else if (timeRange == "2")//本季度
            {
                qt.startTime = dt.AddMonths(0 - (dt.Month - 1) % 3).AddDays(1 - dt.Day).ToString("yyyy-MM-dd") + " 00:00:00";
                qt.endTime = dt.AddMonths(0 - (dt.Month - 1) % 3).AddDays(1 - dt.Day).AddMonths(3).AddDays(-1).ToString("yyyy-MM-dd") + " 23:59:59";
            }
            else if (timeRange == "3")//本年
            {
                qt.startTime = new DateTime(dt.Year, 1, 1).ToString("yyyy-MM-dd") + " 00:00:00";
                qt.endTime = new DateTime(dt.Year, 12, 31).ToString("yyyy-MM-dd") + " 23:59:59";
            }

            return qt;
        }

        /// <summary>
        /// 获取时间范围内销售订单总金额
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="currency">币种</param>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回时间范围内销售订单总金额信息</returns>
        public async Task<decimal> GetORDRAmount(string startTime, string endTime, string currency, int? slpCode)
        {
            if (currency == "ALL")
            {
                var ordrList = await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode)
                                                      .WhereIf(startTime != "", r => r.CreateDate >= Convert.ToDateTime(startTime))
                                                      .WhereIf(endTime != "", r => r.CreateDate <= Convert.ToDateTime(endTime))
                                                      .Where(r => r.CANCELED == "N")
                                                      .ToListAsync();

                return Math.Round(ordrList.Sum(r => r.DocTotal).ToDecimal(), 2);
            }
            else
            {
                var ordrList = await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode && r.DocCur == currency)
                                                              .WhereIf(startTime != "", r => r.CreateDate >= Convert.ToDateTime(startTime))
                                                              .WhereIf(endTime != "", r => r.CreateDate <= Convert.ToDateTime(endTime))
                                                              .Where(r => r.CANCELED == "N")
                                                              .ToListAsync();

                return Math.Round(ordrList.Sum(r => currency == "RMB" ? r.DocTotal : r.DocTotalFC).ToDecimal(), 2);
            }
        }

        /// <summary>
        /// 获取时间范围内销售交货单总金额
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="currency">币种</param>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回时间范围内销售交货单总金额信息</returns>
        public async Task<decimal> GetODLNAmount(string startTime, string endTime, string currency, int? slpCode)
        {
            if (currency == "ALL")
            {
                var odlnList = await UnitWork.Find<ODLN>(r => r.SlpCode == slpCode)
                                                .WhereIf(startTime != "", r => r.CreateDate >= Convert.ToDateTime(startTime))
                                                .WhereIf(endTime != "", r => r.CreateDate <= Convert.ToDateTime(endTime))
                                                .Where(r => r.CANCELED == "N")
                                                .ToListAsync();

                return Math.Round(odlnList.Sum(r => r.DocTotal).ToDecimal(), 2);
            }
            else
            {
                var odlnList = await UnitWork.Find<ODLN>(r => r.SlpCode == slpCode && r.DocCur == currency)
                                                       .WhereIf(startTime != "", r => r.CreateDate >= Convert.ToDateTime(startTime))
                                                       .WhereIf(endTime != "", r => r.CreateDate <= Convert.ToDateTime(endTime))
                                                       .Where(r => r.CANCELED == "N")
                                                       .ToListAsync();

                return Math.Round(odlnList.Sum(r => currency == "RMB" ? r.DocTotal : r.DocTotalFC).ToDecimal(), 2);
            }
        }

        /// <summary>
        /// 获取时间范围内销售收款总金额
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="currency">币种</param>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回时间范围内销售收款总金额信息</returns>
        public async Task<decimal> GetORCTAmount(string startTime, string endTime, string currency, int? slpCode)
        {
            if (currency == "ALL")
            {
                var orctTotalList = from a in await UnitWork.Find<ORCT>(r => r.Canceled == "N").Select(r => new { r.U_XSDD, r.DocTotal, r.CreateDate }).ToListAsync()
                                    join b in await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode).Select(r => new { r.DocEntry }).ToListAsync() on a.U_XSDD equals b.DocEntry
                                    into ab
                                    from b in ab.DefaultIfEmpty()
                                    where a.CreateDate >= Convert.ToDateTime(startTime) && a.CreateDate <= Convert.ToDateTime(endTime)
                                    select new { a.DocTotal };

                return Math.Round(orctTotalList.Sum(r => r.DocTotal).ToDecimal(), 2);
            }
            else
            {
                var orctTotalList = from a in await UnitWork.Find<ORCT>(r => r.Canceled == "N" && r.DocCurr == currency).Select(r => new { r.U_XSDD, r.DocTotal, r.DocTotalFC, r.CreateDate }).ToListAsync()
                                    join b in await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode && r.DocCur == currency).Select(r => new { r.DocEntry }).ToListAsync() on a.U_XSDD equals b.DocEntry
                                    into ab
                                    from b in ab.DefaultIfEmpty()
                                    where a.CreateDate >= Convert.ToDateTime(startTime) && a.CreateDate <= Convert.ToDateTime(endTime)
                                    select new { a.DocTotal, a.DocTotalFC };

                return Math.Round(orctTotalList.Sum(r => currency == "RMB" ? r.DocTotal : r.DocTotalFC).ToDecimal(), 2);
            }
        }

        /// <summary>
        /// 获取时间范围内增值税发票金额
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="currency">币种</param>
        /// <param name="slpCode">业务员编码</param>
        /// <param name="loginUser">当前登录者信息</param>
        /// <returns>返回时间范围内增值税发票金额信息</returns>
        public async Task<decimal> GetFBMAmount(string startTime, string endTime, string currency, int? slpCode, User loginUser)
        {
            if (currency == "ALL")
            {
                var ordrList = await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode).Where(r => r.CANCELED == "N").ToListAsync();
                List<int> docEntryList = ordrList.Select(r => r.DocEntry).ToList();
                var billList = await UnitWork.Find<finance_billapplication_master>(r => docEntryList.Contains(Convert.ToInt32(r.DocEntry)))
                                                     .WhereIf(startTime != "", r => r.ApplicationTime >= Convert.ToDateTime(startTime))
                                                     .WhereIf(endTime != "", r => r.ApplicationTime <= Convert.ToDateTime(endTime))
                                                     .Where(r => r.billStatus != 2 && r.sbo_id == 1)
                                                     .ToListAsync();

                return Math.Round(billList.Sum(r => r.totalmn).ToDecimal(), 2);
            }
            else
            {
                var billList = await UnitWork.Find<finance_billapplication_master>(r => r.Applicantor == loginUser.User_Id)
                                                     .WhereIf(startTime != "", r => r.ApplicationTime >= Convert.ToDateTime(startTime))
                                                     .WhereIf(endTime != "", r => r.ApplicationTime <= Convert.ToDateTime(endTime))
                                                     .Where(r => r.billStatus != 2 && r.sbo_id == 1)
                                                     .ToListAsync();

                List<int?> docEntryList = billList.Select(r => r.DocEntry).ToList();
                var ordrList = await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode && docEntryList.Contains(r.DocEntry)).Where(r => r.CANCELED == "N" && r.DocCur == currency).ToListAsync();
                return Math.Round(ordrList.Sum(r => currency == "RMB" ? r.DocTotal : r.DocTotalFC).ToDecimal(), 2);
            }
        }

        /// <summary>
        /// 获取时间范围内贷项凭证金额
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="currency">币种</param>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回时间范围内贷项凭证金额信息</returns>
        public async Task<decimal> GetORINAmount(string startTime, string endTime, string currency, int? slpCode)
        {
            if (currency == "ALL")
            {
                var orinList = await UnitWork.Find<ORIN>(r => r.SlpCode == slpCode)
                                                    .WhereIf(startTime != "", r => r.CreateDate >= Convert.ToDateTime(startTime))
                                                    .WhereIf(endTime != "", r => r.CreateDate <= Convert.ToDateTime(endTime))
                                                    .Where(r => r.CANCELED != "N")
                                                    .ToListAsync();

                return Math.Round(orinList.Sum(r => r.DocTotal).ToDecimal(), 2);
            }
            else
            {
                var orinList = await UnitWork.Find<ORIN>(r => r.SlpCode == slpCode && r.DocCur == currency)
                                                        .WhereIf(startTime != "", r => r.CreateDate >= Convert.ToDateTime(startTime))
                                                        .WhereIf(endTime != "", r => r.CreateDate <= Convert.ToDateTime(endTime))
                                                        .Where(r => r.CANCELED != "N")
                                                        .ToListAsync();

                return Math.Round(orinList.Sum(r => currency == "RMB" ? r.DocTotal : r.DocTotalFC).ToDecimal(), 2);
            }
        }
        #endregion
    }
}
