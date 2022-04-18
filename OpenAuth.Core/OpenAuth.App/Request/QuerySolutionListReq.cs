namespace OpenAuth.App.Request
{
    public class QuerySolutionListReq : PageReq
    {
        public QuerySolutionListReq()
        {
        }
        //todo:添加自己的请求字段

        /// <summary>
        /// 目录等级
        /// </summary>
        public int Rank { get; set; }

        public string Subject { get; set; }
    }
}