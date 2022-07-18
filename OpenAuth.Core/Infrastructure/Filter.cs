namespace Infrastructure
{
   
    public class Filter
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Contrast { get; set; }
        public string Text { get; set; }
        public string Operation { get; set; }
        /// <summary>
        /// 左括号
        /// </summary>
        public string BracketLeft { get; set; }
        /// <summary>
        /// 右括号
        /// </summary>
        public string BracketRight { get; set; }
        /// <summary>
        /// 字段别名
        /// </summary>
        public string Alias { get; set; }
    }

    public class FilterGroup 
    {
        /// <summary>
        /// or /and
        /// </summary>
        public string Operation { get; set; }
        public Filter[] Filters { get; set; }
        public FilterGroup[] Children { get; set; }
    }

    public class FilterList
    {
        public Filter[] FilterUser { get; set; }
        public Filter[] FilterData { get; set; }
    }



}