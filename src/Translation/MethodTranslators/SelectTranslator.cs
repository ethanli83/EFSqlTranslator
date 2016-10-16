using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
            // group by can be a column, a expression, or a list of columns / expressions
            var arguments = state.ResultStack.Pop();
            var dbSelect = (IDbSelect)state.ResultStack.Pop();

            // if the selection of the select is not empty
            // always wrap it inside another select which will be used
            // for the recent of translation
            DbReference newSelectRef = null;
            if (dbSelect.Selection.Any())
            {
                newSelectRef = _dbFactory.BuildRef(dbSelect);
                dbSelect = _dbFactory.BuildSelect(newSelectRef);
                newSelectRef.Alias = nameGenerator.GetAlias(dbSelect, "sq", true);
            }
            
            // put selections onto the select
            var selections = SqlTranslationHelper.ProcessSelection(arguments, _dbFactory);
            foreach(var selectable in selections)
            {
                SqlTranslationHelper.UpdateJoinType(selectable.Ref);

                var newSelection = GetOrCreateSelectable(selectable, newSelectRef);
                dbSelect.AddSelection(newSelection, _dbFactory);
            }

            state.ResultStack.Push(dbSelect);
        }

        private IDbSelectable GetOrCreateSelectable(IDbSelectable selectable, DbReference dbRef)
        {
            if (dbRef == null)
                return selectable;

            IDbSelectable newSelectable = null;
            if (selectable is IDbColumn)
            {
                var oCol = (IDbColumn)selectable;
                newSelectable = _dbFactory.BuildColumn(oCol);
                newSelectable.Ref = dbRef;
            }
            else if (selectable is IDbRefColumn)
            {
                var oRefCol = (IDbRefColumn)selectable;
                var oSelect = oRefCol.Ref.OwnerSelect;
                if (!oSelect.Selection.Contains(oRefCol))
                    oSelect.AddSelection(oRefCol, _dbFactory);
                
                var nRefCol = _dbFactory.BuildRefColumn(dbRef, oRefCol.Alias);
                nRefCol.RefTo = oRefCol;

                newSelectable = nRefCol;
            }
            else if (selectable is DbReference)
            {
                var oRef = (DbReference)selectable;
                var nRefCol = _dbFactory.BuildRefColumn(dbRef, oRef.Alias);
                newSelectable = nRefCol;
            }
            
            if (newSelectable == null)
                throw new InvalidOperationException();

            return newSelectable;
        }
    }
}