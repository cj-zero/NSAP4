using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace OpenAuth.App.DDVoice.Common
{
    /// <summary>
    /// 合同管理配置帮助
    /// </summary>
    public class DDSettingHelp
    {
        /// <summary>
        /// 配置文件
        /// </summary>
        public IConfiguration _configuration;

        /// <summary>
        /// 构造函数
        /// </summary>
        public DDSettingHelp(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 获取配置文件中钉钉配置信息
        /// </summary>
        /// <param name="key">配置属性</param>
        /// <returns>返回钉钉配置信息</returns>
        public string GetDDKey(string key)
        {
            return _configuration.GetValue<string>(string.Format("DD:{0}", key));
        }

        /// <summary>
        /// 获取配置文件中委托单token
        /// </summary>
        /// <param name="key">配置属性</param>
        /// <returns>返回配置文件中委托单token</returns>
        public string GetCalibrationKey(string key)
        {
            return _configuration.GetValue<string>(string.Format("Calibration:{0}", key));
        }
    }
}
