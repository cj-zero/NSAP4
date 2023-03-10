using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 
    /// </summary>
    [Table("devicebindmap")]
    public partial class DeviceBindMap : BaseEntity<long>
    {
        /// <summary>
        /// 生产码
        /// </summary>
        public string GeneratorCode { get; set; }
        /// <summary>
        /// 中位机guid
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// 上位机ip
        /// </summary>
        public string BtsServerIp { get; set; }
        /// <summary>
        /// 边缘计算guid
        /// </summary>
        public string EdgeGuid { get; set; }
        /// <summary>
        /// 上位机guid
        /// </summary>
        public string SrvGuid { get; set; }
        /// <summary>
        /// 中位机编号
        /// </summary>
        public int DevUid { get; set; }
        /// <summary>
        /// 下位机单元id
        /// </summary>
        public int UnitId { get; set; }
        /// <summary>
        /// 绑定时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 绑定操作人id
        /// </summary>
        public string CreateUserId { get; set; }
        /// <summary>
        /// 绑定操作人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 下位机guid
        /// </summary>
        public string LowGuid { get; set; }
        /// <summary>
        /// 绑定类型：1.中位机绑定 2.下位机绑定
        /// </summary>
        public int BindType { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        public string Department { get; set; }
        public long OrderNo { get; set; }
        public string RangeCurrArray { get; set; }
        public int LowNo { get; set; }
        /// <summary>
        /// 数据来源(0:人工绑定 1:设备同步自动绑定  2:扫码自动绑定)
        /// </summary>
        //public int DataSource { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public override void GenerateDefaultKeyVal()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
