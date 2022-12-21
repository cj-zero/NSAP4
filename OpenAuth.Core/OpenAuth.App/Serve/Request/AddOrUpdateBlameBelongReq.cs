using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    [AutoMapTo(typeof(BlameBelong))]
    public class AddOrUpdateBlameBelongReq
    {
        public AddOrUpdateBlameBelongReq()
        {
            this.Source = 1;
            this.SerialNumber = string.Empty;
        }
        public int? Id { get; set; }
        public int? AppUserId { get; set; }
        /// <summary>
        /// 归属部门
        /// </summary>
        public string VestinOrg { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public int? DocType { get; set; }
        /// <summary>
        /// 责任依据
        /// </summary>
        public string Basis { get; set; }
        /// <summary>
        /// 单据单号
        /// </summary>
        public int? OrderNo { get; set; }
        /// <summary>
        /// 销售单号
        /// </summary>
        public int? SaleOrderId { get; set; }
        /// <summary>
        /// 预估影响金额
        /// </summary>
        public decimal AffectMoney { get; set; }
        /// <summary>
        /// 详情描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        public List<string> FileIds { get; set; } = new List<string>();
        /// <summary>
        /// 是否草稿
        /// </summary>
        public bool IsDraft { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// 来源 1-erp 2-app
        /// </summary>
        public int? Source { get; set; }

        /// <summary>
        /// 预估金额说明
        /// </summary>
        public string AffectRemark { get; set; }

        /// <summary>
        /// 预估工时
        /// </summary>
        public string AffectHour { get; set; }

        /// <summary>
        /// 预估责任人
        /// </summary>
        public string AffectUser { get; set; }
        /// <summary>
        /// 预估责任人
        /// </summary>
        public string AffectUserName { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string cardCode { get; set; }


        /// <summary>
        /// 客户名称
        /// </summary>
        public string cardName { get; set; }

        /// <summary>
        /// 生产部门
        /// </summary>
        public string ProductOrg { get; set; }
        
        /// <summary>
        /// 归属部门
        /// </summary>
        // public virtual List<AddBlameBelongOrgReq> OrgInfos { get; set; }
    }

    public class AddBlameBelongOrgReq
    {
        public string OrgId { get; set; }
        public string OrgName { get; set; }
    }
}
