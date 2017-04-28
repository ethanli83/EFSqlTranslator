using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

                        if (entityType.IsAnonymouse())
                        {
                            var result = connection.Query<dynamic>(sql);
                            node.Result = ConvertDynamicToRealType(entityType, result);
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
        }

        public IEnumerable<object> ConvertDynamicToRealType(Type type, IEnumerable<dynamic> data)
        {
            var properties = typeof(T).GetProperties();
            var constructor = type.GetConstructors().Single();

            var fdList = new List<object>();
            var valTypes = new Dictionary<int, Type>();
            var infoTypes = new Dictionary<int, Type>();
            foreach (var row in data)
            {
                var objIdx = 0;
                var objArray = new object[properties.Length];

                var valDict = (IDictionary<string, object>)row;
                for (var i = 0; i < properties.Length; i ++)
                {
                    var info = properties[i];
                    var val = valDict[info.Name];

                    var valType = valTypes.ContainsKey(i) ? valTypes[i] : valTypes[i] = val.GetType();
                    var infoType = infoTypes.ContainsKey(i) ? infoTypes[i] : infoTypes[i] = info.PropertyType.StripNullable();
                    if (valType != infoType)
                    {
                        objArray[objIdx++] = infoType == typeof(Guid)
                            ? new Guid((byte[])val)
                            : Convert.ChangeType(val, infoType);
                    }
                    else
                    {
                        objArray[objIdx++] = val;
                    }
                }

                var obj = constructor.Invoke(objArray);
                fdList.Add(obj);
            }

            return fdList;
        }

        public IDbScript Script { get; }
    }

    public class LinqExecutorMaker
    {
        public static LinqExecutor<T> Make<T>(IQueryable<T> queryable, IModelInfoProvider infoProvider, IDbObjectFactory dbFactory, DbContext dtx)
        {
            IncludeGraph includeGraph;
            var script = QueryTranslator.Translate(queryable.Expression, infoProvider, dbFactory, out includeGraph);
            return new LinqExecutor<T>(includeGraph, script, dtx);
        }
    }
}