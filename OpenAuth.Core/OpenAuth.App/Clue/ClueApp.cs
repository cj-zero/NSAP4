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

        public Task<string> AddClueAsync(AddClueReq addClueReq)
        {
            OpenAuth.Repository.Domain.Serve.Clue clue = new Repository.Domain.Serve.Clue
            {
                CardName = addClueReq.CardName,
                CustomerSource = addClueReq.CustomerSource,
                IndustryInvolved = addClueReq.IndustryInvolved,
                StaffSize = addClueReq.StaffSize,
                WebSite = addClueReq.WebSite,
                Remark = addClueReq.Remark,
                IsCertification = addClueReq.IsCertification,
                Status = 0
            };
            var data = UnitWork.Add<OpenAuth.Repository.Domain.Serve.Clue, int>(clue);
            UnitWork.Save();
            return null;
        }
    }
}
