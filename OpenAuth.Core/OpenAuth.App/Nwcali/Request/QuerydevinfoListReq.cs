namespace OpenAuth.App.Request
{
    /// <summary>
    /// 查询参数
    /// </summary>
    public class QueryDevInfoListReq : PageReq
    {
        /// <summary>
        /// 边缘计算Guid
        /// </summary>
        public string edge_id { get; set; }
        /// <summary>
        /// 上位机guid
        /// </summary>
        public string srv_guid { get; set; }
        /// <summary>
        /// 中位机guid
        /// </summary>
        public string mid_guid { get; set; }
        /// <summary>
        /// 订单唯一标识
        /// </summary>
        public string order_no { get; set; }
        /// <summary>
        /// BTS服务器IP
        /// </summary>
        public string bts_server_ip { get; set; }
        /// <summary>
        /// 中位机IP
        /// </summary>
        public string mid_ip { get; set; }
        /// <summary>
        /// 通道号id
        /// </summary>
        public int chl_id { get; set; }
        /// <summary>
        /// 物理通道id
        /// </summary>
        public int pyh_id { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        public int bts_type { get; set; }
        /// <summary>
        /// 设备id
        /// </summary>
        public int dev_uid { get; set; }
        /// <summary>
        /// 下位机号
        /// </summary>
        public short low_no { get; set; }
        /// <summary>
        /// 下位机guid
        /// </summary>
        public string low_guid { get; set; }
        /// <summary>
        /// 单元号
        /// </summary>
        public int unit_id { get; set; }
        /// <summary>
        /// 测试id
        /// </summary>
        public long test_id { get; set; }
        /// <summary>
        /// 操作人员
        /// </summary>
        public string test_user { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public long create_time { get; set; }
        /// <summary>
        /// 测试状态
        /// </summary>
        public int test_status { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public long? update_time { get; set; }
    }
}