using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr.Request
{
    /// <summary>
    /// 排序交换
    /// </summary>
    public class SortExchange<T>
    {
        /// <summary>
        /// 需要替换的Id
        /// </summary>
        public T OldId { get; set; }


        /// <summary>
        /// 被替换的Id
        /// </summary>
        public T NewId { get; set; }


    }
}
