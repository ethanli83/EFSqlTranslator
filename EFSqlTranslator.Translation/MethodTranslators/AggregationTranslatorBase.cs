using System;
using System.Linq;
using System.Linq.Expressions;
using EFSqlTranslator.Translation.DbObjects;
using EFSqlTranslator.Translation.Extensions;

namespace EFSqlTranslator.Translation.MethodTranslators
{
    public abstract class AggregationTranslatorBase : AbstractMethodTranslator
    {
        protected AggregationTranslatorBase(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory)
            : base(infoProvider, dbFactory)
        {
        }

        protected void CreateAggregation(
            MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator,
            IDbSelect childSelect, IDbFunc dbFunc)
        {
            var dbSelect = (IDbSelect)state.ResultStack.Peek();

            if (childSelect == null)
            {
                state.ResultStack.Push(dbFunc);
                return;
            }

            var alias = nameGenerator.GenerateAlias(dbSelect, dbFunc.Name, true);
            dbFunc.Alias = alias;
            childSelect.Selection.Add(dbFunc);

            /**
             * Aggregation can happen after a method that generate new select.
             * In this case, join from the main select to the child select will not be
             * updated yet at this stage, so we need to correct the join to on the
             * correct child select statment. 
             * For example:
             * var query = db.Blogs
             *     .Where(b => b.BlogId > 0)
             *     .Select(b => new 
             *      {
             *          b.BlogId,
             *          Cnt = b.Posts.Select(p => p.Title).Distinct().Count()
             *      });
             * `b.Posts.Select(p => p.Title).Distinct()` will create a new child select and
             * it will not be the one that the main select is currently targeting, so we 
             * need to correct the join target.
             */
            ReLinkToChildSelect(dbSelect, childSelect);

            var cRef = dbSelect.Joins.Single(j => ReferenceEquals(j.To.Referee, childSelect)).To;
            var column = _dbFactory.BuildColumn(cRef, alias, m.Method.ReturnType);

            var dbDefaultVal = _dbFactory.BuildConstant(Activator.CreateInstance(m.Method.ReturnType));
            var dbIsNullFunc = _dbFactory.BuildNullCheckFunc(column, dbDefaultVal);

            state.ResultStack.Push(dbIsNullFunc);
        }

        protected static IDbSelectable GetAggregationTarget(MethodCallExpression m, TranslationState state)
        {
            if (!m.GetArguments().Any())
                return null;

            var dbObj = state.ResultStack.Pop();
            return (IDbSelectable)dbObj;
        }

        private void ReLinkToChildSelect(IDbSelect dbSelect, IDbSelect childSelect) 
        {
            var joinToRelink = dbSelect.Joins.SingleOrDefault(j => ReferenceEquals(j.To.Referee, childSelect.From.Referee));
            if (joinToRelink == null) {
                return;
            }
            joinToRelink.To = _dbFactory.BuildRef(childSelect, joinToRelink.To.Alias);
        }
    }
}