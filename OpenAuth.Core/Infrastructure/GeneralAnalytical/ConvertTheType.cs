using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.GeneralAnalytical
{
    public class ConvertTheType
    {
        /// <summary>
        /// 
        /// </summary>
        public enum ConvertType
        {
            /// <summary>
            /// 转简体字
            /// </summary>
            FamiliarStyle = 1,
            /// <summary>
            /// 转繁体字
            /// </summary>
            ComplexFont = 2,
            /// <summary>
            /// 数字转中文大写
            /// </summary>
            UpperCase = 3,
            /// <summary>
            /// 数字转小写中文
            /// </summary>
            LowerCase = 4,
            /// <summary>
            /// 中文转拼音
            /// </summary>
            Spelling = 5,
            /// <summary>
            /// 数字转货币大写中文
            /// </summary>
            MonetaryCapital = 6,
            /// <summary>
            /// 数字转货币小写中文
            /// </summary>
            MonetaryLowercase = 7,
            /// <summary>
            /// 中文转数字
            /// </summary>
            TurnTheNumerical = 8,
            /// <summary>
            /// 数字转编号
            /// </summary>
            SerialNumber =9,
            /// <summary>
            /// 编号转数字
            /// </summary>
            NumberSerial =10
        }
        /// <summary>
        /// 
        /// </summary>
        public enum Different 
        {
            /// <summary>
            /// 第一种
            /// </summary>
            One=1,
            /// <summary>
            /// 第二种
            /// </summary>
            Two=2
        }
    }
}
