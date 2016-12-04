using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EFSqlTranslator.ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            using (var db = new BloggingContext())
            {
                var query11 = db.Posts.
                    Where(p => p.Content != null).
                    GroupBy(p => p.BlogId).
                    Select(g => new { cnt = g.Count() });

                var query12 = db.Blogs.
                    Where(b => b.Url != null).
                    Select(b => new { cnt = b.Posts.Count() });

                var query13 = db.Posts.
                    Where(p => p.Content != null).
                    GroupBy(p => p.BlogId).
                    Select(g => new { cnt = g.Count(p => p.Blog.Url != null) });

                var query14 = db.Blogs.
                    Where(b => b.Url != null).
                    Select(b => new
                    {
                        b.Url,
                        b.User.UserId,
                        cnt = b.Posts.Count(p => p.Content != null)
                    });

                var query15 = db.Blogs.
                    Where(b => b.Url != null).
                    Select(b => new
                    {
                        b.Url,
                        b.User.UserId,
                        cnt = b.Posts.Count(p => p.User.UserName != null)
                    });

                var query16 = db.Posts.
                    Where(p => p.Content != null).
                    GroupBy(p => new { p.Blog }).
                    Select(g => new
                    {
                        g.Key.Blog.Url,
                        g.Key.Blog.User.UserId,
                        cnt = g.Count(p => p.User.UserName != null)
                    });

                db.Query(query11);
                db.Query(query12);
                db.Query(query13);
                db.Query(query14);
                db.Query(query15);
                db.Query(query16);
            }
        }
    }

    public static class DbContextExtensions
    {
        public static IEnumerable<T> Query<T>(this DbContext db, IQueryable<T> query)
        {
            using (var connection = new SqliteConnection(BloggingContext.ConnectionString))
            {
                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();
                Console.WriteLine(sql);
                
                try
                {
                    var results = connection.Query(sql);
                    return results.OfType<T>();
                }
                catch (Exception e)
                {
                    Console.WriteLine("NOT WORKING!!");
                    Console.WriteLine(e);
                    return null;
                }
            }
        }
    }
}