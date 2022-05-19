using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Model
{
    /// <summary>
    /// 通道控制---预约暂停
    /// </summary>
    public class ChannelControlReserveStopTestInput : ChannelControlArg
    {
        /// <summary>
        /// 上位机Ip
        /// </summary>
        [Display(Name = "上位机Ip")]
        [Required(ErrorMessage = "{0}不可为空")]
        public string ip { get; set; }

        /// <summary>
        /// 1：设置预约停止 2:取消预约停止
        /// </summary>
        public int reserve_stop_opreate_type { get; set; }

        /// <summary>
        /// 预约停止的工步ID与循环ID,如果没有可以为空
        /// </summary>
        public StepCycleStopDetail step_cycle_stop { get; set; }

        public class StepCycleStopDetail
        {
            /// <summary>
            /// 预约停止的工步ID 0xFFFF表示当前的工步
            /// </summary>
            public uint step { get; set; }

            /// <summary>
            /// 预约停止的循环ID 0xFFFFFFFF表示当前的循环
            /// </summary>
            public uint cycle { get; set; }
        }

        /// <summary>
        /// 超时时间(单位:ms)
        /// </summary>
        public int out_time { get; set; }

        /// <summary>
        /// 详情
        /// </summary>
        public List<ReserveStopTestChlInput> chl { get; set; }


        public class ReserveStopTestChlInput
        {
            /// <summary>
            /// 设备号(240001)
            /// </summary>
            public int dev_uid { get; set; }

            /// <summary>
            /// 单元ID
            /// </summary>
            public int unit_id { get; set; }

            /// <summary>
            /// 通道ID
            /// </summary>
            public int chl_id { get; set; }
            /// <summary>
            /// 通道唯一id
            /// </summary>
            [Required]
            public string chl_snowflake_id { get; set; }
        }


      
    }
}
