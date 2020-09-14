using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.App
{
    public class AppUserBindApp : BaseApp<AppUserBind>
    {
        private RevelanceManagerApp _revelanceApp;

        public AppUserBindApp(IUnitWork unitWork, IRepository<AppUserBind> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
        }


        // <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryAppUserBindListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }


            var result = new TableData();
            var objs = UnitWork.Find<AppUserBind>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id.Contains(request.key));
            }
            result.Data = objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToList();
            result.Count = objs.Count();
            return result;
        }

        /// <summary>
        /// 添加绑定申请记录
        /// </summary>
        /// <param name="req"></param>
        public async Task AddOrUpdateAppUserBind(AddOrUpdateAppUserBindReq req)
        {
            //判断是否存在正在审核的绑定申请信息 有则做更新操作 否则新一条申请记录 
            var isExist = (await UnitWork.Find<AppUserBind>(a => a.AppUserId == req.AppUserId && a.IsDeleted == 0 && a.AuditState == 0).ToListAsync()).Count;
            if (isExist > 0)
            {
                await UnitWork.UpdateAsync<AppUserBind>(u => u.AppUserId == req.AppUserId && u.IsDeleted == 0 && u.AuditState == 0, u => new AppUserBind
                {
                    RealName = req.RealName,
                    CustomerCode = req.CustomerCode,
                    CustomerName = req.CustomerName,
                    Linkman = req.Linkman,
                    LinkmanTel = req.LinkmanTel
                });
            }
            else
            {
                var obj = req.MapTo<AppUserBind>();
                //todo:补充或调整自己需要的字段
                obj.CreateTime = DateTime.Now;
                obj.AuditState = 0;
                await UnitWork.AddAsync(obj);
            }
            //将之前成功的记录标示为已删除
            var successId = (await UnitWork.Find<AppUserBind>(a => a.AppUserId == req.AppUserId && a.IsDeleted == 0 && a.AuditState == 0).FirstOrDefaultAsync())?.Id;
            if (!string.IsNullOrEmpty(successId))
            {
                await UnitWork.UpdateAsync<AppUserBind>(u => u.Id == successId, u => new AppUserBind
                {
                    IsDeleted = 1
                });
            }
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 审核客户绑定申请
        /// </summary>
        /// <param name="id">申请Id</param>
        /// <param name="type">审核操作 1审核成功 2审核失败</param>
        /// <param name="reason">审核失败原因</param>
        public async Task AuditApply(string id, int type, string reason)
        {
            var user = _auth.GetCurrentUser().User;

            await UnitWork.UpdateAsync<AppUserBind>(u => u.Id == id, u => new AppUserBind
            {
                AuditState = type,
                AuditTime = DateTime.Now,
                Auditer = user.Id,
                AuditerName = user.Name,
                RefuseReason = reason
            });
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 获取绑定结果
        /// </summary>
        /// <param name="appUserId"></param>
        /// <returns></returns>
        public async Task<TableData> GetBindInfo(int appUserId)
        {
            var result = new TableData();
            var bindInfo = await UnitWork.Find<AppUserBind>(a => a.AppUserId == appUserId).OrderByDescending(o => o.CreateTime)
                .Select(s => new { s.CustomerCode, s.CustomerName, s.Linkman, s.LinkmanTel, s.RealName, s.AuditState, s.RefuseReason }).FirstOrDefaultAsync();
            result.Data = bindInfo;
            return result;
        }
    }
}