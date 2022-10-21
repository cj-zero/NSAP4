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

namespace Sap.Handler.Service
{
    public class BOneOCTGSapHandler : ICapSubscribe
    {
        private readonly IUnitWork UnitWork;
        private readonly Company company;
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁

        public BOneOCTGSapHandler(IUnitWork unitWork, Company company)
        {
            UnitWork = unitWork;
            this.company = company;
        }

        /// <summary>
        /// 同步sap
        /// </summary>
        /// <param name="model">付款条件实体</param>
        /// <returns></returns>
        [CapSubscribe("Serve.BOneOCTG.Create")]
        public async Task HandleBOneOCTG(saleCrmOctgCfg model)
        {
            StringBuilder allerror = new StringBuilder();
            int eCode;
            string eMesg;
            string docNum = string.Empty;
            int resultCountJob = 0;
            try
            {
                PaymentTermsTypes dts = (PaymentTermsTypes)company.GetBusinessObject(BoObjectTypes.oPaymentTermsTypes);              
                Log.Logger.Error($"添加付款条件", typeof(BOneOCTGSapHandler));
                dts.PaymentTermsGroupName = model.ModelCrmOctg.PymntGroup;
                Log.Logger.Error($"准备调用Add方法", typeof(BOneOCTGSapHandler));
                var res = dts.Add();
                if (res != 0)
                {
                    company.GetLastError(out eCode, out eMesg);
                    allerror.Append("添加付款条件到SAP时异常！错误代码：" + eCode + "错误信息：" + eMesg + "参数："+ model.ToJson());
                }
                else
                {
                    company.GetNewObjectCode(out docNum);
                    string resultGroupNumber = docNum;
                    resultCountJob = Convert.ToInt32(string.IsNullOrEmpty(resultGroupNumber) ? "0" : resultGroupNumber.ToString());
                }
            }
            catch (Exception e)
            {
                allerror.Append("调用SBO接口添加付款条件时异常：" + e.ToString() + "" + "参数：" + model.ToJson());
            }

            if (!string.IsNullOrEmpty(docNum))
            {
                //用信号量代替锁
                await semaphoreSlim.WaitAsync();
                try
                {
                    if (resultCountJob > 0)
                    {
                        Log.Logger.Error($"反写4.0成功，SAP_ID：{resultCountJob}", typeof(BOneOCTGSapHandler));
                    }
                    else
                    {
                        Log.Logger.Error($"反写4.0失败，SAP_ID：{docNum} 参数：{model.ToJson()}", typeof(SellOrderSapHandler));
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"反写4.0失败，SAP_ID：{docNum}失败原因:{ex.Message} + 参数：+ {model.ToJson()}", typeof(SellOrderSapHandler));
                }
                finally
                {
                    semaphoreSlim.Release();
                    model.ModelCrmOctg.GroupNum = resultCountJob.ToString();
                    model.GroupNum = resultCountJob.ToString();
                    await HandleBoneOCTGERP(model);
                }

                Log.Logger.Warning($"同步成功，SAP_ID：{docNum}", typeof(SellOrderSapHandler));
            }
            if (!string.IsNullOrWhiteSpace(allerror.ToString()))
            {
                Log.Logger.Error(allerror.ToString(), typeof(SellOrderSapHandler));
                throw new Exception(allerror.ToString());
            }
        }

        /// <summary>
        /// 同步到ERP3.0
        /// </summary>
        /// <param name="model">付款条件是实体</param>
        /// <returns></returns>
        [CapSubscribe("Serve.BOneOCTG.ERPCreate")]
        public async Task HandleBoneOCTGERP(saleCrmOctgCfg model)
        {
            string message = "";
            var dbContext = UnitWork.GetDbContext<crm_octg>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    int GroupNum = Convert.ToInt32(model.ModelCrmOctg.GroupNum);
                    if (GroupNum > 0)
                    {
                        //groupNum重复则修改,否则添加
                        var crm_octgs = await UnitWork.Find<crm_octg>(r => r.GroupNum == GroupNum).ToListAsync();
                        if (crm_octgs != null && crm_octgs.Count() > 0)
                        {
                            //付款条件修改
                            await UnitWork.UpdateAsync<crm_octg>(r => r.GroupNum == GroupNum, r => new crm_octg()
                            {
                                sbo_id = Define.SBO_ID
                            });
                        }
                        else
                        {
                            //付款条件添加
                            await UnitWork.AddAsync<crm_octg, int>(new crm_octg()
                            {
                                GroupNum = Convert.ToInt32(model.ModelCrmOctg.GroupNum),
                                sbo_id = Define.SBO_ID
                            });
                        }

                        //groupNum重复则修改,否则添加
                        var crm_octg_cfgs = await UnitWork.Find<crm_octg_cfg>(r => r.GroupNum == GroupNum).ToListAsync();
                        if (crm_octg_cfgs != null && crm_octg_cfgs.Count() > 0)
                        {
                            //付款条件配置修改
                            await UnitWork.UpdateAsync<crm_octg_cfg>(r => r.GroupNum == Convert.ToInt32(model.GroupNum), r => new crm_octg_cfg()
                            {
                                sbo_id = Define.SBO_ID,
                                PrepaDay = Convert.ToInt32(string.IsNullOrEmpty(model.PrepaDay) ? "0" : model.PrepaDay),
                                PrepaPro = Convert.ToDecimal(string.IsNullOrEmpty(model.PrepaPro) ? "0" : model.PrepaPro),
                                PayBefShip = Convert.ToDecimal(string.IsNullOrEmpty(model.PayBefShip) ? "0" : model.PayBefShip),
                                GoodsToDay = Convert.ToInt32(string.IsNullOrEmpty(model.GoodsToDay) ? "0" : model.GoodsToDay),
                                GoodsToPro = Convert.ToDecimal(string.IsNullOrEmpty(model.GoodsToPro) ? "0" : model.GoodsToPro)
                            });
                        }
                        else
                        {
                            //付款条件配置添加
                            await UnitWork.AddAsync<crm_octg_cfg, int>(new crm_octg_cfg()
                            {
                                GroupNum = Convert.ToInt32(model.GroupNum),
                                sbo_id = Define.SBO_ID,
                                PrepaDay = Convert.ToInt32(string.IsNullOrEmpty(model.PrepaDay) ? "0" : model.PrepaDay),
                                PrepaPro = Convert.ToDecimal(string.IsNullOrEmpty(model.PrepaPro) ? "0" : model.PrepaPro),
                                PayBefShip = Convert.ToDecimal(string.IsNullOrEmpty(model.PayBefShip) ? "0" : model.PayBefShip),
                                GoodsToDay = Convert.ToInt32(string.IsNullOrEmpty(model.GoodsToDay) ? "0" : model.GoodsToDay),
                                GoodsToPro = Convert.ToDecimal(string.IsNullOrEmpty(model.GoodsToPro) ? "0" : model.GoodsToPro)
                            });
                        }
                    }
                   
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                    Log.Logger.Debug($"同步3.0成功，SAP_ID：{model.GroupNum}", typeof(BOneOCTGSapHandler));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    message = $"同步3.0失败，SAP_ID：{model.GroupNum}" + ex.Message;
                    Log.Logger.Error($"同步3.0失败，SAP_ID：{model.GroupNum}" + ex.Message + "参数：" + model.ToJson(), typeof(BOneOCTGSapHandler));
                }
            }
            if (message != "")
            {
                throw new Exception(message.ToString());
            }
        }
    }
}
