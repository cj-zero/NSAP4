namespace OpenAuth.App.Request
{
    public class QueryGlobalAreaListReq : PageReq
    {
        //todo:添加自己的请求字段
        /// <summary>
        /// 省编号
        /// </summary>
        public string provinceCode { get; set; }
        /// <summary>
        /// 市编号
        /// </summary>
        public string cityCode { get; set; }
        /// <summary>
        /// 国家编号
        /// </summary>
        public string StateCode { get; set; }
    }
}