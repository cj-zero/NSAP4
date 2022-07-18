namespace OpenAuth.App.Request
{
    public class QueryDataPrivilegeRuleListReq : PageReq
    {
        //todo:添加自己的请求字段
        public bool? IsNew { get; set; }
        public string SourceCode { get; set; }
        public string CreateUserName { get; set; }
        public string Description { get; set; }
        public bool? Enable { get; set; }
        public string SourceName { get; set; }
    }
}