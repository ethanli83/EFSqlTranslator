using System.Linq;
using System.Linq.Expressions;
using Translation.DbObjects;

namespace Translation.MethodTranslators
{
    public class GroupByTranslator : AbstractMethodTranslator
    {
        public GroupByTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory) 
            : base(infoProvider, dbFactory)
        {
        }

        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("groupby", this);
        }


        public override void Translate(
            MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            // group by can be a column, a expression, or a list of columns / expressions
            var arguments = state.ResultStack.Pop();
            var dbSelect = (IDbSelect)state.ResultStack.Pop();
            
            var groupBys = dbSelect.GroupBys;
            groupBys.IsSingleKey = !(arguments is IDbList<DbKeyValue>);

            var selections = SqlTranslationHelper.ProcessSelection(arguments, _dbFactory);
            foreach(var selectable in selections)
            {
                SqlTranslationHelper.UpdateJoinType(selectable.Ref);
                SqlTranslationHelper.UpdateOnSelection(selectable, onGrouping: true);

                var refCol = selectable as IDbRefColumn;
                // if we group on a ref column, we are actaully group by on the primary key
                // of the entity that ref column referring to. In the refering entity is actually
                // another ref column, then we will need to get the primay key recursive from RefTo
                if (refCol != null && refCol.RefTo != null)
                {
                    foreach(var pk in refCol.GetPrimaryKeys())
                        refCol.RefTo.AddColumnToReferedSubSelect(pk.Name, pk.ValType, _dbFactory);
                }
                
                dbSelect.GroupBys.Add(selectable);
            }

            state.ResultStack.Push(dbSelect);
        }
    }
}