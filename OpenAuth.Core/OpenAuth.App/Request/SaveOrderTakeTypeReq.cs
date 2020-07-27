using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class SaveOrderTakeTypeReq
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 接单类型 0 未接单 1电话服务 2上门服务 3电话服务(已拨打)
        /// </summary>
        public int Type { get; set; }
    }
}
