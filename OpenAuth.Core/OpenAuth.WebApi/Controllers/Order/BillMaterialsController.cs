using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Order;
using OpenAuth.App.Order.ModelDto;
using OpenAuth.App.Response;
using OpenAuth.Repository.Interface;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
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
		public TableData SelectMaterialsInventoryData(string ItemCode, string Operating, string SboId, string IsOpenSap) {
			var result = new TableData();
			DataTable data = new DataTable();
			try {
				bool rIsOpenSap = IsOpenSap == "1" ? true : false;
				if (Operating == "add") {
					data = _serviceSaleOrderApp.SelectMaterialsInventoryData(ItemCode, SboId, rIsOpenSap, Operating);
					result.Data = data;
					result.Count = data.Rows.Count;
				} else {
					if (!string.IsNullOrEmpty(ItemCode)) {
						data = _serviceSaleOrderApp.SelectMaterialsInventoryData(ItemCode, SboId, rIsOpenSap, Operating);
						result.Data = data;
						result.Count = data.Rows.Count;
					} else { return null; }

				}
			} catch (Exception e) {
				result.Code = 500;
				result.Message = e.InnerException?.Message ?? e.Message;
				Log.Logger.Error($"地址：{Request.Path}，参数：'', 错误：{result.Message}");
			}
			return result;
		}
		/// <summary>
		/// 查询单个物料信息
		/// </summary>
		[HttpGet]
		[Route("SelectSingleStoreOitmInfo")]
		public Response<SelectSingleStoreOitmInfoDto> SelectSingleStoreOitmInfo(string ItemCode, string SboId, string IsOpenSap) {
			var result = new Response<SelectSingleStoreOitmInfoDto>();
			bool rIsOpenSap = IsOpenSap == "1" ? true : false;
			result.Result = _serviceSaleOrderApp.SelectSingleStoreOitmInfo(ItemCode, SboId, rIsOpenSap);
			return result;
		}
		/// <summary>
		/// 获取物料类型的示例
		/// </summary>
		[HttpGet]
		[Route("GetItemTypeExpInfo")]
		public Response<GetItemTypeExpInfoDto> GetItemTypeExpInfo(string TypeId) {
			var result = new Response<GetItemTypeExpInfoDto>();

			if (!string.IsNullOrEmpty(TypeId)) {
				result.Result = _serviceSaleOrderApp.GetItemTypeExpInfo(TypeId);
			} else {
				return null;
			}

			return result;
		}
		/// <summary>
		/// 获取物料类型的自定义字段
		/// </summary>
		[HttpGet]
		[Route("GetItemTypeCustomFields")]
		public Response<List<GetItemTypeCustomFieldsDto>> GetItemTypeCustomFields(string TypeId) {
			var result = new Response<List<GetItemTypeCustomFieldsDto>>();

			if (!string.IsNullOrEmpty(TypeId)) {
				result.Result = _serviceSaleOrderApp.GetItemTypeCustomFields(TypeId);
			} else {
				return null;
			}

			return result;
		}
		/// <summary>
		/// 获取物料类型的自定义字段 — 有效值
		/// </summary>
		[HttpGet]
		[Route("GetItemTypeCustomValue")]
		public Response<List<GetItemTypeCustomValueDto>> GetItemTypeCustomValue(string TypeId, string FieldNm) {
			var result = new Response<List<GetItemTypeCustomValueDto>>();

			result.Result = _serviceSaleOrderApp.GetItemTypeCustomValue(TypeId, FieldNm);

			return result;
		}
		/// <summary>
		/// 关税组 - 值
		/// </summary>
		[HttpGet]
		[Route("DropListCstGrpCodeValue")]
		public Response<string> DropListCstGrpCodeValue(string SboId, string KeyId) {
			var result = new Response<string>();

			try {
				if (!string.IsNullOrEmpty(SboId) && !string.IsNullOrEmpty(KeyId))
					result.Result = _serviceSaleOrderApp.DropListCstGrpCodeValue(SboId, KeyId);

				else
					return null;
			} catch (Exception e) {
				result.Message = e.Message;
			}

			return result;
		}
		/// <summary>
		/// 税收组（采购） - 值
		/// </summary>
		[HttpGet]
		[Route("DropListVatGroupPuValue")]
		public Response<string> DropListVatGroupPuValue(string SboId, string KeyId) {
			var result = new Response<string>();

			try {
				if (!string.IsNullOrEmpty(SboId) && !string.IsNullOrEmpty(KeyId))
					result.Result = _serviceSaleOrderApp.DropListVatGroupValue(SboId, "buy", KeyId);

			} catch (Exception e) {
				result.Message = e.Message;
			}

			return result;
		}
		/// <summary>
		/// 税收组（销售） - 值
		/// </summary>
		[HttpGet]
		[Route("DropListVatGourpSaValue")]
		public Response<string> DropListVatGourpSaValue(string SboId, string KeyId) {
			var result = new Response<string>();

			try {
				if (!string.IsNullOrEmpty(SboId) && !string.IsNullOrEmpty(KeyId))
					result.Result = _serviceSaleOrderApp.DropListVatGroupValue(SboId, "sale", KeyId);

			} catch (Exception e) {
				result.Message = e.Message;
			}

			return result;
		}

		/// <summary>
		/// 获取物料的类型自定义字段 — 值
		/// </summary>
		[HttpGet]
		[Route("GetMaterialTypeCustomValue")]
		public Response<string> GetMaterialTypeCustomValue(string ItemCode, string SboId) {
			var result = new Response<string>();

			try {
				result.Result = _serviceSaleOrderApp.GetMaterialTypeCustomValue(ItemCode, SboId);
			} catch (Exception e) {
				result.Message = e.Message;
			}
			return result;
		}
		/// <summary>
		/// 获取页面地址对应的附件类型
		/// </summary>
		[HttpGet]
		[Route("GetFileTypeByUrl")]
		public Response<string> GetFileTypeByUrl(string PageUrl) {
			var result = new Response<string>();

			try {
				result.Result = _serviceSaleOrderApp.GetFileTypeByUrl(PageUrl);
			} catch (Exception e) {
				result.Message = e.Message;
			}
			return result;
		}
		/// <summary>
		/// 自定义字段
		/// </summary>
		/// <param name="TableName"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("GetCustomFields")]
		public Response<string> GetCustomFields(string TableName) {
			var result = new Response<string>();

			try {
				result.Result = _serviceSaleOrderApp.GetCustomFieldsNos(TableName);
			} catch (Exception e) {
				result.Message = e.Message;
			}
			return result;
		}
		/// <summary>
		///自定义字段(下拉列表)
		///</summary>
		/// <param name="TableID"></param>
		/// <param name="FieldID"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("GetCustomValue")]
		public Response<string> GetCustomValue(string TableID, string FieldID) {
			var result = new Response<string>();


			try {
				result.Result = _serviceSaleOrderApp.GetCustomValue(TableID, FieldID);
			} catch (Exception e) {
				result.Message = e.Message;
			}
			return result;
		}
		/// <summary>
		/// 查询物料属性对应的名称
		/// </summary>
		[HttpGet]
		[Route("GetMaterialPropertyName")]
		public Response<string> GetMaterialPropertyName() {
			var result = new Response<string>();

			var UserID = _serviceBaseApp.GetUserNaspId();
			var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
			try {
				result.Result = _serviceSaleOrderApp.GetMaterialPropertyName(SboID.ToString());
			} catch (Exception e) {
				result.Message = e.Message;
			}
			return result;
		}


		/// <summary>
		/// 查询指定帐套是否开启
		/// </summary>
		[HttpGet]
		[Route("GetSapSboIsOpen")]
		public Response<string> GetSapSboIsOpen(string sbo_id) {
			var result = new Response<string>();

			try {
				if (!string.IsNullOrEmpty(sbo_id))
					result.Result = _serviceSaleOrderApp.GetSapSboIsOpen(sbo_id) ? "1" : "0";
				else
					return null;
			} catch (Exception e) {
				result.Message = e.Message;
			}
			return result;
		}
		/// <summary>
		/// 物料组
		/// </summary>
		[HttpGet]
		[Route("DropListItmsGrpCod")]
		public Response<string> DropListItmsGrpCod(string SboId) {
			var result = new Response<string>();
			try {
				result.Result = _serviceSaleOrderApp.DropListItmsGrpCod(SboId);
			} catch (Exception e) {
				result.Message = e.Message;
			}
			return result;
		}
		/// <summary>
		/// 装运类型
		/// </summary>
		[HttpGet]
		[Route("DropListShipType")]
		public Response<string> DropListShipType(string SboId) {
			var result = new Response<string>();


			try {
				result.Result = _serviceSaleOrderApp.DropListShipType(SboId);
			} catch (Exception e) {
				result.Message = e.Message;
			}
			return result;
		}
		/// <summary>
		/// 关税组
		/// </summary>
		[HttpGet]
		[Route("DropListCstGrpCode")]
		public Response<string> DropListCstGrpCode(string SboId) {
			var result = new Response<string>();

			try {
				result.Result = _serviceSaleOrderApp.DropListCstGrpCode(SboId);
			} catch (Exception e) {
				result.Message = e.Message;
			}
			return result;
		}
		/// <summary>
		/// 税收组（采购）
		/// </summary>
		[HttpGet]
		[Route("DropListVatGroupPu")]
		public Response<string> DropListVatGroupPu(string SboId) {
			var result = new Response<string>();


			try {
				result.Result = _serviceSaleOrderApp.DropListVatGroup(SboId, "buy");
			} catch (Exception e) {
				result.Message = e.Message;
			}
			return result;
		}
		/// <summary>
		/// 税收组（销售）
		/// </summary>
		[HttpGet]
		[Route("DropListVatGourpSa")]
		public Response<string> DropListVatGourpSa(string SboId) {
			var result = new Response<string>();

			try {
				result.Result = _serviceSaleOrderApp.DropListVatGroup(SboId, "sale");
			} catch (Exception e) {
				result.Message = e.Message;
			}
			return result;

		}
		/// <summary>
		/// 价格清单
		/// LstEvlPric【该字段为 最后结算价格】
		/// </summary>
		[HttpGet]
		[Route("DropListPriceList")]
		public Response<string> DropListPriceList(string SboId) {
			var result = new Response<string>();

			try {
				result.Result = _serviceSaleOrderApp.DropListPriceList(SboId);
			} catch (Exception e) {
				result.Message = e.Message;
			}
			return result;
		}
		/// <summary>
		/// 制造商
		/// </summary>
		[HttpGet]
		[Route("DropListFirmCode")]
		public Response<string> DropListFirmCode(string SboId) {
			var result = new Response<string>();

			try {
				result.Result = _serviceSaleOrderApp.DropListFirmCode(SboId);
			} catch (Exception e) {
				result.Message = e.Message;
			}
			return result;
		}
		/// <summary>
		/// 长度单位
		/// </summary>
		[HttpGet]
		[Route("DropListLengthUnit")]
		public Response<List<DropListUnit>> DropListLengthUnit(string SboId) {
			var result = new Response<List<DropListUnit>>();

			result.Result = _serviceSaleOrderApp.DropListLengthUnit(SboId);

			return result;
		}
		/// <summary>
		/// 体积单位
		/// </summary>
		[HttpGet]
		[Route("DropListVolumeUnit")]
		public Response<List<DropListUnit>> DropListVolumeUnit(string SboId) {
			var result = new Response<List<DropListUnit>>();

			result.Result = _serviceSaleOrderApp.DropListVolumeUnit(SboId);

			return result;
		}
		/// <summary>
		/// 重量单位
		/// </summary>
		[HttpGet]
		[Route("DropListWeightUnit")]
		public Response<List<DropListUnit>> DropListWeightUnit(string SboId) {
			var result = new Response<List<DropListUnit>>();


			result.Result = _serviceSaleOrderApp.DropListWeightUnit(SboId);

			return result;
		}
		#region 查询物料的过往采购记录
		/// <summary>
		/// 查询物料的过往采购记录
		/// </summary>
		[HttpGet]
		[Route("GetMaterialsPurHistory")]
		public  TableData GetMaterialsPurHistory(string page, string rp, string qtype, string query, string sortname, string sortorder, string ItemCode) {
			var result = new TableData();

			if (!string.IsNullOrEmpty(ItemCode)) {

				DataTable dt= _serviceSaleOrderApp.GetMaterialsPurHistory(int.Parse(rp), int.Parse(page), query, sortname, sortorder, ItemCode);
				result.Data = dt;
				result.Count = dt.Rows.Count;
			}
			return result;
		}
		#endregion
	}
}
