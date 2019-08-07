using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using EFSqlTranslator.Translation.Extensions;
using Xunit;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    public class DistinctCountTranslationTests
    {
        [Fact]
        public void Test_DistinctCount_In_GroupBy()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.
                    Where(p => p.Content != null).
                    GroupBy(p => p.BlogId).
                    Select(g => new { cnt = g.DistinctCount(p => p.UserId) });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select count(distinct p0.UserId) as 'cnt'
from Posts p0
where p0.Content is not null
group by p0.BlogId";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_DistinctCount_On_Child_Relation()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.
                    Where(p => p.Content != null).
                    Select(g => new
                    {
                        g.BlogId,
                        cnt = g.Comments.DistinctCount(c => c.User.UserName)
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select p0.BlogId, count(distinct u0.UserName) as 'cnt', c0.PostId as 'PostId_jk0'
from Comments c0
inner join Users u0 on c0.UserId = u0.UserId
group by c0.PostId, p0.BlogId";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}
