using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.Request
{
    public class AddTag
    {
        public int ClueId { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public List<string> Tag { get; set; } 
    }
}
