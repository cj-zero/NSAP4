using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
	public class SelectAccountsReq : PageReq
	{
		public string qtype { get; set; }
		public string query { get; set; }
		public string sortname { get; set; }//排序字段
		public string sortorder { get; set; }//排序顺序
		public string CardCode { get; set; }
		public string SlpCode { get; set; }//销售
		public string type { get; set; }
	}
}
