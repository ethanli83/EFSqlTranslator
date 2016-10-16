namespace Translation.Tests
{
    using Xunit;
    using System.Text.RegularExpressions;

    public static class TestUtils
    {
        public static void AssertStringEqual(string expected, string actual)
        {
            expected = Regex.Replace(expected, @"[\n\r\s]+", " ").Trim();
            actual = Regex.Replace(actual, @"[\n\r\s]+", " ").Trim();
            Assert.Equal(expected, actual);
        }
    }
}