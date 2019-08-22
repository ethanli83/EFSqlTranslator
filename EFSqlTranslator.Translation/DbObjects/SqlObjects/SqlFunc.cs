using System;
using System.Linq;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlFunc : SqlSelectable, IDbFunc
    {
        public SqlFunc(string name, Type type, IDbObject[] parameters)
        {
            Name = name;
            Parameters = parameters;
            ReturnType = type;
        }

        public string Name { get; }

        public IDbObject[] Parameters { get; }

        public Type ReturnType { get; }

        public override string ToString()
        {
            return $"{Name}({string.Join(", ", Parameters.AsEnumerable())})";
        }
    }
}