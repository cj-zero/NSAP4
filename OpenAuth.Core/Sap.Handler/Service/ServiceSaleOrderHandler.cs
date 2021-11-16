using DotNetCore.CAP;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using SAPbobsCOM;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAuth.Repository.Domain.Sap;
using System.Reactive;
using Sap.Handler.Service.Request;

namespace Sap.Handler.Service
{
    /// <summary>
    /// 销售报价单同步SAP
    /// </summary>
    public class ServiceSaleOrderHandler : ICapSubscribe
    {
        private readonly IUnitWork UnitWork;
        private readonly Company company;

        public ServiceSaleOrderHandler(IUnitWork unitWork, Company company)
        {
            UnitWork = unitWork;
            this.company = company;
        }
        [CapSubscribe("Serve.ServiceSaleOrder.Create")]
        public async Task HandleServiceOrder(int theServiceOrderId)
        {
        }
    }
}
