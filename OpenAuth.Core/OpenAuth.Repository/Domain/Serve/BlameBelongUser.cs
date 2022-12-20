using Infrastructure.AutoMapper;
using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Serve
{
    [Table("blamebelonguser")]
    public class BlameBelongUser : BaseEntity<int>
    {
        public BlameBelongUser()
        {
            this.Description = string.Empty;
            this.HandleUserId = string.Empty;
            this.HandleUserName = string.Empty;
            this.HandleStatus = 0;
            this.HandleTransfer = string.Empty;
            this.HandleRemark = string.Empty;
            this.AppealNum =0;
            this.HrStatus = -1;
            this.HrTransfer = string.Empty;
            this.ActualMoney = 0;
            this.JudgeMoney = 0;
            this.IsRead = false;
            this.IsFirst = true;
        }
        public int BlameBelongId { get; set; }
        public string Description { get; set; }
        public string HandleUserId { get; set; }
        public string HandleUserName { get; set; }
        public int HandleStatus { get; set; }
        public string HandleTransfer { get; set; }
        public string HandleRemark { get; set; }
        public DateTime? ApprovalTime { get; set; }
        public int AppealNum { get; set; }
        public int HrStatus { get; set; }
        public string HrTransfer { get; set; }
        public DateTime? HrTime { get; set; }
        public decimal? ActualMoney { get; set; }
        public decimal? JudgeMoney { get; set; }
        public string Remark { get; set; }
        public DateTime? CreateTime { get; set; }
        public bool IsRead { get; set; }
        public bool IsFirst { get; set; }
        public int? SuperiorId { get; set; }
        public int? IsPay { get; set; }
        public int? MoneyStatus { get; set; }

        


        public virtual List<BlameBelongUserFile> BlameBelongUserFile { get; set; }
        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }


    [AutoMapTo(typeof(BlameBelongUser))]
    public class BlameBelongUserReq : BlameBelongUser
    {
        public bool IsHandleButton { get; set; } = false;  //责任部门审核-责任人审批
        public bool IsHrButton { get; set; } = false; //责任部门审核-hr审批
        public bool IsAmountButton { get; set; } = false;//责任金额判断
        public bool IsStorageButton { get; set; } = false;//收纳确认
    }
}
