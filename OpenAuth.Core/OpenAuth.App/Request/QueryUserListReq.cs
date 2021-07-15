namespace OpenAuth.App.Request
{
    public class QueryUserListReq : PageReq
    {
        public string orgId { get; set; }
        public string account { get; set; }
        public string name { get; set; }
    }
}
