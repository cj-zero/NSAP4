﻿using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class MaterialRecordReq
    {

        /// <summary>
        /// 物料代码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string DocEntry { get; set; }

        /// <summary>
        /// 客户代码
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// 仓库
        /// </summary>
        public string WhsCode { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        public int pageIndex { get; set; }
        public int pageSize { get; set; }
    }

    public class SubmitItemCode
    {
        /// <summary>
        /// 配置类型
        /// </summary>
        public string U_ZS { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
    }


    public class AutoSubmitItemCode
    {
        /// <summary>
        /// 配置类型
        /// </summary>
        public string U_ZS { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }

        public int LineNum { get; set; }
    }

    public class UpdateManageScreen
    {
        public int id { get; set; }
        public string url { get; set; }
        public string VersionNo { get; set; }
        public DateTime? UrlUpdate { get; set; }

        public string IsDemo { get; set; }
        public DateTime? DemoUpdate { get; set; }
    }

    public class UpdatePManageScreen
    {
        /// <summary>
        /// 单据编码
        /// </summary>
        public string DocEntry { get; set; }
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 业务员名称
        /// </summary>
        public string SlpName { get; set; }

        /// <summary>
        /// 图纸编码
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 生产订单号
        /// </summary>
        public string ProduceNo { get; set; }

        public string IsDemo { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string VersionNo { get; set; }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FileUrl { get; set; }
    }
}
