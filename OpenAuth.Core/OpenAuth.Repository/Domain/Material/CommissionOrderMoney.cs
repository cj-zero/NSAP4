using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Material
{
    [Table("commissionordermoney")]
    public class CommissionOrderMoney : BaseEntity<int>
    {
        public int CommissionOrderId { get; set; }
        public int Type { get; set; }
        public decimal Money { get; set; }
        public string Remarks { get; set; }


        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }


    }
}
