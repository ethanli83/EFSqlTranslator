using System.Linq.Expressions;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation.MethodTranslators
{
    public class StringStartsEndsTranslator : AbstractMethodTranslator
    {
        public StringStartsEndsTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory)
            : base(infoProvider, dbFactory)
        {
        }

        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("startswith", this);
            plugIns.RegisterMethodTranslator("endswith", this);
        }

        public override void Translate(MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            var dbContants = (IDbConstant)state.ResultStack.Pop();
            dbContants.Val = m.Method.Name == "StartsWith"
                    ? $"{dbContants.Val}%"
                    : $"%{dbContants.Val}";

            var dbExpression = (IDbSelectable)state.ResultStack.Pop();
            var dbBinary = _dbFactory.BuildBinary(dbExpression, DbOperator.Like, dbContants);

            state.ResultStack.Push(dbBinary);
        }
    }
}
