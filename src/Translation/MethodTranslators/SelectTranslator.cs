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
                newSelectRef.Alias = nameGenerator.GetAlias(dbSelect, SqlTranslationHelper.SubSelectPrefix, true);
            }
            
            // put selections onto the select
            var selections = SqlTranslationHelper.ProcessSelection(arguments, _dbFactory);
            foreach(var selectable in selections)
            {
                SqlTranslationHelper.UpdateJoinType(selectable.Ref);

                var newSelection = SqlTranslationHelper.GetOrCreateSelectable(selectable, newSelectRef, _dbFactory);
                dbSelect.AddSelection(newSelection, _dbFactory);
            }

            state.ResultStack.Push(dbSelect);
        }
    }
}