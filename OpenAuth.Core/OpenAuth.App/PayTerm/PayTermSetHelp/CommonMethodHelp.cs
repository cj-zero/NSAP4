using System;
using System.Text;
using System.Collections.Generic;
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
    }
}
