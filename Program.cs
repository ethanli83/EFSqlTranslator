using System;
using Microsoft.Data.Sqlite;
using Dapper;
using System.Linq;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;

namespace EFSqlTranslator
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
                    db.Blogs.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
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
            using (var connection = new SqliteConnection(BloggingContext.ConnectionString))
            {
                var query = db.Blogs.Where(b => b.Url != null);
                var script = EFLinqTranslator.Translate(query.Expression, new ModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();
                var blogs = connection.Query<Blog>(sql);
                
                foreach (var blog in blogs)
                {
                    Console.WriteLine(" - {0}", blog.Url);
                }
            }
        }
    }
}
