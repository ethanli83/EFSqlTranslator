using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace EFSqlTranslator.Tests
{
    public static class TestUtils
    {
        public static void AssertStringEqual(string expected, string actual)
        {
            Console.WriteLine(actual);

            expected = Regex.Replace(expected, @"[\n\r\s]+", " ").Trim();
            actual = Regex.Replace(actual, @"[\n\r\s]+", " ").Trim();
            Assert.AreEqual(expected, actual);
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CategoryReadMeAttribute : Attribute
    {
        public int Index { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TranslationReadMeAttribute : Attribute
    {
        public int Index { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ExpressionDescription { get; set; }
        public string SqlDescription { get; set; }
    }
}