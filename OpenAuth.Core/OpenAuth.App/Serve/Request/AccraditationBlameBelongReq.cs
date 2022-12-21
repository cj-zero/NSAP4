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
        public int? Id { get; set; }//按灯单据ID
        public string FlowInstanceId { get; set; }//流程Id

        public int HandleId { get; set; }//审批单据ID
        public int? Idea { get; set; }//审批状态  1-同意 2-移交 3-申述
        public string Description { get; set; }//审批意见
        public List<string> Files { get; set; }//文件
        public string UserId { get; set; } //移交人员
        public string UserName { get; set; }
        public List<HandleList> HrHandleLists { get; set; }//hr审批结果
        public List<HandleMoery> HandleMoeryLists { get; set; }//责任金额判断
        public List<AffectList> AffectLists { get; set; }//核查部门填写数据
        public List<HandlePay> HandlePay { get; set; }//出纳填写数据
    }
    public class HandleList
    {
        public int? Id { get; set; }
        public int? HrIdea { get; set; }
        public List<AffectList> AffectUsers { get; set; }

    }
    public class AffectList
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
    }
    public class HandleMoery
    {
        public int? Id { get; set; }
        public decimal? actualMoney { get; set; }//实际影响金额
        public decimal? judgeMoney { get; set; }//判定金额
        public string Remark { get; set; }//判定金额
    }

    public class HandlePay
    {
        public int? Id { get; set; }
        public int IsPay { get; set; }//是否支付  0=未支付  1=已支付
    }
}
