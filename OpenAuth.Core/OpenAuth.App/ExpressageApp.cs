using Infrastructure;
using KuaiDi100.Common.Request;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenAuth.App.Interface;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.App
{
    public class ExpressageApp : OnlyUnitWorkBaeApp
    {
        public ExpressageApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {

        }

        /// <summary>
        /// 查询物流信息
        /// </summary>
        /// <param name="trackNumber"></param>
        /// <returns></returns>
        public async Task<TableData> GetExpressInfo(string trackNumber)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            //根据快递单号查询快递公司编码
            string comCode = AutoNum.query(trackNumber);
            if (comCode != "[]")
            {
                string com = JsonConvert.DeserializeObject<dynamic>(comCode)[0].comCode;
                QueryTrackParam trackReq = new QueryTrackParam
                {
                    com = com,
                    num = trackNumber,
                    resultv2 = "2"
                };
                string response = QueryTrack.queryTrackInfo(trackReq);
                var returndata = JsonConvert.DeserializeObject<dynamic>(response);
                string message = returndata.message;
                if ("ok".Equals(message))
                {
                    result.Data = response;
                    return result;
                }
                else
                {
                    result.Code = 500;
                    result.Message = message;
                    return result;
                }
            }
            else
            {
                result.Code = 500;
                result.Message = "未查询到有效的物流信息";
                return result;
            }
        }
    }
}
