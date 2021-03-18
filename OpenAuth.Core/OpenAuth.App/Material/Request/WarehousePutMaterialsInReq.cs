using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public class WarehousePutMaterialsInReq
    {
        /// <summary>
        /// 退料单Id
        /// </summary>
        public int ReturnNoteId { get; set; }

        /// <summary>
        /// 物流单Id
        /// </summary>
        public string ExpressageId { get; set; }

        public List<PutInMaterial> putInMaterials { get; set; }
    }
    public class PutInMaterial
    {
        /// <summary>
        /// 退料明细Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 物料Id
        /// </summary>
        public string MaterialId { get; set; }

        /// <summary>
        /// 入库类型 37良品仓 39次品仓
        /// </summary>
        public int WhsCode { get; set; }

        /// <summary>
        /// 入库数量
        /// </summary>
        public int Qty { get; set; }
    }
}
