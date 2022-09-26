using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 客户关系  中间商与终端互查关系
    /// </summary>
    [Table("ClientRelation")]
    public partial class ClientRelation : Entity
    {

        /// <summary>
        /// 客户编号
        /// </summary>
        [Description("客户编号")]
        public string ClientNo { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        [Description("客户名称")]
        public string ClientName { get; set; }

        /// <summary>
        /// 父节点（关联节点）
        /// </summary>
        [Description("父节点（关联节点）")]
        public string ParentNo { get; set; }

        /// <summary>
        /// 子节点（关联节点）
        /// </summary>
        [Description("子节点（关联节点）")]
        public string SubNo { get; set; }

        /// <summary>
        /// 标识： 0.终端 1.中间商 2.中间商关联自己 
        /// </summary>
        [Description("标识")]
        public int Flag { get; set; }

        /// <summary>
        /// 是否草稿： 0 否 1 是
        /// </summary>
        [Description("草稿标识")]
        public int ScriptFlag { get; set; }

        /// <summary>
        /// 是否删除： 0 否 1 是
        /// </summary>
        [Description("是否删除")]
        public int IsDelete { get; set; }

        /// <summary>
        /// 是否有效： 0 否 1 是
        /// </summary>
        [Description("是否有效")]
        public int IsActive { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [Description("创建日期")]
        public System.DateTime CreateDate { get; set; }

        /// <summary>
        /// 更新日期
        /// </summary>
        [Description("更新日期")]
        public System.DateTime UpdateDate { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        public string Creator { get; set; }

        /// <summary>
        /// 创建人id
        /// </summary>
        [Description("创建人id")]
        public string Creatorid { get; set; }

        /// <summary>
        /// 变更人
        /// </summary>
        [Description("变更人")]
        public string Updater { get; set; }

        /// <summary>
        /// 变更人id
        /// </summary>
        [Description("变更人id")]
        public string Updaterid { get; set; }

        /// <summary>
        /// 业务员名字
        /// </summary>
        [Description("业务员名字")]
        public string Operator { get; set; }

        /// <summary>
        /// 业务员编号
        /// </summary>
        [Description("业务员编号")]
        public string Operatorid { get; set; }


        /// <summary>
        /// 客户变更记录
        /// </summary>
        public virtual List<ClientRelHistory> clientRelHistoryList { get; set; }
    }
}
