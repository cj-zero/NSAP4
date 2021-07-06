using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.App.Workbench.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Workbench;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Workbench
{
    /// <summary>
    /// 提交给我的
    /// </summary>
    public class PendingApp : OnlyUnitWorkBaeApp
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public PendingApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
        }
        /// <summary>
        /// 服务单详情
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceOrderResp> ServiceOrderDetails(string ServiceOrderId) 
        {

            return new ServiceOrderResp();
        }
        /// <summary>
        /// 报价单详情
        /// </summary>
        /// <returns></returns>
        public async Task<QuotationDetailsResp> QuotationDetails(string QuotationId)
        {

            return new QuotationDetailsResp();
        }
        /// <summary>
        /// 退料单详情
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnnoteDetailsResp> ReturnnoteDetails(string ReturnnoteId)
        {

            return new ReturnnoteDetailsResp();
        }
        /// <summary>
        /// 结算单详情
        /// </summary>
        /// <returns></returns>
        public async Task<OutsourcDetailsResp> OutsourcDetails(string OutsourcId)
        {

            return new OutsourcDetailsResp();
        }
        /// <summary>
        /// 报销单详情
        /// </summary>
        /// <returns></returns>
        public async Task<ReimburseDetailsResp> ReimburseDetails(string ReimburseId)
        {

            return new ReimburseDetailsResp();
        }

        /// <summary>
        ///判断增加还是修改待处理
        /// </summary>
        /// <returns></returns>
        public async Task AddOrUpdate(WorkbenchPending obj)
        {
            //增加全局待处理
            var workbenchPendingObj = await UnitWork.Find<WorkbenchPending>(w => w.SourceNumbers == obj.SourceNumbers && w.OrderType == obj.OrderType).FirstOrDefaultAsync();
            if (workbenchPendingObj != null)
            {
                await UnitWork.UpdateAsync<WorkbenchPending>(w => w.ApprovalNumber == workbenchPendingObj.ApprovalNumber, w => new WorkbenchPending
                {
                    UpdateTime = obj.UpdateTime,
                    Remark = obj.Remark,
                });
            }
            else
            {
                await UnitWork.AddAsync<WorkbenchPending>(new WorkbenchPending
                {
                    OrderType = obj.OrderType,
                    TerminalCustomer = obj.TerminalCustomer,
                    TerminalCustomerId = obj.TerminalCustomerId,
                    ServiceOrderId = obj.ServiceOrderId,
                    ServiceOrderSapId =obj.ServiceOrderSapId,
                    UpdateTime = obj.UpdateTime,
                    Remark = obj.Remark,
                    FlowInstanceId = obj.FlowInstanceId,
                    TotalMoney = obj.TotalMoney,
                    Petitioner = obj.Petitioner,
                    SourceNumbers = obj.SourceNumbers
                });
            }
            await UnitWork.SaveAsync();
        }

        /// <summary>
        ///判断增加还是修改待处理
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> Load()
        {
            var reult = new TableData();
            //增加全局待处理
            reult.Data = await UnitWork.Find<WorkbenchPending>(null).ToListAsync();
            return reult;
        }

    }
}
