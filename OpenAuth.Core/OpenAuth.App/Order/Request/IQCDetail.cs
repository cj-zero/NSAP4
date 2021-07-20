using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    /// <summary>
    /// 来料检查记录
    /// </summary>
    [Serializable]
    [DataContract]
    public class IQCDetail
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        [DataMember]
        public string ItemCode { get; set; }

        /// <summary>
        /// 物料描述
        /// </summary>
        [DataMember]
        public string ItemName { get; set; }

        /// <summary>
        /// 来料日期
        /// </summary>
        [DataMember]
        public string Income_dt { get; set; }

        /// <summary>
        /// 尺寸规格
        /// </summary>
        [DataMember]
        public string Inspect_dimension { get; set; }

        /// <summary>
        /// 功能检查
        /// </summary>
        [DataMember]
        public string Inspect_function { get; set; }

        /// <summary>
        /// 外观检查
        /// </summary>
        [DataMember]
        public string Inspect_appearance { get; set; }

        /// <summary>
        /// 其他检查
        /// </summary>
        [DataMember]
        public string Inspect_other { get; set; }

        /// <summary>
        /// 来料数量
        /// </summary>
        [DataMember]
        public string Income_num { get; set; }

        /// <summary>
        /// 样品数量
        /// </summary>
        [DataMember]
        public string Sample_num { get; set; }
        /// <summary>
        /// 不良数
        /// </summary>
        [DataMember]
        public string Defective_num { get; set; }

        /// <summary>
        /// 结果判定
        /// </summary>
        [DataMember]
        public string Inspect_result { get; set; }

        /// <summary>
        /// 不良描述
        /// </summary>
        [DataMember]
        public string Defective_desc { get; set; }
        /// <summary>
        /// 检查备注
        /// </summary>
        [DataMember]
        public string Check_remark { get; set; }
        /// <summary>
        /// 实物图片
        /// </summary>
        [DataMember]
        public string Item_pic { get; set; }

        /// <summary>
        /// 处理操作
        /// </summary>
        [DataMember]
        public string ProcessOperation { get; set; }
        /// <summary>
        /// 允收数量
        /// </summary>
        [DataMember]
        public string Accept_num { get; set; }
        /// <summary>
        /// 拒收数量
        /// </summary>
        [DataMember]
        public string Reject_num { get; set; }
    }
}
