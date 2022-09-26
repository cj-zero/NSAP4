using System.IO;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using NStandard;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Order;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;
using OpenAuth.App.Order.Request;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using OpenAuth.App.ClientRelation.Response;
using System.Data;
using System.Text;
using System.Collections.Generic;
using SixLabors.ImageSharp.PixelFormats;
using Newtonsoft.Json.Linq;
using OpenAuth.Repository.Core;
using TencentCloud.Gaap.V20180529.Models;
using Newtonsoft.Json;

namespace OpenAuth.App.ClientRelation
{
    /// <summary>
    /// 客户（中间商与终端）关系操作
    /// </summary>
    public class ClientRelationApp : OnlyUnitWorkBaeApp
    {
        ServiceBaseApp _serviceBaseApp;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public ClientRelationApp(IUnitWork unitWork, IAuth auth, ServiceBaseApp serviceBaseApp) : base(unitWork, auth)
        {
            _serviceBaseApp = serviceBaseApp;   
        }

        /// <summary>
        /// 获取关系列表
        /// </summary>
        /// <param name="clientId">客户编号</param>
        /// <returns>返回关系信息</returns>
        public async Task<RelationGraphRsp> GetClientRelationList(string clientId)
        {
            RelationGraphRsp rgp = new RelationGraphRsp();
            rgp.rootId = clientId;
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var clientRel = await UnitWork.Find<OpenAuth.Repository.Domain.ClientRelation>(u => u.ClientNo == clientId).FirstAsync();
            if (clientRel == null)
            {
                return rgp;
            }

            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("with recursive cte (ClientNo, ClientName,Flag,ParentNo,SubNo) as ( ");
            strSql.AppendFormat("  select     a.ClientNo, a.ClientName,a.Flag,a.ParentNo,a.SubNo  from  clientrelation a ");
            strSql.AppendFormat("  where      a.IsDelete = 0 && a.IsActive =1 && a.OperatorId = \"{1}\" &&  (LOCATE(\"{0}\", a.SubNo) !=0 || LOCATE(\"{0}\", a.ParentNo) !=0) ", clientId, loginContext.User.Id);
            strSql.AppendFormat("  union DISTINCT ");
            strSql.AppendFormat("  select     p.ClientNo, p.ClientName,p.Flag,p.ParentNo,p.SubNo from  clientrelation p ");
            strSql.AppendFormat("   inner join cte ");
            strSql.AppendFormat("           on p.IsDelete = 0 && p.IsActive =1 && p.OperatorId = \"{0}\"  && (LOCATE(cte.ClientNo, p.SubNo)  !=0 || LOCATE(cte.ClientNo, p.ParentNo)  !=0)   limit 100", loginContext.User.Id);
            strSql.AppendFormat(" ) ");
            strSql.AppendFormat(" select * from cte; ");
            List<RawGraph> rawGraphList = new List<RawGraph>();
            //var graphObject = UnitWork.ExcuteSql(ContextType.DefaultContextType, strSql.ToString(),CommandType.Text,null);
            rawGraphList.AddRange(UnitWork.ExcuteSql<RawGraph>(ContextType.DefaultContextType, strSql.ToString(),CommandType.Text,null));
           // rawGraphList.AddRange(IEnumerable<RawGraph>)graphObject).ToList());
            if (rawGraphList.Count!=0)
            {
                foreach (var graph in rawGraphList)
                {
                    #region 节点
                    GraphNodes gn = new GraphNodes();
                    gn.Id = graph.ClientNo;
                    gn.Text = graph.ClientName;
                    gn.flag = graph.Flag;
                    rgp.Nodes.Add(gn);
                    #endregion

                    #region 关系
                    if (graph.ParentNo != null)
                    {
                        var parentLinks = JsonConvert.DeserializeObject<JArray>(graph.ParentNo);
                        foreach (var plink in parentLinks)
                        {
                            rgp.Links.Add(new GraphLinks
                            {
                                From = plink.ToString(),
                                To = clientId
                            });
                        }
                    }
                    if (graph.SubNo != null)
                    {
                        var subLinks = JsonConvert.DeserializeObject<JArray>(graph.SubNo);
                        foreach (var sublink in subLinks)
                        {
                            rgp.Links.Add(new GraphLinks
                            {
                                From = clientId,
                                To = sublink.ToString()
                            });
                        }

                    }
                    #endregion
                }
            }
            return rgp;

        }



    }
}
