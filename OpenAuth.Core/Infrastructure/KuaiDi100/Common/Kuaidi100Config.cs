
namespace KuaiDi100.Common
{
    /// <summary>
    /// 快递100的基础账号信息，可以在这里获取 
    /// https://poll.kuaidi100.com/manager/page/myinfo/enterprise
    /// </summary>
    public class KuaiDi100Config
    {
        /// <summary>
        /// 授权key
        /// </summary>
        public string key { get; set; } = "xfEWlpDL3993";
        /// <summary>
        /// customer
        /// </summary>
        public string customer { get; set; } = "5A22443B478B450D1E7488E3EF0BDF02";

        /// <summary>
        /// secret
        /// </summary>
        public string secret { get; set; } = "819e7e135ac243af851dd23f564b572c";
        /// <summary>
        /// 电子面单模板id
        /// </summary>
        public string siid { get; set; }
        /// <summary>
        /// userid
        /// </summary>
        public string userid { get; set; } = "b1eb8001d532422a98979f983d2f7e7f";
        /// <summary>
        /// 短信模板id
        /// </summary>
        public string tid { get; set; }
        /// <summary>
        /// 电子面单快递公司账号信息（月结账号）
        /// </summary>
        public string partnerId { get; set; }
        /// <summary>
        /// 电子面单快递公司账号信息（账号密码）
        /// </summary>
        public string partnerKey { get; set; }
    }
}
