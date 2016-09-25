using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EFSqlTranslator.Translation
{
    public static class EFEntityTypeExtensions
    {
        private const string _tableNameKey = "Relational:TableName";

        public static EntityInfo ToEntityInfo(this IEntityType et)
        {
            var a = et.GetAnnotations();
            var annotation = et.FindAnnotation(_tableNameKey);
            return new EntityInfo 
            {
                Namespace = "",
                EntityName = annotation.Value.ToString(),
                Type = et.ClrType
            };
        }

        public static EntityFieldInfo ToEntityFieldInfo(this IProperty p, EntityInfo e, bool isPk = false)
        {
            return new EntityFieldInfo 
            {
                Name = p.Name,
                ValType = p.ClrType,
                IsPrimaryKey = isPk,
                Entity = e
            };
        }

        public static List<EntityFieldInfo> ToEntityFieldInfo(this IKey k, EntityInfo e)
        {
            return k.Properties.Select(p => p.ToEntityFieldInfo(e, true)).ToList();
        }
    }
}