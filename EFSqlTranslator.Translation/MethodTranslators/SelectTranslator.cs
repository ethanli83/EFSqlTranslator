using System.Linq;
using System.Linq.Expressions;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation.MethodTranslators
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

            newSelectRef.Alias = nameGenerator.GenerateAlias(dbSelect, TranslationConstants.SubSelectPrefix, true);

            selections = selections.Concat(dbSelect.Selection.Where(s => s.IsJoinKey)).ToArray();
            foreach(var selectable in selections)
            {
                var newSelectable = CreateNewSelectableForWrappingSelect(selectable, newSelectRef, _dbFactory);

                newSelect.Selection.Add(newSelectable);
            }

            state.ResultStack.Push(newSelect);
        }

        private static IDbSelectable CreateNewSelectableForWrappingSelect(
            IDbSelectable selectable, DbReference dbRef, IDbObjectFactory dbFactory)
        {
            if (dbRef == null)
                return selectable;

            var oCol =  selectable as IDbColumn;
            if (oCol != null)
                return dbFactory.BuildColumn(dbRef, oCol.GetAliasOrName(), oCol.ValType);

            var oRefCol = selectable as IDbRefColumn;
            if (oRefCol != null)
                return dbFactory.BuildRefColumn(dbRef, oRefCol.Alias, oRefCol);

            return dbFactory.BuildColumn(selectable.Ref, selectable.Alias, typeof(string));
        }
    }
}