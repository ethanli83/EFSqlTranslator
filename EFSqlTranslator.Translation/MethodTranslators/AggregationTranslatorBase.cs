using System;
using System.Linq;
using System.Linq.Expressions;
using EFSqlTranslator.Translation.DbObjects;

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
            }
            else
            {
                var alias = nameGenerator.GenerateAlias(dbSelect, dbFunc.Name, true);
                dbFunc.Alias = alias;
                childSelect.Selection.Add(dbFunc);

                var cRef = dbSelect.Joins.Single(j => ReferenceEquals(j.To.Referee, childSelect)).To;
                var column = _dbFactory.BuildColumn(cRef, alias, m.Method.ReturnType);

                var dbDefaultVal = _dbFactory.BuildConstant(Activator.CreateInstance(m.Method.ReturnType));
                var dbIsNullFunc = _dbFactory.BuildNullCheckFunc(column, dbDefaultVal);

                state.ResultStack.Push(dbIsNullFunc);
            }
        }
    }
}