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
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class UserManagerApp : BaseApp<User>
    {
        private RevelanceManagerApp _revelanceApp;

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
                           join relevance in UnitWork.Find<Relevance>(u => u.Key == "UserOrg")
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
                               r.Key,
                               r.SecondId,
                               OrgId = o.Id,
                               OrgName = o.Name
                           };

            //如果请求的orgId不为空
            if (!string.IsNullOrEmpty(request.orgId))
            {
                var org = loginUser.Orgs.SingleOrDefault(u => u.Id == request.orgId);
                var cascadeId = org.CascadeId;

                var orgIds = loginUser.Orgs.Where(u => u.CascadeId.Contains(cascadeId)).Select(u => u.Id).ToArray();

                //只获取机构里面的用户
                userOrgs = userOrgs.Where(u => u.Key == Define.USERORG && orgIds.Contains(u.OrgId));
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
                OrganizationIds = string.Join(",", u.Select(x => x.OrgId))
                ,
                Organizations = string.Join(",", u.Select(x => x.OrgName))

            });

            return new TableData
            {
                Count = userViews.Count(),
                Data = userViews.OrderBy(u => u.Name)
                    .Skip((request.page - 1) * request.limit)
                    .Take(request.limit),
            };
        }

        public void AddOrUpdate(UpdateUserReq request)
        {
            if (string.IsNullOrEmpty(request.OrganizationIds))
                throw new Exception("请为用户分配机构");
            User requser = request;
            requser.CreateId = _auth.GetCurrentUser().User.Id;
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
                request.Id = requser.Id; //要把保存后的ID存入view
            }
            else
            {
                UnitWork.Update<User>(u => u.Id == request.Id, u => new User
                {
                    Account = requser.Account,
                    BizCode = requser.BizCode,
                    Name = requser.Name,
                    Sex = requser.Sex,
                    Status = requser.Status
                });
                if (!string.IsNullOrEmpty(requser.Password))  //密码为空的时候，不做修改
                {
                    UnitWork.Update<User>(u => u.Id == request.Id, u => new User
                    {
                        Password = Encryption.Encrypt(requser.Password)
                    });
                }
            }

            UnitWork.Save();
            string[] orgIds = request.OrganizationIds.Split(',').ToArray();

            _revelanceApp.DeleteBy(Define.USERORG, requser.Id);
            _revelanceApp.Assign(Define.USERORG, orgIds.ToLookup(u => requser.Id));
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
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
        }

        public void ChangePassword(ChangePasswordReq request)
        {
            Repository.Update(u => u.Account == request.Account, user => new User
            {
                Password = Encryption.Encrypt(request.Password)
            });
        }

        public TableData LoadByRole(QueryUserListByRoleReq request)
        {
            var users = from userRole in UnitWork.Find<Relevance>(u =>
                    u.SecondId == request.roleId && u.Key == Define.USERROLE)
                        join user in UnitWork.Find<User>(null) on userRole.FirstId equals user.Id into temp
                        from c in temp.Where(u => u.Id != null)
                        select c;

            return new TableData
            {
                Count = users.Count(),
                Data = users.Skip((request.page - 1) * request.limit).Take(request.limit)
            };
        }

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
        /// 绑定App用户Id
        /// </summary>
        /// <param name="appUserMap"></param>
        /// <returns></returns>
        public async Task BindAppUser(AddOrUpdateAppUserMapReq req)
        {
            var obj = req.MapTo<AppUserMap>();
            await UnitWork.AddAsync(obj);
            await UnitWork.SaveAsync();
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
    }
}