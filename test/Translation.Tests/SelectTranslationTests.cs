using System;
using System.Linq;
using Xunit;
using Translation;
using Translation.DbObjects.SqlObjects;
using Translation.EF;

namespace Translation.Tests
{
    public class SelectTranslationTests
    {
        [Fact]
        public void Test_Translate_Select_Columns() 
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.
                    Where(p => p.User.UserName != null).
                    Select(p => new { p.Content, p.Blog.User.UserName });
                
                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();
                
                const string expected = @"
select p0.'Content' as 'Content', u1.'UserName' as 'UserName'
from Posts p0
inner join Users u0 on p0.'UserId' = u0.'UserId'
left outer join Blogs b0 on p0.'BlogId' = b0.'BlogId'
left outer join Users u1 on b0.'UserId' = u1.'UserId'
where u0.'UserName' is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        public void Test_Select_Ref_And_Columns()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.
                    Where(p => p.Content != null).
                    Select(p => new { p.Blog, p.User.UserName });
                
                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();
                
                const string expected = @"
select b0.*, u1.'UserName' as 'UserName'
from Posts p0
inner join Users u0 on p0.'UserId' = u0.'UserId'
left outer join Blogs b0 on p0.'BlogId' = b0.'BlogId'
left outer join Users u1 on b0.'UserId' = u1.'UserId'
where p0.'Content' is not null and u0.'UserName' is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        public void Test_Multiple_Select_Calls()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.
                    Where(p => p.Content != null).
                    Select(p => new { p.Blog, p.User.UserName }).
                    Select(p => new { p.Blog.Url, p.Blog.Name, p.UserName });
                
                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();
                
                const string expected = @"
select sq0.'Url' as 'Url', sq0.'Name' as 'Name', sq0.'UserName' as 'UserName'
from (
    select b0.*, u0.'UserName' as 'UserName'
    from Posts p0
    left outer join Blogs b0 on p0.'BlogId' = b0.'BlogId'
    left outer join Users u0 on p0.'UserId' = u0.'UserId'
    where p0.'Content' is not null
) sq0";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        public void Test_Multiple_Select_Calls2()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.
                    Where(p => p.Content != null).
                    Select(p => p.Blog).
                    Select(g => new { g.User, g.Url }).
                    Select(g => new { g.User.UserName, g.Url });
                
                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();
                
                const string expected = @"
select sq0.'UserName' as 'UserName', sq0.'Url' as 'Url'
from (
    select sq0.*, sq0.'Url' as 'Url'
    from (
        select b0.*, u0.*
        from Posts p0
        left outer join Blogs b0 on p0.'BlogId' = b0.'BlogId'
        left outer join Users u0 on b0.'UserId' = u0.'UserId'
        where p0.'Content' is not null
    ) sq0
) sq0";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}
