using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Infrastructure.Extensions
{
    public static class QueryableExtension
    {
        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool excute, Expression<Func<TSource, bool>> predicate)
        {
            if (excute)
                return source.Where(predicate);
            return source;
        }
    }
}
