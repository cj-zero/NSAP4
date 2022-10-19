using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSAP.App.WebApi.Controllers
{
    /// <summary>
    /// 技术员收入
    /// </summary>
    [ApiController]
    public class TechnicianIncomeController : BaseController
    {
        private readonly ServiceOrderApp _serviceOrderApp;

        public TechnicianIncomeController(ServiceOrderApp serviceOrderApp)
        {
            _serviceOrderApp = serviceOrderApp;
        }

        /// <summary>
        /// 我的收入
        /// </summary>
        /// <param name="CurrentUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> MyIncome(int CurrentUserId)
        {
            var result = new TableData();
            try
            {
                return await _serviceOrderApp.MyIncome(CurrentUserId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 我的收入详情
        /// </summary>
        /// <param name="CurrentUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> MyIncomeDetail(int CurrentUserId)
        {
            var result = new TableData();
            try
            {
                return await _serviceOrderApp.MyIncomeDetail(CurrentUserId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 更多排名
        /// </summary>
        /// <param name="CurrentUserId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> MoreRankings(int CurrentUserId, int year, int month, int type)
        {
            var result = new TableData();
            try
            {
                return await _serviceOrderApp.MoreRankings(CurrentUserId, year, month, type);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 历史收入
        /// </summary>
        /// <param name="CurrentUserId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> HistoricalIncome(int CurrentUserId, int type)
        {
            var result = new TableData();
            try
            {
                return await _serviceOrderApp.HistoricalIncome(CurrentUserId,  type);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        [HttpGet]
        public async Task summy()
        {
            await _serviceOrderApp.GenerateIncomeSummary();
        }
    }
}
