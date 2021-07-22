using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// 扩展类
    /// </summary>
    public class ByteExtension
    {
        /// <summary>
        /// 将对象转换为byte
        /// </summary>
        /// <param name="oclass"></param>
        /// <returns></returns>
        public static byte[] ToSerialize(dynamic oclass)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, oclass);
                return stream.ToArray();
            }
        }
        /// <summary>
        /// 将byte转换为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static T ToDeSerialize<T>(byte[] bytes)
        {
            T oClass = default(T);
            if (bytes.Length == 0 || bytes == null) return oClass;
            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter bs = new BinaryFormatter();
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)bs.Deserialize(stream);
            }
        }
    }
}
