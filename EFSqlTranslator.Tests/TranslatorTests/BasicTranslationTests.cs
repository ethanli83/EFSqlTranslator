using System;
using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;
using EFSqlTranslator.Translation.Extensions;
using Xunit;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    [CategoryReadMe(
        Index = 0,
        Title = @"Basic Translation",
        Description = @"This section demostrates how the basic linq expression is translated into sql."
    )]
    public class BasicTranslationTests
    {
        [Fact]
        [TranslationReadMe(
            Index = 0,
            Title = "Basic filtering on column values in where clause")]
        public void Test_Translate_Filter_On_Simple_Column()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Url != null &&
                                b.Name.StartsWith("Ethan") &&
                                (b.UserId > 1 || b.UserId < 100));

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
where ((b0.Url is not null) and (b0.Name like 'Ethan%')) and ((b0.UserId > 1) or (b0.UserId < 100))";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
        
        [Fact]
        [TranslationReadMe(
            Index = 1,
            Title = "Filter result using list of values")]
        public void Test_Translate_Filter_On_Simple_Column_With_Values()
        {
            using (var db = new TestingContext())
            {
                var ids = new[] {2, 3, 5};
                var query = db.Blogs
                    .Where(b => b.BlogId.In(1, 2, 4) && b.BlogId.In(ids));

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
where (b0.BlogId in (1, 2, 4)) and (b0.BlogId in (2, 3, 5))";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
        
        [Fact]
        public void Test_Escape_String()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.Url != null && b.Name.StartsWith("Eth'an"));

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
where (b0.Url is not null) and (b0.Name like 'Eth''an%')";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Translate_Filter_On_Contains()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Url != null &&
                                b.Name.Contains("Ethan") &&
                                (b.UserId > 1 || b.UserId < 100));

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
where ((b0.Url is not null) and (b0.Name like '%Ethan%')) and ((b0.UserId > 1) or (b0.UserId < 100))";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Ues_Left_Join_If_In_Or_Condition()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.Where(p => p.User.UserName != null || p.Content != null);

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select p0.*
from Posts p0
left outer join Users u0 on p0.UserId = u0.UserId
where (u0.UserName is not null) or (p0.Content is not null)";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Nullable_Value_In_Condition()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.CommentCount > 10);

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.* from Blogs b0 where b0.CommentCount > 10";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Query_Unwrapping()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.CommentCount > 10)
                    .Select(b => new { KKK = b.BlogId })
                    .Select(b => new { K = b.KKK });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.BlogId as 'K' from Blogs b0 where b0.CommentCount > 10";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Query_Unwrapping2()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Select(b => b);

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.* from Blogs b0";

                TestUtils.AssertStringEqual(expected, sql);

            }
        }

        [Fact]
        public void Test_Query_SupportValueTypes()
        {
            using (var db = new TestingContext())
            {
                var query = db.Statistics
                    .GroupBy(s => s.BlogId)
                    .Select(g => new
                    {
                        BId = g.Key,
                        FloatVal = g.Sum(s => s.FloatVal),
                        DecimalVal = g.Sum(s => s.DecimalVal),
                        DoubleVal = g.Sum(s => s.DoubleVal)
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                Console.WriteLine(sql);

                const string expected = @"
select s0.BlogId as 'BId', sum(s0.FloatVal) as 'FloatVal', sum(s0.DecimalVal) as 'DecimalVal', sum(s0.DoubleVal) as 'DoubleVal'
from Statistics s0
group by s0.BlogId";

                TestUtils.AssertStringEqual(expected, sql);

            }
        }

        [Fact]
        public void Test_Query_OrderByDateTime()
        {
            using (var db = new TestingContext())
            {
                var query = db.Items
                    .OrderBy(x => x.Timer)
                    ;

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                Console.WriteLine(sql);

                const string expected = @"select i0.* from fin.Item i0 order by i0.Timer";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Query_EqualsVariable()
        {
            using (var db = new TestingContext())
            {
                int q = 3;
                var query = db.Items.Where(x => x.ItemId == q);

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                Console.WriteLine(sql);

                const string expected = @"select i0.* from fin.Item i0 where i0.ItemId = 3";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Query_EqualsVariable_Null()
        {
            using (var db = new TestingContext())
            {
                DateTime? q = null;
                var query = db.Items.Where(x => x.Timer == q);

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                Console.WriteLine(sql);

                const string expected = @"select i0.* from fin.Item i0 where i0.Timer is null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Query_ForDateTime()
        {
            using (var db = new TestingContext())
            {
                var date = new DateTime(2017, 6, 30, 1, 30, 1);
                var query = db.Items.Where(x => x.Timer == date);

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                Console.WriteLine(sql);

                const string expected = @"select i0.* from fin.Item i0 where i0.Timer = '2017-06-30T01:30:01'";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void Test_Query_ForBoolean()
        {
            using (var db = new TestingContext())
            {
                var query = db.Comments
                    .Select(c => new
                    {
                        c.IsDeleted
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                Console.WriteLine(sql);

                const string expected = @"select c0.IsDeleted from Comments c0";

                TestUtils.AssertStringEqual(expected, sql);

            }
        }

        [Fact]
        public void Test_Query_FilterOnBoolean()
        {
            using (var db = new TestingContext())
            {
                var query = db.Comments
                    .Where(x => x.IsDeleted)
                    .Select(c => new
                    {
                        c.IsDeleted
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                Console.WriteLine(sql);

                const string expected = @"select c0.IsDeleted from Comments c0 where c0.IsDeleted = 1";

                TestUtils.AssertStringEqual(expected, sql);

            }
        }
        
        [Fact]
        public void Test_Query_FilterOnBoolean2()
        {
            using (var db = new TestingContext())
            {
                var query = db.Comments
                    .Where(x => x.IsDeleted == false)
                    .Select(c => new
                    {
                        c.IsDeleted
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                Console.WriteLine(sql);

                const string expected = @"select c0.IsDeleted from Comments c0 where c0.IsDeleted = 0";

                TestUtils.AssertStringEqual(expected, sql);

            }
        }
        
        [Fact]
        public void Test_Query_FilterOnBoolean3()
        {
            using (var db = new TestingContext())
            {
                var query = db.Comments
                    .Where(x => !x.IsDeleted)
                    .Select(c => new
                    {
                        c.IsDeleted
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                Console.WriteLine(sql);

                const string expected = @"select c0.IsDeleted from Comments c0 where c0.IsDeleted != 1";

                TestUtils.AssertStringEqual(expected, sql);

            }
        }

        [Fact]
        public void Test_Query_FilterOnNullableBoolean()
        {
            using (var db = new TestingContext())
            {
                var query = db.Comments
                    .Where(x => x.IsDeletedNullable == false)
                    .Select(c => new
                    {
                        c.IsDeletedNullable
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                Console.WriteLine(sql);

                const string expected = @"select c0.IsDeletedNullable from Comments c0 where c0.IsDeletedNullable = 0";

                TestUtils.AssertStringEqual(expected, sql);

            }
        }

        [Fact]
        public void Test_Query_FilterOnNullableBooleanEqualsNull()
        {
            using (var db = new TestingContext())
            {
                var query = db.Comments
                    .Where(x => x.IsDeletedNullable == null)
                    .Select(c => new
                    {
                        c.IsDeletedNullable
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                Console.WriteLine(sql);

                const string expected = @"select c0.IsDeletedNullable from Comments c0 where c0.IsDeletedNullable is null";

                TestUtils.AssertStringEqual(expected, sql);

            }
        }

        [Fact]
        public void Test_Query_DifferentSchema()
        {
            using (var db = new TestingContext())
            {
                var query = db.Items
                    .GroupBy(i => i.CategoryId)
                    .Select(g => new
                    {
                        CategoryId = g.Key,
                        Sum = g.Sum(i => i.Value)
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select i0.CategoryId, sum(i0.Value) as 'Sum'
from fin.Item i0
group by i0.CategoryId";

                TestUtils.AssertStringEqual(expected, sql);

            }
        }

        [Fact]
        public void Test_Query_Fill_Class()
        {
            using (var db = new TestingContext())
            {
                var query = db.Items
                    .GroupBy(i => i.CategoryId)
                    .Select(g => new MyClass
                    {
                        Id = g.Key,
                        Val = g.Sum(i => i.Value)
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqlObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select i0.CategoryId as 'Id', sum(i0.Value) as 'Val'
from fin.Item i0
group by i0.CategoryId";

                TestUtils.AssertStringEqual(expected, sql);

            }
        }
    }

    public class MyClass
    {
        public int Id { get; set; }

        public decimal? Val { get; set; }
    }
}