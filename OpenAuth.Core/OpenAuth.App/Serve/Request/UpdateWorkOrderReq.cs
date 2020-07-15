using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class UpdateWorkOrderReq
    {
        /// <summary>
        /// 工单Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 优先级 4-紧急 3-高 2-中 1-低
        /// </summary>
        public int? Priority { get; set; }
        /// <summary>
        /// 服务类型 1-免费 2-收费
        /// </summary>
        public int? FeeType { get; set; }
        /// <summary>
        /// 呼叫主题
        /// </summary>
        public string FromTheme { get; set; }
        /// <summary>
        /// 问题类型Id
        /// </summary>
        public string ProblemTypeId { get; set; }
        /// <summary>
        /// 呼叫类型1-提交呼叫 2-在线解答（已解决）
        /// </summary>
        public int? FromType { get; set; }
        /// <summary>
        /// 解决方案Id
        /// </summary>
        public string SolutionId { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
