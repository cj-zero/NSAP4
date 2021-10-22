using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    [AutoMapTo(typeof(InternalContactBatchNumber))]
    public class InternalContactBatchNumberReq
    {
        /// <summary>
        /// 
        /// </summary>
        public int InternalContactId { get; set; }
        /// <summary>
        /// 批次号
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 起始SN
        /// </summary>
        public string StartSN { get; set; }
        /// <summary>
        /// 结束SN
        /// </summary>
        public string EndSN { get; set; }
    }
}
