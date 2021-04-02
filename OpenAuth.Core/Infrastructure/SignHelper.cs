using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infrastructure
{
    public class SignHelper
    {
        private static string Key = "neware2021erp";

        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="dicParams">签名参数</param>
        /// <returns></returns>
        public static string Sign(Dictionary<string, string> dicParams)
        {
            //将字典中按ASCII码升序排序
            Dictionary<string, string> dicDestSign = new Dictionary<string, string>();
            dicDestSign = AsciiDictionary(dicParams);
            var sb = new StringBuilder();
            foreach (var sA in dicDestSign)//参数名ASCII码从小到大排序（字典序）；
            {
                if (string.IsNullOrEmpty(sA.Value) || string.Compare(sA.Key, "sign", true) == 0)
                {
                    continue;// 参数中为签名的项，不参加计算//参数的值为空不参与签名；
                }
                string value = sA.Value.ToString();

                sb.Append(sA.Key).Append("=").Append(sA.Value).Append("&");

            }
            sb.Append(Key);//在stringA最后拼接上key=(API密钥的值)得到stringSignTemp字符串
            var stringSignTemp = sb.ToString();
            var sign = Md5.Encrypt(stringSignTemp, "UTF-8").ToUpper();//对stringSignTemp进行MD5运算，再将得到的字符串所有字符转换为大写，得到sign值signValue。 
            return sign;
        }

        /// <summary>
        /// 将集合key以ascii码从小到大排序
        /// </summary>
        /// <param name="sArray">源数组</param>
        /// <returns>目标数组</returns>
        public static Dictionary<string, string> AsciiDictionary(Dictionary<string, string> sArray)
        {
            Dictionary<string, string> asciiDic = new Dictionary<string, string>();
            string[] arrKeys = sArray.Keys.ToArray();
            Array.Sort(arrKeys, string.CompareOrdinal);
            foreach (var key in arrKeys)
            {
                string value = sArray[key];
                asciiDic.Add(key, value);
            }
            return asciiDic;
        }
    }
}
