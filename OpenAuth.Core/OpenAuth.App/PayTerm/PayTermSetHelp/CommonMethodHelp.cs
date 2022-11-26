using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using OpenAuth.Repository.Domain;

namespace OpenAuth.App.PayTerm.PayTermSetHelp
{
    public static class CommonMethodHelp
    {
        /// <summary>
        /// 文本序列化
        /// </summary>
        /// <param name="Text">文本</param>
        /// <returns>返回序列化后的文本</returns>
        public static string FilterSerialize(this string Text)
        {
            if (string.IsNullOrEmpty(Text)) return string.Empty;
            return Text.Replace("&#92;&#34;", "\\\"").FilterESC();
        }

        /// <summary>
        /// 文本
        /// </summary>
        /// <param name="Text">文本</param>
        /// <returns>返回文本信息</returns>
        public static string FilterESC(this string Text)
        {
            if (string.IsNullOrEmpty(Text)) return string.Empty;
            return Text.Replace("&#60;", "<").Replace("&#62;", ">").Replace("&#34;", "\"").Replace("&#39;", "'");
        }

        /// <summary>
        /// 字符串转对象
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="pJson">json字符串</param>
        /// <returns>返回泛型类对象</returns>
        public static T ParseModel<T>(string pJson)
        {
            T pObject = default(T);
            using (var pStream = new MemoryStream(Encoding.UTF8.GetBytes(pJson)))
            {
                pObject = (T)new DataContractJsonSerializer(typeof(T)).ReadObject(pStream);
            }
            return pObject;
        }

        /// <summary>
        /// 对象转字节
        /// </summary>
        /// <param name="oClass">实体类</param>
        /// <returns>返回字节信息</returns>
        public static byte[] Serialize(dynamic oClass)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, oClass);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// 获取时间单位
        /// </summary>
        /// <param name="unit">单位</param>
        /// <returns>返回时间单位</returns>
        public static string GetDateTimeUnit(string unit)
        {
            return unit == "D" ? "天" : (unit == "M" ? "月" : "年");
        }

        /// <summary>
        /// 获取百分比
        /// </summary>
        /// <param name="typeName">类型名称</param>
        /// <param name="groupNums">付款条件实体数据</param>
        /// <returns>返回收款条件类型对应的百分比</returns>
        public static decimal GetReceTypePerentage(string typeName, PayTermSave groupNums)
        {
            decimal perentage = 0;
            switch (typeName)
            {
                case "预付/货前款":
                    perentage = Convert.ToDecimal(groupNums.PrepaPro + groupNums.BefShipPro) / 100;
                    break;
                case "货到款":
                    perentage = Convert.ToDecimal(groupNums.GoodsToPro) / 100;
                    break;
                case "验收款":
                    perentage = Convert.ToDecimal(groupNums.AcceptancePayPro) / 100;
                    break;
                default:
                    perentage = Convert.ToDecimal(groupNums.QualityAssurancePro) / 100;
                    break;
            }

            return perentage;
        }

        /// <summary>
        /// 获取应收到期日期
        /// </summary>
        /// <param name="typeName">收款类型名称</param>
        /// <param name="originalDate">起始日期（发票创建日期）</param>
        /// <param name="groupNums">付款条件实体数据信息</param>
        /// <returns>返回各可用阶段应收到期日期信息</returns>
        public static DateTime GetReceTypeDate(string typeName, DateTime originalDate, PayTermSave groupNums)
        {
            DateTime receDate;
            int dateNumber = Convert.ToInt32(groupNums.SaleGoodsToDay);
            string dateUnit = groupNums.SaleGoodsToUnit;
            DateTime dt;
            switch (typeName)
            {
                case "预付/货前款":
                    receDate = originalDate;
                    break;
                case "货到款":
                    dt = dateUnit == "D" ? originalDate.AddDays((double)dateNumber) : (dateUnit == "M" ? originalDate.AddMonths((int)dateNumber): originalDate.AddYears((int)dateNumber));
                    receDate = (groupNums.GoodsToDay == 0 || groupNums.GoodsToDay == null) ? dt : groupNums.GoodsToUnit == "D" ? dt.AddDays((double)groupNums.GoodsToDay) : (groupNums.GoodsToUnit == "M" ? dt.AddMonths((int)groupNums.GoodsToDay) : dt.AddYears((int)groupNums.GoodsToDay));
                    break;
                case "验收款":
                    dt = dateUnit == "D" ? originalDate.AddDays((double)dateNumber) : (dateUnit == "M" ? originalDate.AddMonths((int)dateNumber) : originalDate.AddYears((int)dateNumber));
                    dt = (groupNums.AcceptancePayLimit == 0 || groupNums.AcceptancePayLimit == null) ? dt : (groupNums.AcceptancePayLimitUnit == "D" ? dt.AddDays((double)groupNums.AcceptancePayLimit) : (groupNums.AcceptancePayLimitUnit == "M" ? dt.AddMonths((int)groupNums.AcceptancePayLimit) : dt.AddYears((int)groupNums.AcceptancePayLimit)));
                    receDate = (groupNums.AcceptancePayDay == 0 || groupNums.AcceptancePayDay == null) ? dt : (groupNums.AcceptancePayDayUnit == "D" ? dt.AddDays((double)groupNums.AcceptancePayDay) : (groupNums.AcceptancePayDayUnit == "M" ? dt.AddMonths((int)groupNums.AcceptancePayDay) : dt.AddYears((int)groupNums.AcceptancePayDay))); 
                    break;
                default:
                    dt = dateUnit == "D" ? originalDate.AddDays((double)dateNumber) : (dateUnit == "M" ? originalDate.AddMonths((int)dateNumber) : originalDate.AddYears((int)dateNumber));
                    dt = (groupNums.QualityAssuranceLimit == 0 || groupNums.QualityAssuranceLimit == null) ? dt : (groupNums.QualityAssuranceLimitUnit == "D" ? dt.AddDays((double)groupNums.QualityAssuranceLimit) : (groupNums.QualityAssuranceLimitUnit == "M" ? dt.AddMonths((int)groupNums.QualityAssuranceLimit) : ((groupNums.QualityAssuranceLimit > (int)groupNums.QualityAssuranceLimit) ? (dt.AddYears((int)groupNums.QualityAssuranceLimit)).AddMonths(6) : dt.AddYears((int)groupNums.QualityAssuranceLimit))));
                    receDate = (groupNums.QualityAssuranceDay == 0 || groupNums.QualityAssuranceDay == null) ? dt : (groupNums.QualityAssuranceDayUnit == "D" ? dt.AddDays((double)groupNums.QualityAssuranceDay) : (groupNums.QualityAssuranceDayUnit == "M" ? dt.AddMonths((int)groupNums.QualityAssuranceDay) : dt.AddYears((int)groupNums.QualityAssuranceDay)));
                    break;
            }

            return receDate;
        }

        /// <summary>
        /// 获取日期
        /// </summary>
        /// <param name="typeName">收款类型名称</param>
        /// <param name="originalDate">起始日期（发票创建日期）</param>
        /// <param name="groupNums">付款条件实体数据信息</param>
        /// <returns>返回货到日期、验收日期、质保日期</returns>
        public static string GetReceTypeDetailDate(string typeName, DateTime originalDate, PayTermSave groupNums)
        {
            string receTypeDate = "";
            int dateNumber = Convert.ToInt32(groupNums.SaleGoodsToDay);
            string dateUnit = groupNums.SaleGoodsToUnit ;
            DateTime dt;
            switch (typeName)
            {
                case "预付/货前款":
                    break;
                case "货到款":
                    dt = dateUnit == "D" ? originalDate.AddDays((double)dateNumber) : (dateUnit == "M" ? originalDate.AddMonths((int)dateNumber) : originalDate.AddYears((int)dateNumber));
                    receTypeDate = "货到日期：" + dt.ToString("yyyy.MM.dd");
                    break;
                case "验收款":
                    dt = dateUnit == "D" ? originalDate.AddDays((double)dateNumber) : (dateUnit == "M" ? originalDate.AddMonths((int)dateNumber) : originalDate.AddYears((int)dateNumber));
                    dt = (groupNums.AcceptancePayLimit == 0 || groupNums.AcceptancePayLimit == null) ? dt : (groupNums.AcceptancePayLimitUnit == "D" ? dt.AddDays((double)groupNums.AcceptancePayLimit) : (groupNums.AcceptancePayLimitUnit == "M" ? dt.AddMonths((int)groupNums.AcceptancePayLimit) : dt.AddYears((int)groupNums.AcceptancePayLimit)));
                    receTypeDate = "验收日期：" + dt.ToString("yyyy.MM.dd");
                    break;
                default:
                    dt = dateUnit == "D" ? originalDate.AddDays((double)dateNumber) : (dateUnit == "M" ? originalDate.AddMonths((int)dateNumber) : originalDate.AddYears((int)dateNumber));
                    dt = (groupNums.QualityAssuranceLimit == 0 || groupNums.QualityAssuranceLimit == null) ? dt : (groupNums.QualityAssuranceLimitUnit == "D" ? dt.AddDays((double)groupNums.QualityAssuranceLimit) : (groupNums.QualityAssuranceLimitUnit == "M" ? dt.AddMonths((int)groupNums.QualityAssuranceLimit) : ((groupNums.QualityAssuranceLimit > (int)groupNums.QualityAssuranceLimit) ? (dt.AddYears((int)groupNums.QualityAssuranceLimit)).AddMonths(6) : dt.AddYears((int)groupNums.QualityAssuranceLimit))));
                    receTypeDate = "质保日期：" + dt.ToString("yyyy.MM.dd");
                    break;
            }

            return receTypeDate;
        }

        /// <summary>
        /// 判读逻辑表达式
        /// </summary>
        /// <param name="str">需要判断的逻辑表达式字符串</param>
        /// <returns></returns>
        public static Boolean LogicExpression(string str)
        {
            Boolean result = false;
            DataTable dt = new DataTable();
            string[] logicExData = Regex.Split(str, @"\|\||&&", RegexOptions.IgnoreCase);
            string logicStr = "||";
            for (int logicExIndex = 0; logicExIndex < logicExData.Length; logicExIndex++)
            {
                if (logicExIndex != 0)
                {
                    logicStr = str.Substring(str.IndexOf(logicExData[logicExIndex - 1]) + logicExData[logicExIndex - 1].Length, 2);
                }

                Boolean re = (Boolean)dt.Compute(logicExData[logicExIndex], "");
                result = LogicEx(result, re, logicStr);
            }

            return result;
        }

        /// <summary>
        /// 通过字符串来比较逻辑后结果
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="re"></param>
        /// <returns></returns>
        public static Boolean LogicEx(Boolean a1, Boolean a2, string re)
        {
            Boolean result = false;
            switch (re)
            {
                case "&&": result = a1 && a2; break;
                case "||": result = a1 || a2; break;
            }

            return result;
        }

        /// <summary>
        /// 表达式进行拆分，包括括号内的逻辑表达式以及没有括号的直接判断方法
        /// </summary>
        /// <param name="str">需要判断的表达式字符串</param>
        /// <returns></returns>
        public static Boolean ExpressionSplit(string str)
        {
            Boolean result = false;

            //首先判断括号，有多少个
            string[] parentheses = str.Split('(');

            //创建表达式判断函数
            DataTable dt = new DataTable();

            //有括号的情况
            if (parentheses.Length > 1)
            {
                //先判断括号里面的
                for (int parenthesesIndex = 1; parenthesesIndex < parentheses.Length; parenthesesIndex++)
                {
                    //括号里面的内容
                    string ParenthData = parentheses[parenthesesIndex].Split(')')[0];
                    if(!string.IsNullOrEmpty(ParenthData))
                    {
                        //进行替换
                        string reParenth = "(" + ParenthData + ")";
                        str = str.Replace(reParenth, LogicExpression(ParenthData).ToString());
                    }
                }

                //递归处理外层括号
                if (str.Contains("("))
                {
                    return ExpressionSplit(str);
                }

                //判断完括号里面后，在进行外部的判断
                string[] logicExData = Regex.Split(str, @"\|\||&&", RegexOptions.IgnoreCase);
                if (logicExData.Length > 1)
                {
                    string logicStr = "||";
                    for (int logicExIndex = 0; logicExIndex < logicExData.Length; logicExIndex++)
                    {
                        if (logicExIndex != 0)
                        {
                            logicStr = str.Substring(str.IndexOf(logicExData[logicExIndex - 1]) + logicExData[logicExIndex - 1].Length, 2);
                        }

                        Boolean re = (Boolean)dt.Compute(logicExData[logicExIndex], "");
                        result = LogicEx(result, re, logicStr);
                    }
                }
            }
            else
            {
                //没括号的情况，直接判断
                string[] logicExData = Regex.Split(str, @"\|\||&&", RegexOptions.IgnoreCase);
                if (logicExData.Length > 1)
                {
                    string logicStr = "||";
                    for (int logicExIndex = 0; logicExIndex < logicExData.Length; logicExIndex++)
                    {
                        if (logicExIndex != 0)
                        {
                            logicStr = str.Substring(str.IndexOf(logicExData[logicExIndex - 1]) + logicExData[logicExIndex - 1].Length, 2);
                        }

                        Boolean re = (Boolean)dt.Compute(logicExData[logicExIndex], "");
                        result = LogicEx(result, re, logicStr);
                    }
                }
                else
                {
                    result = (Boolean)dt.Compute(logicExData[0], "");
                }
            }

            return result;
        }

        /// <summary>
        /// 获取表达式
        /// </summary>
        /// <param name="Contrast">表达式值</param>
        /// <returns>返回表达式字符串</returns>
        public static string GetContrast(string Contrast)
        {
            string value = "";
            switch (Contrast)
            {
                case "=":
                    value = "等于";
                    break;
                case ">":
                    value = "大于";
                    break;
                case "<":
                    value = "小于";
                    break;
                case ">=":
                    value = "大于等于";
                    break;
                case "<=":
                    value = "小于等于";
                    break;
                case "<>":
                    value = "不等于";
                    break;
            }

            return value;
        }

        /// <summary>
        /// 逻辑运算结果
        /// </summary>
        /// <param name="Contrast">表达式</param>
        /// <param name="KeyValue">比较值</param>
        /// <param name="DocTotalVaue">被比较直</param>
        /// <returns>返回比较结果</returns>
        public static string GetContrastConvert(string Contrast, decimal KeyValue, decimal DocTotalVaue)
        {
            string value = "";
            switch (Contrast)
            {
                case "=":
                    value = KeyValue == DocTotalVaue ? "true" : "false";
                    break;
                case ">":
                    value = DocTotalVaue > KeyValue ? "true" : "false";
                    break;
                case "<":
                    value = DocTotalVaue < KeyValue ? "true" : "false";
                    break;
                case ">=":
                    value = DocTotalVaue >= KeyValue ? "true" : "false";
                    break;
                case "<=":
                    value = DocTotalVaue <= KeyValue ? "true" : "false";
                    break;
                default:
                    value = DocTotalVaue != KeyValue ? "true" : "false";
                    break;
            }

            return value;
        }
    }
}
