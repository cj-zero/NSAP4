using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;

namespace OpenAuth.WebApi.Controllers.Serve
{
    /// <summary>
    /// 报销
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    public class ReimburseController : ControllerBase
    {
        private readonly ReimburseInfoApp _reimburseinfoapp;

        private readonly CategoryApp _categoryapp;

        public ReimburseController(ReimburseInfoApp reimburseinfoapp,CategoryApp categoryapp)
        {
            _reimburseinfoapp = reimburseinfoapp;
            _categoryapp = categoryapp;
        }
        /// <summary>
        /// 查看报销单列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Load([FromQuery]QueryReimburseInfoListReq request) 
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.Load(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// App查看报销单列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AppLoad([FromQuery] QueryReimburseInfoListReq request)
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.AppLoad(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 获取未报销服务单列别
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetServiceOrder([FromQuery]QueryReimburseServerOrderListReq request)
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.GetServiceOrder(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }
        /// <summary>
        /// 获取报销单
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetDetails(int ReimburseInfoId)
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.GetDetails(ReimburseInfoId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 获取字典
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public TableData GetListCategoryName()
        {
            var result = new TableData();
            string ids = "SYS_OtherExpenses,SYS_Transportation,SYS_TransportationAllowance,SYS_TravellingAllowance,SYS_ServiceRelations,SYS_RemburseStatus,SYS_ReimburseType,SYS_Responsibility,SYS_ProjectName";
            try
            {
                return  _categoryapp.GetListCategoryName(ids);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 添加报销单
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Add(AddOrUpdateReimburseInfoReq obj)
        {
            var result = new Response();
            try
            {
                await _reimburseinfoapp.Add(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }
        /// <summary>
        /// 修改报销单
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> UpDate(AddOrUpdateReimburseInfoReq obj)
        {
            var result = new Response();
            try
            {
                await _reimburseinfoapp.UpDate(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 撤回操作
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Revocation([FromQuery]ReimburseRevocationReq req)
        {
            var result = new TableData();
            try
            {
               return await _reimburseinfoapp.Revocation(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 审批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> Accraditation(AccraditationReimburseInfoReq req)
        {
            var result = new TableData();
            try
            {
                 await _reimburseinfoapp.Accraditation(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 发票号码是否唯一
        /// </summary>
        /// <param name="InvoiceNumber"></param>
        /// <returns></returns>
        [HttpPost]
        public Response IsSole( List<string> InvoiceNumber) 
        {
            var result = new Response();
            try
            {
                if (!_reimburseinfoapp.IsSole(InvoiceNumber)) 
                {
                    throw new CommonException("添加报销单失败。发票存在已使用，不可二次使用！", Define.INVALID_InvoiceNumber);
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 删除报销单
        /// </summary>
        /// <param name="ReimburseInfoId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response> Delete(int ReimburseInfoId)
        {
            var result = new Response();
            try
            {
                await _reimburseinfoapp.Delete(ReimburseInfoId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }
    }

}
