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
		/// <summary>
		/// 排序字段
		/// </summary>
		public string sortname { get; set; } = "upd_dt";
		/// <summary>
		/// 排序方式
		/// </summary>
		public string sortorder { get; set; } = "desc";
		public string types { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string Applicator { get; set; }
		/// <summary>
		/// 客户代码名称
		/// </summary>
		public string Customer { get; set; }
		/// <summary>
		/// 状态
		/// </summary>
		public string Status { get; set; }
		/// <summary>
		/// 开始时间
		/// </summary>
		public string BeginDate { get; set; }
		/// <summary>
		/// 结束时间
		/// </summary>
		public string EndDate { get; set; }
		/// <summary>
		/// 审批序号
		/// </summary>
		public string Job_Id { get; set; }
		/// <summary>
		/// 单据类型
		/// </summary>
		public string Job_Type_nm { get; set; }
		/// <summary>
		/// 状态
		/// </summary>
		public string Job_state { get; set; }
		/// <summary>
		/// 任务名称
		/// </summary>
		public string Job_nm { get; set; }
		/// <summary>
		/// 备注
		/// </summary>
		public string  Remarks { get; set; }
		/// <summary>
		/// 单据编号
		/// </summary>
		public string Sbo_itf_return { get; set; }
		/// <summary>
		/// 源单据编号
		/// </summary>
		public string Base_entry { get; set; }
	}
}
