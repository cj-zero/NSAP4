using System;
using System.Collections.Generic;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System.Linq;
using OpenAuth.App.Request;

namespace OpenAuth.App
{
    public class RoleApp : BaseApp<Role>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载当前登录用户可访问的全部角色
        /// </summary>
        public List<Role> Load(QueryRoleListReq request)
        {
            var loginUser = _auth.GetCurrentUser();
            var roles = loginUser.Roles;

            if (roles.Exists(r => r.Name.Equals("系统管理员") || r.Name.Equals("ERP4.0人事管理")))
                roles = UnitWork.Find<Role>(null).ToList();

            if (!string.IsNullOrEmpty(request.key))
            {
                roles = roles.Where(u => u.Name.Contains(request.key)).ToList();
            }

            return roles.OrderBy(r => r.Name).ToList();
        }


        public void Add(RoleView obj)
        {
          
            Role role = obj;
            role.CreateTime = DateTime.Now;
            List<Role> roles = new List<Role>();
            if (string.IsNullOrEmpty(obj.RoleKey))
            {
               roles = UnitWork.Find<Role>(r => r.RoleKey == obj.RoleKey).ToList();
            }

            if (roles != null && roles.Count() > 0)
            {
                throw new Exception("角色标识不允许重复");
            }
            else
            {
                Repository.Add(role);
                obj.Id = role.Id;   //要把保存后的ID存入view

                //如果当前账号不是SYSTEM，则直接分配
                var loginUser = _auth.GetCurrentUser();
                if (loginUser.User.Account != Define.SYSTEM_USERNAME)
                {
                    _revelanceApp.Assign(new AssignReq
                    {
                        type = Define.USERROLE,
                        firstId = loginUser.User.Id,
                        secIds = new[] { role.Id }
                    });
                }
            }
        }
        
        public void Update(RoleView obj)
        {
            Role role = obj;
            List<Role> roles = new List<Role>();
            if (string.IsNullOrEmpty(obj.RoleKey))
            {
                roles = UnitWork.Find<Role>(r => r.RoleKey == obj.RoleKey).ToList();
            }

            if (roles != null && roles.Count() > 1)
            {
                throw new Exception("角色标识不允许重复");
            }
            else
            {
                UnitWork.Update<Role>(u => u.Id == obj.Id, u => new Role
                {
                    Name = role.Name,
                    Status = role.Status,
                    Identity = role.Identity,
                    RoleKey = role.RoleKey
                });
            }
        }


        public RoleApp(IUnitWork unitWork, IRepository<Role> repository,
            RevelanceManagerApp app,IAuth auth) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
        }
    }
}