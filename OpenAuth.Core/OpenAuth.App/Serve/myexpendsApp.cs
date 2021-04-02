using System;
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
        private readonly RevelanceManagerApp _revelanceApp;
        private readonly ReimburseInfoApp _reimburseinfoApp;

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
                user = await GetUserId(Convert.ToInt32(request.AppId));
            }

            var result = new TableData();
            var objs = UnitWork.Find<MyExpends>(m => m.CreateUserId == user.Id && m.IsDelete==false);
            objs = objs.WhereIf(request.StartTime != null, m => m.CreateTime >= request.StartTime);
            objs = objs.WhereIf(request.EndTime != null, m => m.CreateTime < Convert.ToDateTime(request.EndTime).AddMinutes(1440));

            var MyExpend = await objs.OrderBy(u => u.Id)
               .Skip((request.page - 1) * request.limit)
               .Take(request.limit).ToListAsync();
            var MyExpendsDetails = MyExpend.MapToList<MyExpendsResp>();
            var ReimburseAttachmentIds = MyExpendsDetails.Select(m => m.Id).ToList();
            var ReimburseAttachments = await UnitWork.Find<ReimburseAttachment>(r => ReimburseAttachmentIds.Contains(r.ReimburseId) && r.ReimburseType == 5).ToListAsync();
            var fileids = ReimburseAttachments.Select(r => r.FileId).ToList();
            var file = await UnitWork.Find<UploadFile>(f=> fileids.Contains(f.Id)).ToListAsync();
            MyExpendsDetails.ForEach(m => m.ReimburseAttachments = ReimburseAttachments.Where(r=>r.ReimburseId.Equals(m.Id)).Select(r => new ReimburseAttachmentResp
            {
                Id = r.Id,
                FileId = r.FileId,
                AttachmentName = file.FirstOrDefault(f => f.Id.Equals(r.FileId)).FileName,
                FileType = file.FirstOrDefault(f => f.Id.Equals(r.FileId)).FileType,
                AttachmentType = r.AttachmentType,
                ReimburseId = r.ReimburseId,
                ReimburseType = r.ReimburseType
            }).ToList());

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
            var fileids = ReimburseAttachments.Select(r => r.FileId).ToList();
            var file = await UnitWork.Find<UploadFile>(f=> fileids.Contains(f.Id)).ToListAsync();
            var MyExpendsDetails = obj.MapTo<AddOrUpdateMyExpendsReq>();
            MyExpendsDetails.ReimburseAttachments = ReimburseAttachments.Select(r => new ReimburseAttachmentResp
            {
                Id = r.Id,
                FileId = r.FileId,
                AttachmentName = file.FirstOrDefault(f => f.Id.Equals(r.FileId)).FileName,
                FileType = file.FirstOrDefault(f => f.Id.Equals(r.FileId)).FileType,
                AttachmentType = r.AttachmentType,
                ReimburseId = r.ReimburseId,
                ReimburseType = r.ReimburseType,
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
            var user = _auth.GetCurrentUser().User;
            if (user.Account == "App")
            {
                user = await GetUserId(Convert.ToInt32(req.AppId));
            }
            List<string> InvoiceNumbers = new List<string> { req.InvoiceNumber };
            if (!await IsSole(req.AppId.ToString(), req.InvoiceNumber) || !await _reimburseinfoApp.IsSole(InvoiceNumbers))
            {
                throw new CommonException("添加费用失败。发票已使用，不可二次使用！", Define.INVALID_InvoiceNumber);
            }
            var obj = req.MapTo<MyExpends>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            obj.IsDelete = false;
            obj = await UnitWork.AddAsync<MyExpends, int>(obj);
            await UnitWork.SaveAsync();
            if (req.ReimburseAttachments != null && req.ReimburseAttachments.Count > 0)
            {
                var ReimburseAttachments = req.ReimburseAttachments.MapToList<ReimburseAttachment>();
                ReimburseAttachments.ForEach(r => { r.ReimburseId = obj.Id; r.ReimburseType = 5; r.Id = Guid.NewGuid().ToString(); });
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
                user = await GetUserId(Convert.ToInt32(obj.AppId));
            }
            var MyExpendsModel = await UnitWork.Find<MyExpends>(m => m.Id == obj.Id).FirstOrDefaultAsync();
            List<string> InvoiceNumbers = new List<string> { obj.InvoiceNumber };
            if ((MyExpendsModel != null && MyExpendsModel.InvoiceNumber != obj.InvoiceNumber && !await IsSole(user.Id,obj.InvoiceNumber)) || !await _reimburseinfoApp.IsSole(InvoiceNumbers))
            {
                throw new CommonException("添加费用失败。发票已使用，不可二次使用！", Define.INVALID_InvoiceNumber);
            }
            await UnitWork.UpdateAsync<MyExpends>(u => u.Id == obj.Id, u => new MyExpends
            {
                ReimburseType = obj.ReimburseType,
                FeeType = obj.FeeType,
                SerialNumber = obj.SerialNumber,
                TrafficType = obj.TrafficType,
                Transport = obj.Transport,
                From = obj.From,
                To = obj.To,
                Money = obj.Money,
                InvoiceNumber = obj.InvoiceNumber,
                Remark = obj.Remark,
                Days = obj.Days,
                TotalMoney = obj.TotalMoney,
                ExpenseCategory = obj.ExpenseCategory,
                UpdateTime = DateTime.Now,
                UpdateUserId = user.Id,
                UpdateUserName = user.Name,
                InvoiceTime = obj.InvoiceTime
                //todo:补充或调整自己需要的字段
            });

            if (obj.ReimburseAttachments != null && obj.ReimburseAttachments.Count > 0)
            {
                obj.ReimburseAttachments = obj.ReimburseAttachments.Where(a => string.IsNullOrWhiteSpace(a.Id) || a.Id == "0").ToList();
                if (obj.ReimburseAttachments != null && obj.ReimburseAttachments.Count > 0)
                {
                    var ReimburseAttachments = obj.ReimburseAttachments.MapToList<ReimburseAttachment>();
                    ReimburseAttachments.ForEach(r => { r.ReimburseId = Convert.ToInt32(obj.Id); r.ReimburseType = 5; r.Id = Guid.NewGuid().ToString(); });
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
            objs.ForEach(item =>  UnitWork.Delete<MyExpends>(item));

            var ReimburseAttachmentsIds = objs.Select(m => m.Id).ToList();

            var ReimburseAttachments = await UnitWork.Find<ReimburseAttachment>(a => ReimburseAttachmentsIds.Contains(a.ReimburseId) && a.ReimburseType == 5).ToListAsync();
            ReimburseAttachments.ForEach(item => UnitWork.Delete<ReimburseAttachment>(item));

            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="AppId"></param>
        /// <returns></returns>
        private async Task<User> GetUserId(int AppId)
        {
            var userid = UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(AppId)).Select(u => u.UserID).FirstOrDefault();

            return await UnitWork.Find<User>(u => u.Id.Equals(userid)).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 发票号个人唯一
        /// </summary>
        /// <param name="InvoiceNumber"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<bool> IsSole(string UserId,string InvoiceNumber)
        {
            var user = _auth.GetCurrentUser().User;
            if (user.Account == "App")
            {
                user = await GetUserId(Convert.ToInt32(UserId));
            }
            var rta = await UnitWork.Find<MyExpends>(r =>r.CreateUserId== user.Id && r.InvoiceNumber.Equals(InvoiceNumber) && r.IsDelete!=true).CountAsync();
            if (rta > 0)
            {
                return false;
            }
            return true;
        }

        public MyExpendsApp(IUnitWork unitWork, RevelanceManagerApp app, ReimburseInfoApp reimburseinfoApp, IAuth auth) : base(unitWork, auth)
        {
            _revelanceApp = app;
            _reimburseinfoApp = reimburseinfoApp;
        }
    }
}