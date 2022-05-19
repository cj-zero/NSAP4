using OpenAuth.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Model
{
    public class GrpcBaseInput
    {
        /// <summary>
        /// 应用id(用户名)
        /// </summary>
        public string app_id => AppSetting.GrpcOrMqttUserName;
        /// <summary>
        /// 应用秘钥（密码）
        /// </summary>
        public string app_secret => AppSetting.GrpcOrMqttPwd;
        /// <summary>
        /// 边缘计算guid
        /// </summary>
        public string edge_guid { get; set; }
    }
}
