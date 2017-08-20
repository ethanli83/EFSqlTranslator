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
            UpdateData(this);
            return this;
        }
        
        private static void UpdateData(TestDataContext db)
        {
            db.Database.OpenConnection();
            
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
            
            db.SaveChanges();
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