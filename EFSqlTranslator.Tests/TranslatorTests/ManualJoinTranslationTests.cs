using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;
using NUnit.Framework;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    [TestFixture]
    [CategoryReadMe(
         Index = 5,
         Title = "Translating manual join",
         Description = @"
This libary supports more complicated join. You can define your own join condition rather than
have to be limited to column pairs."
     )]
    public class ManualTranslationTests
    {
        [Test]
        [TranslationReadMe(
             Index = 0,
             Title = "Join on custom condition"
         )]
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
select p0.'PostId' as 'PId', sq0.'Name'
from Posts p0
inner join Users u0 on p0.'UserId' = u0.'UserId'
left outer join (
    select b0.'Name', b0.'BlogId' as 'BlogId_jk0'
    from Blogs b0
    left outer join (
        select p0.'BlogId' as 'BlogId_jk0'
        from Posts p0
        inner join Users u0 on p0.'UserId' = u0.'UserId'
        where u0.'UserName' is not null
        group by p0.'BlogId'
    ) sq0 on b0.'BlogId' = sq0.'BlogId_jk0'
    where sq0.'BlogId_jk0' is not null
) sq0 on (p0.'BlogId' = sq0.'BlogId_jk0') and (u0.'UserName' = 'ethan')";

                TestUtils.AssertStringEqual(expected, sql);                
            }
        }
    }
}
