using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace OpenAuth.App.Order.ModelDto
{
    public class ShopProductDto
    {
    
        /// <summary>
        /// 商品Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// code
        /// </summary>
        public string NSAPCode { get; set; }
        /// <summary>
        /// 商品卖点
        /// </summary>
        public string SellingPoints { get; set; }
        /// <summary>
        /// 主图
        /// </summary>
        public string MainImgUrl { get; set; }
        /// <summary>
        /// 详情图
        /// </summary>
        public List<ImageUrl> DetailsImg { get; set; }
        /// <summary>
        /// 分享链接
        /// </summary>
        public string ShareUrl { get; set; }
        /// <summary>
        /// 商品规格文件名称
        /// </summary>
        public string SpecsFileName { get; set; }
        /// <summary>
        /// 商品规格文件地址
        /// </summary>
        public string SpecsFileUrl { get; set; }

    }
    /// <summary>
    /// 商品图片
    /// </summary>
    public class ImageUrl
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonIgnoreAttribute]
        public int? Id { get; set; }
        /// <summary>
        /// 图片排序
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 图片类型 0：首图 ,1:轮播图,2:详情图,3：分享图
        /// </summary>
        [JsonIgnoreAttribute]
        public int Type { get; set; }
        /// <summary>
        /// 商品图片
        /// </summary>
        public string ImgeUrl { get; set; }
        /// <summary>
        /// 枚举类型
        /// </summary>
        [JsonIgnoreAttribute]
        public ProductImageEnum ProductImageEnum { get; set; }
    }
    /// <summary>
    /// 图片类型
    /// </summary>
    public enum ProductImageEnum
    {
        //图片类型 0：首图 ,1:轮播图,2:详情图,3：分享图
        /// <summary>
        /// 首图
        /// </summary>
        [Description("首图")]
        Main = 0,
        /// <summary>
        /// 轮播图
        /// </summary>
        [Description("轮播图")]
        Rotation = 1,
        /// <summary>
        /// 详情图
        /// </summary>
        [Description("详情图")]
        Details = 2,
        /// <summary>
        /// 分享图
        /// </summary>
        [Description("分享图")]
        Share = 3,
    }
}
