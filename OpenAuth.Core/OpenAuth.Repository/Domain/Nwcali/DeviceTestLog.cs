using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    [Table("devicetestlog")]
    public class DeviceTestLog : BaseEntity<long>
    {
        public string GeneratorCode { get; set; }

        public string EdgeGuid { get; set; }
        public string SrvGuid { get; set; }
        public string BtsServerIp { get; set; }
        public string MidGuid { get; set; }
        public string LowGuid { get; set; }
        public int DevUid { get; set; }
        public int UnitId { get; set; }
        public int Status { get; set; }
        public long TestId { get; set; }
        public DateTime CreateTime { get; set; }
        public int ChlId { get; set; }
        public string CreateUserId { get; set; }
        public string CreateUser { get; set; }
        public long OrderNo { get; set; }
        public string Department { get; set; }
        public DateTime ChangeStatusTime { get; set; }
        public int StepId { get; set; }
        public string CodeTxt { get; set; }
        public int StepCount { get; set; }
        public string PrtCode { get; set; }
        public int MaxRange { get; set; }
        public string FileIds { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public override void GenerateDefaultKeyVal()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
