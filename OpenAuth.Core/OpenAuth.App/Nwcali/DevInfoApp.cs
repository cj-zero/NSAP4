using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class DevInfoApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;
        public DevInfoApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
        }

        public async Task<TableData> GetDetails(long id)
        {
            var result = new TableData();
            var query = UnitWork.Find<DevInfo>(null).Where(c => c.Id == id);

            result.Data = await query.ToListAsync();
            result.Count = await query.CountAsync();
            return result;
        }
        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryDevInfoListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("devinfo");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<DevInfo>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.order_no.Contains(request.key));
            }


            var propertyStr = string.Join(',', properties.Select(u => u.Key));
            result.columnHeaders = properties;
            result.Data = objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select($"new ({propertyStr})");
            result.Count = objs.Count();
            return result;
        }

        public async Task Add(AddOrUpdateDevInfoReq req)
        {
            var obj = req.MapTo<DevInfo>();
            //todo:补充或调整自己需要的字段
            //obj.create_time = DateTime.UtcNow;
            var user = _auth.GetCurrentUser().User;
            obj.test_user = user.Name;
            obj = await UnitWork.AddAsync<DevInfo, int>(obj);
            await UnitWork.SaveAsync();
        }
        public async Task Update(AddOrUpdateDevInfoReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<DevInfo>(u => u.Id == obj.id, u => new DevInfo
            {
                edge_id = obj.edge_id,
                srv_guid = obj.srv_guid,
                mid_guid = obj.mid_guid,
                order_no = obj.order_no,
                bts_server_ip = obj.bts_server_ip,
                mid_ip = obj.mid_ip,
                chl_id = obj.chl_id,
                low_id = obj.low_id,
                mid_id = obj.mid_id,
                pyh_id = obj.pyh_id,
                bts_type = obj.bts_type,
                dev_uid = obj.dev_uid,
                low_no = obj.low_no,
                low_guid = obj.low_guid,
                unit_id = obj.unit_id,
                test_id = obj.test_id,
                test_user = obj.test_user,
                create_time = obj.create_time,
                test_status = obj.test_status,
                update_time = obj.update_time
                //todo:补充或调整自己需要的字段
            });

        }
        public async Task Delete(List<long> ids)
        {
            await UnitWork.DeleteAsync<DevInfo>(u => ids.Contains(u.Id));
        }
    }
}