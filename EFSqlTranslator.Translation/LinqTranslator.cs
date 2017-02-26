using System;
using System.Linq;
using System.Linq.Expressions;
using EFSqlTranslator.Translation.DbObjects;
using EFSqlTranslator.Translation.MethodTranslators;
using NLog;

namespace EFSqlTranslator.Translation
{
    public static class LinqTranslator
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public static IDbScript Translate(Expression exp, IModelInfoProvider infoProvider, IDbObjectFactory dbFactory)
        {
            IncludeGraph graph;
            return Translate(exp, infoProvider, dbFactory, out graph);
        }

        public static IDbScript Translate(Expression exp, IModelInfoProvider infoProvider, IDbObjectFactory dbFactory, out IncludeGraph includeGraph)
        {
            includeGraph = IncludeGraphBuilder.Build(exp);

            var script = TranslateGraph(includeGraph, infoProvider, dbFactory, new UniqueNameGenerator());

            if (Logger.IsDebugEnabled)
                Logger.Debug(Tuple.Create(exp, script.ToString()));

            return script;
        }

        private static IDbScript TranslateGraph(
            IncludeGraph includeGraph, IModelInfoProvider infoProvider, IDbObjectFactory dbFactory, UniqueNameGenerator nameGenerator)
        {
            var script = dbFactory.BuildScript();

            TranslateGraphNode(includeGraph.Root, script, infoProvider, dbFactory, nameGenerator);

            return script;
        }

        private static void TranslateGraphNode(
            IncludeNode graphNode, IDbScript script, IModelInfoProvider infoProvider, IDbObjectFactory dbFactory, UniqueNameGenerator nameGenerator)
        {
            //TODO: check if there is a chain in the relation and throw exceptions

            var fromSelect = graphNode.FromNode?.Select;

            var state = new TranslationState();
            if (fromSelect != null)
                state.ResultStack.Push(graphNode.FromNode.Select);

            var translator = new ExpressionToSqlTranslator(infoProvider, dbFactory, state);

            // translated current included node
            translator.Visit(graphNode.Expression);

            var dbObject = translator.GetElement();
            var dbRef = dbObject as DbReference;

            var includedSelect = dbRef != null ? dbFactory.BuildSelect(dbRef) : (IDbSelect)dbObject;
            includedSelect = includedSelect.Optimize();

            // if from node is not null, then we need to add the forgien key into the selection
            // of the temp table, and make the new select join to the translated select
            if (graphNode.FromNode != null)
            {
                UpdateFromNodeTempTable(graphNode.FromNode, dbObject, includedSelect, dbFactory, nameGenerator);
            }

            // if the graph node has child node, we need to create temp table for current node
            // so that the child nodes can join to the temp table containing forgien keys
            if (graphNode.ToNodes.Any())
            {
                UpdateIncludeSelectAndProcessToNodes(graphNode, includedSelect, script, infoProvider, dbFactory, nameGenerator);
            }
            else
            {
                graphNode.Select = includedSelect;
                script.Scripts.Add(graphNode.Select);
            }

            // update fill function
            if (graphNode.FromNode != null)
            {
                graphNode.FillFunc = FillFunctionMaker.Make(graphNode, infoProvider);
            }

            graphNode.Graph.ScriptToNodes[graphNode.Select] = graphNode;
        }

        private static void UpdateFromNodeTempTable(
            IncludeNode fromNode, IDbObject dbObject, IDbSelect includedSelect,
            IDbObjectFactory dbFactory, UniqueNameGenerator nameGenerator)
        {
            var fromSelect = fromNode.Select;
            var fromRef = dbObject as DbReference;

            var dbJoin = fromSelect.Joins.Single(
                j => fromRef != null ? ReferenceEquals(j.To, fromRef) : ReferenceEquals(j.To.Referee, dbObject));

            // remove the join to included relation from fromSelect
            fromSelect.Joins.Remove(dbJoin);

            if (dbObject is IDbSelect)
            {
                var refCol = includedSelect.Selection.Single(c => c is IDbRefColumn);
                includedSelect.Selection.Remove(refCol);
            }

            var keys = dbJoin.Condition.GetDbObjects<IDbColumn>().ToArray();
            var fromKeys = keys.Where(c => !ReferenceEquals(c.Ref, dbJoin.To)).ToArray();
            var toKeys = keys.Where(c => ReferenceEquals(c.Ref, dbJoin.To)).ToArray();

            var tempTable = fromNode.TempTable;
            var sourceSelect = tempTable.SourceSelect;

            fromRef = sourceSelect.GetReturnEntityRef();
            var returnRef = includedSelect.GetReturnEntityRef();

            var tempSelect = dbFactory.BuildSelect(tempTable);
            tempSelect.From.Alias = nameGenerator.GenerateAlias(tempSelect, tempTable.TableName);

            var joinToTemp = MakeJoin(includedSelect, tempSelect, dbFactory, nameGenerator);
            includedSelect.Joins.Add(joinToTemp);

            for (var i = 0; i < fromKeys.Length; i++)
            {
                var fromKey = fromKeys[i];
                var toKey = toKeys[i];

                var fromPkCol = dbFactory.BuildColumn(fromRef, fromKey.Name, fromKey.ValType);
                sourceSelect.Selection.Add(fromPkCol);

                var tempPkCol = dbFactory.BuildColumn(tempSelect.From, fromKey.Name, fromKey.ValType);
                tempSelect.Selection.Add(tempPkCol);
                tempSelect.GroupBys.Add(tempPkCol);

                if (dbObject is IDbSelect)
                {
                    toKey = (IDbColumn)includedSelect.Selection.Single(c => c.GetAliasOrName() == toKey.GetNameOrAlias());
                    includedSelect.Selection.Remove(toKey);
                    includedSelect.GroupBys.Remove(toKey);
                }

                fromPkCol = dbFactory.BuildColumn(returnRef, fromKey.Name, fromKey.ValType);
                var toPkCol = dbFactory.BuildColumn(joinToTemp.To, toKey.Name, toKey.ValType);
                var binary = dbFactory.BuildBinary(fromPkCol, DbOperator.Equal, toPkCol);
                joinToTemp.Condition = joinToTemp.Condition.UpdateBinary(binary, dbFactory);
            }
        }

        private static void UpdateIncludeSelectAndProcessToNodes(
            IncludeNode graphNode, IDbSelect includedSelect, IDbScript script,
            IModelInfoProvider infoProvider, IDbObjectFactory dbFactory, UniqueNameGenerator nameGenerator)
        {
            // create temp table
            var entityRef = includedSelect.GetReturnEntityRef();
            var returnTable = (IDbTable)entityRef.Referee;

            var newIncludedSelect = dbFactory.BuildSelect(returnTable);
            newIncludedSelect.From.Alias = nameGenerator.GenerateAlias(newIncludedSelect, returnTable.TableName);

            var tempTableName = TranslationConstants.TempTablePreix + nameGenerator.GenerateAlias(null, returnTable.TableName, true);
            var tempTable = dbFactory.BuildTempTable(tempTableName, includedSelect);

            var tempSelect = dbFactory.BuildSelect(tempTable);
            tempSelect.From.Alias = nameGenerator.GenerateAlias(tempSelect, tempTable.TableName);

            var joinToTemp = MakeJoin(newIncludedSelect, tempSelect, dbFactory, nameGenerator);
            var joinTo = joinToTemp.To;

            newIncludedSelect.Joins.Add(joinToTemp);

            foreach (var pk in returnTable.PrimaryKeys)
            {
                var fromPkCol = dbFactory.BuildColumn(entityRef, pk.Name, pk.ValType);
                includedSelect.Selection.Add(fromPkCol);

                var toPkCol = dbFactory.BuildColumn(tempSelect.From, pk.Name, pk.ValType);
                tempSelect.Selection.Add(toPkCol);
                tempSelect.GroupBys.Add(toPkCol);

                toPkCol = dbFactory.BuildColumn(joinTo, pk.Name, pk.ValType);
                var binary = dbFactory.BuildBinary(fromPkCol, DbOperator.Equal, toPkCol);
                joinToTemp.Condition = joinToTemp.Condition.UpdateBinary(binary, dbFactory);
            }

            var orderCol = dbFactory.BuildColumn(tempSelect.From, tempTable.RowNumberColumnName, typeof(int));
            tempSelect.Selection.Add(orderCol);

            orderCol = dbFactory.BuildColumn(joinTo, tempTable.RowNumberColumnName, typeof(int));
            newIncludedSelect.OrderBys.Add(orderCol);

            graphNode.Select = newIncludedSelect;
            graphNode.TempTable = tempTable;

            script.Scripts.Add(tempTable.GetCreateStatement(dbFactory));

            script.Scripts.Add(graphNode.Select);

            foreach (var toNode in graphNode.ToNodes)
                TranslateGraphNode(toNode, script, infoProvider, dbFactory, nameGenerator);

            script.Scripts.Add(tempTable.GetDropStatement(dbFactory));
        }

        private static IDbJoin MakeJoin(IDbSelect ownerSelect, IDbObject tempSelect, IDbObjectFactory dbFactory, UniqueNameGenerator nameGenerator)
        {
            var joinAlias = nameGenerator.GenerateAlias(ownerSelect, TranslationConstants.SubSelectPrefix);
            var joinTo = dbFactory.BuildRef(tempSelect, joinAlias);
            return dbFactory.BuildJoin(joinTo, ownerSelect);
        }
    }
}