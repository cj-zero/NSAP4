using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
	public class GridRelORDRReq : PageReq
	{
		/// <summary>
		/// 排序字段
		/// </summary>
		public string sortname { get; set; } = "docentry";
		/// <summary>
		/// 升序还是降序
		/// </summary>
		public string sortorder { get; set; }
		/// <summary>
		/// 销售员代码
		/// </summary>
		public string SlpCode { get; set; }
		/// <summary>
		/// 订单编号
		/// </summary>
		public string DocEntry { get; set; }
		/// <summary>
		///客户代码
		/// </summary>
		public string cardcode { get; set; }
	}
}
