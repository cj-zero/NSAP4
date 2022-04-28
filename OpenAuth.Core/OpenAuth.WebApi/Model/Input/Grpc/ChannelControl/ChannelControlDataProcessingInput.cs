using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Model
{
    /// <summary>
    /// 通道控制---执行自定义数据处理代码
    /// </summary>
    /// <returns></returns>
    public class ChannelControlDataProcessingInput : ChannelControlArg
    {
        /// <summary>
        /// 自定义函数
        /// </summary>
        public List<ModulesItem> modules { get; set; }
        /// <summary>
        /// 测试信息
        /// </summary>
        public List<Test_infoItem> test_info { get; set; }
        public class Test_infoItem
        {
            /// <summary>
            /// 测试编号
            /// </summary>
            [Required]
            public ulong test_num { get; set; }
            /// <summary>
            /// LIMS上标识测试的唯一编号
            /// </summary>
            public string lotid { get; set; }
        }
        public class ModulesItem
        {
            /// <summary>
            /// 算法的唯一名称
            /// </summary>
            [Required]
            public string key { get; set; }
            /// <summary>
            /// python函数原型:def data_processing(system_parameter, user_parameter)
            /// </summary>
            [Required]
            public string source_code { get; set; }
            /// <summary>
            /// 用户参数列表："参数名":"值"
            /// </summary>
            [Required]
            public string[] user_parameter { get; set; }
        }
    }
}













