using System;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 用户操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManagerApp _app;

        [HttpGet]
        public Response<UserView> Get(string id)
        {
            var result = new Response<UserView>();
            try
            {
                result.Result = _app.Get(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 修改用户资料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public Response ChangeProfile(ChangeProfileReq request)
        {
            var result = new Response();

            try
            {
                _app.ChangeProfile(request);
                result.Message = "修改成功，重新登录生效";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public Response ChangePassword(ChangePasswordReq request)
        {
            var result = new Response();
            try
            {
                _app.ChangePassword(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        //添加或修改
        [HttpPost]
        public Response<string> AddOrUpdate(UpdateUserReq obj)
        {
            var result = new Response<string>();
            try
            {
                _app.AddOrUpdate(obj);
                result.Result = obj.Id;   //返回ID
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }


        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public TableData Load([FromQuery] QueryUserListReq request)
        {
            return _app.Load(request);
        }

        [HttpPost]
        public Response Delete([FromBody] string[] ids)
        {
            var result = new Response();
            try
            {
                _app.Delete(ids);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 加载指定角色的用户
        /// </summary>
        [HttpGet]
        public TableData LoadByRole([FromQuery] QueryUserListByRoleReq request)
        {
            return _app.LoadByRole(request);
        }
        /// <summary>
        /// 根据用户角色查询用户，可用用户名做条件搜索
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> LoadByRoleName([FromQuery] QueryUserListByRoleNameReq request)
        {
            return await _app.LoadByRoleName(request);
        }
        /// <summary>
        /// 绑定App用户Id
        /// </summary>
        /// <param name="appUserMap"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> BindAppUser(AddOrUpdateAppUserMapReq request)
        {
            await _app.BindAppUser(request);
            return new Response();
        }
        /// <summary>
        /// 加载指定部门的用户
        /// 不包含下级部门的用户
        /// </summary>
        [HttpGet]
        public TableData LoadByOrg([FromQuery] QueryUserListByOrgReq request)
        {
            return _app.LoadByOrg(request);
        }

        [HttpPost]
        public async Task<Response> BlockUp(BlockUpUserReq req)
        {
            var result = new Response();
            try
            {
                await _app.BlockUp(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 根据App用户Id获取用户信息
        /// </summary>
        /// <param name="appUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetUserInfoByAppUserId(int appUserId)
        {
            var result = new TableData();
            try
            {
                result = await _app.GetUserInfoByAppUserId(appUserId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 获取用户全部信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public TableData GetUserAll() 
        {
            var result = new TableData();
            try
            {
                result =  _app.GetUserAll();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
        public UsersController(UserManagerApp app)
        {
            _app = app;
        }
    }
}