using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain.Material;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{

    [AutoMapTo(typeof(CommonUsedMaterial))]
    public class AddCommonUsedMaterialReq
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string MaterialDescription { get; set; }
    }
}
