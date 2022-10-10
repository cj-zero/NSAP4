using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class AccraditationBlameBelongReq
    {
        public AccraditationBlameBelongReq()
        {
            this.Files = new List<string>();
        }
        public int? Id { get; set; }
        public string HandleId { get; set; }
        public string FlowInstanceId { get; set; }
        public int? Idea { get; set; }
        public string Description { get; set; }
        public List<string> Files { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        //public string TransferOrg { get; set; }
        //public string TransferOrgId { get; set; }
        public List<HandleList> HandleLists { get; set; }
    }

    public class HandleList
    {
        public string Id { get; set; }
        public int? HrIdea { get; set; }
        public string TransferOrg { get; set; }
        public string TransferOrgId { get; set; }
        public decimal Amount { get; set; }

    }
}
