using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.MySqlObjects;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    [TestFixture]
    public class IncludeTranslatorTests
    {
        [Test]
        [TranslationReadMe(
            Index = 0,
            Title = "Count on basic grouping"
        )]
        public void Test_Basic()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts
                    .Where(p => p.Blog.Url != null)
                    .Include(p => p.User);

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
create temporary table if not exists 'Temp_Table_Posts0' as
    select p0.'PostId', p0.'UserId'
    from 'Posts' p0
    inner join 'Blogs' b0 on p0.'BlogId' = b0.'BlogId'
    where b0.'Url' is not null;

select p0.*
from 'Posts' p0
inner join (
    select t0.'PostId', t0.'_rowid_'
    from 'Temp_Table_Posts0' t0
    group by t0.'PostId', t0.'_rowid_'
) s0 on p0.'PostId' = s0.'PostId'
order by s0.'_rowid_';

select u0.*
from 'Users' u0
inner join (
    select t0.'UserId'
    from 'Temp_Table_Posts0' t0
    group by t0.'UserId'
) s0 on u0.'UserId' = s0.'UserId';

drop table if exists 'Temp_Table_Posts0'";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        [TranslationReadMe(
            Index = 1,
            Title = "Count on basic grouping"
        )]
        public void Test_Basic_2()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts
                    .Where(p => p.Blog.Url != null)
                    .Include(p => p.User)
                    .ThenInclude(u => u.Blogs);

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
create temporary table if not exists 'Temp_Table_Posts0' as
    select p0.'PostId', p0.'UserId'
    from 'Posts' p0
    inner join 'Blogs' b0 on p0.'BlogId' = b0.'BlogId'
    where b0.'Url' is not null;

select p0.*
from 'Posts' p0
inner join (
    select t0.'PostId', t0.'_rowid_'
    from 'Temp_Table_Posts0' t0
    group by t0.'PostId', t0.'_rowid_'
) s0 on p0.'PostId' = s0.'PostId'
order by s0.'_rowid_';

create temporary table if not exists 'Temp_Table_Users0' as
    select u0.'UserId'
    from 'Users' u0
    inner join (
        select t0.'UserId'
        from 'Temp_Table_Posts0' t0
        group by t0.'UserId'
    ) s0 on u0.'UserId' = s0.'UserId';

select u0.*
from 'Users' u0
inner join (
    select t0.'UserId', t0.'_rowid_'
    from 'Temp_Table_Users0' t0
    group by t0.'UserId', t0.'_rowid_'
) s0 on u0.'UserId' = s0.'UserId'
order by s0.'_rowid_';

select b0.*
from 'Blogs' b0
inner join (
    select t0.'UserId'
    from 'Temp_Table_Users0' t0
    group by t0.'UserId'
) s0 on b0.'UserId' = s0.'UserId';

drop table if exists 'Temp_Table_Users0';

drop table if exists 'Temp_Table_Posts0'
";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        [TranslationReadMe(
            Index = 1,
            Title = "Count on basic grouping"
        )]
        public void Test_Basic_3()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts
                    .Where(p => p.Blog.Url != null)
                    .Include(p => p.User)
                    .Include(p => p.Blog);

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
create temporary table if not exists 'Temp_Table_Posts0' as
    select p0.'PostId', p0.'UserId', p0.'BlogId'
    from 'Posts' p0
    inner join 'Blogs' b0 on p0.'BlogId' = b0.'BlogId'
    where b0.'Url' is not null;

select p0.*
from 'Posts' p0
inner join (
    select t0.'PostId', t0.'_rowid_'
    from 'Temp_Table_Posts0' t0
    group by t0.'PostId', t0.'_rowid_'
) s0 on p0.'PostId' = s0.'PostId'
order by s0.'_rowid_';

select u0.*
from 'Users' u0
inner join (
    select t0.'UserId'
    from 'Temp_Table_Posts0' t0
    group by t0.'UserId'
) s0 on u0.'UserId' = s0.'UserId';

select b0.*
from 'Blogs' b0
inner join (
    select t0.'BlogId'
    from 'Temp_Table_Posts0' t0
    group by t0.'BlogId'
) s0 on b0.'BlogId' = s0.'BlogId';

drop table if exists 'Temp_Table_Posts0'";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }

        [Test]
        [TranslationReadMe(
            Index = 1,
            Title = "Count on basic grouping"
        )]
        public void Test_Basic_4()
        {
            using (var db = new TestingContext())
            {
                var query = db.Posts
                    .Where(p => p.Blog.Url != null)
                    .Include(p => p.User)
                    .ThenInclude(u => u.Comments)
                    .ThenInclude(c => c.User)
                    .Include(p => p.Blog);

                var script = LinqTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
create temporary table if not exists 'Temp_Table_Posts0' as
    select p0.'PostId', p0.'UserId', p0.'BlogId'
    from 'Posts' p0
    inner join 'Blogs' b0 on p0.'BlogId' = b0.'BlogId'
    where b0.'Url' is not null;

select p0.*
from 'Posts' p0
inner join (
    select t0.'PostId', t0.'_rowid_'
    from 'Temp_Table_Posts0' t0
    group by t0.'PostId', t0.'_rowid_'
) s0 on p0.'PostId' = s0.'PostId'
order by s0.'_rowid_';

create temporary table if not exists 'Temp_Table_Users0' as
    select u0.'UserId'
    from 'Users' u0
    inner join (
        select t0.'UserId'
        from 'Temp_Table_Posts0' t0
        group by t0.'UserId'
    ) s0 on u0.'UserId' = s0.'UserId';
select u0.*

from 'Users' u0
inner join (
    select t0.'UserId', t0.'_rowid_'
    from 'Temp_Table_Users0' t0
    group by t0.'UserId', t0.'_rowid_'
) s0 on u0.'UserId' = s0.'UserId'
order by s0.'_rowid_';

create temporary table if not exists 'Temp_Table_Comments0' as
    select c0.'CommentId', c0.'UserId'
    from 'Comments' c0
    inner join (
        select t0.'UserId'
        from 'Temp_Table_Users0' t0
        group by t0.'UserId'
    ) s0 on c0.'UserId' = s0.'UserId';

select c0.*
from 'Comments' c0
inner join (
    select t0.'CommentId', t0.'_rowid_'
    from 'Temp_Table_Comments0' t0
    group by t0.'CommentId', t0.'_rowid_'
) s0 on c0.'CommentId' = s0.'CommentId'
order by s0.'_rowid_';

select u0.*
from 'Users' u0
inner join (
    select t0.'UserId'
    from 'Temp_Table_Comments0' t0
    group by t0.'UserId'
) s0 on u0.'UserId' = s0.'UserId';

drop table if exists 'Temp_Table_Comments0';

drop table if exists 'Temp_Table_Users0';
select b0.*
from 'Blogs' b0
inner join (
    select t0.'BlogId'
    from 'Temp_Table_Posts0' t0
    group by t0.'BlogId'
) s0 on b0.'BlogId' = s0.'BlogId';
drop table if exists 'Temp_Table_Posts0'";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}