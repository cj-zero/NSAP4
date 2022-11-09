using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;
using Infrastructure;
using OpenAuth.Repository.Interface;
using Sap.Handler.Sap;
using SAPbobsCOM;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSAP.Entity.Sales;
using NSAP.Entity.Client;
using System.Data;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain.Sap;

namespace Sap.Handler.Service
{
    public class BOneOCRDAssignSapHandler : ICapSubscribe
    {
        private readonly IUnitWork UnitWork;
        private readonly SAPbobsCOM.Company company;
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁

        public BOneOCRDAssignSapHandler(IUnitWork unitWork, Company company)
        {
            UnitWork = unitWork;
            this.company = company;
        }

        /// <summary>
        /// 同步sap
        /// </summary>
        /// <param name="model">客户信息</param>
        /// <returns></returns>
        [CapSubscribe("Serve.BOneOCRDAssign.Update")]
        public async Task HandleBOneOCRDAssign(clientOCRD model)
        {
            StringBuilder allerror = new StringBuilder();
            int res = 0; int eCode = 8888;
            bool Result = false;
            string eMesg;
            try
            {
                if (!string.IsNullOrEmpty(model.CardCode))
                {
                    string[] strClientList = model.CardCode.Split('☆');

                    int pNum = 0;
                    for (int i = 0; i < strClientList.Length; i++)
                    {
                        string rCardCode = strClientList[i];
                        BusinessPartners bp = (BusinessPartners)company.GetBusinessObject(BoObjectTypes.oBusinessPartners);
                        Log.Logger.Error($"客户分配业务员", typeof(BOneOCRDAssignSapHandler));
                        bp.GetByKey(rCardCode);
                        bp.SalesPersonCode = Convert.ToInt32(model.SlpCode);
                        res = bp.Update();
                        if (res != 0)
                        {
                            pNum++;
                            company.GetLastError(out eCode, out eMesg);
                            Log.Logger.Error("操作失败，客户代码【{0}】,业务员代码【{1}】错误代码：{2}错误信息：{3}", rCardCode, Convert.ToInt32(model.SlpCode), eCode, eMesg);
                        }
                        else
                        {
                            Log.Logger.Error("操作成功，客户代码【{0}】", rCardCode);
                        }
                    }
                    Result = pNum == 0 ? true : false;
                }
                else
                {
                    Log.Logger.Error("客户代码为空！");
                    Result = false;
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error("业务伙伴分配销售员，调接口错误！错误代号:{1} 错误信息：{2}", eCode, ex.Message);
                Result = false;
            }
            await HandleBoneOCRDAssignERP(model);
        }

        /// <summary>
        /// 同步到ERP3.0
        /// </summary>
        /// <param name="model">付款条件是实体</param>
        /// <returns></returns>
        [CapSubscribe("Serve.BOneOCRDAssign.ERPUpdate")]
        public async Task HandleBoneOCRDAssignERP(clientOCRD model)
        {
            Log.Logger.Error("开始同步3.0");
            var dbContext = UnitWork.GetDbContext<crm_ocrd>();
            //using (var transaction = await dbContext.Database.BeginTransactionAsync())
            //{
            try
            {
                List<CmdParameter> cmdMain = new List<CmdParameter>();
                if (!string.IsNullOrEmpty(model.CardCode))
                {
                    #region 更新store_OITM表
                    string rCode = model.CardCode.Replace("'", "''").Replace("☆", "','");
                    var SapOcrdList = UnitWork.Find<OCRD>(q => q.CardCode == rCode).ToList();
                    string rCardCode = "", rSlpCode = "";
                    for (int i = 0; i < SapOcrdList.Count; i++)
                    {
                        rCardCode = SapOcrdList[i].CardCode.ToString();
                        rSlpCode = SapOcrdList[i].SlpCode.ToString();
                        int Sboid = Define.SBO_ID;

                        var crm_ocrd = await UnitWork.Find<crm_ocrd>(r => r.CardCode == rCardCode && r.sbo_id == Sboid).FirstOrDefaultAsync();
                        if (crm_ocrd != null)
                        {
                            Log.Logger.Error("业务员编码为" + rSlpCode);

                            //修改客户信息 
                            //记录业务伙伴的分配历史
                            await UnitWork.UpdateAsync<crm_ocrd>(r => r.CardCode == rCardCode && r.sbo_id == Sboid, r => new crm_ocrd()
                            {
                                SlpCode = Convert.ToInt32(rSlpCode)
                            });
                            await UnitWork.AddAsync<crm_orcd_assign_hist, int>(new crm_orcd_assign_hist()
                            {
                                cardcode = rCardCode,
                                new_staff_id = Convert.ToInt32(rSlpCode),
                                remarks = "批量分配"
                            });

                        }
                        else
                        {
                            Log.Logger.Error("crm_ocrd为空，客户编码为" + rCardCode);
                        }
                    };
                    #endregion
                    await UnitWork.SaveAsync();
                    //await transaction.CommitAsync();
                    Log.Logger.Debug($"同步3.0成功，SAP_ID：{model.GroupNum}", typeof(BOneOCRDAssignSapHandler));
                }
                else
                {
                    Log.Logger.Error("客户代码为空！");
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error("更新MySql时捕获到错误:【{0}】", ex.Message);
            }
            //}
        }
    }
}
