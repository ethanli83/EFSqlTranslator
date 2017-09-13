using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation.MethodTranslators
{
    public class InTranslator : AbstractMethodTranslator
    {
        public InTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory)
            : base(infoProvider, dbFactory)
        {
        }

        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("in", this);
        }

        public override void Translate(
            MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            var vals = new List<object>();
            while (state.ResultStack.Peek() is IDbConstant)
            {
                var dbConstants = (IDbConstant)state.ResultStack.Pop();
                var val = dbConstants.Val as IEnumerable;
                if (val != null)
                {
                    vals = val.Cast<object>().ToList();
                    break;
                }
                vals.Insert(0, dbConstants.Val);
            }


            IDbBinary dbBinary;
            var dbExpression = (IDbSelectable)state.ResultStack.Pop();
            if (vals.Count == 0)
            {
                dbBinary = _dbFactory.BuildBinary(_dbFactory.BuildConstant(0), DbOperator.Equal, _dbFactory.BuildConstant(1));
            }
            else
            {
                dbBinary = _dbFactory.BuildBinary(dbExpression, DbOperator.In, _dbFactory.BuildConstant(vals));
            }

            state.ResultStack.Push(dbBinary);
        }
    }
}