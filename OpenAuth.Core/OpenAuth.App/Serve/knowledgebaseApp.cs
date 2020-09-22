using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class KnowledgeBaseApp : BaseTreeApp<KnowledgeBase>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> Load(QueryKnowledgeBaseListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }


            var result = new TableData();
            var objs = UnitWork.Find<KnowledgeBase>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id.Contains(request.key));
            }

            result.Data = await objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            result.Count = objs.Count();
            return result;
        }

        public void Add(AddOrUpdateKnowledgeBaseReq req)
        {
            var obj = req.MapTo<KnowledgeBase>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            Repository.Add(obj);
        }

         public void Update(AddOrUpdateKnowledgeBaseReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<KnowledgeBase>(u => u.Id == obj.Id, u => new KnowledgeBase
            {
                SequenceNumber = obj.SequenceNumber,
                Type = obj.Type,
                Org = obj.Org,
                Name = obj.Name,
                Content = obj.Content,
                ParentId = obj.ParentId,
                ParentName = obj.ParentName,
                CascadeId = obj.CascadeId,
                UpdateTime = DateTime.Now,
                UpdateUserId = user.Id,
                UpdateUserName = user.Name
                //todo:补充或调整自己需要的字段
            });

        }
            

        public KnowledgeBaseApp(IUnitWork unitWork, IRepository<KnowledgeBase> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}