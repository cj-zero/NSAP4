using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.ContractManager.Request
{
    public class FlowInstanceListJson
    {
        /// <summary>
        /// 节点集合
        /// </summary>
        public List<FlowInstanceNodeMsg> nodes { get; set; }
    }

    public class FlowInstanceNodeMsg
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

        /// <summary>
        /// 节点类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 设置信息
        /// </summary>
        public SetNodeInfo setInfo { get; set; }
    }

    public class SetNodeInfo
    { 
        /// <summary>
        /// 节点设置
        /// </summary>
        public NodeDesignateDatas NodeDesignateData { get; set; }
    }

    public class NodeDesignateDatas
    { 
        /// <summary>
        /// 用户集合
        /// </summary>
        public string[] users { get; set; }

        /// <summary>
        /// 角色集合
        /// </summary>
        public string[] roles { get; set; }

        /// <summary>
        /// 文本
        /// </summary>
        public string Texts { get; set; }
    }
}
