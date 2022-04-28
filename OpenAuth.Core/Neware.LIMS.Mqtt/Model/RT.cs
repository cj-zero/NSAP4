using System;
using System.ComponentModel;

namespace Neware.LIMS.Mqtt
{
    /// <summary>
    /// ef和dapper实体类继承接口
    /// </summary>
    public interface IRT
    {
        /// <summary>
        /// 租户id
        /// </summary>
        int tenant_id { get; set; }

        /// <summary>
        /// 边缘计算唯一id
        /// </summary>
        string edge_guid { get; set; }

        /// <summary>
        /// 上位机guid
        /// </summary>
        string srv_guid { get; set; }

        /// <summary>
        /// 上位机ip
        /// </summary>
        string ip { get; set; }

        /// <summary>
        /// 中位机guid
        /// </summary>
        string mid_guid { get; set; }

        /// <summary>
        /// 中位机编号
        /// </summary>
        int dev_uid { get; set; }

        /// <summary>
        /// 单元编号
        /// </summary>
        int unit_id { get; set; }

        /// <summary>
        /// 通道编号
        /// </summary>
        int chl_id { get; set; }

        /// <summary>
        /// 辅助通道
        /// </summary>
        int aux_id { get; set; }

        /// <summary>
        /// V
        /// </summary>
        double volt { get; set; }

        /// <summary>
        /// mA
        /// </summary>
        double curr { get; set; }

        /// <summary>
        /// mAh充电容量
        /// </summary>
        double chg_cap { get; set; }

        /// <summary>
        /// mAh放电容量
        /// </summary>
        double dchg_cap { get; set; }

        /// <summary>
        /// mAh容量
        /// </summary>
        double cap { get; }

        /// <summary>
        /// mWh充电能量
        /// </summary>
        double chg_eng { get; set; }

        /// <summary>
        /// mWh放电能量
        /// </summary>
        double dchg_eng { get; set; }

        /// <summary>
        /// mWh能量
        /// </summary>
        double eng { get; }

        /// <summary>
        /// 工步时间
        /// </summary>
        ulong step_time { get; set; }

        /// <summary>
        /// 状态信息
        /// </summary>
        uint work_type { get; set; }

        /// <summary>
        /// 工步类型
        /// </summary>
        uint step_type { get; set; }

        /// <summary>
        /// 保护代码
        /// </summary>
        uint prt_code { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        long update_time { get; set; }

        /// <summary>
        /// 首次记录完成状态的时间
        /// </summary>
        DateTime? completion_time { get; set; }

        /// <summary>
        /// 当前通道状态
        /// </summary>
        ChannelState channel_state { get; set; }

        /// <summary>
        /// 测试id
        /// </summary>
        uint test_id { get; set; }

        /// <summary>
        /// 循环ID
        /// </summary>
		uint cycle_id { get; set; }

        /// <summary>
        /// 工步ID
        /// </summary>
		uint step_id { get; set; }

        /// <summary>
        /// 温度（已乘10）
        /// </summary>
		int temp { get; set; }

        /// <summary>
        /// 通道是否已离线
        /// </summary>
        bool is_off_line { get; }

        /// <summary>
        /// 当前量程参数
        /// </summary>
        int cur_step_range { get; set; }
    }

    /// <summary>
    /// rt数据(通道唯一值判断：上位机guid,新威设备id,单元id,通道id)
    /// </summary>

    public class BasicRT : IRT
    {
        public int tenant_id { get; set; }
        public string edge_guid { get; set; }
        public string srv_guid { get; set; }
        public string ip { get; set; }
        public string mid_guid { get; set; }
        public int dev_uid { get; set; }
        public int unit_id { get; set; }
        public int chl_id { get; set; }
        public double volt { get; set; }
        public double curr { get; set; }
        public double chg_cap { get; set; }
        public double dchg_cap { get; set; }
        public double cap { get { return chg_cap + dchg_cap; } }
        public double chg_eng { get; set; }
        public double dchg_eng { get; set; }
        public double eng { get { return chg_eng + dchg_eng; } }
        public ulong step_time { get; set; }
        public uint work_type { get; set; }
        public uint step_type { get; set; }
        public long update_time { get; set; }
        public DateTime? completion_time { get; set; }
        public ChannelState channel_state { get; set; }
        public uint test_id { get; set; }
        public uint cycle_id { get; set; }
        public uint step_id { get; set; }
        public int temp { get; set; }
        public uint prt_code { get; set; }
        public int aux_id { get; set; }
        public bool is_off_line { get => IsOffline(update_time); }
        public int cur_step_range { get; set; }
        public string barcode { get; set; }
        public string creator { get; set; }
        public DateTime beg_time { get; set; }
        public uint seq_id { get; set; }
        public int pyh_id { get; set; }
        public int bts_type { get; set; }

        /// <summary>
        /// 是否离线
        /// </summary>
        /// <param name="update_time"></param>
        /// <returns></returns>
        public static bool IsOffline(long update_time)
        {
            return (update_time + 60) <= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

    ///// <summary>
    ///// rt表0
    ///// </summary>
    //[Table("equip_rt0")]
    //public class RT0 : BasicRT
    //{
    //}

    ///// <summary>
    ///// rt表1
    ///// </summary>
    //[Table("equip_rt1")]
    //public class RT1 : BasicRT
    //{
    //}

    ///// <summary>
    ///// rt表0
    ///// </summary>
    //[Dapper.Table("equip_rt0")]
    //public class DapperRT0 : DapperBasicRT
    //{
    //}

    ///// <summary>
    ///// rt表1
    ///// </summary>
    //[Dapper.Table("equip_rt1")]
    //public class DapperRT1 : DapperBasicRT
    //{
    //}

    //public partial class RT0Configuration : IEntityTypeConfiguration<RT0>
    //{
    //    public void Configure(EntityTypeBuilder<RT0> builder)
    //    {
    //        builder.HasKey(x => new { x.mid_guid, x.unit_id, x.chl_id });
    //        builder.Property(x => x.edge_guid).HasMaxLength(36);
    //        builder.Property(x => x.mid_guid).HasMaxLength(36);
    //        builder.Property(x => x.ip).HasMaxLength(20);
    //        builder.Property(x => x.update_time).HasDefaultValue(new DateTime(2000, 1, 1));
    //    }
    //}
    //public partial class RT1Configuration : IEntityTypeConfiguration<RT1>
    //{
    //    public void Configure(EntityTypeBuilder<RT1> builder)
    //    {
    //        builder.HasKey(x => new { x.mid_guid, x.unit_id, x.chl_id });
    //        builder.Property(x => x.edge_guid).HasMaxLength(36);
    //        builder.Property(x => x.mid_guid).HasMaxLength(36);
    //        builder.Property(x => x.ip).HasMaxLength(20);
    //        builder.Property(x => x.update_time).HasDefaultValue(new DateTime(2000, 1, 1));
    //    }
    //}
    //BTS 工步类型
    public enum BTS_STEP_TYPE : short
    {
        StepTypeCount = 15,
        StepT_Undefined = 0,    //	未定义
        StepT_ChargeCurrent = 1,    //	恒流充电
        StepT_DischargeCurrent = 2, //	恒流放电
        StepT_ChargeVoltage = 3,    //	恒压充电
        StepT_Rest = 4, //	静置
        StepT_Cycle = 5,    //	循环
        StepT_Stop = 6, //	停止
        StepT_ChargeCurrentVoltage = 7, //	恒流恒压充电
        StepT_DischargePower = 8,   //	恒功率放电
        StepT_ChargePower = 9,  //	恒功率充电
        StepT_DischargeRes = 10,    //	恒阻放电
        StepT_ChargeRes = 11,   //	恒阻充电
        StepT_MeasureRes = 12,  //	测量内阻
        StepT_Suspend = 13, //	挂起工步
        StepT_Pulse = 16,   //	脉冲工步
        StepT_Sim = 17, //	模拟工步
        StepT_PCCCV = 18,   //	电池组 动态恒压放电工步
        StepT_DV = 19,  //	恒压放电
        StepT_DCDV = 20,	//	恒流恒压放电
    };

    /// <summary>
    /// 工步类型
    /// </summary>
    public enum StepType : uint
    {
        /// <summary>
        /// 未定义
        /// </summary>
        [Description("未定义")]
        Undefined = 0,

        /// <summary>
        /// 恒流充电
        /// </summary>
        [Description("恒流充电")]
        ChgCurr = 1,

        /// <summary>
        /// 恒流放电
        /// </summary>
        [Description("恒流放电")]
        DChgCurr = 2,

        /// <summary>
        /// 恒压充电
        /// </summary>
        [Description("恒压充电")]
        ChgVolt = 3,

        /// <summary>
        /// 搁置
        /// </summary>
        [Description("搁置")]
        Rest = 4,

        /// <summary>
        /// 循环
        /// </summary>
        [Description("循环")]
        Cycle = 5,

        /// <summary>
        /// 结束
        /// </summary>
        [Description("结束")]
        Stop = 6,

        /// <summary>
        /// 恒流恒压充电
        /// </summary>
        [Description("恒流恒压充电")]
        ChgCurrVolt = 7,

        /// <summary>
        /// 恒功率放电
        /// </summary>
        [Description("恒功率放电")]
        DChgPower = 8,

        /// <summary>
        /// 恒功率充电
        /// </summary>
        [Description("恒功率充电")]
        ChgPower = 9,

        /// <summary>
        /// 恒阻放电
        /// </summary>
        [Description("恒阻放电")]
        DChgRes = 10,

        /// <summary>
        /// 恒阻充电
        /// </summary>
        [Description("恒阻充电")]
        ChgRes = 11,

        /// <summary>
        /// 测量内阻
        /// </summary>
        [Description("测量内阻")]
        MeasureRes = 12,

        /// <summary>
        /// 暂停工步
        /// </summary>
        [Description("暂停工步")]
        Pause = 13,

        /// <summary>
        /// 闭环恒压充电工步
        /// </summary>
        [Description("闭环恒压充电工步")]
        LCCV = 14,

        /// <summary>
        /// 闭环恒流恒压充电工步
        /// </summary>
        [Description("闭环恒流恒压充电工步")]
        LCCCCV = 15,

        /// <summary>
        /// 脉冲工步
        /// </summary>
        [Description("脉冲工步")]
        Pulse = 16,

        /// <summary>
        /// 模拟工步
        /// </summary>
        [Description("模拟工步")]
        Sim = 17,

        /// <summary>
        /// 电池组恒流恒压
        /// </summary>
        [Description("电池组恒流恒压")]
        PCCCV = 18,

        /// <summary>
        /// 恒压放电
        /// </summary>
        [Description("恒压放电")]
        DChgVolt = 19,

        /// <summary>
        /// 恒流恒压放电
        /// </summary>
        [Description("恒流恒压放电")]
        DChgCurrVolt = 20,

        /// <summary>
        /// 恒功率恒压充电
        /// </summary>
        [Description("恒功率恒压充电")]
        ChgPowerVolt = 21,

        /// <summary>
        /// 恒功率恒压放电
        /// </summary>
        [Description("恒功率恒压放电")]
        DChgPowerVolt = 22,

        /// <summary>
        /// 工步最大个数, 添加新的工步类型时, 必须修相应改此字段
        /// </summary>
        StepTypeCount = 23,//工步最大个数, 添加新的工步类型时, 必须修相应改此字段

        /// <summary>
        /// 条件参数, 客户端内部使用
        /// </summary>
        Condition = 0xF0,//条件参数, 客户端内部使用

        /// <summary>
        /// 条件参数添加模式, 客户端内部使用
        /// </summary>
        ConditionAdd = 0xF1,//条件参数添加模式, 客户端内部使用
    };

    /// <summary>
    /// 通道状态
    /// </summary>
    public enum ChannelState : byte
    {
        /// <summary>
        /// 运行
        /// </summary>
        [Description("运行")]
        Run = 0,

        /// <summary>
        /// 异常
        /// </summary>
        [Description("异常")]
        Abnormal = 1,

        /// <summary>
        /// 暂停
        /// </summary>
        [Description("暂停")]
        Suspend = 2,

        /// <summary>
        /// 完成
        /// </summary>
        [Description("完成")]
        Completed = 3,

        /// <summary>
        /// 空闲
        /// </summary>
        [Description("空闲")]
        Free = 4,

        /// <summary>
        /// 离线(弃用，直接判断最近更新时间是否在一分钟以前)
        /// </summary>
        [Description("离线")]
        OffLine = 5,

        ///// <summary>
        ///// 蜂鸣器报警
        ///// </summary>
        //[Description("蜂鸣器报警")]
        //BuzzerAlarm = 5,
        ///// <summary>
        ///// 同步控制
        ///// </summary>
        //[Description("同步控制")]
        //SynControl = 6,
        ///// <summary>
        ///// 占用下位机
        ///// </summary>
        //[Description("占用下位机")]
        //OccupyLower = 7,
        ///// <summary>
        ///// 点灯
        ///// </summary>
        //[Description("点灯")]
        //LightUp = 8,
        ///// <summary>
        ///// 抽真空
        ///// </summary>
        //[Description("抽真空")]
        //Vacuum = 9,
        ///// <summary>
        ///// 泄真空
        ///// </summary>
        //[Description("泄真空")]
        //VentVacuum = 10,
        ///// <summary>
        ///// 测漏率
        ///// </summary>
        //[Description("测漏率")]
        //LeakDetectionRate=11,
        ///// <summary>
        ///// 测堵
        ///// </summary>
        //[Description("测堵")]
        //BlockTest = 12,
        ///// <summary>
        ///// 同步超时
        ///// </summary>
        //[Description("同步超时")]
        //SynTimeout=13,

    }
}
