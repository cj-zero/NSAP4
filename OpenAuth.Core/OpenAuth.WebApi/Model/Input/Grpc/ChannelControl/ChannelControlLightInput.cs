using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Model
{
    /// <summary>
    /// 通道控制---点灯
    /// </summary>
    public class ChannelControlLightInput : ChannelControlArg
    {
        /// <summary>
        /// 上位机Ip
        /// </summary>
        [Display(Name = "上位机Ip")]
        [Required(ErrorMessage = "{0}不可为空")]
        public string ip { get; set; }

        /// <summary>
        /// 是否亮灯  0:不亮 1：亮
        /// </summary>
        public int light_up { get; set; }

        /// <summary>
        /// 通道
        /// </summary>
        public List<LightChlInput> chl { get; set; }

        public class LightChlInput
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
