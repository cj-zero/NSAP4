using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Infrastructure.QRCode;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client.Content;

namespace OpenAuth.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "General")]
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
            await _capBus.PublishAsync("Serve.ServcieOrder.CreateFromAPP", id);
        }
        //[HttpPost]
        //public async Task<string> TestQrCode([FromForm] IFormFileCollection files)
        //{ 

        //    var file = files[0];
        //    var stream = file.OpenReadStream();

        //    //var bs = new byte[stream.Length];
        //    //await stream.ReadAsync(bs, 0, (int)stream.Length);
        //    return QRCodeReder.Read(stream);
        //}
    }
}
