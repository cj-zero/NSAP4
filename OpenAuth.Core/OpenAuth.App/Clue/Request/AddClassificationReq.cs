using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.Request
{
    public class AddClassificationReq
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 父级Id
        /// </summary>
        public int ParentId { get; set; }
    }
}
