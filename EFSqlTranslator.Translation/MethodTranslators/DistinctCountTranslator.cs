using System;
using System.Linq.Expressions;

using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation.MethodTranslators
{
    /// <summary> Translator for DistinctCount aggregate function </summary>
    public class DistinctCountTranslator : AggregationTranslatorBase
    {
        /// <summary> ctor </summary>
        public DistinctCountTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory)
            : base(infoProvider, dbFactory)
        {
        }

        /// <inheritdoc />
        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("DistinctCount", this);
        }

        /// <summary> Translate to SQL </summary>
        public override void Translate(
            MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            var dbExpression = (IDbSelectable)state.ResultStack.Pop();

            if (dbExpression == null)
            {
                throw new NotSupportedException("Can not aggregate.");
            }

            if (!(dbExpression is IDistinctable))
            {
                throw new NotSupportedException("Expression must be Distinctable");
            }
            
            var distinctable = (IDistinctable)dbExpression;
            distinctable.IsDistinct = true;
            
            var dbFunc = _dbFactory.BuildFunc("count", true, dbExpression);

            CreateAggregation(m, state, nameGenerator, null, dbFunc);
        }
    }
}
