using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.ClientRelation;
using OpenAuth.App.Material;

namespace OpenAuth.WebApi.Controllers.Material
{
    /// <summary>
    /// 工程部账号绑定相关
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Material")]
    public class ManageAccBindController : ControllerBase
    {
        private readonly ManageAccBindApp _app;
        /// <summary>
        /// construct func
        /// </summary>
        /// <param name="app"></param>
        public ManageAccBindController(ManageAccBindApp app)
        {
            _app = app;
        }



    }
}
