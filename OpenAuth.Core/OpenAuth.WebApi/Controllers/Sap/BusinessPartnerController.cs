using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Sap.BusinessPartner;
using OpenAuth.App.Sap.Request;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OpenAuth.WebApi.Controllers.Sap
{
    [Route("Sap/api/[controller]/[action]")]
    [ApiController]
    public class BusinessPartnerController : ControllerBase
    {
        private readonly BusinessPartnerApp _businessPartnerApp;

        public BusinessPartnerController(BusinessPartnerApp businessPartnerApp)
        {
            _businessPartnerApp = businessPartnerApp;
        }
        [HttpGet]
        public async Task<TableData> Load([FromQuery]QueryBusinessPartnerListReq req)
        {
            return await _businessPartnerApp.Load(req);
        }
    }
}
