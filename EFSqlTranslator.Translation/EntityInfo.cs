using System;
using System.Collections.Generic;

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
    }

    public class EntityRelation
    {
        public EntityInfo FromEntity { get; set; }

        public EntityInfo ToEntity { get; set; }

        public IList<EntityFieldInfo> FromKeys { get; set; }

        public IList<EntityFieldInfo> ToKeys { get; set; }

        public bool IsChildRelation { get; set; }
    }

    public class EntityFieldInfo
    {
        public string Name { get; set; }
        
        public EntityInfo Entity { get; set; }

        public Type ValType { get; set; }

        public bool IsPrimaryKey { get; set; }
    }
}