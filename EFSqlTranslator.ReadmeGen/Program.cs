using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using AgileObjects.ReadableExpressions;
using EFSqlTranslator.Tests;
using EFSqlTranslator.Translation;
using NLog;
using NLog.Config;

namespace EFSqlTranslator.ReadmeGen
{
    internal class Program
    {
        private static readonly Regex NameRegex = new Regex(@"^Microsoft.*\[EFSqlTranslator\.Tests\.(\w+)\]");
        private static readonly Regex CommentsRegex = new Regex(@"\n\s.* Quoted to induce a closure:\n\s+");

        private const string Beginning =
            @"# EFSqlTranslator [![Build Status](https://travis-ci.org/ethanli83/EFSqlTranslator.svg?branch=master)](https://travis-ci.org/ethanli83/EFSqlTranslator)

A standalone linq to sql translator that can be used with EF and Dapper.";

        public static void Main(string[] args)
        {
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration
            var consoleTarget = new MyFirstTarget();
            config.AddTarget(typeof(LinqTranslator).AssemblyQualifiedName, consoleTarget);

            // Step 3. Set target properties
            consoleTarget.Layout = @"${date:format=HH\:mm\:ss} ${logger} ${message}";

            // Step 4. Define rules
            var rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add(rule1);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;

            var categories = GetCategories(consoleTarget);

            const string path = @"../README.md";
            if (File.Exists(path))
                File.Delete(path);

            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            using (var sw = new StreamWriter(fs))
            {
                sw.WriteLine(Beginning);

                foreach (var category in categories)
                    category.WriteTo(sw);
            }
        }

        private static IEnumerable<ReadMeCategory> GetCategories(MyFirstTarget consoleTarget)
        {
            var assambly = Assembly.Load(new AssemblyName("EFSqlTranslator.Tests"));
            var classes = assambly.GetTypes();

            var categories = new SortedSet<ReadMeCategory>(ReadMeCategory.Comparer);

            foreach (var type in classes)
            {
                var typeInfo = type.GetTypeInfo();
                var cAttr = typeInfo.GetCustomAttribute<CategoryReadMeAttribute>();
                if (cAttr == null)
                    continue;

                var category = new ReadMeCategory
                {
                    CategoryAttr = cAttr
                };

                categories.Add(category);

                var methods = type.GetMethods();
                foreach (var methodInfo in methods)
                {
                    var translationEntry = GetTranslationEntry(consoleTarget, methodInfo, type);
                    if (translationEntry == null)
                        continue;

                    category.Entries.Add(translationEntry);
                }
            }

            return categories;
        }

        private static ReadMeTranslationEntry GetTranslationEntry(MyFirstTarget consoleTarget, MethodBase methodInfo,
            Type type)
        {
            var tAttr = methodInfo.GetCustomAttribute<TranslationReadMeAttribute>();
            if (tAttr == null)
                return null;

            var test = Activator.CreateInstance(type);
            methodInfo.Invoke(test, new object[0]);

            var record = consoleTarget.Records.OfType<Tuple<Expression, string>>().Single();
            consoleTarget.Records.Clear();

            var expression = record.Item1;
            var sql = record.Item2;

            var expStr = expression.ToReadableString();

            var matchs = NameRegex.Match(expStr);
            var name = matchs.Groups[1].Value;

            expStr = NameRegex.Replace(expStr, $"db.{name}s");
            expStr = CommentsRegex.Replace(expStr, "");

            return new ReadMeTranslationEntry
            {
                TranslationAttr = tAttr,
                ExpressionString = expStr,
                Sql = sql
            };
        }
    }
}