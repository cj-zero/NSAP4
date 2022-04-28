using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neware.LIMS.Model
{
    /// <summary>
    /// 通道控制---获取设备日志命令
    /// </summary>
    public class ChannelControlGetDevLogInput //: ChannelControlArg
    {
        /// <summary>
        /// 上位机Ip
        /// </summary>
        [Display(Name = "上位机Ip")]
        [Required(ErrorMessage = "{0}不可为空")]
        public string ip { get; set; }
        /// <summary>
        /// 设备号(240001)
        /// </summary>
        [Required]
        public int dev_uid { get; set; }
        /// <summary>
        /// 单元id
        /// </summary>
        [Required]
        public int unit_id { get; set; }
        /// <summary>
        /// 通道id
        /// </summary>
        [Required]
        public int chl_id { get; set; }
        /// <summary>
        /// 通道唯一id
        /// </summary>
        [Required]
        public string chl_snowflake_id { get; set; }
        /// <summary>
        /// 测试id（如果为0表示获取当前测试）
        /// </summary>       
        public int test_id { get; set; }
        /// <summary>
        /// 日志起始时间格式:YYYY-MM-DD hh:mm:ss
        /// </summary>
        public string beg_time { get; set; }
        /// <summary>
        /// 日志截至时间格式:YYYY-MM-DD hh:mm:ss
        /// </summary>
        public string end_time { get; set; }
    }
}


