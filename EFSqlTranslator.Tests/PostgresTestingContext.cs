using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EFSqlTranslator.Tests
{
    public class PostgresTestingContext : TestingContext
    {
        public DbSet<Note> Notes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(ConnectionString);
        }
    }

    public class Note
    {
        public int Id { get; set; }

        //[NotMapped]
        public int[] RelatedIds { get; set; }

        //[NotMapped]
        public string[] Tags { get; set; }

        public string Text { get; set; }
    }
}
