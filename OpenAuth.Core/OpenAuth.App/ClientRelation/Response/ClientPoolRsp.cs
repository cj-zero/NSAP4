
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.ClientRelation.Response
{
    /// <summary>
    /// 客户勾选数据源
    /// </summary>
    public class ClientPoolRsp
    {
        /// <summary>
        /// 客户列表
        /// </summary>
        public List<ClientsSource> PoolList { get; set; } = new List<ClientsSource>();
        /// <summary>
        /// 客户数
        /// </summary>
        public int  PoolCount { get; set; }

    }


    public class ClientLegitRelation
    {
        public int Flag { get; set; }
        public string Terminals { get; set; }
    }


    /// <summary>
    /// 客户源
    /// </summary>
    public class ClientsSource
    {
        /// <summary>
        /// 客户编号
        /// </summary>
        public string ClientNo { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string ClientName { get; set; }

    }

}
