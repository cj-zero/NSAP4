using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.NsapBone
{
    /// <summary>
	/// 
	/// </summary>
    [Table("store_drawing_job")]
    public class store_drawing_job
    {
        public int Id { get; set; }
        /// <summary>
        /// 帐套ID
        /// </summary>
        public int sbo_id { get; set; }
        public int job_id { get; set; }
        public int job_idMe { get; set; }
        public int SalesId { get; set; }
        /// <summary>
        /// 产品编码
        /// </summary>
        public int productNum { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string itemcode { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public string file_ver { get; set; }
        /// <summary>
        /// 状态（0-无图纸；1-未清；2-结算中;3-已结算）
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// 任务数据
        /// </summary>
        public byte[] job_data { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string projhsl { get; set; }
        /// <summary>
        /// 仓库代码
        /// </summary>
        public string WhsCode { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime upd_date { get; set; }
        /// <summary>
        /// 删除
        /// </summary>
        public int isDel { get; set; }
        /// <summary>
        /// 是否同步到
        /// SAP B1(0待同步；1同步中；2同步失败；3接口成功，数据同步失败；4同步完成)
        /// </summary>
        public int sync_stat { get; set; }
        public int StatusTZ { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public int Typeid { get; set; }
        /// <summary>
        /// 成品
        /// </summary>
        public int TypeTP { get; set; }
        /// <summary>
        /// 图纸
        /// </summary>
        public int TypeTZ { get; set; }
    }
}
