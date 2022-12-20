using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Infrastructure;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App.CommonHelp
{
    public class UserDepartMsgHelp : OnlyUnitWorkBaeApp
    {
        public UserDepartMsgHelp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {

        }

        /// <summary>
        /// 获取4.0用户对应的部门名称
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns>返回用户部门名称，如果存在多个部门则部门名称拼接成字符串返回</returns>
        public string GetUserOrgName(string userId)
        {
            string OrgName = "";
            lock (OrgName)
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    //查询当前用户Id对应的部门名称
                    var petitioner = (from a in UnitWork.Find<User>(r => r.Id == userId)
                                      join b in UnitWork.Find<Relevance>(r => r.Key == Define.USERORG) on a.Id equals b.FirstId into ab
                                      from b in ab.DefaultIfEmpty()
                                      join c in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on b.SecondId equals c.Id into bc
                                      from c in bc.DefaultIfEmpty()
                                      select new
                                      {
                                          Name = c.Name
                                      }).ToList();

                    if (petitioner != null && petitioner.Count() > 0)
                    {
                        List<string> petitioners = (petitioner.GroupBy(r => new { r.Name }).Select(r => r.Key.Name)).ToList();
                        OrgName = string.Join(",", petitioners);
                    }
                }

                return OrgName;
            }
        }

        /// <summary>
        /// 获取3.0用户对应的部门
        /// </summary>
        /// <param name="slpCode">业务员编码</param>
        /// <returns>返回部门信息</returns>
        public string GetUserDepart(int slpCode)
        {
            string departName = "";
            lock (departName)
            {
                var userids = UnitWork.Find<sbo_user>(r => r.sale_id == slpCode && r.sbo_id == Define.SBO_ID).Select(r => r.user_id).FirstOrDefault();
                if (userids != 0)
                {
                    List<int> deptusers = UnitWork.Find<base_user_detail>(r => r.user_id == userids).Select(r => r.dep_id).ToList();
                    if (deptusers != null && deptusers.Count() > 0)
                    {
                        List<string> depts = UnitWork.Find<base_dep>(r => deptusers.Contains(r.dep_id)).Select(r => r.dep_alias).ToList();
                        departName = string.Join(",", depts);
                    }
                }

                return departName;
            }
        }

        /// <summary>
        /// 获取3.0用户对应的部门
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns>返回部门信息</returns>
        public string GetUserIdDepart(int userId)
        {
            string departName = "";
            lock (departName)
            {
                if (userId != 0)
                {
                    List<int> deptusers = UnitWork.Find<base_user_detail>(r => r.user_id == userId).Select(r => r.dep_id).ToList();
                    if (deptusers != null && deptusers.Count() > 0)
                    {
                        List<string> depts = UnitWork.Find<base_dep>(r => deptusers.Contains(r.dep_id)).Select(r => r.dep_alias).ToList();
                        departName = string.Join(",", depts);
                    }
                }

                return departName;
            }
        }

        /// <summary>
        /// 获取3.0用户对应的部门
        /// </summary>
        /// <param name="userName">用户名称</param>
        /// <returns>返回部门信息</returns>
        public string GetUserNameDept(string userName)
        {
            string departName = "";
            if (!string.IsNullOrEmpty(userName))
            {
                int userId = UnitWork.Find<base_user>(r => r.user_nm == userName).Select(r => Convert.ToInt32(r.user_id)).FirstOrDefault();
                if (userId > 0)
                {
                    List<int> deptusers = UnitWork.Find<base_user_detail>(r => r.user_id == userId).Select(r => r.dep_id).ToList();
                    if (deptusers != null && deptusers.Count() > 0)
                    {
                        List<string> depts = UnitWork.Find<base_dep>(r => deptusers.Contains(r.dep_id)).Select(r => r.dep_alias).ToList();
                        departName = string.Join(",", depts);
                    }
                }
            }

            return departName;
        }
    }
}
