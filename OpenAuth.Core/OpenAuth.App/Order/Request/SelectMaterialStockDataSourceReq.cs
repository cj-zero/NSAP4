using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request {
	public class SelectMaterialStockDataSourceReq : PageReq {
		public string qtype { get; set; }
		public string query { get; set; }
		public string sortname { get; set; }
		public string sortorder { get; set; }
		public string SboId { get; set; }
		public string WhsCode { get; set; }
		public string ItemCode { get; set; }
		public string ItemOperaType { get; set; }
		public string IsOpenSap { get; set; }

	}
}
