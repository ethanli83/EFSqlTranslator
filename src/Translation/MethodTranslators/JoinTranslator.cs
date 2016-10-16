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

            var toSelectRef = _dbFactory.BuildRef(toSelect, nameGenerator.GetAlias(fromSelect, "sq", true));

            // create result selection for final select
            UpdateSelection(fromSelect, selection, toSelectRef);

            // create join to inner select
            foreach(var joinKey in joinCondition.GetChildren<IDbColumn>(c => c.Ref.OwnerSelect == toSelect))
            {
                var alias = nameGenerator.GetAlias(toSelect, joinKey.Name + "_jk", true);
                var innerCol = _dbFactory.BuildColumn(joinKey);
                innerCol.Alias = alias;
                toSelect.AddSelection(innerCol, _dbFactory);
                
                joinKey.Ref = toSelectRef;
                joinKey.Name = alias;
                joinKey.Alias = string.Empty;
            }

            var dbJoin = _dbFactory.BuildJoin(toSelectRef, joinCondition, (JoinType)joinType.Val);
            fromSelect.Joins.Add(dbJoin);

            var finalSelectRef = _dbFactory.BuildRef(fromSelect, nameGenerator.GetAlias(null, "sq", true));
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
            if (keyValue != null)
            {
                var selectable = GetSelectable(fromSelect, keyValue.Value, toSelectRef);
                selectable.Alias = keyValue.Key;
                fromSelect.AddSelection(selectable, _dbFactory);
            }
            else
            {
                var selectable = GetSelectable(fromSelect, selection, toSelectRef);
                fromSelect.AddSelection(selectable, _dbFactory);
            }
        }

        private IDbSelectable GetSelectable(IDbSelect fromSelect, IDbObject selection, DbReference toSelectRef)
        {
            var dbRef = selection as DbReference;
            if (dbRef != null)
            {
                var refColumn = _dbFactory.BuildRefColumn(dbRef);

                if (dbRef.OwnerSelect != fromSelect)
                {
                    var toSelect = (IDbSelect)toSelectRef.Referee;
                    var toRefCol = _dbFactory.BuildRefColumn(dbRef);
                    toSelect.AddSelection(toRefCol, _dbFactory);
                    
                    refColumn.RefTo = toRefCol;
                    dbRef = toSelectRef;
                }

                return refColumn;   
            }

            var column = selection as IDbColumn;
            if (column != null)
            {
                if (column.Ref.OwnerSelect != fromSelect)
                {
                    var toSelect = (IDbSelect)toSelectRef.Referee;
                    toSelect.AddSelection(column, _dbFactory);
                    
                    column = _dbFactory.BuildColumn(column);
                    column.Ref = toSelectRef;
                }

                return column;
            }

            throw new NotSupportedException();
        }
    }
}