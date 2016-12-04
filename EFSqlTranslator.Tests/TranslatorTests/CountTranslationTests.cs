using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;
using NUnit.Framework;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    [TestFixture]
    public class CountTranslationTests
    {
        [Test]
        public void Test_Basic()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.
                    Where(p => p.Content != null).
                    GroupBy(p => p.BlogId).
                    Select(g => new { cnt = g.Count() });

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select count(1) as cnt
from Posts p0
where p0.'Content' is not null
group by p0.'BlogId'";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
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
                        cnt = g.Count(p => p.Blog.Url != null)
                    });

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select p0.'BlogId' as 'BID', count(case
    when b0.'Url' is not null then 1
    else null
end) as cnt
from Posts p0
inner join Blogs b0 on p0.'BlogId' = b0.'BlogId'
where p0.'Content' is not null
group by p0.'BlogId'";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        public void Test_On_Child_Relation()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.
                    Where(b => b.Url != null).
                    Select(b => new { cnt = b.Posts.Count() });

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select isnull(sq0.'count0', 0) as cnt
from Blogs b0
left outer join (
    select p0.'BlogId' as 'BlogId_jk0', count(1) as count0
    from Posts p0
    group by p0.'BlogId'
) sq0 on b0.'BlogId' = sq0.'BlogId_jk0'
where b0.'Url' is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
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

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.'Url', u0.'UserId', isnull(sq0.'count0', 0) as cnt
from Blogs b0
left outer join Users u0 on b0.'UserId' = u0.'UserId'
left outer join (
    select p0.'BlogId' as 'BlogId_jk0', count(case
        when u0.'UserName' is not null then 1
        else null
    end) as count0
    from Posts p0
    inner join Users u0 on p0.'UserId' = u0.'UserId'
    group by p0.'BlogId'
) sq0 on b0.'BlogId' = sq0.'BlogId_jk0'
where b0.'Url' is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
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
                        cnt = g.Count(p => p.User.UserName != null)
                    });

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.'Url', u0.'UserId', count(case
    when u1.'UserName' is not null then 1
    else null
end) as cnt
from Posts p0
left outer join Blogs b0 on p0.'BlogId' = b0.'BlogId'
left outer join Users u0 on b0.'UserId' = u0.'UserId'
inner join Users u1 on p0.'UserId' = u1.'UserId'
where p0.'Content' is not null
group by b0.'BlogId', b0.'Url', u0.'UserId'";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}