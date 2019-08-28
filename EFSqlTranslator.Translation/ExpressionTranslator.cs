using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EFSqlTranslator.Translation.DbObjects;
using EFSqlTranslator.Translation.Extensions;
using EFSqlTranslator.Translation.MethodTranslators;

namespace EFSqlTranslator.Translation
{
    public class ExpressionTranslator : ExpressionVisitor
    {
        private readonly IModelInfoProvider _infoProvider;

        private readonly IDbObjectFactory _dbFactory;

        private readonly TranslationState _state;

        private readonly UniqueNameGenerator _nameGenerator = new UniqueNameGenerator();

        private readonly TranslationPlugIns _plugIns = new TranslationPlugIns();

        public ExpressionTranslator(
            IModelInfoProvider infoProvider, IDbObjectFactory dbFactory,
            TranslationState state = null, IEnumerable<AbstractMethodTranslator> methodTranslators = null)
        {
            _infoProvider = infoProvider;
            _dbFactory = dbFactory;
            _state = state ?? new TranslationState();

            RegisterDefaultPlugIns();

            if (methodTranslators == null)
                return;

            foreach (var translator in methodTranslators)
                translator.Register(_plugIns);
        }

        private void RegisterDefaultPlugIns()
        {
            new WhereTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new AnyTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new JoinTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new GroupByTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new OrderByTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new SelectTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new CountTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new AggregationTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new StringStartsEndsTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new ContainsTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new InTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new LimitTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new DistinctTranslator(_infoProvider, _dbFactory).Register(_plugIns);
            new DistinctCountTranslator(_infoProvider, _dbFactory).Register(_plugIns);
        }

        internal IDbObject GetElement()
        {
            return _state.ResultStack.Peek();
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
            else if (!c.Type.IsAnonymouse())
            {
                var dbConstant = _dbFactory.BuildConstant(c.Value, true);
                _state.ResultStack.Push(dbConstant);
            }

            return c;
        }

        protected override Expression VisitNew(NewExpression n)
        {
            var list = _dbFactory.BuildList<DbKeyValue>();

            if (n.Members != null)
            {
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

            _state.ResultStack.Push(list);
            return base.VisitNew(n);
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment ma)
        {
            var list = (IDbList<DbKeyValue>)_state.ResultStack.Pop();

            var member = ma.Member;
            var assignment = ma.Expression;

            Visit(assignment);

            var dbObj = _state.ResultStack.Pop();
            var dbKeyValue = _dbFactory.BuildKeyValue(member.Name, dbObj);
            list.Add(dbKeyValue);

            _state.ResultStack.Push(list);
            return ma;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            return VisitParameterInteral(p, false);
        }

        private Expression VisitParameterInteral(ParameterExpression p, bool ignoreParamStack)
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

                foreach (var selectable in collection)
                    dbRef.RefSelection[selectable.GetAliasOrName()] = selectable;
            }


            if (dbRef == null && !ignoreParamStack && _state.ParamterStack.Count > 0)
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
                var dbSelect = _state.GetLastSelect();

                var refCol = (_state.ResultStack.Peek() as IDbRefColumn) ??
                             dbSelect.Selection.OfType<IDbRefColumn>().LastOrDefault();

                dbRef = refCol != null ? refCol.Ref : dbSelect.From;
            }

            if (dbRef == null)
                throw new NullReferenceException();

            _state.ResultStack.Push(dbRef);

            return p;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            var expression = Visit(m.Expression);
            if (expression is ConstantExpression constExpr)
            {
                var container = constExpr.Value;
                var member = m.Member;
                
                object value = null;
                var valueRetrieved = false;
                switch (member)
                {
                    case FieldInfo field:
                        value = field.GetValue(container);
                        valueRetrieved = true;
                        break;
                    case PropertyInfo prop:
                        value = prop.GetValue(container, null);
                        valueRetrieved = true;
                        break;
                }

                if (valueRetrieved)
                {
                    var dbObject = _dbFactory.BuildConstant(value, true);
                    _state.ResultStack.Push(dbObject);
                    return m;
                }
            }

            var typeInfo = m.Type.GetTypeInfo();

            if (m.Expression.Type.IsAnonymouse())
            {
                var dbRef = (DbReference)_state.ResultStack.Peek();
                if (dbRef.RefSelection.ContainsKey(m.Member.Name))
                {
                    var dbObj = dbRef.RefSelection[m.Member.Name];

                    // pop out the dbRef from the stack, it was the result of
                    // translate a parameter, and it is not required for following translation
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
                    var kColumn = dbSelect.GroupBys.Single();
                    _state.ResultStack.Push(kColumn);
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

                var dbJoin = GetOrCreateJoin(relation, fromRef, refCol ?? fromRef.ReferredRefColumn);

                // RefColumnAlias is used as alias in case we need to create a ref column for this dbRef
                dbJoin.To.RefColumnAlias = m.Member.Name;

                if (refCol != null)
                {
                    refCol = _dbFactory.BuildRefColumn(dbJoin.To, m.Member.Name);

                    if (dbJoin.To.Referee is IDbSelect subSelect)
                        refCol.RefTo = subSelect.Selection.OfType<IDbRefColumn>().Single();

                    _state.ResultStack.Push(refCol);
                    return m;
                }

                _state.ResultStack.Push(relation.IsChildRelation ? dbJoin.To.Referee : dbJoin.To);
                return m;
            }

            if (typeInfo.Namespace.StartsWith("System"))
            {
                var dbObj = _state.ResultStack.Pop();
                var refCol = dbObj as IDbRefColumn;
                var dbRef = refCol != null ? refCol.Ref : (DbReference)dbObj;

                var fieldInfo = _infoProvider.FindFieldInfo(m.Member);
                var col = _dbFactory.BuildColumn(dbRef, fieldInfo.DbName, fieldInfo.ValType);
                _state.ResultStack.Push(col);

                // if we create a column whose DbRef is using by a RefColumn
                // we need to make sure the column is added to the ref column's owner select
                // This normally happen when we are accessing a column from a child relation
                refCol = refCol ?? dbRef.ReferredRefColumn;

                // if the ref column is not now, and it is referring another ref column
                // we need to make sure the column we translated is in the sub select which
                // owns the ref column that referred by the current refColumn
                refCol?.RefTo?.AddToReferedSelect(_dbFactory, fieldInfo.DbName, fieldInfo.ValType);

                return m;
            }

            return base.VisitMember(m);
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

            if (!relation.IsChildRelation && _state.CreatedJoins.ContainsKey(tupleKey))
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

                var tableAlias = _nameGenerator.GenerateAlias(dbSelect, TranslationConstants.SubSelectPrefix, true);
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

                var fromColumn = _dbFactory.BuildColumn(fromRef, fromKey.DbName, fromKey.ValType);
                var toColumn = _dbFactory.BuildColumn(joinTo, toKey.DbName, toKey.ValType);

                // If we have created a sub for child relation, we need to the columns
                // that are used in join condition selected from the sub select.
                if (childRef != null && childSelect != null)
                {
                    var alias = _nameGenerator.GenerateAlias(childSelect, toKey.DbName + TranslationConstants.JoinKeySuffix, true);
                    var childColumn = _dbFactory.BuildColumn(childRef, toKey.DbName, toKey.ValType, alias, true);

                    /**
                     * We need to also put the join key in the group of the sub select.
                     * This is to make sure the sub select is grouped by the key so that
                     * the parent (outer select) will not be repeated
                     * This operation needs to happen here not in the aggregation method call.
                     * The reason is that in aggregtion method calls we do not know which column
                     * from the entity is used in relation, so they will not be able to create
                     * the correct column
                     */
                    childSelect.Selection.Add(_dbFactory.BuildRefColumn(childRef));
                    childSelect.Selection.Add(childColumn);
                    childSelect.GroupBys.Add(childColumn);

                    toColumn.Name = alias;
                    toColumn.Alias = string.Empty;
                }

                // if the relation is found on a fromRef which is referring a sub-select,
                // it means the from key of the join is not on a table but a derived select.
                // In this case, we need to add the from key into the derived select, as we will
                // be using it in the join
                if (fromRef.Referee is IDbSelect)
                {
                    var alias = _nameGenerator.GenerateAlias(dbSelect, toKey.DbName + TranslationConstants.JoinKeySuffix, true);
                    fromColumn.Name = alias;
                    fromColumn.Alias = string.Empty;

                    // try to recursively add the join key to all connected sub select.
                    refCol.RefTo?.AddToReferedSelect(_dbFactory, fromKey.DbName, fromKey.ValType, alias);
                }

                var binary = _dbFactory.BuildBinary(fromColumn, DbOperator.Equal, toColumn);
                condition = condition.UpdateBinary(binary, _dbFactory);
            }

            dbJoin.Condition = condition;

            // all relations need to follow the join type
            if (fromRef.OwnerJoin != null)
                dbJoin.Type = fromRef.OwnerJoin.Type;

            if (relation.IsChildRelation)
                dbJoin.Type = DbJoinType.LeftOuter;

            return _state.CreatedJoins[tupleKey] = dbJoin;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            var caller = m.GetCaller();
            var args = m.GetArguments();

            Visit(caller);

            // remove unneed DbReference from stack, this is a side effect of translation of the caller
            // we need to translate the caller to have the required select on the stack
            // but we dont neeed other thing that come with it, such as the DbReference
            while (_state.ResultStack.Count > 0 && _state.ResultStack.Peek() is DbReference)
                _state.ResultStack.Pop();

            VistMethodArguments(args);

            if (!_plugIns.TranslateMethodCall(m, _state, _nameGenerator))
                throw new NotSupportedException();

            return m;
        }

        private void VistMethodArguments(Expression[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            foreach (var argExpr in args)
                Visit(argExpr);
        }

        protected override Expression VisitLambda<T>(Expression<T> l)
        {
            var pList = new Dictionary<ParameterExpression, DbReference>();
            // the order of the parameters need to be reversed, as the first
            // query that be translated will be at the bottom of the stack, and the query
            // for the last parameter will be at the top
            var results = new Stack<IDbObject>();
            foreach (var p in l.Parameters.Reverse())
            {
                VisitParameterInteral(p, true);

                var dbRef = (DbReference)_state.ResultStack.Pop();
                pList[p] = dbRef;

                // pop out used select for the parameter just translated
                // so that the next parameter will be assigned with current select
                while (_state.ResultStack.Count > 0)
                {
                    var dbObj = _state.ResultStack.Pop();
                    results.Push(dbObj);
                    if (dbObj is IDbSelect)
                        break;
                }
            }

            while (results.Count > 0)
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

            var dbOptr = _dbFactory.GetDbOperator(b.NodeType, b.Left.Type, b.Right.Type);
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
                var dbRefs = dbBinary.GetOperands().OfType<IDbSelectable>().Select(s => s.Ref);
                foreach (var dbRef in dbRefs)
                    SqlTranslationHelper.UpdateJoinType(dbRef);
            }

            _state.ResultStack.Push(dbBinary);
            return b;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Not)
            {
                var expression = Visit(node.Operand);
                var dbElement =  _state.ResultStack.Pop();

                var one = _dbFactory.BuildConstant(true);
                var dbBinary = _dbFactory.BuildBinary(dbElement, DbOperator.NotEqual, one);
                
                _state.ResultStack.Push(dbBinary);
                
                return expression;
            }
            
            return base.VisitUnary(node);
        }
    }
}