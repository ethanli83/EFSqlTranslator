using System;
using System.Collections.Generic;
using System.Text;

namespace EFSqlTranslator.Translation.DbObjects
{
    public class DbReference : IDbObject
    {
        public DbReference(IDbObject dbObject)
        {
            Referee = dbObject;
        }

        public IDbObject SelectExpression { get; set; }

        public DbReference Ref { get; set; }

        public IDbObject Referee { get; }

        // the select that contains the reference
        public IDbSelect OwnerSelect { get; set; }

        // the join that joining to this reference
        public IDbJoin OwnerJoin { get; set; }
        
        public IDictionary<string, IDbSelectable> RefSelection { get; set; } = new Dictionary<string, IDbSelectable>();

        //public IDbSelect OwnerSelect { get; set; }
        public string Alias { get; set; }

        public override string ToString()
        {
            if (Referee is IDbTable)
                return $"{Referee} {Alias}";

            var sb = new StringBuilder();

            sb.AppendLine("(");

            var refStr = Referee.ToString();
            var lines = refStr.Split(new [] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            refStr = string.Join("\n    ", lines);

            sb.AppendLine($"    {refStr}");
            sb.Append($") {Alias}");

            return sb.ToString();
        }
    }
}