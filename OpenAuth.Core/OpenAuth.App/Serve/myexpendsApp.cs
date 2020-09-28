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
    public class MyExpendsApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> Load(QueryMyExpendsListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUserId = loginContext.User.Id;
            if (!string.IsNullOrWhiteSpace(request.AppId.ToString()))
            {
                loginUserId = await UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(request.AppId)).Select(u => u.UserID).FirstOrDefaultAsync();
                //usermodel = await UnitWork.Find<User>(u => u.Id.Equals(loginUserId)).FirstOrDefaultAsync();
            }
            //var properties = loginContext.GetProperties("myexpends");

            //if (properties == null || properties.Count == 0)
            //{
            //    throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            //}

            var result = new TableData();
            var objs = UnitWork.Find<MyExpends>(m=>m.CreateUserId== loginUserId);
            objs = objs.WhereIf(request.StartTime != null && request.EndTime != null, m => m.CreateTime >= request.StartTime && m.CreateTime < Convert.ToDateTime(request.EndTime).AddMinutes(1440));


            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id.Equals(request.key));
            }

            result.Data =await objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            result.Count = objs.Count();
            return result;
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Add(AddOrUpdateMyExpendsReq req)
        {
            var obj = req.MapTo<MyExpends>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            obj = await UnitWork.AddAsync<MyExpends, int>(obj);
            await UnitWork.SaveAsync();
            if (req.ReimburseAttachments.Count > 0)
            {
                var ReimburseAttachments = req.ReimburseAttachments.MapToList<ReimburseAttachment>();
                ReimburseAttachments.ForEach(r => r.ReimburseId = obj.Id);
                await UnitWork.BatchAddAsync<ReimburseAttachment>(ReimburseAttachments.ToArray());
            }

        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="obj"></param>
        public async Task Update(AddOrUpdateMyExpendsReq obj)
        {
            var user = _auth.GetCurrentUser().User;

            if (obj.fileid != null && obj.fileid.Count > 0)
            {
                var ReimburseAttachments = await UnitWork.Find<ReimburseAttachment>(a => obj.fileid.Contains(a.Id) && a.ReimburseType == 5).ToListAsync();
                ReimburseAttachments.ForEach(a => UnitWork.DeleteAsync<ReimburseAttachment>(a));
            }

            await UnitWork.UpdateAsync<MyExpends>(u => u.Id == obj.Id, u => new MyExpends
            {
                FeeType = obj.FeeType,
                SerialNumber = obj.SerialNumber,
                TrafficType = obj.TrafficType,
                Transport = obj.Transport,
                From = obj.From,
                To = obj.To,
                Money = obj.Money,
                InvoiceNumber = obj.InvoiceNumber,
                Remark = obj.Remark,
                CreateTime = obj.CreateTime,
                Days = obj.Days,
                TotalMoney = obj.TotalMoney,
                ExpenseCategory = obj.ExpenseCategory,
                UpdateTime = DateTime.Now,
                UpdateUserId = user.Id,
                UpdateUserName = user.Name
                //todo:补充或调整自己需要的字段
            });

            if (obj.ReimburseAttachments != null && obj.ReimburseAttachments.Count > 0)
            {
                obj.ReimburseAttachments = obj.ReimburseAttachments.Where(a => string.IsNullOrWhiteSpace(a.Id)).ToList();
                if (obj.ReimburseAttachments != null && obj.ReimburseAttachments.Count > 0)
                {
                    var ReimburseAttachments = obj.ReimburseAttachments.MapToList<ReimburseAttachment>();
                    ReimburseAttachments.ForEach(r => r.ReimburseId = Convert.ToInt32(obj.Id));
                    await UnitWork.BatchAddAsync<ReimburseAttachment>(ReimburseAttachments.ToArray());
                }
            }


        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task Delete(List<int> ids)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var objs = await UnitWork.Find<MyExpends>(m => ids.Contains(m.Id)).ToListAsync();
            objs.ForEach(m => UnitWork.DeleteAsync<MyExpends>(m));

            var ReimburseAttachmentsIds = objs.Select(m => m.Id).ToList();

            var ReimburseAttachments = await UnitWork.Find<ReimburseAttachment>(a => ReimburseAttachmentsIds.Contains(a.ReimburseId) && a.ReimburseType == 5).ToListAsync();
            ReimburseAttachments.ForEach(a => UnitWork.DeleteAsync<ReimburseAttachment>(a));

        }
        public MyExpendsApp(IUnitWork unitWork, RevelanceManagerApp app, IAuth auth) : base(unitWork, auth)
        {
            _revelanceApp = app;
        }
    }
}