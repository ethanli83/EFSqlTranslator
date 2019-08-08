using System;
using System.Linq;
using System.Linq.Expressions;
using EFSqlTranslator.Translation.DbObjects;
using EFSqlTranslator.Translation.Extensions;

namespace EFSqlTranslator.Translation.MethodTranslators
{
    public class AggregationTranslator : AggregationTranslatorBase
    {
        public AggregationTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory)
            : base(infoProvider, dbFactory)
        {
        }

        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("sum", this);
            plugIns.RegisterMethodTranslator("max", this);
            plugIns.RegisterMethodTranslator("min", this);
            plugIns.RegisterMethodTranslator("average", this);
        }

        public override void Translate(MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            var dbExpression = GetAggregationTarget(m, state);

            IDbSelect childSelect = null;
            if (!m.GetCaller().Type.IsGrouping())
                childSelect = state.ResultStack.Pop() as IDbSelect;

            // if the aggregation does not have expression, it means
            // the caller of the aggregation method must be a Select method call
            // In this case, the actual expression that we need to aggregate on,
            // will be inside the child select statement
            if (dbExpression == null && childSelect != null)
            {
                // to get the actual expression, we need to first unwrap the child select
                // because the translation of Select call always wrap the actual select
                childSelect = (IDbSelect)childSelect.From.Referee;

                dbExpression = childSelect.Selection.Last(
                    s => string.IsNullOrEmpty(s.Alias) ||
                         !s.Alias.EndsWith(TranslationConstants.JoinKeySuffix));

                childSelect.Selection.Remove(dbExpression);
                childSelect.GroupBys.Remove(dbExpression);
            }

            if (dbExpression == null)
                throw new NotSupportedException("Can not aggregate.");

            var dbCountFunc = _dbFactory.BuildFunc(GetSqlMethodName(m.Method.Name), true, dbExpression);

            CreateAggregation(m, state, nameGenerator, childSelect, dbCountFunc);
        }

        private static string GetSqlMethodName(string methodName)
        {
            methodName = methodName.ToLower();
            switch (methodName)
            {
                case "average":
                    return "avg";
                default:
                    return methodName;
            }
        }
    }
}