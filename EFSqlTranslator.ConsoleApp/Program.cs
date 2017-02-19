using System;
using System.Collections.Generic;
using System.Linq;
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
                db.SaveChanges();

                var query11 = db.Blogs.
                    Where(b => b.User.UserName.StartsWith("ethan")).
                    Include(b => b.User).
                    ThenInclude(u => u.Comments).
                    Include(b => b.Posts).
                    ThenInclude(p => p.Comments);

                var g = IncludeGraphBuilder.Build(query11.Expression);

                var sql = "";
                try
                {
                    var blogs = db.Query(
                        query11,
                        new EFModelInfoProvider(db),
                        new SqliteObjectFactory(),
                        out sql);

                    foreach (var item in blogs)
                    {
                        Console.WriteLine($"{item.BlogId}, {item.UserId}, {item.Url}"); //, {item.User.UserName}
                    }
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
        }
    }
}