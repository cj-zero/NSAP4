using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.ContractManager.Common
{
    public static class ContractMethodHelp
    {
        /// <summary>
        /// 获取配置的合同状态
        /// </summary>
        /// <param name="contractType">合同类型</param>
        /// <returns>返回合同类型对应的合同状态</returns>
        public static string GetConfigTypeStatus(string contractType)
        {
            string status = "";
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            keyValues.Add("1", "3");
            keyValues.Add("2", "4");
            keyValues.Add("3", "10");
            keyValues.Add("4", "12");
            if (!string.IsNullOrEmpty(contractType))
            {
                keyValues.TryGetValue(contractType, out string value);
                status = value?.ToString() ?? "";
            }

            return status;
        }
    }
}
