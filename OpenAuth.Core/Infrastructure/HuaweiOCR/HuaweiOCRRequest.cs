using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.HuaweiOCR
{
    public class HuaweiOCRRequest
    {
        /// <summary>
        /// 图像数据，base64编码，要求base64编码后大小不超过10MB。
        /// 图片最小边不小于15px，最长边不超过8000px，支持JPEG、JPG、PNG、BMP、TIFF格式。
        /// </summary>
        public string image { get; set; }

        ///// <summary>
        ///// 图片的url路径，目前支持：
        /////公网http/https url
        /////OBS提供的url，使用OBS数据需要进行授权。包括对服务授权、临时授权、匿名公开授权，详情参见配置OBS访问权限。
        ///// </summary>
        //public string url { get; set; }

        ///// <summary>
        ///// 可以指定要识别的票证，指定后不出现在此List的票证不识别。不指定时默认返回所有支持类别票证的识别信息。
        ///// </summary>
        //public List<string> type_list { get; set; }

    }
}
