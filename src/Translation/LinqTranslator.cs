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
            new JoinMethodTranslator(_infoProvider, _dbFactory).Register(_plugIns);
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

        protected override Expression VisitNew(NewExpression n)
        {
            var list = _dbFactory.BuildList<DbKeyValue>();
            for (var i = 0; i < n.Members.Count; i++)
            {
                var member = n.Members[i];
                var assignment = n.Arguments[i];

                Visit(assignment);

                var dbObj = _state.ResultStack.Pop();
                var dbKeyValue = _dbFactory.BuildKeyValue(member.Name, dbObj);
                list.Add(dbKeyValue);
            }

            _state.ResultStack.Push(list);
            return n;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            DbReference dbRef = null;

            if (_state.ParamterStack.Count > 0)
            {
                var dbRefs = _state.ParamterStack.Peek();
                if (dbRefs.ContainsKey(p))
                    dbRef = dbRefs[p];
            }
            
            // if we can not find the parameter expression in the ParamterStack,
            // it means this is the first time we translates the parameter, so we
            // need to look for it in the most recently translated select 
            if (dbRef == null)
            {
                var results = new Stack<IDbObject>();
                while(_state.ResultStack.Count > 0)
                {
                    var dbObject = _state.ResultStack.Pop();
                    results.Push(dbObject);
                    
                    var dbSelect = dbObject as IDbSelect;
                    if (dbSelect != null)
                    {
                        dbRef = dbSelect.Targets.First();
                        break;
                    }
                }

                while(results.Count > 0)
                    _state.ResultStack.Push(results.Pop());
            }

            if (dbRef == null)
                throw new NullReferenceException();
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

                    var tableAlias = _nameGenerator.GetAlias(dbSelect, "sq", true);
                    joinTo = _dbFactory.BuildRef(childSelect, tableAlias);
                }

                joinTo.OwnerSelect = dbSelect;

                // build join condition
                var dbJoin = _dbFactory.BuildJoin(joinTo);

                IDbBinary condition = null;
                for (var i = 0; i < relation.FromKeys.Count; i++)
                {
                    var fromKey = relation.FromKeys[i];
                    var toKey = relation.ToKeys[i];

                    var fromColumn = _dbFactory.BuildColumn(fromRef, fromKey.Name, fromKey.ValType);
                    var toColumn = _dbFactory.BuildColumn(joinTo, toKey.Name, toKey.ValType);

                    if (childRef != null && childSelect != null)
                    {
                        var alias = _nameGenerator.GetAlias(dbSelect, "JoinKey", true);
                        var childColumn = _dbFactory.BuildColumn(childRef, toKey.Name, toKey.ValType, alias);
                        childSelect.Selection.Add(childColumn);
                        childSelect.GroupBys.Add(childColumn);

                        toColumn.Name = alias;
                        toColumn.Alias = string.Empty;
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
            var pList = new Dictionary<ParameterExpression, DbReference>();
            // the order of the parameters need to be reversed, as the first
            // query that be translated will be at the bottom of the stack, and the query
            // for the last parameter will be at the top
            var results = new Stack<IDbObject>();
            foreach(var p in l.Parameters.Reverse())
            {
                Visit(p);
                var dbRef = (DbReference)_state.ResultStack.Pop();
                pList[p] = dbRef;

                // pop out used select for the parameter just translated
                // so that the next parameter will be assigned with current select
                while(_state.ResultStack.Count > 0)
                {
                    var dbObj = _state.ResultStack.Pop();
                    results.Push(dbObj);
                    if (dbObj is IDbSelect)
                        break;
                }
            }

            while(results.Count > 0)
                _state.ResultStack.Push(results.Pop());                
            
            _state.ParamterStack.Push(pList);
            Visit(l.Body);
            _state.ParamterStack.Pop();

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

        private readonly Stack<Dictionary<ParameterExpression, DbReference>> _paramterStack = 
            new Stack<Dictionary<ParameterExpression, DbReference>>();
        
        private readonly Dictionary<Tuple<IDbSelect, EntityRelation>, IDbJoin> _createdJoins = 
            new Dictionary<Tuple<IDbSelect, EntityRelation>, IDbJoin>();

        public Stack<IDbObject> ResultStack => _resultStack;

        public Stack<Dictionary<ParameterExpression, DbReference>> ParamterStack => _paramterStack;

        public Dictionary<Tuple<IDbSelect, EntityRelation>, IDbJoin> CreatedJoins => _createdJoins;
    }

    public class UniqueNameGenerator
    {
        private readonly Dictionary<IDbSelect, Dictionary<string, int>> _uniqueAliasNames = 
            new Dictionary<IDbSelect, Dictionary<string, int>>();

            private readonly Dictionary<string, int> _globalUniqueAliasNames = 
                new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);

        public string GetAlias(IDbSelect dbSelect, string name, bool fullName = false)
        {
            int count;
            if (dbSelect == null)
            {
                count = _globalUniqueAliasNames.ContainsKey(name)
                ? ++_globalUniqueAliasNames[name]
                : _globalUniqueAliasNames[name] = 0;
            }
            else
            {
                var uniqueNames = _uniqueAliasNames.ContainsKey(dbSelect)
                ? _uniqueAliasNames[dbSelect]
                : _uniqueAliasNames[dbSelect] = new Dictionary<string, int>();

                 count = uniqueNames.ContainsKey(name)
                    ? ++uniqueNames[name]
                    : uniqueNames[name] = 0;
            }
            
            return !fullName 
                ? $"{name.Substring(0, 1).ToLower()}{count}"
                : $"{name}{count}";
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