using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.Extensions.Options;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class FormApp : BaseApp<Form>
    {
        private IOptions<AppSetting> _appConfiguration;
        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryFormListReq request)
        {
            var result = new TableData();
            var forms = GetDataPrivilege("u");
            if (!string.IsNullOrEmpty(request.key))
            {
                forms = forms.Where(u => u.Name.Contains(request.key) || u.Id.Contains(request.key));
            }

            result.Data = forms.OrderByDescending(u => u.CreateDate)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToList();
            result.Count = forms.Count();
            return result;
        }

        public void Add(Form obj)
        {
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            UnitWork.Add(obj);
            if (!string.IsNullOrEmpty(obj.DbName))
            {
                UnitWork.ExecuteSql(FormUtil.GetSql(obj, _appConfiguration.Value.DbType), ContextType.DefaultContextType);
            }
            UnitWork.Save();
        }

        public void Update(Form obj)
        {
            Repository.Update(u => u.Id == obj.Id, u => new Form
            {
                ContentData = obj.ContentData,
                Content = obj.Content,
                ContentParse = obj.ContentParse,
                Name = obj.Name,
                Disabled = obj.Disabled,
                DbName = obj.DbName,
                SortCode = obj.SortCode,
                Description = obj.Description,
                OrgId =  obj.OrgId,
                ModifyDate = DateTime.Now
            });

            if (!string.IsNullOrEmpty(obj.DbName))
            {
                UnitWork.ExecuteSql(FormUtil.GetSql(obj, _appConfiguration.Value.DbType), ContextType.DefaultContextType);
            }
        }

        public FormResp FindSingle(string id)
        {
            var form = Get(id);
            return form.MapTo<FormResp>();
        }
        public async Task<FormResp> FindSingleAsync(string id)
        {
            var form = await GetAsync(id);
            return form.MapTo<FormResp>();
        }

        public FormApp(IUnitWork unitWork, IRepository<Form> repository,
            IAuth auth, IOptions<AppSetting> appConfiguration) : base(unitWork, repository, auth)
        {
            _appConfiguration = appConfiguration;
        }
    }
}