using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class SaveWorkOrderTakeTypeReq
    {
        /// <summary>
        /// SapId
        /// </summary>
        [Required]
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// 接单类型 0 未接单 1电话服务 2上门服务 3电话服务(已拨打)
        /// </summary>
        [Required]
        public int Type { get; set; }

        /// <summary>
        /// 当前技术员Id
        /// </summary>
        [Required]
        public int CurrentUserId { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        [Required]
        public string MaterialType { get; set; }
    }
}
