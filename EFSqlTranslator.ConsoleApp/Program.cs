using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AgileObjects.ReadableExpressions;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
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
                if (db.Database.EnsureCreated())
                {
                    // Insert hash if created
                    UpdateData(db);
                    db.SaveChanges();
                }
            }

            using (var db = new BloggingContext())
            {
                var query = db.Posts
                    .Where(p => p.Blog.Url != null)
                    .OrderBy(p => p.Comments.Average(c => c.PostId));

                var sql = "";
                try
                {
                    var blogs = db.Query(
                        query,
                        new EFModelInfoProvider(db),
                        new SqliteObjectFactory(),
                        out sql);
                }
                finally
                {
                    Console.WriteLine(sql);
                }
            }
        }

        public static void UpdateData(BloggingContext db)
        {
            db.Users.Add(new User
            {
                UserId = 1,
                UserName = "Ethan Li"
            });

            db.Users.Add(new User
            {
                UserId = 2,
                UserName = "Feng Xu"
            });

            db.Blogs.Add(new Blog
            {
                BlogId = 1,
                Url = "ethan1.com",
                UserId = 1
            });

            db.Blogs.Add(new Blog
            {
                BlogId = 2,
                Url = "ethan2.com",
                UserId = 1
            });

            db.Blogs.Add(new Blog
            {
                BlogId = 3,
                Url = "ethan3.com",
                UserId = 1
            });

            db.Blogs.Add(new Blog
            {
                BlogId = 4,
                Url = "xu1.com",
                UserId = 2
            });

            db.Blogs.Add(new Blog
            {
                BlogId = 5,
                Url = "xu2.com",
                UserId = 2
            });

            db.Posts.Add(new Post
            {
                PostId = 1,
                Content = "Post 1",
                BlogId = 1,
                UserId = 1
            });

            db.Posts.Add(new Post
            {
                PostId = 2,
                Content = "Post 2",
                BlogId = 1,
                UserId = 1
            });
        }
    }
}