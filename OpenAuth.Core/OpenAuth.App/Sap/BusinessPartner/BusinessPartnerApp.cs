using Infrastructure;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.App.Sap.Request;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Interface;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Extensions;
using OpenAuth.Repository.Domain;
using System.Linq.Dynamic.Core;
using System.IO;
using OpenAuth.Repository.Domain.Serve;

namespace OpenAuth.App.Sap.BusinessPartner
{
    public class BusinessPartnerApp : OnlyUnitWorkBaeApp
    {


        public BusinessPartnerApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {

        }

        public async Task<TableData> Get(QueryBusinessPartnerListReq req)
        {
            var result = new TableData();
            var query = from a in UnitWork.Find<OCRD>(null)
                        join b in UnitWork.Find<OSLP>(null) on a.SlpCode equals b.SlpCode into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<OCRY>(null) on a.Country equals c.Code into ac
                        from c in ac.DefaultIfEmpty()
                        join d in UnitWork.Find<OCST>(null) on a.State1 equals d.Code into ad
                        from d in ad.DefaultIfEmpty()
                        join e in UnitWork.Find<OCRY>(null) on a.MailCountr equals e.Code into ae
                        from e in ae.DefaultIfEmpty()
                        select new { a, b, c, d, e };
            query = query.WhereIf(!string.IsNullOrWhiteSpace(req.CardCodeOrCardName), q => q.a.CardCode.Contains(req.CardCodeOrCardName) || q.a.CardName.Contains(req.CardCodeOrCardName));
            var query2 = query.Select(q => new
            {
                q.a.CardCode,
                q.a.CardName,
                q.a.CntctPrsn,
                q.b.SlpName,
                q.a.Currency,
                q.a.Balance,
                q.a.U_Name,
                Address = $"{ q.a.ZipCode ?? "" }{ q.c.Name ?? "" }{ q.d.Name ?? "" }{ q.a.City ?? ""}{ q.a.Building ?? "" }",
                Address2 = $"{ q.a.MailZipCod ?? "" }{ q.e.Name ?? "" }{ q.d.Name ?? "" }{ q.a.MailCity ?? "" }{ q.a.MailBuildi ?? "" }",
                q.a.U_FPLB,
                q.a.SlpCode
            });


            //var propertyStr = string.Join(',', properties.Select(u => u.Key));
            //result.columnHeaders = properties;
            result.Data = await query2//.OrderBy(u => u.Id)
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync();///Select($"new ({propertyStr})");
            result.Count = query2.Count();
            return result;
        }
        public async Task<TableData> Load(QueryBusinessPartnerListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser=loginContext.User;
            var loginRoles = loginContext.Roles;
            var loginOrgs = loginContext.Orgs;
            if (loginContext.User.Account == Define.USERAPP) 
            {
                loginUser = await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.AppUserId).Select(a => a.User).FirstOrDefaultAsync();
                var roleIds = await UnitWork.Find<Relevance>(r => r.Key == Define.USERROLE && r.FirstId.Equals(loginUser.Id)).Select(r=>r.SecondId).ToListAsync();
                loginRoles = await UnitWork.Find<Role>(r => roleIds.Contains(r.Id)).ToListAsync();
                var orgIds = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && r.FirstId.Equals(loginUser.Id)).Select(r => r.SecondId).ToListAsync();
                loginOrgs = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(r => orgIds.Contains(r.Id)).ToListAsync();
            }
            var result = new TableData();
            var query = from a in UnitWork.Find<OCRD>(null)
                        join b in UnitWork.Find<OSLP>(null) on a.SlpCode equals b.SlpCode into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<OCRG>(null) on (int)a.GroupCode equals c.GroupCode into ac
                        from c in ac.DefaultIfEmpty()
                        join d in UnitWork.Find<OIDC>(null) on a.Indicator equals d.Code into ad
                        from d in ad.DefaultIfEmpty()
                        join e in UnitWork.Find<OHEM>(null) on a.DfTcnician equals e.empID into ae
                        from e in ae.DefaultIfEmpty()
                        join f in UnitWork.Find<OCRY>(null) on a.Country equals f.Code into af
                        from f in af.DefaultIfEmpty()
                        join g in UnitWork.Find<OCST>(null) on a.State1 equals g.Code into ag
                        from g in ag.DefaultIfEmpty()
                        select new { a, b, c, d, e, f, g };
            List<string> carCode = new List<string>();
            if (!string.IsNullOrWhiteSpace(req.ManufSN))
            {
                carCode = await UnitWork.Find<OINS>(null).Where(o => o.manufSN.Contains(req.ManufSN)).Select(o => o.customer).ToListAsync();
                if (carCode.Count == 0)
                {
                    carCode = await UnitWork.Find<ServiceOins>(s => s.manufSN.Contains(req.ManufSN)).Select(s => s.customer).ToListAsync();
                }

            }
            query = query.WhereIf(!string.IsNullOrWhiteSpace(req.CardCodeOrCardName), q => q.a.CardCode.Contains(req.CardCodeOrCardName) || q.a.CardName.Contains(req.CardCodeOrCardName))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.ManufSN), q => carCode.Contains(q.a.CardCode))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.slpName), q => q.b.SlpName.Contains(req.slpName));
            if (req.PageType == 2)
            {
                var cardCodes = await UnitWork.Find<SharingPartner>(null).Select(s => s.CardCode).ToListAsync();
                query = query.Where(q => cardCodes.Contains(q.a.CardCode));
            }
            else if (req.PageType == 3)
            {
                var cardCodes = await UnitWork.Find<SharingPartner>(null).Select(s => s.CardCode).ToListAsync();
                query = query.Where(q => !cardCodes.Contains(q.a.CardCode));
            }
            var query2 = await query.Select(q => new
            {
                q.a.CardCode,
                q.a.CardName,
                q.a.CntctPrsn,
                q.b.SlpName,
                q.a.Currency,
                Balance = q.a.Balance ?? 0m,
                Technician = $"{q.e.lastName ?? ""}{q.e.firstName}",
                Address = $"{ q.f.Name ?? "" }{ q.g.Name ?? "" }{ q.a.City ?? "" }{ q.a.Building ?? "" }",
                Address2 = $"{ q.f.Name ?? "" }{ q.g.Name ?? "" }{ q.a.MailCity ?? "" }{ q.a.MailBuildi ?? "" }",
                q.a.Phone1,
                q.a.Cellular,
                q.a.DNotesBal,
                q.a.OrdersBal,
                q.a.OprCount,
                q.a.UpdateDate,
                q.a.DfTcnician,
                BalanceTotal = 0.00m,
                q.a.validFor,
                q.a.validFrom,
                q.a.validTo,
                q.a.ValidComm,
                q.a.frozenFor,
                q.a.frozenFrom,
                q.a.frozenTo,
                q.a.FrozenComm,
                q.a.QryGroup2,
                q.a.QryGroup3,
                q.c.GroupName,
                q.a.Free_Text,
                q.a.U_FPLB,
                q.a.SlpCode,
                q.a.U_Name,
            }).ToListAsync();
            if (loginUser.Account != Define.SYSTEM_USERNAME && !loginRoles.Any(c => c.Name == "呼叫中心"))
            {
                var cardCodes = await UnitWork.Find<SharingPartner>(null).Select(s => s.CardCode).ToListAsync();
                if (loginRoles.Any(r => r.Name == "销售员") && loginRoles.Any(r => r.Name == "售后技术员"))
                {
                    var orgIds = loginOrgs.Select(o => o.Id).ToList();
                    var userIds = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && orgIds.Contains(r.SecondId)).Select(r => r.FirstId).ToListAsync();
                    var userNames = await UnitWork.Find<User>(u => userIds.Contains(u.Id)).Select(u => u.Name).ToListAsync();
                    var userId = (await UnitWork.Find<NsapUserMap>(n => n.UserID.Equals(loginUser.Id)).FirstOrDefaultAsync())?.NsapUserId;
                    var slpCode = (await UnitWork.Find<sbo_user>(s => s.user_id == userId && s.sbo_id == Define.SBO_ID).FirstOrDefaultAsync())?.sale_id;
                    query2 = query2.Where(q => q.SlpCode == slpCode || userNames.Contains(q.Technician) || cardCodes.Contains(q.CardCode)).ToList();
                }
                else if (loginRoles.Any(r => r.Name == "销售员"))
                {
                    var userId = (await UnitWork.Find<NsapUserMap>(n => n.UserID.Equals(loginUser.Id)).FirstOrDefaultAsync())?.NsapUserId;
                    var slpCode = (await UnitWork.Find<sbo_user>(s => s.user_id == userId && s.sbo_id == Define.SBO_ID).FirstOrDefaultAsync())?.sale_id;
                    query2 = query2.Where(q => q.SlpCode == slpCode || cardCodes.Contains(q.CardCode)).ToList();

                }
                else if (loginRoles.Any(r => r.Name == "售后技术员"))
                {
                    var orgIds = loginOrgs.Select(o => o.Id).ToList();
                    var userIds = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && orgIds.Contains(r.SecondId)).Select(r => r.FirstId).ToListAsync();
                    var userNames = await UnitWork.Find<User>(u => userIds.Contains(u.Id)).Select(u => u.Name).ToListAsync();
                    query2 = query2.Where(q => userNames.Contains(q.Technician) || cardCodes.Contains(q.CardCode)).ToList();
                }
                else
                {
                    query2 = query2.Where(q => cardCodes.Contains(q.CardCode)).ToList();
                }
            }
            if (!string.IsNullOrWhiteSpace(req.Technician)) query2 = query2.Where(q => q.Technician.Contains(req.Technician)).ToList();
            if (!string.IsNullOrWhiteSpace(req.Address)) query2 = query2.Where(q => q.Address2.Contains(req.Address)).ToList();

            result.Data = query2.Skip((req.page - 1) * req.limit).Take(req.limit);
            result.Count = query2.Count;

            return result;
        }

        /// <summary>
        /// 服务呼叫查询单个客户信息 by zlg 2020.09.15
        /// </summary>
        /// <param name="CardCode"></param>
        /// <returns></returns>
        public async Task<TableData> GetBusinessAssociate(string CardCode)
        {
            var result = new TableData();
            var query = from a in UnitWork.Find<OCRD>(null)
                        join b in UnitWork.Find<OSLP>(null) on a.SlpCode equals b.SlpCode into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<OCRG>(null) on (int)a.GroupCode equals c.GroupCode into ac
                        from c in ac.DefaultIfEmpty()
                        join d in UnitWork.Find<OIDC>(null) on a.Indicator equals d.Code into ad
                        from d in ad.DefaultIfEmpty()
                        join e in UnitWork.Find<OHEM>(null) on a.DfTcnician equals e.empID into ae
                        from e in ae.DefaultIfEmpty()
                        join f in UnitWork.Find<OCRY>(null) on a.Country equals f.Code into af
                        from f in af.DefaultIfEmpty()
                        join g in UnitWork.Find<OCST>(null) on a.State1 equals g.Code into ag
                        from g in ag.DefaultIfEmpty()
                        select new { a, b, c, d, e, f, g };
            query = query.Where(q => q.a.CardCode.Equals(CardCode));

            var BusinessAssociate = await query.Select(q => new
            {
                q.a.CardCode,
                q.a.CardName,
                q.a.CntctPrsn,
                q.b.SlpName,
                q.a.Currency,
                Balance = q.a.Balance ?? 0m,
                Technician = $"{q.e.lastName ?? ""}{q.e.firstName}",
                Address = $"{ q.a.ZipCode ?? "" }{ q.f.Name ?? "" }{ q.g.Name ?? "" }{ q.a.City ?? "" }{ q.a.Building ?? "" }",
                Address2 = $"{ q.a.MailZipCod ?? "" }{ q.f.Name ?? "" }{ q.g.Name ?? "" }{ q.a.MailCity ?? "" }{ q.a.MailBuildi ?? "" }",
                q.a.Phone1,
                q.a.Cellular,
                q.a.DNotesBal,
                q.a.OrdersBal,
                q.a.OprCount,
                q.a.UpdateDate,
                q.a.DfTcnician,
                BalanceTotal = 0.00m,
                q.a.validFor,
                q.a.validFrom,
                q.a.validTo,
                q.a.ValidComm,
                q.a.frozenFor,
                q.a.frozenFrom,
                q.a.frozenTo,
                q.a.FrozenComm,
                q.a.QryGroup2,
                q.a.QryGroup3,
                q.c.GroupName,
                q.a.Free_Text,
                q.a.U_FPLB,
                q.a.U_Name,
                q.a.SlpCode
            }).FirstOrDefaultAsync();
            result.Data = BusinessAssociate;
            return result;
        }
        /// <summary>
        /// 通过客户编码得到业务伙伴名称，联系人与地址列表
        /// </summary>
        /// <param name="cardCode">客户编码</param>
        /// <returns></returns>
        public async Task<BusinessPartnerDetailsResp> GetDetails(string cardCode)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //var properties = loginContext.GetProperties("businesspartner");

            //if (properties == null || properties.Count == 0)
            //{
            //    throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            //}
            var obj = from a in UnitWork.Find<OCRD>(null)
                      join b in UnitWork.Find<OSLP>(null) on a.SlpCode equals b.SlpCode into ab
                      from b in ab.DefaultIfEmpty()
                      join e in UnitWork.Find<OHEM>(null) on a.DfTcnician equals e.empID into ae
                      from e in ae.DefaultIfEmpty()
                      select new { a, b, e };
            obj = obj.Where(o => o.a.CardCode.Equals(cardCode));

            var query = obj.Select(q => new
            {
                q.a.CardCode,
                q.a.CardName,
                q.a.CntctPrsn,
                q.a.Phone1,
                q.a.SlpCode,
                q.b.SlpName,
                q.a.U_Name,
                TechID = q.a.DfTcnician,
                TechName = $"{q.e.lastName ?? ""}{q.e.firstName}",
                CntctPrsnList = UnitWork.Find<OCPR>(null).Where(o => o.CardCode.Equals(q.a.CardCode)).ToList(),
                AddressList = UnitWork.Find<CRD1>(null).Where(o => o.CardCode.Equals(q.a.CardCode) && o.Address.Equals(q.a.ShipToDef)).ToList(),
            });

            var rltList = await query.FirstOrDefaultAsync();
            var result = rltList.MapTo<BusinessPartnerDetailsResp>();
            return result;
        }

        /// <summary>
        /// 验证是否存在客户（新威智能App）
        /// </summary>
        /// <param name="cardCode">客户编码</param>
        /// <param name="customerName">客户编码</param>
        /// <param name="userName">帐户</param>
        /// <param name="passWord">密码</param>
        /// <param name="appUserId">密码</param>
        /// <returns></returns>
        public async Task<TableData> AppGetCustomerCode(string cardCode, string customerName, string userName, string passWord, int appUserId)
        {
            var result = new TableData();
            string nsapId = string.Empty;
            int clearUserId = 0;
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //判断账户密码是否正确
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(passWord))
            {
                var User = await UnitWork.Find<User>(u => u.Account == userName && u.Password == Encryption.Encrypt(passWord)).FirstOrDefaultAsync();
                if (User == null)
                {
                    throw new CommonException("帐户或密码不正确", 90017);
                }
                nsapId = User.Id;
            }
            //判断当前账号是否存在服务单 若存在不允许进行解绑或绑定操作
            var isExistService = (await UnitWork.Find<ServiceWorkOrder>(w => w.CurrentUserId == appUserId).ToListAsync())?.Count > 0 ? true : false;
            if (isExistService)
            {
                throw new CommonException("当前账号存在服务关系不可绑定", 90019);
            }
            var obj = from a in UnitWork.Find<OCRD>(null)
                      join b in UnitWork.Find<OSLP>(null) on a.SlpCode equals b.SlpCode into ab
                      from b in ab.DefaultIfEmpty()
                      join e in UnitWork.Find<OHEM>(null) on a.DfTcnician equals e.empID into ae
                      from e in ae.DefaultIfEmpty()
                      select new { a, b, e };
            obj = obj.Where(o => o.a.CardCode.ToLower().Equals(cardCode.ToLower()) && o.a.CardName.ToLower().Equals(customerName.ToLower()));

            var rltList = await obj.Select(q => new
            {
                q.a.CardCode
            }).FirstOrDefaultAsync();
            if (rltList == null)
            {
                throw new CommonException("当前客户不存在", 90016);
            }
            //判断若绑定的不是新威尔电子有限公司 解除绑定关系
            if (!"C00550".Equals(rltList.CardCode, StringComparison.OrdinalIgnoreCase))
            {
                var map = await UnitWork.Find<AppUserMap>(a => a.AppUserId == appUserId).FirstOrDefaultAsync();
                if (map != null)
                {
                    //判断技术员和管理员角色不允许修改绑定关系
                    if (map.AppUserRole == 2 || map.AppUserRole == 3)
                    {
                        throw new CommonException("当前帐号无法绑定", 90019);
                    }
                    clearUserId = appUserId;
                    await UnitWork.DeleteAsync(map);
                }
            }
            else
            {
                //添加app与erp绑定关系
                var userMap = await UnitWork.Find<AppUserMap>(a => a.UserID == nsapId).FirstOrDefaultAsync();
                if (userMap == null)
                {
                    var map = new AppUserMap
                    {
                        UserID = nsapId,
                        AppUserId = appUserId,
                        AppUserRole = 1
                    };
                    await UnitWork.AddAsync(map);
                }
                else
                {
                    //判断技术员和管理员角色不允许修改绑定关系
                    if (userMap.AppUserRole == 2 || userMap.AppUserRole == 3)
                    {
                        throw new CommonException("当前Erp帐号不可绑定", 90018);
                    }
                    clearUserId = (int)userMap.AppUserId;
                    await UnitWork.UpdateAsync<AppUserMap>(s => s.UserID == nsapId, o => new AppUserMap
                    {
                        AppUserId = appUserId,
                        AppUserRole = 1
                    });
                }
            }
            await UnitWork.SaveAsync();
            var data = new
            {
                rltList.CardCode,
                nsapId,
                clearUserId
            };
            result.Data = data;
            return result;
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        public async Task<dynamic> GenerateQRCode(GenerateQRCodeReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var globainfo = await UnitWork.Find<GlobalArea>(c => c.Pid != "99").ToListAsync();
            var province = globainfo.Where(c => c.AreaName == req.Province && c.AreaLevel == "1").FirstOrDefault();
            var vaild = false;
            //验证省市区是否正确
            if (province != null)
            {
                var cityinfo = globainfo.Where(c => c.Pid == province.Id && c.AreaName == req.City).FirstOrDefault();
                if (cityinfo != null)
                {
                    var area = globainfo.Where(c => c.Pid == cityinfo.Id && c.AreaName == req.Area).FirstOrDefault();
                    if (area != null) vaild = true;
                }
            }
            if (!vaild) return "";


            string url = "https://app.neware.work/appshare.html";
            var timespan = Infrastructure.Helpers.DateTimeHelper.GetTimeStamp(DateTime.Now, true);
            url = $"{url}?CardCode={req.CardCode}" +
                $"&CardName={System.Web.HttpUtility.UrlEncode(req.CardName)}" +
                $"&Province={System.Web.HttpUtility.UrlEncode(req.Province)}" +
                $"&City={System.Web.HttpUtility.UrlEncode(req.City)}" +
                $"&Area={System.Web.HttpUtility.UrlEncode(req.Area)}" +
                $"&Address={System.Web.HttpUtility.UrlEncode(req.Address)}" +
                $"&Contacter={System.Web.HttpUtility.UrlEncode(req.Contacter)}" +
                $"&Tel={req.Tel}" +
                $"&timespan={timespan}";
            //url = System.Web.HttpUtility.UrlEncode(url);
            string result = url;
            if (req.IsQRCode)
            {
                var logo = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "新威智能logo.png");
                var bitmap = QRCoderHelper.GetLogoQRCode(url, logo, 10);
                result = QRCoderHelper.BitmapToBase64(bitmap);
            }

            return result;
        }

        /// <summary>
        /// 转为共享伙伴
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task AddSharingPartner(QueryBusinessPartnerListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var cardCodes = await UnitWork.Find<SharingPartner>(null).Select(s => s.CardCode).ToListAsync();
            var sharingPartners = req.CardCodes.Where(c => !cardCodes.Contains(c)).Select(c => new SharingPartner { CardCode = c, CreateTime = DateTime.Now, CreateUserId = loginContext.User.Id, CreateUserName = loginContext.User.Name, Id = Guid.NewGuid().ToString() }).ToList();
            await UnitWork.BatchAddAsync<SharingPartner>(sharingPartners.ToArray());
            await UnitWork.SaveAsync();
        }
        /// <summary>
        /// 转为普通伙伴
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task DeleteSharingPartner(QueryBusinessPartnerListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            await UnitWork.DeleteAsync<SharingPartner>(s => req.CardCodes.Contains(s.CardCode));
            await UnitWork.SaveAsync();
        }
    }
}
