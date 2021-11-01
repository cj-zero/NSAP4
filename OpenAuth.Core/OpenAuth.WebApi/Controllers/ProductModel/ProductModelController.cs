using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.ProductModel;
using OpenAuth.App.ProductModel.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.ProductModel
{
    [Route("api/Meeting/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Meeting")]
    public class ProductModelController : Controller
    {
        private readonly ServiceProductModelApp _serviceProductModelApp;
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        /// <summary>
        /// 选型列表
        /// </summary>
        /// <param name="QueryModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Load")]
        public async Task<TableData> Load(ProductModelReq QueryModel)
        {
            int rowcount = 0;
            var result = new TableData();
            try
            {
                result.Data = _serviceProductModelApp.Load(QueryModel, out rowcount);
                result.Count = rowcount;

            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }


            return result;
        }

    }
}
