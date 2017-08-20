using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Dapper;
using EFSqlTranslator.Translation.DbObjects;
using EFSqlTranslator.Translation.Extensions;
using EFSqlTranslator.Translation.MethodTranslators;

namespace EFSqlTranslator.Translation
{
    public class LinqExecutor<T>
    {
        private readonly IncludeGraph _graph;

        public LinqExecutor(IncludeGraph graph, IDbScript script)
        {
            _graph = graph;
            Script = script;
        }

        public IEnumerable<T> Execute(DbConnection connection)
        {
            foreach (var statement in Script.Scripts)
            {
                var sql = statement.ToString();

                if (statement is IDbSelect)
                {
                    var node = _graph.ScriptToNodes[statement];
                    var entityType = node.Expression.GetReturnBaseType();

                    if (entityType.IsAnonymouse())
                    {
                        var result = connection.Query(sql);
                        node.Result = new DynamicDataConvertor(entityType).Convert(result);
                    }
                    else
                    {
                        node.Result = connection.Query(entityType, sql);
                    }

                    if (node.FromNode != null)
                        node.FillFunc.Compile().DynamicInvoke(node.FromNode.Result, node.Result);
                }
                else
                {
                    connection.Execute(sql);
                }
            }

            return _graph.Root.Result.Cast<T>();
        }

        public IDbScript Script { get; }
    }

    public class LinqExecutorMaker
    {
        public static LinqExecutor<T> Make<T>(
            IQueryable<T> queryable, IModelInfoProvider infoProvider, IDbObjectFactory dbFactory, 
            IEnumerable<AbstractMethodTranslator> addons = null)
        {
            IncludeGraph includeGraph;
            var script = QueryTranslator.Translate(queryable.Expression, infoProvider, dbFactory, out includeGraph, addons);
            return new LinqExecutor<T>(includeGraph, script);
        }
    }
}