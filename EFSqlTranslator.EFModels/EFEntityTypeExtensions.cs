using System.Collections.Generic;
using System.Linq;
using EFSqlTranslator.Translation;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFSqlTranslator.EFModels
{
    public static class EFEntityTypeExtensions
    {
        private const string TableNameKey = "Relational:TableName";

        public static EntityInfo ToEntityInfo(this IEntityType et)
        {
            var a = et.GetAnnotations();
            var annotation = et.FindAnnotation(TableNameKey);
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