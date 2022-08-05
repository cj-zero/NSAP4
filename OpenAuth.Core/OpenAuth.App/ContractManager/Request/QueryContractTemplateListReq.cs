using System;

namespace OpenAuth.App.Request
{
    public class QueryContractTemplateListReq : PageReq
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 合同模板编号
        /// </summary>
        public string TemplateNo { get; set; }

        /// <summary>
        /// 上传人
        /// </summary>
        public string CreateUserId { get; set; }

        /// <summary>
        /// 模板说明
        /// </summary>
        public string TemplateRemark { get; set; }

        /// <summary>
        /// 模板文件Id
        /// </summary>
        public string TemplateFileId { get; set; }

        /// <summary>
        /// 所属公司
        /// </summary>
        public string CompanyType { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}