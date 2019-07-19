using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using Xunit;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    public class DistinctTranslationTests
    {
        [Fact]
        public void Test_Distinct_Without_Select()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.BlogId > 0).Distinct();

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select distinct b0.*
from Blogs b0
where b0.BlogId > 0";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Distinct_With_Select()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.BlogId > 0)
                    .Select(b => new 
                    {
                        b.BlogId,
                        Cnt = b.Posts.Count()
                    })
                    .Distinct();

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select distinct b0.BlogId, coalesce(sq0.count0, 0) as 'Cnt'
from Blogs b0
left outer join (
    select p0.BlogId as 'BlogId_jk0', count(1) as 'count0'
    from Posts p0
    group by p0.BlogId
) sq0 on b0.BlogId = sq0.BlogId_jk0
where b0.BlogId > 0";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
        
        [Fact]
        public void Test_Distinct_With_Count()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.BlogId > 0)
                    .Select(b => new 
                    {
                        b.BlogId,
                        Cnt = b.Posts.Distinct().Count()
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.BlogId, coalesce(sq0.count0, 0) as 'Cnt'
from Blogs b0
left outer join (
    select distinct p0.BlogId as 'BlogId_jk0', p0.PostId, count(1) as 'count0'
    from Posts p0
    group by p0.BlogId, p0.PostId
) sq0 on b0.BlogId = sq0.BlogId_jk0
where b0.BlogId > 0";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Distinct_With_Count_And_Select()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.BlogId > 0)
                    .Select(b => new 
                    {
                        b.BlogId,
                        Cnt = b.Posts.Select(p => p.Title).Distinct().Count()
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.BlogId, coalesce(sq0.count0, 0) as 'Cnt'
from Blogs b0
left outer join (
    select distinct sq0.Title, sq0.BlogId_jk0, count(1) as 'count0'
    from (
        select p0.BlogId as 'BlogId_jk0', p0.Title
        from Posts p0
        group by p0.BlogId, p0.Title
    ) sq0
) sq0 on b0.BlogId = sq0.BlogId_jk0
where b0.BlogId > 0";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}