using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Response
{
    public class ReturnMaterialDetailResp
    {
        /// <summary>
        /// 物料Id
        /// </summary>
        public string MaterialId { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 物料描述
        /// </summary>
        public string MaterialDescription { get; set; }

        /// <summary>
        /// 本次退还数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 需退总计
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 核对收货 1通过 0未通过
        /// </summary>
        public int Check { get; set; }

        /// <summary>
        /// 收货备注
        /// </summary>
        public string ReceivingRemark { get; set; }

        /// <summary>
        /// 发货备注
        /// </summary>
        public string ShippingRemark { get; set; }

        /// <summary>
        /// 物流单Id
        /// </summary>
        public string ExpressId { get; set; }

        /// <summary>
        /// 图片Id
        /// </summary>
        public string PictureId { get; set; }

        /// <summary>
        /// 良品数量
        /// </summary>
        public int GoodQty { get; set; }

        /// <summary>
        /// 次品数量
        /// </summary>
        public int SecondQty { get; set; }

        /// <summary>
        /// 剩余需退数量
        /// </summary>
        public int SurplusQty { get; set; }

        /// <summary>
        /// 退料明细Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 良品是否入库
        /// </summary>
        public int IsGoodFinish { get; set; }

        /// <summary>
        /// 次品是否入库
        /// </summary>
        public int IsSecondFinish { get; set; }
    }
}
