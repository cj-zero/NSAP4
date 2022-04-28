using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Model
{
    /// <summary>
    /// 通道控制---设置温箱绑定关系
    /// </summary>
    /// <returns></returns>
    public class ChannelControlSetThermostatMapInput : ChannelControlArg
    {
        /// <summary>
        /// 上位机Ip
        /// </summary>
        [Display(Name = "上位机Ip")]
        [Required(ErrorMessage = "{0}不可为空")]
        public string ip { get; set; }
        /// <summary>
        /// 设备类型(23,24)
        /// </summary>
        [Required]
        public int dev_type { get; set; }
        /// <summary>
        /// 设备ID(1,2,3...)
        /// </summary>
        [Required]
        public int dev_id { get; set; }
        /// <summary>
        /// 命令类型:0 表示将映射信息保存到数据库;1 表示发送映射信息到中位机;2 表示删除数据库中的映射信息
        /// </summary>
        [Required]
        public int type { get; set; }
        /// <summary>
        /// 温箱号
        /// </summary>
        [Required]
        public int thermostat_id { get; set; }
        /// <summary>
        /// 温箱层号
        /// </summary>
        [Required]
        public int floor_id { get; set; }
        /// <summary>
        /// 通道集合
        /// </summary>
        [Required]
        public List<SetThermostatMapChlInput> chl { get; set; }
    }
    public class SetThermostatMapChlInput
    {
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
    }
}







