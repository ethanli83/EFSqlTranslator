using System.Linq;
using System.Linq.Expressions;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation.MethodTranslators
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

                var refCol = selectable as IDbRefColumn;
                // if we group on a ref column, we are actaully group by on the primary key
                // of the entity that ref column referring to. In the refering entity is actually
                // another ref column, then we will need to get the primay key recursive from RefTo
                if (refCol?.RefTo != null)
                {
                    foreach(var pk in refCol.GetPrimaryKeys())
                        refCol.RefTo.AddToReferedSelect(_dbFactory, pk.Name, pk.ValType);
                }
                
                dbSelect.GroupBys.Add(selectable);
            }

            // if the group by is a single expression like groupby(x => x.Children.Count())
            // it will be translated into a expression and will not have alias
            // in this case, we will need to give it an alias which will be used later
            if (groupBys.IsSingleKey)
            {
                var column = groupBys.Single();
                if (column.Alias == null)
                    column.Alias = "Key";
            }

            state.ResultStack.Push(dbSelect);
        }
    }
}