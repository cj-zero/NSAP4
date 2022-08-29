using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using OpenAuth.App;
using OpenAuth.App.SaleBusiness.Common;
using OpenAuth.App.SaleBusiness;
using OpenAuth.App.Response;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using OpenAuth.App.Order.ModelDto;
using OpenAuth.App.Order;
using OpenAuth.App.SaleBusiness.Request;
using OpenAuth.Repository.Domain;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 个人看板
    /// </summary>
    [Route("api/SaleBoard/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "SaleBoard")]
    public class SaleBusinessController : ControllerBase
    {
        IAuth _auth;
        IUnitWork UnitWork;
        private readonly SaleBusinessApp _saleBusinessApp;
        private readonly ServiceBaseApp _serviceBaseApp;
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
        private readonly SaleBusinessMethodHelp _saleBusinessMethodHelp;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="UnitWork"></param>
        /// <param name="auth"></param>
        public SaleBusinessController(SaleBusinessApp saleBusinessApp, ServiceBaseApp serviceBaseApp, SaleBusinessMethodHelp saleBusinessMethodHelp, ServiceSaleOrderApp serviceSaleOrderApp, IUnitWork UnitWork, IAuth auth)
        {
            this.UnitWork = UnitWork;
            this._auth = auth;
            this._saleBusinessApp = saleBusinessApp;
            this._serviceBaseApp = serviceBaseApp;
            this._serviceSaleOrderApp = serviceSaleOrderApp;
            _saleBusinessMethodHelp = saleBusinessMethodHelp;
        }

        /// <summary>
        /// 销售概况
        /// </summary>
        /// <param name="timeRange">合同申请单查询实体数据</param>
        /// <returns>成功返回合同申请单列表信息，失败返回异常信息</returns>
        [HttpGet]
        public async Task<TableData> GetSaleSituation(string timeRange)
        {
            var result = new TableData();
            try
            {
                return await _saleBusinessApp.GetSaleSituation(timeRange);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{timeRange.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 销售状态提醒
        /// </summary>
        /// <returns>返回销售状态提醒信息</returns>
        [HttpGet]
        public async Task<TableData> GetSaleStatus()
        {
            var result = new TableData();
            try
            {
                return await _saleBusinessApp.GetSaleStatus();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ex.Message.ToString().ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 所有仓库
        /// </summary>
        /// <returns>返回仓库信息</returns>
        [HttpGet]
        public Response<List<DropPopupDocCurDto>> DropPopupWhsCode()
        {
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            var result = new Response<List<DropPopupDocCurDto>>();
            try
            {
                result.Result = _serviceSaleOrderApp.DropPopupWhsCode(SboID);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{"SaleStatus".ToJson()}, 错误：{result.Message}");
            }
            return result;

        }

        /// <summary>
        /// 库存查询
        /// </summary>
        /// <param name="req">库存查询实体</param>
        /// <returns>返回库存信息</returns>
        [HttpPost]
        public async Task<TableData> GetWareHouse(QueryWareHouse req)
        {
            var result = new TableData();
            try
            {
                return await _saleBusinessApp.GetWareHouse(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{"SaleStatus".ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 币种信息
        /// </summary>
        /// <returns>返回币种信息</returns>
        [HttpGet]
        public async Task<TableData> GetCoinInfo()
        {
            var result = new TableData();
            try
            {
                return await _saleBusinessApp.GetCoinInfo();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ex.Message.ToString().ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 应收款情况
        /// </summary>
        /// <param name="currency">币种</param>
        /// <returns>返回应收款情况信息</returns>
        [HttpGet]
        public async Task<TableData> GetDeliveryMsg(string currency)
        {
            var result = new TableData();
            try
            {
                return await _saleBusinessApp.GetDeliveryMsg(currency);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{currency.ToString().ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 个人部门与公司排名
        /// </summary>
        /// <param name="currency">币种</param>
        /// <returns>返回业务员在部门与公司应收款排名信息</returns>
        [HttpGet]
        public async Task<TableData> GetDeliveryDepartRank(string currency)
        {
            var result = new TableData();
            try
            {
                User loginUser = _saleBusinessMethodHelp.GetLoginUser(out int? slpCode);
                QueryOINVRank query = new QueryOINVRank();
                query.DepartRank = await _saleBusinessApp.GetDeliveryDepartRank(currency, loginUser, slpCode);
                query.CompanyRank = await _saleBusinessApp.GetDelilveryCompanyRank(currency, loginUser, slpCode);
                result.Data = query;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{currency.ToString().ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 百分比个人部门与公司排名
        /// </summary>
        /// <returns>返回业务员在部门与公司应收款比例排名信息</returns>
        [HttpGet]
        public async Task<TableData> GetPericentageDepartRank()
        {
            var result = new TableData();
            try
            {
                User loginUser = _saleBusinessMethodHelp.GetLoginUser(out int? slpCode);
                QueryOINVRank query = new QueryOINVRank();
                query.DepartRank = await _saleBusinessApp.GetPericentageDepartRank(loginUser, slpCode);
                query.CompanyRank = await _saleBusinessApp.GetPericentageCompanyRank(loginUser, slpCode);
                result.Data = query;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ex.Message.ToString().ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 业务增长趋势
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="timeType">时间类型（查看维度）</param>
        /// <returns>返回业务增长趋势信息</returns>
        [HttpGet]
        public async Task<TableData> GetServiceGrowthTrend(string startTime, string endTime, string timeType)
        {
            var result = new TableData();
            try
            {
                return await _saleBusinessApp.GetServiceGrowthTrend(startTime, endTime, timeType);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ex.Message.ToString().ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 新增客户业务增长趋势部门与公司排名
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="timeType">时间类型（查看维度）</param>
        /// <returns>返回业务员新增客户业务趋势在部门与公司排名信息</returns>
        [HttpGet]
        public async Task<TableData> GetCustomerRank(string startTime, string endTime, string timeType)
        {
            var result = new TableData();
            try
            {
                User loginUser = _saleBusinessMethodHelp.GetLoginUser(out int? slpCode);
                QueryTableData td = _saleBusinessMethodHelp.TimeRangeType(startTime, endTime, timeType);
                if (td.IsSuccess)
                {
                    List<string> customerDepartList = new List<string>();
                    List<string> customerCompanyList = new List<string>();
                    foreach (QueryTime item in td.queryTimes)
                    {
                        string customerDepartRank = await _saleBusinessApp.GetDeaprtRank(item.startTime, item.endTime, slpCode, loginUser, "Cusotmer");//获取时间范围内业务员新增客户趋势部门排名
                        string customerCompanyRank = await _saleBusinessApp.GetCompanyRank(item.startTime, item.endTime, slpCode, loginUser, "Cusotmer");//获取时间范围内业务员新增客户趋势公司排名
                        customerDepartList.Add(customerDepartRank);
                        customerCompanyList.Add(customerCompanyRank);
                    }

                    result.Data = new
                    {
                        xAxis = td.xNum,
                        customerYDepartRank = customerDepartList,
                        customerYCompanyRank = customerCompanyList
                    };
                }
                else
                {
                    result.Message = td.Message;
                    result.Code = 500;
                }             
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ex.Message.ToString().ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 新增合同业务增长趋势公司排名
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="timeType">时间类型（查看维度）</param>
        /// <returns>返回业务员新增合同趋势在部门与公司排名信息</returns>
        [HttpGet]
        public async Task<TableData> GetContractRank(string startTime, string endTime, string timeType)
        {
            var result = new TableData();
            try
            {
                User loginUser = _saleBusinessMethodHelp.GetLoginUser(out int? slpCode);
                QueryTableData td = _saleBusinessMethodHelp.TimeRangeType(startTime, endTime, timeType);
                if (td.IsSuccess)
                {
                    List<string> saleOrderDepartList = new List<string>();
                    List<string> saleOrderCompanyList = new List<string>();
                    foreach (QueryTime item in td.queryTimes)
                    {
                        string saleOrderDepartRank = await _saleBusinessApp.GetDeaprtRank(item.startTime, item.endTime, slpCode, loginUser, "SaleOrder");//获取时间范围内业务员新增合同数趋势部门排名
                        string saleOrderCompanyRank = await _saleBusinessApp.GetCompanyRank(item.startTime, item.endTime, slpCode, loginUser, "SaleOrder");//获取时间范围内业务员新增合同数趋势公司排名                        customerDepartList.Add(customerDepartRank);
                        saleOrderDepartList.Add(saleOrderDepartRank);
                        saleOrderCompanyList.Add(saleOrderCompanyRank);
                    }

                    result.Data = new
                    {
                        xAxis = td.xNum,
                        saleOrderYDepartRank = saleOrderDepartList,
                        saleOrderYCompanyRank = saleOrderCompanyList
                    };
                }
                else
                {
                    result.Message = td.Message;
                    result.Code = 500;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ex.Message.ToString().ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 销售趋势
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="currency">币种</param>
        /// <param name="timeType">时间类型（查看维度）</param>
        /// <returns>返回时间范围内销售订单金额，销售交货金额，销售回款金额，销售开票金额，贷项凭证金额信息</returns>
        [HttpGet]
        public async Task<TableData> GetSaleTrend(string startTime, string endTime, string currency, string timeType)
        {
            var result = new TableData();
            try
            {
                return await _saleBusinessApp.GetSaleTrend(startTime, endTime, currency, timeType);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ex.Message.ToString().ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 销售订单金额趋势部门与公司排名
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="currency">币种</param>
        /// <param name="timeType">时间类型（查看维度）</param>
        /// <returns>返回业务员销售订单金额趋势部门与公司排名信息</returns>
        [HttpGet]
        public async Task<TableData> GetSaleTrendORDRRank(string startTime, string endTime, string currency, string timeType)
        {
            var result = new TableData();
            try
            {
                User loginUser = _saleBusinessMethodHelp.GetLoginUser(out int? slpCode);
                QueryTableData td = _saleBusinessMethodHelp.TimeRangeType(startTime, endTime, timeType);
                if (td.IsSuccess)
                {
                    List<string> ordrYDepartList = new List<string>();
                    List<string> ordrYCompanyList = new List<string>();
                    foreach (QueryTime item in td.queryTimes)
                    {
                        string ordrDepartRank = await _saleBusinessApp.GetSaleDeaprtRank(item.startTime, item.endTime, slpCode, loginUser, "ORDR", currency);//业务员销售订单金额部门排名
                        string ordrCompanyRank = await _saleBusinessApp.GetSaleCompanyRank(item.startTime, item.endTime, slpCode, loginUser, "ORDR", currency);//业务员销售订单金额公司排名
                        ordrYDepartList.Add(ordrDepartRank);
                        ordrYCompanyList.Add(ordrCompanyRank);
                    }

                    result.Data = new
                    {
                        xAxis = td.xNum,
                        ordrYDepartRank = ordrYDepartList,
                        ordrYCompanyRank = ordrYCompanyList
                    };
                }
                else
                {
                    result.Message = td.Message;
                    result.Code = 500;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ex.Message.ToString().ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 销售交货金额趋势部门与公司排名
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="currency">币种</param>
        /// <param name="timeType">时间类型（查看维度）</param>
        /// <returns>返回业务员销售交货金额趋势部门与公司排名信息</returns>
        [HttpGet]
        public async Task<TableData> GetSaleTrendODLNRank(string startTime, string endTime, string currency, string timeType)
        {
            var result = new TableData();
            try
            {
                User loginUser = _saleBusinessMethodHelp.GetLoginUser(out int? slpCode);
                QueryTableData td = _saleBusinessMethodHelp.TimeRangeType(startTime, endTime, timeType);
                if (td.IsSuccess)
                {
                    List<string> odlnYDepartList = new List<string>();
                    List<string> odlnYCompanyList = new List<string>();
                    foreach (QueryTime item in td.queryTimes)
                    {
                        string odlnDepartRank = await _saleBusinessApp.GetSaleDeaprtRank(item.startTime, item.endTime, slpCode, loginUser, "ODLN", currency);//业务员销售交货金额部门排名
                        string odlnCompanyRank = await _saleBusinessApp.GetSaleCompanyRank(item.startTime, item.endTime, slpCode, loginUser, "ODLN", currency);//业务员销售交货金额公司排名
                        odlnYDepartList.Add(odlnDepartRank);
                        odlnYCompanyList.Add(odlnCompanyRank);
                    }

                    result.Data = new
                    {
                        xAxis = td.xNum,
                        odlnYDepartRank = odlnYDepartList,
                        odlnYCompanyRank = odlnYCompanyList
                    };
                }
                else
                {
                    result.Message = td.Message;
                    result.Code = 500;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ex.Message.ToString().ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 销售回款金额趋势部门与公司排名
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="currency">币种</param>
        /// <param name="timeType">时间类型（查看维度）</param>
        /// <returns>返回业务员销售回款金额趋势部门与公司排名信息</returns>
        [HttpGet]
        public async Task<TableData> GetSaleTrendORCTRank(string startTime, string endTime, string currency, string timeType)
        {
            var result = new TableData();
            try
            {
                User loginUser = _saleBusinessMethodHelp.GetLoginUser(out int? slpCode);
                QueryTableData td = _saleBusinessMethodHelp.TimeRangeType(startTime, endTime, timeType);
                if (td.IsSuccess)
                {
                    List<string> orctYDepartList = new List<string>();
                    List<string> orctYCompanyList = new List<string>();
                    foreach (QueryTime item in td.queryTimes)
                    {
                        string orctDepartRank = await _saleBusinessApp.GetSaleDeaprtRank(item.startTime, item.endTime, slpCode, loginUser, "ORCT", currency);//业务员销售回款金额部门排名
                        string orctCompanyRank = await _saleBusinessApp.GetSaleCompanyRank(item.startTime, item.endTime, slpCode, loginUser, "ORCT", currency);//业务员销售回款金额公司排名
                        orctYDepartList.Add(orctDepartRank);
                        orctYCompanyList.Add(orctCompanyRank);
                    }

                    result.Data = new
                    {
                        xAxis = td.xNum,
                        orctYDepartRank = orctYDepartList,
                        orctYComapnyRank = orctYCompanyList
                    };
                }
                else
                {
                    result.Message = td.Message;
                    result.Code = 500;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ex.Message.ToString().ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 销售开票金额趋势部门与公司排名
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="currency">币种</param>
        /// <param name="timeType">时间类型（查看维度）</param>
        /// <returns>返回业务员销售开票金额趋势部门与公司排名信息</returns>
        [HttpGet]
        public async Task<TableData> GetSaleTrendFBMRank(string startTime, string endTime, string currency, string timeType)
        {
            var result = new TableData();
            try
            {
                User loginUser = _saleBusinessMethodHelp.GetLoginUser(out int? slpCode);
                QueryTableData td = _saleBusinessMethodHelp.TimeRangeType(startTime, endTime, timeType);
                if (td.IsSuccess)
                {
                    List<string> fbmYDepartList = new List<string>();
                    List<string> fbmYCompanyList = new List<string>();
                    foreach (QueryTime item in td.queryTimes)
                    {
                        string fbmDepartRank = await _saleBusinessApp.GetSaleDeaprtRank(item.startTime, item.endTime, slpCode, loginUser, "FBM", currency);//业务员销售开票金额部门排名
                        string fbmCompanyRank = await _saleBusinessApp.GetSaleCompanyRank(item.startTime, item.endTime, slpCode, loginUser, "FBM", currency);//业务员销售开票金额公司排名
                        fbmYDepartList.Add(fbmDepartRank);
                        fbmYCompanyList.Add(fbmCompanyRank);
                    }

                    result.Data = new
                    {
                        xAxis = td.xNum,
                        fbmYDepartRank = fbmYDepartList,
                        fbmYCompanyRank = fbmYCompanyList
                    };
                }
                else
                {
                    result.Message = td.Message;
                    result.Code = 500;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ex.Message.ToString().ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 贷项凭证金额趋势部门与公司排名
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="currency">币种</param>
        /// <param name="timeType">时间类型（查看维度）</param>
        /// <returns>返回业务员贷项凭证金额趋势部门与公司排名信息</returns>
        [HttpGet]
        public async Task<TableData> GetSaleTrendORINRank(string startTime, string endTime, string currency, string timeType)
        {
            var result = new TableData();
            try
            {
                User loginUser = _saleBusinessMethodHelp.GetLoginUser(out int? slpCode);
                QueryTableData td = _saleBusinessMethodHelp.TimeRangeType(startTime, endTime, timeType);
                if (td.IsSuccess)
                {
                    List<string> orinYDepartList = new List<string>();
                    List<string> orinYCompanyList = new List<string>();
                    foreach (QueryTime item in td.queryTimes)
                    {
                        string orinDepartRank = await _saleBusinessApp.GetSaleDeaprtRank(item.startTime, item.endTime, slpCode, loginUser, "ORIN", currency);//业务员贷项凭证金额部门排名
                        string orinCompanyRank = await _saleBusinessApp.GetSaleCompanyRank(item.startTime, item.endTime, slpCode, loginUser, "ORIN", currency);//业务员贷项凭证金额公司排名
                        orinYDepartList.Add(orinDepartRank);
                        orinYCompanyList.Add(orinCompanyRank);
                    }

                    result.Data = new
                    {
                        xAxis = td.xNum,
                        orinYDepart = orinYDepartList,
                        orinYCompanyRank = orinYCompanyList
                    };
                }
                else
                {
                    result.Message = td.Message;
                    result.Code = 500;
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ex.Message.ToString().ToJson()}, 错误：{result.Message}");
            }

            return result;
        }
    }
}