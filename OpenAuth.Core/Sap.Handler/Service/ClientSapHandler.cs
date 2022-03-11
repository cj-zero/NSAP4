using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Infrastructure.Extensions;
using Microsoft.Data.SqlClient;
using NSAP.Entity.Client;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using SAPbobsCOM;

namespace Sap.Handler.Service
{
    /// <summary>
    /// 业务伙伴同步
    /// </summary>
    public class ClientSapHandler : ICapSubscribe
    {
        private string gCardCode = "";

        private readonly IUnitWork UnitWork;
        private readonly Company company;
        public ClientSapHandler(IUnitWork unitWork, Company company)
        {
            UnitWork = unitWork;
            this.company = company;
        }
        [CapSubscribe("Serve.Client.Create")]
        public async Task ClientHandle(int jobID)
        {
            var wfa_job = UnitWork.FindSingle<wfa_job>(s => s.job_id == jobID);
            var client = ByteExtension.ToDeSerialize<clientOCRD>(wfa_job.job_data);
            int res, eCode = 0;

            string eMesg = string.Empty;
            string errorMsg = "";
            if (client.ClientOperateType == "edit") //修改
            {
                if (company != null)
                {
                    try
                    {
                        //string sapConn = AidTool.GetSqlConnection(client.SboId);
                        #region 调接口
                        errorMsg += string.Format("修改【业务伙伴:{0}】开始调用接口! ", client.CardCode);
                        SAPbobsCOM.BusinessPartners bp = (SAPbobsCOM.BusinessPartners)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);
                        bp.GetByKey(client.CardCode);

                        #region 基本信息
                        bp.CardName = client.CardName;
                        bp.CardForeignName = client.CardFName;
                        bp.CompanyPrivate = client.CmpPrivate == "C" ? BoCardCompanyTypes.cCompany : BoCardCompanyTypes.cPrivate;  //公司/个人
                        #region 业务伙伴类型
                        if (client.CardType == "C")
                        {
                            bp.CardType = BoCardTypes.cCustomer;
                        }
                        else if (client.CardType == "L")
                        {
                            bp.CardType = BoCardTypes.cLid;
                        }
                        else if (client.CardType == "S")
                        {
                            bp.CardType = BoCardTypes.cSupplier;
                        }
                        #endregion
                        bp.GroupCode = Convert.ToInt32(client.GroupCode);   //组代码
                        bp.UserFields.Fields.Item("U_is_reseller").Value = client.is_reseller;
                        bp.UserFields.Fields.Item("U_EndCustomerName").Value = client.EndCustomerName;
                        bp.UserFields.Fields.Item("U_EndCustomerContact").Value = client.EndCustomerContact;
                        if (client.CmpPrivate == "C" && (client.CardType == "C" || client.CardType == "L") && client.GroupCode != "102")
                        {
                            bp.UserFields.Fields.Item("U_Prefix").Value = client.CardNamePrefix;
                            bp.UserFields.Fields.Item("U_Name").Value = client.CardNameCore;
                            bp.UserFields.Fields.Item("U_Suffix").Value = client.CardNameSuffix;
                        }
                        else
                        {
                            bp.UserFields.Fields.Item("U_Prefix").Value = "";
                            bp.UserFields.Fields.Item("U_Name").Value = "";
                            bp.UserFields.Fields.Item("U_Suffix").Value = "";
                        }
                        bp.Currency = client.Currency;     //货币
                        if (!string.IsNullOrEmpty(client.ShipType)) bp.ShippingType = Convert.ToInt32(client.ShipType);     //装运类型
                        bp.FederalTaxID = client.LicTradNum;      //国税编号
                        if (!string.IsNullOrEmpty(client.IntrntSite)) bp.Website = client.IntrntSite;       //网站
                        //bp.SalesPersonCode = Convert.ToInt32(client.SlpCode);      //销售员编号
                        if (!string.IsNullOrEmpty(client.DfTcnicianCode)) bp.DefaultTechnician = Convert.ToInt32(client.DfTcnicianCode);      //售后主管 技术员编号
                        if (!string.IsNullOrEmpty(client.Territory)) bp.Territory = Convert.ToInt32(client.Territory);  //地区
                        if (!string.IsNullOrEmpty(client.IndustryC)) bp.Industry = Convert.ToInt32(client.IndustryC);   //行业
                        #endregion
                        errorMsg += "基本信息完成！";

                        #region 常规
                        bp.Phone1 = client.Phone1;          //电话1
                        //bp.AliasName = client.AliasName;    //别名
                        bp.Cellular = client.Cellular;      //移动电话
                        bp.Fax = client.Fax;                //传真
                        bp.EmailAddress = client.E_Mail;    //电子邮件
                        bp.PayTermsGrpCode = Convert.ToInt32(client.GroupNum);        //付款条款代码
                        //bp.GTSRegNo = client.GTSRegNum;                 //金税注册号
                        //bp.GTSBankAccountNo = client.GTSBankAct;        //金税账号
                        //bp.GTSBillingAddrTel = client.GTSBilAddr;       //金税开票地址
                        bp.FreeText = client.FreeText;       //备注

                        #region 业务伙伴状态
                        if (client.IsActive == "1")        //活跃
                        {
                            bp.Frozen = BoYesNoEnum.tNO;
                            bp.Valid = BoYesNoEnum.tYES;
                            if (!string.IsNullOrEmpty(client.ValidFrom)) bp.ValidFrom = Convert.ToDateTime(client.ValidFrom);  //可用从
                            if (!string.IsNullOrEmpty(client.ValidTo)) bp.ValidTo = Convert.ToDateTime(client.ValidTo);   //可用至
                            bp.ValidRemarks = client.ValidComm;      //可用备注
                        }
                        else if (client.IsActive == "2")   //冻结
                        {
                            bp.Valid = BoYesNoEnum.tNO;
                            bp.Frozen = BoYesNoEnum.tYES;
                            if (!string.IsNullOrEmpty(client.FrozenFrom)) bp.FrozenFrom = Convert.ToDateTime(client.FrozenFrom); //冻结从
                            if (!string.IsNullOrEmpty(client.FrozenTo)) bp.FrozenTo = Convert.ToDateTime(client.FrozenTo);  //冻结至
                            bp.FrozenRemarks = client.FrozenComm;    //冻结备注
                        }
                        else if (client.IsActive == "3")   //高级
                        {
                            bp.Valid = BoYesNoEnum.tYES;
                            bp.Frozen = BoYesNoEnum.tYES;
                            if (!string.IsNullOrEmpty(client.ValidFrom)) bp.ValidFrom = Convert.ToDateTime(client.ValidFrom);  //可用从
                            if (!string.IsNullOrEmpty(client.ValidTo)) bp.ValidTo = Convert.ToDateTime(client.ValidTo);   //可用至
                            bp.ValidRemarks = client.ValidComm;      //可用备注
                            if (!string.IsNullOrEmpty(client.FrozenFrom)) bp.FrozenFrom = Convert.ToDateTime(client.FrozenFrom); //冻结从
                            if (!string.IsNullOrEmpty(client.FrozenTo)) bp.FrozenTo = Convert.ToDateTime(client.FrozenTo);  //冻结至
                            bp.FrozenRemarks = client.FrozenComm;    //冻结备注
                        }
                        #endregion

                        bp.ContactPerson = client.CntctPrsn;      //联系人
                        #endregion
                        errorMsg += "常规完成！";

                        #region 默认 开票到
                        bp.BilltoDefault = client.BillToDef;      //开票到地址标识
                        bp.Country = client.Country;            //收款方国家
                        if (!string.IsNullOrEmpty(client.State1)) bp.BillToState = client.State1;     //收款方省
                        bp.City = client.City;                  //收款方城市
                        bp.BillToBuildingFloorRoom = client.Building;      //收款方详细地址
                        bp.ZipCode = client.ZipCode;            //收款方邮编
                        #endregion
                        errorMsg += "默认 开票到完成！";

                        #region 默认 运达到
                        foreach (clientCRD1 crd1Ext in client.AddrList)  //补漏
                        {
                            if (crd1Ext.AdresType == "S" && crd1Ext.IsDefault == "1" && client.ShipToDef != crd1Ext.Address)
                            {
                                client.ShipToDef = crd1Ext.Address;
                            }
                        }
                        bp.ShipToDefault = client.ShipToDef;      //运达到地址标识
                        bp.MailCountry = client.MailCountr;        //收货方国家
                        bp.MailCity = client.MailCity;             //收货方城市
                        bp.ShipToBuildingFloorRoom = client.MailBuildi;    //收货方详细地址
                        bp.MailZipCode = client.MailZipCod;        //收货方邮编
                        #endregion
                        errorMsg += "默认 运达到完成！";

                        #region 联系人
                        if (client.ContactList.Count > 0)
                        {
                            string contactListSql = "SELECT CntctCode, CardCode, Name, Position, [Address], Tel1, Tel2, Cellolar, Fax, E_MailL, Notes1, Notes2, BirthDate, Gender, Active, U_ACCT, U_BANK FROM OCPR WHERE CardCode=@CardCode order by CntctCode ASC";
                            DataTable contactListDt = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, contactListSql, CommandType.Text, new SqlParameter("@CardCode", client.CardCode));

                            #region 验证是否有相同名称的联系人

                            #region 把联系人取出【不可操作的】
                            string rExistsCntctCode = "";
                            foreach (clientOCPR pa in client.ContactList)  //把联系人编号取出【可操作的】
                            {
                                if (!string.IsNullOrEmpty(pa.CntctCode))
                                {
                                    rExistsCntctCode += rExistsCntctCode.Length > 0 ? "," + pa.CntctCode : pa.CntctCode;
                                }
                            }
                            DataTable rExistsContactList = new DataTable();
                            if (rExistsCntctCode.Length > 0)
                            {
                                string rExistsContactListSql = string.Format("SELECT CntctCode, CardCode, Name FROM OCPR WHERE CardCode=@CardCode AND CntctCode NOT IN ({0})", rExistsCntctCode);
                                rExistsContactList = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, rExistsContactListSql, CommandType.Text, new SqlParameter("@CardCode", client.CardCode));
                            }
                            #endregion
                            if (rExistsContactList.Rows.Count > 0)  //当联系人大于零【不可操作的】
                            {
                                List<clientOCPR> listOcpr = new List<clientOCPR>();
                                #region 循环找出重名的联系人
                                for (int h = 0; h < rExistsContactList.Rows.Count; h++)
                                {
                                    string rNewName = ""; int rIsExistsNum = 0;
                                    foreach (clientOCPR pb in client.ContactList)   //如果联系人重名【不可操作的】
                                    {
                                        if (pb.Name.ToLower().Trim() == rExistsContactList.Rows[h][2].ToString().ToLower().Trim())
                                        {
                                            rIsExistsNum++; break;
                                        }
                                    }
                                    if (rIsExistsNum > 0)
                                    {    //有重名的联系人
                                        int rIsExistsNum2 = 1, rIsExistsNum3 = 0, rIsExistsNum4 = 0, rIsExistsNum5 = 0;
                                        string rOldName = rExistsContactList.Rows[h][2].ToString().Trim();
                                        while (rIsExistsNum2 > 0)   //循环计算出不重名的名称
                                        {
                                            #region 循环计算出不重名的名称
                                            rIsExistsNum3 = 0; rIsExistsNum4 = 0; rIsExistsNum5 = 0;
                                            rNewName = string.Format(rOldName + "{0}", rIsExistsNum);
                                            //查找当前计算出的联系人名称是否重复【可操作】
                                            foreach (clientOCPR pc in client.ContactList)
                                            {
                                                if (pc.Name.ToLower().Trim() == rNewName.ToLower())
                                                {
                                                    rIsExistsNum3++; break;
                                                }
                                            }
                                            //查找当前计算出的联系人名称是否重复【不可操作】
                                            if (rIsExistsNum3 == 0)
                                            {
                                                for (int k = 0; k < rExistsContactList.Rows.Count; k++)
                                                {
                                                    if (rExistsContactList.Rows[k][2].ToString().Trim() == rNewName)
                                                    {
                                                        rIsExistsNum4++; break;
                                                    }
                                                }
                                            }
                                            if (rIsExistsNum3 == 0 && rIsExistsNum4 == 0)
                                            {
                                                //查找当前计算出的联系人名称是否重复【新生成的】
                                                foreach (clientOCPR pd in listOcpr)
                                                {
                                                    if (pd.Name.ToLower() == rNewName.ToLower())
                                                    {
                                                        rIsExistsNum5++; break;
                                                    }
                                                }
                                            }
                                            if (rIsExistsNum3 == 0 && rIsExistsNum4 == 0 && rIsExistsNum5 == 0)
                                            {
                                                rIsExistsNum2 = 0;
                                            }
                                            else
                                            {
                                                rIsExistsNum++;
                                            }
                                            #endregion
                                        }
                                        clientOCPR ocpr2 = new clientOCPR();
                                        ocpr2.CntctCode = rExistsContactList.Rows[h][0].ToString();
                                        ocpr2.Name = rNewName;
                                        listOcpr.Add(ocpr2);
                                    }
                                }
                                #endregion

                                #region 修改新生成的联系人名称
                                if (listOcpr.Count > 0)
                                {
                                    foreach (clientOCPR pe in listOcpr)
                                    {
                                        int rExistsEditLineNum = 0;
                                        for (int l = 0; l < contactListDt.Rows.Count; l++)
                                        {
                                            if (pe.CntctCode == contactListDt.Rows[l][0].ToString())
                                            {
                                                rExistsEditLineNum = l; break;
                                            }
                                        }
                                        bp.ContactEmployees.SetCurrentLine(rExistsEditLineNum);
                                        bp.ContactEmployees.Name = pe.Name;       //新联系人名称
                                        bp.ContactEmployees.Add();
                                    }
                                }
                                #endregion
                            }

                            #endregion

                            #region 更新联系人
                            int rLineNum = contactListDt.Rows.Count, rEditLineNum = 0;
                            foreach (clientOCPR ocpr in client.ContactList)
                            {
                                if (!string.IsNullOrEmpty(ocpr.CntctCode))
                                {
                                    #region 已存在
                                    for (int m = 0; m < contactListDt.Rows.Count; m++)
                                    {
                                        if (ocpr.CntctCode == contactListDt.Rows[m][0].ToString())
                                        {
                                            rEditLineNum = m; break;
                                        }
                                    }
                                    bp.ContactEmployees.SetCurrentLine(rEditLineNum);
                                    bp.ContactEmployees.Active = ocpr.Active == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                                    bp.ContactEmployees.Name = ocpr.Name.Trim();       //联系人名称
                                    switch (ocpr.Gender)            //性别
                                    {
                                        case "E":
                                            bp.ContactEmployees.Gender = SAPbobsCOM.BoGenderTypes.gt_Undefined;
                                            break;
                                        case "M":
                                            bp.ContactEmployees.Gender = SAPbobsCOM.BoGenderTypes.gt_Male;
                                            break;
                                        case "F":
                                            bp.ContactEmployees.Gender = SAPbobsCOM.BoGenderTypes.gt_Female;
                                            break;
                                        default:
                                            bp.ContactEmployees.Gender = SAPbobsCOM.BoGenderTypes.gt_Undefined;
                                            break;
                                    }
                                    bp.ContactEmployees.Title = ocpr.Title;             //标题
                                    bp.ContactEmployees.Position = ocpr.Position;       //职位
                                    bp.ContactEmployees.Address = ocpr.Address;         //地址
                                    bp.ContactEmployees.Remarks1 = ocpr.Notes1;         //备注1
                                    bp.ContactEmployees.Remarks2 = ocpr.Notes2;         //备注2
                                    bp.ContactEmployees.Phone1 = ocpr.Tel1;             //电话1
                                    bp.ContactEmployees.Phone2 = ocpr.Tel2;             //电话2
                                    bp.ContactEmployees.MobilePhone = ocpr.Cellolar;    //移动电话
                                    bp.ContactEmployees.Fax = ocpr.Fax;                 //传真
                                    bp.ContactEmployees.E_Mail = ocpr.E_MailL;          //电子邮件
                                    if (!string.IsNullOrEmpty(ocpr.BirthDate) && ocpr.BirthDate != "0000/0/0 0:00:00") bp.ContactEmployees.DateOfBirth = Convert.ToDateTime(ocpr.BirthDate);  //生日
                                    bp.ContactEmployees.UserFields.Fields.Item("U_ACCT").Value = ocpr.U_ACCT;       //账号
                                    bp.ContactEmployees.UserFields.Fields.Item("U_BANK").Value = ocpr.U_BANK;       //开户行
                                    bp.ContactEmployees.Add();
                                    #endregion
                                }
                                else
                                {
                                    #region 新添加
                                    bp.ContactEmployees.SetCurrentLine(rLineNum);
                                    bp.ContactEmployees.Active = ocpr.Active == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                                    bp.ContactEmployees.Name = ocpr.Name.Trim();       //联系人名称
                                    switch (ocpr.Gender)                        //性别
                                    {
                                        case "E":
                                            bp.ContactEmployees.Gender = SAPbobsCOM.BoGenderTypes.gt_Undefined;
                                            break;
                                        case "M":
                                            bp.ContactEmployees.Gender = SAPbobsCOM.BoGenderTypes.gt_Male;
                                            break;
                                        case "F":
                                            bp.ContactEmployees.Gender = SAPbobsCOM.BoGenderTypes.gt_Female;
                                            break;
                                        default:
                                            bp.ContactEmployees.Gender = SAPbobsCOM.BoGenderTypes.gt_Undefined;
                                            break;
                                    }
                                    bp.ContactEmployees.Title = ocpr.Title;             //标题
                                    bp.ContactEmployees.Position = ocpr.Position;       //职位
                                    bp.ContactEmployees.Address = ocpr.Address;         //地址
                                    bp.ContactEmployees.Remarks1 = ocpr.Notes1;         //备注1
                                    bp.ContactEmployees.Remarks2 = ocpr.Notes2;         //备注2
                                    bp.ContactEmployees.Phone1 = ocpr.Tel1;             //电话1
                                    bp.ContactEmployees.Phone2 = ocpr.Tel2;             //电话2
                                    bp.ContactEmployees.MobilePhone = ocpr.Cellolar;    //移动电话
                                    bp.ContactEmployees.Fax = ocpr.Fax;                 //传真
                                    bp.ContactEmployees.E_Mail = ocpr.E_MailL;          //电子邮件
                                    if (!string.IsNullOrEmpty(ocpr.BirthDate) && ocpr.BirthDate != "0000/0/0 0:00:00") bp.ContactEmployees.DateOfBirth = Convert.ToDateTime(ocpr.BirthDate);  //生日
                                    bp.ContactEmployees.UserFields.Fields.Item("U_ACCT").Value = ocpr.U_ACCT;       //账号
                                    bp.ContactEmployees.UserFields.Fields.Item("U_BANK").Value = ocpr.U_BANK;       //开户行
                                    bp.ContactEmployees.Add();
                                    rLineNum++;
                                    #endregion
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            errorMsg += string.Format("修改业务伙伴（{0}）未定义联系人", client.CardName);
                        }
                        #endregion
                        errorMsg += "联系人完成！";

                        #region 地址
                        if (client.AddrList.Count > 0)
                        {
                            #region 循环找出重名的联系人
                            int rNum1 = 0;
                            foreach (clientCRD1 pa in client.AddrList)
                            {
                                string rNewName = ""; int rIsExistsNum = 0, rNum2 = 0;
                                foreach (clientCRD1 pb in client.AddrList)   //如果地址标识重复
                                {
                                    if (pb.Address == pa.Address && rNum1 != rNum2)
                                    {
                                        rIsExistsNum++; break;
                                    }
                                    rNum2++;
                                }
                                if (rIsExistsNum > 0)
                                {    //有重复的地址标识
                                    int rIsExistsNum2 = 1, rIsExistsNum3 = 0;
                                    string rOldName = pa.Address;
                                    while (rIsExistsNum2 > 0)   //循环计算出不重复的地址标识
                                    {
                                        #region 循环计算出不重复的地址标识
                                        rIsExistsNum3 = 0;
                                        rNewName = string.Format(rOldName + "{0}", rIsExistsNum);
                                        //查找当前计算出的地址标识是否重复【可操作】
                                        foreach (clientCRD1 pc in client.AddrList)
                                        {
                                            if (pc.Address == rNewName)
                                            {
                                                rIsExistsNum3++; break;
                                            }
                                        }
                                        if (rIsExistsNum3 == 0)
                                        {
                                            rIsExistsNum2 = 0;
                                        }
                                        else
                                        {
                                            rIsExistsNum++;
                                        }
                                        #endregion
                                    }
                                    pa.Address = rNewName;
                                }
                                rNum1++;
                            }
                            #endregion

                            string addrListSql = "SELECT LineNum, [Address], CardCode, AdresType, ZipCode, City, Country, [State], Building, U_Active FROM CRD1 WHERE CardCode=@CardCode order by LineNum ASC";
                            DataTable addrListDt = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, addrListSql, CommandType.Text, new SqlParameter("@CardCode", client.CardCode));

                            int rLineNum = addrListDt.Rows.Count, rEditLineNum = 0;
                            foreach (clientCRD1 crd1 in client.AddrList)
                            {
                                if (!string.IsNullOrEmpty(crd1.LineNum))
                                {
                                    #region 已存在
                                    for (int j = 0; j < addrListDt.Rows.Count; j++)
                                    {
                                        if (crd1.LineNum == addrListDt.Rows[j][0].ToString())
                                        {
                                            rEditLineNum = j; break;
                                        }
                                    }
                                    bp.Addresses.SetCurrentLine(rEditLineNum);
                                    bp.Addresses.UserFields.Fields.Item("U_Active").Value = crd1.Active == "N" ? "N" : "Y";  //是否可用
                                    switch (crd1.AdresType)       //地址类型
                                    {
                                        case "B":
                                            bp.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_BillTo;
                                            break;
                                        case "S":
                                            bp.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_ShipTo;
                                            break;
                                    }
                                    bp.Addresses.AddressName = crd1.Address;          //地址标识
                                    bp.Addresses.Country = crd1.CountryId;            //国家
                                    bp.Addresses.State = string.IsNullOrEmpty(crd1.StateId) ? "" : crd1.StateId; //省
                                    bp.Addresses.City = crd1.City;              //城市
                                    bp.Addresses.BuildingFloorRoom = crd1.Building;     //详细地址
                                    bp.Addresses.ZipCode = crd1.ZipCode;     //邮编
                                    bp.Addresses.Add();
                                    #endregion
                                }
                                else
                                {
                                    #region 新添加
                                    bp.Addresses.SetCurrentLine(rLineNum);
                                    bp.Addresses.UserFields.Fields.Item("U_Active").Value = crd1.Active == "N" ? "N" : "Y";  //是否可用
                                    switch (crd1.AdresType)       //地址类型
                                    {
                                        case "B":
                                            bp.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_BillTo;
                                            break;
                                        case "S":
                                            bp.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_ShipTo;
                                            break;
                                        default:
                                            bp.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_BillTo;
                                            break;
                                    }
                                    bp.Addresses.AddressName = crd1.Address;          //地址标识
                                    bp.Addresses.Country = crd1.CountryId;            //国家
                                    if (!string.IsNullOrEmpty(crd1.StateId)) bp.Addresses.State = crd1.StateId; //省
                                    bp.Addresses.City = crd1.City;              //城市
                                    bp.Addresses.BuildingFloorRoom = crd1.Building;     //详细地址
                                    bp.Addresses.ZipCode = crd1.ZipCode;     //邮编
                                    bp.Addresses.Add();
                                    rLineNum++;
                                    #endregion
                                }
                            }
                        }
                        else
                        {
                            errorMsg += string.Format("修改业务伙伴（{0}）未定义地址", client.CardName);
                        }
                        #endregion
                        errorMsg += "地址完成！";

                        #region 扩展信息
                        if (!string.IsNullOrEmpty(client.CustomFields))  // 自定义字段
                        {
                            string[] fieldsName = client.CustomFields.Replace(",", "，").Replace("≮0≯", ",").Split(',');
                            string[] fieldsValue = "".Split(',');
                            for (int i = 0; i < fieldsName.Length; i++)
                            {
                                fieldsValue = fieldsName[i].Replace(":", "：").Replace("≮1≯", ":").Split(':');
                                if (!string.IsNullOrEmpty(fieldsValue[0]) && !string.IsNullOrEmpty(fieldsValue[1]) && IsExistCustomFields("OCRD", fieldsValue[0].ToString()))
                                {
                                    bp.UserFields.Fields.Item(fieldsValue[0]).Value = fieldsValue[1];
                                }
                            }
                        }
                        #endregion
                        errorMsg += "扩展信息完成！";

                        #region 属性
                        bp.set_Properties(1, client.QryGroup1 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性1
                        bp.set_Properties(2, client.QryGroup2 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性2
                        bp.set_Properties(3, client.QryGroup3 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性3
                        bp.set_Properties(4, client.QryGroup4 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性4
                        bp.set_Properties(5, client.QryGroup5 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性5
                        bp.set_Properties(6, client.QryGroup6 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性6
                        bp.set_Properties(7, client.QryGroup7 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性7
                        bp.set_Properties(8, client.QryGroup8 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性8
                        bp.set_Properties(9, client.QryGroup9 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性9
                        bp.set_Properties(10, client.QryGroup10 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性10
                        bp.set_Properties(11, client.QryGroup11 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性11
                        bp.set_Properties(12, client.QryGroup12 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性12
                        bp.set_Properties(13, client.QryGroup13 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性13
                        bp.set_Properties(14, client.QryGroup14 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性14
                        bp.set_Properties(15, client.QryGroup15 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性15
                        bp.set_Properties(16, client.QryGroup16 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性16
                        bp.set_Properties(17, client.QryGroup17 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性17
                        bp.set_Properties(18, client.QryGroup18 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性18
                        bp.set_Properties(19, client.QryGroup19 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性19
                        bp.set_Properties(20, client.QryGroup20 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性20
                        bp.set_Properties(21, client.QryGroup21 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性21
                        bp.set_Properties(22, client.QryGroup22 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性22
                        bp.set_Properties(23, client.QryGroup23 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性23
                        bp.set_Properties(24, client.QryGroup24 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性24
                        bp.set_Properties(25, client.QryGroup25 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性25
                        bp.set_Properties(26, client.QryGroup26 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性26
                        bp.set_Properties(27, client.QryGroup27 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性27
                        bp.set_Properties(28, client.QryGroup28 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性28
                        bp.set_Properties(29, client.QryGroup29 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性29
                        bp.set_Properties(30, client.QryGroup30 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性30
                        bp.set_Properties(31, client.QryGroup31 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性31
                        bp.set_Properties(32, client.QryGroup32 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性32
                        bp.set_Properties(33, client.QryGroup33 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性33
                        bp.set_Properties(34, client.QryGroup34 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性34
                        bp.set_Properties(35, client.QryGroup35 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性35
                        bp.set_Properties(36, client.QryGroup36 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性36
                        bp.set_Properties(37, client.QryGroup37 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性37
                        bp.set_Properties(38, client.QryGroup38 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性38
                        bp.set_Properties(39, client.QryGroup39 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性39
                        bp.set_Properties(40, client.QryGroup40 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性40
                        bp.set_Properties(41, client.QryGroup41 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性41
                        bp.set_Properties(42, client.QryGroup42 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性42
                        bp.set_Properties(43, client.QryGroup43 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性43
                        bp.set_Properties(44, client.QryGroup44 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性44
                        bp.set_Properties(45, client.QryGroup45 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性45
                        bp.set_Properties(46, client.QryGroup46 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性46
                        bp.set_Properties(47, client.QryGroup47 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性47
                        bp.set_Properties(48, client.QryGroup48 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性48
                        bp.set_Properties(49, client.QryGroup49 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性49
                        bp.set_Properties(50, client.QryGroup50 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性50
                        bp.set_Properties(51, client.QryGroup51 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性51
                        bp.set_Properties(52, client.QryGroup52 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性52
                        bp.set_Properties(53, client.QryGroup53 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性53
                        bp.set_Properties(54, client.QryGroup54 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性54
                        bp.set_Properties(55, client.QryGroup55 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性55
                        bp.set_Properties(56, client.QryGroup56 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性56
                        bp.set_Properties(57, client.QryGroup57 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性57
                        bp.set_Properties(58, client.QryGroup58 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性58
                        bp.set_Properties(59, client.QryGroup59 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性59
                        bp.set_Properties(60, client.QryGroup60 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性60
                        bp.set_Properties(61, client.QryGroup61 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性61
                        bp.set_Properties(62, client.QryGroup62 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性62
                        bp.set_Properties(63, client.QryGroup63 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性63
                        bp.set_Properties(64, client.QryGroup64 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性64
                        #endregion
                        errorMsg += "属性完成！";

                        res = bp.Update();
                        if (res != 0)
                        {
                            company.GetLastError(out eCode, out errorMsg);
                            errorMsg += "调SAP接口，修改业务伙伴信息时异常！流程任务编号：【" + jobID + "】错误代码：" + eCode + "错误信息：" + eMesg;

                        }
                        else
                        {
                            errorMsg += "调SAP接口，修改业务伙伴【" + jobID + "】操作成功";

                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        errorMsg += "修改业务伙伴【" + jobID + "】时调接口错误！错误代号:" + eCode + " 错误信息：" + ex.Message;

                    }
                }
            }
            else
            { //添加
                if (company != null)
                {
                    try
                    {

                        if (client.IsApplicationChange == "0" || (client.IsApplicationChange == "1" && client.ChangeType == "Add"))
                        {
                            #region 调接口
                            errorMsg += string.Format("添加【业务伙伴:{0}】开始调用接口! ", client.CardName);
                            SAPbobsCOM.BusinessPartners bp = (SAPbobsCOM.BusinessPartners)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);

                            string rCardCode = GetCardCodeByCardType(client.CardType);
                            if (rCardCode == "0")
                            {
                                errorMsg += string.Format("生成业务伙伴代码不对【{0}】！", rCardCode);
                            }
                            else
                            {
                                #region 基本信息
                                bp.CardCode = rCardCode;
                                bp.CardName = client.CardName;
                                bp.CardForeignName = client.CardFName;
                                bp.CompanyPrivate = client.CmpPrivate == "C" ? BoCardCompanyTypes.cCompany : BoCardCompanyTypes.cPrivate;  //公司/个人
                                #region 业务伙伴类型
                                if (client.CardType == "C")
                                {
                                    bp.CardType = BoCardTypes.cCustomer;
                                }
                                else if (client.CardType == "L")
                                {
                                    bp.CardType = BoCardTypes.cLid;
                                }
                                else if (client.CardType == "S")
                                {
                                    bp.CardType = BoCardTypes.cSupplier;
                                }
                                #endregion
                                bp.GroupCode = Convert.ToInt32(client.GroupCode);   //组代码
                                bp.UserFields.Fields.Item("U_is_reseller").Value = client.is_reseller;
                                bp.UserFields.Fields.Item("U_EndCustomerName").Value = client.EndCustomerName;
                                bp.UserFields.Fields.Item("U_EndCustomerContact").Value = client.EndCustomerContact;
                                if (client.CmpPrivate == "C" && (client.CardType == "C" || client.CardType == "L") && client.GroupCode != "102")
                                {
                                    bp.UserFields.Fields.Item("U_Prefix").Value = client.CardNamePrefix;
                                    bp.UserFields.Fields.Item("U_Name").Value = client.CardNameCore;
                                    bp.UserFields.Fields.Item("U_Suffix").Value = client.CardNameSuffix;
                                }
                                bp.Currency = client.Currency;     //货币
                                if (!string.IsNullOrEmpty(client.ShipType)) bp.ShippingType = Convert.ToInt32(client.ShipType);     //装运类型
                                bp.FederalTaxID = client.LicTradNum;      //国税编号
                                bp.Website = client.IntrntSite;           //网站
                                bp.SalesPersonCode = Convert.ToInt32(client.SlpCode);      //销售员编号
                                if (!string.IsNullOrEmpty(client.DfTcnicianCode)) bp.DefaultTechnician = Convert.ToInt32(client.DfTcnicianCode);      //售后主管 技术员编号
                                if (!string.IsNullOrEmpty(client.Territory)) bp.Territory = Convert.ToInt32(client.Territory);  //地区
                                if (!string.IsNullOrEmpty(client.IndustryC)) bp.Industry = Convert.ToInt32(client.IndustryC);   //行业
                                #endregion
                                errorMsg += "基本信息完成！";

                                #region 常规
                                bp.Phone1 = client.Phone1;          //电话1
                                bp.AliasName = client.AliasName;    //别名
                                bp.Cellular = client.Cellular;      //移动电话
                                bp.Fax = client.Fax;                //传真
                                bp.EmailAddress = client.E_Mail;    //电子邮件
                                bp.PayTermsGrpCode = Convert.ToInt32(client.GroupNum);        //付款条款代码
                                //bp.GTSRegNo = client.GTSRegNum;                 //金税注册号
                                //bp.GTSBankAccountNo = client.GTSBankAct;        //金税账号
                                //bp.GTSBillingAddrTel = client.GTSBilAddr;       //金税开票地址
                                bp.FreeText = client.FreeText;       //备注
                                #region 业务伙伴状态
                                bp.Frozen = BoYesNoEnum.tNO;
                                bp.Valid = BoYesNoEnum.tYES;
                                bp.ValidRemarks = "";      //可用备注

                                //if (client.IsActive == "1")        //活跃
                                //{
                                //    bp.Frozen = BoYesNoEnum.tNO;
                                //    bp.Valid = BoYesNoEnum.tYES;
                                //    if (!string.IsNullOrEmpty(client.ValidFrom)) bp.ValidFrom = Convert.ToDateTime(client.ValidFrom);  //可用从
                                //    if (!string.IsNullOrEmpty(client.ValidTo)) bp.ValidTo = Convert.ToDateTime(client.ValidTo);   //可用至
                                //    bp.ValidRemarks = client.ValidComm;      //可用备注
                                //}
                                //else if (client.IsActive == "2")   //冻结
                                //{
                                //    bp.Valid = BoYesNoEnum.tNO;
                                //    bp.Frozen = BoYesNoEnum.tYES;
                                //    if (!string.IsNullOrEmpty(client.FrozenFrom)) bp.FrozenFrom = Convert.ToDateTime(client.FrozenFrom); //冻结从
                                //    if (!string.IsNullOrEmpty(client.FrozenTo)) bp.FrozenTo = Convert.ToDateTime(client.FrozenTo);  //冻结至
                                //    bp.FrozenRemarks = client.FrozenComm;    //冻结备注
                                //}
                                //else if (client.IsActive == "3")   //高级
                                //{
                                //    bp.Valid = BoYesNoEnum.tYES;
                                //    bp.Frozen = BoYesNoEnum.tYES;
                                //    if (!string.IsNullOrEmpty(client.ValidFrom)) bp.ValidFrom = Convert.ToDateTime(client.ValidFrom);  //可用从
                                //    if (!string.IsNullOrEmpty(client.ValidTo)) bp.ValidTo = Convert.ToDateTime(client.ValidTo);   //可用至
                                //    bp.ValidRemarks = client.ValidComm;      //可用备注
                                //    if (!string.IsNullOrEmpty(client.FrozenFrom)) bp.FrozenFrom = Convert.ToDateTime(client.FrozenFrom); //冻结从
                                //    if (!string.IsNullOrEmpty(client.FrozenTo)) bp.FrozenTo = Convert.ToDateTime(client.FrozenTo);  //冻结至
                                //    bp.FrozenRemarks = client.FrozenComm;    //冻结备注
                                //}
                                #endregion

                                bp.ContactPerson = client.CntctPrsn;      //联系人
                                #endregion
                                errorMsg += "常规完成！";

                                #region 默认 开票到
                                bp.BilltoDefault = client.BillToDef;      //开票到地址标识
                                bp.Country = client.Country;            //收款方国家
                                if (!string.IsNullOrEmpty(client.State1)) bp.BillToState = client.State1;     //收款方省
                                bp.City = client.City;                  //收款方城市
                                bp.BillToBuildingFloorRoom = client.Building;      //收款方详细地址
                                bp.ZipCode = client.ZipCode;            //收款方邮编
                                #endregion
                                errorMsg += "默认 开票到完成！";

                                #region 默认 运达到
                                bp.ShipToDefault = client.ShipToDef;      //运达到地址标识
                                bp.MailCountry = client.MailCountr;        //收货方国家
                                bp.MailCity = client.MailCity;             //收货方城市
                                bp.ShipToBuildingFloorRoom = client.MailBuildi;    //收货方详细地址
                                bp.MailZipCode = client.MailZipCod;        //收货方邮编
                                #endregion
                                errorMsg += "默认 运达到完成！";

                                #region 联系人
                                if (client.ContactList.Count > 0)
                                {
                                    int rLineNum = 0;
                                    foreach (clientOCPR ocpr in client.ContactList)
                                    {
                                        bp.ContactEmployees.SetCurrentLine(rLineNum);
                                        bp.ContactEmployees.Active = ocpr.Active == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                                        bp.ContactEmployees.Name = ocpr.Name.Trim();       //联系人名称
                                        switch (ocpr.Gender)            //性别
                                        {
                                            case "E":
                                                bp.ContactEmployees.Gender = SAPbobsCOM.BoGenderTypes.gt_Undefined;
                                                break;
                                            case "M":
                                                bp.ContactEmployees.Gender = SAPbobsCOM.BoGenderTypes.gt_Male;
                                                break;
                                            case "F":
                                                bp.ContactEmployees.Gender = SAPbobsCOM.BoGenderTypes.gt_Female;
                                                break;
                                            default:
                                                bp.ContactEmployees.Gender = SAPbobsCOM.BoGenderTypes.gt_Undefined;
                                                break;
                                        }
                                        bp.ContactEmployees.Title = ocpr.Title;             //标题
                                        bp.ContactEmployees.Position = ocpr.Position;       //职位
                                        bp.ContactEmployees.Address = ocpr.Address;         //地址
                                        bp.ContactEmployees.Remarks1 = ocpr.Notes1;         //备注1
                                        bp.ContactEmployees.Remarks2 = ocpr.Notes2;         //备注2
                                        bp.ContactEmployees.Phone1 = ocpr.Tel1;             //电话1
                                        bp.ContactEmployees.Phone2 = ocpr.Tel2;             //电话2
                                        bp.ContactEmployees.MobilePhone = ocpr.Cellolar;    //移动电话
                                        bp.ContactEmployees.Fax = ocpr.Fax;                 //传真
                                        bp.ContactEmployees.E_Mail = ocpr.E_MailL;          //电子邮件
                                        if (!string.IsNullOrEmpty(ocpr.BirthDate) && ocpr.BirthDate != "0000/0/0 0:00:00") bp.ContactEmployees.DateOfBirth = Convert.ToDateTime(ocpr.BirthDate);  //生日
                                        bp.ContactEmployees.UserFields.Fields.Item("U_ACCT").Value = ocpr.U_ACCT;       //账号
                                        bp.ContactEmployees.UserFields.Fields.Item("U_BANK").Value = ocpr.U_BANK;       //开户行
                                        bp.ContactEmployees.Add();
                                        rLineNum++;
                                    }
                                }
                                else
                                {
                                    errorMsg += string.Format("添加业务伙伴（{0}）未定义联系人", client.CardName);
                                }
                                #endregion
                                errorMsg += "联系人完成！";

                                #region 地址
                                if (client.AddrList.Count > 0)
                                {
                                    int rLineNum = 0;
                                    foreach (clientCRD1 crd1 in client.AddrList)
                                    {
                                        bp.Addresses.SetCurrentLine(rLineNum);
                                        bp.Addresses.UserFields.Fields.Item("U_Active").Value = crd1.Active == "N" ? "N" : "Y";  //是否可用
                                        switch (crd1.AdresType)       //地址类型
                                        {
                                            case "B":
                                                bp.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_BillTo;
                                                break;
                                            case "S":
                                                bp.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_ShipTo;
                                                break;
                                            default:
                                                bp.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_BillTo;
                                                break;
                                        }
                                        bp.Addresses.AddressName = crd1.Address;          //地址标识
                                        bp.Addresses.Country = crd1.CountryId;            //国家
                                        if (!string.IsNullOrEmpty(crd1.StateId)) bp.Addresses.State = crd1.StateId; //省
                                        bp.Addresses.City = crd1.City;              //城市
                                        bp.Addresses.BuildingFloorRoom = crd1.Building;     //详细地址
                                        bp.Addresses.ZipCode = crd1.ZipCode;     //邮编
                                        bp.Addresses.Add();
                                        rLineNum++;
                                    }
                                }
                                else
                                {
                                    errorMsg += string.Format("添加业务伙伴（{0}）未定义地址", client.CardName);
                                }
                                #endregion
                                errorMsg += "地址完成！";

                                #region 扩展信息
                                if (!string.IsNullOrEmpty(client.CustomFields))  // 自定义字段
                                {
                                    string[] fieldsName = client.CustomFields.Replace(",", "，").Replace("≮0≯", ",").Split(',');
                                    string[] fieldsValue = "".Split(',');
                                    for (int i = 0; i < fieldsName.Length; i++)
                                    {
                                        fieldsValue = fieldsName[i].Replace(":", "：").Replace("≮1≯", ":").Split(':');
                                        if (!string.IsNullOrEmpty(fieldsValue[0]) && !string.IsNullOrEmpty(fieldsValue[1]) && IsExistCustomFields("OCRD", fieldsValue[0].ToString()))
                                        {
                                            bp.UserFields.Fields.Item(fieldsValue[0]).Value = fieldsValue[1];
                                        }
                                    }
                                }
                                #endregion
                                errorMsg += "扩展信息完成！";

                                #region 属性
                                bp.set_Properties(1, client.QryGroup1 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性1
                                bp.set_Properties(2, client.QryGroup2 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性2
                                bp.set_Properties(3, client.QryGroup3 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性3
                                bp.set_Properties(4, client.QryGroup4 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性4
                                bp.set_Properties(5, client.QryGroup5 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性5
                                bp.set_Properties(6, client.QryGroup6 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性6
                                bp.set_Properties(7, client.QryGroup7 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性7
                                bp.set_Properties(8, client.QryGroup8 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性8
                                bp.set_Properties(9, client.QryGroup9 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性9
                                bp.set_Properties(10, client.QryGroup10 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性10
                                bp.set_Properties(11, client.QryGroup11 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性11
                                bp.set_Properties(12, client.QryGroup12 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性12
                                bp.set_Properties(13, client.QryGroup13 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性13
                                bp.set_Properties(14, client.QryGroup14 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性14
                                bp.set_Properties(15, client.QryGroup15 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性15
                                bp.set_Properties(16, client.QryGroup16 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性16
                                bp.set_Properties(17, client.QryGroup17 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性17
                                bp.set_Properties(18, client.QryGroup18 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性18
                                bp.set_Properties(19, client.QryGroup19 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性19
                                bp.set_Properties(20, client.QryGroup20 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性20
                                bp.set_Properties(21, client.QryGroup21 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性21
                                bp.set_Properties(22, client.QryGroup22 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性22
                                bp.set_Properties(23, client.QryGroup23 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性23
                                bp.set_Properties(24, client.QryGroup24 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性24
                                bp.set_Properties(25, client.QryGroup25 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性25
                                bp.set_Properties(26, client.QryGroup26 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性26
                                bp.set_Properties(27, client.QryGroup27 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性27
                                bp.set_Properties(28, client.QryGroup28 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性28
                                bp.set_Properties(29, client.QryGroup29 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性29
                                bp.set_Properties(30, client.QryGroup30 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性30
                                bp.set_Properties(31, client.QryGroup31 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性31
                                bp.set_Properties(32, client.QryGroup32 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性32
                                bp.set_Properties(33, client.QryGroup33 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性33
                                bp.set_Properties(34, client.QryGroup34 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性34
                                bp.set_Properties(35, client.QryGroup35 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性35
                                bp.set_Properties(36, client.QryGroup36 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性36
                                bp.set_Properties(37, client.QryGroup37 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性37
                                bp.set_Properties(38, client.QryGroup38 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性38
                                bp.set_Properties(39, client.QryGroup39 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性39
                                bp.set_Properties(40, client.QryGroup40 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性40
                                bp.set_Properties(41, client.QryGroup41 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性41
                                bp.set_Properties(42, client.QryGroup42 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性42
                                bp.set_Properties(43, client.QryGroup43 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性43
                                bp.set_Properties(44, client.QryGroup44 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性44
                                bp.set_Properties(45, client.QryGroup45 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性45
                                bp.set_Properties(46, client.QryGroup46 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性46
                                bp.set_Properties(47, client.QryGroup47 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性47
                                bp.set_Properties(48, client.QryGroup48 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性48
                                bp.set_Properties(49, client.QryGroup49 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性49
                                bp.set_Properties(50, client.QryGroup50 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性50
                                bp.set_Properties(51, client.QryGroup51 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性51
                                bp.set_Properties(52, client.QryGroup52 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性52
                                bp.set_Properties(53, client.QryGroup53 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性53
                                bp.set_Properties(54, client.QryGroup54 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性54
                                bp.set_Properties(55, client.QryGroup55 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性55
                                bp.set_Properties(56, client.QryGroup56 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性56
                                bp.set_Properties(57, client.QryGroup57 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性57
                                bp.set_Properties(58, client.QryGroup58 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性58
                                bp.set_Properties(59, client.QryGroup59 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性59
                                bp.set_Properties(60, client.QryGroup60 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性60
                                bp.set_Properties(61, client.QryGroup61 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性61
                                bp.set_Properties(62, client.QryGroup62 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性62
                                bp.set_Properties(63, client.QryGroup63 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性63
                                bp.set_Properties(64, client.QryGroup64 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性64
                                #endregion
                                errorMsg += "属性完成！";

                                res = bp.Add();
                                if (res == 0)
                                {
                                    company.GetNewObjectCode(out gCardCode);
                                    errorMsg += "调SAP接口，添加业务伙伴【" + jobID + "】操作成功！返回的唯一编码：" + gCardCode;

                                    #region 把接口返回值写入wfa_job表
                                    int saveKeyValue = GetSboReturn(gCardCode, jobID.ToString());
                                    errorMsg += "保存接口的唯一编码:" + (saveKeyValue > 0 ? "成功!" : "失败!");
                                    #endregion
                                }
                                else
                                {
                                    company.GetLastError(out eCode, out eMesg);
                                    errorMsg += "调SAP接口，添加业务伙伴信息时异常！流程任务编号：【" + jobID + "】错误代码：" + eCode + "错误信息：" + eMesg;
                                }
                            }
                            #endregion
                        }
                        else if (client.IsApplicationChange == "1" && client.ChangeType == "Edit" && !string.IsNullOrEmpty(client.ChangeCardCode))
                        {
                            client.CardCode = client.ChangeCardCode;  //变更的业务伙伴
                            gCardCode = client.ChangeCardCode;  //变更的业务伙伴
                            bool gCodeIsMe = false;    //变更的业务伙伴是否属于申请人

                            #region 查询变更的业务伙伴是否属于申请人
                            string strSqlIsMe = "SELECT COUNT(*) FROM OCRD WHERE CardCode=@CardCode AND SlpCode=@SlpCode";
                            IDataParameter[] strParaIsMe = {
                               new SqlParameter("@CardCode", client.ChangeCardCode),
                               new SqlParameter("@SlpCode", client.SlpCode)
                            };
                            gCodeIsMe = Convert.ToInt32(UnitWork.ExecuteScalar(ContextType.SapDbContextType, strSqlIsMe, CommandType.Text, strParaIsMe).ToString()) > 0 ? true : false;
                            #endregion

                            #region 调接口
                            errorMsg += string.Format("变更【业务伙伴:{0} 销售员:{1}】开始调用接口! ", client.CardCode, client.SlpName);
                            SAPbobsCOM.BusinessPartners bp = (SAPbobsCOM.BusinessPartners)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);
                            bp.GetByKey(client.CardCode);

                            #region 基本信息
                            bp.CardName = client.CardName;
                            bp.CardForeignName = client.CardFName;
                            bp.CompanyPrivate = client.CmpPrivate == "C" ? BoCardCompanyTypes.cCompany : BoCardCompanyTypes.cPrivate;  //公司/个人
                            #region 业务伙伴类型
                            if (client.CardType == "C")
                            {
                                bp.CardType = BoCardTypes.cCustomer;
                            }
                            else if (client.CardType == "L")
                            {
                                bp.CardType = BoCardTypes.cLid;
                            }
                            else if (client.CardType == "S")
                            {
                                bp.CardType = BoCardTypes.cSupplier;
                            }
                            #endregion
                            bp.GroupCode = Convert.ToInt32(client.GroupCode);   //组代码
                            bp.UserFields.Fields.Item("U_is_reseller").Value = client.is_reseller;
                            bp.UserFields.Fields.Item("U_EndCustomerName").Value = client.EndCustomerName;
                            bp.UserFields.Fields.Item("U_EndCustomerContact").Value = client.EndCustomerContact;
                            if (client.CmpPrivate == "C" && (client.CardType == "C" || client.CardType == "L") && client.GroupCode != "102")
                            {
                                bp.UserFields.Fields.Item("U_Prefix").Value = client.CardNamePrefix;
                                bp.UserFields.Fields.Item("U_Name").Value = client.CardNameCore;
                                bp.UserFields.Fields.Item("U_Suffix").Value = client.CardNameSuffix;
                            }
                            else
                            {
                                bp.UserFields.Fields.Item("U_Prefix").Value = "";
                                bp.UserFields.Fields.Item("U_Name").Value = "";
                                bp.UserFields.Fields.Item("U_Suffix").Value = "";
                            }
                            //bp.Currency = client.Currency;     //货币   【error:日记账条目链接到卡】
                            if (!string.IsNullOrEmpty(client.ShipType)) bp.ShippingType = Convert.ToInt32(client.ShipType);     //装运类型
                            bp.FederalTaxID = client.LicTradNum;      //国税编号
                            bp.Website = client.IntrntSite;           //网站
                            bp.SalesPersonCode = Convert.ToInt32(client.SlpCode);           //销售员编号【申请变更需要更改销售员】
                            if (!string.IsNullOrEmpty(client.DfTcnicianCode)) bp.DefaultTechnician = Convert.ToInt32(client.DfTcnicianCode);      //售后主管 技术员编号
                            if (!string.IsNullOrEmpty(client.Territory)) bp.Territory = Convert.ToInt32(client.Territory);  //地区
                            if (!string.IsNullOrEmpty(client.IndustryC)) bp.Industry = Convert.ToInt32(client.IndustryC);   //行业
                            #endregion
                            errorMsg += "基本信息完成！";

                            #region 常规
                            bp.Phone1 = client.Phone1;          //电话1
                            //bp.AliasName = client.AliasName;    //别名
                            bp.Cellular = client.Cellular;      //移动电话
                            bp.Fax = client.Fax;                //传真
                            bp.EmailAddress = client.E_Mail;    //电子邮件
                            bp.PayTermsGrpCode = Convert.ToInt32(client.GroupNum);        //付款条款代码
                            //bp.GTSRegNo = client.GTSRegNum;                 //金税注册号
                            //bp.GTSBankAccountNo = client.GTSBankAct;        //金税账号
                            //bp.GTSBillingAddrTel = client.GTSBilAddr;       //金税开票地址
                            bp.FreeText = client.FreeText;       //备注

                            #region 业务伙伴状态
                            //if (client.IsActive == "1")        //活跃
                            //{
                            //    bp.Frozen = BoYesNoEnum.tNO;
                            //    bp.Valid = BoYesNoEnum.tYES;
                            //    if (!string.IsNullOrEmpty(client.ValidFrom)) bp.ValidFrom = Convert.ToDateTime(client.ValidFrom);  //可用从
                            //    if (!string.IsNullOrEmpty(client.ValidTo)) bp.ValidTo = Convert.ToDateTime(client.ValidTo);   //可用至
                            //    bp.ValidRemarks = client.ValidComm;      //可用备注
                            //}
                            //else if (client.IsActive == "2")   //冻结
                            //{
                            //    bp.Valid = BoYesNoEnum.tNO;
                            //    bp.Frozen = BoYesNoEnum.tYES;
                            //    if (!string.IsNullOrEmpty(client.FrozenFrom)) bp.FrozenFrom = Convert.ToDateTime(client.FrozenFrom); //冻结从
                            //    if (!string.IsNullOrEmpty(client.FrozenTo)) bp.FrozenTo = Convert.ToDateTime(client.FrozenTo);  //冻结至
                            //    bp.FrozenRemarks = client.FrozenComm;    //冻结备注
                            //}
                            //else if (client.IsActive == "3")   //高级
                            //{
                            //    bp.Valid = BoYesNoEnum.tYES;
                            //    bp.Frozen = BoYesNoEnum.tYES;
                            //    if (!string.IsNullOrEmpty(client.ValidFrom)) bp.ValidFrom = Convert.ToDateTime(client.ValidFrom);  //可用从
                            //    if (!string.IsNullOrEmpty(client.ValidTo)) bp.ValidTo = Convert.ToDateTime(client.ValidTo);   //可用至
                            //    bp.ValidRemarks = client.ValidComm;      //可用备注
                            //    if (!string.IsNullOrEmpty(client.FrozenFrom)) bp.FrozenFrom = Convert.ToDateTime(client.FrozenFrom); //冻结从
                            //    if (!string.IsNullOrEmpty(client.FrozenTo)) bp.FrozenTo = Convert.ToDateTime(client.FrozenTo);  //冻结至
                            //    bp.FrozenRemarks = client.FrozenComm;    //冻结备注
                            //}
                            #endregion

                            bp.ContactPerson = client.CntctPrsn;      //联系人
                            #endregion
                            errorMsg += "常规完成！";

                            #region 如果新默认地址标识与旧默认地址标识重复，则修改新的
                            //DataTable rAddrRepeatDt = Sql.SAPAction.ExecuteDataset(sapConn, CommandType.Text, "SELECT BillToDef,ShipToDef FROM OCRD WHERE CardCode=@CardCode", Sql.SAPAction.GetParameter("@CardCode", client.ChangeCardCode)).Tables[0];
                            //if (client.BillToDef == rAddrRepeatDt.Rows[0][0].ToString()) //默认开票到地址标识重复
                            //{
                            //    #region 循环计算出不重名的名称
                            //    string rNewName = ""; int rIsExistsNum = 1;
                            //    int rIsExistsNum2 = 1, rIsExistsNum3 = 0;
                            //    string rOldName = client.BillToDef;
                            //    while (rIsExistsNum2 > 0)   //循环计算出不重名的地址标识
                            //    {
                            //        rIsExistsNum3 = 0;
                            //        rNewName = string.Format(rOldName + "{0}", rIsExistsNum);
                            //        //查找当前计算出的地址标识是否重复【可操作】
                            //        foreach (clientCRD1 pa in client.AddrList)
                            //        {
                            //            if (pa.Address == rNewName)
                            //            {
                            //                rIsExistsNum3++; break;
                            //            }
                            //        }
                            //        if (rIsExistsNum3 == 0)
                            //        {
                            //            rIsExistsNum2 = 0;
                            //        }
                            //        else
                            //        {
                            //            rIsExistsNum++;
                            //        }
                            //    }
                            //    #endregion
                            //    #region 修改默认的地址标识
                            //    client.BillToDef = rNewName;
                            //    foreach (clientCRD1 pb in client.AddrList)
                            //    {
                            //        if (pb.Address == rOldName)
                            //        {
                            //            pb.Address = rNewName; break;
                            //        }
                            //    }
                            //    #endregion
                            //}
                            //if (client.ShipToDef == rAddrRepeatDt.Rows[0][1].ToString()) //默认运达到地址标识重复
                            //{
                            //    #region 循环计算出不重名的名称
                            //    string rNewName = ""; int rIsExistsNum = 1;
                            //    int rIsExistsNum2 = 1, rIsExistsNum3 = 0;
                            //    string rOldName = client.ShipToDef;
                            //    while (rIsExistsNum2 > 0)   //循环计算出不重名的地址标识
                            //    {
                            //        rIsExistsNum3 = 0;
                            //        rNewName = string.Format(rOldName + "{0}", rIsExistsNum);
                            //        //查找当前计算出的地址标识是否重复【可操作】
                            //        foreach (clientCRD1 pa in client.AddrList)
                            //        {
                            //            if (pa.Address == rNewName)
                            //            {
                            //                rIsExistsNum3++; break;
                            //            }
                            //        }
                            //        if (rIsExistsNum3 == 0)
                            //        {
                            //            rIsExistsNum2 = 0;
                            //        }
                            //        else
                            //        {
                            //            rIsExistsNum++;
                            //        }
                            //    }
                            //    #endregion
                            //    #region 修改默认的地址标识
                            //    client.ShipToDef = rNewName;
                            //    foreach (clientCRD1 pb in client.AddrList)
                            //    {
                            //        if (pb.Address == rOldName)
                            //        {
                            //            pb.Address = rNewName; break;
                            //        }
                            //    }
                            //    #endregion
                            //}
                            #endregion

                            #region 默认 开票到
                            bp.BilltoDefault = client.BillToDef;      //开票到地址标识
                            bp.Country = client.Country;            //收款方国家
                            bp.BillToState = string.IsNullOrEmpty(client.State1) ? "" : client.State1;  //收款方省
                            bp.City = client.City;                  //收款方城市
                            bp.BillToBuildingFloorRoom = client.Building;      //收款方详细地址
                            bp.ZipCode = client.ZipCode;            //收款方邮编
                            #endregion
                            errorMsg += "默认 开票到完成！";

                            #region 默认 运达到
                            bp.ShipToDefault = client.ShipToDef;      //运达到地址标识
                            bp.MailCountry = client.MailCountr;        //收货方国家
                            bp.MailCity = client.MailCity;             //收货方城市
                            bp.ShipToBuildingFloorRoom = client.MailBuildi;    //收货方详细地址
                            bp.MailZipCode = client.MailZipCod;        //收货方邮编
                            #endregion
                            errorMsg += "默认 运达到完成！";

                            #region 联系人
                            if (client.ContactList.Count > 0)
                            {
                                string contactListSql = "SELECT CntctCode, CardCode, Name, Position, [Address], Tel1, Tel2, Cellolar, Fax, E_MailL, Notes1, Notes2, BirthDate, Gender, Active, U_ACCT, U_BANK FROM OCPR WHERE CardCode=@CardCode order by CntctCode ASC";
                                DataTable contactListDt = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, contactListSql, CommandType.Text, new SqlParameter("@CardCode", client.CardCode));

                                #region 验证是否有相同名称的联系人
                                DataTable rExistsContactList = contactListDt;
                                if (rExistsContactList.Rows.Count > 0)  //当联系人大于零【不可操作的】
                                {
                                    List<clientOCPR> listOcpr = new List<clientOCPR>();
                                    #region 循环找出重名的联系人
                                    for (int h = 0; h < rExistsContactList.Rows.Count; h++)
                                    {
                                        string rNewName = ""; int rIsExistsNum = 0;
                                        foreach (clientOCPR pb in client.ContactList)   //如果联系人重名【不可操作的】
                                        {
                                            if (pb.Name.ToLower().Trim() == rExistsContactList.Rows[h][2].ToString().ToLower().Trim())
                                            {
                                                rIsExistsNum++; break;
                                            }
                                        }
                                        if (rIsExistsNum > 0)
                                        {    //有重名的联系人
                                            int rIsExistsNum2 = 1, rIsExistsNum3 = 0, rIsExistsNum4 = 0, rIsExistsNum5 = 0;
                                            string rOldName = rExistsContactList.Rows[h][2].ToString().Trim();
                                            while (rIsExistsNum2 > 0)   //循环计算出不重名的名称
                                            {
                                                #region 循环计算出不重名的名称
                                                rIsExistsNum3 = 0; rIsExistsNum4 = 0; rIsExistsNum5 = 0;
                                                rNewName = string.Format(rOldName + "{0}", rIsExistsNum);
                                                //查找当前计算出的联系人名称是否重复【可操作】
                                                foreach (clientOCPR pc in client.ContactList)
                                                {
                                                    if (pc.Name.ToLower().Trim() == rNewName.ToLower())
                                                    {
                                                        rIsExistsNum3++; break;
                                                    }
                                                }
                                                //查找当前计算出的联系人名称是否重复【不可操作】
                                                if (rIsExistsNum3 == 0)
                                                {
                                                    for (int k = 0; k < rExistsContactList.Rows.Count; k++)
                                                    {
                                                        if (rExistsContactList.Rows[k][2].ToString() == rNewName)
                                                        {
                                                            rIsExistsNum4++; break;
                                                        }
                                                    }
                                                }
                                                if (rIsExistsNum3 == 0 && rIsExistsNum4 == 0)
                                                {
                                                    //查找当前计算出的联系人名称是否重复【新生成的】
                                                    foreach (clientOCPR pd in listOcpr)
                                                    {
                                                        if (pd.Name.ToLower() == rNewName.ToLower())
                                                        {
                                                            rIsExistsNum5++; break;
                                                        }
                                                    }
                                                }
                                                if (rIsExistsNum3 == 0 && rIsExistsNum4 == 0 && rIsExistsNum5 == 0)
                                                {
                                                    rIsExistsNum2 = 0;
                                                }
                                                else
                                                {
                                                    rIsExistsNum++;
                                                }
                                                #endregion
                                            }
                                            clientOCPR ocpr2 = new clientOCPR();
                                            ocpr2.CntctCode = rExistsContactList.Rows[h][0].ToString();
                                            ocpr2.Name = rNewName;
                                            listOcpr.Add(ocpr2);
                                        }
                                    }
                                    #endregion

                                    #region 修改新生成的联系人名称与禁用原来的联系人
                                    int rExistsEditLineNum = -1;
                                    for (int l = 0; l < contactListDt.Rows.Count; l++)
                                    {
                                        if (listOcpr.Count > 0)
                                        {
                                            int rExistsEditLineNum2 = 0;
                                            foreach (clientOCPR pe in listOcpr)
                                            {
                                                if (pe.CntctCode == contactListDt.Rows[l][0].ToString())
                                                {
                                                    rExistsEditLineNum = rExistsEditLineNum2; break;
                                                }
                                                rExistsEditLineNum2++;
                                            }
                                        }
                                        #region 设置有效性
                                        bp.ContactEmployees.SetCurrentLine(l);
                                        if (rExistsEditLineNum >= 0)
                                        {
                                            bp.ContactEmployees.Name = listOcpr[rExistsEditLineNum].Name;  //新联系人名称
                                        }
                                        if (contactListDt.Rows[l][14].ToString() != "N")
                                        {
                                            if (gCodeIsMe)
                                            {
                                                bp.ContactEmployees.Active = BoYesNoEnum.tYES;
                                            }
                                            else
                                            {
                                                bp.ContactEmployees.Active = BoYesNoEnum.tNO;
                                            }
                                        }
                                        bp.ContactEmployees.Add();
                                        #endregion
                                        rExistsEditLineNum = -1;
                                    }
                                    #endregion
                                }
                                #endregion

                                #region 添加联系人
                                int rLineNum = contactListDt.Rows.Count;
                                foreach (clientOCPR ocpr in client.ContactList)
                                {
                                    bp.ContactEmployees.SetCurrentLine(rLineNum);
                                    bp.ContactEmployees.Active = ocpr.Active == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                                    bp.ContactEmployees.Name = ocpr.Name.Trim();       //联系人名称
                                    switch (ocpr.Gender)                        //性别
                                    {
                                        case "E":
                                            bp.ContactEmployees.Gender = SAPbobsCOM.BoGenderTypes.gt_Undefined;
                                            break;
                                        case "M":
                                            bp.ContactEmployees.Gender = SAPbobsCOM.BoGenderTypes.gt_Male;
                                            break;
                                        case "F":
                                            bp.ContactEmployees.Gender = SAPbobsCOM.BoGenderTypes.gt_Female;
                                            break;
                                        default:
                                            bp.ContactEmployees.Gender = SAPbobsCOM.BoGenderTypes.gt_Undefined;
                                            break;
                                    }
                                    bp.ContactEmployees.Title = ocpr.Title;             //标题
                                    bp.ContactEmployees.Position = ocpr.Position;       //职位
                                    bp.ContactEmployees.Address = ocpr.Address;         //地址
                                    bp.ContactEmployees.Remarks1 = ocpr.Notes1;         //备注1
                                    bp.ContactEmployees.Remarks2 = ocpr.Notes2;         //备注2
                                    bp.ContactEmployees.Phone1 = ocpr.Tel1;             //电话1
                                    bp.ContactEmployees.Phone2 = ocpr.Tel2;             //电话2
                                    bp.ContactEmployees.MobilePhone = ocpr.Cellolar;    //移动电话
                                    bp.ContactEmployees.Fax = ocpr.Fax;                 //传真
                                    bp.ContactEmployees.E_Mail = ocpr.E_MailL;          //电子邮件
                                    if (!string.IsNullOrEmpty(ocpr.BirthDate) && ocpr.BirthDate != "0000/0/0 0:00:00") bp.ContactEmployees.DateOfBirth = Convert.ToDateTime(ocpr.BirthDate);  //生日
                                    bp.ContactEmployees.UserFields.Fields.Item("U_ACCT").Value = ocpr.U_ACCT;       //账号
                                    bp.ContactEmployees.UserFields.Fields.Item("U_BANK").Value = ocpr.U_BANK;       //开户行
                                    bp.ContactEmployees.Add();
                                    rLineNum++;
                                }
                                #endregion
                            }
                            else
                            {
                                errorMsg += string.Format("变更业务伙伴（{0}）未定义联系人", client.CardName);
                            }
                            #endregion
                            errorMsg += "联系人完成！";

                            #region 地址
                            if (client.AddrList.Count > 0)
                            {
                                string addrListSql = "SELECT LineNum, [Address], CardCode, AdresType, ZipCode, City, Country, [State], Building, U_Active FROM CRD1 WHERE CardCode=@CardCode order by LineNum ASC";
                                DataTable addrListDt = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, addrListSql,CommandType.Text,  new SqlParameter("@CardCode", client.CardCode));

                                #region 验证是否有相同名称的地址
                                DataTable rExistsAddrListDt = addrListDt;
                                if (rExistsAddrListDt.Rows.Count > 0)  //当地址大于零【不可操作的】
                                {
                                    List<clientCRD1> listCrd1 = new List<clientCRD1>();
                                    #region 循环找出重名的地址
                                    for (int h = 0; h < rExistsAddrListDt.Rows.Count; h++)
                                    {
                                        string rNewName = ""; int rIsExistsNum = 0;
                                        foreach (clientCRD1 pb in client.AddrList)   //如果地址重名【不可操作的】
                                        {
                                            if (pb.Address.ToLower().Trim() == rExistsAddrListDt.Rows[h][1].ToString().ToLower().Trim())
                                            {
                                                rIsExistsNum++; break;
                                            }
                                        }
                                        if (rIsExistsNum > 0)
                                        {    //有重名的地址标识
                                            int rIsExistsNum2 = 1, rIsExistsNum3 = 0, rIsExistsNum4 = 0, rIsExistsNum5 = 0;
                                            string rOldName = rExistsAddrListDt.Rows[h][1].ToString().Trim();
                                            while (rIsExistsNum2 > 0)   //循环计算出不重名的地址标识
                                            {
                                                #region 循环计算出不重名的名称
                                                rIsExistsNum3 = 0; rIsExistsNum4 = 0; rIsExistsNum5 = 0;
                                                rNewName = string.Format(rOldName + "{0}", rIsExistsNum);
                                                //查找当前计算出的地址标识是否重复【可操作】
                                                foreach (clientCRD1 pc in client.AddrList)
                                                {
                                                    if (pc.Address.ToLower().Trim() == rNewName.ToLower())
                                                    {
                                                        rIsExistsNum3++; break;
                                                    }
                                                }
                                                //查找当前计算出的地址标识是否重复【不可操作】
                                                if (rIsExistsNum3 == 0)
                                                {
                                                    for (int k = 0; k < rExistsAddrListDt.Rows.Count; k++)
                                                    {
                                                        if (rExistsAddrListDt.Rows[k][1].ToString() == rNewName)
                                                        {
                                                            rIsExistsNum4++; break;
                                                        }
                                                    }
                                                }
                                                if (rIsExistsNum3 == 0 && rIsExistsNum4 == 0)
                                                {
                                                    //查找当前计算出的地址标识是否重复【新生成的】
                                                    foreach (clientCRD1 pd in listCrd1)
                                                    {
                                                        if (pd.Address.ToLower().Trim() == rNewName.ToLower())
                                                        {
                                                            rIsExistsNum5++; break;
                                                        }
                                                    }
                                                }
                                                if (rIsExistsNum3 == 0 && rIsExistsNum4 == 0 && rIsExistsNum5 == 0)
                                                {
                                                    rIsExistsNum2 = 0;
                                                }
                                                else
                                                {
                                                    rIsExistsNum++;
                                                }
                                                #endregion
                                            }
                                            clientCRD1 crd2 = new clientCRD1();
                                            crd2.LineNum = rExistsAddrListDt.Rows[h][0].ToString();
                                            crd2.Address = rNewName;
                                            listCrd1.Add(crd2);
                                        }
                                    }
                                    #endregion

                                    #region 修改新生成的地址名称与禁用原来的地址
                                    int rExistsEditLineNum = -1;
                                    for (int l = 0; l < addrListDt.Rows.Count; l++)
                                    {
                                        if (listCrd1.Count > 0)
                                        {
                                            int rExistsEditLineNum2 = 0;
                                            foreach (clientCRD1 pe in listCrd1)
                                            {
                                                if (pe.LineNum == addrListDt.Rows[l][0].ToString())
                                                {
                                                    rExistsEditLineNum = rExistsEditLineNum2; break;
                                                }
                                                rExistsEditLineNum2++;
                                            }
                                        }
                                        #region 设置有效性
                                        bp.Addresses.SetCurrentLine(l);
                                        if (rExistsEditLineNum >= 0)
                                        {
                                            bp.Addresses.AddressName = listCrd1[rExistsEditLineNum].Address;  //新地址标识
                                        }
                                        if (addrListDt.Rows[l][9].ToString() != "N")
                                        {
                                            if (gCodeIsMe)
                                            {
                                                bp.Addresses.UserFields.Fields.Item("U_Active").Value = "Y";
                                            }
                                            else
                                            {
                                                bp.Addresses.UserFields.Fields.Item("U_Active").Value = "N";
                                            }
                                        }
                                        bp.Addresses.Add();
                                        #endregion
                                        rExistsEditLineNum = -1;
                                    }
                                    #endregion
                                }
                                #endregion

                                int rLineNum = addrListDt.Rows.Count;
                                foreach (clientCRD1 crd1 in client.AddrList)
                                {
                                    #region 新添加
                                    bp.Addresses.SetCurrentLine(rLineNum);
                                    bp.Addresses.UserFields.Fields.Item("U_Active").Value = crd1.Active == "N" ? "N" : "Y";  //是否可用
                                    switch (crd1.AdresType)       //地址类型
                                    {
                                        case "B":
                                            bp.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_BillTo;
                                            break;
                                        case "S":
                                            bp.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_ShipTo;
                                            break;
                                        default:
                                            bp.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_BillTo;
                                            break;
                                    }
                                    bp.Addresses.AddressName = crd1.Address;          //地址标识
                                    bp.Addresses.Country = crd1.CountryId;            //国家
                                    if (!string.IsNullOrEmpty(crd1.StateId)) bp.Addresses.State = crd1.StateId; //省
                                    bp.Addresses.City = crd1.City;              //城市
                                    bp.Addresses.BuildingFloorRoom = crd1.Building;     //详细地址
                                    bp.Addresses.ZipCode = crd1.ZipCode;     //邮编
                                    bp.Addresses.Add();
                                    rLineNum++;
                                    #endregion
                                }
                            }
                            else
                            {
                                errorMsg += string.Format("变更业务伙伴（{0}）未定义地址", client.CardName);
                            }
                            #endregion
                            errorMsg += "地址完成！";

                            #region 扩展信息
                            if (!string.IsNullOrEmpty(client.CustomFields))  // 自定义字段
                            {
                                string[] fieldsName = client.CustomFields.Replace(",", "，").Replace("≮0≯", ",").Split(',');
                                string[] fieldsValue = "".Split(',');
                                for (int i = 0; i < fieldsName.Length; i++)
                                {
                                    fieldsValue = fieldsName[i].Replace(":", "：").Replace("≮1≯", ":").Split(':');
                                    if (!string.IsNullOrEmpty(fieldsValue[0]) && !string.IsNullOrEmpty(fieldsValue[1]) &&IsExistCustomFields("OCRD", fieldsValue[0].ToString()))
                                    {
                                        bp.UserFields.Fields.Item(fieldsValue[0]).Value = fieldsValue[1];
                                    }
                                }
                            }
                            #endregion
                            errorMsg += "扩展信息完成！";

                            #region 属性
                            bp.set_Properties(1, client.QryGroup1 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性1
                            bp.set_Properties(2, client.QryGroup2 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性2
                            bp.set_Properties(3, client.QryGroup3 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性3
                            bp.set_Properties(4, client.QryGroup4 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性4
                            bp.set_Properties(5, client.QryGroup5 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性5
                            bp.set_Properties(6, client.QryGroup6 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性6
                            bp.set_Properties(7, client.QryGroup7 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性7
                            bp.set_Properties(8, client.QryGroup8 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性8
                            bp.set_Properties(9, client.QryGroup9 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性9
                            bp.set_Properties(10, client.QryGroup10 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性10
                            bp.set_Properties(11, client.QryGroup11 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性11
                            bp.set_Properties(12, client.QryGroup12 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性12
                            bp.set_Properties(13, client.QryGroup13 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性13
                            bp.set_Properties(14, client.QryGroup14 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性14
                            bp.set_Properties(15, client.QryGroup15 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性15
                            bp.set_Properties(16, client.QryGroup16 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性16
                            bp.set_Properties(17, client.QryGroup17 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性17
                            bp.set_Properties(18, client.QryGroup18 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性18
                            bp.set_Properties(19, client.QryGroup19 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性19
                            bp.set_Properties(20, client.QryGroup20 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性20
                            bp.set_Properties(21, client.QryGroup21 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性21
                            bp.set_Properties(22, client.QryGroup22 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性22
                            bp.set_Properties(23, client.QryGroup23 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性23
                            bp.set_Properties(24, client.QryGroup24 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性24
                            bp.set_Properties(25, client.QryGroup25 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性25
                            bp.set_Properties(26, client.QryGroup26 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性26
                            bp.set_Properties(27, client.QryGroup27 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性27
                            bp.set_Properties(28, client.QryGroup28 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性28
                            bp.set_Properties(29, client.QryGroup29 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性29
                            bp.set_Properties(30, client.QryGroup30 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性30
                            bp.set_Properties(31, client.QryGroup31 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性31
                            bp.set_Properties(32, client.QryGroup32 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性32
                            bp.set_Properties(33, client.QryGroup33 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性33
                            bp.set_Properties(34, client.QryGroup34 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性34
                            bp.set_Properties(35, client.QryGroup35 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性35
                            bp.set_Properties(36, client.QryGroup36 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性36
                            bp.set_Properties(37, client.QryGroup37 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性37
                            bp.set_Properties(38, client.QryGroup38 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性38
                            bp.set_Properties(39, client.QryGroup39 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性39
                            bp.set_Properties(40, client.QryGroup40 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性40
                            bp.set_Properties(41, client.QryGroup41 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性41
                            bp.set_Properties(42, client.QryGroup42 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性42
                            bp.set_Properties(43, client.QryGroup43 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性43
                            bp.set_Properties(44, client.QryGroup44 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性44
                            bp.set_Properties(45, client.QryGroup45 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性45
                            bp.set_Properties(46, client.QryGroup46 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性46
                            bp.set_Properties(47, client.QryGroup47 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性47
                            bp.set_Properties(48, client.QryGroup48 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性48
                            bp.set_Properties(49, client.QryGroup49 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性49
                            bp.set_Properties(50, client.QryGroup50 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性50
                            bp.set_Properties(51, client.QryGroup51 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性51
                            bp.set_Properties(52, client.QryGroup52 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性52
                            bp.set_Properties(53, client.QryGroup53 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性53
                            bp.set_Properties(54, client.QryGroup54 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性54
                            bp.set_Properties(55, client.QryGroup55 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性55
                            bp.set_Properties(56, client.QryGroup56 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性56
                            bp.set_Properties(57, client.QryGroup57 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性57
                            bp.set_Properties(58, client.QryGroup58 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性58
                            bp.set_Properties(59, client.QryGroup59 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性59
                            bp.set_Properties(60, client.QryGroup60 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性60
                            bp.set_Properties(61, client.QryGroup61 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性61
                            bp.set_Properties(62, client.QryGroup62 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性62
                            bp.set_Properties(63, client.QryGroup63 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性63
                            bp.set_Properties(64, client.QryGroup64 == "1" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO);  //属性64
                            #endregion
                            errorMsg += "属性完成！";

                            res = bp.Update();
                            if (res == 0)
                            {
                                errorMsg += "调SAP接口，变更业务伙伴【" + jobID + "】操作成功";

                                #region 把接口返回值写入wfa_job表
                                int saveKeyValue = GetSboReturn(gCardCode, jobID.ToString());
                                errorMsg += "保存接口的唯一编码:" + (saveKeyValue > 0 ? "成功!" : "失败!");
                                #endregion
                            }
                            else
                            {
                                company.GetLastError(out eCode, out eMesg);
                                errorMsg += "调SAP接口，变更业务伙伴信息时异常！流程任务编号：【" + jobID + "】错误代码：" + eCode + "错误信息：" + eMesg;
                            }
                            #endregion
                        }
                        else
                        {
                            errorMsg += "添加业务伙伴，相关参数错误！";
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMsg += "添加业务伙伴【" + jobID + "】时调接口错误！错误代号:" + eCode + " 错误信息：" + ex.Message;
                    }
                }
            }

        }
        #region [Sqlserver]判断自定义字段是否存在
        /// <summary>
        /// [Sqlserver]判断自定义字段是否存在
        /// </summary>
        public bool IsExistCustomFields(string tablename, string filename)
        {
            bool result = false;
            string strSql = string.Format("SELECT COUNT(*) FROM syscolumns WHERE id=object_id('{0}') AND name='{1}'", tablename, filename);
            object obj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, strSql, CommandType.Text, null);
            if (obj.ToString() == "0" || obj.ToString() == null) { result = false; }
            else { result = true; }
            return result;
        }
        #endregion
        #region 根据用户类型生成相应的业务伙伴代码
        /// <summary>
        /// 根据用户类型生成相应的业务伙伴代码
        /// </summary>
        public string GetCardCodeByCardType(string CardType)
        {
            try
            {
                string max = "";
                StringBuilder strSql = new StringBuilder();
                if (CardType.ToUpper() == "L" || CardType.ToUpper() == "C")
                {
                    string sql = "SELECT TOP 1 CardCode FROM OCRD WHERE CardType='C' OR CardType='L' ORDER BY CAST( SUBSTRING(CardCode,2,LEN(CardCode)) as int) DESC";
                    object strObj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, sql, CommandType.Text, null);
                    max = strObj == null ? "C00000" : strObj.ToString();
                    //strSql.AppendFormat("SELECT CardCode FROM {0}.crm_OCRD WHERE CardType='C' OR CardType='L' ORDER BY SUBSTRING(CardCode,2,LENGTH(CardCode)) DESC LIMIT 1 ", Sql.BOneDatabaseName);
                    //max = Sql.Action.ExecuteScalar(Sql.GB2312ConnectionString, CommandType.Text, strSql.ToString()).ToString();
                }
                else if (CardType.ToUpper() == "S")
                {
                    string sql = "SELECT TOP 1 CardCode FROM OCRD WHERE CardType='S' ORDER BY CAST( SUBSTRING(CardCode,2,LEN(CardCode)) as int) DESC";
                    object strObj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, sql, CommandType.Text, null);
                    max = strObj == null ? "V00000" : strObj.ToString();
                    //strSql.AppendFormat("SELECT CardCode FROM {0}.crm_OCRD WHERE CardType='S' ORDER BY SUBSTRING(CardCode,2,LENGTH(CardCode)) DESC LIMIT 1 ", Sql.BOneDatabaseName);
                    //max = Sql.Action.ExecuteScalar(Sql.GB2312ConnectionString, CommandType.Text, strSql.ToString()).ToString();
                }
                else
                {
                    string sql = "SELECT TOP 1 CardCode FROM OCRD WHERE CardType=@CardType ORDER BY CAST( SUBSTRING(CardCode,2,LEN(CardCode)) as int) DESC";
                    max = UnitWork.ExecuteScalar(ContextType.SapDbContextType, sql, CommandType.Text, new SqlParameter("@CardType", CardType)).ToString();
                    //strSql.AppendFormat("SELECT CardCode FROM {0}.crm_OCRD WHERE CardType=?CardType ORDER BY SUBSTRING(CardCode,2,LENGTH(CardCode)) DESC LIMIT 1 ", Sql.BOneDatabaseName);
                    //IDataParameter[] strPara = { 
                    //    Sql.Action.GetParameter("?CardType", CardType)
                    //};
                    //max = Sql.Action.ExecuteScalar(Sql.GB2312ConnectionString, CommandType.Text, strSql.ToString(), strPara).ToString();
                }
                string zyt = max.Substring(0, 1).ToUpper();
                string tmpNum = max.Substring(1, max.Length - 1);
                string Num = (Convert.ToInt32(tmpNum) + 1).ToString();
                string mid = string.Empty;
                for (int i = 0; i < tmpNum.Length - Num.Length; i++)
                {
                    mid += "0";
                }
                return zyt + mid + Num;
            }
            catch { return "0"; }
        }
        #endregion
        #region 给单据添加返回值
        /// <summary>
        /// 给单据添加返回值
        /// </summary>
        /// <param name="docNum">单号</param>
        /// <param name="jobID">流程编号</param>
        public  int GetSboReturn(string docNum, string jobID)
        {
            string strSqlJob = string.Format("UPDATE {0}.wfa_job SET sbo_itf_return={1} WHERE job_id={2}", "nsap_base", docNum, jobID);
        
            return UnitWork.ExecuteSql(strSqlJob,ContextType.NsapBaseDbContext);
        }
        #endregion

    }
}
