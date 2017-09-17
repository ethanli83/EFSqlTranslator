using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using EFSqlTranslator.Translation.DbObjects;
using EFSqlTranslator.Translation.MethodTranslators;
using Microsoft.EntityFrameworkCore;

namespace EFSqlTranslator.Translation.Extensions
{
    public static class DbContextExtensions
    {
        public static IEnumerable<T> Query<T>(this DbContext db,
            IQueryable<T> query, IModelInfoProvider infoProvider, IDbObjectFactory factory,
            AbstractMethodTranslator[] addons = null)
        {
            var executor = LinqExecutorMaker.Make(query, infoProvider, factory, addons);
            
            var connection = db.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            
            var result = executor.Execute(connection);

            return result;
        }

        public static IEnumerable<dynamic> QueryDynamic(this DbContext db,
            IQueryable query, IModelInfoProvider infoProvider, IDbObjectFactory factory,
            AbstractMethodTranslator[] addons = null)
        {
            var connection = db.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            
            var script = QueryTranslator.Translate(query.Expression, infoProvider, factory, addons);
            
            var constants = script.Parameterise();
            var dParams = new DynamicParameters();
            foreach (var dbConstant in constants)
            {
                dParams.Add(dbConstant.ParamName, dbConstant.Val, dbConstant.ValType.DbType);
            }
            
            var sql = script.ToString();
            var results = connection.Query(sql, dParams);
            return results;
        }

        public static IEnumerable<T> Query<T>(this DbContext db,
            IQueryable<T> query, IModelInfoProvider infoProvider, IDbObjectFactory factory, out string sql,
            AbstractMethodTranslator[] addons = null)
        {
            var executor = LinqExecutorMaker.Make(query, infoProvider, factory, addons);
            sql = executor.Script.ToString();
            
            var connection = db.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            
            var result = executor.Execute(connection);
            
            return result;
        }

        public static IEnumerable<dynamic> QueryDynamic(this DbContext db,
            IQueryable query, IModelInfoProvider infoProvider, IDbObjectFactory factory, out string sql,
            AbstractMethodTranslator[] addons = null)
        {
            var connection = db.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            
            var script = QueryTranslator.Translate(query.Expression, infoProvider, factory, addons);
            
            var constants = script.Parameterise();
            var dParams = new DynamicParameters();
            foreach (var dbConstant in constants)
            {
                dParams.Add(dbConstant.ParamName, dbConstant.Val, dbConstant.ValType.DbType);
            }
            
            sql = script.ToString();
            var results = connection.Query(sql, dParams);
            return results;
        }
    }
}