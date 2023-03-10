using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class SolutionApp : BaseApp<Solution>
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> Load(QuerySolutionListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("solution");

            //if (properties == null || properties.Count == 0)
            //{
            //    throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            //}

            var result = new TableData();
            //var objs = UnitWork.Find<Solution>(s => s.UseBy == 1);
            var objs = UnitWork.Find<Solution>(s => s.IsNew == true).WhereIf(request.Rank != 0, s => s.Rank == request.Rank);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id.Contains(request.key) || u.Subject.Contains(request.key) || u.Symptom.Contains(request.key)).WhereIf(int.TryParse(request.key, out int code), u => u.SltCode == code);
            }

            //var propertyStr = string.Join(',', properties.Select(u => u.Key));
            result.columnHeaders = properties;
            result.Data = await objs.OrderByDescending(u => u.SltCode)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select(x => new
                {
                    x.Id,
                    x.SltCode, //编号
                    x.Status, //状态
                    x.Symptom, //症状
                    x.Subject, //解决方案
                    x.Cause, //原因
                    Code = x.Descriptio + "-" + x.Code, //编码
                    x.Descriptio, //备注
                    x.UpdateUserName, //更新人名字
                    x.CreateTime, //创建时间
                }).ToListAsync();//.Select($"new ({propertyStr})");
            result.Count = await objs.CountAsync();
            return result;
        }
        /// <summary>
        /// 加载技术员解决方案列表
        /// </summary>
        public async Task<TableData> TechnicianLoad(QuerySolutionListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("solution");

            //if (properties == null || properties.Count == 0)
            //{
            //    throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            //}


            var result = new TableData();
            //var objs = UnitWork.Find<Solution>(s => s.UseBy == 2);
            var objs = UnitWork.Find<Solution>(s => s.IsNew == true && s.Rank == request.Rank);
                //.WhereIf(!string.IsNullOrWhiteSpace(request.Subject), c => c.Subject.Contains(request.Subject));
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Subject.Contains(request.key) || u.Symptom.Contains(request.key));
            }


            //var propertyStr = string.Join(',', properties.Select(u => u.Key));
            result.columnHeaders = properties;
            result.Data = await objs.OrderByDescending(u => u.CreateTime)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select(x => new { x.Id, x.Code, ParentCode = x.Descriptio, x.Subject, x.CreateTime, x.CreateUserName, x.UpdateTime, x.ParentName }).ToListAsync();
            result.Count = await objs.CountAsync();
            return result;
        }

        public async Task Add(AddOrUpdateSolutionReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var maxCode = await Repository.Find(s=>s.UseBy == 1).Select(s => s.SltCode).MaxAsync();
            req.SltCode = ++maxCode;
            var obj = req.MapTo<Solution>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = loginContext.User;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            obj.UseBy = 1;
            await Repository.AddAsync(obj);
        }
        public async Task TechnicianAdd(AddOrUpdateSolutionReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //var maxCode = await Repository.Find(s=>s.UseBy == 2).Select(s => s.SltCode).MaxAsync();
            //req.SltCode = ++maxCode;
            var obj = req.MapTo<Solution>();
            if (!string.IsNullOrWhiteSpace(obj.Descriptio))
            {
                var parent = await UnitWork.Find<Solution>(c => c.Code == obj.Descriptio).Select(c => new { c.Id, c.Subject }).FirstOrDefaultAsync();
                var last = await UnitWork.Find<Solution>(c => c.Descriptio == obj.Descriptio && c.Rank == 2).OrderByDescending(c => c.Code).FirstOrDefaultAsync();

                obj.Code = (Convert.ToInt32(last.Code) + 1).ToString().PadLeft(4, '0');
                obj.ParentId = parent.Id;
                obj.ParentName = parent.Subject;
            }
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = loginContext.User;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            obj.IsNew = true;
            //obj.UseBy = 2;
            await Repository.AddAsync(obj);
        }

         public void Update(AddOrUpdateSolutionReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            string ParentId = "", ParentName = "";
            if (!string.IsNullOrWhiteSpace(obj.Descriptio))
            {
                var parent =  UnitWork.Find<Solution>(c => c.Code == obj.Descriptio).Select(c => new { c.Id, c.Subject }).FirstOrDefault();
                var last = UnitWork.Find<Solution>(c => c.Descriptio == obj.Descriptio && c.Rank == 2).OrderByDescending(c => c.Code).FirstOrDefault();
                obj.Code = (Convert.ToInt32(last.Code) + 1).ToString().PadLeft(4, '0');
                ParentId = parent.Id;
                ParentName = parent.Subject;
            }
            UnitWork.Update<Solution>(u => u.Id == obj.Id, u => new Solution
            {
                Code = obj.Code,
                //SltCode = obj.SltCode,
                Subject = obj.Subject,
                ParentId = ParentId,
                ParentName = ParentName,
                //Cause = obj.Cause,
                //Symptom = obj.Symptom,
                Descriptio = obj.Descriptio,
                //Status = obj.Status,
                UpdateTime = DateTime.Now,
                UpdateUserId = user.Id,
                UpdateUserName = user.Name
                //todo:补充或调整自己需要的字段
            });

        }
            

        public SolutionApp(IUnitWork unitWork, IRepository<Solution> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository,auth)
        {
            _revelanceApp = app;
        }
    }
}