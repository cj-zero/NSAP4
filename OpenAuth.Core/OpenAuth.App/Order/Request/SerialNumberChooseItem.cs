using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    /// <summary>
    /// 序列号 - 物料
    /// </summary>
    [Serializable]
    [DataContract]
    public class SerialNumber
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
        public string Dscription { get; set; }

        /// <summary>
        /// 仓库
        /// </summary>
        [DataMember]
        public string WhsCode { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        [DataMember]
        public string Quantity { get; set; }

        /// <summary>
        /// 选择数量
        /// </summary>
        [DataMember]
        public string SelectQty { get; set; }

        /// <summary>
        /// 序列号 - 系统编号
        /// </summary>
        [DataMember]
        public IList<SerialNumberChooseItem> Details { get; set; }
    }

    /// <summary>
    /// 序列号 - 系统编号
    /// </summary>
    [Serializable]
    [DataContract]
    public class SerialNumberChooseItem
    {
        /// <summary>
        /// 系统编号
        /// </summary>
        [DataMember]
        public string SysSerial { get; set; }

        /// <summary>
        /// 制造商序列号
        /// </summary>
        [DataMember]
        public string SuppSerial { get; set; }

        /// <summary>
        /// 序列号
        /// </summary>
        [DataMember]
        public string IntrSerial { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [DataMember]
        public string CreateDate { get; set; }

        /// <summary>
        /// 原单据类型
        /// </summary>
        [DataMember]
        public string BaseType { get; set; }

        /// <summary>
        /// 原单据编号
        /// </summary>
        [DataMember]
        public string BaseEntry { get; set; }

        [DataMember]
        public string AbsEntry { get; set; }
    }
}
