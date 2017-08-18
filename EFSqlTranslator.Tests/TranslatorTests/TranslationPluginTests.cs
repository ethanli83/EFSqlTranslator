using System.Linq;
using System.Linq.Expressions;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using EFSqlTranslator.Translation.MethodTranslators;
using Xunit;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    public class TranslationPluginTests
    {
        [Fact]
        public void Add_Support_To_Custom_Extension()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.BlogId.MyFunc(1));

                var infoProvider = new EFModelInfoProvider(db);
                var factory = new SqliteObjectFactory();
                var script = QueryTranslator.Translate(
                    query.Expression, infoProvider, factory,
                    new []{ new MyFuncTranslator(infoProvider, factory) });
                
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
where MyFunc(b0.BlogId, 1) = 1";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
    
    public class MyFuncTranslator : AbstractMethodTranslator
    {
        public MyFuncTranslator(IModelInfoProvider infoProvider, IDbObjectFactory dbFactory) 
            : base(infoProvider, dbFactory)
        {
        }

        public override void Register(TranslationPlugIns plugIns)
        {
            plugIns.RegisterMethodTranslator("MyFunc", this);
        }

        public override void Translate(
            MethodCallExpression m, TranslationState state, UniqueNameGenerator nameGenerator)
        {
            var dbConstants = (IDbConstant)state.ResultStack.Pop();
            var dbExpression = (IDbSelectable)state.ResultStack.Pop();
            var dbBinary = _dbFactory.BuildFunc("MyFunc", false, dbExpression, dbConstants);

            state.ResultStack.Push(dbBinary);
        }
    }

    public static class MyMethodExtension
    {
        public static bool MyFunc<T>(this T num, T p)
        {
            return true;
        }
    }
}