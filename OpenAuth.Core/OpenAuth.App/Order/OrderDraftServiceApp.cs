using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using OpenAuth.App.Response;
using OpenAuth.App.Interface;
using OpenAuth.App.Order;
using OpenAuth.Repository;
using OpenAuth.Repository.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Sap;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Order.Request;
using NSAP.Entity.Sales;
using Infrastructure;

namespace OpenAuth.App.Order
{
    public class OrderDraftServiceApp : OnlyUnitWorkBaeApp
    {
        private IUnitWork _UnitWork;
        private IAuth _auth;
        private ServiceBaseApp _serviceBaseApp;
        private List<int> _docEntrys = new List<int>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        /// <param name="serviceBaseApp"></param>
        public OrderDraftServiceApp(IUnitWork unitWork, IAuth auth, ServiceBaseApp serviceBaseApp) : base(unitWork, auth)
        {
            _UnitWork = unitWork;
            _auth = auth;
            _serviceBaseApp = serviceBaseApp;
        }

        #region 单据对比
        /// <summary>
        /// 获取单据对比内容
        /// </summary>
        /// <param name="DocEntry">凭证编码</param>
        /// <param name="DocEntryList">凭证编码集合</param>
        /// <param name="DataType">数据类型</param>
        /// <returns>返回单据基础信息对比和单据明细对比信息</returns>
        public async Task<TableData> GetOrderDataContent(int DocEntry, List<int> DocEntryList, string DataType)
        {
            var result = new TableData();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            decimal totalQutation = 0;
            decimal totalSaleOrder = 0;
            decimal totalDiff = 0;
            string orderCompareRate;
            OrderDataCompareReq orderCurrentCompareReq = DataType == "Quto" ? await GetCurrentCsutomerContrast(DocEntry, sboid) : await GetCurrentCsutomerSaleOrderContrast(DocEntry, sboid);//销售报价单或销售订单基础信息
            OrderDataCompareReq saleOrderCompareReq = await GetSaleOrderCsutomerContrast(DocEntryList, sboid);//参与对比的销售订单基础信息
            orderCurrentCompareReq.CustomerContrasts.AddRange(saleOrderCompareReq.CustomerContrasts);
            orderCurrentCompareReq.CoinContrasts.AddRange(saleOrderCompareReq.CoinContrasts);
            orderCurrentCompareReq.PaymentContrasts.AddRange(saleOrderCompareReq.PaymentContrasts);
            var coinList = orderCurrentCompareReq.CoinContrasts.Select(r => new CoinGroupByName { Name = r.Name }).GroupBy(r => r.Name).ToList();
            if (coinList.Count() > 1)
            {
                orderCurrentCompareReq.ItemCompareDetails = DataType == "Quto" ? await GetOrderDataDetail(DocEntry, DocEntryList, sboid, "RMB") : await GetSaleOrderDataDetail(DocEntry, DocEntryList, sboid, "RMB");
            }
            else
            {
                orderCurrentCompareReq.ItemCompareDetails = DataType == "Quto" ? await GetOrderDataDetail(DocEntry, DocEntryList, sboid, "") : await GetSaleOrderDataDetail(DocEntry, DocEntryList, sboid, "");
            }

            //定义合计，并添加到物料明细对比集合中
            ItemCompareDetail itemCompareDetail = new ItemCompareDetail();
            itemCompareDetail.ItemCode = "合计";
            itemCompareDetail.QuotationQuantity = "";
            itemCompareDetail.SaleOrderQuantity = "";
            itemCompareDetail.DifferenceQuantity = "";
            itemCompareDetail.QuotationPrice = "";
            itemCompareDetail.SaleOrderPrice = "";
            itemCompareDetail.DifferencePrice = "";
            totalQutation = orderCurrentCompareReq.ItemCompareDetails.Sum(r => Convert.ToDecimal(r.QutationTotalAmount));
            itemCompareDetail.QutationTotalAmount = totalQutation.ToString();
            totalSaleOrder = orderCurrentCompareReq.ItemCompareDetails.Sum(r => Convert.ToDecimal(r.SaleOrderTotalAmount));
            itemCompareDetail.SaleOrderTotalAmount = totalSaleOrder.ToString();
            totalDiff = orderCurrentCompareReq.ItemCompareDetails.Sum(r => Convert.ToDecimal(r.DifferenceTotalAmount));
            itemCompareDetail.DifferenceTotalAmount = totalDiff.ToString();
            orderCurrentCompareReq.ItemCompareDetails.Add(itemCompareDetail);

            //计算当前单据相较于对比单据变化百分比
            if (totalDiff > 0)
            {
                orderCompareRate = totalSaleOrder == 0 ? "0%" : "+" + (Math.Round((totalDiff / totalSaleOrder) * 100, 2)).ToString() + "%";
            }
            else
            {
                orderCompareRate = totalSaleOrder == 0 ? "0%" : (Math.Round((totalDiff / totalSaleOrder) * 100, 2)).ToString() + "%";
            }
            
            result.Data = new { OrderCompareReq = orderCurrentCompareReq, OrderCompareRate = orderCompareRate };
            return result;
        }

        /// <summary>
        /// 获取当前报价单单据基础信息对比
        /// </summary>
        /// <param name="DocEntry">凭证代码</param>
        /// <param name="sboid">账套Id</param>
        /// <returns>返回客户对比信息、币种汇率对比信息、付款条件对比信息</returns>
        public async Task<OrderDataCompareReq> GetCurrentCsutomerContrast(int DocEntry, int sboid)
        {
            OrderDataCompareReq orderDataCompareReq = new OrderDataCompareReq();
            var oqutList = await UnitWork.Find<OQUT>(r => r.DocEntry == DocEntry).Select(r => new { r.DocEntry, r.CardCode, r.CardName, r.DocCur, r.DocRate, r.GroupNum }).ToListAsync();
            var currentOrderList = from a in oqutList
                                   join b in await UnitWork.Find<crm_ocrn>(r => r.sbo_id == sboid).Select(r => new { r.CurrCode, r.CurrName }).ToListAsync() on a.DocCur equals b.CurrCode into ab
                                   from b in ab.DefaultIfEmpty()
                                   join c in await UnitWork.Find<crm_octg>(r => r.sbo_id == sboid).Select(r => new { r.GroupNum, r.PymntGroup }).ToListAsync() on Convert.ToInt32(a.GroupNum) equals c.GroupNum into ac
                                   from c in ac.DefaultIfEmpty()
                                   select new
                                   {
                                       a.DocEntry,
                                       a.CardCode,
                                       a.CardName,
                                       a.DocCur,
                                       b.CurrName,
                                       a.DocRate,
                                       a.GroupNum,
                                       c.PymntGroup
                                   };

            orderDataCompareReq.CustomerContrasts = currentOrderList.Select(r => new CustomerContrast { DocEntry = "当前单据(" + r.DocEntry + ")", CardCode = r.CardCode, CardName = r.CardName }).ToList();
            orderDataCompareReq.CoinContrasts = currentOrderList.Select(r => new CoinContrast { DocEntry = "当前单据(" + r.DocEntry + ")", Name = r.CurrName, DocRate = r.DocRate }).ToList();
            orderDataCompareReq.PaymentContrasts = currentOrderList.Select(r => new PaymentContrast { DocEntry = "当前单据(" + r.DocEntry + ")", GroupNum = r.PymntGroup }).ToList();
            return orderDataCompareReq;
        }

        /// <summary>
        /// 获取当前销售订单单单据基础信息对比
        /// </summary>
        /// <param name="DocEntry">凭证代码</param>
        /// <param name="sboid">账套Id</param>
        /// <returns>返回客户对比信息、币种汇率对比信息、付款条件对比信息</returns>
        public async Task<OrderDataCompareReq> GetCurrentCsutomerSaleOrderContrast(int DocEntry, int sboid)
        {
            OrderDataCompareReq orderDataCompareReq = new OrderDataCompareReq();
            var oqutList = await UnitWork.Find<ORDR>(r => r.DocEntry == DocEntry).Select(r => new { r.DocEntry, r.CardCode, r.CardName, r.DocCur, r.DocRate, r.GroupNum }).ToListAsync();
            var currentOrderList = from a in oqutList
                                   join b in await UnitWork.Find<crm_ocrn>(r => r.sbo_id == sboid).Select(r => new { r.CurrCode, r.CurrName }).ToListAsync() on a.DocCur equals b.CurrCode into ab
                                   from b in ab.DefaultIfEmpty()
                                   join c in await UnitWork.Find<crm_octg>(r => r.sbo_id == sboid).Select(r => new { r.GroupNum, r.PymntGroup }).ToListAsync() on Convert.ToInt32(a.GroupNum) equals c.GroupNum into ac
                                   from c in ac.DefaultIfEmpty()
                                   select new
                                   {
                                       a.DocEntry,
                                       a.CardCode,
                                       a.CardName,
                                       a.DocCur,
                                       b.CurrName,
                                       a.DocRate,
                                       a.GroupNum,
                                       c.PymntGroup
                                   };

            orderDataCompareReq.CustomerContrasts = currentOrderList.Select(r => new CustomerContrast { DocEntry = "当前单据(" + r.DocEntry + ")", CardCode = r.CardCode, CardName = r.CardName }).ToList();
            orderDataCompareReq.CoinContrasts = currentOrderList.Select(r => new CoinContrast { DocEntry = "当前单据(" + r.DocEntry + ")", Name = r.CurrName, DocRate = r.DocRate }).ToList();
            orderDataCompareReq.PaymentContrasts = currentOrderList.Select(r => new PaymentContrast { DocEntry = "当前单据(" + r.DocEntry + ")", GroupNum = r.PymntGroup }).ToList();
            return orderDataCompareReq;
        }

        /// <summary>
        /// 获取销售订单基础信息对比
        /// </summary>
        /// <param name="DocEntryList">凭证编码集合</param>
        /// <param name="sboid">账套Id</param>
        /// <returns>返回参与对比的销售订单客户对比、币种汇率对比、付款条件对比信息</returns>
        public async Task<OrderDataCompareReq> GetSaleOrderCsutomerContrast(List<int> DocEntryList, int sboid)
        {
            OrderDataCompareReq orderDataCompareReq = new OrderDataCompareReq();
            var ordrList = await UnitWork.Find<ORDR>(r => DocEntryList.Contains(r.DocEntry)).Select(r => new { r.DocEntry, r.CardCode, r.CardName, r.DocCur, r.DocRate, r.GroupNum }).ToListAsync();
            var currentOrderList = from a in ordrList
                                   join b in await UnitWork.Find<crm_ocrn>(r => r.sbo_id == sboid).Select(r => new { r.CurrCode, r.CurrName }).ToListAsync() on a.DocCur equals b.CurrCode into ab
                                   from b in ab.DefaultIfEmpty()
                                   join c in await UnitWork.Find<crm_octg>(r => r.sbo_id == sboid).Select(r => new { r.GroupNum, r.PymntGroup }).ToListAsync() on Convert.ToInt32(a.GroupNum) equals c.GroupNum into ac
                                   from c in ac.DefaultIfEmpty()
                                   select new
                                   {
                                       a.DocEntry,
                                       a.CardCode,
                                       a.CardName,
                                       a.DocCur,
                                       b.CurrName,
                                       a.DocRate,
                                       a.GroupNum,
                                       c.PymntGroup
                                   };

            orderDataCompareReq.CustomerContrasts = currentOrderList.Select(r => new CustomerContrast { DocEntry = r.DocEntry.ToString(), CardCode = r.CardCode, CardName = r.CardName }).ToList();
            orderDataCompareReq.CoinContrasts = currentOrderList.Select(r => new CoinContrast { DocEntry = r.DocEntry.ToString(), Name = r.CurrName, DocRate = r.DocRate }).ToList();
            orderDataCompareReq.PaymentContrasts = currentOrderList.Select(r => new PaymentContrast { DocEntry = r.DocEntry.ToString(), GroupNum = r.PymntGroup }).ToList();
            return orderDataCompareReq;
        }

        /// <summary>
        /// 获取报价单单据明细信息对比
        /// </summary>
        /// <param name="DocEntry">凭证编码</param>
        /// <param name="DocEntryList">凭证编码集合</param>
        /// <param name="sboid">账套Id</param>
        /// <param name="currency">币种</param>
        /// <returns>返回单据物料数量对比，单价对比，总金额对比信息</returns>
        public async Task<List<ItemCompareDetail>> GetOrderDataDetail(int DocEntry, List<int> DocEntryList, int sboid, string currency)
        {
            //销售报价单及物料明细
            var qut1List = from a in await UnitWork.Find<OQUT>(r => r.DocEntry == DocEntry).Select(r => new { r.DocEntry, r.DocRate }).ToListAsync()
                           join b in await UnitWork.Find<sale_qut1>(r => r.DocEntry == DocEntry && r.sbo_id == sboid).Select(r => new { r.DocEntry, r.ItemCode, r.Quantity, r.Price }).ToListAsync() on a.DocEntry equals b.DocEntry
                           into ab
                           from b in ab.DefaultIfEmpty()
                           select new ItemCodeGroup
                           {
                               DocEntry = a.DocEntry,
                               DocRate = Convert.ToDecimal(a.DocRate),
                               ItemCode = b == null ? "" : b.ItemCode,
                               Quantity = b == null ? 0 : b.Quantity,
                               Price = b == null ? 0 : b.Price
                           };

            //销售订单及物料明细
            var rdr1List = from a in await UnitWork.Find<ORDR>(r => DocEntryList.Contains(r.DocEntry)).Select(r => new { r.DocEntry, r.DocRate }).ToListAsync()
                           join b in await UnitWork.Find<sale_rdr1>(r => DocEntryList.Contains(r.DocEntry) && r.sbo_id == sboid).Select(r => new { r.DocEntry, r.ItemCode, r.Quantity, r.Price }).ToListAsync() on a.DocEntry equals b.DocEntry
                           into ab
                           from b in ab.DefaultIfEmpty()
                           select new ItemCodeGroup
                           {
                               DocEntry = a.DocEntry,
                               DocRate = Convert.ToDecimal(a.DocRate),
                               ItemCode = b == null ? "" : b.ItemCode,
                               Quantity = b == null ? 0 : b.Quantity,
                               Price = b == null ? 0 : b.Price
                           };

            //销售报价单与销售订单全部物料编码
            var itemList = qut1List.Select(r => r.ItemCode).Union(rdr1List.Select(x => x.ItemCode)).ToList();

            //销售报价单物料编码，税率，数量，单价，总价明细集合
            var itemQUT1List = qut1List.GroupBy(r => new { r.ItemCode, r.Price, r.DocRate })
                                       .Select(r => new ItemGroup 
                                       { 
                                           ItemCode = r.Key.ItemCode, 
                                           TotalQuantity = r.Sum(x => x.Quantity), 
                                           TotalAmount = r.Sum(x => (x.Price * x.Quantity)),
                                           Price = r.Key.Price,
                                           DocRate = r.Key.DocRate
                                       }).ToList();

            //销售订单物料编码，税率，数量，单价，总价明细集合
            var itemRDR1List = rdr1List.GroupBy(r => new { r.ItemCode, r.Price, r.DocRate })
                                       .Select(r => new ItemGroup
                                       {
                                           ItemCode = r.Key.ItemCode,
                                           TotalQuantity = r.Sum(x => x.Quantity),
                                           TotalAmount = r.Sum(x => (x.Price * x.Quantity)),
                                           Price = r.Key.Price,
                                           DocRate = r.Key.DocRate
                                       }).ToList();

            //销报价单与销售订单物料明细对比
            List<ItemCompareDetail> itemComapreDetailList = new List<ItemCompareDetail>();
            if (currency == "RMB")
            {
                var itemDetailList = from a in itemList
                                     join b in itemQUT1List on a equals b.ItemCode into ab
                                     from b in ab.DefaultIfEmpty()
                                     join c in itemRDR1List on a equals c.ItemCode into ac
                                     from c in ac.DefaultIfEmpty()
                                     select new ItemCompareDetail
                                     {
                                         ItemCode = a,
                                         QuotationQuantity = b == null ? "0" : (b.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalQuantity), 2)).ToString(),
                                         SaleOrderQuantity = c == null ? "0" : (c.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalQuantity), 2)).ToString(),
                                         DifferenceQuantity = ((b == null ? 0 : (b.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalQuantity), 2))) - (c == null ? 0 : (c.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalQuantity), 2)))).ToString(),
                                         QuotationPrice = b == null ? "0" : (Math.Round(Convert.ToDecimal(b.Price), 2)).ToString(),
                                         SaleOrderPrice = c == null ? "0" : ((c.TotalQuantity == null || c.TotalQuantity == 0) ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount / c.TotalQuantity), 2)).ToString(),
                                         DifferencePrice = ((b == null ? 0 : Math.Round(Convert.ToDecimal(b.Price), 2)) - (c==null ? 0 :((c.TotalQuantity == null || c.TotalQuantity == 0) ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount / c.TotalQuantity), 2)))).ToString(),
                                         QutationTotalAmount = b == null ? "0" : (b.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalAmount), 2)).ToString(),
                                         SaleOrderTotalAmount = c == null ? "0" : (c.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount), 2)).ToString(),
                                         DifferenceTotalAmount = ((b == null ? 0 : (b.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalAmount), 2))) - (c == null ? 0 : (c.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount), 2)))).ToString()
                                     };

                itemComapreDetailList = itemDetailList.ToList();
            }
            else
            {
                var itemDetailList = from a in itemList
                                     join b in itemQUT1List on a equals b.ItemCode into ab
                                     from b in ab.DefaultIfEmpty()
                                     join c in itemRDR1List on a equals c.ItemCode into ac
                                     from c in ac.DefaultIfEmpty()
                                     select new ItemCompareDetail
                                     {
                                         ItemCode = a,
                                         QuotationQuantity = b == null ? "0" : (b.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalQuantity), 2)).ToString(),
                                         SaleOrderQuantity = c == null ? "0" : (c.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalQuantity), 2)).ToString(),
                                         DifferenceQuantity = ((b == null ? 0 : (b.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalQuantity), 2))) - (c == null ? 0 : (c.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalQuantity), 2)))).ToString(),
                                         QuotationPrice = b == null ? "0" : (Math.Round(Convert.ToDecimal(b.Price * b.DocRate), 2)).ToString(),
                                         SaleOrderPrice = c == null ? "0" : ((c.TotalQuantity == null || c.TotalQuantity == 0) ? 0 : Math.Round(Convert.ToDecimal((c.TotalAmount * c.DocRate) / c.TotalQuantity), 2)).ToString(),
                                         DifferencePrice = ((b == null ? 0 : Math.Round(Convert.ToDecimal(b.Price * b.DocRate), 2)) - (c == null ? 0 : (c.TotalQuantity == null || c.TotalQuantity == 0) ? 0 : Math.Round(Convert.ToDecimal((c.TotalAmount * c.DocRate) / c.TotalQuantity), 2))).ToString(),
                                         QutationTotalAmount = b == null ? "0" : (b.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalAmount * b.DocRate), 2)).ToString(),
                                         SaleOrderTotalAmount = c == null ? "0" : (c.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount * c.DocRate), 2)).ToString(),
                                         DifferenceTotalAmount = ((b == null ? 0 : (b.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalAmount * b.DocRate), 2))) - (c == null ? 0 :(c.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount * c.DocRate), 2)))).ToString()
                                     };

                itemComapreDetailList = itemDetailList.ToList();
            }

            return itemComapreDetailList;
        }

        /// <summary>
        /// 获取销售订单单据明细信息对比
        /// </summary>
        /// <param name="DocEntry">凭证编码</param>
        /// <param name="DocEntryList">凭证编码集合</param>
        /// <param name="sboid">账套Id</param>
        /// <param name="currency">币种</param>
        /// <returns>返回单据物料数量对比，单价对比，总金额对比信息</returns>
        public async Task<List<ItemCompareDetail>> GetSaleOrderDataDetail(int DocEntry, List<int> DocEntryList, int sboid, string currency)
        {
            //销售订单当前单据及物料明细
            var qut1List = from a in await UnitWork.Find<ORDR>(r => r.DocEntry == DocEntry).Select(r => new { r.DocEntry, r.DocRate }).ToListAsync()
                           join b in await UnitWork.Find<sale_qut1>(r => r.DocEntry == DocEntry && r.sbo_id == sboid).Select(r => new { r.DocEntry, r.ItemCode, r.Quantity, r.Price }).ToListAsync() on a.DocEntry equals b.DocEntry
                           into ab
                           from b in ab.DefaultIfEmpty()
                           select new ItemCodeGroup
                           {
                               DocEntry = a.DocEntry,
                               DocRate = Convert.ToDecimal(a.DocRate),
                               ItemCode = b == null ? "" : b.ItemCode,
                               Quantity = b == null ? 0 : b.Quantity,
                               Price = b == null ? 0 : b.Price
                           };

            //销售订单及物料明细
            var rdr1List = from a in await UnitWork.Find<ORDR>(r => DocEntryList.Contains(r.DocEntry)).Select(r => new { r.DocEntry, r.DocRate }).ToListAsync()
                           join b in await UnitWork.Find<sale_rdr1>(r => DocEntryList.Contains(r.DocEntry) && r.sbo_id == sboid).Select(r => new { r.DocEntry, r.ItemCode, r.Quantity, r.Price }).ToListAsync() on a.DocEntry equals b.DocEntry
                           into ab
                           from b in ab.DefaultIfEmpty()
                           select new ItemCodeGroup
                           {
                               DocEntry = a.DocEntry,
                               DocRate = Convert.ToDecimal(a.DocRate),
                               ItemCode = b == null ? "" : b.ItemCode,
                               Quantity = b == null ? 0 : b.Quantity,
                               Price = b == null ? 0 : b.Price
                           };

            //销售报价单与销售订单全部物料编码
            var itemList = qut1List.Select(r => r.ItemCode).Union(rdr1List.Select(x => x.ItemCode)).ToList();

            //销售报价单物料编码，税率，数量，单价，总价明细集合
            var itemQUT1List = qut1List.GroupBy(r => new { r.ItemCode, r.Price, r.DocRate })
                                       .Select(r => new ItemGroup
                                       {
                                           ItemCode = r.Key.ItemCode,
                                           TotalQuantity = r.Sum(x => x.Quantity),
                                           TotalAmount = r.Sum(x => (x.Price * x.Quantity)),
                                           Price = r.Key.Price,
                                           DocRate = r.Key.DocRate
                                       }).ToList();

            //销售订单物料编码，税率，数量，单价，总价明细集合
            var itemRDR1List = rdr1List.GroupBy(r => new { r.ItemCode, r.Price, r.DocRate })
                                       .Select(r => new ItemGroup
                                       {
                                           ItemCode = r.Key.ItemCode,
                                           TotalQuantity = r.Sum(x => x.Quantity),
                                           TotalAmount = r.Sum(x => (x.Price * x.Quantity)),
                                           Price = r.Key.Price,
                                           DocRate = r.Key.DocRate
                                       }).ToList();

            //销报价单与销售订单物料明细对比
            List<ItemCompareDetail> itemComapreDetailList = new List<ItemCompareDetail>();
            if (currency == "RMB")
            {
                var itemDetailList = from a in itemList
                                     join b in itemQUT1List on a equals b.ItemCode into ab
                                     from b in ab.DefaultIfEmpty()
                                     join c in itemRDR1List on a equals c.ItemCode into ac
                                     from c in ac.DefaultIfEmpty()
                                     select new ItemCompareDetail
                                     {
                                         ItemCode = a,
                                         QuotationQuantity = b == null ? "0" : (b.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalQuantity), 2)).ToString(),
                                         SaleOrderQuantity = c == null ? "0" : (c.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalQuantity), 2)).ToString(),
                                         DifferenceQuantity = ((b == null ? 0 : (b.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalQuantity), 2))) - (c == null ? 0 : (c.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalQuantity), 2)))).ToString(),
                                         QuotationPrice = b == null ? "0" : (Math.Round(Convert.ToDecimal(b.Price), 2)).ToString(),
                                         SaleOrderPrice = c == null ? "0" : ((c.TotalQuantity == null || c.TotalQuantity == 0) ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount / c.TotalQuantity), 2)).ToString(),
                                         DifferencePrice = ((b == null ? 0 : Math.Round(Convert.ToDecimal(b.Price), 2)) - (c == null ? 0 : ((c.TotalQuantity == null || c.TotalQuantity == 0) ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount / c.TotalQuantity), 2)))).ToString(),
                                         QutationTotalAmount = b == null ? "0" : (b.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalAmount), 2)).ToString(),
                                         SaleOrderTotalAmount = c == null ? "0" : (c.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount), 2)).ToString(),
                                         DifferenceTotalAmount = ((b == null ? 0 : (b.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalAmount), 2))) - (c == null ? 0 : (c.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount), 2)))).ToString()
                                     };

                itemComapreDetailList = itemDetailList.ToList();
            }
            else
            {
                var itemDetailList = from a in itemList
                                     join b in itemQUT1List on a equals b.ItemCode into ab
                                     from b in ab.DefaultIfEmpty()
                                     join c in itemRDR1List on a equals c.ItemCode into ac
                                     from c in ac.DefaultIfEmpty()
                                     select new ItemCompareDetail
                                     {
                                         ItemCode = a,
                                         QuotationQuantity = b == null ? "0" : (b.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalQuantity), 2)).ToString(),
                                         SaleOrderQuantity = c == null ? "0" : (c.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalQuantity), 2)).ToString(),
                                         DifferenceQuantity = ((b == null ? 0 : (b.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalQuantity), 2))) - (c == null ? 0 : (c.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalQuantity), 2)))).ToString(),
                                         QuotationPrice = b == null ? "0" : (Math.Round(Convert.ToDecimal(b.Price * b.DocRate), 2)).ToString(),
                                         SaleOrderPrice = c == null ? "0" : ((c.TotalQuantity == null || c.TotalQuantity == 0) ? 0 : Math.Round(Convert.ToDecimal((c.TotalAmount * c.DocRate) / c.TotalQuantity), 2)).ToString(),
                                         DifferencePrice = ((b == null ? 0 : Math.Round(Convert.ToDecimal(b.Price * b.DocRate), 2)) - (c == null ? 0 : (c.TotalQuantity == null || c.TotalQuantity == 0) ? 0 : Math.Round(Convert.ToDecimal((c.TotalAmount * c.DocRate) / c.TotalQuantity), 2))).ToString(),
                                         QutationTotalAmount = b == null ? "0" : (b.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalAmount * b.DocRate), 2)).ToString(),
                                         SaleOrderTotalAmount = c == null ? "0" : (c.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount * c.DocRate), 2)).ToString(),
                                         DifferenceTotalAmount = ((b == null ? 0 : (b.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalAmount * b.DocRate), 2))) - (c == null ? 0 : (c.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount * c.DocRate), 2)))).ToString()
                                     };

                itemComapreDetailList = itemDetailList.ToList();
            }

            return itemComapreDetailList;
        }

        /// <summary>
        /// 单据对比明细信息
        /// </summary>
        /// <param name="DocEntryList">凭证编码集合</param>
        /// <param name="sboid">账套id</param>
        /// <param name="currency">币种</param>
        /// <param name="itemCodeGroups">当前单据基础信息</param>
        /// <returns>单据对比明细信息</returns>
        public async Task<List<ItemCompareDetail>> GetMyCreateOrder(List<int> DocEntryList, int sboid, string currency, List<ItemCodeGroup> itemCodeGroups)
        {
            //明细
            var qut1List = itemCodeGroups;

            //销售订单及物料明细
            var rdr1List = from a in await UnitWork.Find<ORDR>(r => DocEntryList.Contains(r.DocEntry)).Select(r => new { r.DocEntry, r.DocRate }).ToListAsync()
                           join b in await UnitWork.Find<sale_rdr1>(r => DocEntryList.Contains(r.DocEntry) && r.sbo_id == sboid).Select(r => new { r.DocEntry, r.ItemCode, r.Quantity, r.Price }).ToListAsync() on a.DocEntry equals b.DocEntry
                           into ab
                           from b in ab.DefaultIfEmpty()
                           select new ItemCodeGroup
                           {
                               DocEntry = a.DocEntry,
                               DocRate = Convert.ToDecimal(a.DocRate),
                               ItemCode = b == null ? "" : b.ItemCode,
                               Quantity = b == null ? 0 : b.Quantity,
                               Price = b == null ? 0 : b.Price
                           };

            //销售报价单与销售订单全部物料编码
            var itemList = qut1List.Select(r => r.ItemCode).Union(rdr1List.Select(x => x.ItemCode)).ToList();

            //销售报价单物料编码，税率，数量，单价，总价明细集合
            var itemQUT1List = qut1List.GroupBy(r => new { r.ItemCode, r.Price, r.DocRate })
                                       .Select(r => new ItemGroup
                                       {
                                           ItemCode = r.Key.ItemCode,
                                           TotalQuantity = r.Sum(x => x.Quantity),
                                           TotalAmount = r.Sum(x => (x.Price * x.Quantity)),
                                           Price = r.Key.Price,
                                           DocRate = r.Key.DocRate
                                       }).ToList();

            //销售订单物料编码，税率，数量，单价，总价明细集合
            var itemRDR1List = rdr1List.GroupBy(r => new { r.ItemCode, r.Price, r.DocRate })
                                       .Select(r => new ItemGroup
                                       {
                                           ItemCode = r.Key.ItemCode,
                                           TotalQuantity = r.Sum(x => x.Quantity),
                                           TotalAmount = r.Sum(x => (x.Price * x.Quantity)),
                                           Price = r.Key.Price,
                                           DocRate = r.Key.DocRate
                                       }).ToList();

            //销报价单与销售订单物料明细对比
            List<ItemCompareDetail> itemComapreDetailList = new List<ItemCompareDetail>();
            if (currency == "RMB")
            {
                var itemDetailList = from a in itemList
                                     join b in itemQUT1List on a equals b.ItemCode into ab
                                     from b in ab.DefaultIfEmpty()
                                     join c in itemRDR1List on a equals c.ItemCode into ac
                                     from c in ac.DefaultIfEmpty()
                                     select new ItemCompareDetail
                                     {
                                         ItemCode = a,
                                         QuotationQuantity = b == null ? "0" : (b.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalQuantity), 2)).ToString(),
                                         SaleOrderQuantity = c == null ? "0" : (c.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalQuantity), 2)).ToString(),
                                         DifferenceQuantity = ((b == null ? 0 : (b.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalQuantity), 2))) - (c == null ? 0 : (c.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalQuantity), 2)))).ToString(),
                                         QuotationPrice = b == null ? "0" : (Math.Round(Convert.ToDecimal(b.Price), 2)).ToString(),
                                         SaleOrderPrice = c == null ? "0" : ((c.TotalQuantity == null || c.TotalQuantity == 0) ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount / c.TotalQuantity), 2)).ToString(),
                                         DifferencePrice = ((b == null ? 0 : Math.Round(Convert.ToDecimal(b.Price), 2)) - (c == null ? 0 : ((c.TotalQuantity == null || c.TotalQuantity == 0) ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount / c.TotalQuantity), 2)))).ToString(),
                                         QutationTotalAmount = b == null ? "0" : (b.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalAmount), 2)).ToString(),
                                         SaleOrderTotalAmount = c == null ? "0" : (c.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount), 2)).ToString(),
                                         DifferenceTotalAmount = ((b == null ? 0 : (b.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalAmount), 2))) - (c == null ? 0 : (c.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount), 2)))).ToString()
                                     };

                itemComapreDetailList = itemDetailList.ToList();
            }
            else
            {
                var itemDetailList = from a in itemList
                                     join b in itemQUT1List on a equals b.ItemCode into ab
                                     from b in ab.DefaultIfEmpty()
                                     join c in itemRDR1List on a equals c.ItemCode into ac
                                     from c in ac.DefaultIfEmpty()
                                     select new ItemCompareDetail
                                     {
                                         ItemCode = a,
                                         QuotationQuantity = b == null ? "0" : (b.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalQuantity), 2)).ToString(),
                                         SaleOrderQuantity = c == null ? "0" : (c.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalQuantity), 2)).ToString(),
                                         DifferenceQuantity = ((b == null ? 0 : (b.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalQuantity), 2))) - (c == null ? 0 : (c.TotalQuantity == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalQuantity), 2)))).ToString(),
                                         QuotationPrice = b == null ? "0" : (Math.Round(Convert.ToDecimal(b.Price * b.DocRate), 2)).ToString(),
                                         SaleOrderPrice = c == null ? "0" : ((c.TotalQuantity == null || c.TotalQuantity == 0) ? 0 : Math.Round(Convert.ToDecimal((c.TotalAmount * c.DocRate) / c.TotalQuantity), 2)).ToString(),
                                         DifferencePrice = ((b == null ? 0 : Math.Round(Convert.ToDecimal(b.Price * b.DocRate), 2)) - (c == null ? 0 : (c.TotalQuantity == null || c.TotalQuantity == 0) ? 0 : Math.Round(Convert.ToDecimal((c.TotalAmount * c.DocRate) / c.TotalQuantity), 2))).ToString(),
                                         QutationTotalAmount = b == null ? "0" : (b.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalAmount * b.DocRate), 2)).ToString(),
                                         SaleOrderTotalAmount = c == null ? "0" : (c.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount * c.DocRate), 2)).ToString(),
                                         DifferenceTotalAmount = ((b == null ? 0 : (b.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(b.TotalAmount * b.DocRate), 2))) - (c == null ? 0 : (c.TotalAmount == null ? 0 : Math.Round(Convert.ToDecimal(c.TotalAmount * c.DocRate), 2)))).ToString()
                                     };

                itemComapreDetailList = itemDetailList.ToList();
            }

            return itemComapreDetailList;
        }

        /// <summary>
        /// 我的创建时审批单据对比
        /// </summary>
        /// <param name="jobId">账套id</param>
        /// <param name="isAudit">审计</param>
        /// <param name="docType">凭证类型</param>
        /// <param name="DocEntryList">凭证编码集合</param>
        /// <returns>返回单据对比信息</returns>
        public async Task<TableData> GetMyCreateCompare(string jobId, string isAudit, string docType, List<int> DocEntryList)
        {
            var result = new TableData();
            var userId = _serviceBaseApp.GetUserNaspId();
            var sboid = _serviceBaseApp.GetUserNaspSboID(userId);
            decimal totalQutation = 0;
            decimal totalSaleOrder = 0;
            decimal totalDiff = 0;
            string orderCompareRate;
            OrderDataCompareReq orderDataCompareReq = new OrderDataCompareReq();
            billDelivery bill = GetDeliverySalesInfoNew(jobId, isAudit, docType);

            //客户基础信息
            List<CustomerContrast> customerContrasts = new List<CustomerContrast>();
            customerContrasts.Add(new CustomerContrast() { DocEntry = "当前单据(" + jobId + ")", CardCode = bill.CardCode, CardName = bill.CardName });

            //货币基础信息
            List<CoinContrast> coinContrasts = new List<CoinContrast>();
            string CurrName = (await UnitWork.Find<crm_ocrn>(r => r.sbo_id == 1 && r.CurrCode == bill.DocCur).Select(r => new { r.CurrCode, r.CurrName }).FirstOrDefaultAsync()).CurrName;
            coinContrasts.Add(new CoinContrast() { DocEntry = "当前单据(" + jobId + ")", Name = CurrName, DocRate = Math.Round(Convert.ToDecimal(bill.DocRate), 2) });

            //付款条件基础信息
            List<PaymentContrast> paymentContrasts = new List<PaymentContrast>();
            string PymntGroup = (await UnitWork.Find<crm_octg>(r => r.sbo_id == 1).Select(r => new { r.GroupNum, r.PymntGroup }).FirstOrDefaultAsync()).PymntGroup;
            paymentContrasts.Add(new PaymentContrast() { DocEntry = "当前单据(" + jobId + ")", GroupNum = PymntGroup });

            //基础信息添加
            orderDataCompareReq.CustomerContrasts = customerContrasts;
            orderDataCompareReq.CoinContrasts = coinContrasts;
            orderDataCompareReq.PaymentContrasts = paymentContrasts;

            //参与对比的销售订单基础信息
            OrderDataCompareReq saleOrderCompareReq = await GetSaleOrderCsutomerContrast(DocEntryList, sboid);
            orderDataCompareReq.CustomerContrasts.AddRange(saleOrderCompareReq.CustomerContrasts);
            orderDataCompareReq.CoinContrasts.AddRange(saleOrderCompareReq.CoinContrasts);
            orderDataCompareReq.PaymentContrasts.AddRange(saleOrderCompareReq.PaymentContrasts);
            var coinList = orderDataCompareReq.CoinContrasts.Select(r => new CoinGroupByName { Name = r.Name }).GroupBy(r => r.Name).ToList();
            
            //将当前审批单据信息转换为list集合
            List<billDelivery> billDeliveries = new List<billDelivery>();
            bill.Remark = jobId;
            billDeliveries.Add(bill);
            List<ItemCodeGroup>  itemCodeGroups = (from a in billDeliveries.Select(r => new { r.billBaseEntry, r.DocCur, r.Remark, r.DocRate })
                              join b in bill.billSalesDetails.Select(r => new { r.BaseEntry, r.ItemCode, r.Price, r.Quantity }) on a.billBaseEntry equals b.BaseEntry into ab
                              from b in ab.DefaultIfEmpty()
                              select new ItemCodeGroup
                              {
                                  DocEntry = Convert.ToInt32(a.Remark),
                                  DocRate = Convert.ToDecimal(a.DocRate),
                                  ItemCode = b == null ? "" : b.ItemCode,
                                  Price = b == null ? 0 : Convert.ToDecimal(b.Price),
                                  Quantity = b == null ? 0 : Convert.ToDecimal(b.Quantity)
                              }).ToList();

            if (coinList.Count() > 1)
            {
                orderDataCompareReq.ItemCompareDetails = await GetMyCreateOrder(DocEntryList, sboid, "RMB", itemCodeGroups);
            }
            else
            {
                orderDataCompareReq.ItemCompareDetails = await GetMyCreateOrder(DocEntryList, sboid, "", itemCodeGroups);
            }

            //定义合计，并添加到物料明细对比集合中
            ItemCompareDetail itemCompareDetail = new ItemCompareDetail();
            itemCompareDetail.ItemCode = "合计";
            itemCompareDetail.QuotationQuantity = "";
            itemCompareDetail.SaleOrderQuantity = "";
            itemCompareDetail.DifferenceQuantity = "";
            itemCompareDetail.QuotationPrice = "";
            itemCompareDetail.SaleOrderPrice = "";
            itemCompareDetail.DifferencePrice = "";
            totalQutation = orderDataCompareReq.ItemCompareDetails.Sum(r => Convert.ToDecimal(r.QutationTotalAmount));
            itemCompareDetail.QutationTotalAmount = totalQutation.ToString();
            totalSaleOrder = orderDataCompareReq.ItemCompareDetails.Sum(r => Convert.ToDecimal(r.SaleOrderTotalAmount));
            itemCompareDetail.SaleOrderTotalAmount = totalSaleOrder.ToString();
            totalDiff = orderDataCompareReq.ItemCompareDetails.Sum(r => Convert.ToDecimal(r.DifferenceTotalAmount));
            itemCompareDetail.DifferenceTotalAmount = totalDiff.ToString();
            orderDataCompareReq.ItemCompareDetails.Add(itemCompareDetail);

            //计算当前单据相较于对比单据变化百分比
            if (totalDiff > 0)
            {
                orderCompareRate = totalSaleOrder == 0 ? "0%" : "+" + (Math.Round((totalDiff / totalSaleOrder) * 100, 2)).ToString() + "%";
            }
            else
            {
                orderCompareRate = totalSaleOrder == 0 ? "0%" : (Math.Round((totalDiff / totalSaleOrder) * 100, 2)).ToString() + "%";
            }

            result.Data = new { OrderCompareReq = orderDataCompareReq, OrderCompareRate = orderCompareRate };
            return result;
        }

        /// <summary>
        /// 获取审批中单据数据
        /// </summary>
        /// <param name="jobId">任务Id</param>
        /// <param name="isAudit">是否审计</param>
        /// <param name="docType">凭证类型</param>
        /// <returns>返回单据信息</returns>
        public billDelivery GetDeliverySalesInfoNew(string jobId, string isAudit, string docType)
        {
            billDelivery bill = DeSerialize<billDelivery>((byte[])(GetSalesInfo(jobId)));
            DataTable dt = GetSboNamePwd(int.Parse(bill.SboId));
            string dRowData = string.Empty; string isOpen = "0"; string sboname = "0"; string sqlconn = "0";
            if (dt.Rows.Count > 0) { isOpen = dt.Rows[0][6].ToString(); sboname = dt.Rows[0][0].ToString(); sqlconn = dt.Rows[0][5].ToString(); }
            if (docType != "oqut" && !string.IsNullOrEmpty(bill.billBaseEntry))
            {
                DataTable _callInfo = GetCallInfoById(bill.billBaseEntry, bill.SboId, docType, "1", "call");
                if (_callInfo.Rows.Count > 0)
                {
                    bill.U_CallID = _callInfo.Rows[0][0].ToString();
                    bill.U_CallName = _callInfo.Rows[0][1].ToString();
                    bill.U_SerialNumber = _callInfo.Rows[0][2].ToString();
                }
            }
            string type = bill.DocType;
            string _main = JsonHelper.ParseModel(bill);
            DateTime docDate;
            DateTime.TryParse(bill.DocDate, out docDate);
            bill.DocDate = docDate.ToString("yyyy/MM/dd");
            DateTime docDueDate;
            DateTime.TryParse(bill.DocDueDate, out docDueDate);
            bill.DocDueDate = docDueDate.ToString("yyyy/MM/dd");
            DateTime prepaData;
            DateTime.TryParse(bill.PrepaData, out prepaData);
            bill.PrepaData = prepaData.ToString("yyyy/MM/dd");
            DateTime goodsToDate;
            DateTime.TryParse(bill.GoodsToDate, out goodsToDate);
            bill.GoodsToDate = goodsToDate.ToString("yyyy/MM/dd");
            foreach (var files in bill.attachmentData)
            {
                DateTime filetime;
                DateTime.TryParse(files.filetime, out filetime);
                files.filetime = filetime.ToString("yyyy/MM/dd hh:mm:ss");
            }

            return bill;
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public T DeSerialize<T>(byte[] bytes)
        {
            T oClass = default(T);
            if (bytes.Length == 0 || bytes == null) return oClass;
            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter bs = new BinaryFormatter();
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)bs.Deserialize(stream);
            }
        }

        /// <summary>
        /// 获取sql密码，名称等信息
        /// </summary>
        /// <param name="SboId">账套id</param>
        /// <returns>返回sql链接数据信息</returns>
        public DataTable GetSboNamePwd(int SboId)
        {
            string strSql = string.Format("SELECT sql_db,sql_name,sql_pswd,sap_name,sap_pswd,sql_conn,is_open FROM {0}.sbo_info WHERE sbo_id={1}", "nsap_base", SboId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }

        /// <summary>
        /// 根据jobid获取信息
        /// </summary>
        public object GetSalesInfo(string jobId)
        {
            string sql = string.Format("SELECT job_data FROM {0}.wfa_job WHERE job_id = {1}", "nsap_base", jobId);
            return UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, sql, CommandType.Text, null);
        }

        /// <summary>
        /// 查询单据关联的服务呼叫或物料成本
        /// </summary>
        /// <param name="docEntry">单据编号</param>
        /// <param name="sboId">帐套ID</param>
        /// <param name="docType">当前表</param>
        /// <param name="isAudit">查看或是审核</param>
        /// <param name="openDoc">call 关联报价单服务呼叫信息   stock关联报价单的物料成本</param>
        /// <returns></returns>
        public DataTable GetCallInfoById(string docEntry, string sboId, string docType, string isAudit, string openDoc)
        {
            if (isAudit == "0")
            {
                switch (docType)
                {
                    case "sale_oqut": docType = "a"; break;
                    case "sale_ordr": docType = "b"; break;
                    case "sale_odln": docType = "c"; break;
                    case "sale_oinv": docType = "d"; break;
                    case "sale_ordn": docType = "e"; break;
                    case "sale_orin": docType = "f"; break;
                    default: docType = "a"; break;
                }
            }
            else
            {
                switch (docType)
                {
                    case "ordr": docType = "a"; break;
                    case "odln": docType = "b"; break;
                    case "oinv": docType = "c"; break;
                    case "ordn": docType = "d"; break;
                    case "orin": docType = "e"; break;
                    default: docType = "a"; break;
                }
            }
            StringBuilder strSql = new StringBuilder();
            if (openDoc == "call")
            {
                strSql.AppendFormat("SELECT a.U_CallID,a.U_CallName,a.U_SerialNumber FROM {0}.sale_oqut a ", "nsap_bone");
            }
            else
            {
                strSql.AppendFormat("SELECT a.DocEntry,a.ItemCode ,a.StockPrice FROM {0}.sale_qut1 a  ", "nsap_bone");
            }
            strSql.AppendFormat("LEFT JOIN {0}.sale_rdr1 b ON b.BaseEntry = a.DocEntry AND b.sbo_id = a.sbo_id ", "nsap_bone");
            strSql.AppendFormat("LEFT JOIN {0}.sale_dln1 c ON c.BaseEntry = b.DocEntry AND c.sbo_id = b.sbo_id ", "nsap_bone");
            strSql.AppendFormat("LEFT JOIN {0}.sale_inv1 d ON d.BaseEntry = c.DocEntry AND d.sbo_id = c.sbo_id ", "nsap_bone");
            strSql.AppendFormat("LEFT JOIN {0}.sale_rdn1 e ON e.BaseEntry = c.DocEntry AND e.sbo_id = c.sbo_id ", "nsap_bone");
            strSql.AppendFormat("LEFT JOIN {0}.sale_rin1 f ON f.BaseEntry = d.DocEntry AND f.sbo_id = d.sbo_id ", "nsap_bone");
            strSql.AppendFormat("WHERE {2}.DocEntry = {0} AND a.sbo_id = '{1}' GROUP BY a.DocEntry", docEntry, sboId, docType);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
        }
        #endregion

        #region 查询关联订单号
        /// <summary>
        /// 获取所有关联订单号
        /// </summary>
        /// <param name="docEntry">凭证编码</param>
        /// <returns>返回关联订单号集合信息</returns>
        public List<int> GetDocEntrys(string docEntry)
        {
            GetAssociateDownDocEntry(docEntry);
            GetAssociateUpDocEntry(docEntry);
            _docEntrys = _docEntrys.Distinct().ToList();
            return _docEntrys;
        }

        /// <summary>
        /// 向下查询所有销售订单凭证编号
        /// </summary>
        /// <param name="docEntry">凭证编号</param>
        /// <returns>返回向上所有销售订单凭证编号集合</returns>
        public List<int> GetAssociateDownDocEntry(string docEntry)
        {
            try
            {
                if (docEntry != null && docEntry != "")
                {
                    if (docEntry.Contains(","))
                    {
                        foreach (string item in docEntry.Split(','))
                        {
                            _docEntrys.Add(Convert.ToInt32(item));
                            var docEntryDownList = from a in  UnitWork.Find<OQUT>(r => r.U_New_ORDRID == item).Select(r => new { r.DocEntry, r.U_New_ORDRID }).ToList()
                                                   join b in  UnitWork.Find<RDR1>(null).Select(r => new { r.DocEntry, r.BaseEntry }).ToList() on a.DocEntry equals b.BaseEntry
                                                   into ab
                                                   from b in ab.DefaultIfEmpty()
                                                   select new QueryDocEntryReq { DocEntry = a.DocEntry, U_New_ORDRID = a.U_New_ORDRID, BaseEntry = Convert.ToInt32(b == null ? 0 : b.BaseEntry), SaleDocEntry = Convert.ToInt32(b.DocEntry) };

                            List<QueryDocEntryReq> queryDocEntryReqs = (docEntryDownList.GroupBy(r => new { r.DocEntry, r.BaseEntry, r.U_New_ORDRID, r.SaleDocEntry })
                                                                                 .Select(r => new QueryDocEntryReq
                                                                                 {
                                                                                     DocEntry = r.Key.DocEntry,
                                                                                     BaseEntry = r.Key.BaseEntry,
                                                                                     U_New_ORDRID = r.Key.U_New_ORDRID,
                                                                                     SaleDocEntry = r.Key.SaleDocEntry
                                                                                 })).ToList();

                            if (queryDocEntryReqs.Distinct().Count() > 0)
                            {
                                foreach (QueryDocEntryReq req in queryDocEntryReqs)
                                {
                                    _docEntrys.Add(req.SaleDocEntry);
                                     GetAssociateDownDocEntry(req.SaleDocEntry.ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        _docEntrys.Add(Convert.ToInt32(docEntry));
                        var docEntryDownList = from a in  UnitWork.Find<OQUT>(r => r.U_New_ORDRID == docEntry).Select(r => new { r.DocEntry, r.U_New_ORDRID }).ToList()
                                               join b in  UnitWork.Find<RDR1>(null).Select(r => new { r.DocEntry, r.BaseEntry }).ToList() on a.DocEntry equals b.BaseEntry
                                               into ab
                                               from b in ab.DefaultIfEmpty()
                                               select new QueryDocEntryReq { DocEntry = a.DocEntry, U_New_ORDRID = a.U_New_ORDRID, BaseEntry = Convert.ToInt32(b == null ? 0 : b.BaseEntry), SaleDocEntry = Convert.ToInt32(b.DocEntry) };

                        List<QueryDocEntryReq> queryDocEntryReqs = (docEntryDownList.GroupBy(r => new { r.DocEntry, r.BaseEntry, r.U_New_ORDRID, r.SaleDocEntry })
                                                                                .Select(r => new QueryDocEntryReq
                                                                                {
                                                                                    DocEntry = r.Key.DocEntry,
                                                                                    BaseEntry = r.Key.BaseEntry,
                                                                                    U_New_ORDRID = r.Key.U_New_ORDRID,
                                                                                    SaleDocEntry = r.Key.SaleDocEntry
                                                                                })).ToList();

                        if (queryDocEntryReqs.Distinct().Count() > 0)
                        {
                            foreach (QueryDocEntryReq req in queryDocEntryReqs)
                            {
                                _docEntrys.Add(req.SaleDocEntry);
                                GetAssociateDownDocEntry(req.SaleDocEntry.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string a = ex.Message.ToString();
            }

            return _docEntrys;
        }

        /// <summary>
        /// 向上查询所有销售订单凭证编号
        /// </summary>
        /// <param name="docEntry">凭证编号</param>
        /// <returns>返回向上所有销售订单凭证编号集合</returns>
        public List<int> GetAssociateUpDocEntry(string docEntry)
        {
            try
            {
                if (docEntry != null && docEntry != "")
                {
                    List<int> docEntryList = new List<int>();
                    if (docEntry.Contains(","))
                    {
                        foreach (string item in docEntry.Split(','))
                        {
                            docEntryList.Add(Convert.ToInt32(item));
                        }

                        _docEntrys.AddRange(docEntryList);
                        _docEntrys = GetAssociateUpDocEntryFor(docEntryList);
                    }
                    else
                    {
                        docEntryList.Add(Convert.ToInt32(docEntry));
                        _docEntrys.AddRange(docEntryList);
                        _docEntrys = GetAssociateUpDocEntryFor(docEntryList);
                    }
                }
            }
            catch (Exception ex)
            {
                string a = ex.Message.ToString();
            }

            return _docEntrys;
        }

        /// <summary>
        /// 向上递归查询
        /// </summary>
        /// <param name="docEntrys">凭证编码集合</param>
        /// <returns>返回向上编码集合信息</returns>
        public List<int> GetAssociateUpDocEntryFor(List<int> docEntrys)
        {
            _docEntrys.AddRange(docEntrys);
            var docEntryUpList = from a in UnitWork.Find<RDR1>(r => docEntrys.Contains(Convert.ToInt32(r.DocEntry))).Select(r => new { r.DocEntry, r.BaseEntry }).ToList()
                                 join b in UnitWork.Find<OQUT>(null).Select(r => new { r.DocEntry, r.U_New_ORDRID }).ToList() on a.BaseEntry equals b.DocEntry
                                 into ab
                                 from b in ab.DefaultIfEmpty()
                                 where b.U_New_ORDRID != null
                                 select new QueryDocEntryReq { DocEntry = b.DocEntry, U_New_ORDRID = b.U_New_ORDRID, BaseEntry = Convert.ToInt32(a.BaseEntry), SaleDocEntry = Convert.ToInt32(a.DocEntry) };

            List<QueryDocEntryReq> queryDocEntryReqs = (docEntryUpList.GroupBy(r => new { r.DocEntry, r.BaseEntry, r.U_New_ORDRID, r.SaleDocEntry })
                                                                 .Select(r => new QueryDocEntryReq
                                                                 {
                                                                     DocEntry = r.Key.DocEntry,
                                                                     BaseEntry = r.Key.BaseEntry,
                                                                     U_New_ORDRID = r.Key.U_New_ORDRID,
                                                                     SaleDocEntry = r.Key.SaleDocEntry
                                                                 })).ToList();

            if (queryDocEntryReqs.Distinct().Count() > 0)
            {
                List<int> docEntryList = new List<int>();
                foreach (QueryDocEntryReq req in queryDocEntryReqs)
                {
                    if (req.U_New_ORDRID.Contains(","))
                    {
                        foreach (string item in req.U_New_ORDRID.Split(','))
                        {
                            docEntryList.Add(Convert.ToInt32(item));
                        }
                    }
                    else
                    {
                        docEntryList.Add(Convert.ToInt32(req.U_New_ORDRID));
                    }
                }

                _docEntrys.AddRange(docEntryList);
                GetAssociateUpDocEntryFor(docEntryList);
            }

            return _docEntrys;
        }
        #endregion
    }
}
