using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.ModelDto
{
    /// <summary>
    /// 地址
    /// </summary>
    public class AddressDto
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 邮编
        /// </summary>
        public string ZipCode { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string State { get; set; }
    }
    /// <summary>
    /// 地址类型
    /// </summary>
    public class AddresTypeDto
    {
        /// <summary>
        /// B： 付款方
        /// S:  收货方
        /// </summary>
        public string AddresType { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        public List<AddressDto> Address { get; set; }
    }
}
