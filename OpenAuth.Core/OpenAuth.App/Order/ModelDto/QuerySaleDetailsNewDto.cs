using OpenAuth.App.Response;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace OpenAuth.App.Order.ModelDto
{
    public class QuerySaleDetailsNewDto
    {
        public Main main { get; set; }
        public Manager manager { get; set; }
        public Sales sales { get; set; }
        public List<Mark> mark { get; set; }
        public ShipType shipType { get; set; }
        public PaymentCond paymentCond { get; set; }
        public Sales andbuy { get; set; }
        public DocCur docCur { get; set; }
        public CntctCode cntctCode { get; set; }
        public ShipToCode shipToCode { get; set; }
        public ShipToCode payToCode { get; set; }
        public string balance { get; set; }
        public string balanceS { get; set; }
        public string WhsCode { get; set; }
        public string billSalesName { get; set; }
        public string CustomFields { get; set; }
        public DataTable lineTable { get; set; }
        public DataTable CustomNew { get; set; }
        public DataTable Storehouse { get; set; }
        public DataTable AttachmentData { get; set; }
    }
    public class Main
    {
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 联系人代码
        /// </summary>
        public int CntctCode { get; set; }
        public string NumAtCard { get; set; }
        /// <summary>
        /// 货币类型
        /// </summary>
        public string DocCur { get; set; }
        /// <summary>
        /// 汇率
        /// </summary>
        public string DocRate { get; set; }
        public string DocNum { get; set; }
        public string DocType { get; set; }
        public string DocTotal { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DocDueDate { get; set; }
        public DateTime TaxDate { get; set; }
        public string SupplCode { get; set; }
        public string ShipToCode { get; set; }
        public string PayToCode { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// 销售员代码
        /// </summary>
        public int SlpCode { get; set; }
        public int TrnspCode { get; set; }
        public int GroupNum { get; set; }
        public string PeyMethod { get; set; }
        public string VatPercent { get; set; }
        public string LicTradNum { get; set; }
        public string Indicator { get; set; }
        public string PartSupply { get; set; }
        public string ReqDate { get; set; }
        public string CANCELED { get; set; }
        public string DpmPrcnt { get; set; }
        public string Printed { get; set; }
        public string DocStatus { get; set; }
        public int OwnerCode { get; set; }
        public string U_FPLB { get; set; }
        public string U_SL { get; set; }
        public string U_YGMD { get; set; }
        public string U_YWY { get; set; }
        public string sbo_id { get; set; }
        public string U_CallID { get; set; }
        public string U_CallName { get; set; }
        public string U_SerialNumber { get; set; }
        public string DocTotalFC { get; set; }
        public string DiscSum { get; set; }
        public string DiscSumFC { get; set; }
        public string DiscPrcnt { get; set; }


    }


    public class Manager {
        public string id { get; set; }
        public string name { get; set; }
    }
    public class Sales {
        public int id { get; set; }
        public string name { get; set; }
    }
    public class Mark {
        public string id { get; set; }
        public string name { get; set; }
    }
    public class ShipType {

        public string id { get; set; }
        public string name { get; set; }
 
    }
    public class PaymentCond
    {


        public int id { get; set; }
        public string name { get; set; }

    }
    public class DocCur
    {


        public string id { get; set; }
        public string name { get; set; }

    }
    public class CntctCode
    {


        public int id { get; set; }
        public string name { get; set; }

    }
    
    public class ShipToCode {
        public string id { get; set; }
        public string name { get; set; }
        public string ZipCode { get; set; }
        public string State { get; set; }

    }
}
