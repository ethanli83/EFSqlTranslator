using System.Text.RegularExpressions;
using NUnit.Framework;

namespace EFSqlTranslator.Tests
{
    public static class TestUtils
    {
        public static void AssertStringEqual(string expected, string actual)
        {
            expected = Regex.Replace(expected, @"[\n\r\s]+", " ").Trim();
            actual = Regex.Replace(actual, @"[\n\r\s]+", " ").Trim();
            Assert.AreEqual(expected, actual);
        }
    }
}