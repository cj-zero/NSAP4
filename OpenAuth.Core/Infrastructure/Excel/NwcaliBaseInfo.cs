using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Excel
{
    /// <summary>
    /// 基础信息页
    /// </summary>
    public class NwcaliBaseInfo
    {
        /// <summary>
        /// 校准结束时间
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// 校准软件版本
        /// </summary>
        public string FileVersion { get; set; }
        /// <summary>
        /// 设备制造厂
        /// </summary>
        public string TesterMake { get; set; }
        /// <summary>
        /// 设备型号
        /// </summary>
        public string TesterModel { get; set; }
        /// <summary>
        /// 设备出厂编号
        /// </summary>
        public string TesterSn { get; set; }

        /// <summary>
        /// 资产编号
        /// </summary>
        public string AssetNo { get; set; }
        /// <summary>
        /// 校准地点
        /// </summary>
        public string SiteCode { get; set; }
        /// <summary>
        /// 相对温度
        /// </summary>
        public string Temperature { get; set; }
        /// <summary>
        /// 相对湿度
        /// </summary>
        public string RelativeHumidity { get; set; }
        /// <summary>
        /// 电流精度
        /// </summary>
        public double RatedAccuracyC { get; set; }
        /// <summary>
        /// 电压精度
        /// </summary>
        public double RatedAccuracyV { get; set; }

        public int AmmeterBits { get; set; }

        public int VoltmeterBits { get; set; }
        /// <summary>
        /// 证书编号
        /// </summary>
        public string CertificateNumber { get; set; }
        /// <summary>
        /// 校准机构
        /// </summary>
        public string CallbrationEntity { get; set; }
        /// <summary>
        /// 校准人
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// 校准类型
        /// </summary>
        public string CalibrationType { get; set; }

        /// <summary>
        /// 重复性测量次数
        /// </summary>
        public int RepetitiveMeasurementsCount { get; set; }

        /// <summary>
        /// TUR
        /// </summary>
        public string TUR { get; set; }
        /// <summary>
        /// 接受限
        /// </summary>
        public string AcceptedTolerance { get; set; }
        /// <summary>
        /// 包含因子K
        /// </summary>
        public double K { get; set; }
        /// <summary>
        /// 复校间隔
        /// </summary>
        public string TestInterval { get; set; }
        /// <summary>
        /// 校准设备
        /// </summary>
        public List<Etalon> Etalons { get; set; } = new List<Etalon>();
        /// <summary>
        /// 下位机
        /// </summary>
        public List<PCPLC> PCPLCs { get; set; } = new List<PCPLC>();
    }

    public class PCPLC
    {
        /// <summary>
        /// 下位机编号
        /// </summary>
        public int No { get; set; }
        /// <summary>
        /// 下位机GUID
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// 外观及工作正常性检查
        /// </summary>
        public string Comment { get; set; }

    }


    public class Etalon
    {
        /// <summary>
        /// 标准器名称/型号规格
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 计量特性
        /// </summary>
        public string Characteristics { get; set; }
        /// <summary>
        /// 设备编号
        /// </summary>
        public string AssetNo { get; set; }
        /// <summary>
        /// 证书号
        /// </summary>
        public string CertificateNo { get; set; }
        /// <summary>
        /// 有效期至
        /// </summary>
        public string DueDate { get; set; }
    }
}
