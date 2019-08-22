using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;
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
                var constants = statement.Parameterise();
                
                var dParams = new DynamicParameters();
                foreach (var dbConstant in constants)
                {
                    dParams.Add(dbConstant.ParamName, dbConstant.Val, dbConstant.ValType.DbType);
                }
                
                var sql = statement.ToString();

                if (statement is IDbSelect)
                {
                    var node = _graph.ScriptToNodes[statement];
                    var entityType = node.Expression.GetReturnBaseType();

                    if (entityType.IsAnonymouse())
                    {
                        var result = connection.Query(sql, dParams);
                        node.Result = new DynamicDataConvertor(entityType).Convert(result);
                    }
                    else
                    {
                        node.Result = connection.Query(entityType, sql, dParams);

                        // Dapper will always use Int64 for default value '0', 
                        // so we need to convert it to the correct type
                        if (entityType.IsNumeric())
                        {
                            node.Result = node.Result
                               .Select(v => Convert.ChangeType(v, entityType)).ToArray();
                        }
                    }

                    if (node.FromNode != null)
                        node.FillFunc.Compile().DynamicInvoke(node.FromNode.Result, node.Result);
                }
                else
                {
                    connection.Execute(sql, dParams);
                }
            }

            return _graph.Root.Result.Cast<T>();
        }

        public IDbScript Script { get; }
    }

    public static class LinqExecutorMaker
    {
        public static LinqExecutor<T> Make<T>(
            IQueryable<T> queryable, IModelInfoProvider infoProvider, IDbObjectFactory dbFactory, 
            AbstractMethodTranslator[] addons = null)
        {
            var script = QueryTranslator.Translate(queryable.Expression, infoProvider, dbFactory, out var includeGraph, addons);
            return new LinqExecutor<T>(includeGraph, script);
        }
    }
}