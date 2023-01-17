using System;
using System.Collections.Generic;
using System.Text;
using OpenAuth.App.Request;

namespace OpenAuth.App.DDVoice.EntityHelp
{
    public class LimsEntityResultHelp
    {
        /// <summary>
        /// 编码
        /// </summary>
        public int? code { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public ResultData data { get; set; }

        /// <summary>
        /// 信息
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 成功与否
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// 总数记录
        /// </summary>
        public int? totalRecordes { get; set; }
    }

    public class ResultData
    {
        /// <summary>
        /// 身份token
        /// </summary>
        public string identityToken { get; set; }
    }

    public class AccessResultHelp
    {
        /// <summary>
        /// token
        /// </summary>
        public string data { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string errorMessage { get; set; }

        /// <summary>
        /// 结果编码
        /// </summary>
        public int? resultCode { get; set; }

        /// <summary>
        /// 成功与否
        /// </summary>
        public bool success { get; set; }
    }

    public class AccessHelp
    {
        /// <summary>
        /// 客户端Id
        /// </summary>
        public string clientId { get; set; }

        /// <summary>
        /// 客户端密钥
        /// </summary>
        public string clientSecret { get; set; }

        /// <summary>
        /// 域
        /// </summary>
        public string scope { get; set; }
    }

    public class LimsServerHelp
    {
        /// <summary>
        /// 企业Id
        /// </summary>
        public int enterpriseId { get; set; }

        /// <summary>
        /// 通过接口Id
        /// </summary>
        public int? passportId { get; set; }

    }
}
