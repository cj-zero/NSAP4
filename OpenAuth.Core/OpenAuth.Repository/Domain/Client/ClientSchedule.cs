/*
 * @author : wangying
 * @date : 2022-8-15
 * @desc : 客户日程
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
    /// 客户规则
    /// </summary>
    [Table("client_schedule")]
    public class ClientSchedule : BaseEntity<int>
    {
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 业务员编码
        /// </summary>
        public int SlpCode { get; set; }

        /// <summary>
        /// 业务员名称
        /// </summary>
        public string SlpName { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// 参与者
        /// </summary>
        public string Participants { get; set; }
        /// <summary>
        /// 提醒时间
        /// </summary>
        public DateTime? RemindTime { get; set; }
        /// <summary>
        /// 类型（0 客户 1 报价单 2 销售订单）
        /// </summary>
        public int ScheduleType { get; set; }
        /// <summary>
        /// 类型备注（客户，报价单号 销售订单号）
        /// </summary>
        public string ScheduleRemark { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Remark { get; set; }
        
        /// <summary>
        /// 创建人
        /// </summary>
        public int CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateDate { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>
        public int UpdateUser { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateDate { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        [Description("是否删除")]
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
