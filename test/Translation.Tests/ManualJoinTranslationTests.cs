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
                var query = db.Blogs.Where(b => b.Posts.Any(p => p.User.UserName != null));
                var query1 = db.Posts.
                    Join(
                        query, 
                        (p, b) => p.BlogId == b.BlogId && p.User.UserName == "ethan", 
                        (p, b) => new { PId = p.PostId, b.Name },
                        JoinType.LeftOuter);

                var script = LinqTranslator.Translate(query1.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
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
