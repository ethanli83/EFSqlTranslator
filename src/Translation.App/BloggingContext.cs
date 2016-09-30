using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Translation.App
{
    public class BloggingContext : DbContext
    {
        public const string ConnectionString = "Filename=./blog.db";

        public DbSet<Blog> Blogs { get; set; }
        
        public DbSet<Post> Posts { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Comment> Comments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionString);
        }
    }

    public class Blog
    {
        public int BlogId { get; set; }
        
        public string Url { get; set; }
        
        public string Name { get; set; }
        
        public int UserId { get; set; }
        
        public User User { get; set; }
        
        public List<Post> Posts { get; set; }
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

        public List<Comment> Comments { get; set; }
    }

    public class User
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public List<Blog> Blogs { get; set; }

        public List<Post> Posts { get; set; }

        public List<Comment> Comments { get; set; }
    }

    public class Comment
    {
        public int CommentId { get; set; }

        public int UserId { get; set; }

        public int PostId { get; set; }

        public User User { get; set; }

        public Post Post { get; set; }
    }
}