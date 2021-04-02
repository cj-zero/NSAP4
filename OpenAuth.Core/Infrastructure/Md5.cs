using System;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure
{
    public class Md5
    {
        public static string Encrypt(string str)
        {

            string pwd = String.Empty;

            MD5 md5 = MD5.Create();

            // 编码UTF8/Unicode　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(str));

            // 转换成字符串
            for (int i = 0; i < s.Length; i++)
            {
                //格式后的字符是小写的字母
                //如果使用大写（X）则格式后的字符是大写字符
                pwd = pwd + s[i].ToString("X");

            }

            return pwd;
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="encypStr">需要md5加密的字符串</param>
        /// <param name="charset">编码</param>
        /// <returns>返回加密后的MD5字符串</returns>
        public static string Encrypt(string encypStr, string charset = "UTF-8")
        {
            string retStr = string.Empty;
            MD5CryptoServiceProvider m5 = new MD5CryptoServiceProvider();

            //创建md5对象
            byte[] inputBye;
            byte[] outputBye;

            //使用GB2312编码方式把字符串转化为字节数组．
            try
            {
                inputBye = Encoding.GetEncoding(charset).GetBytes(encypStr);
            }
            catch
            {
                inputBye = Encoding.GetEncoding("GB2312").GetBytes(encypStr);
            }
            outputBye = m5.ComputeHash(inputBye);

            retStr = BitConverter.ToString(outputBye);
            retStr = retStr.Replace("-", "").ToUpper();
            return retStr;
        }
    }
}
