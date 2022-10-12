using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.ClientRelation.Response
{
    public class  RelationGraphRsp
    {
        /// <summary>
        /// 根节点
        /// </summary>
        public string rootId { get; set; }
        /// <summary>
        /// 关联节点
        /// </summary>
        public List<GraphNodes> Nodes { get; set; } = new List<GraphNodes>();
        /// <summary>
        /// 关联关系
        /// </summary>
        public List<GraphLinks> Links { get; set; } = new List<GraphLinks> { };
    }

    public class RawGraph
    {
        public string ClientNo { get; set; }

        public string ClientName { get; set; }

        public int Flag { get; set; }

        public string ParentNo { get; set; }

        public string SubNo { get; set; }

    }

    /// <summary>
    /// 节点
    /// </summary>
    public class GraphNodes
    {
        public string Id { get; set; }
        public string CardCode { get; set; }
        public string Text { get; set; }
        public int flag { get; set; }
    }

    /// <summary>
    /// 连接关系
    /// </summary>
    public class GraphLinks
    {
        public string From { get; set; }
        public string To { get; set; }
    }

}
