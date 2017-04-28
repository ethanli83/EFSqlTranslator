using System;
using System.Collections.Generic;
using System.Linq;
using EFSqlTranslator.Translation;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFSqlTranslator.EFModels
{
    public static class EFEntityTypeExtensions
    {
        private const string TableSchemaAnnoName = "Relational:Schema";

        private const string TableNameAnnoName = "Relational:TableName";
        
        private const string ColumnNameAnnoName = "Relational:ColumnName";

        public static EntityInfo ToEntityInfo(this IEntityType et)
        {
            var annoName = et.FindAnnotation(TableNameAnnoName);
            if (annoName == null)
            {
                throw new NotSupportedException("Entity must have a table name");
            }

            var annoSchema = et.FindAnnotation(TableSchemaAnnoName);

            var info = new EntityInfo
            {
                Namespace = annoSchema != null ? annoSchema.Value.ToString() : string.Empty,
                EntityName = annoName.Value.ToString(),
                Type = et.ClrType
            };

            return info;
        }

        public static EntityFieldInfo ToEntityFieldInfo(this IProperty p, EntityInfo e, bool isPk = false)
        {
            var info = new EntityFieldInfo
            {
                ClrProperty = p.PropertyInfo,
                Name = p.Name,
                ValType = p.ClrType,
                IsPrimaryKey = isPk,
                Entity = e
            };

            var annotation = p.FindAnnotation(ColumnNameAnnoName);
            if (annotation != null && annotation.Value.ToString() != info.Name)
            {
                throw new NotSupportedException(
                    "Annotation specifying column name is not support, " +
                    "we need to use it to match property name at the moment");
            }

            return info;
        }

        public static List<EntityFieldInfo> ToEntityFieldInfo(this IKey k, EntityInfo e)
        {
            return k.Properties.Select(p => p.ToEntityFieldInfo(e, true)).ToList();
        }
    }
}