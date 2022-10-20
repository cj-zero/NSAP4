using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.ClientRelation.Request
{
    public class JobReq
    {
        public int JobId { get; set; }

        public string ClientNo { get; set; }
    }

    public class ResignRelReq
    {
        /// <summary>
        /// 分配者（当前用户）编号
        /// </summary>
        public string userid { get; set; }
        /// <summary>
        /// 分配者（当前用户）
        /// </summary>
        public string username { get; set; }
        /// <summary>
        /// 业务员编号
        /// </summary>
        public string job_userid { get; set; }
        /// <summary>
        /// 业务员名字
        /// </summary>
        public string job_username { get; set; }
        /// <summary>
        /// 流程编号
        /// </summary>
        public int jobid { get; set; }
        /// <summary>
        /// 客户编号
        /// </summary>
        public string ClientNo { get; set; }
        /// <summary>
        /// 0.终端 1.中间商 2.中间商关联自己 
        /// </summary>
        public int flag { get; set; }
        /// <summary>
        /// 变更类型：0.新增中间商 1.新增终端 2.分配中间商 3.分配终端 4.公海领取中间商 5.公海领取终端  6.修改草稿 7.业务员修改客户
        /// </summary>
        public int OperateType { get; set; }
    }


    public class ResignOper
    {
        /// <summary>
        /// 客户编号
        /// </summary>
        public string ClientNo { get; set; }
        /// <summary>
        /// 新业务员编号
        /// </summary>
        public string TerminalList { get; set; }
    }


    public class ClientRelJob
    {
        public string customerNo { get; set; }

        public string customerName { get; set; }
    }

    public class JobScriptReq
    {
        public int JobId { get; set; }

        public string ClientNo { get; set; }

        public int Flag { get; set; }

        public string ClientName { get; set; }

        public string EndCustomerName { get; set; }

        public string Operator { get; set; }

        public string Operatorid { get; set; }

    }


}
