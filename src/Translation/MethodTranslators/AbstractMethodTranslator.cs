using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Translation.DbObjects;

namespace Translation.MethodTranslators
{
    public abstract class AbstractMethodTranslator
    {
        protected readonly IModelInfoProvider _infoProvider;

        protected readonly IDbObjectFactory _dbFactory;

        public AbstractMethodTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory)
        {
            _infoProvider = infoProvider;
            _dbFactory = dbFactory;
        }

        public abstract void Register(TranslationPlugIns plugIns);

        public abstract void Translate(
            MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator);
    }

    public class WhereMethodTranslator : AbstractMethodTranslator
    {
        public WhereMethodTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory) 
            : base(infoProvider, dbFactory)
        {
        }

        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("where", this);
        }

        public override void Translate(MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            var whereClause = (IDbBinary)state.ResultStack.Pop();
            var dbSelect = (IDbSelect)state.ResultStack.Peek();
            
            dbSelect.Where = dbSelect.Where != null 
                ? _dbFactory.BuildBinary(dbSelect.Where, DbOperator.And, whereClause)
                : whereClause;
        }   
    }

    public class AnyMethodTranslator : AbstractMethodTranslator
    {
        public AnyMethodTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory) 
            : base(infoProvider, dbFactory)
        {
        }

        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("any", this);
        }

        public override void Translate(
            MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            var condition = state.ResultStack.Pop();
            var childSelect = (IDbSelect)state.ResultStack.Pop();
            childSelect.Where = condition;

            var dbSelect = (IDbSelect)state.ResultStack.Peek();
            var dbJoin = dbSelect.Joins.Single(j => j.To.Referee == childSelect);

            IDbBinary whereClause = null;
            foreach(var joinKey in dbJoin.GetChildren<IDbColumn>(c => c.Ref == dbJoin.To))
            {
                var pkColumn = _dbFactory.BuildColumn(dbJoin.To, joinKey.Name, joinKey.ValType.DotNetType, joinKey.Alias);
                var binary = _dbFactory.BuildBinary(pkColumn, DbOperator.NotEqual, _dbFactory.BuildConstant(null));
                whereClause = whereClause != null 
                    ? _dbFactory.BuildBinary(whereClause, DbOperator.And, binary)
                    : binary;
            }

            state.ResultStack.Push(whereClause);
        }
    }
}