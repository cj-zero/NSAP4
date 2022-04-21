/*
 * @author : Eaven
 * @date : 2022-4-20
 * @desc : 公海设置
 */
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
    /// 公海设置
    /// </summary>
    [Table("customer_sea_conf")]
    public class CustomerSeaConf : BaseEntity<int>
    {
        #region 启用自动放入公海
        /// <summary>
        /// 放入时间
        /// </summary>
        [MaxLength(20)]
        [Column("Put_Time")]
        [Description("放入时间")]
        public string PutTime { get; set; }

        /// <summary>
        /// 通知时间
        /// </summary>
        [MaxLength(20)]
        [Column("Notify_Time")]
        [Description("通知时间")]
        public string NotifyTime { get; set; }

        /// <summary>
        /// 提前通知天数
        /// </summary>
        [Column("Notify_Day")]
        [Description("提前通知天数")]
        public int NotifyDay { get; set; }

        /// <summary>
        /// 规则说明
        /// </summary>
        [MaxLength(500)]
        [Column("Notify_Rule_Explain")]
        [Description("通知规则说明")]
        public string NotifyRuleExplain { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Description("是否启用")]
        public bool Enable { get; set; }

        #endregion

        /// <summary>
        /// 创建人
        /// </summary>
        [Column("Create_User")]
        [Required(ErrorMessage = "创建人不能为空")]
        [MaxLength(20)]
        [Description("创建人")]
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("Create_DateTime")]
        [Description("创建时间")]
        public DateTime CreateDatetime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        [Column("Update_User")]
        [MaxLength(20)]
        [Description("更新人")]
        public string UpdateUser { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("Update_DateTime")]
        [Description("更新时间")]
        public DateTime? UpdateDatetime { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        [Description("是否删除")]
        public bool Isdelete { get; set; }

        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
    /// <summary>
    /// 公海回收机制
    /// </summary>
    public class CustomerSeaRecover : CustomerSeaConf
    {
        #region 公海回收机制
        /// <summary>
        /// 未报价天数;
        /// </summary>
        [Column("Recover_No_Price")]
        [Description("未报价天数")]
        public int RecoverNoPrice { get; set; }

        /// <summary>
        /// 未成交天数;
        /// </summary>
        [Column("Recover_No_Order")]
        [Description("未成交天数")]
        public int RecoverNoOrder { get; set; }

        /// <summary>
        /// 是否启用;公海回收机制
        /// </summary>
        [Column("Recover_Enable")]
        [Description("是否启用")]
        public bool RecoverEnable { get; set; }

        /// <summary>
        /// 公海回收机制规则说明
        /// </summary>
        [MaxLength(500)]
        [Column("Recover_Rule_Explain")]
        [Description("公海回收机制规则说明")]
        public string RecoverRuleExplain { get; set; }
        #endregion
    }

    /// <summary>
    /// 业务员公海认领分配规则
    /// </summary>
    public class CustomerSeaReceive : CustomerSeaConf
    {
        #region  业务员公海认领分配规则

        /// <summary>
        /// 每天最多认领数量;
        /// </summary>
        [Column("Receive_Max_Limit")]
        [Description("领取最大数量限制")]
        public int ReceiveMaxLimit { get; set; }

        /// <summary>
        /// 业务员入职最大时间
        /// </summary>
        [Column("Receive_Job_Max")]
        [Description("业务员入职最大时间")]
        public int ReceiveJobMax { get; set; }

        /// <summary>
        /// 业务员入职最小时间
        /// </summary>
        [Column("Receive_Job_Min")]
        [Description("业务员入职最小时间")]
        public int ReceiveJobMin { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("Receive_Enable")]
        [Description("是否启用")]
        public bool ReceiveEnable { get; set; }

        /// <summary>
        /// 认领分配规则规则说明
        /// </summary>
        [MaxLength(500)]
        [Column("Receive_Rule_Explain")]
        [Description("公海回收机制规则说明")]
        public string ReceiveRuleExplain { get; set; }
        #endregion
    }

    /// <summary>
    /// 主动掉入公海限制
    /// </summary>
    public class CustomerSeaAutomatic : CustomerSeaConf
    {
        #region 主动掉入公海限制
        /// <summary>
        /// 领取后天数限制
        /// </summary>
        [Column("Automatic_Day_Limit")]
        [Description("公海领取后天数限制")]
        public int AutomaticDayLimit { get; set; }

        /// <summary>
        /// 领取后次数限制
        /// </summary>
        [Column("Automatic_Limit")]
        [Description("公海领取后次数限制")]
        public int AutomaticLimit { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("Automatic_Enable")]
        [Description("是否启用主动掉入公海限制")]
        public bool AutomaticEnable { get; set; }

        /// <summary>
        ///  主动掉入公海规则说明
        /// </summary>
        [MaxLength(500)]
        [Column("Automatic_Rule_Explain")]
        [Description("掉入公海规则说明")]
        public string AutomaticRuleExplain { get; set; }
        #endregion
    }

    /// <summary>
    /// 掉入公海后抢回限制
    /// </summary>
    public class CustomerSeaBack : CustomerSeaConf
    {
        #region 掉入公海后抢回限制
        /// <summary>
        /// 掉入公海后抢回限制
        /// </summary>
        [Column("Back_Day")]
        [Description("掉入公海后抢回限制")]
        public int BackDay { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("Back_Enable")]
        [Description("是否启用掉入公海后抢回限制")]
        public bool BackEnable { get; set; }
        /// <summary>
        ///  抢回限制规则说明
        /// </summary>
        [MaxLength(500)]
        [Column("Back_Rule_Explain")]
        [Description("抢回限制规则说明")]
        public string BackRuleExplain { get; set; }
        #endregion
    }
}
