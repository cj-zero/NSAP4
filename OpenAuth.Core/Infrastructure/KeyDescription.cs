namespace Infrastructure
{
    public class KeyDescription
    {
        /// <summary>
        /// 键值
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 键的描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 前端是否显示
        /// </summary>
        public bool Browsable { get; set; }
        
        /// <summary>
        /// 字段类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int? SortNo { get; set; }

        /// <summary>
        /// 字段权限 
        /// </summary>
        public string Permission { get; set; }

    }
}
