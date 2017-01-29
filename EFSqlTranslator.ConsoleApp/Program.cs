using System;
using System.Linq;
using EFSqlTranslator.EFModels;
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
                    Include(b => b.User);

                string sql;
                var blogs = db.Query(
                    query11,
                    (b, u) =>
                    {
                        b.User = u;
                        return b;
                    },
                    out sql);

                foreach (var item in blogs)
                {
                    Console.WriteLine($"{item.BlogId}, {item.UserId}, {item.Url}, {item.User.UserName}");
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