using EFSqlTranslator.Translation.DbObjects;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;
using NUnit.Framework;

namespace EFSqlTranslator.Tests.SqlObjectsTests
{
    [TestFixture]
    public class SqlObjectsEqualtyTests
    {
        [Test]
        public void Test_Column_Equals()
        {
            var s1 = new SqlColumn
            {
                Name = "a",
                ValType = new DbType
                {
                    DotNetType = typeof(int),
                    TypeName = "int"
                },
                Alias = "aaa"
            };

            var s2 = new SqlColumn
            {
                Name = "a",
                ValType = new DbType
                {
                    DotNetType = typeof(int),
                    TypeName = "int"
                },
                Alias = "aaa"
            };

            var s3 = new SqlColumn
            {
                Name = "a",
                ValType = new DbType
                {
                    DotNetType = typeof(int),
                    TypeName = "int"
                },
                Alias = "aaaa"
            };

            Assert.AreEqual(s1, s2);
            Assert.AreNotEqual(s2, s3);
        }
    }
}