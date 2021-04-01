using OpenAuth.App.Serve.Response;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class AddDailyExpendsReq
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// 技术员App用户Id
        /// </summary>
        public int AppUserId { get; set; }

        /// <summary>
        /// 差旅费
        /// </summary>
        public TravelExpense travelExpense { get; set; }

        /// <summary>
        /// 交通费集合
        /// </summary>
        public List<TransportExpense> transportExpenses { get; set; }

        /// <summary>
        /// 住宿费集合
        /// </summary>
        public List<HotelExpense> hotelExpenses { get; set; }

        /// <summary>
        /// 其他费用集合
        /// </summary>
        public List<OtherExpense> otherExpenses { get; set; }
    }

    /// <summary>
    /// 差旅费
    /// </summary>
    public class TravelExpense
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime? CreateTime { get; set; }
        /// <summary>
        /// 天数
        /// </summary>
        public int? Days { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Money { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }

    /// <summary>
    /// 交通费
    /// </summary>
    public class TransportExpense
    {
        /// <summary>
        /// 费用类型
        /// </summary>
        public string FeeType { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int? SerialNumber { get; set; }

        /// <summary>
        /// 交通类型(1去程 2返程)
        /// </summary>
        public string TrafficType { get; set; }

        /// <summary>
        /// 交通工具（1飞机票 2铁路票 3长途车船票 4市内交通费）
        /// </summary>
        public string Transport { get; set; }

        /// <summary>
        /// 出发地
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// 目的地
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// 出发地址经度
        /// </summary>
        public string FromLng { get; set; }

        /// <summary>
        /// 出发地址纬度
        /// </summary>
        public string FromLat { get; set; }

        /// <summary>
        /// 到达地址经度
        /// </summary>
        public string ToLng { get; set; }

        /// <summary>
        /// 到达地址纬度
        /// </summary>
        public string ToLat { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Money { get; set; }

        /// <summary>
        /// 发票号码
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 开票日期
        /// </summary>
        public DateTime? InvoiceTime { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? TotalMoney { get; set; }

        /// <summary>
        /// 附件集合
        /// </summary>
        public List<DailyAttachment> dailyAttachments { get; set; }


        public List<ReimburseAttachmentResp> ReimburseAttachments { get; set; }
    }

    /// <summary>
    /// 住宿费
    /// </summary>
    public class HotelExpense
    {
        /// <summary>
        /// 费用类型
        /// </summary>
        public string FeeType { get; set; }

        /// <summary>
        /// 天数
        /// </summary>
        public int? Days { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Money { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 发票号码
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// 开票日期
        /// </summary>
        public DateTime? InvoiceTime { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int? SerialNumber { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? TotalMoney { get; set; }

        /// <summary>
        /// 附件集合
        /// </summary>
        public List<DailyAttachment> dailyAttachments { get; set; }

        public List<ReimburseAttachmentResp> ReimburseAttachments { get; set; }
    }

    /// <summary>
    /// 其他费用
    /// </summary>
    public class OtherExpense
    {
        /// <summary>
        /// 费用类别 1水电费 2物业管理费 3劳保费 4劳务费 5服务费 6清洁/保洁费 7福利费 8保险费 9办公费 10维修费 11业务招待费 12物流运输费 13快递费 14咨询顾问费 15 宣传费 16工具费 17耗材费 18其他
        /// </summary>
        public string ExpenseCategory { get; set; }

        /// <summary>
        /// 费用类型
        /// </summary>
        public string FeeType { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Money { get; set; }

        /// <summary>
        /// 发票号码
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// 开票日期
        /// </summary>
        public DateTime? InvoiceTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int? SerialNumber { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? TotalMoney { get; set; }

        /// <summary>
        /// 附件集合
        /// </summary>
        public List<DailyAttachment> dailyAttachments { get; set; }

        public List<ReimburseAttachmentResp> ReimburseAttachments { get; set; }
    }
}
