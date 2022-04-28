using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Model
{
    /// <summary>
    /// 获取工步数据
    /// </summary>
    public class GrpcGetStepDataInput:GrpcBaseInput
    {
        public GetStepDataChlInput chl_info { get; set; } = null;

        public GetStepDataTestConditionInput test_condition { get; set; } = null;

        public GetStepDataDataConditionInput data_condition { get; set; } = null;

        public class GetStepDataChlInput
        {
            /// <summary>
            /// 上位机GUID
            /// </summary>
            public string srv_guid { get; set; }

            /// <summary>
            /// 上位机IP
            /// </summary>
            public string srv_ip { get; set; }

            /// <summary>
            /// 中位机编号,如:240001
            /// </summary>
            public int dev_uid { get; set; }

            /// <summary>
            /// 单元ID
            /// </summary>
            public int unit_id { get; set; }

            /// <summary>
            /// 通道ID
            /// </summary>
            public int chl_id { get; set; }

        }


        public class GetStepDataTestConditionInput
        {
            /// <summary>
            /// 测试编号
            /// </summary>
            public ulong test_num { get; set; }

            /// <summary>
            /// 搜索起始时间Unix时间戳闭区间
            /// </summary>
            public ulong test_beg_time { get; set; }

            /// <summary>
            /// 搜索结束时间Unix时间戳闭区间
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
            /// 是否模糊匹配,0:启用 1:禁用 默认值0 只对条码、创建者、备注有效
            /// </summary>
            public int is_fuzzy_matching { get; set; }
        }


        public class GetStepDataDataConditionInput
        {
            /// <summary>
            /// 数据起始时间Unix时间戳 只要与对应的工步有交集则获取 闭区间
            /// </summary>
            public ulong data_begin_time { get; set; }

            /// <summary>
            /// 数据结束时间Unix时间戳 只要与对应的工步有交集则获取 闭区间
            /// </summary>
            public ulong data_end_time { get; set; }

            /// <summary>
            /// 循环起始编号 先充后放统计的循环 闭区间
            /// </summary>
            public int cd_begin_cycle_id { get; set; }

            /// <summary>
            /// 循环结束编号 先充后放统计的循环 闭区间
            /// </summary>
            public int cd_end_cycle_id { get; set; }

            /// <summary>
            /// 循环起始编号 先放后充统计的循环闭区间
            /// </summary>
            public int dc_begin_cycle_id { get; set; }

            /// <summary>
            /// 循环结束编号 先放后充统计的循环闭区间
            /// </summary>
            public int dc_end_cycle_id { get; set; }

            /// <summary>
            /// 起始工步号闭区间
            /// </summary>
            public int begin_step_num { get; set; }

            /// <summary>
            /// 结束工步号闭区间
            /// </summary>
            public int end_step_num { get; set; }

            /// <summary>
            /// 起始记录序号 只要与对应的工步有交集则获取 闭区间
            /// </summary>
            public uint begin_seq_id { get; set; }

            /// <summary>
            /// 结束记录序号 只要与对应的工步有交集则获取 闭区间
            /// </summary>
            public uint end_seq_id { get; set; }

            /// <summary>
            /// 工步类型[4,2,7...]
            /// </summary>
            public int[] step_type { get; set; }

            /// <summary>
            /// 起始行
            /// </summary>
            public uint beg_row { get; set; }

            /// <summary>
            /// 返回条数限制
            /// </summary>
            public uint row_limit { get; set; }

            /// <summary>
            /// 需要返回的字段['test_id', 'dev_uid'...] 如果为空则返回所有字段
            /// </summary>
            public string[] field { get; set; }
        }
    }
}
