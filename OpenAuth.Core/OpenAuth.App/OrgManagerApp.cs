using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using NetOffice.WordApi;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;

namespace OpenAuth.App
{
    public class OrgManagerApp : BaseTreeApp<OpenAuth.Repository.Domain.Org>
    {
        private RevelanceManagerApp _revelanceApp;
        /// <summary>
        /// 添加部门
        /// </summary>
        /// <param name="org">The org.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="System.Exception">未能找到该组织的父节点信息</exception>
        public string Add(OpenAuth.Repository.Domain.Org org)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            CaculateCascade(org);

            Repository.Add(org);

            //如果当前账号不是SYSTEM，则直接分配
            var loginUser = _auth.GetCurrentUser();
            if (loginUser.User.Account != Define.SYSTEM_USERNAME)
            {
                _revelanceApp.Assign(new AssignReq
                {
                    type = Define.USERORG,
                    firstId = loginContext.User.Id,
                    secIds = new[] { org.Id }
                });
            }

            return org.Id;
        }

        public string Update(OpenAuth.Repository.Domain.Org org)
        {

            UpdateTreeObj(org);

            return org.Id;
        }

        /// <summary>
        /// 删除指定ID的部门及其所有子部门
        /// </summary>
        public void DelOrgCascade(string[] ids)
        {
            var delOrgCascadeIds = UnitWork.Find<Repository.Domain.Org>(u => ids.Contains(u.Id)).Select(u => u.CascadeId).ToArray();
            var delOrgIds = new List<string>();
            foreach (var cascadeId in delOrgCascadeIds)
            {
                delOrgIds.AddRange(UnitWork.Find<Repository.Domain.Org>(u => u.CascadeId.Contains(cascadeId)).Select(u => u.Id).ToArray());
            }
            UnitWork.Delete<Relevance>(u => u.Key == Define.USERORG && delOrgIds.Contains(u.SecondId));
            UnitWork.Delete<Repository.Domain.Org>(u => delOrgIds.Contains(u.Id));
            UnitWork.Save();
        }


        /// <summary>
        /// 加载特定用户的部门
        /// </summary>
        /// <param name="userId">The user unique identifier.</param>
        public List<OpenAuth.Repository.Domain.Org> LoadForUser(string userId)
        {
            //用户角色与自己分配到的角色ID
            var ids =
                UnitWork.Find<Relevance>(
                    u => u.FirstId == userId && u.Key == Define.USERORG).Select(u => u.SecondId).ToList();

            if (!ids.Any()) return new List<OpenAuth.Repository.Domain.Org>();
            return UnitWork.Find<OpenAuth.Repository.Domain.Org>(u => ids.Contains(u.Id)).ToList();
        }

        /// <summary>
        /// 按名称模糊查询部门 by zlg 2020.7.31
        /// </summary>
        /// <param name="name">部门名称</param>
        /// <returns></returns>
        public async Task<TableData> GetListOrg(string name)
        {
            var result = new TableData();
            var objs = UnitWork.Find<OpenAuth.Repository.Domain.Org>(null);
            objs = objs.WhereIf(!string.IsNullOrWhiteSpace(name), u => u.Name.Contains(name.ToUpper()));
            result.Data = await objs.Select(u => new { u.Name, u.Id }).ToListAsync();
            return result;
        }

        public async Task<TableData> GetReimburseOrgs()
        {
            var result = new TableData();
            var objs = UnitWork.Find<OpenAuth.Repository.Domain.Org>(w => w.ParentName == "R研发部" || w.ParentName == "S售后部" || w.ParentName == "P生产部");
            var data = await objs.Select(u => new ReimburseOrg { Label = u.Name, Value = u.Id, ParentName = u.ParentName }).ToListAsync();
            var reimburseOrgResps = data.GroupBy(g => g.ParentName).Select(s => new ReimburseOrgResp { Label = GetOrgName(s.Key), Value = GetOrgName(s.Key), Children = s.ToList() }).ToList();
            reimburseOrgResps.Add(new ReimburseOrgResp
            {
                Label="公司",
                Value= "7ff76fd2-56e4-4db3-be9c-2da2114e3f4e",
            });
            result.Data = reimburseOrgResps;
            return result;
        }

        private string GetOrgName(string name)
        {

            string orgname = string.Empty;
            switch (name)
            {
                case "R研发部":
                    orgname = "研发部门";
                    break;
                case "S售后部":
                    orgname = "售后部门";
                    break;
                case "P生产部":
                    orgname = "生产部门";
                    break;
            }
            return orgname;
        }

        public OrgManagerApp(IUnitWork unitWork, IRepository<OpenAuth.Repository.Domain.Org> repository, IAuth auth,
            RevelanceManagerApp revelanceApp) : base(unitWork, repository, auth)
        {
            _revelanceApp = revelanceApp;
        }
    }
}