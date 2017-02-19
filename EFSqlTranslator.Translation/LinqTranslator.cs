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
            var includeGraph = IncludeGraphBuilder.Build(exp);

            var script = TranslateGraph(includeGraph, infoProvider, dbFactory);

            if (Logger.IsDebugEnabled)
                Logger.Debug(Tuple.Create(exp, script.ToString()));

            return script;
        }

        private static IDbScript TranslateGraph(IncludeGraph includeGraph, IModelInfoProvider infoProvider, IDbObjectFactory dbFactory)
        {
            var script = dbFactory.BuildScript();

            TranslateGraphNode(includeGraph.Root, script, infoProvider, dbFactory);

            return script;
        }

        private static void TranslateGraphNode(IncludeNode graphNode, IDbScript script, IModelInfoProvider infoProvider, IDbObjectFactory dbFactory)
        {
            var state = new TranslationState();

            var fromSelect = graphNode.FromNode?.Select as IDbSelect;
            if (fromSelect != null)
                state.ResultStack.Push(graphNode.FromNode.Select);

            var translator = new ExpressionToSqlTranslator(infoProvider, dbFactory, state);

            translator.Visit(graphNode.Expression);

            var dbObject = translator.GetElement();
            var dbRef = dbObject as DbReference;

            var toSelect = dbRef != null ? dbFactory.BuildSelect(dbRef) : (IDbSelect)dbObject;
            graphNode.Select = toSelect.Optimize();

            if (fromSelect != null)
            {
                var dbJoin = fromSelect.Joins.Single(
                    j => dbRef != null ? ReferenceEquals(j.To, dbRef) : ReferenceEquals(j.To.Referee, dbObject));

                fromSelect.Joins.Remove(dbJoin);

                var keys = dbJoin.Condition.GetDbObjects<IDbColumn>().ToArray();
                var fromKeys = keys.Where(c => !ReferenceEquals(c.Ref, dbJoin.To));
                foreach (var key in fromKeys)
                {
                    fromSelect.Selection.Add(key);
                }

                toSelect.Selection.Clear();
                toSelect.GroupBys.Clear();

//                var toKeys = keys.Where(c => ReferenceEquals(c.Ref, dbJoin.To));
//                foreach (var key in toKeys)
//                {
//                    var columns = toSelect.Selection.Where(c => c.GetAliasOrName() == key.GetNameOrAlias());
//                    foreach (var col in columns.ToArray())
//                    {
//                        toSelect.Selection.Remove(col);
//                        toSelect.GroupBys.Remove(col);
//                    }
//                }


            }

            script.Scripts.Add(graphNode.Select);

            if (!graphNode.ToNodes.Any())
                return;

            var entityInfo = infoProvider.FindEntityInfo(graphNode.Expression.GetReturnType());
            foreach (var keyInfo in entityInfo.Keys)
            {
                var keyCol = dbFactory.BuildColumn(toSelect.From, keyInfo.Name, keyInfo.ValType);
                toSelect.Selection.Add(keyCol);
            }

            foreach (var toNode in graphNode.ToNodes)
                TranslateGraphNode(toNode, script, infoProvider, dbFactory);
        }
    }
}