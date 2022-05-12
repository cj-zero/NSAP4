using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Customer
{
    /// <summary>
    /// 客户列表
    /// </summary>
    [Table("customer_list")]
    public class CustomerList: BaseEntity<int>
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 所属部门,如果为all则属于公司全体
        /// </summary>
        public string DepartMent { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        [Column("Customer_No")]
        public string CustomerNo { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        [Column("Customer_Name")]
        [Required(ErrorMessage = "客户名称不能为空")]
        [MaxLength(255)]
        [Description("客户名称")]
        public string CustomerName { get; set; }

        /// <summary>
        /// 客户来源
        /// </summary>
        public string CustomerSource { get; set; }

        /// <summary>
        /// 客户创建时间
        /// </summary>
        public DateTime? CustomerCreateDate { get; set; }

        /// <summary>
        /// 销售员在3.0的编码
        /// </summary>
        public int SlpCode { get; set; }

        /// <summary>
        /// 销售员名称
        /// </summary>
        [Column("Saler_Name")]
        public string SlpName { get; set; }

        /// <summary>
        /// 标签Id:3-已经掉入公海、4-即将掉入公海
        /// </summary>
        [Column("Label_Index")]
        public int LabelIndex { get; set; }

        /// <summary>
        /// 标签:3-已经掉入公海、4-即将掉入公海
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Column("Create_User")]
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("Create_DateTime")]
        public DateTime CreateDateTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [Column("Update_User")]
        public string UpdateUser { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [Column("Update_DateTime")]
        public DateTime UpdateDateTime { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }

        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
