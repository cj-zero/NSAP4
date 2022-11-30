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
        /// 用户部门
        /// </summary>
        public string UserDept { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 是否当前节点
        /// </summary>

        public bool IsNode  { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Number { get; set; }
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

        public int Number { get; set; }
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

        public List<FlowInstanceCompares > Compares { get; set; }
    }
    public class FlowInstanceCompares
    {
        /// <summary>
        /// 判断名称
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// 判断
        /// </summary>
        public string Operation { get; set; }
        /// <summary>
        /// 判断值
        /// </summary>
        public string Value { get; set; }
    }
}
