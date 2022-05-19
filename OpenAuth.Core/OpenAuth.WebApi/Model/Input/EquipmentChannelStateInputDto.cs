using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Model
{
    public class EquipmentChannelStateInputDto
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        public List<long> equip_ids { get; set; }
    }
}
