using System;

namespace EFSqlTranslator.Translation.DbObjects
{
    public class DbType
    {
        public Type DotNetType { get; set; }
        public string TypeName { get; set; }
        public object[] Parameters { get; set; }

        protected bool Equals(DbType other)
        {
            return DotNetType == other.DotNetType && string.Equals(TypeName, other.TypeName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((DbType) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((DotNetType?.GetHashCode() ?? 0) * 397) ^ (TypeName?.GetHashCode() ?? 0);
            }
        }
    }
}