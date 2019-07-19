using System;
using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.PostgresQlObjects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFSqlTranslator.Tests.DbSpecificTests
{
    public class PostgresQlTests
    {
        [Fact]
        public void TestStringOperator()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Url != null && b.Name.StartsWith("Ethan"))
                    .Select(b => new
                    {
                        Text = b.Name + "||" + b.Url
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new PostgresQlObjectFactory());
                var sql = script.ToString();
                
                Console.WriteLine(sql);

                const string expected = @"
select (b0.""Name"" || '||') || b0.""Url"" as ""Text""
from public.""Blogs"" b0
where (b0.""Url"" is not null) and (b0.""Name"" like 'Ethan%')";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
        
        [Fact]
        public void TestBooleanType()
        {
            using (var db = new TestingContext())
            {
                var query = db.Comments
                    .Where(c => !c.IsDeleted)
                    .Select(c => new
                    {
                        NotDeleted = !c.IsDeleted
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new PostgresQlObjectFactory());
                var sql = script.ToString();
                
                Console.WriteLine(sql);

                const string expected = @"
select case when c0.""IsDeleted"" != TRUE then TRUE else FALSE end as ""NotDeleted""
from public.""Comments"" c0 where c0.""IsDeleted"" != TRUE";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
        
        [Fact]
        public void TestInclude()
        {
            using (var db = new TestingContext())
            {
                var query =  db.Blogs
                    .Where(b => b.Url != null)
                    .Include(b => b.User)
                    .ThenInclude(u => u.Posts);

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new PostgresQlObjectFactory());
                var sql = script.ToString();
                
                Console.WriteLine(sql);

                const string expected = @"
create temporary table if not exists ""Temp_Table_Blogs0"" as 
    select b0.""BlogId"", b0.""UserId""
    from public.""Blogs"" b0
where b0.""Url"" is not null;

select b0.*
from public.""Blogs"" b0
inner join (
select t0.""BlogId"", t0.""ctid""
from ""Temp_Table_Blogs0"" t0
group by t0.""BlogId"", t0.""ctid""
) s0 on b0.""BlogId"" = s0.""BlogId""
order by s0.""ctid"";

create temporary table if not exists ""Temp_Table_Users0"" as 
select u0.""UserId""
from public.""Users"" u0
inner join (
select t0.""UserId""
from ""Temp_Table_Blogs0"" t0
group by t0.""UserId""
) s0 on u0.""UserId"" = s0.""UserId"";

select u0.*
from public.""Users"" u0
inner join (
select t0.""UserId"", t0.""ctid""
from ""Temp_Table_Users0"" t0
group by t0.""UserId"", t0.""ctid""
) s0 on u0.""UserId"" = s0.""UserId""
order by s0.""ctid"";

select p0.*
from public.""Posts"" p0
inner join (
select t0.""UserId""
from ""Temp_Table_Users0"" t0
group by t0.""UserId""
) s0 on p0.""UserId"" = s0.""UserId"";

drop table if exists ""Temp_Table_Users0"";

drop table if exists ""Temp_Table_Blogs0""";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void TestNestedQuery()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Url != null && b.Name.StartsWith("Ethan"))
                    .GroupBy(b => b.User.UserName)
                    .Select(g => new
                    {
                        User = g.Key,
                        Count = g.Count()
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new PostgresQlObjectFactory());
                var sql = script.ToString();
                
                Console.WriteLine(sql);

                const string expected = @"
select u0.""UserName"" as ""User"", count(1) as ""Count""
from public.""Blogs"" b0
left outer join public.""Users"" u0 on b0.""UserId"" = u0.""UserId""
where (b0.""Url"" is not null) and (b0.""Name"" like 'Ethan%')
group by u0.""UserName""";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Fact]
        public void TestDistinct()
        {
            using (var db = new TestingContext())
            {
                var query = db.Blogs
                    .Where(b => b.Url != null && b.Name.StartsWith("Ethan"))
                    .Select(g => new
                    {
                        User = g.User.UserName,
                        Count = g.Posts.Distinct().Count()
                    });

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new PostgresQlObjectFactory());
                var sql = script.ToString();
                
                Console.WriteLine(sql);

                const string expected = @"
select u0.""UserName"" as ""User"", coalesce(sq0.""count0"", 0) as ""Count""
from public.""Blogs"" b0
left outer join public.""Users"" u0 on b0.""UserId"" = u0.""UserId""
left outer join (
    select distinct p0.""BlogId"" as ""BlogId_jk0"", p0.""PostId"", count(1) as ""count0""
    from public.""Posts"" p0
    group by p0.""BlogId"", p0.""PostId""
) sq0 on b0.""BlogId"" = sq0.""BlogId_jk0""
where (b0.""Url"" is not null) and (b0.""Name"" like 'Ethan%')";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}