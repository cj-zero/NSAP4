using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Infrastructure;
using OpenAuth.App.Response;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using OpenAuth.App.Order.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.App.PayTerm.PayTermSetHelp;
using Microsoft.EntityFrameworkCore;

namespace OpenAuth.App.PayTerm
{
    public class PayLimitRuleApp : OnlyUnitWorkBaeApp
    {
        private IUnitWork _UnitWork;
        private IAuth _auth;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public PayLimitRuleApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
            _UnitWork = unitWork;
            _auth = auth;
        }

        /// <summary>
        /// 校验当前客户是否满足限制规则
        /// </summary>
        /// <param name="addOrderReq"></param>
        /// <returns>成功返回true，失败返回false</returns>
        public async Task<bool> CheckOrderDraft(AddOrderReq addOrderReq)
        {
            bool isSuccess = false;
            string cardCode = addOrderReq.Order.CardCode;
            decimal PreBefProTotal = 0;
            int groupNum = addOrderReq.Order.GroupNum;
            decimal docTotal = addOrderReq.Order.DocTotal;
            bool isFlag = false;

            //如果客户为VIP客户，不走限制规则判定
            var vipCustomers = await UnitWork.Find<PayVIPCustomer>(r => r.CardCode == cardCode).ToListAsync();
            if (vipCustomers != null && vipCustomers.Count() > 0)
            {
                isSuccess = true;
                return isSuccess;
            }

            //查询客户类型
            string cardType = await UnitWork.Find<crm_ocrd>(r => r.CardCode == cardCode).Select(r => r.U_CardTypeStr).FirstOrDefaultAsync();

            //查询当前客户的付款条件预付和货前比例之和
            var crmOctgs = await UnitWork.Find<crm_octg>(r => r.GroupNum == groupNum).ToListAsync();
            if (crmOctgs != null && crmOctgs.Count() > 0)
            {
                var payTermSaves = await UnitWork.Find<PayTermSave>(r => r.GroupNum == (crmOctgs.FirstOrDefault().PymntGroup)).ToListAsync();
                if (payTermSaves != null && payTermSaves.Count() > 0)
                {
                    decimal prepaPro = payTermSaves.FirstOrDefault().PrepaPro == null ? 0 : (decimal)payTermSaves.FirstOrDefault().PrepaPro;
                    decimal befShipPro = payTermSaves.FirstOrDefault().BefShipPro == null ? 0 : (decimal)payTermSaves.FirstOrDefault().BefShipPro;
                    PreBefProTotal = prepaPro + befShipPro;
                }
            }

            //查询当前客户是否为中间商
            var clientRelations = await UnitWork.Find<OpenAuth.Repository.Domain.ClientRelation>(r => r.ClientNo == cardCode).ToListAsync();
            if (clientRelations != null && clientRelations.Count() > 0)
            {
                isFlag = clientRelations.FirstOrDefault().Flag == 1 ? true : false;
            }

            try
            {
                //根据限制规则生成逻辑运算公式
                var limitRules = await UnitWork.Find<PayLimitRule>(r => r.IsUse == true).OrderByDescending(r => r.PayPriority).ToListAsync();
                if (limitRules != null && limitRules.Count() > 0)
                {
                    foreach (PayLimitRule item in limitRules)
                    {
                        StringBuilder filterStr = new StringBuilder();
                        var limitRuleDetails = await UnitWork.Find<PayLimitRuleDetail>(r => r.PayLimitRuleId == item.Id).ToListAsync();
                        if (limitRuleDetails != null && limitRuleDetails.Count() > 0)
                        {
                            //如果条件转换为逻辑运算公式
                            List<PayLimitRuleDetail> payLimitRuleDetailIfs = limitRuleDetails.Where(r => r.RuleType == 1).OrderBy(r => r.RuleNum).ToList();
                            foreach (PayLimitRuleDetail objIf in payLimitRuleDetailIfs)
                            {
                                if (!string.IsNullOrEmpty(objIf.BracketLeft))
                                {
                                    filterStr.Append("(");
                                }

                                switch (objIf.Key)
                                {
                                    case "U_CardTypeStr":
                                        if (!string.IsNullOrEmpty(cardType))
                                        {
                                            string cardTypeNew = cardType.Substring(1);
                                            cardTypeNew = cardTypeNew.Substring(0, cardTypeNew.Substring(1).Length - 1);
                                            if (cardTypeNew.Contains(","))
                                            {
                                                string[] cardTypes = cardTypeNew.Split(',');
                                                cardTypeNew = cardTypes[cardTypes.Length - 1];
                                            }

                                            //如果客户类型表达式是属于，并且当前客户类型包含在值里面为true，否则false
                                            if (objIf.Contrast == "IsType")
                                            {
                                                if (objIf.Value.Contains(cardTypeNew))
                                                {
                                                    filterStr.Append("true");
                                                }
                                                else
                                                {
                                                    filterStr.Append("false");
                                                }
                                            }
                                            else
                                            {
                                                if (objIf.Value.Contains(cardTypeNew))
                                                {
                                                    filterStr.Append("false");
                                                }
                                                else
                                                {
                                                    filterStr.Append("true");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            filterStr.Append("false");
                                        }

                                        break;
                                    case "Flag":
                                        if (objIf.Value == "yes")
                                        {
                                            if (isFlag)
                                            {
                                                filterStr.Append("true");
                                            }
                                            else
                                            {
                                                filterStr.Append("false");
                                            }
                                        }
                                        else
                                        {
                                            if (isFlag)
                                            {
                                                filterStr.Append("false");
                                            }
                                            else
                                            {
                                                filterStr.Append("true");
                                            }
                                        }

                                        break;
                                    case "DocTotal":
                                        filterStr.Append(CommonMethodHelp.GetContrastConvert(objIf.Contrast, Convert.ToDecimal(objIf.Value) * 10000, docTotal));
                                        break;
                                    case "CardCode":
                                        if (objIf.Contrast == "IsCode")
                                        {
                                            if (objIf.Value.Contains(cardCode))
                                            {
                                                filterStr.Append("true");
                                            }
                                            else
                                            {
                                                filterStr.Append("false");
                                            }
                                        }
                                        else
                                        {
                                            if (objIf.Value.Contains(cardCode))
                                            {
                                                filterStr.Append("false");
                                            }
                                            else
                                            {
                                                filterStr.Append("true");
                                            }
                                        }

                                        break;
                                }

                                filterStr.Append(objIf.BracketRight + (objIf.AndOr == "and" ? "&&" : (objIf.AndOr == "or" ? "||" : "")));
                            }

                            filterStr.Append("&&");

                            //那么条件转换为逻辑运算公式
                            List<PayLimitRuleDetail> payLimitRuleDetailThens = limitRuleDetails.Where(r => r.RuleType == 2).OrderBy(r => r.RuleNum).ToList();
                            foreach (PayLimitRuleDetail objThen in payLimitRuleDetailThens)
                            {
                                if (!string.IsNullOrEmpty(objThen.BracketLeft))
                                {
                                    filterStr.Append("(");
                                }

                                filterStr.Append(CommonMethodHelp.GetContrastConvert(objThen.Contrast, Convert.ToDecimal(objThen.Value), PreBefProTotal));
                                filterStr.Append(objThen.BracketRight + (objThen.AndOr == "and" ? "&&" : (objThen.AndOr == "or" ? "||" : "")));
                            }
                        }

                        //将字符串逻辑运算公式转化为逻辑运算，并返回运算结果
                        isSuccess = CommonMethodHelp.ExpressionSplit(filterStr.ToString());
                        if (isSuccess)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error("验证限制规则失败：" + ex.Message.ToString());
                isSuccess = false;
            }

            return isSuccess;
        }

        /// <summary>
        /// 获取已经冻结的客户编码
        /// </summary>
        /// <returns>返回拼客户编码集合</returns>
        public async Task<List<string>> GetFreezeCustomers()
        {
            List<string> freezeCus = await UnitWork.Find<PayFreezeCustomer>(null).Select(r => r.CardCode).ToListAsync();
            if (freezeCus != null && freezeCus.Count() > 0)
            {
                return freezeCus;
            }
            else
            {
                return new List<string>();
            }
        }
    }
}
