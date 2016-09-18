using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation
{
    public class EFLinqTranslator : ExpressionVisitor
    {
        private readonly Stack<IDbObject> _resultStack = new Stack<IDbObject>();
        private readonly ModelInfoProvider _infoProvider;
        private readonly IDbObjectFactory _dbFactory;

        public EFLinqTranslator(ModelInfoProvider infoProvider, IDbObjectFactory dbFactory)
        {
            _infoProvider = infoProvider;
            _dbFactory = dbFactory;
        }

        public static IDbScript Translate(Expression exp, ModelInfoProvider infoProvider, IDbObjectFactory dbFactory)
        {
            var translator = new EFLinqTranslator(infoProvider, dbFactory);
            translator.Visit(exp);
            return translator.GetResult();
        }

        internal IDbScript GetResult()
        {
            var dbSelect = _resultStack.Pop();
            var script = _dbFactory.BuildScript();
            script.Scripts.Add(dbSelect);
            return script;
        }

        public override Expression Visit(Expression e)
        {
            return base.Visit(e);
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            var entityInfo = _infoProvider.FindEntityInfo(c.Type);
            if (entityInfo != null)
            {
                var dbTable = _dbFactory.BuildTable(entityInfo);
                var dbSelect = _dbFactory.BuildSelect(dbTable);
                _resultStack.Push(dbSelect);
            }
            else
            {
                var dbConstant = _dbFactory.BuildConstant(c.Value);
                _resultStack.Push(dbConstant);
            }

            return c;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            var dbObject = _resultStack.Pop();
            
            var dbSelect = dbObject as IDbSelect;
            if (dbSelect != null)
            {
                _resultStack.Push(dbObject);
                _resultStack.Push(dbSelect.Targets.First());
                return p;
            }
            
            var dbRefs = (IDbList<DbReference>)dbObject;
            var dbRef = dbRefs.First(r => r.Alias == p.Name);

            _resultStack.Push(dbRef);
            return p;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            var typeInfo = m.Type.GetTypeInfo();
            if (typeInfo.Namespace.StartsWith("System"))
            {
                // build a column expression for
                Visit(m.Expression);

                var dbRef = (DbReference)_resultStack.Pop();
                var col = _dbFactory.BuildColumn(dbRef, m.Member.Name, m.Type);
                
                _resultStack.Push(col);
                return m;
            }
            
            var entityInfo = _infoProvider.FindEntityInfo(m.Type);
            if (entityInfo != null)
            {
                    
            }

            return base.VisitMember(m);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            var caller = m.Object ?? m.Arguments.First();
            var args = m.Arguments.Where(a => a != caller).ToArray(); 

            Visit(caller);
            VistMethodArguments(args);

            switch(m.Method.Name.ToLower())
            {
                case "where":
                    var whereClause = _resultStack.Pop();
                    var dbSelect = (IDbSelect)_resultStack.Peek();
                    dbSelect.Where = whereClause;
                    break;
            }

            return m;
        }

        private void VistMethodArguments(Expression[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            foreach(var argExpr in args)
                Visit(argExpr);
        }

        protected override Expression VisitLambda<T>(Expression<T> l)
        {
            var pList = _dbFactory.BuildList<DbReference>();
            // the order of the parameters need to be reversed, as the first
            // query that be translated will be at the bottom of the stack, and the query
            // for the last parameter will be at the top 
            foreach(var p in l.Parameters.Reverse())
            {
                Visit(p);
                var dbRef = (DbReference)_resultStack.Pop();
                // todo: make a hash set of referred name istead of using alias name 
                // as a ref can be referred by different parameter, the alias name may be overriden
                dbRef.Alias = p.Name; 
                pList.Add(dbRef);
            }
            _resultStack.Push(pList);

            Visit(l.Body);
            return l;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            Visit(b.Left);
            var left = _resultStack.Pop();

            Visit(b.Right);
            var right = _resultStack.Pop();

            var dbOptr = SqlTranslationHelper.GetDbOperator(b.NodeType);
            var dbBinary = _dbFactory.BuildBinary(left, dbOptr, right);

            _resultStack.Push(dbBinary);
            return b;
        }
    }
}