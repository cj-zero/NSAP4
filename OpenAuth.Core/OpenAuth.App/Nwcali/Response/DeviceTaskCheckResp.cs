using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Response
{
    public class DeviceTaskCheckResp
    {
        public long CheckId { get; set; }
        /// <summary>
        /// 0:测试是否按流程完成
        /// 1:搁置工步是否漏电
        /// 2:恒压工步电压是否稳定
        /// 3:放电工步电压是否下降
        /// 4:充电工步电压是否上升
        /// 5:恒流工步电流稳定
        /// </summary>
        public int CheckType { get; set; }
        public string CheckName { get; set; }
        public int ErrCount { get; set; }
        public int CheckStatus { get; set; }
        public List<string> ErrList { get; set; }
    }
}
