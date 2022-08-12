using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 生产进度
    /// </summary>
    [Table("productionschedule")]
    public class ProductionSchedule : Entity
    {
        /// <summary>
        /// 生产单号
        /// </summary>
        public int? DocEntry { get; set; }
        /// <summary>
        /// 生产唯一码
        /// </summary>
        public string GeneratorCode { get; set; }
        /// <summary>
        /// 生产状态 1 未完成 2 完成
        /// </summary>
        public int? ProductionStatus { get; set; }
        /// <summary>
        /// 烤机状态 1-待烤机 2-烤机中 3-烤机通过 4-异常
        /// </summary>
        public int? DeviceStatus { get; set; }
        /// <summary>
        /// 烤机操作人
        /// </summary>
        public string DeviceOperator { get; set; }
        public string DeviceOperatorId { get; set; }
        /// <summary>
        /// 烤机完成时间
        /// </summary>
        public DateTime? DeviceTime { get; set; }
        /// <summary>
        /// 校准状态 1-待校准 2-通过
        /// </summary>
        public int? NwcailStatus { get; set; }
        /// <summary>
        /// 校准操作人
        /// </summary>
        public string NwcailOperator { get; set; }
        public string NwcailOperatorId { get; set; }
        /// <summary>
        /// 校准完成时间
        /// </summary>
        public DateTime? NwcailTime { get; set; }
        /// <summary>
        /// 收货状态 1未收货 2 已收货
        /// </summary>
        public int? ReceiveStatus { get; set; }
        /// <summary>
        /// 收货操作人
        /// </summary>
        public string ReceiveOperator { get; set; }
        public string ReceiveOperatorId { get; set; }
        /// <summary>
        /// 收货完成时间
        /// </summary>
        public DateTime? ReceiveTime { get; set; }
        /// <summary>
        /// 库位
        /// </summary>
        public string ReceiveLocation { get; set; }
        /// <summary>
        /// 排序号
        /// </summary>
        public int? SortNo { get; set; }
        /// <summary>
        /// 收货单号
        /// </summary>
        public int? ReceiveNo { get; set; }
    }
}
