using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class QueryProductionOrderReq : PageReq
    {
        public QueryProductionOrderReq()
        {
        }
        //public List<Equipment> Equipments { get; set; }

        /// <summary>
        /// 前缀
        /// </summary>
        public List<string> Prefix { get; set; }

        /// <summary>
        /// 系列
        /// </summary>
        public List<string> Series { get; set; }
        /// <summary>
        /// 电压开始
        /// </summary>
        public int? VoltsStart { get; set; }
        /// <summary>
        /// 电压结束
        /// </summary>
        public int? VoltseEnd { get; set; }
        /// <summary>
        /// 电流单位
        /// </summary>
        public string CurrentUnit { get; set; }
        /// <summary>
        /// 夹具
        /// </summary>
        public List<string> Fixture { get; set; }
        /// <summary>
        /// 电流开始
        /// </summary>
        public int? AmpsStart { get; set; }
        /// <summary>
        /// 电流结束
        /// </summary>
        public int? AmpsEnd { get; set; }
        /// <summary>
        /// 特殊要求
        /// </summary>
        public List<string> Special { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        //public string MaterialDescription { get; set; }
        /// <summary>
        /// 仓库
        /// </summary>
        //public string[] WareHouse { get; set; }
        /// <summary>
        /// 开始执行时间
        /// </summary>
        public DateTime? StartExcelTime { get; set; }
        /// <summary>
        /// 结束执行时间
        /// </summary>
        public DateTime? EndExcelTime { get; set; }

        public string FromTheme { get; set; }
        public string FromThemeList { get; set; }
        public string SelectList { get; set; }

        public string CardCodes { get; set; }
        public string SaleOrderNo { get; set; }
    }

    //public class MaterialInfo
    //{
    //}

    public class Equipment 
    {
        /// <summary>
        /// 前缀
        /// </summary>
        public List<string> Prefix { get; set; }

        /// <summary>
        /// 系列
        /// </summary>
        public List<int> Series { get; set; }
        /// <summary>
        /// 电压开始
        /// </summary>
        public int VoltsStart { get; set; }
        /// <summary>
        /// 电压结束
        /// </summary>
        public int VoltseEnd { get; set; }
        /// <summary>
        /// 电流开始
        /// </summary>
        public int AmpsStart { get; set; }
        /// <summary>
        /// 电流结束
        /// </summary>
        public int AmpsEnd { get; set; }
        /// <summary>
        /// 特殊要求
        /// </summary>
        public List<string> Special { get; set; }
    }


    public class QueryProduction 
    {
        public List<Versions> Versions { get; set; }
        /// <summary>
        /// 开始执行时间
        /// </summary>
        public DateTime StartExcelTime { get; set; }
        /// <summary>
        /// 结束执行时间
        /// </summary>
        public DateTime EndExcelTime { get; set; }

        public string FromTheme { get; set; }

        public string ItemCode { get; set; }
    }

    public class Versions
    {
        /// <summary>
        /// 物料
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// BOM版本
        /// </summary>
        public string Version { get; set; }
    }

    public class QueryCustomerOrder : PageReq
    {
        public List<string> CardCode { get; set; }
        public int? SaleCode { get; set; }
        public string CustomerId { get; set; }
    }
}
