//------------------------------------------------------------------------------
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

namespace OpenAuth.Repository.Domain.Sap
{
    /// <summary>
	/// 
	/// </summary>
    [Table("TSP1")]
    public partial class TSP1 : Entity
    {
        public TSP1()
        {
          this.VehicleTyp= string.Empty;
          this.VehicleNo= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? TransMode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VehicleTyp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VehicleNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LogInstanc { get; set; }
    }
}