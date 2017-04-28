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
using Microsoft.EntityFrameworkCore.Query.Internal;
using NLog;
using NLog.Config;

namespace EFSqlTranslator.ReadmeGen
{
    internal class Program
    {
        private static readonly Regex QueryableNameRegex;
        private static readonly Regex SingleLineCommentsRegex = new Regex(@"\s*//[^\r\n]+\s+");

        static Program()
        {
            var queryableType = typeof(EntityQueryable<>);
            var contextNamespace = typeof(TestingContext).Namespace;
            QueryableNameRegex = new Regex($@"{Regex.Escape(queryableType.FullName)}\[{Regex.Escape(contextNamespace)}\.(\w+)\]");
        }

        private const string Beginning = @"# <img src=""https://github.com/ethanli83/LinqRunner/blob/master/LinqRunner.Client/src/img/Butterfly.png"" align=""left"" height=""40"" width=""40""/>EFSqlTranslator [![Build Status](https://travis-ci.org/ethanli83/EFSqlTranslator.svg?branch=master)](https://travis-ci.org/ethanli83/EFSqlTranslator)

A standalone linq to sql translator that can be used with EF and Dapper.

The translator is a nuget libary. To use the libary, use your nuget managment tool to install package [EFSqlTranslator.Translation](https://www.nuget.org/packages/EFSqlTranslator.Translation/) and [EFSqlTranslator.EFModels](https://www.nuget.org/packages/EFSqlTranslator.EFModels/).

You can now try the translator out on http://linqrunner.daydreamer.io/.";

        private const string Ending = @"";

        public static void Main(string[] args)
        {
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration
            var consoleTarget = new MyFirstTarget();
            config.AddTarget(nameof(QueryTranslator), consoleTarget);

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
                sw.WriteLine();

                foreach (var category in categories)
                    category.WriteTo(sw);

                sw.WriteLine();
                sw.WriteLine(Ending);
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

            expStr = QueryableNameRegex.Replace(expStr, @"db.$1s");
            expStr = SingleLineCommentsRegex.Replace(expStr, "");

            return new ReadMeTranslationEntry
            {
                TranslationAttr = tAttr,
                ExpressionString = expStr,
                Sql = sql
            };
        }
    }
}