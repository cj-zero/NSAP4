using Infrastructure;
using OpenAuth.App.Clue.Request;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Domain.ProductModel;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.App
{
    /// <summary>
    /// 线索服务
    /// </summary>
    public class ClueApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;
        ServiceBaseApp _serviceBaseApp;
        public ClueApp(ServiceBaseApp serviceBaseApp, IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
            _serviceBaseApp = serviceBaseApp;

        }
        /// <summary>
        /// 线索列表
        /// </summary>
        /// <param name="clueListReq"></param>
        /// <param name="rowcount"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<dynamic> GetClueListAsync(ClueListReq clueListReq, out int rowcount)
        {
            throw new NotImplementedException();
        }

        public async Task<string>  AddClueAsync(AddClueReq addClueReq)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            OpenAuth.Repository.Domain.Serve.Clue clue = new Repository.Domain.Serve.Clue
            {
                CardName = addClueReq.CardName,
                CustomerSource = addClueReq.CustomerSource,
                IndustryInvolved = addClueReq.IndustryInvolved,
                StaffSize = addClueReq.StaffSize,
                WebSite = addClueReq.WebSite,
                Remark = addClueReq.Remark,
                IsCertification = addClueReq.IsCertification,
                Status = 0,
                CreateTime = DateTime.Now,
                CreateUser = loginUser.Name
            };
            var data = UnitWork.Add<OpenAuth.Repository.Domain.Serve.Clue, int>(clue);
            UnitWork.Save();
            OpenAuth.Repository.Domain.Serve.ClueContacts cluecontacts = new Repository.Domain.Serve.ClueContacts
            {
                ClueId = data.Id,
                Name = addClueReq.Name,
                Tel1 = addClueReq.Tel1,
                Role = addClueReq.Role,
                Position = addClueReq.Position,
                Address1 = addClueReq.Address1,
                Address2 = addClueReq.Address2,
                CreateTime = DateTime.Now,
                CreateUser = loginUser.Name
            };
            UnitWork.Add<OpenAuth.Repository.Domain.Serve.ClueContacts, int>(cluecontacts);
            UnitWork.Save();
            return data.Id.ToString();
        }
    }
}
