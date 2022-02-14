using Microsoft.AspNetCore.Http;
using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    /// <summary>
    /// 新增或修改中位机版本记录
    /// </summary>
    public class AddOrUpdateZWJSoftwareInfoReq
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// 是否是默认版本
        /// </summary>
        public bool DefaultVersion { get; set; }

        /// <summary>
        /// 程序版本号
        /// </summary>
        public string ZWJSoftwareVersionName { get; set; }

        /// <summary>
        /// 上传文件
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
        /// 可适配的中位机硬件版型号
        /// </summary>
        public List<string>? ZWJSns { get; set; } 
    }

    /// <summary>
    /// 查询中位机软件版本记录
    /// </summary>
    public class QueryZWJSoftwareListReq : PageReq
    {
        /// <summary>
        /// ZWJ程序版本
        /// </summary>
        public string ZWJSoftwareVersionName { get; set; }

        /// <summary>
        /// ZWJ硬件版型号
        /// </summary>
        public string ZWJSn { get; set; }
    }


    /// <summary>
    /// 新增或修改下位机版本记录
    /// </summary>
    public class AddOrUpdateXWJSoftwareInfoReq
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 下位机程序版本别名
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 下位机程序版本
        /// </summary>
        public string XWJSoftwareVersionName { get; set; }

        /// <summary>
        /// 程序文件
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
    }

    /// <summary>
    /// 查询下位机软件版本记录
    /// </summary>
    public class QueryXWJSoftwareListReq : PageReq
    {
        /// <summary>
        /// 下位机程序版本别名
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 下位机程序版本
        /// </summary>
        public string XWJSoftwareVersionName { get; set; }
    }


    /// <summary>
    /// 新增下位机版本映射
    /// </summary>
    public class AddOrUpdateXWJMapReq
    {
        /// <summary>
        /// 下位机硬件版型号
        /// </summary>
        public string XWJSn { get; set; }

        /// <summary>
        /// 下位机中文程序版本别名
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 下位机英文程序版本别名
        /// </summary>
        public string AliasEn { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }
    }

    /// <summary>
    /// 查询下位机版本映射
    /// </summary>
    public class QueryXWJHarewareListReq : PageReq
    {
        /// <summary>
        /// 下位机硬件版型号
        /// </summary>
        public string XWJSn { get; set; }

        /// <summary>
        /// 下位机程序版本
        /// </summary>
        public string XWJSoftwareVersionName { get; set; }

        /// <summary>
        /// 下位机软件版本别名
        /// </summary>
        public string Alias { get; set; }
    }

    /// <summary>
    /// 新增或修改临时版本记录
    /// </summary>
    public class AddOrUpdateTempVersionReq
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 销售合同号
        /// </summary>
        public string ContractNo { get; set; }

        /// <summary>
        /// 硬件类型
        /// </summary>
        public string HardwareType { get; set; }

        /// <summary>
        /// 程序版本Id
        /// </summary>
        public int SoftwareVersionId { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }

    /// <summary>
    /// 查询版本记录
    /// </summary>
    public class QueryTempVersionReq : PageReq
    {
        public string ContractNo { get; set; }
    }
}
