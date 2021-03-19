using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public class CheckOutMaterialsReq
    {
        /// <summary>
        /// 退料单Id
        /// </summary>
        public int ReturnNoteId { get; set; }

        /// <summary>
        /// 物流Id
        /// </summary>
        public string ExpressageId { get; set; }

        /// <summary>
        /// 品质检验物料明细
        /// </summary>
        public List<CheckOutMaterial> checkOutMaterials { get; set; }

        /// <summary>
        /// 物料明细Id集合
        /// </summary>
        public List<string> DetailIds { get; set; }
    }

    public class CheckOutMaterial
    {
        /// <summary>
        /// 退料明细Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 良品数量
        /// </summary>
        public int GoodQty { get; set; }

        /// <summary>
        /// 次品数量
        /// </summary>
        public int SecondQty { get; set; }
    }
}
