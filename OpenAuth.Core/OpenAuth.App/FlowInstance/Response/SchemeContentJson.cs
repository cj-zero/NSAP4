using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class SchemeContentJson
    {
        public string title { get;set;}

        public string initNum { get; set; }

        public List<Line> Lines { get; set; }
        public List<Node> Nodes { get; set; }
        public new string[] areas { get; set; }
    }
    public class Line
    {
        public string id { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string name { get; set; }
        public string dash { get; set; }
        public List<Compare> Compares { get; set; }


    }
    public class Node 
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int? left { get; set; }
        public int? top { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public bool? alt { get; set; }
    }

    public class Compare 
    {
        public string Operation { get; set; }
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public string Value { get; set; }
    }
    public class SetInfo 
    {
        public string NodeDesignate { get; set; }
        public List<NodeDesignateData> NodeDesignateData { get; set; }
        public string NodeCode { get; set; }
        public string NodeName { get; set; }
        public string ThirdPartyUrl { get; set; }
        public string NodeRejectType { get; set; }
        public string Taged { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string Description { get; set; }
        public string TagedTime { get; set; }
        public string NodeConfluenceType { get; set; }
        public string ConfluenceOk { get; set; }
        public string ConfluenceNo { get; set; }
    }
    public class NodeDesignateData 
    {
        public new string[] users { get; set; }
        public new string[] roles { get; set; }
        public new string[] orgs { get; set; }
    }
}
