using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using EFSqlTranslator.Translation.DbObjects;
using Microsoft.EntityFrameworkCore;

namespace EFSqlTranslator.Translation
{
    public static class DbContextExtensions
    {
        public static IEnumerable<T> Query<T>(this DbContext db,
            IQueryable<T> query, IModelInfoProvider infoProvider, IDbObjectFactory factory, out string sql)
        {
            using (var connection = db.Database.GetDbConnection())
            {
                var script = LinqTranslator.Translate(query.Expression, infoProvider, factory);
                sql = script.ToString();

                Console.WriteLine(sql);


                var results = connection.Query<T>(sql);
                return results;
            }
        }

        public static IEnumerable<dynamic> Query(this DbContext db,
            IQueryable query, IModelInfoProvider infoProvider, IDbObjectFactory factory, out string sql)
        {
            using (var connection = db.Database.GetDbConnection())
            {
                var script = LinqTranslator.Translate(query.Expression, infoProvider, factory);
                sql = script.ToString();

                var results = connection.Query(sql);
                return results;
            }
        }

//        public static IEnumerable<T> Query<T, T1>(this DbContext db,
//            IIncludableQueryable<T, T1> query,
//            Func<T, T1, T> fillFunc, IModelInfoProvider infoProvider, IDbObjectFactory factory, out string sql)
//        {
//            using (var connection = db.Database.GetDbConnection())
//            {
//                var script = LinqTranslator.Translate(query.Expression, infoProvider, factory);
//                sql = script.ToString();
//
//                var splitOn = string.Join(",", script.IncludeSplitKeys);
//                var results = connection.Query(sql, fillFunc, splitOn: splitOn);
//                return results;
//            }
//        }
//
//        public static IEnumerable<T> Query<T, T1, T2>(this DbContext db,
//            IIncludableQueryable<T, T2> query, Func<T, T1, T2, T> fillFunc,
//            IModelInfoProvider infoProvider, IDbObjectFactory factory, out string sql)
//        {
//            using (var connection = db.Database.GetDbConnection())
//            {
//                var script = LinqTranslator.Translate(query.Expression, infoProvider, factory);
//                sql = script.ToString();
//
//                var splitOn = string.Join(",", script.IncludeSplitKeys);
//                var results = connection.Query(sql, fillFunc, splitOn: splitOn);
//                return results;
//            }
//        }
//
//        public static IEnumerable<T> Query<T, T1, T2, T3>(this DbContext db,
//            IIncludableQueryable<T, T3> query,
//            Func<T, T1, T2, T3, T> fillFunc, IModelInfoProvider infoProvider, IDbObjectFactory factory,
//            out string sql)
//        {
//            using (var connection = db.Database.GetDbConnection())
//            {
//                var script = LinqTranslator.Translate(query.Expression, infoProvider, factory);
//                sql = script.ToString();
//
//                var splitOn = string.Join(",", script.IncludeSplitKeys);
//                var results = connection.Query(sql, fillFunc, splitOn: splitOn);
//                return results;
//            }
//        }
//
//        public static IEnumerable<T> Query<T, T1, T2, T3, T4>(this DbContext db,
//            IIncludableQueryable<T, T4> query,
//            Func<T, T1, T2, T3, T4, T> fillFunc, IModelInfoProvider infoProvider, IDbObjectFactory factory,
//            out string sql)
//        {
//            using (var connection = db.Database.GetDbConnection())
//            {
//                var script = LinqTranslator.Translate(query.Expression, infoProvider, factory);
//                sql = script.ToString();
//
//                var splitOn = string.Join(",", script.IncludeSplitKeys);
//                var results = connection.Query(sql, fillFunc, splitOn: splitOn);
//                return results;
//            }
//        }
//
//        public static IEnumerable<T> Query<T, T1, T2, T3, T4, T5>(this DbContext db,
//            IIncludableQueryable<T, T5> query,
//            Func<T, T1, T2, T3, T4, T5, T> fillFunc, IModelInfoProvider infoProvider, IDbObjectFactory factory,
//            out string sql)
//        {
//            using (var connection = db.Database.GetDbConnection())
//            {
//                var script = LinqTranslator.Translate(query.Expression, infoProvider, factory);
//                sql = script.ToString();
//
//                var splitOn = string.Join(",", script.IncludeSplitKeys);
//                var results = connection.Query(sql, fillFunc, splitOn: splitOn);
//                return results;
//            }
//        }
//
//        public static IEnumerable<T> Query<T, T1, T2, T3, T4, T5, T6>(this DbContext db,
//            IIncludableQueryable<T, T6> query,
//            Func<T, T1, T2, T3, T4, T5, T6, T> fillFunc, IModelInfoProvider infoProvider, IDbObjectFactory factory,
//            out string sql)
//        {
//            using (var connection = db.Database.GetDbConnection())
//            {
//                var script = LinqTranslator.Translate(query.Expression, infoProvider, factory);
//                sql = script.ToString();
//
//                var splitOn = string.Join(",", script.IncludeSplitKeys);
//                var results = connection.Query(sql, fillFunc, splitOn: splitOn);
//                return results;
//            }
//        }
    }
}