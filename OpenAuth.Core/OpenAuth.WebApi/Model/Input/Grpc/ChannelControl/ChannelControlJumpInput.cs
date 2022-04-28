using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Model
{
    /// <summary>
    /// 通道控制---跳转
    /// </summary>
    public class ChannelControlJumpInput : ChannelControlArg
    {
        /// <summary>
        /// 上位机Ip
        /// </summary>
        [Display(Name = "上位机Ip")]
        [Required(ErrorMessage = "{0}不可为空")]
        public string ip { get; set; }

        /// <summary>
        /// 跳转到第几工步
        /// </summary>
        public int step_id { get; set; }

        /// <summary>
        /// 通道
        /// </summary>
        public List<JumpChlInput> chl { get; set; }

        public class JumpChlInput
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
