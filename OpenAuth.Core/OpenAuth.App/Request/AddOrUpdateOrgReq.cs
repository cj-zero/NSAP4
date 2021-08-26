using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure.AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenAuth.App.Request
{
    [Table("Org")]
    [AutoMapTo(typeof(OpenAuth.Repository.Domain.Org))]
    public partial class AddOrUpdateOrgReq
    {
        public AddOrUpdateOrgReq()
        {
            this.CascadeId = string.Empty;
            this.Name = string.Empty;
            this.HotKey = string.Empty;
            this.ParentName = string.Empty;
            this.IconName = string.Empty;
            this.Status = 0;
            this.BizCode = string.Empty;
            this.CustomCode = string.Empty;
            this.CreateTime = DateTime.Now;
            this.CreateId = 0;
            this.SortNo = 0;
            this.ParentId = null;
            this.TypeName = string.Empty;
            this.TypeId = string.Empty;
        }
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 节点语义ID
        /// </summary>
        public string CascadeId { get; set; }
        /// <summary>
        /// 功能模块名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 父节点流水号
        /// </summary>
        public string ParentId { get; set; }
        /// <summary>
        /// 父节点名称
        /// </summary>
        public string ParentName { get; set; }
        /// <summary>
        /// 热键
        /// </summary>
        public string HotKey { get; set; }

        /// <summary>
        /// 是否叶子节点
        /// </summary>
        public bool IsLeaf { get; set; }
        /// <summary>
        /// 是否自动展开
        /// </summary>
        public bool IsAutoExpand { get; set; }
        /// <summary>
        /// 节点图标文件名称
        /// </summary>
        public string IconName { get; set; }
        /// <summary>
        /// 当前状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 业务对照码
        /// </summary>
        public string BizCode { get; set; }
        /// <summary>
        /// 自定义扩展码
        /// </summary>
        public string CustomCode { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime CreateTime { get; set; }
        /// <summary>
        /// 创建人ID
        /// </summary>
        public int CreateId { get; set; }
        /// <summary>
        /// 排序号
        /// </summary>
        public int SortNo { get; set; }
        /// <summary>
        /// 分类名称
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// 分类ID
        /// </summary>
        public string TypeId { get; set; }
        /// <summary>
        /// 公司Id
        /// </summary>
        public string CorpId { get; set; }
        /// <summary>
        /// 部门负责人
        /// </summary>
        public List<string> DeptManager { get; set; }
    }
}
