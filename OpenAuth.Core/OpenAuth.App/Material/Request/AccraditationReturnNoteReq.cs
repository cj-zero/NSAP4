using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public class AccraditationReturnNoteReq
    {
        /// <summary>
        /// Id
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 是否驳回
        /// </summary>
        public bool IsReject { get; set; }

        public List<ReturnnoteMaterials> returnnoteMaterials { get; set; }
    }
    public class ReturnnoteMaterials
    {
        /// <summary>
        /// 物料id
        /// </summary>
        public string MaterialsId { get; set; }
        /// <summary>
        /// 良品数量
        /// </summary>
        public bool IsGood { get; set; }
        /// <summary>
        ///良品仓库
        /// </summary>
        public string GoodWhsCode { get; set; }

        /// <summary>
        ///次品仓库
        /// </summary>
        public string SecondWhsCode { get; set; }
    }
}
