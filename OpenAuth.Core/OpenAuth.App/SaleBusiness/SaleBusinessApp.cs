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

namespace OpenAuth.App.SaleBusiness
{
    public class SaleBusinessApp: OnlyUnitWorkBaeApp
    {
        private IUnitWork _UnitWork;
        private IAuth _auth;
        private ServiceSaleOrderApp _serviceSaleOrderApp;
        private SaleBusinessMethodHelp _saleBusinessMethodHelp;
        private ServiceBaseApp _serviceBaseApp;
        private DateTime NowYearFirstDay = Convert.ToDateTime(DateTime.Now.Year.ToString() + "-01-01 00:00:00");//当前年份第一天
        private DateTime NowYearLastDay = Convert.ToDateTime(DateTime.Now.Year.ToString() + "-12-31 23:59:59");//当前年份最后一天
        private DateTime LastYearFirstDay = Convert.ToDateTime(DateTime.Now.AddYears(-1).Year.ToString() + "-01-01 00:00:00");//去年年份第一天
        private DateTime LastYearLastDay = Convert.ToDateTime(DateTime.Now.AddYears(-1).Year.ToString() + "-12-31 23:59:59");//去年年份最后一天
        private const string OQUT = "OQUT";//销售报价单
        private const string ORDR = "ORDR";//销售订单
        private const string ODLN = "ODLN";//销售交货单
        private const string OINV = "OINV";//应收发票
        private const string ORDN = "ORDN";//销售退货单
        private const string ORIN = "ORIN";//应收贷项凭证
        private const string XSBJD = "销售报价单";
        private const string XSDD = "销售订单";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        /// <param name="serviceSaleOrderApp"></param>
        /// <param name="serviceBaseApp"></param>
        /// <param name="saleBusinessMethodHelp"></param>
        public SaleBusinessApp(IUnitWork unitWork, IAuth auth, ServiceSaleOrderApp serviceSaleOrderApp, ServiceBaseApp serviceBaseApp, SaleBusinessMethodHelp saleBusinessMethodHelp) : base(unitWork, auth)
        {
            _UnitWork = unitWork;
            _auth = auth;
            _serviceSaleOrderApp = serviceSaleOrderApp;
            _saleBusinessMethodHelp = saleBusinessMethodHelp;
            _serviceBaseApp = serviceBaseApp;
        }

        #region 销售情况概览
        /// <summary>
        /// 销售情况概览
        /// </summary>
        /// <param name="timeRange">时间范围</param>
        /// <returns>返回销售情况概览数据集合</returns>
        public async Task<TableData> GetSaleSituation(string timeRange)
        {
            User loginUser = _saleBusinessMethodHelp.GetLoginUser(out int? slpCode);
            var result = new TableData();
            if (slpCode == 0)
                return result;

            List<QuerySaleBusinessRequest> saleList = new List<QuerySaleBusinessRequest>();
            QueryTime qt = _saleBusinessMethodHelp.TimeRange(timeRange);
            if (qt == null || qt.endTime == null || qt.startTime == null)
            {
                saleList.Add(await GetCustomer("", "", slpCode));//获取全部新增客户
                saleList.Add(await GetSaleTypeDocTotal(OQUT, "", "", slpCode));//获取销售报价单总金额
                saleList.Add(await GetSaleTypeDocTotal(ORDR, "", "", slpCode));//获取销售订单总金额
                saleList.Add(await GetSaleTypeDocTotal(ODLN, "", "", slpCode));//获取销售交货单总金额
                saleList.Add(await GetSaleTypeDocTotal(OINV, "", "", slpCode));//获取应收发票总金额
                saleList.Add(await GetSaleReceivables("", "", slpCode));//获取销售收款总金额
                saleList.Add(await GetClue("", "", loginUser));//获取全部新增线索            
                saleList.Add(await GetProductList("", "", slpCode));//获取生产订单个数             
                saleList.Add(await GetBillApplication("", "", slpCode));//获取增值税发票总金额
                saleList.Add(await GetSaleTypeDocTotal(ORDN, "", "", slpCode));//获取销售退货单总金额
                saleList.Add(await GetSaleTypeDocTotal(ORIN, "", "", slpCode));//获取应收贷项凭证总金额               
            }
            else
            {
                saleList.Add(await GetCustomer(qt.startTime, qt.endTime, slpCode));//获取时间范围内新增客户
                saleList.Add(await GetSaleTypeDocTotal(OQUT, qt.startTime, qt.endTime, slpCode));//获取时间范围内销售报价单总金额
                saleList.Add(await GetSaleTypeDocTotal(ORDR, qt.startTime, qt.endTime, slpCode));//获取时间范围内销售订单总金额
                saleList.Add(await GetSaleTypeDocTotal(ODLN, qt.startTime, qt.endTime, slpCode));//获取时间范围内销售交货单总金额
                saleList.Add(await GetSaleTypeDocTotal(OINV, qt.startTime, qt.endTime, slpCode));//获取时间范围内应收发票总金额
                saleList.Add(await GetSaleReceivables(qt.startTime, qt.endTime, slpCode));//获取时间范围内销售收款总金额
                saleList.Add(await GetClue(qt.startTime, qt.endTime, loginUser));//获取时间范围内新增线索
                saleList.Add(await GetProductList(qt.startTime, qt.endTime, slpCode));//获取时间范围内生产订单个数
                saleList.Add(await GetBillApplication(qt.startTime, qt.endTime, slpCode));//获取时间范围内增值税发票总金额             
                saleList.Add(await GetSaleTypeDocTotal(ORDN, qt.startTime, qt.endTime, slpCode));//获取时间范围内销售退货单总金额
                saleList.Add(await GetSaleTypeDocTotal(ORIN, qt.startTime, qt.endTime, slpCode));//获取时间范围内应收贷项凭证总金额               
            }

            result.Data = new { saleList = saleList, moduleSize = "1"};
            return result;
        }

        /// <summary>
        /// 获取新增线索
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="loginUser">当前登陆者信息</param>
        /// <returns>返回新增线索概况信息</returns>
        public async Task<QuerySaleBusinessRequest> GetClue(string startTime, string endTime, User loginUser)
        {
            string modelNum;
            QuerySaleBusinessRequest clueBusiness = new QuerySaleBusinessRequest();
            if (startTime == "" || endTime == "")
            {
                var clueList = await UnitWork.Find<OpenAuth.Repository.Domain.Serve.Clue>(r => r.CreateUser == loginUser.Name).CountAsync();
                modelNum = clueList.ToString();
            }
            else
            {
                var clueList = await UnitWork.Find<OpenAuth.Repository.Domain.Serve.Clue>(r => r.CreateUser == loginUser.Name)
                                             .WhereIf(startTime != "", r => r.CreateTime >= Convert.ToDateTime(startTime))
                                             .WhereIf(endTime != "", r => r.CreateTime <= Convert.ToDateTime(endTime))
                                             .CountAsync();

                modelNum = clueList.ToString();
            }

            clueBusiness.ModelName = "新增线索";
            clueBusiness.ModelNum = modelNum + "个";
            clueBusiness.Url = "api/Clue/Clue/GetClueListAsync";
            return clueBusiness;
        }

        /// <summary>
        /// 获取新增客户
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回新增客户概况信息</returns>
        public async Task<QuerySaleBusinessRequest> GetCustomer(string startTime, string endTime, int? slpCode)
        {
            string modelNum;
            QuerySaleBusinessRequest customerBusiness = new QuerySaleBusinessRequest();
            if (startTime == "" || endTime == "")
            {
                var customerList = await UnitWork.Find<OCRD>(r => r.SlpCode == slpCode).Select(r => r.CardCode).CountAsync();
                modelNum = customerList.ToString();
            }
            else
            {
                int count = await _saleBusinessMethodHelp.GetTimeRangCustomer(startTime, endTime, slpCode);
                modelNum = count.ToString();
            }

            customerBusiness.ModelName = "新增客户";
            customerBusiness.ModelNum = modelNum + "个";
            customerBusiness.Url = "api/v1/Client/View";
            return customerBusiness;
        }

        /// <summary>
        /// 获取各销售类型总金额
        /// </summary>
        /// <param name="type">销售类型</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回各销售类型总金额</returns>
        public async Task<QuerySaleBusinessRequest> GetSaleTypeDocTotal(string type, string startTime, string endTime, int? slpCode)
        {
            decimal modelNum = 0;
            string modelName = "";
            string url = "";
            QuerySaleBusinessRequest customerBusiness = new QuerySaleBusinessRequest();
            if (!string.IsNullOrEmpty(type))
            {
                //销售报价单
                if (type == OQUT)
                {
                    modelName = "销售报价单";
                    url = "api/Order/OrderDraft/sales";
                    if (startTime == "" || endTime == "")
                    {
                        var oqutList = await UnitWork.Find<OQUT>(r => r.SlpCode == slpCode).Where(r => r.CANCELED == "N").ToListAsync();
                        modelNum = oqutList.Sum(r => r.DocTotal).ToDecimal();
                    }
                    else
                    {
                        var oqutList = await UnitWork.Find<OQUT>(r => r.SlpCode == slpCode)
                                               .WhereIf(startTime != "", r => r.CreateDate >= Convert.ToDateTime(startTime))
                                               .WhereIf(endTime != "", r => r.CreateDate <= Convert.ToDateTime(endTime))
                                               .Where(r => r.CANCELED == "N")
                                               .ToListAsync();

                        modelNum = oqutList.Sum(r => r.DocTotal).ToDecimal();
                    }
                }

                //销售订单
                if (type == ORDR)
                {
                    modelName = "销售订单";
                    url = "api/Order/SalesOrder/GridDataBind";
                    if (startTime == "" || endTime == "")
                    {
                        var ordrList = await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode).Where(r => r.CANCELED == "N").ToListAsync();
                        modelNum = ordrList.Sum(r => r.DocTotal).ToDecimal();
                    }
                    else
                    {
                        var ordrList = await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode)
                                               .WhereIf(startTime != "", r => r.CreateDate >= Convert.ToDateTime(startTime))
                                               .WhereIf(endTime != "", r => r.CreateDate <= Convert.ToDateTime(endTime))
                                               .Where(r => r.CANCELED == "N")
                                               .ToListAsync();

                        modelNum = ordrList.Sum(r => r.DocTotal).ToDecimal();
                    }
                }

                //销售交货单
                if (type == ODLN)
                {
                    modelName = "销售交货";
                    url = "api/Order/SalesDelivery/GridDataBind";
                    if (startTime == "" || endTime == "")
                    {
                        var odlnList = await UnitWork.Find<ODLN>(r => r.SlpCode == slpCode).Where(r => r.CANCELED == "N").ToListAsync();
                        modelNum = odlnList.Sum(r => r.DocTotal).ToDecimal();
                    }
                    else
                    {
                        var odlnList = await UnitWork.Find<ODLN>(r => r.SlpCode == slpCode)
                                               .WhereIf(startTime != "", r => r.CreateDate >= Convert.ToDateTime(startTime))
                                               .WhereIf(endTime != "", r => r.CreateDate <= Convert.ToDateTime(endTime))
                                               .Where(r => r.CANCELED == "N")
                                               .ToListAsync();

                        modelNum = odlnList.Sum(r => r.DocTotal).ToDecimal();
                    }
                }

                //应收发票
                if (type == OINV)
                {
                    modelName = "应收发票";
                    url = "api/Order/SalesDelivery/SalesInvoiceGridDataBind";
                    if (startTime == "" || endTime == "")
                    {
                        var oinvList = await UnitWork.Find<OINV>(r => r.SlpCode == slpCode).Where(r => r.CANCELED == "N").ToListAsync();
                        modelNum = oinvList.Sum(r => r.DocTotal).ToDecimal();
                    }
                    else
                    {
                        var oinvList = await UnitWork.Find<OINV>(r => r.SlpCode == slpCode)
                                               .WhereIf(startTime != "", r => r.CreateDate >= Convert.ToDateTime(startTime))
                                               .WhereIf(endTime != "", r => r.CreateDate <= Convert.ToDateTime(endTime))
                                               .Where(r => r.CANCELED == "N")
                                               .ToListAsync();

                        modelNum = oinvList.Sum(r => r.DocTotal).ToDecimal();
                    }
                }

                //销售退货单
                if (type == ORDN)
                {
                    modelName = "销售退货";
                    url = "";
                    if (startTime == "" || endTime == "")
                    {
                        var ordnList = await UnitWork.Find<ORDN>(r => r.SlpCode == slpCode).Where(r => r.CANCELED == "N").ToListAsync();
                        modelNum = ordnList.Sum(r => r.DocTotal).ToDecimal();
                    }
                    else
                    {
                        var ordnList = await UnitWork.Find<ORDN>(r => r.SlpCode == slpCode)
                                               .WhereIf(startTime != "", r => r.CreateDate >= Convert.ToDateTime(startTime))
                                               .WhereIf(endTime != "", r => r.CreateDate <= Convert.ToDateTime(endTime))
                                               .Where(r => r.CANCELED == "N")
                                               .ToListAsync();

                        modelNum = ordnList.Sum(r => r.DocTotal).ToDecimal();
                    }
                }

                //应收贷项凭证 
                if (type == ORIN)
                {
                    modelName = "应收贷项凭证";
                    url = "api/Order/SalesDelivery/SalesCreditMemoGridDataBind";
                    if (startTime == "" || endTime == "")
                    {
                        var orinList = await UnitWork.Find<ORIN>(r => r.SlpCode == slpCode).Where(r => r.CANCELED == "N").ToListAsync();
                        modelNum = orinList.Sum(r => r.DocTotal).ToDecimal();
                    }
                    else
                    {
                        var orinList = await UnitWork.Find<ORIN>(r => r.SlpCode == slpCode)
                                               .WhereIf(startTime != "", r => r.CreateDate >= Convert.ToDateTime(startTime))
                                               .WhereIf(endTime != "", r => r.CreateDate <= Convert.ToDateTime(endTime))
                                               .Where(r => r.CANCELED != "N")
                                               .ToListAsync();

                        modelNum = orinList.Sum(r => r.DocTotal).ToDecimal();
                    }
                }
            }

            if (modelName != "")
            {
                string docTotalSum = modelNum == 0 ? "0.00" : _serviceBaseApp.MoneyToCoin(modelNum, 2);//以货币形式输出，整数部分每隔三位数添加逗号
                customerBusiness.ModelName = modelName;
                customerBusiness.ModelNum = docTotalSum;
                customerBusiness.Url = url;
            }
            
            return customerBusiness;
        }

        /// <summary>
        /// 获取增值税发票总金额
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回增值税发票总金额信息</returns>
        public async Task<QuerySaleBusinessRequest> GetBillApplication(string startTime, string endTime, int? slpCode)
        {
            decimal modelNum = 0;
            QuerySaleBusinessRequest customerBusiness = new QuerySaleBusinessRequest();
            var ordrList = await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode).Where(r => r.CANCELED == "N").ToListAsync();
            List<int> docEntryList = ordrList.Select(r => r.DocEntry).ToList();
            if (startTime == "" || endTime == "")
            {
                var billList = await UnitWork.Find<finance_billapplication_master>(r => docEntryList.Contains(Convert.ToInt32(r.DocEntry))).Where(r => r.billStatus != 2).ToListAsync();
                modelNum = billList.Sum(r => r.totalmn).ToDecimal();
            }
            else
            {
                var billList = await UnitWork.Find<finance_billapplication_master>(r => docEntryList.Contains(Convert.ToInt32(r.DocEntry)))
                                             .WhereIf(startTime != "", r => r.ApplicationTime >= Convert.ToDateTime(startTime))
                                             .WhereIf(endTime != "", r => r.ApplicationTime <= Convert.ToDateTime(endTime))
                                             .Where(r => r.billStatus != 2 && r.sbo_id == 1)
                                             .ToListAsync();

                modelNum = billList.Sum(r => r.totalmn).ToDecimal();
            }

            string docTotalSum = modelNum == 0 ? "0.00" : _serviceBaseApp.MoneyToCoin(modelNum, 2);//以货币形式输出，整数部分每隔三位数添加逗号
            customerBusiness.ModelName = "增值税发票";
            customerBusiness.ModelNum = docTotalSum;
            customerBusiness.Url = "";

            return customerBusiness;
        }

        /// <summary>
        /// 获取销售收款总金额
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回销售收款总金额信息</returns>
        public async Task<QuerySaleBusinessRequest> GetSaleReceivables(string startTime, string endTime, int? slpCode)
        {
            decimal modelNum = 0;
            QuerySaleBusinessRequest customerBusiness = new QuerySaleBusinessRequest();
            if (startTime == "" || endTime == "")
            {
                var orctTotalList = from a in await UnitWork.Find<ORCT>(r => r.Canceled == "N").Select(r => new { r.U_XSDD, r.DocTotal, r.Canceled }).ToListAsync()
                                    join b in await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode).Select(r => new { r.DocEntry, r.SlpCode }).ToListAsync() on a.U_XSDD equals b.DocEntry
                                    where b == null ? a.Canceled == "N" : b.SlpCode == slpCode
                                    select new { a.DocTotal };

                modelNum = orctTotalList.Sum(r => r.DocTotal).ToDecimal();
            }
            else
            {
                var orctTotalList = from a in await UnitWork.Find<ORCT>(r => r.Canceled == "N").Select(r => new { r.U_XSDD, r.DocTotal, r.CreateDate, r.Canceled }).ToListAsync()
                                    join b in await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode).Select(r => new { r.DocEntry, r.SlpCode }).ToListAsync() on a.U_XSDD equals b.DocEntry       
                                    where a.CreateDate >= Convert.ToDateTime(startTime) && a.CreateDate <= Convert.ToDateTime(endTime) && (b == null ?  a.Canceled == "N" : b.SlpCode == slpCode)
                                    select new { a.DocTotal };

                modelNum = orctTotalList.Sum(r => r.DocTotal).ToDecimal();
            }

            string docTotalSum = modelNum == 0 ? "0.00" : _serviceBaseApp.MoneyToCoin(modelNum, 2);//以货币形式输出，整数部分每隔三位数添加逗号
            customerBusiness.ModelName = "销售收款";
            customerBusiness.ModelNum = docTotalSum;
            customerBusiness.Url = "";
            return customerBusiness;
        }

        /// <summary>
        /// 获取生产订单个数
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回生产订单信息</returns>
        public async Task<QuerySaleBusinessRequest> GetProductList(string startTime, string endTime, int? slpCode)
        {
            int modelNum = 0;
            QuerySaleBusinessRequest customerBusiness = new QuerySaleBusinessRequest();
            StringBuilder strSql = new StringBuilder();

            if (startTime == "" || endTime == "")
            {
                strSql.Append("SELECT w.CreateDate,w.DocEntry,w.ItemCode,w.txtitemName,w.Type,w.Status,w.OriginAbs,w.CardCode ");
                strSql.AppendFormat(" FROM {0}.product_owor as w ", "nsap_bone");
                strSql.AppendFormat(" LEFT JOIN {0}.sale_ordr AS u ON w.OriginAbs = u.DocEntry  and w.sbo_id = u.sbo_id", "nsap_bone");
                strSql.AppendFormat(" LEFT JOIN {0}.crm_oslp AS s ON u.SlpCode=s.SlpCode  and u.sbo_id = s.sbo_id", "nsap_bone");
                strSql.AppendFormat(" LEFT JOIN {0}.store_owhs AS z ON w.Warehouse = z.whsCode and w.sbo_id = z.sbo_id", "nsap_bone");
                strSql.AppendFormat(" LEFT JOIN {0}.wfa_job a ON a.job_id = w.U_job_id ", "nsap_base");
                strSql.Append(" WHERE W.sbo_id =1 AND w.PlannedQty > w.CmpltQty AND w.Status = 'R' AND ");
                strSql.AppendFormat(" u.SlpCode = {0}", slpCode);
            }
            else
            {
                strSql.Append("SELECT w.CreateDate,w.DocEntry,w.ItemCode,w.txtitemName,w.Type,w.Status,w.OriginAbs,w.CardCode ");
                strSql.AppendFormat(" FROM {0}.product_owor as w ", "nsap_bone");
                strSql.AppendFormat(" LEFT JOIN {0}.sale_ordr AS u ON w.OriginAbs = u.DocEntry  and w.sbo_id = u.sbo_id", "nsap_bone");
                strSql.AppendFormat(" LEFT JOIN {0}.crm_oslp AS s ON u.SlpCode=s.SlpCode  and u.sbo_id = s.sbo_id", "nsap_bone");
                strSql.AppendFormat(" LEFT JOIN {0}.store_owhs AS z ON w.Warehouse = z.whsCode and w.sbo_id = z.sbo_id", "nsap_bone");
                strSql.AppendFormat(" LEFT JOIN {0}.wfa_job a ON a.job_id = w.U_job_id ", "nsap_base");
                strSql.Append(" WHERE W.sbo_id =1 AND w.PlannedQty > w.CmpltQty AND w.Status = 'R' AND ");
                strSql.AppendFormat("  u.SlpCode = {0} and w.CreateDate >= '{1}' and w.CreateDate <= '{2}'", slpCode, Convert.ToDateTime(startTime), Convert.ToDateTime(endTime));
            }
     
            DataTable dTable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
            modelNum = dTable == null ? 0 : dTable.Rows.Count;
            customerBusiness.ModelName = "生产订单";
            customerBusiness.ModelNum = modelNum.ToString() + "个";
            customerBusiness.Url = "";
            return customerBusiness;
        }

        /// <summary>
        /// 获取冻结客户和将要冻结的客户数量
        /// </summary>
        /// <returns>返回客户数量</returns>
        public async Task<TableData> GetFreezeCustomer()
        {
            var result = new TableData();
            int willFreeze = (await UnitWork.Find<PayWillFreezeCustomer>(null).ToListAsync()).Count();
            int freeze = (await UnitWork.Find<PayFreezeCustomer>(null).ToListAsync()).Count();
            result.Data = new { WillFreezeCount = willFreeze, FreezeCount = freeze };
            result.Message = "获取成功";
            return result;
        }
        #endregion

        #region 销售状态提醒
        /// <summary>
        /// 销售状态提醒
        /// </summary>
        /// <returns>返回销售状态提醒数据集合</returns>
        public async Task<TableData> GetSaleStatus()
        {
            User loginUser = _saleBusinessMethodHelp.GetLoginUser(out int? slpCode);
            var result = new TableData();
            if (slpCode == 0)
                return result;

            List<QuerySaleBusinessRequest> saleStatusList = new List<QuerySaleBusinessRequest>();
            saleStatusList.Add(await GetSalesQuotationOrderAudit(XSBJD, loginUser));//获取审批中销售报价单
            saleStatusList.Add(await GetSalesQuotationOrderAudit(XSDD, loginUser));//获取审批中销售订单
            saleStatusList.Add(await GetNoComSaleOrder(slpCode));//获取未完工销售订单
            saleStatusList.Add(await GetBillSaleOrder(loginUser, slpCode));//获取未开增值税发票销售订单
            result.Data = new { saleList = saleStatusList, moduleSize = "2" };
            return result;
        }

        /// <summary>
        /// 获取审批中的销售报价单或销售订单
        /// </summary>
        /// <param name="saleType">销售类型</param>
        /// <param name="loginUser">当前登录者信息</param>
        /// <returns>返回审批中报价单和订单信息</returns>
        public async Task<QuerySaleBusinessRequest> GetSalesQuotationOrderAudit(string saleType, User loginUser)
        {
            string modelName = "";
            string url = "";
            QuerySaleBusinessRequest customerBusiness = new QuerySaleBusinessRequest();
            IEnumerable<QueryWfaJob> saleQutationList = new List<QueryWfaJob>();
            if (saleType == XSBJD)
            {
                var wfa_jobList = await UnitWork.Find<wfa_job>(r => r.user_id == loginUser.User_Id)
                                         .Where(r => r.job_state > -1 && r.job_nm == saleType && r.job_state != -1 && r.job_state == 1)
                                         .Select(r => new QueryWfaJob { job_type_id = r.job_type_id, sbo_id = Convert.ToInt32(r.sbo_id), step_id = r.step_id, user_id = r.user_id, job_id = r.job_id })
                                         .ToListAsync();

                var wfa_typeList = await UnitWork.Find<wfa_type>(null).ToListAsync();
                var base_userList = await UnitWork.Find<base_user>(r => r.user_id == loginUser.User_Id).ToListAsync();
                var wfa_stepList = await UnitWork.Find<wfa_step>(null).ToListAsync();
                var sbo_info = await UnitWork.Find<sbo_info>(null).ToListAsync();
                    saleQutationList = from a in wfa_jobList
                                       join b in wfa_typeList on a.job_type_id equals b.job_type_id into ab
                                       from b in ab.DefaultIfEmpty()
                                       join c in base_userList on a.user_id equals Convert.ToInt32(c.user_id) into ac
                                       from c in ac.DefaultIfEmpty()
                                       join d in wfa_stepList on a.step_id equals d.step_id into ad
                                       from d in ad.DefaultIfEmpty()
                                       join g in sbo_info on a.sbo_id equals g.sbo_id into ag
                                       from g in ag.DefaultIfEmpty()
                                       where b.job_type_nm == saleType && c.user_id == loginUser.User_Id
                                       group a by new { a.job_id } into h
                                       select new QueryWfaJob{ job_id = 1 };

                modelName = "审批中报价单";
                url = "api/OrderWorkbench/OrderWorkbench/GetSubmtToMe";
            }
            else
            {
                var wfa_jobList = await UnitWork.Find<wfa_job>(r => r.user_id == loginUser.User_Id)
                                           .Where(r => r.job_state > -1 && r.job_nm == XSDD && r.job_state != -1 && r.job_state == 1)
                                           .Select(r => new QueryWfaJob { job_type_id = r.job_type_id, sbo_id = Convert.ToInt32(r.sbo_id), step_id = r.step_id, user_id = r.user_id, job_id = r.job_id })
                                           .ToListAsync();

                var wfa_typeList = await UnitWork.Find<wfa_type>(null).ToListAsync();
                var base_userList = await UnitWork.Find<base_user>(r => r.user_id == loginUser.User_Id).ToListAsync();
                var wfa_stepList = await UnitWork.Find<wfa_step>(null).ToListAsync();
                var sbo_info = await UnitWork.Find<sbo_info>(null).ToListAsync();
                saleQutationList = from a in wfa_jobList
                                   join b in wfa_typeList on a.job_type_id equals b.job_type_id into ab
                                   from b in ab.DefaultIfEmpty()
                                   join c in base_userList on a.user_id equals Convert.ToInt32(c.user_id) into ac
                                   from c in ac.DefaultIfEmpty()
                                   join d in wfa_stepList on a.step_id equals d.step_id into ad
                                   from d in ad.DefaultIfEmpty()
                                   join g in sbo_info on a.sbo_id equals g.sbo_id into ag
                                   from g in ag.DefaultIfEmpty()
                                   where b.job_type_nm == saleType && c.user_id == loginUser.User_Id
                                   group a by new { a.job_id } into h
                                   select new QueryWfaJob { job_id = 1 };

                modelName = "审批中销售订单";
                url = "api/OrderWorkbench/OrderWorkbench/GetSubmtToMe";
            }
            
            customerBusiness.ModelName = modelName;
            customerBusiness.ModelNum = saleQutationList.Count().ToString();
            customerBusiness.Url = url;
            return customerBusiness;
        }

        /// <summary>
        /// 获取需申请工单订单（待开发）
        /// </summary>
        /// <returns>返回需申请工单订单信息</returns>
        public async Task<QuerySaleBusinessRequest> GetNotEnoughGoosSaleOrder()
        {
            string modelNum = "";
            QuerySaleBusinessRequest customerBusiness = new QuerySaleBusinessRequest();
            customerBusiness.ModelName = "需申请工单订单";
            customerBusiness.ModelNum = "待开发";
            customerBusiness.Url = "";
            return customerBusiness;
        }

        /// <summary>
        /// 获取未完工销售订单
        /// </summary>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回未完工销售订单信息</returns>
        public async Task<QuerySaleBusinessRequest> GetNoComSaleOrder(int? slpCode)
        {
            string modelNum = "";
            QuerySaleBusinessRequest customerBusiness = new QuerySaleBusinessRequest();
            var oworList = await UnitWork.Find<OWOR>(null).Where(r => (r.Status == "R" || r.Status == "P") && (r.PlannedQty > r.CmpltQty)).ToListAsync();
            var ordrList = await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode).Where(r => r.CANCELED == "N").ToListAsync();
            if (oworList != null && ordrList != null)
            {
                var queryNoComSaleOrderList = from a in oworList
                                              join b in ordrList on a.OriginAbs equals b.DocEntry into ab
                                              from b in ab.DefaultIfEmpty()
                                              where b == null ? (a.Status == "R" || a.Status == "P") : (b.SlpCode == slpCode && b.CANCELED == "N")
                                              select new { SlpCode = b == null ? 0 : b.SlpCode, a.DocEntry, a.OriginAbs };

                modelNum = queryNoComSaleOrderList.ToList().Where(r => r.SlpCode != null).Count().ToString();
            }

            customerBusiness.ModelName = "未完工销售订单";
            customerBusiness.ModelNum = modelNum;
            customerBusiness.Url = "";
            return customerBusiness;
        }

        /// <summary>
        /// 获取可交货销售订单（待开发）
        /// </summary>
        /// <returns>返回可交货销售订单信息</returns>
        public async Task<QuerySaleBusinessRequest> GetEnoughGoosSaleOrder()
        {
            string modelNum = "";
            QuerySaleBusinessRequest customerBusiness = new QuerySaleBusinessRequest();
            customerBusiness.ModelName = "可交货销售订单";
            customerBusiness.ModelNum = "待开发";
            customerBusiness.Url = "";
            return customerBusiness;
        }

        /// <summary>
        /// 获取未开增值税发票销售订单
        /// </summary>
        /// <param name="loginUser">当前登录者信息</param>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回未开增值税发票销售订单信息</returns>
        public async Task<QuerySaleBusinessRequest> GetBillSaleOrder(User loginUser, int? slpCode)
        {
            string modelNum = "";
            QuerySaleBusinessRequest customerBusiness = new QuerySaleBusinessRequest();
            var ordrList = await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode).Where(r => r.CANCELED == "N").ToListAsync();
            var billList = await UnitWork.Find<finance_billapplication_master>(r => r.Applicantor == loginUser.User_Id).ToListAsync();
            if (ordrList != null && billList != null)
            {
                var saleOrderBillList = from a in ordrList
                                        join b in billList on a.DocEntry equals b.DocEntry
                                        where b.Applicantor == loginUser.User_Id
                                        select new { a.DocEntry, a.DocTotal, b.totalmn, b.billType };

                modelNum =((ordrList.Count() - saleOrderBillList.Count()) +  saleOrderBillList.Where(r => r.DocTotal > r.totalmn).Count()).ToString();
            }
           
            customerBusiness.ModelName = "未开增值税发票订单";
            customerBusiness.ModelNum = modelNum;
            customerBusiness.Url = "";
            return customerBusiness;
        }

        /// <summary>
        /// 获取待收货前款订单（待开发）
        /// </summary>
        /// <returns>返回待收货前款订单信息</returns>
        public async Task<QuerySaleBusinessRequest> GetWaitPaySaleOrder()
        {
            string modelNum = "";
            QuerySaleBusinessRequest customerBusiness = new QuerySaleBusinessRequest();
            customerBusiness.ModelName = "待收货前款订单";
            customerBusiness.ModelNum = "待开发";
            customerBusiness.Url = "";
            return customerBusiness;
        }

        /// <summary>
        /// 获取待收货到款订单（待开发）
        /// </summary>
        /// <returns>返回待收货到款订单信息</returns>
        public async Task<QuerySaleBusinessRequest> GetPaySaleOrder()
        {
            string modelNum = "";
            QuerySaleBusinessRequest customerBusiness = new QuerySaleBusinessRequest();
            customerBusiness.ModelName = "待收货到款订单";
            customerBusiness.ModelNum = "待开发";
            customerBusiness.Url = "";
            return customerBusiness;
        }

        /// <summary>
        /// 获取待收验收款订单（待开发）
        /// </summary>
        /// <returns>返回待收验收款订单信息</returns>
        public async Task<QuerySaleBusinessRequest> GetAcceptancePaySaleOrder()
        {
            string modelNum = "";
            QuerySaleBusinessRequest customerBusiness = new QuerySaleBusinessRequest();
            customerBusiness.ModelName = "待收验收款订单";
            customerBusiness.ModelNum = "待开发";
            customerBusiness.Url = "";
            return customerBusiness;
        }

        /// <summary>
        /// 获取待收尾款订单（待开发）
        /// </summary>
        /// <returns>返回待收尾款订单信息</returns>
        public async Task<QuerySaleBusinessRequest> GetBalancePaySaleOrder()
        {
            string modelNum = "";
            QuerySaleBusinessRequest customerBusiness = new QuerySaleBusinessRequest();
            customerBusiness.ModelName = "待收尾款订单";
            customerBusiness.ModelNum = "待开发";
            customerBusiness.Url = "";
            return customerBusiness;
        }
        #endregion

        #region 库存查询
        /// <summary>
        /// 库存查询
        /// </summary>
        /// <param name="query">库存查询实体</param>
        /// <returns>返回库存信息</returns>
        public async Task<TableData> GetWareHouse(QueryWareHouse query)
        {
            var result = new TableData();
            string sortString = string.Empty;
            string filterString = string.Empty;
            int rowCounts = 0;
            StringBuilder tableName = new StringBuilder();
            StringBuilder filedName = new StringBuilder();
            if (!string.IsNullOrEmpty(query.SortName) && !string.IsNullOrEmpty(query.SortOrder))
            {
                sortString = string.Format("{0} {1}", query.SortName.Replace("ItemCode", "m.ItemCode"), query.SortOrder.ToUpper());
            }

            if (!string.IsNullOrEmpty(query.ItemCode))
            {
                filterString += string.Format("m.ItemCode LIKE '%{0}%' AND ", query.ItemCode.FilterWildCard());
            }

            if (!string.IsNullOrEmpty(query.ItemName))
            {
                filterString += string.Format("m.ItemName LIKE '%{0}%' AND ", query.ItemName.FilterWildCard());
            }

            if (!string.IsNullOrEmpty(query.WhsCode))
            {
                filterString += string.Format("w.WhsCode = '{0}' AND ", query.WhsCode.FilterWildCard());
            }

            filterString += string.Format(" m.sbo_id={0} ", 1);
            filedName.Append("m.ItemCode,m.ItemName,IFNULL(s.WhsName,'') AS WhsName,IFNULL(c.high_price,0) AS high_price,IFNULL(c.low_price,0) AS low_price,w.OnHand,m.OnHand AS SumOnHand,m.IsCommited,m.OnOrder,");
            filedName.Append("(m.OnHand-m.IsCommited+m.OnOrder) AS Available,w.WhsCode,IFNULL(U_TDS,'0') AS U_TDS,IFNULL(U_DL,0) AS U_DL,");
            filedName.Append("IFNULL(U_DY,0) AS U_DY,m.U_JGF,m.LastPurPrc,IFNULL(c.item_cfg_id,0) item_cfg_id,IFNULL(c.pic_path,m.PicturName) pic_path,IFNULL(w.AvgPrice,0),");
            filedName.Append("((CASE m.QryGroup1 WHEN 'N' then 0 else 0.5 END)");
            filedName.Append("+(CASE m.QryGroup2 WHEN 'N' then 0 else 3 END)");
            filedName.Append("+(CASE m.QryGroup3 WHEN 'N' then 0 else 2 END)) AS QryGroup,c.item_desp,IFNULL(m.U_US,0) U_US,IFNULL(m.U_FS,0) U_FS,m.QryGroup3,m.SVolume,m.SWeight1,");
            filedName.Append("(CASE m.QryGroup1 WHEN 'N' THEN 0 ELSE '0.5' END) AS QryGroup1,");
            filedName.Append("(CASE m.QryGroup2 WHEN 'N' THEN 0 ELSE '3' END) AS QryGroup2,");
            filedName.Append("(CASE m.QryGroup3 WHEN 'N' THEN 0 ELSE '2' END) AS _QryGroup3,m.U_JGF1,IFNULL(m.U_YFCB,'0') U_YFCB,m.MinLevel,m.PurPackUn,c.item_counts,m.buyunitmsr");
            tableName.AppendFormat(" {0}.store_oitm m", "nsap_bone");
            tableName.AppendFormat(" LEFT JOIN {0}.store_oitw w ON m.ItemCode = w.ItemCode AND m.sbo_id=w.sbo_id ", "nsap_bone");
            tableName.AppendFormat(" LEFT JOIN {0}.base_item_cfg c ON m.ItemCode = c.ItemCode AND type_id={1} ", "nsap_bone", "0");
            tableName.AppendFormat(" LEFT JOIN {0}.store_owhs s ON w.WhsCode = s.WhsCode ", "nsap_bone");
            DataTable dt = _serviceSaleOrderApp.SelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), query.limit,query.page, sortString, filterString, out rowCounts);
            result.Data = dt;
            result.Count = rowCounts;
            return result;
        }
        #endregion

        #region 应收款情况
        /// <summary>
        /// 获取应收款情况
        /// </summary>
        /// <param name="currency">币种</param>
        /// <returns>返回应收款情况信息</returns>
        public async Task<TableData> GetDeliveryMsg(string currency)
        {
            User loginUser = _saleBusinessMethodHelp.GetLoginUser(out int? slpCode);
            var result = new TableData();
            if (slpCode == 0)
                return result;

            QueryOINV query = new QueryOINV();
            query.DeliveryNum = await GetDeliveryMoney(currency, slpCode);//获取应收款金额
            query.CompareLaterYear = await GetDeliveryCompareLastYearRatio(slpCode);//获取应收款余额占去年总回款比例
            result.Data = query;
            return result;
        }

        /// <summary>
        /// 获取币种信息
        /// </summary>
        /// <returns>返回币种信息</returns>
        public async Task<TableData> GetCoinInfo()
        {
            var result = new TableData();
            List<QueryOCRN> coinList = new List<QueryOCRN>();
            coinList = await UnitWork.Find<crm_ocrn>(null).Select(r => new QueryOCRN(r.CurrCode, r.CurrName)).ToListAsync();
            coinList.Add(new QueryOCRN("ALL", "全部"));
            result.Data = coinList.OrderBy(r => r.CurrCode == "ALL" || r.CurrCode == "RMB" || r.CurrCode == "USD" || r.CurrCode == "AUD").Reverse();
            return result;
        }

        /// <summary>
        /// 获取截至今日应收余额
        /// </summary>
        /// <param name="currency">币种</param>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回应收余额</returns>
        public async Task<string> GetDeliveryMoney(string currency, int? slpCode)
        {
            string deliveryNum = "";
            if (currency == "ALL")
            {
                decimal sumAmount = await GetAllMoney(slpCode, "");
                deliveryNum = sumAmount == 0 ? "0.00" : _serviceBaseApp.MoneyToCoin(sumAmount, 2);//以货币形式输出，整数部分每隔三位数添加逗号                
            }
            else if (currency == "RMB")
            {
                decimal sumAmount = await GetRMBMoney(slpCode);
                deliveryNum = sumAmount == 0 ? "0.00" : _serviceBaseApp.MoneyToCoin(sumAmount, 2);//以货币形式输出，整数部分每隔三位数添加逗号                
            }
            else
            {
                decimal sumAmount = await GetForeignCurrencyMoney(slpCode, currency);
                deliveryNum = sumAmount == 0 ? "0.00" :  _serviceBaseApp.MoneyToCoin(sumAmount, 2);//以货币形式输出，整数部分每隔三位数添加逗号                
            }

            return deliveryNum;
        }

        /// <summary>
        /// 获取个人部门排名
        /// </summary>
        /// <param name="currency">币种</param>
        /// <param name="loginUser">当前登陆者信息</param>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回个人部门排名信息</returns>
        public async Task<string> GetDeliveryDepartRank(string currency, User loginUser, int? slpCode)
        {
            string departRank = "0/0";
            var userDepart = await UnitWork.Find<base_user_detail>(r => r.user_id == loginUser.User_Id).FirstOrDefaultAsync();
            string depAlias = (await UnitWork.Find<base_dep>(r => r.dep_id == userDepart.dep_id).FirstOrDefaultAsync()).dep_alias;
            var departRankList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_DepartRank")).Select(u => u.DtValue).ToListAsync();
            if (departRankList.Contains(depAlias))
            {
                var userList = await UnitWork.Find<base_user_detail>(r => r.dep_id == userDepart.dep_id).Select(r => r.user_id).ToListAsync();
                var slpCodeList = (await UnitWork.Find<sbo_user>(r => userList.Contains(r.user_id)).Where(r => r.sbo_id == 1).Select(r => new QuerySlpCode { SlpCode = Convert.ToInt32(r.sale_id) }).ToListAsync()).GroupBy(r => new { r.SlpCode }).Select(r => new QuerySlpCode { SlpCode = r.Key.SlpCode }).ToList();      
                List<QueryRank> slpList = new List<QueryRank>();
                if (currency == "ALL")
                {
                    foreach (QuerySlpCode item in slpCodeList)
                    {
                        decimal sumAmount = await GetAllMoney(item.SlpCode, "");
                        slpList.Add(new QueryRank(item.SlpCode, Math.Round(sumAmount, 2)));
                    }
                }
                else if (currency == "RMB")
                {
                    foreach (QuerySlpCode item in slpCodeList)
                    {
                        decimal sumAmount = await GetRMBMoney(item.SlpCode);
                        slpList.Add(new QueryRank(item.SlpCode, Math.Round(sumAmount, 2)));
                    }
                }
                else
                {
                    foreach (QuerySlpCode item in slpCodeList)
                    {
                        decimal sumAmount = await GetForeignCurrencyMoney(item.SlpCode, currency);
                        slpList.Add(new QueryRank(item.SlpCode, Math.Round(sumAmount, 2)));
                    }
                }

                slpList = slpList.OrderByDescending(r => r.TotalAmount).ToList();
                int indexOf = slpList.IndexOf(slpList.Where(r => r.SlpCode == slpCode).FirstOrDefault()) + 1;
                int totalSlp = slpCodeList.Count();
                departRank = indexOf + "/" + totalSlp;
            }

            return departRank;
        }

        /// <summary>
        /// 获取个人公司排名
        /// </summary>
        /// <param name="currency">币种</param>
        /// <param name="loginUser">当前登陆者信息</param>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回个人公司排名</returns>
        public async Task<string> GetDelilveryCompanyRank(string currency, User loginUser, int? slpCode)
        {
            string companyRank = "0/0";
            var userDepart = await UnitWork.Find<base_user_detail>(r => r.user_id == loginUser.User_Id).FirstOrDefaultAsync();
            string depAlias = (await UnitWork.Find<base_dep>(r => r.dep_id == userDepart.dep_id).FirstOrDefaultAsync()).dep_alias;
            var departRankList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_DepartRank")).Select(u => u.DtValue).ToListAsync();
            if (departRankList.Contains(depAlias))
            {
                var depIdList = await UnitWork.Find<base_dep>(r => departRankList.Contains(r.dep_alias)).Select(r => r.dep_id).ToListAsync();
                var userList = await UnitWork.Find<base_user_detail>(r => depIdList.Contains(r.dep_id)).Select(r => r.user_id).ToListAsync();
                var slpCodeList = (await UnitWork.Find<sbo_user>(r => userList.Contains(r.user_id)).Where(r => r.sbo_id == 1).Select(r => new QuerySlpCode { SlpCode = Convert.ToInt32(r.sale_id) }).ToListAsync()).GroupBy(r => new { r.SlpCode }).Select(r => new QuerySlpCode { SlpCode = r.Key.SlpCode }).ToList();
                List<QueryRank> slpList = new List<QueryRank>();
                if (currency == "ALL")
                {
                    foreach (QuerySlpCode item in slpCodeList)
                    {
                        decimal sumAmount = await GetAllMoney(item.SlpCode, "");
                        slpList.Add(new QueryRank(item.SlpCode, Math.Round(sumAmount, 2)));
                    }
                }
                else if (currency == "RMB")
                {
                    foreach (QuerySlpCode item in slpCodeList)
                    {
                        decimal sumAmount = await GetRMBMoney(item.SlpCode);
                        slpList.Add(new QueryRank(item.SlpCode, Math.Round(sumAmount, 2)));
                    }
                }
                else
                {
                    foreach (QuerySlpCode item in slpCodeList)
                    {
                        decimal sumAmount = await GetForeignCurrencyMoney(item.SlpCode, currency);
                        slpList.Add(new QueryRank(item.SlpCode, Math.Round(sumAmount, 2)));
                    }
                }

                slpList = slpList.OrderByDescending(r => r.TotalAmount).ToList();
                int indexOf = slpList.IndexOf(slpList.Where(r => r.SlpCode == slpCode).FirstOrDefault()) + 1;
                int totalSlp = slpCodeList.Count();
                companyRank = indexOf + "/" + totalSlp;
            }

            return companyRank;
        }

        /// <summary>
        /// 获取全部应收款金额
        /// </summary>
        /// <param name="slpCode">业务员编码</param>
        /// <param name="timeType">时间类型</param>
        /// <returns>返回全部应收款金额</returns>
        public async Task<decimal> GetAllMoney(int? slpCode, string timeType)
        {
            decimal oinvSumAmount = 0;
            decimal orctSumAmount = 0;
            decimal orinSumAmount = 0;
            decimal sumAmount = 0;
            if (timeType == "NowYear")
            {
                //获取未清应收发票
                var oinvList = await UnitWork.Find<OINV>(r => r.SlpCode == slpCode && r.CANCELED == "N" && r.CreateDate >= NowYearFirstDay && r.CreateDate <= NowYearLastDay).Select(r => new { r.DocTotal, r.PaidToDate }).ToListAsync();
                if (oinvList != null && oinvList.Count() != 0)
                {
                    oinvSumAmount = oinvList.Sum(r => (r.DocTotal - r.PaidToDate)).ToDecimal();
                }

                //获取未清销售收款
                var orctList = from a in await UnitWork.Find<ORCT>(r => r.Canceled == "N" && r.CreateDate >= NowYearFirstDay && r.CreateDate <= NowYearLastDay).Select(r => new { r.U_XSDD, r.OpenBal, r.Canceled }).ToListAsync()
                               join b in await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode).Select(r => new { r.DocEntry, r.SlpCode }).ToListAsync() on a.U_XSDD equals b.DocEntry
                               where b == null ? a.Canceled == "N" : b.SlpCode == slpCode
                               select new { a.OpenBal };

                if (orctList != null && orctList.Count() != 0)
                {
                    orctSumAmount = orctList.Sum(r => r.OpenBal).ToDecimal();
                }

                //获取未清应收贷项
                var orinList = await UnitWork.Find<ORIN>(r => r.SlpCode == slpCode && r.CANCELED == "N" && r.CreateDate >= NowYearFirstDay && r.CreateDate <= NowYearLastDay).Select(r => new { r.DocTotal, r.PaidToDate }).ToListAsync();
                if (orinList != null && orinList.Count() != 0)
                {
                    orinSumAmount = orinList.Sum(r => (r.DocTotal - r.PaidToDate)).ToDecimal();
                }
            }
            else
            {
                //获取未清应收发票
                var oinvList = await UnitWork.Find<OINV>(r => r.SlpCode == slpCode && r.CANCELED == "N").Select(r => new { r.DocTotal, r.PaidToDate }).ToListAsync();
                if (oinvList != null && oinvList.Count() != 0)
                {
                    oinvSumAmount = oinvList.Sum(r => (r.DocTotal - r.PaidToDate)).ToDecimal();
                }

                //获取未清销售收款
                var orctList = from a in await UnitWork.Find<ORCT>(r => r.Canceled == "N").Select(r => new { r.U_XSDD, r.OpenBal, r.Canceled }).ToListAsync()
                               join b in await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode).Select(r => new { r.DocEntry, r.SlpCode }).ToListAsync() on a.U_XSDD equals b.DocEntry
                               where b == null ? a.Canceled == "N" : b.SlpCode == slpCode
                               select new { a.OpenBal };

                if (orctList != null && orctList.Count() != 0)
                {
                    orctSumAmount = orctList.Sum(r => r.OpenBal).ToDecimal();
                }

                //获取未清应收贷项
                var orinList = await UnitWork.Find<ORIN>(r => r.SlpCode == slpCode && r.CANCELED == "N").Select(r => new { r.DocTotal, r.PaidToDate }).ToListAsync();
                if (orinList != null && orinList.Count() != 0)
                {
                    orinSumAmount = orinList.Sum(r => (r.DocTotal - r.PaidToDate)).ToDecimal();
                }

                //业务员应收款余额
                sumAmount = oinvSumAmount - orctSumAmount - orinSumAmount;
            }
          
            return sumAmount;
        }

        /// <summary>
        /// 获取币种为RMB应收款金额
        /// </summary>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回币种为RMB应收款金额</returns>
        public async Task<decimal> GetRMBMoney(int? slpCode)
        {
            decimal oinvSumAmount = 0;
            decimal orctSumAmount = 0;
            decimal orinSumAmount = 0;

            //获取未清应收发票（RMB）
            var oinvList = await UnitWork.Find<OINV>(r => r.SlpCode == slpCode && r.CANCELED == "N" && r.DocCur == "RMB").Select(r => new { r.DocTotal, r.PaidToDate }).ToListAsync();
            if (oinvList != null && oinvList.Count() != 0)
            {
                oinvSumAmount = oinvList.Sum(r => (r.DocTotal - r.PaidToDate)).ToDecimal();
            }

            //获取未清销售收款（RMB）
            var orctList = from a in await UnitWork.Find<ORCT>(r => r.Canceled == "N" && r.DocCurr == "RMB").Select(r => new { r.U_XSDD, r.OpenBal, r.Canceled }).ToListAsync()
                           join b in await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode).Select(r => new { r.DocEntry, r.SlpCode }).ToListAsync() on a.U_XSDD equals b.DocEntry
                           where b == null ? a.Canceled == "N" : b.SlpCode == slpCode
                           select new { a.OpenBal };

            if (orctList != null && orctList.Count() != 0)
            {
                orctSumAmount = orctList.Sum(r => r.OpenBal).ToDecimal();
            }

            //获取未清应收贷项（RMB）
            var orinList = await UnitWork.Find<ORIN>(r => r.SlpCode == slpCode && r.CANCELED == "N" && r.DocCur == "RMB").Select(r => new { r.DocTotal, r.PaidToDate }).ToListAsync();
            if (orinList != null && orinList.Count() != 0)
            {
                orinSumAmount = orinList.Sum(r => (r.DocTotal - r.PaidToDate)).ToDecimal();
            }

            //业务员应收款余额（RMB）
            decimal sumAmount = oinvSumAmount - orctSumAmount - orinSumAmount;
            return sumAmount;
        }
        
        /// <summary>
        /// 获取币种为外币的应收款金额
        /// </summary>
        /// <param name="slpCode">业务员编码</param>
        /// <param name="currency">币种</param>
        /// <returns>返回币种为外币的应收款金额</returns>
        public async Task<decimal> GetForeignCurrencyMoney(int? slpCode, string currency)
        {
            decimal oinvSumAmount = 0;
            decimal orctSumAmount = 0;
            decimal orinSumAmount = 0;

            //获取未清应收发票（外币）
            var oinvList = await UnitWork.Find<OINV>(r => r.SlpCode == slpCode && r.CANCELED == "N" && r.DocCur == currency).Select(r => new { r.DocTotal, r.PaidToDate, r.DocRate }).ToListAsync();
            if (oinvList != null && oinvList.Count() != 0)
            {
                oinvSumAmount = oinvList.Sum(r => ((r.DocTotal - r.PaidToDate) / r.DocRate)).ToDecimal();
            }

            //获取未清销售收款（外币）
            var orctList = from a in await UnitWork.Find<ORCT>(r => r.Canceled == "N" && r.DocCurr == currency).Select(r => new { r.U_XSDD, r.OpenBalFc, r.DocRate, r.Canceled }).ToListAsync()
                           join b in await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode).Select(r => new { r.DocEntry, r.SlpCode }).ToListAsync() on a.U_XSDD equals b.DocEntry
                           where b == null ? a.Canceled == "N" : b.SlpCode == slpCode
                           select new { a.OpenBalFc };

            if (orctList != null && orctList.Count() != 0)
            {
                orctSumAmount = orctList.Sum(r => r.OpenBalFc).ToDecimal();
            }

            //获取未清应收贷项（外币）
            var orinList = await UnitWork.Find<ORIN>(r => r.SlpCode == slpCode && r.CANCELED == "N" && r.DocCur == currency).Select(r => new { r.DocTotal, r.PaidToDate, r.DocRate }).ToListAsync();
            if (orinList != null && orinList.Count() != 0)
            {
                orinSumAmount = orinList.Sum(r => (r.DocTotal - r.PaidToDate) / r.DocRate).ToDecimal();
            }

            //业务员应收款余额（外币）
            decimal sumAmount = oinvSumAmount - orctSumAmount - orinSumAmount;
            return sumAmount;
        }

        /// <summary>
        /// 获取应收款余额占去年总回款比例
        /// </summary>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回应收款余额占去年总回款比例</returns>
        public async Task<string> GetDeliveryCompareLastYearRatio(int? slpCode)
        {
            string deliveryNum = "0%";
            var orctTotalList = from a in await UnitWork.Find<ORCT>(r => r.Canceled == "N").Select(r => new { r.U_XSDD, r.DocTotal, r.OpenBal, r.CreateDate, r.Canceled }).ToListAsync()
                                join b in await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode).Select(r => new { r.DocEntry, r.SlpCode }).ToListAsync() on a.U_XSDD equals b.DocEntry
                                into ab
                                from b in ab.DefaultIfEmpty()
                                where b == null ? a.Canceled == "N" : b.SlpCode == slpCode
                                select new { a.DocTotal, a.OpenBal, a.CreateDate };

            decimal lastYearAmount = orctTotalList.Where(r => r.CreateDate >= LastYearFirstDay && r.CreateDate <= LastYearLastDay).Sum(r => r.DocTotal).ToDecimal();
            decimal nowYearAmount = await GetAllMoney(slpCode, "NowYear");
            if (lastYearAmount != 0)
            {
                deliveryNum = Math.Round((nowYearAmount / lastYearAmount), 2).ToString();
            }
            
            return deliveryNum;
        }

        /// <summary>
        /// 获取百分比个人部门排名
        /// </summary>
        /// <param name="loginUser">当前登录者信息</param>
        /// <param name="slpCode">业务员编码</param>
        /// <param name="compareLastYearRatio">当前业务员应收余额占去年总回款比例</param>
        /// <returns>返回个人部门排名信息</returns>
        public async Task<string> GetPericentageDepartRank(User loginUser, int? slpCode)
        {
            string departRank = "0/0";
            var userDepart = await UnitWork.Find<base_user_detail>(r => r.user_id == loginUser.User_Id).FirstOrDefaultAsync();
            string depAlias = (await UnitWork.Find<base_dep>(r => r.dep_id == userDepart.dep_id).FirstOrDefaultAsync()).dep_alias;
            var departRankList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_DepartRank")).Select(u => u.DtValue).ToListAsync();
            if (departRankList.Contains(depAlias))
            {
                var userList = await UnitWork.Find<base_user_detail>(r => r.dep_id == userDepart.dep_id).Select(r => r.user_id).ToListAsync();
                var slpCodeList = (await UnitWork.Find<sbo_user>(r => userList.Contains(r.user_id)).Where(r => r.sbo_id == 1).Select(r => new QuerySlpCode { SlpCode = Convert.ToInt32(r.sale_id) }).ToListAsync()).GroupBy(r => new { r.SlpCode }).Select(r => new QuerySlpCode { SlpCode = r.Key.SlpCode }).ToList();
                List<QueryRank> slpList = new List<QueryRank>();
                foreach (QuerySlpCode item in slpCodeList)
                {
                    decimal sumAmount = Math.Round(Convert.ToDecimal((await GetDeliveryCompareLastYearRatio(item.SlpCode)).Split('%')[0]), 2); 
                    slpList.Add(new QueryRank(item.SlpCode, Math.Round(sumAmount, 2)));
                }

                slpList = slpList.OrderByDescending(r => r.TotalAmount).ToList();
                int indexOf = slpList.IndexOf(slpList.Where(r => r.SlpCode == slpCode).FirstOrDefault()) + 1;
                int totalSlp = slpCodeList.Count();
                departRank = indexOf + "/" + totalSlp;
            }

            return departRank;
        }

        /// <summary>
        /// 获取百分比个人公司排名
        /// </summary>
        /// <param name="loginUser">当前登录者信息</param>
        /// <param name="slpCode">业务员编码</param>
        /// <param name="compareLastYearRatio">当前业务员应收余额占去年总回款比例</param>
        /// <returns>返回个人公司排名信息</returns>
        public async Task<string> GetPericentageCompanyRank(User loginUser, int? slpCode)
        {
            string companyRank = "0/0";
            var userDepart = await UnitWork.Find<base_user_detail>(r => r.user_id == loginUser.User_Id).FirstOrDefaultAsync();
            string depAlias = (await UnitWork.Find<base_dep>(r => r.dep_id == userDepart.dep_id).FirstOrDefaultAsync()).dep_alias;
            var departRankList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_DepartRank")).Select(u => u.DtValue).ToListAsync();
            if (departRankList.Contains(depAlias))
            {
                var depIdList = await UnitWork.Find<base_dep>(r => departRankList.Contains(r.dep_alias)).Select(r => r.dep_id).ToListAsync();
                var userList = await UnitWork.Find<base_user_detail>(r => depIdList.Contains(r.dep_id)).Select(r => r.user_id).ToListAsync();
                var slpCodeList = (await UnitWork.Find<sbo_user>(r => userList.Contains(r.user_id)).Where(r => r.sbo_id == 1).Select(r => new QuerySlpCode { SlpCode = Convert.ToInt32(r.sale_id) }).ToListAsync()).GroupBy(r => new { r.SlpCode }).Select(r => new QuerySlpCode { SlpCode = r.Key.SlpCode }).ToList();
                List<QueryRank> slpList = new List<QueryRank>();
                foreach (QuerySlpCode item in slpCodeList)
                {
                    decimal sumAmount = Math.Round(Convert.ToDecimal((await GetDeliveryCompareLastYearRatio(item.SlpCode)).Split('%')[0]), 2);
                    slpList.Add(new QueryRank(item.SlpCode, Math.Round(sumAmount, 2)));
                }

                slpList = slpList.OrderByDescending(r => r.TotalAmount).ToList();
                int indexOf = slpList.IndexOf(slpList.Where(r => r.SlpCode == slpCode).FirstOrDefault()) + 1;
                int totalSlp = slpCodeList.Count();
                companyRank = indexOf + "/" + totalSlp;
            }

            return companyRank;
        }
        #endregion

        #region 业务增长趋势
        /// <summary>
        /// 业务增长趋势
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="timeType">查看维度</param>
        /// <returns>返回新增合同数与新增客户数增长趋势信息</returns>
        public async Task<TableData> GetServiceGrowthTrend(string startTime, string endTime, string timeType)
        {
            var result = new TableData();
            User loginUser = _saleBusinessMethodHelp.GetLoginUser(out int? slpCode);
            if (slpCode == 0)
                return result;

            QueryTableData td = _saleBusinessMethodHelp.TimeRangeType(startTime, endTime, timeType);
            if (td.IsSuccess)
            {
                List<int> customerYList = new List<int>();
                List<int> saleOrderYList = new List<int>();
                foreach (QueryTime item in td.queryTimes)
                {
                    int customerCount = await _saleBusinessMethodHelp.GetTimeRangCustomer(item.startTime, item.endTime, slpCode);//获取时间范围内新增用户数量
                    int saleOrderCount = await GetSaleOrderAddNum(item.startTime, item.endTime, slpCode);//获取时间范围内新增合同数
                    customerYList.Add(customerCount);
                    saleOrderYList.Add(saleOrderCount);
                }

                result.Data = new 
                { 
                     xAxis = td.xNum, 
                     customerYAxis = customerYList, 
                     saleOrderYAxis = saleOrderYList
                };
            }
            else
            {
                result.Message = td.Message;
                result.Code = 500;
            }
            
            return result;
        }

        /// <summary>
        /// 获取新增合同数
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回新增合同数</returns>
        public async Task<int> GetSaleOrderAddNum(string startTime, string endTime, int? slpCode)
        {
            var ordrList = await UnitWork.Find<ORDR>(r => r.SlpCode == slpCode)
                                                   .WhereIf(startTime != "", r => r.CreateDate >= Convert.ToDateTime(startTime))
                                                   .WhereIf(endTime != "", r => r.CreateDate <= Convert.ToDateTime(endTime))
                                                   .Where(r => r.CANCELED == "N")
                                                   .Select(r => r.DocEntry)
                                                   .ToListAsync();

            return ordrList.Count();
        }

        /// <summary>
        /// 获取业务趋势部门排名
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="slpCode">业务员编码</param>
        /// <param name="loginUser">当前登录者信息</param>
        /// <param name="BusinessType">业务类型</param>
        /// <returns>返回业务员趋势部门排名信息</returns>
        public async Task<string> GetDeaprtRank(string startTime, string endTime, int? slpCode, User loginUser, string BusinessType)
        {
            string departRank = "0/0";
            var userDepart = await UnitWork.Find<base_user_detail>(r => r.user_id == loginUser.User_Id).FirstOrDefaultAsync();
            string depAlias = (await UnitWork.Find<base_dep>(r => r.dep_id == userDepart.dep_id).FirstOrDefaultAsync()).dep_alias;
            var departRankList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_DepartRank")).Select(u => u.DtValue).ToListAsync();
            if (departRankList.Contains(depAlias))
            {
                var userList = await UnitWork.Find<base_user_detail>(r => r.dep_id == userDepart.dep_id).Select(r => r.user_id).ToListAsync();
                var slpCodeList = (await UnitWork.Find<sbo_user>(r => userList.Contains(r.user_id)).Where(r => r.sbo_id == 1).Select(r => new QuerySlpCode { SlpCode = Convert.ToInt32(r.sale_id) }).ToListAsync()).GroupBy(r => new { r.SlpCode }).Select(r => new QuerySlpCode { SlpCode = r.Key.SlpCode }).ToList();
                List<QueryRank> slpList = new List<QueryRank>();
                foreach (QuerySlpCode item in slpCodeList)
                {
                    decimal sumAmount = Convert.ToDecimal(BusinessType == "Cusotmer" ? await _saleBusinessMethodHelp.GetTimeRangCustomer(startTime, endTime, slpCode) : await GetSaleOrderAddNum(startTime, endTime, slpCode));
                    slpList.Add(new QueryRank(item.SlpCode, Math.Round(sumAmount, 2)));
                }

                slpList = slpList.OrderByDescending(r => r.TotalAmount).ToList();
                int indexOf = slpList.IndexOf(slpList.Where(r => r.SlpCode == slpCode).FirstOrDefault()) + 1;
                int totalSlp = slpCodeList.Count();
                departRank = indexOf + "/" + totalSlp;
            }

            return departRank;
        }

        /// <summary>
        /// 获取业务趋势公司排名
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="slpCode">业务员编码</param>
        /// <param name="loginUser">当前登录者信息</param>
        /// <param name="BusinessType">业务类型</param>
        /// <returns>返回业务员趋势公司排名信息</returns>
        public async Task<string> GetCompanyRank(string startTime, string endTime, int? slpCode, User loginUser, string BusinessType)
        {
            string companyRank = "0/0";
            var userDepart = await UnitWork.Find<base_user_detail>(r => r.user_id == loginUser.User_Id).FirstOrDefaultAsync();
            string depAlias = (await UnitWork.Find<base_dep>(r => r.dep_id == userDepart.dep_id).FirstOrDefaultAsync()).dep_alias;
            var departRankList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_DepartRank")).Select(u => u.DtValue).ToListAsync();
            if (departRankList.Contains(depAlias))
            {
                var depIdList = await UnitWork.Find<base_dep>(r => departRankList.Contains(r.dep_alias)).Select(r => r.dep_id).ToListAsync();
                var userList = await UnitWork.Find<base_user_detail>(r => depIdList.Contains(r.dep_id)).Select(r => r.user_id).ToListAsync();
                var slpCodeList = (await UnitWork.Find<sbo_user>(r => userList.Contains(r.user_id)).Where(r => r.sbo_id == 1).Select(r => new QuerySlpCode { SlpCode = Convert.ToInt32(r.sale_id) }).ToListAsync()).GroupBy(r => new { r.SlpCode }).Select(r => new QuerySlpCode { SlpCode = r.Key.SlpCode }).ToList();
                List<QueryRank> slpList = new List<QueryRank>();
                foreach (QuerySlpCode item in slpCodeList)
                {
                    decimal sumAmount = Convert.ToDecimal(BusinessType == "Customer" ? await _saleBusinessMethodHelp.GetTimeRangCustomer(startTime, endTime, slpCode) : await GetSaleOrderAddNum(startTime, endTime, slpCode));
                    slpList.Add(new QueryRank(item.SlpCode, Math.Round(sumAmount, 2)));
                }

                slpList = slpList.OrderByDescending(r => r.TotalAmount).ToList();
                int indexOf = slpList.IndexOf(slpList.Where(r => r.SlpCode == slpCode).FirstOrDefault()) + 1;
                int totalSlp = slpCodeList.Count();
                companyRank = indexOf + "/" + totalSlp;
            }

            return companyRank;
        }
        #endregion

        #region 销售趋势
        /// <summary>
        /// 获取销售趋势
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="currency">币种</param>
        /// <param name="timeType">查看维度</param>
        /// <returns>返回销售趋势信息</returns>
        public async Task<TableData> GetSaleTrend(string startTime, string endTime, string currency, string timeType)
        {
            var result = new TableData();
            User loginUser = _saleBusinessMethodHelp.GetLoginUser(out int? slpCode);
            if (slpCode == 0)
                return result;

            QueryTableData td = _saleBusinessMethodHelp.TimeRangeType(startTime, endTime, timeType);
            if (td.IsSuccess)
            {
                List<decimal> ordrYList = new List<decimal>();
                List<decimal> odlnYList = new List<decimal>();
                List<decimal> orctYList = new List<decimal>();
                List<decimal> fbmYList = new List<decimal>();
                List<decimal> orinYList = new List<decimal>();
                foreach (QueryTime item in td.queryTimes)
                {
                    decimal ordrAmount = await _saleBusinessMethodHelp.GetORDRAmount(item.startTime, item.endTime, currency, slpCode);//销售订单金额
                    decimal odlnAmount = await _saleBusinessMethodHelp.GetODLNAmount(item.startTime, item.endTime, currency, slpCode);//销售交货金额
                    decimal orctAmount = await _saleBusinessMethodHelp.GetORCTAmount(item.startTime, item.endTime, currency, slpCode);//销售回款金额
                    decimal fbmAmount = await _saleBusinessMethodHelp.GetFBMAmount(item.startTime, item.endTime, currency, slpCode, loginUser);//销售开票金额
                    decimal orinAmount = await _saleBusinessMethodHelp.GetORINAmount(item.startTime, item.endTime, currency, slpCode);//贷项凭证金额
                    ordrYList.Add(ordrAmount);
                    odlnYList.Add(odlnAmount);
                    orctYList.Add(orctAmount);
                    fbmYList.Add(fbmAmount);
                    orinYList.Add(orinAmount); 
                }

                result.Data = new
                {
                    xAxis = td.xNum,
                    ordrY = ordrYList,
                    odlnY = odlnYList,
                    orctY = orctYList,
                    fbmY = fbmYList,
                    orinY = orinYList,
                };
            }
            else
            {
                result.Message = td.Message;
                result.Code = 500;
            }

            return result;
        }

        /// <summary>
        /// 获取销售趋势部门排名
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="slpCode">业务员编码</param>
        /// <param name="loginUser">当前登录者信息</param>
        /// <param name="SaleType">销售类型</param>
        /// <param name="currency">币种</param>
        /// <returns>返回业务员销售趋势部门排名信息</returns>
        public async Task<string> GetSaleDeaprtRank(string startTime, string endTime, int? slpCode, User loginUser, string SaleType, string currency)
        {
            string departRank = "0/0";
            var userDepart = await UnitWork.Find<base_user_detail>(r => r.user_id == loginUser.User_Id).FirstOrDefaultAsync();
            string depAlias = (await UnitWork.Find<base_dep>(r => r.dep_id == userDepart.dep_id).FirstOrDefaultAsync()).dep_alias;
            var departRankList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_DepartRank")).Select(u => u.DtValue).ToListAsync();
            if (departRankList.Contains(depAlias))
            {
                var userList = await UnitWork.Find<base_user_detail>(r => r.dep_id == userDepart.dep_id).Select(r => r.user_id).ToListAsync();
                var slpCodeList = (await UnitWork.Find<sbo_user>(r => userList.Contains(r.user_id)).Where(r => r.sbo_id == 1).Select(r => new QuerySlpCode { SlpCode = Convert.ToInt32(r.sale_id) }).ToListAsync()).GroupBy(r => new { r.SlpCode }).Select(r => new QuerySlpCode { SlpCode = r.Key.SlpCode }).ToList();
                List<QueryRank> slpList = new List<QueryRank>();
                foreach (QuerySlpCode item in slpCodeList)
                {
                    decimal sumAmount = 0;
                    switch (SaleType)
                    {
                        case "ORDR":
                             sumAmount = await _saleBusinessMethodHelp.GetORDRAmount(startTime, endTime, currency, slpCode);
                            break;
                        case "ODLN":
                            sumAmount = await _saleBusinessMethodHelp.GetODLNAmount(startTime, endTime, currency, slpCode);
                            break;
                        case "ORCT":
                            sumAmount = await _saleBusinessMethodHelp.GetORCTAmount(startTime, endTime, currency, slpCode);
                            break;
                        case "FBM":
                            sumAmount = await _saleBusinessMethodHelp.GetFBMAmount(startTime, endTime, currency, slpCode, loginUser);
                            break;
                        case "ORIN":
                            sumAmount = await _saleBusinessMethodHelp.GetORINAmount(startTime, endTime, currency, slpCode);
                            break;
                        default:
                            sumAmount = 0;
                            break;
                    }

                    slpList.Add(new QueryRank(item.SlpCode, Math.Round(sumAmount, 2)));
                }

                slpList = slpList.OrderByDescending(r => r.TotalAmount).ToList();
                int indexOf = slpList.IndexOf(slpList.Where(r => r.SlpCode == slpCode).FirstOrDefault()) + 1;
                int totalSlp = slpCodeList.Count();
                departRank = indexOf + "/" + totalSlp;
            }

            return departRank;
        }

        /// <summary>
        /// 获取销售趋势公司排名
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="slpCode">业务员编码</param>
        /// <param name="loginUser">当前登录者信息</param>
        /// <param name="SaleType">销售类型</param>
        /// <param name="currency">币种</param>
        /// <returns>返回业务员销售趋势公司排名信息</returns>
        public async Task<string> GetSaleCompanyRank(string startTime, string endTime, int? slpCode, User loginUser, string SaleType, string currency)
        {
            string companyRank = "0/0";
            var userDepart = await UnitWork.Find<base_user_detail>(r => r.user_id == loginUser.User_Id).FirstOrDefaultAsync();
            string depAlias = (await UnitWork.Find<base_dep>(r => r.dep_id == userDepart.dep_id).FirstOrDefaultAsync()).dep_alias;
            var departRankList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_DepartRank")).Select(u => u.DtValue).ToListAsync();
            if (departRankList.Contains(depAlias))
            {
                var depIdList = await UnitWork.Find<base_dep>(r => departRankList.Contains(r.dep_alias)).Select(r => r.dep_id).ToListAsync();
                var userList = await UnitWork.Find<base_user_detail>(r => depIdList.Contains(r.dep_id)).Select(r => r.user_id).ToListAsync();
                var slpCodeList = (await UnitWork.Find<sbo_user>(r => userList.Contains(r.user_id)).Where(r => r.sbo_id == 1).Select(r => new QuerySlpCode { SlpCode = Convert.ToInt32(r.sale_id) }).ToListAsync()).GroupBy(r => new { r.SlpCode }).Select(r => new QuerySlpCode { SlpCode = r.Key.SlpCode }).ToList();
                List<QueryRank> slpList = new List<QueryRank>();
                foreach (QuerySlpCode item in slpCodeList)
                {
                    decimal sumAmount = 0;
                    switch (SaleType)
                    {
                        case "ORDR":
                            sumAmount = await _saleBusinessMethodHelp.GetORDRAmount(startTime, endTime, currency, slpCode);
                            break;
                        case "ODLN":
                            sumAmount = await _saleBusinessMethodHelp.GetODLNAmount(startTime, endTime, currency, slpCode);
                            break;
                        case "ORCT":
                            sumAmount = await _saleBusinessMethodHelp.GetORCTAmount(startTime, endTime, currency, slpCode);
                            break;
                        case "FBM":
                            sumAmount = await _saleBusinessMethodHelp.GetFBMAmount(startTime, endTime, currency, slpCode, loginUser);
                            break;
                        case "ORIN":
                            sumAmount = await _saleBusinessMethodHelp.GetORINAmount(startTime, endTime, currency, slpCode);
                            break;
                        default:
                            sumAmount = 0;
                            break;
                    }
                    slpList.Add(new QueryRank(item.SlpCode, Math.Round(sumAmount, 2)));
                }

                slpList = slpList.OrderByDescending(r => r.TotalAmount).ToList();
                int indexOf = slpList.IndexOf(slpList.Where(r => r.SlpCode == slpCode).FirstOrDefault()) + 1;
                int totalSlp = slpCodeList.Count();
                companyRank = indexOf + "/" + totalSlp;
            }

            return companyRank;
        }
        #endregion
    }
}
