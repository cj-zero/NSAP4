using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Response
{
    public class ReturnContractReview
    {
        /// <summary>
        /// 排序号
        /// </summary>
        public string? Sbo_Id { get; set; }
        /// <summary>
        /// 物料类型
        /// </summary>
        public int? Contract_Id { get; set; }
        public string CardCode { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public string Dscription { get; set; }
        /// <summary>
        /// 关联报价单
        /// </summary>
        public string Price { get; set; }
        /// <summary>
        /// sn和pn
        /// </summary>
        public string qty { get; set; }
        /// <summary>
        /// 原物料sn和pn
        /// </summary>
        public string sum_total { get; set; }
        /// <summary>
        /// 原物料编码
        /// </summary>
        public string maori { get; set; }
        /// <summary>
        /// 原物料描述
        /// </summary>
        public string SlpName { get; set; }

        public DateTime? Apply_dt { get; set; }
        public string Remarks { get; set; }
        public string DocStatus { get; set; }
        public DateTime? upd_dt { get; set; }
        public string Software_Review_Id { get; set; }
        public string ProductType { get; set; }
        public string HasBom { get; set; }
        public string DOCNUM { get; set; }
    }

    public class AutoContractReview
    {
        /// <summary>
        /// 排序号
        /// </summary>
        public string? Sbo_Id { get; set; }
        /// <summary>
        /// 物料类型
        /// </summary>
        public int? Contract_Id { get; set; }
        public string CardCode { get; set; }
        public string SlpCode { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public string Dscription { get; set; }
        /// <summary>
        /// 关联报价单
        /// </summary>
        public string Price { get; set; }
        /// <summary>
        /// sn和pn
        /// </summary>
        public string qty { get; set; }
        /// <summary>
        /// 原物料sn和pn
        /// </summary>
        public string sum_total { get; set; }
        /// <summary>
        /// 原物料编码
        /// </summary>
        public string maori { get; set; }
        /// <summary>
        /// 原物料描述
        /// </summary>
        public string SlpName { get; set; }

        public DateTime? Apply_dt { get; set; }
        public string Remarks { get; set; }
        public string DocStatus { get; set; }
        public DateTime? upd_dt { get; set; }
        public string Software_Review_Id { get; set; }
        public string ProductType { get; set; }
        public string HasBom { get; set; }
        public string DOCNUM { get; set; }

    }
}
