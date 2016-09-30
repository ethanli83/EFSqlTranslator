using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Translation.DbObjects;
using Translation.MethodTranslators;

namespace Translation
{
    public class LinqTranslator : ExpressionVisitor
    {
        private readonly IModelInfoProvider _infoProvider;
        
        private readonly IDbObjectFactory _dbFactory;

        private readonly TranslationState _state;

        private readonly UniqueNameGenerator _nameGenerator = new UniqueNameGenerator();

        private readonly TranslationPlugIns _plugIns = new TranslationPlugIns();

        public LinqTranslator(
            IModelInfoProvider infoProvider, IDbObjectFactory dbFactory, 
            TranslationState state = null, IEnumerable<AbstractMethodTranslator> methodTranslators = null)
        {
            _infoProvider = infoProvider;
            _dbFactory = dbFactory;
            _state = state ?? new TranslationState();

            RegisterDefaultPlugIns();

            if (methodTranslators != null)
            {
                foreach(var translator in methodTranslators)
                    translator.Register(_plugIns);
            }
        }

        private void RegisterDefaultPlugIns()
        {
            new WhereMethodTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new AnyMethodTranslator(_infoProvider, _dbFactory).Register(_plugIns);
        }

        public static IDbScript Translate(Expression exp, IModelInfoProvider infoProvider, IDbObjectFactory dbFactory)
        {
            var translator = new LinqTranslator(infoProvider, dbFactory);
            translator.Visit(exp);
            return translator.GetResult();
        }

        internal IDbScript GetResult()
        {
            var dbSelect = _state.ResultStack.Pop();
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
                dbRef.Alias = _nameGenerator.GetAlias(dbSelect, dbTable.TableName);
                
                _state.ResultStack.Push(dbSelect);
            }
            else
            {
                var dbConstant = _dbFactory.BuildConstant(c.Value);
                _state.ResultStack.Push(dbConstant);
            }

            return c;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            var dbObject = _state.ResultStack.Pop();
            
            var dbSelect = dbObject as IDbSelect;
            if (dbSelect != null)
            {
                _state.ResultStack.Push(dbObject);
                _state.ResultStack.Push(dbSelect.Targets.First());
                return p;
            }
            
            var dbRefs = (IDbList<DbReference>)dbObject;
            var dbRef = dbRefs.First(r => r.Alias == p.Name);

            _state.ResultStack.Push(dbRef);
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

                var fromRef = (DbReference)_state.ResultStack.Pop();
                var fromEntity = _infoProvider.FindEntityInfo(e.Type);
                var relation = fromEntity.GetRelation(m.Member.Name);

                var dbJoin = GetOrCreateJoin(relation, fromRef);
                if(relation.IsChildRelation)
                    _state.ResultStack.Push(dbJoin.To.Referee);
                else
                    _state.ResultStack.Push(dbJoin.To);

                return m;                     
            }

            if (typeInfo.Namespace.StartsWith("System"))
            {
                // build a column expression for
                Visit(m.Expression);

                var dbRef = (DbReference)_state.ResultStack.Pop();
                var col = _dbFactory.BuildColumn(dbRef, m.Member.Name, m.Type);
                
                _state.ResultStack.Push(col);
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
            var dbSelect = (IDbSelect)_state.ResultStack.Peek();
            var tupleKey = Tuple.Create(dbSelect, relation);

            if (!_state.CreatedJoins.ContainsKey(tupleKey))
            {
                var toEntity = relation.ToEntity;
                var dbTable = _dbFactory.BuildTable(toEntity);

                DbReference joinTo;
                DbReference childRef = null;
                IDbSelect childSelect = null;
                if (!relation.IsChildRelation)
                {
                    var tableAlias = _nameGenerator.GetAlias(dbSelect, dbTable.TableName);
                    joinTo = _dbFactory.BuildRef(dbTable, tableAlias);
                }
                else
                {
                    childRef = _dbFactory.BuildRef(dbTable);
                    childSelect = _dbFactory.BuildSelect(childRef);
                    childRef.Alias = _nameGenerator.GetAlias(childSelect, dbTable.TableName);

                    var tableAlias = _nameGenerator.GetAlias(dbSelect, "x");
                    joinTo = _dbFactory.BuildRef(childSelect, tableAlias);
                }

                // build join condition
                var dbJoin = _dbFactory.BuildJoin(joinTo);

                IDbBinary condition = null;
                for (var i = 0; i < relation.FromKeys.Count; i++)
                {
                    var fromKey = relation.FromKeys[i];
                    var toKey = relation.ToKeys[i];

                    var fromColumn = _dbFactory.BuildColumn(fromRef, fromKey.Name, fromKey.ValType);
                    var toColumn = _dbFactory.BuildColumn(joinTo, toKey.Name, toKey.ValType);

                    dbJoin.FromKeys.Add(fromColumn);
                    dbJoin.ToKeys.Add(toColumn);

                    if (childRef != null && childSelect != null)
                    {
                        var childColumn = _dbFactory.BuildColumn(childRef, toKey.Name, toKey.ValType);
                        childSelect.Selection.Add(childColumn);
                        childSelect.GroupBys.Add(childColumn);
                    }

                    var binary = _dbFactory.BuildBinary(fromColumn, DbOperator.Equal, toColumn);
                    condition = condition != null 
                        ? _dbFactory.BuildBinary(condition, DbOperator.And, binary)
                        : binary;
                }

                dbJoin.Condition = condition;
                dbJoin.Type = !relation.IsChildRelation ? JoinType.Inner : JoinType.LeftOuter;

                dbSelect.Joins.Add(dbJoin);
                _state.CreatedJoins[tupleKey] = dbJoin;
            }
            
            return _state.CreatedJoins[tupleKey];
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            var caller = m.Object ?? m.Arguments.First();
            var args = m.Arguments.Where(a => a != caller).ToArray(); 

            Visit(caller);
            VistMethodArguments(args);

            if (!_plugIns.TranslateMethodCall(m, _state, _nameGenerator))
                throw new NotSupportedException();

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
                var dbRef = (DbReference)_state.ResultStack.Pop();
                // todo: make a hash set of referred name istead of using alias name 
                // as a ref can be referred by different parameter, the alias name may be overriden
                dbRef.Alias = p.Name; 
                pList.Add(dbRef);
            }
            _state.ResultStack.Push(pList);

            Visit(l.Body);
            return l;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            Visit(b.Left);
            var left = _state.ResultStack.Pop();

            Visit(b.Right);
            var right = _state.ResultStack.Pop();

            var dbOptr = SqlTranslationHelper.GetDbOperator(b.NodeType);
            var dbBinary = _dbFactory.BuildBinary(left, dbOptr, right);

            _state.ResultStack.Push(dbBinary);
            return b;
        }
    }

    public class TranslationState
    {
        private readonly Stack<IDbObject> _resultStack = new Stack<IDbObject>();
        
        private readonly Dictionary<Tuple<IDbSelect, EntityRelation>, IDbJoin> _createdJoins = 
            new Dictionary<Tuple<IDbSelect, EntityRelation>, IDbJoin>();

        public Stack<IDbObject> ResultStack => _resultStack;

        public Dictionary<Tuple<IDbSelect, EntityRelation>, IDbJoin> CreatedJoins => _createdJoins;
    }

    public class UniqueNameGenerator
    {
        private readonly Dictionary<IDbSelect, Dictionary<string, int>> _uniqueAliasNames = 
            new Dictionary<IDbSelect, Dictionary<string, int>>();

        public string GetAlias(IDbSelect dbSelect, string tableName)
        {
            var uniqueNames = _uniqueAliasNames.ContainsKey(dbSelect)
                ? _uniqueAliasNames[dbSelect]
                : _uniqueAliasNames[dbSelect] = new Dictionary<string, int>();

            var count = uniqueNames.ContainsKey(tableName)
                ? ++uniqueNames[tableName]
                : uniqueNames[tableName] = 0;
            
            return $"{tableName.Substring(0, 1).ToLower()}{count}";
        }
    }

    public class TranslationPlugIns
    {
        private Dictionary<string, AbstractMethodTranslator> _methodTranslators = 
            new Dictionary<string, AbstractMethodTranslator>(StringComparer.CurrentCultureIgnoreCase); 

        public void RegisterMethodTranslator(string methodName, AbstractMethodTranslator translator)
        {
            _methodTranslators[methodName] = translator;
        }

        internal bool TranslateMethodCall(
            MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            var method = m.Method;
            if (!_methodTranslators.ContainsKey(method.Name))
                return false;

            var translator = _methodTranslators[method.Name];
            translator.Translate(m, state, nameGenerator);
            return true;
        }
    }
}