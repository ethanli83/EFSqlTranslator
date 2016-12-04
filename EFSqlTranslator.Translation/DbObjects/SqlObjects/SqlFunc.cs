using System.Linq;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlFunc : SqlSelectable, IDbFunc
    {
        public SqlFunc(string name, IDbObject[] parameters)
        {
            Name = name;
            Parameters = parameters;
        }

        public string Name { get; }
        public IDbObject[] Parameters { get; }

        public override string ToString()
        {
            return $"{Name}({string.Join(", ", Parameters.AsEnumerable())})";
        }
    }
}