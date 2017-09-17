using System;
using System.Data;

namespace EFSqlTranslator.Translation.DbObjects
{
    public class DbValType
    {
        public DbValType(Type dotNetType)
        {
            DotNetType = dotNetType;
        }

        public Type DotNetType { get; }
        public DbType DbType { get; set; }
        public object[] Parameters { get; set; }

        private bool Equals(DbValType other)
        {
            return DotNetType == other.DotNetType && DbType == other.DbType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DbValType) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((DotNetType != null ? DotNetType.GetHashCode() : 0) * 397) ^ (int) DbType;
            }
        }
    }
}