using System;
using System.Linq;
using Xunit;
using Translation;
using Translation.DbObjects.SqlObjects;
using Translation.EF;

namespace Translation.Tests
{
    public class ManualTranslationTests
    {
        [Fact]
        public void Test_Translate_Join_Select_Columns() 
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.User.Comments.Any(c => c.Post.Content != null));

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select q0.*
from (
    select p0.'PostId' as 'PId', q0.'BlogId' as 'BlogId'
    from Posts p0
    inner join (
        select b0.'BlogId', b0.'BlogId', u0.'UserName'
        from Blogs b0
        inner join Users u0 on b0.'UserId' = u0.'UserId'
        where b0.'Url' != null
    ) q0 on p0.'BlogId' = q0.'BlogId' and q0.'UserName' = 'ethan'
) q0";

                TestUtils.AssertStringEqual(expected, sql);                
            }
        }

        public void Test_Translate_Join_Select_Table() 
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.User.Comments.Any(c => c.Post.Content != null));

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select q0.*
from (
    select p0.'PostId' as 'PId', q0.'BlogId' as 'BlogId'
    from Posts p0
    inner join (
        select b0.'BlogId', b0.'BlogId', u0.'UserName'
        from Blogs b0
        inner join Users u0 on b0.'UserId' = u0.'UserId'
        where b0.'Url' != null
    ) q0 on p0.'BlogId' = q0.'BlogId' and q0.'UserName' = 'ethan'
) q0";

                TestUtils.AssertStringEqual(expected, sql);                
            }
        }
    }
}
