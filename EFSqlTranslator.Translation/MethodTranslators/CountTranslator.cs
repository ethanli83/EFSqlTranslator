using System;
using System.Linq;
using System.Linq.Expressions;
using EFSqlTranslator.Translation.DbObjects;
using EFSqlTranslator.Translation.Extensions;

namespace EFSqlTranslator.Translation.MethodTranslators
{
    public class CountTranslator : AggregationTranslatorBase
    {
        public CountTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory)
            : base(infoProvider, dbFactory)
        {
        }
        
        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("count", this);
        }

        public override void Translate(
            MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            var predicate = BuildCondition(m, state);

            IDbSelect childSelect = null;
            if (!m.GetCaller().Type.IsGrouping())
                childSelect = state.ResultStack.Pop() as IDbSelect;

            var dbCountFunc = _dbFactory.BuildFunc(m.Method.Name.ToLower(), true, predicate);

            CreateAggregation(m, state, nameGenerator, childSelect, dbCountFunc);
        }

        private IDbObject BuildCondition(MethodCallExpression m, TranslationState state)
        {
            var countOne = _dbFactory.BuildConstant(1);
            if (!m.GetArguments().Any())
                return countOne;

            var dbBinary = (IDbBinary)state.ResultStack.Pop();

            var tuple = Tuple.Create<IDbBinary, IDbObject>(dbBinary, countOne);
            return _dbFactory.BuildCondition(new [] { tuple }, _dbFactory.BuildConstant(null));
        }
    }
}