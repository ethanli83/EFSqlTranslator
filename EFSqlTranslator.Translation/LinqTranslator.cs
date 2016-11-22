using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EFSqlTranslator.Translation.DbObjects;
using EFSqlTranslator.Translation.MethodTranslators;

namespace EFSqlTranslator.Translation
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
            new WhereTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new AnyTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new JoinTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new GroupByTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new SelectTranslator(_infoProvider, _dbFactory).Register(_plugIns);
        }

        public static IDbScript Translate(Expression exp, IModelInfoProvider infoProvider, IDbObjectFactory dbFactory)
        {
            var translator = new LinqTranslator(infoProvider, dbFactory);
            translator.Visit(exp);
            return translator.GetResult();
        }

        internal IDbScript GetResult()
        {
            var dbSelect = _state.ResultStack.Pop() as IDbSelect;
            dbSelect = dbSelect.Optimize();

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
                dbRef.Alias = _nameGenerator.GenerateAlias(dbSelect, dbTable.TableName);
                
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

            if (p.Type.IsAnonymouse() || p.Type.IsGrouping())
            {
                var dbSelect = _state.GetLastSelect();
                dbRef = _dbFactory.BuildRef(null);
                dbRef.OwnerSelect = dbSelect;
                
                var collection = p.Type.IsGrouping() 
                    ? dbSelect.GroupBys.AsEnumerable() 
                    : dbSelect.Selection;

                foreach(var selectable in collection)
                    dbRef.RefSelection[selectable.GetAliasOrName()] = selectable;
            }


            if (dbRef == null && _state.ParamterStack.Count > 0)
            {
                var dbRefs = _state.ParamterStack.Peek();
                if (dbRefs.ContainsKey(p))
                    dbRef = dbRefs[p];
            }
            
            // if we can not find the parameter expression in the ParamterStack,
            // it means this is the first time we translates the parameter, so we
            // need to look for it in the most recently translated select
            // this is required because we may not always has select on the top
            // of the stack, especially we translating arguments for method calls 
            if (dbRef == null)
            {
                // var dbSelect = (IDbSelect)_state.ResultStack.Peek();
                // dbRef = dbSelect.Targets.First();
                var dbSelect = _state.GetLastSelect();
                var refCol = dbSelect.Selection.OfType<IDbRefColumn>().LastOrDefault();
                dbRef = refCol != null ? refCol.Ref : dbSelect.From;
            }

            if (dbRef == null)
                throw new NullReferenceException();

            _state.ResultStack.Push(dbRef);

            return p;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            Visit(m.Expression);

            var typeInfo = m.Type.GetTypeInfo();

            if (m.Expression.Type.IsAnonymouse())
            {
                var dbRef = (DbReference)_state.ResultStack.Peek();
                if (dbRef.RefSelection.ContainsKey(m.Member.Name))
                {
                    var dbObj = dbRef.RefSelection[m.Member.Name];
                   _state.ResultStack.Pop();
                    _state.ResultStack.Push(dbObj);
                    return m;
                }
            }

            if (m.Expression.Type.IsGrouping())            
            {
                var dbRef = (DbReference)_state.ResultStack.Pop();
                
                var dbSelect = dbRef.OwnerSelect;
                if (dbSelect.GroupBys.IsSingleKey)
                {
                    var kColumn = dbRef.RefSelection.First();
                    var colum = _dbFactory.BuildColumn(kColumn.Value.Ref, kColumn.Key, m.Type, m.Member.Name);
                    _state.ResultStack.Push(colum);
                }
                else
                {
                    _state.ResultStack.Push(dbRef);   
                }

                return m;
            }

            // if the member is a queryable entity, we need to translate it
            // into a relation, which means a join 
            var entityInfo = _infoProvider.FindEntityInfo(m.Type);
            if (entityInfo != null)
            {
                var dbObj = _state.ResultStack.Pop();
                var refCol = dbObj as IDbRefColumn;
                var fromRef = refCol != null ? refCol.Ref : (DbReference)dbObj;

                var fromEntity = _infoProvider.FindEntityInfo(m.Expression.Type);
                var relation = fromEntity.GetRelation(m.Member.Name);

                var dbJoin = GetOrCreateJoin(relation, fromRef, refCol);
                if (refCol != null)
                {
                    var newRefCol = _dbFactory.BuildRefColumn(dbJoin.To, m.Member.Name);
                    _state.ResultStack.Push(newRefCol);
                }    
                else if(relation.IsChildRelation)
                    _state.ResultStack.Push(dbJoin.To.Referee);
                else
                    _state.ResultStack.Push(dbJoin.To);

                return m;                     
            }

            if (typeInfo.Namespace.StartsWith("System"))
            {
                var dbObj = _state.ResultStack.Pop();
                var refCol = dbObj as IDbRefColumn;
                var dbRef = refCol != null ? refCol.Ref : (DbReference)dbObj;
                
                var col = _dbFactory.BuildColumn(dbRef, m.Member.Name, m.Type);
                _state.ResultStack.Push(col);

                // if the ref column is not now, and it is referring another ref column
                // we need to make sure the column we translated is in the sub select which
                // owns the ref column that referred by the current refColumn
                if (refCol != null)
                {
                    refCol.IsReferred = true;
                    if (refCol.RefTo != null)
                        refCol.RefTo.AddColumnToReferedSubSelect(m.Member.Name, m.Type, _dbFactory);
                }
                // todo: check if there is a case where if refCol is null but the dbRef is
                // referring a sub-select, we should not allow this to happen, as we always
                // want to know for sure what the column's owner ref should be. if the ref col
                // is null, then we need guest on what the column owner should be
                // else if (dbRef.Referee is IDbSelect)
                // {

                // }

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

        /// Create a join for the relation
        /// For parent relation, we create a join that joins to the parent table
        /// For child relation, we will create a sub select that returns the child table,
        /// and then joins to the sub select.
        /// The reason for joining to sub select for child relation, is that we want to be
        /// able to group on the join key, so that we will not repeat the parent row.
        private IDbJoin GetOrCreateJoin(EntityRelation relation, DbReference fromRef, IDbRefColumn refCol)
        {
            var dbSelect = fromRef.OwnerSelect;
            var tupleKey = Tuple.Create(dbSelect, relation);

            if (_state.CreatedJoins.ContainsKey(tupleKey))
                return _state.CreatedJoins[tupleKey];

            var toEntity = relation.ToEntity;
            var dbTable = _dbFactory.BuildTable(toEntity);

            DbReference joinTo;
            DbReference childRef = null;
            IDbSelect childSelect = null;
            
            // Create the join. For parent join, we just need to join to a Ref to the table
            // For child relation, we will firstly create a sub select that return the child table
            // and then join to then sub select
            if (!relation.IsChildRelation)
            {
                var tableAlias = _nameGenerator.GenerateAlias(dbSelect, dbTable.TableName);
                joinTo = _dbFactory.BuildRef(dbTable, tableAlias);
            }
            else
            {
                childRef = _dbFactory.BuildRef(dbTable);
                childSelect = _dbFactory.BuildSelect(childRef);
                childRef.Alias = _nameGenerator.GenerateAlias(childSelect, dbTable.TableName);

                var tableAlias = _nameGenerator.GenerateAlias(dbSelect, SqlTranslationHelper.SubSelectPrefix, true);
                joinTo = _dbFactory.BuildRef(childSelect, tableAlias);
            }

            var dbJoin = _dbFactory.BuildJoin(joinTo, dbSelect);
            dbSelect.Joins.Add(dbJoin);

            // build join condition
            IDbBinary condition = null;
            for (var i = 0; i < relation.FromKeys.Count; i++)
            {
                var fromKey = relation.FromKeys[i];
                var toKey = relation.ToKeys[i];

                var fromColumn = _dbFactory.BuildColumn(fromRef, fromKey.Name, fromKey.ValType);
                var toColumn = _dbFactory.BuildColumn(joinTo, toKey.Name, toKey.ValType);

                // If we have created a sub for child relation, we need to the columns 
                // that are used in join condition selected from the sub select.
                if (childRef != null && childSelect != null)
                {
                    var alias = _nameGenerator.GenerateAlias(childSelect, toKey.Name + SqlTranslationHelper.JoinKeySuffix, true);
                    var childColumn = _dbFactory.BuildColumn(childRef, toKey.Name, toKey.ValType, alias, true);
                    
                    // We need to also put the join key in the group of the sub select.
                    // This is to make sure the sub select is grouped by the key so that 
                    // the parent (outer select) will not be repeated
                    childSelect.Selection.Add(childColumn);

                    // todo: remove this, and get translator handler to add group bys
                    childSelect.GroupBys.Add(childColumn);
                    
                    toColumn.Name = alias;
                    toColumn.Alias = string.Empty;
                }

                // if the relation is found on a ref column, which means the from key of the 
                // join is not on a table but a derived select. In this case, we need to add
                // the from key into the derived select, as we will be using it in the join
                if (refCol != null && refCol.RefTo != null)
                {
                    var alias = _nameGenerator.GenerateAlias(dbSelect, toKey.Name + SqlTranslationHelper.JoinKeySuffix, true);
                    refCol.RefTo.AddColumnToReferedSubSelect(fromKey.Name, fromKey.ValType, _dbFactory, alias);

                    fromColumn.Name = alias;
                    fromColumn.Alias = string.Empty;
                }

                var binary = _dbFactory.BuildBinary(fromColumn, DbOperator.Equal, toColumn);
                condition = condition != null 
                    ? _dbFactory.BuildBinary(condition, DbOperator.And, binary)
                    : binary;
            }

            dbJoin.Condition = condition;
            dbJoin.Type = !relation.IsChildRelation ? JoinType.Inner : JoinType.LeftOuter;

            return _state.CreatedJoins[tupleKey] = dbJoin;
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
            if (left.IsNullVal() || right.IsNullVal())
            {
                dbOptr = dbOptr == DbOperator.Equal
                    ? DbOperator.Is
                    : dbOptr == DbOperator.NotEqual
                        ? DbOperator.IsNot
                        : dbOptr;
            }

            var dbBinary = _dbFactory.BuildBinary(left, dbOptr, right);
            
            if (dbOptr == DbOperator.Or)
            {
                var dbRefs = dbBinary.GetChildren<DbReference>().Distinct();
                foreach(var dbRef in dbRefs)
                    SqlTranslationHelper.ProcessSelection(dbRef, _dbFactory);    
            }

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

        public IDbSelect GetLastSelect()
        {
            IDbSelect dbSelect = null;

            var results = new Stack<IDbObject>();
            while(ResultStack.Count > 0)
            {
                var dbObject = ResultStack.Pop();
                results.Push(dbObject);
                
                dbSelect = dbObject as IDbSelect;
                if (dbSelect != null)
                    break;
            }

            while(results.Count > 0)
                ResultStack.Push(results.Pop());

            return dbSelect;
        }
    }

    public class UniqueNameGenerator
    {
        private readonly Dictionary<IDbSelect, Dictionary<string, int>> _uniqueAliasNames = 
            new Dictionary<IDbSelect, Dictionary<string, int>>();

            private readonly Dictionary<string, int> _globalUniqueAliasNames = 
                new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);

        public string GenerateAlias(IDbSelect dbSelect, string name, bool fullName = false)
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