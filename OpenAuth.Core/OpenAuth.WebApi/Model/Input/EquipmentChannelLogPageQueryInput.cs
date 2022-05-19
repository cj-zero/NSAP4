using System.Collections.Generic;

namespace OpenAuth.WebApi.Model
{
    public class EquipmentChannelLogPageQueryInput : BasePageInput
    {
        /// <summary>
        /// 设备id
        /// </summary>
        public long equip_id { get; set; }
        /// <summary>
        /// 单元号
        /// </summary>
        public int? unit_id { get; set; }
        /// <summary>
        /// 通道号
        /// </summary>
        public int? chl_id { get; set; }
        /// <summary>
        /// 单元和通道号键值对集合（key单元号  value通道号；若有值，则单独传入的单元和通道字段无效）
        /// </summary>
        public List<KeyValue<int?, int?>>? kv_list { get; set; }

        /// <summary>
        /// 时间起秒
        /// </summary>
        public long? pub_time { get; set; }
        /// <summary>
        /// 时间止秒
        /// </summary>
        public long? pub_time_e { get; set; }
    }
}
