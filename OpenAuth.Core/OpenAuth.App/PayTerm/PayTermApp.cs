using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using OpenAuth.App.Response;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.App.PayTerm.PayTermSetHelp;
using NSAP.Entity.Sales;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Order;
using OpenAuth.Repository.Domain.Sap;

namespace OpenAuth.App.PayTerm
{
    public class PayTermApp : OnlyUnitWorkBaeApp
    {
        private IUnitWork _UnitWork;
        private IAuth _auth;
        private ServiceSaleOrderApp _serviceSaleOrderApp;
        private ServiceBaseApp _serviceBaseApp;
        private List<string> RecePayTypes = new List<string>() { "预付/货前款", "货到款", "验收款", "质保款" };

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public PayTermApp(IUnitWork unitWork, IAuth auth, ServiceSaleOrderApp serviceSaleOrderApp, ServiceBaseApp serviceBaseApp) : base(unitWork, auth)
        {
            _UnitWork = unitWork;
            _auth = auth;
            _serviceSaleOrderApp = serviceSaleOrderApp;
            _serviceBaseApp = serviceBaseApp;
        }

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
                DateNumber =  Convert.ToDecimal(r.DateNumber.ToString().Split('.')[1]) == 0 ? Convert.ToInt32(r.DateNumber) : Math.Round(Convert.ToDecimal(r.DateNumber), 2),
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
                return "当前付款条件已经存在，不允许重复添加";
            }

            //判断是否已经存在各阶段节点计算方法
            if (obj.ModuleTypeId == 1 || obj.ModuleName == "各阶段节点计算方法")
            {
                var payTermSets = await UnitWork.Find<PayTermSet>(r => r.ModuleTypeId == 1 || r.ModuleName == "各阶段节点计算方法").ToListAsync();
                if (payTermSets.Count() >= 1)
                {
                    return "各阶段节点计算方法模块已经存在，不允许重复添加";
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
                    await UnitWork.UpdateAsync<PayTermSet>(u => u.Id == obj.Id, u => new PayTermSet
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
                //当可选阶段不为空时，判定是否存在重复条件
                var objs = UnitWork.Find<PayTermSet>((r => r.DateNumber == obj.DateNumber && r.ModuleTypeId == obj.ModuleTypeId && r.ModuleName == r.ModuleName && r.DateUnit == obj.DateUnit)).Include(r => r.PayPhases).ToList();
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

                if (payTermSets != null && payTermSets.Count() > 0)
                {
                    var payTermSetRepeat = payTermSets.Where(r => r.DateNumber == obj.DateNumber && r.ModuleTypeId == obj.ModuleTypeId && r.ModuleName == r.ModuleName && r.DateUnit == obj.DateUnit && r.PayPhaseName == GetPayPhaseName(obj.PayPhases.OrderBy(x => x.PayPhaseType).ToList()));
                    if (payTermSetRepeat != null && payTermSetRepeat.Count() > 0)
                    {
                        isRepeat = false;
                    }
                }
            }
            else
            {
                //当可选阶段为空时，判定是否存在重复条件
                var objs = UnitWork.Find<PayTermSet>(r => r.DateNumber == obj.DateNumber && r.ModuleTypeId == obj.ModuleTypeId && r.ModuleName == r.ModuleName && r.DateUnit == obj.DateUnit).Include(r => r.PayPhases).ToList();
                if (objs != null && objs.Count() > 0)
                {
                    isRepeat = false;
                }
            }

            return isRepeat;
        }

        /// <summary>
        /// 新增付款条件到3.0
        /// </summary>
        /// <param name="rData">销售客户付款条件实体</param>
        /// <returns>成功返回2，失败抛出异常</returns>
        public async Task<string> InsertCrmOctg(saleCrmOctgCfg rData)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            string result = "0";
            string job_id = "0";
            int userID = _serviceBaseApp.GetUserNaspId();
            int funcId = _serviceBaseApp.GetFuncsByUserID("sales/SalesCrmOctgConfiguration.aspx", userID);
            int sboID = _serviceBaseApp.GetUserNaspSboID(userID);
            byte[] job_data = CommonMethodHelp.Serialize(rData);

            //创建流程
            job_id = _serviceSaleOrderApp.WorkflowBuild("付款条件", funcId, userID, job_data, "付款条件", sboID, "", "", 0, 0, 0, "BOneAPI", "NSAP.B1Api.BOneOCTG");//BOneAPI NSAP.B1Api.BOneOCTG
            if (Convert.ToInt32(job_id) > 0)
            {
                //提交流程
                result = _serviceSaleOrderApp.WorkflowSubmit(Convert.ToInt32(job_id), userID, "付款条件", "提交审核", 0);
            }
               
            return result;
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
            string PrepaName = obj.PrepaDay == 0 && obj.PrepaPro == 0 ? "" : (obj.PrepaDay == 0 && obj.PrepaPro != 0 ? "预付" + obj.PrepaPro + "%" : (obj.PrepaDay != 0 && obj.PrepaPro == 0 ? obj.PrepaDay + CommonMethodHelp.GetDateTimeUnit(obj.PrepaUnit) + "内预付0%": obj.PrepaDay + CommonMethodHelp.GetDateTimeUnit(obj.PrepaUnit) + "内预付" + obj.PrepaPro + "%"));
            string BefShiName = (obj.BefShipDay == 0 && obj.BefShipPro == 0 ? "" : (obj.BefShipDay == 0 && obj.BefShipPro != 0 ? "，货前付" + obj.BefShipPro + "%" : (obj.BefShipDay != 0 && obj.BefShipPro == 0 ? "，货前" + obj.BefShipDay + CommonMethodHelp.GetDateTimeUnit(obj.BefShipUnit) + "内付0%" : "，货前" + obj.BefShipDay + CommonMethodHelp.GetDateTimeUnit(obj.BefShipUnit) + "内付" + obj.BefShipPro + "%")));
            string GoodsToName = (obj.GoodsToDay == 0 && obj.GoodsToPro == 0 ? "" : (obj.GoodsToDay == 0 && obj.GoodsToPro != 0 ? "，货到付" + obj.GoodsToPro + "%" : (obj.GoodsToDay != 0 && obj.GoodsToPro == 0 ? "，货到" + obj.GoodsToDay + CommonMethodHelp.GetDateTimeUnit(obj.GoodsToUnit) + "内付0%" : "，货到" + obj.GoodsToDay + CommonMethodHelp.GetDateTimeUnit(obj.GoodsToUnit) + "内付" + obj.GoodsToPro + "%")));
            string AcceptancePayName = GetAcceptancePayName(obj);
            string QualityAssuranceName = GetQualityAssuranceName(obj);
            string groupName = "合同后" + PrepaName + BefShiName + GoodsToName + AcceptancePayName + QualityAssuranceName;

            //判定付款条件是否已经存在
            var crmOctgList = await UnitWork.Find<crm_octg>(r => r.PymntGroup == groupName).ToListAsync();
            var paytermsaves = await UnitWork.Find<PayTermSave>(r => r.GroupNum == groupName).ToListAsync();
            var payTermSets = await UnitWork.Find<PayTermSet>(r => r.ModuleName == "各阶段节点计算方法").ToListAsync();
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

            if (paytermsaves != null && paytermsaves.Count() > 0)
            {
                result.Code = 201;
                result.Data = new
                {
                    Id = crmOctgList.FirstOrDefault().GroupNum,
                    GroupNum = groupName,
                    Message = "4.0中已经存在该配置的付款条件"
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
            scoc.GroupNum = "";
            scoc.sbo_id = "1";
            scoc.PymntGroup = groupName;
            scoc.PrepaDay = obj.PrepaDay.ToString();
            scoc.PrepaPro = obj.PrepaPro.ToString();
            scoc.PayBefShip = obj.BefShipPro.ToString();
            scoc.GoodsToDay = obj.GoodsToDay.ToString();
            scoc.GoodsToPro = obj.GoodsToPro.ToString();

            //3.0付款条件实体
            saleCrmOctg modelCrmOctg = new saleCrmOctg();
            modelCrmOctg.PymntGroup = groupName;
            modelCrmOctg.GroupNum = "0";
            scoc.ModelCrmOctg = modelCrmOctg;

            //付款条件同步到3.0接口
            string isResult = await InsertCrmOctg(scoc);
            if (isResult == "2")
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

                result.Message = "操作成功";
                return result;
            }
            else
            {
                result.Code = 500;
                result.Message = "操作失败";
                return result;
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
                                 join d in await UnitWork.Find<RCT2>(r => r.DocEntry == ordr.DocEntry).Select(r => new SapEntityHelp { DocEntry = r.DocNum, BaseEntry = r.DocEntry, TotalAmount = r.SumApplied, TotalAmountFC = r.AppliedFC }).ToListAsync()
                                 on c.DocEntry equals d.DocEntry 
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
                        rct2s = await UnitWork.Find<RCT2>(r => r.DocEntry == oINVHelp.DocEntry).Select(r => new SapEntityHelp { DocEntry = r.DocNum, BaseEntry = r.DocEntry, TotalAmount = r.SumApplied, TotalAmountFC = r.AppliedFC }).ToListAsync();
                        var orctlist = await UnitWork.Find<ORCT>(r => r.CardCode == oINVHelp.CardCode).Select(r => new SapEntityHelp { DocEntry = r.DocNum, CreateDate = r.CreateDate, DocCur = r.DocCurr, DocRate = r.DocRate }).ToListAsync();
                        if (rct2s != null && rct2s.Count() > 0)
                        {
                            rct2s = (from a in rct2s
                                     join b in orctlist
                                     on a.DocEntry equals b.DocEntry into ab
                                     from b in ab.DefaultIfEmpty()
                                     select new SapEntityHelp
                                     {
                                         DocEntry = b == null ? 0 : b.DocEntry,
                                         BaseEntry = oINVHelp.DocEntry,
                                         CreateDate = b == null? DateTime.Now : b.CreateDate,
                                         DocCur = b == null ? "" : b.DocCur,
                                         DocRate = b == null ? 0 : b.DocRate,
                                         TotalAmount = a.TotalAmount,
                                         TotalAmountFC = a.TotalAmountFC
                                     }).ToList();
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
                    
                    //总体情况
                    SaleReceHelp saleReceHelp = new SaleReceHelp();
                    saleReceHelp.DocEntry = docEntry;
                    saleReceHelp.DocCur = (receDetailHelps.GroupBy(r => new { r.ReceDocCur }).Select(r => r.Key.ReceDocCur).ToList()).FirstOrDefault();
                    saleReceHelp.DocRate = (Convert.ToDecimal((receDetailHelps.GroupBy(r => new { r.ReceDocRate }).Select(r => r.Key.ReceDocRate).ToList()).FirstOrDefault())).ToString();
                    saleReceHelp.SaleReceAmount = ordr.DocTotal == 0 || ordr.DocTotal == null ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(ordr.DocTotal), 2);
                    saleReceHelp.SaleReceAmountFC = ordr.DocTotalFC == 0 || ordr.DocTotal == null ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(ordr.DocTotalFC), 2);
                    saleReceHelp.ReceAmount = receHelps.Sum(r => r.ReceAmount) == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelps.Sum(r => r.ReceAmount)), 2);
                    saleReceHelp.ReceAmountFC = receHelps.Sum(r => r.ReceAmountFC) == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelps.Sum(r => r.ReceAmountFC)), 2);
                    saleReceHelp.ActualAmount = (sapEntityHelpList == null ? 0 : sapEntityHelpList.Sum(r => r.TotalAmount)) == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(sapEntityHelpList == null ? 0 : sapEntityHelpList.Sum(r => r.TotalAmount)) , 2);
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
                        receDetailHelp.ReceAmount = receHelp.ReceAmount == 0 ? "" :  _serviceBaseApp.MoneyToCoin(receHelp.ReceAmount, 2);
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
                                receDetailHelp.WithinLimitDay = DateTime.Now <= Convert.ToDateTime(receHelp.ReceDateTime) ? "" : (DateTime.Now - Convert.ToDateTime(receHelp.ReceDateTime)).Days.ToString() ;
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
                                receDetailHelp.ActualReceDate = Convert.ToDecimal(receHelp.TotalAmount) <= receiveAmount ? (sapEntityHelpList.GroupBy(r => new { r.CreateDate }).Select(r => Convert.ToDateTime(r.Key.CreateDate)).ToList().OrderByDescending(r => r).FirstOrDefault()).ToString("yyyy.MM.dd") : "";
                                receDetailHelp.SaleReceiptNo = string.Join(",", sapEntityHelpList.GroupBy(r => new { r.DocEntry }).Select(r => r.Key.DocEntry).ToList());
                                receDetailHelp.ReceiveAmount = receiveAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(receiveAmount, 2);
                                receDetailHelp.NoReceiveAmount = Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount == 0 ? "" : _serviceBaseApp.MoneyToCoin(Convert.ToDecimal(receHelp.ReceAmount) - receiveAmount, 2);
                                int days = (DateTime.Now - Convert.ToDateTime(receHelp.ReceDateTime)).Days;
                                int recedays = receDetailHelp.ActualReceDate == "" ? 0 : (Convert.ToDateTime(receHelp.ReceDateTime) - Convert.ToDateTime(receDetailHelp.ActualReceDate)).Days;
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
                                    int receDays = receDetailHelp.ActualReceDate == "" ? 0 : (Convert.ToDateTime(receHelp.ReceDateTime) - Convert.ToDateTime(receDetailHelp.ActualReceDate)).Days;
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
                                    int receDays = receDetailHelp.ActualReceDate == "" ? 0 : (Convert.ToDateTime(receHelp.ReceDateTime) - Convert.ToDateTime(receDetailHelp.ActualReceDate)).Days;
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
                                         GroupNum = b.GroupNum,
                                         PymntGroup = b.PymntGroup
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
    }
}
