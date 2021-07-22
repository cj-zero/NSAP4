using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order
{
    public class SelectOption
    {
        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 选项
        /// </summary>
        public string Option { get; set; }
    }
    public class DropDownOption
    {
        /// <summary>
        /// Key
        /// </summary>
        public object Id { get; set; }
        /// <summary>
        /// 选项
        /// </summary>
        public object Name { get; set; }
    }
}
