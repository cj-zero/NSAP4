using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.NsapBone
{
    /// <summary>
    /// 
    /// </summary>
    [Table("store_osrn_alreadyexists")]
    public class store_osrn_alreadyexists
    {
        public string ItemCode { get; set; }
        public int SysNumber { get; set; }
        public string DistNumber { get; set; }
        public string MnfSerial { get; set; }
        public string IsChange { get; set; }
        public DateTime UpdateDate { get; set; }
        public int JobId { get; set; }
    }
}
