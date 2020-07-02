using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Response;
using OpenAuth.App.Sap.Request;
using OpenAuth.App.Sap.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OpenAuth.WebApi.Controllers.Sap
{
    [Route("api/Sap/[controller]/[action]")]
    [ApiController]
    public class SerialNumberController : ControllerBase
    {
        private readonly SerialNumberApp _serialNumberApp;

        public SerialNumberController(SerialNumberApp serialNumberApp)
        {
            _serialNumberApp = serialNumberApp;
        }

        [HttpGet]
        public async Task<TableData> Get([FromQuery]QuerySerialNumberListReq req)
        {
            var result = new TableData();
            try
            {
                result = await _serialNumberApp.Find(req);
            }
            catch (Exception ex)
            {
                result.code = 500;
                result.msg = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }
    }
}
