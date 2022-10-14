/*
 * @author : Eaven
 * @date : 2022-4-20
 * @desc : 客户规则
 */
using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// LIMS推广员联系人维护表
    /// </summary>
    [Table("client_lims_ocpr")]
    public class LimsOCPR : BaseEntity<int>
    {
        /// <summary>
        /// 联合主键
        /// </summary>
        public int CntctCode { get; set; }
        public int sbo_id { get; set; }
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 业务员编码
        /// </summary>
        public int SlpCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 职位
        /// </summary>
        public string Position { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 电话1
        /// </summary>
        public string Tel1 { get; set; }
        /// <summary>
        /// 电话2
        /// </summary>
        public string Tel2 { get; set; }
        public string Cellolar { get; set; }
        public string Fax { get; set; }
        public string E_MailL { get; set; }
        public string Pager { get; set; }
        public string Notes1 { get; set; }
        public string Notes2 { get; set; }
        public string DataSource { get; set; }
        public int UserSign { get; set; }
        public string Password { get; set; }
        public int LogInstanc { get; set; }
        public string ObjType { get; set; }
        public string BirthPlace { get; set; }
        public DateTime? BirthDate { get; set; }
        /// <summary>
        /// M代表男  F代表女  E  默认值为E
        /// </summary>
        public string Gender { get; set; }
        public string Profession { get; set; }
        public DateTime? updateDate { get; set; }
        public int? updateTime { get; set; }
        public string Title { get; set; }
        public string BirthCity { get; set; }
        /// <summary>
        /// Y代表Yes是   N代表No否   默认值为Y
        /// </summary>
        public string Active { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string U_ACCT { get; set; }
        public string U_BANK { get; set; }
        public string U_PRX_SID { get; set; }
        public string U_PRX_Pwd { get; set; }
        /// <summary>
        /// 草稿或提交
        /// </summary>
        public string Type { get; set; }

        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
