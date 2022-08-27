using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository.Domain.Sap
{
    /// <summary>
    /// 生产校准详情
    /// </summary>
    public class ProductionCalibration
    {
        /// <summary>
        /// 设备型号
        /// </summary>
        public string TesterModel { get; set; }
        /// <summary>
        /// 产品编号
        /// </summary>
        public string GeneratorCode { get; set; }
        /// <summary>
        /// 生产校准完成时间
        /// </summary>
        public DateTime? NwcailTime { get; set; }
        /// <summary>
        /// 生产校准操作人
        /// </summary>
        public string ReceiveOperator { get; set; }
        /// <summary>
        /// 生产部门
        /// </summary>
        public string Department { get; set; }
    }
}
