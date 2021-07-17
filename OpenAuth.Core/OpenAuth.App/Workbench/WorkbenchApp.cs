using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Domain.Workbench;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Workbench
{
    public class WorkbenchApp: OnlyUnitWorkBaeApp
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public WorkbenchApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
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
                    ServiceOrderSapId = obj.ServiceOrderSapId,
                    UpdateTime = obj.UpdateTime,
                    Remark = obj.Remark,
                    FlowInstanceId = obj.FlowInstanceId,
                    TotalMoney = obj.TotalMoney,
                    Petitioner = obj.Petitioner,
                    SourceNumbers = obj.SourceNumbers,
                    PetitionerId = obj.PetitionerId

                });
            }
            await UnitWork.SaveAsync();
        }

    }
}
