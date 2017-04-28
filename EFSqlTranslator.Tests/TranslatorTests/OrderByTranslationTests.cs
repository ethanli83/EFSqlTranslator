using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using Xunit;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    [CategoryReadMe(
        Index = 5,
        Title = "Translating OrderBys",
        Description = @"This section demostrates how the OrderBy is translated into sql."
    )]
    public class OrderByTranslationTests
    {
        [Fact]
        [TranslationReadMe(
            Index = 0,
            Title = "OrderBy on normal column")]
        public void Test_OrderBy()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Url.StartsWith("ethan.com"))
                    .OrderBy(b => b.User.UserName);

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
left outer join Users u0 on b0.UserId = u0.UserId
where b0.Url like 'ethan.com%'
order by u0.UserName";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_ThenBy()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Url.StartsWith("ethan.com"))
                    .OrderBy(b => b.User.UserName)
                    .ThenBy(b => b.CommentCount);

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
left outer join Users u0 on b0.UserId = u0.UserId
where b0.Url like 'ethan.com%'
order by u0.UserName, b0.CommentCount";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_OrderByDescending()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Url.StartsWith("ethan.com"))
                    .OrderByDescending(b => b.User.UserName);

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
left outer join Users u0 on b0.UserId = u0.UserId
where b0.Url like 'ethan.com%'
order by u0.UserName desc";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        [TranslationReadMe(
            Index = 1,
            Title = "OrderBy with different direction")]
        public void Test_ThenByDescending()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Url.StartsWith("ethan.com"))
                    .OrderBy(b => b.User.UserName)
                    .ThenByDescending(b => b.CommentCount);

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
left outer join Users u0 on b0.UserId = u0.UserId
where b0.Url like 'ethan.com%'
order by u0.UserName, b0.CommentCount desc";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Mix()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Url.StartsWith("ethan.com"))
                    .OrderBy(b => b.User.UserName)
                    .ThenByDescending(b => b.CommentCount)
                    .ThenBy(b => b.Url);

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
left outer join Users u0 on b0.UserId = u0.UserId
where b0.Url like 'ethan.com%'
order by u0.UserName, b0.CommentCount desc, b0.Url";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_OnChildRelation()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Url.StartsWith("ethan.com"))
                    .OrderBy(b => b.Posts.Sum(p => p.LikeCount));

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
left outer join (
    select p0.BlogId as 'BlogId_jk0', sum(p0.LikeCount) as 'sum0'
    from Posts p0
    group by p0.BlogId
) sq0 on b0.BlogId = sq0.BlogId_jk0
where b0.Url like 'ethan.com%'
order by ifnull(sq0.sum0, 0)";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}