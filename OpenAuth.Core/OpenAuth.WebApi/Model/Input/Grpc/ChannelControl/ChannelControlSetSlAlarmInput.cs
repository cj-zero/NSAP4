using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Model
{
    public class ChannelControlSetSlAlarmInput  : ChannelControlArg
    {
        /// <summary>
        /// 上位机Ip
        /// </summary>
        [Display(Name = "上位机Ip")]
        [Required(ErrorMessage = "{0}不可为空")]
        public string ip { get; set; }

        /// <summary>
        /// 通道
        /// </summary>
        public List<SetSlAlarmChlInput> chl { get; set; }

        public class SetSlAlarmChlInput
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

            /// <summary>
            /// 设置16表示复位声光报警
            /// </summary>
            public int sl_status { get; set; }

        }
    }
}
