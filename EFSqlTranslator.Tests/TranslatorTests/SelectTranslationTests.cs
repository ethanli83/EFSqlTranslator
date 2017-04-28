using System.Diagnostics;
using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using Xunit;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    [CategoryReadMe(
         Index = 2,
         Title = "Translating Select",
         Description = @"
In this section, we will show you multiple ways to select data. You can basically:
  1. Translate an anonymous object by selecting columns from different table.
  2. Do multiple Selects to get the final output.
  3. Use relations in your Select method calls."
     )]
    public class SelectTranslationTests
    {
        [Fact]
        [TranslationReadMe(
             Index = 0,
             Title = "Select out only required columns"
         )]
        public void Test_Translate_Select_Columns() 
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts
                    .Where(p => p.User.UserName != null)
                    .Select(p => new { p.Content, p.Title });
                
                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();
                
                const string expected = @"
select p0.Content, p0.Title
from Posts p0
inner join Users u0 on p0.UserId = u0.UserId
where u0.UserName is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        [TranslationReadMe(
             Index = 1,
             Title = "Select out required columns from related entity"
         )]
        public void Test_Translate_Select_Columns_With_Relations()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts
                    .Where(p => p.User.UserName != null)
                    .Select(p => new { p.Content, p.Blog.User.UserName });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select p0.Content, u1.UserName
from Posts p0
inner join Users u0 on p0.UserId = u0.UserId
left outer join Blogs b0 on p0.BlogId = b0.BlogId
left outer join Users u1 on b0.UserId = u1.UserId
where u0.UserName is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        [TranslationReadMe(
             Index = 2,
             Title = "Translate up selection with columns and expression"
         )]
        public void Test_Translate_Select_Columns_With_Expressions()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts
                    .Where(p => p.Content != null)
                    .Select(p => new
                    {
                        TitleContent = p.Title + "|" + p.Content,
                        Num = p.BlogId / p.User.UserId,
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select (p0.Title + '|') + p0.Content as 'TitleContent', p0.BlogId / u0.UserId as 'Num'
from Posts p0
inner join Users u0 on p0.UserId = u0.UserId
where p0.Content is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Select_Ref_And_Columns()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts
                    .Where(p => p.Content != null)
                    .Select(p => new { p.Blog, p.User.UserName });
                
                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();
                
                const string expected = @"
select b0.*, u0.UserName
from Posts p0
left outer join Blogs b0 on p0.BlogId = b0.BlogId
left outer join Users u0 on p0.UserId = u0.UserId
where p0.Content is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        [TranslationReadMe(
             Index = 3,
             Title = "Multiple selections with selecting whole entity",
             Description = "This will become really useful when combining with Group By."
         )]
        public void Test_Multiple_Select_Calls()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts
                    .Where(p => p.Content != null)
                    .Select(p => new { p.Blog, p.User.UserName })
                    .Select(p => new { p.Blog.Url, p.Blog.Name, p.UserName });
                
                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();
                
                const string expected = @"
select b0.Url, b0.Name, u0.UserName
from Posts p0
left outer join Blogs b0 on p0.BlogId = b0.BlogId
left outer join Users u0 on p0.UserId = u0.UserId
where p0.Content is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Multiple_Select_Calls1()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts
                    .Where(p => p.Content != null)
                    .Select(p => p.Blog)
                    .Select(b => b.Url);

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.Url
from Posts p0
left outer join Blogs b0 on p0.BlogId = b0.BlogId
where p0.Content is not null";

                Trace.WriteLine(sql);
                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Multiple_Select_Calls2()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts
                    .Where(p => p.Content != null)
                    .Select(p => p.Blog)
                    .Select(g => new { g.User, g.Url })
                    .Select(g => new { g.User.UserName, g.Url });
                
                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();
                
                const string expected = @"
select u0.UserName, sq0.Url
from (
    select b0.UserId as 'UserId_jk0', b0.Url
    from Posts p0
    left outer join Blogs b0 on p0.BlogId = b0.BlogId
    where p0.Content is not null
) sq0
left outer join Users u0 on sq0.UserId_jk0 = u0.UserId";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Multiple_Select_Calls_After_Grouping()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts
                    .Where(p => p.Content != null)
                    .Select(p => new { p.Blog })
                    .GroupBy(g => new { g.Blog, g.Blog.Url })
                    .Select(p => new { p.Key.Blog, p.Key.Blog.User, p.Key.Url })
                    .Select(g => new { g.Blog.Name, g.User.UserName, g.Url });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select sq0.Name, u0.UserName, sq0.Url
from (
    select b0.Url, b0.BlogId, b0.UserId as 'UserId_jk0', b0.Name
    from Posts p0
    left outer join Blogs b0 on p0.BlogId = b0.BlogId
    where p0.Content is not null
) sq0
left outer join Users u0 on sq0.UserId_jk0 = u0.UserId
group by sq0.BlogId, sq0.Url, u0.UserId, sq0.Name, u0.UserName";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Multiple_Aggregatons()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Select(b => new
                    {
                        Cnt1 = b.Posts.Count(p => p.LikeCount > 10),
                        Cnt2 = b.Posts.Count(p => p.LikeCount < 50)
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select ifnull(sq0.count0, 0) as 'Cnt1', ifnull(sq0.count1, 0) as 'Cnt2'
from Blogs b0
left outer join (
    select p0.BlogId as 'BlogId_jk0', count(case
        when p0.LikeCount > 10 then 1
        else null
    end) as 'count0', count(case
        when p0.LikeCount < 50 then 1
        else null
    end) as 'count1'
    from Posts p0
    group by p0.BlogId
) sq0 on b0.BlogId = sq0.BlogId_jk0";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}
