using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AgileObjects.ReadableExpressions;
using EFSqlTranslator.Tests.TranslatorTests;
using EFSqlTranslator.Translation;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace EFSqlTranslator.ReadmeGen
{
    internal class Program
    {
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

            var test = new SelectTranslationTests();
            test.Test_Multiple_Select_Calls2();

            foreach (var record in consoleTarget.Records.Cast<Tuple<Expression, string>>())
            {
                Console.WriteLine(record.Item1.ToReadableString());
                Console.WriteLine(record.Item2);
            }
        }
    }

    [Target("MyFirst")]
    public sealed class MyFirstTarget: TargetWithLayout
    {
        public MyFirstTarget()
        {
            this.Host = "localhost";
        }

        [RequiredParameter]
        public string Host { get; set; }

        public List<object> Records { get; set; } = new List<object>();

        protected override void Write(LogEventInfo logEvent)
        {
            Records.Add(logEvent.Parameters.Single());
        }
    }
}