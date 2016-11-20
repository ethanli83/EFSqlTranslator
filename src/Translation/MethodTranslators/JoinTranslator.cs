using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Translation.DbObjects;

namespace Translation.MethodTranslators
{
    public class JoinTranslator : AbstractMethodTranslator
    {
        public JoinTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory) 
            : base(infoProvider, dbFactory)
        {
        }

        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("join", this);
        }

        public override void Translate(
            MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            var joinType = (IDbConstant)state.ResultStack.Pop();
            var selection = state.ResultStack.Pop();
            var joinCondition = (IDbBinary)state.ResultStack.Pop();
            
            var toSelect = (IDbSelect)state.ResultStack.Pop();
            var fromSelect = (IDbSelect)state.ResultStack.Pop();

            var toSelectRef = _dbFactory.BuildRef(toSelect, nameGenerator.GenerateAlias(fromSelect, "sq", true));

            // create result selection for final select
            UpdateSelection(fromSelect, selection, toSelectRef);

            // create join to inner select
            foreach(var joinKey in joinCondition.GetChildren<IDbColumn>(c => c.Ref.OwnerSelect == toSelect))
            {
                var alias = nameGenerator.GenerateAlias(toSelect, joinKey.Name + "_jk", true);
                var innerCol = _dbFactory.BuildColumn(joinKey);
                innerCol.Alias = alias;
                toSelect.Selection.Add(innerCol);
                
                joinKey.Ref = toSelectRef;
                joinKey.Name = alias;
                joinKey.Alias = string.Empty;
            }

            var dbJoin = _dbFactory.BuildJoin(toSelectRef, fromSelect, joinCondition, (JoinType)joinType.Val);
            fromSelect.Joins.Add(dbJoin);

            var finalSelectRef = _dbFactory.BuildRef(fromSelect, nameGenerator.GenerateAlias(null, "sq", true));
            var finalSelect = _dbFactory.BuildSelect(finalSelectRef);

            state.ResultStack.Push(finalSelect);
        }

        private void UpdateSelection(IDbSelect fromSelect, IDbObject selection, DbReference toSelectRef)
        {
            var dbList = selection as IEnumerable<DbKeyValue>;
            if (dbList != null)
            {
                foreach(var dbObj in dbList)
                    UpdateSelection(fromSelect, dbObj, toSelectRef);
                return;
            }

            var keyValue = selection as DbKeyValue;
            selection = keyValue != null ? keyValue.Value : selection;             
            
            var selectable = GetSelectable(fromSelect, selection, toSelectRef);

            if (keyValue != null)
                selectable.Alias = keyValue.Key;

            fromSelect.Selection.Add(selectable);
        }

        private IDbSelectable GetSelectable(IDbSelect fromSelect, IDbObject selection, DbReference toSelectRef)
        {
            var dbRef = selection as DbReference;
            if (dbRef != null)
            {
                IDbRefColumn toRefCol = null;
                if (dbRef.OwnerSelect != fromSelect)
                {
                    var toSelect = (IDbSelect)toSelectRef.Referee;
                    toRefCol = _dbFactory.BuildRefColumn(dbRef);
                    
                    toSelect.Selection.Add(toRefCol);
                    dbRef = toSelectRef;
                }

                var refColumn = _dbFactory.BuildRefColumn(dbRef);
                refColumn.RefTo = toRefCol;
                return refColumn;   
            }

            var column = selection as IDbColumn;
            if (column != null)
            {
                if (column.Ref.OwnerSelect != fromSelect)
                {
                    var toSelect = (IDbSelect)toSelectRef.Referee;
                    toSelect.Selection.Add(column);
                    
                    column = _dbFactory.BuildColumn(column);
                    column.Ref = toSelectRef;
                }

                return column;
            }

            throw new NotSupportedException();
        }
    }
}