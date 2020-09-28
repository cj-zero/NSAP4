using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.TecentOCR
{
    public class InvoiceResponse
    {
        /// <summary>
        /// 发票号码
        /// </summary>
        public string InvoiceNo { get; set; }

        /// <summary>
        /// 合计金额（含税）
        /// </summary>
        public decimal AmountWithTax { get; set; }

        /// <summary>
        /// 开票公司名字
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 纳税人识别号
        /// </summary>
        public string ComapnyTaxCode { get; set; }

        /// <summary>
        /// 是否核验通过
        /// </summary>
        public int IsValidate { get; set; }

        /// <summary>
        /// 是否已使用
        /// </summary>
        public int IsUsed { get; set; }

        /// <summary>
        /// 票据类型 0：出租车发票 1：定额发票 2：火车票 3：增值税发票 5：机票行程单 8：通用机打发票 9：汽车票 10：轮船票 11：增值税发票（卷票 ）12：购车发票 13：过路过桥费发票
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 是否可报销 0不可报销 1可报销
        /// </summary>
        public int IsCanExpense { get; set; }

        /// <summary>
        /// 未通过原因
        /// </summary>
        public string NotPassReason { get; set; }
    }


    public class Result<T>
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public int Code;

        /// <summary>
        /// 操作消息
        /// </summary>
        public string Message;

        /// <summary>
        /// 数据内容
        /// </summary>
        public T Data;

        public Result()
        {
            Code = 200;
            Message = "请求成功";
        }
    }

    /// <summary>
    /// table的返回数据
    /// </summary>
    public class Result : Result<dynamic>
    {

    }

    public class TicketInfo
    {
        /// <summary>
        /// 发票类型
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 发票代码
        /// </summary>
        public string InvoiceCode { get; set; }

        /// <summary>
        /// 发票号码
        /// </summary>
        public string InvoiceNo { get; set; }

        /// <summary>
        /// 合计金额（含税）
        /// </summary>
        public decimal AmountWithTax { get; set; }

        /// <summary>
        /// 开票时间
        /// </summary>
        public string InvoiceDate { get; set; }

        /// <summary>
        /// 开票公司名字
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 纳税人识别号
        /// </summary>
        public string ComapnyTaxCode { get; set; }

        /// <summary>
        /// 发票校验码
        /// </summary>
        public string CheckCode { get; set; }

        public ExtendInfo Extend { get; set; }

    }

    public class ExtendInfo
    {
        /// <summary>
        /// 出发站
        /// </summary>
        public string OriginationStation { get; set; }

        /// <summary>
        /// 到达站
        /// </summary>
        public string ArrivalStation { get; set; }
    }
}
