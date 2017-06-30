using System.Linq.Expressions;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation.MethodTranslators
{
    public class WhereTranslator : AbstractMethodTranslator
    {
        public WhereTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory) 
            : base(infoProvider, dbFactory)
        {
        }

        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("where", this);
        }

        public override void Translate(MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            var dbElement = state.ResultStack.Pop();
            var dbBinary = dbElement as IDbBinary;

            IDbBinary whereClause;
            if (dbBinary != null)
            {
                whereClause = dbBinary;
            }
            else
            {
                var one = _dbFactory.BuildConstant(true);
                whereClause = _dbFactory.BuildBinary(dbElement, DbOperator.Equal, one);
            }
                
            var dbSelect = (IDbSelect)state.ResultStack.Peek();
            dbSelect.UpdateWhereClause(whereClause, _dbFactory);
        }   
    }
}