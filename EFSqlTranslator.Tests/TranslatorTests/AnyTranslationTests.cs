using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using Xunit;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    public class AnyTranslationTests
    {
        [Fact]
        public void Test_Any_With_Predicate()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.Posts.Any(p => p.Content != null));

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
left outer join (
    select p0.BlogId as 'BlogId_jk0'
    from Posts p0
    where p0.Content is not null
    group by p0.BlogId
) sq0 on b0.BlogId = sq0.BlogId_jk0
where sq0.BlogId_jk0 is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Any_Without_Predicate()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.Posts.Any());

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
left outer join (
    select p0.BlogId as 'BlogId_jk0'
    from Posts p0
    group by p0.BlogId
) sq0 on b0.BlogId = sq0.BlogId_jk0
where sq0.BlogId_jk0 is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
        
        [Fact]
        public void Test_Any_With_Boolean()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.Where(b => b.Comments.Any(x => x.IsDeleted));

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select p0.*
from Posts p0
left outer join (
    select c0.PostId as 'PostId_jk0'
    from Comments c0
    where c0.IsDeleted = 1
    group by c0.PostId
) sq0 on p0.PostId = sq0.PostId_jk0
where sq0.PostId_jk0 is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Any_With_InversedBoolean()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.Where(b => b.Comments.Any(x => !x.IsDeleted));

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select p0.*
from Posts p0
left outer join (
    select c0.PostId as 'PostId_jk0'
    from Comments c0
    where c0.IsDeleted != 1
    group by c0.PostId
) sq0 on p0.PostId = sq0.PostId_jk0
where sq0.PostId_jk0 is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}