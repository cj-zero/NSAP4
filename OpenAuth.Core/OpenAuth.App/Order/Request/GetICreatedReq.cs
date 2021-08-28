using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
	public class GetICreatedReq : PageReq
	{
		public string qtype { get; set; }
		public string query { get; set; }
		public string sortname { get; set; }
		public string sortorder { get; set; }
		public string types { get; set; }
		public string Applicator { get; set; }
		public string Customer { get; set; }
		public string Status { get; set; }
		public string BeginDate { get; set; }
		public string EndDate { get; set; }
	}
}
