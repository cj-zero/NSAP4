using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Domain;

namespace OpenAuth.App.ContractManager.Request
{
    public class AccraditationContractApplyReq
    {
        /// <summary>
        /// 合同申请单单个Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 流程id
        /// </summary>
        public string FlowInstanceId { get; set; }

        /// <summary>
        /// 是否驳回
        /// </summary>
        public bool IsReject { get; set; }

        /// <summary>
        /// 合同申请单多个Id
        /// </summary>
        public List<string> ContractId { get; set; }

        public List<ContractResult> contractResults { get; set; }

        /// <summary>
        /// 合同申请单实体
        /// </summary>
        public ContractApply contractApply { get; set; }
    }

    public class ContractResult
    {

        public string Id { get; set; }

        public bool Value { get; set; }
    }
}
