using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;
using NUnit.Framework;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    [TestFixture]
    public class BasicTranslationTests
    {
        [Test]
        public void Test_Translate_Filter_On_Simple_Column() 
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.Url != null);
                
                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();
                
                const string expected = @"
select b0.*
from Blogs b0
where b0.'Url' is not null";

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
inner join Users u0 on p0.'UserId' = u0.'UserId'
where u0.'UserName' is not null or p0.'Content' is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}
