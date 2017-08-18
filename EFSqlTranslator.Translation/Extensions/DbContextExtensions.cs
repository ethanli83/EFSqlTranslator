using System.Collections.Generic;
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
            IEnumerable<AbstractMethodTranslator> addons = null)
        {
            var executor = LinqExecutorMaker.Make(query, infoProvider, factory, db, addons);
            var result = executor.Execute();

            return result;
        }

        public static IEnumerable<dynamic> QueryDynamic(this DbContext db,
            IQueryable query, IModelInfoProvider infoProvider, IDbObjectFactory factory,
            IEnumerable<AbstractMethodTranslator> addons = null)
        {
            using (var connection = db.Database.GetDbConnection())
            {
                var script = QueryTranslator.Translate(query.Expression, infoProvider, factory, addons);
                var sql = script.ToString();

                var results = connection.Query(sql);
                return results;
            }
        }

        public static IEnumerable<T> Query<T>(this DbContext db,
            IQueryable<T> query, IModelInfoProvider infoProvider, IDbObjectFactory factory, out string sql,
            IEnumerable<AbstractMethodTranslator> addons = null)
        {
            var executor = LinqExecutorMaker.Make(query, infoProvider, factory, db, addons);
            sql = executor.Script.ToString();
            
            var result = executor.Execute();
            return result;
        }

        public static IEnumerable<dynamic> QueryDynamic(this DbContext db, 
            IQueryable query, IModelInfoProvider infoProvider, IDbObjectFactory factory, out string sql, 
            IEnumerable<AbstractMethodTranslator> addons = null)
        {
            using (var connection = db.Database.GetDbConnection())
            {
                var script = QueryTranslator.Translate(query.Expression, infoProvider, factory, addons);
                sql = script.ToString();

                var results = connection.Query(sql);
                return results;
            }
        }
    }
}