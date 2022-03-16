using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.ModelDto
{
    /// <summary>
    /// 销售交货单打印
    /// </summary>
    public class PrintSalesDelivery
    {
        /// <summary>
        /// 销售报价单单号
        /// </summary>
        public string DocEntry { get; set; }
        /// <summary>
        /// 源单号
        /// </summary>
        public string BaseEntry { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public string CreateDate { get; set; }
        /// <summary>
        /// 销售
        /// </summary>
        public string SlpName { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string Memo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string NumAtCard { get; set; }
        /// <summary>
        /// 客户编号
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string CntactName { get; set; }
        /// <summary>
        /// 电话1
        /// </summary>
        public string Tel1 { get; set; }
        /// <summary>
        /// 电话2
        /// </summary>
        public string Tel2 { get; set; }
        /// <summary>
        /// 供方联系人电话
        /// </summary>
        public string Cellolar { get; set; }
        /// <summary>
        /// 传真
        /// </summary>
        public string Fax { get; set; }
    
        /// <summary>
        /// 客户地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 交货时间
        /// </summary>
        public string DeliveryDate { get; set; }
        /// <summary>
        /// 箱号                                                                                                    
        /// </summary>
        public string U_CPH { get; set; }
        /// <summary>
        /// 标识
        /// </summary>
        public string  indicator { get; set; }
        /// <summary>
        /// logo
        /// </summary>
        public string logo { get; set; }

        /// <summary>
        /// 二维码
        /// </summary>
        public string QRcode { get; set; }
      
        /// <summary>
        /// 物料信息
        /// </summary>
        public List<SalesDeliveryDeatils> SalesDeliveryDeatils { get; set; }


    }

    /// <summary>
    /// 物料信息
    /// </summary>
    public class SalesDeliveryDeatils
    {
        /// <summary>
        /// 产品编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Dscription { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public string Quantity { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string unitMsr { get; set; }
        /// <summary>
        /// 仓库编码
        /// </summary>
        public string WhsCode { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public string Price { get; set; }
        /// <summary>
        /// 源单号
        /// </summary>
        public string BaseEntry { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public string Total { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Currency { get; set; }
    }
}
