using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using EFSqlTranslator.Translation.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFSqlTranslator.Tests.QueryResultTests
{
    public class FillingEntityAsPropertyTests
    {
        [Fact]
        public void Test_Fill_Entity_With_Same_Property_Name()
        {
            using (var db = new TestDataContext().WithData())
            {
                var query = db.Blogs
                    .Where(b => b.BlogId == 1)
                    .Select(b => new
                    {
                        UserName = "Nooo~",
                        b.User
                    });

                var result = db.Query(
                    query,
                    new EFModelInfoProvider(db),
                    new SqliteObjectFactory());

                Assert.Equal("Ethan Li", result.Single().User.UserName);
                
                db.Database.CloseConnection();
            }
        }
        
        [Fact]
        public void Test_Fill_Entity_With_Column_Not_In_Alphabetical_Order()
        {
            using (var db = new TestDataContext().WithData())
            {
                var query = db.Posts
                    .Where(p => p.PostId == 1)
                    .Select(x => new
                    {
                        x.User.UserId,
                        Title = "No",
                        Post = x,
                        x.Blog.BlogId,
                        x.PostId
                    });

                var result = db.Query(
                    query,
                    new EFModelInfoProvider(db),
                    new SqliteObjectFactory());

                var post = result.Single();
                
                Assert.Equal(1, post.PostId);
                Assert.Equal(1, post.Post.PostId);
                
                Assert.Equal("No", post.Title);
                Assert.Equal("Title 1", post.Post.Title);
                
                db.Database.CloseConnection();
            }
        }
    }
}