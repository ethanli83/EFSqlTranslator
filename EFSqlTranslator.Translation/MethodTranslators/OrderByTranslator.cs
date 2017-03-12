using System.Linq.Expressions;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation.MethodTranslators
{
    public class OrderByTranslator : AbstractMethodTranslator
    {
        public OrderByTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory) 
            : base(infoProvider, dbFactory)
        {
        }

        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("OrderBy", this);
            plugIns.RegisterMethodTranslator("OrderByDescending", this);
            plugIns.RegisterMethodTranslator("ThenBy", this);
            plugIns.RegisterMethodTranslator("ThenByDescending", this);
        }

        public override void Translate(MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            // group by can be a column, a expression, or a list of columns / expressions
            var arguments = state.ResultStack.Pop();
            var dbSelect = (IDbSelect)state.ResultStack.Pop();

            var direction = m.Method.Name.EndsWith("Descending") ? DbOrderDirection.Desc : DbOrderDirection.Asc;

            var selections = SqlTranslationHelper.ProcessSelection(arguments, _dbFactory);
            foreach(var selectable in selections)
            {
                SqlTranslationHelper.UpdateJoinType(selectable.Ref);
                var orderByCol = _dbFactory.BuildOrderByColumn(selectable, direction);
                dbSelect.OrderBys.Add(orderByCol);
            }

            state.ResultStack.Push(dbSelect);
        }
    }
}