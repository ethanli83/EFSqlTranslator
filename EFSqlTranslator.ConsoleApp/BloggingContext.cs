using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EFSqlTranslator.ConsoleApp
{
    public class BloggingContext : DbContext
    {
        public const string SqliteConnectionString = "Filename=./blog.db";

        public const string SqlConnectionStr = @"server=localhost;userid=root;pwd=chenli1234;database=Blogging;port=3306;sslmode=none;";

        public const string SqlServerConnectionStr = @"Data Source=localhost\sqlexpress;Initial Catalog=BloggingContext;Integrated Security=True;";
        
        public const string PostgresQlConnectionStr = "Host=localhost;Database=db;Username=postgres;Password=mysecretpassword";

        public DbSet<Blog> Blogs { get; set; }
        
        public DbSet<Post> Posts { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Statistic> Statistics { get; set; }

        public DbSet<Item> Items { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Domain> Domains { get; set; }

        public DbSet<Route> Routes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(SqlServerConnectionStr);
            optionsBuilder.UseSqlite(SqliteConnectionString);
            //optionsBuilder.UseMySql(SqlConnectionStr);
            //optionsBuilder.UseNpgsql(PostgresQlConnectionStr);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }

    public class Blog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
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
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PostId { get; set; }

        public string Title { get; set; }
        
        public string Content { get; set; }
        
        public int BlogId { get; set; }

        public int UserId { get; set; }
        
        public int LikeCount { get; set; }
        
        public short LikeCountShort { get; set; }
        
        public long LikeCountLong { get; set; }
        
        public decimal LikeCountDecimal { get; set; }
        
        public User User { get; set; }

        public Blog Blog { get; set; }

        public List<Comment> Comments { get; set; } = new List<Comment>();
    }

    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }

        public string UserName { get; set; }

        public List<Blog> Blogs { get; set; } = new List<Blog>();

        public List<Post> Posts { get; set; } = new List<Post>();

        public List<Comment> Comments { get; set; } = new List<Comment>();
    }

    public class Comment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CommentId { get; set; }

        public int? UserId { get; set; }

        public bool IsDeleted { get; set; }

        public int PostId { get; set; }

        public User User { get; set; }

        public Post Post { get; set; }
    }

    public class Statistic
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
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
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
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

    [Table("db_domain")]
    public class Domain
    {
        [Column("pk_domain_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DomainId { get; set; }

        [Column("domain_name")]
        public string Name { get; set; }

        public List<Route> Routes { get; set; }
    }

    [Table("db_route")]
    public class Route
    {
        [Column("pk_route_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RouteId { get; set; }

        [Column("route_name")]
        public string Name { get; set; }

        [Column("fk_domain_id")]
        public int DomainId { get; set; }

        public Domain Domain { get; set; }
    }
}