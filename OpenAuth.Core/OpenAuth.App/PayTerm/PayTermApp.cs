extern alias MySqlConnectorAlias;
using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Infrastructure;
using DotNetCore.CAP;
using OpenAuth.App.Response;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.App.PayTerm.PayTermSetHelp;
using NSAP.Entity.Sales;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Order;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Domain.Serve;
using OpenAuth.App.DDVoice;
using OpenAuth.Repository;
using OpenAuth.App.CommonHelp;
using Infrastructure.Extensions;
using OpenAuth.Repository.Extensions;
using OpenAuth.App.SaleBusiness.Request;
using OpenAuth.App.SaleBusiness.Common;
using OpenAuth.App.Material;
using OpenAuth.Repository.Domain.View;

namespace OpenAuth.App.PayTerm
{
    public class PayTermApp : OnlyUnitWorkBaeApp
    {
        private IUnitWork _UnitWork;
        private IAuth _auth;
        private ICapPublisher _capBus;
        private ServiceBaseApp _serviceBaseApp;
        private ServiceSaleOrderApp _serviceSaleOrderApp;
        private UserDepartMsgHelp _userDepartMsgHelp;
        private SaleBusinessMethodHelp _saleBusinessMethodHelp;
        private ManageAccBindApp _manageAccBindApp;
        private readonly DDVoiceApp _dDVoice;
        private List<string> RecePayTypes = new List<string>() { "预付/货前款", "货到款", "验收款", "质保款" };

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public PayTermApp(IUnitWork unitWork, IAuth auth, ICapPublisher capBus, UserDepartMsgHelp userDepartMsgHelp, SaleBusinessMethodHelp saleBusinessMethodHelp, ServiceBaseApp serviceBaseApp, ServiceSaleOrderApp serviceSaleOrderApp, DDVoiceApp dDVoiceApp, ManageAccBindApp manageAccBindApp) : base(unitWork, auth)
        {
            _UnitWork = unitWork;
            _auth = auth;
            _serviceBaseApp = serviceBaseApp;
            _serviceSaleOrderApp = serviceSaleOrderApp;
            _capBus = capBus;
            _userDepartMsgHelp = userDepartMsgHelp;
            _dDVoice = dDVoiceApp;
            _saleBusinessMethodHelp = saleBusinessMethodHelp;
            _manageAccBindApp = manageAccBindApp;
        }

        #region 付款条件设置
        /// <summary>
        /// 付款条件设置信息
        /// </summary>
        /// <returns>返回付款条件基本设置信息</returns>
        public async Task<TableData> GetPayTermSetList()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();
            var objs = await UnitWork.Find<PayTermSet>(null).Include(r => r.PayPhases).ToListAsync();
            var payTermSets = objs.Select(r => new PayTermHelp
            {
                Id = r.Id,
                ModuleTypeId = r.ModuleTypeId,
                ModuleName = r.ModuleName,
                DateNumber = Convert.ToDecimal(r.DateNumber.ToString().Split('.')[1]) == 0 ? Convert.ToInt32(r.DateNumber) : Math.Round(Convert.ToDecimal(r.DateNumber), 2),
                DateUnit = r.DateUnit,
                DateUnitName = (Convert.ToDecimal(r.DateNumber.ToString().Split('.')[1]) == 0 ? (Convert.ToInt32(r.DateNumber)).ToString() : (Math.Round(Convert.ToDecimal(r.DateNumber), 2)).ToString()) + CommonMethodHelp.GetDateTimeUnit(r.DateUnit),
                IsDefault = r.IsDefault,
                CreateUserId = r.CreateUserId,
                CreateUser = r.CreateUser,
                CreateDate = r.CreateDate,
                UpdateUserId = r.UpdateUserId,
                UpdateUser = r.UpdateUser,
                UpdateDate = r.UpdateDate,
                PayPhaseName = r.PayPhases != null && r.PayPhases.Count() > 0 ? GetPayPhaseName(r.PayPhases.OrderBy(r => r.PayPhaseType).ToList()) : "",
                PayPhases = r.PayPhases.OrderBy(r => r.PayPhaseType).ToList()
            }).ToList();

            result.Count = payTermSets.Count();
            result.Data = payTermSets.OrderByDescending(r => r.CreateDate);
            return result;
        }

        /// <summary>
        /// 获取可用阶段名称
        /// </summary>
        /// <param name="payPhases">可用阶段实体集合</param>
        /// <returns>返回可用阶段名称</returns>
        public string GetPayPhaseName(List<PayPhase> payPhases)
        {
            StringBuilder payPhaseName = new StringBuilder();
            foreach (PayPhase item in payPhases)
            {
                payPhaseName.Append(item.PayPhaseName + "、");
            }

            return (payPhaseName.ToString()).Substring(0, payPhaseName.Length - 1);
        }

        /// <summary>
        /// 付款条件详情
        /// </summary>
        /// <param name="payTermSetId">付款条件Id</param>
        /// <returns>返回付款条件设置详情信息</returns>
        public async Task<TableData> GetPayTermSetDetail(string payTermSetId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();
            var objs = await UnitWork.Find<PayTermSet>(r => r.Id == payTermSetId).Include(r => r.PayPhases).ToListAsync();
            result.Data = objs;
            return result;
        }

        /// <summary>
        /// 新增付款条件设置
        /// </summary>
        /// <param name="obj">付款条件实体数据</param>
        /// <returns>成功返回操作成功/returns>
        public async Task<string> Add(PayTermSet obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;

            //判断当前付款条件是否已经存在
            if (!GetPayTermSetIsRepeat(obj))
            {
                throw new Exception("当前付款条件已经存在，不允许重复添加");
            }

            //判断是否已经存在各阶段节点计算方法
            if (obj.ModuleTypeId == 1 || obj.ModuleName == "各阶段节点计算方法")
            {
                var payTermSets = await UnitWork.Find<PayTermSet>(r => r.ModuleTypeId == 1 || r.ModuleName == "各阶段节点计算方法").ToListAsync();
                if (payTermSets.Count() >= 1)
                {
                    throw new Exception("各阶段节点计算方法模块已经存在，不允许重复添加");
                }
            }

            var dbContext = UnitWork.GetDbContext<PayTermSet>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    obj.Id = Guid.NewGuid().ToString();
                    obj.IsDefault = false;
                    obj.CreateDate = DateTime.Now;
                    obj.CreateUserId = loginUser.Id;
                    obj.CreateUser = loginUser.Name;
                    obj.UpdateDate = null;

                    //保存可用阶段
                    if (obj.PayPhases.Count() > 0)
                    {
                        List<PayPhase> payPhaseList = new List<PayPhase>();
                        foreach (PayPhase item in obj.PayPhases)
                        {
                            item.Id = Guid.NewGuid().ToString();
                            item.PayTermSetId = obj.Id;
                            payPhaseList.Add(item);
                        }

                        if (payPhaseList != null && payPhaseList.Count > 0)
                        {
                            await UnitWork.BatchAddAsync<PayPhase>(payPhaseList.ToArray());
                        }
                    }

                    obj = await UnitWork.AddAsync<PayTermSet, int>(obj);
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("创建付款条件失败,请重试");
                }
            }

            return "操作成功";
        }

        /// <summary>
        /// 修改付款条件设置
        /// </summary>
        /// <param name="obj">付款条件实体数据</param>
        /// <returns>成功返回操作成功</returns>
        public async Task<string> UpDate(PayTermSet obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;

            //判断当前付款条件是否已经存在
            if (!GetPayTermSetIsRepeat(obj))
            {
                return "当前付款条件已经存在，不允许重复添加";
            }

            var dbContext = UnitWork.GetDbContext<PayTermSet>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    #region 删除
                    var payPhases = await UnitWork.Find<PayPhase>(r => r.PayTermSetId == obj.Id).ToListAsync();
                    if (payPhases != null && payPhases.Count() > 0)
                    {
                        foreach (PayPhase item in payPhases)
                        {
                            await UnitWork.DeleteAsync<PayPhase>(r => r.Id == item.Id);
                        }
                    }

                    await UnitWork.SaveAsync();
                    #endregion

                    #region 新增
                    //保存可用阶段
                    if (obj.PayPhases.Count() > 0)
                    {
                        List<PayPhase> payPhaseList = new List<PayPhase>();
                        foreach (PayPhase item in obj.PayPhases)
                        {
                            item.Id = Guid.NewGuid().ToString();
                            item.PayTermSetId = obj.Id;
                            payPhaseList.Add(item);
                        }

                        if (payPhaseList != null && payPhaseList.Count > 0)
                        {
                            await UnitWork.BatchAddAsync<PayPhase>(payPhaseList.ToArray());
                        }
                    }

                    //清空旧数据
                    obj.PayPhases.Clear();
                    await UnitWork.SaveAsync();
                    #endregion

                    //修改主表数据
                    await UnitWork.UpdateAsync<PayTermSet>(u => u.Id == obj.Id, u => new PayTermSet()
                    {
                        UpdateUser = loginUser.Name,
                        UpdateUserId = loginUser.Id,
                        UpdateDate = DateTime.Now,
                        ModuleTypeId = obj.ModuleTypeId,
                        ModuleName = obj.ModuleName,
                        DateNumber = obj.DateNumber,
                        DateUnit = obj.DateUnit,
                        IsDefault = obj.IsDefault,
                        CreateUserId = obj.CreateUserId,
                        CreateUser = obj.CreateUser,
                        CreateDate = obj.CreateDate
                    });

                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("修改付款条件失败,请重试");
                }
            }

            return "操作成功";
        }

        /// <summary>
        /// 删除付款条件设置
        /// </summary>
        /// <param name="payTermSetId">付款条件Id</param>
        /// <returns>成功返回操作成功</returns>
        public async Task<string> Delete(string payTermSetId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var payTermSets = await UnitWork.Find<PayTermSet>(r => r.Id == payTermSetId).ToListAsync();
            if (payTermSets != null && payTermSets.Count() > 0)
            {
                var payPhases = await UnitWork.Find<PayPhase>(r => r.PayTermSetId == payTermSetId).ToListAsync();
                if (payPhases != null && payPhases.Count() > 0)
                {
                    await UnitWork.BatchDeleteAsync<PayPhase>(payPhases.ToArray());
                }

                await UnitWork.DeleteAsync<PayTermSet>(r => r.Id == payTermSetId);
                await UnitWork.SaveAsync();
                return "操作成功";
            }
            else
            {
                throw new Exception("删除失败，该付款条件不存在。");
            }
        }

        /// <summary>
        /// 判定付款条件是否重复
        /// </summary>
        /// <param name="obj">付款条件实体数据</param>
        /// <returns>付款条件重复，返回false，不重复返回true</returns>
        public bool GetPayTermSetIsRepeat(PayTermSet obj)
        {
            bool isRepeat = true;
            if (obj.PayPhases != null && obj.PayPhases.Count() > 0)
            {
                var objs = UnitWork.Find<PayTermSet>((r => r.DateNumber == obj.DateNumber && r.ModuleTypeId == obj.ModuleTypeId && r.ModuleName == r.ModuleName && r.DateUnit == obj.DateUnit && r.IsDefault == obj.IsDefault)).Include(r => r.PayPhases).ToList();
                if (objs != null && objs.Count() > 0)
                {
                    string payname = GetPayPhaseName(obj.PayPhases.OrderBy(r => r.PayPhaseType).ToList());
                    string dbpayname = GetPayPhaseName((objs.FirstOrDefault()).PayPhases.OrderBy(r => r.PayPhaseType).ToList());
                    if (payname == dbpayname)
                    {
                        isRepeat = false;
                    }
                }
            }
            else
            {
                var objs = UnitWork.Find<PayTermSet>((r => r.DateNumber == obj.DateNumber && r.ModuleTypeId == obj.ModuleTypeId && r.ModuleName == r.ModuleName && r.DateUnit == obj.DateUnit && r.IsDefault == obj.IsDefault)).Include(r => r.PayPhases).ToList();
                if (objs != null && objs.Count() > 0)
                {
                    isRepeat = false;
                }
            }

            return isRepeat;
        }

        /// <summary>
        /// 付款条件保存4.0
        /// </summary>
        /// <param name="obj">付款条件保存实体</param>
        /// <returns>返回操作成功</returns>
        public async Task<TableData> PayTermSave(PayTermSave obj)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var dbContext = UnitWork.GetDbContext<PayTermSave>();

            //拼接付款条款组名称
            string groupName = GetGroupName(obj);

            //判定付款条件是否已经存在
            var crmOctgList = await UnitWork.Find<crm_octg>(r => r.PymntGroup == groupName).ToListAsync();
            var octgLIst = await UnitWork.Find<OCTG>(r => r.PymntGroup == groupName).ToListAsync();
            var paytermsaves = await UnitWork.Find<PayTermSave>(r => r.GroupNum == groupName).ToListAsync();
            var payTermSets = await UnitWork.Find<PayTermSet>(r => r.ModuleName == "各阶段节点计算方法").ToListAsync();
            int groupNum = (await UnitWork.Find<crm_octg>(null).ToListAsync()).Max(r => Convert.ToInt32(r.GroupNum)) + 1;
            if (crmOctgList != null && crmOctgList.Count() > 0)
            {
                result.Code = 201;
                result.Data = new
                {
                    Id = crmOctgList.FirstOrDefault().GroupNum,
                    GroupNum = groupName,
                    Message = "3.0中已经存在该配置的付款条件"
                };

                return result;
            }

            if (octgLIst != null && octgLIst.Count() > 0)
            {
                result.Code = 201;
                result.Data = new
                {
                    Id = octgLIst.FirstOrDefault().GroupNum,
                    GroupNum = groupName,
                    Message = "已经存在该配置的付款条件"
                };

                return result;
            }

            if (payTermSets == null || payTermSets.Count() == 0)
            {
                result.Code = 500;
                result.Message = "付款条件基础配置中没有配置各阶段节点计算方法，请配置之后新增付款条件";
                return result;
            }

            //3.0付款条件配置实体
            saleCrmOctgCfg scoc = new saleCrmOctgCfg();
            scoc.GroupNum = groupNum.ToString();
            scoc.sbo_id = "1";
            scoc.PymntGroup = groupName;
            scoc.PrepaDay = string.IsNullOrEmpty(obj.PrepaDay.ToString()) ? "0" : obj.PrepaDay.ToString();
            scoc.PrepaPro = string.IsNullOrEmpty(obj.PrepaPro.ToString()) ? "0" : obj.PrepaPro.ToString();
            scoc.PayBefShip = string.IsNullOrEmpty(obj.BefShipPro.ToString()) ? "0" : obj.BefShipPro.ToString();
            scoc.GoodsToDay = string.IsNullOrEmpty(obj.GoodsToDay.ToString()) ? "0" : obj.GoodsToDay.ToString();
            scoc.GoodsToPro = string.IsNullOrEmpty(obj.GoodsToPro.ToString()) ? "0" : obj.GoodsToPro.ToString();

            //3.0付款条件实体
            saleCrmOctg modelCrmOctg = new saleCrmOctg();
            modelCrmOctg.PymntGroup = groupName;
            modelCrmOctg.GroupNum = "0";
            scoc.ModelCrmOctg = modelCrmOctg;

            //付款条件同步到SAP,3.0接口
            _capBus.Publish("Serve.BOneOCTG.Create", scoc);
            if (paytermsaves != null && paytermsaves.Count() > 0)
            {
                //如果存在则修改
                await UnitWork.UpdateAsync<PayTermSave>(r => r.GroupNum == (paytermsaves.FirstOrDefault()).GroupNum, r => new PayTermSave()
                {
                    SaleGoodsToDay = Convert.ToInt32((payTermSets.FirstOrDefault()).DateNumber),
                    SaleGoodsToUnit = (payTermSets.FirstOrDefault()).DateUnit,
                    CreateUserId = loginUser.Id,
                    CreateUserName = loginUser.Name,
                    CreateTime = DateTime.Now,
                    UpdateUserId = "",
                    GroupNum = groupName
                });

                await UnitWork.SaveAsync();
            }
            else
            {
                using (var transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        obj.Id = Guid.NewGuid().ToString();
                        obj.SaleGoodsToDay = Convert.ToInt32((payTermSets.FirstOrDefault()).DateNumber);
                        obj.SaleGoodsToUnit = (payTermSets.FirstOrDefault()).DateUnit;
                        obj.CreateUserId = loginUser.Id;
                        obj.CreateUserName = loginUser.Name;
                        obj.CreateTime = DateTime.Now;
                        obj.UpdateUserId = "";
                        obj.GroupNum = groupName;
                        obj = await UnitWork.AddAsync<PayTermSave, int>(obj);
                        await UnitWork.SaveAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception("保存付款条件失败,请重试");
                    }
                }
            }

            result.Data = new
            {
                GroupNum = groupName,
            };

            result.Message = "操作成功";
            return result;
        }

        /// <summary>
        /// 获取付款条件详细信息
        /// </summary>
        /// <param name="GroupNum">付款条件名称</param>
        /// <returns>返回付款条件信息</returns>
        public async Task<TableData> GetPayTermSaveDetail(string GroupNum)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //查询付款条件
            var payTermSave = await UnitWork.Find<PayTermSave>(r => r.GroupNum == GroupNum).FirstOrDefaultAsync();
            if (payTermSave != null)
            {
                result.Data = payTermSave;
                result.Message = "获取成功";
            }

            return result;
        }

        /// <summary>
        /// 拼接付款条件名称
        /// </summary>
        /// <param name="obj">付款条件保存实体</param>
        /// <returns>返回付款条件名称</returns>
        public string GetGroupName(PayTermSave obj)
        {
            if (obj != null)
            {
                string PrepaName = obj.PrepaDay == 0 && obj.PrepaPro == 0 ? "" : (obj.PrepaDay == 0 && obj.PrepaPro != 0 ? "预付" + obj.PrepaPro + "%" : (obj.PrepaDay != 0 && obj.PrepaPro == 0 ? obj.PrepaDay + CommonMethodHelp.GetDateTimeUnit(obj.PrepaUnit) + "内预付0%" : obj.PrepaDay + CommonMethodHelp.GetDateTimeUnit(obj.PrepaUnit) + "内预付" + obj.PrepaPro + "%"));
                string BefShiName = (obj.BefShipDay == 0 && obj.BefShipPro == 0 ? "" : (obj.BefShipDay == 0 && obj.BefShipPro != 0 ? "，货前付" + obj.BefShipPro + "%" : (obj.BefShipDay != 0 && obj.BefShipPro == 0 ? "，货前" + obj.BefShipDay + CommonMethodHelp.GetDateTimeUnit(obj.BefShipUnit) + "内付0%" : "，货前" + obj.BefShipDay + CommonMethodHelp.GetDateTimeUnit(obj.BefShipUnit) + "内付" + obj.BefShipPro + "%")));
                string GoodsToName = (obj.GoodsToDay == 0 && obj.GoodsToPro == 0 ? "" : (obj.GoodsToDay == 0 && obj.GoodsToPro != 0 ? "，货到付" + obj.GoodsToPro + "%" : (obj.GoodsToDay != 0 && obj.GoodsToPro == 0 ? "，货到" + obj.GoodsToDay + CommonMethodHelp.GetDateTimeUnit(obj.GoodsToUnit) + "内付0%" : "，货到" + obj.GoodsToDay + CommonMethodHelp.GetDateTimeUnit(obj.GoodsToUnit) + "内付" + obj.GoodsToPro + "%")));
                string AcceptancePayName = GetAcceptancePayName(obj);
                string QualityAssuranceName = GetQualityAssuranceName(obj);
                string groupName = "合同后" + PrepaName + BefShiName + GoodsToName + AcceptancePayName + QualityAssuranceName;
                return groupName;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 拼接验收期条件名称
        /// </summary>
        /// <param name="obj">付款条件保存实体</param>
        /// <returns>返回验收期付款条件名称</returns>
        public string GetAcceptancePayName(PayTermSave obj)
        {
            string AcceptancePayName = "";
            if (obj.AcceptancePayDay == 0 && obj.AcceptancePayLimit == 0 && obj.AcceptancePayPro == 0)
            {
                AcceptancePayName = "";
            }
            else if (obj.AcceptancePayDay == 0 && obj.AcceptancePayLimit == 0 && obj.AcceptancePayPro != 0)
            {
                AcceptancePayName = "，验收期后付" + obj.AcceptancePayPro + "%";
            }
            else if (obj.AcceptancePayDay == 0 && obj.AcceptancePayLimit != 0 && obj.AcceptancePayPro == 0)
            {
                AcceptancePayName = "，" + obj.AcceptancePayLimit + CommonMethodHelp.GetDateTimeUnit(obj.AcceptancePayLimitUnit) + "验收期后付0%";
            }
            else if (obj.AcceptancePayDay != 0 && obj.AcceptancePayLimit == 0 && obj.AcceptancePayPro == 0)
            {
                AcceptancePayName = "，验收期后" + obj.AcceptancePayDay + CommonMethodHelp.GetDateTimeUnit(obj.AcceptancePayDayUnit) + "内付0%";
            }
            else if (obj.AcceptancePayDay == 0 && obj.AcceptancePayLimit != 0 && obj.AcceptancePayPro != 0)
            {
                AcceptancePayName = "，" + obj.AcceptancePayLimit + CommonMethodHelp.GetDateTimeUnit(obj.AcceptancePayLimitUnit) + "验收期后付" + obj.AcceptancePayPro + "%";
            }
            else if (obj.AcceptancePayDay != 0 && obj.AcceptancePayLimit != 0 && obj.AcceptancePayPro == 0)
            {
                AcceptancePayName = "，" + obj.AcceptancePayLimit + CommonMethodHelp.GetDateTimeUnit(obj.AcceptancePayLimitUnit) + "验收期后" + obj.AcceptancePayDay + CommonMethodHelp.GetDateTimeUnit(obj.AcceptancePayDayUnit) + "内付0%";
            }
            else if (obj.AcceptancePayDay != 0 && obj.AcceptancePayLimit == 0 && obj.AcceptancePayPro != 0)
            {
                AcceptancePayName = "，验收期后" + obj.AcceptancePayDay + CommonMethodHelp.GetDateTimeUnit(obj.AcceptancePayDayUnit) + "内付" + obj.AcceptancePayPro + "%";
            }
            else
            {
                AcceptancePayName = "，" + obj.AcceptancePayLimit + CommonMethodHelp.GetDateTimeUnit(obj.AcceptancePayLimitUnit) + "验收期后" + obj.AcceptancePayDay + CommonMethodHelp.GetDateTimeUnit(obj.AcceptancePayDayUnit) + "内付" + obj.AcceptancePayPro + "%";
            }

            return AcceptancePayName;
        }

        /// <summary>
        /// 拼接质保款付款条件名称
        /// </summary>
        /// <param name="obj">付款条件保存实体</param>
        /// <returns>返回质保款付款条件名称</returns>
        public string GetQualityAssuranceName(PayTermSave obj)
        {
            string QualityAssuranceName = "";
            if (obj.QualityAssuranceDay == 0 && obj.QualityAssuranceLimit == 0 && obj.QualityAssurancePro == 0)
            {
                QualityAssuranceName = "";
            }
            else if (obj.QualityAssuranceDay == 0 && obj.QualityAssuranceLimit == 0 && obj.QualityAssurancePro != 0)
            {
                QualityAssuranceName = "，质保期后付" + obj.QualityAssurancePro + "%";
            }
            else if (obj.QualityAssuranceDay == 0 && obj.QualityAssuranceLimit != 0 && obj.QualityAssurancePro == 0)
            {
                QualityAssuranceName = "，" + obj.QualityAssuranceLimit + CommonMethodHelp.GetDateTimeUnit(obj.QualityAssuranceLimitUnit) + "质保期后付0%";
            }
            else if (obj.QualityAssuranceDay != 0 && obj.QualityAssuranceLimit == 0 && obj.QualityAssurancePro == 0)
            {
                QualityAssuranceName = "，质保期后" + obj.QualityAssuranceDay + CommonMethodHelp.GetDateTimeUnit(obj.QualityAssuranceDayUnit) + "内付0%";
            }
            else if (obj.QualityAssuranceDay == 0 && obj.QualityAssuranceLimit != 0 && obj.QualityAssurancePro != 0)
            {
                QualityAssuranceName = "，" + obj.QualityAssuranceLimit + CommonMethodHelp.GetDateTimeUnit(obj.QualityAssuranceLimitUnit) + "质保期后付" + obj.QualityAssurancePro + "%";
            }
            else if (obj.QualityAssuranceDay != 0 && obj.QualityAssuranceLimit == 0 && obj.QualityAssurancePro != 0)
            {
                QualityAssuranceName = "，质保期后" + obj.QualityAssuranceDay + CommonMethodHelp.GetDateTimeUnit(obj.QualityAssuranceDayUnit) + "内付" + obj.QualityAssurancePro + "%";
            }
            else if (obj.QualityAssuranceDay != 0 && obj.QualityAssuranceLimit != 0 && obj.QualityAssurancePro == 0)
            {
                QualityAssuranceName = "，" + obj.QualityAssuranceLimit + CommonMethodHelp.GetDateTimeUnit(obj.QualityAssuranceLimitUnit) + "质保期后" + obj.QualityAssuranceDay + CommonMethodHelp.GetDateTimeUnit(obj.QualityAssuranceDayUnit) + "内付0%";
            }
            else
            {
                QualityAssuranceName = "，" + obj.QualityAssuranceLimit + CommonMethodHelp.GetDateTimeUnit(obj.QualityAssuranceLimitUnit) + "质保期后" + obj.QualityAssuranceDay + CommonMethodHelp.GetDateTimeUnit(obj.QualityAssuranceDayUnit) + "内付" + obj.QualityAssurancePro + "%";
            }

            return QualityAssuranceName;
        }

        /// <summary>
        /// 获取付款条件可选时间信息
        /// </summary>
        /// <returns>返回设置付款条件各阶段可选时间集合信息</returns>
        public async Task<TableData> GetPayTermSetMsg()
        {
            var result = new TableData();
            var payTermSets = await UnitWork.Find<PayTermSet>(r => r.ModuleTypeId == 2 || r.ModuleTypeId == 3 || r.ModuleTypeId == 4).Include(r => r.PayPhases).ToListAsync();

            //预付款可选时间集合
            var prepPaList = payTermSets.Where(r => r.PayPhases.Any(x => x.PayPhaseType == 1))
                                        .Select(r => new PayPhaseHelp
                                        {
                                            IsDefault = r.IsDefault,
                                            Number = Convert.ToDecimal(r.DateNumber.ToString().Split('.')[1]) == 0 ? Convert.ToInt32(r.DateNumber) : Math.Round(Convert.ToDecimal(r.DateNumber), 2),
                                            Unit = r.DateUnit,
                                            Name = (Convert.ToDecimal(r.DateNumber.ToString().Split('.')[1]) == 0 ? Convert.ToInt32(r.DateNumber) : Math.Round(Convert.ToDecimal(r.DateNumber), 2)).ToString() + CommonMethodHelp.GetDateTimeUnit(r.DateUnit)
                                        }).ToList();

            //货前款可选时间集合
            var befShipList = payTermSets.Where(r => r.PayPhases.Any(x => x.PayPhaseType == 2))
                                         .Select(r => new PayPhaseHelp
                                         {
                                             IsDefault = r.IsDefault,
                                             Number = Convert.ToDecimal(r.DateNumber.ToString().Split('.')[1]) == 0 ? Convert.ToInt32(r.DateNumber) : Math.Round(Convert.ToDecimal(r.DateNumber), 2),
                                             Unit = r.DateUnit,
                                             Name = (Convert.ToDecimal(r.DateNumber.ToString().Split('.')[1]) == 0 ? Convert.ToInt32(r.DateNumber) : Math.Round(Convert.ToDecimal(r.DateNumber), 2)).ToString() + CommonMethodHelp.GetDateTimeUnit(r.DateUnit)
                                         }).ToList();

            //货到款可选时间集合
            var goodsToList = payTermSets.Where(r => r.PayPhases.Any(x => x.PayPhaseType == 3))
                                         .Select(r => new PayPhaseHelp
                                         {
                                             IsDefault = r.IsDefault,
                                             Number = Convert.ToDecimal(r.DateNumber.ToString().Split('.')[1]) == 0 ? Convert.ToInt32(r.DateNumber) : Math.Round(Convert.ToDecimal(r.DateNumber), 2),
                                             Unit = r.DateUnit,
                                             Name = (Convert.ToDecimal(r.DateNumber.ToString().Split('.')[1]) == 0 ? Convert.ToInt32(r.DateNumber) : Math.Round(Convert.ToDecimal(r.DateNumber), 2)).ToString() + CommonMethodHelp.GetDateTimeUnit(r.DateUnit)
                                         }).ToList();

            //验收款可选时间集合
            var acceptancePayList = payTermSets.Where(r => r.PayPhases.Any(x => x.PayPhaseType == 4))
                                               .Select(r => new PayPhaseHelp
                                               {
                                                   IsDefault = r.IsDefault,
                                                   Number = Convert.ToDecimal(r.DateNumber.ToString().Split('.')[1]) == 0 ? Convert.ToInt32(r.DateNumber) : Math.Round(Convert.ToDecimal(r.DateNumber), 2),
                                                   Unit = r.DateUnit,
                                                   Name = (Convert.ToDecimal(r.DateNumber.ToString().Split('.')[1]) == 0 ? Convert.ToInt32(r.DateNumber) : Math.Round(Convert.ToDecimal(r.DateNumber), 2)).ToString() + CommonMethodHelp.GetDateTimeUnit(r.DateUnit)
                                               }).ToList();

            //验收款期限时间集合
            var acceptancePayLimitList = payTermSets.Where(r => r.ModuleTypeId == 4)
                                                    .Select(r => new PayPhaseHelp
                                                    {
                                                        IsDefault = r.IsDefault,
                                                        Number = Convert.ToDecimal(r.DateNumber.ToString().Split('.')[1]) == 0 ? Convert.ToInt32(r.DateNumber) : Math.Round(Convert.ToDecimal(r.DateNumber), 2),
                                                        Unit = r.DateUnit,
                                                        Name = (Convert.ToDecimal(r.DateNumber.ToString().Split('.')[1]) == 0 ? Convert.ToInt32(r.DateNumber) : Math.Round(Convert.ToDecimal(r.DateNumber), 2)).ToString() + CommonMethodHelp.GetDateTimeUnit(r.DateUnit)
                                                    }).ToList();

            //质保款可选时间集合
            var qualityAssuranceList = payTermSets.Where(r => r.PayPhases.Any(x => x.PayPhaseType == 5))
                                                  .Select(r => new PayPhaseHelp
                                                  {
                                                      IsDefault = r.IsDefault,
                                                      Number = Convert.ToDecimal(r.DateNumber.ToString().Split('.')[1]) == 0 ? Convert.ToInt32(r.DateNumber) : Math.Round(Convert.ToDecimal(r.DateNumber), 2),
                                                      Unit = r.DateUnit,
                                                      Name = (Convert.ToDecimal(r.DateNumber.ToString().Split('.')[1]) == 0 ? Convert.ToInt32(r.DateNumber) : Math.Round(Convert.ToDecimal(r.DateNumber), 2)).ToString() + CommonMethodHelp.GetDateTimeUnit(r.DateUnit)
                                                  }).ToList();

            //质保款期限时间集合
            var qualityAssuranceLimitList = payTermSets.Where(r => r.ModuleTypeId == 3)
                                                       .Select(r => new PayPhaseHelp
                                                       {
                                                           IsDefault = r.IsDefault,
                                                           Number = Convert.ToDecimal(r.DateNumber.ToString().Split('.')[1]) == 0 ? Convert.ToInt32(r.DateNumber) : Math.Round(Convert.ToDecimal(r.DateNumber), 2),
                                                           Unit = r.DateUnit,
                                                           Name = (Convert.ToDecimal(r.DateNumber.ToString().Split('.')[1]) == 0 ? Convert.ToInt32(r.DateNumber) : Math.Round(Convert.ToDecimal(r.DateNumber), 2)).ToString() + CommonMethodHelp.GetDateTimeUnit(r.DateUnit)
                                                       }).ToList();

            result.Data = new
            {
                PrepPaList = prepPaList,
                BefShipList = befShipList,
                GoodsToList = goodsToList,
                AcceptancePayList = acceptancePayList,
                AcceptancePayLimitList = acceptancePayLimitList,
                QualityAssuranceList = qualityAssuranceList,
                QualityAssuranceLimitList = qualityAssuranceLimitList
            };

            return result;
        }

        /// <summary>
        /// 付款条件详情信息
        /// </summary>
        /// <param name="groupNum">付款条件分组名称</param>
        /// <returns>返回付款条件明细信息</returns>
        public async Task<TableData> GetPayTermSetDetailMsg(string groupNum)
        {
            var result = new TableData();
            var payTermSaveList = await UnitWork.Find<PayTermSave>(r => r.GroupNum == groupNum).ToListAsync();
            if (payTermSaveList != null && payTermSaveList.Count() > 0)
            {
                var obj = payTermSaveList.FirstOrDefault();
                var prepaName = new PayPhaseDetailHelp()
                {
                    Percentage = obj.PrepaPro == 0 ? "" : obj.PrepaPro + "%",
                    DateNumber = obj.PrepaDay == 0 ? "" : obj.PrepaDay + CommonMethodHelp.GetDateTimeUnit(obj.PrepaUnit)
                };

                var befShipName = new PayPhaseDetailHelp()
                {
                    Percentage = obj.BefShipPro == 0 ? "" : obj.BefShipPro + "%",
                    DateNumber = obj.BefShipDay == 0 ? "" : obj.BefShipDay + CommonMethodHelp.GetDateTimeUnit(obj.BefShipUnit)
                };

                var goodsToName = new PayPhaseDetailHelp()
                {
                    Percentage = obj.GoodsToPro == 0 ? "" : obj.GoodsToPro + "%",
                    DateNumber = obj.GoodsToDay == 0 ? "" : obj.GoodsToDay + CommonMethodHelp.GetDateTimeUnit(obj.GoodsToUnit)
                };

                var qualityAssuranceName = new PayPhaseDetailHelp()
                {
                    Percentage = obj.QualityAssurancePro == 0 ? "" : obj.QualityAssurancePro + "%",
                    DateNumber = obj.QualityAssuranceDay == 0 ? "" : obj.QualityAssuranceDay + CommonMethodHelp.GetDateTimeUnit(obj.QualityAssuranceDayUnit),
                    DateLimit = obj.QualityAssuranceLimit == 0 ? "" : obj.QualityAssuranceLimit + CommonMethodHelp.GetDateTimeUnit(obj.QualityAssuranceLimitUnit)
                };

                var acceptancePayName = new PayPhaseDetailHelp()
                {
                    Percentage = obj.AcceptancePayPro == 0 ? "" : obj.AcceptancePayPro + "%",
                    DateNumber = obj.AcceptancePayDay == 0 ? "" : obj.AcceptancePayDay + CommonMethodHelp.GetDateTimeUnit(obj.AcceptancePayDayUnit),
                    DateLimit = obj.AcceptancePayLimit == 0 ? "" : obj.AcceptancePayLimit + CommonMethodHelp.GetDateTimeUnit(obj.AcceptancePayLimitUnit)
                };

                result.Data = new
                {
                    PrePaName = prepaName,
                    BefShipName = befShipName,
                    GoodsToName = goodsToName,
                    QualityAssuranceName = qualityAssuranceName,
                    AcceptancePayName = acceptancePayName
                };
            }

            return result;
        }

        /// <summary>
        /// 获取应收信息
        /// </summary>
        /// <param name="docEntry">销售订单单号</param>
        /// <param name="groupNum">付款条件</param>
        /// <returns>返回应收明细与销售订单总体情况信息</returns>
        public async Task<TableData> GetReceivableDetail(int docEntry, string groupNum)
        {
            var result = new TableData();
            if (string.IsNullOrEmpty(groupNum))
            {
                result.Code = 500;
                result.Message = "付款条件不能为空!";
                return result;
            }

            var groupNums = await UnitWork.Find<PayTermSave>(r => r.GroupNum == groupNum).FirstOrDefaultAsync();
            List<ReceDetailHelp> receDetailHelps = new List<ReceDetailHelp>();
            List<ReceHelp> receHelps = new List<ReceHelp>();
            List<SapEntityHelp> sapEntityHelps = new List<SapEntityHelp>();
            List<SapEntityHelp> sapEntityHelpList = new List<SapEntityHelp>();
            List<SapEntityHelp> rct2s = new List<SapEntityHelp>();
            if (groupNums != null && groupNums.GroupNum != "")
            {
                //获取销售订单对应的销售交货单
                var ordr = await UnitWork.Find<ORDR>(r => r.DocEntry == docEntry).Select(r => new { r.DocEntry, r.DocTotal, r.DocTotalFC }).FirstOrDefaultAsync();
                if (ordr == null)
                {
                    result.Data = new SaleOrderDetailHelp();
                    return result;
                }

                var odlns = await UnitWork.Find<DLN1>(r => r.BaseEntry == ordr.DocEntry).GroupBy(r => new { r.DocEntry }).Select(r => r.Key.DocEntry).ToListAsync();
                if (odlns != null && odlns.Count() > 0)
                {
                    //获取当前销售订单的应收发票
                    var inv1s = await UnitWork.Find<INV1>(r => odlns.Contains(r.BaseEntry)).GroupBy(r => new { r.DocEntry, r.BaseEntry, }).Select(r => new SapEntityHelp { DocEntry = r.Key.DocEntry, BaseEntry = r.Key.BaseEntry }).ToListAsync();
                    var oinvs = await UnitWork.Find<OINV>(r => r.CANCELED == "N").Select(r => new SapEntityHelp { DocEntry = r.DocEntry, CreateDate = r.CreateDate, TotalAmount = r.DocTotal, TotalAmountFC = r.DocTotalFC, CardCode = r.CardCode, DocCur = r.DocCur, DocRate = r.DocRate }).ToListAsync();
                    List<SapEntityHelp> receList = (from a in oinvs
                                                    join b in inv1s
                                                    on a.DocEntry equals b.DocEntry
                                                    select new SapEntityHelp
                                                    {
                                                        DocEntry = a.DocEntry,
                                                        BaseEntry = b == null ? 0 : b.BaseEntry,
                                                        CreateDate = a.CreateDate,
                                                        CardCode = a.CardCode,
                                                        DocCur = a.DocCur,
                                                        DocRate = a.DocRate,
                                                        TotalAmount = a.TotalAmount,
                                                        TotalAmountFC = a.TotalAmountFC
                                                    }).OrderBy(r => r.CreateDate).ToList();

                    //当销售收款单只有预付款且收款单不为空时，获取当前销售订单的所有预付款实际收款金额
                    var orcts = await UnitWork.Find<ORCT>(r => r.U_XSDD == ordr.DocEntry && r.Canceled == "N").Select(r => new SapEntityHelp { DocEntry = r.DocEntry, BaseEntry = r.U_XSDD, CreateDate = r.CreateDate, DocCur = r.DocCurr, DocRate = r.DocRate, TotalAmount = r.DocTotal, TotalAmountFC = r.DocTotalFC }).ToListAsync();
                    if (orcts != null && orcts.Count() > 0)
                    {
                        orcts = (from c in orcts
                                 join d in await UnitWork.Find<RCT2>(null).Select(r => new SapEntityHelp { DocEntry = r.DocNum, BaseEntry = r.DocEntry, TotalAmount = r.SumApplied, TotalAmountFC = r.AppliedFC }).ToListAsync()
                                 on c.DocEntry equals d.DocEntry into cd
                                 from d in cd.DefaultIfEmpty()
                                 where d is null
                                 select new SapEntityHelp
                                 {
                                     DocEntry = c.DocEntry,
                                     BaseEntry = d == null ? 0 : d.BaseEntry,
                                     DocCur = c.DocCur,
                                     DocRate = c.DocRate,
                                     CreateDate = c.CreateDate,
                                     TotalAmount = c.TotalAmount,
                                     TotalAmountFC = c.TotalAmountFC
                                 }).ToList();
                    }

                    foreach (SapEntityHelp oINVHelp in receList)
                    {
                        foreach (string recePayType in RecePayTypes)
                        {
                            #region 获取应收销售订单金额、应收日期等信息
                            ReceHelp receHelp = new ReceHelp();
                            receHelp.ReceInvoice = oINVHelp.DocEntry.ToString();
                            receHelp.ReceCreateDate = oINVHelp.CreateDate;
                            receHelp.TotalAmount = oINVHelp.TotalAmount;
                            receHelp.TotalAmountFC = oINVHelp.TotalAmountFC;
                            receHelp.DocCur = oINVHelp.DocCur;
                            receHelp.RecePayType = recePayType;
                            receHelp.ReceDocRate = oINVHelp.DocRate;
                            receHelp.ReceAmount = Convert.ToDecimal(oINVHelp.TotalAmount) * CommonMethodHelp.GetReceTypePerentage(recePayType, groupNums);
                            receHelp.ReceAmountFC = Convert.ToDecimal(oINVHelp.TotalAmountFC) * CommonMethodHelp.GetReceTypePerentage(recePayType, groupNums);
                            receHelp.ReceDate = CommonMethodHelp.GetReceTypeDate(recePayType, (DateTime)oINVHelp.CreateDate, groupNums).ToString("yyyy.MM.dd");
                            receHelp.ReceTypeDate = CommonMethodHelp.GetReceTypeDetailDate(recePayType, (DateTime)oINVHelp.CreateDate, groupNums);
                            receHelp.ReceDateTime = CommonMethodHelp.GetReceTypeDate(recePayType, (DateTime)oINVHelp.CreateDate, groupNums);
                            receHelps.Add(receHelp);
                            #endregion
                        }

                        #region 当销售收款单在存在应收发票，并且产生销售收款单，则根据应收发票找到对应销售收款单对应的实际金额，并获取实际总金额
                        List<SapEntityHelp> rct2Nows = await UnitWork.Find<RCT2>(r => r.DocEntry == oINVHelp.DocEntry).Select(r => new SapEntityHelp { DocEntry = r.DocNum, BaseEntry = r.DocEntry, TotalAmount = r.SumApplied, TotalAmountFC = r.AppliedFC }).ToListAsync();
                        var orctlist = await UnitWork.Find<ORCT>(r => r.CardCode == oINVHelp.CardCode).Select(r => new SapEntityHelp { DocEntry = r.DocNum, CreateDate = r.CreateDate, DocCur = r.DocCurr, DocRate = r.DocRate }).ToListAsync();
                        if (rct2Nows != null && rct2Nows.Count() > 0)
                        {
                            rct2Nows = (from a in rct2Nows
                                        join b in orctlist
                                        on a.DocEntry equals b.DocEntry into ab
                                        from b in ab.DefaultIfEmpty()
                                        select new SapEntityHelp
                                        {
                                            DocEntry = b == null ? 0 : b.DocEntry,
                                            BaseEntry = oINVHelp.DocEntry,
                                            CreateDate = b == null ? DateTime.Now : b.CreateDate,
                                            DocCur = b == null ? "" : b.DocCur,
                                            DocRate = b == null ? 0 : b.DocRate,
                                            TotalAmount = a.TotalAmount,
                                            TotalAmountFC = a.TotalAmountFC
                                        }).ToList();

                            foreach (SapEntityHelp item in rct2Nows)
                                rct2s.Add(item);
                        }
                        #endregion
                    }

                    //合并销售订单对应的销售收款单
                    if ((orcts == null || orcts.Count() == 0) && (rct2s == null || rct2s.Count() == 0))
                    {
                        sapEntityHelps = null;
                        sapEntityHelpList = null;
                    }
                    else if ((orcts != null && orcts.Count() > 0) && (rct2s == null || rct2s.Count() == 0))
                    {
                        sapEntityHelps = orcts.OrderBy(r => r.CreateDate).ToList();
                        sapEntityHelpList = orcts.OrderBy(r => r.CreateDate).ToList();
                    }
                    else if ((orcts == null || orcts.Count() == 0) && (rct2s != null && rct2s.Count() > 0))
                    {
                        sapEntityHelps = rct2s.OrderBy(r => r.CreateDate).ToList();
                        sapEntityHelpList = rct2s.OrderBy(r => r.CreateDate).ToList();
                    }
                    else
                    {
                        sapEntityHelps = (orcts.Union(rct2s).ToList<SapEntityHelp>()).OrderBy(r => r.CreateDate).ToList();
                        sapEntityHelpList = (orcts.Union(rct2s).ToList<SapEntityHelp>()).OrderBy(r => r.CreateDate).ToList();
                    }

                    //计算应收明细                 
                    receDetailHelps = GetReceDetailHelps(sapEntityHelps, receHelps);
                    receDetailHelps = receDetailHelps.OrderBy(r => Convert.ToInt32(r.ReceInvoice)).ToList();

                    //总体情况
                    SaleReceHelp saleReceHelp = new SaleReceHelp();
                    saleReceHelp.DocEntry = docEntry;
                    saleReceHelp.DocCur = (receDetailHelps.GroupBy(r => new { r.ReceDocCur }).Select(r => r.Key.ReceDocCur).ToList()).FirstOrDefault();
                    saleReceHelp.DocRate = (Convert.ToDecimal((receDetailHelps.GroupBy(r => new { r.ReceDocRate }).Select(r => r.Key.ReceDocRate).ToList()).FirstOrDefault())).ToString();
                    saleReceHelp.SaleReceAmount = ordr.DocTotal == 0 || ordr.DocTotal == null ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(ordr.DocTotal), 2);
                    saleReceHelp.SaleReceAmountFC = ordr.DocTotalFC == 0 || ordr.DocTotal == null ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(ordr.DocTotalFC), 2);
                    saleReceHelp.ReceAmount = receHelps.Sum(r => r.ReceAmount) == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelps.Sum(r => r.ReceAmount)), 2);
                    saleReceHelp.ReceAmountFC = receHelps.Sum(r => r.ReceAmountFC) == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelps.Sum(r => r.ReceAmountFC)), 2);
                    saleReceHelp.ActualAmount = (sapEntityHelpList == null ? 0 : sapEntityHelpList.Sum(r => r.TotalAmount)) == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(sapEntityHelpList == null ? 0 : sapEntityHelpList.Sum(r => r.TotalAmount)), 2);
                    saleReceHelp.ActualAmountFC = (sapEntityHelpList == null ? 0 : sapEntityHelpList.Sum(r => r.TotalAmountFC)) == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(sapEntityHelpList == null ? 0 : sapEntityHelpList.Sum(r => r.TotalAmountFC)), 2);
                    saleReceHelp.WithOutLimitMaxDay = receDetailHelps.OrderByDescending(r => r.WithinLimitDay).Select(r => r.WithinLimitDay).FirstOrDefault();
                    saleReceHelp.WithOutLimitAmount = _serviceBaseApp.MoneyToCoin(receDetailHelps.Sum(r => r.WithinAmount), 2);
                    saleReceHelp.WithOutLimitPre = receHelps.Sum(r => r.ReceAmount) == 0 ? "0%" : (saleReceHelp.DocCur == "RMB" ? (Convert.ToDecimal(sapEntityHelpList == null ? 0 : receDetailHelps.Sum(r => r.WithinAmount))) / (Convert.ToDecimal(receHelps.Sum(r => r.ReceAmount))) : (Convert.ToDecimal(sapEntityHelpList == null ? 0 : receDetailHelps.Sum(r => r.WithinAmount))) / (Convert.ToDecimal(receHelps.Sum(r => r.ReceAmountFC)))) * 100 + "%";

                    //应收明细和总体情况
                    result.Data = new SaleOrderDetailHelp
                    {
                        ReceDetailHelps = receDetailHelps,
                        SaleReceHelp = saleReceHelp
                    };
                }
                else
                {
                    result.Data = new SaleOrderDetailHelp();
                }
            }
            else
            {
                result.Code = 201;
                result.Message = "当前付款条件不在4.0中，无法查看应收详情";
            }

            return result;
        }

        /// <summary>
        /// 计算应收发票与销售收款明细
        /// </summary>
        /// <param name="sapEntityHelps">销售收款实体数据集</param>
        /// <param name="receHelps">应收发票实体数据集</param>
        /// <returns>返回应收发票与销售收款计算逾期信息</returns>
        public List<ReceDetailHelp> GetReceDetailHelps(List<SapEntityHelp> sapEntityHelps, List<ReceHelp> receHelps)
        {
            List<ReceDetailHelp> receDetailHelps = new List<ReceDetailHelp>();
            receHelps = receHelps.OrderBy(r => r.ReceDateTime).ToList();
            if (sapEntityHelps != null && sapEntityHelps.Count() > 0)
            {
                var sapDocCurs = sapEntityHelps.GroupBy(r => new { r.DocCur }).Select(r => r.Key.DocCur).ToList();
                if (sapDocCurs.Count() > 1 || sapDocCurs.Contains("RMB"))
                {
                    foreach (ReceHelp receHelp in receHelps)
                    {
                        decimal totalAmount = 0;
                        decimal receAmount = Convert.ToDecimal(receHelp.ReceAmount);

                        //应收明细实体
                        ReceDetailHelp receDetailHelp = new ReceDetailHelp();
                        receDetailHelp.ReceInvoice = receHelp.ReceInvoice;
                        receDetailHelp.ReceDocRate = receHelp.ReceDocRate;
                        receDetailHelp.ReceCreateDate = Convert.ToDateTime(receHelp.ReceCreateDate).ToString("yyyy.MM.dd");
                        receDetailHelp.InvoiceTotalAmount = receHelp.TotalAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelp.TotalAmount), 2);
                        receDetailHelp.RecePayType = receHelp.RecePayType;
                        receDetailHelp.ReceDocCur = "RMB";
                        receDetailHelp.ReceAmount = receHelp.ReceAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(receHelp.ReceAmount, 2);
                        receDetailHelp.ReceDate = receHelp.ReceDate;
                        receDetailHelp.ReceTypeDate = receHelp.ReceTypeDate;

                        //应收发票金额与销售收款金额匹配
                        List<SapEntityHelp> sapEntityHelpList = new List<SapEntityHelp>();
                        if (sapEntityHelps != null && sapEntityHelps.Count() > 0)
                        {
                            foreach (SapEntityHelp item in sapEntityHelps.ToArray())
                            {
                                totalAmount = totalAmount + Convert.ToDecimal(item.TotalAmount);
                                if (receAmount >= totalAmount)
                                {
                                    //移除已经参与计算的收款金额
                                    sapEntityHelpList.Add(item);
                                    sapEntityHelps.Remove(item);
                                    receAmount = receAmount - totalAmount;
                                    if (sapEntityHelps.Count() == 0)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    if (receAmount != 0)
                                    {
                                        //剩余应收发票应收金额
                                        SapEntityHelp sapEntityActualHelp = new SapEntityHelp();
                                        sapEntityActualHelp.DocEntry = item.DocEntry;
                                        sapEntityActualHelp.BaseEntry = item.BaseEntry;
                                        sapEntityActualHelp.CreateDate = item.CreateDate;
                                        sapEntityActualHelp.DocCur = item.DocCur;
                                        sapEntityActualHelp.TotalAmount = receAmount;
                                        sapEntityActualHelp.TotalAmountFC = item.TotalAmountFC;
                                        sapEntityActualHelp.DocRate = item.DocRate;
                                        sapEntityHelpList.Add(sapEntityActualHelp);

                                        if ((item.TotalAmount - receAmount) != 0)
                                        {
                                            //剩余收款金额重新添加到收款实体排序参与循环
                                            SapEntityHelp sapEntityHelp = new SapEntityHelp();
                                            sapEntityHelp.DocEntry = item.DocEntry;
                                            sapEntityHelp.BaseEntry = item.BaseEntry;
                                            sapEntityHelp.CreateDate = item.CreateDate;
                                            sapEntityHelp.DocCur = item.DocCur;
                                            sapEntityHelp.TotalAmount = item.TotalAmount - receAmount;
                                            sapEntityHelp.TotalAmountFC = item.TotalAmountFC;
                                            sapEntityHelp.DocRate = item.DocRate;
                                            sapEntityHelps.Add(sapEntityHelp);
                                            sapEntityHelps = sapEntityHelps.OrderBy(r => r.CreateDate).ToList();
                                        }

                                        sapEntityHelps.Remove(item);
                                    }

                                    break;
                                }
                            }
                        }

                        if (sapEntityHelpList == null || sapEntityHelpList.Count() == 0)
                        {
                            if (receHelp.RecePayType == "预付/货前款")
                            {
                                receDetailHelp.ActualReceDate = "";
                                receDetailHelp.SaleReceiptNo = "";
                                receDetailHelp.ReceiveAmount = "";
                                receDetailHelp.NoReceiveAmount = "";
                                receDetailHelp.WithinLimitDay = "";
                                receDetailHelp.WithinLimitAmount = "";
                                receDetailHelp.WithinAmount = 0;
                                receDetailHelp.WithinLimitPre = "";
                                receDetailHelp.AccountRece = "未收款";
                            }
                            else
                            {
                                receDetailHelp.ActualReceDate = "";
                                receDetailHelp.SaleReceiptNo = "";
                                receDetailHelp.ReceiveAmount = "";
                                receDetailHelp.NoReceiveAmount = receDetailHelp.ReceAmount;
                                receDetailHelp.WithinLimitDay = DateTime.Now <= Convert.ToDateTime(receHelp.ReceDateTime) ? "" : (DateTime.Now - Convert.ToDateTime(receHelp.ReceDateTime)).Days.ToString();
                                receDetailHelp.WithinLimitAmount = receDetailHelp.WithinLimitDay == "" ? "" : receDetailHelp.ReceAmount;
                                receDetailHelp.WithinAmount = receDetailHelp.WithinLimitDay == "" ? 0 : (receHelp.ReceAmount == 0 ? 0 : receHelp.ReceAmount);
                                receDetailHelp.WithinLimitPre = receDetailHelp.WithinLimitDay == "" ? "0%" : "100%";
                                receDetailHelp.AccountRece = "未收款";
                            }
                        }
                        else
                        {
                            if (receHelp.RecePayType == "预付/货前款")
                            {
                                decimal receiveAmount = Convert.ToDecimal(sapEntityHelpList.Sum(r => r.TotalAmount));
                                receDetailHelp.ActualReceDate = Convert.ToDecimal(receHelp.ReceAmount) <= receiveAmount ? (sapEntityHelpList.GroupBy(r => new { r.CreateDate }).Select(r => Convert.ToDateTime(r.Key.CreateDate)).ToList().OrderByDescending(r => r).FirstOrDefault()).ToString("yyyy.MM.dd") : "";
                                receDetailHelp.SaleReceiptNo = string.Join(",", sapEntityHelpList.GroupBy(r => new { r.DocEntry }).Select(r => r.Key.DocEntry).ToList());
                                receDetailHelp.ReceiveAmount = receiveAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(receiveAmount, 2);
                                receDetailHelp.NoReceiveAmount = Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount, 2);
                                receDetailHelp.WithinLimitDay = "";
                                receDetailHelp.WithinLimitAmount = "";
                                receDetailHelp.WithinAmount = 0;
                                receDetailHelp.WithinLimitPre = "";
                                receDetailHelp.AccountRece = Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount == 0 ? "已收款" : (Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount > 0) ? "部分收款" : "已收款";
                            }
                            else
                            {
                                decimal receiveAmount = Convert.ToDecimal(sapEntityHelpList.Sum(r => r.TotalAmount));
                                receDetailHelp.ActualReceDate = Convert.ToDecimal(receHelp.ReceAmount) <= receiveAmount ? (sapEntityHelpList.GroupBy(r => new { r.CreateDate }).Select(r => Convert.ToDateTime(r.Key.CreateDate)).ToList().OrderByDescending(r => r).FirstOrDefault()).ToString("yyyy.MM.dd") : "";
                                receDetailHelp.SaleReceiptNo = string.Join(",", sapEntityHelpList.GroupBy(r => new { r.DocEntry }).Select(r => r.Key.DocEntry).ToList());
                                receDetailHelp.ReceiveAmount = receiveAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(receiveAmount, 2);
                                receDetailHelp.NoReceiveAmount = Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount, 2);
                                int days = (DateTime.Now - Convert.ToDateTime(receHelp.ReceDateTime)).Days;
                                int recedays = receDetailHelp.ActualReceDate == "" ? 0 : (Convert.ToDateTime(receDetailHelp.ActualReceDate) - Convert.ToDateTime(receHelp.ReceDateTime)).Days;
                                receDetailHelp.WithinLimitDay = Convert.ToDecimal(receHelp.ReceAmount) > receiveAmount ? (days <= 0 ? "" : days.ToString()) : (receHelp.ReceDateTime >= Convert.ToDateTime(receDetailHelp.ActualReceDate) ? (recedays <= 0 ? "" : recedays.ToString()) : "");
                                receDetailHelp.WithinLimitAmount = receDetailHelp.WithinLimitDay == "" ? "" : (Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount, 2));
                                receDetailHelp.WithinAmount = receDetailHelp.WithinLimitDay == "" ? 0 : (Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount == 0 ? 0 : Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount);
                                receDetailHelp.WithinLimitPre = receDetailHelp.WithinLimitDay == "" ? "0%" : (Convert.ToDecimal(receHelp.ReceAmount) == 0 ? "0%" : Math.Round(((Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount == 0 ? 0 : Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount) / (Convert.ToDecimal(receHelp.ReceAmount))) * 100, 2) + "%");
                                receDetailHelp.AccountRece = receiveAmount == 0 ? "未收款" : (Convert.ToDecimal(receHelp.ReceAmount) > receiveAmount ? "部分收款" : "已收款");
                            }
                        }

                        receDetailHelps.Add(receDetailHelp);
                    }
                }
                else
                {
                    foreach (ReceHelp receHelp in receHelps)
                    {
                        if (receHelp.DocCur != "RMB" && sapDocCurs.FirstOrDefault() == receHelp.DocCur)
                        {
                            decimal totalAmount = 0;
                            decimal receAmount = Convert.ToDecimal(receHelp.ReceAmountFC);

                            //应收明细实体
                            ReceDetailHelp receDetailHelp = new ReceDetailHelp();
                            receDetailHelp.ReceInvoice = receHelp.ReceInvoice;
                            receDetailHelp.ReceDocRate = receHelp.ReceDocRate;
                            receDetailHelp.ReceCreateDate = Convert.ToDateTime(receHelp.ReceCreateDate).ToString("yyyy.MM.dd");
                            receDetailHelp.InvoiceTotalAmount = receHelp.TotalAmountFC == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelp.TotalAmountFC), 2);
                            receDetailHelp.RecePayType = receHelp.RecePayType;
                            receDetailHelp.ReceDocCur = receHelp.DocCur;
                            receDetailHelp.ReceAmount = receHelp.ReceAmountFC == 0 ? "" : _serviceBaseApp.MoneyToCoin(receHelp.ReceAmountFC, 2);
                            receDetailHelp.ReceDate = receHelp.ReceDate;
                            receDetailHelp.ReceTypeDate = receHelp.ReceTypeDate;

                            //应收发票金额与销售收款金额匹配（外币）
                            List<SapEntityHelp> sapEntityHelpList = new List<SapEntityHelp>();
                            foreach (SapEntityHelp item in sapEntityHelps.ToArray())
                            {
                                totalAmount = totalAmount + Convert.ToDecimal(item.TotalAmountFC);
                                if (receAmount >= totalAmount)
                                {
                                    sapEntityHelps.Remove(item);
                                    receAmount = receAmount - totalAmount;
                                    sapEntityHelpList.Add(item);
                                }
                                else
                                {
                                    if (receAmount != 0)
                                    {
                                        //剩余应收发票应收金额（外币）
                                        SapEntityHelp sapEntityActualHelp = new SapEntityHelp();
                                        sapEntityActualHelp.DocEntry = item.DocEntry;
                                        sapEntityActualHelp.BaseEntry = item.BaseEntry;
                                        sapEntityActualHelp.CreateDate = item.CreateDate;
                                        sapEntityActualHelp.DocCur = item.DocCur;
                                        sapEntityActualHelp.TotalAmount = receAmount;
                                        sapEntityActualHelp.TotalAmountFC = item.TotalAmountFC;
                                        sapEntityActualHelp.DocRate = item.DocRate;
                                        sapEntityHelpList.Add(sapEntityActualHelp);

                                        //剩余收款金额重新添加到收款实体排序参与循环（外币）
                                        SapEntityHelp sapEntityHelp = new SapEntityHelp();
                                        sapEntityHelp.DocEntry = item.DocEntry;
                                        sapEntityHelp.BaseEntry = item.BaseEntry;
                                        sapEntityHelp.CreateDate = item.CreateDate;
                                        sapEntityHelp.DocCur = item.DocCur;
                                        sapEntityHelp.TotalAmount = item.TotalAmountFC - receAmount;
                                        sapEntityHelp.TotalAmountFC = item.TotalAmountFC;
                                        sapEntityHelp.DocRate = item.DocRate;
                                        sapEntityHelps.Remove(item);
                                        sapEntityHelps.Add(sapEntityHelp);
                                        sapEntityHelps = sapEntityHelps.OrderBy(r => r.CreateDate).ToList();
                                    }

                                    break;
                                }
                            }

                            //获取应收明细实收信息
                            if (sapEntityHelpList == null || sapEntityHelpList.Count() == 0)
                            {
                                if (receHelp.RecePayType == "预付/货前款")
                                {
                                    receDetailHelp.ActualReceDate = "";
                                    receDetailHelp.SaleReceiptNo = "";
                                    receDetailHelp.ReceiveAmount = "";
                                    receDetailHelp.NoReceiveAmount = "";
                                    receDetailHelp.WithinLimitDay = "";
                                    receDetailHelp.WithinLimitAmount = "";
                                    receDetailHelp.WithinAmount = 0;
                                    receDetailHelp.WithinLimitPre = "";
                                    receDetailHelp.AccountRece = "未收款";
                                }
                                else
                                {
                                    receDetailHelp.ActualReceDate = "";
                                    receDetailHelp.SaleReceiptNo = "";
                                    receDetailHelp.ReceiveAmount = "";
                                    receDetailHelp.NoReceiveAmount = receDetailHelp.ReceAmount;
                                    int days = (DateTime.Now - Convert.ToDateTime(receHelp.ReceDateTime)).Days;
                                    receDetailHelp.WithinLimitDay = DateTime.Now <= Convert.ToDateTime(receHelp.ReceDateTime) ? "" : (days <= 0 ? "" : days.ToString());
                                    receDetailHelp.WithinLimitAmount = receDetailHelp.WithinLimitDay == "" ? "" : receDetailHelp.ReceAmount;
                                    receDetailHelp.WithinAmount = receDetailHelp.WithinLimitDay == "" ? 0 : (receHelp.ReceAmount == 0 ? 0 : receHelp.ReceAmount);
                                    receDetailHelp.WithinLimitPre = receDetailHelp.WithinLimitDay == "" ? "0%" : "100%";
                                    receDetailHelp.AccountRece = "未收款";
                                }
                            }
                            else
                            {
                                if (receHelp.RecePayType == "预付/货前款")
                                {
                                    decimal receiveAmount = Convert.ToDecimal(sapEntityHelpList.Sum(r => r.TotalAmount));
                                    receDetailHelp.ActualReceDate = Convert.ToDecimal(receHelp.ReceAmountFC) <= receiveAmount ? (sapEntityHelpList.GroupBy(r => new { r.CreateDate }).Select(r => Convert.ToDateTime(r.Key.CreateDate)).ToList().OrderByDescending(r => r).FirstOrDefault()).ToString("yyyy.MM.dd") : "";
                                    receDetailHelp.SaleReceiptNo = string.Join(",", sapEntityHelpList.GroupBy(r => new { r.DocEntry }).Select(r => r.Key.DocEntry).ToList());
                                    receDetailHelp.ReceiveAmount = receiveAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(receiveAmount, 2);
                                    receDetailHelp.NoReceiveAmount = Convert.ToDecimal(receHelp.ReceAmountFC) - receiveAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelp.ReceAmountFC) - receiveAmount, 2);
                                    receDetailHelp.WithinLimitDay = "";
                                    receDetailHelp.WithinLimitAmount = "";
                                    receDetailHelp.WithinAmount = 0;
                                    receDetailHelp.WithinLimitPre = "";
                                    receDetailHelp.AccountRece = Convert.ToDecimal(receHelp.ReceAmountFC) - receiveAmount == 0 ? "已收款" : (Convert.ToDecimal(receHelp.ReceAmountFC) - receiveAmount > 0) ? "部分收款" : "已收款";
                                }
                                else
                                {
                                    decimal receiveAmount = Convert.ToDecimal(sapEntityHelpList.Sum(r => r.TotalAmountFC));
                                    receDetailHelp.ActualReceDate = Convert.ToDecimal(receHelp.ReceAmountFC) <= receiveAmount ? (sapEntityHelpList.GroupBy(r => new { r.CreateDate }).Select(r => Convert.ToDateTime(r.Key.CreateDate)).ToList().OrderByDescending(r => r).FirstOrDefault()).ToString("yyyy.MM.dd") : "";
                                    receDetailHelp.SaleReceiptNo = string.Join(",", sapEntityHelpList.GroupBy(r => new { r.DocEntry }).Select(r => r.Key.DocEntry).ToList());
                                    receDetailHelp.ReceiveAmount = receiveAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(receiveAmount, 2);
                                    receDetailHelp.NoReceiveAmount = Convert.ToDecimal(receHelp.ReceAmountFC) - receiveAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelp.ReceAmountFC) - receiveAmount, 2);
                                    int days = (DateTime.Now - Convert.ToDateTime(receHelp.ReceDateTime)).Days;
                                    int receDays = receDetailHelp.ActualReceDate == "" ? 0 : (Convert.ToDateTime(receDetailHelp.ActualReceDate) - Convert.ToDateTime(receHelp.ReceDateTime)).Days;
                                    receDetailHelp.WithinLimitDay = Convert.ToDecimal(receHelp.ReceAmountFC) > receiveAmount ? (days <= 0 ? "" : days.ToString()) : (receHelp.ReceDateTime >= Convert.ToDateTime(receDetailHelp.ActualReceDate) ? (receDays <= 0 ? "" : receDays.ToString()) : "");
                                    receDetailHelp.WithinLimitAmount = receDetailHelp.WithinLimitDay == "" ? "" : (Convert.ToDecimal(receHelp.ReceAmountFC) - receiveAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelp.ReceAmountFC) - receiveAmount, 2));
                                    receDetailHelp.WithinAmount = receDetailHelp.WithinLimitDay == "" ? 0 : (Convert.ToDecimal(receHelp.ReceAmountFC) - receiveAmount == 0 ? 0 : Convert.ToDecimal(receHelp.ReceAmountFC) - receiveAmount);
                                    receDetailHelp.WithinLimitPre = receDetailHelp.WithinLimitDay == "" ? "0%" : (Convert.ToDecimal(receHelp.ReceAmountFC) == 0 ? "0%" : Math.Round(((Convert.ToDecimal(receHelp.ReceAmountFC) - receiveAmount == 0 ? 0 : Convert.ToDecimal(receHelp.ReceAmountFC) - receiveAmount) / (Convert.ToDecimal(receHelp.ReceAmountFC))) * 100, 2) + "%");
                                    receDetailHelp.AccountRece = receiveAmount == 0 ? "未收款" : (Convert.ToDecimal(receHelp.TotalAmount) > receiveAmount ? "部分收款" : "已收款");
                                }
                            }

                            receDetailHelps.Add(receDetailHelp);
                        }
                        else
                        {
                            decimal totalAmount = 0;
                            decimal receAmount = Convert.ToDecimal(receHelp.ReceAmount);

                            //应收明细实体
                            ReceDetailHelp receDetailHelp = new ReceDetailHelp();
                            receDetailHelp.ReceInvoice = receHelp.ReceInvoice;
                            receDetailHelp.ReceDocRate = receHelp.ReceDocRate;
                            receDetailHelp.ReceCreateDate = Convert.ToDateTime(receHelp.ReceCreateDate).ToString("yyyy.MM.dd");
                            receDetailHelp.InvoiceTotalAmount = receHelp.TotalAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelp.TotalAmount), 2);
                            receDetailHelp.RecePayType = receHelp.RecePayType;
                            receDetailHelp.ReceDocCur = "RMB";
                            receDetailHelp.ReceAmount = receHelp.ReceAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(receHelp.ReceAmount, 2);
                            receDetailHelp.ReceDate = receHelp.ReceDate;

                            //应收发票金额与销售收款金额匹配
                            List<SapEntityHelp> sapEntityHelpList = new List<SapEntityHelp>();
                            foreach (SapEntityHelp item in sapEntityHelps.ToArray())
                            {
                                totalAmount = totalAmount + Convert.ToDecimal(item.TotalAmount);
                                if (receAmount >= totalAmount)
                                {
                                    sapEntityHelps.Remove(item);
                                    receAmount = receAmount - totalAmount;
                                    sapEntityHelpList.Add(item);
                                }
                                else
                                {
                                    if (receAmount != 0)
                                    {
                                        //剩余应收发票应收金额
                                        SapEntityHelp sapEntityActualHelp = new SapEntityHelp();
                                        sapEntityActualHelp.DocEntry = item.DocEntry;
                                        sapEntityActualHelp.BaseEntry = item.BaseEntry;
                                        sapEntityActualHelp.CreateDate = item.CreateDate;
                                        sapEntityActualHelp.DocCur = item.DocCur;
                                        sapEntityActualHelp.TotalAmount = receAmount;
                                        sapEntityActualHelp.TotalAmountFC = item.TotalAmount;
                                        sapEntityActualHelp.DocRate = item.DocRate;
                                        sapEntityHelpList.Add(sapEntityActualHelp);

                                        //剩余收款金额重新添加到收款实体排序参与循环
                                        SapEntityHelp sapEntityHelp = new SapEntityHelp();
                                        sapEntityHelp.DocEntry = item.DocEntry;
                                        sapEntityHelp.BaseEntry = item.BaseEntry;
                                        sapEntityHelp.CreateDate = item.CreateDate;
                                        sapEntityHelp.DocCur = item.DocCur;
                                        sapEntityHelp.TotalAmount = item.TotalAmount - receAmount;
                                        sapEntityHelp.TotalAmountFC = item.TotalAmount;
                                        sapEntityHelp.DocRate = item.DocRate;
                                        sapEntityHelps.Remove(item);
                                        sapEntityHelps.Add(sapEntityHelp);
                                        sapEntityHelps = sapEntityHelps.OrderBy(r => r.CreateDate).ToList();
                                    }

                                    break;
                                }
                            }

                            //获取应收明细实收信息
                            if (sapEntityHelpList == null || sapEntityHelpList.Count() == 0)
                            {
                                if (receHelp.RecePayType == "预付/货前款")
                                {
                                    receDetailHelp.ActualReceDate = "";
                                    receDetailHelp.SaleReceiptNo = "";
                                    receDetailHelp.ReceiveAmount = "";
                                    receDetailHelp.NoReceiveAmount = "";
                                    receDetailHelp.WithinLimitDay = "";
                                    receDetailHelp.WithinLimitAmount = "";
                                    receDetailHelp.WithinAmount = 0;
                                    receDetailHelp.WithinLimitPre = "";
                                    receDetailHelp.AccountRece = "未收款";
                                }
                                else
                                {
                                    receDetailHelp.ActualReceDate = "";
                                    receDetailHelp.SaleReceiptNo = "";
                                    receDetailHelp.ReceiveAmount = "";
                                    receDetailHelp.NoReceiveAmount = receDetailHelp.ReceAmount;
                                    int days = (DateTime.Now - Convert.ToDateTime(receHelp.ReceDateTime)).Days;
                                    receDetailHelp.WithinLimitDay = DateTime.Now <= Convert.ToDateTime(receHelp.ReceDateTime) ? "" : (days <= 0 ? "" : days.ToString());
                                    receDetailHelp.WithinLimitAmount = receDetailHelp.WithinLimitDay == "" ? "" : receDetailHelp.ReceAmount;
                                    receDetailHelp.WithinAmount = receDetailHelp.WithinLimitDay == "" ? 0 : (receHelp.ReceAmount == 0 ? 0 : receHelp.ReceAmount);
                                    receDetailHelp.WithinLimitPre = receDetailHelp.WithinLimitDay == "" ? "0%" : "100%";
                                    receDetailHelp.AccountRece = "未收款";
                                }
                            }
                            else
                            {
                                if (receHelp.RecePayType == "预付/货前款")
                                {
                                    decimal receiveAmount = Convert.ToDecimal(sapEntityHelpList.Sum(r => r.TotalAmount));
                                    receDetailHelp.ActualReceDate = Convert.ToDecimal(receHelp.ReceAmount) <= receiveAmount ? (sapEntityHelpList.GroupBy(r => new { r.CreateDate }).Select(r => Convert.ToDateTime(r.Key.CreateDate)).ToList().OrderByDescending(r => r).FirstOrDefault()).ToString("yyyy.MM.dd") : "";
                                    receDetailHelp.SaleReceiptNo = string.Join(",", sapEntityHelpList.GroupBy(r => new { r.DocEntry }).Select(r => r.Key.DocEntry).ToList());
                                    receDetailHelp.ReceiveAmount = receiveAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(receiveAmount, 2);
                                    receDetailHelp.NoReceiveAmount = Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount, 2);
                                    receDetailHelp.WithinLimitDay = "";
                                    receDetailHelp.WithinLimitAmount = "";
                                    receDetailHelp.WithinAmount = 0;
                                    receDetailHelp.WithinLimitPre = "";
                                    receDetailHelp.AccountRece = Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount == 0 ? "已收款" : (Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount > 0) ? "部分收款" : "已收款";
                                }
                                else
                                {
                                    decimal receiveAmount = Convert.ToDecimal(sapEntityHelpList.Sum(r => r.TotalAmount));
                                    receDetailHelp.ActualReceDate = Convert.ToDecimal(receHelp.ReceAmount) <= receiveAmount ? (sapEntityHelpList.GroupBy(r => new { r.CreateDate }).Select(r => Convert.ToDateTime(r.Key.CreateDate)).ToList().OrderByDescending(r => r).FirstOrDefault()).ToString("yyyy.MM.dd") : "";
                                    receDetailHelp.SaleReceiptNo = string.Join(",", sapEntityHelpList.GroupBy(r => new { r.DocEntry }).Select(r => r.Key.DocEntry).ToList());
                                    receDetailHelp.ReceiveAmount = receiveAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(receiveAmount, 2);
                                    receDetailHelp.NoReceiveAmount = Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount, 2);
                                    int days = (DateTime.Now - Convert.ToDateTime(receHelp.ReceDateTime)).Days;
                                    int receDays = receDetailHelp.ActualReceDate == "" ? 0 : (Convert.ToDateTime(receDetailHelp.ActualReceDate) - Convert.ToDateTime(receHelp.ReceDateTime)).Days;
                                    receDetailHelp.WithinLimitDay = Convert.ToDecimal(receHelp.ReceAmount) > receiveAmount ? (days <= 0 ? "" : days.ToString()) : (receHelp.ReceDateTime >= Convert.ToDateTime(receDetailHelp.ActualReceDate) ? (receDays <= 0 ? "" : receDays.ToString()) : "");
                                    receDetailHelp.WithinLimitAmount = receDetailHelp.WithinLimitDay == "" ? "" : (Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelp.TotalAmount) - receiveAmount, 2));
                                    receDetailHelp.WithinAmount = receDetailHelp.WithinLimitDay == "" ? 0 : (Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount == 0 ? 0 : Convert.ToDecimal(receHelp.TotalAmount) - receiveAmount);
                                    receDetailHelp.WithinLimitPre = receDetailHelp.WithinLimitDay == "" ? "0%" : (Convert.ToDecimal(receHelp.ReceAmount) == 0 ? "0%" : Math.Round(((Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount == 0 ? 0 : Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount) / (Convert.ToDecimal(receHelp.ReceAmount))) * 100, 2) + "%");
                                    receDetailHelp.AccountRece = receiveAmount == 0 ? "未收款" : (Convert.ToDecimal(receHelp.ReceAmount) > receiveAmount ? "部分收款" : "已收款");
                                }
                            }

                            receDetailHelps.Add(receDetailHelp);
                        }
                    }
                }
            }
            else
            {
                foreach (ReceHelp receHelp in receHelps)
                {
                    //应收明细实体
                    ReceDetailHelp receDetailHelp = new ReceDetailHelp();
                    receDetailHelp.ReceInvoice = receHelp.ReceInvoice;
                    receDetailHelp.ReceDocRate = receHelp.ReceDocRate;
                    receDetailHelp.ReceCreateDate = Convert.ToDateTime(receHelp.ReceCreateDate).ToString("yyyy.MM.dd");
                    receDetailHelp.InvoiceTotalAmount = receHelp.TotalAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelp.TotalAmount), 2);
                    receDetailHelp.RecePayType = receHelp.RecePayType;
                    receDetailHelp.ReceDocCur = "RMB";
                    receDetailHelp.ReceAmount = receHelp.ReceAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(receHelp.ReceAmount, 2);
                    receDetailHelp.ReceDate = receHelp.ReceDate;
                    receDetailHelp.ReceTypeDate = receHelp.ReceTypeDate;
                    if (receHelp.RecePayType == "预付/货前款")
                    {
                        receDetailHelp.ActualReceDate = "";
                        receDetailHelp.SaleReceiptNo = "";
                        receDetailHelp.ReceiveAmount = "";
                        receDetailHelp.NoReceiveAmount = "";
                        receDetailHelp.WithinLimitDay = "";
                        receDetailHelp.WithinLimitAmount = "";
                        receDetailHelp.WithinAmount = 0;
                        receDetailHelp.WithinLimitPre = "";
                        receDetailHelp.AccountRece = "未收款";
                    }
                    else
                    {
                        receDetailHelp.ActualReceDate = "";
                        receDetailHelp.SaleReceiptNo = "";
                        receDetailHelp.ReceiveAmount = "";
                        receDetailHelp.NoReceiveAmount = receDetailHelp.ReceAmount;
                        int days = (DateTime.Now - Convert.ToDateTime(receHelp.ReceDateTime)).Days;
                        receDetailHelp.WithinLimitDay = DateTime.Now <= Convert.ToDateTime(receHelp.ReceDateTime) ? "" : (days <= 0 ? "" : days.ToString());
                        receDetailHelp.WithinLimitAmount = receDetailHelp.WithinLimitDay == "" ? "" : receDetailHelp.ReceAmount;
                        receDetailHelp.WithinAmount = receDetailHelp.WithinLimitDay == "" ? 0 : (receHelp.ReceAmount == 0 ? 0 : receHelp.ReceAmount);
                        receDetailHelp.WithinLimitPre = receDetailHelp.WithinLimitDay == "" ? "0%" : "100%";
                        receDetailHelp.AccountRece = "未收款";
                    }

                    receDetailHelps.Add(receDetailHelp);
                }
            }

            return receDetailHelps;
        }

        /// <summary>
        /// 获取已逾期的销售订单号
        /// </summary>
        /// <returns>返回销售订单号</returns>
        public string GetDocEntrys()
        {
            List<int> docEntrys = new List<int>();
            var ordr = UnitWork.Find<ORDR>(r => r.CANCELED == "N").Select(r => new { r.DocEntry, r.GroupNum }).ToList();
            var crmoctg = UnitWork.Find<crm_octg>(r => r.sbo_id == 1).Select(r => new { r.GroupNum, r.PymntGroup }).ToList();
            var docEntryGroupNums = (from a in ordr
                                     join b in crmoctg on Convert.ToInt32(a.GroupNum) equals b.GroupNum into ab
                                     from b in ab.DefaultIfEmpty()
                                     select new DocEntryGroupNumHelp
                                     {
                                         DocEntry = a.DocEntry,
                                         GroupNum = b == null ? 0 : b.GroupNum,
                                         PymntGroup = b == null ? "" : b.PymntGroup
                                     }).ToList();

            //判定付款条件是否在4.0中存在
            var paytem = (from a in docEntryGroupNums
                          join b in UnitWork.Find<PayTermSave>(null).ToList() on a.PymntGroup equals b.GroupNum
                          select new DocEntryGroupNumHelp
                          {
                              DocEntry = a.DocEntry,
                              GroupNum = a.GroupNum,
                              PymntGroup = a.PymntGroup
                          }).ToList();

            if (paytem != null && paytem.Count() > 0)
            {
                foreach (DocEntryGroupNumHelp item in paytem)
                {
                    var groupNums = UnitWork.Find<PayTermSave>(r => r.GroupNum == item.PymntGroup).FirstOrDefault();
                    if (groupNums != null && groupNums.GroupNum != "")
                    {
                        SaleOrderDetailHelp saleOrderDetailHelp = (GetReceivableDetail(item.DocEntry, item.PymntGroup).Result).Data;
                        if (saleOrderDetailHelp != null)
                        {
                            if (saleOrderDetailHelp.SaleReceHelp != null)
                            {
                                if (saleOrderDetailHelp.SaleReceHelp.WithOutLimitAmount != "0.00")
                                {
                                    docEntrys.Add(Convert.ToInt32(saleOrderDetailHelp.SaleReceHelp.DocEntry));
                                }
                            }
                        }
                    }
                }
            }

            return string.Join(",", docEntrys);
        }

        /// <summary>
        /// 获取客户付款条件历史记录
        /// </summary>
        /// <param name="cardCode">客户编码</param>
        /// <returns>返回客户付款条件历史记录信息</returns>
        public async Task<TableData> GetCardCodePayTermHistory(string cardCode)
        {
            var result = new TableData();
            if (!string.IsNullOrEmpty(cardCode))
            {
                //查询sap客户付款条件历史记录
                List<int> oquts = await UnitWork.Find<OQUT>(r => r.CardCode == cardCode).Select(r => Convert.ToInt32(r.GroupNum)).ToListAsync();
                if (oquts != null && oquts.Count() > 0)
                {
                    //查询3.0付款条件
                    List<string> octgs = await UnitWork.Find<crm_octg>(r => oquts.Contains(r.GroupNum) && r.sbo_id == Define.SBO_ID).Select(r => r.PymntGroup).ToListAsync();
                    if (octgs != null && octgs.Count() > 0)
                    {
                        //查询4.0付款条件
                        var payterms = ((await UnitWork.Find<PayTermSave>(r => octgs.Contains(r.GroupNum)).Select(r => new { Id = r.Id, GroupNum = r.GroupNum, CreateTime = r.CreateTime }).ToListAsync()).OrderBy(r => r.CreateTime)).ToList();
                        if (payterms != null && payterms.Count() > 0)
                        {
                            result.Data = payterms;
                            result.Message = "获取成功";
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取付款条件Id
        /// </summary>
        /// <param name="groupNum">付款条件</param>
        /// <returns>返回条件Id</returns>
        public async Task<TableData> GetGroupNumId(string groupNum)
        {
            var result = new TableData();
            var crmOctgs = await UnitWork.Find<crm_octg>(r => r.PymntGroup == groupNum && r.sbo_id == Define.SBO_ID).ToListAsync();
            if (crmOctgs != null && crmOctgs.Count() > 0)
            {
                result.Data = new { Id = crmOctgs.FirstOrDefault().GroupNum, GroupNum = groupNum };
            }

            return result;
        }
        #endregion

        #region 常用比例
        /// <summary>
        /// 添加常用比例
        /// </summary>
        /// <param name="obj">添加常用比例实体</param>
        /// <returns>返回添加结果</returns>
        public async Task<TableData> AddUsedRate(AddPayUsedRate obj)
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //同步到4.0,sap,3.0
            result = await PayTermSave(obj.payTermSave);

            //事务创建常用比例
            var dbContext = UnitWork.GetDbContext<PayUserRate>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    PayUserRate payUserRate = new PayUserRate();
                    payUserRate.PayUsedRateName = obj.payUserRate.PayUsedRateName;
                    payUserRate.GroupNum = GetGroupName(obj.payTermSave);
                    payUserRate.CreateUserId = loginContext.User.Id;
                    payUserRate.CreateUserName = loginContext.User.Name;
                    payUserRate.CreateTime = DateTime.Now;
                    payUserRate = await UnitWork.AddAsync<PayUserRate, int>(payUserRate);
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                    result.Message = "创建成功";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("创建失败,请重试");
                }
            }

            return result;
        }

        /// <summary>
        /// 修改常用比例
        /// </summary>
        /// <param name="obj">修改常用比例实体</param>
        /// <returns>返回修改结果</returns>
        public async Task<TableData> UpdateUsedRate(AddPayUsedRate obj)
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //同步到4.0,sap,3.0
            result = await PayTermSave(obj.payTermSave);

            //事务修改常用比例
            var dbContext = UnitWork.GetDbContext<PayUserRate>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await UnitWork.UpdateAsync<PayUserRate>(r => r.Id == obj.payUserRate.Id, r => new PayUserRate()
                    {
                        PayUsedRateName = obj.payUserRate.PayUsedRateName,
                        GroupNum = GetGroupName(obj.payTermSave),
                        UpdateTime = DateTime.Now
                    });

                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                    result.Message = "修改成功";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("修改失败,请重试");
                }
            }

            return result;
        }

        /// <summary>
        /// 删除常用比例
        /// </summary>
        /// <param name="payUsedRateId">常用比例Id</param>
        /// <returns>返回删除结果</returns>
        public async Task<TableData> DeleteUsedRate(string payUsedRateId)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var payuseds = await UnitWork.Find<PayUserRate>(r => r.Id == payUsedRateId).ToListAsync();
            if (payuseds != null && payuseds.Count() > 0)
            {
                await UnitWork.DeleteAsync<PayUserRate>(r => r.Id == payUsedRateId);
                await UnitWork.SaveAsync();
                result.Message = "删除成功";
            }
            else
            {
                result.Code = 500;
                result.Message = "删除失败";
            }

            return result;
        }

        /// <summary>
        /// 获取常用比例列表
        /// </summary>
        /// <returns>返回列表信息</returns>
        public async Task<TableData> GetUsedRate()
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //查询常用比例
            var payUsedRates = await UnitWork.Find<PayUserRate>(null).ToListAsync();
            result.Data = payUsedRates;
            return result;
        }

        /// <summary>
        /// 获取常用比例详情
        /// </summary>
        /// <param name="payUsedRateId">常用比例Id</param>
        /// <returns>返回常用比例详情信息</returns>
        public async Task<TableData> GetUsedRateDetail(string payUsedRateId)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //查询常用比例
            PayUserRate payUsedRate = await UnitWork.Find<PayUserRate>(r => r.Id == payUsedRateId).FirstOrDefaultAsync();

            //查询付款条件
            PayTermSave payTermSave = await UnitWork.Find<PayTermSave>(r => r.GroupNum == payUsedRate.GroupNum).FirstOrDefaultAsync();
            if (payTermSave != null)
            {
                AddPayUsedRate addPayUsedRate = new AddPayUsedRate();
                addPayUsedRate.payUserRate = payUsedRate;
                addPayUsedRate.payTermSave = payTermSave;
                result.Data = addPayUsedRate;
                result.Message = "获取成功";
            }

            return result;
        }
        #endregion

        #region 付款条件限制规则
        /// <summary>
        /// 获取限制规则列表
        /// </summary>
        /// <returns>返回限制规则列表信息</returns>
        public async Task<TableData> GetPayLimitRule()
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var payLimitRules = await UnitWork.Find<PayLimitRule>(null).Include(r => r.PayLimitRuleDetails).ToListAsync();
            result.Data = payLimitRules;
            result.Message = "获取成功";
            return result;
        }

        /// <summary>
        /// 获取限制规则详情
        /// </summary>
        /// <param name="payLimitRuleId">限制规则Id</param>
        /// <returns>返回限制规则详情信息</returns>
        public async Task<TableData> GetPayLimitRuleDetail(string payLimitRuleId)
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var payLimitRule = await UnitWork.Find<PayLimitRule>(r => r.Id == payLimitRuleId).Include(r => r.PayLimitRuleDetails).ToListAsync();
            result.Data = payLimitRule;
            result.Message = "获取成功";
            return result;
        }

        /// <summary>
        /// 添加限制规则
        /// </summary>
        /// <param name="obj">限制规则实体</param>
        /// <returns>返回添加结果</returns>
        public async Task<TableData> AddPayLimitRule(PayLimitRule obj)
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //获取拼接名称
            result = await GetLimitRuleName(obj.PayLimitRuleDetails);
            if (result.Message != "ok")
            {
                result.Code = 500;
                return result;
            }

            //判定规则是否重复
            string limitRuleName = result.Data;
            var payLimitRules = await UnitWork.Find<PayLimitRule>(r => r.PayRulesName == limitRuleName).ToListAsync();
            if (payLimitRules != null && payLimitRules.Count() > 0)
            {
                result.Code = 500;
                result.Message = "已经存在该规则，不允许重复";
                return result;
            }

            //事务创建限制规则
            var dbContext = UnitWork.GetDbContext<PayLimitRule>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    obj.Id = obj.Id = Guid.NewGuid().ToString();
                    obj.PayRulesName = limitRuleName;
                    obj.CreateUserId = loginContext.User.Id;
                    obj.CreateUserName = loginContext.User.Name;
                    obj.CreateTime = DateTime.Now;
                    obj.UpdateTime = null;

                    //保存限制规则明细
                    if (obj.PayLimitRuleDetails.Count() > 0)
                    {
                        List<PayLimitRuleDetail> payLimitRuleDetails = new List<PayLimitRuleDetail>();
                        foreach (PayLimitRuleDetail item in obj.PayLimitRuleDetails)
                        {
                            if (string.IsNullOrEmpty(item.Contrast) || string.IsNullOrEmpty(item.Value))
                            {
                                result.Code = 500;
                                result.Message = "表达式或值不能为空";
                                return result;
                            }

                            item.Id = Guid.NewGuid().ToString();
                            item.PayLimitRuleId = obj.Id;
                            payLimitRuleDetails.Add(item);
                        }

                        if (payLimitRuleDetails != null && payLimitRuleDetails.Count > 0)
                        {
                            await UnitWork.BatchAddAsync<PayLimitRuleDetail>(payLimitRuleDetails.ToArray());
                        }
                    }

                    obj = await UnitWork.AddAsync<PayLimitRule, int>(obj);
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                    result.Message = "创建成功";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result.Code = 500;
                    result.Message = ex.Message.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// 修改限制规则
        /// </summary>
        /// <param name="obj">限制规则实体</param>
        /// <returns>返回修改结果</returns>
        public async Task<TableData> UpdatePayLimitRule(PayLimitRule obj)
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //获取拼接名称
            result = await GetLimitRuleName(obj.PayLimitRuleDetails);
            if (result.Message != "ok")
            {
                result.Code = 500;
                return result;
            }

            //判定规则是否重复
            string limitRuleName = result.Data;
            var payLimitRules = await UnitWork.Find<PayLimitRule>(r => r.PayRulesName == limitRuleName).ToListAsync();
            if (payLimitRules != null && payLimitRules.Count() > 1)
            {
                result.Code = 500;
                result.Message = "已经存在该规则，不允许重复";
                return result;
            }

            //事务创建限制规则
            var dbContext = UnitWork.GetDbContext<PayLimitRule>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    #region 删除
                    var payLimitRuleDetails = await UnitWork.Find<PayLimitRuleDetail>(r => r.PayLimitRuleId == obj.Id).ToListAsync();
                    if (payLimitRuleDetails != null && payLimitRuleDetails.Count() > 0)
                    {
                        foreach (PayLimitRuleDetail item in payLimitRuleDetails)
                        {
                            await UnitWork.DeleteAsync<PayLimitRuleDetail>(r => r.Id == item.Id);
                        }

                        await UnitWork.SaveAsync();
                    }
                    #endregion

                    #region 新增
                    //保存可用阶段
                    if (obj.PayLimitRuleDetails.Count() > 0)
                    {
                        List<PayLimitRuleDetail> payLimitRuleDetailList = new List<PayLimitRuleDetail>();
                        foreach (PayLimitRuleDetail item in obj.PayLimitRuleDetails)
                        {
                            if (string.IsNullOrEmpty(item.Contrast) || string.IsNullOrEmpty(item.Value))
                            {
                                result.Code = 500;
                                result.Message = "表达式或值不能为空";
                                return result;
                            }

                            item.Id = Guid.NewGuid().ToString();
                            item.PayLimitRuleId = obj.Id;
                            payLimitRuleDetailList.Add(item);
                        }

                        if (payLimitRuleDetailList != null && payLimitRuleDetailList.Count > 0)
                        {
                            await UnitWork.BatchAddAsync<PayLimitRuleDetail>(payLimitRuleDetailList.ToArray());
                        }
                    }

                    //清空旧数据
                    obj.PayLimitRuleDetails.Clear();
                    await UnitWork.SaveAsync();
                    #endregion

                    //修改主表数据
                    obj.PayRulesName = result.Data;
                    await UnitWork.UpdateAsync<PayLimitRule>(u => u.Id == obj.Id, u => new PayLimitRule()
                    {
                        UpdateTime = DateTime.Now,
                        PayRulesName = obj.PayRulesName,
                        IsUse = obj.IsUse,
                        IsOrigiCustomer = obj.IsOrigiCustomer,
                        PayPriority = obj.PayPriority,
                        CreateUserId = obj.CreateUserId,
                        CreateUserName = obj.CreateUserName,
                        CreateTime = obj.CreateTime
                    });

                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                    result.Message = "创建成功";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result.Code = 500;
                    result.Message = ex.Message.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// 删除限制规则
        /// </summary>
        /// <param name="payLimitRuleId">限制规则Id</param>
        /// <returns>返回删除结果</returns>
        public async Task<TableData> DeletePayLimitRule(string payLimitRuleId)
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var payLimitRules = await UnitWork.Find<PayLimitRule>(r => r.Id == payLimitRuleId).ToListAsync();
            if (payLimitRules != null && payLimitRules.Count() > 0)
            {
                var payLimitRuleDetails = await UnitWork.Find<PayLimitRuleDetail>(r => r.PayLimitRuleId == payLimitRuleId).ToListAsync();
                if (payLimitRuleDetails != null && payLimitRuleDetails.Count() > 0)
                {
                    await UnitWork.BatchDeleteAsync<PayLimitRuleDetail>(payLimitRuleDetails.ToArray());
                }

                await UnitWork.DeleteAsync<PayLimitRule>(r => r.Id == payLimitRuleId);
                await UnitWork.SaveAsync();
                result.Message = "删除成功";
            }
            else
            {
                result.Message = "删除失败，该限制规则不存在";
                result.Code = 500;
            }

            return result;
        }

        /// <summary>
        /// 获取规则名称
        /// </summary>
        /// <param name="payLimitRuleDetails">限制规则明细集合</param>
        /// <returns>返回获取规则名称结果</returns>
        public async Task<TableData> GetLimitRuleName(List<PayLimitRuleDetail> payLimitRuleDetails)
        {
            var result = new TableData();
            string ruleName = "如果：";
            int left = 0;
            int right = 0;

            //如果条件名称拼接
            List<PayLimitRuleDetail> conditionIf = (payLimitRuleDetails.Where(r => r.RuleType == 1).OrderBy(r => r.RuleNum)).ToList();
            if (conditionIf != null && conditionIf.Count() > 0)
            {
                if ((conditionIf.Last()).AndOr == "")
                {
                    //左括号总数与右括号总数相等
                    if (conditionIf.Count(r => r.BracketLeft == "(") == conditionIf.Count(r => r.BracketRight == ")"))
                    {
                        foreach (PayLimitRuleDetail item in conditionIf)
                        {
                            if (string.IsNullOrEmpty(item.Key) || string.IsNullOrEmpty(item.Contrast) || string.IsNullOrEmpty(item.Value))
                            {
                                result.Message = "付款条件不符合规则，请重试";
                                break;
                            }

                            if (item.BracketLeft == "(")
                            {
                                left = left + 1;
                            }

                            if (item.BracketRight == ")")
                            {
                                right = right + 1;
                            }

                            //当前左括号数量大于等于当前右括号数量，符合条件，开始"如果"拼接
                            if (left >= right)
                            {
                                ruleName = ruleName + item.BracketLeft;
                                switch (item.Key)
                                {
                                    case "U_CardTypeStr":
                                        ruleName = ruleName + "客户类型" + (item.Contrast == "IsType" ? "属于" : "不属于");
                                        if (item.Value.Contains(","))
                                        {
                                            List<string> ids = item.Value.Split(',').ToList();
                                            List<string> names = new List<string>();
                                            foreach (string id in ids)
                                            {
                                                names.Add((await UnitWork.Find<ClueClassification>(r => r.Id == Convert.ToInt32(id)).FirstOrDefaultAsync()).Name);
                                            }

                                            ruleName = ruleName + string.Join("、", names);
                                        }
                                        else
                                        {
                                            ruleName = ruleName + (await UnitWork.Find<ClueClassification>(r => r.Id == Convert.ToInt32(item.Value)).FirstOrDefaultAsync()).Name;
                                        }

                                        ruleName = ruleName + item.BracketRight + (item.AndOr == "and" ? " 并且 " : (item.AndOr == "or" ? " 或者 " : "；"));
                                        break;
                                    case "Flag":
                                        ruleName = ruleName + "中间商" + (item.Contrast == "IsFlag" ? "为" : "不为") + (item.Value == "yes" ? "是" : "否") + item.BracketRight + item.BracketRight + (item.AndOr == "and" ? " 并且 " : (item.AndOr == "or" ? " 或者 " : "；"));
                                        break;
                                    case "DocTotal":
                                        ruleName = ruleName + "订单金额（万元）" + CommonMethodHelp.GetContrast(item.Contrast) + item.Value + item.BracketRight + (item.AndOr == "and" ? " 并且 " : (item.AndOr == "or" ? " 或者 " : "；"));
                                        break;
                                    case "CardCode":
                                        ruleName = ruleName + "客户代码" + (item.Contrast == "IsCode" ? "属于" : "不属于") + item.Value + item.BracketRight + (item.AndOr == "and" ? " 并且 " : (item.AndOr == "or" ? " 或者 " : "；"));
                                        break;
                                }
                            }
                        }

                        ruleName = ruleName + "那么：";
                    }
                    else
                    {
                        result.Message = "付款条件不符合规则，请重试";
                    }
                }
                else
                {
                    result.Message = "付款条件不符合规则，请重试";
                }
            }
            else
            {
                result.Message = "付款条件不符合规则，请重试";
            }

            //那么条件名称拼接
            List<PayLimitRuleDetail> conditionThen = (payLimitRuleDetails.Where(r => r.RuleType == 2).OrderBy(r => r.RuleNum)).ToList();
            if (conditionThen != null && conditionThen.Count() > 0)
            {
                if ((conditionThen.Last()).AndOr == "")
                {
                    //左括号总数与右括号总数相等
                    if (conditionThen.Count(r => r.BracketLeft == "(") == conditionThen.Count(r => r.BracketRight == ")"))
                    {
                        foreach (PayLimitRuleDetail item in conditionThen)
                        {
                            if (string.IsNullOrEmpty(item.Key) || string.IsNullOrEmpty(item.Contrast) || string.IsNullOrEmpty(item.Value))
                            {
                                result.Message = "付款条件不符合规则，请重试";
                                break;
                            }

                            if (item.BracketLeft == "(")
                            {
                                left = left + 1;
                            }

                            if (item.BracketRight == ")")
                            {
                                right = right + 1;
                            }

                            //当前左括号数量大于等于当前右括号数量，符合条件，开始"那么"拼接
                            if (left >= right)
                            {
                                ruleName = ruleName + item.BracketLeft + "预付货前比例之和" + CommonMethodHelp.GetContrast(item.Contrast) + item.Value + "%" + item.BracketRight + (item.AndOr == "and" ? " 并且 " : (item.AndOr == "or" ? " 或者 " : ""));
                            }
                        }

                        result.Data = ruleName;
                        result.Message = "ok";
                    }
                    else
                    {
                        result.Message = "付款条件不符合规则，请重试";
                    }
                }
            }
            else
            {
                result.Message = "付款条件不符合规则，请重试";
            }

            return result;
        }
        #endregion

        #region 自动冻结规则
        /// <summary>
        /// 获取自动冻结信息
        /// </summary>
        /// <returns>返回自动冻结信息</returns>
        public async Task<TableData> GetAutoFreeze()
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var payLimitRules = await UnitWork.Find<PayAutoFreeze>(null).ToListAsync();
            result.Data = payLimitRules;
            result.Message = "获取成功";
            return result;
        }

        /// <summary>
        /// 自动冻结规则添加
        /// </summary>
        /// <param name="objs">自动冻结实体集合</param>
        /// <returns>返回添加结果</returns>
        public async Task<TableData> AddAutoFreeze(List<PayAutoFreeze> objs)
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            if (objs != null && objs.Count() > 0)
            {
                List<PayAutoFreeze> payAutoFreezes = new List<PayAutoFreeze>();
                foreach (PayAutoFreeze item in objs)
                {
                    payAutoFreezes.Add(new PayAutoFreeze()
                    {
                        ModelTypeId = item.ModelTypeId,
                        ModelTypeName = item.ModelTypeName,
                        Number = item.Number,
                        DataNumber = item.DataNumber,
                        DataFormat = item.DataFormat,
                        IsUse = item.IsUse,
                        CreateUserId = loginContext.User.Id,
                        CreateUserName = loginContext.User.Name,
                        CreateTime = DateTime.Now,
                        UpdateTime = null
                    });
                }

                await UnitWork.BatchAddAsync<PayAutoFreeze>(payAutoFreezes.ToArray());
                await UnitWork.SaveAsync();
                result.Message = "创建成功";
            }
            else
            {
                result.Code = 500;
                result.Message = "创建失败";
            }

            return result;
        }

        /// <summary>
        /// 修改自动冻结
        /// </summary>
        /// <param name="objs">自动冻结实体集合</param>
        /// <returns>返回修改结果</returns>
        public async Task<TableData> UpdateAutoFreeze(List<PayAutoFreeze> objs)
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            if (objs != null && objs.Count() > 0)
            {
                List<PayAutoFreeze> payAutoFreezes = new List<PayAutoFreeze>();
                foreach (PayAutoFreeze item in objs)
                {
                    payAutoFreezes.Add(new PayAutoFreeze()
                    {
                        Id = item.Id,
                        ModelTypeId = item.ModelTypeId,
                        ModelTypeName = item.ModelTypeName,
                        Number = item.Number,
                        DataNumber = item.DataNumber,
                        DataFormat = item.DataFormat,
                        IsUse = item.IsUse,
                        CreateUserId = item.CreateUserId,
                        CreateUserName = item.CreateUserName,
                        CreateTime = item.CreateTime,
                        UpdateTime = DateTime.Now
                    });
                }

                await UnitWork.BatchUpdateAsync<PayAutoFreeze>(payAutoFreezes.ToArray());
                await UnitWork.SaveAsync();
                result.Message = "修改成功";
            }
            else
            {
                result.Code = 500;
                result.Message = "修改失败";
            }

            return result;
        }

        /// <summary>
        /// 删除自动冻结
        /// </summary>
        /// <param name="payAutoFreezeIds">自动冻结Id集合</param>
        /// <returns>返回删除结果</returns>
        public async Task<TableData> DeleteAutoFreeze(List<string> payAutoFreezeIds)
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            List<PayAutoFreeze> payAutoFreezes = await UnitWork.Find<PayAutoFreeze>(r => payAutoFreezeIds.Contains(r.Id)).ToListAsync();
            await UnitWork.BatchDeleteAsync<PayAutoFreeze>(payAutoFreezes.ToArray());
            await UnitWork.SaveAsync();
            result.Message = "删除成功";
            return result;
        }

        /// <summary>
        /// 自动冻结解冻任务
        /// </summary>
        /// <returns></returns>
        public async Task SetAutoFreezeJob()
        {
            var result = new TableData();

            try
            {
                //查询自动冻结时间
                var payautofreezes = await UnitWork.Find<PayAutoFreeze>(r => r.ModelTypeId == 3).ToListAsync();
                if (payautofreezes != null && payautofreezes.Count() > 0)
                {
                    //每天自动冻结时间
                    DateTime freezeDateTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " " + payautofreezes.Where(r => r.Number == 1).FirstOrDefault().DataFormat);

                    //冻结前将要冻结的客户提醒业务员时间
                    int freezeDate = (int)payautofreezes.Where(r => r.Number == 2).FirstOrDefault().DataNumber;
                    DateTime date = Convert.ToDateTime(DateTime.Now.AddDays(freezeDate).ToString("yyyy-MM-dd") + " " + payautofreezes.Where(r => r.Number == 2).FirstOrDefault().DataFormat);
                    List<RuleCustomer> ruleCustomers = new List<RuleCustomer>();

                    //获取符合冻结比例规则的客户
                    result = await GetAutoFreezeProList();
                    if (result.Message == "ok")
                    {
                        ruleCustomers.InsertRange(ruleCustomers.Count(), result.Data);
                    }

                    //获取符合冻结金额规则的客户
                    result = await GetAutoFreezeAmountList();
                    if (result.Message == "ok")
                    {
                        ruleCustomers.InsertRange(ruleCustomers.Count(), result.Data);
                    }

                    //符合规则的客户
                    ruleCustomers = ruleCustomers.GroupBy(r => new { r.CardCode, r.CardName }).Select(r => new RuleCustomer { CardCode = r.Key.CardCode, CardName = r.Key.CardName }).ToList();

                    //查询已经冻结的客户
                    var freezeCustomerAllList = await UnitWork.Find<PayFreezeCustomer>(null).ToListAsync();
                    List<RuleCustomer> freezeCustomers = freezeCustomerAllList.GroupBy(r => new { r.CardCode, r.CardName }).Select(r => new RuleCustomer { CardCode = r.Key.CardCode, CardName = r.Key.CardName }).ToList();

                    //查询即将冻结的客户
                    var willFreezeAllCustomers = await UnitWork.Find<PayWillFreezeCustomer>(null).ToListAsync();
                    List<RuleCustomer> willFreezeCustomers = willFreezeAllCustomers.GroupBy(r => new { r.CardCode, r.CardName }).Select(r => new RuleCustomer { CardCode = r.Key.CardCode, CardName = r.Key.CardName }).ToList();

                    //查询到所有已冻结和将要冻结的客户
                    freezeCustomers.InsertRange(freezeCustomers.Count(), willFreezeCustomers);
                    freezeCustomers = freezeCustomers.GroupBy(r => new { r.CardCode, r.CardName }).Select(r => new RuleCustomer { CardCode = r.Key.CardCode, CardName = r.Key.CardName }).ToList();

                    #region 开始自动冻结
                    //将已在冻结和将要冻结表中的客户排除
                    List<RuleCustomer> ruleCustomerExcepts = ruleCustomers.Where(r => !freezeCustomers.Any(x => x.CardCode == r.CardCode && x.CardName == r.CardName)).ToList();

                    //查询vip客户
                    List<RuleCustomer> vipCustomers = await UnitWork.Find<PayVIPCustomer>(null).GroupBy(r => new { r.CardCode, r.CardName }).Select(r => new RuleCustomer { CardCode = r.Key.CardCode, CardName = r.Key.CardName }).ToListAsync();

                    //符合规则客户的掉入即将冻结表
                    List<PayWillFreezeCustomer> payWillFreezeCustomers = new List<PayWillFreezeCustomer>();
                    foreach (RuleCustomer item in ruleCustomerExcepts)
                    {
                        //VIP客户不调入即将冻结
                        if (!vipCustomers.Exists(r => r.CardCode == item.CardCode))
                        {
                            int? slpCode = await UnitWork.Find<OCRD>(r => r.CardCode == item.CardCode).Select(r => r.SlpCode).FirstOrDefaultAsync();
                            if (slpCode != null)
                            {
                                string slpName = await UnitWork.Find<OSLP>(r => r.SlpCode == slpCode).Select(r => r.SlpName).FirstOrDefaultAsync();
                                payWillFreezeCustomers.Add(new PayWillFreezeCustomer()
                                {
                                    SaleId = slpCode.ToString(),
                                    SaleName = slpName,
                                    CardCode = item.CardCode,
                                    CardName = item.CardName,
                                    FreezeDateTime = date,
                                    Remark = "满足自动冻结规则，并且不属于VIP客户，拉入即将冻结客户",
                                    CreateUserId = "00000000-0000-0000-0000-000000000000",
                                    CreateUserName = "超级管理员",
                                    CreateTime = DateTime.Now
                                });
                            }
                            else
                            {
                                payWillFreezeCustomers.Add(new PayWillFreezeCustomer()
                                {
                                    SaleId = "0",
                                    SaleName = "",
                                    CardCode = item.CardCode,
                                    CardName = item.CardName,
                                    FreezeDateTime = date,
                                    Remark = "满足自动冻结规则，并且不属于VIP客户，拉入即将冻结客户",
                                    CreateUserId = "00000000-0000-0000-0000-000000000000",
                                    CreateUserName = "超级管理员",
                                    CreateTime = DateTime.Now
                                });
                            }
                        }
                    }

                    if (payWillFreezeCustomers != null && payWillFreezeCustomers.Count() > 0)
                    {
                        await UnitWork.BatchAddAsync<PayWillFreezeCustomer>(payWillFreezeCustomers.ToArray());
                        await UnitWork.SaveAsync();
                    }

                    //冻结时间内即将冻结的客户进行冻结
                    List<PayWillFreezeCustomer> willFreezeCustomerAgains = await UnitWork.Find<PayWillFreezeCustomer>(null).ToListAsync();
                    List<PayWillFreezeCustomer> willFreezeCustomerRemove = new List<PayWillFreezeCustomer>();
                    if (willFreezeCustomerAgains != null && willFreezeCustomerAgains.Count() > 0)
                    {
                        List<PayFreezeCustomer> payFreezeCustomers = new List<PayFreezeCustomer>();
                        foreach (PayWillFreezeCustomer item in willFreezeCustomerAgains)
                        {
                            if (DateTime.Now >= item.FreezeDateTime)
                            {
                                int? slpCode = await UnitWork.Find<OCRD>(r => r.CardCode == item.CardCode).Select(r => r.SlpCode).FirstOrDefaultAsync();
                                if (slpCode != null)
                                {
                                    string slpName = await UnitWork.Find<OSLP>(r => r.SlpCode == slpCode).Select(r => r.SlpName).FirstOrDefaultAsync();
                                    payFreezeCustomers.Add(new PayFreezeCustomer()
                                    {
                                        CardCode = item.CardCode,
                                        CardName = item.CardName,
                                        SaleId = slpCode.ToString(),
                                        SaleName = slpName,
                                        FreezeCause = "满足自动冻结规则，并且不属于VIP客户，拉入冻结客户",
                                        IsAutoThaw = true,
                                        FreezeStartTime = null,
                                        FreezeEndTime = null,
                                        SendCount = 0,
                                        ThawStartTime = null,
                                        ThawEndTime = null,
                                        ListName = "冻结客户",
                                        FreezeType = 1,
                                        CreateUserId = "00000000-0000-0000-0000-000000000000",
                                        CreateUserName = "超级管理员",
                                        CreateTime = DateTime.Now
                                    });
                                }
                                else
                                {
                                    payFreezeCustomers.Add(new PayFreezeCustomer()
                                    {
                                        CardCode = item.CardCode,
                                        CardName = item.CardName,
                                        SaleId = "0",
                                        SaleName = "",
                                        FreezeCause = "满足自动冻结规则，并且不属于VIP客户，拉入冻结客户",
                                        IsAutoThaw = true,
                                        FreezeStartTime = null,
                                        FreezeEndTime = null,
                                        SendCount = 0,
                                        ThawStartTime = null,
                                        ThawEndTime = null,
                                        ListName = "冻结客户",
                                        FreezeType = 1,
                                        CreateUserId = "00000000-0000-0000-0000-000000000000",
                                        CreateUserName = "超级管理员",
                                        CreateTime = DateTime.Now
                                    });
                                }

                                willFreezeCustomerRemove.Add(item);
                            }
                        }

                        //批量添加需要冻结的客户
                        if (payFreezeCustomers != null && payFreezeCustomers.Count() > 0)
                        {
                            await UnitWork.BatchAddAsync<PayFreezeCustomer>(payFreezeCustomers.ToArray());
                            await UnitWork.SaveAsync();
                        }

                        //批量移除将要冻结的客户
                        if (willFreezeCustomerRemove != null && willFreezeCustomerRemove.Count() > 0)
                        {
                            await UnitWork.BatchDeleteAsync<PayWillFreezeCustomer>(willFreezeCustomerRemove.ToArray());
                            await UnitWork.SaveAsync();
                        }
                    }
                    #endregion

                    #region 自动解冻
                    //查询需要自动解冻的客户
                    List<PayFreezeCustomer> autoThawNeedCustomers = freezeCustomerAllList.Where(r => r.IsAutoThaw == true && r.ThawEndTime != null && r.ThawStartTime != null && ((r.ThawStartTime <= DateTime.Now && r.ThawEndTime >= DateTime.Now))).ToList();

                    //移除在自动冻结规则中的客户
                    List<PayFreezeCustomer> autoThawNeedDelete = autoThawNeedCustomers.Where(r => !ruleCustomers.Any(x => x.CardCode == r.CardCode && x.CardName == r.CardName)).ToList();
                    var willFreezeAllCustomerLists = await UnitWork.Find<PayWillFreezeCustomer>(null).ToListAsync();
                    List<PayWillFreezeCustomer> willFreezeAllDelete = willFreezeAllCustomerLists.Where(r => !ruleCustomers.Any(x => x.CardCode == r.CardCode && x.CardName == r.CardName)).ToList();
                    await UnitWork.BatchDeleteAsync<PayFreezeCustomer>(autoThawNeedDelete.ToArray());
                    await UnitWork.BatchDeleteAsync<PayWillFreezeCustomer>(willFreezeAllDelete.ToArray());
                    await UnitWork.SaveAsync();
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error("自动冻结失败：" + ex.Message.ToString());
            }
        }

        /// <summary>
        /// 钉钉推送即将冻结客户和已经冻结客户给业务员
        /// </summary>
        /// <returns></returns>
        public async Task SetDDSendMsgJob()
        {
            //查询即将冻结客户
            List<DDSendSale> willFreezeCurtomers = await UnitWork.Find<PayWillFreezeCustomer>(null).Select(r => new DDSendSale { CardCode = r.CardCode, CardName = r.CardName, FreezeDateTime = r.FreezeDateTime.ToString("yyyy-MM-dd") }).ToListAsync();

            //查询已经冻结客户
            List<DDSendSale> freezeCustomers = await UnitWork.Find<PayFreezeCustomer>(r => r.SendCount <= 0).Select(r => new DDSendSale { CardCode = r.CardCode, CardName = r.CardName, FreezeDateTime = r.CreateTime.ToString("yyyy-MM-dd") }).ToListAsync();

            #region 钉钉推送即将冻结的客户给业务员
            if (willFreezeCurtomers != null && willFreezeCurtomers.Count() > 0)
            {
                try
                {
                    //查询客户对应的业务员编码
                    var slpCodes1 = await UnitWork.Find<crm_ocrd>(r => r.sbo_id == Define.SBO_ID && (freezeCustomers.Select(x => x.CardCode).ToList()).Contains(r.CardCode) && r.SlpCode != null).Select(r => new CrmOcrdHelp { SlpCode = (int)r.SlpCode, CardCode = r.CardCode }).ToListAsync();
                    if (slpCodes1 != null && slpCodes1.Count() > 0)
                    {
                        //查询业务员绑定的3.0 userId
                        var userIds1 = await UnitWork.Find<sbo_user>(r => r.sbo_id == Define.SBO_ID && (slpCodes1.Select(x => x.SlpCode).ToList()).Contains((int)r.sale_id)).Select(r => new SboUserHelp { user_id = (int)r.user_id, sale_id = (int)r.sale_id }).ToListAsync();
                        if (userIds1 != null && userIds1.Count() > 0)
                        {
                            //查询3.0 userId绑定的4.0 userId
                            var users1 = await UnitWork.Find<User>(r => (userIds1.Select(r => r.user_id).ToList()).Contains((int)r.User_Id)).Select(r => new UserHelp { Id = r.Id, User_Id = (int)r.User_Id }).ToListAsync();
                            if (users1 != null && users1.Count() > 0)
                            {
                                //查询绑定的钉钉用户Id
                                var ddBindUsers = await UnitWork.Find<DDBindUser>(r => (users1.Select(r => r.Id).ToList()).Contains(r.UserId)).ToListAsync();
                                var ddSendSales = (from a in willFreezeCurtomers
                                                   join b in slpCodes1 on a.CardCode equals b.CardCode
                                                   join c in userIds1 on b.SlpCode equals c.sale_id
                                                   join d in users1 on c.user_id equals d.User_Id
                                                   join e in ddBindUsers on d.Id equals e.UserId
                                                   select new DDSendSale
                                                   {
                                                       CardCode = a.CardCode,
                                                       CardName = a.CardName,
                                                       FreezeDateTime = a.FreezeDateTime,
                                                       DDUserId = e == null ? "" : e.DDUserId
                                                   }).ToList();

                                List<string> ddUsers = ddSendSales.GroupBy(r => new { r.DDUserId }).Select(r => r.Key.DDUserId).ToList();
                                foreach (string item in ddUsers)
                                {
                                    List<DDSendSale> cardCodes = ddSendSales.Where(r => r.DDUserId == item).ToList();
                                    if (cardCodes != null && cardCodes.Count() > 0)
                                    {
                                        StringBuilder remarks = new StringBuilder();
                                        foreach (DDSendSale dDSendSale in cardCodes)
                                        {
                                            remarks.Append("客户编码为" + dDSendSale.CardCode + "的客户 " + dDSendSale.CardName + ",将在" + dDSendSale.FreezeDateTime + " 即将冻结 ");
                                        }

                                        await _dDVoice.DDSendMsg("text", remarks.ToString(), item);
                                    }
                                    else
                                    {
                                        //钉钉推送操作历史记录
                                        await UnitWork.AddAsync<DDSendMsgHitory>(new DDSendMsgHitory()
                                        {
                                            MsgType = "钉钉推送即将冻结客户给业务员",
                                            MsgContent = "即将冻结客户推送给业务员失败",
                                            MsgResult = "失败",
                                            CreateName = "超级管理员",
                                            CreateUserId = "00000000-0000-0000-0000-000000000000",
                                            CreateTime = DateTime.Now
                                        });

                                        await UnitWork.SaveAsync();
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("钉钉推送即将冻结客户失败：" + ex.Message.ToString());
                }
            }
            #endregion

            #region 钉钉推送已经冻结的客户给业务员
            if (freezeCustomers != null && freezeCustomers.Count() > 0)
            {
                try
                {
                    //查询客户对应的业务员编码
                    List<CrmOcrdHelp> slpCodes = UnitWork.Find<crm_ocrd>(r => r.sbo_id == Define.SBO_ID && (freezeCustomers.Select(x => x.CardCode).ToList()).Contains(r.CardCode) && r.SlpCode != null).Select(r => new CrmOcrdHelp { SlpCode = (int)r.SlpCode, CardCode = r.CardCode }).ToList();
                    if (slpCodes != null && slpCodes.Count() > 0)
                    {
                        //查询业务员绑定的3.0 userId
                        var userIds = await UnitWork.Find<sbo_user>(r => r.sbo_id == Define.SBO_ID && (slpCodes.Select(x => x.SlpCode).ToList()).Contains((int)r.sale_id)).Select(r => new SboUserHelp { user_id = (int)r.user_id, sale_id = (int)r.sale_id }).ToListAsync();
                        if (userIds != null && userIds.Count() > 0)
                        {
                            //查询3.0 userId绑定的4.0 userId
                            var users = await UnitWork.Find<User>(r => (userIds.Select(r => r.user_id).ToList()).Contains((int)r.User_Id)).Select(r => new UserHelp { Id = r.Id, User_Id = (int)r.User_Id }).ToListAsync();
                            if (users != null && users.Count() > 0)
                            {
                                //查询绑定的钉钉用户Id
                                var ddBindUsers = await UnitWork.Find<DDBindUser>(r => (users.Select(r => r.Id).ToList()).Contains(r.UserId)).ToListAsync();
                                var ddSendSales = (from a in freezeCustomers
                                                   join b in slpCodes on a.CardCode equals b.CardCode
                                                   join c in userIds on b.SlpCode equals c.sale_id
                                                   join d in users on c.user_id equals d.User_Id
                                                   join e in ddBindUsers on d.Id equals e.UserId
                                                   select new DDSendSale
                                                   {
                                                       CardCode = a.CardCode,
                                                       CardName = a.CardName,
                                                       FreezeDateTime = a.FreezeDateTime,
                                                       DDUserId = e == null ? "" : e.DDUserId
                                                   }).ToList();

                                List<string> ddUsers = ddSendSales.GroupBy(r => new { r.DDUserId }).Select(r => r.Key.DDUserId).ToList();
                                List<string> cardCodeLists = new List<string>();
                                foreach (string item in ddUsers)
                                {
                                    if (!string.IsNullOrEmpty(item))
                                    {
                                        List<DDSendSale> cardCodes = ddSendSales.Where(r => r.DDUserId == item).ToList();
                                        if (cardCodes != null && cardCodes.Count() > 0)
                                        {
                                            StringBuilder remarks = new StringBuilder();
                                            foreach (DDSendSale dDSendSale in cardCodes)
                                            {
                                                remarks.Append("客户编码为" + dDSendSale.CardCode + "的客户 " + dDSendSale.CardName + ",在" + dDSendSale.FreezeDateTime + "已冻结 ");
                                                cardCodeLists.Add(dDSendSale.CardCode);
                                            }

                                            await _dDVoice.DDSendMsg("text", remarks.ToString(), item);
                                        }
                                        else
                                        {
                                            //钉钉推送操作历史记录
                                            await UnitWork.AddAsync<DDSendMsgHitory>(new DDSendMsgHitory()
                                            {
                                                MsgType = "钉钉推送即将冻结客户给业务员",
                                                MsgContent = "已经冻结客户推送给业务员失败",
                                                MsgResult = "失败",
                                                CreateName = "超级管理员",
                                                CreateUserId = "00000000-0000-0000-0000-000000000000",
                                                CreateTime = DateTime.Now
                                            });

                                            await UnitWork.SaveAsync();
                                        }
                                    }
                                }

                                //将已经发送通知的冻结客户发送次数更改为1
                                if (cardCodeLists.Count() > 0)
                                {
                                    foreach (string item in cardCodeLists)
                                    {
                                        await UnitWork.UpdateAsync<PayFreezeCustomer>(r => r.CardCode == item, r => new PayFreezeCustomer()
                                        {
                                            SendCount = 1
                                        });
                                    }

                                    await UnitWork.SaveAsync();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("钉钉推送已冻结客户失败：" + ex.Message.ToString());
                }
            }
            #endregion
        }

        /// <summary>
        /// 获取自动冻结比例规则用户
        /// </summary>
        /// <returns>返回客户信息</returns>
        public async Task<TableData> GetAutoFreezeProList()
        {
            var result = new TableData();
            var payautofreee = await UnitWork.Find<PayAutoFreeze>(r => r.ModelTypeId == 2 && r.IsUse == true).ToListAsync();
            string filedStrig = "1=1 ";
            if (payautofreee != null && payautofreee.Count() > 0)
            {
                filedStrig = filedStrig + " AND T.DuePercent " + payautofreee.FirstOrDefault().DataFormat + ((payautofreee.FirstOrDefault().DataNumber) / 100);
            }
            else
            {
                return result;
            }

            //获取客户总数
            List<RuleCustomer> ruleCustomers = GetClientList(1, 1, "1=1", out int rowCount);
            List<RuleCustomer> ruleCustomerList = new List<RuleCustomer>();

            //客户500为一组进行分组循环查询
            int totalPage = Convert.ToInt32((rowCount / 500)) + 1;
            for (int i = 0; i < totalPage; i++)
            {
                ruleCustomerList.InsertRange(ruleCustomerList.Count(), GetClientList(500, i + 1, filedStrig, out int rowCounts));
            }

            if (ruleCustomerList != null && ruleCustomerList.Count() > 0)
            {
                result.Data = ruleCustomerList;
                result.Message = "ok";
            }

            return result;
        }

        /// <summary>
        /// 获取符合自动冻结金额规则客户
        /// </summary>
        /// <returns>返回客户信息</returns>
        public async Task<TableData> GetAutoFreezeAmountList()
        {
            var result = new TableData();

            //查询自动冻结
            List<PayAutoFreeze> autoFreezes = await UnitWork.Find<PayAutoFreeze>(r => r.ModelTypeId == 1 && r.IsUse == true).OrderBy(r => r.Number).ToListAsync();
            if (autoFreezes != null && autoFreezes.Count() > 0)
            {
                List<string> ocrds = await UnitWork.Find<OCRD>(null).Select(r => r.CardCode).ToListAsync();
                if (ocrds != null && ocrds.Count() > 0)
                {
                    List<RuleCustomer> ruleCustomers = new List<RuleCustomer>();
                    int totalPage = Convert.ToInt32((ocrds.Count() / 500)) + 1;

                    //已交货未完全回款时间
                    string startDateTimeUnit = autoFreezes.Where(r => r.Number == 1).Select(r => r.DataFormat).FirstOrDefault();
                    int startNumber = autoFreezes.Where(r => r.Number == 1).Select(r => (int)r.DataNumber).FirstOrDefault();
                    DateTime startTime = startDateTimeUnit == "Y" ? DateTime.Now.AddYears(-startNumber) : DateTime.Now.AddMonths(-startNumber);

                    //时间段内没有回款记录
                    string endDateTimeUnit = autoFreezes.Where(r => r.Number == 2).Select(r => r.DataFormat).FirstOrDefault();
                    int endNumber = autoFreezes.Where(r => r.Number == 2).Select(r => (int)r.DataNumber).FirstOrDefault();
                    DateTime endTime = endDateTimeUnit == "Y" ? DateTime.Now.AddYears(-endNumber) : DateTime.Now.AddMonths(-endNumber);

                    //欠款金额表达式
                    string key = autoFreezes.Where(r => r.Number == 3).Select(r => r.DataFormat).FirstOrDefault();

                    //欠款金额
                    decimal amount = autoFreezes.Where(r => r.Number == 3).Select(r => (decimal)r.DataNumber).FirstOrDefault();

                    //客户500为一组进行分组循环查询
                    for (int i = 0; i < totalPage; i++)
                    {
                        List<string> ocrdCards = ocrds.Skip(i * 500).Take(500).ToList();
                        result = await GetAutoRuleCustomer(ocrdCards, startTime, endTime, amount, key);//查询符合冻结规则的客户
                        if (result.Message == "ok")
                        {
                            ruleCustomers.InsertRange(ruleCustomers.Count(), result.Data);
                        }
                        else
                        {
                            ruleCustomers.Clear();
                            break;
                        }
                    }

                    if (ruleCustomers != null && ruleCustomers.Count() > 0)
                    {
                        result.Data = ruleCustomers;
                        result.Message = "ok";
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取符合冻结规则的客户
        /// </summary>
        /// <param name="ocrdCards">客户编码集合</param>
        /// <param name="startTime">已交货未回款时间</param>
        /// <param name="endTime">时间段内没有任何回款记录</param>
        /// <param name="amount">欠款金额</param>
        /// <param name="key">表达式</param>
        /// <returns>返回符合冻结规则客户信息集合</returns>
        public async Task<TableData> GetAutoRuleCustomer(List<string> ocrdCards, DateTime startTime, DateTime endTime, decimal amount, string key)
        {
            var result = new TableData();
            try
            {
                //查询客户对应的销售订单
                List<PayAutoFreezeRule> ordrs = await UnitWork.Find<ORDR>(r => ocrdCards.Contains(r.CardCode) && r.CANCELED == "N").Select(r => new PayAutoFreezeRule
                {
                    DocEntry = r.DocEntry,
                    DocTotal = r.DocTotal,
                    DocTotalFC = r.DocTotalFC,
                    CardCode = r.CardCode,
                    CardName = r.CardName
                }).ToListAsync();

                //查询销售订单对应的交货单明细
                List<RuleDLN1> dln1DocEntrys = await UnitWork.Find<DLN1>(r => (ordrs.Select(x => x.DocEntry).ToList()).Contains((int)r.BaseEntry)).Select(r => new RuleDLN1
                {
                    DocEntry = (int)r.DocEntry,
                    BaseEntry = (int)r.BaseEntry
                }).ToListAsync();

                //查询交货单明细对应的销售交货单
                var odlns = await UnitWork.Find<ODLN>(r => (dln1DocEntrys.Select(r => r.DocEntry).ToList()).Contains((int)r.DocEntry) && r.CANCELED == "N" && r.CreateDate <= startTime).Select(r => new PayAutoFreezeRule
                {
                    DocEntry = (int)r.DocEntry,
                    DocTotal = r.DocTotal,
                    DocTotalFC = r.DocTotalFC,
                    CardCode = r.CardCode,
                    CardName = r.CardName
                }).ToListAsync();

                //联合查询销售交货单
                List<RuleDLN1> odlnSums = (from a in odlns
                                           join b in dln1DocEntrys on a.DocEntry equals b.DocEntry into ab
                                           from b in ab.DefaultIfEmpty()
                                           select new RuleDLN1
                                           {
                                               DocEntry = a.DocEntry,
                                               DocTotal = a.DocTotal,
                                               DocTotalFC = a.DocTotalFC,
                                               CardCode = a.CardCode,
                                               CardName = a.CardName,
                                               BaseEntry = b == null ? 0 : b.BaseEntry
                                           }).ToList();

                //统计销售交货单总金额
                List<RuleDLN1> odlnAmount = odlnSums.GroupBy(r => new { r.DocEntry, r.CardCode, r.CardName, r.BaseEntry }).Select(r => new RuleDLN1
                {
                    DocEntry = r.Key.DocEntry,
                    CardCode = r.Key.CardCode,
                    CardName = r.Key.CardName,
                    BaseEntry = r.Key.BaseEntry,
                    DocTotal = r.Sum(x => x.DocTotal),
                    DocTotalFC = r.Sum(x => x.DocTotalFC)
                }).ToList();

                //查询已经完成交货的销售订单
                var oRDRODLNs = (from a in ordrs
                                 join b in odlnAmount on a.DocEntry equals b.BaseEntry
                                 where b == null ? true : a.CardCode == b.CardCode && (a.DocTotalFC == 0 ? a.DocTotal == b.DocTotal : a.DocTotalFC == b.DocTotalFC)
                                 select new PayAutoFreezeRule
                                 {
                                     CardCode = a.CardCode,
                                     DocEntry = a.DocEntry,
                                     CardName = a.CardName,
                                     DocTotal = a.DocTotal,
                                     DocTotalFC = a.DocTotalFC
                                 }).ToList();

                #region 查询收款单总金额，收款单明细为空
                //查询销售收款单
                List<ORCT> orcts = await UnitWork.Find<ORCT>(r => r.Canceled == "N" && (ordrs.Select(x => x.DocEntry).ToList()).Contains((int)r.U_XSDD)).ToListAsync();
                List<RCT2> rct2s = await UnitWork.Find<RCT2>(r => (orcts.Select(x => x.DocEntry).ToList()).Contains((int)r.DocEntry)).ToListAsync();

                //联合查询销售收款单且收款单明细为空的收款单
                List<RuleORCT> oRCTList = (from a in orcts
                                           join b in rct2s on a.DocNum equals b.DocNum into ab
                                           from b in ab.DefaultIfEmpty()
                                           where b == null
                                           select new RuleORCT
                                           {
                                               DocEntry = a.DocEntry,
                                               CardCode = a.CardCode,
                                               CardName = a.CardName,
                                               DocTotal = a.DocTotal,
                                               DocTotalFC = a.DocTotalFC,
                                               U_XSDD = (int)a.U_XSDD
                                           }).ToList();

                //联合查询销售订单-销售收款单，收款单明细为空
                var ordrodlnorcts = (from a in oRDRODLNs
                                     join b in oRCTList on a.DocEntry equals b.U_XSDD into ab
                                     from b in ab.DefaultIfEmpty()
                                     where b == null ? true : a.CardCode == b.CardCode
                                     select new RuleORDR
                                     {
                                         DocEntry = a.DocEntry,
                                         CardCode = a.CardCode,
                                         CardName = a.CardName,
                                         DocTotal = a.DocTotal,
                                         DocTotalFC = a.DocTotalFC,
                                         SumDocTotal = b == null ? 0 : b.DocTotal,
                                         SumDocTotalFC = b == null ? 0 : b.DocTotalFC
                                     }).ToList();
                #endregion

                #region 通过应收发票查询收款单明细，并查询实际收款金额
                //查询应收发票
                var inv1s = await UnitWork.Find<INV1>(r => (odlnAmount.Select(x => x.DocEntry).ToList()).Contains((int)r.BaseEntry) && r.BaseEntry != null).GroupBy(r => new { r.DocEntry, r.BaseEntry }).Select(r => new { r.Key.DocEntry, r.Key.BaseEntry }).ToListAsync();

                //查询收款单
                var rct2List = await UnitWork.Find<RCT2>(r => (inv1s.Select(x => x.DocEntry).ToList()).Contains(r.DocEntry)).GroupBy(r => new { r.DocNum, r.DocEntry, r.SumApplied, r.AppliedFC }).Select(r => new { r.Key.DocNum, r.Key.DocEntry, r.Key.SumApplied, r.Key.AppliedFC }).ToListAsync();
                var orctList = await UnitWork.Find<ORCT>(r => (rct2List.Select(x => x.DocNum).ToList()).Contains(r.DocNum)).ToListAsync();

                //联合查询应收发票对应的收款单
                var orctrct2inv1 = (from a in inv1s
                                    join b in rct2List on a.DocEntry equals b.DocEntry into ab
                                    from b in ab.DefaultIfEmpty()
                                    join c in orctList on b.DocNum equals c.DocNum into bc
                                    from c in bc.DefaultIfEmpty()
                                    select new
                                    {
                                        a.BaseEntry,
                                        CardCode = c == null ? "" : c.CardCode,
                                        SumApplied = b == null ? 0 : b.SumApplied,
                                        AppliedFC = b == null ? 0 : b.AppliedFC
                                    }).ToList();

                //联合查询已交货的销售订单对应的应收发票实际收款金额
                var ordrinv1rct2orct = (from a in oRDRODLNs
                                        join b in orctrct2inv1 on a.DocEntry equals b.BaseEntry into ab
                                        from b in ab.DefaultIfEmpty()
                                        where b == null ? true : a.CardCode == b.CardCode
                                        select new RuleORDR
                                        {
                                            DocEntry = a.DocEntry,
                                            CardCode = a.CardCode,
                                            CardName = a.CardName,
                                            DocTotal = a.DocTotal,
                                            DocTotalFC = a.DocTotalFC,
                                            SumDocTotal = b == null ? 0 : b.SumApplied,
                                            SumDocTotalFC = b == null ? 0 : b.AppliedFC
                                        }).ToList();
                #endregion

                //合并查询到符合已交货且未完全回款销售订单
                List<RuleORDR> ruleORDRs = (ordrodlnorcts.Union(ordrinv1rct2orct).ToList<RuleORDR>()).ToList();
                List<RuleORDR> ruleORDRslist = new List<RuleORDR>();
                if (key == ">")
                {
                    //未完全回款销售订单且欠款金额大于amount
                    ruleORDRslist = (ruleORDRs.GroupBy(r => new { r.DocEntry, r.CardCode, r.CardName, r.DocTotal, r.DocTotalFC }).Select(r => new RuleORDR
                    {
                        DocEntry = r.Key.DocEntry,
                        CardCode = r.Key.CardCode,
                        CardName = r.Key.CardName,
                        DocTotal = r.Key.DocTotal,
                        DocTotalFC = r.Key.DocTotalFC,
                        SumDocTotal = r.Sum(x => x.SumDocTotal),
                        SumDocTotalFC = r.Sum(x => x.SumDocTotalFC)
                    }).ToList()).Where(r => r.DocTotal - r.SumDocTotal > amount).ToList();
                }
                else
                {
                    //未完全回款销售订单且欠款金额大于等于amount
                    ruleORDRslist = (ruleORDRs.GroupBy(r => new { r.DocEntry, r.CardCode, r.CardName, r.DocTotal, r.DocTotalFC }).Select(r => new RuleORDR
                    {
                        DocEntry = r.Key.DocEntry,
                        CardCode = r.Key.CardCode,
                        CardName = r.Key.CardName,
                        DocTotal = r.Key.DocTotal,
                        DocTotalFC = r.Key.DocTotalFC,
                        SumDocTotal = r.Sum(x => x.SumDocTotal),
                        SumDocTotalFC = r.Sum(x => x.SumDocTotalFC)
                    }).ToList()).Where(r => r.DocTotal - r.SumDocTotal >= amount).ToList();
                }

                //查询近时间段内没有任何回款的客户
                var orctLists = await UnitWork.Find<ORCT>(r => (ruleORDRslist.Select(r => r.DocEntry).ToList()).Contains((int)r.U_XSDD) && r.CreateDate >= endTime).ToListAsync();
                List<RuleORDR> ruleORDRFinals = (from a in ruleORDRslist
                                                 join b in orctLists on a.DocEntry equals b.U_XSDD into ab
                                                 from b in ab.DefaultIfEmpty()
                                                 where b == null
                                                 select new RuleORDR
                                                 {
                                                     DocEntry = a.DocEntry,
                                                     CardCode = a.CardCode,
                                                     CardName = a.CardName,
                                                     DocTotal = a.DocTotal,
                                                     DocTotalFC = a.DocTotalFC,
                                                     SumDocTotal = a.SumDocTotal,
                                                     SumDocTotalFC = a.SumDocTotalFC
                                                 }).ToList();

                //近期内也没有通过应收发票回款的客户
                List<RuleORDR> ruleORDRFinalList = new List<RuleORDR>();
                foreach (RuleORDR item in ruleORDRFinals)
                {
                    var dln1sNo = await UnitWork.Find<DLN1>(r => r.BaseEntry == item.DocEntry).FirstOrDefaultAsync();
                    if (dln1sNo != null)
                    {
                        var odlnsNo = await UnitWork.Find<ODLN>(r => r.DocEntry == dln1sNo.DocEntry).FirstOrDefaultAsync();
                        if (odlnsNo != null)
                        {
                            var inv1sNo = await UnitWork.Find<INV1>(r => r.BaseEntry == odlnsNo.DocEntry).FirstOrDefaultAsync();
                            if (inv1sNo != null)
                            {
                                var rct2sNo = await UnitWork.Find<RCT2>(r => r.DocEntry == inv1sNo.DocEntry).ToListAsync();
                                if (rct2sNo == null || rct2sNo.Count() == 0)
                                {
                                    ruleORDRFinalList.Add(item);
                                }
                            }
                        }
                    }
                }

                //返回符合冻结规则的客户列表
                List<RuleCustomer> ruleCustomers = ruleORDRFinalList.GroupBy(r => new { r.CardCode, r.CardName }).Select(r => new RuleCustomer { CardCode = r.Key.CardCode, CardName = r.Key.CardName }).ToList();
                result.Data = ruleCustomers;
                result.Message = "ok";
            }
            catch (Exception ex)
            {
                result.Message = "error:" + ex.Message.ToString();
            }

            return result;
        }
        #endregion

        #region 3.0获取客户管理
        /// <summary>
        /// 获取用户信息Id
        /// </summary>
        /// <param name="SboId"></param>
        /// <param name="UserId"></param>
        /// <param name="SeeType"></param>
        /// <returns></returns>
        private string GetUserInfoById(string SboId, string UserId, string SeeType)
        {
            string rRoleNm = "", rFiledName = "";
            if (SeeType == "1")
            {
                rFiledName = "A.sale_id";
                rRoleNm = "销售";
            }
            else if (SeeType == "2")
            {
                rFiledName = "A.sale_id";
                rRoleNm = "采购";
            }
            else if (SeeType == "3")
            {
                rFiledName = "A.tech_id";
                rRoleNm = "技术";
            }
            else
            {
                return "0";
            }
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT {0} FROM {1}.sbo_user A ", rFiledName, "nsap_base");
            strSql.AppendFormat("LEFT JOIN {0}.base_user_role B ON A.user_id=B.user_id  ", "nsap_base");
            strSql.AppendFormat("LEFT JOIN {0}.base_role C ON B.role_id=C.role_id ", "nsap_base");
            strSql.AppendFormat("WHERE A.sbo_id={0} AND A.user_id={1} AND C.role_nm LIKE '%{2}%' AND {3}>0 GROUP BY A.user_id ", SboId, UserId, rRoleNm, rFiledName);
            object strObj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
            return strObj == null ? "0" : strObj.ToString();
        }

        /// <summary>
        /// 获取部门
        /// </summary>
        /// <param name="rPurCode"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        private bool OCRDisSpecial(string rPurCode, string v1, string v2)
        {
            bool ret = false;
            string strSql = string.Format(" SELECT count(*)  FROM {0}.crm_OCQG_assign WHERE sbo_id=?sbo_id AND SlpCode=?SlpCode AND GroupCode=?GroupCode ", "nsap_bone");
            IDataParameter[] strPara = {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?sbo_id", v2),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?SlpCode", rPurCode),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?GroupCode", v1)

            };

            object obj = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSql, CommandType.Text, strPara);
            if (obj != null)
            {
                int num = Convert.ToInt32(obj.ToString());
                if (num > 0)
                {
                    ret = true;
                }
            }

            return ret;
        }

        /// <summary>
        /// 获取用户部门信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<UserResp> GetUserOrgInfo(string userId, string name = "")
        {
            List<string> nameList = null;
            if (!string.IsNullOrWhiteSpace(name))
            {
                nameList = name.Split(',').ToList();
            }
            var petitioner = (from a in UnitWork.Find<User>(null)
                                           .WhereIf(!string.IsNullOrWhiteSpace(userId), c => c.Id == userId)
                                           .WhereIf(!string.IsNullOrWhiteSpace(name), c => nameList.Contains(c.Name))
                              join b in UnitWork.Find<Relevance>(r => r.Key == Define.USERORG) on a.Id equals b.FirstId into ab
                              from b in ab.DefaultIfEmpty()
                              join c in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on b.SecondId equals c.Id into bc
                              from c in bc.DefaultIfEmpty()
                              select new UserResp
                              {
                                  Name = a.Name,
                                  Id = a.Id,
                                  OrgId = c.Id,
                                  OrgName = c.Name,
                                  CascadeId = c.CascadeId,
                                  Account = a.Account,
                                  Sex = a.Sex,
                                  Mobile = a.Mobile,
                                  Email = a.Email
                              }).OrderByDescending(u => u.CascadeId).ToList();
            return petitioner;
        }

        /// <summary>
        /// 获取客户列表
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="page"></param>
        /// <param name="query"></param>
        /// <param name="sortname"></param>
        /// <param name="sortorder"></param>
        /// <param name="sboid"></param>
        /// <param name="userId"></param>
        /// <param name="rIsViewSales"></param>
        /// <param name="rIsViewSelf"></param>
        /// <param name="rIsViewSelfDepartment"></param>
        /// <param name="rIsViewFull"></param>
        /// <param name="depID"></param>
        /// <param name="label"></param>
        /// <param name="contectTel"></param>
        /// <param name="slpName"></param>
        /// <param name="isReseller"></param>
        /// <param name="Day"></param>
        /// <param name="CntctPrsn"></param>
        /// <param name="address"></param>
        /// <param name="U_CardTypeStr"></param>
        /// <param name="U_ClientSource"></param>
        /// <param name="U_CompSector"></param>
        /// <param name="U_TradeType"></param>
        /// <param name="U_StaffScale"></param>
        /// <param name="CreateStartTime"></param>
        /// <param name="CreateEndTime"></param>
        /// <param name="DistributionStartTime"></param>
        /// <param name="DistributionEndTime"></param>
        /// <param name="dNotesBalStart"></param>
        /// <param name="dNotesBalEnd"></param>
        /// <param name="ordersBalStart"></param>
        /// <param name="ordersBalEnd"></param>
        /// <param name="balanceStart"></param>
        /// <param name="balanceEnd"></param>
        /// <param name="balanceTotalStart"></param>
        /// <param name="balanceTotalEnd"></param>
        /// <param name="CardName"></param>
        /// <param name="rowCount"></param>
        /// <returns></returns>
        public DataTable SelectClientList(int limit, int page, string query, string sortname, string sortorder,
          int sboid, int userId, bool rIsViewSales, bool rIsViewSelf, bool rIsViewSelfDepartment, bool rIsViewFull,
          int depID, string label, string contectTel, string slpName, string isReseller, int? Day, string CntctPrsn, string address,
          string U_CardTypeStr, string U_ClientSource, string U_CompSector, string U_TradeType, string U_StaffScale,
          DateTime? CreateStartTime, DateTime? CreateEndTime, DateTime? DistributionStartTime, DateTime? DistributionEndTime,
          decimal? dNotesBalStart, decimal? dNotesBalEnd, decimal? ordersBalStart, decimal? ordersBalEnd,
          decimal? balanceStart, decimal? balanceEnd, decimal? balanceTotalStart, decimal? balanceTotalEnd, string CardName, out int rowCounts)
        {
            bool IsSaler = false, IsPurchase = false, IsTech = false, IsClerk = false;//业务员，采购员，技术员，文员
            string rSalCode = GetUserInfoById(sboid.ToString(), userId.ToString(), "1");
            string rPurCode = GetUserInfoById(sboid.ToString(), userId.ToString(), "2");
            string rTcnicianCode = GetUserInfoById(sboid.ToString(), userId.ToString(), "3");
            if (Convert.ToInt32(rSalCode) > 0)
            {
                IsSaler = true;
            }

            if (Convert.ToInt32(rPurCode) > 0)
            {
                IsPurchase = true;
            }

            if (Convert.ToInt32(rTcnicianCode) > 0)
            {
                IsTech = true;
            }

            bool IsOpenSap = _serviceSaleOrderApp.GetSapSboIsOpen(sboid.ToString());
            string sortString = string.Empty;
            StringBuilder filterString = new StringBuilder();
            filterString.Append(IsOpenSap ? " 1=1 " : string.Format(" sbo_id={0} ", sboid.ToString()));
            if (!rIsViewFull)
            {
                #region 查看本部门
                if (rIsViewSelfDepartment)
                {
                    string filter_str = string.Empty;
                    if (IsPurchase)//采购员
                    {
                        bool isMechanical = OCRDisSpecial(rPurCode, "2", sboid.ToString());
                        if (isMechanical)
                        {
                            filter_str = string.Format(" (CardCode IN ('V00733','V00735','V00836') OR (SlpCode='-1' and (QryGroup2='Y' OR QryGroup3='Y')))", rPurCode);
                        }
                        else
                        {
                            filter_str = string.Format(" (CardCode IN ('V00733','V00735','V00836') OR (SlpCode='-1' and QryGroup2='N')) ", rPurCode);
                        }
                    }

                    filterString.AppendFormat(" {0} AND DfTcnician ={1} ", filter_str, rTcnicianCode);
                }
                #endregion
                #region 查看自己
                if (rIsViewSelf && !rIsViewSelfDepartment)
                {
                    if (!IsSaler && !IsPurchase && !IsTech && !IsClerk)
                    {
                        filterString.AppendFormat(" AND 1<>1 ");
                    }
                    else
                    {
                        int flag = 0;
                        filterString.AppendFormat(" AND ( ");
                        if (IsSaler)//业务员
                        {
                            flag = 1;
                            filterString.AppendFormat(" (SlpCode={0} and CardCode like 'C%') ", rSalCode);
                        }
                        if (IsPurchase)//采购员
                        {
                            if (flag == 1)
                            {
                                filterString.AppendFormat(" OR (SlpCode={0} and CardCode like 'V%') ", rPurCode);
                            }
                            else
                            {
                                flag = 1;
                                string filter_str = string.Empty;
                                bool isMechanical = OCRDisSpecial(rPurCode, "2", sboid.ToString());
                                if (isMechanical)
                                {
                                    filter_str = string.Format(" (CardCode IN ('V00733','V00735','V00836') OR SlpCode ={0} OR ( SlpCode='-1' and (QryGroup2='Y' OR QryGroup3='Y')) ) ", rPurCode);
                                }
                                else
                                {
                                    filter_str = string.Format(" ( CardCode IN ('V00733','V00735','V00836') OR SlpCode ={0} OR ( SlpCode='-1' and QryGroup2='N') ) ", rPurCode);
                                }

                                filterString.AppendFormat(" ({0} and CardCode like 'V%') ", filter_str);
                            }
                        }
                        if (IsTech)//技术员
                        {
                            if (flag == 1)
                            {
                                filterString.AppendFormat(" OR (DfTcnician={0} and CardCode like 'C%') ", rTcnicianCode);
                            }
                            else
                            {
                                flag = 1;
                                filterString.AppendFormat(" DfTcnician={0} and CardCode like 'C%' ", rTcnicianCode);
                            }
                        }
                        filterString.AppendFormat(" ) ");
                    }
                }
                #endregion
            }
            else
            {
                if ((IsSaler || IsTech || IsClerk) && !IsPurchase)//业务员或技术员,文员
                {
                    filterString.Append(" AND CardCode LIKE 'C%' ");
                }
                else if (IsPurchase && !IsSaler && !IsTech && !IsClerk)//采购员
                {
                    filterString.Append("AND CardCode LIKE 'V%' ");
                }
            }
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());
            if (!string.IsNullOrEmpty(query))
            {
                string[] whereArray = query.Split('`');
                for (int i = 1; i < whereArray.Length; i++)
                {
                    string[] p = whereArray[i].Split(':');
                    if (!string.IsNullOrEmpty(p[1]))
                    {
                        filterString.AppendFormat("AND {0} LIKE '%{1}%' ", p[0], p[1].Trim().FilterSQL().Replace("*", "%"));
                    }
                }
            }

            StringBuilder filedName = new StringBuilder();
            StringBuilder tableName = new StringBuilder();
            DataTable clientTable = new DataTable();
            if (!IsOpenSap) { filedName.Append("sbo_id,"); }
            filedName.Append("CardCode, CardName, SlpName, Technician, CntctPrsn, Address, Phone1, Cellular, ");
            if (rIsViewSales)
            {
                filedName.Append("Balance, BalanceTotal, DNotesBal, OrdersBal, OprCount, ");
            }
            else
            {
                filedName.Append("'****' AS Balance, '****' AS BalanceTotal, '****' AS DNotesBal, '****' AS OrdersBal, '****' AS OprCount, ");
            }

            filedName.Append("UpdateDate , ");
            filedName.Append(" validFor,validFrom,validTo,ValidComm,frozenFor,frozenFrom,frozenTo,FrozenComm ,GroupName,Free_Text");
            filedName.Append(",case when INVTotal90P>0 and Due90>0 then (Due90/INVTotal90P)*100 else 0 end as Due90Percent");
            if (IsOpenSap)
            {
                tableName.Append("(SELECT A.CardCode,A.CardName,B.SlpName,(ISNULL(E.lastName,'')+ISNULL(E.firstName,'')) as Technician,");
                tableName.Append("A.CntctPrsn,(ISNULL(F.Name,'')+ISNULL(G.Name,'')+ISNULL(A.City,'')+ISNULL(CONVERT(NVARCHAR(100),A.Building),'')) AS Address, ");
                tableName.Append("A.Phone1,A.Cellular,");//,A.Balance,ISNULL(A.Balance,0) + ISNULL(H.doctoal,0) AS BalanceTotal
                tableName.Append("A.DNotesBal,A.OrdersBal,A.OprCount,A.UpdateDate,A.SlpCode,A.DfTcnician ");
                tableName.Append(",isnull(A.Balance,0) as Balance,0.00 as BalanceTotal ");
                tableName.Append(" , A.validFor,A.validFrom,A.validTo,A.ValidComm,A.frozenFor,A.frozenFrom,A.frozenTo,A.FrozenComm,A.QryGroup2,A.QryGroup3 ");
                tableName.Append(",C.GroupName,A.Free_Text");

                //90天内未清收款金额
                tableName.Append(",isnull(A.Balance,0)+isnull((select SUM(openBal) from ORCT WHERE CANCELED = 'N' AND openBal<>0 and datediff(DAY, docdate, getdate())<= 90 AND CardCode = A.CardCode),0)");

                //90天内未清发票金额
                tableName.Append("-isnull((select SUM(DocTotal - PaidToDate) from OINV WHERE CANCELED = 'N' and DocTotal-PaidToDate > 0 and datediff(DAY, docdate, getdate())<= 90 and CardCode = A.CardCode),0) ");

                //90天内未清贷项金额
                tableName.Append("+isnull((select SUM(DocTotal - PaidToDate) from ORIN where CANCELED = 'N' and DocTotal-PaidToDate > 0 and datediff(DAY, docdate, getdate())<= 90 and CardCode = A.CardCode),0) as Due90");

                //90天前未清发票的发票总额
                tableName.Append(",(select SUM(DocTotal) from OINV WHERE CANCELED = 'N' and DocTotal-PaidToDate > 0 and datediff(DAY, docdate, getdate())> 90 and CardCode = A.CardCode) as INVTotal90P ");
                tableName.Append(" FROM OCRD A ");
                tableName.Append("LEFT JOIN OSLP B ON B.SlpCode=A.SlpCode ");
                tableName.Append("LEFT JOIN OCRG C ON C.GroupCode=A.GroupCode ");
                tableName.Append("LEFT JOIN OIDC D ON D.Code=A.Indicator ");
                tableName.Append("LEFT JOIN OHEM E ON E.empID=A.DfTcnician ");
                tableName.Append("LEFT JOIN OCRY F ON F.Code=A.Country ");
                tableName.Append("LEFT JOIN OCST G ON G.Code=A.State1 ");
                tableName.Append(") T");
                clientTable = _serviceSaleOrderApp.SAPSelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), limit, page, sortString, filterString.ToString(), out rowCounts);
            }
            else
            {
                tableName.Append("(SELECT A.sbo_id,A.CardCode,A.CardName,B.SlpName,CONCAT(IFNULL(E.lastName,''),IFNULL(E.firstName,'')) as Technician,");
                tableName.Append("A.CntctPrsn,CONCAT(IFNULL(F.Name,''),IFNULL(G.Name,''),IFNULL(A.City,''),IFNULL(A.Building,'')) AS Address, ");
                tableName.Append("A.Phone1,A.Cellular, ");//,A.Balance,H.Balance AS BalanceTotal
                tableName.Append("A.DNotesBal,A.OrdersBal,A.OprCount,A.upd_dt AS UpdateDate,A.SlpCode,A.DfTcnician ");
                tableName.Append(",IFNULL(A.Balance,0) as Balance,0.00 as BalanceTotal ");
                tableName.Append(", case  when LOCATE(\"C\", Y.SubNo)  > 0  ||  LOCATE(\"C\", Y.ParentNo) > 0  then 1  ELSE 0 end as relationFlag ");
                tableName.Append(" , A.validFor,A.validFrom,A.validTo,A.ValidComm,A.frozenFor,A.frozenFrom,A.frozenTo,A.FrozenComm,A.QryGroup2,A.QryGroup3 ");
                tableName.Append(",C.GroupName,A.Free_Text");
                //90天内未清收款金额
                tableName.Append(",IFNULL(A.Balance,0)+IFNULL((select SUM(openBal) from FINANCE_ORCT WHERE CANCELED = 'N' AND openBal<>0 and datediff(NOW(), docdate)<= 90 AND CardCode = A.CardCode),0)");
                //90天内未清发票金额
                tableName.Append("-IFNULL((select SUM(DocTotal - PaidToDate) from SALE_OINV WHERE CANCELED = 'N' and DocTotal-PaidToDate > 0 and datediff(NOW(), docdate)<= 90 and CardCode = A.CardCode),0)");
                //90天内未清贷项金额
                tableName.Append("+IFNULL((select SUM(DocTotal - PaidToDate) from SALE_ORIN where CANCELED = 'N' and DocTotal-PaidToDate > 0 and datediff(NOW(), docdate)<= 90 and CardCode = A.CardCode),0) as Due90");
                //90天前未清发票的发票总额
                tableName.Append(",(select SUM(DocTotal) from SALE_OINV WHERE CANCELED = 'N' and DocTotal-PaidToDate > 0 and datediff(NOW(), docdate)> 90 and CardCode = A.CardCode) as INVTotal90P ");

                tableName.AppendFormat(" FROM {0}.crm_OCRD A  ", "nsap_bone");
                tableName.AppendFormat("LEFT JOIN {0}.crm_OSLP B ON B.SlpCode=A.SlpCode AND B.sbo_id=A.sbo_id ", "nsap_bone");
                tableName.AppendFormat("LEFT JOIN {0}.crm_OCRG C ON C.GroupCode=A.GroupCode AND C.sbo_id=A.sbo_id ", "nsap_bone");
                tableName.AppendFormat("LEFT JOIN {0}.crm_OIDC D ON D.Code=A.Indicator AND D.sbo_id=A.sbo_id ", "nsap_bone");
                tableName.AppendFormat("LEFT JOIN {0}.crm_OHEM E ON E.empID=A.DfTcnician AND E.sbo_id=A.sbo_id ", "nsap_bone");
                tableName.AppendFormat("LEFT JOIN {0}.crm_OCRY F ON F.Code=A.Country ", "nsap_bone");
                tableName.AppendFormat("LEFT JOIN {0}.crm_OCST G ON G.Code=A.State1 ", "nsap_bone");
                tableName.AppendFormat("LEFT JOIN {0}.wfa_job H ON H.sbo_itf_return=A.CardCode ", "nsap_base");
                tableName.AppendFormat("LEFT JOIN {0}.clue I ON I.Id=H.base_entry", "nsap_serve");
                tableName.AppendFormat("LEFT JOIN  (SELECT c.Id, c.SubNo ,c.ClientNo, c.ParentNo, c.IsActive, c.IsDelete, c.ScriptFlag,ROW_NUMBER() OVER (PARTITION BY ClientNo ORDER BY UpdateDate) rn from {0}.clientrelation c)   Y ON Y.ClientNo = A.CardCode  AND Y.IsActive =1 AND Y.ScriptFlag =0        AND  Y.IsDelete = 0   ", "erp4");
                tableName.AppendFormat("LEFT JOIN {0}.cluefollowup J ON J.ClueId=I.Id ORDER BY b.FollowUpTime DESC LIMIT 1 ", "nsap_serve");
                tableName.Append(") T");
                clientTable = _serviceSaleOrderApp.SelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), limit, page, sortString, filterString.ToString(), out rowCounts);
            }

            for (int i = 0; i < clientTable.Rows.Count; i++)
            {
                string slpname = clientTable.Rows[i]["SlpName"].ToString();
                string technician = clientTable.Rows[i]["Technician"].ToString();
                var recepUserOrgInfo = GetUserOrgInfo("", slpname + "," + technician);
                clientTable.Rows[i]["SlpName"] = recepUserOrgInfo.FirstOrDefault(q => q.Name == slpname) == null ? clientTable.Rows[i]["SlpName"] : recepUserOrgInfo.FirstOrDefault(q => q.Name == slpname).OrgName + "-" + clientTable.Rows[i]["SlpName"];
                clientTable.Rows[i]["Technician"] = recepUserOrgInfo.FirstOrDefault(q => q.Name == technician) == null ? clientTable.Rows[i]["Technician"] : recepUserOrgInfo.FirstOrDefault(q => q.Name == technician).OrgName + "-" + clientTable.Rows[i]["Technician"];
            }

            return clientTable;
        }

        /// <summary>
        /// 分页获取客户
        /// </summary>
        /// <param name="limit">当前页大小</param>
        /// <param name="page">页码</param>
        /// <param name="filedString">条件</param>
        /// <param name="rowCounts">总数</param>
        /// <returns>返回客户信息</returns>
        public List<RuleCustomer> GetClientList(int limit, int page, string filedString, out int rowCounts)
        {
            DataTable clientTable = new DataTable();
            StringBuilder tableName = new StringBuilder();
            StringBuilder filedName = new StringBuilder();
            filedName.Append(" *    ");
            tableName.Append(" (SELECT CardCode, CardName,  ");
            tableName.Append("case when INVTotal90P>0 and Due90>0 then (Due90/INVTotal90P)*100 else 0 end as DuePercent FROM");
            tableName.Append("(SELECT A.CardCode,A.CardName");
            //90天内未清收款金额
            tableName.Append(",isnull(A.Balance,0)+isnull((select SUM(openBal) from ORCT WHERE CANCELED = 'N' AND openBal<>0 and datediff(DAY, docdate, getdate())<= 90 AND CardCode = A.CardCode),0)");
            //90天内未清发票金额
            tableName.Append("-isnull((select SUM(DocTotal - PaidToDate) from OINV WHERE CANCELED = 'N' and DocTotal-PaidToDate > 0 and datediff(DAY, docdate, getdate())<= 90 and CardCode = A.CardCode),0) ");
            //90天内未清贷项金额
            tableName.Append("+isnull((select SUM(DocTotal - PaidToDate) from ORIN where CANCELED = 'N' and DocTotal-PaidToDate > 0 and datediff(DAY, docdate, getdate())<= 90 and CardCode = A.CardCode),0) as Due90");
            //90天前未清发票的发票总额
            tableName.Append(",(select SUM(DocTotal) from OINV WHERE CANCELED = 'N' and DocTotal-PaidToDate > 0 and datediff(DAY, docdate, getdate())> 90 and CardCode = A.CardCode) as INVTotal90P ");
            tableName.Append(" FROM OCRD A ");
            tableName.Append(") T) T");
            clientTable = _serviceSaleOrderApp.SAPSelectPagingHaveRowsCount(tableName.ToString(), filedName.ToString(), limit, page, "cardcode desc", filedString, out rowCounts);
            List<RuleCustomer> ruleCustomers = clientTable.Tolist<RuleCustomer>();
            return ruleCustomers;
        }
        #endregion

        #region VIP客户
        /// <summary>
        /// 获取VIP客户列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetVIPCustomer(CustomerReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();

            //查询vip客户
            var objs = UnitWork.Find<PayVIPCustomer>(null);
            var vIPCustomers = objs.WhereIf(!string.IsNullOrWhiteSpace(request.SaleUser), r => r.SaleId.Contains(request.SaleUser) || r.SaleName.Contains(request.SaleUser))
                                   .WhereIf(!string.IsNullOrWhiteSpace(request.CustomerCodeOrName), r => r.CardCode.Contains(request.CustomerCodeOrName) || r.CardName.Contains(request.CustomerCodeOrName));

            //分页按照创建时间排序
            vIPCustomers = vIPCustomers.OrderByDescending(r => r.CreateTime);
            var vIPCustomersList = await vIPCustomers.Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
            var vIPCustomersLists = vIPCustomersList.Select(r => new
            {
                r.Id,
                r.CardCode,
                r.CardName,
                r.SaleId,
                SaleDeptName = _userDepartMsgHelp.GetUserDepart(Convert.ToInt32(r.SaleId)),
                r.SaleName,
                r.ListName,
                r.Remark,
                r.CreateUserId,
                r.CreateUserName,
                CreateDeptName = _userDepartMsgHelp.GetUserOrgName(r.CreateUserId),
                r.CreateTime,
                r.UpdateUserId,
                UpdateDeptName = _userDepartMsgHelp.GetUserOrgName(r.UpdateUserId),
                r.UpdateUserName,
                r.UpdateTime
            });

            result.Data = vIPCustomersLists;
            result.Count = vIPCustomers.Count();
            result.Message = "获取成功";
            return result;
        }

        /// <summary>
        /// 获取vip客户详情
        /// </summary>
        /// <param name="Id">vip客户Id</param>
        /// <returns>返回vip客户详情信息</returns>
        public async Task<TableData> GetVIPCustomerDetail(string Id)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();

            //查询vip客户详情
            var vIPCustomers = await UnitWork.Find<PayVIPCustomer>(r => r.Id == Id).Select(r => new
            {
                r.Id,
                r.CardCode,
                r.CardName,
                r.SaleId,
                SaleDeptName = _userDepartMsgHelp.GetUserDepart(Convert.ToInt32(r.SaleId)),
                r.SaleName,
                r.ListName,
                r.Remark,
                r.CreateUserId,
                r.CreateUserName,
                CreateDeptName = _userDepartMsgHelp.GetUserOrgName(r.CreateUserId),
                r.CreateTime,
                r.UpdateUserId,
                UpdateDeptName = _userDepartMsgHelp.GetUserOrgName(r.UpdateUserId),
                r.UpdateUserName,
                r.UpdateTime
            }).ToListAsync();

            result.Data = vIPCustomers;
            result.Message = "获取成功";
            return result;
        }

        /// <summary>
        /// 添加VIP客户
        /// </summary>
        /// <param name="obj">vip客户实体</param>
        /// <returns>返回添加结果</returns>
        public async Task<TableData> AddVIPCustomer(PayVIPCustomer obj)
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //判定客户是否重复
            var payVIPCustomers = await UnitWork.Find<PayVIPCustomer>(r => r.CardCode == obj.CardCode).ToListAsync();
            if (payVIPCustomers != null && payVIPCustomers.Count() > 0)
            {
                result.Code = 500;
                result.Message = "已经存在该VIP客户，不允许重复";
                return result;
            }

            //事务创建VIP客户
            var dbContext = UnitWork.GetDbContext<PayVIPCustomer>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    obj.ListName = "VIP客户";
                    obj.SaleId = string.IsNullOrEmpty(obj.SaleId) ? "0" : obj.SaleId;
                    obj.CreateUserId = loginContext.User.Id;
                    obj.CreateUserName = loginContext.User.Name;
                    obj.CreateTime = DateTime.Now;
                    obj.UpdateUserId = null;
                    obj.UpdateUserName = null;
                    obj.UpdateTime = null;
                    obj = await UnitWork.AddAsync<PayVIPCustomer, int>(obj);
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                    result.Message = "创建成功";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result.Code = 500;
                    result.Message = ex.Message.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// 修改VIP客户
        /// </summary>
        /// <param name="obj">vip客户实体</param>
        /// <returns>返回修改结果</returns>
        public async Task<TableData> UpdateVIPCustomer(PayVIPCustomer obj)
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //判定客户是否重复
            var payVIPCustomers = await UnitWork.Find<PayVIPCustomer>(r => r.CardCode == obj.CardCode).ToListAsync();
            if (payVIPCustomers != null && payVIPCustomers.Count() > 1)
            {
                result.Code = 500;
                result.Message = "已经存在该VIP客户，不允许重复";
                return result;
            }

            //事务创建VIP客户
            var dbContext = UnitWork.GetDbContext<PayVIPCustomer>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await UnitWork.UpdateAsync<PayVIPCustomer>(u => u.Id == obj.Id, u => new PayVIPCustomer()
                    {
                        CardCode = obj.CardCode,
                        CardName = obj.CardName,
                        SaleId = string.IsNullOrEmpty(obj.SaleId) ? "0" : obj.SaleId,
                        SaleName = obj.SaleName,
                        ListName = obj.ListName,
                        Remark = obj.Remark,
                        CreateUserId = obj.CreateUserId,
                        CreateUserName = obj.CreateUserName,
                        CreateTime = obj.CreateTime,
                        UpdateUserId = loginContext.User.Id,
                        UpdateUserName = loginContext.User.Name,
                        UpdateTime = DateTime.Now
                    });

                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                    result.Message = "修改成功";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result.Code = 500;
                    result.Message = ex.Message.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// 删除VIP客户
        /// </summary>
        /// <param name="Id">VIP客户Id</param>
        /// <returns>返回删除结果</returns>
        public async Task<TableData> DeleteVIPCustomer(string Id)
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var payVIPCustomers = await UnitWork.Find<PayVIPCustomer>(r => r.Id == Id).ToListAsync();
            if (payVIPCustomers != null && payVIPCustomers.Count() > 0)
            {
                await UnitWork.DeleteAsync<PayVIPCustomer>(r => r.Id == Id);
                await UnitWork.SaveAsync();
                result.Message = "删除成功";
            }
            else
            {
                result.Message = "删除失败，该客户不存在";
                result.Code = 500;
            }

            return result;
        }
        #endregion

        #region 冻结客户
        /// <summary>
        /// 获取冻结客户列表
        /// </summary>
        /// <param name="request">冻结客户查询实体</param>
        /// <returns>返回冻结客户列表信息</returns>
        public async Task<TableData> GetFreezeCustomer(CustomerReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();
            QueryTime qt = _saleBusinessMethodHelp.TimeRange(request.TimeRange);
            if (qt == null || qt.endTime == null || qt.startTime == null)
            {
                //查询冻结客户
                var objs = UnitWork.Find<PayFreezeCustomer>(null);
                var freezeCustomers = objs.WhereIf(!string.IsNullOrWhiteSpace(request.SaleUser), r => r.SaleId.Contains(request.SaleUser) || r.SaleName.Contains(request.SaleUser))
                                       .WhereIf(!string.IsNullOrWhiteSpace(request.CustomerCodeOrName), r => r.CardCode.Contains(request.CustomerCodeOrName) || r.CardName.Contains(request.CustomerCodeOrName));

                //分页按照创建时间排序
                freezeCustomers = freezeCustomers.OrderByDescending(r => r.CreateTime);
                var freezeCustomerList = await freezeCustomers.Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
                var freezeCustomersLists = freezeCustomerList.Select(r => new
                {
                    r.Id,
                    r.CardCode,
                    r.CardName,
                    r.SaleId,
                    SaleDeptName = _userDepartMsgHelp.GetUserDepart(Convert.ToInt32(r.SaleId)),
                    r.SaleName,
                    r.FreezeCause,
                    r.IsAutoThaw,
                    r.FreezeStartTime,
                    r.FreezeEndTime,
                    r.ThawStartTime,
                    r.ThawEndTime,
                    r.ListName,
                    r.FreezeType,
                    r.SendCount,
                    r.CreateUserId,
                    r.CreateUserName,
                    CreateDeptName = _userDepartMsgHelp.GetUserOrgName(r.CreateUserId),
                    r.CreateTime,
                    r.UpdateUserId,
                    UpdateDeptName = _userDepartMsgHelp.GetUserOrgName(r.UpdateUserId),
                    r.UpdateUserName,
                    r.UpdateTime
                });

                result.Data = freezeCustomersLists;
                result.Count = freezeCustomers.Count();
                result.Message = "获取成功";
            }
            else
            {
                //查询冻结客户
                var objs = UnitWork.Find<PayFreezeCustomer>(r => r.CreateTime >= Convert.ToDateTime(qt.startTime) && r.CreateTime <= Convert.ToDateTime(qt.endTime));
                var freezeCustomers = objs.WhereIf(!string.IsNullOrWhiteSpace(request.SaleUser), r => r.SaleId.Contains(request.SaleUser) || r.SaleName.Contains(request.SaleUser))
                                       .WhereIf(!string.IsNullOrWhiteSpace(request.CustomerCodeOrName), r => r.CardCode.Contains(request.CustomerCodeOrName) || r.CardName.Contains(request.CustomerCodeOrName));

                //分页按照创建时间排序
                freezeCustomers = freezeCustomers.OrderByDescending(r => r.CreateTime);
                var freezeCustomerList = await freezeCustomers.Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
                var freezeCustomersLists = freezeCustomerList.Select(r => new
                {
                    r.Id,
                    r.CardCode,
                    r.CardName,
                    r.SaleId,
                    SaleDeptName = _userDepartMsgHelp.GetUserDepart(Convert.ToInt32(r.SaleId)),
                    r.SaleName,
                    r.FreezeCause,
                    r.IsAutoThaw,
                    r.FreezeStartTime,
                    r.FreezeEndTime,
                    r.ThawStartTime,
                    r.ThawEndTime,
                    r.ListName,
                    r.FreezeType,
                    r.SendCount,
                    r.CreateUserId,
                    r.CreateUserName,
                    CreateDeptName = _userDepartMsgHelp.GetUserOrgName(r.CreateUserId),
                    r.CreateTime,
                    r.UpdateUserId,
                    UpdateDeptName = _userDepartMsgHelp.GetUserOrgName(r.UpdateUserId),
                    r.UpdateUserName,
                    r.UpdateTime
                });

                result.Data = freezeCustomersLists;
                result.Count = freezeCustomers.Count();
                result.Message = "获取成功";
            }

            return result;
        }

        /// <summary>
        /// 获取当前冻结客户详细信息
        /// </summary>
        /// <param name="Id">冻结客户Id</param>
        /// <returns>返回冻结客户详细信息</returns>
        public async Task<TableData> GetFreezeCustomerDetail(string Id)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();

            //查询vip客户详情
            var freezeCustomers = await UnitWork.Find<PayFreezeCustomer>(r => r.Id == Id).Select(r => new
            {
                r.Id,
                r.CardCode,
                r.CardName,
                r.SaleId,
                SaleDeptName = _userDepartMsgHelp.GetUserDepart(Convert.ToInt32(r.SaleId)),
                r.SaleName,
                r.FreezeCause,
                r.IsAutoThaw,
                r.FreezeStartTime,
                r.FreezeEndTime,
                r.ThawStartTime,
                r.ThawEndTime,
                r.ListName,
                r.SendCount,
                r.FreezeType,
                r.CreateUserId,
                r.CreateUserName,
                CreateDepartName = _userDepartMsgHelp.GetUserOrgName(r.CreateUserId),
                r.CreateTime,
                r.UpdateUserId,
                UpdateDeptNamr = _userDepartMsgHelp.GetUserOrgName(r.UpdateUserId),
                r.UpdateUserName,
                r.UpdateTime
            }).ToListAsync();

            result.Data = freezeCustomers;
            result.Message = "获取成功";
            return result;
        }

        /// <summary>
        /// 添加冻结客户
        /// </summary>
        /// <param name="obj">冻结客户实体</param>
        /// <returns>返回冻结客户添加结果</returns>
        public async Task<TableData> AddFreezeCustomer(PayFreezeCustomer obj)
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //判定客户是否重复
            var payFreezeCustomers = await UnitWork.Find<PayFreezeCustomer>(r => r.CardCode == obj.CardCode).ToListAsync();
            if (payFreezeCustomers != null && payFreezeCustomers.Count() > 0)
            {
                result.Code = 500;
                result.Message = "已经存在该冻结客户，不允许重复";
                return result;
            }

            //事务创建客户
            var dbContext = UnitWork.GetDbContext<PayFreezeCustomer>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    obj.SendCount = 0;
                    obj.ListName = "冻结客户";
                    obj.FreezeType = 2;
                    obj.SaleId = string.IsNullOrEmpty(obj.SaleId) ? "0" : obj.SaleId;
                    obj.CreateUserId = loginContext.User.Id;
                    obj.CreateUserName = loginContext.User.Name;
                    obj.CreateTime = DateTime.Now;
                    obj.UpdateUserId = null;
                    obj.UpdateUserName = null;
                    obj.UpdateTime = null;
                    obj = await UnitWork.AddAsync<PayFreezeCustomer, int>(obj);
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                    result.Message = "创建成功";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result.Code = 500;
                    result.Message = ex.Message.ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// 修改冻结客户
        /// </summary>
        /// <param name="obj">冻结客户实体</param>
        /// <returns>返回冻结客户结果</returns>
        public async Task<TableData> UpdateFreezeCustomer(PayFreezeCustomer obj)
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //判定客户是否重复
            var payFreezeCustomers = await UnitWork.Find<PayFreezeCustomer>(r => r.CardCode == obj.CardCode).ToListAsync();
            if (payFreezeCustomers != null && payFreezeCustomers.Count() > 1)
            {
                result.Code = 500;
                result.Message = "已经存在该冻结客户，不允许重复";
                return result;
            }

            //事务修改客户
            var dbContext = UnitWork.GetDbContext<PayFreezeCustomer>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await UnitWork.UpdateAsync<PayFreezeCustomer>(u => u.Id == obj.Id, u => new PayFreezeCustomer()
                    {
                        CardCode = obj.CardCode,
                        CardName = obj.CardName,
                        SaleId = string.IsNullOrEmpty(obj.SaleId) ? "0" : obj.SaleId,
                        SaleName = obj.SaleName,
                        ListName = obj.ListName,
                        FreezeCause = obj.FreezeCause,
                        IsAutoThaw = obj.IsAutoThaw,
                        FreezeStartTime = obj.FreezeStartTime,
                        FreezeEndTime = obj.FreezeEndTime,
                        ThawStartTime = obj.ThawStartTime,
                        ThawEndTime = obj.ThawEndTime,
                        SendCount = obj.SendCount,
                        FreezeType = obj.FreezeType,
                        CreateUserId = obj.CreateUserId,
                        CreateUserName = obj.CreateUserName,
                        CreateTime = obj.CreateTime,
                        UpdateUserId = loginContext.User.Id,
                        UpdateUserName = loginContext.User.Name,
                        UpdateTime = DateTime.Now
                    });

                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                    result.Message = "修改成功";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result.Code = 500;
                    result.Message = ex.Message.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// 删除冻结客户
        /// </summary>
        /// <param name="Id">东接客户Id</param>
        /// <returns>返回冻结客户删除结果</returns>
        public async Task<TableData> DeleteFreezeCustomer(string Id)
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var payFreezeCustomers = await UnitWork.Find<PayFreezeCustomer>(r => r.Id == Id).ToListAsync();
            if (payFreezeCustomers != null && payFreezeCustomers.Count() > 0)
            {
                await UnitWork.DeleteAsync<PayFreezeCustomer>(r => r.Id == Id);
                await UnitWork.SaveAsync();
                result.Message = "删除成功";
            }
            else
            {
                result.Message = "删除失败，该客户不存在";
                result.Code = 500;
            }

            return result;
        }
        #endregion

        #region 即将冻结客户
        /// <summary>
        /// 获取即将冻结客户列表
        /// </summary>
        /// <param name="request">冻结客户查询实体</param>
        /// <returns>返回即将冻结客户列表</returns>
        public async Task<TableData> GetWillFreezeCustomer(CustomerReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();
            QueryTime qt = _saleBusinessMethodHelp.TimeRange(request.TimeRange);
            if (qt == null || qt.endTime == null || qt.startTime == null)
            {
                //查询即将冻结客户
                var objs = UnitWork.Find<PayWillFreezeCustomer>(null);
                var willFreezeCustomers = objs.WhereIf(!string.IsNullOrWhiteSpace(request.SaleUser), r => r.SaleId.Contains(request.SaleUser) || r.SaleName.Contains(request.SaleUser))
                                              .WhereIf(!string.IsNullOrWhiteSpace(request.CustomerCodeOrName), r => r.CardCode.Contains(request.CustomerCodeOrName) || r.CardName.Contains(request.CustomerCodeOrName));

                //分页按照创建时间排序
                willFreezeCustomers = willFreezeCustomers.OrderBy(r => r.FreezeDateTime);
                var willFreezeCustomerList = await willFreezeCustomers.Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
                var willFreezeCustomersLists = willFreezeCustomerList.Select(r => new
                {
                    r.Id,
                    r.SaleId,
                    SaleDeptName = _userDepartMsgHelp.GetUserDepart(Convert.ToInt32(r.SaleId)),
                    r.SaleName,
                    r.CardCode,
                    r.CardName,
                    r.FreezeDateTime,
                    r.Remark,
                    r.CreateUserId,
                    r.CreateUserName,
                    CreateDeptName = _userDepartMsgHelp.GetUserOrgName(r.CreateUserId),
                    r.CreateTime,
                    r.UpdateUserId,
                    UpdateDeptName = _userDepartMsgHelp.GetUserOrgName(r.UpdateUserId),
                    r.UpdateUserName,
                    r.UpdateTime
                });

                result.Data = willFreezeCustomersLists;
                result.Count = willFreezeCustomers.Count();
                result.Message = "获取成功";
            }
            else
            {
                //查询即将冻结客户
                var objs = UnitWork.Find<PayWillFreezeCustomer>(r => r.CreateTime >= Convert.ToDateTime(qt.startTime) && r.CreateTime <= Convert.ToDateTime(qt.endTime));
                var willFreezeCustomers = objs.WhereIf(!string.IsNullOrWhiteSpace(request.SaleUser), r => r.SaleId.Contains(request.SaleUser) || r.SaleName.Contains(request.SaleUser))
                                              .WhereIf(!string.IsNullOrWhiteSpace(request.CustomerCodeOrName), r => r.CardCode.Contains(request.CustomerCodeOrName) || r.CardName.Contains(request.CustomerCodeOrName));

                //分页按照创建时间排序
                willFreezeCustomers = willFreezeCustomers.OrderBy(r => r.FreezeDateTime);
                var willFreezeCustomerList = await willFreezeCustomers.Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
                var willFreezeCustomersLists = willFreezeCustomerList.Select(r => new
                {
                    r.Id,
                    r.SaleId,
                    SaleDeptName = _userDepartMsgHelp.GetUserDepart(Convert.ToInt32(r.SaleId)),
                    r.SaleName,
                    r.CardCode,
                    r.CardName,
                    r.FreezeDateTime,
                    r.Remark,
                    r.CreateUserId,
                    r.CreateUserName,
                    CreateDeptName = _userDepartMsgHelp.GetUserOrgName(r.CreateUserId),
                    r.CreateTime,
                    r.UpdateUserId,
                    UpdateDeptName = _userDepartMsgHelp.GetUserOrgName(r.UpdateUserId),
                    r.UpdateUserName,
                    r.UpdateTime
                });

                result.Data = willFreezeCustomersLists;
                result.Count = willFreezeCustomers.Count();
                result.Message = "获取成功";
            }

            return result;
        }

        /// <summary>
        /// 删除即将冻结客户
        /// </summary>
        /// <param name="Id">即将冻结客户Id</param>
        /// <returns>返回删除结果</returns>
        public async Task<TableData> DeleteWillFreezeCustomer(string Id)
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var payWillFreezeCustomers = await UnitWork.Find<PayWillFreezeCustomer>(r => r.Id == Id).ToListAsync();
            if (payWillFreezeCustomers != null && payWillFreezeCustomers.Count() > 0)
            {
                await UnitWork.DeleteAsync<PayWillFreezeCustomer>(r => r.Id == Id);
                await UnitWork.SaveAsync();
                result.Message = "删除成功";
            }
            else
            {
                result.Message = "删除失败，该客户不存在";
                result.Code = 500;
            }

            return result;
        }
        #endregion

        #region  订单流程
        /// <summary>
        /// 订单流程
        /// </summary>
        /// <param name="saleId">销售订单Id'</param>
        /// <returns>返回流程信息</returns>
        public async Task<TableData> GetSaleOrderFlow(int saleId)
        {
            var result = new TableData();
            if (saleId > 0)
            {
                List<SaleOrderFlowHelp> saleOrderFlowHelps = new List<SaleOrderFlowHelp>();

                #region 提交到工程部
                //获取工程部信息
                MaterialDataReq materialDataReq = new MaterialDataReq();
                materialDataReq.ProjectNo = saleId.ToString();
                materialDataReq.Alpha = await UnitWork.Find<sale_rdr1>(r => r.DocEntry == saleId && r.sbo_id == Define.SBO_ID).Select(r => r.ItemCode).ToListAsync();
                List<BetaSubFinalView> betaSubFinalViews = _manageAccBindApp.GetDataD(materialDataReq).Data;

                //将工程部信息添加到流程
                SaleOrderFlowHelp saleOrderFlowGCHelp = new SaleOrderFlowHelp();
                saleOrderFlowGCHelp.ModelName = "提交至工程部";
                saleOrderFlowGCHelp.Name = "刘静";
                saleOrderFlowGCHelp.Dept = "E2";
                saleOrderFlowGCHelp.DataTime = betaSubFinalViews.Count() == 0 ? DateTime.Now : betaSubFinalViews.FirstOrDefault().Start;
                saleOrderFlowGCHelp.Flag = betaSubFinalViews.Count() == 0 ? false : true;
                saleOrderFlowHelps.Add(saleOrderFlowGCHelp);
                #endregion

                #region 采购
                SaleOrderFlowHelp saleOrderFlowHelp = new SaleOrderFlowHelp();
                saleOrderFlowHelp.ModelName = "采购";
                saleOrderFlowHelp.Flag = false;

                //获取采购单关联表
                var buyporrels = await UnitWork.Find<buy_porrel>(r => r.sbo_id == Define.SBO_ID && r.RelDoc_Entry == saleId).ToListAsync();
                if (buyporrels != null && buyporrels.Count() > 0)
                {
                    var buyopors = await UnitWork.Find<buy_opor>(r => r.CANCELED == "N" && r.sbo_id == Define.SBO_ID && r.DocEntry == (buyporrels.FirstOrDefault()).POR_Entry).ToListAsync();
                    if (buyopors != null && buyopors.Count() > 0)
                    {
                        var comBuyopors = buyopors.Where(r => r.DocStatus == "C").ToList();

                        //采购信息
                        saleOrderFlowHelp.Name = comBuyopors.FirstOrDefault().U_YGMD;
                        saleOrderFlowHelp.Dept = _userDepartMsgHelp.GetUserNameDept(comBuyopors.FirstOrDefault().U_YGMD);
                        if (comBuyopors != null && comBuyopors.Count() > 0)
                        {
                            saleOrderFlowHelp.DataTime = comBuyopors.FirstOrDefault().UpdateDate;
                            saleOrderFlowHelp.Flag = true;
                        }
                        else
                        {
                            saleOrderFlowHelp.DataTime = buyopors.FirstOrDefault().CreateDate;
                            saleOrderFlowHelp.Flag = false;
                        }
                    }
                }

                saleOrderFlowHelps.Add(saleOrderFlowHelp);
                #endregion

                #region 生产
                SaleOrderFlowHelp saleOrderFlowSCHelp = new SaleOrderFlowHelp();
                saleOrderFlowSCHelp.ModelName = "生产";
                saleOrderFlowSCHelp.Flag = false;

                //获取销售订单
                var saleOrdrs = await UnitWork.Find<sale_ordr>(r => r.CANCELED == "N" && r.sbo_id == Define.SBO_ID && r.DocEntry == saleId).ToListAsync();
                if (saleOrdrs != null && saleOrdrs.Count() > 0)
                {
                    int slpCode = (int)saleOrdrs.FirstOrDefault().SlpCode;
                    var userids = UnitWork.Find<sbo_user>(r => r.sale_id == slpCode && r.sbo_id == Define.SBO_ID).Select(r => r.user_id).FirstOrDefault();
                    saleOrderFlowSCHelp.Dept = _userDepartMsgHelp.GetUserIdDepart(Convert.ToInt32(userids));

                    //获取生产单
                    var owors = await UnitWork.Find<product_owor>(r => (r.Status == "P" || r.Status == "R" || r.Status == "L") && r.sbo_id == Define.SBO_ID && r.OriginAbs == saleId).ToListAsync();
                    if (owors != null && owors.Count() > 0)
                    {
                        var oworLs = owors.Where(r => r.Status == "L").ToList();
                        if (oworLs != null && oworLs.Count() > 0)
                        {
                            saleOrderFlowSCHelp.DataTime = owors.FirstOrDefault().UpdateDate;
                            saleOrderFlowSCHelp.Name = owors.FirstOrDefault().U_WO_LTDW;
                            saleOrderFlowSCHelp.Flag = true;
                        }
                        else
                        {
                            saleOrderFlowSCHelp.DataTime = owors.FirstOrDefault().CreateDate;
                            saleOrderFlowSCHelp.Flag = false;
                        }
                    }
                }

                saleOrderFlowHelps.Add(saleOrderFlowSCHelp);
                #endregion

                #region 交货
                SaleOrderFlowHelp saleOrderFlowJHHelp = new SaleOrderFlowHelp();
                saleOrderFlowJHHelp.ModelName = "交货";
                saleOrderFlowJHHelp.Flag = false;

                //获取交货单明细
                var dln1s = await UnitWork.Find<sale_dln1>(r => r.sbo_id == Define.SBO_ID && r.BaseEntry == saleId).ToListAsync();
                if (dln1s != null && dln1s.Count() > 0)
                {
                    //获取交货单
                    var odlns = await UnitWork.Find<sale_odln>(r => r.sbo_id == Define.SBO_ID && r.CANCELED == "N" && r.DocEntry == (dln1s.FirstOrDefault().DocEntry)).ToListAsync();
                    if (odlns != null && odlns.Count() > 0)
                    {
                        int slpCode = (int)odlns.FirstOrDefault().SlpCode;
                        var userids = UnitWork.Find<sbo_user>(r => r.sale_id == slpCode && r.sbo_id == Define.SBO_ID).Select(r => r.user_id).FirstOrDefault();
                        saleOrderFlowJHHelp.Name = await UnitWork.Find<base_user>(r => r.user_id == userids).Select(r => r.user_nm).FirstOrDefaultAsync();
                        saleOrderFlowJHHelp.Dept = _userDepartMsgHelp.GetUserIdDepart(Convert.ToInt32(userids));

                        //获取已清交货单
                        var odlnCs = odlns.Where(r => r.DocStatus == "C").ToList();
                        if (odlnCs != null && odlnCs.Count() > 0)
                        {
                            saleOrderFlowJHHelp.DataTime = odlnCs.FirstOrDefault().UpdateDate;                            
                            saleOrderFlowJHHelp.Flag = true;
                        }
                        else
                        {
                            saleOrderFlowJHHelp.DataTime = odlns.FirstOrDefault().CreateDate;
                            saleOrderFlowJHHelp.Flag = false;
                        }
                    }
                }

                saleOrderFlowHelps.Add(saleOrderFlowJHHelp);
                #endregion

                #region 收款
                SaleOrderFlowHelp saleOrderFlowSKHelp = new SaleOrderFlowHelp();
                saleOrderFlowSKHelp.ModelName = "收款";
                saleOrderFlowSKHelp.Flag = false;
                if (saleOrdrs != null && saleOrdrs.Count() > 0)
                {
                    int slpCode = (int)saleOrdrs.FirstOrDefault().SlpCode;
                    var userids = UnitWork.Find<sbo_user>(r => r.sale_id == slpCode && r.sbo_id == Define.SBO_ID).Select(r => r.user_id).FirstOrDefault();
                    saleOrderFlowSCHelp.Name = await UnitWork.Find<base_user>(r => r.user_id == userids).Select(r => r.user_nm).FirstOrDefaultAsync();
                    saleOrderFlowSCHelp.Dept = _userDepartMsgHelp.GetUserIdDepart(Convert.ToInt32(userids));

                    //获取数据库名
                    string strSql = string.Format("SELECT sql_db,sql_name,sql_pswd,sap_name,sap_pswd,sql_conn,is_open FROM {0}.sbo_info WHERE sbo_id={1}", "nsap_base", Define.SBO_ID);
                    DataTable dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
                    string sboname = dt.Rows[0][0].ToString();

                    //获取应收发票
                    StringBuilder tableName2 = new StringBuilder();
                    tableName2.Append("select SaleNo,sum(DocTotal) as sum_DocTotal,sum(DocTotalFC) as sum_DocTotalFC from (");
                    tableName2.AppendFormat("SELECT T0.DocEntry,T3.DocEntry AS SaleNo,T0.DocTotal,T0.DocTotalFC ");
                    tableName2.AppendFormat(" FROM " + sboname + ".dbo." + "ORCT AS T0 ");
                    tableName2.AppendFormat("LEFT JOIN " + sboname + ".dbo." + "RCT2 AS T4 ON T0.DocEntry = T4.DocNum ");
                    tableName2.AppendFormat("LEFT JOIN " + sboname + ".dbo." + "INV1 AS T1 ON T4.DocEntry = T1.DocEntry ");
                    tableName2.AppendFormat("LEFT JOIN " + sboname + ".dbo." + "DLN1 AS T2 ON T1.BaseEntry = T2.DocEntry AND T1.BaseLine = T2.LineNum AND T1.BaseType = 15 ");
                    tableName2.AppendFormat("LEFT JOIN " + sboname + ".dbo." + "RDR1 AS T3 ON T2.BaseEntry = T3.DocEntry AND T2.BaseLine = T3.LineNum AND T2.BaseType = 17 ");
                    tableName2.AppendFormat(" where T0.Canceled='N' AND T3.DocEntry='" + saleOrdrs.FirstOrDefault().DocEntry + "'");
                    tableName2.AppendFormat(" group by T0.DocEntry,T3.DocEntry,T0.DocTotal,T0.DocTotalFC ");
                    tableName2.AppendFormat(" union ");
                    tableName2.AppendFormat("select Tk.DocEntry,Tk.U_XSDD as SaleNo,Tk.DocTotal,Tk.DocTotalFC");
                    tableName2.AppendFormat(" from " + sboname + ".dbo." + "ORCT Tk where Tk.Canceled='N' and Tk.U_XSDD='" + saleOrdrs.FirstOrDefault().DocEntry + "'");
                    tableName2.AppendFormat(") v1 group by saleNo");
                    DataTable dTable = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, tableName2.ToString(), CommandType.Text, null);
                    List<SaleOrderORCT> saleOrderORCTs = dTable.Tolist<SaleOrderORCT>();
                    if (saleOrderORCTs.Count != 0)
                    {
                        if (saleOrdrs.FirstOrDefault().DocTotal == saleOrderORCTs.FirstOrDefault().sum_DocTotal)
                        {
                            saleOrderFlowSKHelp.DataTime = saleOrdrs.FirstOrDefault().UpdateDate;
                            saleOrderFlowSKHelp.Flag = true;
                        }
                        else
                        {
                            saleOrderFlowSKHelp.DataTime = saleOrdrs.FirstOrDefault().CreateDate;
                        }
                    }
                }

                saleOrderFlowHelps.Add(saleOrderFlowSKHelp);
                #endregion

                #region 完成
                SaleOrderFlowHelp saleOrderFlowComHelp = new SaleOrderFlowHelp();
                saleOrderFlowComHelp.ModelName = "完成";
                saleOrderFlowComHelp.Name = "";
                saleOrderFlowComHelp.Dept = "";
                saleOrderFlowComHelp.DataTime = DateTime.Now;
                saleOrderFlowComHelp.Flag = (saleOrderFlowGCHelp.Flag && saleOrderFlowHelp.Flag && saleOrderFlowSCHelp.Flag && saleOrderFlowJHHelp.Flag && saleOrderFlowSKHelp.Flag) ? true : false;
                saleOrderFlowHelps.Add(saleOrderFlowComHelp);
                #endregion

                result.Data = saleOrderFlowHelps;
            }

            return result;
        }

        /// <summary>
        /// 获取采购进度
        /// </summary>
        /// <param name="saleId">销售订单Id</param>
        /// <returns>返回采购订单进度信息</returns>
        public async Task<TableData> GetSaleBuy(int saleId)
        {
            var result = new TableData();
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("select d.ItemCode,c.DocEntry,c.U_YGMD,c.CreateDate,c.DocDueDate ");
            strSql.AppendFormat("from nsap_bone.sale_ordr a ");
            strSql.AppendFormat("left join nsap_bone.buy_porrel b on a.Docentry = b.RelDoc_Entry ");
            strSql.AppendFormat("left join nsap_bone.buy_opor c on b.POR_Entry = c.Docentry ");
            strSql.AppendFormat("left join nsap_bone.buy_por1 d on c.Docentry = d.DocEntry ");
            strSql.AppendFormat("where a.docentry = " + saleId + "");
            DataTable dTable = UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null);
            result.Data = dTable.Tolist<SaleBuyOPOR>();
            return result;
        }

        /// <summary>
        /// 获取生产订单
        /// </summary>
        /// <param name="saleId">销售订单Id</param>
        /// <returns>返回生产订单信息</returns>
        public async Task<TableData> GetSaleProduct(int saleId)
        {
            var result = new TableData();
            int userId = _serviceBaseApp.GetUserNaspId();
            if (userId > 0)
            {
                //列出"BOM类别权限分配"所对应的产品编码的生产单
                StringBuilder strBomSql = new StringBuilder();
                strBomSql.AppendFormat("select type_id,type_code FROM nsap_bone.store_item_type_user_map WHERE user_id= " + userId + " ");
                DataTable ittab = UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strBomSql.ToString(), CommandType.Text, null);
                var code = string.Empty; var code28 = string.Empty;
                foreach (DataRow item in ittab.Rows)
                {
                    if (item[1].ToString().Equals("R21"))
                    {
                        code = item[1].ToString();
                    }

                    if (item[1].ToString().Equals("R28"))
                    {
                        code28 = item[1].ToString();
                    }
                }

                //获取生产单
                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat("SELECT a.DocEntry ,a.ItemCode,a.PlannedQty,a.CmpltQty,a.Status, ");
                strSql.AppendFormat("(CASE a.Status WHEN 'P' THEN '已计划' WHEN 'R' THEN '已审核' WHEN 'C' THEN '已取消' ELSE '已清' END) as StatusName, ");
                strSql.AppendFormat("a.CreateDate,a.U_WO_LTDW,c.DocEntry as DocNum,c.WhsCode ");
                strSql.AppendFormat("FROM nsap_bone.product_owor as a ");
                strSql.AppendFormat("LEFT JOIN nsap_bone.sale_ordr AS b ON a.OriginAbs = b.DocEntry  and a.sbo_id = b.sbo_id ");
                strSql.AppendFormat("LEFT JOIN nsap_bone.product_ign1 as c on a.DocEntry = c.BaseEntry and a.sbo_id = c.sbo_id ");
                strSql.AppendFormat("LEFT JOIN nsap_bone.product_oign as d on c.docEntry  = d.DocEntry and c.sbo_id = d.sbo_id ");
                strSql.AppendFormat("WHERE 1=1 AND a.sbo_id = " + Define.SBO_ID + " AND b.DocEntry = " + saleId + " ");
                if (ittab != null && ittab.Rows.Count > 0)
                {
                    strSql.AppendFormat("AND ( EXISTS (select 1 from nsap_bone.store_item_type_user_map m WHERE m.user_id=" + userId + " and left(a.itemcode,LENGTH(m.type_code))=m.type_code)");
                    if (code.Equals("R21"))
                    {
                        strSql.AppendFormat(" OR w.ItemCode LIKE '%R21' ");
                    }

                    if (code28.Equals("R28"))
                    {
                        strSql.AppendFormat(" OR w.ItemCode LIKE '%R28%' ");
                    }

                    strSql.AppendFormat(") ");
                }

                strSql.AppendFormat("order by a.docentry DESC ");
                DataTable dTable = UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null);
                result.Data = dTable.Tolist<SaleProductOWOR>();
            }

            return result;
        }

        /// <summary>
        /// 获取交货单信息
        /// </summary>
        /// <param name="saleId">销售订单Id</param>
        /// <returns>返回交货单信息</returns>
        public async Task<TableData> GetSaleODLN(int saleId)
        {
            var result = new TableData();

            //获取数据库名
            string strTableName = string.Format("SELECT sql_db,sql_name,sql_pswd,sap_name,sap_pswd,sql_conn,is_open FROM {0}.sbo_info WHERE sbo_id={1}", "nsap_base", Define.SBO_ID);
            DataTable dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strTableName, CommandType.Text, null);
            string sboname = dt.Rows[0][0].ToString();

            //获取交货单
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT a.DocEntry,CASE WHEN 1 = 1 THEN a.CardName ELSE '******' END AS CardName,CASE WHEN 1 = 1 THEN a.DocTotal ELSE 0 END AS DocTotal,");
            strSql.AppendFormat("'' AS BuyDocEntry,'' AS TransportName,'' AS TransportID,'' AS TransportSum,");
            strSql.AppendFormat("a.DocDate ,b.ItemCode,b.Quantity,a.CreateDate ");
            strSql.AppendFormat("FROM " + sboname + ".dbo.ODLN a LEFT JOIN " + sboname + ".dbo.DLN1 b on a.DocEntry = b.DocEntry ");
            strSql.AppendFormat("LEFT JOIN " + sboname + ".dbo.OSLP c ON a.SlpCode = c.SlpCode ");
            strSql.AppendFormat("LEFT JOIN " + sboname + ".dbo.OCRD d ON a.CardCode = d.CardCode ");
            strSql.AppendFormat("LEFT JOIN " + sboname + ".dbo.OCTG e ON a.GroupNum = e.GroupNum WHERE b.BaseEntry = " + saleId + " ORDER BY a.docentry DESC");
            DataTable thistab = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
            thistab.Columns[3].MaxLength = -1;
            thistab.Columns[4].MaxLength = -1;
            thistab.Columns[5].MaxLength = -1;
            thistab.Columns[6].MaxLength = -1;

            //获取物流公司和运输单号
            foreach (DataRow odlnrow in thistab.Rows)
            {
                string docnum = odlnrow["DocEntry"].ToString();
                DataTable thist = GetSalesDelivery_PurchaseOrderList(docnum, Define.SBO_ID.ToString());
                string buyentry = "";
                string transname = "";
                string transid = "";
                double transsum = 0.00;
                string tempname = "";
                string transDocTotal = "";
                for (int i = 0; i < thist.Rows.Count; i++)
                {
                    transsum += double.Parse(thist.Rows[i]["DocTotal"].ToString());// 交货对应采购单总金额

                    //快递单号，对应采购单编号
                    if (string.IsNullOrEmpty(buyentry))
                    {
                        buyentry = thist.Rows[i]["Buy_DocEntry"].ToString();
                        transid = string.IsNullOrEmpty(thist.Rows[i]["LicTradNum"].ToString()) ? "000000000" : thist.Rows[i]["LicTradNum"].ToString();
                        tempname = thist.Rows[i]["CardName"].ToString();
                        transname = tempname;
                        transDocTotal = thist.Rows[i]["DocTotal"].ToString();
                    }
                    else
                    {
                        buyentry += ";" + thist.Rows[i]["Buy_DocEntry"].ToString();
                        transid += ";" + (string.IsNullOrEmpty(thist.Rows[i]["LicTradNum"].ToString()) ? "000000000" : thist.Rows[i]["LicTradNum"].ToString());

                        //物流公司名称如果连续重复，则只显示第一个
                        if (tempname != thist.Rows[i]["CardName"].ToString())
                        {
                            tempname = thist.Rows[i]["CardName"].ToString();
                        }
                        else
                        {
                            tempname = "";
                        }

                        transname += ";;" + tempname;
                        transDocTotal += ";" + thist.Rows[i]["DocTotal"].ToString();
                    }
                }

                odlnrow["BuyDocEntry"] = buyentry.ToString();
                odlnrow["TransportName"] = transname.ToString();
                odlnrow["TransportID"] = transid;
                odlnrow["TransportSum"] = transsum.ToString() + ";" + transDocTotal;
            }

            //数据集映射实体
            result.Data = thistab.Tolist<SaleODLN>();
            return result;
        }

        /// <summary>
        /// 获取交货订单运输信息
        /// </summary>
        /// <param name="DeliveryId">交货单单据编号</param>
        /// <param name="SboId">账套Id</param>
        /// <returns>返回交货订单信息</returns>
        public DataTable GetSalesDelivery_PurchaseOrderList(string DeliveryId, string SboId)
        {
            string lstr = string.Format(@"select t0.Buy_DocEntry,t1.CardCode,t1.CardName,t1.DocTotal,t1.LicTradNum from {0}.sale_transport t0 INNER JOIN {0}.buy_opor t1 on t1.DocEntry=t0.Buy_DocEntry and t1.sbo_id=t0.SboId and t1.CANCELED='N' WHERE t0.Base_DocType=24 and t0.Base_DocEntry={1} and t0.SboId={2} ", "nsap_bone", DeliveryId, SboId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, lstr, CommandType.Text, null);
        }

        /// <summary>
        /// 获取收款单信息
        /// </summary>
        /// <param name="saleId">销售订单Id</param>
        /// <returns>返回销售收款信息</returns>
        public async Task<TableData> GetSaleORCT(int saleId)
        {
            var result = new TableData();

            //获取数据库名
            string strTableName = string.Format("SELECT sql_db,sql_name,sql_pswd,sap_name,sap_pswd,sql_conn,is_open FROM {0}.sbo_info WHERE sbo_id={1}", "nsap_base", Define.SBO_ID);
            DataTable dtable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strTableName, CommandType.Text, null);
            string sboname = dtable.Rows[0][0].ToString();

            //获取销售收款单
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT T0.DocEntry,(SELECT TOP 1 T2.BaseEntry FROM " + sboname + ".dbo.RCT2 AS T4 LEFT JOIN neware_202005.dbo.INV1 AS T1 ON T4.DocEntry = T1.DocEntry LEFT JOIN " + sboname + ".dbo.DLN1 AS T2 ON T1.BaseEntry = T2.DocEntry AND T1.BaseLine = T2.LineNum AND T1.BaseType = 15 WHERE T4.DocNum = T0.DocEntry AND T2.BaseType = 17) AS OrderNo,T0.TransId,T0.DocTotal,T0.OpenBal,T0.U_XSDD,'' AS SettleType,T0.DocCurr,T0.DocTotalFC,T0.OpenBalFc, ");
            strSql.AppendFormat("SE.DocCur SEDocCur,SE.DocTotal AS SEDocTotal,SE.DocTotalFC AS SEDocTotalFC,0 AS sum_DocTotal,0 AS sum_DocTotalFC ");
            strSql.AppendFormat("FROM " + sboname + ".dbo.ORCT AS T0 LEFT OUTER JOIN " + sboname + ".dbo.ORDR AS SE ON T0.U_XSDD = SE.DocEntry WHERE ");
            strSql.AppendFormat("SE.DocEntry = " + saleId + " ORDER BY t0.updatedate DESC");
            DataTable dt = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
            dt.Columns[6].MaxLength = -1;
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow tRow in dt.Rows)
                {
                    tRow["SettleType"] = GetMyFieldValueForSalesReceive("U_SettleType", tRow["DocEntry"].ToString(), Define.SBO_ID.ToString());
                    DataTable sumtab = GetSumSalesReceiveForOrder(tRow["U_XSDD"].ToString(), sboname);
                    if (sumtab != null && sumtab.Rows.Count > 0)
                    {
                        tRow["sum_DocTotal"] = sumtab.Rows[0]["sum_DocTotal"];
                        tRow["sum_DocTotalFC"] = sumtab.Rows[0]["sum_DocTotalFC"];
                    }
                }
            }

            //列表获取未收金额
            List<SaleReceORCT> saleReceORCTs = dt.Tolist<SaleReceORCT>();
            foreach (SaleReceORCT item in saleReceORCTs)
            {
                item.NoReceMoney = item.SEDocTotal - item.DocTotal;
            }

            result.Data = saleReceORCTs;
            return result;
        }

        /// <summary>
        /// 获取设置类型
        /// </summary>
        /// <param name="fieldName">设置类型</param>
        /// <param name="docentry">单据编号</param>
        /// <param name="sboid">账套id</param>
        /// <returns>返回设置类型</returns>
        public string GetMyFieldValueForSalesReceive(string fieldName, string docentry, string sboid)
        {
            string selsql = string.Format("select {0} from {1}.finance_orct where docentry={2} and sbo_id={3}", fieldName, "nsap_bone", docentry, sboid);
            object obj = UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, selsql, CommandType.Text, null);
            return obj == null ? "" : obj.ToString();
        }

        /// <summary>
        /// 获取实收总金额
        /// </summary>
        /// <param name="OrderNo">销售订单号</param>
        /// <param name="sboname">数据库名</param>
        /// <returns>返回收款实际总金额</returns>
        public DataTable GetSumSalesReceiveForOrder(string OrderNo, string sboname)
        {
            StringBuilder tableName2 = new StringBuilder();
            tableName2.Append("select SaleNo,sum(DocTotal) as sum_DocTotal,sum(DocTotalFC) as sum_DocTotalFC from (");
            tableName2.AppendFormat("SELECT T0.DocEntry,T3.DocEntry AS SaleNo,T0.DocTotal,T0.DocTotalFC ");
            tableName2.AppendFormat(" FROM " + sboname + ".dbo." + "ORCT AS T0 ");
            tableName2.AppendFormat("LEFT JOIN " + sboname + ".dbo." + "RCT2 AS T4 ON T0.DocEntry = T4.DocNum ");
            tableName2.AppendFormat("LEFT JOIN " + sboname + ".dbo." + "INV1 AS T1 ON T4.DocEntry = T1.DocEntry ");
            tableName2.AppendFormat("LEFT JOIN " + sboname + ".dbo." + "DLN1 AS T2 ON T1.BaseEntry = T2.DocEntry AND T1.BaseLine = T2.LineNum AND T1.BaseType = 15 ");
            tableName2.AppendFormat("LEFT JOIN " + sboname + ".dbo." + "RDR1 AS T3 ON T2.BaseEntry = T3.DocEntry AND T2.BaseLine = T3.LineNum AND T2.BaseType = 17 ");
            tableName2.AppendFormat(" WHERE T0.Canceled='N' AND T3.DocEntry='" + OrderNo + "'");
            tableName2.AppendFormat(" group by T0.DocEntry,T3.DocEntry,T0.DocTotal,T0.DocTotalFC ");
            tableName2.AppendFormat(" union ");
            tableName2.AppendFormat("select Tk.DocEntry,Tk.U_XSDD as SaleNo,Tk.DocTotal,Tk.DocTotalFC");
            tableName2.AppendFormat(" from " + sboname + ".dbo." + "ORCT Tk where Tk.Canceled='N' and Tk.U_XSDD='" + OrderNo + "'");
            tableName2.AppendFormat(") v1 group by saleNo");
            DataTable tempTab = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, tableName2.ToString(), CommandType.Text, null);
            return tempTab;
        }

        /// <summary>
        /// 获取退货/贷项凭证信息
        /// </summary>
        /// <param name="saleId"></param>
        /// <returns>返回销售退货/应收贷项凭证信息</returns>
        public async Task<TableData> GetSaleORDNorORIN(int saleId)
        {
            var result = new TableData();

            //获取数据库名
            string strTableName = string.Format("SELECT sql_db,sql_name,sql_pswd,sap_name,sap_pswd,sql_conn,is_open FROM {0}.sbo_info WHERE sbo_id={1}", "nsap_base", Define.SBO_ID);
            DataTable dtable = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strTableName, CommandType.Text, null);
            string sboname = dtable.Rows[0][0].ToString();

            //获取销售退货和贷项凭证
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("SELECT a.DocEntry,a.CreateDate,a.DocStatus,(CASE a.DocStatus WHEN 'O' THEN '未清' ELSE '已清' END) as StatusName,");
            strSql.AppendFormat("b.ItemCode,b.Quantity,b.Price,b.Quantity * b.Price AS DocTotal ");
            strSql.AppendFormat("FROM " + sboname + ".dbo.ORDN a ");
            strSql.AppendFormat("LEFT JOIN " + sboname + ".dbo.RDN1 b ON a.DocEntry = b.DocEntry ");
            strSql.AppendFormat("LEFT JOIN " + sboname + ".dbo.ODLN c ON b.BaseEntry = c.DocEntry ");
            strSql.AppendFormat("LEFT JOIN " + sboname + ".dbo.DLN1 d ON c.DocEntry = d.DocEntry ");
            strSql.AppendFormat("LEFT JOIN " + sboname + ".dbo.ORDR e ON d.BaseEntry = e.DocEntry ");
            strSql.AppendFormat("WHERE a.CANCELED = 'N' AND a.DocEntry = " + saleId + " UNION ");
            strSql.AppendFormat("SELECT a.DocEntry,a.CreateDate,a.DocStatus,(CASE a.DocStatus WHEN 'O' THEN '未清' ELSE '已清' END) as StatusName,");
            strSql.AppendFormat("b.ItemCode,b.Price,b.Quantity,b.Quantity * b.Price AS DocTotal ");
            strSql.AppendFormat("FROM " + sboname + ".dbo.ORIN a ");
            strSql.AppendFormat("LEFT JOIN " + sboname + ".dbo.RIN1 b ON a.DocEntry = b.DocEntry ");
            strSql.AppendFormat("LEFT JOIN " + sboname + ".dbo.OINV c ON b.BaseEntry = c.DocEntry ");
            strSql.AppendFormat("LEFT JOIN " + sboname + ".dbo.INV1 d ON c.DocEntry = d.DocEntry ");
            strSql.AppendFormat("LEFT JOIN " + sboname + ".dbo.ODLN e ON d.BaseEntry = e.DocEntry ");
            strSql.AppendFormat("LEFT JOIN " + sboname + ".dbo.DLN1 f ON e.DocEntry = f.DocEntry ");
            strSql.AppendFormat("LEFT JOIN " + sboname + ".dbo.ORDR g ON f.BaseEntry = g.DocEntry ");
            strSql.AppendFormat("WHERE a.CANCELED = 'N' AND a.DocEntry = " + saleId + "");
            DataTable tempTab = UnitWork.ExcuteSqlTable(ContextType.SapDbContextType, strSql.ToString(), CommandType.Text, null);
            result.Data = tempTab.Tolist<SaleReturnGoods>();
            return result;
        }
        #endregion
    }
}
