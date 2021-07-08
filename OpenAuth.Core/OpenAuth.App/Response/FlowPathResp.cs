using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class FlowPathResp
    {
        /// <summary>
        /// 审批节点
        /// </summary>
        public string ActivityName { get; set; }
        /// <summary>
        /// 审批时间
        /// </summary>

        public string CreateTime { get; set; }
        /// <summary>
        /// 审批时常
        /// </summary>

        public string IntervalTime { get; set; }

        /// <summary>
        /// 是否当前节点
        /// </summary>

        public bool IsNode  { get; set; }
    }
    public class FlowInstanceJson
    {
        /// <summary>
        /// 审批节点
        /// </summary>
        public List<FlowInstanceNodes> nodes { get; set; }
        /// <summary>
        /// ID
        /// </summary>

        public List<FlowInstanceLines> lines { get; set; }
    }
    public class FlowInstanceNodes
    {
        /// <summary>
        /// 审批节点
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// ID
        /// </summary>

        public string id { get; set; }
        /// <summary>
        /// 排序
        /// </summary>

        public string Number { get; set; }
    }
    public class FlowInstanceLines
    {
        /// <summary>
        /// 审批节点
        /// </summary>
        public string from { get; set; }
        /// <summary>
        /// ID
        /// </summary>

        public string id { get; set; }

        /// <summary>
        /// ID
        /// </summary>

        public string to { get; set; }
    }
}
