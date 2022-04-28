using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Model
{
    public class GrpcTestInfoInput : GrpcBaseInput
    {
        public chl_info chl_info { get; set; }
        public test_info_condition test_condition { get; set; }

    }

    public class chl_info
    {
        /// <summary>
        /// 上位机guid
        /// </summary>
        public string srv_guid { get; set; }
        /// <summary>
        /// 上位机ip
        /// </summary>
        public string srv_ip { get; set; }
        /// <summary>
        /// 中位机编号,如:240001
        /// </summary>
        public int dev_uid { get; set; }
        /// <summary>
        /// 单元id
        /// </summary>
        public int unit_id { get; set; }
        /// <summary>
        /// 通道id
        /// </summary>
        public int chl_id { get; set; }
    }
    public class test_info_condition
    {
        /// <summary>
        /// 搜索起始时间 Unix时间戳 闭区间
        /// </summary>
        public ulong test_begin_time { get; set; }
        /// <summary>
        /// 搜索起始时间 Unix时间戳 闭区间
        /// </summary>
        public ulong test_end_time { get; set; }
        /// <summary>
        /// 条码
        /// </summary>
        public string barcode { get; set; }
        /// <summary>
        /// 创建者
        /// </summary>
        public string creator { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string desc { get; set; }
        /// <summary>
        /// 是否模糊匹配,0:启用 1:禁用，默认值0，只对条码、创建者、备注有效
        /// </summary>
        public int is_fuzzy_matching { get; set; }
        /// <summary>
        /// 起始行
        /// </summary>
        public uint beg_row { get; set; }
        /// <summary>
        /// 返回条数现在
        /// </summary>
        public uint row_limit { get; set; }
        /// <summary>
        /// 需要返回的字段['test_id', 'dev_uid'...]如果为空则返回所有字段
        /// </summary>
        public string[] field { get; set; }
    }
}



