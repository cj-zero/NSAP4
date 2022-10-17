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
    /// LIMS推广员地址维护表
    /// </summary>
    [Table("client_lims_crd1")]
    public class LimsCRD1 : BaseEntity<int>
    {
        /// <summary>
        /// 账套id
        /// </summary>
        public int sbo_id { get; set; }
        /// <summary>
        /// 业务伙伴地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 业务伙伴代码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 业务员编码
        /// </summary>
        public int SlpCode { get; set; }
        /// <summary>
        /// 邮政编码
        /// </summary>
        public string ZipCode { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 地区
        /// </summary>
        public string County { get; set; }
        /// <summary>
        /// 国家
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// 省
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 日志实例
        /// </summary>
        public int LogInstanc { get; set; }
        /// <summary>
        /// 对象类型
        /// </summary>
        public string ObjType { get; set; }
        /// <summary>
        /// 国税编号
        /// </summary>
        public string LicTradNum { get; set; }
        /// <summary>
        /// 行编号
        /// </summary>
        public int LineNum { get; set; }
        /// <summary>
        /// 税代码
        /// </summary>
        public string TaxCode { get; set; }
        /// <summary>
        /// 大楼/楼层/房间
        /// </summary>
        public string Building { get; set; }
        /// <summary>
        /// 地址类型
        /// </summary>
        public string AdresType { get; set; }
        /// <summary>
        /// 地址2
        /// </summary>
        public string Address2 { get; set; }
        /// <summary>
        /// 地址3
        /// </summary>
        public string Address3 { get; set; }
        /// <summary>
        /// 可用    Y代表Yes是,N代表No否,默认值为Y
        /// </summary>
        public string U_Active { get; set; }
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
