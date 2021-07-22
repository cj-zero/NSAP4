using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OpenAuth.Repository.Extensions
{
    /// <summary>
    /// 事务
    /// </summary>
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    //public class UnitOfWorkAttribute : Attribute, IActionFilter
    //{

    //    IUnitWork _unitOfWork { get; set; }
    //    IServiceProvider _serviceProvider { get; set; }
    //    public void OnActionExecuting(ActionExecutingContext context)
    //    {
    //        IServiceProvider provider = context.HttpContext.RequestServices;
    //        _unitOfWork = _serviceProvider.GetService(typeof(IUnitWork)) as IUnitWork;
    //    }
    //    public void OnActionExecuted(ActionExecutedContext context)
    //    {
    //        try
    //        {
    //            var dbContext = _unitOfWork.GetDbContext<Quotation>();
    //            dbContext.Database.BeginTransaction();
    //            using (var transaction = dbContext.Database.BeginTransaction())
    //            {
    //                try
    //                {
    //                    _unitOfWork.Save();
    //                    transaction.Commit();
    //                }
    //                catch (Exception ex)
    //                {
    //                    transaction.RollbackAsync();
    //                    throw ex;
    //                }
    //            }
    //        }
    //        catch (Exception)
    //        {
    //        }
    //    }
    //}
}