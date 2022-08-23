using Infrastructure;
using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;

namespace OpenAuth.App.Request
{
    /// <summary>
    /// 添加或修改用户信息的请求
    /// </summary>
    [AutoMapTo(typeof(User))]
    public  class UpdateUserReq
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        /// <returns></returns>
        public string Id { get; set; }


        /// <summary>
        /// </summary>
        /// <returns></returns>
        public string Account { get; set; }
        
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public string Password { get; set; }


        /// <summary>
        /// 组织名称
        /// </summary>
        /// <returns></returns>
        public string Name { get; set; }


        /// <summary>
        /// </summary>
        /// <returns></returns>
        public int Sex { get; set; }


        /// <summary>
        /// 当前状态
        /// </summary>
        /// <returns></returns>
        public int Status { get; set; }

        /// <summary>
        /// 劳务关系
        /// </summary>
        /// <returns></returns>
        public string ServiceRelations { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>
        /// <returns></returns>
        public string CardNo { get; set; }

        /// <summary>
        /// 所属组织Id，多个可用，分隔
        /// </summary>
        /// <value>The organizations.</value>
        public string OrganizationIds { get; set; }

        /// <summary>
        /// 是否同步
        /// </summary>
        /// <value>The organizations.</value>
        public bool? IsSync { get; set; }
        /// <summary>
        /// 3.0用户ID
        /// </summary>
        public int NsapUserId { get; set; }

        public static implicit operator UpdateUserReq(User user)
        {
            return user.MapTo<UpdateUserReq>();
        }

        public static implicit operator User(UpdateUserReq view)
        {
            return view.MapTo<User>();
        }

        public UpdateUserReq()
        {
            OrganizationIds = string.Empty;
        }


        public DateTime? EntryTime { get; set; }

        
    }
}
