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
                    Where(p => p.Blog.User.Comments.Any(c => c.CommentId > 20));

                var query1 = db.Posts.
                    Where(p => p.Content != null).
                    GroupBy(p => p.BlogId).
                    Select(g => new { g.Key });

                var query2 = db.Posts.
                    Where(p => p.Content != null).
                    GroupBy(p => new { p.BlogId, p.User.UserName }).
                    Select(g => new { g.Key.UserName });

                var query2_1 = db.Posts.
                    Where(p => p.Content != null).
                    GroupBy(p => new { p.Blog }).
                    Select(g => new { g.Key.Blog.Url, g.Key.Blog.User.UserName });

                var query3 = db.Posts.
                    Where(p => p.Content != null).
                    Select(p => new { p.Blog, p.User }).
                    GroupBy(x => new { x.Blog }).
                    Select(x => new { x.Key.Blog.Url, x.Key.Blog.User.UserName });

                var query4 = db.Posts.
                    Where(p => p.Content != null).
                    Select(p => new { p.Blog, p.User }).
                    Select(x => new { x.Blog.Url, x.User.UserId, u = x.Blog.User.UserName });

                db.Query(query2_1);
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