using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Excel;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using Npoi.Mapper;
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
                .WhereIf(!string.IsNullOrWhiteSpace(request.key), k => k.Name.Contains(request.key) || k.Content.Contains(request.key) || k.Code.Contains(request.key))
                .Where(k => k.IsNew == false);

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
            var objs = await UnitWork.Find<KnowledgeBase>(null).ToListAsync();
            if (!string.IsNullOrEmpty(request.key))
            {
                var ids = objs.Where(u => u.Id.Contains(request.key) || u.Name.Contains(request.key) || u.ParentName.Contains(request.key)).Select(s => s.CascadeId).ToList();
                List<KnowledgeBase> KnowledgeBases = new List<KnowledgeBase>();
                foreach (var item in ids)
                {
                    KnowledgeBases.AddRange(objs.Where(u => item.Contains(u.CascadeId)).ToList());
                }
                objs = KnowledgeBases.GroupBy(u => u).Select(u => u.First()).OrderBy(u => u.Id).ToList();
            }

            result.Result = objs.GenerateTree(d => d.Id, d => d.ParentId).ToList();
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
            obj.IsNew = false;
            var maxSN = await UnitWork.Find<KnowledgeBase>(k => k.Type == obj.Type).MaxAsync(k => k.SequenceNumber);
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

        /// <summary>
        /// 加载新知识库列表
        /// </summary>
        public async Task<TableData> NewLoad(QueryKnowledgeBaseListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var objs = UnitWork.Find<KnowledgeBase>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(request.Rank), k => k.Rank == int.Parse(request.Rank))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Code), k => k.Name.Contains(request.Code) || k.Code.Contains(request.Code))
                .Where(k => k.IsNew == true);
            var knowledgeBases = await objs.OrderBy(u => u.Code)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            var ids = knowledgeBases.Select(k => k.ParentId);
            var codes = await UnitWork.Find<KnowledgeBase>(k => ids.Contains(k.Id)).Select(k => new { k.Id, k.Code }).ToListAsync();
            result.Data = knowledgeBases.Select(k => new
            {
                k.Id,
                k.Code,
                k.Name,
                k.CreateUserName,
                k.ParentId,
                Rank = k.Rank.ToString(),
                k.ParentName,
                ParentCode = codes.Where(c => c.Id.Equals(k.ParentId)).FirstOrDefault()?.Code,
                k.CreateTime,
                k.UpdateTime
            });
            result.Count = objs.Count();
            return result;
        }
        /// <summary>
        /// 添加新知识库
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task NewAdd(KnowledgeBase obj)
        {
            //todo:补充或调整自己需要的字段
            var num = await UnitWork.Find<KnowledgeBase>(k => k.Code == obj.Code).CountAsync();
            if (num > 0)
            {
                throw new Exception("已存在该编码，请检查。");
            }
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            var org = _auth.GetCurrentUser().Orgs.OrderByDescending(s => s.CascadeId).FirstOrDefault().Name;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            obj.Org = org;
            obj.IsNew = true;
            if (!string.IsNullOrWhiteSpace(obj.ParentId))
            {
                var parent = await UnitWork.Find<KnowledgeBase>(k => k.Code == obj.ParentId).Select(k => new { k.Id, k.Name }).FirstOrDefaultAsync();
                if (parent == null)
                {
                    throw new Exception("无父级目录，请检查。");
                }
                obj.ParentId = parent.Id;
                obj.ParentName = parent.Name;
            }
            else if (obj.Rank == 3)
            {
                var ParentId = obj.Code.Substring(0, 2);
                var parent = await UnitWork.Find<KnowledgeBase>(k => k.Code == ParentId && k.Rank == 2).Select(k => new { k.Id, k.Name }).FirstOrDefaultAsync();
                if (parent == null)
                {
                    throw new Exception("无父级目录，请检查。");
                }
                obj.ParentId = parent.Id;
                obj.ParentName = parent.Name;
            }
            await Repository.AddAsync(obj);
            await Repository.SaveAsync();
        }
        /// <summary>
        /// 修改新知识库
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task NewUpdate(KnowledgeBase obj)
        {
            var user = _auth.GetCurrentUser().User;
            var num = await UnitWork.Find<KnowledgeBase>(k => k.Code == obj.Code && !k.Id.Equals(obj.Id)).CountAsync();
            if (num > 0)
            {
                throw new Exception("已存在该编码，请检查。");
            }
            if (!string.IsNullOrWhiteSpace(obj.ParentId))
            {
                var parent = await UnitWork.Find<KnowledgeBase>(k => k.Code == obj.ParentId).Select(k => new { k.Id, k.Name }).FirstOrDefaultAsync();
                if (parent == null) 
                {
                    throw new Exception("无父级目录，请检查。");
                }
                obj.ParentId = parent.Id;
                obj.ParentName = parent.Name;
            }
            else if (obj.Rank == 3)
            {
                var ParentId = obj.Code.Substring(0, 2);
                var parent = await UnitWork.Find<KnowledgeBase>(k => k.Code == ParentId && k.Rank == 2).Select(k => new { k.Id, k.Name }).FirstOrDefaultAsync();
                if (parent == null)
                {
                    throw new Exception("无父级目录，请检查。");
                }
                obj.ParentId = parent.Id;
                obj.ParentName = parent.Name;
            }
            else
            {
                obj.ParentId = null;
                obj.ParentName = null;
            }
            await UnitWork.UpdateAsync<KnowledgeBase>(k => k.Id.Equals(obj.Id), k => new KnowledgeBase
            {
                ParentId=obj.ParentId,
                ParentName=obj.ParentName,
                Code = obj.Code,
                Name = obj.Name,
                UpdateTime = DateTime.Now,
                UpdateUserId = user.Id,
                UpdateUserName = user.Name,
            });
            if (string.IsNullOrWhiteSpace(obj.ParentId)) 
            {
                await UnitWork.UpdateAsync<KnowledgeBase>(k => k.ParentId.Equals(obj.Id), k => new KnowledgeBase
                {
                    ParentName = obj.Name
                });
            }
            await UnitWork.SaveAsync();
        }
        /// <summary>
        /// 删除新知识库
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task NewDelete(QueryKnowledgeBaseListReq request)
        {
            var user = _auth.GetCurrentUser().User;
            var knowledge = await UnitWork.Find<KnowledgeBase>(k => k.Id == request.Id || k.ParentId == request.Id).ToListAsync();
            var parentIds = knowledge.Select(k => k.Id);
            knowledge = await UnitWork.Find<KnowledgeBase>(k => parentIds.Contains(k.ParentId) || parentIds.Contains(k.Id)).ToListAsync();
            await UnitWork.BatchDeleteAsync<KnowledgeBase>(knowledge.ToArray());
            await UnitWork.SaveAsync();
        }
        /// <summary>
        /// 导入知识库
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public async Task ImportRepository(ExcelHandler handler)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var KnowledgeBaseList = handler.GetListData<KnowledgeBase>(mapper =>
            {
                var data = mapper
                .Map<KnowledgeBase>(0, a => a.Code)
                .Map<KnowledgeBase>(1, a => a.Name)
                .Map<KnowledgeBase>(2, a => a.Rank)
                .Map<KnowledgeBase>(3, a => a.Content)
                .Take<KnowledgeBase>(0);
                return data.Select(d => d.Value).SkipWhile(v => v is null).ToList();
            });
            KnowledgeBaseList.ForEach(k =>
            {
                k.CreateUserId = loginContext.User.Id;
                k.CreateUserName = loginContext.User.Name;
                k.CreateTime = DateTime.Now;
                k.IsNew = true;
            });
            await UnitWork.BatchAddAsync<KnowledgeBase>(KnowledgeBaseList.Where(k=>k.Rank!=3).ToArray());
            await UnitWork.SaveAsync();
            var knowledgeBases= await UnitWork.Find<KnowledgeBase>(k => k.IsNew == true && k.Rank< 3).Select(k=>new { k.Id,k.Name,k.Code}).ToListAsync();
            KnowledgeBaseList.Where(k => k.Rank == 3).ForEach(k =>
            {
                var code = k.Code.Substring(0, 2);
                var knowledgeBase = knowledgeBases.Where(k=>k.Code== code).FirstOrDefault();
                k.ParentId = knowledgeBase.Id;
                k.ParentName= knowledgeBase.Name;
            });
            await UnitWork.BatchAddAsync<KnowledgeBase>(KnowledgeBaseList.Where(k => k.Rank == 3).ToArray());
            await UnitWork.SaveAsync();
        }
        public KnowledgeBaseApp(IUnitWork unitWork, IRepository<KnowledgeBase> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
        }
    }
}