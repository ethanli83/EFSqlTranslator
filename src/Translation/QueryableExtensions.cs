using System;
using System.Linq;
using System.Linq.Expressions;

namespace Translation
{
    public static class QueryableExtensions
    {
        public static IQueryable<TResult> Join<TOuter, TInner, TResult>(
            this IQueryable<TOuter> outer,
            IQueryable<TInner> inner,
            Expression<Func<TOuter, TInner,bool>> joinCondition,
            Expression<Func<TOuter, TInner, TResult>> resultSelector,
            JoinType joinType = JoinType.Inner)
        {   
            return null;
        }
    }
}