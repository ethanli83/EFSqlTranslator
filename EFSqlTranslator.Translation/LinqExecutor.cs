using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using EFSqlTranslator.Translation.DbObjects;
using EFSqlTranslator.Translation.MethodTranslators;
using Microsoft.EntityFrameworkCore;

namespace EFSqlTranslator.Translation
{
    public class LinqExecutor<T>
    {
        private readonly IncludeGraph _graph;
        private readonly DbContext _dtx;

        public LinqExecutor(IncludeGraph graph, IDbScript script, DbContext dtx)
        {
            _graph = graph;
            _dtx = dtx;
            Script = script;
        }

        public IEnumerable<T> Execute()
        {
            using (var connection = _dtx.Database.GetDbConnection())
            {
                connection.Open();

                foreach (var statement in Script.Scripts)
                {
                    var sql = statement.ToString();

                    if (statement is IDbSelect)
                    {
                        var node = _graph.ScriptToNodes[statement];
                        var entityType = node.Expression.GetReturnBaseType();
                        node.Result = connection.Query(entityType, sql);

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
        }

        public IDbScript Script { get; }
    }

    public class LinqExecutorMaker
    {
        public static LinqExecutor<T> Make<T>(IQueryable<T> queryable, IModelInfoProvider infoProvider, IDbObjectFactory dbFactory, DbContext dtx)
        {
            IncludeGraph includeGraph;
            var script = LinqTranslator.Translate(queryable.Expression, infoProvider, dbFactory, out includeGraph);
            return new LinqExecutor<T>(includeGraph, script, dtx);
        }
    }
}