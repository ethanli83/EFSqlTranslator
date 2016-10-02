using System;
using Microsoft.Data.Sqlite;
using Dapper;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Translation.EF;
using Translation.DbObjects.SqlObjects;

namespace Translation.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            using (var db = new BloggingContext())
            {
                var query = db.Blogs.Where(b => b.Posts.Any(p => p.User.UserName != null));
                var query1 = db.Posts.
                    Join(
                        query, 
                        (p, b) => p.BlogId == b.BlogId && p.Content != null,
                        (p, b) => new { PId = p.PostId, b.Name },
                        JoinType.LeftOuter);

                db.Query(query);
                db.Query(query1);
            }
        }
    }

    public static class DbContextExtensions
    {
        public static IEnumerable<T> Query<T>(this DbContext db, IQueryable<T> query)
        {
            using (var connection = new SqliteConnection(BloggingContext.ConnectionString))
            {
                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();
                Console.WriteLine(sql);
                var results = connection.Query(sql);
                return results.OfType<T>();
            }
        }
    }
}
