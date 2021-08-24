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
		public string DropListCstGrpCodeValue(string SboId, string KeyId) {
			var result = "";
			try {
				if (!string.IsNullOrEmpty(SboId) && !string.IsNullOrEmpty(KeyId))
					result = _serviceSaleOrderApp.DropListCstGrpCodeValue(SboId, KeyId);

				else
					return "";
			} catch (Exception e) {
				return e.Message;
			}
			if (result == "[]") result = "0";
			return result;
		}
		/// <summary>
		/// 税收组（采购） - 值
		/// </summary>
		[HttpGet]
		[Route("DropListVatGroupPuValue")]
		public string DropListVatGroupPuValue(string SboId, string KeyId) {
			var result = "";
			try {
				if (!string.IsNullOrEmpty(SboId) && !string.IsNullOrEmpty(KeyId))
					result = _serviceSaleOrderApp.DropListVatGroupValue(SboId, "buy", KeyId);
				else
					return "";
			} catch (Exception e) {
				return e.Message;
			}
			if (result == "[]") result = "0";
			return result;
		}
		/// <summary>
		/// 税收组（销售） - 值
		/// </summary>
		[HttpGet]
		[Route("DropListVatGourpSaValue")]
		public string DropListVatGourpSaValue(string SboId, string KeyId) {
			var result = "";
			try {
				if (!string.IsNullOrEmpty(SboId) && !string.IsNullOrEmpty(KeyId))
					result = _serviceSaleOrderApp.DropListVatGroupValue(SboId, "sale", KeyId);
				else
					return "";
			} catch (Exception e) {
				return e.Message;
			}
			if (result == "[]") result = "0";
			return result;
		}

		/// <summary>
		/// 获取物料的类型自定义字段 — 值
		/// </summary>
		[HttpGet]
		[Route("GetMaterialTypeCustomValue")]
		public string GetMaterialTypeCustomValue(string ItemCode, string SboId) {
			var result = "";
			try {
				result = _serviceSaleOrderApp.GetMaterialTypeCustomValue(ItemCode, SboId);
			} catch (Exception e) {
				return e.Message;
			}
			return result;
		}
		/// <summary>
		/// 获取页面地址对应的附件类型
		/// </summary>
		[HttpGet]
		[Route("GetFileTypeByUrl")]
		public string GetFileTypeByUrl(string PageUrl) {
			var result = "";
			try {
				result = _serviceSaleOrderApp.GetFileTypeByUrl(PageUrl);
			} catch (Exception e) {
				return e.Message;
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
		public string GetCustomFields(string TableName) {
			var result = "";
			try {
				result = _serviceSaleOrderApp.GetCustomFieldsNos(TableName);
			} catch (Exception e) {
				return e.Message;
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
		public string GetCustomValue(string TableID, string FieldID) {
			var result = "";

			try {
				result = _serviceSaleOrderApp.GetCustomValue(TableID, FieldID);
			} catch (Exception e) {
				return e.Message;
			}
			return result;
		}
		/// <summary>
		/// 查询物料属性对应的名称
		/// </summary>
		[HttpGet]
		[Route("GetMaterialPropertyName")]
		public string GetMaterialPropertyName() {
			var result = "";
			var UserID = _serviceBaseApp.GetUserNaspId();
			var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
			try {
				result = _serviceSaleOrderApp.GetMaterialPropertyName(SboID.ToString());
			} catch (Exception e) {
				return e.Message;
			}
			return result;
		}


		/// <summary>
		/// 查询指定帐套是否开启
		/// </summary>
		[HttpGet]
		[Route("GetSapSboIsOpen")]
		public string GetSapSboIsOpen(string sbo_id) {
			var result = "";
			try {
				if (!string.IsNullOrEmpty(sbo_id))
					result = _serviceSaleOrderApp.GetSapSboIsOpen(sbo_id) ? "1" : "0";
				else
					return "0";
			} catch (Exception e) {
				return e.Message;
			}
			return result;
		}
		/// <summary>
		/// 物料组
		/// </summary>
		[HttpGet]
		[Route("DropListItmsGrpCod")]
		public string DropListItmsGrpCod(string SboId) {
			var result = "";
			try {
				result = _serviceSaleOrderApp.DropListItmsGrpCod(SboId);
			} catch (Exception e) {
				return e.Message;
			}
			return result;
		}
		/// <summary>
		/// 装运类型
		/// </summary>
		[HttpGet]
		[Route("DropListShipType")]
		public string DropListShipType(string SboId) {
			var result = "";

			try {
				result = _serviceSaleOrderApp.DropListShipType(SboId);
			} catch (Exception e) {
				return e.Message;
			}
			return result;
		}
		/// <summary>
		/// 关税组
		/// </summary>
		[HttpGet]
		[Route("DropListCstGrpCode")]
		public string DropListCstGrpCode(string SboId) {
			var result = "";
			try {
				result = _serviceSaleOrderApp.DropListCstGrpCode(SboId);
			} catch (Exception e) {
				return e.Message;
			}
			return result;
		}
		/// <summary>
		/// 税收组（采购）
		/// </summary>
		[HttpGet]
		[Route("DropListVatGroupPu")]
		public string DropListVatGroupPu(string SboId) {
			var result = "";

			try {
				result = _serviceSaleOrderApp.DropListVatGroup(SboId, "buy");
			} catch (Exception e) {
				return e.Message;
			}
			return result;
		}
		/// <summary>
		/// 税收组（销售）
		/// </summary>
		[HttpGet]
		[Route("DropListVatGourpSa")]
		public string DropListVatGourpSa(string SboId) {
			var result = "";
			try {
				result = _serviceSaleOrderApp.DropListVatGroup(SboId, "sale");
			} catch (Exception e) {
				return e.Message;
			}
			return result;

		}
		/// <summary>
		/// 价格清单
		/// LstEvlPric【该字段为 最后结算价格】
		/// </summary>
		[HttpGet]
		[Route("DropListPriceList")]
		public string DropListPriceList(string SboId) {
			var result = "";
			try {
				result = _serviceSaleOrderApp.DropListPriceList(SboId);
			} catch (Exception e) {
				return e.Message;
			}
			return result;
		}
		/// <summary>
		/// 制造商
		/// </summary>
		[HttpGet]
		[Route("DropListFirmCode")]
		public string DropListFirmCode(string SboId) {
			var result = "";
			try {
				result = _serviceSaleOrderApp.DropListFirmCode(SboId);
			} catch (Exception e) {
				return e.Message;
			}
			return result;
		}
		/// <summary>
		/// 长度单位
		/// </summary>
		[HttpGet]
		[Route("DropListLengthUnit")]
		public Response<List<GetItemTypeCustomValueDto>> DropListLengthUnit(string SboId) {
			var result = new Response<List<GetItemTypeCustomValueDto>>();

			result.Result = _serviceSaleOrderApp.DropListLengthUnit(SboId);

			return result;
		}
		/// <summary>
		/// 体积单位
		/// </summary>
		[HttpGet]
		[Route("DropListVolumeUnit")]
		public Response<List<GetItemTypeCustomValueDto>> DropListVolumeUnit(string SboId) {
			var result = new Response<List<GetItemTypeCustomValueDto>>();

			result.Result = _serviceSaleOrderApp.DropListVolumeUnit(SboId);

			return result;
		}
		/// <summary>
		/// 重量单位
		/// </summary>
		[HttpGet]
		[Route("DropListWeightUnit")]
		public Response<List<GetItemTypeCustomValueDto>> DropListWeightUnit(string SboId) {
			var result = new Response<List<GetItemTypeCustomValueDto>>();


			result.Result = _serviceSaleOrderApp.DropListWeightUnit(SboId);

			return result;
		}
	}
}
