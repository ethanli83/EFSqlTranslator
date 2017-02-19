using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation
{
    public static class QueryableExtensions
    {
        public static IQueryable<TResult> Like<TOuter, TInner, TResult>(
            this IQueryable<TOuter> outer,
            IQueryable<TInner> inner,
            Expression<Func<TOuter, TInner,bool>> joinCondition,
            Expression<Func<TOuter, TInner, TResult>> resultSelector,
            DbJoinType dbJoinType = DbJoinType.Inner)
        {
            var method = typeof(QueryableExtensions).GetMethod("Join");
            var callExpression = Expression.Call(
                null,
                method.MakeGenericMethod(new []
                {
                    typeof(TOuter),
                    typeof(TInner),
                    typeof(TResult)
                }),
                new Expression[]
                {
                    outer.Expression,
                    inner.Expression,
                    Expression.Quote(joinCondition),
                    Expression.Quote(resultSelector),
                    Expression.Constant(dbJoinType)
                });

            return outer.Provider.CreateQuery<TResult>(callExpression);
        }

        public static IQueryable<TResult> Join<TOuter, TInner, TResult>(
            this IQueryable<TOuter> outer,
            IQueryable<TInner> inner,
            Expression<Func<TOuter, TInner,bool>> joinCondition,
            Expression<Func<TOuter, TInner, TResult>> resultSelector,
            DbJoinType dbJoinType = DbJoinType.Inner)
        {   
            var method = typeof(QueryableExtensions).GetMethod("Join");
            var callExpression = Expression.Call(
                null,
                method.MakeGenericMethod(new []
                    { 
                        typeof(TOuter),
                        typeof(TInner),
                        typeof(TResult) 
                    }),
                new Expression[] 
                    { 
                        outer.Expression, 
                        inner.Expression,
                        Expression.Quote(joinCondition),
                        Expression.Quote(resultSelector),
                        Expression.Constant(dbJoinType)
                    });
                    
            return outer.Provider.CreateQuery<TResult>(callExpression);
        }
    }
}