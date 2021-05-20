namespace OpenAuth.App.Request
{
    public class QueryCompletionReportListReq : PageReq
    {
        //todo:添加自己的请求字段
        /// <summary>
        /// 制造商序列号
        /// </summary>
        public string ManufacturerSerialNumber { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// app用户id
        /// </summary>
        public int? CurrentUserId { get; set; }
        
    }
}