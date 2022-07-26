using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App
{
    /// <summary>
    /// 必修课模块
    /// </summary>
    public class CompulsoryCourseApp:OnlyUnitWorkBaeApp
    {
        /// <summary>
        /// 必修课模块
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public CompulsoryCourseApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> TeacherList()
        {
            var result = new TableData();
            result.Data= await UnitWork.Find<classroom_teacher>(null).ToListAsync();
            return result;
        }
    }
}
