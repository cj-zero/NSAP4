using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Model
{
    /// <summary>
    /// 通道控制---启动测试
    /// </summary>
    public class ChannelControlStartTestInput : ChannelControlArg,IValidatableObject
    {
        /// <summary>
        /// 补充信息存放json
        /// </summary>
        [Required]
        public string json_supplement { get; set; }
        /// <summary>
        /// 上位机Ip
        /// </summary>
        [Display(Name = "上位机Ip")]
        [Required(ErrorMessage = "{0}不可为空")]
        public string ip { get; set; }

        /// <summary>
        /// 流程数据(对xml进行base64编码)
        /// </summary>
        [Display(Name = "流程数据")]
        public string step_data { get; set; }


        /// <summary>
        /// 工步文件id
        /// </summary>
        [Required]
        public string file_id { get; set; }
        /// <summary>
        /// 工步文件名
        /// </summary>
        public string step_file_name { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string batch_no { get; set; }

        /// <summary>
        /// 测试名
        /// </summary>
        public string test_name { get; set; }

        /// <summary>
        /// 起始工步ID
        /// </summary>
        public int start_step { get; set; }

        /// <summary>
        /// 创建者
        /// </summary>
        public string creator { get; set; }

        /// <summary>
        /// 当前通道的系数:量程大于1000mA系数为10,量程大于100mA系数为100,量程大于10mA系数为1000,量程小于10mA为10000
        /// </summary>
        public int scale { get; set; }

        /// <summary>
        /// 质量(单位:微克)
        /// </summary>
        public uint battery_mass { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string desc { get; set; }

        /// <summary>
        /// 通道
        /// </summary>
        public List<StartTestChlInput> chl { get; set; }


        public class StartTestChlInput : IValidatableObject
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
            /// 条码
            /// </summary>
            public string barcode { get; set; }

            /// <summary>
            /// 质量(单位:微克)
            /// </summary>
            public uint battery_mass { get; set; }

            /// <summary>
            /// 备注
            /// </summary>
            public string desc { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (!string.IsNullOrWhiteSpace(barcode) && Encoding.Default.GetByteCount(barcode) > 100)
                {
                    yield return new ValidationResult("条码长度要小于100字节", new[] { nameof(barcode) });
                }

                if (!string.IsNullOrWhiteSpace(desc) && Encoding.Default.GetByteCount(desc) > 90)
                {
                    yield return new ValidationResult("备注长度要小于90字节", new[] { nameof(desc) });
                }
            }
        }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(batch_no) && Encoding.Default.GetByteCount(batch_no) > 60)
            {
                yield return new ValidationResult("批号长度要小于60字节", new[] { nameof(batch_no) });
            }

            if (!string.IsNullOrWhiteSpace(test_name) && Encoding.Default.GetByteCount(test_name) > 60)
            {
                yield return new ValidationResult("测试名长度要小于60字节", new[] { nameof(test_name) });
            }


            if (!string.IsNullOrWhiteSpace(creator) && Encoding.Default.GetByteCount(creator) > 60)
            {
                yield return new ValidationResult("创建者长度要小于60字节", new[] { nameof(creator) });
            }

            if (!string.IsNullOrWhiteSpace(desc) && Encoding.Default.GetByteCount(desc) > 90)
            {
                yield return new ValidationResult("备注长度要小于90字节", new[] { nameof(desc) });
            }

        }

    }

}
