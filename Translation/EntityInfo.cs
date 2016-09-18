using System;

namespace EFSqlTranslator.Translation
{
    public class EntityInfo
    {
        public string Namespace { get; set; }
        public string EntityName { get; set; }
    }

    public class FieldInfo
    {
        public string Name { get; set; }
        public Type type { get; set; }
    }
}