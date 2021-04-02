using System;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure
{
    public class Encryption
    {
        private static string encryptKey = "4h!@w$rng,i#$@x1%)5^3(7*5P31/Ee0";
        private static string Aeskey = "NEWARE2021032613";

        //默认密钥向量
        private static byte[] Keys = { 0x41, 0x72, 0x65, 0x79, 0x6F, 0x75, 0x6D, 0x79, 0x53, 0x6E, 0x6F, 0x77, 0x6D, 0x61, 0x6E, 0x3F };
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="encryptString"></param>
        /// <returns></returns>
        public static string Encrypt(string encryptString)
        {
            if (string.IsNullOrEmpty(encryptString))
                return string.Empty;
            RijndaelManaged rijndaelProvider = new RijndaelManaged();
            rijndaelProvider.Key = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 32));
            rijndaelProvider.IV = Keys;
            ICryptoTransform rijndaelEncrypt = rijndaelProvider.CreateEncryptor();

            byte[] inputData = Encoding.UTF8.GetBytes(encryptString);
            byte[] encryptedData = rijndaelEncrypt.TransformFinalBlock(inputData, 0, inputData.Length);

            return Convert.ToBase64String(encryptedData);
        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="decryptString"></param>
        /// <returns></returns>
        public static string Decrypt(string decryptString)
        {
            if (string.IsNullOrEmpty(decryptString))
                return string.Empty;
            try
            {
                RijndaelManaged rijndaelProvider = new RijndaelManaged();
                rijndaelProvider.Key = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 32));
                rijndaelProvider.IV = Keys;
                ICryptoTransform rijndaelDecrypt = rijndaelProvider.CreateDecryptor();

                byte[] inputData = Convert.FromBase64String(decryptString);
                byte[] decryptedData = rijndaelDecrypt.TransformFinalBlock(inputData, 0, inputData.Length);

                return Encoding.UTF8.GetString(decryptedData);
            }
            catch
            {
                return "";
            }
        }


        /// <summary>
        ///  AES 加密
        /// </summary>
        /// <param name="str">明文（待加密）</param>
        /// <param name="key">密文</param>
        /// <returns></returns>
        public static string PrintEncrypt(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            str = Aeskey+ str;
            Byte[] toEncryptArray = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(toEncryptArray);
        }

        /// <summary>
        ///  AES 解密
        /// </summary>
        /// <param name="str">明文（待解密）</param>
        /// <param name="key">密文</param>
        /// <returns></returns>
        public static string PrintDecrypt(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Convert.FromBase64String(str);
            var reult = Encoding.UTF8.GetString(toEncryptArray);
            reult=reult.Substring(16, reult.Length -16);
            return reult;
        }
    }
}
