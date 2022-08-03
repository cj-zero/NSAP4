using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 
    /// </summary>
    public class SubjectOptionResp
    {
        /// <summary>
        /// 选项 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 选项内容
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool is_choice { get; set; } = false;
    }
}
