using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Cache;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.SSO;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.NsapBase;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class UserManagerApp : BaseApp<User>
    {
        private RevelanceManagerApp _revelanceApp;
        private OrgManagerApp _orgManagerApp;

        public User GetByAccount(string account)
        {
            return Repository.FindSingle(u => u.Account == account);
        }

        /// <summary>
        /// 加载当前登录用户可访问的一个部门及子部门全部用户
        /// </summary>
        public TableData Load(QueryUserListReq request)
        {
            var loginUser = _auth.GetCurrentUser();
            IQueryable<User> query = UnitWork.Find<User>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                query = UnitWork.Find<User>(u => u.Name.Contains(request.key) || u.Account.Contains(request.key));
            }

            var userOrgs = from user in query
                           join relevance in UnitWork.Find<Relevance>(u => u.Key == Define.USERORG)
                               on user.Id equals relevance.FirstId into temp
                           from r in temp.DefaultIfEmpty()
                           join org in UnitWork.Find<Repository.Domain.Org>(null)
                               on r.SecondId equals org.Id into orgtmp
                           from o in orgtmp.DefaultIfEmpty()
                           join a in UnitWork.Find<DDBindUser>(null) on user.Id equals a.UserId
                           into usera 
                           from a  in usera.DefaultIfEmpty()
                           join b in UnitWork.Find<DDUserMsg>(null) on a.DDUserId equals b.UserId
                           into ab 
                           from b in ab.DefaultIfEmpty()
                           select new
                           {
                               user.Account,
                               user.Name,
                               user.Id,
                               user.Sex,
                               user.Status,
                               user.BizCode,
                               user.CreateId,
                               user.CreateTime,
                               user.TypeId,
                               user.TypeName,
                               user.ServiceRelations,
                               user.CardNo,
                               r.Key,
                               r.SecondId,
                               OrgId = o.Id,
                               OrgName = o.Name,
                               user.EntryTime,
                               DDUserId = a == null ? "" : a.DDUserId,
                               DDUserName = b == null ? "" : b.UserName
                           };

            //如果请求的orgId不为空
            if (!string.IsNullOrEmpty(request.orgId))
            {
                //如果用户的角色标识是管理员,则查看该组织及子部门下的所有成员
                if (loginUser.Roles.Select(x => x.Identity).Where(x => x != null).Any(x => x.Equals("5")))
                {
                    var cascade = UnitWork.Find<Repository.Domain.Org>(null).Where(o => o.Id == request.orgId).FirstOrDefault()?.CascadeId;
                    var ids = UnitWork.Find<Repository.Domain.Org>(null).Where(o => o.CascadeId.Contains(cascade)).Select(x => x.Id);
                    userOrgs = userOrgs.Where(x => ids.Contains(x.OrgId));
                }
                else
                {
                    var org = loginUser.Orgs.SingleOrDefault(u => u.Id == request.orgId);
                    var cascadeId = org.CascadeId;

                    var orgIds = loginUser.Orgs.Where(u => u.CascadeId.Contains(cascadeId)).Select(u => u.Id).ToArray();

                    //只获取机构里面的用户
                    userOrgs = userOrgs.Where(u => u.Key == Define.USERORG && orgIds.Contains(u.OrgId));
                }
            }
            else  //todo:如果请求的orgId为空，即为跟节点，这时可以额外获取到机构已经被删除的用户，从而进行机构分配。可以根据自己需求进行调整
            {
                var orgIds = loginUser.Orgs.Select(u => u.Id).ToArray();

                //获取用户可以访问的机构的用户和没有任何机构关联的用户（机构被删除后，没有删除这里面的关联关系）
                userOrgs = userOrgs.Where(u => (u.Key == Define.USERORG && orgIds.Contains(u.OrgId)) || (u.OrgId == null));
            }

            var userViews = userOrgs.ToList().GroupBy(b => b.Account).Select(u => new UserView
            {
                Id = u.First().Id,
                Account = u.Key,
                Name = u.First().Name,
                Sex = u.First().Sex,
                Status = u.First().Status,
                CreateTime = u.First().CreateTime,
                CreateUser = u.First().CreateId,
                ServiceRelations = u.First()?.ServiceRelations,
                CardNo = u.First()?.CardNo,
                OrganizationIds = string.Join(",", u.Select(x => x.OrgId)),
                Organizations = string.Join(",", u.Select(x => x.OrgName)),
                EntryTime = u.First().EntryTime,
                DDUserId = u.First().DDUserId,
                DDUserName = u.First().DDUserName

            });

            return new TableData
            {
                Count = userViews.Count(),
                Data = userViews.OrderBy(u => u.Status)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit),
            };
        }

        /// <summary>
        /// 根据token获取最终父节点下的所有用户
        /// </summary>
        /// <returns></returns>
        public TableData GetUsers(QueryUserListReq request)
        {
            var loginUser = _auth.GetCurrentUser();
            var orgIds = new List<string>(); //用户所属最终父节点的id
            //用户的节点cascade
            var cascadeIds = loginUser.Orgs.Select(x => x.CascadeId);
            //最终的父节点cascade
            var parenCascadeIds = UnitWork.Find<Repository.Domain.Org>(null).Where(o => string.IsNullOrWhiteSpace(o.ParentId)).Select(x => new { x.Id, x.CascadeId });
            foreach (var item1 in parenCascadeIds)
            {
                foreach(var item2 in cascadeIds)
                {
                    if (item2.Contains(item1.CascadeId))
                    {
                        orgIds.Add(item1.Id);
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                
            }

            var userOrgs = from user in UnitWork.Find<User>(null)
                           join relevance in UnitWork.Find<Relevance>(u => u.Key == Define.USERORG)
                               on user.Id equals relevance.FirstId into temp
                           from r in temp.DefaultIfEmpty()
                           join org in UnitWork.Find<Repository.Domain.Org>(null)
                               on r.SecondId equals org.Id into orgtmp
                           from o in orgtmp.DefaultIfEmpty()
                           select new
                           {
                               user.Account,
                               user.Name,
                               user.Id,
                               user.Sex,
                               user.Status,
                               user.BizCode,
                               user.CreateId,
                               user.CreateTime,
                               user.TypeId,
                               user.TypeName,
                               user.ServiceRelations,
                               user.CardNo,
                               r.Key,
                               r.SecondId,
                               OrgId = o.Id,
                               OrgName = o.Name
                           };



            var cascadeId = UnitWork.Find<Repository.Domain.Org>(null).FirstOrDefault(o => orgIds.Distinct().Contains(o.Id))?.CascadeId;
            //模糊查询,查询所有包含cascadeId的org
            var orgIdsData = UnitWork.Find<Repository.Domain.Org>(null).Where(o => o.CascadeId.Contains(cascadeId)).Select(x => x.Id).ToList();
            userOrgs = userOrgs.Where(u => orgIdsData.Contains(u.OrgId));

            if (!string.IsNullOrWhiteSpace(request.name))
            {
                userOrgs = userOrgs.Where(u => u.Name.Contains(request.name));
            }
            if (!string.IsNullOrWhiteSpace(request.account))
            {
                userOrgs = userOrgs.Where(u => u.Account == request.account);
            }
            if (!string.IsNullOrWhiteSpace(request.orgId))
            {
                var cascade = UnitWork.Find<Repository.Domain.Org>(null).Where(o => o.Id == request.orgId).FirstOrDefault()?.CascadeId;
                var ids = UnitWork.Find<Repository.Domain.Org>(null).Where(o => o.CascadeId.Contains(cascade)).Select(x => x.Id);
                userOrgs = userOrgs.Where(u => ids.Contains(u.OrgId));
            }


            var userViews = userOrgs.ToList().GroupBy(b => b.Account).Select(u => new UserView
            {
                Id = u.First().Id,
                Account = u.Key,
                Name = u.First().Name,
                Sex = u.First().Sex,
                Status = u.First().Status,
                CreateTime = u.First().CreateTime,
                CreateUser = u.First().CreateId,
                ServiceRelations = u.First()?.ServiceRelations,
                CardNo = u.First()?.CardNo,
                OrganizationIds = string.Join(",", u.Select(x => x.OrgId)),
                Organizations = string.Join(",", u.Select(x => x.OrgName))
            });

            return new TableData
            {
                Count = userViews.Count(),
                Data = userViews.OrderBy(u => u.Name).Skip((request.page - 1) * request.limit)
                    .Take(request.limit)
            };
        }

        public void AddOrUpdate(UpdateUserReq request)
        {
            //if (string.IsNullOrEmpty(request.OrganizationIds))
            //    throw new Exception("请为用户分配机构");
            User requser = request;
            if (request.IsSync != null && (bool)request.IsSync)
            {
                requser.CreateId = "00000000-0000-0000-0000-000000000000";
            }
            else 
            {
                requser.CreateId = _auth.GetCurrentUser().User.Id;
            }
            if (string.IsNullOrEmpty(request.Id))
            {
                if (UnitWork.IsExist<User>(u => u.Account == request.Account))
                {
                    throw new Exception("用户账号已存在");
                }

                if (string.IsNullOrEmpty(requser.Password))
                {
                    requser.Password = requser.Account;   //如果客户端没提供密码，默认密码同账号
                }

                requser.CreateTime = DateTime.Now;
                requser.Password = Encryption.Encrypt(requser.Password);
                UnitWork.Add(requser);
                UnitWork.Save();
                request.Id = requser.Id; //要把保存后的ID存入view
                //新增的用户默认分配普通用户角色
                var SecondId=UnitWork.Find<Role>(r => r.Name == "普通用户").FirstOrDefault()?.Id;
                UnitWork.Add<Relevance>(new Relevance { Key = Define.USERROLE, FirstId = request.Id,OperateTime=DateTime.Now,SecondId= SecondId });
                UnitWork.Save();
            }
            else
            {
                UnitWork.Update<User>(u => u.Id == request.Id, u => new User
                {
                    Account = requser.Account,
                    BizCode = requser.BizCode,
                    Name = requser.Name,
                    Sex = requser.Sex,
                    Status = requser.Status,
                    ServiceRelations =request.ServiceRelations,
                    CardNo = request.CardNo,
                    EntryTime = request.EntryTime
                });
                if (!string.IsNullOrEmpty(requser.Password))  //密码为空的时候，不做修改
                {
                    UnitWork.Update<User>(u => u.Id == request.Id, u => new User
                    {
                        Password = Encryption.Encrypt(requser.Password)
                    });
                }
            }

            //保存erp3.0关联用户
            if (request.NsapUserId > 0)
            {
                var nusermap = UnitWork.Find<NsapUserMap>(c => c.UserID == requser.Id).FirstOrDefault();
                if (nusermap == null)
                {
                    UnitWork.Add(new NsapUserMap
                    {
                        UserID = requser.Id,
                        NsapUserId = request.NsapUserId
                    });
                }
                else
                {
                    UnitWork.Update<NsapUserMap>(c => c.UserID == requser.Id, c => new NsapUserMap { NsapUserId = request.NsapUserId });
                }
            }
            UnitWork.Save();

            string[] orgIds = request.OrganizationIds.Split(',').ToArray();

            _revelanceApp.DeleteBy(Define.USERORG, requser.Id);
            _revelanceApp.Assign(Define.USERORG, orgIds.ToLookup(u => requser.Id));

        }

        public async Task BlockUp(BlockUpUserReq req)
        {
            await UnitWork.UpdateAsync<User>(u => u.Id.Equals(req.UserId), o => new User
            {
                Status = 1
            });
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 加载用户的所有机构
        /// </summary>
        public IEnumerable<OpenAuth.Repository.Domain.Org> LoadByUser(string userId)
        {
            var result = from userorg in UnitWork.Find<Relevance>(null)
                         join org in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on userorg.SecondId equals org.Id
                         where userorg.FirstId == userId && userorg.Key == Define.USERORG
                         select org;
            return result;
        }


        public UserManagerApp(IUnitWork unitWork, IRepository<User> repository,
            RevelanceManagerApp app, IAuth auth, OrgManagerApp orgManagerApp) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
            _orgManagerApp = orgManagerApp;
        }
        /// <summary>
        /// 删除用户,包含用户与组织关系、用户与角色关系
        /// </summary>
        /// <param name="ids"></param>
        public override void Delete(string[] ids)
        {
            UnitWork.Delete<Relevance>(u => (u.Key == Define.USERROLE || u.Key == Define.USERORG)
                               && ids.Contains(u.FirstId));
            UnitWork.Delete<User>(u => ids.Contains(u.Id));
            UnitWork.Save();
        }
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="request"></param>
        public void ChangePassword(ChangePasswordReq request)
        {
            if (request.Password.ToLower() == "xinwei123") 
            {
                throw new Exception("密码与原始密码相同，请重新修改");
            }
            Repository.Update(u => u.Account == request.Account, user => new User
            {
                Password = Encryption.Encrypt(request.Password)
            });
        }
        /// <summary>
        /// 获取指定角色包含的用户列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public TableData LoadByRole(QueryUserListByRoleReq request)
        {
            var users = from userRole in UnitWork.Find<Relevance>(u =>
                    u.SecondId == request.roleId && u.Key == Define.USERROLE)
                        join user in UnitWork.Find<User>(r => r.Status == 0) on userRole.FirstId equals user.Id into temp
                        from c in temp.Where(u => u.Id != null)
                        select c;

            return new TableData
            {
                Count = users.Count(),
                Data = users.Skip((request.page - 1) * request.limit).Take(request.limit)
            };
        }
        /// <summary>
        /// 获取指定机构包含的用户列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public TableData LoadByOrg(QueryUserListByOrgReq request)
        {
            var users = from userRole in UnitWork.Find<Relevance>(u =>
                    u.SecondId == request.orgId && u.Key == Define.USERORG)
                        join user in UnitWork.Find<User>(null) on userRole.FirstId equals user.Id into temp
                        from c in temp.Where(u => u.Id != null)
                        select c;

            return new TableData
            {
                Count = users.Count(),
                Data = users.Skip((request.page - 1) * request.limit).Take(request.limit)
            };
        }

        public async Task<List<User>> LoadByRoleName(string roleName)
        {
            var role = await UnitWork.Find<Role>(r => r.Name.Equals(roleName)).FirstOrDefaultAsync();
            var users = from userRole in UnitWork.Find<Relevance>(u =>
                    u.SecondId == role.Id && u.Key == Define.USERROLE)
                        join user in UnitWork.Find<User>(null) on userRole.FirstId equals user.Id into temp
                        from c in temp.DefaultIfEmpty()
                        select c;

            return await users.ToListAsync();
        }
        public async Task<List<User>> LoadByRoleName(string[] roleName)
        {
            var role = await UnitWork.Find<Role>(r => roleName.Contains(r.Name)).Select(r=>r.Id).ToListAsync();
            var users = from userRole in UnitWork.Find<Relevance>(u =>
                             role.Contains(u.SecondId) && u.Key == Define.USERROLE)
                        join user in UnitWork.Find<User>(null) on userRole.FirstId equals user.Id into temp
                        from c in temp.DefaultIfEmpty()
                        select c;

            return await users.ToListAsync();
        }
        /// <summary>
        /// 根据用户角色查询用户，可用用户名做条件搜索
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> LoadByRoleName(QueryUserListByRoleNameReq request)
        {
            var roles = await UnitWork.Find<Role>(r => request.RoleNames.Contains(r.Name)).ToListAsync();
            var roleIds = roles.Select(r => r.Id).ToList();
            var users = from userRole in UnitWork.Find<Relevance>(u =>
                        roleIds.Contains(u.SecondId) && u.Key == Define.USERROLE)
                        join user in UnitWork.Find<User>(null) on userRole.FirstId equals user.Id into temp
                        from c in temp.DefaultIfEmpty()
                        select new { c.Id, c.Name };
            users = users.WhereIf(!string.IsNullOrWhiteSpace(request.UserName), u => u.Name.Contains(request.UserName));
            var count = await users.CountAsync();
            var data = await users.Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
            return new TableData
            {
                Count = count,
                Data = data
            };
        }

        /// <summary>
        /// 通过角色查人员部门信息
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public async Task<TableData> LoadInfoByRoleName(string roleName)
        {
            var query = await (from a in UnitWork.Find<Role>(null)
                               join b in UnitWork.Find<Relevance>(null) on a.Id equals b.SecondId
                               join c in UnitWork.Find<User>(null) on b.FirstId equals c.Id
                               join d in UnitWork.Find<Relevance>(null) on c.Id equals d.FirstId
                               join e in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on d.SecondId equals e.Id
                               where a.Name == roleName && b.Key == "UserRole" && d.Key == "UserOrg"
                               select new { Id = c.Id, Name = c.Name, OrgId = e.Id, OrgName = e.Name, e.CascadeId }).ToListAsync();
            query = query.GroupBy(c => c.Id).Select(c => c.OrderByDescending(o => o.CascadeId).First()).ToList();
            return new TableData
            {
                Data = query
            };
        }

        /// <summary>
        /// 绑定App用户Id
        /// </summary>
        /// <param name="appUserMap"></param>
        /// <returns></returns>
        public async Task BindAppUser(AddOrUpdateAppUserMapReq req)
        {
            // 校验ERP账号是否被重复绑定
            var  userMapsList = await UnitWork.Find<AppUserMap>(r => r.UserID == req.UserID).ToListAsync();
            if (userMapsList.Count >0)
            {
                // 是该用户绑定
                var exis_erp = userMapsList.Where(c=>c.AppUserId == req.AppUserId).FirstOrDefault();
                if(userMapsList.Count == 1 && exis_erp != null)
                {
                    // 允许绑定
                }
                else
                {
                    var firstUserMap = userMapsList.FirstOrDefault();
                    var appUserId = firstUserMap.AppUserId;
                    throw new Exception("当前ERP 账号已被其他APP账号绑定！AppUserId:" + appUserId);
                }
            }
            var o = await UnitWork.FindSingleAsync<AppUserMap>(a => a.AppUserId == req.AppUserId);
            if (o is null)
            {
                var obj = req.MapTo<AppUserMap>();
                await UnitWork.AddAsync(obj);
                await UnitWork.SaveAsync();
            }
            else
            {
                await UnitWork.UpdateAsync<AppUserMap>(a => a.Id.Equals(o.Id), s => new AppUserMap
                {
                    UserID = req.UserID,
                    PassPortId = req.PassPortId 

                });
                await UnitWork.SaveAsync();
            }
        }

        /// <summary>
        /// 修改用户资料
        /// </summary>
        /// <param name="request"></param>
        public void ChangeProfile(ChangeProfileReq request)
        {
            if (request.Account == Define.SYSTEM_USERNAME)
            {
                throw new Exception("不能修改超级管理员信息");
            }

            Repository.Update(u => u.Account == request.Account, user => new User
            {
                Name = request.Name,
                Sex = request.Sex
            });
        }

        /// <summary>
        /// 根据app用户信息获取token
        /// </summary>
        /// <param name="appUserId"></param>
        /// <returns></returns>
        public string GetTokenByAppUserId(int appUserId)
        {
            string token = string.Empty;
            var userMap = UnitWork.Find<AppUserMap>(a => a.AppUserId == appUserId).FirstOrDefault();
            if (userMap == null)
            {
                return token;
            }
            var userId = userMap.UserID;
            var userInfo = UnitWork.Find<User>(u => u.Id == userId).FirstOrDefault();
            if (userInfo == null)
            {
                return token;
            }
            var currentSession = new UserAuthSession
            {
                Account = userInfo.Account,
                Name = userInfo.Name,
                Token = Guid.NewGuid().ToString().GetHashCode().ToString("x"),
                AppKey = "openauth",
                CreateTime = DateTime.Now
            };

            RedisCacheContext cacheContext = new RedisCacheContext();
            //创建Session
            cacheContext.Set(currentSession.Token, currentSession, DateTime.Now.AddDays(10));
            return currentSession.Token;
        }
        /// <summary>
        /// 根据app用户信息获取token
        /// </summary>
        /// <param name="passportId"></param>
        /// <returns></returns>
        public string GetTokenByPassportId(int passportId)
        {
            string token = string.Empty;
            var userMap = UnitWork.Find<AppUserMap>(a => a.PassPortId == passportId).FirstOrDefault();
            if (userMap == null)
            {
                return token;
            }
            var userId = userMap.UserID;
            var userInfo = UnitWork.Find<User>(u => u.Id == userId).FirstOrDefault();
            if (userInfo == null)
            {
                return token;
            }
            var currentSession = new UserAuthSession
            {
                Account = userInfo.Account,
                Name = userInfo.Name,
                Token = Guid.NewGuid().ToString().GetHashCode().ToString("x"),
                AppKey = "openauth",
                CreateTime = DateTime.Now
            };

            RedisCacheContext cacheContext = new RedisCacheContext();
            //创建Session
            cacheContext.Set(currentSession.Token, currentSession, DateTime.Now.AddDays(10));
            return currentSession.Token;
        }


        /// <summary>
        /// 判断是否关联了帐号
        /// </summary>
        /// <param name="appUserId"></param>
        /// <returns></returns>
        public bool IsHaveNsapAccount(int appUserId)
        {
            var userMap = UnitWork.Find<AppUserMap>(a => a.AppUserId == appUserId).FirstOrDefault();
            if (userMap == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 按名称模糊查询人员
        /// </summary>
        public async Task<TableData> GetListUser(string name, string Orgid)
        {
            var loginUser = _auth.GetCurrentUser();

            List<string> userIds = new List<string>();
            if (string.IsNullOrWhiteSpace(Orgid))
            {
                var ids = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).Select(o => o.Id).ToArrayAsync();
                userIds = _revelanceApp.Get(Define.USERORG, false, ids);
            }
            else 
            {
                userIds = _revelanceApp.Get(Define.USERORG, false, Orgid);
            }
            var result = new TableData();
            var objs = UnitWork.Find<User>(u=>userIds.Contains(u.Id));
            objs = objs.WhereIf(!string.IsNullOrWhiteSpace(name),u => u.Name.Contains(name));
            result.Data = await objs.Select(u => new { u.Name, u.Id }).ToListAsync();
            return result;
        }

        /// <summary>
        /// 获取用户全部信息
        /// </summary>
        /// <returns></returns>
        public TableData GetUserAll()
        {
            var loginUser = _auth.GetCurrentUser();
            //获取级别最低部门
            var Relevances = _revelanceApp.Get(Define.USERORG, true, loginUser.User.Id);
            string OrgName = "";
            if (Relevances != null && Relevances.Count > 0) 
            {
                var Orgs = loginUser.Orgs.Where(o => Relevances.Contains(o.Id)).ToList();
                OrgName = Orgs.OrderByDescending(o => o.CascadeId).FirstOrDefault().Name;
            }
            //获取appuserid
            var appUserId=UnitWork.Find<AppUserMap>(r => r.UserID.Equals(loginUser.User.Id)).FirstOrDefault()?.AppUserId.ToString();

            //获取角色权限
            Relevances = _revelanceApp.Get(Define.USERROLE, true, loginUser.User.Id);
            var Roles = loginUser.Roles.Where(o => Relevances.Contains(o.Id)).Select(r=>r.Name).ToList();
            //获取能看到的报表
            var reportid = UnitWork.Find<Relevance>(c => Relevances.Contains(c.FirstId) && c.Key == Define.REPORTROLE).Select(c => c.SecondId).Distinct().ToList();
            var report = UnitWork.Find<ReportInfo>(c => reportid.Contains(c.Id)).ToList();

            var result = new TableData();
            result.Data = new
            {
                Account= loginUser.User.Account,
                UserId = loginUser.User.Id,
                UserName = loginUser.User.Name,
                ServiceRelations = string.IsNullOrWhiteSpace(loginUser.User.ServiceRelations) ? "未录入" : loginUser.User.ServiceRelations,
                OfficeSpace = string.IsNullOrWhiteSpace(loginUser.User.OfficeSpace) ? "未录入" : loginUser.User.ServiceRelations,
                OrgName = OrgName,
                Roles = Roles,
                IsPassword =Encryption.Decrypt(loginUser.User.Password).ToLower() == "xinwei123" ? true : false,
                AppUserId=Encryption.EncryptRSA(appUserId),
                report
            };
            return result;
        }
        /// <summary>
        /// 根据App用户Id获取用户信息
        /// </summary>
        /// <param name="AppUserId"></param>
        /// <returns></returns>
        public async Task<TableData> GetUserInfoByAppUserId(int AppUserId)
        {
            var result = new TableData();
            var userInfo = await UnitWork.Find<AppUserMap>(null).Include(a => a.User).Where(w => w.AppUserId == AppUserId).Select(s => new { s.User.Name, s.User.Id, s.AppUserId }).FirstOrDefaultAsync();
            result.Data = userInfo;
            return result;
        }
        /// <summary>
        /// 根据PassPortId获取用户信息
        /// </summary>
        /// <param name="AppUserId"></param>
        /// <returns></returns>
        public async Task<TableData> GetUserInfoByPassPortId(List<int?> passPortIds)
        {
            var result = new TableData();
            var userInfo = (from a in UnitWork.Find<AppUserMap>(w => passPortIds.Contains(w.PassPortId))
                            join u in UnitWork.Find<User>(null) on a.UserID equals u.Id
                            join b in UnitWork.Find<Relevance>(r => r.Key == Define.USERORG) on a.UserID equals b.FirstId
                            join c in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on b.SecondId equals c.Id
                            select new
                            {
                                erpUserId = a.UserID,
                                a.PassPortId,
                                erpUserName = u.Name,
                                orgId = c.Id,
                                orgName = c.Name
                            }).ToList();
            result.Data = userInfo;
            return result;
        }
        /// <summary>
        /// 根据UserId获取PassportID/AppUserID
        /// </summary>
        /// <param name="AppUserId"></param>
        /// <returns></returns>
        public async Task<TableData> GetAppUserIDByErpUserId(string Id)
        {
            var result = new TableData();
            var userInfo = await UnitWork.Find<AppUserMap>(w => w.UserID == Id).Select(s => new {s.UserID, s.AppUserId , s.PassPortId }).FirstOrDefaultAsync();
            result.Data = userInfo;
            return result;
        }
        /// <summary>
        /// 获取erp3.0人员
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetErp3User(QueryUserListReq request)
        {
            TableData result = new TableData();
            var list = UnitWork.Find<base_user>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(request.account), c => c.log_nm.Contains(request.account))
                .WhereIf(!string.IsNullOrWhiteSpace(request.name), c => c.user_nm.Contains(request.name));

            result.Count = await list.CountAsync();
            result.Data = await list.Select(c => new { c.user_id, c.log_nm, c.user_nm }).Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
            return result;
        }

        /// <summary>
        /// 获取单个erp3.0用户
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public async Task<TableData> GetErp3UserSingle(string id)
        {
            TableData result = new TableData();
            var map = await UnitWork.Find<NsapUserMap>(c => c.UserID == id).FirstOrDefaultAsync();
            if (map!=null)
            {
                var user = await UnitWork.Find<base_user>(c => c.user_id == map.NsapUserId).Select(c => new { c.user_id, c.log_nm, c.user_nm }).FirstOrDefaultAsync();
                result.Data = user;
            }
            return result;
        }

        /// <summary>
        /// 同步erp3.0用户
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public async Task SysnERPUser()
        {
            //var user = await UnitWork.Find<User>(null).Select(u => new { u.Account, u.Status, u.Email }).ToListAsync();
            var user = await (from a in UnitWork.Find<User>(null)
                              join b in UnitWork.Find<NsapUserMap>(null) on a.Id equals b.UserID into ab
                              from b in ab.DefaultIfEmpty()
                              select new { a.Id, a.Account, a.Status, a.Email, NsapUserId = b == null ? null : b.NsapUserId, a.EntryTime }).ToListAsync();
            var userAccounts = user.Select(c => c.Account).ToList();
            var query = from a in UnitWork.Find<base_user>(null)
                        join b in UnitWork.Find<base_user_detail>(null) on a.user_id equals b.user_id into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<base_dep>(null) on b.dep_id equals c.dep_id into bc
                        from c in bc.DefaultIfEmpty()
                            //where !userAccounts.Contains(a.log_nm) && b.out_date.ToString()== "0000-00-00"
                        select new { a.log_nm, a.user_nm, a.user_id, b.office_addr, c.dep_alias, out_date = b.out_date.ToString(), a.email, b.try_date };
            var erpUsers = await query.ToListAsync();
            var newUsers = erpUsers.Where(c => !userAccounts.Contains(c.log_nm) && c.out_date.ToString() == "0000-00-00").ToList();
            var orgs = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).ToListAsync();
            foreach (var item in newUsers)
            {
                var officeaddr = "新威尔";
                if (item.office_addr == "东莞塘厦") officeaddr = "东莞新威";
                if (item.office_addr == "深圳龙华") officeaddr = "新能源";
                var orgObj = orgs.Where(o => o.Name == item.dep_alias).FirstOrDefault();
                AddOrUpdate(new UpdateUserReq {Account= item.log_nm,Name=item.user_nm,Password="xinwei123",ServiceRelations= officeaddr, OrganizationIds= orgObj?.Id,NsapUserId=(int)item.user_id,IsSync=true });
            }
            var category = await UnitWork.Find<Category>(c => c.TypeId == "SYS_QuitShieldUser").Select(c => c.DtValue).ToListAsync();
            var quitUsers = erpUsers.Where(c => c.out_date.ToString() != "0000-00-00").Select(c => c.log_nm).ToList();//3.0已离职
            userAccounts = user.Where(c => c.Status == 0 && quitUsers.Contains(c.Account) && !category.Contains(c.Account)).Select(c => c.Account).ToList();//4.0未停用用户

            #region 4.0 user_id 为空用户赋值
            List<User> updateUserList = new List<User>();
            var emptyUser = await UnitWork.Find<User>(c => c.User_Id == null && c.Status== 0).ToListAsync();
            var emptyUserName = emptyUser.Select(a => a.Name).ToList();
            //  check base_user
            var relatedBaseUser = await UnitWork.Find<base_user>(c => emptyUserName.Contains(c.user_nm)).ToListAsync();
            foreach (var ruitem in relatedBaseUser)
            {
                var uitem = emptyUser.Where(a=>a.Name == ruitem.user_nm).FirstOrDefault();
                if (uitem!=null)
                {
                    uitem.User_Id = (int)ruitem.user_id;
                    updateUserList.Add(uitem);
                }

            }
            await UnitWork.BatchUpdateAsync<User>(updateUserList.ToArray());
            await UnitWork.SaveAsync();
            #endregion

            foreach (var item in userAccounts)
            {
                UnitWork.Update<User>(c => c.Account == item, c => new User
                {
                    Status = 1
                });
                UnitWork.Save();
            }

            var onUser = user.Where(c => c.Status == 0).ToList();
            foreach (var item in onUser)
            {
                var u = erpUsers.Where(c => c.log_nm == item.Account).FirstOrDefault();
                if (u!=null)
                {
                    if (item.Email!=u.email)
                    {
                        UnitWork.Update<User>(c => c.Account == item.Account, c => new User
                        {
                            Email = u.email
                        });
                        UnitWork.Save();
                    }
                    if (item.NsapUserId == null)
                    {
                        UnitWork.Add(new NsapUserMap { UserID = item.Id, NsapUserId = (int?)u.user_id });
                        UnitWork.Save();
                    }
                    if (item.EntryTime != u.try_date)
                    {
                        UnitWork.Update<User>(c => c.Account == item.Account, c => new User
                        {
                            EntryTime = u.try_date
                        }) ;
                        UnitWork.Save();
                    }
                }
            }

            var saleUser3 = UnitWork.Find<base_user_role>(a => a.role_id == 1).Select(a => (int?)a.user_id).ToList();
            var listOut = erpUsers.Where(c => c.out_date.ToString() != "0000-00-00").Select(c => (int?)c.user_id).ToList();//3.0已离职
            var saleUser4 = UnitWork.Find<NsapUserMap>(a => saleUser3.Contains(a.NsapUserId) && !listOut.Contains(a.NsapUserId)).Select(a => a.UserID).ToList();

            var saleRoleId = UnitWork.Find<Role>(a => a.Name == "销售员").FirstOrDefault();
            var saleList =  UnitWork.Find<Relevance>(a => a.Key == Define.USERROLE && a.SecondId == saleRoleId.Id).Select(a=> a.FirstId).ToList();

            foreach (var item in saleUser4)
            {
                if (!saleList.Contains(item))
                {
                    Relevance relevance = new Relevance();
                    relevance.Key = Define.USERROLE;
                    relevance.Status = 0;
                    relevance.OperateTime = DateTime.Now;
                    relevance.OperatorId = "";
                    relevance.FirstId = item;
                    relevance.SecondId = saleRoleId.Id;
                    relevance.ThirdId = "";
                    relevance.ExtendInfo = "";
                    relevance.Description = "同步ERP3.0";
                    UnitWork.Add(relevance);
                }
            }
            UnitWork.Save();

        }

        public async Task<TableData> GetAppUserInfo(string keyword)
        {
            TableData result = new TableData();
            var query = (from a in UnitWork.Find<AppUserMap>(null)
                        join u in UnitWork.Find<User>(null) on a.UserID equals u.Id
                        select new
                        {
                            erpId = u.Id,
                            erpAccount = u.Account,
                            erpName = u.Name,
                            appUserId = a.AppUserId
                        }).WhereIf(!string.IsNullOrWhiteSpace(keyword),u=> u.erpName.Contains(keyword) || u.erpAccount.Contains(keyword));

            result.Count = await query.CountAsync();
            result.Data = await query.ToListAsync();

            return result;
        }

        /// <summary>
        /// 获取用户部门信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<UserResp> GetUserOrgInfo(string userId, string name = "")
        {

            var petitioner = await (from a in UnitWork.Find<User>(null)
                                            .WhereIf(!string.IsNullOrWhiteSpace(userId), c => c.Id == userId)
                                            .WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name == name)
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
                                    }).OrderByDescending(u => u.CascadeId).FirstOrDefaultAsync();
            return petitioner;
        }


        /// <summary>
        /// 获取用户部门信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<UserResp>> GetUsersOrg(List<string> userId)
        {

            var userList = (from a in UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && userId.Contains(r.FirstId))
                            join c in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on a.SecondId equals c.Id
                            select new UserResp { Id = a.FirstId ,OrgId = a.SecondId, OrgName =  c.Name }).ToList();
            return userList;
        }

        /// <summary>
        /// 加载用户的同部门人员
        /// </summary>
        public async Task<TableData> GetOrgUser(string userId)
        {
            TableData result = new TableData();
            var orgId = UnitWork.Find<Relevance>(a => a.FirstId == userId && a.Key == Define.USERORG).FirstOrDefault().SecondId;

            var data = from a in UnitWork.Find<Relevance>(null)
                      join b in UnitWork.Find<User>(null) on a.FirstId equals b.Id
                      join c in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on a.SecondId equals c.Id
                      where a.SecondId == orgId && a.Key == Define.USERORG
                      select new
                      {
                          userId = b.Id,
                          userName = b.Name,
                          orgId = c.Id,
                          orgName = c.Name

                      };

            result.Data = data;
            return result;
        }
    }
}
