using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    //视图，非数据库表格
    public partial class OSRIModel
    {
        public OSRIModel()
        {
            //this.BaseType = null;
        }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 序列号ID
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public string SuppSerial { get; set; }
        /// <summary>
        /// 仓库号
        /// </summary>
        //public string WhsCode { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal? Quantity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //public int? BaseType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? BaseEntry { get; set; }
        public int? DocEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? BaseLinNum { get; set; }
        public string WhsCode { get; set; }
        public int? SysSerial { get; set; }
        public int? Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //public string ItemName { get; set; }
        /// <summary>
        /// 
        /// </summary>
       // public int Status { get; set; }

        #region 非视图字段
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string Technician { get; set; }
        public string PartItemCode { get; set; }
        #endregion
    }
}
