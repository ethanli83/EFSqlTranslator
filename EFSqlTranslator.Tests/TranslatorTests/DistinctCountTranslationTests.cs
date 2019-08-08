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
                var query = db.Blogs.
                    Where(b => b.Url != null).
                    Select(b => new
                    {
                        b.Name,
                        cnt = b.Posts.DistinctCount(p => p.PostId)
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.Name, coalesce(sq0.count0, 0) as 'cnt'
from Blogs b0
left outer join (
    select p0.BlogId as 'BlogId_jk0', count(distinct p0.PostId) as 'count0'
    from Posts p0
    group by p0.BlogId
) sq0 on b0.BlogId = sq0.BlogId_jk0
where b0.Url is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}
