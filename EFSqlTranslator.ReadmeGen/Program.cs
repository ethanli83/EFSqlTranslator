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
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.EntityFrameworkCore.Query.Internal;

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
            var hierarchy = (Hierarchy)LogManager.GetRepository(typeof(QueryTranslator).Assembly);
            
            var patternLayout = new PatternLayout
            {
                ConversionPattern = "%date [%thread] %-5level %logger - %message%newline"
            };
            patternLayout.ActivateOptions();

            var memoryAppender = new MemoryAppender
            {
                Layout = patternLayout
            };
            
            memoryAppender.ActivateOptions();
            hierarchy.Root.AddAppender(memoryAppender);

            hierarchy.Root.Level = Level.Debug;
            hierarchy.Configured = true;

            var categories = GetCategories(memoryAppender);

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

        private static IEnumerable<ReadMeCategory> GetCategories(MemoryAppender appender)
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
                    var translationEntry = GetTranslationEntry(appender, methodInfo, type);
                    if (translationEntry == null)
                        continue;

                    category.Entries.Add(translationEntry);
                }
            }

            return categories;
        }

        private static ReadMeTranslationEntry GetTranslationEntry(MemoryAppender appender, MethodBase methodInfo,
            Type type)
        {
            var tAttr = methodInfo.GetCustomAttribute<TranslationReadMeAttribute>();
            if (tAttr == null)
                return null;

            var test = Activator.CreateInstance(type);
            methodInfo.Invoke(test, new object[0]);

            var record = appender.GetEvents()
                .Select(e => e.MessageObject)
                .OfType<Tuple<Expression, string>>()
                .Single();
            
            appender.Clear();
            
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