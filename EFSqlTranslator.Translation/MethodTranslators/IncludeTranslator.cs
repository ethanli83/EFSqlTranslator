using System;
using System.Linq;
using System.Linq.Expressions;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation.MethodTranslators
{
    public class IncludeTranslator : AbstractMethodTranslator
    {
        public IncludeTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory)
            : base(infoProvider, dbFactory)
        {
        }

        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("include", this);
        }

        public override void Translate(MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            var args = state.ResultStack.Pop();
            var dbSelect = (IDbSelect)state.ResultStack.Peek();

            var selections = SqlTranslationHelper.ProcessSelection(args, _dbFactory);
            var refColumn = (IDbRefColumn)selections.Single();

            if (!dbSelect.Selection.Any())
            {
                var oRefColumn = _dbFactory.BuildRefColumn(dbSelect.From, dbSelect.From.Alias);
                dbSelect.Selection.Add(oRefColumn);
            }

            dbSelect.Selection.Add(refColumn);

            var pk = refColumn.GetPrimaryKeys().First().GetNameOrAlias();
            state.IncludeSplits.Add(pk);
        }
    }
}