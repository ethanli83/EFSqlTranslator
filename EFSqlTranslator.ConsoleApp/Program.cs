using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Config;
using NLog.Targets;

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
                    Select(p => new
                    {
                        Cnt = p.Blog.Posts.Count(bp => bp.User.UserName != null) + p.Comments.Average(c => c.CommentId)
                    });


                var query12 = db.Posts.
                    Where(p => p.Content != null).
                    Select(p => new { p.Blog, p.User }).
                    GroupBy(x => new { x.Blog }).
                    Select(g => new
                    {
                        g.Key.Blog.Url,
                        g.Key.Blog.User.UserName,
                        Cnt = g.Sum(x => x.User.UserId + x.Blog.BlogId)
                    });

                var query13 = db.Posts.
                    Where(p => p.Content != null).
                    Select(p => new { p.Blog, p.User }).
                    GroupBy(x => new { x.Blog }).
                    Select(g => new
                    {
                        g.Key.Blog.Url,
                        g.Key.Blog.User.UserName,
                        Cnt = g.Max(x => x.User.UserId)
                    });

                var query14 = db.Blogs.
                    Where(b => b.Url != null).
                    Select(b => new
                    {
                        b.Url,
                        b.User.UserId,
                        cnt = b.Posts.Min(p => p.PostId)
                    });

                var query15 = db.Blogs.
                    Where(b => b.Url != null).
                    Select(b => new
                    {
                        b.Url,
                        b.User.UserId,
                        cnt = b.Posts.Select(p => p.User.UserId).Average()
                    });

                db.Query(query11);
//                db.Query(query12);
//                db.Query(query13);
//                db.Query(query14);
//                db.Query(query15);
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