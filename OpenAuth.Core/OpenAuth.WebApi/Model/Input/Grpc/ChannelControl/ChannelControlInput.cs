using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Model
{
    /// <summary>
    /// 通道控制
    /// </summary>
    public class ChannelControlInput: GrpcBaseInput
    {

        public ControlInput control { get; set; }


        public class ControlInput
        {
            /// <summary>
            /// 命令类型
            /// </summary>
            public string cmd_type { get; set; }



            /// <summary>
            /// 命令参数
            /// </summary>
            public string arg { get; set; }
        }


       
    }
}
