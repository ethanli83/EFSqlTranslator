using System.Collections.Generic;
using System.IO;
using System.Linq;
using EFSqlTranslator.Tests;

namespace EFSqlTranslator.ReadmeGen
{
    public class ReadMeTranslationEntry
    {
        public static IComparer<ReadMeTranslationEntry> Comparer =
            Comparer<ReadMeTranslationEntry>.Create((a, b) => a.TranslationAttr.Index.CompareTo(b.TranslationAttr.Index));

        public TranslationReadMeAttribute TranslationAttr { get; set; }

        public string ExpressionString { get; set;}

        public string Sql { get; set; }

        public void WriteTo(TextWriter writer)
        {
            if (!string.IsNullOrEmpty(TranslationAttr.Title))
                writer.WriteLine($"### {TranslationAttr.Index + 1}. {TranslationAttr.Title}");

            if (!string.IsNullOrEmpty(TranslationAttr.Description))
                writer.WriteLine($"{TranslationAttr.Description.Trim()}");

            var desc = GetComments(TranslationAttr.ExpressionDescription, "\\");

            writer.WriteLine($"```csharp\n// Linq expression:\n{desc}{ExpressionString.Trim()}\n```");

            if (!string.IsNullOrEmpty(TranslationAttr.SqlDescription))
                writer.WriteLine(TranslationAttr.SqlDescription.Trim());

            desc = GetComments(TranslationAttr.SqlDescription, "--");

            writer.WriteLine($"```sql\n-- Transalted Sql:\n{desc}{Sql.Trim()}\n```");
        }

        private static string GetComments(string desc, string commentPrefix)
        {
            if (string.IsNullOrEmpty(desc))
                return desc;

            desc = desc.Trim();
            var lines = desc.Split('\n').Select(l => $"{commentPrefix} {l}");
            desc = string.Join("\n", lines) + "\n";

            return desc;
        }
    }
}