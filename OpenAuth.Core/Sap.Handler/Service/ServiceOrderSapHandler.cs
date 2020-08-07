using DotNetCore.CAP;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sap.Handler.Service
{
    public class ServiceOrderSapHandler : ICapSubscribe
    {
        private readonly IUnitWork UnitWrok;

        public ServiceOrderSapHandler(IUnitWork unitWrok)
        {
            UnitWrok = unitWrok;
        }

        [CapSubscribe("Serve.ServcieOrder.Create")]
        public async Task HandleServiceOrder(int id)
        {

        }
    }
}
