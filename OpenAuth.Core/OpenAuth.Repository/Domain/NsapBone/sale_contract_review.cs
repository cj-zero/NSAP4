﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:Yubao Li
// </autogenerated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 
	/// </summary>
    [Table("sale_contract_review")]
    public partial class sale_contract_review
    {

        public int sbo_id { get; set; }
        public int contract_id { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public DateTime apply_dt { get; set; }
        public DateTime deliver_dt { get; set; }
        public int SlpCode { get; set; }
        public bool is_new { get; set; }
        public string ItemCode { get; set; }
        public string ItemDesc { get; set; }
        public decimal price { get; set; }
        public int qty { get; set; }
        public decimal cost_total { get; set; }
        public decimal sum_total { get; set; }
        public decimal maori { get; set; }
        public decimal comm_rate { get; set; }
        public decimal walts { get; set; }
        public string power_option { get; set; }
        public decimal min_discharge_volt { get; set; }
        public int is_create_drawing_task { get; set; }
        public int custom_type { get; set; }
        public string custom_req { get; set; }
        public string detection_capability { get; set; }
        public string remarks { get; set; }
        public int DocStatus { get; set; }
        public TimeSpan upd_dt { get; set; }
        public string UnitMsr { get; set; }
        public string U_JGF { get; set; }
        public string ProjectDesc { get; set; }
        public char ProductType { get; set; }
        public byte[] ProductProperty { get; set; }
        public int software_review_id { get; set; }
        public string PDF_FilePath { get; set; }
        public string PDF_FilePath_S { get; set; }


    }
}