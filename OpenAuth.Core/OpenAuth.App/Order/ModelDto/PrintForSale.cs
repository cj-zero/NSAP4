using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.ModelDto
{
    public class PrintForSale
    {
        //页眉部分
        /// <summary>
        /// 评审单号
        /// </summary>
        public string contract_id { get; set; }
        /// <summary>
        /// 销售员
        /// </summary>
        public string SlpName { get; set; }
        /// <summary>
        /// 申请日期
        /// </summary>
        public string ApplyDate { get; set; }
        /// <summary>
        /// 交货日期
        /// </summary>
        public string DeliverDate { get; set; }
        /// <summary>
        /// 产品型号
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string ItemDesc { get; set; }
        /// <summary>
        /// 客户编号
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 客户类型
        /// </summary>
        public string CustomType { get; set; }

        /// <summary>
        /// 提成比例
        /// </summary>
        public string CommRate { get; set; }
        /// <summary>
        /// 每瓦单价
        /// </summary>
        public string Wattage { get; set; }
        /// <summary>
        /// 计量单位
        /// </summary>
        public string UnitMsr { get; set; }
        /// <summary>
        /// 加工费
        /// </summary>
        public string U_JGF { get; set; }
        /// <summary>
        /// 负电源个数
        /// </summary>
        public string U_FDY { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public string Price { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public string Quantity { get; set; }
        /// <summary>
        /// 总计
        /// </summary>
        public string SumTotal { get; set; }
        /// <summary>
        /// 总成本
        /// </summary>
        public string CostTotal { get; set; }
        /// <summary>
        /// 毛利
        /// </summary>
        public string Maori { get; set; }
        /// <summary>
        /// 检测能力
        /// </summary>
        public string DetectionCapability { get; set; }
        /// <summary>
        /// 客户特殊要求
        /// </summary>
        public string CustomReq { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }
        /// <summary>
        /// 项目情况说明
        /// </summary>
        public string ProjectDesc { get; set; }
        /// <summary>
        /// 机箱颜色
        /// </summary>
        public string CaseColor { get; set; }
        /// <summary>
        /// 机箱丝印
        /// </summary>
        public string CaseSilkPRT { get; set; }
        /// <summary>
        /// 最低放电电压
        /// </summary>
        public string MinDischargeVoltage { get; set; }
        /// <summary>
        /// 设备老化时间
        /// </summary>
        public string AgeingTime { get; set; }
        /// <summary>
        /// 主设备箱号
        /// </summary>
        public string MainCaseNo { get; set; }
        /// <summary>
        /// 设备精度要求（电压，电流）
        /// </summary>
        public string RequiredPrecision { get; set; }
        /// <summary>
        /// 中位机
        /// </summary>
        public string MiddleMachineType { get; set; }
        /// <summary>
        /// 辅助设备箱号
        /// </summary>
        public string AdditionalCaseNo { get; set; }
        /// <summary>
        /// 出线方式
        /// </summary>
        public string WiringMethod { get; set; }
        /// <summary>
        /// 中英文版本
        /// </summary>
        public string LanguageVer { get; set; }
        /// <summary>
        /// 配电选项
        /// </summary>
        public string DisOptions { get; set; }
        /// <summary>
        /// 其他特殊要求
        /// </summary>
        public string MainSpecialRequire { get; set; }
        /// <summary>
        /// 其他要求
        /// </summary>
        public string MainOtherRequire { get; set; }
        /// <summary>
        /// 辅助通道
        /// </summary>
        public string addchlinfo { get; set; }
        /// <summary>
        /// 压床
        /// </summary>
        public string pressmacinfo { get; set; }
        /// <summary>
        /// 高低温箱
        /// </summary>
        public string hltempinfo { get; set; }
        /// <summary>
        /// 网线
        /// </summary>
        public string EthernetCable { get; set; }
        /// <summary>
        /// 电源线
        /// </summary>
        public string PowerLine { get; set; }
        /// <summary>
        /// 转换插头
        /// </summary>
        public string PlugAdaptor { get; set; }
        /// <summary>
        /// 交换机
        /// </summary>
        public string SwitchBoard { get; set; }
        /// <summary>
        /// 通道电流线
        /// </summary>
        public string ChannelElectricCurrentLine { get; set; }
        /// <summary>
        /// 通道电压线
        /// </summary>
        public string ChannelVoltageLine { get; set; }
        /// <summary>
        /// 托盘
        /// </summary>
        public string Pallet { get; set; }
        /// <summary>
        /// 家具面板
        /// </summary>
        public string FixturePanel { get; set; }
        /// <summary>
        /// 龙门架
        /// </summary>
        public string Gantry { get; set; }
        /// <summary>
        /// 定制电池升降机/定制电池架
        /// </summary>
        public string CustomBatteryElevator { get; set; }
        /// <summary>
        /// 自定义防爆炸类型/定制防爆箱
        /// </summary>
        public string CustomAntiExplosionType { get; set; }
        /// <summary>
        /// 圆柱形电池信息尺寸
        /// </summary>
        public string CylindricalBatteryInfo_Size { get; set; }
        /// <summary>
        ///圆柱电池- 极柱结构
        /// </summary>
        public string CylindricalBatteryInfo_Struct { get; set; }
        /// <summary>
        /// 软包电池-尺寸
        /// </summary>
        public string SoftPackingBatteryInfo_Size { get; set; }
        /// <summary>
        /// /软包电池-极耳类型
        /// </summary>
        public string SoftPackingBatteryInfo_Jtype { get; set; }
        /// <summary>
        /// 方形电池-尺寸
        /// </summary>
        public string LiionBatteryInfo_Size { get; set; }
        /// <summary>
        /// 方形电池-极耳类型
        /// </summary>
        public string LiionBatteryInfo_Jtype { get; set; }
        /// <summary>
        /// 方形电池-极柱结构
        /// </summary>
        public string LiionBatteryInfo_struct { get; set; }
        /// <summary>
        /// 其它特殊要求
        /// </summary>
        public string CTSepecialRequire { get; set; }
        /// <summary>
        /// 极柱结构
        /// </summary>
        public string ProductBatteryInfo { get; set; }
        /// <summary>
        /// logo
        /// </summary>
        public string logo { get; set; }
        /// <summary>
        /// QRcode
        /// </summary>
        public string QRcode { get; set; }
        /// <summary>
        /// 排风方式
        /// </summary>
        public string  AirExhaustingMethod { get; set; }
        /// <summary>
        /// BOM单
        /// </summary>
        public List<BomCost> BomCost { get; set; }
        /// <summary>
        /// 通道温度线
        /// </summary>
        public string ChannelThermalCouple { get; set; }
    }
    /// <summary>
    /// BOM单
    /// </summary>
    public class BomCost
    {
        /// <summary>
        /// 类别
        /// </summary>
        public string ItemTypeName { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public string qty { get; set; }
        /// <summary>
        /// 计量单位
        /// </summary>
        public string UnitMsr { get; set; }
    }
}
