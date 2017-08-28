using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EFSqlTranslator.Translation
{
    public class EntityInfo
    {
        private readonly Dictionary<string, EntityRelation> _retlations = 
            new Dictionary<string, EntityRelation>();

        public string Namespace { get; set; }
        
        public string EntityName { get; set; }

        public Type Type { get; set; }

        public List<EntityFieldInfo> Keys { get; set; }

        public List<EntityFieldInfo> Columns { get; set; }

        public void AddRelation(string name, EntityRelation relation)
        {
            _retlations[name] = relation;
        }

        public EntityRelation GetRelation(string relationName)
        {
            return _retlations.ContainsKey(relationName) ? _retlations[relationName] : null;
        }

        public bool RequirePropertyNameMapping()
        {
            return Columns.Any(c => c.RequirePropertyNameMapping());
        }
    }

    public class EntityRelation
    {
        public EntityInfo FromEntity { get; set; }

        public EntityInfo ToEntity { get; set; }

        public PropertyInfo FromProperty { get; set; }

        public PropertyInfo ToProperty { get; set; }

        public IList<EntityFieldInfo> FromKeys { get; set; }

        public IList<EntityFieldInfo> ToKeys { get; set; }

        public bool IsChildRelation { get; set; }
    }

    public class EntityFieldInfo
    {
        public string DbName { get; set; }

        public string PropertyName { get; set; }
        
        public EntityInfo Entity { get; set; }

        public Type ValType { get; set; }

        public bool IsPrimaryKey { get; set; }

        public MemberInfo ClrProperty { get; set; }

        public bool RequirePropertyNameMapping()
        {
            return !string.Equals(DbName, PropertyName, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}