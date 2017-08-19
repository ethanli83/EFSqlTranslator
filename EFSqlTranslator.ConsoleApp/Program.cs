using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Dapper;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation.DbObjects;
using EFSqlTranslator.Translation.DbObjects.PostgresQlObjects;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using EFSqlTranslator.Translation.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EFSqlTranslator.ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            SqlMapper.AddTypeHandler(new GuidTypeHandler());

            using (var db = new BloggingContext())
            {
                if (db.Database.EnsureDeleted() && db.Database.EnsureCreated())
                {
                    UpdateData(db);
                }
            }

            using (var db = new BloggingContext())
            {
                var sql = "";
                try
                {
                    var query = db.Blogs.Select(x => new
                    {
                        UId = x.UserId,
                        Blog = x,
                        UName = x.User.UserName,
                        x.User
                    });

                    var result = db.Query(
                        query,
                        new EFModelInfoProvider(db),
                        new SqliteObjectFactory(),
                        out sql);
                    
                    var json = JsonConvert.SerializeObject(result, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        Formatting = Formatting.Indented
                    });

                    Console.WriteLine("Result:");
                    Console.WriteLine(json);
                }
                finally
                {
                    Console.WriteLine("Sql:");
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
                Title = "Title 1",
                BlogId = 1,
                UserId = 1
            });

            db.Posts.Add(new Post
            {
                PostId = 2,
                Content = "Post 2",
                Title = "Title 2",
                BlogId = 1,
                UserId = 1
            });

            db.Comments.Add(new Comment
            {
                CommentId = 1,
                UserId = 1,
                PostId = 1
            });

            var iid = 1;
            for (var i = 1; i < 2; i++)
            {
                db.Statistics.Add(new Statistic
                {
                    BlogId = i % 3,
                    GuidId = Guid.NewGuid(),
                    ViewCount = 100,
                    FloatVal = 1.1f,
                    DecimalVal = 2.2m,
                    DoubleVal = 3.3d
                });
            }

            for (var i = 0; i < 10; i++)
            {
                var cid = Guid.NewGuid();
                db.Companies.Add(new Company
                {
                    CompanyId = cid,
                    Name = "C_" + i
                });

                for(var c = 0; c < 5; c++)
                {
                    db.Items.Add(new Item
                    {
                        ItemId = iid++,
                        CompanyId = cid,
                        CategoryId = i % 7,
                        Val = i * c / 2m
                    }); 
                }
            }

            db.Domains.Add(new Domain
            {
                DomainId = 1,
                Name = "daydreamer"
            });

            db.Routes.Add(new Route
            {
                RouteId = 1,
                Name = "ethan",
                DomainId = 1
            });

            db.Routes.Add(new Route
            {
                RouteId = 2,
                Name = "xufeng",
                DomainId = 1
            });

            db.SaveChanges();
        }
    }

    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            var inVal = (byte[])value;
            byte[] outVal = { inVal[3], inVal[2], inVal[1], inVal[0], inVal[5], inVal[4], inVal[7], inVal[6], inVal[8], inVal[9], inVal[10], inVal[11], inVal[12], inVal[13], inVal[14], inVal[15] };
            return new Guid(outVal);
        }

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            var inVal = value.ToByteArray();
            byte[] outVal = { inVal[3], inVal[2], inVal[1], inVal[0], inVal[5], inVal[4], inVal[7], inVal[6], inVal[8], inVal[9], inVal[10], inVal[11], inVal[12], inVal[13], inVal[14], inVal[15] };
            parameter.Value = outVal;
        }
    }

    public class MyClass
    {
        public int Id { get; set; }

        public decimal? Val { get; set; }
    }
}