using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class ShipmentCalibrationResp
    {
        /// <summary>
        /// 设备型号
        /// </summary>
        public string TesterModel { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public string TesterSn { get; set; }
        /// <summary>
        /// 出货校准完成时间
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// 出货校准操作人
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 出证人
        /// </summary>
        public string GiveWitness { get; set; }
        /// <summary>
        /// 销售单号
        /// </summary>
        public string SalesOrder { get; set; }

        /// <summary>
        /// 交货号
        /// </summary>
        public string DeliveryNumber { get; set; }

        /// <summary>
        /// 销售员
        /// </summary>
        public string Salesman { get; set; }
    }
}
