using System.Collections.Generic;
using Infrastructure;

namespace OpenAuth.App.Response
{
    /// <summary>
    /// 返回确定类型的表格数据，可以为swagger提供精准的注释
    /// </summary>
    public class TableResp<T>
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public int Code;
        /// <summary>
        /// 操作消息
        /// </summary>
        public string Msg;

        /// <summary>
        /// 总记录条数
        /// </summary>
        public int Count;

        /// <summary>
        ///  返回的列表头信息
        /// </summary>
        public List<KeyDescription> ColumnHeaders;

        /// <summary>
        /// 数据内容
        /// </summary>
        public List<T> Data;

        public TableResp()
        {
            Code = 200;
            Msg = "加载成功";
            ColumnHeaders = new List<KeyDescription>();
        }
    }
}