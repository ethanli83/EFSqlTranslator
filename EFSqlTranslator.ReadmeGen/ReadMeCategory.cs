using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EFSqlTranslator.Tests;

namespace EFSqlTranslator.ReadmeGen
{
    public class ReadMeCategory
    {
        public static IComparer<ReadMeCategory> Comparer =
            Comparer<ReadMeCategory>.Create((a, b) => a.CategoryAttr.Index.CompareTo(b.CategoryAttr.Index));

        public CategoryReadMeAttribute CategoryAttr { get; set; }

        public SortedSet<ReadMeTranslationEntry> Entries { get; } =
            new SortedSet<ReadMeTranslationEntry>(ReadMeTranslationEntry.Comparer);

        public void WriteTo(TextWriter writer)
        {
            if (!string.IsNullOrEmpty(CategoryAttr.Title))
                writer.WriteLine($"## {Roman(CategoryAttr.Index + 1)}. {CategoryAttr.Title}");

            if (!string.IsNullOrEmpty(CategoryAttr.Description))
            {
                writer.WriteLine(CategoryAttr.Description.Trim());

                if (CategoryAttr.Description.IndexOf("\n", StringComparison.CurrentCultureIgnoreCase) >= 0)
                    writer.WriteLine();
            }

            var last = Entries.Last();
            foreach (var entry in Entries)
            {
                entry.WriteTo(writer);
                if (entry != last)
                    writer.WriteLine();
            }
        }

        private static string Roman(int number)
        {
            var result = new StringBuilder();
            int[] digitsValues = { 1, 4, 5, 9, 10, 40, 50, 90, 100, 400, 500, 900, 1000 };
            string[] romanDigits = { "I", "IV", "V", "IX", "X", "XL", "L", "XC", "C", "CD", "D", "CM", "M" };
            while (number > 0)
            {
                for (var i = digitsValues.Count() - 1; i >= 0; i--)
                    if (number / digitsValues[i] >= 1)
                    {
                        number -= digitsValues[i];
                        result.Append(romanDigits[i]);
                        break;
                    }
            }
            return result.ToString();
        }
    }
}