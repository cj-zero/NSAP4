using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Model
{
    public class GrpcDevInfoInput : GrpcBaseInput
    {

        public GrpcDevInfoChlInput chl_info { get; set; }

        public GrpcDevInfoDataCondition data_condition { get; set; }

        public class GrpcDevInfoChlInput
        {
            /// <summary>
            /// 上位机guid
            /// </summary>
            public string srv_guid { get; set; }
            /// <summary>
            /// 上位机ip
            /// </summary>
            public string srv_ip { get; set; }
            /// <summary>
            /// 中位机编号,如:240001
            /// </summary>
            public int dev_uid { get; set; }
          
        }

        public class GrpcDevInfoDataCondition
        {
            /// <summary>
            /// 需要返回的字段['test_id', 'dev_uid'...]如果为空则返回所有字段
            /// </summary>
            public string[] field { get; set; }
        }

    }
}
