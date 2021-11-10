using System;
using System.Collections.Generic;
using System.Text;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using OpenAuth.App;

namespace OpenAuth.App
{
    public class RequestActionLogApp : BaseApp<RequestActionLog>
    {
        public RequestActionLogApp(IUnitWork unitWork, IRepository<RequestActionLog> repository) : base(unitWork, repository, null) { }

        public void Add(RequestActionLog obj)
        {
            UnitWork.AddAsync(obj);
            UnitWork.SaveAsync();
        }
    }
}
