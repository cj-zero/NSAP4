using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain.Serve;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    /// <summary>
    /// 
    /// </summary>
    [Table("ChangeTheMaterial")]
    [AutoMapTo(typeof(ChangeTheMaterial))]
    public class ChangeTheMaterialReq
    {
        /// <summary>
        /// 完工报告Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 完工报告Id
        /// </summary>
        public string CompletionReportId { get; set; }
        /// <summary>
        /// 物料
        /// </summary>
        public string Material { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int? Count { get; set; }
    }
}
