using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using OpenAuth.App.Response;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.App.Request;
using OpenAuth.App.CommonHelp;
using Microsoft.EntityFrameworkCore;

namespace OpenAuth.App.Dictionary
{
    public class NewareDictionaryApp : OnlyUnitWorkBaeApp
    {
        private UserDepartMsgHelp _userDepartMsgHelp;
        private IUnitWork _UnitWork;
        private IAuth _auth;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public NewareDictionaryApp(UserDepartMsgHelp userDepartMsgHelp, IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
            _UnitWork = unitWork;
            _auth = auth;
            _userDepartMsgHelp = userDepartMsgHelp;
        }

        /// <summary>
        /// 获取词典列表
        /// </summary>
        /// <param name="request">词典查询实体数据</param>
        /// <returns>返回词典列表信息</returns>
        public async Task<TableData> GetNewareDictionaryList(QueryDictionaryReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();
            var objs = UnitWork.Find<NewareDictionary>(null);
            var newareDictionaries = objs.WhereIf(!string.IsNullOrWhiteSpace(request.Chinese), r => r.Chinese.Contains(request.Chinese))
                                         .WhereIf(!string.IsNullOrWhiteSpace(request.English), r => r.English.Contains(request.English))
                                         .WhereIf(!string.IsNullOrWhiteSpace(request.ChineseExplain), r => r.ChineseExplain.Contains(request.ChineseExplain))
                                         .WhereIf(!string.IsNullOrWhiteSpace(request.EnglishExplain), r => r.EnglishExplain.Contains(request.EnglishExplain))
                                         .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUser), r => r.CreateUser.Contains(request.CreateUser))
                                         .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUserName), r => r.CreateUserName.Contains(request.CreateUserName));

            var newareDictionarieList = await newareDictionaries.Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
            result.Count = await newareDictionaries.CountAsync();
            result.Data = newareDictionarieList.Select(r => new NewareDictionary 
            { 
                Id = r.Id,
                Chinese = r.Chinese,
                English = r.English,
                ChineseExplain = r.ChineseExplain,
                EnglishExplain = r.EnglishExplain,
                CreateUser = r.CreateUser,
                CreateUserName = _userDepartMsgHelp.GetUserOrgName(r.CreateUser) + r.CreateUserName,
                CreateTime = r.CreateTime,
                UpdateTime = r.UpdateTime
            }).ToList();
            return result;
        }

        /// <summary>
        /// 获取词典详情
        /// </summary>
        /// <param name="id">词典id</param>
        /// <returns>返回词典详细信息</returns>
        public async Task<TableData> GetNewareDictionaryDetail(int id)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();
            var newareDictionaries = await UnitWork.Find<NewareDictionary>(r => r.Id == id).ToListAsync();
            if (newareDictionaries != null)
            {
                result.Data = newareDictionaries.Select(r => new NewareDictionary
                {
                    Id = r.Id,
                    Chinese = r.Chinese,
                    English = r.English,
                    ChineseExplain = r.ChineseExplain,
                    EnglishExplain = r.EnglishExplain,
                    CreateUser = r.CreateUser,
                    CreateUserName = _userDepartMsgHelp.GetUserOrgName(r.CreateUser) + r.CreateUserName,
                    CreateTime = r.CreateTime,
                    UpdateTime = r.UpdateTime
                }).ToList(); ;
            }
            else
            {
                result.Message = "该词典不存在";
            }

            return result;
        }

        /// <summary>
        /// 新增词典
        /// </summary>
        /// <param name="obj">新威词典实体数据</param>
        /// <returns>成功返回操作成功/returns>
        public async Task<string> Add(NewareDictionary obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var dbContext = UnitWork.GetDbContext<ContractSeal>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    obj.CreateTime = DateTime.Now;
                    obj.CreateUser = loginUser.Id;
                    obj.CreateUserName = loginUser.Name;
                    obj.UpdateTime = null;
                    obj = await UnitWork.AddAsync<NewareDictionary, int>(obj);
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("创建词典失败,请重试");
                }
            }

            return "操作成功";
        }

        /// <summary>
        /// 修改词典
        /// </summary>
        /// <param name="obj">新威词典实体数据</param>
        /// <returns>成功返回操作成功</returns>
        public async Task<string> UpDate(NewareDictionary obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var dbContext = UnitWork.GetDbContext<ContractSeal>();
            using (var transaction = dbContext.Database.BeginTransactionAsync().ConfigureAwait(false).GetAwaiter().GetResult())
            {
                try
                {
                    //修改词典信息
                    await UnitWork.UpdateAsync<NewareDictionary>(r => r.Id == obj.Id, r => new NewareDictionary
                    {
                        UpdateTime = DateTime.Now,
                        Chinese = obj.Chinese,
                        English = obj.English,
                        ChineseExplain = obj.ChineseExplain,
                        EnglishExplain = obj.EnglishExplain,
                        CreateUser = obj.CreateUser,
                        CreateUserName = obj.CreateUserName,
                        CreateTime = obj.CreateTime
                    });

                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("修改词典信息失败,请重试。");
                }
            }

            return "操作成功";
        }

        /// <summary>
        /// 删除词典
        /// </summary>
        /// <param name="id">词典id</param>
        /// <returns>成功返回操作成功</returns>
        public async Task<string> DeleteDictionary(int id)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            StringBuilder remark = new StringBuilder();
            var newareDictionaries = await UnitWork.Find<NewareDictionary>(r => r.Id == id).ToListAsync();
            if (newareDictionaries != null && newareDictionaries.Count() > 0)
            {
                await UnitWork.DeleteAsync<NewareDictionary>(q => q.Id == id);
                await UnitWork.SaveAsync();
                return "操作成功";
            }
            else
            {
                throw new Exception("删除失败，该词典不存在。");
            }
        }

        /// <summary>
        /// 批量删除词典
        /// </summary>
        /// <param name="idList">词典Id集合</param>
        /// <returns>成功返回操作成功</returns>
        public async Task<string> DeleteDictionarys(List<int> idList)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var newareDictionaries = await UnitWork.Find<NewareDictionary>(r => idList.Contains(r.Id)).ToListAsync();
            if (newareDictionaries != null && newareDictionaries.Count() > 0)
            {
                await UnitWork.BatchDeleteAsync<NewareDictionary>(newareDictionaries.ToArray());
                await UnitWork.SaveAsync();
                return "操作成功";
            }
            else
            {
                throw new Exception("删除失败，该词典不存在。");
            }
        }
    }
}
