using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using NUnit.Framework;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    [TestFixture]
    public class OrderByTranslationTests
    {
        [Test]
        public void Test_OrderBy()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Url.StartsWith("ethan.com"))
                    .OrderBy(b => b.User.UserName);

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from 'Blogs' b0
left outer join 'Users' u0 on b0.'UserId' = u0.'UserId'
where b0.'Url' like 'ethan.com%'
order by u0.'UserName'";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        public void Test_ThenBy()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Url.StartsWith("ethan.com"))
                    .OrderBy(b => b.User.UserName)
                    .ThenBy(b => b.CommentCount);

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from 'Blogs' b0
left outer join 'Users' u0 on b0.'UserId' = u0.'UserId'
where b0.'Url' like 'ethan.com%'
order by u0.'UserName', b0.'CommentCount'";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        public void Test_OrderByDescending()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Url.StartsWith("ethan.com"))
                    .OrderByDescending(b => b.User.UserName);

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from 'Blogs' b0
left outer join 'Users' u0 on b0.'UserId' = u0.'UserId'
where b0.'Url' like 'ethan.com%'
order by u0.'UserName' desc";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        public void Test_ThenByDescending()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Url.StartsWith("ethan.com"))
                    .OrderBy(b => b.User.UserName)
                    .ThenByDescending(b => b.CommentCount);

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from 'Blogs' b0
left outer join 'Users' u0 on b0.'UserId' = u0.'UserId'
where b0.'Url' like 'ethan.com%'
order by u0.'UserName', b0.'CommentCount' desc";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        public void Test_Mix()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Url.StartsWith("ethan.com"))
                    .OrderBy(b => b.User.UserName)
                    .ThenByDescending(b => b.CommentCount)
                    .ThenBy(b => b.Url);

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from 'Blogs' b0
left outer join 'Users' u0 on b0.'UserId' = u0.'UserId'
where b0.'Url' like 'ethan.com%'
order by u0.'UserName', b0.'CommentCount' desc, b0.'Url'";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        public void Test_OnChildRelation()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Url.StartsWith("ethan.com"))
                    .OrderBy(b => b.Posts.Sum(p => p.LikeCount));

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from 'Blogs' b0
left outer join (
    select p0.'BlogId' as 'BlogId_jk0', sum(p0.'LikeCount') as 'sum0'
    from 'Posts' p0
    group by p0.'BlogId'
) sq0 on b0.'BlogId' = sq0.'BlogId_jk0'
where b0.'Url' like 'ethan.com%'
order by ifnull(sq0.'sum0', 0)";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}