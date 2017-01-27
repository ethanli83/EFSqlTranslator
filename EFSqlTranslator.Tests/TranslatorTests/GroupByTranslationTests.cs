using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using NUnit.Framework;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    [TestFixture]
    [CategoryReadMe(
         Index = 3,
         Title = "Translating GroupBy",
         Description = @"
Grouping is always used along with aggregations. In this section, we will demostrate number of
ways that you can group your data. In the next section, you will then see how the group by works
with aggregation methods."
     )]
    public class GroupByTranslationTests
    {
        [Test]
        [TranslationReadMe(
             Index = 0,
             Title = "Basic grouping on table column"
         )]
        public void Test_GroupBy_On_Column()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.
                    Where(p => p.Content != null).
                    GroupBy(p => p.BlogId).
                    Select(g => new { g.Key });

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select p0.'BlogId' as 'Key'
from Posts p0
where p0.'Content' is not null
group by p0.'BlogId'";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        [TranslationReadMe(
             Index = 1,
             Title = "Using relationships in grouping"
         )]
        public void Test_GroupBy_On_Multiple_Columns()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.
                    Where(p => p.Content != null).
                    GroupBy(p => new { p.Blog.Url, p.User.UserName }).
                    Select(g => new { g.Key.Url, g.Key.UserName });

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.'Url', u0.'UserName'
from Posts p0
left outer join Blogs b0 on p0.'BlogId' = b0.'BlogId'
left outer join Users u0 on p0.'UserId' = u0.'UserId'
where p0.'Content' is not null
group by b0.'Url', u0.'UserName'";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        [TranslationReadMe(
             Index = 2,
             Title = "Group on whole entity",
             Description = @"
This feature allows developers to write sophisticated aggregtion in a much simplier way."
         )]
        public void Test_GroupBy_On_Entity()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.
                    Where(p => p.Content != null).
                    GroupBy(p => new { p.Blog }).
                    Select(g => new { g.Key.Blog.User.UserId });

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select u0.'UserId'
from Posts p0
left outer join Blogs b0 on p0.'BlogId' = b0.'BlogId'
left outer join Users u0 on b0.'UserId' = u0.'UserId'
where p0.'Content' is not null
group by b0.'BlogId', u0.'UserId'";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        [TranslationReadMe(
             Index = 3,
             Title = "Mix of Select and Group method calls"
         )]
        public void Test_GroupBy_On_Multiple_Entities()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.
                    Where(p => p.Content != null).
                    Select(p => new { p.Blog, p.User }).
                    GroupBy(x => new { x.Blog }).
                    Select(x => new { x.Key.Blog.Url, x.Key.Blog.User.UserName });

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select sq0.'Url', u0.'UserName'
from (
    select b0.'BlogId', b0.'Url', b0.'UserId' as 'UserId_jk0'
    from Posts p0
    left outer join Blogs b0 on p0.'BlogId' = b0.'BlogId'
    left outer join Users u0 on p0.'UserId' = u0.'UserId'
    where p0.'Content' is not null
) sq0
left outer join Users u0 on sq0.'UserId_jk0' = u0.'UserId'
group by sq0.'BlogId', sq0.'Url', u0.'UserName'";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        public void Test_GroupBy_On_Aggregation()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.
                    Where(b => b.Url != null).
                    GroupBy(b => new { Cnt = b.Posts.Count() }).
                    Select(x => new { x.Key.Cnt });

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select ifnull(sq0.'count0', 0) as 'Cnt'
from Blogs b0
left outer join (
    select p0.'BlogId' as 'BlogId_jk0', count(1) as 'count0'
    from Posts p0
    group by p0.'BlogId'
) sq0 on b0.'BlogId' = sq0.'BlogId_jk0'
where b0.'Url' is not null
group by ifnull(sq0.'count0', 0)";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        public void Test_GroupBy_On_Aggregation2()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.
                    Where(b => b.Url != null).
                    GroupBy(b => b.Posts.Count()).
                    Select(x => new { x.Key, Sum = x.Sum(b => b.CommentCount) });

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select ifnull(sq0.'count0', 0) as 'Key', sum(b0.'CommentCount') as 'Sum'
from Blogs b0
left outer join (
    select p0.'BlogId' as 'BlogId_jk0', count(1) as 'count0'
    from Posts p0
    group by p0.'BlogId'
) sq0 on b0.'BlogId' = sq0.'BlogId_jk0'
where b0.'Url' is not null
group by ifnull(sq0.'count0', 0)";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        public void Test_GroupBy_On_Aggregation4()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.
                    Where(b => b.Url != null).
                    GroupBy(b => b.Posts.Count()).
                    Select(x => new { x.Key, Sum = x.Sum(b => b.Comments.Count()) });

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select ifnull(sq0.'count0', 0) as 'Key', sum(ifnull(sq1.'count1', 0)) as 'Sum'
from Blogs b0
left outer join (
    select p0.'BlogId' as 'BlogId_jk0', count(1) as 'count0'
    from Posts p0
    group by p0.'BlogId'
) sq0 on b0.'BlogId' = sq0.'BlogId_jk0'
left outer join (
    select c0.'BlogId' as 'BlogId_jk0', count(1) as 'count1'
    from Comments c0
    group by c0.'BlogId'
) sq1 on b0.'BlogId' = sq1.'BlogId_jk0'
where b0.'Url' is not null
group by ifnull(sq0.'count0', 0)";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        [TranslationReadMe(
            Index = 4,
            Title = "Group On Aggregation"
        )]
        public void Test_GroupBy_On_Aggregation3()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.
                    Where(b => b.Url != null).
                    GroupBy(b => new { Cnt = b.Posts.Count(), Avg = b.Posts.Average(p => p.LikeCount) }).
                    Select(x => new { x.Key.Cnt, x.Key.Avg, CommentCount = x.Sum(b => b.CommentCount) });

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select ifnull(sq0.'count0', 0) as 'Cnt', ifnull(sq1.'avg0', 0) as 'Avg', sum(b0.'CommentCount') as 'CommentCount'
from Blogs b0
left outer join (
    select p0.'BlogId' as 'BlogId_jk0', count(1) as 'count0', avg(p0.'LikeCount') as 'avg0'
    from Posts p0
    group by p0.'BlogId'
) sq0 on b0.'BlogId' = sq0.'BlogId_jk0'
where b0.'Url' is not null
group by ifnull(sq0.'count0', 0), ifnull(sq1.'avg0', 0)";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        public void Test_GroupBy_On_Aggregation5()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.
                    Where(b => b.Url != null).
                    GroupBy(b => new
                    {
                        Cnt = b.Posts.Where(p => p.LikeCount > 10).Count(p => p.LikeCount < 1000),
                        Avg = b.Posts.Average(p => p.LikeCount)
                    }).
                    Select(x => new
                    {
                        x.Key.Cnt,
                        x.Key.Avg,
                        CommentCount = x.Sum(b => b.CommentCount)
                    });

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select ifnull(sq0.'count0', 0) as 'Cnt', ifnull(sq1.'avg0', 0) as 'Avg', sum(b0.'CommentCount') as 'CommentCount'
from Blogs b0
left outer join (
    select p0.'BlogId' as 'BlogId_jk0', count(case
        when p0.'LikeCount' < 1000 then 1
        else null
    end) as 'count0'
    from Posts p0
    where p0.'LikeCount' > 10
    group by p0.'BlogId'
) sq0 on b0.'BlogId' = sq0.'BlogId_jk0'
left outer join (
    select p0.'BlogId' as 'BlogId_jk0', avg(p0.'LikeCount') as 'avg0'
    from Posts p0
    group by p0.'BlogId'
) sq1 on b0.'BlogId' = sq1.'BlogId_jk0'
where b0.'Url' is not null
group by ifnull(sq0.'count0', 0), ifnull(sq1.'avg0', 0)";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}