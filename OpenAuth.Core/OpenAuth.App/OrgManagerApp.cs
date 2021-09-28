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
        public string Add(OpenAuth.App.Request.AddOrUpdateOrgReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var org = obj.MapTo<OpenAuth.Repository.Domain.Org>();
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

            _revelanceApp.DeleteBySecondId(Define.ORGROLE, obj.Id);
            if (obj.DeptManager != null && obj.DeptManager.Count > 0)
            {
                //添加部门负责人
                _revelanceApp.AssignBy(Define.ORGROLE, obj.DeptManager.ToLookup(c => obj.Id));
            }
            return org.Id;
        }

        public string Update(OpenAuth.App.Request.AddOrUpdateOrgReq obj)
        {
            var org = obj.MapTo<OpenAuth.Repository.Domain.Org>();
            UpdateTreeObj(org);


            _revelanceApp.DeleteBySecondId(Define.ORGROLE, obj.Id);
            if (obj.DeptManager != null && obj.DeptManager.Count > 0)
            {
                //添加部门负责人
                _revelanceApp.AssignBy(Define.ORGROLE, obj.DeptManager.ToLookup(c => obj.Id));
            }
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
        /// 根据公司获取部门
        /// </summary>
        /// <param name="corpId"></param>
        /// <returns></returns>
        public List<OpenAuth.Repository.Domain.Org> GetOrgs(string corpId)
        {
            return UnitWork.Find<OpenAuth.Repository.Domain.Org>(c => c.CorpId == corpId).ToList();
        }
        /// <summary>
        /// 获取部门树和用户
        /// </summary>
        /// <param name="corpId"></param>
        /// <returns></returns>
        public async Task<TableData> GetOrgTreeAndUser()
        {
            TableData result = new TableData();
            var org = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).ToListAsync();
            //var user = await UnitWork.Find<OpenAuth.Repository.Domain.User>(null).ToListAsync();

            var query = from a in UnitWork.Find<OpenAuth.Repository.Domain.User>(null)
                        join b in UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.Key == Define.USERORG)
                        on a.Id equals b.FirstId into ab
                        from b in ab.DefaultIfEmpty()
                        select new UserOrg { Name=a.Name, SecondId=b.SecondId };

            List<object> trees = new List<object>();
            GetTree(trees, "", org, query);

            void GetTree(List<object> trees, string pid,List<OpenAuth.Repository.Domain.Org> org,IQueryable<UserOrg> user)
            {
                var child = org.Where(c => c.ParentId == pid).ToList();
                if (string.IsNullOrWhiteSpace(pid))
                    child = org.Where(c => c.ParentName == "根节点").ToList();

                if (child.Count > 0)
                {
                    foreach (var item in child)
                    {
                        OrgTree tree = new OrgTree();
                        tree.Name = item.Name;
                        List<object> childtree = new List<object>();
                        tree.Node = childtree;
                        trees.Add(tree);
                        GetTree(tree.Node, item.Id, org, user);
                    }
                }
                else
                {
                    user.Where(c => !string.IsNullOrWhiteSpace(c.SecondId) && c.SecondId == pid).ToList().ForEach(c => { trees.Add(c); });
                }

            }
            //var data = org.Where(c => c.ParentName == "根节点").Select(c => new
            //{
            //    Name = c.Name,
            //    Child = org.Where(o => o.ParentId == c.Id).Count() > 0 ? org.Where(o => o.ParentId == c.Id).Select(o => new
            //    {
            //        Name = o.Name,
            //        Type="org",
            //        Child = 1
            //    }) : query.Where(o =>!string.IsNullOrWhiteSpace(o.SecondId) && o.SecondId == c.Id).Select(c => new { Name = c.Name, Type = "user", Child = 1 })
            //});
            result.Data = trees;
            return result;
        }

        /// <summary>
        /// 获取部门信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<OpenAuth.Repository.Domain.Org> GetOrgInfo(string id,string name)
        {
            return await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).WhereIf(!string.IsNullOrWhiteSpace(id), c => c.Id == id).WhereIf(!string.IsNullOrWhiteSpace(name), c => c.Name == name).FirstOrDefaultAsync();
        }

        //private void GetTree(List<Tree> trees,string pid)
        //{
        //    var child = UnitWork.Find<OpenAuth.Repository.Domain.Org>(c=>c.ParentId==pid).ToList();
        //    if (child.Count > 0)
        //    {
        //        foreach (var item in child)
        //        {
        //            Tree tree = new Tree();
        //            tree.Name = item.Name;
        //            List<Tree> childtree = new List<Tree>();
        //            tree.Node = childtree;
        //            trees.Add(tree);
        //            GetTree(tree.Node, item.Id);
        //        }
        //        //trees.Add(tree);
        //    }
        //    else
        //    {

        //    }
        //    //var child = org.Where(c => c.ParentId == pid).ToList();
            
        //}

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
            var objs = UnitWork.Find<OpenAuth.Repository.Domain.Org>(w => w.ParentName == "R研发部" || w.ParentName == "S售后部" || w.ParentName == "P生产部" || w.ParentName == "M2" || w.ParentName == "S销售部");
            var data = await objs.Select(u => new ReimburseOrg { Label = u.Name, Value = u.Id, ParentName = u.ParentName }).ToListAsync();
            var reimburseOrgResps = data.GroupBy(g => g.ParentName).Select(s => new ReimburseOrgResp { Label = GetOrgName(s.Key), Value = GetOrgName(s.Key), Children = s.ToList() }).ToList();
            reimburseOrgResps.Add(new ReimburseOrgResp
            {
                Label="公司",
                Value= "公司",
            });
            reimburseOrgResps.Add(new ReimburseOrgResp
            {
                Label = "T1",
                Value = "T1",
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
                case "S销售部":
                    orgname = "销售部门";
                    break;
                case "M2":
                    orgname = "M2部门";
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