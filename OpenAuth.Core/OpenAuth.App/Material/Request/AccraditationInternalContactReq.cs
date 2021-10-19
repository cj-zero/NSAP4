using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public class AccraditationInternalContactReq
    {
        public int Id { get; set; }
        public string Remark { get; set; }
        /// <summary>
        /// 是否驳回
        /// </summary>
        public bool IsReject { get; set; }
        /// <summary>
        /// 是否暂定
        /// </summary>
        public bool IsTentative { get; set; }
        /// <summary>
        /// 1-查收 2-执行
        /// </summary>
        public int ExecType { get; set; }
        /// <summary>
        /// 1-审批 2-执行
        /// </summary>
        public int HanleType { get; set; }
    }
}
