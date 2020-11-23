using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Response
{
    public class MaterialCodeListReap
    {
        /// <summary>
        /// 序列号
        /// </summary>
        public string MnfSerial { get; set; }
        /// <summary>
        /// 设备所用零件列表
        /// </summary>

        public List<MaterialDetails> MaterialDetailList { get; set; }
    }

    public class MaterialDetails
    {
        /// <summary>
        /// 零件编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 库存量
        /// </summary>

        public decimal? OnHand { get; set; }
        /// <summary>
        /// 仓库
        /// </summary>

        public string WhsCode { get; set; }
        /// <summary>
        /// 设备所用个数
        /// </summary>

        public decimal? Quantity { get; set; }
    }
}
