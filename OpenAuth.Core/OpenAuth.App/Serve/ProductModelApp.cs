using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App
{
    /// <summary>
    /// 商品选项服务
    /// </summary>
    public class ProductModelApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;
        public ProductModelApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
        }
    }
}
