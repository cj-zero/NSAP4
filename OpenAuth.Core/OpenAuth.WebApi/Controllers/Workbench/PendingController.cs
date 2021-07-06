using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Response;
using OpenAuth.App.Workbench;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Workbench
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Workbench")]
    public class PendingController : ControllerBase
    {
        private readonly PendingApp _app;
        [HttpGet]
        public async Task<TableData> Load() 
        {
            return await _app.Load();
        }
       
        public PendingController(PendingApp app)
        {
            _app = app;
        }
    }
}
