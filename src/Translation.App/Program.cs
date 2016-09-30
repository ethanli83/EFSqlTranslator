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
                var query = db.Blogs.Where(b => b.Url != null);
                var blogs = db.Query(query);

                var query2 = db.Posts.
                    Join(query, (p, b) => p.BlogId == b.BlogId, (p, b) => p).
                    Select(p => new 
                    {
                        p.PostId,
                        p.Content
                    });

                var query3 = db.Posts.
                    Join(query, (p, b) => p.BlogId == b.BlogId, (p, b) => new { p, b }).
                    Select(x => new 
                    {
                        x.b.Url,
                        x.p.Content
                    });
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
                return connection.Query<T>(sql);
            }
        }
    }
}
