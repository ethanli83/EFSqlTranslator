using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using EFSqlTranslator.Translation.Extensions;
using Xunit;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    public class ParameteriseTests
    {
        [Fact]
        public void Test_Parameterise_Constant() 
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.Url == "Ethan");

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());

                var constants = script.Parameterise();
                Assert.Equal(1, constants.Length);
                Assert.Equal("@param0", constants[0].ParamName);
                
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
where b0.Url = @param0";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
        
        [Fact]
        public void Test_Parameterise_Constant_With_Duplicate_Values() 
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.Url == "Ethan" || b.User.UserName == "Ethan");

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());

                var constants = script.Parameterise();
                Assert.Equal(1, constants.Length);
                Assert.Equal("@param0", constants[0].ParamName);
                
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
left outer join Users u0 on b0.UserId = u0.UserId
where (b0.Url = @param0) or (u0.UserName = @param0)";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
        
        [Fact]
        public void Test_Parameterise_Constant_With_List() 
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.BlogId.In(1, 2, 3));

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());

                var constants = script.Parameterise();
                Assert.Equal(1, constants.Length);
                Assert.Equal("@param0", constants[0].ParamName);
                Assert.Equal(new [] {1, 2, 3}, constants[0].Val);
                
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
where b0.BlogId in @param0";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
        
        [Fact]
        public void Test_Parameterise_Constant_With_List2() 
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.BlogId.In(1, 2, 3));

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());

                var constants = script.Parameterise(true);
                Assert.Equal(0, constants.Length);
                
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
where b0.BlogId in (1, 2, 3)";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}