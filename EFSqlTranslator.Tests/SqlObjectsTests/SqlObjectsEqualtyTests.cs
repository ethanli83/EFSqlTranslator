using System.Data;
using EFSqlTranslator.Translation.DbObjects;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;
using Xunit;

namespace EFSqlTranslator.Tests.SqlObjectsTests
{
    public class SqlObjectsEqualtyTests
    {
        [Fact]
        public void Test_Column_Equals()
        {
            var s1 = new SqlColumn
            {
                Name = "a",
                ValType = new DbValType(typeof(int))
                {
                    DbType = DbType.Int32
                },
                Alias = "aaa"
            };

            var s2 = new SqlColumn
            {
                Name = "a",
                ValType = new DbValType(typeof(int))
                {
                    DbType = DbType.Int32
                },
                Alias = "aaa"
            };

            var s3 = new SqlColumn
            {
                Name = "a",
                ValType = new DbValType(typeof(int))
                {
                    DbType = DbType.Int32
                },
                Alias = "aaaa"
            };

            Assert.Equal(s1, s2);
            Assert.NotEqual(s2, s3);
        }
    }
}