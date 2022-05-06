using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neware.LIMS.Mqtt
{

    public class AppSetting
    {
        /// <summary>
        /// 配置信息
        /// </summary>
        public static AppConfig AppConfig { get; set; }
    }

    public class AppConfig
    {
        /// <summary>
        /// 远程服务
        /// </summary>
        public ServerConfig RemoteServer { get; set; }
        /// <summary>
        /// 本地服务
        /// </summary>
        public ServerConfig LocalServer { get; set; }
    }

    public class ServerConfig
    {
        /// <summary>
        /// ip地址
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
    }

}
