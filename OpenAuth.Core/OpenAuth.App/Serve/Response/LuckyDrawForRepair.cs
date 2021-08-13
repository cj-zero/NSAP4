using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    /// <summary>
    /// 
    /// </summary>
    public class LuckyDrawForRepair
    {

    }
    /// <summary>
    /// 
    /// </summary>
    public class LuckyServiceOrder
    {
        /// <summary>
        /// 服务id
        /// </summary>
        public int U_SAP_ID { get; set; }
        /// <summary>
        /// 序列号集合
        /// </summary>
        public List<string> ManufacturerSerialNumberList { get; set; }
        /// <summary>
        /// 呼叫主题编号集合
        /// </summary>
        public List<LuckyFromTheme> CodeList { get; set; }
    }
    /// <summary>
    /// 序列号对应的呼叫主题集合
    /// </summary>
    public class LuckyFromTheme
    {
        /// <summary>
        /// 序列号
        /// </summary>
        public string ManufacturerSerialNumber { get; set; }
        /// <summary>
        /// 呼叫主题集合
        /// </summary>
        public List<string> code_list { get; set; }
    }
    /// <summary>
    /// 呼叫主题实体
    /// </summary>
    public class LuckyFromThemeModel
    {
        /// <summary>
        /// id
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        public string code { get; set; }
    }
}
