using System;
using Microsoft.Data.Sqlite;
using Dapper;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;
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
                if (!db.Blogs.Any())
                {
                    var user = db.Users.Add(new User { UserName = "Ethan" });
                    db.Blogs.Add(new Blog { Url = "http://blogs.msdn.com/adonet", UserId = user.Entity.UserId });
                    var count = db.SaveChanges();
                    Console.WriteLine("{0} records saved to database", count);
                    Console.WriteLine();
                }

                Console.WriteLine("All blogs in database:");
                foreach (var blog in db.Blogs)
                {
                    Console.WriteLine(" - {0}", blog.Url);
                }
            }

            using (var db = new BloggingContext())
            {
                var query = db.Blogs.Where(b => b.Url != null);
                var blogs = db.Query(query);

                var query2 = db.Posts.Where(p => p.Blog.Url != null);
                var posts = db.Query(query2);

                var query3 = db.Blogs.Where(b => b.Posts.Any(p => p.Content != null));
                var r3 = db.Query(query3);

                var query4 = db.Blogs.Where(b => b.User.Comments.Any(c => c.Post.Content != null));
                var r4 = db.Query(query4);
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

    public static class QueryableExtensions
    {
        public static IQueryable<TResult> Join<TOuter, TInner, TKey, TResult>(
            this IQueryable<TOuter> outer,
            IQueryable<TInner> inner,
            Expression<Func<TOuter, TInner, TResult>> joinCondition,
            Expression<Func<TOuter, TInner, TResult>> resultSelector,
            JoinType joinType = JoinType.Inner
        )
        {   
            return outer.Provider.CreateQuery(resultSelector) as IQueryable<TResult>;
        }
    }
}
