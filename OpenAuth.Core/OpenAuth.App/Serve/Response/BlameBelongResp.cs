using AutoMapper.Configuration.Annotations;
using Infrastructure.AutoMapper;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    [AutoMapTo(typeof(BlameBelong))]
    public class BlameBelongResp
    {
        public int? Id { get; set; }
        /// <summary>
        /// 
        /// </summary>

        public string VestinOrg { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? DocType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Basis { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? OrderNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? AffectMoney { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? SaleOrderId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ProductionOrg { get; set; }
        /// <summary>
        /// 1-撤回 2-责任部门审核 3-人事审核 4-责任金额判断 5-出纳 6-结束
        /// </summary>
        
        public int? Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CardCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CardName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FlowInstanceId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreateUserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CreateUser { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? CreateTime { get; set; }
        public string SerialNumber { get; set; }
        public virtual List<BlameBelongOrgResp> BlameBelongOrgs { get; set; }
        /// <summary>
        /// 图片文件列表
        /// </summary>
        [Ignore]
        public virtual List<UploadFileResp> Files { get; set; }

        public List<FlowPathResp> FlowPathResp { get; set; }
    }
}
