using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation.MethodTranslators
{
    public class CountTranslator : AbstractMethodTranslator
    {
        public CountTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory) 
            : base(infoProvider, dbFactory)
        {
        }
        
        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("count", this);
        }

        public override void Translate(
            MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            var predicate = BuildCondition(m, state);

            IDbSelect childSelect = null;
            if (m.GetCaller().Type.IsGrouping())
                state.ResultStack.Pop();
            else
                childSelect = state.ResultStack.Pop() as IDbSelect;

            var dbCountFunc = _dbFactory.BuildFunc("count", true, predicate);
            var dbSelect = (IDbSelect)state.ResultStack.Peek();

            if (childSelect == null)
            {
                state.ResultStack.Push(dbCountFunc);
            }
            else
            {
                var alias = nameGenerator.GenerateAlias(dbSelect, "count", true);
                dbCountFunc.Alias = alias;
                childSelect.Selection.Add(dbCountFunc);

                var cRef = dbSelect.Joins.Single(j => ReferenceEquals(j.To.Referee, childSelect)).To;
                var column = _dbFactory.BuildColumn(cRef, alias, m.Method.ReturnType);

                var dbDefaultVal = _dbFactory.BuildConstant(Activator.CreateInstance(m.Method.ReturnType));
                var dbIsNullFunc = _dbFactory.BuildNullCheckFunc(column, dbDefaultVal);

                state.ResultStack.Push(dbIsNullFunc);
            }
        }

        private IDbObject BuildCondition(MethodCallExpression m, TranslationState state)
        {
            var countOne = _dbFactory.BuildConstant(1);
            if (!m.GetArguments().Any())
                return countOne;

            var dbBinary = (IDbBinary)state.ResultStack.Pop();

            var tuple = Tuple.Create<IDbBinary, IDbObject>(dbBinary, countOne);
            return _dbFactory.BuildCondition(new [] { tuple }, _dbFactory.BuildConstant(null));
        }
    }
}