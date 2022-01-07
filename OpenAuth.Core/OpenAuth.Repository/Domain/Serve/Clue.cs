using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Serve
{
    /// <summary>
    /// 线索
    /// </summary>
    [Table("clue")]
    public class Clue : BaseEntity<int>
    {
        /// <summary>
        /// 客户名称
        /// </summary>
        [Description("客户名称")]
        public string CardName { get; set; }
        /// <summary>
        /// 客户来源 0:领英、1:国内展会、2:国外展会、3:客户介绍、4:新威官网、5:其他
        /// </summary>
        [Description("客户来源")]
        public int CustomerSource { get; set; }
        /// <summary>
        /// 所属行业
        /// </summary>
        [Description("所属行业")]
        public int IndustryInvolved { get; set; }
        /// <summary>
        /// 人员规模
        /// </summary>
        [Description("人员规模")]
        public int StaffSize { get; set; }
        /// <summary>
        /// 网址
        /// </summary>
        [Description("网址")]
        public string WebSite { get; set; }
        /// <summary>
        ///备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }
        /// <summary>
        /// 标签集合
        /// </summary>
        [Description("标签集合")]
        public string Tags { get; set; }
        /// <summary>
        /// 状态（0：销售线索，1：已转客户）
        /// </summary>
        [Description("状态")]
        public int Status { get; set; }
        /// <summary>
        /// 是否认证
        /// </summary>
        [Description("是否认证")]
        public int IsCertification { get; set; }
        /// <summary>
        /// 线索编号
        /// </summary>
        [Description("线索编号")]
        public string SerialNumber { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime CreateTime { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>
        [Description("更新人")]
        public string UpdateUser { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [Description("更新时间")]
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        [Description("是否删除")]
        public bool IsDelete { get; set; } = false;

        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
