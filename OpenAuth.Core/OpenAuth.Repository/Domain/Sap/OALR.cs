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
    [Table("OALR")]
    public partial class OALR : Entity
    {
        public OALR()
        {
          this.Type= string.Empty;
          this.Priority= string.Empty;
          this.Subject= string.Empty;
          this.UserText= string.Empty;
          this.DataParams= string.Empty;
          this.MsgData= string.Empty;
          this.Attachment= string.Empty;
          this.DataSource= string.Empty;
          this.AltType= string.Empty;
          this.U_PRX_SID= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Priority { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? TCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Subject { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UserText { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DataCols { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DataParams { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MsgData { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Attachment { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DataSource { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? AtcEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AltType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_PRX_SID { get; set; }
    }
}