using System.Collections.Generic;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace EFSqlTranslator.ReadmeGen
{
    [Target("MyFirst")]
    public sealed class MyFirstTarget: TargetWithLayout
    {
        public MyFirstTarget()
        {
            Host = "localhost";
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