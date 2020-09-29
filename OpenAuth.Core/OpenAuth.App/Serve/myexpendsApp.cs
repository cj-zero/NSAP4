﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Npoi.Mapper;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Response;
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
            var user = loginContext.User;
            if (loginContext.User.Account == "App")
            {
                user = GetUserId(Convert.ToInt32(request.AppId));
            }

            var result = new TableData();
            var objs = UnitWork.Find<MyExpends>(m => m.CreateUserId == user.Id);
            objs = objs.WhereIf(request.StartTime != null , m => m.CreateTime >= request.StartTime);
            objs = objs.WhereIf(request.EndTime != null, m => m.CreateTime < Convert.ToDateTime(request.EndTime).AddMinutes(1440));

            var MyExpend = await objs.OrderBy(u => u.Id)
               .Skip((request.page - 1) * request.limit)
               .Take(request.limit).ToListAsync();
            var MyExpendsDetails = objs.MapToList<MyExpendsResp>();
            var file = await UnitWork.Find<UploadFile>(null).ToListAsync();
            foreach (var item in MyExpend)
            {
                var ReimburseAttachments = await UnitWork.Find<ReimburseAttachment>(r => r.ReimburseId == item.Id && r.ReimburseType == 5).ToListAsync();
                MyExpendsDetails.Where(m=>m.Id.Equals(item.Id)).ForEach(m=>m.ReimburseAttachments= ReimburseAttachments.Select(r => new ReimburseAttachmentResp
                {
                    Id = r.Id,
                    FileId = r.FileId,
                    AttachmentName = file.Where(f => f.Id.Equals(r.FileId)).Select(f => f.FileName).FirstOrDefault(),
                    AttachmentType = r.AttachmentType,
                    ReimburseId = r.ReimburseId,
                    ReimburseType = r.ReimburseType
                }).ToList());
            }
            MyExpendsDetails.ForEach(m => m.IsImport = 1);
            result.Data = MyExpendsDetails;
            result.Count = objs.Count();
            return result;
        }

        /// <summary>
        /// 费用详情
        /// </summary>
        /// <param name="MyExpendsId"></param>
        /// <returns></returns>
        public async Task<TableData> Details(int MyExpendsId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var obj = await UnitWork.Find<MyExpends>(m => m.Id == MyExpendsId).FirstOrDefaultAsync();
            var result = new TableData();
            var ReimburseAttachments = await UnitWork.Find<ReimburseAttachment>(r => r.ReimburseId == obj.Id && r.ReimburseType == 5).ToListAsync();
            var file = await UnitWork.Find<UploadFile>(null).ToListAsync();
            var MyExpendsDetails = obj.MapTo<AddOrUpdateMyExpendsReq>();
            MyExpendsDetails.ReimburseAttachments = ReimburseAttachments.Select(r => new ReimburseAttachmentResp
            {
                Id = r.Id,
                FileId = r.FileId,
                AttachmentName = file.Where(f => f.Id.Equals(r.FileId)).Select(f => f.FileName).FirstOrDefault(),
                AttachmentType = r.AttachmentType,
                ReimburseId = r.ReimburseId,
                ReimburseType = r.ReimburseType
            }).ToList();
            result.Data = MyExpendsDetails;
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
            if (user.Account == "App")
            {
                user = GetUserId(Convert.ToInt32(req.AppId));
            }
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            obj = await UnitWork.AddAsync<MyExpends, int>(obj);
            await UnitWork.SaveAsync();
            if (req.ReimburseAttachments != null && req.ReimburseAttachments.Count > 0)
            {
                var ReimburseAttachments = req.ReimburseAttachments.MapToList<ReimburseAttachment>();
                ReimburseAttachments.ForEach(r => { r.ReimburseId = obj.Id; r.Id = Guid.NewGuid().ToString(); });
                await UnitWork.BatchAddAsync<ReimburseAttachment>(ReimburseAttachments.ToArray());
            }
            await UnitWork.SaveAsync();
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
            if (user.Account == "App")
            {
                user = GetUserId(Convert.ToInt32(obj.AppId));
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
                obj.ReimburseAttachments = obj.ReimburseAttachments.Where(a => string.IsNullOrWhiteSpace(a.Id) || a.Id=="0").ToList();
                if (obj.ReimburseAttachments != null && obj.ReimburseAttachments.Count > 0)
                {
                    var ReimburseAttachments = obj.ReimburseAttachments.MapToList<ReimburseAttachment>();
                    ReimburseAttachments.ForEach(r => {r.ReimburseId = Convert.ToInt32(obj.Id) ; r.Id = Guid.NewGuid().ToString(); });
                    await UnitWork.BatchAddAsync<ReimburseAttachment>(ReimburseAttachments.ToArray());
                }
            }
            await UnitWork.SaveAsync();

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

            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="AppId"></param>
        /// <returns></returns>
        private User GetUserId(int AppId)
        {
            var userid = UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(AppId)).Select(u => u.UserID).FirstOrDefault();

            return UnitWork.Find<User>(u => u.Id.Equals(userid)).FirstOrDefault();
        }

        public MyExpendsApp(IUnitWork unitWork, RevelanceManagerApp app, IAuth auth) : base(unitWork, auth)
        {
            _revelanceApp = app;
        }
    }
}