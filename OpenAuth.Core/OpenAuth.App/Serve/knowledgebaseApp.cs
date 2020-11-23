using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
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
            var objs = UnitWork.Find<KnowledgeBase>(null)
                .WhereIf(request.Type.HasValue, k => k.Type == request.Type.Value)
                .WhereIf(!string.IsNullOrWhiteSpace(request.ParentId), k => k.ParentId.Equal(request.ParentId))
                .WhereIf(!string.IsNullOrWhiteSpace(request.key), k => k.Name.Contains(request.key) || k.Content.Contains(request.key) || k.Code.Contains(request.key));

            result.Data = await objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            result.Count = objs.Count();
            return result;
        }
        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<Response<IEnumerable<TreeItem<KnowledgeBase>>>> LoadTree(QueryKnowledgeBaseListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new Response<IEnumerable<TreeItem<KnowledgeBase>>>();
            var objs = UnitWork.Find<KnowledgeBase>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id.Contains(request.key) || u.Name.Contains(request.key));
            }

            var Data = await objs.OrderBy(u => u.Id).ToListAsync();
            result.Result = Data.GenerateTree(d=>d.Id, d=>d.ParentId);
            return result;
        }

        public async Task Add(KnowledgeBase obj)
        {
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            var org = _auth.GetCurrentUser().Orgs.OrderByDescending(s => s.CascadeId).FirstOrDefault().Name;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            obj.Org = org;
            var maxSN = await UnitWork.Find<KnowledgeBase>(k => k.Type == obj.Type).MaxAsync(k=>k.SequenceNumber);
            if (maxSN == null)
            {
                obj.SequenceNumber = 1;
            }
            else
            {
                obj.SequenceNumber = ++maxSN;
            }
           
            await CaculateCascadeAsync(obj);
            await Repository.AddAsync(obj);
            await Repository.SaveAsync();
        }

         public void Update(KnowledgeBase obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<KnowledgeBase>(u => u.Id == obj.Id, u => new KnowledgeBase
            {
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
            UpdateTreeObj(obj);
        }
            

        public KnowledgeBaseApp(IUnitWork unitWork, IRepository<KnowledgeBase> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}