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
                var query = db.Posts.
                    Where(p => p.User.UserName != null).
                    Select(p => new { p.Content, p.Blog.User.UserName });

                var query1 = db.Posts.
                    Where(p => p.Content != null && p.User.UserName != null).
                    Select(p => new { p.Blog, p.Blog.User.UserName });

                var query2 = db.Posts.
                    Where(p => p.Content != null).
                    Select(p => new { p.Blog, p.User.UserName }).
                    Select(p => new { p.Blog.Url, p.Blog.Name, p.UserName });

                var query3 = db.Posts.
                    Where(p => p.Content != null).
                    Select(p => p.Blog).
                    Select(g => new { g.User, g.Url }).
                    Select(g => new { g.User.UserName, g.Url });

                var query4 = db.Blogs.Where(b => b.Posts.Any(p => p.User.UserName != null));
                var query5 = db.Posts.
                    Join(
                        query4, 
                        (p, b) => p.BlogId == b.BlogId && p.User.UserName == "ethan", 
                        (p, b) => new { PId = p.PostId, b.Name },
                        JoinType.LeftOuter);

                db.Query(query5);
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
