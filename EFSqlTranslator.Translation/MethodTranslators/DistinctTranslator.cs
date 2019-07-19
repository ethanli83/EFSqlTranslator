using System;
using System.Linq;
using System.Linq.Expressions;
using EFSqlTranslator.Translation.DbObjects;
using EFSqlTranslator.Translation.Extensions;

namespace EFSqlTranslator.Translation.MethodTranslators
{
    public class DistinctTranslator : AbstractMethodTranslator
    {
        public DistinctTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory)
            : base(infoProvider, dbFactory)
        {
        }
        
        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("distinct", this);
        }

        public override void Translate(
            MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            var dbSelect = state.ResultStack.Peek() as IDbSelect;
            if (dbSelect != null) {
                dbSelect.Selection.IsDistinct = true;
            }
            
            var caller = m.GetCaller();
            var entityInfo = caller.NodeType == ExpressionType.MemberAccess 
                ? this._infoProvider.FindEntityInfo(caller.Type)
                : null;

            // if the caller of the Distinct function is a Entity
            // We need to add its primary keys into the query so that the result
            // of distinct is correct. Otherwise, it will only distinct on join keys
            if (entityInfo != null) 
            {
                foreach (var pk in entityInfo.Keys)
                {
                    var pkColumn = _dbFactory.BuildColumn(dbSelect.GetReturnEntityRef(), pk.DbName, pk.ValType);
                    dbSelect.Selection.Add(pkColumn);
                }
            }
        }
    }
}