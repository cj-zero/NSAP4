using System;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;
using Serilog.Core;

namespace OpenAuth.App.Test
{
    class TestDbExtension :TestBase
    {
        private ILogger log = Log.Logger;

        [Test]
        public void TestGetProperties()
        {
           
            var app = _autofacServiceProvider.GetService<DbExtension>();

            var result = app.GetProperties("Category");
            Console.WriteLine(JsonHelper.Instance.Serialize(result));
        }
    }
}
