﻿using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Order;
using OpenAuth.App.Order.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Order
{
	/// <summary>
	/// 我创建的
	/// </summary>
	[Route("api/Order/[controller]")]
	[ApiController]
	[ApiExplorerSettings(GroupName = "Order")]
	public class ICreatedController : Controller
	{
		private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
		IAuth _auth;
		IUnitWork UnitWork;
		ServiceBaseApp _serviceBaseApp;
		public ICreatedController(IUnitWork UnitWork, ServiceBaseApp _serviceBaseApp, IAuth _auth, ServiceSaleOrderApp serviceSaleOrderApp)
		{
			this.UnitWork = UnitWork;
			this._serviceBaseApp = _serviceBaseApp;
			this._auth = _auth;
			_serviceSaleOrderApp = serviceSaleOrderApp;
		}
		#region 我创建的
		/// <summary>
		/// 我创建的
		/// </summary>
		[HttpPost]
		[Route("SelectMaterialsInventoryData")]
		public TableData GetICreated(GetICreatedReq model)
		{
			var UserID = _serviceBaseApp.GetUserNaspId();
			var result = new TableData();

			DataTable dt = _serviceSaleOrderApp.GetICreated(model.limit, model.page, model.query, model.sortname, model.sortorder, UserID, model.types, model.Applicator, model.Customer, model.Status, model.BeginDate, model.EndDate, _serviceSaleOrderApp.GetPagePowersByUrl("mywork/AuditAllNew.aspx",UserID).ViewCustom, _serviceSaleOrderApp.GetPagePowersByUrl("mywork/AuditAllNew.aspx",UserID).ViewSales);

			result.Data = dt;
			result.Count = dt.Rows.Count;
			return result;

		}
		#endregion
	}
}
