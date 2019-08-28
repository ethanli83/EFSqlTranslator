using System.Linq;

using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.PostgresQlObjects;

using Xunit;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    public class ArrayTranslationTest
    {
        [Fact]
        public void Test_Contains_In_Int_Array()
        {
            using (var db = new PostgresTestingContext())
            {
                var query = db.Notes.Where(n => n.RelatedIds.Contains(10));

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new PostgresQlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select n0.*
from public.""Notes"" n0
where 10 = any(n0.""RelatedIds"")";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Contains_In_String_Array()
        {
            using (var db = new PostgresTestingContext())
            {
                var query = db.Notes
                    .Where(n => n.Tags.Contains("news"))
                    .Select(n => n.Tags);

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new PostgresQlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select n0.""Tags""
from public.""Notes"" n0
where 'news' = any(n0.""Tags"")";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}
