using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Model
{
    /// <summary>
    /// 通道控制---数据加急上传
    /// </summary>
    public class ChannelControlDataUploadUrgentInput : ChannelControlArg, IValidatableObject
    {
        /// <summary>
        /// 上位机Ip
        /// </summary>
        [Display(Name = "上位机Ip")]
        [Required(ErrorMessage = "{0}不可为空")]
        public string ip { get; set; }

        /// <summary>
        /// BTS类型4:4系设备；5:5系设备
        /// </summary>
        public int bts_type { get; set; }

        /// <summary>
        /// 1:时间区间上传(必填：ip,type,begin_time,end_time),2：加急上传只上传测试信息(必填:ip, type),3:加急上传，测试信息和测试数据都上传(必填：ip, type，如果没有通道信息，则上传所有通道的测试数据)
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// 起始时间(YYYY-MM-DD hh:mm:ss)
        /// </summary>
        public string beg_time { get; set; }

        /// <summary>
        /// 截至时间 (YYYY-MM-DD hh:mm:ss)
        /// </summary>
        public string end_time { get; set; }

        /// <summary>
        /// 通道集合
        /// </summary>
        public List<DataUploadUrgentChlInput> chl { get; set; }
        public class DataUploadUrgentChlInput
        {
            /// <summary>
            /// 设备号
            /// </summary>
            public int dev_uid { get; set; }
            /// <summary>
            /// 单元id
            /// </summary>
            public int unit_id { get; set; }
            /// <summary>
            /// 通道id
            /// </summary>
            public int chl_id { get; set; }
            /// <summary>
            /// 通道唯一id
            /// </summary>
            [Required]
            public string chl_snowflake_id { get; set; }
        }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            if (type == 1 && (string.IsNullOrWhiteSpace(beg_time) || string.IsNullOrWhiteSpace(end_time)))
            {
                yield return new ValidationResult("时间区间上传,起始时间与截至时间不能为空", new[] { nameof(beg_time), nameof(end_time) });
            }
        }
    }
}




