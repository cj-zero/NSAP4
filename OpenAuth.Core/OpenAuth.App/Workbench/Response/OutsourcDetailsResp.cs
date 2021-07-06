using OpenAuth.App.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Workbench.Response
{
    public class OutsourcDetailsResp
    {
        /// <summary>
        /// 支付时间
        /// </summary>
        public System.DateTime? PayTime { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? TotalMoney { get; set; }
        /// <summary>
        /// 结算方式
        /// </summary>
        public int? ServiceMode { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 创建人姓名
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public System.DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 费用明细表
        /// </summary>
        public List<OutsourcExpensesResp> OutsourcExpenses { get; set; }
    }
    /// <summary>
    /// 费用明细表
    /// </summary>
    public class OutsourcExpensesResp
    {
        /// <summary>
        /// 费用类型
        /// </summary>
        public int? ExpensesType { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Money { get; set; }
        /// <summary>
        /// 出发地
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// 到达地
        /// </summary>
        public string To { get; set; }
        /// <summary>
        /// 出发地经度
        /// </summary>
        public string FromLng { get; set; }
        /// <summary>
        /// 出发地经度
        /// </summary>
        public string FromLat { get; set; }
        /// <summary>
        /// 到达地经度
        /// </summary>
        public string ToLng { get; set; }
        /// <summary>
        /// 到达地纬度
        /// </summary>
        public string ToLat { get; set; }
        /// <summary>
        /// 天数
        /// </summary>
        public int? Days { get; set; }
        /// <summary>
        /// 工时
        /// </summary>
        public int? ManHour { get; set; }

        /// <summary>
        /// 完工时间
        /// </summary>
        public DateTime? CompleteTime { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        public int? SerialNumber { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 附件表
        /// </summary>
        public List<FileResp> Files { get; set; }
    }
}
