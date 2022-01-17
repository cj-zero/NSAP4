using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain.Material
{
    /// <summary>
    /// 中位机程序版本
    /// </summary>
    [Table("ZWJSoftwareVersion")]
    public class ZWJSoftwareVersion : BaseEntity
    {
        /// <summary>
        /// 自增Id
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// 中位机程序版本
        /// </summary> 
        public string ZWJSoftwareVersionName { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }


        public virtual List<ZWJHardware> ZWJHardwares { get; set; }
    }

    /// <summary>
    /// 中位机硬件
    /// </summary>
    [Table("ZWJHardware")]
    public class ZWJHardware : BaseEntity
    {
        /// <summary>
        /// 自增Id
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 中位机程序版本Id
        /// </summary>
        public int ZWJSoftwareVersionId { get; set; }

        public virtual ZWJSoftwareVersion ZWJSoftwareVersion { get; set; }

        /// <summary>
        /// 中位机硬件版序列号
        /// </summary>
        public string ZWJSn { get; set; }
    }

    /// <summary>
    /// 下位机程序版本
    /// </summary>
    [Table("XWJSoftwareVersion")]
    public class XWJSoftwareVersion : BaseEntity
    {
        /// <summary>
        /// 自增Id
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 下位机程序版本
        /// </summary>
        public string XWJSoftwareVersionName { get; set; }

        /// <summary>
        /// 下位机程序版本别名
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }

    /// <summary>
    /// 下位机硬件
    /// </summary>
    [Table("XWJHardware")]
    public class XWJHardware : BaseEntity
    {
        /// <summary>
        /// 自增Id
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 下位机硬件版型号
        /// </summary>
        public string XWJSn { get; set; }

        /// <summary>
        /// 下位机程序版本别名
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}
