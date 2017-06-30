using System.Linq;
using EFSqlTranslator.EFModels;
using EFSqlTranslator.Translation;
using EFSqlTranslator.Translation.DbObjects.SqliteObjects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFSqlTranslator.Tests.TranslatorTests
{
    public class AnnotationTests
    {
        [Fact]
        public void Test_Column_Name_And_Includes()
        {
            using (var db = new TestingContext())
            {
                var query = db.Domains
                    .Where(d => d.Name.StartsWith("day"))
                    .Include(d => d.Routes.Where(r => r.Name.StartsWith("ethan")));

                var script = QueryTranslator.Translate(query.Expression, new EFModelInfoProvider(db), new SqliteObjectFactory());
                var sql = script.ToString();

                const string expected = @"
create temporary table if not exists Temp_Table_db_domain0 as
    select d0.pk_domain_id
    from db_domain d0
    where d0.domain_name like 'day%';

select sq0.pk_domain_id as 'DomainId', sq0.domain_name as 'Name'
from (
    select d0.*
    from db_domain d0
    inner join (
        select t0.pk_domain_id, t0._rowid_
        from Temp_Table_db_domain0 t0
        group by t0.pk_domain_id, t0._rowid_
    ) s0 on d0.pk_domain_id = s0.pk_domain_id
    order by s0._rowid_
) sq0;

select sq0.pk_route_id as 'RouteId', sq0.fk_domain_id as 'DomainId', sq0.route_name as 'Name'
from (
    select d0.*
    from db_route d0
    inner join (
        select t0.pk_domain_id
        from Temp_Table_db_domain0 t0
        group by t0.pk_domain_id
    ) s0 on d0.fk_domain_id = s0.pk_domain_id
    where d0.route_name like 'ethan%'
) sq0;

drop table if exists Temp_Table_db_domain0";

                TestUtils.AssertStringEqual(expected, sql);
            }
        }
    }
}