using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.ModelDto
{
    /// <summary>
    /// 科目（服务）
    /// </summary>
    public class OactDto
    {
        /// <summary>
        /// 总帐科目
        /// </summary>
        public string AcctCode { get; set; }
        /// <summary>
        /// 总账科目名称
        /// </summary>
        public string AcctName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal CurrTotal { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Details { get; set; }
    }
}
