using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public class ReturnNoteAuditReq
    {
        /// <summary>
        /// 物流单Id
        /// </summary>
        public string ExpressageId { get; set; }

        /// <summary>
        /// 退料单Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 签收备注
        /// </summary>
        public string Remark { get; set; }

        public List<ReturnMaterial> ReturnMaterials { get; set; }
    }

    public class ReturnMaterial
    {
        public string Id { get; set; }
        /// <summary>
        /// 是否通过 2未通过 1通过
        /// </summary>
        public int IsPass { get; set; }

        /// <summary>
        /// 收货备注
        /// </summary>
        public string ReceiveRemark { get; set; }
    }
}
