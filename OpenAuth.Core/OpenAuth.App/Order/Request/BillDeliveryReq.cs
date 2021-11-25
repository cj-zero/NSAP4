using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace OpenAuth.App.Order.Request {
	/// <summary>
	/// 单据附件
	/// </summary>
	[Serializable]
	[DataContract]
	public class BillDeliveryReq {


		/// <summary>
		/// 附件类型Id
		/// </summary>
		[DataMember]
		public uint filetypeId { get; set; }

		/// <summary>
		/// 单号
		/// </summary>
		[DataMember]
		public uint docEntry { get; set; }
		/// <summary>
		/// 详情
		/// </summary>
		[DataMember]
		public List<billDeliveryDeatil> deatil { get; set; }
	}
	public class billDeliveryDeatil {
		/// <summary>
		/// 附件名称
		/// </summary>
		[DataMember]
		public string filename { get; set; }
		/// <summary>
		/// 附件备注
		/// </summary>
		[DataMember]
		public string remarks { get; set; }
		/// <summary>
		/// 附件下载路径
		/// </summary>
		[DataMember]
		public string filepath { get; set; }
		/// <summary>
		/// 帐套ID
		/// </summary>
		[DataMember]
		public uint filesboid { get; set; }
	}
}
