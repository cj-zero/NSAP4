namespace OpenAuth.App.Request
{
    public class QueryKnowledgeBaseListReq : PageReq
    {
        //todo:添加自己的请求字段
        public int? Type { get; set; }

        public string ParentId { get; set; }

        public string Rank { get; set; }

        public string Code { get; set; }

        public string Id { get; set; }
    }
}