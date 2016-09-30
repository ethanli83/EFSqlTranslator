using System;
using System.Linq;
using Xunit;
using Translation;
using Translation.DbObjects.SqlObjects;
using Translation.EF;

namespace Translation.Tests
{
    public class BasicTranslationTests
    {
        [Fact]
        public void Test_Translate_Filter_On_Simple_Column() 
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.Url != null);
                
                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();
                
                const string expected = @"
select b.*
from Blogs b
where b.'Url' != null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Translate_Filter_On_Parent_Relation() 
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.Where(p => p.Blog.Url != null);

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select p.*
from Posts p
inner join Blogs b0 on p.'BlogId' = b0.'BlogId'
where b0.'Url' != null ";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Translate_Filter_On_Child_Relation() 
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.Posts.Any(p => p.Content != null));

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b.*
from Blogs b
left outer join (
    select p.'BlogId'
    from Posts p
    where p.'Content' != null
    group by p.'BlogId'
) x0 on b.'BlogId' = x0.'BlogId'
where x0.'BlogId' != null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Translate_Filter_On_Multi_Level_Relation() 
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.User.Comments.Any(c => c.Post.Content != null));

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b.*
from Blogs b
inner join Users u0 on b.'UserId' = u0.'UserId'
left outer join (
    select c.'UserId'
    from Comments c
    inner join Posts p0 on c.'PostId' = p0.'PostId'
    where p0.'Content' != null
    group by c.'UserId'
) x0 on u0.'UserId' = x0.'UserId'
where x0.'UserId' != null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}
