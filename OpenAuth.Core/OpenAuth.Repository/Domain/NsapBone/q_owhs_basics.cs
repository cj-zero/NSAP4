using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository.Domain.NsapBone
{
    /// <summary>
    /// 仓库部门和主管，非数据库表格
    /// </summary>
    public class q_owhs_basics
    {
        public q_owhs_basics()
        {
            
        }

        public string WhsCode { get; set; }
        /// <summary>
        /// 仓库名
        /// </summary>
        public string WhsName { get; set; }
        public string OrgName { get; set; }
        public string OrgManager { get; set; }
    }
}
