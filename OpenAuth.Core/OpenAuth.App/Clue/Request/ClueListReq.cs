using OpenAuth.App.Request;

namespace OpenAuth.App.Clue.Request
{
    /// <summary>
    /// 查询
    /// </summary>
    public class ClueListReq : PageReq
    {
        /// <summary>
        /// 账套ID
        /// </summary>
        public int SboId { get; set; }
        /// <summary>
        /// 线索代码或者名称
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string Contacts { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 创建开始时间
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// 创建结束时间
        /// </summary>
        public string EndTime { get; set; }
    }
}
