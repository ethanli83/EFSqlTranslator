using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    [TestFixture]
    public class IncludeTranslatorTests
    {
        [Test]
        [TranslationReadMe(
            Index = 0,
            Title = "Count on basic grouping"
        )]
        public void Test_Basic()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts
                    .Where(p => p.Blog.Url != null)
                    .Include(p => p.User);

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new MySqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select count(1) as 'cnt'
from 'Posts' p0
where p0.'Content' is not null
group by p0.'BlogId'";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        [TranslationReadMe(
            Index = 1,
            Title = "Count on basic grouping"
        )]
        public void Test_Basic_2()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts
                    .Where(p => p.Blog.Url != null)
                    .Include(p => p.User)
                    .ThenInclude(u => u.Blogs);

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new MySqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select * from posts
into #tempp

select * from posts
join #tempp

select * from users
into #tempu
join #tempp

select * from user
join #tempu

select * fom blogs
join #tempu

";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        [TranslationReadMe(
            Index = 1,
            Title = "Count on basic grouping"
        )]
        public void Test_Basic_3()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts
                    .Where(p => p.Blog.Url != null)
                    .Include(p => p.User)
                    .Include(p => p.Blog);

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new MySqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select count(1) as 'cnt'
from 'Posts' p0
where p0.'Content' is not null
group by p0.'BlogId'";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        [TranslationReadMe(
            Index = 1,
            Title = "Count on basic grouping"
        )]
        public void Test_Basic_4()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts
                    .Where(p => p.Blog.Url != null)
                    .Include(p => p.User)
                    .ThenInclude(u => u.Comments)
                    .ThenInclude(c => c.User)
                    .Include(p => p.Blog)
                    .ThenInclude(b => b.Statistic);

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new MySqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select count(1) as 'cnt'
from 'Posts' p0
where p0.'Content' is not null
group by p0.'BlogId'";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}