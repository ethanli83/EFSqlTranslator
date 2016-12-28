using System;
using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;
using NUnit.Framework;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    [TestFixture]
    [CategoryReadMe(
         Index = 1,
         Title = "Translating Relationsheips",
         Description = @"
In this section, we will show you how relationships are translated. The basic rules are:
  1. All relations is translated into a inner join be default.
  2. If a relation is used in a Or binary expression, Select, or Group By then join type will be changed to Left Outer Join.
  3. Parent relation will be a join to the parent entity.
  4. Child relation will be converted into a sub-select, which then got joined to."
     )]
    public class RelationTranslationTests
    {
        [Test]
        [TranslationReadMe(
             Index = 0,
             Title = "Join to a parent relation"
         )]
        public void Test_Translate_Filter_On_Parent_Relation() 
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts.Where(p => p.Blog.Url != null);

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select p0.*
from Posts p0
inner join Blogs b0 on p0.'BlogId' = b0.'BlogId'
where b0.'Url' is not null ";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        [TranslationReadMe(
             Index = 1,
             Title = "Join to a child relation"
         )]
        public void Test_Translate_Filter_On_Child_Relation()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.Posts.Any(p => p.Content != null));

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
left outer join (
    select p0.'BlogId' as 'BlogId_jk0'
    from Posts p0
    where p0.'Content' is not null
    group by p0.'BlogId'
) sq0 on b0.'BlogId' = sq0.'BlogId_jk0'
where sq0.'BlogId_jk0' is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        [TranslationReadMe(
             Index = 2,
             Title = "Use relationships in a chain"
         )]
        public void Test_Translate_Filter_On_Multi_Level_Relation() 
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs.Where(b => b.User.Comments.Any(c => c.Post.Content != null));

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
select b0.*
from Blogs b0
inner join Users u0 on b0.'UserId' = u0.'UserId'
left outer join (
    select c0.'UserId' as 'UserId_jk0'
    from Comments c0
    inner join Posts p0 on c0.'PostId' = p0.'PostId'
    where p0.'Content' is not null
    group by c0.'UserId'
) sq0 on u0.'UserId' = sq0.'UserId_jk0'
where sq0.'UserId_jk0' is not null";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}
