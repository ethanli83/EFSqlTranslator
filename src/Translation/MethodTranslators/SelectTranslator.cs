using System.Linq.Expressions;
using Translation.DbObjects;

namespace Translation.MethodTranslators
{
    public class SelectTranslator : AbstractMethodTranslator
    {
        public SelectTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory) 
            : base(infoProvider, dbFactory)
        {
        }

        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("select", this);
        }

        public override void Translate(
            MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            var arguments = state.ResultStack.Pop();
            var dbSelect = (IDbSelect)state.ResultStack.Pop();

            var selections = SqlTranslationHelper.ProcessSelection(arguments, _dbFactory);
            foreach(var selectable in selections)
            {
                SqlTranslationHelper.UpdateJoinType(selectable.Ref);
                dbSelect.Selection.Add(selectable);
            }

            var newSelectRef = _dbFactory.BuildRef(dbSelect);
            var newSelect = _dbFactory.BuildSelect(newSelectRef);
            newSelectRef.Alias = nameGenerator.GenerateAlias(dbSelect, SqlTranslationHelper.SubSelectPrefix, true);

            foreach(var selectable in selections)
            {
                var newSelectable = SqlTranslationHelper.CreateNewSelectable(selectable, newSelectRef, _dbFactory);
                newSelect.Selection.Add(newSelectable);
            }

            state.ResultStack.Push(newSelect);
        }
    }
}