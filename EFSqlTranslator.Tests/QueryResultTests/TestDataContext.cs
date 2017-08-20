using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EFSqlTranslator.Tests.QueryResultTests
{
    public class TestDataContext : DbContext
    {
        public const string ConnectionString = "Data Source=:memory:";

        public DbSet<Blog> Blogs { get; set; }
        
        public DbSet<Post> Posts { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Comment> Comments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionString);
        }

        public TestDataContext WithData()
        {
            Database.OpenConnection();
            Database.EnsureCreated();
            TestUtil.UpdateData(this);
            return this;
        }
    }

    public class Blog
    {
        public int BlogId { get; set; }
        
        public string Url { get; set; }
        
        public string Name { get; set; }
        
        public int UserId { get; set; }

        public int? CommentCount { get; set; }
        
        public User User { get; set; }

        public List<Post> Posts { get; set; }

        public List<Comment> Comments { get; set; }
    }

    public class Post
    {
        public int PostId { get; set; }
        
        public string Title { get; set; }
        
        public string Content { get; set; }

        public int LikeCount { get; set; }
        
        public int BlogId { get; set; }

        public int UserId { get; set; }
        
        public User User { get; set; }

        public Blog Blog { get; set; }

        public HashSet<Comment> Comments { get; set; }
    }

    public class User
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public HashSet<Blog> Blogs { get; set; }

        public List<Post> Posts { get; set; }

        public List<Comment> Comments { get; set; }
    }

    public class Comment
    {
        public int CommentId { get; set; }

        public int UserId { get; set; }

        public int BlogId { get; set; }

        public int PostId { get; set; }

        public bool IsDeleted { get; set; }

        public bool? IsDeletedNullable { get; set; }

        public User User { get; set; }

        public Blog Blog { get; set; }

        public Post Post { get; set; }
    }
}