using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class CertinfoView
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 证书编号
        /// </summary>
        public string CertNo { get; set; }
        /// <summary>
        /// 证书编号
        /// </summary>
        public string EncryptCertNo { get; set; }
        /// <summary>
        /// 证书审批状态
        /// </summary>
        public string ActivityName { get; set; }
        /// <summary>
        /// 设备型号
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// 设备出厂编号
        /// </summary>
        public string Sn { get; set; }
        /// <summary>
        /// 资产编号
        /// </summary>
        public string AssetNo { get; set; }
        /// <summary>
        /// 校准人
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime CreateTime { get; set; }
        /// <summary>
        /// 校准日期
        /// </summary>
        public DateTime? CalibrationDate { get; set; }
        /// <summary>
        /// 复校时间
        /// </summary>
        public DateTime? ExpirationDate { get; set; }
        /// <summary>
        /// 工作流程绑定Id
        /// </summary>
        public string FlowInstanceId { get; set; }
        /// <summary>
        /// 驳回原因
        /// </summary>
        public string RejectContent { get; set; }
        public int? IsFinish { get; set; }
        public string Issuer { get; set; }
        public string PdfPath { get; set; }


    }
}
