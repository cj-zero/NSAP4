using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Order;
using OpenAuth.App.Response;
using OpenAuth.Repository.Interface;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Order {
	/// <summary>
	/// 主物料
	/// </summary>
	[Route("api/Order/[controller]")]
	[ApiController]
	[ApiExplorerSettings(GroupName = "Order")]
	public class BillMaterialsController : Controller {
		private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
		IAuth _auth;
		IUnitWork UnitWork;
		ServiceBaseApp _serviceBaseApp;
		public BillMaterialsController(IUnitWork UnitWork, ServiceBaseApp _serviceBaseApp, IAuth _auth, ServiceSaleOrderApp serviceSaleOrderApp) {
			this.UnitWork = UnitWork;
			this._serviceBaseApp = _serviceBaseApp;
			this._auth = _auth;
			_serviceSaleOrderApp = serviceSaleOrderApp;
		}
		/// <summary>
		/// 查询物料的库存数据
		/// </summary>
		/// <param name="ItemCode"></param>
		/// <param name="Operating"></param>
		/// <param name="SboId"></param>
		/// <param name="IsOpenSap"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("SelectMaterialsInventoryData")]
		public async Task<TableData> SelectMaterialsInventoryData(string ItemCode, string Operating, string SboId, string IsOpenSap) {
			var result = new TableData();
			try {
				bool rIsOpenSap = IsOpenSap == "1" ? true : false;
				//if (Operating == "add") {
				//	result.Data = _serviceSaleOrderApp.SelectMaterialsInventoryData(ItemCode, SboId, rIsOpenSap, Operating);
				//} else {
				//	if (!string.IsNullOrEmpty(ItemCode))
				//		result.Data = _serviceSaleOrderApp.SelectMaterialsInventoryData(ItemCode, SboId, rIsOpenSap, Operating);
				//	else
				//		return null;
				//}
			} catch (Exception e) {
				result.Code = 500;
				result.Message = e.InnerException?.Message ?? e.Message;
				Log.Logger.Error($"地址：{Request.Path}，参数：'', 错误：{result.Message}");
			}
			return result;
		}

	}
}
