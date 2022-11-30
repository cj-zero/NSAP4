using System;

namespace OpenAuth.App.Request
{
    public class QueryContractSealListReq : PageReq
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 印章编号
        /// </summary>
        public string SealNo { get; set; }

        /// <summary>
        /// 上传人
        /// </summary>
        public string CreateUserId { get; set; }

        /// <summary>
        /// 印章说明
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 所属公司
        /// </summary>
        public string CompanyType { get; set; }

        /// <summary>
        /// 印章类型
        /// </summary>
        public string SealType { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime? EndDate { get; set; }
    }

    public class QuerySealHistoryReq : PageReq
    {
        /// <summary>
        /// Id
        /// </summary>
        public string SealId { get; set; }
    }
}