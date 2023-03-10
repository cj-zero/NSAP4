using Infrastructure;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System.Threading.Tasks;

namespace OpenAuth.App
{
    public class FrmLeaveReqApp : BaseApp<FrmLeaveReq>, ICustomerForm
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryFrmLeaveReqListReq request)
        {
             return new TableData
            {
                Count = Repository.GetCount(null),
                Data = Repository.Find(request.page, request.limit, "Id desc")
            };
        }

        public void Add(FrmLeaveReq obj)
        {
            Repository.Add(obj);
        }
        
        public async Task AddAsync(FrmLeaveReq obj)
        {
            await Repository.AddAsync(obj);
        }
        
        public void Update(FrmLeaveReq obj)
        {
            UnitWork.Update<FrmLeaveReq>(u => u.Id == obj.Id, u => new FrmLeaveReq
            {
               //todo:要修改的字段赋值
            });

        }

        public FrmLeaveReqApp(IUnitWork unitWork, IRepository<FrmLeaveReq> repository,
            RevelanceManagerApp app,IAuth auth) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
        }

        public void Add(string flowInstanceId, string frmData)
        {
            var req = JsonHelper.Instance.Deserialize<FrmLeaveReq>(frmData);
            req.FlowInstanceId = flowInstanceId;
            Add(req);
        }
        public async Task AddAsync(string flowInstanceId, string frmData)
        {
            var req = JsonHelper.Instance.Deserialize<FrmLeaveReq>(frmData);
            req.FlowInstanceId = flowInstanceId;
            await AddAsync(req);
        }
    }
}