using Chinese;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Infrastructure.GeneralAnalytical.ConvertTheType;

namespace Infrastructure.GeneralAnalytical
{
    public class TransformCharOrNumber
    {
        /// <summary>
        /// 转换通用方法 by zlg 2020.09.10
        /// </summary>
        /// <param name="number"></param>
        /// <param name="text"></param>
        /// <param name="Type"></param>
        /// <param name="different"></param>
        /// <returns></returns>
        public static string SumConvert(string text ="",decimal number = 0,ConvertType Type = ConvertType.MonetaryCapital ,Different different =Different.One) 
        {
            var options = new ChineseNumberOptions();
            switch (Type,different)
            {
                case (ConvertType.FamiliarStyle,Different.One):
                    return ChineseConverter.ToSimplified(text);     // "免费"

                case (ConvertType.ComplexFont, Different.One):
                    return ChineseConverter.ToTraditional(text);    // "免費"

                case (ConvertType.UpperCase,Different.One):
                    options = new ChineseNumberOptions { Simplified = false, Upper = true };
                    return ChineseNumber.GetString(number, options);  // "壹拾万零壹"

                case (ConvertType.UpperCase, Different.Two):
                    options = new ChineseNumberOptions { Simplified = true, Upper = true };
                    return ChineseNumber.GetString(number, options); // "拾万零壹"

                case (ConvertType.LowerCase, Different.One):
                    options = new ChineseNumberOptions { Simplified = false, Upper = false };
                    return ChineseNumber.GetString(number, options); // "一十万零一"

                case (ConvertType.LowerCase, Different.Two):
                    options = new ChineseNumberOptions { Simplified = true, Upper = false };
                    return ChineseNumber.GetString(number, options);  // "十万零一"

                case (ConvertType.Spelling, Different.One):
                    return Pinyin.GetString(text, PinyinFormat.WithoutTone); // "mian fei"

                case (ConvertType.MonetaryCapital, Different.One):
                    options = new ChineseNumberOptions { Simplified = false, Upper = true };
                    return ChineseCurrency.GetString(number, options); // "壹拾万零壹圆整"

                case (ConvertType.MonetaryCapital, Different.Two):
                    options = new ChineseNumberOptions { Simplified = true, Upper = true };
                    return ChineseCurrency.GetString(number, options);  // "拾万零壹圆整"

                case (ConvertType.MonetaryLowercase, Different.One):
                    options = new ChineseNumberOptions { Simplified = false, Upper = false };
                    return ChineseCurrency.GetString(number, options); // "一十万零一元整"

                case (ConvertType.MonetaryLowercase, Different.Two):
                    options = new ChineseNumberOptions { Simplified = true, Upper = false };
                    return ChineseCurrency.GetString(number, options); // "十万零一元整"

                case (ConvertType.TurnTheNumerical,Different.One):
                    return ChineseCurrency.GetNumber(text).ToString();  // 10_0001

                case (ConvertType.SerialNumber, Different.One):
                    return ChineseNumber.GetCodeString(number.ToString(), upper: false); // "一〇〇〇〇一"

                case (ConvertType.SerialNumber, Different.Two):
                    return ChineseNumber.GetCodeString(number.ToString(), upper: true); // "壹零零零零壹"

                case (ConvertType.NumberSerial, Different.One):
                    return ChineseNumber.GetCodeNumber(text); // "100001"
            }
            return null;
        }
    }
}
