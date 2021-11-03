using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Wrod
{
    public class WordModel
    {
        /// <summary>
        /// 0 正文；1页眉；2页脚
        /// </summary>
        public int MarkPosition { get; set; }

        /// <summary>
        /// 下标从 1 开始
        /// </summary>
        public int TableMark { get; set; }

        /// <summary>
        /// 下标从 1 开始
        /// </summary>
        public int XCellMark { get; set; }

        /// <summary>
        /// 下标从 1 开始
        /// </summary>
        public int YCellMark { get; set; }

        /// <summary>
        /// 0文本(string)；1图片(string)；2列表(datatable)；3html字符串（string）
        /// </summary>
        public int ValueType { get; set; }

        public object ValueData { get; set; }
        //public FileData Filedata { get; set; }

        public int FileXCellMark { get; set; }
        public int FileYCellMark { get; set; }
    }

    public class WordMarkModel
    {
        /// <summary>
        /// 标签
        /// </summary>
        public string MarkName { get; set; }
        /// <summary>
        /// 0文本(string)；1图片(string)；2列表(datatable)；3html字符串（string）
        /// </summary>
        public int MarkType { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public object MarkValue { get; set; }
    }
}
