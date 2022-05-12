using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Response
{
    public class QueryCustomerDetailResponse
    {
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }

        /// <summary>
        /// 客户类型
        /// </summary>
        public string CardName { get; set; }

        /// <summary>
        /// 客户归属(销售员)
        /// </summary>
        public string SlpName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 售后主管
        /// </summary>
        public string DfTcnician { get; set; }

        /// <summary>
        /// 终端用户名
        /// </summary>
        public string EndCustomerName { get; set; }

        /// <summary>
        /// 终端联系人
        /// </summary>
        public string EndCustomerContact { get; set; }

        /// <summary>
        /// 客户类型
        /// </summary>
        public string U_CardTypeStr { get; set; }

        /// <summary>
        /// 所属行业
        /// </summary>
        public string U_CompSector { get; set; }

        /// <summary>
        /// 贸易类型
        /// </summary>
        public string U_TradeType { get; set; }

        /// <summary>
        /// 客户来源
        /// </summary>
        public string U_ClientSource { get; set; }

        /// <summary>
        /// 人员规模
        /// </summary>
        public string U_StaffScale { get; set; }

        /// <summary>
        /// 是否是中间商
        /// </summary>
        public string Is_reseller { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string FreeText { get; set; }

        /// <summary>
        /// 网址
        /// </summary>
        public string IntrntSite { get; set; }

        /// <summary>
        /// 科目余额
        /// </summary>
        public decimal? Balance { get; set; }

        /// <summary>
        /// 总科目余额
        /// </summary>
        public decimal? TotalBalance { get; set; }

        /// <summary>
        /// 未清订单金额
        /// </summary>
        public decimal? OrdersBal { get; set; }

        /// <summary>
        /// 未清交货单金额
        /// </summary>
        public decimal? DNotesBal { get; set; }
    }
}
