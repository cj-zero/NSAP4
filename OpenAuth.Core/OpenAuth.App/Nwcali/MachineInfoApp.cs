using OpenAuth.App.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App
{
    public class MachineInfoApp:BaseApp<MachineInfo>
    {
        private readonly IRepository<MachineInfo> _repository;
        public MachineInfoApp(IUnitWork unitWork, IRepository<MachineInfo> repository,RevelanceManagerApp app, IAuth auth) : base(unitWork, repository, auth)
        {
            _repository = repository;
        }

        /// <summary>
        /// 批量新增
        /// </summary>
        /// <param name="machineInfos"></param>
        /// <returns></returns>
        public async Task BatchAddAsycn(List<MachineInfo> machineInfos)
        {
            await _repository.BatchAddAsync(machineInfos.ToArray());
        }
    }
}
