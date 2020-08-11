using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OpenAuth.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestCapController : ControllerBase
    {
        private ICapPublisher _capBus;

        public TestCapController(ICapPublisher capBus)
        {
            _capBus = capBus;
        }

        [HttpGet]
        public async Task Test(int id)
        {
            await _capBus.PublishAsync("Serve.ServcieOrder.Create", id);
        }
    }
}
