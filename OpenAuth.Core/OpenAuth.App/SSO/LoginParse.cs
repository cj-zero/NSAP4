/*
 * 登录解析
 * 处理登录逻辑，验证客户段提交的账号密码，保存登录信息
 */
using System;
using System.Linq;
using Infrastructure;
using Infrastructure.Cache;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App.SSO
{
    public class LoginParse
    {

        //这个地方使用IRepository<User> 而不使用UserManagerApp是防止循环依赖
        public IRepository<User> _app;
        private ICacheContext _cacheContext;
        private readonly IRepository<Application> _appManager;
        private readonly IRepository<Relevance> _appRelevance;
        private readonly IRepository<Repository.Domain.Org> _appOrg;

        public LoginParse(IRepository<Application> appManager, ICacheContext cacheContext, IRepository<User> userApp,
            IRepository<Relevance> appRelevance, IRepository<Repository.Domain.Org> appOrg)
        {
            _appManager = appManager;
            _cacheContext = cacheContext;
            _app = userApp;
            _appRelevance = appRelevance;
            _appOrg = appOrg;
        }

        public  LoginResult Do(PassportLoginRequest model)
        {
            var result = new LoginResult();
            try
            {
                model.Trim();
                //获取应用信息
                var appInfo = _appManager.Find(app=>app.AppKey.Equals(model.AppKey)).FirstOrDefault();
                if (appInfo == null)
                {
                    throw  new Exception("应用不存在");
                }
                //获取用户信息
                User userInfo = null;
                //if (model.Account == Define.SYSTEM_USERNAME)
                //{
                //    userInfo = new User
                //    {
                //        Id = Guid.Empty.ToString(), 
                //        Account = Define.SYSTEM_USERNAME,
                //        Name ="超级管理员",
                //        Password = Define.SYSTEM_USERPWD
                //    };
                //}
                //else
                //{
                    userInfo = _app.FindSingle(u =>u.Account == model.Account);
                //}
           


                if (userInfo == null)
                {
                    throw new Exception("用户不存在");
                }
                if (userInfo.Password != Encryption.Encrypt(model.Password) && userInfo.Password != model.Password)
                {
                    throw new Exception("密码错误");
                }

                if(userInfo.Status == 1)
                {
                    throw new Exception("该用户已停用");
                }
                bool isQuality = false;
                var relevance = _appRelevance.FindSingle(a => a.Key == "UserOrg" && a.FirstId == userInfo.Id);
                if (relevance != null)
                {
                    var userOrg = _appOrg.FindSingle(a => a.Id == relevance.SecondId);
                    if (userOrg!=null && userOrg.Name == "T1")
                    {
                        isQuality = true;
                    }

                }

                var currentSession = new UserAuthSession
                {
                    Account = model.Account,
                    Name = userInfo.Name,
                    Token = Guid.NewGuid().ToString().GetHashCode().ToString("x"),
                    AppKey = model.AppKey,
                    CreateTime = DateTime.Now,
                    isQuality = isQuality
                    //    , IpAddress = HttpContext.Current.Request.UserHostAddress
                };

                //创建Session
                _cacheContext.Set(currentSession.Token, currentSession, DateTime.Now.AddDays(10));

                result.Code = 200;
                result.ReturnUrl = appInfo.ReturnUrl;
                result.Token = currentSession.Token;
                result.Name = userInfo.Name;
                result.isQuality = currentSession.isQuality;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }

            return result;
        }
    }
}