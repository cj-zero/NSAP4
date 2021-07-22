using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    [Serializable]
    [DataContract]
    public class SalesAcctCode
    {
        [DataMember]
        public string Details { get; set; }
        /// <summary>
        /// 科目代码
        /// </summary>
        [DataMember]
        public string AcctCode { get; set; }
        /// <summary>
        /// 科目代码名称
        /// </summary>
        [DataMember]
        public string AcctName { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        [DataMember]
        public string Price { get; set; }
        /// <summary>
        /// 每行折扣 %
        /// </summary>
        [DataMember]
        public string DiscPrcnt { get; set; }
        /// <summary>
        /// 汇率
        /// </summary>
        [DataMember]
        public string Rate { get; set; }
        /// <summary>
        /// 折扣后价格
        /// </summary>
        [DataMember]
        public string PriceBefDi { get; set; }
        /// <summary>
        /// 每行税收百分比
        /// </summary>
        [DataMember]
        public string VatPrcnt { get; set; }
        /// <summary>
        /// 每行税收百分比
        /// </summary>
        [DataMember]
        public string VatGroup { get; set; }

        /// <summary>
        /// 毛价
        /// </summary>
        [DataMember]
        public string PriceAfVAT { get; set; }

        /// <summary>
        /// 行总计
        /// </summary>
        [DataMember]
        public string LineTotal { get; set; }
        /// <summary>
        /// 目标凭证类型(-1,0,13,16,203,默认值为-1)
        /// </summary>
        [DataMember]
        public string TargetType { get; set; }

        /// <summary>
        /// 目标凭证代码
        /// </summary>
        [DataMember]
        public string TrgetEntry { get; set; }

        /// <summary>
        /// 基本凭证参考
        /// </summary>
        [DataMember]
        public string BaseRef { get; set; }

        /// <summary>
        /// 基本凭证类型(-1,0,23，17，16，13，165,默认值为-1)
        /// </summary>
        [DataMember]
        public string BaseType { get; set; }

        /// <summary>
        /// 基本凭证代码
        /// </summary>
        [DataMember]
        public string BaseEntry { get; set; }

        /// <summary>
        /// 基础行
        /// </summary>
        [DataMember]
        public string BaseLine { get; set; }
        [DataMember]
        public string U_WLLY { get; set; }
        [DataMember]
        public string U_YYFX { get; set; }
        [DataMember]
        public string U_ZXDH { get; set; }
        [DataMember]
        public string U_TYWP { get; set; }
        [DataMember]
        public string U_CPH { get; set; }
        [DataMember]
        public string U_TYSL { get; set; }
    }
}
