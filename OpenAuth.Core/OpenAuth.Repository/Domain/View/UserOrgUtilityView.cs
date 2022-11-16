using OpenAuth.Repository.Domain.Material;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.View
{

    public class UserOrgUtilityView
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Account { get; set; }

        public string deptName { get; set; }


    }

    public class UserManageUtilityRsp
    {
        public int Count { get; set; }

        public List<UserManageUtilityView> muv { get; set; } = new List<UserManageUtilityView>();
    }


    public class UserManageUtilityView
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Account { get; set; }

        public string deptName { get; set; }

        public int Status { get; set; }
    }

    public class UserManageUtilityRequest
    {

        public string SlpName { get; set; }

        public string deptName { get; set; }

        public int page { get; set; }

        public int Limit { get; set; }
    }

    public class BindUtilityRequest
    {

        public int page { get; set; }

        public int Limit { get; set; }

        public string query { get; set; }
    }

    public class BindUtilityUpdateRequest
    {

        public string MAccount { get; set; }

        public string MName { get; set; }

        public string LAccount { get; set; }

        public string LName { get; set; }

        public string  Level { get; set; }

        public int  DutyFlag { get; set; }
        public int IsDelete { get; set; }
    }


    public class BindUtilityRep
    {

        public int Count { get; set; }

        public List<ManageAccountBind> mab { get; set; } = new List<ManageAccountBind>();
    }


    public class MaterialUsers
    {
        public int UserID { get; set; }

        public string UserName { get; set; }

        public string FirstNameAndLastName { get; set; }
    }


}
