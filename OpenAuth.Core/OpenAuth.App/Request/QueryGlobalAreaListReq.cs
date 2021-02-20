namespace OpenAuth.App.Request
{
    public class QueryGlobalAreaListReq : PageReq
    {
        //todo:添加自己的请求字段
        /// <summary>
        /// 地区ID
        /// </summary>
        public string ReqId { get; set; }

        /// <summary>
        /// 地区名称
        /// </summary>
        public string AreaName { get; set; }
    }
}