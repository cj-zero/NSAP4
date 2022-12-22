using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Core;


namespace OpenAuth.Repository.Domain
{
    [Table("RateAnnix")]
    public  class RateAnnix : BaseEntity<int>
    {
        public string Level { get; set; }

        public string Name { get; set; }

        public string LowDifficulty { get; set; }

        public string MediumDifficulty { get; set; }

        public string HighDifficulty { get; set; }

        public string SuperDifficulty { get; set; }

        public string OnTime { get; set; }

        public string Delayed { get; set; }

        public string Total { get; set; }

        public string TotalSc { get; set; }

        public string TotalQ { get; set; }

        public string OnTimePer { get; set; }

        public string OnTimeSc { get; set; }

        public string OverQ { get; set; }

        public string OverSc { get; set; }

        public string Score { get; set; }

        public string TotalScore { get; set; }

        public string Rank { get; set; }

        public string Contribution { get; set; }

        public string Product { get; set; }

        public string Month { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        [Description("是否删除")]
        public int IsDelete { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [Description("创建日期")]
        public System.DateTime CreateDate { get; set; }

        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }

    }
}
