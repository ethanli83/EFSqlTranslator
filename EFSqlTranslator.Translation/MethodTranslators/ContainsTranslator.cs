using System;
using System.Linq.Expressions;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation.MethodTranslators
{
    class ContainsTranslator : AbstractMethodTranslator
    {
        public ContainsTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory)
            : base(infoProvider, dbFactory)
        {
        }

        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("contains", this);
        }

        public override void Translate(MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            MemberExpression member = (m.Object ?? m.Arguments[0]) as MemberExpression;
            IDbObject dbBinary = null;

            if (member.Type == typeof(string))
            {
                var dbConstants = (IDbConstant)state.ResultStack.Pop();
                dbConstants.Val = $"%{dbConstants.Val}%";

                var dbExpression = (IDbSelectable)state.ResultStack.Pop();
                dbBinary = _dbFactory.BuildBinary(dbExpression, DbOperator.Like, dbConstants);
            }
            else if (member.Type.IsArray)
            {
                var dbConstants = (IDbConstant)state.ResultStack.Pop();

                var dbExpression = (IDbSelectable)state.ResultStack.Pop();
                var dbFunc = _dbFactory.BuildFunc(SqlTranslationHelper.GetSqlOperator(DbOperator.Any), false, dbExpression);
                dbBinary = _dbFactory.BuildBinary(dbConstants, DbOperator.Equal, dbFunc);
            }
            else
            {
                throw new NotSupportedException($"Type {member.Type} is not supported");
            }

            state.ResultStack.Push(dbBinary);
        }
    }
}