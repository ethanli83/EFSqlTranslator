using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EFSqlTranslator.ConsoleApp
{
    public class BloggingContext : DbContext
    {
        public const string SqliteConnectionString = "Filename=./blog.db";

        public const string SqlConnectionStr = "server=localhost;userid=root;pwd=chenli1234;database=Blogging;port=3306;sslmode=none;";

        public DbSet<Blog> Blogs { get; set; }
        
        public DbSet<Post> Posts { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Statistic> Statistics { get; set; }

        public DbSet<Item> Items { get; set; }

        public DbSet<Company> Companies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(SqliteConnectionString);
            //optionsBuilder.UseMySql(SqlConnectionStr);
        }
    }

    public class Blog
    {
        public int BlogId { get; set; }
        
        public string Url { get; set; }
        
        public string Name { get; set; }
        
        public int UserId { get; set; }
        
        public User User { get; set; }
        
        public List<Post> Posts { get; set; } = new List<Post>();

        public List<Statistic> Statistics { get; set; }
    }

    public class Post
    {
        public int PostId { get; set; }

        public string Title { get; set; }
        
        public string Content { get; set; }
        
        public int BlogId { get; set; }

        public int UserId { get; set; }
        
        public User User { get; set; }

        public Blog Blog { get; set; }

        public List<Comment> Comments { get; set; } = new List<Comment>();
    }

    public class User
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public List<Blog> Blogs { get; set; } = new List<Blog>();

        public List<Post> Posts { get; set; } = new List<Post>();

        public List<Comment> Comments { get; set; } = new List<Comment>();
    }

    public class Comment
    {
        public int CommentId { get; set; }

        public int? UserId { get; set; }

        public int PostId { get; set; }

        public User User { get; set; }

        public Post Post { get; set; }
    }

    public class Statistic
    {
        public int StatisticId { get; set; }

        public int ViewCount { get; set; }

        public float FloatVal { get; set; }

        public decimal? DecimalVal { get; set; }

        public double DoubleVal { get; set; }

        public int BlogId { get; set; }

        public Guid? GuidId { get; set; }

        public virtual Blog Blog { get; set; }
    }

    //[Table(nameof(Item), Schema="fin")]
    public class Item
    {
        public int ItemId { get; set; }

        public Guid CompanyId { get; set; }

        public int CategoryId { get; set; }

        public decimal? Val { get; set; }
    }

    public class Company
    {
        public Guid CompanyId { get; set; }

        public string Name { get; set; }

        public List<Item> Items { get; set; }
    }
}