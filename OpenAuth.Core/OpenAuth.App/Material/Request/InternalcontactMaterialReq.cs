using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    /// <summary>
    /// 联络单物料
    /// </summary>
    [AutoMapTo(typeof(InternalContactMaterial))]
    public class InternalContactMaterialReq
    {
        /// <summary>
        /// 联络单ID
        /// </summary>
        public int InternalContactId { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 仓库
        /// </summary>
        public string WhsCode { get; set; }
        /// <summary>
        /// 存货量
        /// </summary>
        public int OnHand { get; set; }
    }
}
