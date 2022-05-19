using System;

namespace OpenAuth.WebApi.Model
{
    public class ListChannelStateRateInput
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime start { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime end { get; set; }
        /// <summary>
        /// 利用率日期范围分类
        /// </summary>
        public DTType RDstate { get; set; }

    }
    /// <summary>
    /// 利用率查询日期分类
    /// </summary>
    public enum DTType : byte
    {
        当天 = 1,
        其他 = 2,
    }
}
