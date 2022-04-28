using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Model
{
    public class GrpcGetCycleDataInput:GrpcBaseInput
    {
        public GetCycleDataChlInput chl_info { get; set; }
        public GetCycleDataTestConditonInput test_conditon { get; set; }
        public GetCycleDataDataConditionInput data_condition { get; set; }

        public class GetCycleDataChlInput
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
            /// 中位机编号
            /// </summary>
            public uint dev_uid { get; set; }

            /// <summary>
            /// 单元ID
            /// </summary>
            public uint unit_id { get; set; }

            /// <summary>
            /// 通道ID
            /// </summary>
            public uint chl_id { get; set; }

        }


        public class GetCycleDataTestConditonInput
        {
            /// <summary>
            /// 测试编号,通过测试信息接口获取
            /// </summary>
            public ulong test_num { get; set; }

            /// <summary>
            /// 搜索起始时间 Unix时间戳 闭区间
            /// </summary>
            public ulong beg_time { get; set; }

            /// <summary>
            /// 搜索结束时间 Unix时间戳 闭区间
            /// </summary>
            public ulong end_time { get; set; }

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
            public uint is_fuzzy_matching { get; set; }
        }

        public class GetCycleDataDataConditionInput
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
            public uint cd_begin_cycle_id { get; set; }

            /// <summary>
            /// 循环结束编号 先充后放统计的循环 闭区间
            /// </summary>
            public uint cd_end_cycle_id { get; set; }

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
            /// 循环统计方式0:默认循环统计 1:先充后放统计循环 2:先放后充统计循环 默认值1
            /// </summary>
            public uint cycle_modle { get; set; }

            /// <summary>
            /// 需要返回的字段['test_id', 'dev_uid'...] 如果为空则返回所有字段
            /// </summary>
            public string[] field { get; set; }
        }
    }
}
