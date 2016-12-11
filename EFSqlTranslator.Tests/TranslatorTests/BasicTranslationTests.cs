using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;
using NUnit.Framework;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    [TestFixture]
    [CategoryReadMe(
         Index = 0,
         Title = @"Basic Translation",
         Description = @"This section demostrates how the basic linq expression is translated into sql."
     )]
    public class BasicTranslationTests
    {
        [Test]
        [TranslationReadMe(
             Index = 0,
             Title = "Basic filtering on column values in where clause")]
        public void Test_Translate_Filter_On_Simple_Column() 
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.
                    Where(b => b.Url != null && b.Name.StartsWith("Ethan") && (b.UserId > 1 || b.UserId < 100));
                
                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();
                
                const string expected = @"
select b0.*
from Blogs b0
where ((b0.'Url' is not null) and (b0.'Name' like '%Ethan')) and ((b0.'UserId' > 1) or (b0.'UserId' < 100))";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        public void Test_Ues_Left_Join_If_In_Or_Condition()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.
                Where(p => p.User.UserName != null || p.Content != null);
                
                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();
                
                const string expected = @"
select p0.*
from Posts p0
left outer join Users u0 on p0.'UserId' = u0.'UserId'
where (u0.'UserName' is not null) or (p0.'Content' is not null)";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}