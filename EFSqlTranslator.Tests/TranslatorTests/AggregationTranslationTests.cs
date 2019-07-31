using System.Collections.Generic;
using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.Extensions;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;
using Xunit;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    [CategoryReadMe(
         Index = 4,
         Title = "Translating Aggregtaions",
         Description = @"
In this section, we will give you several examples to show how the aggregation is translated.
We will also demostrate few powerful aggregations that you can do with this libary."
     )]
    public class AggregationTranslationTests
    {
        [Fact]
        [TranslationReadMe(
             Index = 0,
             Title = "Count on basic grouping"
         )]
        public void Test_Basic()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.
                    Where(p => p.Content != null).
                    GroupBy(p => p.BlogId).
                    Select(g => new { cnt = g.Count() });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select count(1) as 'cnt'
from Posts p0
where p0.Content is not null
group by p0.BlogId";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        [TranslationReadMe(
             Index = 1,
             Title = "Combine aggregations in selection"
         )]
        public void Test_Basic_Expression()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.
                    Where(p => p.Content != null).
                    GroupBy(p => p.BlogId).
                    Select(g => new
                    {
                        BId = g.Key,
                        cnt = g.Count(),
                        Exp = g.Sum(p => p.User.UserId) * g.Count(p => p.Content.StartsWith("Ethan"))
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select p0.BlogId as 'BId', count(1) as 'cnt', sum(u0.UserId) * count(case
    when p0.Content like 'Ethan%' then 1
    else null
end) as 'Exp'
from Posts p0
inner join Users u0 on p0.UserId = u0.UserId
where p0.Content is not null
group by p0.BlogId";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        [TranslationReadMe(
             Index = 2,
             Title = "Count on basic grouping with condition"
         )]
        public void Test_Basic_With_Condition()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.
                    Where(p => p.Content != null).
                    GroupBy(p => p.BlogId).
                    Select(g => new
                    {
                        BID = g.Key,
                        cnt = g.Count(p => p.Blog.Url != null || p.User.UserId > 0)
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select p0.BlogId as 'BID', count(case
    when (b0.Url is not null) or (u0.UserId > 0) then 1
    else null
end) as 'cnt'
from Posts p0
left outer join Blogs b0 on p0.BlogId = b0.BlogId
left outer join Users u0 on p0.UserId = u0.UserId
where p0.Content is not null
group by p0.BlogId";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        [TranslationReadMe(
             Index = 3,
             Title = "Sum on child relationship"
         )]
        public void Test_On_Child_Relation()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.
                    Where(b => b.Url != null).
                    Select(b => new
                    {
                        b.Name,
                        cnt = b.Posts.Sum(p => p.PostId)
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.Name, coalesce(sq0.sum0, 0) as 'cnt'
from Blogs b0
left outer join (
    select p0.BlogId as 'BlogId_jk0', sum(p0.PostId) as 'sum0'
    from Posts p0
    group by p0.BlogId
) sq0 on b0.BlogId = sq0.BlogId_jk0
where b0.Url is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_On_Children_With_Condition()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.
                    Where(b => b.Url != null).
                    Select(b => new
                    {
                        b.Url,
                        b.User.UserId,
                        cnt = b.Posts.Count(p => p.User.UserName != null)
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.Url, u0.UserId, coalesce(sq0.count0, 0) as 'cnt'
from Blogs b0
left outer join Users u0 on b0.UserId = u0.UserId
left outer join (
    select p0.BlogId as 'BlogId_jk0', count(case
        when u0.UserName is not null then 1
        else null
    end) as 'count0'
    from Posts p0
    inner join Users u0 on p0.UserId = u0.UserId
    group by p0.BlogId
) sq0 on b0.BlogId = sq0.BlogId_jk0
where b0.Url is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        [TranslationReadMe(
             Index = 4,
             Title = "Aggregate after grouping on an entity."
         )]
        public void Test_Group_On_Entity_With_Condition()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.
                    Where(p => p.Content != null).
                    GroupBy(p => new { p.Blog }).
                    Select(g => new
                    {
                        g.Key.Blog.Url,
                        g.Key.Blog.User.UserId,
                        Cnt = g.Key.Blog.Comments.Count(c => c.User.UserName.StartsWith("Ethan"))
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.Url, u0.UserId, count(case
    when u1.UserName like 'Ethan%' then 1
    else null
end) as 'Cnt'
from Posts p0
left outer join Blogs b0 on p0.BlogId = b0.BlogId
left outer join Users u0 on b0.UserId = u0.UserId
left outer join (
    select c0.BlogId as 'BlogId_jk0', c0.UserId as 'UserId_jk0'
    from Comments c0
    group by c0.BlogId, c0.UserId
) sq0 on b0.BlogId = sq0.BlogId_jk0
left outer join Users u1 on sq0.UserId_jk0 = u1.UserId
where p0.Content is not null
group by b0.BlogId, b0.Url, u0.UserId";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_GroupBy_On_Multiple_Entities2()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.
                    Where(p => p.Content != null).
                    Select(p => new { p.Blog, p.User }).
                    GroupBy(x => new { x.Blog }).
                    Select(g => new
                    {
                        g.Key.Blog.Url,
                        g.Key.Blog.User.UserName,
                        Cnt = g.Count(x => x.User.UserName != "ethan")
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select sq0.Url, u0.UserName, count(case
    when sq0.UserName != 'ethan' then 1
    else null
end) as 'Cnt'
from (
    select b0.BlogId, b0.Url, b0.UserId as 'UserId_jk0', u0.UserName
    from Posts p0
    left outer join Blogs b0 on p0.BlogId = b0.BlogId
    left outer join Users u0 on p0.UserId = u0.UserId
    where p0.Content is not null
) sq0
left outer join Users u0 on sq0.UserId_jk0 = u0.UserId
group by sq0.BlogId, sq0.Url, u0.UserName";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Distinc_count()
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