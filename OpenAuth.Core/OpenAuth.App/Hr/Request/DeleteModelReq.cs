using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr.Request
{
    /// <summary>
    /// 删除请求
    /// </summary>

    public class DeleteModelReq<T>
    {
        /// <summary>
        /// 待删除的Id
        /// </summary>
        public  T  Id  {get; set; }
    }
}
