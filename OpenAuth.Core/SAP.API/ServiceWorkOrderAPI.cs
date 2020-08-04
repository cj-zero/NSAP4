using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Text;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Serve;

namespace SAP.API
{
   
    public class ServiceWorkOrderAPI
    {
         private readonly Company company;
        public ServiceWorkOrderAPI(Company company)
        {
            this.company = company;
        }

        public bool  AddServiceWorkOrder( ServiceWorkOrder thisWorkOrder , out string docNum,out string errorMsg)
        {
            #region 调用接口

            bool Result = false;
            StringBuilder allerror = new StringBuilder();
            errorMsg = "★"; docNum = "";
            string newSolutionID = ""; 
            int eCode = 0; string eMesg = string.Empty; int res = 0;
            
            if (company != null)
            {
                try
                {
                    SAPbobsCOM.ServiceCalls sc = (SAPbobsCOM.ServiceCalls)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oServiceCalls);
                    SAPbobsCOM.KnowledgeBaseSolutions kbs = (SAPbobsCOM.KnowledgeBaseSolutions)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oKnowledgeBaseSolutions);


                    #region 主数据 （添加）
                    sc.Subject = thisWorkOrder.FromTheme;
                    sc.CustomerCode = thisWorkOrder.ServiceOrder.CustomerId;
                    sc.CustomerName = thisWorkOrder.ServiceOrder.CustomerName;
                    //sc.ContactCode
                    if (thisWorkOrder.ContractId.Trim() != "" && thisWorkOrder.ContractId != null && thisWorkOrder.ContractId.Trim() != "-1")
                    {
                        sc.ContractID = Convert.ToInt32(thisWorkOrder.ContractId);
                    }
                    sc.ManufacturerSerialNum = thisWorkOrder.ManufacturerSerialNumber;
                    sc.InternalSerialNum = thisWorkOrder.InternalSerialNumber;

                    if (thisWorkOrder.ServiceOrder.FromId != null && thisWorkOrder.ServiceOrder.FromId != -1)
                    {
                        sc.Origin = (int)thisWorkOrder.ServiceOrder.FromId;
                    }
                    sc.ItemCode = thisWorkOrder.MaterialCode;
                    sc.ItemDescription = thisWorkOrder.MaterialDescription;
                    if (thisWorkOrder.Status != null)
                    {
                        sc.Status = (int)thisWorkOrder.Status;
                    }
                    if (thisWorkOrder.Priority != null && thisWorkOrder.Priority == 3)
                    {
                        sc.Priority = BoSvcCallPriorities.scp_High;
                    }
                    else if (thisWorkOrder.Priority != null && thisWorkOrder.Priority == 2)
                    {
                        sc.Priority = BoSvcCallPriorities.scp_Medium;
                    }
                    else
                    {
                        sc.Priority = BoSvcCallPriorities.scp_Low;
                    }
                    if (thisWorkOrder.FromType != null)
                    {
                        sc.CallType = (int)thisWorkOrder.FromType;
                    }
                    if (thisWorkOrder.ProblemType != null)
                    {
                        sc.ProblemType = thisWorkOrder.ProblemType.PrblmTypID;
                    }
                    sc.Description = thisWorkOrder.Remark;
                    //sc.TechnicianCode = thisWorkOrder.ServiceOrder.SupervisorId;
                    sc.City = thisWorkOrder.ServiceOrder.City;
                    sc.Room = thisWorkOrder.ServiceOrder.Addr;
                    sc.State = thisWorkOrder.ServiceOrder.Province;
                    //sc.Country = thisWorkOrder.ServiceOrder.

                    #endregion

                    #region 解决方案
                    int kbsRes = 0;
                    int lineNum = 0;

                    for (int i = 0; i < sc.Solutions.Count; i++)
                    {
                        sc.Solutions.SetCurrentLine(i);
                        sc.Solutions.Delete();
                    }

                    //添加行明细
                    if (thisWorkOrder.Solution != null)
                    {
                        Solution solution = thisWorkOrder.Solution;
                        if (solution.SltCode == 0)
                        {
                            //先添加解决方案
                            kbs.ItemCode = thisWorkOrder.MaterialCode;
                            if (solution.Status != null)
                            {
                                kbs.Status = (int)solution.Status;
                            }
                            kbs.Solution = solution.Subject;
                            kbs.Symptom = solution.Symptom;
                            kbs.Cause = solution.Cause;
                            kbs.Description = solution.Descriptio;
                            kbsRes = kbs.Add();

                            if (kbsRes == 0)
                            {
                                company.GetNewObjectCode(out newSolutionID);
                                sc.Solutions.SetCurrentLine(lineNum);
                                sc.Solutions.SolutionID = Convert.ToInt32(newSolutionID);
                                sc.Solutions.UserFields.Fields.Item("U_ZZSXLH").Value = thisWorkOrder.ManufacturerSerialNumber;
                                //sc.Solutions.UserFields.Fields.Item("U_PCBBH").Value = solution.U_PCBBH;
                                //sc.Solutions.UserFields.Fields.Item("U_WZWL").Value = solution.U_WZWL;
                                sc.Solutions.Add();
                            }
                            else
                            {
                                company.GetLastError(out eCode, out eMesg);
                                allerror.Append("添加解决方案到SAP时异常！错误代码：" + eCode + "错误信息：" + eMesg);
                            }

                        }
                        else
                        {
                            sc.Solutions.SetCurrentLine(lineNum);
                            sc.Solutions.SolutionID = Convert.ToInt32(solution.SltCode);
                            sc.Solutions.UserFields.Fields.Item("U_ZZSXLH").Value = thisWorkOrder.ManufacturerSerialNumber;
                            //sc.Solutions.UserFields.Fields.Item("U_PCBBH").Value = solution.U_PCBBH;
                            //sc.Solutions.UserFields.Fields.Item("U_WZWL").Value = solution.U_WZWL;
                            sc.Solutions.Add();
                        }
                    }
                    #endregion

                    res = sc.Add();
                    if (res != 0)
                    {
                        company.GetLastError(out eCode, out eMesg);
                        allerror.Append("添加服务呼叫到SAP时异常！错误代码：" + eCode + "错误信息：" + eMesg);
                        Result = false;
                    }
                    else
                    {
                        company.GetNewObjectCode(out docNum);
                        allerror.Append("调用接口服务呼叫操作成功");
                        Result = true;
                    }

                }
                catch (Exception e)
                {
                    allerror.Append("调用SBO接口添加服务呼叫时异常," + res + ",异常信息：" + e.ToString() + "");
                    Result = false;
                }
            }
            errorMsg += "******" + allerror.ToString();
            return Result;
            #endregion

        }

    }
}
