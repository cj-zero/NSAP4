namespace OpenAuth.WebApi.Model
{
    /// <summary>
    /// 键值对
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    public class KeyValue<TKey, TValue>
    {
        /// <summary>
        /// 键
        /// </summary>
        public TKey key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public TValue value { get; set; }

        public KeyValue(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }

        public KeyValue()
        {
        }
    }
}
