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
    public class BeforeSaleDemandProjectApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;
        public BeforeSaleDemandProjectApp(IUnitWork unitWork,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, auth)
        {
            _revelanceApp = app;
        }
        /// <summary>
        /// 加载项目列表
        /// </summary>
        public async Task<TableData> Load(QueryBeforeSaleDemandProjectListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var BeforeSaleDemandIds = await UnitWork.Find<BeforeSaleDemandOperationHistory>(x => x.CreateUserId == loginContext.User.Id).Select(x => x.BeforeSaleDemandId).Distinct().ToListAsync();

            TableData result = new TableData();
            var query = UnitWork.Find<BeforeSaleDemandProject>(null)
             .Where(x=> BeforeSaleDemandIds.Contains(x.BeforeSaleDemandId.Value))
            //.Include(b => b.BeforeSaleProSchedulings)
            .WhereIf(!string.IsNullOrWhiteSpace(req.PromoterName), c => c.PromoterName.Contains(req.PromoterName))
            .WhereIf(!string.IsNullOrWhiteSpace(req.KeyWord), k => k.ProjectName.Contains(req.KeyWord) || k.ProjectNum.Contains(req.KeyWord) || k.CustomerId.Contains(req.KeyWord) || k.CustomerName.Contains(req.KeyWord) || k.ReqUserName.Contains(req.KeyWord) || k.DevUserName.Contains(req.KeyWord) || k.TestUserName.Contains(req.KeyWord))
            .WhereIf(!string.IsNullOrWhiteSpace(req.CreateTimeStart.ToString()), q => q.CreateTime > req.CreateTimeStart)
            .WhereIf(!string.IsNullOrWhiteSpace(req.CreateTimeEnd.ToString()), q => q.CreateTime < Convert.ToDateTime(req.CreateTimeStart).AddDays(1));
            if (req.Status != null && req.Status != 0)//所有项目流程
            {
                query = query.Where(c => c.Status == req.Status);
            }
            var resp = await query.OrderByDescending(c => c.CreateTime).Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync();

            List<string> userIds = new List<string>();

            userIds.AddRange(resp.Select(a => a.PromoterId));
            userIds.AddRange(resp.Select(a => a.ReqUserId));
            userIds.AddRange(resp.Select(a => a.DevUserId));
            userIds.AddRange(resp.Select(a => a.TestUserId));
            userIds.AddRange(resp.Select(a => a.ExecutorUserId));
            userIds = userIds.Distinct().ToList();

            var user = (from a in UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && userIds.Contains(r.FirstId))
                        join c in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on a.SecondId equals c.Id
                        select new { a.FirstId, c.Name }).ToList();

            var dxxx = resp.SelectMany(a => a.PromoterId);
            foreach (var item in resp) 
            {
                item.PromoterName = user.FirstOrDefault(a => a.FirstId == item.PromoterId)?.Name + "-" + item.PromoterName;
                item.ReqUserName = user.FirstOrDefault(a => a.FirstId == item.ReqUserId)?.Name + "-" + item.ReqUserName;
                item.DevUserName = user.FirstOrDefault(a => a.FirstId == item.DevUserId)?.Name + "-" + item.DevUserName;
                item.TestUserName = user.FirstOrDefault(a => a.FirstId == item.TestUserId)?.Name + "-" + item.TestUserName;
                item.ExecutorName = user.FirstOrDefault(a => a.FirstId == item.ExecutorUserId)?.Name + "-" + item.ExecutorName;
            }
            result.Data = resp;
            result.Count = await query.CountAsync();
            return result;
        }


        /// <summary>
        /// 获取项目详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TableData> GetDetails(int id)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var detail = await UnitWork.Find<BeforeSaleDemandProject>(c => c.Id == id)
                            .FirstOrDefaultAsync();
            //项目排期信息
            var beforeSaleProSchedulings = await UnitWork.Find<BeforeSaleProScheduling>(c => c.BeforeSaleDemandProjectId == detail.Id)
                .OrderBy(c => c.Id).Select(h => new
                {
                    h.BeforeSaleDemandId,
                    h.BeforeSaleDemandProjectId,
                    h.CreateTime,
                    h.Stage,
                    h.UserId,
                    h.UserName,
                    h.StartDate,
                    h.EndDate
                }).ToListAsync();
            result.Data = new
            {
                detail.ActualStartDate,
                detail.SubmitDate,
                detail.DevUserId,
                detail.DevUserName,
                detail.ActualDevStartDate,
                detail.ActualDevEndDate,
                detail.TestUserId,
                detail.TestUserName,
                detail.ActualTestStartDate,
                detail.ActualTestEndDate,
                beforeSaleProSchedulings
            };
            return result;
        }
        
        public async Task<TableData> GetGanttData(QueryBeforeSaleDemandProjectListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            TableData result = new TableData();
            var query = UnitWork.Find<BeforeSaleDemand>(null)
                        .Include(c => c.BeforeSaleProSchedulings)
                        .Where(c => c.BeforeSaleDemandProjectId ==req.ProjectId);

            var resp = await query.OrderByDescending(c => c.CreateTime).Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync();
            result.Data = resp.ToList();
            result.Count = await query.CountAsync();
            return result;
        }

        public void Add(AddOrUpdateBeforeSaleDemandProjectReq req)
        {
            var obj = req.MapTo<BeforeSaleDemandProject>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            //Repository.Add(obj);
        }

        public void Update(AddOrUpdateBeforeSaleDemandProjectReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<BeforeSaleDemandProject>(u => u.Id.Equals(obj.Id), u => new BeforeSaleDemandProject
            {
                ProjectNum = obj.ProjectNum,
                PromoterId = obj.PromoterId,
                PromoterName = obj.PromoterName,
                ReqUserId = obj.ReqUserId,
                ReqUserName = obj.ReqUserName,
                DevUserId = obj.DevUserId,
                DevUserName = obj.DevUserName,
                TestUserId = obj.TestUserId,
                TestUserName = obj.TestUserName,
                ActualStartDate = obj.ActualStartDate,
                SubmitDate = obj.SubmitDate,
                FlowInstanceId = obj.FlowInstanceId,
                Status = obj.Status,
                ProjectUrl = obj.ProjectUrl,
                ProjectDocURL = obj.ProjectDocURL,
                ActualDevStartDate = obj.ActualDevStartDate,
                ActualDevEndDate = obj.ActualDevEndDate,
                CreateUserName = obj.CreateUserName,
                CreateUserId = obj.CreateUserId,
                CreateTime = obj.CreateTime,
                UpdateTime = obj.UpdateTime
                //todo:补充或调整自己需要的字段
            });

        }
    }
}