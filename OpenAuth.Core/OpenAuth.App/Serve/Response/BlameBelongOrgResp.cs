using AutoMapper.Configuration.Annotations;
using Infrastructure.AutoMapper;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    [AutoMapTo(typeof(BlameBelongOrg))]
    public class BlameBelongOrgResp
    {
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? BlameBelongId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? OrgIdea { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        
        public string OrgId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string OrgName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Manager { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        
        public string ManagerId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Amount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        
        public int? HrIdea { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public bool? IsHistory { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string TransferOrg { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        
        public string TransferOrgId { get; set; }

        public int AppealTime { get; set; }

        /// <summary>
        /// 图片文件列表
        /// </summary>
        [Ignore]
        public virtual List<UploadFileResp> Files { get; set; }
    }
}
