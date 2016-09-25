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
        
        private readonly Dictionary<Tuple<IDbSelect, EntityRelation>, IDbJoin> _createdJoins = 
            new Dictionary<Tuple<IDbSelect, EntityRelation>, IDbJoin>();

        private readonly Dictionary<IDbSelect, Dictionary<string, int>> _uniqueAliasNames = 
            new Dictionary<IDbSelect, Dictionary<string, int>>();
        
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
                var dbRef = _dbFactory.BuildRef(dbTable);

                var dbSelect = _dbFactory.BuildSelect(dbRef);
                dbRef.Alias = GetUniqueAliasName(dbSelect, dbTable.TableName);
                
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

            // if the member is a queryable entity, we need to translate it
            // into a relation, which means a join 
            var entityInfo = _infoProvider.FindEntityInfo(m.Type);
            if (entityInfo != null)
            {
                var e = m.Expression;
                Visit(e);

                var fromRef = (DbReference)_resultStack.Pop();
                var fromEntity = _infoProvider.FindEntityInfo(e.Type);
                var relation = fromEntity.GetRelation(m.Member.Name);

                var dbJoin = GetOrCreateJoin(relation, fromRef);
                if(relation.IsChildRelation)
                    _resultStack.Push(dbJoin.To.Referee);
                else
                    _resultStack.Push(dbJoin.To);

                return m;                     
            }

            if (typeInfo.Namespace.StartsWith("System"))
            {
                // build a column expression for
                Visit(m.Expression);

                var dbRef = (DbReference)_resultStack.Pop();
                var col = _dbFactory.BuildColumn(dbRef, m.Member.Name, m.Type);
                
                _resultStack.Push(col);
                return m;
            }

            return base.VisitMember(m);
        }

        private bool IsQueryOnChildren(Type type, TypeInfo typeInfo, MemberExpression m)
        {
            return typeInfo.IsGenericType && (
                typeof(IEnumerable<>).MakeGenericType(type).IsAssignableFrom(m.Type) ||
                typeof(IQueryable<>).MakeGenericType(type).IsAssignableFrom(m.Type));
        }

        private IDbJoin GetOrCreateJoin(EntityRelation relation, DbReference fromRef)
        {
            var dbSelect = (IDbSelect)_resultStack.Peek();
            var tupleKey = Tuple.Create(dbSelect, relation);

            if (!_createdJoins.ContainsKey(tupleKey))
            {
                var toEntity = relation.ToEntity;
                var dbTable = _dbFactory.BuildTable(toEntity);

                DbReference joinTo;
                if (!relation.IsChildRelation)
                {
                    var tableAlias = GetUniqueAliasName(dbSelect, dbTable.TableName);
                    joinTo = _dbFactory.BuildRef(dbTable, tableAlias);
                }
                else
                {
                    var dbRef = _dbFactory.BuildRef(dbTable);
                    var childSelect = _dbFactory.BuildSelect(dbRef);
                    dbRef.Alias = GetUniqueAliasName(childSelect, dbTable.TableName);

                    var tableAlias = GetUniqueAliasName(dbSelect, "x");
                    joinTo = _dbFactory.BuildRef(childSelect, tableAlias);
                }

                // build join condition
                IDbBinary condition = null;
                for (var i = 0; i < relation.FromKeys.Count; i++)
                {
                    var fromKey = relation.FromKeys[i];
                    var toKey = relation.ToKeys[i];

                    var fromColumn = _dbFactory.BuildColumn(fromRef, fromKey.Name, fromKey.ValType);
                    var toColumn = _dbFactory.BuildColumn(joinTo, toKey.Name, toKey.ValType);

                    var binary = _dbFactory.BuildBinary(fromColumn, DbOperator.Equal, toColumn);
                    condition = condition != null 
                        ? _dbFactory.BuildBinary(condition, DbOperator.And, binary)
                        : binary;
                }

                var dbJoin = _dbFactory.BuildJoin(
                    joinTo, condition, !relation.IsChildRelation ? JoinType.Inner : JoinType.LeftOuter);

                dbSelect.Joins.Add(dbJoin);
                _createdJoins[tupleKey] = dbJoin;
            }
            
            return _createdJoins[tupleKey];
        }

        private string GetUniqueAliasName(IDbSelect dbSelect, string tableName)
        {
            var uniqueNames = _uniqueAliasNames.ContainsKey(dbSelect)
                ? _uniqueAliasNames[dbSelect]
                : _uniqueAliasNames[dbSelect] = new Dictionary<string, int>();

            var count = uniqueNames.ContainsKey(tableName)
                ? ++uniqueNames[tableName]
                : uniqueNames[tableName] = 0;
            
            return $"{tableName.Substring(0, 1).ToLower()}{count}";
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            var caller = m.Object ?? m.Arguments.First();
            var args = m.Arguments.Where(a => a != caller).ToArray(); 

            Visit(caller);
            VistMethodArguments(args);

            switch(m.Method.Name)
            {
                case "Where":
                    TranslateWhereMethod();
                    break;

                case "Any":
                    TranslateAnyMethod(caller);
                    break;
            }

            return m;
        }

        private void TranslateWhereMethod()
        {
            var whereClause = (IDbBinary)_resultStack.Pop();
            var dbSelect = (IDbSelect)_resultStack.Peek();
            
            dbSelect.Where = dbSelect.Where != null 
                ? _dbFactory.BuildBinary(dbSelect.Where, DbOperator.And, whereClause)
                : whereClause;
        }

        private void TranslateAnyMethod(Expression caller)
        {
            var condition = _resultStack.Pop();
            var childSelect = (IDbSelect)_resultStack.Pop();
            childSelect.Where = condition;

            var dbSelect = (IDbSelect)_resultStack.Peek();
            var dbJoin = dbSelect.Joins.Single(j => j.To.Referee == childSelect);

            var childEntity = _infoProvider.FindEntityInfo(caller.Type);
            IDbBinary whereClause = null;
            foreach(var pk in childEntity.Keys)
            {
                var pkColumn = _dbFactory.BuildColumn(dbJoin.To, pk.Name, pk.ValType);
                var binary = _dbFactory.BuildBinary(pkColumn, DbOperator.NotEqual, _dbFactory.BuildConstant(null));
                whereClause = whereClause != null 
                    ? _dbFactory.BuildBinary(whereClause, DbOperator.And, binary)
                    : binary;
            }

            _resultStack.Push(whereClause);
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