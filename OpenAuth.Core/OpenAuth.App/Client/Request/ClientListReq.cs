using System;
using System.Collections.Generic;
using System.Text;
using OpenAuth.App.Request;

namespace OpenAuth.App.Client.Request
{
    public class ClientListReq : PageReq
    {
        public string qtype { get; set; }
        public string query { get; set; }
        public string sortname { get; set; }
        public string sortorder { get; set; }

        /// <summary>
        /// 标签：0-全部,1-未报价,2-已成交,3-公海领取,4-即将掉入公海
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string ContectTel { get; set; }

        /// <summary>
        /// 归属业务员
        /// </summary>
        public string SlpName { get; set; }
        /// <summary>
        /// 是否中间商
        /// </summary>
        public string isReseller { get; set; }
        /// <summary>
        /// 多少天未报价
        /// </summary>
        public int? Day { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string CntctPrsn { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// 人员规模
        /// </summary>
        public string U_StaffScale { get; set; }
        /// <summary>
        /// 贸易类型
        /// </summary>
        public string U_TradeType { get; set; }
        /// <summary>
        /// 客户来源
        /// </summary>
        public string U_ClientSource { get; set; }
        /// <summary>
        /// 所属行业
        /// </summary>
        public string U_CompSector { get; set; }
        /// <summary>
        /// 客户类型
        /// </summary>
        public string U_CardTypeStr { get; set; }
        /// <summary>
        /// 创建开始时间
        /// </summary>
        public DateTime? CreateStartTime { get; set; }
        /// <summary>
        /// 创建结束时间
        /// </summary>
        public DateTime? CreateEndTime { get; set; }
        /// <summary>
        /// 归属变更开始时间
        /// </summary>
        public DateTime? DistributionStartTime { get; set; }
        /// <summary>
        /// 归属变更结束时间
        /// </summary>
        public DateTime? DistributionEndTime { get; set; }
        /// <summary>
        /// 未清交货单余额开始
        /// </summary>
        public decimal? dNotesBalStart { get; set; }
        /// <summary>
        /// 未清交货单余额结束
        /// </summary>
        public decimal? dNotesBalEnd { get; set; }
        /// <summary>
        /// 未清订单余额开始
        /// </summary>
        public decimal? ordersBalStart { get; set; }
        /// <summary>
        /// 未清订单余额结束
        /// </summary>
        public decimal? ordersBalEnd { get; set; }
        /// <summary>
        /// 科目余额开始
        /// </summary>
        public decimal? balanceStart { get; set; }
        /// <summary>
        /// 科目余额结束
        /// </summary>
        public decimal? balanceEnd { get; set; }
        /// <summary>
        /// 科目总余额开始
        /// </summary>
        public decimal? balanceTotalStart { get; set; }
        /// <summary>
        /// 科目总余额结束
        /// </summary>
        public decimal? balanceTotalEnd { get; set; }
        /// <summary>
        /// 产品类型
        /// </summary>
        public string ProductType { get; set; }
        /// <summary>
        /// lims推广员名称
        /// </summary>
        public string LimsName { get; set; }

    }
    /// <summary>
    /// 查询所有技术员model
    /// </summary>
    public class GetTcnicianInfoReq : PageReq
    {
        public string qtype { get; set; }
        public string query { get; set; }
        public string sortname { get; set; }
        public string sortorder { get; set; }
        public string SboId { get; set; }


    }
    ///查询 国家·省·市
    public class GetStateProvincesInfoReq : PageReq
    {
        public string qtype { get; set; }
        public string query { get; set; }
        public string sortname { get; set; }
        public string sortorder { get; set; }
        public string AddrType { get; set; }
        public string CountryId { get; set; }
        public string StateId { get; set; }

    }
    /// <summary>
    /// 客户下销售报价单
    /// </summary>
    public class SelectOqutReq:PageReq
    {

        public string Docentry { get; set; }
        public string Slpname { get; set; }
        public string Status { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string CardCode { get; set; }

        public bool isCount { get; set; } = false;
    }
    /// <summary>
    /// 客户下销售订单
    /// </summary>
    public class SelectOrdrReq : PageReq
    {

        public string Docentry { get; set; }
        public string Slpname { get; set; }
        public string Status { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string CardCode { get; set; }
    }
}
